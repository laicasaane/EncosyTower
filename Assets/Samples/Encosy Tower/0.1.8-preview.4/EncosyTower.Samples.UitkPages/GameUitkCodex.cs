using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Tasks;

namespace EncosyTower.Samples.UitkPages
{
    public sealed class GameUitkCodex : UitkPageCodexInitializer<GamePageFlowScopes>
    {
        public const string SCREEN_MAIN = "uitk-screen-main";
        public const string POPUP_INFO = "uitk-popup-info";

        protected override UnityTask OnInitializeAsync(UitkPageCodex codex, GamePageFlowScopes scopes)
        {
            var publisher = codex.FlowContext.Publisher.Scope(scopes.Screen);

            _ = ShowPageMessage.Async.Publish(
                  in publisher
                , new ShowPageMessage(SCREEN_MAIN, new PageContext {
                    ShowOptions = PageTransitionOptions.NoTransition,
                })
            );

            return UnityTask.CompletedTask;
        }
    }
}
