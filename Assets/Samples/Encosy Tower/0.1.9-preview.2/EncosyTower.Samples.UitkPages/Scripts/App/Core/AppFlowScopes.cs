using EncosyTower.PageFlows;
using EncosyTower.PageFlows.PubSub;

namespace EncosyTower.Samples.UitkPages.App
{
    /// <summary>The scopes of the four flows the Book codex creates.</summary>
    /// <remarks>
    /// Demonstrates <see cref="PageFlowScopeCollectionAttribute"/>; each property matches the identifier of a flow
    /// definition.
    /// </remarks>
    [PageFlowScopeCollection]
    public partial struct AppFlowScopes
    {
        /// <summary>The Screen flow: a single-page stack that shows one screen at a time.</summary>
        public PageFlowScope Screen { get; private set; }

        /// <summary>The Popup flow: a multi-page stack of dialogs and popups above the screen.</summary>
        public PageFlowScope Popup { get; private set; }

        /// <summary>The FloatingContext flow: a single-page stack for floating dialog actions.</summary>
        public PageFlowScope FloatingContext { get; private set; }

        /// <summary>The Toast flow: a single-page stack that holds the toast overlay.</summary>
        public PageFlowScope Toast { get; private set; }
    }
}
