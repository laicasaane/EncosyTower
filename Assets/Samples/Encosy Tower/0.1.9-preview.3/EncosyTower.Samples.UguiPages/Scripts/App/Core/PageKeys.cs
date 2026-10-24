using EncosyTower.AssetKeys;
using EncosyTower.Samples.UguiPages.Ids;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>The asset keys of the pages that the codex shows by name.</summary>
    internal static class PageKeys
    {
        public static AssetKey MainMenuScreen => AssetKeyAPI.Key(ScreenPage.MainMenu);

        public static AssetKey EntranceScreen => AssetKeyAPI.Key(ScreenPage.Entrance);

        public static AssetKey CoreScreen => AssetKeyAPI.Key(ScreenPage.CoreDimension);

        public static AssetKey DimensionScreen => AssetKeyAPI.Key(ScreenPage.Dimension);

        public static AssetKey FloatingContext => AssetKeyAPI.Key(OverlayPage.FloatingContext);

        public static AssetKey Toast => AssetKeyAPI.Key(OverlayPage.Toast);

        public static AssetKey LostAppendix => AssetKeyAPI.Key(DialogPage.LostAppendix);
    }
}
