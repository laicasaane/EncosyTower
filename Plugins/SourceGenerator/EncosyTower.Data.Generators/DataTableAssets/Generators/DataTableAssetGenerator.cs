using static EncosyTower.Data.Generators.DataTableAssets.Helpers;

namespace EncosyTower.Data.Generators.DataTableAssets
{
    [Generator]
    public sealed class DataTableAssetGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      DATA_TABLE_ASSET_ATTRIBUTE
                    , static (node, _) => node is ClassDeclarationSyntax
                    , DataTableAssetSpec.Extract
                )
                .WithTrackingName("DataTableAssetGenerator.Candidates")
                .Where(static x => x.IsValid)
                .WithTrackingName("DataTableAssetGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("DataTableAssetGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , DataTableAssetSpec declaration
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Data.Generators.DataTableAssets.DataTableAssetGenerator"
                , assemblyName
                , declaration.hintName
                , "DataTableAsset"
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
