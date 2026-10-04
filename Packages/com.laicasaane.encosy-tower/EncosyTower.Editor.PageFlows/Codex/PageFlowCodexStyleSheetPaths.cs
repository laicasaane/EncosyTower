#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    internal static class PageFlowCodexStyleSheetPaths
    {
        public const string ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor.PageFlows/Codex";
        public const string USS_PATH = $"{ROOT}/StyleSheets/PageFlowCodexInspector.uss";

        private static StyleSheet s_styleSheet;

        public static StyleSheet StyleSheet => EditorAPI.GetOrLoadAsset(ref s_styleSheet, USS_PATH);
    }
}

#endif
