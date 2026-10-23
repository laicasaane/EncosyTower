using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Tasks;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>Switches the tab and toggles the cards of the dimension screen that is on display.</summary>
    internal sealed class DimensionController
    {
        private readonly CodexContext _ctx;
        private readonly FlowService _flows;
        private readonly ScreenController _screens;
        private readonly PopupController _popups;

        public DimensionController(
              CodexContext ctx
            , FlowService flows
            , ScreenController screens
            , PopupController popups
        )
        {
            _ctx = ctx;
            _flows = flows;
            _screens = screens;
            _popups = popups;
        }

        public async UnityTask SelectTabAsync(int index)
        {
            if (CanChange() == false || (uint)index > 1u)
            {
                return;
            }

            var state = _ctx.State;
            var dimension = state.CurrentDimension;

            if (state.ViewOf(dimension).Tab == index
                || state.Attachments.TryGetActive(BookLocation.Of(dimension), out var set) == false
            )
            {
                return;
            }

            await _flows.ShowAtAsync(set.TabsScope, index, new DimensionPageData(state, BookLocation.Of(dimension)));
        }

        public async UnityTask ToggleCardAsync(int index, bool toggle)
        {
            if (CanChange() == false || (uint)index > 1u)
            {
                return;
            }

            var state = _ctx.State;
            var dimension = state.CurrentDimension;

            if (state.Attachments.TryGetActive(BookLocation.Of(dimension), out var set) == false)
            {
                return;
            }

            var view = state.ViewOf(dimension);
            var visible = index == 0 ? view.ConceptVisible : view.InPracticeVisible;

            if (toggle == false && visible == false)
            {
                return;
            }

            var userData = new DimensionPageData(state, BookLocation.Of(dimension));

            if (visible)
            {
                await _flows.HideAtAsync(set.CardsScope, index, userData);
            }
            else
            {
                await _flows.ShowAtAsync(set.CardsScope, index, userData);
            }
        }

        private bool CanChange()
            => _ctx.IsReady
            && _screens.IsIdle
            && _ctx.State.CurrentScreen == ScreenPage.Dimension
            && _popups.Count == 0
            && _popups.Op == null;
    }
}
