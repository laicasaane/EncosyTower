using System;
using System.Buffers;
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
using UnityEngine.UIElements;

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.UitkPages
{
    /// <summary>
    /// A UI Toolkit page flow that lists pages and can show any number of them, as a
    /// <see cref="PageListMulti{TPage}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An added page stays hidden until it is shown, and a hidden page is hidden again. The flow handles
    /// <see cref="EncosyTower.PageFlows.PubSub.AddPageMessage"/>,
    /// <see cref="EncosyTower.PageFlows.PubSub.ShowPageAtIndexMessage"/>,
    /// <see cref="EncosyTower.PageFlows.PubSub.HidePageAtIndexMessage"/>,
    /// <see cref="EncosyTower.PageFlows.PubSub.ShowAllPagesMessage"/>,
    /// <see cref="EncosyTower.PageFlows.PubSub.HideAllPagesMessage"/> and
    /// <see cref="EncosyTower.PageFlows.PubSub.AbortPageMessage"/>.
    /// </para>
    /// <para>
    /// A multi page list keeps no shown or hidden state: showing a shown page, or hiding a hidden one, runs its
    /// transition again. The all-pages calls treat a page as shown when its element is displayed.
    /// </para>
    /// </remarks>
    public sealed class UitkPageListMulti : UitkPageFlow, IPageFlowRecordable
    {
        private PageListMulti<IUitkPage> _flow;

        /// <inheritdoc/>
        public override bool IsInTransition => _flow != null && _flow.IsInTransition;

        /// <summary>Gets the pages of the list, by index.</summary>
        public ListFast<IUitkPage>.ReadOnly Pages => _flow != null ? _flow.Pages : ListFast<IUitkPage>.ReadOnly.Empty;

        string IPageFlowRecordable.FlowId => Identifier;

        PageFlowKind IPageFlowRecordable.Kind => PageFlowKind.PageListMulti;

        PageFlowScope IPageFlowRecordable.FlowScope => Context.FlowScope;

        MessageSubscriber IPageFlowRecordable.Subscriber => Context.Subscriber;

        MessagePublisher IPageFlowRecordable.Publisher => Context.Publisher;

        bool IPageFlowRecordable.IsAlive => IsInitialized;

        bool IPageFlowRecordable.IsShowingPages => HasShownPage();

        /// <summary>Creates the page of an asset and adds it to the list, hidden.</summary>
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

        /// <summary>Shows the page at an index.</summary>
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

        /// <summary>Hides the page at an index.</summary>
        /// <param name="index">The index of the page to hide.</param>
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
        public async UnityTask<Success<PageFlowError>> HidePageAtIndexAsync(
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
                result = await HidePageAtIndexCoreAsync(index, context, flowContext, token);
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

        /// <summary>Shows every added page that is not shown at the same time. The pages stay added.</summary>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the call was applied; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// Returns <c>NoPage</c> when every added page is shown. <see cref="AbortCurrent"/> aborts every page
        /// transition of the call. Each shown page takes focus in index order, so the last one keeps it, as with
        /// <c>ShowPageAtIndexAsync</c>.
        /// An abort is also
        /// published as a <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/>, any other failure as a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>, except a cancel or an unheard message
        /// that carries no other failure. When the call throws, the report gets the <c>Cancelled</c> or
        /// <c>Exception</c> case with the failures the flow set aside.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">The flow is not initialized.</exception>
        public async UnityTask<Success<PageFlowError>> ShowAllPagesAsync(PageContext context, CancellationToken token)
        {
            PageAbortState abort = null;
            Success<PageFlowError> result = Success.Yes;

            try
            {
                EnsureInitialized();
                var flowContext = context;
                abort = _flow.BeginCall(ref flowContext, PageAbortCallKind.Abortable);
                abort.MarkSetAsideReader();

                result = await TransitionAllPagesCoreAsync(
                      isShow: true
                    , context: context
                    , flowContext: flowContext
                    , token: token
                );

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

        /// <summary>Hides every shown page at the same time. The pages stay added.</summary>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the call was applied; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// Returns <c>NoPage</c> when no page is shown. <see cref="AbortCurrent"/> aborts every page transition
        /// of the call.
        /// An abort is also
        /// published as a <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/>, any other failure as a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>, except a cancel or an unheard message
        /// that carries no other failure. When the call throws, the report gets the <c>Cancelled</c> or
        /// <c>Exception</c> case with the failures the flow set aside.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">The flow is not initialized.</exception>
        public async UnityTask<Success<PageFlowError>> HideAllPagesAsync(PageContext context, CancellationToken token)
        {
            PageAbortState abort = null;
            Success<PageFlowError> result = Success.Yes;

            try
            {
                EnsureInitialized();
                var flowContext = context;
                abort = _flow.BeginCall(ref flowContext, PageAbortCallKind.Abortable);
                abort.MarkSetAsideReader();

                result = await TransitionAllPagesCoreAsync(
                      isShow: false
                    , context: context
                    , flowContext: flowContext
                    , token: token
                );

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
            HidePageAtIndexMessage.Async.Subscribe(in subscriber, HandleAsync);
            ShowAllPagesMessage.Async.Subscribe(in subscriber, HandleAsync);
            HideAllPagesMessage.Async.Subscribe(in subscriber, HandleAsync);
            AbortPageMessage.Subscribe(in subscriber, Handle);

            var processHub = context.ProcessHub.WithState(this);
            IsInTransitionRequest.Register(in processHub, Process);
            GetPageIndexRequest.Register(in processHub, Process);
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

            BeginBlocking();

            Success<PageFlowError> result;

            try
            {
                result = await _flow.ShowAsync(slot.Page, flowContext, token);
            }
            finally
            {
                EndBlocking();
            }

            var isNavigationApplied = result.IsApplied();
            var navFrom = PageRef.None;
            var navTo = PageRef.None;

            if (isNavigationApplied)
            {
                var toIndex = _flow.IndexOf(slot.Page);
                navTo = MakePageRef(slot.Page, Option.SomeIf(toIndex >= 0, toIndex));
            }

            if (result.IsApplied())
            {
                ApplyFocus(slot);
            }
            else
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

        private async UnityTask<Success<PageFlowError>> HidePageAtIndexCoreAsync(
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

            BeginBlocking();

            Success<PageFlowError> result;

            try
            {
                result = await _flow.HideAsync(slot.Page, flowContext, token);
            }
            finally
            {
                EndBlocking();
            }

            var isNavigationApplied = result.IsApplied();
            var navFrom = PageRef.None;

            if (isNavigationApplied)
            {
                var fromIndex = _flow.IndexOf(slot.Page);
                navFrom = MakePageRef(slot.Page, Option.SomeIf(fromIndex >= 0, fromIndex));
            }

            var navTo = PageRef.None;

            if (result.IsApplied())
            {
                HideSlot(slot);
            }

            if (isNavigationApplied)
            {
                result = await PublishNavigatedAsync(PageNavigationKind.HideAt, navFrom, navTo, context, result);
            }

            PublishFailureOrAbort(result, context);
            return result;
        }

        private async UnityTask<Success<PageFlowError>> TransitionAllPagesCoreAsync(
              bool isShow
            , PageContext context
            , PageContext flowContext
            , CancellationToken token
        )
        {
            var listed = _flow.Pages;
            var listedCount = listed.Count;
            var pages = ArrayPool<IUitkPage>.Shared.Rent(listedCount);
            var count = 0;

            try
            {
                for (var i = 0; i < listedCount; i++)
                {
                    if (TryGetSlot(listed[i], out var slot)
                        && (slot.Container.style.display.value != DisplayStyle.None) != isShow
                    )
                    {
                        pages[count] = listed[i];
                        count++;
                    }
                }

                if (count < 1)
                {
                    Success<PageFlowError> failure = PageFlowError.NoPage();
                    PublishFailureOrAbort(failure, context);
                    return failure;
                }

                BeginBlocking();

                Success<PageFlowError> result;

                try
                {
                    result = isShow
                        ? await _flow.ShowAllAsync(pages, count, flowContext, token)
                        : await _flow.HideAllAsync(pages, count, flowContext, token);
                }
                finally
                {
                    EndBlocking();
                }

                var isNavigationApplied = result.IsApplied();

                for (var i = 0; i < count; i++)
                {
                    if (TryGetSlot(pages[i], out var slot) == false)
                    {
                        continue;
                    }

                    if (isNavigationApplied && isShow)
                    {
                        ApplyFocus(slot);
                    }
                    else if (isNavigationApplied != isShow)
                    {
                        HideSlot(slot);
                    }
                }

                if (isNavigationApplied)
                {
                    var kind = isShow ? PageNavigationKind.ShowAt : PageNavigationKind.HideAt;

                    for (var i = 0; i < count; i++)
                    {
                        var pageIndex = _flow.IndexOf(pages[i]);
                        var pageRef = MakePageRef(pages[i], Option.SomeIf(pageIndex >= 0, pageIndex));
                        var navFrom = isShow ? PageRef.None : pageRef;
                        var navTo = isShow ? pageRef : PageRef.None;

                        result = await PublishNavigatedAsync(kind, navFrom, navTo, context, result);
                    }
                }

                PublishFailureOrAbort(result, context);
                return result;
            }
            finally
            {
                ArrayPool<IUitkPage>.Shared.Return(array: pages, clearArray: true);
            }
        }

        private bool HasShownPage()
        {
            if (_flow == null)
            {
                return false;
            }

            var pages = _flow.Pages;
            var count = pages.Count;

            for (var i = 0; i < count; i++)
            {
                if (TryGetSlot(pages[i], out var slot) && slot.Container.style.display.value != DisplayStyle.None)
                {
                    return true;
                }
            }

            return false;
        }

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
              UitkPageListMulti list
            , AddPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (AddPageMessage)msg;
            return list.AddPageAsync(message.AssetKey, message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkPageListMulti list
            , ShowPageAtIndexMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (ShowPageAtIndexMessage)msg;
            return list.ShowPageAtIndexAsync(message.Index, message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkPageListMulti list
            , HidePageAtIndexMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HidePageAtIndexMessage)msg;
            return list.HidePageAtIndexAsync(message.Index, message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkPageListMulti list
            , ShowAllPagesMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (ShowAllPagesMessage)msg;
            return list.ShowAllPagesAsync(message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkPageListMulti list
            , HideAllPagesMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HideAllPagesMessage)msg;
            return list.HideAllPagesAsync(message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Handle(UitkPageListMulti flow, AbortPageMessage msg)
            => _ = flow.AbortCurrent(msg.Mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(UitkPageListMulti list, IsInTransitionRequest _)
            => list.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Process(UitkPageListMulti list, GetPageIndexRequest request)
            => list._flow.IndexOf(request.Page);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ListFast<IUitkPage>.ReadOnly Process(UitkPageListMulti list, GetPageListRequest _)
            => list._flow.Pages;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyCollection<IUitkPage> Process(UitkPageListMulti list, GetPageCollectionRequest _)
            => list._flow.PageCollection;
    }
}
