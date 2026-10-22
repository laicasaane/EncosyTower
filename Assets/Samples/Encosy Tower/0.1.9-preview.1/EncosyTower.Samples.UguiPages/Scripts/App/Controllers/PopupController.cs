using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Overlays;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>
    /// Owns the popup stack: tracks the open dialogs, runs show/hide calls as abortable operations, closes
    /// everything on request, and keeps the offer to reopen the last closed dialog.
    /// </summary>
    internal sealed class PopupController
    {
        private readonly CodexContext _ctx;
        private readonly FlowService _flows;
        private readonly FeedbackPresenter _feedback;
        private readonly List<DialogPage> _pages = new();
        private readonly List<PopupOp> _presentations = new();
        private float _reopenExpiry;

        public PopupController(CodexContext ctx, FlowService flows, FeedbackPresenter feedback)
        {
            _ctx = ctx;
            _flows = flows;
            _feedback = feedback;
        }

        /// <summary>The show or hide call that is running right now, if any.</summary>
        public PopupOp Op { get; private set; }

        /// <summary>True while every popup is being closed so a screen can change.</summary>
        public bool IsCleaning { get; private set; }

        public ReopenOffer Reopen { get; private set; }

        public IReadOnlyList<DialogPage> Pages => _pages;

        public int Count => _pages.Count;

        public bool IsTopPopup(DialogPage page)
            => _pages.Count > 0 && _pages[^1] == page;

        public static bool IsReopenable(DialogPage page)
            => page is DialogPage.More or DialogPage.Echo;

        public void ClearReopen()
        {
            Reopen = default;
        }

        public void ExpireReopen(float now)
        {
            if (Reopen.IsValid && now >= _reopenExpiry)
            {
                Reopen = default;
                _ctx.MarkStatusDirty();
            }
        }

        /// <summary>Follows the popup flow's navigation to keep the list of open dialogs current.</summary>
        public void TrackNavigation(in PageNavigation navigation)
        {
            switch (navigation.Kind)
            {
                case PageNavigationKind.Show:
                case PageNavigationKind.Push:
                {
                    if (AssetKeyAPI.TryGetId(navigation.To.AssetKey, out var id) && id.TryGetValue(out DialogPage page))
                    {
                        _pages.Add(page);
                        _presentations.Add(Op is { IsShow: true } opening && opening.Page == page ? opening : null);
                    }

                    break;
                }

                case PageNavigationKind.Hide:
                case PageNavigationKind.Pop:
                {
                    if (_pages.Count > 0)
                    {
                        _pages.RemoveAt(_pages.Count - 1);
                        _presentations.RemoveAt(_presentations.Count - 1);
                    }

                    break;
                }
            }

            _ctx.State.SetPopupStack(_pages.Count);
            RefreshActive();
            _ctx.MarkStatusDirty();
        }

        public async UnityTask<Success<PageFlowError>> ShowPopupAsync(
              DialogPage page
            , object userData
            , Action onDismiss = null
        )
        {
            var op = new PopupOp(page, isShow: true) { OnDismiss = onDismiss };

            if (userData is DimensionPageData data)
            {
                userData = data with { Dismiss = () => RequestDismiss(op) };
            }

            Op = op;
            Reopen = default;
            RefreshActive();
            _ctx.MarkStatusDirty();

            Success<PageFlowError> result = default;

            try
            {
                var context = _flows.CreateContext(userData: userData, source: op.Source, receipt: op.Receipt);
                result = await _flows.ShowAsync(_ctx.Scopes.Popup, AssetKeyAPI.Key(page), context);
            }
            finally
            {
                EndOp(op);
            }

            ReportAbort(op, in result);
            return result;
        }

        public void Tick()
        {
            if (Op != null || IsCleaning || _ctx.Token.IsCancellationRequested || _presentations.Count == 0)
            {
                return;
            }

            var presentation = _presentations[^1];

            if (presentation is not { DismissRequested: true })
            {
                return;
            }

            presentation.DismissRequested = false;

            if (presentation.OnDismiss != null)
            {
                presentation.OnDismiss();
            }
            else
            {
                HideTopPopupAsync(offerReopen: IsReopenable(presentation.Page)).Forget();
            }
        }

        private void RequestDismiss(PopupOp presentation)
        {
            if (IsCleaning || _ctx.Token.IsCancellationRequested)
            {
                return;
            }

            if (ReferenceEquals(Op, presentation) && presentation.IsShow)
            {
                presentation.Source.Abort(PageAbortMode.Reverse);
                return;
            }

            if (_presentations.Count > 0 && ReferenceEquals(_presentations[^1], presentation))
            {
                presentation.DismissRequested = true;
            }
        }

        public async UnityTask<Success<PageFlowError>> HideTopPopupAsync(bool offerReopen)
        {
            if (_pages.Count == 0)
            {
                return default;
            }

            var page = _pages[^1];
            var op = new PopupOp(page, isShow: false);

            Op = op;
            RefreshActive();
            _ctx.MarkStatusDirty();

            Success<PageFlowError> result = default;

            try
            {
                var context = _flows.CreateContext(source: op.Source, receipt: op.Receipt);
                result = await _flows.HideActiveAsync(_ctx.Scopes.Popup, context);
            }
            finally
            {
                EndOp(op);
            }

            if (result.IsApplied() && offerReopen && IsReopenable(page))
            {
                Reopen = new ReopenOffer(Page: page, Dimension: _ctx.State.CurrentDimension, HasOffer: true);
                _reopenExpiry = Time.unscaledTime + _ctx.Config.Amount(ConfigAmountType.NavigationBarReopenSeconds);
                _ctx.MarkStatusDirty();
            }

            ReportAbort(op, in result);
            return result;
        }

        /// <summary>Hides the top dialog; the map is closed with its own action instead.</summary>
        public async UnityTask CloseDialogAsync(bool offerReopen)
        {
            if (_pages.Count == 0 || Op != null || IsTopPopup(DialogPage.Map))
            {
                return;
            }

            await HideTopPopupAsync(offerReopen);
        }

        /// <summary>Aborts the running call and hides every popup at once.</summary>
        public async UnityTask ClosePopupsAsync()
        {
            IsCleaning = true;
            Reopen = default;
            _ctx.MarkStatusDirty();

            try
            {
                await AbortOpAsync();
                await HideRemainingAsync();
            }
            finally
            {
                IsCleaning = false;
                _ctx.MarkStatusDirty();
            }
        }

        /// <summary>
        /// First half of a two-step close used by travel: aborts the running call and keeps <see cref="IsCleaning"/>
        /// set until <see cref="FadeOutPopupsAsync"/> ends. Returns false when the app is shutting down.
        /// </summary>
        public async UnityTask<bool> BeginClosePopupsAsync()
        {
            IsCleaning = true;
            Reopen = default;
            _ctx.MarkStatusDirty();

            var isReady = false;

            try
            {
                await AbortOpAsync();

                isReady = _ctx.Token.IsCancellationRequested == false;
            }
            finally
            {
                if (isReady == false)
                {
                    IsCleaning = false;
                    _ctx.MarkStatusDirty();
                }
            }

            return isReady;
        }

        public async UnityTask FadeOutPopupsAsync()
        {
            try
            {
                if (_pages.Count > 0)
                {
                    await _flows.HideAllAsync(_ctx.Scopes.Popup, _flows.CreateContext());
                }

                await HideRemainingAsync();
            }
            finally
            {
                IsCleaning = false;
                _ctx.MarkStatusDirty();
            }
        }

        public void CancelOpening()
        {
            if (Op is { IsShow: true } op)
            {
                op.Source.Abort(PageAbortMode.Reverse);
            }
        }

        public void KeepReading()
        {
            if (Op is { IsShow: false } op)
            {
                op.Source.Abort(PageAbortMode.Immediate);
            }
        }

        public void AbortPopup(PageAbortMode mode)
        {
            if (Op == null)
            {
                return;
            }

            var publisher = _ctx.Codex.FlowContext.Publisher.Scope(_ctx.Scopes.Popup);
            AbortPageMessage.Publish(in publisher, new AbortPageMessage(mode));
        }

        private void EndOp(PopupOp op)
        {
            if (ReferenceEquals(Op, op))
            {
                Op = null;
            }

            RefreshActive();
            _ctx.MarkStatusDirty();
        }

        private void RefreshActive()
        {
            var count = _pages.Count;
            var hasTop = count > 0;
            var top = hasTop ? _pages[count - 1] : default;

            if (Op is { IsShow: true } opening)
            {
                hasTop = true;
                top = opening.Page;
            }
            else if (Op is { IsShow: false } closing && hasTop && top == closing.Page)
            {
                hasTop = count > 1;
                top = hasTop ? _pages[count - 2] : default;
            }

            _ctx.State.SetPopupActive(hasTop, hasTop && top == DialogPage.More);
        }

        /// <summary>Aborts the running popup call at once and waits until it ends.</summary>
        /// <remarks>
        /// The <c>AbortCurrent</c> result is ignored on purpose: every failure means the call keeps running or has
        /// already ended, and the wait on <see cref="Op"/> is the confirmation.
        /// </remarks>
        private async UnityTask AbortOpAsync()
        {
            if (Op != null)
            {
                _ctx.Popup.AbortCurrent(PageAbortMode.Immediate);

                await UnityTask.WaitUntil(this, static popups => popups.Op == null, _ctx.Token);
            }
        }

        private async UnityTask HideRemainingAsync()
        {
            while (_pages.Count > 0 && _ctx.Token.IsCancellationRequested == false)
            {
                var count = _pages.Count;
                var context = _flows.CreateContext(hide: PageTransitionOptions.ZeroDuration);

                await _flows.HideActiveAsync(_ctx.Scopes.Popup, context);

                if (_pages.Count >= count)
                {
                    break;
                }
            }
        }

        private void ReportAbort(PopupOp op, in Success<PageFlowError> result)
        {
            if (IsCleaning
                || result.TryGetFailure(out var error) == false
                || error.TryGetAbort(out _, out _) == false
                || error.IsWithdrawn
            )
            {
                return;
            }

            if (op.IsShow == false)
            {
                _feedback.ShowToast(ConfigTextType.ToastKeptOpen, ToastKind.Cancelled);
            }
            else if (op.Page == DialogPage.Echo)
            {
                _feedback.ShowToast(ConfigTextType.ToastEchoClosed, ToastKind.Cancelled);
            }
            else if (op.Page == DialogPage.TowerIncantation)
            {
                _feedback.ShowToast(ConfigTextType.ToastIncantationWaiting, ToastKind.Cancelled);
            }
        }

        public sealed class PopupOp
        {
            public PopupOp(DialogPage page, bool isShow)
            {
                Page = page;
                IsShow = isShow;
            }

            public DialogPage Page { get; }

            public bool IsShow { get; }

            public Action OnDismiss { get; init; }

            public bool DismissRequested { get; set; }

            public PageAbortSource Source { get; } = new();

            public PageCallReceipt Receipt { get; } = new();
        }

        public readonly record struct ReopenOffer(DialogPage Page, Dimension Dimension, bool HasOffer) : IIsValid
        {
            public bool IsValid => HasOffer;
        }
    }
}
