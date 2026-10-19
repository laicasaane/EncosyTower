using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;

namespace EncosyTower.PageFlows
{
    /// <summary>What a flow needs from its owner to run: messaging, scope, services and settings.</summary>
    public interface IPageFlowContext
    {
        /// <summary>Gets the subscriber the flow and its pages receive messages from.</summary>
        MessageSubscriber Subscriber { get; }

        /// <summary>Gets the publisher the flow publishes its messages with.</summary>
        MessagePublisher Publisher { get; }

        /// <summary>Gets the scope of the flow. Its messages are published and received within this scope.</summary>
        PageFlowScope FlowScope { get; }

        /// <summary>Gets the applier that gives the pages of the flow their codex scope collection, if any.</summary>
        Option<IPageFlowScopeCollectionApplier> FlowScopeCollectionApplier { get; }

        /// <summary>Gets the tracker that keeps a page attached to one flow at a time, if any.</summary>
        Option<IPageOwnershipTracker> OwnershipTracker { get; }

        /// <summary>Gets whether a message that no subscriber received is returned as a failure.</summary>
        /// <value>
        /// When <c>true</c>, a message that the flow publishes during a call and that has no subscriber is added to
        /// the result of the call as a <c>SubscriberNotFound</c> case of <see cref="PageFlowError"/>; the call
        /// itself still runs. When <c>false</c>, the flow ignores such a message.
        /// </value>
        bool WarnNoSubscriber { get; }

        /// <summary>Whether list flows reject adding a page that is already in the list.</summary>
        /// <remarks>
        /// <see cref="PageOptions.rejectDuplicatePages"/> of a page applies when this is <c>false</c>.
        /// </remarks>
        bool RejectDuplicatePages { get; }

        /// <summary>Gets the logger the flow uses.</summary>
        ILogger Logger { get; }
    }
}
