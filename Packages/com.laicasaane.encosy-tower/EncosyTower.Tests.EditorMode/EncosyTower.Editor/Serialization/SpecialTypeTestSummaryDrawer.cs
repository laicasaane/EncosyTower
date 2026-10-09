#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.Serialization
{
    [CustomPropertyDrawer(typeof(SpecialTypeTestSummary))]
    public sealed class SpecialTypeTestSummaryDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
            => new PropertyField(property.FindPropertyRelative(nameof(SpecialTypeTestSummary.text)), string.Empty);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(
                  position: position
                , property: property.FindPropertyRelative(nameof(SpecialTypeTestSummary.text))
                , label: GUIContent.none
            );
        }
    }
}

#endif
