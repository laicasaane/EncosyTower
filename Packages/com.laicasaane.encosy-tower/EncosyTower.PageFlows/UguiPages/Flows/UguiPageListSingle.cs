#if UNITY_UGUI

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
using EncosyTower.PageFlows.UguiPages.Processing;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages
{
    /// <summary>
    /// A uGUI page flow that lists pages and shows at most one, as a <see cref="PageListSingle{TPage}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An added page stays inactive until it is shown. A page waits under the pool root while it is activated for its
    /// show, and moves under the flow root when the show starts. The flow handles
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
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UguiPageListSingle : UguiPageFlow, IPageFlowRecordable
    {
        private PageListSingle<IUguiPage> _flow;

        string IPageFlowRecordable.FlowId => Identifier;

        PageFlowKind IPageFlowRecordable.Kind => PageFlowKind.PageListSingle;

        PageFlowScope IPageFlowRecordable.FlowScope => Context.FlowScope;

        MessageSubscriber IPageFlowRecordable.Subscriber => Context.Subscriber;

        MessagePublisher IPageFlowRecordable.Publisher => Context.Publisher;

        bool IPageFlowRecordable.IsAlive => this.IsValid() && IsInitialized;

        bool IPageFlowRecordable.IsShowingPages => _flow != null && _flow.CurrentPage.HasValue;

        /// <summary>Creates the page of an asset and adds it to the list, inactive.</summary>
        /// <param name="assetKey">The key of the page asset.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the call was applied; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// Adding is not abortable. A page that is not added goes back to the pool.
        /// The result is also reported to <see cref="PageContext.Report"/>. An abort is also
        /// published as a <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/>, any other failure as a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>, except a cancel or an unheard message
        /// that carries no other failure. When the call throws, the report gets the <c>Cancelled</c> or
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
                var pageKey = MakePageKey(assetKey);
                var flowContext = context;
                abort = _flow.BeginCall(ref flowContext, PageAbortCallKind.NotAbortable);
                abort.MarkSetAsideReader();
                result = await AddPageCoreAsync(pageKey, context, flowContext, token);
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
        /// The result is also reported to <see cref="PageContext.Report"/>. An abort is also
        /// published as a <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/>, any other failure as a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>, except a cancel or an unheard message
        /// that carries no other failure. When the call throws, the report gets the <c>Cancelled</c> or
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
        /// The result is also reported to <see cref="PageContext.Report"/>. An abort is also
        /// published as a <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/>, any other failure as a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>, except a cancel or an unheard message
        /// that carries no other failure. When the call throws, the report gets the <c>Cancelled</c> or
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
              PageKey pageKey
            , PageContext context
            , PageContext flowContext
            , CancellationToken token
        )
        {
            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            var rent = await RentPageAsync(pageKey, context, flowContext, token);

            if (rent.TryGetValue(out var identifier) == false)
            {
                Success<PageFlowError> failure = rent.GetErrorOrThrow();
                PublishFailureOrAbort(failure, context);
                return failure;
            }

            if (token.IsCancellationRequested)
            {
                ReturnPageToPoolOrLog(identifier, context);
                return PageFlowError.Cancelled();
            }

            Success<PageFlowError> result;

            try
            {
                result = await _flow.AddAsync(identifier.Page, flowContext, token);
            }
            catch (OperationCanceledException)
            {
                if (IsListed(identifier.Page) == false)
                {
                    ReturnPageToPoolOrLog(identifier, context);
                }

                throw;
            }

            if (result.IsApplied())
            {
                identifier.GameObject.SetActive(false);
            }
            else
            {
                ReturnPageToPoolOrLog(identifier, context);
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
            if ((uint)index >= (uint)_flow.Pages.Count
                || TryGetPageIdentifier(_flow.Pages[index], out var identifier) == false
            )
            {
                Success<PageFlowError> failure = PageFlowError.IndexOutOfRange(index, _flow.Pages.Count);
                PublishFailureOrAbort(failure, context);
                return failure;
            }

            ActivateForShow(identifier);

            (Success<PageFlowError> Result, Option<IUguiPage> Hidden) show;

            try
            {
                show = await _flow.ShowAndGetHiddenAsync(identifier.Page, flowContext, token);
            }
            catch (OperationCanceledException)
            {
                if (IsCurrentPage(identifier.Page) == false)
                {
                    identifier.GameObject.SetActive(false);
                }

                throw;
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
                var toIndex = _flow.IndexOf(identifier.Page);
                navTo = MakePageRef(identifier.Page, Option.SomeIf(toIndex >= 0, toIndex));
            }

            if (result.IsApplied() == false && IsCurrentPage(identifier.Page) == false)
            {
                identifier.GameObject.SetActive(false);
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

            Success<PageFlowError> result;

            try
            {
                result = await _flow.HideAsync(flowContext, token);
            }
            catch (OperationCanceledException)
            {
                if (IsCurrentPage(page) == false)
                {
                    DeactivateListedPage(page);
                }

                throw;
            }

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

            if (result.IsApplied())
            {
                DeactivateListedPage(page);
            }

            if (isNavigationApplied)
            {
                result = await PublishNavigatedAsync(PageNavigationKind.HideAt, navFrom, navTo, context, result);
            }

            PublishFailureOrAbort(result, context);
            return result;
        }

        private void DeactivateListedPage(IUguiPage page)
        {
            var index = _flow.IndexOf(page);

            if (index >= 0
                && (uint)index < (uint)_flow.Pages.Count
                && TryGetPageIdentifier(_flow.Pages[index], out var identifier)
            )
            {
                identifier.GameObject.SetActive(false);
            }
        }

        private bool IsCurrentPage(IUguiPage page)
            => _flow.CurrentPage.TryGetValue(out var current) && ReferenceEquals(current, page);

        private bool IsListed(IUguiPage page)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UguiPageListSingle list
            , AddPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (AddPageMessage)msg;
            return list.AddPageAsync(message.AssetKey, message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UguiPageListSingle list
            , ShowPageAtIndexMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (ShowPageAtIndexMessage)msg;
            return list.ShowPageAtIndexAsync(message.Index, message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UguiPageListSingle list
            , HideActivePageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HideActivePageMessage)msg;
            return list.HideActivePageAsync(message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Handle(UguiPageListSingle flow, AbortPageMessage msg)
            => _ = flow.AbortCurrent(msg.Mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(UguiPageListSingle list, IsInTransitionRequest _)
            => list._flow.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Process(UguiPageListSingle list, GetPageIndexRequest req)
            => list._flow.IndexOf(req.Page);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<IUguiPage> Process(UguiPageListSingle list, GetCurrentPageRequest _)
            => list._flow.CurrentPage;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ListFast<IUguiPage>.ReadOnly Process(UguiPageListSingle list, GetPageListRequest _)
            => list._flow.Pages;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyCollection<IUguiPage> Process(UguiPageListSingle list, GetPageCollectionRequest _)
            => list._flow.PageCollection;
    }
}

#endif
