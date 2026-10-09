#if UNITY_EDITOR

using System.Collections.Generic;
using EncosyTower.Editor;
using EncosyTower.Serialization;
using UnityEngine;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class CollectionChoicesTestAsset : ScriptableObject
    {
        [ValueChoices(nameof(ListPresets), applyToCollection: true)]
        public List<int> wavePlan = new() { 1, 2, 4 };

        [ValueChoices(nameof(ArrayPresets), applyToCollection: true)]
        public int[] arrayPlan = { 1, 2, 4 };

        [OverridableDefault(typeof(CollectionChoicesTestAsset), nameof(DefaultList))]
        [ValueChoices(nameof(ListPresets), applyToCollection: true)]
        public Overridable<List<int>> wavePresets = new(value: new() { 99 }, isOverridden: false);

        [OverridableDefault(typeof(CollectionChoicesTestAsset), nameof(DefaultArray))]
        [ValueChoices(nameof(ArrayPresets), applyToCollection: true)]
        public Overridable<int[]> arrayPresets = new(value: new[] { 99 }, isOverridden: false);

        [ValueChoices(nameof(ElementPresets))]
        public List<int> elementPlan = new() { 30, 144 };

        public static List<int> DefaultList => new() { 1, 2, 4 };

        public static int[] DefaultArray => new[] { 1, 2, 4 };

        public static int[] ElementPresets => new[] { 30, 60, 120 };

        public static ValueChoice<List<int>>[] ListPresets => new ValueChoice<List<int>>[] {
            new("Gentle", new() { 1, 2, 3 }),
            new("Standard", new() { 1, 2, 4 }),
            new("Rush", new() { 2, 4, 8 }),
        };

        public static ValueChoice<int[]>[] ArrayPresets => new ValueChoice<int[]>[] {
            new("Gentle", new[] { 1, 2, 3 }),
            new("Standard", new[] { 1, 2, 4 }),
            new("Rush", new[] { 2, 4, 8 }),
        };

        public static ValueChoice[] WrongErasedPresets => new ValueChoice[] {
            new("Valid", new List<int> { 1, 2, 4 }),
            new(value: 4),
        };
    }
}

#endif
