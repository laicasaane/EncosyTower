using System.Buffers;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    public interface IPageFlowContext
    {
        ArrayPool<UnityTask> TaskArrayPool { get; }

        MessageSubscriber Subscriber { get; }

        MessagePublisher Publisher { get; }

        PageFlowScope FlowScope { get; }

        Option<IPageFlowScopeCollectionApplier> FlowScopeCollectionApplier { get; }

        bool WarnNoSubscriber { get; }

        ILogger Logger { get; }
    }
}
