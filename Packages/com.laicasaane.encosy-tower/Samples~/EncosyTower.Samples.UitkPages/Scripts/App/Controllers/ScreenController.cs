using EncosyTower.PageFlows;
using EncosyTower.PageFlows.History;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.Navigation;
using EncosyTower.Samples.UitkPages.Overlays;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Tasks;

namespace EncosyTower.Samples.UitkPages.App
{
    /// <summary>
    /// Moves between screens: travel requests (one runs, the newest waits), back/forward replay through the
    /// history, and the rewards for arriving at a screen.
    /// </summary>
    internal sealed class ScreenController
    {
        private readonly CodexContext _ctx;
        private readonly FlowService _flows;
        private readonly PopupController _popups;
        private readonly FeedbackPresenter _feedback;
        private readonly PersistentService _saves;
        private ScreenRequest _active;
        private ScreenRequest _queued;

        public ScreenController(
              CodexContext ctx
            , FlowService flows
            , PopupController popups
            , FeedbackPresenter feedback
            , PersistentService saves
        )
        {
            _ctx = ctx;
            _flows = flows;
            _popups = popups;
            _feedback = feedback;
            _saves = saves;
        }

        public PageHistory History { get; private set; }

        public bool IsIdle
            => _active == null && _queued == null && _ctx.IsBusy == false && _popups.IsCleaning == false;

        /// <summary>Starts a fresh history for a new browsing session.</summary>
        public void StartHistory()
        {
            DisposeHistory();

            var history = new PageHistory(new PageBackStackHistoryStrategy()) {
                MaxEntries = _ctx.Timings.HistoryEntries,
                ReplayOperation = PageAsyncOperation.Sequential,
                NavigatorFactory = flow => new ScreenHistoryNavigator(flow, History),
            };

            history.Attach(_ctx.Screen);
            History = history;
        }

        public void DisposeHistory()
        {
            History?.Dispose();
            History = null;
        }

        public bool CanTravel(BookLocation location)
        {
            if (_ctx.IsReady == false
                || _ctx.IsBusy
                || _popups.IsCleaning
                || _ctx.State.IsAvailable(location) == false
            )
            {
                return false;
            }

            if (_ctx.State.CurrentScreen == ScreenPage.MainMenu && History == null)
            {
                return false;
            }

            var pending = _queued ?? _active;

            if (pending != null)
            {
                return pending.Location != location;
            }

            return _ctx.State.CurrentLocation != location;
        }

        public void RequestTravel(BookLocation location)
        {
            if (CanTravel(location) == false)
            {
                return;
            }

            TravelAsync(location).Forget();
        }

        public async UnityTask TravelOrCloseTopPopupAsync(BookLocation location)
        {
            if (CanTravel(location))
            {
                await TravelAsync(location);
                return;
            }

            await _popups.HideTopPopupAsync(offerReopen: false);
        }

        public async UnityTask TravelAsync(BookLocation location)
        {
            var hasPopups = _popups.Count > 0 || _popups.Op != null;

            if (hasPopups && await _popups.BeginClosePopupsAsync() == false)
            {
                return;
            }

            var request = new ScreenRequest(location);

            if (_active == null)
            {
                _active = request;
            }
            else
            {
                _queued?.Source.Abort(PageAbortMode.Immediate);
                _queued = request;
            }

            _ctx.MarkStatusDirty();

            var screen = RunScreenRequestAsync(request);
            var popups = hasPopups ? _popups.FadeOutPopupsAsync() : UnityTask.CompletedTask;

            await screen;
            await popups;
        }

        public async UnityTask BackAsync()
        {
            if (_ctx.State.CurrentScreen == ScreenPage.MainMenu)
            {
                return;
            }

            if (_popups.Op != null)
            {
                _popups.AbortPopup(PageAbortMode.Reverse);
                return;
            }

            if (_popups.Count > 0)
            {
                await _popups.CloseDialogAsync(offerReopen: false);
                return;
            }

            if (IsIdle == false || History == null || History.CurrentIndex < 1)
            {
                return;
            }

            await ReplayAsync(goBack: true);
        }

        public async UnityTask ForwardAsync()
        {
            if (IsIdle == false || _popups.Count > 0 || History == null || History.CanGoForward == false)
            {
                return;
            }

            await ReplayAsync(goBack: false);
        }

        private async UnityTask RunScreenRequestAsync(ScreenRequest request)
        {
            var context = _flows.CreateContext(
                  userData: new DimensionPageData(_ctx.State, request.Location)
                , source: request.Source
                , receipt: request.Receipt
            );

            try
            {
                var result = await _flows.ShowAsync(_ctx.Scopes.Screen, AssetKeyAPI.Key(request.Location.Screen), context);

                if (result.IsApplied())
                {
                    OnScreenArrived(request.Location);
                }
            }
            finally
            {
                if (ReferenceEquals(_active, request))
                {
                    _active = _queued;
                    _queued = null;
                }
                else if (ReferenceEquals(_queued, request))
                {
                    _queued = null;
                }

                _ctx.MarkStatusDirty();
            }
        }

        private void OnScreenArrived(BookLocation location)
        {
            var state = _ctx.State;

            if (DimensionCatalog.TryGet(location, out var data))
            {
                if (data.Index < DimensionCatalog.OUTER_COUNT)
                {
                    if (state.DiscoverDimension(data.Index))
                    {
                        _feedback.ShowRuneToast(data);
                        _saves.RequestProgressSave();
                    }
                }
                else if (state.VisitCore())
                {
                    _feedback.ShowToast(ConfigTextType.ToastIncantationLearned, ToastKind.Info);
                    _saves.RequestProgressSave();
                }
            }
            else if (location.Screen == ScreenPage.Entrance && state.ConsumeCoreUnlockNotice())
            {
                _feedback.ShowToast(ConfigTextType.ToastCoreUnlocked, ToastKind.Core);
                _feedback.StartCoreBlink();
                _saves.RequestProgressSave();
            }
        }

        private async UnityTask ReplayAsync(bool goBack)
        {
            _ctx.IsBusy = true;
            _ctx.MarkStatusDirty();

            try
            {
                var result = goBack
                    ? await History.GoBackAsync(_ctx.Token)
                    : await History.GoForwardAsync(_ctx.Token);

                if (result.IsSuccess)
                {
                    OnScreenArrived(_ctx.State.CurrentLocation);
                }
            }
            finally
            {
                _ctx.IsBusy = false;
                _ctx.MarkStatusDirty();
            }
        }

        private sealed class ScreenRequest
        {
            public ScreenRequest(BookLocation location)
            {
                Location = location;
            }

            public BookLocation Location { get; }

            public PageAbortSource Source { get; } = new();

            public PageCallReceipt Receipt { get; } = new();
        }
    }
}
