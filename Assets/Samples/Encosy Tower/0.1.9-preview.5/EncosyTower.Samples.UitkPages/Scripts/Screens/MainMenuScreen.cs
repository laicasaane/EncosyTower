using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.Samples.UitkPages.App;
using EncosyTower.Samples.UitkPages.Messages;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Samples.UitkPages.State;
using EncosyTower.Samples.UitkPages.Views;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Screens
{
    /// <summary>The main menu screen: start a new game or continue the saved one.</summary>
    /// <remarks>
    /// A page of the Screen flow. It demonstrates <see cref="IPageOnCreateAsync"/> and
    /// <see cref="IPageOnReturnToPool"/>.
    /// </remarks>
    [UxmlElement]
    public partial class MainMenuScreen : UitkPageElement<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string CONTINUE_BUTTON = "button-continue";
        private const string NEW_GAME_BUTTON = "button-new-game";

        private Button _buttonContinue;
        private Button _buttonNewGame;
        private AppState _state;

        /// <inheritdoc/>
        public override PageFocusMode FocusMode => PageFocusMode.FirstFocusable;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_buttonContinue == null)
            {
                _buttonContinue = this.Q<Button>(CONTINUE_BUTTON);
                _buttonNewGame = this.Q<Button>(NEW_GAME_BUTTON);

                _buttonContinue.clicked += OnContinueClicked;
                _buttonNewGame.clicked += OnNewGameClicked;
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

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the page root over the screen transition time.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new UitkValueAnimationTransition(slot) {
                ShowDurationMs = _state.Timings.ScreenTransitionMs,
                HideDurationMs = _state.Timings.ScreenTransitionMs,
            };

        private void Render()
        {
            BookView.AttachStates(this);

            BookView.SetEnabled(_buttonContinue, _state.HasSavedGame);
            BookView.SetEnabled(_buttonNewGame, true);
        }

        private void OnStateChanged()
        {
            Render();
        }

        private void OnContinueClicked()
        {
            Publish(AppAction.Continue);
        }

        private void OnNewGameClicked()
        {
            Publish(AppAction.NewGame);
        }

        private void Publish(AppAction action)
        {
            BookActions.Publish(Publisher, FlowScopeCollection, action);
        }
    }
}
