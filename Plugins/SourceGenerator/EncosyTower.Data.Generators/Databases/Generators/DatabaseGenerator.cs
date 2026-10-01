using static EncosyTower.Data.Generators.Databases.Helpers;

namespace EncosyTower.Data.Generators.Databases
{
    [Generator]
    public sealed class DatabaseGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(DatabaseGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, DATABASES_NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      "EncosyTower.Databases.DatabaseAttribute"
                    , static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax
                    , DatabaseSpec.Extract
                )
                .WithTrackingName("DatabaseGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("DatabaseGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("DatabaseGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , DatabaseSpec model
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Data.Generators.Databases.DatabaseGenerator"
                , assemblyName
                , model.hintName
                , "Database"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  model.openingSource
                , model.WriteCode(context.CancellationToken)
                , model.closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }
    }
}
