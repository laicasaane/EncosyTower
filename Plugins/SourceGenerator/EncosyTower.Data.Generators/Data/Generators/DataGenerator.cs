using static EncosyTower.SourceGen.Data.Helpers.Helpers;

namespace EncosyTower.Data.Generators.Data
{
    [Generator]
    public sealed class DataGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      DATA_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax
                    , DataSpec.Extract
                )
                .WithTrackingName("DataGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("DataGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("DataGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , DataSpec declaration
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (declaration.fieldRefs.Count == 0 && declaration.propRefs.Count == 0)
            {
                return;
            }

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Data.Generators.Data.DataGenerator"
                , assemblyName
                , declaration.hintName
                , "Data"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  declaration.openingSource
                , declaration.WriteCode(context.CancellationToken)
                , declaration.closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }
    }
}
