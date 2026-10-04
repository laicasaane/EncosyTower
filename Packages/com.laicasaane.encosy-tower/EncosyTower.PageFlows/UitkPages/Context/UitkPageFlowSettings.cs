using EncosyTower.Logging;
using EncosyTower.Settings;
using Unity.Properties;

namespace EncosyTower.PageFlows.UitkPages
{
    [Settings(SettingsUsage.RuntimeProject, "Encosy Tower/UI Toolkit Page Flow")]
    [GeneratePropertyBag]
    public sealed partial class UitkPageFlowSettings : Settings<UitkPageFlowSettings>
    {
        public bool warnNoSubscriber = false;
        public PageLoaderStrategy loaderStrategy;
        public LogEnvironment logEnvironment;
        public bool forceUIDocument;
    }
}
