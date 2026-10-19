#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using EncosyTower.Editor.Internals;
using EncosyTower.Serialization;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Serialization.Internals
{
    /// <summary>
    /// Builds the UI Toolkit row of an <see cref="Overridable{T}"/> field.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="OverridablePropertyDrawer"/> and <see cref="ValueChoicesPropertyDrawer"/> create it.
    /// The row binds to the real value property while the field is overridden or mixed. Otherwise it binds to the
    /// property of a temporary <see cref="SerializedValueHolder"/> that holds the shown default, and the row is
    /// read-only. <see cref="Refresh"/> swaps the binding.
    /// </para>
    /// <para>
    /// <see cref="Refresh"/> runs when the property value changes, when the row attaches to a panel, when the
    /// settings asset of the default changes, on Undo or Redo for a collection, and when the default changes. A
    /// default from a <c>Settings&lt;T&gt;</c> asset is tracked through a <see cref="SerializedObject"/>. Any other
    /// default is polled every 100 ms by comparing JSON snapshots; the poll pauses while the field is overridden or
    /// mixed and while the row is detached. When the row detaches, it destroys the holder and drops the bindings.
    /// </para>
    /// <para>
    /// A dropdown row lists the default item first, with the default value in parentheses, then the choices.
    /// Its index is -1 when the state is mixed or an overridden value is not in the list. The default tooltip shows
    /// only while the default item is selected and the row is not locked. The warning icon shows only when the
    /// choices are exclusive and the value is not listed.
    /// </para>
    /// <para>
    /// The root swallows the bubbling <c>SerializedObjectBindEvent</c>, so a bind on a parent cannot rebind the
    /// child fields.
    /// </para>
    /// </remarks>
    internal sealed class OverridableRowBuilder
    {
        private const string USS_CLASS_NAME = "encosy-overridable";
        private const string DIMMED_CLASS = USS_CLASS_NAME + "__label--default";
        private const string LOCKED_BODY_CLASS = USS_CLASS_NAME + "__value--locked";
        private const string ARRAY_SIZE_PATH = "Array.size";
        private const long DEFAULT_REFRESH_INTERVAL = 100;

        private static StyleSheet s_styleSheet;
        private static StyleSheet s_darkStyleSheet;
        private static StyleSheet s_lightStyleSheet;

        private readonly SerializedProperty _property;
        private readonly SerializedProperty _valueProperty;
        private readonly SerializedProperty _isOverriddenProperty;
        private readonly Type _valueType;
        private readonly OverridableDefaultAttribute _attribute;
        private readonly IReadOnlyList<ValueChoice> _choices;
        private readonly IReadOnlyList<ValueChoice> _selectableChoices;
        private readonly bool _isExclusive;
        private readonly string _displayLabel;
        private readonly bool _isListElement;
        private readonly bool _locked;
        private readonly OverridableSwitch _switch;
        private readonly ChoicePopupField _choiceField;
        private readonly Image _warning;
        private readonly VisualElement _valueField;
        private readonly VisualElement _valueContainer;
        private readonly Label _label;
        private readonly Foldout _header;
        private readonly IntegerField _sizeField;
        private readonly Button _moreButton;
        private readonly VisualElement _settingsTracker;
        private readonly UnityEngine.Object _settingsAsset;
        private readonly IVisualElementScheduledItem _defaultRefresh;
        private readonly object _defaultSnapshotBox;
        private readonly FieldInfo _defaultSnapshotValue;

        private SerializedValueHolder _holder;
        private SerializedObject _settingsObject;
        private SerializedProperty _boundProperty;
        private Foldout _valueFoldout;
        private string _defaultSnapshot;
        private int _boundArraySize;
        private bool _boundToValue;
        private bool _hasBinding;
        private bool _hasTracking;
        private bool _detached;

        /// <summary>
        /// Creates the row of a property.
        /// </summary>
        /// <remarks>
        /// The row is a dropdown when the choices or the value type ask for one; see
        /// <see cref="ValueChoicesEditorAPI.UsesDropdown"/> and <see cref="OverridableEditorAPI.GetMode"/>.
        /// Otherwise it is an override switch with a value field. A composite or collection value gets a foldout
        /// header, and a collection gets a size field.
        /// </remarks>
        /// <param name="property">The <see cref="Overridable{T}"/> property.</param>
        /// <param name="fieldInfo">The field that holds the property.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <param name="attribute">
        /// The default source. It is ignored for an element of a collection, and it falls back to the attribute of
        /// <paramref name="fieldInfo"/> when it is <see langword="null"/>.
        /// </param>
        /// <param name="label">The row label. The display name of the property is the default.</param>
        /// <param name="choices">
        /// The resolved choices, or <see langword="null"/> when the field has none. A non-exclusive row with at least
        /// one choice gets a menu button.
        /// </param>
        /// <param name="choicesAttribute">The choices attribute; it makes the row exclusive or not.</param>
        public OverridableRowBuilder(
              SerializedProperty property
            , FieldInfo fieldInfo
            , Type valueType
            , OverridableDefaultAttribute attribute
            , string label = null
            , IReadOnlyList<ValueChoice> choices = null
            , ValueChoicesAttribute choicesAttribute = null
        )
        {
            _property = property.Copy();
            _valueProperty = _property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            _isOverriddenProperty = _property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN);
            _valueType = valueType;
            _isListElement = OverridableEditorAPI.IsOverridableListElement(property, fieldInfo);

            _attribute = _isListElement
                ? null
                : attribute ?? fieldInfo?.GetCustomAttribute<OverridableDefaultAttribute>();

            _choices = choices;
            _isExclusive = choices != null && ValueChoicesEditorAPI.IsExclusive(choicesAttribute);
            _locked = PropertyLockAPI.IsLocked(property);
            label ??= property.displayName;
            _displayLabel = label;

            Root = new RowRoot();
            AddStyleSheets(Root);

            var row = new VisualElement();
            row.AddToClassList(UitkAPI.ROW_CLASS_NAME);
            Root.Add(row);

            var usesDropdown = choices != null
                ? ValueChoicesEditorAPI.UsesDropdown(choicesAttribute, valueType)
                : OverridableEditorAPI.GetMode(valueType) == OverridableMode.Dropdown;

            if (usesDropdown)
            {
                _choiceField = new ChoicePopupField(label);
                _choiceField.AddToClassList(BaseField<int>.alignedFieldUssClassName);
                _choiceField.RegisterValueChangedCallback(OnChoiceChanged);
                _choiceField.SetEnabled(_locked == false);
                row.Add(_choiceField);

                if (choices != null)
                {
                    _selectableChoices = ValueChoicesEditorAPI.GetSelectableChoices(
                          choicesAttribute
                        , valueType
                        , choices
                    );
                }

                if (_isExclusive)
                {
                    _warning = UitkAPI.CreateWarningIcon();
                    row.Add(_warning);
                }
            }
            else
            {
                var input = new VisualElement();
                input.AddToClassList(USS_CLASS_NAME + "__input");
                var field = new RowField(label, input);
                _label = field.labelElement;
                row.Add(field);

                _switch = new OverridableSwitch();
                _switch.SetEnabled(_locked == false);
                _switch.tooltip = _locked ? string.Empty : "Override";
                _switch.RegisterValueChangedCallback(OnSwitchChanged);
                input.Add(_switch);

                _valueContainer = new VisualElement();
                _valueContainer.AddToClassList(UitkAPI.VALUE_CLASS_NAME);

                var isCollectionHeader = _valueProperty.isArray
                    && _valueProperty.propertyType != SerializedPropertyType.String;

                if (OverridableImguiRow.UsesHeader(_valueProperty, valueType))
                {
                    row.AddToClassList(USS_CLASS_NAME + "__header");
                    _label.text = string.Empty;
                    _header = new Foldout { text = label };
                    _header.AddToClassList(USS_CLASS_NAME + "__header-label");
                    _header.SetValueWithoutNotify(_valueProperty.isExpanded);
                    _header.RegisterValueChangedCallback(OnHeaderChanged);
                    _label.Add(_header);

                    var spacer = new VisualElement { pickingMode = PickingMode.Ignore };
                    spacer.AddToClassList(USS_CLASS_NAME + "__header-spacer");
                    input.Add(spacer);

                    if (isCollectionHeader)
                    {
                        _sizeField = new IntegerField { isDelayed = true };
                        _sizeField.AddToClassList(BaseListView.arraySizeFieldUssClassName);
                        _sizeField.AddToClassList(BaseListView.arraySizeFieldWithHeaderUssClassName);
                        _sizeField.AddToClassList(USS_CLASS_NAME + "__collection-size");
                        row.Add(_sizeField);
                    }

                    Root.Add(_valueContainer);
                }
                else
                {
                    input.Add(_valueContainer);
                }

                _valueField = _valueProperty.propertyType switch {
                    SerializedPropertyType.Vector4 when ImguiAPI.HasCustomDrawer(valueType) == false
                        => (VisualElement)new Vector4Field(),
                    SerializedPropertyType.Quaternion when ImguiAPI.HasCustomDrawer(valueType) == false
                        => new OverridableQuaternionField(),
                    _ => new PropertyField { label = string.Empty },
                };

                _valueField.AddToClassList(UitkAPI.VALUE_FIELD_CLASS_NAME);
                _valueContainer.Add(_valueField);

                _valueField.RegisterCallback<GeometryChangedEvent>(OnValueGeometryChanged);
            }

            if (choices?.Count > 0 && _isExclusive == false)
            {
                _moreButton = UitkAPI.CreateMenuButton(
                      _locked ? string.Empty : ValueChoicesPropertyDrawer.CHOICES_TOOLTIP
                    , _locked
                );
                _moreButton.clicked += OnMoreClicked;
                row.Add(_moreButton);
                if (_sizeField != null)
                {
                    _sizeField.AddToClassList(USS_CLASS_NAME + "__collection-size--choices");
                    _sizeField.RegisterCallback<GeometryChangedEvent>(OnCollectionGeometryChanged);
                    _moreButton.RegisterCallback<GeometryChangedEvent>(OnCollectionGeometryChanged);
                }
            }

            if (_isListElement && (_moreButton != null || _warning != null))
            {
                UitkAPI.AlignListElementRow(row);
            }

            row.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);

            _settingsAsset = OverridableEditorAPI.GetSettingsAsset(_attribute);

            if (_settingsAsset.IsValid())
            {
                _settingsTracker = new VisualElement { pickingMode = PickingMode.Ignore };
                _settingsTracker.style.display = DisplayStyle.None;
                Root.Add(_settingsTracker);
            }

            if (_attribute != null && _settingsTracker == null && _locked == false)
            {
                var boxType = typeof(SerializedValueBox<>).MakeGenericType(_valueType);
                _defaultSnapshotBox = Activator.CreateInstance(boxType);
                _defaultSnapshotValue = boxType.GetField(nameof(SerializedValueBox<int>.value));
                _defaultRefresh = Root.schedule.Execute(TickDefaultSource).Every(DEFAULT_REFRESH_INTERVAL);
                _defaultRefresh.Pause();
            }

            InitializeTracking();
            Root.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            Root.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            Refresh();
        }

        /// <summary>
        /// The root of the row. Add it to the Inspector.
        /// </summary>
        public VisualElement Root { get; }

        /// <summary>
        /// Adds the Overridable class, the theme style sheet and the shared style sheet to an element.
        /// </summary>
        /// <remarks>
        /// The theme is the dark or the light style sheet, by the Editor skin. The plain field of
        /// <see cref="ValueChoicesPropertyDrawer"/> uses it too.
        /// </remarks>
        /// <param name="root">The element that receives the style sheets.</param>
        public static void AddStyleSheets(VisualElement root)
        {
            UitkAPI.AddRowStyleSheets(root);
            root.AddToClassList(USS_CLASS_NAME);
            root.styleSheets.Add(GetThemeStyleSheet());
            root.styleSheets.Add(EditorAPI.GetOrLoadAsset(ref s_styleSheet, OverridableStyleSheetPaths.USS_PATH));
        }

        private static StyleSheet GetThemeStyleSheet()
            => EditorAPI.GetOrLoadStyleSheet(
                  ref s_darkStyleSheet
                , OverridableStyleSheetPaths.DARK_USS_PATH
                , ref s_lightStyleSheet
                , OverridableStyleSheetPaths.LIGHT_USS_PATH
            );

        /// <summary>
        /// Brings the row in line with the property and the default.
        /// </summary>
        /// <remarks>
        /// It does nothing while the row is detached or the target object is destroyed. A dropdown row refreshes its
        /// items. Otherwise it updates the switch, the dimmed label and the tooltip, and binds the value field to the
        /// real value or to the default holder; see the type remarks.
        /// </remarks>
        public void Refresh()
        {
            if (_detached || _property.serializedObject.targetObject.IsInvalid())
            {
                return;
            }

            _property.serializedObject.UpdateIfRequiredOrScript();
            UpdateDefaultRefresh();

            if (_choiceField != null)
            {
                RefreshChoices();
                return;
            }

            var isMixed = _isOverriddenProperty.hasMultipleDifferentValues;
            var useValue = _isOverriddenProperty.boolValue || isMixed;
            _switch.ShowMixedValue = isMixed;
            _switch.SetValueWithoutNotify(_isOverriddenProperty.boolValue);
            _label.EnableInClassList(DIMMED_CLASS, useValue == false);

            SerializedProperty bindingProperty;

            if (useValue)
            {
                bindingProperty = _valueProperty;
            }
            else
            {
                if (_holder.IsInvalid())
                {
                    _holder = SerializedValueHolder.Create(_valueType);
                }

                bindingProperty = _holder.Write(
                      OverridableEditorAPI.GetShownDefault(_attribute, _valueType)
                    , _valueProperty
                );
            }

            _valueContainer.EnableInClassList(LOCKED_BODY_CLASS, _sizeField != null && (useValue == false || _locked));
            _valueField.SetEnabled(useValue && _locked == false);
            _sizeField?.SetEnabled(useValue && _locked == false);

            _valueContainer.tooltip = useValue || _locked
                ? string.Empty
                : OverridableEditorAPI.GetDefaultTooltip(_attribute, _valueType);

            var arraySize = _sizeField != null ? bindingProperty.arraySize : 0;
            var collectionSizeChanged = _sizeField != null && _boundArraySize != arraySize;

            if (_hasBinding == false || _boundToValue != useValue || collectionSizeChanged)
            {
                _valueField.Unbind();
                _sizeField?.Unbind();
                _valueFoldout = null;
                _boundProperty = bindingProperty;
                _boundArraySize = arraySize;
                _boundToValue = useValue;
                ((IBindable)_valueField).bindingPath = bindingProperty.propertyPath;

                if (_sizeField != null)
                {
                    _sizeField.bindingPath = bindingProperty.FindPropertyRelative(ARRAY_SIZE_PATH).propertyPath;
                }

                if (Root.panel != null)
                {
                    _hasBinding = true;
                    ((IBindable)_valueField).BindProperty(bindingProperty);

                    if (_sizeField != null)
                    {
                        _sizeField.BindProperty(bindingProperty.FindPropertyRelative(ARRAY_SIZE_PATH));
                    }
                }
            }

            RestoreInlineLabelWidths();
            NormalizeValueLayout();
            SynchronizeHeader();
        }

        /// <summary>
        /// Gets the items of the menu of choices.
        /// </summary>
        /// <returns>
        /// No items when the row has no choices, is exclusive, is locked or is disabled. Otherwise one item for each
        /// choice, checked against the current value, which is the shown default while not overridden. An item
        /// applies the choice and turns the override on.
        /// </returns>
        public ValueChoiceMenuItem[] GetChoiceMenuItems()
        {
            if (_choices == null || _isExclusive || _locked || Root.enabledInHierarchy == false)
            {
                return Array.Empty<ValueChoiceMenuItem>();
            }

            _property.serializedObject.UpdateIfRequiredOrScript();

            var currentValue = _isOverriddenProperty.boolValue
                ? SerializedValueAPI.Read(_valueProperty, _valueType)
                : OverridableEditorAPI.GetShownDefault(_attribute, _valueType);

            return ValueChoicesEditorAPI.CreateChoiceMenuItems(
                  _choices
                , currentValue
                , OverridableEditorAPI.IsMixed(_property)
                , ApplyPreset
            );
        }

        public GenericMenu BuildChoiceMenu()
        {
            var menu = new GenericMenu();
            var items = GetChoiceMenuItems();
            var count = items.Length;

            for (var i = 0; i < count; i++)
            {
                var item = items[i];
                menu.AddItem(item.Content, item.IsChecked, item.Callback);
            }

            return menu;
        }

        /// <summary>
        /// Builds the context menu that opens on a right click.
        /// </summary>
        /// <param name="shiftPressed">Whether Shift was held when the menu opened.</param>
        /// <returns>
        /// An empty menu when the row is locked, detached, disabled or destroyed; otherwise the result of
        /// <c>OverridableEditorAPI.BuildPropertyContextMenu</c>.
        /// </returns>
        public GenericMenu BuildContextMenu(bool shiftPressed = false)
        {
            if (_locked
                || _detached
                || Root.enabledInHierarchy == false
                || _property.serializedObject.targetObject.IsInvalid()
                || PropertyLockAPI.IsLocked(_property)
            )
            {
                return new GenericMenu();
            }

            return OverridableEditorAPI.BuildPropertyContextMenu(_property, LocateDefaultSource, shiftPressed);
        }

        /// <summary>
        /// Runs one poll of a default that is not a settings asset.
        /// </summary>
        /// <remarks>
        /// It refreshes the row when the JSON snapshot of the default changed. It pauses the poll while the field is
        /// overridden or mixed. The scheduler calls it; it is public for tests.
        /// </remarks>
        public void TickDefaultSource()
        {
            if (_defaultRefresh == null
                || _detached
                || Root.panel == null
                || _property.serializedObject.targetObject.IsInvalid()
            )
            {
                return;
            }

            _property.serializedObject.UpdateIfRequiredOrScript();

            if (_isOverriddenProperty.boolValue || _isOverriddenProperty.hasMultipleDifferentValues)
            {
                _defaultRefresh.Pause();
                return;
            }

            if (string.Equals(_defaultSnapshot, GetDefaultSnapshot(), StringComparison.Ordinal) == false)
            {
                Refresh();
            }
        }

        private void UpdateDefaultRefresh()
        {
            if (_defaultRefresh == null)
            {
                return;
            }

            if (Root.panel == null
                || _isOverriddenProperty.boolValue
                || _isOverriddenProperty.hasMultipleDifferentValues
            )
            {
                _defaultRefresh.Pause();
                return;
            }

            _defaultSnapshot = GetDefaultSnapshot();
            _defaultRefresh.Resume();
        }

        private string GetDefaultSnapshot()
        {
            var shownDefault = OverridableEditorAPI.GetShownDefault(_attribute, _valueType);
            _defaultSnapshotValue.SetValue(_defaultSnapshotBox, shownDefault);
            return EditorJsonUtility.ToJson(_defaultSnapshotBox);
        }

        private void OnMoreClicked()
        {
            if (_locked || _detached || Root.enabledInHierarchy == false)
            {
                return;
            }

            BuildChoiceMenu().DropDown(_moreButton.worldBound);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button != (int)MouseButton.RightMouse)
            {
                return;
            }

            evt.StopPropagation();
            var menu = BuildContextMenu(evt.shiftKey);

            if (menu.GetItemCount() > 0)
            {
                menu.DropDown(new Rect(evt.position, Vector2.zero));
            }
        }

        private void LocateDefaultSource()
        {
            if (_isListElement)
            {
                var path = _property.propertyPath;
                var arrayIndex = path.LastIndexOf(".Array.data[", StringComparison.Ordinal);
                var collectionPath = path[..arrayIndex];
                var outerPath = collectionPath[..collectionPath.LastIndexOf('.')];
                var outerProperty = _property.serializedObject.FindProperty(outerPath);
                var elementIndex = int.Parse(path[(path.LastIndexOf('[') + 1)..^1]);

                var message = OverridableEditorAPI.GetElementNoDefaultMessage(
                      outerProperty.displayName
                    , _valueType
                    , elementIndex
                );

                EditorUtility.DisplayDialog(title: "Default Value", message: message, ok: "OK");
                return;
            }

            OverridableEditorAPI.Locate(_attribute, _valueType, _displayLabel);
        }

        private void RefreshChoices()
        {
            if (_selectableChoices != null)
            {
                RefreshValueChoices();
                return;
            }

            var values = ValueDisplayAPI.GetValueChoices(_valueType);
            var choices = new string[values.Length + 1];
            choices[0] = OverridableEditorAPI.GetDefaultChoiceLabel(_attribute, _valueType);
            Array.Copy(values, 0, choices, 1, values.Length);

            var index = OverridableEditorAPI.GetSelectedIndex(_property, _valueType);
            _choiceField.Choices = choices;
            _choiceField.showMixedValue = OverridableEditorAPI.IsMixed(_property);
            _choiceField.SetValueWithoutNotify(index);

            _choiceField.InputTooltip = index == 0 && _locked == false
                ? OverridableEditorAPI.GetDefaultTooltip(_attribute, _valueType)
                : string.Empty;
        }

        private void RefreshValueChoices()
        {
            var count = _selectableChoices.Count;
            var labels = new string[count + 1];
            labels[0] = GetDefaultChoiceLabel();

            for (var i = 0; i < count; i++)
            {
                labels[i + 1] = _selectableChoices[i].Label;
            }

            var value = SerializedValueAPI.Read(_valueProperty, _valueType);
            var valueIndex = ValueChoicesEditorAPI.GetChoiceIndex(_selectableChoices, value);
            var isOverridden = _isOverriddenProperty.boolValue;
            var isMixed = OverridableEditorAPI.IsMixed(_property);
            var index = 0;

            if (isMixed || (isOverridden && valueIndex < 0))
            {
                index = -1;
            }
            else if (isOverridden)
            {
                index = valueIndex + 1;
            }

            var text = ValueDisplayAPI.GetDisplayText(_valueType, value);
            _choiceField.Choices = labels;
            _choiceField.UnlistedValueText = text;
            _choiceField.showMixedValue = isMixed;
            _choiceField.SetValueWithoutNotify(index);

            _choiceField.InputTooltip = index == 0 && _locked == false
                ? OverridableEditorAPI.GetDefaultTooltip(_attribute, _valueType)
                : string.Empty;

            if (_warning != null)
            {
                var isUnlisted = isOverridden && isMixed == false && valueIndex < 0;
                UitkAPI.RefreshWarning(_warning, isUnlisted, text, _locked);
            }
        }

        private string GetDefaultChoiceLabel()
        {
            var label = OverridableEditorAPI.GetDefaultLabel(_attribute);

            if (OverridableEditorAPI.TryGetDefaultValue(_attribute, out var value) == false)
            {
                return label;
            }

            var index = ValueChoicesEditorAPI.GetChoiceIndex(_choices, value);

            var text = index >= 0
                ? _choices[index].Label
                : ValueDisplayAPI.GetDisplayText(_valueType, value);

            return $"{label} ({text})";
        }

        private void ApplyPreset(object value)
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
            SerializedValueAPI.Write(_valueProperty, value);
            _isOverriddenProperty.boolValue = true;
            _property.serializedObject.ApplyModifiedProperties();
            Refresh();
        }

        private void OnHeaderChanged(ChangeEvent<bool> evt)
        {
            if (evt.target != _header)
            {
                return;
            }

            _valueProperty.isExpanded = evt.newValue;

            if (_boundProperty != null)
            {
                _boundProperty.isExpanded = evt.newValue;
            }

            if (_valueFoldout != null)
            {
                _valueFoldout.value = evt.newValue;
            }

            SynchronizeHeader();
            evt.StopPropagation();
        }

        private void OnValueGeometryChanged(GeometryChangedEvent evt)
        {
            RestoreInlineLabelWidths();
            NormalizeValueLayout();
            SynchronizeHeader();
        }

        private void RestoreInlineLabelWidths()
        {
            if (_header != null || _detached)
            {
                return;
            }

            Restore(_valueField);

            static void Restore(VisualElement element)
            {
                if (element.ClassListContains(BaseField<int>.alignedFieldUssClassName)
                    || element.ClassListContains(UitkAPI.LOCAL_LABEL_CLASS_NAME)
                )
                {
                    UitkAPI.ConfigureLocalLabel(element);
                }

                var hierarchy = element.hierarchy;
                var count = hierarchy.childCount;

                for (var i = 0; i < count; i++)
                {
                    Restore(hierarchy[i]);
                }
            }
        }

        private void NormalizeValueLayout()
        {
            if (_detached == false)
            {
                UitkAPI.NormalizeValueLayout(_valueField);
            }
        }

        private void SynchronizeHeader()
        {
            if (_header == null || _detached || _boundProperty == null)
            {
                return;
            }

            var expanded = _valueProperty.isExpanded;
            _header.SetValueWithoutNotify(expanded);
            _valueContainer.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;

            if (_sizeField != null)
            {
                var listView = _valueField.Q<ListView>("unity-list-" + _boundProperty.propertyPath);

                if (listView != null)
                {
                    listView.showBoundCollectionSize = false;

                    if (listView.showFoldoutHeader)
                    {
                        listView.showFoldoutHeader = false;
                    }
                }

                return;
            }

            _valueFoldout = _valueField.Q<Foldout>("unity-foldout-" + _boundProperty.propertyPath);

            if (_valueFoldout == null)
            {
                return;
            }

            var toggle = _valueFoldout.Q<Toggle>(className: Foldout.toggleUssClassName);

            if (toggle != null)
            {
                toggle.style.display = DisplayStyle.None;
            }

            _valueFoldout.value = expanded;
        }

        private void InitializeTracking()
        {
            if (_hasTracking)
            {
                return;
            }

            _hasTracking = true;
            Root.TrackPropertyValue(_property, OnPropertyChanged);

            if (_settingsTracker != null && _settingsAsset.IsValid())
            {
                _settingsObject = new SerializedObject(_settingsAsset);
                _settingsTracker.TrackSerializedObjectValue(_settingsObject, OnSettingsChanged);
            }
        }

        private void OnSwitchChanged(ChangeEvent<bool> evt)
        {
            if (_locked || Root.enabledInHierarchy == false)
            {
                return;
            }

            _property.serializedObject.UpdateIfRequiredOrScript();

            if (evt.newValue)
            {
                OverridableEditorAPI.BeginOverride(_property, _attribute, _valueType);
            }
            else
            {
                OverridableEditorAPI.EndOverride(_property);
            }

            Refresh();
            evt.StopPropagation();
        }

        private void OnChoiceChanged(ChangeEvent<int> evt)
        {
            if (_locked || Root.enabledInHierarchy == false)
            {
                return;
            }

            if (_selectableChoices != null)
            {
                _property.serializedObject.UpdateIfRequiredOrScript();

                if (evt.newValue == 0)
                {
                    OverridableEditorAPI.EndOverride(_property);
                }
                else if ((uint)(evt.newValue - 1) < (uint)_selectableChoices.Count)
                {
                    ApplyPreset(_selectableChoices[evt.newValue - 1].Value);
                }
            }
            else
            {
                OverridableEditorAPI.ApplyChoice(_property, _valueType, evt.newValue);
            }

            Refresh();
            evt.StopPropagation();
        }

        private void OnPropertyChanged(SerializedProperty property)
        {
            Refresh();
        }

        private void OnSettingsChanged(SerializedObject serializedObject)
        {
            Refresh();
        }

        private void OnCollectionGeometryChanged(GeometryChangedEvent evt)
        {
            UitkAPI.AlignCollectionMenu(_moreButton, _sizeField);
        }

        private void OnUndoRedoPerformed()
        {
            Refresh();
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (evt.target != Root)
            {
                return;
            }

            _detached = false;

            if (_sizeField != null)
            {
                Undo.undoRedoPerformed += OnUndoRedoPerformed;
            }

            InitializeTracking();
            Refresh();
            UitkAPI.AlignCollectionMenu(_moreButton, _sizeField);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (evt.target != Root)
            {
                return;
            }

            _detached = true;
            _defaultRefresh?.Pause();

            if (_sizeField != null)
            {
                Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            }

            _valueFoldout = null;
            Root.Unbind();
            _hasTracking = false;
            _hasBinding = false;
            _boundProperty = null;
            _settingsObject?.Dispose();
            _settingsObject = null;

            if (_holder.IsValid())
            {
                UnityEngine.Object.DestroyImmediate(_holder);
                _holder = null;
            }
        }

        private sealed class RowRoot : VisualElement
        {
            private const string SERIALIZED_OBJECT_BIND_EVENT = "SerializedObjectBindEvent";

            protected override void HandleEventBubbleUp(EventBase evt)
            {
                if (string.Equals(evt.GetType().Name, SERIALIZED_OBJECT_BIND_EVENT, StringComparison.Ordinal))
                {
                    evt.StopPropagation();
                    return;
                }

                base.HandleEventBubbleUp(evt);
            }
        }

        private sealed class RowField : BaseField<bool>
        {
            public RowField(string label, VisualElement input) : base(label, input)
            {
                AddToClassList(alignedFieldUssClassName);
                AddToClassList(USS_CLASS_NAME + "__field");
                focusable = false;
            }
        }
    }
}

#endif
