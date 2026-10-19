using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.AssetKeys;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.Flows;
using EncosyTower.PageFlows.Internals;
using EncosyTower.PageFlows.Processing;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages.Internals;
using EncosyTower.PageFlows.UitkPages.Processing;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.UitkPages
{
    /// <summary>
    /// A UI Toolkit page flow that lists pages and shows at most one, as a <see cref="PageListSingle{TPage}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An added page stays hidden until it is shown. The flow handles
    /// <see cref="EncosyTower.PageFlows.PubSub.AddPageMessage"/>,
    /// <see cref="EncosyTower.PageFlows.PubSub.ShowPageAtIndexMessage"/>,
    /// <see cref="EncosyTower.PageFlows.PubSub.HideActivePageMessage"/> and
    /// <see cref="EncosyTower.PageFlows.PubSub.AbortPageMessage"/>.
    /// </para>
    /// <para>
    /// The page at <see cref="PageContext.DefaultIndex"/> cannot be hidden, and hiding another page shows it. Showing a
    /// page by index fails with the <c>AlreadyShown</c> case when that page is already the current page.
    /// </para>
    /// </remarks>
    public sealed class UitkPageListSingle : UitkPageFlow, IPageFlowRecordable
    {
        private PageListSingle<IUitkPage> _flow;

        /// <inheritdoc/>
        public override bool IsInTransition => _flow != null && _flow.IsInTransition;

        /// <summary>Gets the page the flow shows, if any.</summary>
        public Option<IUitkPage> CurrentPage => _flow != null ? _flow.CurrentPage : Option<IUitkPage>.None;

        /// <summary>Gets the pages of the list, by index.</summary>
        public ListFast<IUitkPage>.ReadOnly Pages => _flow != null ? _flow.Pages : ListFast<IUitkPage>.ReadOnly.Empty;

        string IPageFlowRecordable.FlowId => Identifier;

        PageFlowKind IPageFlowRecordable.Kind => PageFlowKind.PageListSingle;

        PageFlowScope IPageFlowRecordable.FlowScope => Context.FlowScope;

        MessageSubscriber IPageFlowRecordable.Subscriber => Context.Subscriber;

        MessagePublisher IPageFlowRecordable.Publisher => Context.Publisher;

        bool IPageFlowRecordable.IsAlive => IsInitialized;

        bool IPageFlowRecordable.IsShowingPages => _flow != null && _flow.CurrentPage.HasValue;

        /// <summary>Creates the page of an asset and adds it to the list, hidden.</summary>
        /// <param name="assetKey">The key of the page asset.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the call was applied; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// Adding is not abortable. A page that is not added goes back to the pool.
        /// The result is also reported to <see cref="PageContext.Receipt"/>. An abort is also
        /// published as a <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/>, any other failure as a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>, except a cancel or an unheard message
        /// that carries no other failure. When the call throws, the receipt gets the <c>Cancelled</c> or
        /// <c>Exception</c> case with the failures the flow set aside.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="assetKey"/> is not valid (default or empty).
        /// </exception>
        /// <exception cref="System.InvalidOperationException">The flow is not initialized.</exception>
        public async UnityTask<Success<PageFlowError>> AddPageAsync(
              AssetKey assetKey
            , PageContext context
            , CancellationToken token
        )
        {
            PageAbortState abort = null;
            Success<PageFlowError> result = Success.Yes;

            try
            {
                ThrowHelper.ThrowIfNullOrEmpty(assetKey.Value, nameof(assetKey));
                EnsureInitialized();
                var flowContext = context;
                abort = _flow.BeginCall(ref flowContext, PageAbortCallKind.NotAbortable);
                abort.MarkSetAsideReader();
                result = await AddPageCoreAsync(assetKey, context, flowContext, token);
                ReportCall(result, context);
                return result;
            }
            catch (Exception ex)
            {
                result = PageFlowError.Exception(ex);
                ReportThrownCall(ex, context, abort);
                throw;
            }
            finally
            {
                if (abort != null)
                {
                    _flow.EndCall(abort, result);
                }
            }
        }

        /// <summary>Shows the page at an index and hides the current page.</summary>
        /// <param name="index">The index of the page to show.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the call was applied; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// The result is also reported to <see cref="PageContext.Receipt"/>. An abort is also
        /// published as a <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/>, any other failure as a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>, except a cancel or an unheard message
        /// that carries no other failure. When the call throws, the receipt gets the <c>Cancelled</c> or
        /// <c>Exception</c> case with the failures the flow set aside.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">The flow is not initialized.</exception>
        public async UnityTask<Success<PageFlowError>> ShowPageAtIndexAsync(
              int index
            , PageContext context
            , CancellationToken token
        )
        {
            PageAbortState abort = null;
            Success<PageFlowError> result = Success.Yes;

            try
            {
                EnsureInitialized();
                var flowContext = context;
                abort = _flow.BeginCall(ref flowContext, PageAbortCallKind.Abortable);
                abort.MarkSetAsideReader();
                result = await ShowPageAtIndexCoreAsync(index, context, flowContext, token);
                ReportCall(result, context);
                return result;
            }
            catch (Exception ex)
            {
                result = PageFlowError.Exception(ex);
                ReportThrownCall(ex, context, abort);
                throw;
            }
            finally
            {
                if (abort != null)
                {
                    _flow.EndCall(abort, result);
                }
            }
        }

        /// <summary>Hides the current page.</summary>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the call was applied; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// The result is also reported to <see cref="PageContext.Receipt"/>. An abort is also
        /// published as a <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/>, any other failure as a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>, except a cancel or an unheard message
        /// that carries no other failure. When the call throws, the receipt gets the <c>Cancelled</c> or
        /// <c>Exception</c> case with the failures the flow set aside.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">The flow is not initialized.</exception>
        public async UnityTask<Success<PageFlowError>> HideActivePageAsync(PageContext context, CancellationToken token)
        {
            PageAbortState abort = null;
            Success<PageFlowError> result = Success.Yes;

            try
            {
                EnsureInitialized();
                var flowContext = context;
                abort = _flow.BeginCall(ref flowContext, PageAbortCallKind.Abortable);
                abort.MarkSetAsideReader();
                result = await HideActivePageCoreAsync(context, flowContext, token);
                ReportCall(result, context);
                return result;
            }
            catch (Exception ex)
            {
                result = PageFlowError.Exception(ex);
                ReportThrownCall(ex, context, abort);
                throw;
            }
            finally
            {
                if (abort != null)
                {
                    _flow.EndCall(abort, result);
                }
            }
        }

        /// <summary>Aborts the show or hide this flow is running.</summary>
        /// <param name="mode">How the screen goes back.</param>
        /// <returns>
        /// Success when the flow accepted the request; otherwise a failure; see <see cref="PageAbortError"/>.
        /// </returns>
        /// <remarks>It has the same effect as <see cref="EncosyTower.PageFlows.PubSub.AbortPageMessage"/>.</remarks>
        public Success<PageAbortError> AbortCurrent(PageAbortMode mode)
            => _flow == null ? (PageAbortError)new PageAbortError.FlowIsIdle() : _flow.AbortCurrent(mode);

        protected override void OnInitialize(in InitializationContext context)
        {
            _flow = new(Context, this);

            var subscriber = context.Subscriber.WithState(this);
            AddPageMessage.Async.Subscribe(in subscriber, HandleAsync);
            ShowPageAtIndexMessage.Async.Subscribe(in subscriber, HandleAsync);
            HideActivePageMessage.Async.Subscribe(in subscriber, HandleAsync);
            AbortPageMessage.Subscribe(in subscriber, Handle);

            var processHub = context.ProcessHub.WithState(this);
            IsInTransitionRequest.Register(in processHub, Process);
            GetPageIndexRequest.Register(in processHub, Process);
            GetCurrentPageRequest.Register(in processHub, Process);
            GetPageListRequest.Register(in processHub, Process);
            GetPageCollectionRequest.Register(in processHub, Process);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        private async UnityTask<Success<PageFlowError>> AddPageCoreAsync(
              AssetKey assetKey
            , PageContext context
            , PageContext flowContext
            , CancellationToken token
        )
        {
            var rent = await RentPageAsync(assetKey, context, flowContext, token);

            if (rent.TryGetValue(out var slot) == false)
            {
                Success<PageFlowError> failure = rent.GetErrorOrThrow();
                PublishFailureOrAbort(failure, context);
                return failure;
            }

            if (token.IsCancellationRequested)
            {
                ReturnPageOrLog(slot.Page, context);
                return PageFlowError.Cancelled();
            }

            Success<PageFlowError> result;

            try
            {
                result = await _flow.AddAsync(slot.Page, flowContext, token);
            }
            catch (OperationCanceledException)
            {
                if (IsListed(slot.Page) == false)
                {
                    ReturnPageOrLog(slot.Page, context);
                }

                throw;
            }

            if (result.IsApplied() == false)
            {
                ReturnPageOrLog(slot.Page, context);
            }

            PublishFailureOrAbort(result, context);
            return result;
        }

        private async UnityTask<Success<PageFlowError>> ShowPageAtIndexCoreAsync(
              int index
            , PageContext context
            , PageContext flowContext
            , CancellationToken token
        )
        {
            if (TryGetSlotAt(index, out var slot) == false)
            {
                Success<PageFlowError> failure = PageFlowError.IndexOutOfRange(index, _flow.Pages.Count);
                PublishFailureOrAbort(failure, context);
                return failure;
            }

            var hasOldPage = _flow.CurrentPage.TryGetValue(out var oldPage);

            BeginBlocking();

            (Success<PageFlowError> Result, Option<IUitkPage> Hidden) show;

            try
            {
                show = await _flow.ShowAndGetHiddenAsync(slot.Page, flowContext, token);
            }
            catch (OperationCanceledException)
            {
                if (IsCurrentPage(slot.Page) == false)
                {
                    HideSlot(slot);
                }
                else if (hasOldPage && oldPage != slot.Page && TryGetSlot(oldPage, out var oldSlot))
                {
                    HideSlot(oldSlot);
                }

                throw;
            }
            finally
            {
                EndBlocking();
            }

            var result = show.Result;
            var isNavigationApplied = result.IsApplied();

            var navFrom = PageRef.None;

            if (isNavigationApplied && show.Hidden.TryGetValue(out var navHidden))
            {
                var hiddenIndex = _flow.IndexOf(navHidden);
                navFrom = MakePageRef(navHidden, Option.SomeIf(hiddenIndex >= 0, hiddenIndex));
            }

            var navTo = PageRef.None;

            if (isNavigationApplied)
            {
                var toIndex = _flow.IndexOf(slot.Page);
                navTo = MakePageRef(slot.Page, Option.SomeIf(toIndex >= 0, toIndex));
            }

            if (result.IsApplied())
            {
                if (show.Hidden.TryGetValue(out var hidden)
                    && hidden != slot.Page
                    && TryGetSlot(hidden, out var hiddenSlot)
                )
                {
                    HideSlot(hiddenSlot);
                }

                ApplyFocus(slot);
            }
            else if (IsCurrentPage(slot.Page) == false)
            {
                HideSlot(slot);
            }

            if (isNavigationApplied)
            {
                result = await PublishNavigatedAsync(PageNavigationKind.ShowAt, navFrom, navTo, context, result);
            }

            PublishFailureOrAbort(result, context);
            return result;
        }

        private async UnityTask<Success<PageFlowError>> HideActivePageCoreAsync(
              PageContext context
            , PageContext flowContext
            , CancellationToken token
        )
        {
            if (_flow.CurrentPage.TryGetValue(out var page) == false)
            {
                Success<PageFlowError> failure = PageFlowError.NoPage();
                PublishFailureOrAbort(failure, context);
                return failure;
            }

            TryGetSlotAt(context.DefaultIndex, out var defaultSlot);
            BeginBlocking();

            Success<PageFlowError> result;

            try
            {
                result = await _flow.HideAsync(flowContext, token);
            }
            catch (OperationCanceledException)
            {
                HideDefaultIfNotCurrent(defaultSlot);

                if (IsCurrentPage(page) == false && TryGetSlot(page, out var hiddenSlot))
                {
                    HideSlot(hiddenSlot);

                    if (_flow.CurrentPage.TryGetValue(out var shown) && TryGetSlot(shown, out var shownSlot))
                    {
                        ShowSlot(shownSlot);
                    }
                }

                throw;
            }
            finally
            {
                EndBlocking();
            }

            HideDefaultIfNotCurrent(defaultSlot);

            var isNavigationApplied = result.IsApplied();
            var navFrom = PageRef.None;

            if (isNavigationApplied)
            {
                var index = _flow.IndexOf(page);
                navFrom = MakePageRef(page, Option.SomeIf(index >= 0, index));
            }

            var navTo = PageRef.None;

            if (isNavigationApplied && _flow.CurrentPage.TryGetValue(out var navCurrent))
            {
                var currentIndex = _flow.IndexOf(navCurrent);
                navTo = MakePageRef(navCurrent, Option.SomeIf(currentIndex >= 0, currentIndex));
            }

            if (result.IsApplied() && TryGetSlot(page, out var slot))
            {
                HideSlot(slot);

                if (_flow.CurrentPage.TryGetValue(out var current) && TryGetSlot(current, out var currentSlot))
                {
                    ShowSlot(currentSlot);
                    ApplyFocus(currentSlot);
                }
            }

            if (isNavigationApplied)
            {
                result = await PublishNavigatedAsync(PageNavigationKind.HideAt, navFrom, navTo, context, result);
            }

            PublishFailureOrAbort(result, context);
            return result;
        }

        private void HideDefaultIfNotCurrent(UitkPageSlot slot)
        {
            if (_flow.IsInTransition == false
                && slot != null
                && IsCurrentPage(slot.Page) == false
                && TryGetSlot(slot.Page, out var ownedSlot)
                && ReferenceEquals(slot, ownedSlot)
            )
            {
                HideSlot(slot);
            }
        }

        private bool IsCurrentPage(IUitkPage page)
            => _flow.CurrentPage.TryGetValue(out var current) && ReferenceEquals(current, page);

        private bool IsListed(IUitkPage page)
        {
            var pages = _flow.Pages;
            var count = pages.Count;

            for (var i = 0; i < count; i++)
            {
                if (ReferenceEquals(pages[i], page))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetSlotAt(int index, out UitkPageSlot slot)
        {
            var pages = _flow.Pages;

            if ((uint)index < (uint)pages.Count)
            {
                return TryGetSlot(pages[index], out slot);
            }

            slot = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkPageListSingle list
            , AddPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (AddPageMessage)msg;
            return list.AddPageAsync(message.AssetKey, message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkPageListSingle list
            , ShowPageAtIndexMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (ShowPageAtIndexMessage)msg;
            return list.ShowPageAtIndexAsync(message.Index, message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkPageListSingle list
            , HideActivePageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HideActivePageMessage)msg;
            return list.HideActivePageAsync(message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Handle(UitkPageListSingle flow, AbortPageMessage msg)
            => _ = flow.AbortCurrent(msg.Mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(UitkPageListSingle list, IsInTransitionRequest _)
            => list.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Process(UitkPageListSingle list, GetPageIndexRequest request)
            => list._flow.IndexOf(request.Page);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<IUitkPage> Process(UitkPageListSingle list, GetCurrentPageRequest _)
            => list._flow.CurrentPage;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ListFast<IUitkPage>.ReadOnly Process(UitkPageListSingle list, GetPageListRequest _)
            => list._flow.Pages;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyCollection<IUitkPage> Process(UitkPageListSingle list, GetPageCollectionRequest _)
            => list._flow.PageCollection;
    }
}
