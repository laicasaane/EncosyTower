using EncosyTower.CodeGen;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Samples.UitkPages.Config;

namespace EncosyTower.Samples.UitkPages.Messages
{
    /// <summary>A page asks the Book to do an action.</summary>
    /// <remarks>
    /// A page publishes it in the Screen scope with <c>AppActionMessage.Publish</c>; the codex handles it with
    /// <c>AppActionMessage.Subscribe</c>. It shows how a sample defines its own message beside the PageFlows
    /// messages.
    /// </remarks>
    /// <param name="Action">What to do.</param>
    /// <param name="Location">The place to travel to; the default location when the action does not travel.</param>
    /// <param name="Index">The index of a tab or card that the action needs; 0 when it needs none.</param>
    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    internal readonly partial record struct AppActionMessage(
          AppAction Action
        , BookLocation Location
        , int Index
    );
}
