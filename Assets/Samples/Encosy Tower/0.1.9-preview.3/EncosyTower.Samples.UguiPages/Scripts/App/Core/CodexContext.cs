using System.Threading;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.PageFlows.UguiPages.Codex;
using EncosyTower.Samples.UguiPages.Attachments;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.State;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>The shared, app-wide data that every codex module reads; it holds no behavior of its own.</summary>
    /// <remarks>
    /// <see cref="AppCodex"/> creates it first and fills the late-bound members (flows, state, attachments)
    /// while it initializes. Modules keep a reference to it, so they always see the current values.
    /// </remarks>
    internal sealed class CodexContext
    {
        public CodexContext(UguiPageCodex codex, AppFlowScopes scopes, AppConfig config)
        {
            Codex = codex;
            Scopes = scopes;
            Config = config;
            Token = codex.InitializationToken;
        }

        public UguiPageCodex Codex { get; }

        public AppFlowScopes Scopes { get; }

        public AppConfig Config { get; }

        public CancellationToken Token { get; }

        public System.Random Random { get; } = new();

        public UguiPageStackSingle Screen { get; set; }

        public UguiPageStackMulti Popup { get; set; }

        public AppState State { get; set; }

        public PageAttachments Attachments { get; set; }

        /// <summary>True once start-up has finished; actions are ignored before.</summary>
        public bool IsReady { get; set; }

        /// <summary>True while a new/continue/exit/replay operation owns the screen flow.</summary>
        public bool IsBusy { get; set; }

        public bool IsStatusDirty { get; private set; }

        public void MarkStatusDirty()
        {
            IsStatusDirty = true;
        }

        public bool ConsumeStatusDirty()
        {
            var dirty = IsStatusDirty;
            IsStatusDirty = false;
            return dirty;
        }
    }
}
