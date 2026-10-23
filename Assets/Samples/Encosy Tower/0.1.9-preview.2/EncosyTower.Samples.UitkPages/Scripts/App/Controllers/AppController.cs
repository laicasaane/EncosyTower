using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.Overlays;
using EncosyTower.Samples.UitkPages.Persistence;
using EncosyTower.Tasks;
using ThrowHelper = EncosyTower.Samples.UitkPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UitkPages.App
{
    /// <summary>The life of a play session: start a new game, continue a saved one, or exit to the main menu.</summary>
    internal sealed class AppController
    {
        private readonly CodexContext _ctx;
        private readonly ScreenController _screens;
        private readonly PersistentService _saves;
        private readonly FeedbackPresenter _feedback;

        public AppController(
              CodexContext ctx
            , ScreenController screens
            , PersistentService saves
            , FeedbackPresenter feedback
        )
        {
            _ctx = ctx;
            _screens = screens;
            _saves = saves;
            _feedback = feedback;
        }

        public async UnityTask NewGameAsync()
        {
            if (_ctx.IsReady == false || _ctx.IsBusy || _ctx.State.CurrentScreen != ScreenPage.MainMenu)
            {
                return;
            }

            _ctx.IsBusy = true;
            _ctx.MarkStatusDirty();

            try
            {
                var progress = new UserData { RuneForDimension = RuneCatalog.NewPermutation(_ctx.Random), Theme = _ctx.State.Theme };

                _ctx.State.Load(progress, hasSavedGame: false);

                if (_saves.IsBlocked)
                {
                    ThrowHelper.LogError_SaveUnavailable();
                }
                else if (await UserStorage.SaveProgressAsync(progress, _ctx.Token))
                {
                    _ctx.State.SetSavedGame(true);
                }
                else
                {
                    ThrowHelper.LogWarning_ProgressWriteFailed();
                    _feedback.ShowToast(ConfigTextType.ToastSaveFailed, ToastKind.Failure);
                }
            }
            finally
            {
                _ctx.IsBusy = false;
            }

            await StartBrowsingAsync();
        }

        public async UnityTask ContinueAsync()
        {
            if (_ctx.IsReady == false
                || _ctx.IsBusy
                || _ctx.State.CurrentScreen != ScreenPage.MainMenu
                || _ctx.State.HasSavedGame == false
            )
            {
                return;
            }

            _ctx.IsBusy = true;
            _ctx.MarkStatusDirty();

            try
            {
                var loaded = await UserStorage.LoadProgressAsync(_ctx.Token);

                if (loaded.TryGetValue(out var progress) == false)
                {
                    _ctx.State.SetSavedGame(false);
                    _feedback.ShowToast(ConfigTextType.ToastSaveUnreadable, ToastKind.Failure);
                    return;
                }

                _ctx.State.Load(progress, hasSavedGame: true);
            }
            finally
            {
                _ctx.IsBusy = false;
            }

            await StartBrowsingAsync();
        }

        public async UnityTask ExitAsync()
        {
            if (_ctx.IsReady == false || _screens.IsIdle == false || _ctx.State.CurrentScreen == ScreenPage.MainMenu)
            {
                return;
            }

            _ctx.IsBusy = true;
            _ctx.MarkStatusDirty();

            try
            {
                _screens.DisposeHistory();

                var progress = _ctx.State.CreateProgress();

                if (_saves.IsBlocked == false && await UserStorage.SaveProgressAsync(progress, _ctx.Token))
                {
                    _ctx.State.SetSavedGame(true);
                    _ctx.State.SetProgressSaved();
                }
            }
            finally
            {
                _ctx.IsBusy = false;
            }

            await _screens.TravelAsync(BookLocation.MainMenu);
        }

        private async UnityTask StartBrowsingAsync()
        {
            _screens.StartHistory();

            await _screens.TravelAsync(BookLocation.Entrance);
        }
    }
}
