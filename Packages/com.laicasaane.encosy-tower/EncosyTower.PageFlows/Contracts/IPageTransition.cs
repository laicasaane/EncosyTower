using System.Threading;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    /// <summary>Animates the show and hide of one page.</summary>
    /// <remarks>
    /// <para>
    /// A flow runs the steps of a transition in this order: <see cref="OnBeforeTransitionAsync"/> (together with
    /// the before hooks of the pages), <see cref="OnShowAsync"/> and <see cref="OnHideAsync"/>, then
    /// <see cref="OnAfterTransition"/>. A step that returns a <see cref="UnityTask"/> is awaited.
    /// </para>
    /// <para>
    /// <see cref="PageTransitionOptions.NoTransition"/> skips <see cref="OnShowAsync"/> and
    /// <see cref="OnHideAsync"/>, unless <see cref="ForceRunShow"/> or <see cref="ForceRunHide"/> is <c>true</c>.
    /// <see cref="PageTransitionOptions.ZeroDuration"/> asks the transition to apply the end state at once; the
    /// transition decides how.
    /// </para>
    /// <para>
    /// Stop the animation when <see cref="PageAbortHandle.Token"/> of <see cref="PageTransitionContext.Abort"/>
    /// is cancelled.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public sealed class FadeTransition : IPageTransition
    /// {
    ///     public bool ForceRunHide => false;
    ///
    ///     public bool ForceRunShow => false;
    ///
    ///     public UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
    ///         => FadeAsync(1f, context.ShowOptions, context.Abort.Token, token);
    ///
    ///     // OnHideAsync fades to 0f with context.HideOptions; OnBeforeTransitionAsync and
    ///     // OnAfterTransition do nothing. FadeAsync is your own animation: it jumps to the end when the
    ///     // options contain PageTransitionOptions.ZeroDuration and stops when the abort token is cancelled.
    /// }
    /// </code>
    /// </example>
    public interface IPageTransition
    {
        /// <summary>
        /// Gets whether <see cref="OnHideAsync"/> runs even with <see cref="PageTransitionOptions.NoTransition"/>.
        /// </summary>
        bool ForceRunHide { get; }

        /// <summary>
        /// Gets whether <see cref="OnShowAsync"/> runs even with <see cref="PageTransitionOptions.NoTransition"/>.
        /// </summary>
        bool ForceRunShow { get; }

        /// <summary>Prepares the page before the transition starts.</summary>
        /// <param name="context">What this call is for.</param>
        /// <param name="token">Cancels the step.</param>
        UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token);

        /// <summary>Called after the show or hide ran.</summary>
        /// <param name="context">What this call is for.</param>
        void OnAfterTransition(PageTransitionContext context);

        /// <summary>Animates the show of the page.</summary>
        /// <param name="context">What this call is for.</param>
        /// <param name="token">Cancels the animation.</param>
        UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token);

        /// <summary>Animates the hide of the page.</summary>
        /// <param name="context">What this call is for.</param>
        /// <param name="token">Cancels the animation.</param>
        UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token);
    }

    /// <summary>A page that is notified before its show starts, and can delay it.</summary>
    /// <remarks>
    /// <para>
    /// The flow awaits this step together with the other before steps of both pages.
    /// </para>
    /// <para>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </para>
    /// </remarks>
    public interface IPageOnBeforeShowAsync : IPage
    {
        /// <summary>Prepares the page before it shows.</summary>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        /// <param name="token">Cancels the step.</param>
        UnityTask OnBeforeShowAsync(PageContext context, CancellationToken token);
    }

    /// <summary>A page that is notified before its hide starts, and can delay it.</summary>
    /// <remarks>
    /// <para>
    /// The flow awaits this step together with the other before steps of both pages.
    /// </para>
    /// <para>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </para>
    /// </remarks>
    public interface IPageOnBeforeHideAsync : IPage
    {
        /// <summary>Prepares the page before it hides.</summary>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        /// <param name="token">Cancels the step.</param>
        UnityTask OnBeforeHideAsync(PageContext context, CancellationToken token);
    }

    /// <summary>A page that is notified when its show is about to start.</summary>
    /// <remarks>
    /// <para>
    /// The flow calls this hook after every before step of both pages has completed.
    /// </para>
    /// <para>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </para>
    /// </remarks>
    public interface IPageOnBeforeShow : IPage
    {
        /// <summary>Called right before the page shows.</summary>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        void OnBeforeShow(PageContext context);
    }

    /// <summary>A page that is notified when its hide is about to start.</summary>
    /// <remarks>
    /// <para>
    /// The flow calls this hook after every before step of both pages has completed.
    /// </para>
    /// <para>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </para>
    /// </remarks>
    public interface IPageOnBeforeHide : IPage
    {
        /// <summary>Called right before the page hides.</summary>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        void OnBeforeHide(PageContext context);
    }

    /// <summary>A page that is notified after its show ran.</summary>
    /// <remarks>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </remarks>
    public interface IPageOnAfterShow : IPage
    {
        /// <summary>Called after the page showed.</summary>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        void OnAfterShow(PageContext context);
    }

    /// <summary>A page that is notified after its hide ran.</summary>
    /// <remarks>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </remarks>
    public interface IPageOnAfterHide : IPage
    {
        /// <summary>Called after the page hid.</summary>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        void OnAfterHide(PageContext context);
    }

    /// <summary>A page that animates its own show.</summary>
    /// <remarks>
    /// <para>
    /// The flow skips this step when the show options contain <see cref="PageTransitionOptions.NoTransition"/>.
    /// </para>
    /// <para>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </para>
    /// </remarks>
    public interface IPageOnShowAsync : IPage
    {
        /// <summary>Animates the show of the page.</summary>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        /// <param name="token">Cancels the animation.</param>
        UnityTask OnShowAsync(PageContext context, CancellationToken token);
    }

    /// <summary>A page that animates its own hide.</summary>
    /// <remarks>
    /// <para>
    /// The flow skips this step when the hide options contain <see cref="PageTransitionOptions.NoTransition"/>.
    /// </para>
    /// <para>
    /// Code that runs inside a page flow operation must not await another operation on the same flow:
    /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
    /// </para>
    /// </remarks>
    public interface IPageOnHideAsync : IPage
    {
        /// <summary>Animates the hide of the page.</summary>
        /// <param name="context">The context of the call, with the page as the origin of its abort handle.</param>
        /// <param name="token">Cancels the animation.</param>
        UnityTask OnHideAsync(PageContext context, CancellationToken token);
    }
}
