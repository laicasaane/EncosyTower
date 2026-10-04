using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages
{
    public class PopupGray : UguiPageBase<GamePageFlowScopes>
    {
        [SerializeField] private Button _buttonOpen;
        [SerializeField] private Button _buttonClose;

        private void Awake()
        {
            _buttonOpen.onClick.AddListener(OnOpenClick);
            _buttonClose.onClick.AddListener(OnCloseClick);
        }

        private void OnOpenClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            _ = ShowPageMessage.Async.Publish(
                  in publisher
                , new ShowPageMessage("prefab-popup-green", new PageContext())
            );
        }

        private void OnCloseClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            _ = HideActivePageMessage.Async.Publish(
                  in publisher
                , new HideActivePageMessage(new PageContext())
            );
        }
    }
}
