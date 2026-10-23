using EncosyTower.PubSub;
using EncosyTower.AssetKeys;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Codex;
using EncosyTower.Samples.UitkPages.Attachments;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Tasks;
using UnityEngine.UIElements;
using ThrowHelper = EncosyTower.Samples.UitkPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UitkPages.App
{
    /// <summary>The steps that prepare the app before the first frame: flows, style, prepooling and first pages.</summary>
    /// <remarks>
    /// <see cref="AppCodex"/> decides the order of the steps; this type only knows how to run each of them.
    /// </remarks>
    internal sealed class AppStartup
    {
        private readonly CodexContext _ctx;
        private readonly FlowService _flows;
        private VisualElement _styleTarget;
        private bool _isStyleResolved;
        private bool _isConfigInvalid;

        public AppStartup(CodexContext ctx, FlowService flows)
        {
            _ctx = ctx;
            _flows = flows;
        }

        /// <summary>Finds the four flows the app drives; returns false (and logs) when one is missing.</summary>
        public bool TryResolveFlows()
        {
            var codex = _ctx.Codex;

            _ctx.Screen = ResolveFlow<UitkPageStackSingle>(codex, AppFlowScopes.Identifiers.Screen);
            _ctx.Popup = ResolveFlow<UitkPageStackMulti>(codex, AppFlowScopes.Identifiers.Popup);

            var floatingContext = ResolveFlow<UitkPageStackSingle>(codex, AppFlowScopes.Identifiers.FloatingContext);
            var toast = ResolveFlow<UitkPageStackSingle>(codex, AppFlowScopes.Identifiers.Toast);

            return _ctx.Screen != null && _ctx.Popup != null && floatingContext != null && toast != null;
        }

        /// <summary>Waits for the app element's custom style, then reads the timings and builds the catalogs.</summary>
        /// <returns>True when the style was read and the configuration is valid.</returns>
        public async UnityTask<bool> ResolveStyleAsync()
        {
            _isStyleResolved = false;
            _isConfigInvalid = false;
            _styleTarget = _ctx.Codex.PanelRoot.Q(AppRoot.APP_ROOT);

            if (_styleTarget == null)
            {
                ThrowHelper.LogError_StyleUnresolved();
                return false;
            }

            _styleTarget.RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);

            await UnityTask.WaitUntil(this, static s => s._isStyleResolved || s._isConfigInvalid, _ctx.Token);

            return _ctx.Token.IsCancellationRequested == false && _isConfigInvalid == false && _isStyleResolved;
        }

        public void Dispose()
        {
            _styleTarget?.UnregisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
            _styleTarget = null;
        }

        public async UnityTask PrepoolPagesAsync()
        {
            var context = _flows.CreatePublishingContext();

            await PrepoolAsync(PageKeys.MainMenuScreen, 1, context);
            await PrepoolAsync(PageKeys.EntranceScreen, 1, context);
            await PrepoolAsync(PageKeys.CoreScreen, 1, context);
            await PrepoolAsync(PageKeys.DimensionScreen, PageAttachments.MAX_SETS, context);
        }

        public async UnityTask ShowOverlayPagesAsync()
        {
            var userData = new DimensionPageData(_ctx.State, BookLocation.MainMenu);

            var floatingContext = _flows.CreateContext(userData: userData, show: PageTransitionOptions.ZeroDuration);
            await _flows.ShowAsync(_ctx.Scopes.FloatingContext, PageKeys.FloatingContext, floatingContext);

            var toast = _flows.CreateContext(userData: userData, show: PageTransitionOptions.ZeroDuration);
            await _flows.ShowAsync(_ctx.Scopes.Toast, PageKeys.Toast, toast);
        }

        public async UnityTask ShowStartScreenAsync()
        {
            var context = _flows.CreateContext(
                  userData: new DimensionPageData(_ctx.State, BookLocation.MainMenu)
                , show: PageTransitionOptions.ZeroDuration
            );

            await _flows.ShowAsync(_ctx.Scopes.Screen, PageKeys.MainMenuScreen, context);
        }

        private async UnityTask PrepoolAsync(AssetKey key, int count, PublishingContext context)
        {
            var publisher = _ctx.Codex.FlowContext.Publisher.Scope(_ctx.Scopes.Screen);
            var published = await PrepoolPageMessage.Async.Publish(in publisher, new PrepoolPageMessage(key, count), context);

            if (published.TryGetFailure(out var error) && error.IsSubscriberNotFound == false)
            {
                error.Log(_ctx.Codex.FlowContext.Logger);
            }
        }

        private void OnStyleResolved(CustomStyleResolvedEvent evt)
        {
            _styleTarget.UnregisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);

            if (TryApplyStyle(evt.customStyle))
            {
                _isStyleResolved = true;
            }
            else
            {
                _isConfigInvalid = true;
            }
        }

        private bool TryApplyStyle(ICustomStyle style)
        {
            _ctx.Timings.Read(style);

            if (DimensionCatalog.Initialize(in _ctx.ViewResources, style) == false)
            {
                return false;
            }

            return RuneCatalog.Initialize(in _ctx.ViewResources);
        }

        private static T ResolveFlow<T>(UitkPageCodex codex, string identifier)
            where T : UitkPageFlow
        {
            if (codex.GetFlow(identifier).TryGetValue(out var flow) && flow is T typed)
            {
                return typed;
            }

            ThrowHelper.LogError_FlowMissing(identifier);
            return null;
        }
    }
}
