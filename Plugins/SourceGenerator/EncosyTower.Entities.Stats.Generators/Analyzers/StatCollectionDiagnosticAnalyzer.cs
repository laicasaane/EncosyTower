namespace EncosyTower.Entities.Stats.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class StatCollectionDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.Entities.Stats";
        private const string STAT_COLLECTION_ATTRIBUTE = $"global::{NAMESPACE}.StatCollectionAttribute";
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";

        public static readonly DiagnosticDescriptor StatSystemAttributeRequired = new(
              id: "SG_STAT_COLLECTION_0001"
            , title: "Type argument of [StatCollection] must have [StatSystem]"
            , messageFormat: "\"{0}\" does not have the [StatSystem] attribute. The typeof argument of [StatCollection] must resolve to a type attributed with [StatSystem]."
            , category: "StatCollectionGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type passed to [StatCollection(typeof(...))] must be attributed with [StatSystem]."
        );

        public static readonly DiagnosticDescriptor TypeIdOffsetOverflow = new(
              id: "SG_STAT_COLLECTION_0002"
            , title: "typeIdOffset + StatData count exceeds uint.MaxValue"
            , messageFormat: "The combination of typeIdOffset ({0}) and the number of [StatData] members ({1}) in \"{2}\" exceeds uint.MaxValue. Reduce typeIdOffset or the number of [StatData] members."
            , category: "StatCollectionGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The combination of typeIdOffset and the number of [StatData] nested structs must not exceed uint.MaxValue."
        );

        public static readonly DiagnosticDescriptor MustBeStruct = new(
              id: "SG_STAT_COLLECTION_0003"
            , title: "[StatCollection] can only be applied to a struct"
            , messageFormat: "\"{0}\" is not a struct. [StatCollection] can only be applied to struct types."
            , category: "StatCollectionGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[StatCollection] can only be applied to struct types."
        );

        public static readonly DiagnosticDescriptor MustNotBeNonGeneric = new(
              id: "SG_STAT_COLLECTION_0004"
            , title: "[StatCollection] cannot be applied to an non-generic struct"
            , messageFormat: "\"{0}\" is a generic type. [StatCollection] can only be applied to non-generic structs."
            , category: "StatCollectionGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[StatCollection] can only be applied to non-generic struct types."
        );

        public static readonly DiagnosticDescriptor StatDataOutsideAttributedPart = new(
              id: "SG_STAT_COLLECTION_0005"
            , title: "[StatData] struct is declared outside the [StatCollection] part"
            , messageFormat: "\"{0}\" is declared in a part of \"{1}\" that does not have [StatCollection], so it is " +
                "not a stat of \"{1}\". Declare it in the part that has [StatCollection]."
            , category: "StatCollectionGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Only [StatData] structs declared directly in the partial declaration that has " +
                "[StatCollection] are stats of the collection."
        );

        public static readonly DiagnosticDescriptor MustNotBeRecordOrReadOnly = new(
              id: "SG_STAT_COLLECTION_0006"
            , title: "[StatCollection] cannot be applied to a record struct or readonly struct"
            , messageFormat: "\"{0}\" is a record struct or a readonly struct. [StatCollection] can only be " +
                "applied to non-readonly structs that are not records."
            , category: "StatCollectionGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[StatCollection] generates mutable fields into a partial struct, so it cannot be applied " +
                "to a record struct or a readonly struct."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MustBeStruct
                , MustNotBeNonGeneric
                , StatSystemAttributeRequired
                , TypeIdOffsetOverflow
                , StatDataOutsideAttributedPart
                , MustNotBeRecordOrReadOnly
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.HasAttribute(STAT_COLLECTION_ATTRIBUTE, token) == false
            )
            {
                return;
            }

            if (typeSymbol.TypeKind != TypeKind.Struct)
            {
                if (typeSymbol.TryGetSourceLocation(out var location) == false)
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(MustBeStruct, location, typeSymbol.Name));

                return;
            }

            if (typeSymbol.IsGenericType)
            {
                if (typeSymbol.TryGetSourceLocation(out var location) == false)
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(MustNotBeNonGeneric, location, typeSymbol.Name));

                return;
            }

            if (StatDataRules.IsSupportedTarget(typeSymbol) == false)
            {
                if (typeSymbol.TryGetSourceLocation(out var location) == false)
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(MustNotBeRecordOrReadOnly, location, typeSymbol.Name));

                return;
            }

            var attrib = typeSymbol.GetAttribute(STAT_COLLECTION_ATTRIBUTE, token);

            if (attrib == null || attrib.ConstructorArguments.Length < 1)
            {
                return;
            }

            var typeArg = attrib.ConstructorArguments[0];

            if (typeArg.Kind != TypedConstantKind.Type)
            {
                return;
            }


            if (typeArg.Value is not INamedTypeSymbol statSystemTypeSymbol
                || statSystemTypeSymbol.HasAttribute(STAT_SYSTEM_ATTRIBUTE, token) == false
            )
            {
                var displayName = (typeArg.Value as ISymbol)?.ToDisplayString() ?? typeArg.Value?.ToString() ?? "?";
                var location = attrib.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken)?.GetLocation()
                    ?? typeSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(StatSystemAttributeRequired, location, displayName));

                return;
            }

            token.ThrowIfCancellationRequested();

            var statDataCount = AnalyzeEntries(context, typeSymbol, attrib, token);

            var typeIdOffset = 0uL;

            if (attrib.ConstructorArguments.Length > 1
                && attrib.ConstructorArguments[1].Value is uint offset
            )
            {
                typeIdOffset = offset;
            }

            if (typeIdOffset + (ulong)statDataCount > uint.MaxValue)
            {
                if (typeSymbol.TryGetSourceLocation(out var location) == false)
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      TypeIdOffsetOverflow
                    , location
                    , typeIdOffset
                    , statDataCount
                    , typeSymbol.Name
                ));
            }
        }

        private static int AnalyzeEntries(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , AttributeData attribute
            , CancellationToken token
        )
        {
            if (attribute.ApplicationSyntaxReference?.GetSyntax(token)?.Parent?.Parent
                is not StructDeclarationSyntax declaration
            )
            {
                return 0;
            }

            var count = 0;

            foreach (var member in typeSymbol.GetTypeMembers())
            {
                token.ThrowIfCancellationRequested();

                if (TryGetEntryDeclaration(member, token, out var syntax, out var entryAttribute) == false)
                {
                    continue;
                }

                if (IsSameDeclaration(syntax.Parent, declaration) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          StatDataOutsideAttributedPart
                        , syntax.Identifier.GetLocation()
                        , member.Name
                        , typeSymbol.Name
                    ));

                    continue;
                }

                if (StatDataRules.IsSupportedTarget(member)
                    && IsAcceptedArgument(entryAttribute.ConstructorArguments[0], token)
                )
                {
                    count++;
                }
            }

            return count;
        }

        private static bool TryGetEntryDeclaration(
              INamedTypeSymbol member
            , CancellationToken token
            , out StructDeclarationSyntax syntax
            , out AttributeData attribute
        )
        {
            foreach (var reference in member.DeclaringSyntaxReferences)
            {
                token.ThrowIfCancellationRequested();

                if (StatDataRules.IsEntryDeclaration(reference.GetSyntax(token), out syntax)
                    && syntax.Parent is TypeDeclarationSyntax
                    && StatDataRules.TryGetEntryAttribute(syntax, member, token, out attribute, out _)
                )
                {
                    return true;
                }
            }

            syntax = null;
            attribute = null;
            return false;
        }

        private static bool IsSameDeclaration(SyntaxNode node, StructDeclarationSyntax declaration)
            => node != null
            && node.SyntaxTree == declaration.SyntaxTree
            && node.Span == declaration.Span;

        private static bool IsAcceptedArgument(TypedConstant argument, CancellationToken token)
        {
            if (StatDataRules.IsAcceptedVariantType(argument))
            {
                return true;
            }

            // The generator's compilation lacks generated sources, so a generated enum is an error type there and
            // is skipped; this compilation has it, so exclude it explicitly (task 007 reports SG_STAT_DATA_0005).
            return argument.Kind == TypedConstantKind.Type
                && argument.Value is ITypeSymbol type
                && StatDataRules.IsAcceptedEnumType(type)
                && type.TryFindEncosyTowerGeneratedType(token, out _, out _) == false;
        }
    }
}
