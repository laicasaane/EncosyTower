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
    internal sealed class OverridableRowBuilder
    {
        public const string ROW_CLASS_NAME = "encosy-overridable__row";
        public const string VALUE_FIELD_CLASS_NAME = "encosy-overridable__value-field";

        private const string USS_CLASS_NAME = "encosy-overridable";
        private const string DIMMED_CLASS = USS_CLASS_NAME + "__label--default";
        private const string LOCKED_BODY_CLASS = USS_CLASS_NAME + "__value--locked";
        private const string LOCAL_LABEL_CLASS = USS_CLASS_NAME + "__local-label";
        private const string LIST_LAYOUT_CLASS = USS_CLASS_NAME + "__list-layout";
        private const string IMGUI_LAYOUT_CLASS = USS_CLASS_NAME + "__imgui-layout";
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
        private readonly IReadOnlyList<(string label, object value)> _choices;
        private readonly IReadOnlyList<(string label, object value)> _popupChoices;
        private readonly bool _listsOnlyChoices;
        private readonly string _displayLabel;
        private readonly bool _isListElement;
        private readonly bool _locked;
        private readonly OverridableSwitch _switch;
        private readonly OverridableChoiceField _choiceField;
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

        private OverridableDefaultHolder _holder;
        private SerializedObject _settingsObject;
        private SerializedProperty _boundProperty;
        private Foldout _valueFoldout;
        private string _defaultSnapshot;
        private int _boundArraySize;
        private bool _boundToValue;
        private bool _hasBinding;
        private bool _hasTracking;
        private bool _detached;

        public OverridableRowBuilder(
              SerializedProperty property
            , FieldInfo fieldInfo
            , Type valueType
            , OverridableDefaultAttribute attribute
            , string label = null
            , IReadOnlyList<(string label, object value)> choices = null
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
            _listsOnlyChoices = choices != null && ValueChoicesEditorAPI.ListsOnlyChoices(choicesAttribute, valueType);
            _locked = OverridableImguiRow.IsLocked(property);
            label ??= property.displayName;
            _displayLabel = label;

            Root = new RowRoot();
            AddStyleSheets(Root);

            var row = new VisualElement();
            row.AddToClassList(ROW_CLASS_NAME);
            Root.Add(row);

            var usesDropdown = choices != null
                ? ValueChoicesEditorAPI.UsesDropdown(choicesAttribute, valueType)
                : OverridableEditorAPI.GetMode(valueType) == OverridableMode.Dropdown;

            if (usesDropdown)
            {
                _choiceField = new OverridableChoiceField(label);
                _choiceField.AddToClassList(BaseField<int>.alignedFieldUssClassName);
                _choiceField.RegisterValueChangedCallback(OnChoiceChanged);
                _choiceField.SetEnabled(_locked == false);
                row.Add(_choiceField);

                if (choices != null)
                {
                    _popupChoices = ValueChoicesPropertyDrawer.GetPopupChoices(choicesAttribute, valueType, choices);
                }

                if (_listsOnlyChoices)
                {
                    _warning = ValueChoicesPropertyDrawer.CreateWarningIcon();
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
                _valueContainer.AddToClassList(USS_CLASS_NAME + "__value");

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
                    SerializedPropertyType.Vector4 when OverridableImguiRow.HasCustomDrawer(valueType) == false
                        => (VisualElement)new Vector4Field(),
                    SerializedPropertyType.Quaternion when OverridableImguiRow.HasCustomDrawer(valueType) == false
                        => new OverridableQuaternionField(),
                    _ => new PropertyField { label = string.Empty },
                };

                _valueField.AddToClassList(VALUE_FIELD_CLASS_NAME);
                _valueContainer.Add(_valueField);

                _valueField.RegisterCallback<GeometryChangedEvent>(OnValueGeometryChanged);
            }

            if (choices?.Count > 0 && _listsOnlyChoices == false)
            {
                _moreButton = CreateMenuButton(
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
                AlignListElementRow(row);
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
                var boxType = typeof(OverridableDefaultBox<>).MakeGenericType(_valueType);
                _defaultSnapshotBox = Activator.CreateInstance(boxType);
                _defaultSnapshotValue = boxType.GetField(nameof(OverridableDefaultBox<int>.value));
                _defaultRefresh = Root.schedule.Execute(TickDefaultSource).Every(DEFAULT_REFRESH_INTERVAL);
                _defaultRefresh.Pause();
            }

            InitializeTracking();
            Root.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            Root.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            Refresh();
        }

        public VisualElement Root { get; }

        public static void AddStyleSheets(VisualElement root)
        {
            root.AddToClassList(USS_CLASS_NAME);
            root.styleSheets.Add(GetThemeStyleSheet());
            root.styleSheets.Add(EditorAPI.GetOrLoadAsset(ref s_styleSheet, OverridableStyleSheetPaths.USS_PATH));
            root.RegisterCallback<GeometryChangedEvent>(OnValueSubtreeGeometryChanged);
        }

        public static void AlignListElementRow(VisualElement row)
        {
            _ = new ListElementAlignment(row);
        }

        public static Button CreateMenuButton(string tooltip, bool locked)
        {
            var button = new Button { tooltip = tooltip };
            button.AddToClassList(USS_CLASS_NAME + "__more");
            button.SetEnabled(locked == false);

            var iconName = EditorGUIUtility.isProSkin ? "d__menu@2x" : "_menu@2x";

            var icon = new Image {
                image = EditorGUIUtility.IconContent(iconName).image,
                pickingMode = PickingMode.Ignore,
            };

            icon.AddToClassList(USS_CLASS_NAME + "__more-icon");
            button.Add(icon);
            return button;
        }

        public static void AlignCollectionMenu(Button button, VisualElement sizeField)
        {
            var parent = button?.hierarchy.parent;

            if (parent == null
                || sizeField?.panel == null
                || button.panel != sizeField.panel
                || sizeField.layout.height <= 0f
                || button.layout.height <= 0f
            )
            {
                return;
            }

            var center = parent.WorldToLocal(sizeField.worldBound.center).y;
            var offset = center - button.layout.center.y;

            if (float.IsNaN(offset) || float.IsInfinity(offset))
            {
                return;
            }

            if (Mathf.Approximately(button.style.translate.value.y.value, offset) == false)
            {
                button.style.translate = new Translate(x: 0f, y: offset, z: 0f);
            }
        }

        private static StyleSheet GetThemeStyleSheet()
            => EditorAPI.GetOrLoadStyleSheet(
                  ref s_darkStyleSheet
                , OverridableStyleSheetPaths.DARK_USS_PATH
                , ref s_lightStyleSheet
                , OverridableStyleSheetPaths.LIGHT_USS_PATH
            );

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
                    _holder = OverridableDefaultHolder.Create(_valueType);
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

        public (GUIContent Content, bool IsChecked, GenericMenu.MenuFunction Callback)[] GetChoiceMenuItems()
        {
            if (_choices == null || _listsOnlyChoices || _locked || Root.enabledInHierarchy == false)
            {
                return Array.Empty<(GUIContent, bool, GenericMenu.MenuFunction)>();
            }

            _property.serializedObject.UpdateIfRequiredOrScript();

            var currentValue = _isOverriddenProperty.boolValue
                ? ValueChoicesPropertyDrawer.ReadValue(_valueProperty, _valueType)
                : OverridableEditorAPI.GetShownDefault(_attribute, _valueType);

            return ValueChoicesPropertyDrawer.CreateMenuItems(
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

        public GenericMenu BuildContextMenu(bool shiftPressed = false)
        {
            if (_locked
                || _detached
                || Root.enabledInHierarchy == false
                || _property.serializedObject.targetObject.IsInvalid()
                || OverridableImguiRow.IsLocked(_property)
            )
            {
                return new GenericMenu();
            }

            return OverridableEditorAPI.BuildPropertyContextMenu(_property, LocateDefaultSource, shiftPressed);
        }

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
            if (_popupChoices != null)
            {
                RefreshValueChoices();
                return;
            }

            var values = OverridableEditorAPI.GetValueChoices(_valueType);
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
            var count = _popupChoices.Count;
            var labels = new string[count + 1];
            labels[0] = GetDefaultChoiceLabel();

            for (var i = 0; i < count; i++)
            {
                labels[i + 1] = _popupChoices[i].label;
            }

            var value = ValueChoicesPropertyDrawer.ReadValue(_valueProperty, _valueType);
            var valueIndex = ValueChoicesPropertyDrawer.GetChoiceIndex(_popupChoices, value);
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

            var text = OverridableEditorAPI.GetDisplayText(_valueType, value);
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
                ValueChoicesPropertyDrawer.RefreshWarning(_warning, isUnlisted, text, _locked);
            }
        }

        private string GetDefaultChoiceLabel()
        {
            var label = OverridableEditorAPI.GetDefaultLabel(_attribute);

            if (OverridableEditorAPI.TryGetDefaultValue(_attribute, out var value) == false)
            {
                return label;
            }

            var index = ValueChoicesPropertyDrawer.GetChoiceIndex(_choices, value);

            var text = index >= 0
                ? _choices[index].label
                : OverridableEditorAPI.GetDisplayText(_valueType, value);

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
            ValueChoicesPropertyDrawer.WriteValue(_valueProperty, value);
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
                    || element.ClassListContains(LOCAL_LABEL_CLASS)
                )
                {
                    ConfigureLocalLabel(element);
                }

                var hierarchy = element.hierarchy;
                var count = hierarchy.childCount;

                for (var i = 0; i < count; i++)
                {
                    Restore(hierarchy[i]);
                }
            }
        }

        private static void ConfigureLocalLabel(VisualElement field)
        {
            field.AddToClassList(LOCAL_LABEL_CLASS);
            field.RemoveFromClassList(BaseField<int>.alignedFieldUssClassName);
            field.RegisterCallback<GeometryChangedEvent>(OnLocalFieldGeometryChanged);
            field.RegisterCallback<CustomStyleResolvedEvent>(OnLocalFieldStyleResolved);
            RestoreLocalLabelWidth(field);
        }

        private static void OnLocalFieldGeometryChanged(GeometryChangedEvent evt)
        {
            RestoreLocalLabelWidth((VisualElement)evt.currentTarget);
        }

        private static void OnLocalFieldStyleResolved(CustomStyleResolvedEvent evt)
        {
            RestoreLocalLabelWidth((VisualElement)evt.currentTarget);
        }

        private static void OnLocalLabelGeometryChanged(GeometryChangedEvent evt)
        {
            var field = ((VisualElement)evt.currentTarget).hierarchy.parent;

            if (field != null)
            {
                RestoreLocalLabelWidth(field);
            }
        }

        private static void RestoreLocalLabelWidth(VisualElement field)
        {
            field.RemoveFromClassList(BaseField<int>.alignedFieldUssClassName);
            var hierarchy = field.hierarchy;
            var count = hierarchy.childCount;

            for (var i = 0; i < count; i++)
            {
                if (hierarchy[i] is not Label label
                    || label.ClassListContains(BaseField<int>.labelUssClassName) == false
                )
                {
                    continue;
                }

                label.RegisterCallback<GeometryChangedEvent>(OnLocalLabelGeometryChanged);
                label.style.width = StyleKeyword.Null;
                label.style.flexShrink = 0;

                if (label.panel == null)
                {
                    continue;
                }

                var measured = label.MeasureTextSize(
                      label.text
                    , 0
                    , VisualElement.MeasureMode.Undefined
                    , 0
                    , VisualElement.MeasureMode.Undefined
                );
                var style = label.resolvedStyle;
                var width = measured.x + style.paddingLeft + style.paddingRight
                    + style.borderLeftWidth + style.borderRightWidth;

                if (float.IsNaN(width) == false && float.IsInfinity(width) == false)
                {
                    label.style.minWidth = Mathf.Ceil(width);
                }
            }
        }

        private void NormalizeValueLayout()
        {
            if (_detached == false)
            {
                NormalizeValueLayout(_valueField);
            }
        }

        private static void OnValueSubtreeGeometryChanged(GeometryChangedEvent evt)
        {
            NormalizeValueLayout((VisualElement)evt.currentTarget);
        }

        private static void NormalizeValueLayout(VisualElement element)
        {
            if (element is PropertyField || element is BaseListView)
            {
                element.RegisterCallback<GeometryChangedEvent>(OnValueSubtreeGeometryChanged);
            }

            if (element is BaseListView list && list.ClassListContains(LIST_LAYOUT_CLASS) == false)
            {
                list.AddToClassList(LIST_LAYOUT_CLASS);
                var normalize = list.schedule.Execute(() => NormalizeValueLayout(list));
                normalize.Pause();
                list.itemsAdded += _ => normalize.ExecuteLater(0);
                list.itemsRemoved += _ => normalize.ExecuteLater(0);
                list.itemsSourceChanged += () => normalize.ExecuteLater(0);
                var scroll = list.Q<ScrollView>();

                if (scroll != null)
                {
                    scroll.verticalScroller.valueChanged += _ => normalize.ExecuteLater(0);
                }
            }

            if (element.ClassListContains(BaseCompositeField<Vector3, FloatField, float>.inputUssClassName)
                || element.ClassListContains(BaseCompositeField<Vector3, FloatField, float>.fieldGroupUssClassName)
            )
            {
                element.RegisterCallback<GeometryChangedEvent>(OnCompositeGeometryChanged);
                NormalizeCompositeMargins(element);
            }

            var parent = element.hierarchy.parent;

            if (element.ClassListContains(BaseCompositeField<Vector3, FloatField, float>.fieldUssClassName)
                || parent?.ClassListContains(BoundsField.inputUssClassName) == true
                || parent?.ClassListContains(BoundsIntField.inputUssClassName) == true
            )
            {
                ConfigureLocalLabel(element);
            }

            var hierarchy = element.hierarchy;
            var count = hierarchy.childCount;
            var imguiProperty = element is PropertyField
                && element.ClassListContains(PropertyField.ussClassName + "__imgui-container-property");

            for (var i = 0; i < count; i++)
            {
                var child = hierarchy[i];

                if (imguiProperty && child is IMGUIContainer container)
                {
                    for (var scope = element.hierarchy.parent; scope != null; scope = scope.hierarchy.parent)
                    {
                        if (scope.ClassListContains(USS_CLASS_NAME + "__value"))
                        {
                            ConfigureImguiValue(container);
                            break;
                        }
                    }
                }

                NormalizeValueLayout(child);
            }
        }

        private static void ConfigureImguiValue(IMGUIContainer container)
        {
            container.style.marginLeft = -EditorStyles.inspectorDefaultMargins.padding.left;
            container.style.marginRight = 0;

            if (container.ClassListContains(IMGUI_LAYOUT_CLASS))
            {
                return;
            }

            container.AddToClassList(IMGUI_LAYOUT_CLASS);
            var draw = container.onGUIHandler;

            container.onGUIHandler = () => {
                var width = container.contentRect.width;

                if (float.IsNaN(width) || float.IsInfinity(width) || width <= 0f)
                {
                    draw();
                    return;
                }

                width += EditorStyles.inspectorDefaultMargins.padding.right;
                GUILayout.BeginVertical(GUIStyle.none, GUILayout.Width(width));

                try
                {
                    draw();
                }
                finally
                {
                    GUILayout.EndVertical();
                }
            };
        }

        private static void OnCompositeGeometryChanged(GeometryChangedEvent evt)
        {
            NormalizeValueLayout((VisualElement)evt.currentTarget);
        }

        private static void NormalizeCompositeMargins(VisualElement element)
        {
            var hierarchy = element.hierarchy;
            var count = hierarchy.childCount;
            VisualElement first = null;
            VisualElement previous = null;
            var spacing = 0f;

            for (var i = 0; i < count; i++)
            {
                var child = hierarchy[i];

                if (child.ClassListContains(BaseCompositeField<Vector3, FloatField, float>.fieldUssClassName) == false)
                {
                    continue;
                }

                if (first == null)
                {
                    first = child;
                    spacing = child.resolvedStyle.marginRight;
                }
                else if (previous != first)
                {
                    previous.style.marginRight = spacing;
                }

                previous = child;
            }

            if (previous != null && previous != first)
            {
                previous.style.marginRight = 0;
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

            if (_popupChoices != null)
            {
                _property.serializedObject.UpdateIfRequiredOrScript();

                if (evt.newValue == 0)
                {
                    OverridableEditorAPI.EndOverride(_property);
                }
                else if ((uint)(evt.newValue - 1) < (uint)_popupChoices.Count)
                {
                    ApplyPreset(_popupChoices[evt.newValue - 1].value);
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
            AlignCollectionMenu(_moreButton, _sizeField);
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
            AlignCollectionMenu(_moreButton, _sizeField);
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

        private sealed class ListElementAlignment
        {
            private readonly VisualElement _row;
            private VisualElement _field;
            private float _rightMargin;

            public ListElementAlignment(VisualElement row)
            {
                _row = row;
                row.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

                if (row.hierarchy.childCount > 0 && row.hierarchy[0] is PropertyField propertyField)
                {
                    propertyField.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                }
            }

            private void OnGeometryChanged(GeometryChangedEvent evt)
            {
                if (_row.hierarchy.childCount == 0)
                {
                    return;
                }

                var field = _row.hierarchy[0];

                if (field is PropertyField)
                {
                    field = field.Q(className: BaseField<int>.ussClassName);
                }

                if (field?.panel == null || field.layout.width <= 0f)
                {
                    return;
                }

                if (_field != field)
                {
                    _field?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                    _field = field;
                    _rightMargin = field.resolvedStyle.marginRight;
                    field.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                }

                _row.style.marginRight = _rightMargin;
                field.style.marginRight = 0;
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
