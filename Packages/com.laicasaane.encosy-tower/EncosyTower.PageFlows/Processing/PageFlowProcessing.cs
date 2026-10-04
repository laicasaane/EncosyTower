using EncosyTower.CodeGen;
using EncosyTower.Processing;

namespace EncosyTower.PageFlows
{
    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct IsInTransitionRequest() : IRequest<bool>;
}
