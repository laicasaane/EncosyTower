#if UNITY_EDITOR

using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor;
using EncosyTower.Serialization;
using UnityEngine;

namespace EncosyTower.DevTools.Showcases.StringEnums
{
    public sealed class StringEnumShowcaseAsset : ScriptableObject
    {
        [Header("StringEnum<TEnum>")]
        public StringEnum<ShowcaseElement> element;

        public StringEnum<ShowcaseElement> elementPicked = ShowcaseElement.Fire;

        public StringEnum<ShowcaseElement> elementUnlisted = new("Lava");

        public StringEnum<ShowcaseLayers> layers = ShowcaseLayers.Ground;

        [Header("StringEnum<TEnum, TConverter>")]
        public StringEnum<ShowcaseElement, ShowcaseElementConverter> convertedElement = ShowcaseElement.Water;

        public StringEnum<ShowcaseDifficulty, ShowcaseDifficultyCodeConverter> difficulty = ShowcaseDifficulty.Hard;

        public StringEnum<ShowcaseElement, ShowcaseBrokenConverter> brokenConverter = new("fire");

        [Header("Arrays, Lists and Nested Classes")]
        public StringEnum<ShowcaseElement>[] elementArray = {
            ShowcaseElement.Fire,
            ShowcaseElement.Wind,
        };

        public List<StringEnum<ShowcaseDifficulty, ShowcaseDifficultyCodeConverter>> difficultyList = new() {
            ShowcaseDifficulty.Easy,
            ShowcaseDifficulty.Hard,
        };

        public ShowcaseSpawnRule spawnRule = new();

        [Header("With Overridable<T>")]
        [OverridableDefault(typeof(StringEnumShowcaseDefaults), nameof(StringEnumShowcaseDefaults.Element))]
        public Overridable<StringEnum<ShowcaseElement>> overridableElement
            = new(value: default, isOverridden: false);

        [OverridableDefault(
              typeof(StringEnumShowcaseDefaults)
            , nameof(StringEnumShowcaseDefaults.ConvertedElement)
        )]
        public Overridable<StringEnum<ShowcaseElement, ShowcaseElementConverter>> overridableConverted
            = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(StringEnumShowcaseSettings), nameof(StringEnumShowcaseSettings.difficulty))]
        public Overridable<StringEnum<ShowcaseDifficulty, ShowcaseDifficultyCodeConverter>> projectDifficulty
            = new(value: default, isOverridden: false);

        public Overridable<StringEnum<ShowcaseElement>> overridableNoDefault;

        public Overridable<StringEnum<ShowcaseElement>> overridableOverridden
            = new(value: ShowcaseElement.Wind, isOverridden: true);

        public Overridable<StringEnum<ShowcaseElement>>[] overridableArray = new Overridable<StringEnum<ShowcaseElement>>[2];

        [OverridableDefault(typeof(StringEnumShowcaseDefaults), nameof(StringEnumShowcaseDefaults.SpawnRule))]
        public Overridable<ShowcaseSpawnRule> overridableSpawnRule
            = new(value: new(), isOverridden: false);

        [OverridableDefault(typeof(StringEnumShowcaseDefaults), nameof(StringEnumShowcaseDefaults.ElementPlan))]
        public Overridable<List<StringEnum<ShowcaseElement>>> overridablePlan
            = new(value: new(), isOverridden: false);

        [Header("With ValueChoices")]
        [ValueChoices(typeof(StringEnumShowcaseChoices), nameof(StringEnumShowcaseChoices.ElementPresets))]
        public StringEnum<ShowcaseElement> choicesElement = ShowcaseElement.Fire;

        [ValueChoices(
              typeof(StringEnumShowcaseChoices)
            , nameof(StringEnumShowcaseChoices.ElementPresets)
            , IsExclusive = true
        )]
        public StringEnum<ShowcaseElement> strictChoicesElement = ShowcaseElement.Fire;

        [ValueChoices(typeof(StringEnumShowcaseChoices), nameof(StringEnumShowcaseChoices.ConvertedElementPresets))]
        public StringEnum<ShowcaseElement, ShowcaseElementConverter> choicesConverted = ShowcaseElement.Wind;

        [ValueChoices(typeof(StringEnumShowcaseChoices), nameof(StringEnumShowcaseChoices.ElementPresets))]
        public StringEnum<ShowcaseElement>[] choicesArray = { ShowcaseElement.Fire, ShowcaseElement.Water };

        [ValueChoices(
              typeof(StringEnumShowcaseChoices)
            , nameof(StringEnumShowcaseChoices.ElementPlanPresets)
            , applyToCollection: true
        )]
        public List<StringEnum<ShowcaseElement>> choicesPlan = new() { ShowcaseElement.Fire };

        [Header("With Overridable<T> and ValueChoices")]
        [OverridableDefault(typeof(StringEnumShowcaseDefaults), nameof(StringEnumShowcaseDefaults.Element))]
        [ValueChoices(typeof(StringEnumShowcaseChoices), nameof(StringEnumShowcaseChoices.ElementPresets))]
        public Overridable<StringEnum<ShowcaseElement>> overridableChoices
            = new(value: default, isOverridden: false);

        [ValueChoices(
              typeof(StringEnumShowcaseChoices)
            , nameof(StringEnumShowcaseChoices.ElementPresets)
            , IsExclusive = true
        )]
        public Overridable<StringEnum<ShowcaseElement>> overridableStrictChoices
            = new(value: ShowcaseElement.Fire, isOverridden: true);

        [OverridableDefault(typeof(StringEnumShowcaseDefaults), nameof(StringEnumShowcaseDefaults.ConvertedElement))]
        [ValueChoices(typeof(StringEnumShowcaseChoices), nameof(StringEnumShowcaseChoices.ConvertedElementPresets))]
        public Overridable<StringEnum<ShowcaseElement, ShowcaseElementConverter>> overridableConvertedChoices
            = new(value: default, isOverridden: false);

        [OverridableDefault(typeof(StringEnumShowcaseDefaults), nameof(StringEnumShowcaseDefaults.ElementPlan))]
        [ValueChoices(
              typeof(StringEnumShowcaseChoices)
            , nameof(StringEnumShowcaseChoices.ElementPlanPresets)
            , applyToCollection: true
        )]
        public Overridable<List<StringEnum<ShowcaseElement>>> overridablePlanChoices
            = new(value: new(), isOverridden: false);
    }
}

#endif
