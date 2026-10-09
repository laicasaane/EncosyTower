#if UNITY_EDITOR

using System.Reflection;
using EncosyTower.Core;
using EncosyTower.Editor.Serialization.Internals;
using EncosyTower.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Serialization
{
    [ApiForEditor]
    [CustomPropertyDrawer(typeof(Overridable<>))]
    public sealed class OverridablePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var valueType = OverridableEditorAPI.GetValueType(fieldInfo, property);
            var attribute = fieldInfo.GetCustomAttribute<OverridableDefaultAttribute>();
            var builder = new OverridableRowBuilder(property, fieldInfo, valueType, attribute, preferredLabel);
            return builder.Root;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => OverridableImguiRow.Get(property, fieldInfo).GetHeight();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OverridableImguiRow.Get(property, fieldInfo).Draw(position, label);
        }
    }
}

#endif
