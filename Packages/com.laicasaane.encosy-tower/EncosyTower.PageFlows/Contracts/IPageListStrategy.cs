using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    /// <summary>The add, remove and show operations of a page list.</summary>
    /// <remarks>
    /// <para>
    /// A page list holds its pages by index. Adding a page attaches it to the flow without showing it; removing a
    /// page detaches it without a transition.
    /// </para>
    /// <para>
    /// The factory overloads follow the factory and release contract of <see cref="IPageStackStrategy{TPage}"/>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public interface IPageListStrategy<TPage>
        where TPage : class, IPage
    {
        /// <summary>Gets the index of a page in the list.</summary>
        /// <param name="page">The page to find.</param>
        /// <returns>The index of the page, or <c>-1</c> when the list does not hold it.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="page"/> is <c>null</c> or a destroyed Unity object.
        /// </exception>
        int IndexOf(TPage page);

        /// <summary>Attaches a page to the flow and adds it to the list, without showing it.</summary>
        /// <param name="page">The page to add.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow added the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// When the list rejects duplicate pages, adding a page that the list already holds fails. The flow
        /// setting <see cref="IPageFlowContext.RejectDuplicatePages"/> and the page setting
        /// <see cref="PageOptions.rejectDuplicatePages"/> control this.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="page"/> is <c>null</c> or a destroyed Unity object.
        /// </exception>
        UnityTask<Success<PageFlowError>> AddAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        /// <summary>Attaches the page that <paramref name="factory"/> produces, and adds it to the list.</summary>
        /// <param name="factory">Produces the page to add, or the failure to return.</param>
        /// <param name="release">Releases a produced page that the flow does not hold.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow added the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="factory"/> or <paramref name="release"/> is <c>null</c>.
        /// </exception>
        UnityTask<Success<PageFlowError>> AddAsync(
              [NotNull] Func<CancellationToken, UnityTask<Result<TPage, PageFlowError>>> factory
            , [NotNull] Action<TPage> release
            , PageContext context
            , CancellationToken token
        );

        /// <summary>Detaches and removes every page without a transition.</summary>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow removed every page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// Every page is removed and deinitialized, even when a failure is returned. A rejection or a hook failure
        /// comes back with the <see cref="PageTransitionState.Completed"/> transition state.
        /// </remarks>
        UnityTask<Success<PageFlowError>> RemoveAllAsync(PageContext context, CancellationToken token);

        /// <summary>Detaches and removes a page without a transition.</summary>
        /// <param name="page">The page to remove.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow removed the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="page"/> is <c>null</c> or a destroyed Unity object.
        /// </exception>
        UnityTask<Success<PageFlowError>> RemoveAsync(
              [NotNull] TPage page
            , PageContext context
            , CancellationToken token
        );

        /// <summary>Detaches and removes the page at an index without a transition.</summary>
        /// <param name="index">The index of the page to remove.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow removed the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        UnityTask<Success<PageFlowError>> RemoveAsync(int index, PageContext context, CancellationToken token);

        /// <summary>Shows a page.</summary>
        /// <param name="page">The page to show.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow showed the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// The flow adds a page that is not in the list first. When the show is not applied, it removes that page
        /// again.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="page"/> is <c>null</c> or a destroyed Unity object.
        /// </exception>
        UnityTask<Success<PageFlowError>> ShowAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        /// <summary>Shows the page that <paramref name="factory"/> produces.</summary>
        /// <param name="factory">Produces the page to show, or the failure to return.</param>
        /// <param name="release">Releases a produced page that the flow does not hold.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow showed the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="factory"/> or <paramref name="release"/> is <c>null</c>.
        /// </exception>
        UnityTask<Success<PageFlowError>> ShowAsync(
              [NotNull] Func<CancellationToken, UnityTask<Result<TPage, PageFlowError>>> factory
            , [NotNull] Action<TPage> release
            , PageContext context
            , CancellationToken token
        );

        /// <summary>Shows the page at an index of the list.</summary>
        /// <param name="index">The index of the page to show.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow showed the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        UnityTask<Success<PageFlowError>> ShowAsync(int index, PageContext context, CancellationToken token);
    }
}
