using static EncosyTower.Persistence.Generators.Helpers;

namespace EncosyTower.Persistence.Generators
{
    [Generator]
    internal sealed class PersistGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(PersistGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      PERSIST_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax
                        or StructDeclarationSyntax
                        or RecordDeclarationSyntax
                    , PersistSpec.Extract
                )
                .WithTrackingName("PersistGenerator.Candidates")
                .Where(static x => x.IsValid)
                .WithTrackingName("PersistGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("PersistGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , PersistSpec spec
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (spec.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Persistence.Generators.PersistGenerator"
                , assemblyName
                , spec.hintName
                , "Persist"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  spec.openingSource
                , spec.WriteCode(context.CancellationToken)
                , spec.closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }
    }
}
