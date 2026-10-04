using EncosyTower.Tasks;

namespace EncosyTower.PageFlows.UitkPages
{
    public interface IUitkPageCodexOnInitialize
    {
        IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier { get; }

        UnityTask OnInitializeAsync(UitkPageCodex codex);
    }
}
