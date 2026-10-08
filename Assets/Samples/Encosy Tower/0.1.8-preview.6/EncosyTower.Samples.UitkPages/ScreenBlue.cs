using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages
{
    [UxmlElement]
    public partial class ScreenBlue : UitkPageElement<GamePageFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string OPEN_SCREEN_BUTTON = "button-open-screen";
        private const string OPEN_POPUP_BUTTON = "button-open-popup";
        private const string CLOSE_SCREEN_BUTTON = "button-close-screen";

        private Button _buttonOpenScreen;
        private Button _buttonOpenPopup;
        private Button _buttonCloseScreen;

        public override PageFocusMode FocusMode => PageFocusMode.FirstFocusable;

        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UnityTask.FromResult(false);
            }

            _buttonOpenScreen = this.Q<Button>(OPEN_SCREEN_BUTTON);
            _buttonOpenPopup = this.Q<Button>(OPEN_POPUP_BUTTON);
            _buttonCloseScreen = this.Q<Button>(CLOSE_SCREEN_BUTTON);

            _buttonOpenScreen.clicked += OnOpenScreenClick;
            _buttonOpenPopup.clicked += OnOpenPopupClick;
            _buttonCloseScreen.clicked += OnCloseScreenClick;

            return UnityTask.FromResult(true);
        }

        public void OnReturnToPool(PageContext context)
        {
            if (_buttonOpenScreen != null)
            {
                _buttonOpenScreen.clicked -= OnOpenScreenClick;
                _buttonOpenPopup.clicked -= OnOpenPopupClick;
                _buttonCloseScreen.clicked -= OnCloseScreenClick;
            }
        }

        protected override IPageTransition CreateTransition(VisualElement slot)
            => new UitkValueAnimationTransition(slot);

        private void OnOpenScreenClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Screen);
            var message = new ShowPageMessage(AssetKey: GameUitkCodex.SCREEN_RED, Context: default);
            _ = ShowPageMessage.Async.Publish(in publisher, message);
        }

        private void OnOpenPopupClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);

            var message = new ShowPageMessage(GameUitkCodex.POPUP_GREEN, new PageContext {
                ShowOptions = PageTransitionOptions.OnlyFirstPageHasDuration,
                HideOptions = PageTransitionOptions.NoTransition,
            });

            _ = ShowPageMessage.Async.Publish(in publisher, message);
        }

        private void OnCloseScreenClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Screen);
            _ = HideActivePageMessage.Async.Publish(in publisher, new HideActivePageMessage(Context: default));
        }
    }
}
