#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.Editor.UIElements;
using EncosyTower.PageFlows.UguiPages;

namespace EncosyTower.Editor.PageFlows.UguiPages.Settings.Views
{
    internal static class Constants
    {
        private const string MODULE_ROOT =
            $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor.PageFlows/UguiPages.Settings";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(UguiPageFlowSettings);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";
    }
}

#endif
