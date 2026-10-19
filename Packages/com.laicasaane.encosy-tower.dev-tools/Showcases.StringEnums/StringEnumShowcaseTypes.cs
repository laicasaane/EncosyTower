#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor;
using EncosyTower.Serialization;
using UnityEngine;

namespace EncosyTower.DevTools.Showcases.StringEnums
{
    public enum ShowcaseElement
    {
        None,
        Fire,
        [InspectorName("Cold Water")] Water,
        Wind,
    }

    public enum ShowcaseDifficulty
    {
        Easy,
        Normal,
        Hard,
    }

    [Flags]
    public enum ShowcaseLayers
    {
        None = 0,
        Ground = 1,
        Water = 2,
        Air = 4,
    }

    public readonly struct ShowcaseElementConverter : IStringEnumConverter<ShowcaseElement>
    {
        public string Transform(ShowcaseElement from)
            => from.ToString().ToLowerInvariant();

        public ShowcaseElement Transform(string from)
            => Enum.TryParse(from, ignoreCase: true, out ShowcaseElement result) ? result : ShowcaseElement.None;
    }

    public readonly struct ShowcaseDifficultyCodeConverter : IStringEnumConverter<ShowcaseDifficulty>
    {
        public string Transform(ShowcaseDifficulty from)
            => from switch {
                ShowcaseDifficulty.Easy => "E",
                ShowcaseDifficulty.Normal => "N",
                ShowcaseDifficulty.Hard => "H",
                _ => string.Empty,
            };

        public ShowcaseDifficulty Transform(string from)
            => from switch {
                "E" => ShowcaseDifficulty.Easy,
                "H" => ShowcaseDifficulty.Hard,
                _ => ShowcaseDifficulty.Normal,
            };
    }

    public readonly struct ShowcaseBrokenConverter : IStringEnumConverter<ShowcaseElement>
    {
        public string Transform(ShowcaseElement from)
            => throw new InvalidOperationException("The converter is broken on purpose.");

        public ShowcaseElement Transform(string from)
            => ShowcaseElement.None;
    }

    [Serializable]
    public sealed class ShowcaseSpawnRule
    {
        public StringEnum<ShowcaseElement> element;
        public StringEnum<ShowcaseDifficulty, ShowcaseDifficultyCodeConverter> difficulty;
        public Overridable<StringEnum<ShowcaseElement>> fallbackElement;
    }

    public static class StringEnumShowcaseDefaults
    {
        public static StringEnum<ShowcaseElement> Element => ShowcaseElement.Fire;

        public static StringEnum<ShowcaseElement, ShowcaseElementConverter> ConvertedElement
            => ShowcaseElement.Water;

        public static StringEnum<ShowcaseDifficulty, ShowcaseDifficultyCodeConverter> Difficulty
            => ShowcaseDifficulty.Hard;

        public static ShowcaseSpawnRule SpawnRule => new() {
            element = ShowcaseElement.Wind,
            difficulty = ShowcaseDifficulty.Easy,
        };

        public static List<StringEnum<ShowcaseElement>> ElementPlan => new() {
            ShowcaseElement.Fire,
            ShowcaseElement.Water,
        };
    }

    public static class StringEnumShowcaseChoices
    {
        public static ValueChoice<StringEnum<ShowcaseElement>>[] ElementPresets
            => new ValueChoice<StringEnum<ShowcaseElement>>[] {
                new("Fire only", ShowcaseElement.Fire),
                new("Wind only", ShowcaseElement.Wind),
                new(ShowcaseElement.Water),
            };

        public static ValueChoice<StringEnum<ShowcaseElement, ShowcaseElementConverter>>[] ConvertedElementPresets
            => new ValueChoice<StringEnum<ShowcaseElement, ShowcaseElementConverter>>[] {
                new("Fire only", ShowcaseElement.Fire),
                new("Wind only", ShowcaseElement.Wind),
            };

        public static ValueChoice<List<StringEnum<ShowcaseElement>>>[] ElementPlanPresets
            => new ValueChoice<List<StringEnum<ShowcaseElement>>>[] {
                new("Fire and Wind", new() { ShowcaseElement.Fire, ShowcaseElement.Wind }),
                new("All three", new() { ShowcaseElement.Fire, ShowcaseElement.Water, ShowcaseElement.Wind }),
            };
    }
}

#endif
