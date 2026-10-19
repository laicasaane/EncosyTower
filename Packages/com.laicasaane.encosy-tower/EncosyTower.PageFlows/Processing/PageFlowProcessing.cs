using EncosyTower.CodeGen;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.Processing;

namespace EncosyTower.PageFlows.Processing
{
    /// <summary>Asks the page flow of this scope whether it is in transition.</summary>
    /// <remarks>
    /// Every UGUI and UI Toolkit page flow registers a handler with <c>IsInTransitionRequest.Register</c>
    /// that answers with <see cref="IPageFlow.IsInTransition"/>.
    /// </remarks>
    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct IsInTransitionRequest() : IRequest<bool>;
}
