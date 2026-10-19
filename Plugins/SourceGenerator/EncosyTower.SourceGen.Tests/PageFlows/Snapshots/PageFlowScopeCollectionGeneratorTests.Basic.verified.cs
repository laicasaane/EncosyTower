#pragma warning disable 0219

using EncosyTower.PageFlows;
using EncosyTower.PageFlows.PubSub;
using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__ETPF = global::EncosyTower.PageFlows;
using g__ETPFPS = global::EncosyTower.PageFlows.PubSub;

namespace TestProject
{


partial struct GamePageFlowScopes : g__ETPF.IPageFlowScopeCollection
{
    [g__SCDC.GeneratedCode("EncosyTower.PageFlows.Generators.PageFlowScopeCollectionGenerator", "0.1.8-preview.8")]
    public static class Identifiers
    {
        public const string Screen = nameof(Screen);
        public const string Popup = nameof(Popup);
    }

    [g__SCDC.GeneratedCode("EncosyTower.PageFlows.Generators.PageFlowScopeCollectionGenerator", "0.1.8-preview.8")]
    private static readonly string[] s_scopeIdentifiers = new string[] { Identifiers.Screen, Identifiers.Popup, };

    [g__SCDC.GeneratedCode("EncosyTower.PageFlows.Generators.PageFlowScopeCollectionGenerator", "0.1.8-preview.8")]
    public readonly g__S.ReadOnlyMemory<string> ScopeIdentifiers => s_scopeIdentifiers;

    [g__SCDC.GeneratedCode("EncosyTower.PageFlows.Generators.PageFlowScopeCollectionGenerator", "0.1.8-preview.8")]
    public bool TrySetScope(string identifier, g__ETPFPS.PageFlowScope scope)
    {
        switch (identifier)
        {
            case nameof(Screen):
            {
                this.Screen = scope;
                return true;
            }

            case nameof(Popup):
            {
                this.Popup = scope;
                return true;
            }

            default:
            {
                return false;
            }
        }
    }
}


}
