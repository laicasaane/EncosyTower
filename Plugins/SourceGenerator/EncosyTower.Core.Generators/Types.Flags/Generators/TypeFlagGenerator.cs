using EncosyTower.Core.Types.Flags;

namespace EncosyTower.Core.Generators.Types.Flags
{
    [Generator]
    internal sealed class TypeFlagGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilationProvider = context.CompilationProvider
                .Select(static (compilation, token) => CompilationSpec.Create(
                      compilation
                    , token
                    , TypeFlagRules.NAMESPACE
                    , TypeFlagRules.SKIP_ATTRIBUTE
                ));

            var specs = context.SyntaxProvider.ForAttributeWithMetadataName(
                  TypeFlagRules.ATTRIBUTE_METADATA_NAME
                , static (node, _) => node is TypeDeclarationSyntax
                , TypeFlagSpec.Extract
            ).WithTrackingName("TypeFlagGenerator.Candidates")
                .Where(static spec => spec.IsValid)
                .WithTrackingName("TypeFlagGenerator.ValidSpecs");

            var outputs = specs
                .Combine(compilationProvider)
                .Where(static pair => pair.Right.IsValid)
                .WithTrackingName("TypeFlagGenerator.Outputs");

            context.RegisterSourceOutput(outputs, static (sourceContext, pair) => {
                var token = sourceContext.CancellationToken;
                token.ThrowIfCancellationRequested();
                sourceContext.AddSource(pair.Left.HintName, TypeFlagSourceWriter.Write(pair.Left, token));
            });
        }
    }
}
