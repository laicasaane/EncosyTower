using EncosyTower.PageFlows;
using EncosyTower.PageFlows.History;
using EncosyTower.PageFlows.UitkPages.Codex;
using EncosyTower.Samples.UitkPages.Attachments;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Messages;
using EncosyTower.Samples.UitkPages.Persistence;
using EncosyTower.Samples.UitkPages.State;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using ThrowHelper = EncosyTower.Samples.UitkPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UitkPages.App
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
    /// It demonstrates <see cref="UitkPageCodexInitializer{TScopes}"/>,
    /// <see cref="PageFlowScopeCollectionAttribute"/>, page messages, <see cref="PageAbortSource"/> and
    /// <see cref="PageAbortMode"/>, <see cref="PageHistory"/> with a custom navigator, and
    /// <see cref="PageHistoryLog"/>. It logs each failed result it reacts to once, with
    /// <see cref="PageFlowError.Log"/>.
    /// </para>
    /// <para>
    /// The colors and amounts come from USS custom properties of the resources layout, not from fields.
    /// </para>
    /// <para>
    /// This class only owns the lifetime: it builds the <see cref="CodexContext"/> and the modules, runs the
    /// start-up steps in order, ticks the modules each frame and disposes them. Every behavior lives in a module:
    /// <list type="bullet">
    /// <item><see cref="AppStartup"/>, <see cref="AppRoot"/>: prepare the flows and bind the panel.</item>
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
    public sealed class AppCodex : UitkPageCodexInitializer<AppFlowScopes>
    {
        /// <summary>
        /// The layout whose USS custom properties hold every text, color and amount the Book uses.
        /// </summary>
        [Tooltip("The layout whose USS custom properties hold every text, color and amount the Book uses.")]
        [SerializeField] private VisualTreeAsset _resources;

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
        /// Resolves the four flows, waits for the panel root, reads the resources and the style, builds the state and
        /// the attachments, subscribes the messages, starts a history log, prepools the screens, shows the overlay
        /// pages and the main menu, then marks the app ready. It stops early when a flow, the resources or the style
        /// is missing.
        /// </remarks>
        protected override async UnityTask OnInitializeAsync(UitkPageCodex codex, AppFlowScopes scopes)
        {
            _ctx = new CodexContext(codex, scopes);
            ComposeModules();

            var token = _ctx.Token;

            if (_startup.TryResolveFlows() == false)
            {
                return;
            }

            await UnityTask.WaitUntil(codex, static c => c.PanelRoot != null, token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            var theme = UserStorage.LoadTheme();

            _root.ApplyTheme(theme);

            _ctx.ViewResources = AppViewResources.Create(_resources);

            if (_ctx.ViewResources.IsMissing)
            {
                ThrowHelper.LogError_ResourcesMissing();
                return;
            }

            if (await _startup.ResolveStyleAsync() == false)
            {
                return;
            }

            _ctx.State = new AppState(_ctx.ViewResources, _ctx.Timings, theme);
            _ctx.State.Changed += _ctx.MarkStatusDirty;
            _ctx.State.ThemeChanged += OnThemeChanged;

            await _saves.LoadSavedGameAsync();

            _ctx.Attachments = new PageAttachments(codex, _ctx.State);
            _ctx.State.Attachments = _ctx.Attachments;

            _listener.Start();
            _router.Start();
            _root.Bind();

            _log = new PageHistoryLog { MaxEntries = _ctx.Timings.LogEntries };
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
            _root.RebindIfRootChanged();

            if (_ctx.ConsumeStatusDirty())
            {
                _status.Render();
                _root.EndBoot();
            }

            _root.TickFocus();
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
            _root.Unbind();

            _ctx.Attachments?.Dispose();
            _ctx.Attachments = null;

            if (_ctx.State != null)
            {
                _ctx.State.Changed -= _ctx.MarkStatusDirty;
                _ctx.State.ThemeChanged -= OnThemeChanged;
            }

            _screens.DisposeHistory();
            _log?.Dispose();
            _log = null;

            _startup.Dispose();

            DimensionCatalog.Reset();
            RuneCatalog.Reset();
            EncosyCustomStyleExtensions.ResetReported();
            AppViewResources.ResetReported();
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
            _root = new AppRoot(_ctx);
            _demos = new AbortDemoRunner(_ctx, _popups, _screens, _dialogs);
        }

        private void OnThemeChanged()
        {
            _root.ApplyTheme(_ctx.State.Theme);
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
