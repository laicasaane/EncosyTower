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
using EncosyTower.UnityExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Dialogs
{
    /// <summary>The settings dialog: the color theme, and exit to the main menu.</summary>
    /// <remarks>
    /// A page of the Popup flow. Fades as one unit through the root <c>CanvasGroup</c> transition of its prefab.
    /// </remarks>
    public class SettingsDialog : UguiPageBehaviour<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        /// <summary>The backdrop behind the dialog, shown when it is the first popup.</summary>
        [Tooltip("The backdrop behind the dialog, shown when it is the first popup.")]
        [SerializeField] private GameObject _backdrop;

        /// <summary>The panel of the dialog.</summary>
        [Tooltip("The panel of the dialog.")]
        [SerializeField] private Image _panel;

        /// <summary>The title of the dialog.</summary>
        [Tooltip("The title of the dialog.")]
        [SerializeField] private TMP_Text _title;

        /// <summary>The label of the theme switch.</summary>
        [Tooltip("The label of the theme switch.")]
        [SerializeField] private TMP_Text _themeLabel;

        /// <summary>The switch that picks the light or the dark theme.</summary>
        [Tooltip("The switch that picks the light or the dark theme.")]
        [SerializeField] private ThemeSwitch _themeSwitch;

        /// <summary>The button that exits to the main menu.</summary>
        [Tooltip("The button that exits to the main menu.")]
        [SerializeField] private Button _exit;

        private AppState _state;
        private bool _isRegistered;

        /// <inheritdoc/>
        /// <remarks>
        /// It rejects the creation when the call has no page user data.
        /// </remarks>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;
                _exit.onClick.AddListener(OnExitClicked);
            }

            UnsubscribeState();

            _state = data.State;
            _state.ThemeChanged += OnThemeChanged;

            var config = _state.Config;

            _backdrop.SetActive(_state.PopupDepth == 0);
            _title.text = config.Text(ConfigTextType.SettingsTitle);
            _themeLabel.text = config.Text(ConfigTextType.SettingsTheme);

            BookView.SetLabel(_exit, config.Text(ConfigTextType.SettingsExit));
            RefreshColors();
            _themeSwitch.Bind(_state);

            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            _themeSwitch.Unbind();
            UnsubscribeState();
        }

        private void OnDestroy()
        {
            _themeSwitch.Unbind();
            UnsubscribeState();

            if (_isRegistered)
            {
                _exit.onClick.RemoveListener(OnExitClicked);
            }
        }

        private void UnsubscribeState()
        {
            if (_state != null)
            {
                _state.ThemeChanged -= OnThemeChanged;
                _state = null;
            }
        }

        private void RefreshColors()
        {
            var config = _state.Config;

            var ink = config.Color(ConfigColorType.Ink);
            var navigationBar = config.Color(ConfigColorType.NavigationBar);

            _panel.color = config.Color(ConfigColorType.PageConfirmation);
            _title.color = ink;
            _themeLabel.color = ink;

            var border = BookView.FindBorder(_panel);

            if (border.IsValid())
            {
                border.color = config.Color(ConfigColorType.ButtonBorder);
            }

            BookView.SetBackdrop(_backdrop, config);
            BookView.SetColors(_exit, navigationBar, ink, config);
        }

        private void OnThemeChanged()
        {
            RefreshColors();
        }

        private void OnExitClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.Exit);
        }
    }
}
