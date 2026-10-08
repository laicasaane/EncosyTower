using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages
{
    public class ScreenRed : UguiPageBehaviour<GamePageFlowScopes>
    {
        [SerializeField] private Button _buttonOpenScreen;
        [SerializeField] private Button _buttonOpenPopup;

        private void Awake()
        {
            _buttonOpenScreen.onClick.AddListener(OnOpenScreenClick);
            _buttonOpenPopup.onClick.AddListener(OnOpenPopupClick);
        }

        private void OnOpenScreenClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Screen);
            _ = ShowPageMessage.Async.Publish(in publisher, new ShowPageMessage("prefab-screen-blue", default));
        }

        private void OnOpenPopupClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            _ = ShowPageMessage.Async.Publish(
                  in publisher
                , new ShowPageMessage("prefab-popup-gray", new PageContext())
            );
        }
    }
}
