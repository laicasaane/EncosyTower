using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages
{
    public class PopupGreen : UguiPageBehaviour<GamePageFlowScopes>
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
