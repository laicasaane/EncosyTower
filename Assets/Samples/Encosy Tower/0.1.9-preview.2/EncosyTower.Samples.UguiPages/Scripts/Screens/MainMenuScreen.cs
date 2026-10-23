using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Samples.UguiPages.App;
using EncosyTower.Samples.UguiPages.Config;
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

namespace EncosyTower.Samples.UguiPages.Screens
{
    /// <summary>The main menu screen: start a new game or continue the saved one.</summary>
    /// <remarks>
    /// A page of the Screen flow. It demonstrates <see cref="IPageOnCreateAsync"/> and
    /// <see cref="IPageOnReturnToPool"/>.
    /// </remarks>
    public class MainMenuScreen : UguiPageBehaviour<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string BACKGROUND_NAME = "background";
        private const string TITLE_NAME = "label-title";
        private const string SUBTITLE_NAME = "label-subtitle";

        /// <summary>The button that continues the saved game.</summary>
        [Tooltip("The button that continues the saved game.")]
        [SerializeField] private Button _continue;

        /// <summary>The button that starts a new game.</summary>
        [Tooltip("The button that starts a new game.")]
        [SerializeField] private Button _newGame;

        private AppState _state;
        private Image[] _backgrounds;
        private TMP_Text[] _titles;
        private TMP_Text[] _subtitles;
        private bool _isRegistered;

        private void Awake()
        {
            _continue.interactable = false;
        }

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
                _continue.onClick.AddListener(OnContinueClicked);
                _newGame.onClick.AddListener(OnNewGameClicked);

                _backgrounds = BookView.FindChildren<Image>(this, BACKGROUND_NAME);
                _titles = BookView.FindChildren<TMP_Text>(this, TITLE_NAME);
                _subtitles = BookView.FindChildren<TMP_Text>(this, SUBTITLE_NAME);
            }

            _state = data.State;
            _state.Changed += OnStateChanged;

            Render();
            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_state != null)
            {
                _state.Changed -= OnStateChanged;
                _state = null;
            }
        }

        private void OnDestroy()
        {
            if (_isRegistered)
            {
                _continue.onClick.RemoveListener(OnContinueClicked);
                _newGame.onClick.RemoveListener(OnNewGameClicked);
            }
        }

        private void Render()
        {
            var config = _state.Config;
            var ink = config.Color(ConfigColorType.Ink);
            var page = config.Color(ConfigColorType.PageMainMenu);

            BookView.SetColors(_backgrounds, page);
            BookView.SetColors(_titles, ink);
            BookView.SetColors(_subtitles, ink);
            SetMenuButtonColors(_continue, _state.HasSavedGame, ink, page, config);
            SetMenuButtonColors(_newGame, true, ink, page, config);
            BookView.SetInteractable(_continue, _state.HasSavedGame, config.Amount(ConfigAmountType.MenuDisabledAlpha));
            BookView.SetInteractable(_newGame, true, config);
        }

        private static void SetMenuButtonColors(
              Button button
            , bool enabled
            , Color ink
            , Color page
            , AppConfig config
        )
        {
            if (enabled == false)
            {
                BookView.SetColors(button, config.Color(ConfigColorType.NavigationBar), ink, config);
                SetBorderColor(button, config.Color(ConfigColorType.ButtonBorder));
                BookView.SetGenericLooks(button, config);
                return;
            }

            BookView.SetColors(button, ink, page, config);
            SetBorderColor(button, ink);

            var hoverBackground = config.Color(ConfigColorType.MenuHoverBackground);
            var hoverInk = config.Color(ConfigColorType.MenuHoverInk);
            var pressBackground = config.Color(ConfigColorType.MenuPressBackground);
            var hover = new ButtonLook(hoverBackground, hoverBackground, hoverInk);
            var press = new ButtonLook(pressBackground, hoverBackground, hoverInk);

            BookView.SetStateLooks(button, in hover, in press, ink);
        }

        private static void SetBorderColor(Button button, Color color)
        {
            var border = BookView.FindBorder(button);

            if (border.IsValid())
            {
                border.color = color;
            }
        }

        private void OnStateChanged()
        {
            Render();
        }

        private void OnContinueClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.Continue);
        }

        private void OnNewGameClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.NewGame);
        }
    }
}
