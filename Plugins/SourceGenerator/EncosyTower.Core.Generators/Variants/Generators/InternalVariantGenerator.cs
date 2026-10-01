using EncosyTower.SourceGen.Helpers.Variants;

namespace EncosyTower.Core.Generators.Variants
{
    [Generator]
    internal sealed class InternalVariantGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.Variants";
        private const string NAMESPACE_PREFIX = $"global::{NAMESPACE}";
        private const string SKIP_ATTRIBUTE = $"{NAMESPACE_PREFIX}.SkipSourceGeneratorsForAssemblyAttribute";

        private const string VARIANT_T = $"{NAMESPACE_PREFIX}.Variant<";
        private const string CONVERTER_T = $"{NAMESPACE_PREFIX}.Converters.CachedVariantConverter<";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE))
                .WithTrackingName("InternalVariantGenerator.Compilation");

            var typeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatched
                , transform: GetSemanticMatch
            ).WithTrackingName("InternalVariantGenerator.Candidates")
                .Where(static x => x.IsValid)
                .WithTrackingName("InternalVariantGenerator.ValidSpecs");

            var collected = typeProvider.Collect()
                .WithTrackingName("InternalVariantGenerator.CollectedSpecs");
            var inputs = collected.Combine(compilationProvider)
                .WithTrackingName("InternalVariantGenerator.Inputs");
            var combined = inputs
                .WithTrackingName("InternalVariantGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static bool IsSyntaxMatched(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (syntaxNode is not MemberAccessExpressionSyntax syntax
                || syntax.Expression is not GenericNameSyntax typeSyntax
                || syntax.Name is not IdentifierNameSyntax memberSyntax
                || typeSyntax.TypeArgumentList is not TypeArgumentListSyntax argListSyntax
                || argListSyntax.Arguments.Count != 1
            )
            {
                return false;
            }

            var typeName = typeSyntax.Identifier.ValueText;
            var memberName = memberSyntax.Identifier.ValueText;

            return (typeName is "Variant" && memberName is "GetConverter")
                || (typeName is "CachedVariantConverter" && memberName is "Default");
        }

        private static InternalVariantSpec GetSemanticMatch(GeneratorSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var semanticModel = context.SemanticModel;
            var syntax = context.Node as MemberAccessExpressionSyntax;
            var typeSyntax = syntax.Expression as GenericNameSyntax;
            var typeInfo = semanticModel.GetTypeInfo(typeSyntax, token).Type;

            if (typeInfo is not INamedTypeSymbol type
                || type.TypeArguments.Length != 1
            )
            {
                return default;
            }

            if (type.TypeArguments[0] is not INamedTypeSymbol typeArg
                || typeArg.IsUnboundGenericType
                || (typeArg.IsGenericType && typeArg.TypeParameters.Length != 0)
            )
            {
                return default;
            }

            if (type.HasFullNamePrefix(VARIANT_T, token) == false
                && type.HasFullNamePrefix(CONVERTER_T, token) == false
            )
            {
                return default;
            }

            return InternalVariantSpecFactory.Create(typeArg, token);
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , ImmutableArray<InternalVariantSpec> candidates
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (candidates.Length < 1 || compilation.IsValid == false)
            {
                return;
            }

            using var valueTypeBuilder = ImmutableArrayBuilder<InternalVariantSpec>.Rent();
            using var refTypeBuilder = ImmutableArrayBuilder<InternalVariantSpec>.Rent();
            var seenTypeNames = new HashSet<string>(StringComparer.Ordinal);
            var assemblyName = compilation.AssemblyName;

            foreach (var candidate in candidates
                .OrderBy(static candidate => candidate.fullTypeName, StringComparer.Ordinal)
            )
            {
                if (seenTypeNames.Add(candidate.fullTypeName) == false)
                {
                    continue;
                }

                InternalVariantWriteCode.WriteVariantCode(ref context, in candidate, assemblyName);

                if (candidate.isValueType)
                {
                    valueTypeBuilder.Add(candidate);
                }
                else
                {
                    refTypeBuilder.Add(candidate);
                }
            }

            InternalVariantWriteCode.WriteStaticClass(
                  ref context
                , valueTypeBuilder.ToImmutable()
                , refTypeBuilder.ToImmutable()
                , assemblyName
            );
        }
    }
}
