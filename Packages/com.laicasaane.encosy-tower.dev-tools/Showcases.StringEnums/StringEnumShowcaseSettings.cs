#if UNITY_EDITOR

using EncosyTower.Common;
using EncosyTower.Serialization;
using EncosyTower.Settings;

namespace EncosyTower.DevTools.Showcases.StringEnums
{
    [Settings(SettingsUsage.EditorProject, "Dev Tools/Showcases/String Enums")]
    public sealed class StringEnumShowcaseSettings : Settings<StringEnumShowcaseSettings>
    {
        public StringEnum<ShowcaseElement> element = ShowcaseElement.Wind;

        public StringEnum<ShowcaseDifficulty, ShowcaseDifficultyCodeConverter> difficulty = ShowcaseDifficulty.Normal;

        public Overridable<StringEnum<ShowcaseElement>>[] elementSteps = {
            new(value: ShowcaseElement.Fire, isOverridden: true),
            default,
        };
    }
}

#endif
