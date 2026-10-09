using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages
{
    public class PopupGray : UitkPageBehaviour<GamePageFlowScopes>, IPageOnCreateAsync
    {
        private const string OPEN_BUTTON = "button-open";
        private const string CLOSE_BUTTON = "button-close";

        private Button _buttonOpen;
        private Button _buttonClose;

        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UnityTask.FromResult(false);
            }

            if (_buttonOpen == null)
            {
                _buttonOpen = Slot.Q<Button>(OPEN_BUTTON);
                _buttonClose = Slot.Q<Button>(CLOSE_BUTTON);

                _buttonOpen.clicked += OnOpenClick;
                _buttonClose.clicked += OnCloseClick;
            }

            return UnityTask.FromResult(true);
        }

        protected override IPageTransition CreateTransition(VisualElement slot)
            => new PopupTransition(slot);

        private void OnDestroy()
        {
            if (_buttonOpen != null)
            {
                _buttonOpen.clicked -= OnOpenClick;
                _buttonClose.clicked -= OnCloseClick;
            }
        }

        private void OnOpenClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            var message = new ShowPageMessage(GameUitkCodex.POPUP_GREEN, new PageContext());
            _ = ShowPageMessage.Async.Publish(in publisher, message);
        }

        private void OnCloseClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            _ = HideActivePageMessage.Async.Publish(in publisher, new HideActivePageMessage(new PageContext()));
        }
    }
}
