using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Tasks;
using UnityEngine;
using ThrowHelper = EncosyTower.Samples.UguiPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>The abort demonstrations that the codex can run from the Inspector context menu.</summary>
    /// <remarks>
    /// Each one issues a call, aborts it, and logs the origin, the mode and the transition state of the result.
    /// </remarks>
    public enum AbortDemo
    {
        /// <summary>Cancels the Echo dialog while it opens; the flows aborts with the Reverse mode.</summary>
        CancelEnteringEcho,

        /// <summary>
        /// Aborts the Echo dialog while it closes, so it stays open; the flows aborts with the Immediate mode.
        /// </summary>
        KeepReadingDuringEchoExit,

        /// <summary>Aborts the More dialog while it opens, with a message to the flow.</summary>
        BackDuringMoreEntrance,

        /// <summary>Aborts the More dialog while it closes, with a message to the flow.</summary>
        UndoMoreClose,

        /// <summary>
        /// Travels to another dimension while the Echo dialog opens; the dialog is aborted by the flow.
        /// </summary>
        SwitchDimensionDuringEcho,

        /// <summary>
        /// Requests three travels at once. The first runs, the waiting second request is withdrawn, and the newest runs
        /// after the first.
        /// </summary>
        NewestPathWins,

        /// <summary>Aborts the incantation dialog while it opens, which uses its own transition.</summary>
        InterruptIncantation,

        /// <summary>Opens a page that does not exist; the call fails, and nothing is aborted.</summary>
        MissingAppendix,
    }

    /// <summary>Runs the <see cref="AbortDemo"/> scenarios against the live popup and screen flows.</summary>
    internal sealed class AbortDemoRunner
    {
        private readonly CodexContext _ctx;
        private readonly PopupController _popups;
        private readonly ScreenController _screens;
        private readonly DialogController _dialogs;
        private bool _isRunning;

        public AbortDemoRunner(
              CodexContext ctx
            , PopupController popups
            , ScreenController screens
            , DialogController dialogs
        )
        {
            _ctx = ctx;
            _popups = popups;
            _screens = screens;
            _dialogs = dialogs;
        }

        private Dimension PrimaryDemoId
            => DimensionCatalog.Outer[DemoIndex(ConfigAmountType.DemoPrimaryDimensionIndex)].Id;

        private Dimension SwitchTargetDemoId
            => DimensionCatalog.Outer[DemoIndex(ConfigAmountType.DemoSwitchDimensionIndex)].Id;

        private Dimension NewestFirstDemoId
            => DimensionCatalog.Outer[DemoIndex(ConfigAmountType.DemoNewestFirstDimensionIndex)].Id;

        private Dimension NewestSecondDemoId
            => DimensionCatalog.Outer[DemoIndex(ConfigAmountType.DemoNewestSecondDimensionIndex)].Id;

        /// <summary>Runs one abort demonstration; it needs a started game and runs one at a time.</summary>
        public async UnityTask RunAsync(AbortDemo demo)
        {
            if (_ctx.IsReady == false || _isRunning || _ctx.State.CurrentScreen == ScreenPage.MainMenu)
            {
                ThrowHelper.LogInfo_Demo(demo.ToString(), "Start a game first, then run one demo at a time.");
                return;
            }

            _isRunning = true;

            try
            {
                switch (demo)
                {
                    case AbortDemo.CancelEnteringEcho: await CancelEnteringEchoAsync(); break;
                    case AbortDemo.KeepReadingDuringEchoExit: await KeepReadingDuringEchoExitAsync(); break;
                    case AbortDemo.BackDuringMoreEntrance: await BackDuringMoreEntranceAsync(); break;
                    case AbortDemo.UndoMoreClose: await UndoMoreCloseAsync(); break;
                    case AbortDemo.SwitchDimensionDuringEcho: await SwitchDimensionDuringEchoAsync(); break;
                    case AbortDemo.NewestPathWins: await NewestPathWinsAsync(); break;
                    case AbortDemo.InterruptIncantation: await InterruptIncantationAsync(); break;
                    case AbortDemo.MissingAppendix: await MissingAppendixAsync(); break;
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        private int DemoIndex(ConfigAmountType name)
            => Mathf.RoundToInt(_ctx.Config.Amount(name));

        private int DemoDelayMs(ConfigAmountType name)
            => Mathf.RoundToInt(_ctx.Config.Amount(name) * 1000f);

        private async UnityTask CancelEnteringEchoAsync()
        {
            await PrepareDimensionAsync(BookLocation.Of(PrimaryDemoId));

            var show = _popups.ShowPopupAsync(DialogPage.Echo, PrimaryData());

            await UnityTask.Delay(DemoDelayMs(ConfigAmountType.DemoAbortDelaySeconds));
            _popups.CancelOpening();

            LogOutcome(AbortDemo.CancelEnteringEcho, await show);
        }

        private async UnityTask KeepReadingDuringEchoExitAsync()
        {
            await PrepareDimensionAsync(BookLocation.Of(PrimaryDemoId));
            await _popups.ShowPopupAsync(DialogPage.Echo, PrimaryData());

            var hide = _popups.HideTopPopupAsync(offerReopen: true);

            await UnityTask.Delay(DemoDelayMs(ConfigAmountType.DemoAbortDelaySeconds));
            _popups.KeepReading();

            LogOutcome(AbortDemo.KeepReadingDuringEchoExit, await hide);
            await _popups.ClosePopupsAsync();
        }

        private async UnityTask BackDuringMoreEntranceAsync()
        {
            await PrepareDimensionAsync(BookLocation.Of(PrimaryDemoId));

            var show = _popups.ShowPopupAsync(DialogPage.More, PrimaryData());

            await UnityTask.Delay(DemoDelayMs(ConfigAmountType.DemoQuickAbortDelaySeconds));
            _popups.AbortPopup(PageAbortMode.Reverse);

            LogOutcome(AbortDemo.BackDuringMoreEntrance, await show);
        }

        private async UnityTask UndoMoreCloseAsync()
        {
            await PrepareDimensionAsync(BookLocation.Of(PrimaryDemoId));
            await _popups.ShowPopupAsync(DialogPage.More, PrimaryData());

            var hide = _popups.HideTopPopupAsync(offerReopen: false);

            await UnityTask.Delay(DemoDelayMs(ConfigAmountType.DemoQuickAbortDelaySeconds));
            _popups.AbortPopup(PageAbortMode.Reverse);

            LogOutcome(AbortDemo.UndoMoreClose, await hide);
            await _popups.ClosePopupsAsync();
        }

        private async UnityTask SwitchDimensionDuringEchoAsync()
        {
            await PrepareDimensionAsync(BookLocation.Of(PrimaryDemoId));

            var show = _popups.ShowPopupAsync(DialogPage.Echo, PrimaryData());

            await UnityTask.Delay(DemoDelayMs(ConfigAmountType.DemoAbortDelaySeconds));
            await _screens.TravelAsync(BookLocation.Of(SwitchTargetDemoId));

            LogOutcome(AbortDemo.SwitchDimensionDuringEcho, await show);
        }

        private async UnityTask NewestPathWinsAsync()
        {
            await PrepareDimensionAsync(BookLocation.Entrance);

            _screens.RequestTravel(BookLocation.Of(NewestFirstDemoId));
            _screens.RequestTravel(BookLocation.Of(NewestSecondDemoId));
            _screens.RequestTravel(BookLocation.Entrance);

            await UnityTask.WaitUntil(_screens, static screens => screens.IsIdle, _ctx.Token);

            ThrowHelper.LogInfo_Demo(
                  AbortDemo.NewestPathWins.ToString()
                , $"Settled on '{_ctx.State.CurrentLocation}'; the waiting request was withdrawn and awarded nothing."
            );
        }

        private async UnityTask InterruptIncantationAsync()
        {
            if (_ctx.State.CoreUnlocked == false)
            {
                ThrowHelper.LogInfo_Demo(AbortDemo.InterruptIncantation.ToString(), "Gather six runes first.");
                return;
            }

            await PrepareDimensionAsync(BookLocation.Of(Dimension.Core));

            if (_ctx.State.IncantationCast)
            {
                ThrowHelper.LogInfo_Demo(AbortDemo.InterruptIncantation.ToString(), "The spell is already cast.");
                return;
            }

            var show = _popups.ShowPopupAsync(
                  DialogPage.TowerIncantation
                , new DimensionPageData(_ctx.State, BookLocation.MainMenu)
            );

            await UnityTask.Delay(DemoDelayMs(ConfigAmountType.DemoIncantationAbortDelaySeconds));
            _popups.CancelOpening();

            LogOutcome(AbortDemo.InterruptIncantation, await show);
        }

        private async UnityTask MissingAppendixAsync()
        {
            await PrepareDimensionAsync(BookLocation.Of(PrimaryDemoId));
            await _popups.ShowPopupAsync(DialogPage.Echo, PrimaryData());
            await _dialogs.OpenAppendixAsync();

            ThrowHelper.LogInfo_Demo(
                  AbortDemo.MissingAppendix.ToString()
                , "The call failed, not aborted. The echo stays open and the player sees one toast."
            );
        }

        private async UnityTask PrepareDimensionAsync(BookLocation location)
        {
            await _popups.ClosePopupsAsync();

            if (_ctx.State.CurrentLocation != location)
            {
                _screens.RequestTravel(location);
            }

            await UnityTask.WaitUntil(
                  (screens: _screens, State: _ctx.State, Location: location)
                , static s => s.screens.IsIdle && s.State.CurrentLocation == s.Location
                , _ctx.Token
            );
        }

        private DimensionPageData PrimaryData()
            => new(_ctx.State, BookLocation.Of(PrimaryDemoId));

        private static void LogOutcome(AbortDemo demo, in Success<PageFlowError> result)
        {
            if (result.TryGetFailure(out var error) == false)
            {
                ThrowHelper.LogInfo_Demo(demo.ToString(), "The call completed; nothing was aborted.");
                return;
            }

            if (error.TryGetAbort(out var origin, out var mode))
            {
                var kind = error.IsWithdrawn ? "withdrawn before it began" : "aborted";

                ThrowHelper.LogInfo_Demo(
                      demo.ToString()
                    , $"The call was {kind}: origin {OriginName(origin)}, mode {mode}, "
                        + $"transition {error.TransitionState}."
                );

                return;
            }

            ThrowHelper.LogInfo_Demo(demo.ToString(), $"The call failed: {error.ToMessage()}");
        }

        private static string OriginName(in PageAbortOrigin origin)
        {
            if (origin.TryGetValue(out PageAbortOrigin.Caller _))
            {
                return "flows";
            }

            if (origin.TryGetValue(out PageAbortOrigin.Flow _))
            {
                return "flow";
            }

            return "page";
        }
    }
}
