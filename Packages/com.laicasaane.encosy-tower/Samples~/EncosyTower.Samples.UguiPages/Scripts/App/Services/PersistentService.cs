using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Overlays;
using EncosyTower.Samples.UguiPages.Persistence;
using EncosyTower.Tasks;
using ThrowHelper = EncosyTower.Samples.UguiPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>Persists the progress and the settings, coalescing requests made while a write is running.</summary>
    internal sealed class PersistentService
    {
        private readonly CodexContext _ctx;
        private readonly FeedbackPresenter _feedback;
        private bool _isProgressRunning;
        private bool _isProgressDirty;
        private bool _isSettingsRunning;
        private bool _isSettingsDirty;

        public PersistentService(CodexContext ctx, FeedbackPresenter feedback)
        {
            _ctx = ctx;
            _feedback = feedback;
        }

        /// <summary>True when the stored progress exists but cannot be read, so it must not be overwritten.</summary>
        public bool IsBlocked { get; private set; }

        public async UnityTask LoadSavedGameAsync()
        {
            var result = await UserStorage.LoadProgressAsync(_ctx.Token);

            if (result.TryGetError(out var error))
            {
                IsBlocked = error != BookLoadError.Missing;
                _ctx.State.SetSavedGame(false);
                return;
            }

            _ctx.State.SetSavedGame(true);
        }

        public void RequestProgressSave()
        {
            if (IsBlocked)
            {
                ThrowHelper.LogError_SaveUnavailable();
                return;
            }

            _isProgressDirty = true;

            if (_isProgressRunning == false)
            {
                FlushProgressAsync().Forget();
            }
        }

        public void RequestSettingsSave()
        {
            _isSettingsDirty = true;

            if (_isSettingsRunning == false)
            {
                FlushSettingsAsync().Forget();
            }
        }

        private async UnityTask FlushProgressAsync()
        {
            _isProgressRunning = true;

            try
            {
                while (_isProgressDirty && _ctx.Token.IsCancellationRequested == false)
                {
                    _isProgressDirty = false;

                    if (await UserStorage.SaveProgressAsync(_ctx.State.CreateProgress(), _ctx.Token))
                    {
                        _ctx.State.SetProgressSaved();
                        _ctx.State.SetSavedGame(true);
                        continue;
                    }

                    ThrowHelper.LogWarning_ProgressWriteFailed();
                    _feedback.ShowToast(ConfigTextType.ToastSaveFailed, ToastKind.Failure);
                    break;
                }
            }
            finally
            {
                _isProgressRunning = false;
            }
        }

        private async UnityTask FlushSettingsAsync()
        {
            _isSettingsRunning = true;

            try
            {
                while (_isSettingsDirty && _ctx.Token.IsCancellationRequested == false)
                {
                    _isSettingsDirty = false;

                    if (await UserStorage.SaveThemeAsync(_ctx.State.Theme, _ctx.Token) == false)
                    {
                        break;
                    }
                }
            }
            finally
            {
                _isSettingsRunning = false;
            }
        }
    }
}
