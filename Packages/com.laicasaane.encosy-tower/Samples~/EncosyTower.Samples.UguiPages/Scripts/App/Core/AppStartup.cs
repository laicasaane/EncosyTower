using System;
using EncosyTower.AssetKeys;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.PageFlows.UguiPages.Codex;
using EncosyTower.PubSub;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using ThrowHelper = EncosyTower.Samples.UguiPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>The steps that prepare the app before the first frame: config, flows, prepooling and first pages.</summary>
    /// <remarks>
    /// <see cref="AppCodex"/> decides the order of the steps; this type only knows how to run each of them.
    /// </remarks>
    internal sealed class AppStartup
    {
        private readonly CodexContext _ctx;
        private readonly FlowService _flows;

        public AppStartup(CodexContext ctx, FlowService flows)
        {
            _ctx = ctx;
            _flows = flows;
        }

        /// <summary>Checks the configuration and builds the catalogs; returns false (and logs) when it is unusable.</summary>
        public bool ResolveConfig()
        {
            var config = _ctx.Config;

            if (config.IsInvalid())
            {
                ThrowHelper.LogError_ConfigMissing();
                return false;
            }

            if (DimensionCatalog.IsValid(config) == false || RuneCatalog.IsValid(config) == false)
            {
                ThrowHelper.LogError_ConfigInvalid();
                return false;
            }

            DimensionCatalog.Initialize(config);
            RuneCatalog.Initialize(config);
            return true;
        }

        /// <summary>Finds the four flows the app drives; returns false (and logs) when one is missing.</summary>
        public bool TryResolveFlows()
        {
            var codex = _ctx.Codex;

            _ctx.Screen = ResolveFlow<UguiPageStackSingle>(codex, AppFlowScopes.Identifiers.Screen);
            _ctx.Popup = ResolveFlow<UguiPageStackMulti>(codex, AppFlowScopes.Identifiers.Popup);

            var floatingContext = ResolveFlow<UguiPageStackSingle>(codex, AppFlowScopes.Identifiers.FloatingContext);
            var toast = ResolveFlow<UguiPageStackSingle>(codex, AppFlowScopes.Identifiers.Toast);

            return _ctx.Screen != null && _ctx.Popup != null && floatingContext != null && toast != null;
        }

        public async UnityTask PrepoolPagesAsync()
        {
            var context = _flows.CreatePublishingContext();

            await PrepoolAsync(PageKeys.MainMenuScreen, 1, context);
            await PrepoolAsync(PageKeys.EntranceScreen, 1, context);
            await PrepoolAsync(PageKeys.CoreScreen, 1, context);
            await PrepoolAsync(PageKeys.DimensionScreen, 2, context);
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

        private static T ResolveFlow<T>(UguiPageCodex codex, string identifier)
            where T : UguiPageFlow
        {
            var count = codex.FlowCount;

            for (var i = 0; i < count; i++)
            {
                if (codex.GetFlowAt(i).TryGetValue(out var flow)
                    && flow is T typed
                    && string.Equals(flow.Identifier, identifier, StringComparison.Ordinal)
                )
                {
                    return typed;
                }
            }

            ThrowHelper.LogError_FlowMissing(identifier);
            return null;
        }
    }
}
