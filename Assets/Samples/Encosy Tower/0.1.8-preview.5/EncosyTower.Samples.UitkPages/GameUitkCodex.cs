using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Tasks;

namespace EncosyTower.Samples.UitkPages
{
    public sealed class GameUitkCodex : UitkPageCodexInitializer<GamePageFlowScopes>
    {
        public const string SCREEN_RED = "uitk-screen-red";
        public const string SCREEN_BLUE = "uitk-screen-blue";
        public const string POPUP_GRAY = "uitk-popup-gray";
        public const string POPUP_GREEN = "uitk-popup-green";

        protected override UnityTask OnInitializeAsync(UitkPageCodex codex, GamePageFlowScopes scopes)
        {
            var publisher = codex.FlowContext.Publisher.Scope(scopes.Screen);

            _ = ShowPageMessage.Async.Publish(
                  in publisher
                , new ShowPageMessage(SCREEN_RED, new PageContext {
                    ShowOptions = PageTransitionOptions.NoTransition,
                })
            );

            return UnityTask.CompletedTask;
        }
    }
}
