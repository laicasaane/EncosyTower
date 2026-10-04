#pragma warning disable 0219

using EncosyTower.PageFlows;
using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__ETPF = global::EncosyTower.PageFlows;

namespace TestProject
{


partial struct GamePageFlowScopes : g__ETPF.IPageFlowScopeCollection
{
    [g__SCDC.GeneratedCode("EncosyTower.PageFlows.Generators.PageFlowScopeCollectionGenerator", "0.1.8-preview.4")]
    private static readonly string[] s_scopeIdentifiers = new string[] { nameof(Screen), nameof(Popup), };

    [g__SCDC.GeneratedCode("EncosyTower.PageFlows.Generators.PageFlowScopeCollectionGenerator", "0.1.8-preview.4")]
    public readonly g__S.ReadOnlyMemory<string> ScopeIdentifiers => s_scopeIdentifiers;

    [g__SCDC.GeneratedCode("EncosyTower.PageFlows.Generators.PageFlowScopeCollectionGenerator", "0.1.8-preview.4")]
    public bool TrySetScope(string identifier, g__ETPF.PageFlowScope scope)
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
