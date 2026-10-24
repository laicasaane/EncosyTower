using System.Threading;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Codex;
using EncosyTower.Samples.UitkPages.Attachments;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.State;

namespace EncosyTower.Samples.UitkPages.App
{
    /// <summary>The shared, app-wide data that every codex module reads; it holds no behavior of its own.</summary>
    /// <remarks>
    /// <see cref="AppCodex"/> creates it first and fills the late-bound members (flows, resources, state,
    /// attachments) while it initializes. Modules keep a reference to it, so they always see the current values.
    /// </remarks>
    internal sealed class CodexContext
    {
        public CodexContext(UitkPageCodex codex, AppFlowScopes scopes)
        {
            Codex = codex;
            Scopes = scopes;
            Token = codex.InitializationToken;
        }

        public UitkPageCodex Codex { get; }

        public AppFlowScopes Scopes { get; }

        public CancellationToken Token { get; }

        public System.Random Random { get; } = new();

        public UitkPageStackSingle Screen { get; set; }

        public UitkPageStackMulti Popup { get; set; }

        public AppViewResources ViewResources;

        public Timings Timings;

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
