namespace EncosyTower.Processing.Analyzers
{
    internal sealed partial class ProcessingRequestAnalyzer
    {
        public static readonly DiagnosticDescriptor InvalidScope = new(
              id: "SG_PROCESSING_0001"
            , title: "Processing scope is invalid"
            , messageFormat: "Request '{0}' uses scope '{1}', which cannot be used as a Processing scope."
            , category: "Processing"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Processing scopes must be closed, non-static, non-ref-like types that can be used "
                + "by the selected Processing hub."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor RepeatedScope = new(
              id: "SG_PROCESSING_0002"
            , title: "Processing scope is repeated"
            , messageFormat: "Scope '{0}' is declared more than once on request '{1}'. Keep exactly one "
                + "Processing attribute for this scope."
            , category: "Processing"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Each semantic Processing scope may be declared only once per request."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor ResultTypeConflict = new(
              id: "SG_PROCESSING_0003"
            , title: "Processing request result types conflict"
            , messageFormat: "Request '{0}' declares conflicting Processing result types: {1}."
            , category: "Processing"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "A Processing request may expose only one normalized result type through "
                + "global::EncosyTower.Processing.IRequest<TResult>."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor UnsupportedDeclaration = new(
              id: "SG_PROCESSING_0004"
            , title: "Processing request declaration is unsupported"
            , messageFormat: "Type '{0}' cannot generate Processing members."
            , category: "Processing"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Processing generation supports non-static, non-ref-like, non-file-local classes, "
                + "structs, record classes, and record structs."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor RedundantAsyncRequestInterface = new(
              id: "SG_PROCESSING_0005"
            , title: "Async request interface is redundant"
            , messageFormat: "Request '{0}' implements redundant async request interface '{1}'. Remove the "
                + "interface and use ApiMode.Async or ApiMode.Both."
            , category: "Processing"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "ProcessingAttribute with ApiMode.Async or ApiMode.Both creates the async request "
                + "interface and derives its result from global::EncosyTower.Processing.IRequest<TResult>."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor InvalidApiMode = new(
              id: "SG_PROCESSING_0006"
            , title: "Processing API mode is invalid"
            , messageFormat: "Request '{0}' uses API mode value '{1}', which is not a defined "
                + "EncosyTower.CodeGen.ApiMode."
            , category: "Processing"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Processing API mode must be Sync, Async, or Both."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor InvalidStateMode = new(
              id: "SG_PROCESSING_0007"
            , title: "Processing state mode is invalid"
            , messageFormat: "Request '{0}' uses state mode value '{1}', which is not a defined "
                + "EncosyTower.CodeGen.StateMode."
            , category: "Processing"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Processing state mode must be Stateless, Stateful, or Both."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  InvalidScope
                , RepeatedScope
                , ResultTypeConflict
                , UnsupportedDeclaration
                , RedundantAsyncRequestInterface
                , InvalidApiMode
                , InvalidStateMode
            );
    }
}
