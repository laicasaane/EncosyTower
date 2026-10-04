#if UNITY_UGUI

using EncosyTower.Logging;
using EncosyTower.Pooling;
using EncosyTower.Settings;
using Unity.Properties;
using UnityEngine.Scripting.APIUpdating;

namespace EncosyTower.PageFlows.UguiPages
{
    [MovedFrom(
          true
        , sourceNamespace: "EncosyTower.PageFlows.MonoPages"
        , sourceAssembly: "EncosyTower.PageFlows.MonoPages"
        , sourceClassName: "MonoPageFlowSettings"
    )]
    [Settings(SettingsUsage.RuntimeProject, "Encosy Tower/uGUI Page Flow")]
    [GeneratePropertyBag]
    public sealed partial class UguiPageFlowSettings : Settings<UguiPageFlowSettings>
    {
        public bool warnNoSubscriber = false;
        public PageLoaderStrategy loaderStrategy;

        public RentingStrategy poolRentingStrategy;
        public ReturningStrategy poolReturningStrategy;

        public UguiMessageScope messageScope;
        public LogEnvironment logEnvironment;
    }
}

#endif
