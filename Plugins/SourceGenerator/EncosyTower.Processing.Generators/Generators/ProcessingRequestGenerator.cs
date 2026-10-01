namespace EncosyTower.Processing.Generators
{
    [Generator]
    internal sealed class ProcessingRequestGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var sharedRequests = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ProcessingSourceGenContract.PROCESSING_ATTRIBUTE
                    , static (node, _) => node is TypeDeclarationSyntax
                    , ProcessingRequestSpec.Extract
                )
                .Where(static spec => spec.IsValid)
                .WithTrackingName("ProcessingRequestGenerator.SharedRequests");

            var requestOutputs = sharedRequests
                .Select(static (spec, _) => spec.ToRequestOutput())
                .WithTrackingName("ProcessingRequestGenerator.RequestOutputs");

            var scopeOutputs = sharedRequests
                .SelectMany(static (spec, _) => spec.Scopes.AsImmutableArray())
                .WithTrackingName("ProcessingRequestGenerator.ScopeOutputs");

            context.RegisterSourceOutput(requestOutputs, static (sourceContext, spec) => {
                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                var generatedSource = ProcessingSourceWriter.WriteRequest(spec, sourceContext.CancellationToken);
                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                sourceContext.AddSource(spec.HintName, generatedSource);
            });

            context.RegisterSourceOutput(scopeOutputs, static (sourceContext, spec) => {
                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                var generatedSource = ProcessingSourceWriter.WriteScope(spec, sourceContext.CancellationToken);
                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                sourceContext.AddSource(spec.HintName, generatedSource);
            });
        }
    }
}
