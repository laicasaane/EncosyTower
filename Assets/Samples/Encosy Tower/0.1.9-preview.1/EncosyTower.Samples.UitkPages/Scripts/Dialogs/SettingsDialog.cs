using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.Samples.UitkPages.App;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.Messages;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Samples.UitkPages.State;
using EncosyTower.Samples.UitkPages.Views;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Dialogs
{
    /// <summary>The settings dialog: the color theme, and exit to the main menu.</summary>
    /// <remarks>
    /// A page of the Popup flow. It keeps the page below it with <see cref="PopupPageOptions.KeepLowerPage"/>.
    /// </remarks>
    [UxmlElement]
    public partial class SettingsDialog : UitkPageElement<AppFlowScopes>, IPageOnCreateAsync
    {
        private const string TITLE_LABEL = "label-title";
        private const string THEME_LABEL = "label-theme";
        private const string THEME_SWITCH = "theme-switch";
        private const string LEAVE_BUTTON = "button-settings-exit";

        private Label _title;
        private Label _theme;
        private ThemeSwitch _themeSwitch;
        private Button _leave;
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
                _leave = this.Q<Button>(LEAVE_BUTTON);

                _leave.clicked += OnLeaveClicked;
            }

            _state = data.State;

            BookView.AttachStates(this);

            var resources = _state.Resources;

            _title.text = resources.Get(ConfigTextType.SettingsTitle);
            _theme.text = resources.Get(ConfigTextType.SettingsTheme);
            _leave.text = resources.Get(ConfigTextType.SettingsExit);

            _themeSwitch.Bind(
                  _state
                , resources.Get(ConfigTextType.SettingsThemeDark)
                , resources.Get(ConfigTextType.SettingsThemeLight)
            );

            return UnityTask.FromResult(true);
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

        private void OnLeaveClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.Exit);
        }
    }
}
