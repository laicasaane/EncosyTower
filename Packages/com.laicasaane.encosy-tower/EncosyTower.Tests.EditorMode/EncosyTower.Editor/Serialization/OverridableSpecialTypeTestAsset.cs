#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor;
using EncosyTower.Serialization;
using UnityEngine;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class OverridableSpecialTypeTestAsset : ScriptableObject
    {
        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.AccentColor))]
        public Overridable<Color> accentColor = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.Summary))]
        [ValueChoices(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.SummaryPresets))]
        public Overridable<SpecialTypeTestSummary> summary = new(value: new(), isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.SpawnBudget))]
        public Overridable<SpecialTypeTestBudget> spawnBudget = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.WaveArray))]
        public Overridable<int[]> waveArray = new(value: Array.Empty<int>(), isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.FixedSamples))]
        public Overridable<SpecialTypeTestFixedSamples> fixedSamples = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.ReferenceGraph))]
        public Overridable<SpecialTypeTestReferenceGraph> referenceGraph = new(value: new(), isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.CollisionMask))]
        public Overridable<LayerMask> collisionMask = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.ResponseCurve))]
        public Overridable<AnimationCurve> responseCurve = new(value: new AnimationCurve(), isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.HeatGradient))]
        public Overridable<Gradient> heatGradient = new(value: new Gradient(), isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.Viewport))]
        public Overridable<Rect> viewport = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.SpawnBounds))]
        public Overridable<Bounds> spawnBounds = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.GridCell))]
        public Overridable<Vector2Int> gridCell = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.Orientation))]
        public Overridable<Quaternion> orientation = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.ContentHash))]
        public Overridable<Hash128> contentHash = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.Marker))]
        public Overridable<char> marker = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.Population))]
        public Overridable<long> population = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.Precision))]
        public Overridable<double> precision = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.UvScale))]
        public Overridable<Vector2> uvScale = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.Direction))]
        public Overridable<Vector3> direction;

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.Weights))]
        public Overridable<Vector4> weights = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.GridPosition))]
        public Overridable<Vector3Int> gridPosition = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.TileArea))]
        public Overridable<RectInt> tileArea = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.CellBounds))]
        public Overridable<BoundsInt> cellBounds = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.ExposedMaterial))]
        public Overridable<ExposedReference<Texture2D>> exposedMaterial = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.PresetMaterial))]
        [ValueChoices(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.MaterialPresets))]
        public Overridable<Texture2D> presetMaterial;

        [OverridableDefault(typeof(SpecialTypeTestDefaults), nameof(SpecialTypeTestDefaults.LimitList))]
        public Overridable<List<Overridable<int>>> limitList = new(value: new(), isOverridden: false);
    }

    [Serializable]
    public sealed class SpecialTypeTestSummary
    {
        public string text = string.Empty;
    }

    [Serializable]
    public struct SpecialTypeTestBudget
    {
        [field: SerializeField]
        public int Count { get; set; }

        [field: SerializeField]
        public float Delay { get; set; }

        public readonly override string ToString()
            => FormattableString.Invariant($"Count: {Count}, Delay: {Delay}");
    }

    [Serializable]
    public unsafe struct SpecialTypeTestFixedSamples
    {
        public fixed int signedValues[2];
        public fixed uint unsignedValues[2];
        public fixed long longValues[2];
        public fixed ulong ulongValues[2];
        public fixed float floatValues[2];
        public fixed double doubleValues[2];
        public fixed bool boolValues[2];
        public fixed char charValues[2];

        public static SpecialTypeTestFixedSamples Create()
        {
            var result = new SpecialTypeTestFixedSamples();
            result.signedValues[0] = -7;
            result.signedValues[1] = 11;
            result.unsignedValues[0] = 4000000000u;
            result.unsignedValues[1] = 23u;
            result.longValues[0] = -9000000000L;
            result.longValues[1] = 9000000000L;
            result.ulongValues[0] = 18000000000000000000UL;
            result.ulongValues[1] = 42UL;
            result.floatValues[0] = 1.25f;
            result.floatValues[1] = -2.5f;
            result.doubleValues[0] = 0.123456789012345d;
            result.doubleValues[1] = -9.87654321098765d;
            result.boolValues[0] = true;
            result.boolValues[1] = false;
            result.charValues[0] = 'Ω';
            result.charValues[1] = 'Z';
            return result;
        }

        public readonly override string ToString()
            => nameof(SpecialTypeTestFixedSamples);
    }

    [Serializable]
    public sealed class SpecialTypeTestReferenceGraph
    {
        [SerializeReference]
        public SpecialTypeTestReferenceNode root;

        [SerializeReference]
        public SpecialTypeTestReferenceNode alias;

        [SerializeReference]
        public SpecialTypeTestReferenceNode optional;

        public SerializableGuid[] ids;

        public static SpecialTypeTestReferenceGraph Create()
        {
            var node = new SpecialTypeTestReferenceNode { number = 7 };

            return new() {
                root = node,
                alias = node,
                ids = new[] { SpecialTypeTestDefaults.SessionId },
            };
        }
    }

    [Serializable]
    public sealed class SpecialTypeTestReferenceNode
    {
        public int number;
    }

    public static class SpecialTypeTestDefaults
    {
        public static Texture2D Material;

        public static SerializableGuid SessionId => new(new Guid("01234567-89ab-4cde-8f01-23456789abcd"));

        public static Color AccentColor => new(r: 0.2f, g: 0.6f, b: 0.9f, a: 0.75f);

        public static SpecialTypeTestSummary Summary => new() { text = "Ready for launch" };

        public static SpecialTypeTestBudget SpawnBudget => new() { Count = 12, Delay = 0.75f };

        public static int[] WaveArray => new[] { 2, 5, 9 };

        public static SpecialTypeTestFixedSamples FixedSamples => SpecialTypeTestFixedSamples.Create();

        public static SpecialTypeTestReferenceGraph ReferenceGraph => SpecialTypeTestReferenceGraph.Create();

        public static LayerMask CollisionMask => (LayerMask)5;

        public static AnimationCurve ResponseCurve => CreateResponseCurve();

        public static Gradient HeatGradient => CreateHeatGradient();

        public static Rect Viewport => new(x: 10f, y: 20f, width: 320f, height: 180f);

        public static Bounds SpawnBounds => new(center: new(x: 1f, y: 2f, z: 3f), size: new(x: 4f, y: 6f, z: 8f));

        public static Vector2Int GridCell => new(x: 3, y: -2);

        public static Quaternion Orientation => Quaternion.Euler(x: 0f, y: 45f, z: 0f);

        public static Hash128 ContentHash => Hash128.Parse("0123456789abcdef0123456789abcdef");

        public static char Marker => 'Ω';

        public static long Population => 9000000000L;

        public static double Precision => 0.123456789012345d;

        public static Vector2 UvScale => new(x: 1.5f, y: 2.5f);

        public static Vector3 Direction => new(x: 1f, y: 2f, z: 3f);

        public static Vector4 Weights => new(x: 0.1f, y: 0.2f, z: 0.3f, w: 0.4f);

        public static Vector3Int GridPosition => new(x: 2, y: -3, z: 4);

        public static RectInt TileArea => new(xMin: 2, yMin: 3, width: 8, height: 6);

        public static BoundsInt CellBounds => new(position: new(x: 1, y: 2, z: 3), size: new(x: 4, y: 5, z: 6));

        public static ExposedReference<Texture2D> ExposedMaterial
            => new() {
                exposedName = new PropertyName("showcase-material"),
                defaultValue = Material,
            };

        public static Texture2D PresetMaterial => Material;

        public static ValueChoice<SpecialTypeTestSummary>[] SummaryPresets
            => new ValueChoice<SpecialTypeTestSummary>[] {
                new("Ready", new() { text = "Ready for launch" }),
                new("Hold", new() { text = "On hold" }),
            };

        public static ValueChoice<Texture2D>[] MaterialPresets => new ValueChoice<Texture2D>[] {
            new("Default material", Material),
            new(label: "None", value: null),
        };

        public static List<Overridable<int>> LimitList => new() {
            new(value: 5, isOverridden: true),
            new(value: 9, isOverridden: false),
        };

        private static AnimationCurve CreateResponseCurve()
        {
            var curve = AnimationCurve.Linear(timeStart: 0f, valueStart: 0.25f, timeEnd: 1f, valueEnd: 1f);
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.PingPong;
            return curve;
        }

        private static Gradient CreateHeatGradient()
        {
            var gradient = new Gradient { mode = GradientMode.Blend };

            gradient.SetKeys(
                  colorKeys: new[] {
                      new GradientColorKey(col: new(r: 0.2f, g: 0.6f, b: 0.9f), time: 0f),
                      new GradientColorKey(col: new(r: 1f, g: 0.4f, b: 0.1f), time: 1f),
                  }
                , alphaKeys: new[] {
                      new GradientAlphaKey(alpha: 1f, time: 0f),
                      new GradientAlphaKey(alpha: 0.25f, time: 1f),
                  }
            );

            return gradient;
        }
    }
}

#endif
