#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows.UguiPages;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UguiPages
{
    [CustomEditor(typeof(UguiPageFlow), true)]
    [CanEditMultipleObjects]
    public class UguiPageFlowInspector : UnityEditor.Editor
    {
        private const string CONTEXT = nameof(UguiPageFlow._context);

        public override VisualElement CreateInspectorGUI()
        {
            var root = PageFlowContextSection.CreateRoot();
            var context = serializedObject.FindProperty(CONTEXT);

            root.Add(PageFlowContextSection.Create(context, OpenSettings, UguiPageFlowContextRows.Rows));
            return root;
        }

        private static void OpenSettings()
        {
            UguiPageFlowSettings.Instance.OpenSettingsWindow();
        }
    }
}

#endif
