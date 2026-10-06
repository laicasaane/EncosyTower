using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages
{
    public class ScreenBlue : UguiPageBehaviour<GamePageFlowScopes>
    {
        [SerializeField] private Button _buttonOpenScreen;
        [SerializeField] private Button _buttonOpenPopup;
        [SerializeField] private Button _buttonCloseScreen;

        private void Awake()
        {
            _buttonOpenScreen.onClick.AddListener(OnOpenSceneClick);
            _buttonOpenPopup.onClick.AddListener(OnOpenPopupClick);
            _buttonCloseScreen.onClick.AddListener(OnCloseScreenClick);
        }

        private void OnOpenSceneClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Screen);
            _ = ShowPageMessage.Async.Publish(in publisher, new ShowPageMessage("prefab-screen-red", default));
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
                , new ShowPageMessage("prefab-popup-green", new PageContext {
                    ShowOptions = PageTransitionOptions.OnlyFirstPageHasDuration,
                    HideOptions = PageTransitionOptions.NoTransition,
                })
            );
        }

        private void OnCloseScreenClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Screen);
            _ = HideActivePageMessage.Async.Publish(in publisher, new HideActivePageMessage(default));
        }
    }
}
