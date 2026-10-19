using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.Flows;
using EncosyTower.PageFlows.Internals;
using EncosyTower.PageFlows.Processing;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages.Processing;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.UitkPages
{
    /// <summary>
    /// A UI Toolkit page flow that keeps every pushed page, as a <see cref="PageStackMulti{TPage}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Showing a page hides the current one, which stays in the stack; hiding the current page removes it and shows the
    /// page below. The flow handles <see cref="EncosyTower.PageFlows.PubSub.ShowPageMessage"/>,
    /// <see cref="EncosyTower.PageFlows.PubSub.HideActivePageMessage"/>,
    /// <see cref="EncosyTower.PageFlows.PubSub.HideAllPagesMessage"/> and
    /// <see cref="EncosyTower.PageFlows.PubSub.AbortPageMessage"/>.
    /// </para>
    /// <para>The stack does not check whether the page is already in it; push a new page instance.</para>
    /// </remarks>
    public sealed class UitkPageStackMulti : UitkPageFlow, IPageFlowRecordable
    {
        private PageStackMulti<IUitkPage> _flow;

        /// <inheritdoc/>
        public override bool IsInTransition => _flow != null && _flow.IsInTransition;

        /// <summary>Gets the page the flow shows, if any.</summary>
        public Option<IUitkPage> CurrentPage => _flow != null ? _flow.CurrentPage : Option<IUitkPage>.None;

        /// <summary>Gets the pages of the stack.</summary>
        public IReadOnlyCollection<IUitkPage> PageCollection
            => _flow != null ? _flow.PageCollection : Array.Empty<IUitkPage>();

        string IPageFlowRecordable.FlowId => Identifier;

        PageFlowKind IPageFlowRecordable.Kind => PageFlowKind.PageStackMulti;

        PageFlowScope IPageFlowRecordable.FlowScope => Context.FlowScope;

        MessageSubscriber IPageFlowRecordable.Subscriber => Context.Subscriber;

        MessagePublisher IPageFlowRecordable.Publisher => Context.Publisher;

        bool IPageFlowRecordable.IsAlive => IsInitialized;

        bool IPageFlowRecordable.IsShowingPages => _flow != null && _flow.PageCollection.Count > 0;

        /// <summary>Creates the page of an asset, then hides the current page and shows the new one.</summary>
        /// <param name="assetKey">The key of the page asset.</param>
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
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="assetKey"/> is not valid (default or empty).
        /// </exception>
        /// <exception cref="System.InvalidOperationException">The flow is not initialized.</exception>
        public async UnityTask<Success<PageFlowError>> ShowPageAsync(
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
                abort = _flow.BeginCall(ref flowContext, PageAbortCallKind.Abortable);
                abort.MarkSetAsideReader();
                result = await ShowPageCoreAsync(assetKey, context, flowContext, abort, token);
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

        /// <summary>Hides the current page and removes it, then shows the page below.</summary>
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

        /// <summary>
        /// Hides every page of the stack at the same time, then removes them from the stack and returns them to
        /// the pool, or destroys them as <see cref="PageContext.ReturnOperation"/> says.
        /// </summary>
        /// <remarks>
        /// Returns <c>NoPage</c> when the stack is empty. <see cref="AbortCurrent"/> aborts every page transition
        /// of the call.
        /// An abort is also
        /// published as a <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/>, any other failure as a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>, except a cancel or an unheard message
        /// that carries no other failure. When the call throws, the receipt gets the <c>Cancelled</c> or
        /// <c>Exception</c> case with the failures the flow set aside.
        /// </remarks>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the call was applied; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
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
                result = await HideAllPagesCoreAsync(context, flowContext, token);
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
            ShowPageMessage.Async.Subscribe(in subscriber, HandleAsync);
            HideActivePageMessage.Async.Subscribe(in subscriber, HandleAsync);
            HideAllPagesMessage.Async.Subscribe(in subscriber, HandleAsync);
            AbortPageMessage.Subscribe(in subscriber, Handle);

            var processHub = context.ProcessHub.WithState(this);
            IsInTransitionRequest.Register(in processHub, Process);
            GetCurrentPageRequest.Register(in processHub, Process);
            GetPageCollectionRequest.Register(in processHub, Process);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        private async UnityTask<Success<PageFlowError>> ShowPageCoreAsync(
              AssetKey assetKey
            , PageContext context
            , PageContext flowContext
            , PageAbortState abort
            , CancellationToken token
        )
        {
            var rent = await RentPageAsync(assetKey, context, flowContext, token);

            if (abort.IsRequested)
            {
                if (rent.TryGetValue(out var rentedSlot))
                {
                    ReturnPageOrLog(rentedSlot.Page, context);
                }

                Success<PageFlowError> aborted = PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode);
                PublishFailureOrAbort(aborted, context);
                return aborted;
            }

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

            RecordFocus();
            BeginBlocking();

            Success<PageFlowError> result;

            try
            {
                result = await _flow.PushAsync(slot.Page, flowContext, token);
            }
            catch (OperationCanceledException)
            {
                if (IsInStack(slot.Page) == false)
                {
                    ReturnPageOrLog(slot.Page, context);
                    RestoreFocus();
                }

                throw;
            }
            finally
            {
                EndBlocking();
            }

            var isNavigationApplied = result.IsApplied();
            var navFrom = isNavigationApplied ? MakeSecondPageRef() : PageRef.None;
            var navTo = isNavigationApplied ? MakePageRef(page: slot.Page, index: Option.None) : PageRef.None;

            if (result.IsApplied())
            {
                ApplyFocus(slot);
            }
            else
            {
                ReturnPageOrLog(slot.Page, context);
                RestoreFocus();
            }

            if (isNavigationApplied)
            {
                result = await PublishNavigatedAsync(PageNavigationKind.Push, navFrom, navTo, context, result);
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

            BeginBlocking();

            PagePopResult<IUitkPage> pop;

            try
            {
                pop = await _flow.PopAsync(flowContext, token);
            }
            catch (OperationCanceledException)
            {
                if (IsInStack(page) == false)
                {
                    ReturnPageOrLog(page, context);
                    RestoreFocus();
                }

                throw;
            }
            finally
            {
                EndBlocking();
            }

            var result = pop.Result;
            var isNavigationApplied = result.IsApplied();

            var navFrom = isNavigationApplied && pop.Popped.TryGetValue(out var navPopped)
                ? MakePageRef(page: navPopped, index: Option.None)
                : PageRef.None;

            var navTo = isNavigationApplied && _flow.CurrentPage.TryGetValue(out var navCurrent)
                ? MakePageRef(page: navCurrent, index: Option.None)
                : PageRef.None;

            if (result.IsApplied())
            {
                if (pop.Popped.TryGetValue(out var popped))
                {
                    ReturnPageOrLog(popped, context);
                }

                RestoreFocus();
            }

            if (isNavigationApplied)
            {
                result = await PublishNavigatedAsync(PageNavigationKind.Pop, navFrom, navTo, context, result);
            }

            PublishFailureOrAbort(result, context);
            return result;
        }

        private async UnityTask<Success<PageFlowError>> HideAllPagesCoreAsync(
              PageContext context
            , PageContext flowContext
            , CancellationToken token
        )
        {
            if (_flow.PageCollection.Count < 1)
            {
                Success<PageFlowError> failure = PageFlowError.NoPage();
                PublishFailureOrAbort(failure, context);
                return failure;
            }

            BeginBlocking();

            PagePopAllResult<IUitkPage> pop;

            try
            {
                pop = await _flow.PopAllAsync(flowContext, token);
            }
            finally
            {
                EndBlocking();
            }

            var pages = pop.Pages;
            var count = pop.Count;
            var result = pop.Result;
            PageRef[] refs = null;

            try
            {
                if (pop.Thrown != null)
                {
                    for (var i = 0; i < count; i++)
                    {
                        ReturnPageOrLog(pages[i], context);
                    }

                    RestoreFocus();

                    pop.Thrown.Throw();
                }

                if (pages != null && result.IsApplied())
                {
                    refs = ArrayPool<PageRef>.Shared.Rent(count);

                    for (var i = 0; i < count; i++)
                    {
                        refs[i] = MakePageRef(page: pages[i], index: Option.None);
                    }

                    for (var i = 0; i < count; i++)
                    {
                        ReturnPageOrLog(pages[i], context);
                    }

                    RestoreFocus();

                    for (var i = 0; i < count; i++)
                    {
                        var navTo = i + 1 < count ? refs[i + 1] : PageRef.None;
                        result = await PublishNavigatedAsync(PageNavigationKind.Pop, refs[i], navTo, context, result);
                    }
                }
                else if (pages != null)
                {
                    for (var i = 0; i < count; i++)
                    {
                        ReturnPageOrLog(pages[i], context);
                    }
                }
            }
            finally
            {
                if (pages != null)
                {
                    ArrayPool<IUitkPage>.Shared.Return(array: pages, clearArray: true);
                }

                if (refs != null)
                {
                    ArrayPool<PageRef>.Shared.Return(array: refs, clearArray: true);
                }
            }

            PublishFailureOrAbort(result, context);
            return result;
        }

        private bool IsInStack(IUitkPage page)
        {
            foreach (var current in _flow.PageCollection)
            {
                if (ReferenceEquals(current, page))
                {
                    return true;
                }
            }

            return false;
        }

        private PageRef MakeSecondPageRef()
        {
            var position = 0;

            foreach (var page in _flow.PageCollection)
            {
                if (position == 1)
                {
                    return MakePageRef(page: page, index: Option.None);
                }

                position++;
            }

            return PageRef.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkPageStackMulti stack
            , ShowPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (ShowPageMessage)msg;
            return stack.ShowPageAsync(message.AssetKey, message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkPageStackMulti stack
            , HideActivePageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HideActivePageMessage)msg;
            return stack.HideActivePageAsync(message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkPageStackMulti stack
            , HideAllPagesMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HideAllPagesMessage)msg;
            return stack.HideAllPagesAsync(message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Handle(UitkPageStackMulti flow, AbortPageMessage msg)
            => _ = flow.AbortCurrent(msg.Mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(UitkPageStackMulti stack, IsInTransitionRequest _)
            => stack.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<IUitkPage> Process(UitkPageStackMulti stack, GetCurrentPageRequest _)
            => stack._flow.CurrentPage;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyCollection<IUitkPage> Process(UitkPageStackMulti stack, GetPageCollectionRequest _)
            => stack._flow.PageCollection;
    }
}
