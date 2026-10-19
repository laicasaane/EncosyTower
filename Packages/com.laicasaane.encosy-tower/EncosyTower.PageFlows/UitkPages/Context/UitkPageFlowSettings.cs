using EncosyTower.Logging;
using EncosyTower.PageFlows.Loading;
using EncosyTower.Settings;
using Unity.Properties;
using UnityEngine;

namespace EncosyTower.PageFlows
{
    /// <summary>The project settings that supply the defaults of <see cref="UitkPageFlowContext"/>.</summary>
    /// <remarks>A flow context uses a value from here when its own value is not overridden.</remarks>
    [Settings(SettingsUsage.RuntimeProject, "Encosy Tower/Page Flows/UI Toolkit")]
    [GeneratePropertyBag]
    public sealed partial class UitkPageFlowSettings : Settings<UitkPageFlowSettings>
    {
        /// <summary>Whether a message that no subscriber received is added to the result of the call.</summary>
        public bool warnNoSubscriber = false;

        /// <summary>List flows reject adding a page that is already in the list.</summary>
        [Tooltip("List flows reject adding a page that is already in the list.")]
        public bool rejectDuplicatePages = false;

        /// <summary>How page assets are loaded.</summary>
        public PageLoaderStrategy loaderStrategy;

        /// <summary>Which logger the flows use.</summary>
        public LogEnvironment logEnvironment;

        /// <summary>Whether a codex draws on a UIDocument even when a PanelRenderer is available.</summary>
        public bool forceUIDocument;
    }
}
