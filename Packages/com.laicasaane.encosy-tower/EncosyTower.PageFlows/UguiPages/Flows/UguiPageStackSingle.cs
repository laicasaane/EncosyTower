#if UNITY_UGUI

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.AssetKeys;
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
    /// <summary>A uGUI page flow that holds at most one page, as a <see cref="PageStackSingle{TPage}"/>.</summary>
    /// <remarks>
    /// <para>
    /// Showing a page hides and removes the current one; the removed page returns to the pool. The flow handles
    /// <see cref="EncosyTower.PageFlows.PubSub.ShowPageMessage"/>,
    /// <see cref="EncosyTower.PageFlows.PubSub.HideActivePageMessage"/> and
    /// <see cref="EncosyTower.PageFlows.PubSub.AbortPageMessage"/>.
    /// </para>
    /// <para>The stack does not check whether the page is already in it; push a new page instance.</para>
    /// </remarks>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UguiPageStackSingle : UguiPageFlow, IPageFlowRecordable
    {
        private PageStackSingle<IUguiPage> _flow;

        string IPageFlowRecordable.FlowId => Identifier;

        PageFlowKind IPageFlowRecordable.Kind => PageFlowKind.PageStackSingle;

        PageFlowScope IPageFlowRecordable.FlowScope => Context.FlowScope;

        MessageSubscriber IPageFlowRecordable.Subscriber => Context.Subscriber;

        MessagePublisher IPageFlowRecordable.Publisher => Context.Publisher;

        bool IPageFlowRecordable.IsAlive => this.IsValid() && IsInitialized;

        bool IPageFlowRecordable.IsShowingPages => _flow != null && _flow.CurrentPage.HasValue;

        /// <summary>Creates the page of an asset, then hides the current page and shows the new one.</summary>
        /// <param name="assetKey">The key of the page asset.</param>
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
                var pageKey = MakePageKey(assetKey);
                var flowContext = context;
                abort = _flow.BeginCall(ref flowContext, PageAbortCallKind.Abortable);
                abort.MarkSetAsideReader();
                result = await ShowPageCoreAsync(pageKey, context, flowContext, abort, token);
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

        /// <summary>Hides the current page and removes it.</summary>
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
            ShowPageMessage.Async.Subscribe(in subscriber, HandleAsync);
            HideActivePageMessage.Async.Subscribe(in subscriber, HandleAsync);
            AbortPageMessage.Subscribe(in subscriber, Handle);

            var processHub = context.ProcessHub.WithState(this);
            IsInTransitionRequest.Register(in processHub, Process);
            GetCurrentPageRequest.Register(in processHub, Process);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        private async UnityTask<Success<PageFlowError>> ShowPageCoreAsync(
              PageKey pageKey
            , PageContext context
            , PageContext flowContext
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            var rent = await RentPageAsync(pageKey, context, flowContext, token);

            if (abort.IsRequested)
            {
                if (rent.TryGetValue(out var rentedIdentifier))
                {
                    ReturnPageToPoolOrLog(rentedIdentifier, context);
                }

                Success<PageFlowError> aborted = PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode);
                PublishFailureOrAbort(aborted, context);
                return aborted;
            }

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

            identifier.GameObject.SetActive(true);

            var previous = _flow.CurrentPage;
            PagePushResult<IUguiPage> push;

            try
            {
                push = await _flow.PushAsync(identifier.Page, flowContext, token);
            }
            catch (OperationCanceledException)
            {
                ReturnPageIfRemoved(identifier.Page, context);

                if (previous.TryGetValue(out var previousPage))
                {
                    ReturnPageIfRemoved(previousPage, context);
                }

                throw;
            }

            var isNavigationApplied = push.Result.IsApplied();

            var navFrom = isNavigationApplied && push.Displaced.TryGetValue(out var navDisplaced)
                ? MakePageRef(page: navDisplaced, index: Option.None)
                : PageRef.None;

            var navTo = isNavigationApplied ? MakePageRef(page: identifier.Page, index: Option.None) : PageRef.None;
            var result = push.Result;

            if (result.IsApplied())
            {
                if (push.Displaced.TryGetValue(out var displaced))
                {
                    ReturnPageToPoolOrLog(displaced, context);
                }
            }
            else
            {
                ReturnPageToPoolOrLog(identifier, context);
            }

            if (isNavigationApplied)
            {
                result = await PublishNavigatedAsync(PageNavigationKind.Show, navFrom, navTo, context, result);
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

            PagePopResult<IUguiPage> pop;

            try
            {
                pop = await _flow.PopAsync(flowContext, token);
            }
            catch (OperationCanceledException)
            {
                ReturnPageIfRemoved(page, context);
                throw;
            }

            var result = pop.Result;
            var isNavigationApplied = result.IsApplied();

            var navFrom = isNavigationApplied && pop.Popped.TryGetValue(out var navPopped)
                ? MakePageRef(page: navPopped, index: Option.None)
                : PageRef.None;

            var navTo = PageRef.None;

            if (result.IsApplied() && pop.Popped.TryGetValue(out var popped))
            {
                ReturnPageToPoolOrLog(popped, context);
            }

            if (isNavigationApplied)
            {
                result = await PublishNavigatedAsync(PageNavigationKind.Hide, navFrom, navTo, context, result);
            }

            PublishFailureOrAbort(result, context);
            return result;
        }

        private void ReturnPageIfRemoved(IUguiPage page, PageContext context)
        {
            if (_flow.CurrentPage.TryGetValue(out var current) && ReferenceEquals(current, page))
            {
                return;
            }

            ReturnPageToPoolOrLog(page, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UguiPageStackSingle stack
            , ShowPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (ShowPageMessage)msg;
            return stack.ShowPageAsync(message.AssetKey, message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UguiPageStackSingle stack
            , HideActivePageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HideActivePageMessage)msg;
            return stack.HideActivePageAsync(message.Context, context.Token).AsUnityTask();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Handle(UguiPageStackSingle flow, AbortPageMessage msg)
            => _ = flow.AbortCurrent(msg.Mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(UguiPageStackSingle stack, IsInTransitionRequest _)
            => stack._flow.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<IUguiPage> Process(UguiPageStackSingle stack, GetCurrentPageRequest _)
            => stack._flow.CurrentPage;
    }
}

#endif
