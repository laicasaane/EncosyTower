#if UNITY_EDITOR

using EncosyTower.Serialization;
using EncosyTower.Settings;
using UnityEngine;

namespace EncosyTower.DevTools.Showcases.Overridables
{
    /// <summary>
    /// Project-wide defaults for the <see cref="OverridableShowcaseAsset"/> fields that read from Project Settings.
    /// </summary>
    [Settings(SettingsUsage.EditorProject, "Dev Tools/Showcases/Overridables")]
    public sealed class OverridableShowcaseSettings : Settings<OverridableShowcaseSettings>
    {
        public bool logWarnings;
        public int maxRetries = 5;
        public ShowcaseQuality quality = ShowcaseQuality.Medium;
        public Material material;
        public Overridable<int>[] limitSteps = { new(value: 2, isOverridden: true), default };
    }
}

#endif
