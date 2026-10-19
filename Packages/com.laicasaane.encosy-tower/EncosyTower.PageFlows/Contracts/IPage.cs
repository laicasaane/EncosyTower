using System.Threading;
using EncosyTower.Common;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    /// <summary>Marks a type as a page that a page flow can show and hide.</summary>
    /// <remarks>
    /// A page takes part in the life of a flow by implementing the interfaces named <c>IPageOn*</c>
    /// (hooks), <c>IPageHas*</c> (configuration) and <c>IPageNeeds*</c> (values the flow sets on attach).
    /// All page flow operations and callbacks run on the Unity main thread; see <see cref="IPageFlow"/>.
    /// </remarks>
    public interface IPage { }

    /// <summary>A page that is notified when a flow rents it, before it attaches to the flow.</summary>
    /// <remarks>
    /// A UGUI or UI Toolkit page flow calls <see cref="OnCreateAsync"/> each time it rents the page from its
    /// pool, whether the instance is new or pooled. The flow gives the page a context whose abort handle has the
    /// page as its origin.
    /// </remarks>
    public interface IPageOnCreateAsync : IPage
    {
        /// <summary>Prepares the page after the flow rents it.</summary>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        /// <param name="token">Cancels the preparation.</param>
        /// <returns>
        /// <c>true</c> to accept the page; <c>false</c> to reject it. The flow then returns the page to its pool
        /// and fails with the <c>CreateRejected</c> case of <see cref="PageFlowError"/>.
        /// </returns>
        UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token);
    }

    /// <summary>A page that is notified when a flow returns it to its pool.</summary>
    public interface IPageOnReturnToPool : IPage
    {
        /// <summary>Called before the flow pools or destroys the page.</summary>
        /// <param name="context">
        /// The context of the call. <see cref="PageContext.ReturnOperation"/> tells whether the flow pools or
        /// destroys the page.
        /// </param>
        void OnReturnToPool(PageContext context);
    }

    /// <summary>A page that is notified when it attaches to a flow.</summary>
    /// <remarks>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </remarks>
    public interface IPageOnAttachToFlowAsync : IPage
    {
        /// <summary>Called after the flow published its <see cref="AttachPageMessage"/>.</summary>
        /// <param name="flow">The flow the page attaches to.</param>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        /// <param name="token">Cancels the attach.</param>
        /// <returns>
        /// <c>true</c> to accept the attach; <c>false</c> to reject it. The flow then undoes the attach and fails
        /// with the <c>AttachRejected</c> case of <see cref="PageFlowError"/>.
        /// </returns>
        UnityTask<bool> OnAttachToFlowAsync(IPageFlow flow, PageContext context, CancellationToken token);
    }

    /// <summary>A page that is notified when it detaches from a flow.</summary>
    /// <remarks>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </remarks>
    public interface IPageOnDetachFromFlowAsync : IPage
    {
        /// <summary>Called before the flow publishes its <see cref="DetachPageMessage"/>.</summary>
        /// <param name="flow">The flow the page detaches from.</param>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        /// <param name="token">Cancels the detach.</param>
        /// <returns>
        /// <c>true</c> to accept the detach; <c>false</c> to reject it. A rejection fails the call with the
        /// <c>DetachRejected</c> case of <see cref="PageFlowError"/>. A detach the flow forces, such as the one
        /// after a pop, completes anyway and reports the rejection in its result.
        /// </returns>
        UnityTask<bool> OnDetachFromFlowAsync(IPageFlow flow, PageContext context, CancellationToken token);
    }

    /// <summary>A page that sets its own <see cref="PageOptions"/>.</summary>
    public interface IPageHasOptions : IPage
    {
        /// <summary>The options a flow reads when it shows or hides the page.</summary>
        PageOptions PageOptions { get; }
    }

    /// <summary>A page that animates its own show and hide.</summary>
    public interface IPageHasTransition : IPage
    {
        /// <summary>The transition a flow runs when it shows or hides the page.</summary>
        IPageTransition PageTransition { get; }
    }

    /// <summary>A page that receives the <see cref="PageFlowScope"/> of the flow it attaches to.</summary>
    public interface IPageNeedsFlowScope : IPage
    {
        /// <summary>Set to the scope of the flow on attach, and to <c>default</c> on detach.</summary>
        PageFlowScope FlowScope { set; }
    }

    /// <summary>A page that receives the <see cref="MessageSubscriber"/> of the flow it attaches to.</summary>
    public interface IPageNeedsMessageSubscriber : IPage
    {
        /// <summary>Set to the subscriber of the flow on attach, and to <c>default</c> on detach.</summary>
        MessageSubscriber Subscriber { set; }
    }

    /// <summary>A page that receives the <see cref="MessagePublisher"/> of the flow it attaches to.</summary>
    public interface IPageNeedsMessagePublisher : IPage
    {
        /// <summary>Set to the publisher of the flow on attach, and to <c>default</c> on detach.</summary>
        MessagePublisher Publisher { set; }
    }

    /// <summary>A page that receives the scope collection of the codex whose flow it attaches to.</summary>
    /// <typeparam name="TCollection">The scope collection struct of the codex.</typeparam>
    public interface IPageNeedsFlowScopeCollection<TCollection> : IPage
        where TCollection : struct, IPageFlowScopeCollection
    {
        /// <summary>The scope collection of the codex.</summary>
        /// <remarks>
        /// Set when the page attaches to a flow whose codex has a scope collection applier. It is not cleared on
        /// detach; the old value stays until the next attach sets it.
        /// </remarks>
        Option<TCollection> FlowScopeCollection { set; }
    }
}
