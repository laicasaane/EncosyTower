using EncosyTower.Samples.UguiPages.Attachments;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Overlays;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>Turns the app state into the statuses that the navigation bars and the floating context render.</summary>
    internal sealed class StatusPresenter
    {
        private readonly CodexContext _ctx;
        private readonly ScreenController _screens;
        private readonly PopupController _popups;

        public StatusPresenter(CodexContext ctx, ScreenController screens, PopupController popups)
        {
            _ctx = ctx;
            _screens = screens;
            _popups = popups;
        }

        public void Render()
        {
            var attachments = _ctx.State.Attachments;
            var floatingContext = attachments.FloatingContext;

            attachments.RenderNavigationBars(BuildNavigationBarStatus());

            if (floatingContext.IsValid())
            {
                floatingContext.Render(BuildFloatingContextStatus());
            }
        }

        private NavigationBarStatus BuildNavigationBarStatus()
        {
            var history = _screens.History;
            var isMenu = _ctx.State.CurrentScreen == ScreenPage.MainMenu;
            var isEntrance = _ctx.State.CurrentScreen == ScreenPage.Entrance;
            var idle = _screens.IsIdle;
            var hasPopup = _popups.Count > 0;
            var popupPending = _popups.Op != null;

            return new NavigationBarStatus(
                  CanBack: isMenu == false
                    && (popupPending
                        || (idle
                            && (hasPopup || (history != null && history.CurrentIndex > 0))
                        ))
                , CanForward: idle && hasPopup == false && history != null && history.CanGoForward
                , CanMap: isMenu == false
                    && isEntrance == false
                    && idle
                    && _popups.IsTopPopup(DialogPage.Map) == false
                    && popupPending == false
                , CanSettings: isMenu == false && idle && popupPending == false
                , CanPick: isMenu == false && _ctx.IsBusy == false && _popups.IsCleaning == false
            );
        }

        private FloatingContextStatus BuildFloatingContextStatus()
        {
            var op = _popups.Op;
            var reopen = _popups.Reopen;
            var isMenu = _ctx.State.CurrentScreen == ScreenPage.MainMenu;
            var idle = _screens.IsIdle;
            var hasPopup = _popups.Count > 0;
            var mapOpen = _popups.IsTopPopup(DialogPage.Map);
            var popupPending = op != null;
            var hasFloatingContext = hasPopup || reopen.IsValid;
            var floatingContextPage = hasPopup ? _popups.Pages[^1] : reopen.Page;

            return new FloatingContextStatus(
                  ShowFloatingContext: isMenu == false
                    && mapOpen == false
                    && (hasPopup || popupPending || reopen.IsValid)
                , CanCancel: op is { IsShow: true }
                , CanStopClosing: op is { IsShow: false }
                , CanKeepReading: op is { IsShow: false }
                , CanReopen: reopen.IsValid && hasPopup == false && popupPending == false && idle
                , FloatingContextColor: FloatingContextColorOf(hasFloatingContext, floatingContextPage)
            );
        }

        private Color FloatingContextColorOf(bool hasPage, DialogPage page)
        {
            if (hasPage == false)
            {
                return _ctx.Config.Color(ConfigColorType.PageConfirmation);
            }

            switch (page)
            {
                case DialogPage.More:
                {
                    return _ctx.Config.Color(ConfigColorType.PageMore);
                }

                case DialogPage.Echo:
                case DialogPage.TowerIncantation:
                {
                    return _ctx.Config.Color(ConfigColorType.PageEcho);
                }

                default:
                {
                    return _ctx.Config.Color(ConfigColorType.PageConfirmation);
                }
            }
        }
    }
}
