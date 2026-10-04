using EncosyTower.Core.Types.Flags;

namespace EncosyTower.Core.Generators.Types.Flags
{
    partial struct TypeFlagSpec
    {
        private const string GLOBAL_PREFIX = "global::";

        private static readonly SymbolDisplayFormat s_nameFormat = new(
              typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly
            , genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            , miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
        );

        public static TypeFlagSpec Extract(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol owner)
            {
                return default;
            }

            var compilation = context.SemanticModel.Compilation;
            var marker = compilation.GetTypeByMetadataName(TypeFlagRules.ATTRIBUTE_METADATA_NAME);
            var typeFlag = compilation.GetTypeByMetadataName(TypeFlagRules.TYPE_FLAG_METADATA_NAME);

            if (marker == null
                || typeFlag == null
                || TypeFlagRules.CountMarkers(owner, marker, token) != 1
                || owner.TypeKind is not (TypeKind.Class or TypeKind.Struct)
                || TypeFlagRules.TryGetUnsupportedKind(owner, out _)
                || TypeFlagRules.HasUnresolvedBase(owner, token)
            )
            {
                return default;
            }

            var values = TypeFlagRules.ReadOptions(TypeFlagRules.GetMarker(owner, marker, token), token);

            if (values.IsValid == false)
            {
                return default;
            }

            var options = values.ToOptions();

            if (TypeFlagRules.TryPlanOwner(
                  owner
                , options
                , marker
                , typeFlag
                , compilation
                , token
                , out var hiddenMembers
            ) == false
            )
            {
                return default;
            }

            var assemblyName = compilation.AssemblyName ?? string.Empty;
            var metadataName = owner.ToMetadataName();
            var depth = 1;

            for (var type = owner.ContainingType; type != null; type = type.ContainingType)
            {
                depth++;
            }

            var declarations = new Declaration[depth];
            var headers = new string[depth];
            var index = depth;

            for (var type = owner; type != null; type = type.ContainingType)
            {
                token.ThrowIfCancellationRequested();
                index--;

                var isRecordClass = type.TypeKind == TypeKind.Class && type.IsRecord;
                var keyword = isRecordClass ? "record" : type.ToPartialTypeKeyword();
                var typeParameterNames = string.Join(
                      ","
                    , type.TypeParameters.Select(static parameter => parameter.Name)
                );

                declarations[index] = new Declaration(keyword, typeParameterNames);
                headers[index] = $"partial {keyword} {type.ToDisplayString(s_nameFormat)}";
            }

            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  TypeFlagRules.GENERATOR_METADATA_NAME
                , assemblyName
                , metadataName
                , "TypeFlag"
                , string.Empty
            );

            return new TypeFlagSpec(
                  assemblyName
                , metadataName
                , ImmutableArray.Create(declarations).AsEquatableArray()
                , owner.TypeKind == TypeKind.Struct
                , options
                , hiddenMembers
                , GetNamespaceName(owner)
                , ImmutableArray.Create(headers, 0, depth - 1).AsEquatableArray()
                , headers[depth - 1]
                , owner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                , hintName
            );
        }

        private static string GetNamespaceName(INamedTypeSymbol owner)
        {
            var ns = owner.ContainingNamespace;

            if (ns == null || ns.IsGlobalNamespace)
            {
                return string.Empty;
            }

            var name = ns.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (name.StartsWith(GLOBAL_PREFIX, StringComparison.Ordinal))
            {
                return name.Substring(GLOBAL_PREFIX.Length);
            }

            return name;
        }
    }
}
