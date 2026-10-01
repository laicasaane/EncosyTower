namespace EncosyTower.Mvvm.Generators.ObservableProperties
{
    [Generator]
    public sealed class ObservablePropertyGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(ObservablePropertyGenerator);
        public const string NAMESPACE = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ObservablePropertySpec.OBSERVABLE_OBJECT_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax
                    , ObservablePropertySpec.Extract
                )
                .WithTrackingName("ObservablePropertyGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("ObservablePropertyGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("ObservablePropertyGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , ObservablePropertySpec declaration
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator"
                , compilation.AssemblyName
                , declaration.hintName
                , "ObservableObject"
                , string.Empty
            );

            var hasMembers = declaration.fieldRefs.Count > 0 || declaration.propRefs.Count > 0;
            var source = hasMembers
                ? declaration.WriteCode(context.CancellationToken)
                : declaration.WriteCodeWithoutMember(context.CancellationToken);

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  declaration.openingSource
                , source
                , declaration.closingSource
                , context.CancellationToken
            );

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }
    }
}
