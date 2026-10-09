#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using EncosyTower.Editor.Internals;
using EncosyTower.Editor.Serialization.Internals;
using EncosyTower.Logging;
using EncosyTower.Serialization;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor
{
    [CustomPropertyDrawer(typeof(ValueChoicesAttribute))]
    public sealed class ValueChoicesPropertyDrawer : PropertyDrawer
    {
        internal const string CHOICES_TOOLTIP = "Choose from preset values (custom values allowed)";

        private ChoicesKey _choicesKey;
        private IReadOnlyList<(string label, object value)> _choices;
        private ValueChoicesError _choicesError;
        private bool _hasChoiceResolution;

        public static IReadOnlyList<(string label, object value)> GetPopupChoices(
              ValueChoicesAttribute attribute
            , Type valueType
            , IReadOnlyList<(string label, object value)> choices
        )
        {
            if (ValueChoicesEditorAPI.ListsOnlyChoices(attribute, valueType))
            {
                return choices;
            }

            var values = Enum.GetValues(valueType);
            var labels = OverridableEditorAPI.GetValueChoices(valueType);
            var count = values.Length;
            var result = new (string label, object value)[count];

            for (var i = 0; i < count; i++)
            {
                result[i] = (labels[i], values.GetValue(i));
            }

            return result;
        }

        public static int GetChoiceIndex(IReadOnlyList<(string label, object value)> choices, object value)
        {
            var count = choices.Count;

            for (var i = 0; i < count; i++)
            {
                if (ValueChoicesEditorAPI.Matches(choices[i].value, value))
                {
                    return i;
                }
            }

            return -1;
        }

        public static object ReadValue(SerializedProperty property, Type valueType)
            => SerializedValueCopy.Read(property, valueType);

        public static void WriteValue(SerializedProperty property, object value)
            => SerializedValueCopy.Write(property, value);

        public static (GUIContent Content, bool IsChecked, GenericMenu.MenuFunction Callback)[] CreateMenuItems(
              IReadOnlyList<(string label, object value)> choices
            , object currentValue
            , bool isMixed
            , Action<object> onSelected
        )
        {
            var count = choices.Count;
            var result = new (GUIContent Content, bool IsChecked, GenericMenu.MenuFunction Callback)[count];

            for (var i = 0; i < count; i++)
            {
                var choice = choices[i];
                var isChecked = isMixed == false && ValueChoicesEditorAPI.Matches(choice.value, currentValue);
                result[i] = (new GUIContent(choice.label), isChecked, SelectChoice);

                void SelectChoice()
                {
                    onSelected(choice.value);
                }
            }

            return result;
        }

        public static Image CreateWarningIcon()
        {
            var iconName = EditorGUIUtility.isProSkin ? "d_console.warnicon" : "console.warnicon";
            var icon = new Image { image = EditorGUIUtility.IconContent(iconName).image };
            icon.AddToClassList("encosy-overridable__warning");
            icon.style.display = DisplayStyle.None;
            return icon;
        }

        public static void RefreshWarning(Image icon, bool isUnlisted, string valueText, bool locked)
        {
            icon.style.display = isUnlisted ? DisplayStyle.Flex : DisplayStyle.None;
            icon.tooltip = isUnlisted && locked == false
                ? $"\"{valueText}\" is not one of the allowed values."
                : string.Empty;
        }

        private static void DrawPlainPopup(
              Rect rect
            , SerializedProperty property
            , Type valueType
            , IReadOnlyList<(string label, object value)> choices
            , bool onlyChoices
            , bool locked
        )
        {
            var value = ReadValue(property, valueType);
            var index = GetChoiceIndex(choices, value);
            var mixed = property.hasMultipleDifferentValues;
            var text = index >= 0 ? choices[index].label : OverridableEditorAPI.GetDisplayText(valueType, value);

            if (onlyChoices && index < 0 && mixed == false)
            {
                rect = OverridableImguiRow.DrawWarning(rect, text, locked);
            }

            if (OverridableImguiRow.DrawPopupButton(rect, text, string.Empty, mixed))
            {
                BuildPlainMenu(property, valueType, choices).DropDown(rect);
            }
        }

        private static GenericMenu BuildPlainMenu(
              SerializedProperty property
            , Type valueType
            , IReadOnlyList<(string label, object value)> choices
        )
        {
            var serializedObject = property.serializedObject;
            var path = property.propertyPath;
            var menu = new GenericMenu();

            var items = CreateMenuItems(
                  choices
                , ReadValue(property, valueType)
                , property.hasMultipleDifferentValues
                , Apply
            );

            var count = items.Length;

            for (var i = 0; i < count; i++)
            {
                var item = items[i];
                menu.AddItem(item.Content, item.IsChecked, item.Callback);
            }

            return menu;

            void Apply(object value)
            {
                if (serializedObject.targetObject.IsInvalid())
                {
                    return;
                }

                serializedObject.UpdateIfRequiredOrScript();
                var current = serializedObject.FindProperty(path);

                if (current == null || OverridableImguiRow.IsLocked(current))
                {
                    return;
                }

                WriteValue(current, value);
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var label = preferredLabel ?? property.displayName;
            var choicesAttribute = (ValueChoicesAttribute)attribute;
            var targetsCollection = choicesAttribute.applyToCollection;

            if (property.isArray
                && property.propertyType != SerializedPropertyType.String
                && targetsCollection == false
            )
            {
                return new PropertyField(property, label);
            }

            var fieldType = fieldInfo.FieldType;

            if (targetsCollection == false && fieldType.IsArray)
            {
                fieldType = fieldType.GetElementType();
            }
            else if (targetsCollection == false
                && fieldType.IsGenericType
                && fieldType.GetGenericTypeDefinition() == typeof(List<>)
            )
            {
                fieldType = fieldType.GetGenericArguments()[0];
            }

            var isOverridable = fieldType.IsGenericType
                && fieldType.GetGenericTypeDefinition() == typeof(Overridable<>)
                ;

            var valueType = isOverridable ? OverridableEditorAPI.GetValueType(fieldInfo, property) : fieldType;

            var resolved = TryResolveChoices(valueType, out var choices);

            if (isOverridable)
            {
                var builder = new OverridableRowBuilder(
                      property
                    , fieldInfo
                    , valueType
                    , fieldInfo.GetCustomAttribute<OverridableDefaultAttribute>()
                    , label
                    , resolved ? choices : null
                    , choicesAttribute
                );

                return builder.Root;
            }

            return resolved
                ? new PlainField(property, valueType, choicesAttribute, choices, label).Root
                : new PropertyField(property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var resolved = TryResolveImgui(property, out var valueType, out var isOverridable, out var choices);

            if (isOverridable)
            {
                return OverridableImguiRow.Get(property, fieldInfo).GetHeight(
                      resolved ? choices : null
                    , (ValueChoicesAttribute)attribute
                );
            }

            if (resolved && ValueChoicesEditorAPI.UsesDropdown((ValueChoicesAttribute)attribute, valueType))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (resolved && ((ValueChoicesAttribute)attribute).applyToCollection)
            {
                var height = EditorGUIUtility.singleLineHeight;

                if (property.isExpanded == false)
                {
                    return height;
                }

                if (OverridableImguiRow.TryGetNativeList(property, out var list))
                {
                    return height + EditorGUIUtility.standardVerticalSpacing + list.GetHeight();
                }

                var count = property.arraySize;

                for (var i = 0; i < count; i++)
                {
                    using var element = property.GetArrayElementAtIndex(i);
                    height += EditorGUIUtility.standardVerticalSpacing
                        + EditorGUI.GetPropertyHeight(property: element, includeChildren: true);
                }

                return height;
            }

            return EditorGUI.GetPropertyHeight(property: property, label: label, includeChildren: true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = new GUIContent(label ?? GUIContent.none);
            var resolved = TryResolveImgui(property, out var valueType, out var isOverridable, out var choices);
            var choicesAttribute = (ValueChoicesAttribute)attribute;

            if (isOverridable)
            {
                OverridableImguiRow.Get(property, fieldInfo).Draw(
                      position
                    , label
                    , resolved ? choices : null
                    , choicesAttribute
                );

                return;
            }

            if (resolved == false)
            {
                EditorGUI.PropertyField(position: position, property: property, label: label, includeChildren: true);
                return;
            }

            var locked = OverridableImguiRow.IsLocked(property);
            var onlyChoices = ValueChoicesEditorAPI.ListsOnlyChoices(choicesAttribute, valueType);
            var previousMixed = EditorGUI.showMixedValue;
            var indent = EditorGUI.indentLevel;
            label = locked ? new GUIContent(label.text) : label;
            label = BeginPlainProperty(position, property, label, choicesAttribute.applyToCollection);

            try
            {
                using (new EditorGUI.DisabledScope(locked))
                {
                    var fieldRect = position;
                    var menuRect = new Rect(
                          position.xMax - OverridableImguiRow.MENU_WIDTH
                        , position.y
                        , OverridableImguiRow.MENU_WIDTH
                        , EditorGUIUtility.singleLineHeight
                    );

                    if (onlyChoices == false)
                    {
                        fieldRect.xMax = Mathf.Max(fieldRect.x, menuRect.x - OverridableImguiRow.MENU_SPACING);
                    }

                    if (choicesAttribute.applyToCollection)
                    {
                        var sizeRect = OverridableImguiRow.GetCollectionSizeRect(fieldRect, indent);
                        menuRect.y = sizeRect.y;
                        menuRect.height = sizeRect.height;
                    }

                    if (ValueChoicesEditorAPI.UsesDropdown(choicesAttribute, valueType))
                    {
                        var input = EditorGUI.PrefixLabel(fieldRect, label);
                        EditorGUI.indentLevel = 0;
                        var popupChoices = GetPopupChoices(choicesAttribute, valueType, choices);
                        DrawPlainPopup(input, property, valueType, popupChoices, onlyChoices, locked);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();

                        if (choicesAttribute.applyToCollection)
                        {
                            DrawPlainCollection(position, fieldRect, property, label, indent);
                        }
                        else if (OverridableImguiRow.TryDrawFullWidthComposite(fieldRect, property, label) == false)
                        {
                            EditorGUI.PropertyField(
                                  position: fieldRect
                                , property: property
                                , label: label
                                , includeChildren: true
                            );
                        }

                        if (EditorGUI.EndChangeCheck() && property.serializedObject.hasModifiedProperties)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }

                    if (onlyChoices == false
                        && OverridableImguiRow.DrawMoreButton(menuRect, locked ? string.Empty : CHOICES_TOOLTIP)
                    )
                    {
                        BuildPlainMenu(property, valueType, choices).DropDown(menuRect);
                    }
                }
            }
            finally
            {
                EditorGUI.indentLevel = indent;
                EditorGUI.showMixedValue = previousMixed;
                EditorGUI.EndProperty();
            }
        }

        private static void DrawPlainCollection(
              Rect position
            , Rect header
            , SerializedProperty property
            , GUIContent label
            , int indent
        )
        {
            header.height = EditorGUIUtility.singleLineHeight;
            var sizeRect = OverridableImguiRow.GetCollectionSizeRect(header, indent);
            var foldout = header;
            foldout.xMax = sizeRect.xMin;
            property.isExpanded = EditorGUI.Foldout(
                  position: foldout
                , foldout: property.isExpanded
                , content: label
                , toggleOnLabelClick: true
            );
            using var size = property.FindPropertyRelative("Array.size");
            EditorGUI.showMixedValue = size.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            var count = EditorGUI.DelayedIntField(sizeRect, size.intValue);

            if (EditorGUI.EndChangeCheck())
            {
                size.intValue = Mathf.Max(0, count);
            }

            EditorGUI.showMixedValue = false;

            if (property.isExpanded == false)
            {
                return;
            }

            var body = position;
            body.yMin = header.yMax + EditorGUIUtility.standardVerticalSpacing;

            if (OverridableImguiRow.TryGetNativeList(property, out var list))
            {
                OverridableImguiRow.DrawNativeList(EditorGUI.IndentedRect(body), list);
                return;
            }

            EditorGUI.indentLevel = indent + 1;
            count = property.arraySize;

            for (var i = 0; i < count; i++)
            {
                using var element = property.GetArrayElementAtIndex(i);
                body.height = EditorGUI.GetPropertyHeight(property: element, includeChildren: true);
                EditorGUI.PropertyField(position: body, property: element, includeChildren: true);
                body.y += body.height + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel = indent;
        }

        internal static GUIContent BeginPlainProperty(
              Rect position
            , SerializedProperty property
            , GUIContent label
            , bool useDisplayName
        )
        {
            if (useDisplayName && string.IsNullOrEmpty(label?.text))
            {
                label = new GUIContent(property.displayName);
            }

            return new GUIContent(EditorGUI.BeginProperty(position, label, property));
        }

        private bool TryResolveImgui(
              SerializedProperty property
            , out Type valueType
            , out bool isOverridable
            , out IReadOnlyList<(string label, object value)> choices
        )
        {
            valueType = fieldInfo.FieldType;
            isOverridable = false;
            choices = null;
            var choicesAttribute = (ValueChoicesAttribute)attribute;
            var targetsCollection = choicesAttribute.applyToCollection;

            if (property.isArray
                && property.propertyType != SerializedPropertyType.String
                && targetsCollection == false
            )
            {
                return false;
            }

            if (targetsCollection == false && valueType.IsArray)
            {
                valueType = valueType.GetElementType();
            }
            else if (targetsCollection == false
                && valueType.IsGenericType
                && valueType.GetGenericTypeDefinition() == typeof(List<>)
            )
            {
                valueType = valueType.GetGenericArguments()[0];
            }

            isOverridable = valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Overridable<>);

            if (isOverridable)
            {
                valueType = OverridableEditorAPI.GetValueType(fieldInfo, property);
            }

            return TryResolveChoices(valueType, out choices);
        }

        private bool TryResolveChoices(Type valueType, out IReadOnlyList<(string label, object value)> choices)
        {
            var choicesAttribute = (ValueChoicesAttribute)attribute;

            var key = new ChoicesKey(
                  fieldInfo.DeclaringType
                , choicesAttribute.MemberName
                , choicesAttribute.SourceType
                , choicesAttribute.SourceTypeName
                , valueType
                , choicesAttribute.applyToCollection
                , choicesAttribute.IsExclusive
            );

            if (_hasChoiceResolution == false || _choicesKey != key)
            {
                var resolved = ValueChoicesEditorAPI.TryGetChoices(
                      choicesAttribute
                    , fieldInfo.DeclaringType
                    , valueType
                    , out _choices
                    , out var error
                );

                _choicesKey = key;
                _choicesError = error;
                _hasChoiceResolution = true;

                if (resolved == false)
                {
                    StaticDevLogger.LogWarning(error.ToString());
                }
            }

            choices = _choices;
            return _choicesError.Is(ValueChoicesError.Type.Undefined);
        }

        private readonly record struct ChoicesKey(
              Type DeclaringType
            , string MemberName
            , Type SourceType
            , string SourceTypeName
            , Type ValueType
            , bool ApplyToCollection
            , bool IsExclusive
        );

        private sealed class PlainField
        {
            private readonly SerializedProperty _property;
            private readonly Type _valueType;
            private readonly IReadOnlyList<(string label, object value)> _choices;
            private readonly IReadOnlyList<(string label, object value)> _popupChoices;
            private readonly bool _locked;
            private readonly OverridableChoiceField _choiceField;
            private readonly Image _warning;
            private readonly Button _choicesButton;
            private readonly PropertyField _collectionField;
            private VisualElement _collectionSizeField;
            private bool _detached;

            public PlainField(
                  SerializedProperty property
                , Type valueType
                , ValueChoicesAttribute attribute
                , IReadOnlyList<(string label, object value)> choices
                , string label
            )
            {
                _property = property.Copy();
                _valueType = valueType;
                _choices = choices;
                _locked = property.serializedObject.targetObject is OverridableDefaultHolder;

                Root = new VisualElement();
                OverridableRowBuilder.AddStyleSheets(Root);
                var row = new VisualElement();
                row.AddToClassList(OverridableRowBuilder.ROW_CLASS_NAME);

                if (attribute.applyToCollection)
                {
                    row.AddToClassList("encosy-overridable__row--collection");
                }

                Root.Add(row);
                var onlyChoices = ValueChoicesEditorAPI.ListsOnlyChoices(attribute, valueType);

                if (ValueChoicesEditorAPI.UsesDropdown(attribute, valueType))
                {
                    _popupChoices = GetPopupChoices(attribute, valueType, choices);
                    var count = _popupChoices.Count;
                    var labels = new string[count];

                    for (var i = 0; i < count; i++)
                    {
                        labels[i] = _popupChoices[i].label;
                    }

                    _choiceField = new OverridableChoiceField(label) { HasDefaultChoice = false, Choices = labels };
                    _choiceField.AddToClassList(BaseField<int>.alignedFieldUssClassName);
                    _choiceField.RegisterValueChangedCallback(OnChoiceChanged);
                    row.Add(_choiceField);

                    if (onlyChoices)
                    {
                        _warning = CreateWarningIcon();
                        row.Add(_warning);
                    }
                }
                else
                {
                    var field = new PropertyField(_property, label);
                    row.Add(field);

                    if (attribute.applyToCollection)
                    {
                        _collectionField = field;
                        field.RegisterCallback<GeometryChangedEvent>(OnCollectionGeometryChanged);
                    }
                }

                if (onlyChoices == false)
                {
                    _choicesButton = OverridableRowBuilder.CreateMenuButton(
                          _locked ? string.Empty : CHOICES_TOOLTIP
                        , _locked
                    );

                    _choicesButton.clicked += OnChoicesClicked;
                    row.Add(_choicesButton);

                    if (_collectionField != null)
                    {
                        _choicesButton.RegisterCallback<GeometryChangedEvent>(OnCollectionGeometryChanged);
                    }
                }

                if (attribute.applyToCollection == false
                    && property.propertyPath.EndsWith("]", StringComparison.Ordinal)
                    && (_choicesButton != null || _warning != null)
                )
                {
                    OverridableRowBuilder.AlignListElementRow(row);
                }

                Root.SetEnabled(_locked == false);
                Root.TrackPropertyValue(_property, OnPropertyChanged);
                Root.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
                Root.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
                Refresh();
            }

            public VisualElement Root { get; }

            private void Refresh()
            {
                RefreshCollectionAlignment();

                if (_choiceField == null || _detached || _property.serializedObject.targetObject.IsInvalid())
                {
                    return;
                }

                _property.serializedObject.UpdateIfRequiredOrScript();
                var value = ReadValue(_property, _valueType);
                var index = GetChoiceIndex(_popupChoices, value);
                var isMixed = _property.hasMultipleDifferentValues;
                var text = OverridableEditorAPI.GetDisplayText(_valueType, value);
                _choiceField.UnlistedValueText = text;
                _choiceField.showMixedValue = isMixed;
                _choiceField.SetValueWithoutNotify(index);

                if (_warning != null)
                {
                    RefreshWarning(_warning, index < 0 && isMixed == false, text, _locked);
                }
            }

            private void RefreshCollectionAlignment()
            {
                if (_detached || _collectionField == null || _choicesButton == null)
                {
                    return;
                }

                var list = _collectionField.Q<BaseListView>();
                VisualElement sizeField = null;

                if (list != null)
                {
                    var hierarchy = list.hierarchy;
                    var count = hierarchy.childCount;

                    for (var i = 0; i < count; i++)
                    {
                        var child = hierarchy[i];

                        if (child.ClassListContains(BaseListView.arraySizeFieldUssClassName))
                        {
                            sizeField = child;
                            break;
                        }
                    }
                }

                if (_collectionSizeField != sizeField)
                {
                    ClearCollectionSizeField();
                    _collectionSizeField = sizeField;
                    sizeField?.AddToClassList("encosy-overridable__plain-collection-size--choices");
                    sizeField?.RegisterCallback<GeometryChangedEvent>(OnCollectionGeometryChanged);
                    sizeField?.RegisterCallback<DetachFromPanelEvent>(OnCollectionSizeDetached);
                }

                OverridableRowBuilder.AlignCollectionMenu(_choicesButton, sizeField);
            }

            private void ClearCollectionSizeField()
            {
                _collectionSizeField?.RemoveFromClassList("encosy-overridable__plain-collection-size--choices");
                _collectionSizeField?.UnregisterCallback<GeometryChangedEvent>(OnCollectionGeometryChanged);
                _collectionSizeField?.UnregisterCallback<DetachFromPanelEvent>(OnCollectionSizeDetached);
                _collectionSizeField = null;
            }

            private void OnCollectionGeometryChanged(GeometryChangedEvent evt)
            {
                RefreshCollectionAlignment();
            }

            private void OnCollectionSizeDetached(DetachFromPanelEvent evt)
            {
                ClearCollectionSizeField();

                if (_detached == false)
                {
                    Root.schedule.Execute(RefreshCollectionAlignment);
                }
            }

            private void OnChoiceChanged(ChangeEvent<int> evt)
            {
                if ((uint)evt.newValue < (uint)_popupChoices.Count)
                {
                    ApplyChoice(_popupChoices[evt.newValue].value);
                }

                evt.StopPropagation();
            }

            private void OnChoicesClicked()
            {
                if (_locked
                    || _detached
                    || Root.enabledInHierarchy == false
                    || _property.serializedObject.targetObject.IsInvalid()
                )
                {
                    return;
                }

                _property.serializedObject.UpdateIfRequiredOrScript();

                var items = CreateMenuItems(
                      _choices
                    , ReadValue(_property, _valueType)
                    , _property.hasMultipleDifferentValues
                    , ApplyChoice
                );

                var menu = new GenericMenu();
                var count = items.Length;

                for (var i = 0; i < count; i++)
                {
                    var item = items[i];
                    menu.AddItem(item.Content, item.IsChecked, item.Callback);
                }

                menu.DropDown(_choicesButton.worldBound);
            }

            private void ApplyChoice(object value)
            {
                if (_locked
                    || _detached
                    || Root.enabledInHierarchy == false
                    || _property.serializedObject.targetObject.IsInvalid()
                )
                {
                    return;
                }

                _property.serializedObject.UpdateIfRequiredOrScript();
                WriteValue(_property, value);
                _property.serializedObject.ApplyModifiedProperties();
                Refresh();
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
                ClearCollectionSizeField();
            }
        }
    }
}

#endif
