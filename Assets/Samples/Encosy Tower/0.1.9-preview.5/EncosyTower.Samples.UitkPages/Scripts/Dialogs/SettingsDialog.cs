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

namespace EncosyTower.Samples.UitkPages.Dialogs
{
    /// <summary>The settings dialog: the color theme, travel to the entrance, and exit to the main menu.</summary>
    /// <remarks>
    /// A page of the Popup flow. It keeps the page below it with <see cref="PopupPageOptions.KeepLowerPage"/>.
    /// </remarks>
    [UxmlElement]
    public partial class SettingsDialog : UitkPageElement<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string TITLE_LABEL = "label-title";
        private const string THEME_LABEL = "label-theme";
        private const string THEME_SWITCH = "theme-switch";
        private const string ENTRANCE_BUTTON = "button-settings-entrance";
        private const string LEAVE_BUTTON = "button-settings-exit";
        private const string CLOSE_BUTTON = "button-close";

        private Label _title;
        private Label _theme;
        private ThemeSwitch _themeSwitch;
        private Button _entrance;
        private Button _leave;
        private Button _close;
        private readonly DialogBackdrop _dismissal = new();

        private AppState _state;

        /// <inheritdoc/>
        public override PageFocusMode FocusMode => PageFocusMode.FirstFocusable;

        /// <inheritdoc/>
        public override PageOptions PageOptions => PopupPageOptions.KeepLowerPage;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_title == null)
            {
                _title = this.Q<Label>(TITLE_LABEL);
                _theme = this.Q<Label>(THEME_LABEL);
                _themeSwitch = this.Q<ThemeSwitch>(THEME_SWITCH);
                _entrance = this.Q<Button>(ENTRANCE_BUTTON);
                _leave = this.Q<Button>(LEAVE_BUTTON);
                _close = this.Q<Button>(CLOSE_BUTTON);

                _entrance.clicked += OnEntranceClicked;
                _leave.clicked += OnLeaveClicked;
                _close.clicked += OnCloseClicked;
            }

            _state = data.State;
            _dismissal.Bind(this, data.Dismiss);

            BookView.AttachStates(this);

            var resources = _state.Resources;

            _title.text = resources.Get(ConfigTextType.SettingsTitle);
            _theme.text = resources.Get(ConfigTextType.SettingsTheme);
            _entrance.text = resources.Get(ConfigTextType.SettingsGoToEntrance);
            _leave.text = resources.Get(ConfigTextType.SettingsExit);
            BookView.SetEnabled(_entrance, _state.CurrentScreen != ScreenPage.Entrance);

            _themeSwitch.Bind(
                  _state
                , resources.Get(ConfigTextType.SettingsThemeDark)
                , resources.Get(ConfigTextType.SettingsThemeLight)
            );

            return UnityTask.FromResult(true);
        }

        public void OnReturnToPool(PageContext context)
        {
            _dismissal.Unbind();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the whole dialog as one unit through its root, so every element fades together, instead of the
        /// two-part <c>PopupTransition</c>.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
        {
            var durationMs = _state.Timings.PopupTransitionMs;

            return new UitkValueAnimationTransition(slot) {
                ShowDurationMs = durationMs,
                HideDurationMs = durationMs,
            };
        }

        private void OnEntranceClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.Travel, BookLocation.Entrance);
        }

        private void OnLeaveClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.Exit);
        }

        private void OnCloseClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.Done);
        }
    }
}
