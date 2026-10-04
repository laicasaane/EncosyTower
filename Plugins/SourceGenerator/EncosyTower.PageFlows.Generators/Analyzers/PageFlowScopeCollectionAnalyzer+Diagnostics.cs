namespace EncosyTower.PageFlows.Analyzers
{
    internal sealed partial class PageFlowScopeCollectionAnalyzer
    {
        public static readonly DiagnosticDescriptor NotPartial = new(
              id: "SG_PAGEFLOWS_0001"
            , title: "Page flow scope collection is not partial"
            , messageFormat: "'{0}' must be declared partial because '{1}' has [PageFlowScopeCollection]."
            , category: "PageFlows"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The generator adds members to the attributed struct, so the struct and every "
                + "containing type must be partial."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor ReadOnlyOrRef = new(
              id: "SG_PAGEFLOWS_0002"
            , title: "Page flow scope collection is readonly or ref"
            , messageFormat: "'{0}' has [PageFlowScopeCollection] and cannot be readonly or ref, because the "
                + "generated TrySetScope assigns its properties."
            , category: "PageFlows"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "A page flow scope collection must be a mutable, non-ref struct."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor NoScope = new(
              id: "SG_PAGEFLOWS_0003"
            , title: "Page flow scope collection has no scope"
            , messageFormat: "'{0}' has no instance property of type PageFlowScope with a setter that is not init."
            , category: "PageFlows"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "A page flow scope collection needs at least one instance PageFlowScope property "
                + "with a setter that is not init."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor PropertyNotScope = new(
              id: "SG_PAGEFLOWS_0004"
            , title: "PageFlowScope property is not a scope"
            , messageFormat: "Property '{0}' of '{1}' is get-only or init-only, so it is not a scope."
            , category: "PageFlows"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Only PageFlowScope properties with a setter that is not init are scopes."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor FieldIgnored = new(
              id: "SG_PAGEFLOWS_0005"
            , title: "PageFlowScope field is ignored"
            , messageFormat: "Field '{0}' of '{1}' is not a scope; only properties are scopes."
            , category: "PageFlows"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Page flow scope collections read scopes from properties only."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public static readonly DiagnosticDescriptor ImplementedByHand = new(
              id: "SG_PAGEFLOWS_0006"
            , title: "IPageFlowScopeCollection is implemented by hand"
            , messageFormat: "'{0}' implements IPageFlowScopeCollection without [PageFlowScopeCollection]; "
                + "declare a partial struct with the attribute instead."
            , category: "PageFlows"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "IPageFlowScopeCollection implementations are generated for partial structs that "
                + "have [PageFlowScopeCollection]."
            , helpLinkUri: null
            , customTags: Array.Empty<string>()
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  NotPartial
                , ReadOnlyOrRef
                , NoScope
                , PropertyNotScope
                , FieldIgnored
                , ImplementedByHand
            );
    }
}
