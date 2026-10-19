using EncosyTower.CodeGen;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Samples.UguiPages.Config;

namespace EncosyTower.Samples.UguiPages.Messages
{

    /// <summary>A page asks the Book to do an action.</summary>
    /// <remarks>
    /// A page publishes it in the Screen scope with <c>AppActionMessage.Publish</c>; the codex handles it with
    /// <c>AppActionMessage.Subscribe</c>. It shows how a sample defines its own message beside the PageFlows
    /// messages.
    /// </remarks>
    /// <param name="Action">What to do.</param>
    /// <param name="Location">The location that the action travels to, or the default location.</param>
    /// <param name="Index">The index of the tab or card that the action needs, or 0.</param>
    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    internal readonly partial record struct AppActionMessage(AppAction Action, BookLocation Location, int Index);
}
