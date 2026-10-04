#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using EncosyTower.PageFlows.UitkPages;

namespace EncosyTower.Editor.PageFlows.UitkPages.Settings.Views
{
    internal static class Constants
    {
        private const string MODULE_ROOT =
            $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor.PageFlows/UitkPages.Settings";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(UitkPageFlowSettings);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
    }
}

#endif
