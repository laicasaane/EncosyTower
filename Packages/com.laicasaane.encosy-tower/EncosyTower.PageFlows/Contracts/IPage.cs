using System.Threading;
using EncosyTower.Common;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    public interface IPage { }

    public interface IPageOnCreateAsync : IPage
    {
        UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token);
    }

    public interface IPageOnReturnToPool : IPage
    {
        void OnReturnToPool(PageContext context);
    }

    public interface IPageOnAttachToFlowAsync : IPage
    {
        UnityTask<bool> OnAttachToFlowAsync(IPageFlow flow, PageContext context, CancellationToken token);
    }

    public interface IPageOnDetachFromFlowAsync : IPage
    {
        UnityTask<bool> OnDetachFromFlowAsync(IPageFlow flow, PageContext context, CancellationToken token);
    }

    public interface IPageHasOptions : IPage
    {
        PageOptions PageOptions { get; }
    }

    public interface IPageHasTransition : IPage
    {
        IPageTransition PageTransition { get; }
    }

    public interface IPageNeedsFlowScope : IPage
    {
        PageFlowScope FlowScope { set; }
    }

    public interface IPageNeedsMessageSubscriber : IPage
    {
        MessageSubscriber Subscriber { set; }
    }

    public interface IPageNeedsMessagePublisher : IPage
    {
        MessagePublisher Publisher { set; }
    }

    public interface IPageNeedsFlowScopeCollection<TCollection> : IPage
        where TCollection : struct, IPageFlowScopeCollection
    {
        Option<TCollection> FlowScopeCollection { set; }
    }
}
