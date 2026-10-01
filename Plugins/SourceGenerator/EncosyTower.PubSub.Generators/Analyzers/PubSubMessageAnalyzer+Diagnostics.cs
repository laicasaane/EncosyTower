namespace EncosyTower.PubSub.Analyzers
{
    internal sealed partial class PubSubMessageAnalyzer
    {
        public static readonly DiagnosticDescriptor InvalidScope = new(
              id: "SG_PUBSUB_0001"
            , title: "PubSub scope is invalid"
            , messageFormat: "Message '{0}' uses scope '{1}', which cannot be used as a PubSub scope."
            , category: "PubSub"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "PubSub scopes must be closed, non-static, non-ref-like types that can be used "
                + "by the selected PubSub publisher."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor RepeatedScope = new(
              id: "SG_PUBSUB_0002"
            , title: "PubSub scope is repeated"
            , messageFormat: "Scope '{0}' is declared more than once on message '{1}'. Keep exactly one "
                + "PubSub attribute for this scope."
            , category: "PubSub"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Each semantic PubSub scope may be declared only once per message."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor UnsupportedDeclaration = new(
              id: "SG_PUBSUB_0003"
            , title: "PubSub message declaration is unsupported"
            , messageFormat: "Type '{0}' cannot generate PubSub members."
            , category: "PubSub"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "PubSub generation supports non-static, non-ref-like, non-file-local classes, "
                + "structs, record classes, and record structs."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor InvalidApiMode = new(
              id: "SG_PUBSUB_0004"
            , title: "PubSub API mode is invalid"
            , messageFormat: "Message '{0}' uses API mode value '{1}', which is not a defined "
                + "EncosyTower.CodeGen.ApiMode."
            , category: "PubSub"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "PubSub API mode must be Sync, Async, or Both."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor InvalidStateMode = new(
              id: "SG_PUBSUB_0005"
            , title: "PubSub state mode is invalid"
            , messageFormat: "Message '{0}' uses state mode value '{1}', which is not a defined "
                + "EncosyTower.CodeGen.StateMode."
            , category: "PubSub"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "PubSub state mode must be Stateless, Stateful, or Both."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  InvalidScope
                , RepeatedScope
                , UnsupportedDeclaration
                , InvalidApiMode
                , InvalidStateMode
            );
    }
}
