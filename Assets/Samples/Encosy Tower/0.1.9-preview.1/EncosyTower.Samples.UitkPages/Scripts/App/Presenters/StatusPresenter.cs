using EncosyTower.Samples.UitkPages.Attachments;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.Overlays;

namespace EncosyTower.Samples.UitkPages.App
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
            _ctx.Attachments.RenderNavigationBars(BuildNavigationBarStatus());
            _ctx.Attachments.FloatingContext.Render(BuildFloatingContextStatus());
        }

        private NavigationBarStatus BuildNavigationBarStatus()
        {
            var history = _screens.History;
            var isMenu = _ctx.State.CurrentScreen == ScreenPage.MainMenu;
            var isEntrance = _ctx.State.CurrentScreen == ScreenPage.Entrance;
            var idle = _screens.IsIdle;
            var hasPopup = _popups.Count > 0;
            var mapOpen = _popups.IsTopPopup(DialogPage.Map);
            var popupPending = _popups.Op != null;

            return new NavigationBarStatus(
                  CanBack: isMenu == false
                    && (popupPending
                        || (idle
                            && (hasPopup || (history != null && history.CurrentIndex > 0))
                        ))
                , CanForward: idle && hasPopup == false && history != null && history.CanGoForward
                , CanMap: isMenu == false && isEntrance == false && idle && mapOpen == false && popupPending == false
                , CanSettings: isMenu == false && idle && popupPending == false
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
                  ShowFloatingContext: isMenu == false && mapOpen == false && (hasFloatingContext || popupPending)
                , CanCancel: op is { IsShow: true }
                , CanStopClosing: op is { IsShow: false }
                , CanKeepReading: op is { IsShow: false }
                , CanReopen: reopen.IsValid && hasPopup == false && popupPending == false && idle
                , Kind: hasFloatingContext ? FloatingContextKindOf(floatingContextPage) : FloatingContextKind.Confirm
            );
        }

        private static FloatingContextKind FloatingContextKindOf(DialogPage page)
            => page switch {
                DialogPage.More => FloatingContextKind.More,
                DialogPage.Echo or DialogPage.TowerIncantation => FloatingContextKind.Echo,
                _ => FloatingContextKind.Confirm,
            };
    }
}
