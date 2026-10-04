#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    internal static class PageFlowCodexStyleSheetPaths
    {
        public const string ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor.PageFlows/Codex";
        public const string USS_PATH = $"{ROOT}/StyleSheets/PageFlowCodexInspector.uss";
        public const string DARK_USS_PATH = $"{ROOT}/StyleSheets/PageFlowCodexInspectorDark.uss";
        public const string LIGHT_USS_PATH = $"{ROOT}/StyleSheets/PageFlowCodexInspectorLight.uss";

        private static StyleSheet s_styleSheet;
        private static StyleSheet s_darkStyleSheet;
        private static StyleSheet s_lightStyleSheet;

        public static StyleSheet StyleSheet => EditorAPI.GetOrLoadAsset(ref s_styleSheet, USS_PATH);

        public static StyleSheet ThemeStyleSheet => EditorGUIUtility.isProSkin
            ? EditorAPI.GetOrLoadAsset(ref s_darkStyleSheet, DARK_USS_PATH)
            : EditorAPI.GetOrLoadAsset(ref s_lightStyleSheet, LIGHT_USS_PATH);

        public static void AddTo(VisualElement element)
        {
            element.styleSheets.Add(ThemeStyleSheet);
            element.styleSheets.Add(StyleSheet);
        }
    }
}

#endif
