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
    /// <summary>
    /// Draws a field marked with <see cref="ValueChoicesAttribute"/> as a dropdown or as free input with a menu
    /// of choices.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The drawer resolves the choices once for each combination of declaring type, member name, source type,
    /// source type name, value type, <c>applyToCollection</c> and <c>IsExclusive</c>, and resolves them again only
    /// when one of these changes. When resolution fails, it logs one warning with the failure message and draws the
    /// field normally.
    /// </para>
    /// <para>
    /// A field of <see cref="Overridable{T}"/> is drawn as the row of <see cref="OverridablePropertyDrawer"/>
    /// with the choices added. Any other field is drawn as a plain field. An array or list field without
    /// <c>applyToCollection</c> is drawn by Unity, which calls this drawer for each element.
    /// </para>
    /// <para>
    /// A dropdown is used when the attribute does not set <c>applyToCollection</c> and the value type is a non-flags
    /// enum or the attribute is exclusive. Otherwise the field keeps its normal input, and a non-exclusive field
    /// gets a menu button with the choices.
    /// </para>
    /// <para>
    /// A locked field, as defined for <see cref="OverridablePropertyDrawer"/>, is disabled.
    /// A selection writes the value and applies it in one Undo step.
    /// </para>
    /// </remarks>
    [CustomPropertyDrawer(typeof(ValueChoicesAttribute))]
    public sealed class ValueChoicesPropertyDrawer : PropertyDrawer
    {
        internal const string CHOICES_TOOLTIP = "Choose from preset values (custom values allowed)";

        private ChoicesKey _choicesKey;
        private IReadOnlyList<ValueChoice> _choices;
        private ValueChoicesError _choicesError;
        private bool _hasChoiceResolution;

        private static void DrawPlainPopup(
              Rect rect
            , SerializedProperty property
            , Type valueType
            , IReadOnlyList<ValueChoice> choices
            , bool isExclusive
            , bool locked
        )
        {
            var value = SerializedValueAPI.Read(property, valueType);
            var index = ValueChoicesEditorAPI.GetChoiceIndex(choices, value);
            var mixed = property.hasMultipleDifferentValues;
            var text = index >= 0 ? choices[index].Label : ValueDisplayAPI.GetDisplayText(valueType, value);

            if (isExclusive && index < 0 && mixed == false)
            {
                rect = ImguiAPI.DrawWarning(rect, text, locked);
            }

            if (ImguiAPI.DrawPopupButton(rect, text, string.Empty, mixed))
            {
                BuildPlainMenu(property, valueType, choices).DropDown(rect);
            }
        }

        /// <summary>
        /// Builds the IMGUI menu of choices for a plain field.
        /// </summary>
        /// <remarks>
        /// A selection finds the property again by its path, and does nothing when the target is destroyed or the
        /// property is locked. Otherwise it writes the value with <see cref="SerializedValueAPI.Write"/> and
        /// applies it.
        /// </remarks>
        private static GenericMenu BuildPlainMenu(
              SerializedProperty property
            , Type valueType
            , IReadOnlyList<ValueChoice> choices
        )
        {
            var serializedObject = property.serializedObject;
            var path = property.propertyPath;
            var menu = new GenericMenu();

            var items = ValueChoicesEditorAPI.CreateChoiceMenuItems(
                  choices
                , SerializedValueAPI.Read(property, valueType)
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

                if (current == null || PropertyLockAPI.IsLocked(current))
                {
                    return;
                }

                SerializedValueAPI.Write(current, value);
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// UI Toolkit path. An array or list field without <c>applyToCollection</c> gets a <see cref="PropertyField"/>
        /// so that Unity calls this drawer for each element. A field of <see cref="Overridable{T}"/> gets an
        /// <see cref="OverridableRowBuilder"/> row with the choices. A field with resolved choices gets the plain
        /// field. When the choices cannot be resolved, the result is a <see cref="PropertyField"/>.
        /// </remarks>
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

        /// <inheritdoc/>
        /// <remarks>
        /// IMGUI path. A field of <see cref="Overridable{T}"/> has the height of its row. A resolved dropdown is
        /// one line high. A collection with <c>applyToCollection</c> is one line when collapsed, otherwise a header
        /// line plus the list height.
        /// </remarks>
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

                if (ImguiAPI.TryGetNativeList(property, out var list))
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

        /// <inheritdoc/>
        /// <remarks>
        /// IMGUI path with the same cases as <see cref="CreatePropertyGUI"/>. A non-exclusive field gets a menu
        /// button at the right edge. The method restores the indent level and the mixed-value flag even when
        /// drawing throws.
        /// </remarks>
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

            var locked = PropertyLockAPI.IsLocked(property);
            var isExclusive = ValueChoicesEditorAPI.IsExclusive(choicesAttribute);
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
                          position.xMax - ImguiAPI.MENU_WIDTH
                        , position.y
                        , ImguiAPI.MENU_WIDTH
                        , EditorGUIUtility.singleLineHeight
                    );

                    if (isExclusive == false)
                    {
                        fieldRect.xMax = Mathf.Max(fieldRect.x, menuRect.x - ImguiAPI.MENU_SPACING);
                    }

                    if (choicesAttribute.applyToCollection)
                    {
                        var sizeRect = ImguiAPI.GetCollectionSizeRect(fieldRect, indent);
                        menuRect.y = sizeRect.y;
                        menuRect.height = sizeRect.height;
                    }

                    if (ValueChoicesEditorAPI.UsesDropdown(choicesAttribute, valueType))
                    {
                        var input = EditorGUI.PrefixLabel(fieldRect, label);
                        EditorGUI.indentLevel = 0;
                        var selectableChoices = ValueChoicesEditorAPI.GetSelectableChoices(
                              choicesAttribute
                            , valueType
                            , choices
                        );

                        DrawPlainPopup(input, property, valueType, selectableChoices, isExclusive, locked);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();

                        if (choicesAttribute.applyToCollection)
                        {
                            DrawPlainCollection(position, fieldRect, property, label, indent);
                        }
                        else if (ImguiAPI.TryDrawFullWidthComposite(fieldRect, property, label) == false)
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

                    if (isExclusive == false
                        && ImguiAPI.DrawMoreButton(menuRect, locked ? string.Empty : CHOICES_TOOLTIP)
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
            var sizeRect = ImguiAPI.GetCollectionSizeRect(header, indent);
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

            if (ImguiAPI.TryGetNativeList(property, out var list))
            {
                ImguiAPI.DrawNativeList(EditorGUI.IndentedRect(body), list);
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
            , out IReadOnlyList<ValueChoice> choices
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

        /// <summary>
        /// Resolves the choices once for the current <see cref="ChoicesKey"/>.
        /// </summary>
        /// <remarks>
        /// A failed resolution is logged as a warning with the message of <see cref="ValueChoicesError"/>, and
        /// <paramref name="choices"/> is empty. The result is kept until the key changes.
        /// </remarks>
        /// <param name="valueType">The element or collection type the choices must fit.</param>
        /// <param name="choices">Receives the resolved choices.</param>
        /// <returns><see langword="true"/> when the choices were resolved.</returns>
        private bool TryResolveChoices(Type valueType, out IReadOnlyList<ValueChoice> choices)
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

        /// <summary>
        /// The UI Toolkit drawing of a field that is not an <see cref="Overridable{T}"/>.
        /// </summary>
        /// <remarks>
        /// It uses a choice field when the attribute asks for a dropdown, otherwise a <see cref="PropertyField"/>
        /// with a menu button for a non-exclusive attribute. A field on an <see cref="SerializedValueHolder"/> is
        /// locked: it is disabled and has no tooltip. Only an exclusive dropdown has a warning icon.
        /// The row reads the value again when the property changes and when it attaches to or detaches from a panel.
        /// </remarks>
        private sealed class PlainField
        {
            private readonly SerializedProperty _property;
            private readonly Type _valueType;
            private readonly IReadOnlyList<ValueChoice> _choices;
            private readonly IReadOnlyList<ValueChoice> _selectableChoices;
            private readonly bool _locked;
            private readonly ChoicePopupField _choiceField;
            private readonly Image _warning;
            private readonly Button _choicesButton;
            private readonly PropertyField _collectionField;
            private VisualElement _collectionSizeField;
            private bool _detached;

            public PlainField(
                  SerializedProperty property
                , Type valueType
                , ValueChoicesAttribute attribute
                , IReadOnlyList<ValueChoice> choices
                , string label
            )
            {
                _property = property.Copy();
                _valueType = valueType;
                _choices = choices;
                _locked = property.serializedObject.targetObject is SerializedValueHolder;

                Root = new VisualElement();
                UitkAPI.AddRowStyleSheets(Root);
                var row = new VisualElement();
                row.AddToClassList(UitkAPI.ROW_CLASS_NAME);

                if (attribute.applyToCollection)
                {
                    row.AddToClassList(UitkAPI.ROW_COLLECTION_CLASS_NAME);
                }

                Root.Add(row);
                var isExclusive = ValueChoicesEditorAPI.IsExclusive(attribute);

                if (ValueChoicesEditorAPI.UsesDropdown(attribute, valueType))
                {
                    _selectableChoices = ValueChoicesEditorAPI.GetSelectableChoices(attribute, valueType, choices);
                    var count = _selectableChoices.Count;
                    var labels = new string[count];

                    for (var i = 0; i < count; i++)
                    {
                        labels[i] = _selectableChoices[i].Label;
                    }

                    _choiceField = new ChoicePopupField(label) { HasDefaultChoice = false, Choices = labels };
                    _choiceField.AddToClassList(BaseField<int>.alignedFieldUssClassName);
                    _choiceField.RegisterValueChangedCallback(OnChoiceChanged);
                    row.Add(_choiceField);

                    if (isExclusive)
                    {
                        _warning = UitkAPI.CreateWarningIcon();
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

                if (isExclusive == false)
                {
                    _choicesButton = UitkAPI.CreateMenuButton(
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
                    UitkAPI.AlignListElementRow(row);
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
                var value = SerializedValueAPI.Read(_property, _valueType);
                var index = ValueChoicesEditorAPI.GetChoiceIndex(_selectableChoices, value);
                var isMixed = _property.hasMultipleDifferentValues;
                var text = ValueDisplayAPI.GetDisplayText(_valueType, value);
                _choiceField.UnlistedValueText = text;
                _choiceField.showMixedValue = isMixed;
                _choiceField.SetValueWithoutNotify(index);

                if (_warning != null)
                {
                    UitkAPI.RefreshWarning(_warning, index < 0 && isMixed == false, text, _locked);
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
                    sizeField?.AddToClassList(UitkAPI.PLAIN_COLLECTION_SIZE_CHOICES_CLASS_NAME);
                    sizeField?.RegisterCallback<GeometryChangedEvent>(OnCollectionGeometryChanged);
                    sizeField?.RegisterCallback<DetachFromPanelEvent>(OnCollectionSizeDetached);
                }

                UitkAPI.AlignCollectionMenu(_choicesButton, sizeField);
            }

            private void ClearCollectionSizeField()
            {
                _collectionSizeField?.RemoveFromClassList(UitkAPI.PLAIN_COLLECTION_SIZE_CHOICES_CLASS_NAME);
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
                if ((uint)evt.newValue < (uint)_selectableChoices.Count)
                {
                    ApplyChoice(_selectableChoices[evt.newValue].Value);
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

                var items = ValueChoicesEditorAPI.CreateChoiceMenuItems(
                      _choices
                    , SerializedValueAPI.Read(_property, _valueType)
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
                SerializedValueAPI.Write(_property, value);
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
