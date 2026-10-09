#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor;
using EncosyTower.Serialization;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.DevTools.Showcases.Overridables
{
    public enum ShowcaseQuality
    {
        Low,
        Medium,
        High,
        [InspectorName("Ultra (Experimental)")] Ultra,
    }

    [Flags]
    public enum ShowcaseLayers
    {
        None = 0,
        Ground = 1,
        Water = 2,
        Air = 4,
    }

    public interface IShowcaseShape
    {
        float Area { get; }
    }

    [Serializable]
    public abstract class ShowcaseShape : IShowcaseShape
    {
        public Overridable<Color> tint;

        public abstract float Area { get; }
    }

    [Serializable]
    public sealed class ShowcaseCircle : ShowcaseShape
    {
        [OverridableDefault(typeof(ShowcaseDefaults), nameof(ShowcaseDefaults.Radius))]
        public Overridable<float> radius;

        public override float Area
        {
            get
            {
                var r = radius.GetValueOrDefault(ShowcaseDefaults.Radius);
                return Mathf.PI * r * r;
            }
        }
    }

    [Serializable]
    public sealed class ShowcaseSquare : ShowcaseShape
    {
        public Overridable<float> size;

        public override float Area => size.value * size.value;
    }

    [Serializable]
    public sealed class ShowcasePalette
    {
        public Color primary = Color.white;
        public Color secondary = Color.black;
    }

    [Serializable]
    public sealed class ShowcaseSummary
    {
        public string text = string.Empty;
    }

    [Serializable]
    public struct ShowcaseBudget
    {
        [field: SerializeField]
        public int Count { get; set; }

        [field: SerializeField]
        public float Delay { get; set; }

        public readonly override string ToString()
            => FormattableString.Invariant($"Count: {Count}, Delay: {Delay}");
    }

    [Serializable]
    public unsafe struct ShowcaseFixedSamples
    {
        public fixed int signedValues[2];
        public fixed uint unsignedValues[2];
        public fixed long longValues[2];
        public fixed ulong ulongValues[2];
        public fixed float floatValues[2];
        public fixed double doubleValues[2];
        public fixed bool boolValues[2];
        public fixed char charValues[2];

        public static ShowcaseFixedSamples Create()
        {
            var result = new ShowcaseFixedSamples();
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
            => nameof(ShowcaseFixedSamples);
    }

    [Serializable]
    public sealed class ShowcaseReferenceGraph
    {
        [SerializeReference]
        public ShowcaseReferenceNode root;

        [SerializeReference]
        public ShowcaseReferenceNode alias;

        [SerializeReference]
        public ShowcaseReferenceNode optional;

        public SerializableGuid[] ids;

        public static ShowcaseReferenceGraph Create()
        {
            var node = new ShowcaseReferenceNode { number = 7 };

            return new() {
                root = node,
                alias = node,
                ids = new[] { ShowcaseDefaults.SessionId },
            };
        }
    }

    [Serializable]
    public sealed class ShowcaseReferenceNode
    {
        public int number;
    }

    public static class ShowcaseChoices
    {
        public static ValueChoice<ShowcaseSummary>[] SummaryPresets => new ValueChoice<ShowcaseSummary>[] {
            new("Ready", new() { text = "Ready for launch" }),
            new("Hold", new() { text = "On hold" }),
        };

        public static ValueChoice<Material>[] MaterialPresets => new ValueChoice<Material>[] {
            new("Default material", ShowcaseDefaults.Material),
            new(label: "None", value: null),
        };

        public static int[] FrameRates => new[] { 30, 60, 120 };

        public static ValueChoice<string>[] Regions => new ValueChoice<string>[] {
            new("Asia", "asia"),
            new("Europe", "eu"),
            new("North America", "na"),
        };
    }

    public static class ShowcaseDefaults
    {
        public static ShowcaseQuality Quality = ShowcaseQuality.High;
        public static int RetryCount = 3;
        public static string DisplayName = "Player";
        public static Vector3 SpawnOffset = new(0f, 1f, 0f);
        public static ShowcaseQuality Tier = ShowcaseQuality.Medium;
        public static float Radius = 0.5f;
        public static int FrameRate = 60;
        public static string Region = "asia";
        public static ShowcaseQuality TierLimit = ShowcaseQuality.High;

        public static bool Enabled => true;

        public static Material Material => AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

        public static Texture2D Icon => Texture2D.whiteTexture;

        public static ShowcasePalette Palette => new();

        public static List<int> SpawnWaves => new() { 1, 2, 4 };

        public static SerializableGuid SessionId => new(new Guid("01234567-89ab-4cde-8f01-23456789abcd"));

        public static Color AccentColor => new(r: 0.2f, g: 0.6f, b: 0.9f, a: 0.75f);

        public static ShowcaseSummary Summary => new() { text = "Ready for launch" };

        public static ShowcaseBudget SpawnBudget => new() { Count = 12, Delay = 0.75f };

        public static int[] WaveArray => new[] { 2, 5, 9 };

        public static ShowcaseFixedSamples FixedSamples => ShowcaseFixedSamples.Create();

        public static ShowcaseReferenceGraph ReferenceGraph => ShowcaseReferenceGraph.Create();

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

        public static Vector4 Weights => new(x: 0.1f, y: 0.2f, z: 0.3f, w: 0.4f);

        public static Vector3Int GridPosition => new(x: 2, y: -3, z: 4);

        public static RectInt TileArea => new(xMin: 2, yMin: 3, width: 8, height: 6);

        public static BoundsInt CellBounds => new(position: new(x: 1, y: 2, z: 3), size: new(x: 4, y: 5, z: 6));

        public static ExposedReference<Material> ExposedMaterial
            => new() {
                exposedName = new PropertyName("showcase-material"),
                defaultValue = Material,
            };

        public static Material PresetMaterial => Material;

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
