using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>Identifies the Book's configurable colors, each with a light and a dark value.</summary>
    [EnumExtensions]
    [EnumMembersForTemplate(typeof(ConfigType_EnumTemplate), 100)]
    [KindForUnionId(typeof(ConfigId), 100, "Color")]
    internal enum ConfigColorType : byte
    {
        Ink,
        InkNavigation,
        InkEntrance,
        NavigationBar,
        PageMainMenu,
        PageEntrance,
        PageMap,
        PageMore,
        PageConfirmation,
        PageEcho,
        ToastFailure,
        ToastCancelled,
        ToastInfo,
        Locked,
        CastGold,
        CoreGlow,
        Backdrop,
        MapLine,
        MapEntranceIdle,
        MapEntranceBorder,
        ButtonBorder,
        RuneBorder,
        Border,
        BorderDialog,
        MenuHoverBackground,
        MenuHoverInk,
        EntranceCardBorderLocked,
        EntranceCardInkDisabled,
        EntranceNavigationBorder,
        CardHoverBorder,
        RuneDefaultInk,
        BorderTranslucentPanel,
        BorderTranslucentDialog,
        PageDimensionDefault,
        FocusOnLight,
        FocusOnDark,
        SettingsThemeSelectedBackground,
        SettingsThemeSelectedInk,
        SettingsThemeUnselectedHover,
        ButtonHover,
        ButtonPress,
        MenuPressBackground,
        EntranceCardLockedHover,
        RuneVfxNearest,
        RuneVfxMiddle,
        RuneVfxFar,
        CoreVfxNearest,
        CoreVfxMiddle,
        CoreVfxFar,
    }
}
