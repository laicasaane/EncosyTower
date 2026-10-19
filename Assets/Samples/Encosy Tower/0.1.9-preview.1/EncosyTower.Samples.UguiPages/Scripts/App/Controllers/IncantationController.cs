using EncosyTower.PageFlows;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Tasks;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>The incantation of the core dimension: cast it, read the name, and write it in the book.</summary>
    internal sealed class IncantationController
    {
        private readonly CodexContext _ctx;
        private readonly PopupController _popups;
        private readonly ScreenController _screens;
        private readonly PersistentService _saves;

        public IncantationController(
              CodexContext ctx
            , PopupController popups
            , ScreenController screens
            , PersistentService saves
        )
        {
            _ctx = ctx;
            _popups = popups;
            _screens = screens;
            _saves = saves;
        }

        public async UnityTask CastAsync()
        {
            if (_ctx.IsReady == false
                || _screens.IsIdle == false
                || _popups.Op != null
                || _popups.Count > 0
                || _ctx.State.CurrentScreen != ScreenPage.CoreDimension
                || _ctx.State.CoreVisited == false
                || _ctx.State.IncantationCast
            )
            {
                return;
            }

            var result = await _popups.ShowPopupAsync(DialogPage.TowerIncantation, MainMenuData());

            if (result.IsApplied() && _ctx.State.CompleteIncantation())
            {
                _saves.RequestProgressSave();
            }
        }

        public async UnityTask ReadNameAsync()
        {
            if (_ctx.IsReady == false
                || _screens.IsIdle == false
                || _popups.Op != null
                || _popups.Count > 0
                || _ctx.State.IncantationCast == false
            )
            {
                return;
            }

            await _popups.ShowPopupAsync(DialogPage.TowerIncantation, MainMenuData());
        }

        public async UnityTask WriteInTheBookAsync()
        {
            if (_popups.IsTopPopup(DialogPage.TowerIncantation) == false
                || _popups.Op != null
                || _ctx.State.IncantationCast == false
            )
            {
                return;
            }

            if (_ctx.State.CurrentScreen == ScreenPage.Entrance)
            {
                await _popups.HideTopPopupAsync(offerReopen: false);
                return;
            }

            await _screens.TravelOrCloseTopPopupAsync(BookLocation.Entrance);
        }

        private DimensionPageData MainMenuData()
            => new(_ctx.State, BookLocation.MainMenu);
    }
}
