#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor.Internals;
using EncosyTower.Editor.Serialization.Internals;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor
{
    [CustomPropertyDrawer(typeof(StringEnum<>), true)]
    [CustomPropertyDrawer(typeof(StringEnum<,>), true)]
    public sealed class StringEnumPropertyDrawer : PropertyDrawer
    {
        private const string UNRESOLVED_TOOLTIP = "The allowed values of this field cannot be resolved.";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var label = preferredLabel ?? property.displayName;

            if (TryResolve(out var choices, out var error) == false)
            {
                using var rawValue = property.FindPropertyRelative(StringEnumEditorAPI.VALUE);
                var root = new VisualElement();
                UitkAPI.AddRowStyleSheets(root);

                var row = new VisualElement();
                row.AddToClassList(UitkAPI.ROW_CLASS_NAME);
                root.Add(row);

                var textField = new TextField(label);
                textField.AddToClassList(BaseField<string>.alignedFieldUssClassName);
                textField.BindProperty(rawValue.Copy());
                row.Add(textField);
                row.Add(UitkAPI.CreateErrorIcon(error));
                return root;
            }

            return new Field(property, choices, label).Root;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using var valueProperty = property.FindPropertyRelative(StringEnumEditorAPI.VALUE);

            if (TryResolve(out var choices, out var error) == false)
            {
                var broken = EditorGUI.PrefixLabel(position, label);
                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                broken = ImguiAPI.DrawError(broken, error);
                EditorGUI.PropertyField(broken, valueProperty, GUIContent.none);
                EditorGUI.indentLevel = indentLevel;
                return;
            }

            var stored = valueProperty.stringValue;
            var isMixed = valueProperty.hasMultipleDifferentValues;
            var locked = PropertyLockAPI.IsLocked(property);
            var previousMixed = EditorGUI.showMixedValue;
            var indent = EditorGUI.indentLevel;

            label = EditorGUI.BeginProperty(position, label, property);

            try
            {
                var rect = EditorGUI.PrefixLabel(position, label);
                EditorGUI.indentLevel = 0;
                EditorGUI.showMixedValue = isMixed;

                using (new EditorGUI.DisabledScope(locked))
                {
                    if (isMixed == false && StringEnumEditorAPI.IsUnlisted(choices, stored))
                    {
                        rect = ImguiAPI.DrawWarning(rect, stored, locked);
                    }

                    var text = StringEnumEditorAPI.GetShownText(choices, stored);

                    if (ImguiAPI.DrawPopupButton(rect, text, string.Empty, isMixed))
                    {
                        BuildMenu(property, choices, stored, isMixed).DropDown(rect);
                    }
                }
            }
            finally
            {
                EditorGUI.showMixedValue = previousMixed;
                EditorGUI.indentLevel = indent;
                EditorGUI.EndProperty();
            }
        }

        private static GenericMenu BuildMenu(
              SerializedProperty property
            , IReadOnlyList<ValueChoice> choices
            , string stored
            , bool isMixed
        )
        {
            var items = StringEnumEditorAPI.CreateMenuItems(
                  property.serializedObject
                , property.propertyPath
                , choices
                , stored
                , isMixed
            );

            var menu = new GenericMenu();
            var count = items.Length;

            for (var i = 0; i < count; i++)
            {
                var item = items[i];
                menu.AddItem(item.Content, item.IsChecked, item.Callback);
            }

            return menu;
        }

        private bool TryResolve(out IReadOnlyList<ValueChoice> choices)
            => TryResolve(out choices, out _);

        private bool TryResolve(out IReadOnlyList<ValueChoice> choices, out string error)
        {
            choices = null;
            error = UNRESOLVED_TOOLTIP;

            return StringEnumEditorAPI.TryGetStructType(fieldInfo, out var structType)
                && StringEnumEditorAPI.TryGetChoices(structType, out choices, out error);
        }

        private sealed class Field
        {
            private readonly SerializedProperty _valueProperty;
            private readonly SerializedObject _serializedObject;
            private readonly string _structPath;
            private readonly IReadOnlyList<ValueChoice> _choices;
            private readonly ChoicePopupField _choiceField;
            private readonly Image _warning;
            private readonly bool _locked;
            private bool _detached;

            public Field(SerializedProperty property, IReadOnlyList<ValueChoice> choices, string label)
            {
                _serializedObject = property.serializedObject;
                _structPath = property.propertyPath;
                using var value = property.FindPropertyRelative(StringEnumEditorAPI.VALUE);
                _valueProperty = value.Copy();
                _choices = choices;
                _locked = PropertyLockAPI.IsLocked(property);

                Root = new VisualElement();
                UitkAPI.AddRowStyleSheets(Root);

                var row = new VisualElement();
                row.AddToClassList(UitkAPI.ROW_CLASS_NAME);
                Root.Add(row);

                var count = choices.Count;
                var labels = new string[count];

                for (var i = 0; i < count; i++)
                {
                    labels[i] = choices[i].Label;
                }

                _choiceField = new ChoicePopupField(label) { HasDefaultChoice = false, Choices = labels };
                _choiceField.AddToClassList(BaseField<int>.alignedFieldUssClassName);
                _choiceField.RegisterValueChangedCallback(OnChoiceChanged);
                row.Add(_choiceField);

                _warning = UitkAPI.CreateWarningIcon();
                row.Add(_warning);

                if (_structPath.EndsWith("]", StringComparison.Ordinal))
                {
                    UitkAPI.AlignListElementRow(row);
                }

                Root.SetEnabled(_locked == false);
                Root.TrackPropertyValue(_valueProperty, OnPropertyChanged);
                Root.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
                Root.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
                Refresh();
            }

            public VisualElement Root { get; }

            private void Refresh()
            {
                if (_detached || _serializedObject.targetObject.IsInvalid())
                {
                    return;
                }

                _serializedObject.UpdateIfRequiredOrScript();

                var stored = _valueProperty.stringValue;
                var isMixed = _valueProperty.hasMultipleDifferentValues;

                _choiceField.UnlistedValueText = StringEnumEditorAPI.GetShownText(_choices, stored);
                _choiceField.showMixedValue = isMixed;
                _choiceField.SetValueWithoutNotify(StringEnumEditorAPI.GetChoiceIndex(_choices, stored));

                UitkAPI.RefreshWarning(
                      _warning
                    , isMixed == false && StringEnumEditorAPI.IsUnlisted(_choices, stored)
                    , stored
                    , _locked
                );
            }

            private void OnChoiceChanged(ChangeEvent<int> evt)
            {
                if ((uint)evt.newValue < (uint)_choices.Count)
                {
                    StringEnumEditorAPI.TryWrite(_serializedObject, _structPath, (string)_choices[evt.newValue].Value);
                    Refresh();
                }

                evt.StopPropagation();
            }

            private void OnPropertyChanged(SerializedProperty property)
            {
                Refresh();
            }

            private void OnAttachToPanel(AttachToPanelEvent evt)
            {
                _detached = false;
                Refresh();
            }

            private void OnDetachFromPanel(DetachFromPanelEvent evt)
            {
                _detached = true;
            }
        }
    }
}

#endif
