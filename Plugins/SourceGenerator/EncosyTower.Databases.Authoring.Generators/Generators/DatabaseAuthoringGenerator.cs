using static EncosyTower.Databases.Authoring.Generators.Helpers;

namespace EncosyTower.Databases.Authoring.Generators
{
    [Generator]
    public sealed class DatabaseAuthoringGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(DatabaseAuthoringGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider.Select(DatabaseAuthoringCompilationSpec.Create);

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      "EncosyTower.Databases.Authoring.AuthorDatabaseAttribute"
                    , static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax
                    , DatabaseSpec.Extract
                )
                .WithTrackingName("DatabaseAuthoringGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("DatabaseAuthoringGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.Compilation.IsValid)
                .WithTrackingName("DatabaseAuthoringGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) =>
                GenerateOutput(
                      sourceProductionContext
                    , source.Right        // AuthoringCompilationSpec
                    , source.Left         // DatabaseSpec
                )
            );
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , DatabaseAuthoringCompilationSpec compilation
            , DatabaseSpec model
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var databaseAuthoring = compilation.DatabaseAuthoring;
            var bakingSheet = compilation.BakingSheet;
            var assemblyName = compilation.Compilation.AssemblyName;
            var printer = new Printer(0, 1024 * 16, context.CancellationToken);

            // SheetContainer
            {
                printer.Clear();
                printer.PrintLineIf(databaseAuthoring, DEFINE_DATABASE_AUTHORING, DEFINE_NO_DATABASE_AUTHORING);
                printer.PrintLineIf(bakingSheet, DEFINE_BAKING_SHEET, DEFINE_NO_BAKING_SHEET);

                var hintName = SourceGenHelpers.BuildSemanticHintName(
                      "EncosyTower.Databases.Authoring.Generators.DatabaseAuthoringGenerator"
                    , assemblyName
                    , model.containerHintName
                    , "DatabaseAuthoringSheetContainer"
                    , string.Empty
                );

                var generatedSource = TypeCreationHelpers.GenerateSourceText(
                      model.openingSource
                    , model.WriteContainer(context.CancellationToken)
                    , model.closingSource
                    , context.CancellationToken
                    , printer
                );
                context.CancellationToken.ThrowIfCancellationRequested();
                context.AddSource(hintName, generatedSource);
            }

            foreach (var sheet in model.sheets)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                printer.Clear();
                printer.PrintLineIf(databaseAuthoring, DEFINE_DATABASE_AUTHORING, DEFINE_NO_DATABASE_AUTHORING);
                printer.PrintLineIf(bakingSheet, DEFINE_BAKING_SHEET, DEFINE_NO_BAKING_SHEET);

                var hintName = SourceGenHelpers.BuildSemanticHintName(
                      "EncosyTower.Databases.Authoring.Generators.DatabaseAuthoringGenerator"
                    , assemblyName
                    , sheet.hintName
                    , "DatabaseAuthoringSheet"
                    , string.Empty
                );

                var generatedSource = TypeCreationHelpers.GenerateSourceText(
                      model.openingSource
                    , model.WriteSheet(in sheet, context.CancellationToken)
                    , model.closingSource
                    , context.CancellationToken
                    , printer
                );
                context.CancellationToken.ThrowIfCancellationRequested();
                context.AddSource(hintName, generatedSource);
            }
        }
    }
}
