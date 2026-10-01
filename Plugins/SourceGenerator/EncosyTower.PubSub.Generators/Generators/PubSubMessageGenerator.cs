namespace EncosyTower.PubSub.Generators
{
    [Generator]
    internal sealed class PubSubMessageGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var sharedMessages = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      PubSubSourceGenContract.PUBSUB_ATTRIBUTE
                    , static (node, _) => node is TypeDeclarationSyntax
                    , PubSubMessageSpec.Extract
                )
                .Where(static spec => spec.IsValid)
                .WithTrackingName("PubSubMessageGenerator.SharedMessages");

            var messageOutputs = sharedMessages
                .Select(static (spec, _) => spec.ToMessageOutput())
                .WithTrackingName("PubSubMessageGenerator.MessageOutputs");

            var scopeOutputs = sharedMessages
                .SelectMany(static (spec, _) => spec.Scopes.AsImmutableArray())
                .WithTrackingName("PubSubMessageGenerator.ScopeOutputs");

            context.RegisterSourceOutput(messageOutputs, static (sourceContext, spec) => {
                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                var generatedSource = PubSubSourceWriter.WriteMessage(spec, sourceContext.CancellationToken);
                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                sourceContext.AddSource(spec.HintName, generatedSource);
            });

            context.RegisterSourceOutput(scopeOutputs, static (sourceContext, spec) => {
                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                var generatedSource = PubSubSourceWriter.WriteScope(spec, sourceContext.CancellationToken);
                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                sourceContext.AddSource(spec.HintName, generatedSource);
            });
        }
    }
}
