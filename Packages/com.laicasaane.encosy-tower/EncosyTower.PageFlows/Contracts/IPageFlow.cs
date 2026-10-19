using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    /// <summary>A flow that shows and hides pages.</summary>
    /// <remarks>
    /// <para>All page flow operations and callbacks run on the Unity main thread.</para>
    /// <para>
    /// A flow runs one transition at a time. A call made while the flow is in transition follows
    /// <see cref="PageContext.AsyncOperation"/>.
    /// </para>
    /// </remarks>
    public interface IPageFlow
    {
        /// <summary>Gets whether the flow is running a transition.</summary>
        bool IsInTransition { get; }

        /// <summary>Aborts the show or hide this flow is running.</summary>
        /// <param name="mode">How the screen goes back.</param>
        /// <returns>
        /// Success when the flow accepted the request; otherwise a failure; see <see cref="PageAbortError"/>.
        /// </returns>
        /// <remarks>
        /// The request is reported with the flow as its origin. When the flow runs no call but has calls waiting,
        /// the request goes to the first waiting call.
        /// </remarks>
        Success<PageAbortError> AbortCurrent(PageAbortMode mode);
    }

    /// <summary>
    /// A collection of <see cref="PageFlowScope"/> values, one for each page flow.
    /// </summary>
    /// <remarks>
    /// Declare a <c>partial struct</c> with <see cref="PageFlowScopeCollectionAttribute"/>;
    /// the source generator implements this interface.
    /// </remarks>
    /// <example>
    /// <code>
    /// [PageFlowScopeCollection]
    /// public partial struct GamePageFlowScopes
    /// {
    ///     public PageFlowScope Screen { get; private set; }
    ///
    ///     public PageFlowScope Popup { get; private set; }
    ///
    ///     public PageFlowScope FreeTop { get; private set; }
    /// }
    /// </code>
    /// </example>
    public interface IPageFlowScopeCollection
    {
        /// <summary>Gets the identifiers of the scopes: the names of the scope properties.</summary>
        ReadOnlyMemory<string> ScopeIdentifiers { get; }

        /// <summary>Sets the scope property that has the given identifier.</summary>
        /// <param name="identifier">The identifier of a scope; one of <see cref="ScopeIdentifiers"/>.</param>
        /// <param name="scope">The scope to set.</param>
        /// <returns><c>true</c> when the collection accepted the scope; otherwise <c>false</c>.</returns>
        bool TrySetScope(string identifier, PageFlowScope scope);
    }

    /// <summary>A page stack that holds at most one page.</summary>
    /// <remarks>
    /// A push hides and removes the current page, then shows the pushed page. A pop hides and removes the
    /// current page.
    /// </remarks>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public interface IPageStackSingle<TPage> : IPageFlow
        , IPageStackStrategy<TPage>
        , IHasCurrentPage<TPage>
        where TPage : class, IPage
    {
        /// <summary>Hides and removes the current page.</summary>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>The result of the pop and the page it removed.</returns>
        new UnityTask<PagePopResult<TPage>> PopAsync(PageContext context, CancellationToken token);

        /// <summary>Hides and removes the current page, then shows <paramref name="page"/>.</summary>
        /// <param name="page">The page to push.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>The result of the push and the page it removed.</returns>
        new UnityTask<PagePushResult<TPage>> PushAsync(
              [NotNull] TPage page
            , PageContext context
            , CancellationToken token
        );

        /// <summary>Pushes the page that <paramref name="factory"/> produces.</summary>
        /// <param name="factory">Produces the page to push.</param>
        /// <param name="release">Releases a produced page that the flow does not hold.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>The result of the push and the page it removed.</returns>
        /// <remarks>
        /// The <paramref name="factory"/> and <paramref name="release"/> callbacks follow the contract of
        /// <see cref="IPageStackStrategy{TPage}"/>.
        /// </remarks>
        new UnityTask<PagePushResult<TPage>> PushAsync(
              [NotNull] Func<CancellationToken, UnityTask<Result<TPage, PageFlowError>>> factory
            , [NotNull] Action<TPage> release
            , PageContext context
            , CancellationToken token
        );
    }

    /// <summary>A page stack that keeps every pushed page.</summary>
    /// <remarks>
    /// A push hides the current page, which stays in the stack, and shows the pushed page. A pop hides and removes
    /// the current page, and shows the page below it.
    /// </remarks>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public interface IPageStackMulti<TPage> : IPageFlow
        , IPageStackStrategy<TPage>
        , IHasCurrentPage<TPage>
        , IHasPageCollection<TPage>
        where TPage : class, IPage
    {
        /// <summary>Hides and removes the current page, and shows the page below it.</summary>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>The result of the pop and the page it removed.</returns>
        new UnityTask<PagePopResult<TPage>> PopAsync(PageContext context, CancellationToken token);

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
    }

    /// <summary>A page list that shows at most one of its pages.</summary>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public interface IPageListSingle<TPage> : IPageFlow
        , IPageListStrategy<TPage>
        , IHasCurrentPage<TPage>
        , IHasPages<TPage>
        , IHasPageCollection<TPage>
        where TPage : class, IPage
    {
        /// <summary>Hides the current page.</summary>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow hid the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// The page at <see cref="PageContext.DefaultIndex"/> cannot be hidden. After the hide, the flow shows the
        /// page at that index when it exists.
        /// </remarks>
        UnityTask<Success<PageFlowError>> HideAsync(PageContext context, CancellationToken token);
    }

    /// <summary>A page list that can show any number of its pages.</summary>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public interface IPageListMulti<TPage> : IPageFlow
        , IPageListStrategy<TPage>
        , IHasPages<TPage>
        where TPage : class, IPage
    {
        /// <summary>Hides a page of the list.</summary>
        /// <param name="page">The page to hide.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow hid the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        UnityTask<Success<PageFlowError>> HideAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        /// <summary>Hides the page that <paramref name="factory"/> produces.</summary>
        /// <param name="factory">Produces the page to hide.</param>
        /// <param name="release">Releases a produced page that the flow does not hold.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow hid the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// The <paramref name="factory"/> and <paramref name="release"/> callbacks follow the contract of
        /// <see cref="IPageStackStrategy{TPage}"/>.
        /// </remarks>
        UnityTask<Success<PageFlowError>> HideAsync(
              [NotNull] Func<CancellationToken, UnityTask<Result<TPage, PageFlowError>>> factory
            , [NotNull] Action<TPage> release
            , PageContext context
            , CancellationToken token
        );

        /// <summary>Hides the page at an index of the list.</summary>
        /// <param name="index">The index of the page to hide.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow hid the page; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        UnityTask<Success<PageFlowError>> HideAsync(int index, PageContext context, CancellationToken token);
    }

    /// <summary>A flow that has a current page.</summary>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public interface IHasCurrentPage<TPage>
        where TPage : class, IPage
    {
        /// <summary>Gets the page the flow shows, if any.</summary>
        Option<TPage> CurrentPage { get; }
    }

    /// <summary>A flow that exposes its pages as a collection.</summary>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public interface IHasPageCollection<TPage>
        where TPage : class, IPage
    {
        /// <summary>Gets the pages of the flow.</summary>
        IReadOnlyCollection<TPage> PageCollection { get; }
    }

    /// <summary>A flow that exposes its pages as a list.</summary>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public interface IHasPages<TPage>
        where TPage : class, IPage
    {
        /// <summary>Gets the pages of the flow, by index.</summary>
        ListFast<TPage>.ReadOnly Pages { get; }
    }
}
