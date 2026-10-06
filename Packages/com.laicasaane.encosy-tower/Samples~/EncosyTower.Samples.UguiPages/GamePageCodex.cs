using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Tasks;

namespace EncosyTower.Samples.UguiPages
{
    public sealed class GamePageCodex : UguiPageCodexInitializer<GamePageFlowScopes>
    {
        private UguiPageCodex _codex;

        protected override UnityTask OnInitializeAsync(UguiPageCodex codex, GamePageFlowScopes scopes)
        {
            _codex = codex;
            ShowStartScreen(scopes);
            return UnityTask.CompletedTask;
        }

        private void ShowStartScreen(GamePageFlowScopes scopes)
        {
            var publisher = _codex.FlowContext.Publisher.Scope(scopes.Screen);

            _ = ShowPageMessage.Async.Publish(
                  in publisher
                , new ShowPageMessage("prefab-screen-red", new PageContext {
                    ShowOptions = PageTransitionOptions.NoTransition,
                })
            );
        }
    }
}
