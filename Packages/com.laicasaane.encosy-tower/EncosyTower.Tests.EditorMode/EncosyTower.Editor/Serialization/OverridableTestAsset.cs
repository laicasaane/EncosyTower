using System;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Serialization;
using UnityEngine;

namespace EncosyTower.Tests.Editor.Serialization
{
    public enum OverridableTestEnum
    {
        First,
        [InspectorName("Second Choice")] Second,
        ThirdValue,
    }

    [Flags]
    public enum OverridableTestFlags
    {
        None = 0,
        A = 1,
        B = 2,
    }

    public static class OverridableTestDefaults
    {
        public static OverridableTestEnum EnumValue = OverridableTestEnum.ThirdValue;

        public static bool BoolValue => true;
    }

    public sealed class OverridableTestAsset : ScriptableObject
    {
        [OverridableDefault(typeof(OverridableTestDefaults), nameof(OverridableTestDefaults.EnumValue))]
        public Overridable<OverridableTestEnum> enumWithDefault;

        public Overridable<OverridableTestEnum> enumWithoutDefault;

        [OverridableDefault(
              typeof(OverridableTestDefaults)
            , nameof(OverridableTestDefaults.BoolValue)
            , Label = "Global"
        )]
        public Overridable<bool> boolWithDefault;

        public Overridable<int> intValue;

        public Overridable<OverridableTestFlags> flagsValue;
    }

    public sealed class OverridableSettingsTestAsset : ScriptableObject
    {
        [OverridableDefault(typeof(UitkPageFlowSettings), nameof(UitkPageFlowSettings.warnNoSubscriber))]
        public Overridable<bool> warnNoSubscriber;
    }
}
