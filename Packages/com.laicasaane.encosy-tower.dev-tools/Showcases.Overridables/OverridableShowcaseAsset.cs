#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor;
using EncosyTower.Serialization;
using UnityEngine;

namespace EncosyTower.DevTools.Showcases.Overridables
{
    /// <summary>
    /// Every <see cref="Overridable{T}"/> use case shown by <see cref="OverridableShowcaseWindow"/>.
    /// </summary>
    /// <remarks>
    /// Defaults come either from <see cref="ShowcaseDefaults"/> (static members) or from
    /// <see cref="OverridableShowcaseSettings"/> (Project Settings > Encosy Tower > Overridable Showcase).
    /// </remarks>
    public sealed class OverridableShowcaseAsset : ScriptableObject
    {
        [Header("Values")]
        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Enabled), Label = "Global")]
        public Overridable<bool> enabled;

        [OverridableDefault(typeof(OverridableShowcaseSettings), nameof(OverridableShowcaseSettings.logWarnings))]
        public Overridable<bool> logWarnings;

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.RetryCount))]
        public Overridable<int> retryCount;

        [OverridableDefault(typeof(OverridableShowcaseSettings), nameof(OverridableShowcaseSettings.maxRetries))]
        public Overridable<int> maxRetries;

        public Overridable<float> speed;

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.DisplayName))]
        public Overridable<string> displayName = new(value: string.Empty, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.SpawnOffset))]
        public Overridable<Vector3> spawnOffset;

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.AccentColor))]
        public Overridable<Color> accentColor = new(value: default, isOverridden: false);

        [Header("Enums")]
        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Quality))]
        public Overridable<ShowcaseQuality> quality;

        [OverridableDefault(typeof(OverridableShowcaseSettings), nameof(OverridableShowcaseSettings.quality))]
        public Overridable<ShowcaseQuality> projectQuality;

        public Overridable<ShowcaseQuality> fallbackQuality;

        public Overridable<ShowcaseLayers> layers;

        [Header("Unity Object References")]
        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Material))]
        public Overridable<Material> material;

        [OverridableDefault(typeof(OverridableShowcaseSettings), nameof(OverridableShowcaseSettings.material))]
        public Overridable<Material> projectMaterial;

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Icon))]
        public Overridable<Texture2D> icon;

        public Overridable<GameObject> prefab;

        [Header("Serializable Classes and Collections")]
        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Palette))]
        public Overridable<ShowcasePalette> palette = new(value: new(), isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Summary))]
        [ValueChoices(typeof(ShowcaseChoices), nameof(ShowcaseChoices.SummaryPresets))]
        public Overridable<ShowcaseSummary> summary = new(value: new(), isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.SpawnBudget))]
        public Overridable<ShowcaseBudget> spawnBudget = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Tier))]
        public Overridable<ShowcaseQuality>[] tiers = new Overridable<ShowcaseQuality>[2];

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.RetryCount))]
        public Overridable<int>[] retryLimits = { default, new(value: 8, isOverridden: true) };

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.SpawnWaves))]
        public Overridable<List<int>> spawnWaves = new(value: new(), isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.WaveArray))]
        public Overridable<int[]> waveArray = new(value: Array.Empty<int>(), isOverridden: false);

        [OverridableDefault(typeof(OverridableShowcaseSettings), nameof(OverridableShowcaseSettings.limitSteps))]
        public Overridable<Overridable<int>[]> limitSteps = new(
              value: Array.Empty<Overridable<int>>()
            , isOverridden: false
        );

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.SessionId))]
        public Overridable<SerializableGuid> sessionId;

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.FixedSamples))]
        public Overridable<ShowcaseFixedSamples> fixedSamples = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.ReferenceGraph))]
        public Overridable<ShowcaseReferenceGraph> referenceGraph = new(value: new(), isOverridden: false);

        [Header("SerializeReference")]
        [SerializeReference]
        public ShowcaseShape shape = new ShowcaseCircle();

        [Header("Value Choices")]
        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.FrameRate))]
        [ValueChoices(
              "EncosyTower.DevTools.Showcases.Overridables.ShowcaseChoices"
            , nameof(ShowcaseChoices.FrameRates)
        )]
        public Overridable<int> frameRate;

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Region))]
        [ValueChoices(typeof(ShowcaseChoices), nameof(ShowcaseChoices.Regions), IsExclusive = true)]
        public Overridable<string> region = new(value: string.Empty, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.TierLimit))]
        [ValueChoices(nameof(TierLimits))]
        public Overridable<ShowcaseQuality> tierLimit;

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.TierLimit))]
        [ValueChoices(nameof(TierLimits), IsExclusive = true)]
        public Overridable<ShowcaseQuality> strictTierLimit = new(value: ShowcaseQuality.Medium, isOverridden: true);

        [ValueChoices(typeof(ShowcaseChoices), nameof(ShowcaseChoices.FrameRates))]
        public int targetFrameRate = 75;

        [ValueChoices(typeof(ShowcaseChoices), nameof(ShowcaseChoices.Regions), IsExclusive = true)]
        public string regionCode = "xx";

        [ValueChoices(typeof(ShowcaseChoices), nameof(ShowcaseChoices.FrameRates))]
        public List<int> frameRateSteps = new() { 30, 144 };

        [ValueChoices(nameof(LayerPresets))]
        public Overridable<ShowcaseLayers> layerPresets;

        [ValueChoices(nameof(LayerPresets), IsExclusive = true)]
        public Overridable<ShowcaseLayers> strictLayerPresets = new(value: ShowcaseLayers.Air, isOverridden: true);

        [ValueChoices(nameof(WavePresets), applyToCollection: true)]
        public List<int> wavePlan = new() { 1, 2, 4 };

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.SpawnWaves))]
        [ValueChoices(nameof(WavePresets), applyToCollection: true)]
        public Overridable<List<int>> wavePresets = new(value: new(), isOverridden: false);

        [Header("Unity Built-in Types")]
        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.CollisionMask))]
        public Overridable<LayerMask> collisionMask = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.ResponseCurve))]
        public Overridable<AnimationCurve> responseCurve = new(value: new AnimationCurve(), isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.HeatGradient))]
        public Overridable<Gradient> heatGradient = new(value: new Gradient(), isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Viewport))]
        public Overridable<Rect> viewport = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.SpawnBounds))]
        public Overridable<Bounds> spawnBounds = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.GridCell))]
        public Overridable<Vector2Int> gridCell = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Orientation))]
        public Overridable<Quaternion> orientation = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.ContentHash))]
        public Overridable<Hash128> contentHash = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Marker))]
        public Overridable<char> marker = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Population))]
        public Overridable<long> population = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Precision))]
        public Overridable<double> precision = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.UvScale))]
        public Overridable<Vector2> uvScale = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Weights))]
        public Overridable<Vector4> weights = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.GridPosition))]
        public Overridable<Vector3Int> gridPosition = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.TileArea))]
        public Overridable<RectInt> tileArea = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.CellBounds))]
        public Overridable<BoundsInt> cellBounds = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.ExposedMaterial))]
        public Overridable<ExposedReference<Material>> exposedMaterial = new(value: default, isOverridden: false);

        [Header("Unity Object Reference Presets")]
        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.PresetMaterial))]
        [ValueChoices(typeof(ShowcaseChoices), nameof(ShowcaseChoices.MaterialPresets))]
        public Overridable<Material> presetMaterial = new(value: null, isOverridden: false);

        [Header("Nested Collections")]
        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.LimitList))]
        public Overridable<List<Overridable<int>>> limitList = new(value: new(), isOverridden: false);

        private static ValueChoice<List<int>>[] WavePresets => new ValueChoice<List<int>>[] {
            new("Gentle", new() { 1, 2, 3 }),
            new("Standard", new() { 1, 2, 4 }),
            new("Rush", new() { 2, 4, 8 }),
        };

        private static ValueChoice[] TierLimits => new ValueChoice[] {
            new("Low", ShowcaseQuality.Low),
            new("High", ShowcaseQuality.High),
            new(ShowcaseQuality.Ultra),
        };

        private static ValueChoice<ShowcaseLayers>[] LayerPresets => new ValueChoice<ShowcaseLayers>[] {
            new("Ground only", ShowcaseLayers.Ground),
            new("Ground and Air", ShowcaseLayers.Ground | ShowcaseLayers.Air),
            new(ShowcaseLayers.Water),
        };
    }
}

#endif
