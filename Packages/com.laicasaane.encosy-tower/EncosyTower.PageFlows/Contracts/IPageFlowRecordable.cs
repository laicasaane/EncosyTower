using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;

namespace EncosyTower.PageFlows
{
    /// <summary>A page flow that navigation histories can record and replay.</summary>
    public interface IPageFlowRecordable
    {
        /// <summary>The flow's identifier in its codex.</summary>
        string FlowId { get; }

        /// <summary>Gets the kind of the flow. A history chooses the navigator that replays the flow by it.</summary>
        PageFlowKind Kind { get; }

        /// <summary>Gets the scope of the flow.</summary>
        PageFlowScope FlowScope { get; }

        /// <summary>Gets the subscriber a history receives <see cref="PageNavigatedMessage"/> from.</summary>
        MessageSubscriber Subscriber { get; }

        /// <summary>Gets the publisher a navigator replays calls with.</summary>
        MessagePublisher Publisher { get; }

        /// <summary>False once the flow is destroyed or disposed.</summary>
        bool IsAlive { get; }

        /// <summary>True when the flow shows at least one page.</summary>
        bool IsShowingPages { get; }
    }
}
