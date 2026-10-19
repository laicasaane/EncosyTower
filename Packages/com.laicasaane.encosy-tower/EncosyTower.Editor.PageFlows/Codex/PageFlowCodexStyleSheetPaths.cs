#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Holds the paths of the style sheets of the codex inspectors, and loads them on first use.
    /// </summary>
    /// <remarks>
    /// There is one main style sheet and one for each of the dark and light themes. A sheet is reloaded when Unity
    /// destroys it.
    /// </remarks>
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

        /// <summary>
        /// Gets the style sheet of the current theme.
        /// </summary>
        /// <value>
        /// The dark sheet when <see cref="EditorGUIUtility.isProSkin"/> is <see langword="true"/>, otherwise the light
        /// sheet. The theme is read at each access, so an element follows a changed skin only after
        /// <see cref="AddTo"/> runs again.
        /// </value>
        public static StyleSheet ThemeStyleSheet => EditorGUIUtility.isProSkin
            ? EditorAPI.GetOrLoadAsset(ref s_darkStyleSheet, DARK_USS_PATH)
            : EditorAPI.GetOrLoadAsset(ref s_lightStyleSheet, LIGHT_USS_PATH);

        /// <summary>
        /// Adds the theme style sheet and then the main style sheet to an element.
        /// </summary>
        /// <param name="element">The root element of an inspector or popup.</param>
        /// <remarks>
        /// The main sheet comes last, so it wins when both sheets set the same property. Call it once for each root;
        /// a second call adds the sheets again.
        /// </remarks>
        public static void AddTo(VisualElement element)
        {
            element.styleSheets.Add(ThemeStyleSheet);
            element.styleSheets.Add(StyleSheet);
        }
    }
}

#endif
