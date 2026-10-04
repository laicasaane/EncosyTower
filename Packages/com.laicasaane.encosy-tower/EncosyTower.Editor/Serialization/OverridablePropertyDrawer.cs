#if UNITY_EDITOR

using System;
using System.Reflection;
using EncosyTower.Serialization;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Serialization
{
    [CustomPropertyDrawer(typeof(Overridable<>))]
    internal sealed class OverridablePropertyDrawer : PropertyDrawer
    {
        private const string USS_CLASS_NAME = "encosy-overridable";
        private const string ROW_USS_CLASS_NAME = USS_CLASS_NAME + "__row";
        private const string OVERRIDE_USS_CLASS_NAME = USS_CLASS_NAME + "__override";
        private const string RESET_USS_CLASS_NAME = USS_CLASS_NAME + "__reset";
        private const string EFFECTIVE_USS_CLASS_NAME = USS_CLASS_NAME + "__effective";
        private const string RESET_TEXT = "Reset";
        private const string OVERRIDE_TEXT = "Override";
        private const float RESET_WIDTH = 48f;
        private const float OVERRIDE_WIDTH = 70f;
        private const float SPACING = 4f;

        private static readonly GUIContent s_resetContent = new(RESET_TEXT, "Use the default value.");
        private static readonly GUIContent s_overrideContent = new(OVERRIDE_TEXT);

        private static StyleSheet s_styleSheet;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var valueType = OverridableDrawerModel.GetValueType(fieldInfo.FieldType);
            var attribute = fieldInfo.GetCustomAttribute<OverridableDefaultAttribute>();
            var isOverriddenProperty = property.FindPropertyRelative(OverridableDrawerModel.IS_OVERRIDDEN);
            var valueProperty = property.FindPropertyRelative(OverridableDrawerModel.VALUE);

            var root = new VisualElement();
            root.AddToClassList(USS_CLASS_NAME);
            root.styleSheets.Add(EditorAPI.GetOrLoadAsset(ref s_styleSheet, OverridableStyleSheetPaths.USS_PATH));

            var row = new VisualElement();
            row.AddToClassList(ROW_USS_CLASS_NAME);
            root.Add(row);

            var effective = new Label();
            effective.AddToClassList(EFFECTIVE_USS_CLASS_NAME);

            var reset = new Button(OnReset) { text = RESET_TEXT, tooltip = s_resetContent.tooltip };
            reset.AddToClassList(RESET_USS_CLASS_NAME);

            OverridableChoiceField choiceField = null;
            PropertyField valueField = null;
            VisualElement alignedField;

            if (OverridableDrawerModel.GetMode(valueType) == OverridableMode.Dropdown)
            {
                choiceField = new OverridableChoiceField(preferredLabel) {
                    Choices = GetChoices(valueType, attribute),
                };

                choiceField.AddToClassList(BaseField<int>.alignedFieldUssClassName);
                choiceField.RegisterValueChangedCallback(OnChoiceChanged);
                row.Add(choiceField);
                alignedField = choiceField;
            }
            else
            {
                valueField = new PropertyField(valueProperty, preferredLabel);
                row.Add(valueField);

                var overrideToggle = new Toggle { text = OVERRIDE_TEXT };
                overrideToggle.AddToClassList(OVERRIDE_USS_CLASS_NAME);
                overrideToggle.BindProperty(isOverriddenProperty);
                row.Add(overrideToggle);
                alignedField = valueField;
            }

            row.Add(reset);
            root.Add(effective);

            alignedField.RegisterCallback<GeometryChangedEvent>(OnAlignedFieldGeometryChanged);
            root.TrackPropertyValue(property, OnPropertyChanged);

            var settingsAsset = OverridableDrawerModel.GetSettingsAsset(attribute);

            if (settingsAsset.IsValid())
            {
                root.TrackSerializedObjectValue(new SerializedObject(settingsAsset), OnSettingsChanged);
            }

            Refresh();
            return root;

            void OnReset()
            {
                OverridableDrawerModel.Reset(property);
            }

            void OnChoiceChanged(ChangeEvent<int> evt)
            {
                OverridableDrawerModel.ApplyChoice(property, valueType, evt.newValue);
            }

            void OnPropertyChanged(SerializedProperty _)
            {
                Refresh();
            }

            void OnSettingsChanged(SerializedObject _)
            {
                Refresh();
            }

            void OnAlignedFieldGeometryChanged(GeometryChangedEvent _)
            {
                var input = alignedField.Q(className: BaseField<int>.inputUssClassName);

                if (input != null)
                {
                    effective.style.marginLeft = input.worldBound.xMin - root.worldBound.xMin;
                }
            }

            void Refresh()
            {
                property.serializedObject.UpdateIfRequiredOrScript();

                var isMixed = OverridableDrawerModel.IsMixed(property);

                if (choiceField != null)
                {
                    choiceField.showMixedValue = isMixed;
                    choiceField.SetValueWithoutNotify(OverridableDrawerModel.GetSelectedIndex(property, valueType));
                }

                valueField?.SetEnabled(OverridableDrawerModel.CanReset(property));
                reset.SetEnabled(OverridableDrawerModel.CanReset(property));
                effective.text = OverridableDrawerModel.GetEffectiveText(property, attribute, valueType);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valueType = OverridableDrawerModel.GetValueType(fieldInfo.FieldType);
            var lineHeight = EditorGUIUtility.singleLineHeight;

            if (OverridableDrawerModel.GetMode(valueType) == OverridableMode.Toggle)
            {
                var valueProperty = property.FindPropertyRelative(OverridableDrawerModel.VALUE);
                lineHeight = Mathf.Max(lineHeight, EditorGUI.GetPropertyHeight(valueProperty, GUIContent.none, true));
            }

            return lineHeight + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueType = OverridableDrawerModel.GetValueType(fieldInfo.FieldType);
            var attribute = fieldInfo.GetCustomAttribute<OverridableDefaultAttribute>();
            var isOverriddenProperty = property.FindPropertyRelative(OverridableDrawerModel.IS_OVERRIDDEN);
            var valueProperty = property.FindPropertyRelative(OverridableDrawerModel.VALUE);
            var mode = OverridableDrawerModel.GetMode(valueType);

            EditorGUI.BeginProperty(position, label, property);

            var firstLineHeight = position.height - EditorGUIUtility.standardVerticalSpacing
                - EditorGUIUtility.singleLineHeight;

            var firstLine = new Rect(position.x, position.y, position.width, firstLineHeight);
            var inputRect = EditorGUI.PrefixLabel(firstLine, GUIUtility.GetControlID(FocusType.Passive), label);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var resetRect = new Rect(inputRect.xMax - RESET_WIDTH, inputRect.y, RESET_WIDTH, lineHeight);
            var fieldWidth = inputRect.width - RESET_WIDTH - SPACING;
            var fieldRect = new Rect(inputRect.x, inputRect.y, fieldWidth, inputRect.height);

            if (mode == OverridableMode.Dropdown)
            {
                DrawPopup(fieldRect, property, valueType, attribute);
            }
            else
            {
                DrawToggleAndValue(fieldRect, isOverriddenProperty, valueProperty);
            }

            using (new EditorGUI.DisabledScope(OverridableDrawerModel.CanReset(property) == false))
            {
                if (GUI.Button(resetRect, s_resetContent))
                {
                    OverridableDrawerModel.Reset(property);
                }
            }

            var effectiveRect = new Rect(
                  inputRect.x
                , firstLine.yMax + EditorGUIUtility.standardVerticalSpacing
                , position.xMax - inputRect.x
                , EditorGUIUtility.singleLineHeight
            );

            EditorGUI.LabelField(
                  effectiveRect
                , OverridableDrawerModel.GetEffectiveText(property, attribute, valueType)
                , EditorStyles.miniLabel
            );

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private static void DrawPopup(
              Rect rect
            , SerializedProperty property
            , Type valueType
            , OverridableDefaultAttribute attribute
        )
        {
            var choices = GetChoices(valueType, attribute);
            var options = new GUIContent[choices.Length + 1];
            options[0] = new GUIContent(choices[0]);
            options[1] = GUIContent.none;

            for (var i = 1; i < choices.Length; i++)
            {
                options[i + 1] = new GUIContent(choices[i]);
            }

            var selectedIndex = OverridableDrawerModel.GetSelectedIndex(property, valueType);
            var displayIndex = selectedIndex <= 0 ? selectedIndex : selectedIndex + 1;
            var showMixedValue = EditorGUI.showMixedValue;

            EditorGUI.showMixedValue = selectedIndex < 0;
            EditorGUI.BeginChangeCheck();

            var newDisplayIndex = EditorGUI.Popup(rect, displayIndex, options);

            if (EditorGUI.EndChangeCheck() && newDisplayIndex != 1)
            {
                var choiceIndex = newDisplayIndex <= 0 ? 0 : newDisplayIndex - 1;
                OverridableDrawerModel.ApplyChoice(property, valueType, choiceIndex);
            }

            EditorGUI.showMixedValue = showMixedValue;
        }

        private static void DrawToggleAndValue(
              Rect rect
            , SerializedProperty isOverriddenProperty
            , SerializedProperty valueProperty
        )
        {
            var valueRect = new Rect(rect.x, rect.y, rect.width - OVERRIDE_WIDTH - SPACING, rect.height);
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var toggleRect = new Rect(valueRect.xMax + SPACING, rect.y, OVERRIDE_WIDTH, lineHeight);
            var isEnabled = isOverriddenProperty.boolValue || isOverriddenProperty.hasMultipleDifferentValues;

            using (new EditorGUI.DisabledScope(isEnabled == false))
            {
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none, true);
            }

            EditorGUI.BeginChangeCheck();

            var showMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = isOverriddenProperty.hasMultipleDifferentValues;

            var isOverridden = EditorGUI.ToggleLeft(toggleRect, s_overrideContent, isOverriddenProperty.boolValue);

            EditorGUI.showMixedValue = showMixedValue;

            if (EditorGUI.EndChangeCheck())
            {
                isOverriddenProperty.boolValue = isOverridden;
            }
        }

        private static string[] GetChoices(Type valueType, OverridableDefaultAttribute attribute)
        {
            var values = OverridableDrawerModel.GetValueChoices(valueType);
            var choices = new string[values.Length + 1];
            choices[0] = OverridableDrawerModel.GetDefaultLabel(attribute);
            Array.Copy(values, 0, choices, 1, values.Length);
            return choices;
        }
    }
}

#endif
