namespace EncosyTower.PubSub.Analyzers
{
    internal sealed partial class PublishResultAnalyzer
    {
        public static readonly DiagnosticDescriptor PublishResultDiscarded = new(
              id: "SG_PUBSUB_0006"
            , title: "PubSub publish result is discarded"
            , messageFormat: "The result of '{0}' is discarded. PubSub never logs a failure it returns; "
                + "read the result and log each failure you react to."
            , category: "PubSub"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "PubSub never logs a failure it returns; read the result and log each failure "
                + "you react to."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(PublishResultDiscarded);
    }
}
