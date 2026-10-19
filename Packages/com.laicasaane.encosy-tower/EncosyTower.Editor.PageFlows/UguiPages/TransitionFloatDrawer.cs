#if UNITY_EDITOR && UNITY_UGUI

using System.Reflection;
using EncosyTower.Editor.UIElements;
using EncosyTower.PageFlows.UguiPages.Transitions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UguiPages
{
    /// <summary>
    /// Draws a <see cref="TransitionFloat"/> as a Start and End pair of float fields on one row.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The drawer also applies to types derived from <see cref="TransitionFloat"/>. It has a UI Toolkit path
    /// (<see cref="CreatePropertyGUI"/>) and an IMGUI path (<see cref="OnGUI"/>). Both forward the text of a
    /// <see cref="TooltipAttribute"/> on the field to the label.
    /// </para>
    /// <para>
    /// The Start and End labels come from the text resources of the editor module.
    /// </para>
    /// </remarks>
    [CustomPropertyDrawer(typeof(TransitionFloat), true)]
    public class TransitionFloatDrawer : PropertyDrawer
    {
        private static readonly GUIContent s_labelStart = new();
        private static readonly GUIContent s_labelEnd = new();

        /// <inheritdoc/>
        /// <remarks>
        /// Returns a field that holds the Start and End float fields. The float fields bind to the properties by
        /// path, so Undo works through the binding.
        /// </remarks>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var startProperty = property.FindPropertyRelative(nameof(TransitionFloat.start));
            var endProperty = property.FindPropertyRelative(nameof(TransitionFloat.end));

            var input = new VisualElement();
            input.style.flexDirection = FlexDirection.Row;

            var field = new TransitionFloatField(property.displayName, input);
            var tooltipAttrib = this.fieldInfo.GetCustomAttribute<TooltipAttribute>(true);

            if (tooltipAttrib != null)
            {
                field.tooltip = tooltipAttrib.tooltip;
            }

            var texts = PageFlowsViewResources.Get().Transition;

            input.Add(CreateFloatField(texts.Start, startProperty, 45f));
            input.Add(CreateFloatField(texts.End, endProperty, 40f));

            var container = new PropertyContainer(property, false);
            container.Add(field);
            return container;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The IMGUI path. It writes the float values straight to the properties, and the inspector that hosts the
        /// drawer applies them. It sets the tooltip of <paramref name="label"/> when the field has a
        /// <see cref="TooltipAttribute"/>.
        /// </remarks>
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var texts = PageFlowsViewResources.Get().Transition;
            s_labelStart.text = texts.Start;
            s_labelEnd.text = texts.End;

            var startProperty = property.FindPropertyRelative(nameof(TransitionFloat.start));
            var endProperty = property.FindPropertyRelative(nameof(TransitionFloat.end));

            var tooltipAttrib = this.fieldInfo.GetCustomAttribute<TooltipAttribute>(true);

            if (tooltipAttrib != null)
            {
                label.tooltip = tooltipAttrib.tooltip;
            }

            rect = EditorGUI.PrefixLabel(rect, label);

            rect.x -= 14f;
            rect.width += 14f;
            rect.width *= 0.5f;

            var rect1 = rect;
            var rect2 = rect;
            rect2.x += rect.width;

            DrawField(rect1, 45f, s_labelStart, startProperty);
            DrawField(rect2, 40f, s_labelEnd, endProperty);
        }

        private static FloatField CreateFloatField(string label, SerializedProperty property, float labelWidth)
        {
            var field = new FloatField(label) { bindingPath = property.propertyPath };
            field.style.flexBasis = 0;
            field.style.flexGrow = 1;
            field.style.minWidth = 0;
            field.labelElement.style.width = labelWidth;
            field.labelElement.style.minWidth = 0;
            field.labelElement.style.flexShrink = 0;
            return field;
        }

        private void DrawField(Rect rect, float labelWidth, GUIContent label, SerializedProperty property)
        {
            var rect1 = rect;
            rect1.width = labelWidth;

            var rect2 = rect;
            rect2.width -= labelWidth;
            rect2.x += labelWidth;

            EditorGUI.LabelField(rect1, label, EditorStyles.label);
            property.floatValue = EditorGUI.FloatField(rect2, property.floatValue);
        }

        private sealed class TransitionFloatField : BaseField<bool>
        {
            public TransitionFloatField(string label, VisualElement input) : base(label, input)
            {
                AddToClassList(alignedFieldUssClassName);
                focusable = false;
            }
        }
    }
}

#endif
