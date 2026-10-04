using EncosyTower.CodeGen;
using EncosyTower.PubSub;

namespace EncosyTower.PageFlows
{
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct PrepoolPageMessage(string AssetKey, int Amount);

    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct TrimPoolMessage(string AssetKey, int AmountToKeep);

    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct AddPageMessage(string AssetKey, PageContext Context);

    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct ShowPageMessage(string AssetKey, PageContext Context);

    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct ShowPageAtIndexMessage(int Index, PageContext Context);

    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct HidePageAtIndexMessage(int Index, PageContext Context);

    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct HideActivePageMessage(PageContext Context);
}
