#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;

namespace EncosyTower.Editor.SourceGen.Settings.Views
{
    internal static class Constants
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor.SourceGen/Settings";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(SourceGenSettings);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
    }
}

#endif
