using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.Samples.UitkPages.App;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.Messages;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Samples.UitkPages.State;
using EncosyTower.Samples.UitkPages.Views;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Overlays
{
    /// <summary>Identifies the dialog represented by the floating context overlay's color.</summary>
    internal enum FloatingContextKind
    {
        Confirm,
        More,
        Echo,
    }

    /// <summary>The visibility and actions available on the floating context overlay.</summary>
    internal readonly record struct FloatingContextStatus(
          bool ShowFloatingContext
        , bool CanCancel
        , bool CanStopClosing
        , bool CanKeepReading
        , bool CanReopen
        , FloatingContextKind Kind
    );

    /// <summary>
    /// A floating context overlay with actions to cancel, undo, or reopen the current dialog.
    /// </summary>
    /// <remarks>
    /// The only page of the FloatingContext flow. Its buttons publish the Cancel, StopClosing, KeepReading and Reopen
    /// actions, which the app maps to <see cref="PageAbortMode"/> requests. It demonstrates abort from the UI.
    /// </remarks>
    [UxmlElement]
    public partial class FloatingContextPage : UitkPageElement<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnReturnToPool
    {
        private const string FLOATING_CONTEXT = "floating-context";
        private const string CANCEL_BUTTON = "button-cancel";
        private const string UNDO_CLOSE_BUTTON = "button-stop-closing";
        private const string KEEP_READING_BUTTON = "button-keep-reading";
        private const string REOPEN_BUTTON = "button-reopen";

        private readonly PaletteStyle _palette = new();

        private VisualElement _floatingContext;
        private Button _cancel;
        private Button _stopClosing;
        private Button _keepReading;
        private Button _reopen;
        private AppState _state;
        private FloatingContextStatus _status;
        private bool _isRegistered;
        private bool _hasStatus;

        /// <summary>Creates the floating context page element.</summary>
        public FloatingContextPage()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_floatingContext == null)
            {
                _floatingContext = this.Q(FLOATING_CONTEXT);
                _cancel = this.Q<Button>(CANCEL_BUTTON);
                _stopClosing = this.Q<Button>(UNDO_CLOSE_BUTTON);
                _keepReading = this.Q<Button>(KEEP_READING_BUTTON);
                _reopen = this.Q<Button>(REOPEN_BUTTON);
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;

                _cancel.clicked += OnCancelClicked;
                _stopClosing.clicked += OnStopClosingClicked;
                _keepReading.clicked += OnKeepReadingClicked;
                _reopen.clicked += OnReopenClicked;
            }

            Slot.pickingMode = PickingMode.Ignore;
            style.display = DisplayStyle.None;

            _state = data.State;
            _state.Attachments.FloatingContext = this;

            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_isRegistered)
            {
                _isRegistered = false;

                _cancel.clicked -= OnCancelClicked;
                _stopClosing.clicked -= OnStopClosingClicked;
                _keepReading.clicked -= OnKeepReadingClicked;
                _reopen.clicked -= OnReopenClicked;
            }

            if (_state != null)
            {
                if (ReferenceEquals(_state.Attachments.FloatingContext, this))
                {
                    _state.Attachments.FloatingContext = null;
                }

                _state = null;
            }

            _hasStatus = false;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the page root with the default duration.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new UitkValueAnimationTransition(slot);

        internal void Render(in FloatingContextStatus status)
        {
            _status = status;
            _hasStatus = true;

            Render();
        }

        private void Render()
        {
            if (_state == null || _hasStatus == false || _palette.IsResolved == false)
            {
                return;
            }

            var status = _status;

            style.display = status.ShowFloatingContext ? DisplayStyle.Flex : DisplayStyle.None;
            _floatingContext.style.backgroundColor = _palette.Color(ColorNameOf(status.Kind));

            var barHeight = _palette.Amount(ConfigAmountType.FloatingContextSelectionHeight);

            SetBottomBar(_cancel, status.CanCancel, barHeight);
            SetBottomBar(_stopClosing, status.CanStopClosing, barHeight);
            SetBottomBar(_keepReading, status.CanKeepReading, barHeight);
            SetBottomBar(_reopen, status.CanReopen, barHeight);

            BookView.AttachStates(this, _palette);

            BookView.SetEnabled(_cancel, status.CanCancel);
            BookView.SetEnabled(_stopClosing, status.CanStopClosing);
            BookView.SetEnabled(_keepReading, status.CanKeepReading);
            BookView.SetEnabled(_reopen, status.CanReopen);
        }

        private static void SetBottomBar(Button button, bool isOn, float height)
        {
            if (isOn)
            {
                button.style.borderBottomWidth = height;
            }
            else
            {
                button.style.borderBottomWidth = StyleKeyword.Null;
            }
        }

        private static ConfigColorType ColorNameOf(FloatingContextKind kind)
            => kind switch {
                FloatingContextKind.More => ConfigColorType.PageMore,
                FloatingContextKind.Echo => ConfigColorType.PageEcho,
                _ => ConfigColorType.PageConfirmation,
            };

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _palette.Read(evt.customStyle, ThemeAPI.IsDark(this));

            Render();
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
