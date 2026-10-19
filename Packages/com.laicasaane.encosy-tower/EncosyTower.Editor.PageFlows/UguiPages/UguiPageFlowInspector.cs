#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UguiPages
{
    /// <summary>
    /// The custom inspector of <see cref="UguiPageFlow"/> and its derived types.
    /// </summary>
    /// <remarks>
    /// Shows one Context section for the flow context. Edits go through the serialized context field, so Undo and
    /// editing several objects work through Unity property binding.
    /// </remarks>
    /// <seealso cref="UguiPageCodexInspector"/>
    [CustomEditor(typeof(UguiPageFlow), true)]
    [CanEditMultipleObjects]
    public class UguiPageFlowInspector : UnityEditor.Editor
    {
        private const string CONTEXT = nameof(UguiPageFlow._context);

        /// <inheritdoc/>
        public override VisualElement CreateInspectorGUI()
        {
            var root = PageFlowContextSection.CreateRoot();
            var context = serializedObject.FindProperty(CONTEXT);

            root.Add(PageFlowContextSection.Create(context, OpenSettings, UguiPageFlowContextSettings.All));
            return root;
        }

        private static void OpenSettings()
        {
            UguiPageFlowSettings.Instance.OpenSettingsWindow();
        }
    }
}

#endif
