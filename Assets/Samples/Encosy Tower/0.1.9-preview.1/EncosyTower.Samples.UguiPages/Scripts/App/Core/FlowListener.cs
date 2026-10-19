using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Samples.UguiPages.Attachments;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Overlays;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>
    /// Listens to what the flows report: failures and aborts are logged (and shown as a toast when the player
    /// should know), and every navigation is applied to the app state.
    /// </summary>
    internal sealed class FlowListener
    {
        private readonly CodexContext _ctx;
        private readonly PopupController _popups;
        private readonly DialogController _dialogs;
        private readonly FeedbackPresenter _feedback;
        private readonly List<ISubscription> _subscriptions = new();
        private readonly Dictionary<PageFlowScope, List<ISubscription>> _flowSubscriptions = new();

        public FlowListener(
              CodexContext ctx
            , PopupController popups
            , DialogController dialogs
            , FeedbackPresenter feedback
        )
        {
            _ctx = ctx;
            _popups = popups;
            _dialogs = dialogs;
            _feedback = feedback;
        }

        /// <summary>Subscribes the four main flows and follows the flows that the attachments create later.</summary>
        public void Start()
        {
            SubscribeScope(_ctx.Scopes.Screen, _subscriptions);
            SubscribeScope(_ctx.Scopes.Popup, _subscriptions);
            SubscribeOverlay(_ctx.Scopes.FloatingContext);
            SubscribeOverlay(_ctx.Scopes.Toast);

            _ctx.Attachments.FlowCreated += OnAttachmentFlowCreated;
            _ctx.Attachments.FlowDropped += OnAttachmentFlowDropped;
        }

        public void Dispose()
        {
            _subscriptions.Unsubscribe();

            foreach (var subscriptions in _flowSubscriptions.Values)
            {
                subscriptions.Unsubscribe();
            }

            _flowSubscriptions.Clear();

            if (_ctx.Attachments != null)
            {
                _ctx.Attachments.FlowCreated -= OnAttachmentFlowCreated;
                _ctx.Attachments.FlowDropped -= OnAttachmentFlowDropped;
            }
        }

        private void SubscribeScope(PageFlowScope scope, List<ISubscription> subscriptions)
        {
            var subscriber = _ctx.Codex.FlowContext.Subscriber
                .Scope(scope)
                .WithState(this)
                .WithSubscriptions(subscriptions);

            PageFlowFailedMessage.Subscribe(in subscriber, HandleFailure);
            PageAbortedMessage.Subscribe(in subscriber, HandleAborted);
            PageNavigatedMessage.Async.Subscribe(in subscriber, HandleNavigatedAsync);
        }

        private void SubscribeOverlay(PageFlowScope scope)
        {
            var subscriber = _ctx.Codex.FlowContext.Subscriber
                .Scope(scope)
                .WithState(this)
                .WithSubscriptions(_subscriptions);

            PageFlowFailedMessage.Subscribe(in subscriber, HandleOverlayFailure);
            PageAbortedMessage.Subscribe(in subscriber, HandleAborted);
        }

        private void OnAttachmentFlowCreated(PageFlowScope scope, PageFlowKind kind)
        {
            var subscriptions = new List<ISubscription>();

            _flowSubscriptions[scope] = subscriptions;
            SubscribeScope(scope, subscriptions);
        }

        private void OnAttachmentFlowDropped(PageFlowScope scope)
        {
            if (_flowSubscriptions.TryGetValue(scope, out var subscriptions))
            {
                _flowSubscriptions.Remove(scope);
                subscriptions.Unsubscribe();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HandleFailure(FlowListener owner, PageFlowFailedMessage message)
        {
            owner.OnFailure(message.Error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HandleOverlayFailure(FlowListener owner, PageFlowFailedMessage message)
        {
            owner.OnOverlayFailure(message.Error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HandleAborted(FlowListener owner, PageAbortedMessage message)
        {
            owner.OnAborted(message.Error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleNavigatedAsync(
              FlowListener owner
            , PageNavigatedMessage.Async message
            , PublishingContext context
        )
        {
            owner.OnNavigated(((PageNavigatedMessage)message).Navigation);
            return UnityTask.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private void OnFailure(PageFlowError error)
        {
            if (error.IsCancelled)
            {
                error.LogSuppressed(_ctx.Codex.FlowContext.Logger);
                return;
            }

            error.Log(_ctx.Codex.FlowContext.Logger);

            var name = _dialogs.IsAppendixPending
                ? ConfigTextType.ToastMissingPage
                : ConfigTextType.ToastOpenFailed;

            _feedback.ShowToast(name, ToastKind.Failure);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private void OnOverlayFailure(PageFlowError error)
        {
            if (error.IsCancelled)
            {
                error.LogSuppressed(_ctx.Codex.FlowContext.Logger);
                return;
            }

            error.Log(_ctx.Codex.FlowContext.Logger);
        }

        /// <summary>Logs the failures met while an abort was undone; the abort itself is expected (D16, D85).</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private void OnAborted(PageFlowError error)
        {
            error.LogSuppressed(_ctx.Codex.FlowContext.Logger);
        }

        private void OnNavigated(in PageNavigation navigation)
        {
            switch (navigation.FlowId)
            {
                case AppFlowScopes.Identifiers.Screen:
                {
                    OnScreenNavigated(in navigation);
                    break;
                }

                case AppFlowScopes.Identifiers.Popup:
                {
                    _popups.TrackNavigation(in navigation);
                    break;
                }

                case PageAttachments.TABS_FLOW:
                {
                    OnTabNavigated(in navigation);
                    break;
                }

                case PageAttachments.CARDS_FLOW:
                {
                    OnCardNavigated(in navigation);
                    break;
                }
            }
        }

        private void OnScreenNavigated(in PageNavigation navigation)
        {
            if (navigation.To.IsValid == false)
            {
                return;
            }

            var context = navigation.Context;

            if (PageUserDataAPI.TryGetPage(in context, out var data))
            {
                _ctx.State.ApplyScreen(data.Location);
            }
            else
            {
                _ctx.State.ApplyScreen(BookLocation.MainMenu);
            }
        }

        private void OnTabNavigated(in PageNavigation navigation)
        {
            if (navigation.Kind != PageNavigationKind.ShowAt
                || navigation.To.Index.TryGetValue(out var tab) == false
                || TryGetDimension(in navigation, out var dimension) == false
            )
            {
                return;
            }

            if (_ctx.State.Attachments.TryGetActive(BookLocation.Of(dimension), out var set))
            {
                set.NoteTab(tab);
            }

            _ctx.State.ApplyTab(dimension, tab);
        }

        private void OnCardNavigated(in PageNavigation navigation)
        {
            if (TryGetDimension(in navigation, out var dimension) == false)
            {
                return;
            }

            if (navigation.Kind == PageNavigationKind.ShowAt && navigation.To.Index.TryGetValue(out var shown))
            {
                ApplyCard(dimension: dimension, index: shown, visible: true);
            }
            else if (navigation.Kind == PageNavigationKind.HideAt && navigation.From.Index.TryGetValue(out var hidden))
            {
                ApplyCard(dimension: dimension, index: hidden, visible: false);
            }
        }

        private void ApplyCard(Dimension dimension, int index, bool visible)
        {
            if (_ctx.State.Attachments.TryGetActive(BookLocation.Of(dimension), out var set))
            {
                set.NoteCard(index, visible);
            }

            _ctx.State.ApplyCard(dimension: dimension, index: index, visible: visible);
        }

        private static bool TryGetDimension(in PageNavigation navigation, out Dimension dimension)
        {
            var context = navigation.Context;

            if (PageUserDataAPI.TryGetPage(in context, out var data) && data.Location.TryGetDimension(out dimension))
            {
                return true;
            }

            dimension = default;
            return false;
        }
    }
}
