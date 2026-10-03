namespace EncosyTower.Entities.Stats
{
    /// <summary>
    /// Diagnostic-neutral acceptance rules for <c>[StatData]</c> and <c>[StatCollection]</c>, shared by
    /// StatDataGenerator, StatCollectionGenerator, StatDataDiagnosticAnalyzer and StatCollectionDiagnosticAnalyzer.
    /// </summary>
    internal static class StatDataRules
    {
        public const string STAT_DATA_ATTRIBUTE = "global::EncosyTower.Entities.Stats.StatDataAttribute";

        private const byte NONE_VARIANT_TYPE = 0;

        /// <summary>
        /// A nested declaration that can be a StatCollection entry: a non-generic <c>struct</c> (not a
        /// <c>record struct</c>) with an attribute list.
        /// </summary>
        public static bool IsEntryDeclaration(SyntaxNode node, out StructDeclarationSyntax syntax)
        {
            if (node is StructDeclarationSyntax { TypeParameterList: null } candidate
                && candidate.AttributeLists.Count > 0
            )
            {
                syntax = candidate;
                return true;
            }

            syntax = null;
            return false;
        }

        /// <summary>
        /// The generated members are mutable fields and constructors of a partial <c>struct</c>, so the type is
        /// neither a <c>record struct</c> nor <c>readonly</c> (on any partial part).
        /// </summary>
        public static bool IsSupportedTarget(INamedTypeSymbol symbol)
            => symbol.IsRecord == false && symbol.IsReadOnly == false;

        /// <summary>
        /// Finds the <c>[StatData]</c> application written on this declaration of <paramref name="symbol"/> that has
        /// at least one constructor argument.
        /// </summary>
        public static bool TryGetEntryAttribute(
              StructDeclarationSyntax syntax
            , INamedTypeSymbol symbol
            , CancellationToken token
            , out AttributeData attribute
            , out AttributeSyntax attributeSyntax
        )
        {
            foreach (var candidate in symbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var reference = candidate.ApplicationSyntaxReference;

                if (reference == null
                    || reference.SyntaxTree != syntax.SyntaxTree
                    || syntax.Span.Contains(reference.Span) == false
                    || candidate.AttributeClass?.ToFullName() is not STAT_DATA_ATTRIBUTE
                    || candidate.ConstructorArguments.Length < 1
                    || reference.GetSyntax(token) is not AttributeSyntax candidateSyntax
                )
                {
                    continue;
                }

                attribute = candidate;
                attributeSyntax = candidateSyntax;
                return true;
            }

            attribute = null;
            attributeSyntax = null;
            return false;
        }

        /// <summary>
        /// A <c>StatVariantType</c> argument whose value is a declared member of its enum type.
        /// </summary>
        public static bool IsDefinedVariantType(TypedConstant argument)
        {
            if (argument.Kind != TypedConstantKind.Enum
                || argument.Value is not byte value
                || argument.Type is not INamedTypeSymbol enumType
            )
            {
                return false;
            }

            foreach (var member in enumType.GetMembers())
            {
                if (member is IFieldSymbol { HasConstantValue: true, ConstantValue: byte constant }
                    && constant == value
                )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// A <c>StatVariantType</c> argument names a value type: a declared member other than <c>None</c>.
        /// </summary>
        public static bool IsAcceptedVariantType(TypedConstant argument)
            => IsDefinedVariantType(argument)
            && argument.Value is byte value
            && value != NONE_VARIANT_TYPE;

        /// <summary>
        /// A <c>typeof</c> argument names an enum with an integral underlying type and no error type in it.
        /// </summary>
        public static bool IsAcceptedEnumType(ITypeSymbol type)
            => type is INamedTypeSymbol {
                TypeKind: TypeKind.Enum,
                EnumUnderlyingType.SpecialType: SpecialType.System_SByte
                    or SpecialType.System_Byte
                    or SpecialType.System_Int16
                    or SpecialType.System_UInt16
                    or SpecialType.System_Int32
                    or SpecialType.System_UInt32
                    or SpecialType.System_Int64
                    or SpecialType.System_UInt64,
            } enumType
            && enumType.ContainsErrorType() == false;
    }
}
