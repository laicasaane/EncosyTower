using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages
{
    [UxmlElement]
    public partial class ScreenMain : UitkPageElement<GamePageFlowScopes>
    {
        public const string SHOW_POPUP_BUTTON = "show-popup-button";
        public const string HIDE_SCREEN_BUTTON = "hide-screen-button";

        public ScreenMain()
        {
            RegisterCallback<ClickEvent>(OnClick);
        }

        public override PageFocusMode FocusMode => PageFocusMode.FirstFocusable;

        private void OnClick(ClickEvent evt)
        {
            if (evt.target is not Button button)
            {
                return;
            }

            switch (button.name)
            {
                case SHOW_POPUP_BUTTON:
                {
                    ShowPopup();
                    break;
                }

                case HIDE_SCREEN_BUTTON:
                {
                    HideScreen();
                    break;
                }
            }
        }

        private void ShowPopup()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            _ = ShowPageMessage.Async.Publish(
                  in publisher
                , new ShowPageMessage(GameUitkCodex.POPUP_INFO, new PageContext())
            );
        }

        private void HideScreen()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Screen);
            _ = HideActivePageMessage.Async.Publish(in publisher, new HideActivePageMessage(new PageContext()));
        }
    }
}
