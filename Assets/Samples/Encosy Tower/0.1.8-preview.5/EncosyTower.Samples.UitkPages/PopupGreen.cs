using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages
{
    public class PopupGreen : UitkPageBehaviour<GamePageFlowScopes>, IPageOnCreateAsync
    {
        private const string CLOSE_BUTTON = "button-close";

        private Button _buttonClose;

        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UnityTask.FromResult(false);
            }

            if (_buttonClose == null)
            {
                _buttonClose = Slot.Q<Button>(CLOSE_BUTTON);

                _buttonClose.clicked += OnCloseClick;
            }

            return UnityTask.FromResult(true);
        }

        protected override IPageTransition CreateTransition(VisualElement slot)
            => new PopupTransition(slot);

        private void OnDestroy()
        {
            if (_buttonClose != null)
            {
                _buttonClose.clicked -= OnCloseClick;
            }
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
