using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>Identifies the Book's configurable texts.</summary>
    [EnumExtensions]
    [EnumMembersForTemplate(typeof(ConfigType_EnumTemplate), 400)]
    [KindForUnionId(typeof(ConfigId), 400, "Text")]
    internal enum ConfigTextType : byte
    {
        ToastMissingPage,
        ToastOpenFailed,
        ToastSaveFailed,
        ToastSaveUnreadable,
        ToastKeptOpen,
        ToastEchoClosed,
        ToastIncantationWaiting,
        ToastRuneDiscovered,
        ToastIncantationLearned,
        ToastCoreUnlocked,
        EntranceTitleCast,
        EntranceTitleUncast,
        DimensionLockedTitle,
        DimensionPageTitle,
        CoreSpellLearned,
        CoreSpellWaiting,
        CoreCastButton,
        CoreReadNameButton,
        TabInscriptionFound,
        TabInscriptionEmpty,
        CardConceptTitle,
        CardInPracticeTitle,
        CardWatchFor,
        EchoTitle,
        EchoNote,
        MapStatusEntrance,
        MapStatusCore,
        MapStatusDimension,
        RuneTooltipCollected,
        RuneTooltipEmpty,
        NavigationBarBack,
        NavigationBarForward,
        NavigationBarSettings,
        SettingsTitle,
        SettingsTheme,
        SettingsThemeDark,
        SettingsThemeLight,
        SettingsExit,
    }
}
