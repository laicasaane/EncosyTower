using System.Threading;
using EncosyTower.CodeGen;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.PubSub;

namespace EncosyTower.PageFlows.PubSub
{
    /// <summary>Identifies one page flow. The flow publishes and receives its messages within its scope.</summary>
    /// <param name="Value">The identifier of the scope.</param>
    public readonly partial record struct PageFlowScope(Id3 Value);

    /// <summary>Published when a page attaches to a flow.</summary>
    /// <remarks>
    /// <para>
    /// The flow publishes it before it calls <see cref="IPageOnAttachToFlowAsync.OnAttachToFlowAsync"/>. An exception
    /// that a subscriber throws fails the attach.
    /// </para>
    /// <para>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </para>
    /// <para>
    /// Handle it with <c>AttachPageMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="Flow">The flow the page attaches to.</param>
    /// <param name="Page">The page that attaches.</param>
    /// <param name="Token">Cancels the attach.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct AttachPageMessage(IPageFlow Flow, IPage Page, CancellationToken Token);

    /// <summary>Published when a page detaches from a flow.</summary>
    /// <remarks>
    /// <para>
    /// The flow publishes it after it calls <see cref="IPageOnDetachFromFlowAsync.OnDetachFromFlowAsync"/>.
    /// </para>
    /// <para>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </para>
    /// <para>
    /// Handle it with <c>DetachPageMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="Flow">The flow the page detaches from.</param>
    /// <param name="Page">The page that detaches.</param>
    /// <param name="Token">Cancels the detach.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct DetachPageMessage(IPageFlow Flow, IPage Page, CancellationToken Token);

    /// <summary>Published when one page starts its show or hide transition.</summary>
    /// <remarks>
    /// <para>
    /// The flow publishes it before any hook or transition step of the transition runs.
    /// </para>
    /// <para>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </para>
    /// <para>
    /// Handle it with <c>PageTransitionBeganMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="Flow">The flow that runs the transition.</param>
    /// <param name="Page">The page that starts its transition.</param>
    /// <param name="Transition">Whether the page shows or hides.</param>
    /// <param name="OtherPage">
    /// The page that shows or hides in the same transition; none when several pages transition at once.
    /// </param>
    /// <param name="Token">Cancels the transition.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct PageTransitionBeganMessage(
          IPageFlow Flow
        , IPage Page
        , PageTransition Transition
        , Option<IPage> OtherPage
        , CancellationToken Token
    );

    /// <summary>Published when one page's show or hide transition has completed or been undone.</summary>
    /// <remarks>
    /// <para>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </para>
    /// <para>
    /// Handle it with <c>PageTransitionEndedMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="Flow">The flow that ran the transition.</param>
    /// <param name="Page">The page whose transition ended.</param>
    /// <param name="Transition">Whether the page showed or hid.</param>
    /// <param name="OtherPage">
    /// The page that showed or hid in the same transition; none when several pages transitioned at once.
    /// </param>
    /// <param name="TransitionState">Whether the transition completed or was undone.</param>
    /// <param name="Token">Cancels the transition.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct PageTransitionEndedMessage(
          IPageFlow Flow
        , IPage Page
        , PageTransition Transition
        , Option<IPage> OtherPage
        , PageTransitionState TransitionState
        , CancellationToken Token
    );
}
