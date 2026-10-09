#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Editor;
using EncosyTower.Serialization;
using UnityEngine;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class OverridableChoiceTestAsset : ScriptableObject
    {
        [OverridableDefault(typeof(TestDefaults), nameof(TestDefaults.SpawnWaves))]
        public Overridable<List<int>> spawnWaves = new(value: new(), isOverridden: false);

        [OverridableDefault(typeof(OverridableTestSettings), nameof(OverridableTestSettings.limitSteps))]
        public Overridable<Overridable<int>[]> limitSteps = new(
              value: Array.Empty<Overridable<int>>()
            , isOverridden: false
        );

        [OverridableDefault(typeof(TestDefaults), nameof(TestDefaults.FrameRate))]
        [ValueChoices(typeof(TestChoices), nameof(TestChoices.FrameRates))]
        public Overridable<int> frameRate;

        [OverridableDefault(typeof(TestDefaults), nameof(TestDefaults.Region))]
        [ValueChoices(typeof(TestChoices), nameof(TestChoices.Regions), IsExclusive = true)]
        public Overridable<string> region = new(value: string.Empty, isOverridden: false);

        [OverridableDefault(typeof(TestDefaults), nameof(TestDefaults.TierLimit))]
        [ValueChoices(nameof(TierLimits))]
        public Overridable<TestQuality> tierLimit;

        [OverridableDefault(typeof(TestDefaults), nameof(TestDefaults.TierLimit))]
        [ValueChoices(nameof(TierLimits), IsExclusive = true)]
        public Overridable<TestQuality> strictTierLimit = new(value: TestQuality.Medium, isOverridden: true);

        [ValueChoices(typeof(TestChoices), nameof(TestChoices.FrameRates))]
        public int targetFrameRate = 75;

        [ValueChoices(typeof(TestChoices), nameof(TestChoices.Regions), IsExclusive = true)]
        public string regionCode = "xx";

        [ValueChoices(typeof(TestChoices), nameof(TestChoices.FrameRates))]
        public List<int> frameRateSteps = new() { 30, 144 };

        [ValueChoices(nameof(LayerPresets))]
        public Overridable<TestLayers> layerPresets;

        [ValueChoices(nameof(LayerPresets), IsExclusive = true)]
        public Overridable<TestLayers> strictLayerPresets = new(value: TestLayers.Air, isOverridden: true);

        private static ValueChoice[] TierLimits => new ValueChoice[] {
            new("Low", TestQuality.Low),
            new("High", TestQuality.High),
            new(TestQuality.Ultra),
        };

        private static ValueChoice<TestLayers>[] LayerPresets => new ValueChoice<TestLayers>[] {
            new("Ground only", TestLayers.Ground),
            new("Ground and Air", TestLayers.Ground | TestLayers.Air),
            new(TestLayers.Water),
        };
    }

    public enum TestQuality
    {
        Low,
        Medium,
        High,
        [InspectorName("Ultra (Experimental)")] Ultra,
    }

    [Flags]
    public enum TestLayers
    {
        None = 0,
        Ground = 1,
        Water = 2,
        Air = 4,
    }

    public static class TestDefaults
    {
        public static int RetryCount = 3;
        public static string DisplayName = "Player";
        public static int FrameRate = 60;
        public static string Region = "asia";
        public static TestQuality TierLimit = TestQuality.High;

        public static List<int> SpawnWaves => new() { 1, 2, 4 };
    }

    public static class TestChoices
    {
        public static int[] FrameRates => new[] { 30, 60, 120 };

        public static ValueChoice<string>[] Regions => new ValueChoice<string>[] {
            new("Asia", "asia"),
            new("Europe", "eu"),
            new("North America", "na"),
        };
    }
}

#endif
