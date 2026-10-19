#if UNITY_UGUI

using EncosyTower.Logging;
using EncosyTower.PageFlows.Loading;
using EncosyTower.Pooling;
using EncosyTower.Settings;
using Unity.Properties;
using UnityEngine;

namespace EncosyTower.PageFlows
{
    /// <summary>The project settings that supply the defaults of <see cref="UguiPageFlowContext"/>.</summary>
    /// <remarks>A flow context uses a value from here when its own value is not overridden.</remarks>
    [Settings(SettingsUsage.RuntimeProject, "Encosy Tower/Page Flows/uGUI")]
    [GeneratePropertyBag]
    public sealed partial class UguiPageFlowSettings : Settings<UguiPageFlowSettings>
    {
        /// <summary>Whether a message that no subscriber received is added to the result of the call.</summary>
        public bool warnNoSubscriber = false;

        /// <summary>List flows reject adding a page that is already in the list.</summary>
        [Tooltip("List flows reject adding a page that is already in the list.")]
        public bool rejectDuplicatePages = false;

        /// <summary>How page assets are loaded.</summary>
        public PageLoaderStrategy loaderStrategy;

        /// <summary>How the page pools rent an instance.</summary>
        public RentingStrategy poolRentingStrategy;

        /// <summary>How the page pools take an instance back.</summary>
        public ReturningStrategy poolReturningStrategy;

        /// <summary>What the scope of a flow identifies.</summary>
        public UguiMessageScope messageScope;

        /// <summary>Which logger the flows use.</summary>
        public LogEnvironment logEnvironment;
    }
}

#endif
