using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Samples.UguiPages.App;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Messages;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Samples.UguiPages.State;
using EncosyTower.Samples.UguiPages.Views;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Overlays
{
    /// <summary>The visibility and actions available on the floating context overlay.</summary>
    internal readonly record struct FloatingContextStatus(
          bool ShowFloatingContext
        , bool CanCancel
        , bool CanStopClosing
        , bool CanKeepReading
        , bool CanReopen
        , Color FloatingContextColor
    );

    /// <summary>
    /// A floating context overlay with actions to cancel, undo, or reopen the current dialog.
    /// </summary>
    /// <remarks>
    /// The only page of the FloatingContext flow. Its buttons publish the Cancel, StopClosing, KeepReading and Reopen
    /// actions, which the app maps to <see cref="PageAbortMode"/> requests. It demonstrates abort from the UI.
    /// </remarks>
    public sealed class FloatingContextPage : UguiPageBehaviour<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnReturnToPool
    {
        /// <summary>The canvas group that shows or hides the floating context.</summary>
        [Tooltip("The canvas group that shows or hides the floating context.")]
        [SerializeField] private CanvasGroup _group;

        /// <summary>The background image of the floating context.</summary>
        [Tooltip("The background image of the floating context.")]
        [SerializeField] private Image _background;

        /// <summary>The button that cancels the dialog that is opening.</summary>
        [Tooltip("The button that cancels the dialog that is opening.")]
        [SerializeField] private Button _cancel;

        /// <summary>The button that stops the dialog that is closing.</summary>
        [Tooltip("The button that stops the dialog that is closing.")]
        [SerializeField] private Button _stopClosing;

        /// <summary>The button that keeps the dialog that is closing open.</summary>
        [Tooltip("The button that keeps the dialog that is closing open.")]
        [SerializeField] private Button _keepReading;

        /// <summary>The button that reopens the dialog that closed last.</summary>
        [Tooltip("The button that reopens the dialog that closed last.")]
        [SerializeField] private Button _reopen;

        private AppState _state;
        private bool _isRegistered;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;
                _cancel.onClick.AddListener(OnCancelClicked);
                _stopClosing.onClick.AddListener(OnStopClosingClicked);
                _keepReading.onClick.AddListener(OnKeepReadingClicked);
                _reopen.onClick.AddListener(OnReopenClicked);
            }

            _state = data.State;
            _state.Attachments.FloatingContext = this;

            Hide();
            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_state != null)
            {
                if (ReferenceEquals(_state.Attachments.FloatingContext, this))
                {
                    _state.Attachments.FloatingContext = null;
                }

                _state = null;
            }
        }

        private void OnDestroy()
        {
            if (_isRegistered)
            {
                _cancel.onClick.RemoveListener(OnCancelClicked);
                _stopClosing.onClick.RemoveListener(OnStopClosingClicked);
                _keepReading.onClick.RemoveListener(OnKeepReadingClicked);
                _reopen.onClick.RemoveListener(OnReopenClicked);
            }
        }

        internal void Render(in FloatingContextStatus status)
        {
            if (_state == null)
            {
                return;
            }

            var config = _state.Config;
            var navigationBar = config.Color(ConfigColorType.NavigationBar);
            var ink = config.Color(ConfigColorType.Ink);

            _group.alpha = status.ShowFloatingContext ? 1f : 0f;
            _group.blocksRaycasts = status.ShowFloatingContext;
            _group.interactable = status.ShowFloatingContext;
            _background.color = status.FloatingContextColor;

            BookView.SetColors(_cancel, navigationBar, ink, config);
            BookView.SetColors(_stopClosing, navigationBar, ink, config);
            BookView.SetColors(_keepReading, navigationBar, ink, config);
            BookView.SetColors(_reopen, navigationBar, ink, config);

            var height = config.Amount(ConfigAmountType.FloatingContextSelectionHeight);

            BookView.SetSelected(_cancel, status.CanCancel, SelectionEdge.Bottom, height);
            BookView.SetSelected(_stopClosing, status.CanStopClosing, SelectionEdge.Bottom, height);
            BookView.SetSelected(_keepReading, status.CanKeepReading, SelectionEdge.Bottom, height);
            BookView.SetSelected(_reopen, status.CanReopen, SelectionEdge.Bottom, height);

            var disabledAlpha = config.Amount(ConfigAmountType.FloatingContextDisabledAlpha);

            BookView.SetInteractable(_cancel, status.CanCancel, disabledAlpha);
            BookView.SetInteractable(_stopClosing, status.CanStopClosing, disabledAlpha);
            BookView.SetInteractable(_keepReading, status.CanKeepReading, disabledAlpha);
            BookView.SetInteractable(_reopen, status.CanReopen, disabledAlpha);
        }

        private void Hide()
        {
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }

        private void Publish(AppAction action)
        {
            BookActions.Publish(Publisher, FlowScopeCollection, action);
        }

        private void OnCancelClicked()
        {
            Publish(AppAction.Cancel);
        }

        private void OnStopClosingClicked()
        {
            Publish(AppAction.StopClosing);
        }

        private void OnKeepReadingClicked()
        {
            Publish(AppAction.KeepReading);
        }

        private void OnReopenClicked()
        {
            Publish(AppAction.Reopen);
        }
    }
}
