namespace EncosyTower.Mvvm.Generators.Binders
{
    [Generator]
    public sealed class BinderGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(BinderGenerator);
        public const string NAMESPACE = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        private const string BINDER_ATTRIBUTE_METADATA = "EncosyTower.Mvvm.ViewBinding.BinderAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      BINDER_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax
                    , BinderSpec.Extract
                )
                .WithTrackingName("BinderGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("BinderGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("BinderGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , BinderSpec declaration
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Mvvm.Generators.Binders.BinderGenerator"
                , compilation.AssemblyName
                , declaration.hintName
                , "Binder"
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
