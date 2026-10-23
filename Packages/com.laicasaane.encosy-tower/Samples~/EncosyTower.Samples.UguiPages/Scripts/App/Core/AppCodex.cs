using EncosyTower.PageFlows;
using EncosyTower.PageFlows.History;
using EncosyTower.PageFlows.UguiPages.Codex;
using EncosyTower.Samples.UguiPages.Attachments;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Messages;
using EncosyTower.Samples.UguiPages.Persistence;
using EncosyTower.Samples.UguiPages.State;
using EncosyTower.Samples.UguiPages.Views;
using EncosyTower.Tasks;
using UnityEngine;
using ThrowHelper = EncosyTower.Samples.UguiPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>The Tower Book app: it initializes the codex and wires the modules that drive every flow.</summary>
    /// <remarks>
    /// <para>
    /// The codex has four flows: Screen (a single-page stack), Popup (a multi-page stack), FloatingContext and Toast
    /// (single page stacks). Each screen also gets a navigation bar, tabs and cards, attached as pages in flows of
    /// their own. Pages ask for actions with <see cref="AppActionMessage"/>; the modules call the flows with
    /// <see cref="ShowPageMessage"/>, <see cref="HideActivePageMessage"/> and <see cref="HideAllPagesMessage"/>, and
    /// read each result from a <see cref="PageCallReceipt"/>.
    /// </para>
    /// <para>
    /// It demonstrates <see cref="UguiPageCodexInitializer{TScopes}"/>,
    /// <see cref="PageFlowScopeCollectionAttribute"/>, page messages, <see cref="PageAbortSource"/> and
    /// <see cref="PageAbortMode"/>, <see cref="PageHistory"/> with a custom navigator, and
    /// <see cref="PageHistoryLog"/>. It logs each failed result it reacts to once, with
    /// <see cref="PageFlowError.Log"/>.
    /// </para>
    /// <para>
    /// The colors and amounts come from the <see cref="AppConfig"/> asset.
    /// </para>
    /// <para>
    /// This class only owns the lifetime: it builds the <see cref="CodexContext"/> and the modules, runs the
    /// start-up steps in order, ticks the modules each frame and disposes them. Every behavior lives in a module:
    /// <list type="bullet">
    /// <item><see cref="AppStartup"/>, <see cref="AppRoot"/>: prepare the flows and bind the input.</item>
    /// <item><see cref="ActionRouter"/>: receives page actions and hands each to the module that owns it.</item>
    /// <item><see cref="AppController"/>, <see cref="IncantationController"/>: the play session and the core spell.</item>
    /// <item><see cref="ScreenController"/>, <see cref="DimensionController"/>: screens, history, tabs and cards.</item>
    /// <item><see cref="PopupController"/>, <see cref="DialogController"/>: the dialogs.</item>
    /// <item><see cref="FlowListener"/>: failures, aborts and navigations reported by the flows.</item>
    /// <item><see cref="FlowService"/>, <see cref="PersistentService"/>, <see cref="FeedbackPresenter"/>,
    /// <see cref="StatusPresenter"/>: shared services. <see cref="AbortDemoRunner"/>: the abort demos.</item>
    /// </list>
    /// </para>
    /// </remarks>
    public sealed class AppCodex : UguiPageCodexInitializer<AppFlowScopes>
    {
        /// <summary>The configuration asset with every text, color, amount, dimension and rune the Book uses.</summary>
        [Tooltip("The configuration asset with every text, color, amount, dimension and rune the Book uses.")]
        [SerializeField] private AppConfig _config;

        private CodexContext _ctx;
        private FlowService _flows;
        private FeedbackPresenter _feedback;
        private PersistentService _saves;
        private PopupController _popups;
        private ScreenController _screens;
        private DimensionController _dimensions;
        private DialogController _dialogs;
        private IncantationController _incantation;
        private AppController _book;
        private StatusPresenter _status;
        private FlowListener _listener;
        private ActionRouter _router;
        private AppStartup _startup;
        private AppRoot _root;
        private AbortDemoRunner _demos;
        private PageHistoryLog _log;

        /// <inheritdoc/>
        /// <remarks>
        /// Resolves the configuration and the four flows, builds the state and the attachments, subscribes the
        /// messages, starts a history log, prepools the screens, shows the overlay pages and the main menu, then
        /// marks the app ready. It stops early when the configuration is missing or invalid, or a flow is missing.
        /// </remarks>
        protected override async UnityTask OnInitializeAsync(UguiPageCodex codex, AppFlowScopes scopes)
        {
            _ctx = new CodexContext(codex, scopes, _config);
            ComposeModules();

            if (_startup.ResolveConfig() == false)
            {
                return;
            }

            _config.ActiveTheme = UserStorage.LoadTheme();

            _root.ApplyCameraBackground();

            if (_startup.TryResolveFlows() == false)
            {
                return;
            }

            ButtonStates.Configure(_config);

            _root.Bind();

            _ctx.State = new AppState(_config);
            _ctx.State.Changed += _ctx.MarkStatusDirty;
            _ctx.State.ThemeChanged += OnThemeChanged;

            _ctx.Attachments = new PageAttachments(codex, _ctx.State);
            _ctx.State.Attachments = _ctx.Attachments;

            await _saves.LoadSavedGameAsync();

            _listener.Start();
            _router.Start();

            var logEntries = Mathf.RoundToInt(_config.Amount(ConfigAmountType.HistoryLogEntries));

            _log = new PageHistoryLog { MaxEntries = logEntries };
            _log.Attach(_ctx.Screen);

            await _startup.PrepoolPagesAsync();
            await _startup.ShowOverlayPagesAsync();
            await _startup.ShowStartScreenAsync();

            _ctx.IsReady = true;
            _ctx.MarkStatusDirty();
        }

        private void LateUpdate()
        {
            if (_ctx is not { IsReady: true, State: not null })
            {
                return;
            }

            var now = Time.unscaledTime;

            _feedback.Tick(now);
            _screens.Tick();
            _popups.Tick();
            _popups.ExpireReopen(now);

            if (_ctx.ConsumeStatusDirty())
            {
                _status.Render();
            }

            _root.TickFocus();
        }

        private void Update()
        {
            if (_ctx is not { IsReady: true, State: not null })
            {
                return;
            }

            _root.PollInput();
        }

        private void OnDestroy()
        {
            if (_ctx == null)
            {
                return;
            }

            _ctx.IsReady = false;

            _router.Dispose();
            _listener.Dispose();

            if (_ctx.State != null)
            {
                _ctx.State.Changed -= _ctx.MarkStatusDirty;
                _ctx.State.ThemeChanged -= OnThemeChanged;
            }

            _screens.DisposeHistory();
            _log?.Dispose();
            _log = null;

            DimensionCatalog.Reset();
            RuneCatalog.Reset();
        }

        /// <summary>The module graph, built from the bottom up: each module only uses the ones created above it.</summary>
        private void ComposeModules()
        {
            _flows = new FlowService(_ctx);
            _feedback = new FeedbackPresenter(_ctx);
            _saves = new PersistentService(_ctx, _feedback);
            _popups = new PopupController(_ctx, _flows, _feedback);
            _screens = new ScreenController(_ctx, _flows, _popups, _feedback, _saves);
            _dimensions = new DimensionController(_ctx, _flows, _screens, _popups);
            _dialogs = new DialogController(_ctx, _flows, _popups, _screens);
            _incantation = new IncantationController(_ctx, _popups, _screens, _saves);
            _book = new AppController(_ctx, _screens, _saves, _feedback);
            _status = new StatusPresenter(_ctx, _screens, _popups);
            _listener = new FlowListener(_ctx, _popups, _dialogs, _feedback);
            _router = new ActionRouter(_ctx, _book, _screens, _dimensions, _dialogs, _popups, _incantation);
            _startup = new AppStartup(_ctx, _flows);
            _root = new AppRoot(_ctx, _screens, this);
            _demos = new AbortDemoRunner(_ctx, _popups, _screens, _dialogs);
        }

        private void OnThemeChanged()
        {
            _root.ApplyTheme();
            _ctx.MarkStatusDirty();
            _saves.RequestSettingsSave();
        }

        /// <summary>Runs one abort demonstration.</summary>
        /// <param name="demo">The demonstration to run.</param>
        /// <remarks>It needs a started game, and runs one demonstration at a time.</remarks>
        public async UnityTask RunAbortDemoAsync(AbortDemo demo)
        {
            if (_demos == null)
            {
                ThrowHelper.LogInfo_Demo(demo.ToString(), "Start a game first, then run one demo at a time.");
                return;
            }

            await _demos.RunAsync(demo);
        }

        [ContextMenu("Abort demo/Cancel an entering echo")]
        private void DemoCancelEnteringEcho() => RunAbortDemoAsync(AbortDemo.CancelEnteringEcho).Forget();

        [ContextMenu("Abort demo/Keep reading while the echo closes")]
        private void DemoKeepReadingDuringEchoExit() => RunAbortDemoAsync(AbortDemo.KeepReadingDuringEchoExit).Forget();

        [ContextMenu("Abort demo/Back while More opens")]
        private void DemoBackDuringMoreEntrance() => RunAbortDemoAsync(AbortDemo.BackDuringMoreEntrance).Forget();

        [ContextMenu("Abort demo/Undo closing More")]
        private void DemoUndoMoreClose() => RunAbortDemoAsync(AbortDemo.UndoMoreClose).Forget();

        [ContextMenu("Abort demo/Switch dimension while the echo opens")]
        private void DemoSwitchDimensionDuringEcho() => RunAbortDemoAsync(AbortDemo.SwitchDimensionDuringEcho).Forget();

        [ContextMenu("Abort demo/Newest path wins")]
        private void DemoNewestPathWins() => RunAbortDemoAsync(AbortDemo.NewestPathWins).Forget();

        [ContextMenu("Abort demo/Interrupt the incantation")]
        private void DemoInterruptIncantation() => RunAbortDemoAsync(AbortDemo.InterruptIncantation).Forget();

        [ContextMenu("Abort demo/Read the missing appendix")]
        private void DemoMissingAppendix() => RunAbortDemoAsync(AbortDemo.MissingAppendix).Forget();
    }
}
