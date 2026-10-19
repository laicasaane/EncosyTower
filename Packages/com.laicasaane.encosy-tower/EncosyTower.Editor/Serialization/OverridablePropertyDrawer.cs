#if UNITY_EDITOR

using System.Reflection;
using EncosyTower.Editor.Serialization.Internals;
using EncosyTower.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Serialization
{
    /// <summary>
    /// Draws an <see cref="Overridable{T}"/> field as a row with an override switch and its value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the switch is off, the row shows the resolved default source dimmed and read-only, with a tooltip
    /// that names the default value and where it comes from. Without an <see cref="OverridableDefaultAttribute"/>,
    /// the default is the type default. Turning the switch on copies the shown default into the value and sets
    /// <c>isOverridden</c> in one Undo step; turning it off clears <c>isOverridden</c> and keeps the value.
    /// </para>
    /// <para>
    /// A non-flags enum uses a dropdown instead of a switch. Its first item selects the default source and shows
    /// the default value in parentheses; the enum values follow after a separator.
    /// </para>
    /// <para>
    /// The row context menu is Unity's property menu plus "Locate Default Source". It opens the Project Settings
    /// entry or the script that declares the default member, or shows a dialog when there is no such place.
    /// Selecting several objects shows a mixed state when their values differ.
    /// </para>
    /// <para>
    /// A row is locked, which means read-only and without tooltip or menu, when it is drawn from a temporary default
    /// holder, or when it is nested in the value of an <see cref="Overridable{T}"/> that is not overridden.
    /// </para>
    /// <para>
    /// A field that also has a <see cref="ValueChoicesAttribute"/> is drawn by
    /// <see cref="ValueChoicesPropertyDrawer"/>, which builds the same row with a menu of choices.
    /// </para>
    /// <para>
    /// The UI Toolkit path builds a new row for every call. The IMGUI path keeps one row for each serialized object
    /// and property path, and clears them on assembly reload and when the selection changes.
    /// </para>
    /// </remarks>
    [CustomPropertyDrawer(typeof(Overridable<>))]
    public sealed class OverridablePropertyDrawer : PropertyDrawer
    {
        /// <inheritdoc/>
        /// <remarks>
        /// Returns the root of a new UI Toolkit row for the property.
        /// </remarks>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var valueType = OverridableEditorAPI.GetValueType(fieldInfo, property);
            var attribute = fieldInfo.GetCustomAttribute<OverridableDefaultAttribute>();
            var builder = new OverridableRowBuilder(property, fieldInfo, valueType, attribute, preferredLabel);
            return builder.Root;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// IMGUI path. Returns the height of the cached row for the property.
        /// </remarks>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => OverridableImguiRow.Get(property, fieldInfo).GetHeight();

        /// <inheritdoc/>
        /// <remarks>
        /// IMGUI path. Draws the cached row for the property.
        /// </remarks>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OverridableImguiRow.Get(property, fieldInfo).Draw(position, label);
        }
    }
}

#endif
