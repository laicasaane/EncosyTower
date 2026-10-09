using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages
{
    [UxmlElement]
    public partial class ScreenRed : UitkPageElement<GamePageFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string OPEN_SCREEN_BUTTON = "button-open-screen";
        private const string OPEN_POPUP_BUTTON = "button-open-popup";

        private Button _buttonOpenScreen;
        private Button _buttonOpenPopup;

        public override PageFocusMode FocusMode => PageFocusMode.FirstFocusable;

        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UnityTask.FromResult(false);
            }

            _buttonOpenScreen = this.Q<Button>(OPEN_SCREEN_BUTTON);
            _buttonOpenPopup = this.Q<Button>(OPEN_POPUP_BUTTON);

            _buttonOpenScreen.clicked += OnOpenScreenClick;
            _buttonOpenPopup.clicked += OnOpenPopupClick;

            return UnityTask.FromResult(true);
        }

        public void OnReturnToPool(PageContext context)
        {
            if (_buttonOpenScreen != null)
            {
                _buttonOpenScreen.clicked -= OnOpenScreenClick;
                _buttonOpenPopup.clicked -= OnOpenPopupClick;
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
            var message = new ShowPageMessage(AssetKey: GameUitkCodex.SCREEN_BLUE, Context: default);
            _ = ShowPageMessage.Async.Publish(in publisher, message);
        }

        private void OnOpenPopupClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            var message = new ShowPageMessage(GameUitkCodex.POPUP_GRAY, new PageContext());
            _ = ShowPageMessage.Async.Publish(in publisher, message);
        }
    }
}
