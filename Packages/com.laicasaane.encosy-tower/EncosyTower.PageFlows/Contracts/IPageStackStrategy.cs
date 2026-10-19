using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    /// <summary>The push and pop operations of a page stack.</summary>
    /// <remarks>
    /// The stack does not check whether a page is already in it; push a new page instance.
    /// </remarks>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public interface IPageStackStrategy<TPage>
        where TPage : class, IPage
    {
        /// <summary>Hides and removes the current page.</summary>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow hid and removed the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        UnityTask<Success<PageFlowError>> PopAsync(PageContext context, CancellationToken token);

        /// <summary>Pushes a page and shows it.</summary>
        /// <param name="page">The page to push.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow showed the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="page"/> is <c>null</c> or a destroyed Unity object.
        /// </exception>
        UnityTask<Success<PageFlowError>> PushAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        /// <summary>Pushes the page that <paramref name="factory"/> produces, and shows it.</summary>
        /// <param name="factory">Produces the page to push, or the failure to return.</param>
        /// <param name="release">Releases a produced page that the flow does not hold.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow showed the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The flow calls <paramref name="factory"/> before it validates the page or the flow state, so a page can be
        /// produced for a call that then fails. A failure that <paramref name="factory"/> returns is returned by the
        /// call.
        /// </para>
        /// <para>
        /// When the call fails, aborts, is cancelled or is dropped after <paramref name="factory"/> produced a
        /// page, and the flow does not hold the page, the flow calls <paramref name="release"/> with it. A failure
        /// or an exception thrown by <paramref name="release"/> is added to the returned failure as a suppressed
        /// error, or logged when the call throws.
        /// </para>
        /// <para>The flow never calls <paramref name="release"/> for a page it holds.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// async UnityTask&lt;Result&lt;MyPage, PageFlowError&gt;&gt; CreateAsync(CancellationToken token)
        /// {
        ///     MyPage page = await MyPages.CreateAsync(token); // your own page creation
        ///     return page;
        /// }
        ///
        /// var result = await stack.PushAsync(CreateAsync, MyPages.Release, context, token);
        ///
        /// if (result.TryGetFailure(out var error)) error.Log(logger);
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="factory"/> or <paramref name="release"/> is <c>null</c>.
        /// </exception>
        UnityTask<Success<PageFlowError>> PushAsync(
              [NotNull] Func<CancellationToken, UnityTask<Result<TPage, PageFlowError>>> factory
            , [NotNull] Action<TPage> release
            , PageContext context
            , CancellationToken token
        );
    }
}
