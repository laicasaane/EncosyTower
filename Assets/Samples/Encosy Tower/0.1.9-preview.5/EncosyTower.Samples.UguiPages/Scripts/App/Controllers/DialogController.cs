using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Tasks;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>Opens and closes the dialogs the player asks for: More, Echo, Appendix, Map, Settings, reopen.</summary>
    internal sealed class DialogController
    {
        private readonly CodexContext _ctx;
        private readonly FlowService _flows;
        private readonly PopupController _popups;
        private readonly ScreenController _screens;

        public DialogController(
              CodexContext ctx
            , FlowService flows
            , PopupController popups
            , ScreenController screens
        )
        {
            _ctx = ctx;
            _flows = flows;
            _popups = popups;
            _screens = screens;
        }

        /// <summary>True while the appendix is being opened, so a failure reads as a missing page.</summary>
        public bool IsAppendixPending { get; private set; }

        public bool CanOpenPopup()
            => _ctx.IsReady
            && _screens.IsIdle
            && _popups.Op == null
            && _ctx.State.CurrentScreen != ScreenPage.MainMenu;

        public async UnityTask OpenActionsAsync()
        {
            if (CanOpenPopup() == false || _popups.Count > 0 || _ctx.State.CurrentScreen != ScreenPage.Dimension)
            {
                return;
            }

            await _popups.ShowPopupAsync(DialogPage.More, CurrentDimensionData());
        }

        public async UnityTask OpenEchoAsync()
        {
            if (CanOpenPopup() == false
                || _ctx.State.CurrentScreen != ScreenPage.Dimension
                || (_popups.Count > 0 && _popups.IsTopPopup(DialogPage.More) == false)
            )
            {
                return;
            }

            await _popups.ShowPopupAsync(DialogPage.Echo, CurrentDimensionData());
        }

        public async UnityTask OpenAppendixAsync()
        {
            if (CanOpenPopup() == false || _popups.IsTopPopup(DialogPage.Echo) == false)
            {
                return;
            }

            IsAppendixPending = true;

            try
            {
                var context = _flows.CreateContext();
                await _flows.ShowAsync(_ctx.Scopes.Popup, PageKeys.LostAppendix, context);
            }
            finally
            {
                IsAppendixPending = false;
            }
        }

        public async UnityTask OpenMapAsync()
        {
            if (_ctx.IsReady == false
                || _screens.IsIdle == false
                || _ctx.State.CurrentScreen == ScreenPage.MainMenu
                || _popups.IsTopPopup(DialogPage.Map)
            )
            {
                return;
            }

            await _popups.ClosePopupsAsync();
            await _popups.ShowPopupAsync(DialogPage.Map, MainMenuData());
        }

        public async UnityTask OpenSettingsAsync()
        {
            if (CanOpenPopup() == false || _popups.IsTopPopup(DialogPage.Settings))
            {
                return;
            }

            await _popups.ClosePopupsAsync();
            await _popups.ShowPopupAsync(DialogPage.Settings, MainMenuData());
        }

        public async UnityTask CloseMapAsync()
        {
            if (_popups.IsTopPopup(DialogPage.Map) == false || _popups.Op != null)
            {
                return;
            }

            await _popups.HideTopPopupAsync(offerReopen: false);
        }

        public async UnityTask MapTravelAsync(BookLocation location)
        {
            if (_popups.IsTopPopup(DialogPage.Map) == false
                || _popups.Op != null
                || _ctx.State.IsAvailable(location) == false
            )
            {
                return;
            }

            await _screens.TravelOrCloseTopPopupAsync(location);
        }

        public async UnityTask ReopenAsync()
        {
            if (_popups.Reopen.IsValid == false || _popups.Count > 0 || CanOpenPopup() == false)
            {
                return;
            }

            var offer = _popups.Reopen;

            _popups.ClearReopen();
            await _popups.ShowPopupAsync(offer.Page, new DimensionPageData(_ctx.State, BookLocation.Of(offer.Dimension)));
        }

        private DimensionPageData CurrentDimensionData()
            => new(_ctx.State, BookLocation.Of(_ctx.State.CurrentDimension));

        private DimensionPageData MainMenuData()
            => new(_ctx.State, BookLocation.MainMenu);
    }
}
