namespace EncosyTower.PageFlows.Generators
{
    [Generator]
    internal sealed class PageFlowScopeCollectionGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var specs = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      PageFlowScopeCollectionSourceGenContract.ATTRIBUTE
                    , static (node, _) => node is StructDeclarationSyntax
                    , PageFlowScopeCollectionSpec.Extract
                )
                .Where(static spec => spec.IsValid)
                .WithTrackingName("PageFlowScopeCollectionGenerator.Specs");

            context.RegisterSourceOutput(specs, static (sourceContext, spec) => {
                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                var generatedSource = PageFlowScopeCollectionSourceWriter.Write(spec, sourceContext.CancellationToken);
                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                sourceContext.AddSource(spec.HintName, generatedSource);
            });
        }
    }
}
