#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.DevTools.Showcases.Overridables
{
    [CustomPropertyDrawer(typeof(ShowcaseSummary))]
    public sealed class ShowcaseSummaryDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
            => new PropertyField(property.FindPropertyRelative(nameof(ShowcaseSummary.text)), string.Empty);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(
                  position: position
                , property: property.FindPropertyRelative(nameof(ShowcaseSummary.text))
                , label: GUIContent.none
            );
        }
    }
}

#endif
