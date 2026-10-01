namespace EncosyTower.Mvvm.Generators.RelayCommands
{
    [Generator]
    public sealed class RelayCommandGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        public const string ATTRIBUTE = "RelayCommand";
        public const string INPUT_NAMESPACE = $"{NAMESPACE}.Input";
        public const string GENERATOR_NAME = nameof(RelayCommandGenerator);

        private const string OBSERVABLE_OBJECT_ATTRIBUTE_METADATA = "EncosyTower.Mvvm.ComponentModel.ObservableObjectAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      OBSERVABLE_OBJECT_ATTRIBUTE_METADATA
                    , static (node, c) => node is ClassDeclarationSyntax s && HasAnyRelayCommandMethod(s, c)
                    , RelayCommandSpec.Extract
                )
                .WithTrackingName("RelayCommandGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("RelayCommandGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("RelayCommandGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static bool HasAnyRelayCommandMethod(ClassDeclarationSyntax cls, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var member in cls.Members)
            {
                token.ThrowIfCancellationRequested();

                if (member is MethodDeclarationSyntax method && method.HasAttributeCandidate(INPUT_NAMESPACE, ATTRIBUTE, token))
                {
                    return true;
                }
            }

            return false;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , RelayCommandSpec declaration
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Mvvm.Generators.RelayCommands.RelayCommandGenerator"
                , compilation.AssemblyName
                , declaration.hintName
                , "RelayCommand"
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
