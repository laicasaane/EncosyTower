using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages
{
    public class PopupInfo : UitkPageBehaviour<GamePageFlowScopes>, IPageOnCreateAsync
    {
        public const string CLOSE_BUTTON = "close-button";

        private Button _closeButton;

        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (_closeButton == null)
            {
                _closeButton = Slot.Q<Button>(CLOSE_BUTTON);

                if (_closeButton != null)
                {
                    _closeButton.clicked += OnCloseClicked;
                }
            }

            return UnityTask.FromResult(true);
        }

        private void OnCloseClicked()
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
