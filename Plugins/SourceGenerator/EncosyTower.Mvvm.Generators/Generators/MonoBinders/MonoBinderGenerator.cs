namespace EncosyTower.Mvvm.Generators.MonoBinders
{
    [Generator]
    public sealed class MonoBinderGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(MonoBinderGenerator);
        public const string NAMESPACE = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        private const string MONO_BINDER_ATTRIBUTE_METADATA =
            "EncosyTower.Mvvm.ViewBinding.Components.MonoBinderAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      MONO_BINDER_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax
                    , MonoBinderSpec.Extract
                )
                .WithTrackingName("MonoBinderGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("MonoBinderGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("MonoBinderGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , MonoBinderSpec declaration
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Mvvm.Generators.MonoBinders.MonoBinderGenerator"
                , compilation.AssemblyName
                , declaration.hintName
                , "MonoBinder"
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
