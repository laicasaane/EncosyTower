#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.DevTools.Showcases.StringEnums
{
    [CustomEditor(typeof(StringEnumShowcaseAsset)), CanEditMultipleObjects]
    public sealed class StringEnumShowcaseAssetEditor : UnityEditor.Editor
    {
        public static bool UseImgui;

        public override VisualElement CreateInspectorGUI()
        {
            if (UseImgui)
            {
                return null;
            }

            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            return root;
        }

        public override void OnInspectorGUI()
        {
            if (UseImgui)
            {
                DrawDefaultInspector();
                return;
            }

            base.OnInspectorGUI();
        }
    }
}

#endif
