#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using EncosyTower.Editor.Internals;
using EncosyTower.Serialization;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EncosyTower.Editor.Serialization.Internals
{
    internal sealed class OverridableImguiRow
    {
        public const float MENU_WIDTH = 12f;
        public const float MENU_SPACING = 2f;

        private const float PREFIX_PADDING_RIGHT = 2f;
        private const float SWITCH_WIDTH = 26f;
        private const float SWITCH_SPACING = 4f;
        private const float COLLECTION_SIZE_WIDTH = 48f;
        private const float TRANSITION_DURATION = 0.12f;
        private const float DISABLED_SWATCH_OPACITY = 0.5f;
        private const float DARK_VIEW_BACKGROUND_INTENSITY = 0.22f;
        private const float LIGHT_VIEW_BACKGROUND_INTENSITY = 0.76f;

        private static readonly GUIContent[] s_vector4Labels = {
            new("X"),
            new("Y"),
            new("Z"),
            new("W"),
        };

        private static readonly Color s_darkBackground = new Color32(r: 42, g: 42, b: 42, a: 255);
        private static readonly Color s_lightBackground = new Color32(r: 240, g: 240, b: 240, a: 255);
        private static readonly Color s_darkBorderTop = new Color32(r: 13, g: 13, b: 13, a: 255);
        private static readonly Color s_darkBorder = new Color32(r: 33, g: 33, b: 33, a: 255);
        private static readonly Color s_lightBorderTop = new Color32(r: 160, g: 160, b: 160, a: 255);
        private static readonly Color s_lightBorder = new Color32(r: 183, g: 183, b: 183, a: 255);
        private static readonly Color s_darkKnob = new Color32(r: 196, g: 196, b: 196, a: 255);
        private static readonly Color s_lightKnob = new Color32(r: 9, g: 9, b: 9, a: 255);
        private static readonly Color s_darkKnobOn = new Color32(r: 13, g: 13, b: 13, a: 255);
        private static readonly Color s_darkOn = new Color32(r: 20, g: 211, b: 104, a: 255);
        private static readonly Color s_lightOn = new Color32(r: 0, g: 129, b: 38, a: 255);
        private static readonly Color s_darkFocus = new Color32(r: 58, g: 121, b: 187, a: 255);
        private static readonly Color s_lightFocus = new Color32(r: 29, g: 80, b: 135, a: 255);

        private static readonly Dictionary<(SerializedObject Object, string Path), OverridableImguiRow> s_rows
            = new(new CacheComparer());

        private static readonly Dictionary<Type, bool> s_customDrawers = new();
        private static readonly ConditionalWeakTable<ReorderableList, CompositeListDrawer> s_compositeLists = new();

        private readonly SerializedObject _serializedObject;
        private readonly string _propertyPath;
        private readonly Type _valueType;
        private readonly OverridableDefaultAttribute _attribute;
        private readonly bool _isListElement;
        private readonly bool _hasCustomClassDrawer;

        private OverridableDefaultHolder _holder;
        private bool _cleared;
        private bool _hasSwitchPosition;
        private float _switchPosition;
        private float _switchStartPosition;
        private float _switchTarget;
        private double _switchStartTime;

        static OverridableImguiRow()
        {
            AssemblyReloadEvents.beforeAssemblyReload += ClearCache;
            Selection.selectionChanged += ClearCache;
        }

        private OverridableImguiRow(SerializedProperty property, FieldInfo fieldInfo)
        {
            _serializedObject = property.serializedObject;
            _propertyPath = property.propertyPath;
            _valueType = OverridableEditorAPI.GetValueType(fieldInfo, property);
            _isListElement = OverridableEditorAPI.IsOverridableListElement(property, fieldInfo);
            var valueProperty = property.FindPropertyRelative(OverridableEditorAPI.VALUE);

            _hasCustomClassDrawer = _valueType.IsClass
                && valueProperty.propertyType == SerializedPropertyType.Generic
                && IsCollection(valueProperty) == false
                && HasCustomDrawer(_valueType);

            _attribute = _isListElement ? null : fieldInfo.GetCustomAttribute<OverridableDefaultAttribute>();
        }

        public static OverridableImguiRow Get(SerializedProperty property, FieldInfo fieldInfo)
        {
            var key = (property.serializedObject, property.propertyPath);

            if (s_rows.TryGetValue(key, out var row) == false)
            {
                row = new OverridableImguiRow(property, fieldInfo);
                s_rows.Add(key, row);
            }

            return row;
        }

        public static void ClearCache()
        {
            foreach (var row in s_rows.Values)
            {
                row._cleared = true;

                if (row._holder.IsValid())
                {
                    UnityEngine.Object.DestroyImmediate(row._holder);
                }

                row._holder = null;
            }

            s_rows.Clear();
        }

        public static bool IsLocked(SerializedProperty property)
        {
            if (property.serializedObject.targetObject is OverridableDefaultHolder)
            {
                return true;
            }

            const string VALUE_SEGMENT = "." + OverridableEditorAPI.VALUE + ".";
            var path = property.propertyPath;
            var index = path.LastIndexOf(VALUE_SEGMENT, StringComparison.Ordinal);

            while (index >= 0)
            {
                path = path[..index];
                using var parent = property.serializedObject.FindProperty(path);
                using var overridden = parent?.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN);

                if (overridden != null
                    && overridden.propertyType == SerializedPropertyType.Boolean
                    && overridden.hasMultipleDifferentValues == false
                    && overridden.boolValue == false
                )
                {
                    return true;
                }

                index = path.LastIndexOf(VALUE_SEGMENT, StringComparison.Ordinal);
            }

            return false;
        }

        public static Rect GetCollectionSizeRect(Rect header, int indent)
        {
            var previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indent;
            var indentWidth = EditorGUI.IndentedRect(header).x - header.x;
            EditorGUI.indentLevel = previousIndent;
            header.height = EditorGUIUtility.singleLineHeight;
            header.xMin = header.xMax - COLLECTION_SIZE_WIDTH - indentWidth * indent;
            return header;
        }

        public static bool DrawMoreButton(Rect rect, string tooltip)
        {
            var iconName = EditorGUIUtility.isProSkin ? "d__menu@2x" : "_menu@2x";
            var content = new GUIContent(EditorGUIUtility.IconContent(iconName).image, tooltip);

            if (Event.current.type == EventType.Repaint && GUI.enabled && rect.Contains(Event.current.mousePosition))
            {
                var shade = EditorGUIUtility.isProSkin ? 103f / 255f : 236f / 255f;
                EditorGUI.DrawRect(rect, new Color(shade, shade, shade));
            }

            var clicked = GUI.Button(position: rect, content: GUIContent.none, style: GUIStyle.none);
            var iconRect = new Rect(rect.x, rect.center.y - MENU_WIDTH * 0.5f, MENU_WIDTH, MENU_WIDTH);
            GUI.Label(iconRect, content, GUIStyle.none);
            return clicked;
        }

        public static Rect DrawWarning(Rect rect, string valueText, bool locked)
        {
            const float WARNING_SIZE = 16f;
            const float WARNING_SPACING = 4f;
            var iconName = EditorGUIUtility.isProSkin ? "d_console.warnicon" : "console.warnicon";
            var tooltip = locked ? string.Empty : $"\"{valueText}\" is not one of the allowed values.";
            var content = new GUIContent(EditorGUIUtility.IconContent(iconName).image, tooltip);
            var warningRect = new Rect(rect.xMax - WARNING_SIZE, rect.y, WARNING_SIZE, WARNING_SIZE);
            GUI.Label(warningRect, content, GUIStyle.none);
            rect.width = Mathf.Max(0f, rect.width - WARNING_SIZE - WARNING_SPACING);
            return rect;
        }

        public static bool DrawPopupButton(Rect rect, string text, string tooltip, bool mixed)
        {
            var previousMixed = EditorGUI.showMixedValue;

            try
            {
                EditorGUI.showMixedValue = mixed;
                var content = new GUIContent(mixed ? "—" : text, tooltip);
                return EditorGUI.DropdownButton(rect, content, FocusType.Keyboard, EditorStyles.popup);
            }
            finally
            {
                EditorGUI.showMixedValue = previousMixed;
            }
        }

        public static bool UsesHeader(SerializedProperty property, Type valueType)
            => IsCollection(property)
                || (property.propertyType == SerializedPropertyType.Generic
                    && (valueType.IsClass || HasCustomDrawer(valueType) == false)
                );

        public static bool TryGetNativeList(SerializedProperty property, out ReorderableList list)
        {
            EditorGUI.GetPropertyHeight(property: property, label: GUIContent.none, includeChildren: true);
            list = ReorderableList.GetReorderableListFromSerializedProperty(property);
            return list != null;
        }

        public static void DrawNativeList(Rect position, ReorderableList list)
        {
            if (list.drawElementCallback != null
                || CompositeListDrawer.IsSupported == false
                || list.serializedProperty.arraySize == 0
            )
            {
                list.DoList(position);
                return;
            }

            using var first = list.serializedProperty.GetArrayElementAtIndex(0);

            if (UsesFullWidthComposite(first) == false)
            {
                list.DoList(position);
                return;
            }

            var drawer = s_compositeLists.GetValue(list, static value => new CompositeListDrawer(value));
            list.drawElementCallback = drawer.Draw;

            try
            {
                list.DoList(position);
            }
            finally
            {
                list.drawElementCallback = null;
            }
        }

        public static bool TryDrawFullWidthComposite(Rect position, SerializedProperty property, GUIContent label)
        {
            if (UsesFullWidthComposite(property) == false)
            {
                return false;
            }

            var indent = EditorGUI.indentLevel;
            var mixed = EditorGUI.showMixedValue;
            label = EditorGUI.BeginProperty(position, label, property);

            try
            {
                var input = EditorGUI.PrefixLabel(position, label);
                EditorGUI.indentLevel = 0;

                if (property.propertyType == SerializedPropertyType.Vector4)
                {
                    using var component = property.FindPropertyRelative("x");
                    EditorGUI.MultiPropertyField(input, s_vector4Labels, component, GUIContent.none);
                }
                else
                {
                    EditorGUI.PropertyField(input, property, GUIContent.none, includeChildren: false);
                }
            }
            finally
            {
                EditorGUI.EndProperty();
                EditorGUI.indentLevel = indent;
                EditorGUI.showMixedValue = mixed;
            }

            return true;
        }

        public static bool HasCustomDrawer(Type valueType)
        {
            if (s_customDrawers.TryGetValue(valueType, out var result))
            {
                return result;
            }

            var types = TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>();
            var count = types.Count;

            for (var i = 0; i < count; i++)
            {
                var type = types[i];

                if (typeof(PropertyDrawer).IsAssignableFrom(type) == false)
                {
                    continue;
                }

                var attributes = CustomAttributeData.GetCustomAttributes(type);
                var attributeCount = attributes.Count;

                for (var j = 0; j < attributeCount; j++)
                {
                    var attribute = attributes[j];

                    if (attribute.AttributeType != typeof(CustomPropertyDrawer))
                    {
                        continue;
                    }

                    var arguments = attribute.ConstructorArguments;
                    var target = (Type)arguments[0].Value;
                    var useForChildren = arguments.Count > 1 && (bool)arguments[1].Value;

                    for (var current = valueType; current != null; current = current.BaseType)
                    {
                        var matches = current == target
                            || (current.IsGenericType && current.GetGenericTypeDefinition() == target);

                        if (matches && (current == valueType || useForChildren))
                        {
                            s_customDrawers[valueType] = true;
                            return true;
                        }
                    }
                }
            }

            s_customDrawers[valueType] = false;
            return false;
        }

        private static bool UsesFullWidthComposite(SerializedProperty property)
        {
            var type = property.propertyType switch {
                SerializedPropertyType.Vector2 => typeof(Vector2),
                SerializedPropertyType.Vector2Int => typeof(Vector2Int),
                SerializedPropertyType.Vector4 => typeof(Vector4),
                SerializedPropertyType.Rect => typeof(Rect),
                SerializedPropertyType.RectInt => typeof(RectInt),
                _ => null,
            };

            return type != null && HasCustomDrawer(type) == false;
        }

        private static bool IsCollection(SerializedProperty property)
            => property.isArray && property.propertyType != SerializedPropertyType.String;

        private static void DrawDisabledColorSwatch(Rect position, SerializedProperty property, bool hasLabel)
        {
            if (GUI.enabled
                || property.propertyType != SerializedPropertyType.Color
                || Event.current.type != EventType.Repaint
            )
            {
                return;
            }

            if (hasLabel)
            {
                position.xMin += EditorGUIUtility.labelWidth + PREFIX_PADDING_RIGHT;
            }
            else
            {
                position = EditorGUI.IndentedRect(position);
            }

            position = EditorStyles.colorField.padding.Remove(position);
            var color = GUI.color;
            var enabled = GUI.enabled;

            var shade = EditorGUIUtility.isProSkin ? DARK_VIEW_BACKGROUND_INTENSITY : LIGHT_VIEW_BACKGROUND_INTENSITY;

            var overlay = new Color(r: shade, g: shade, b: shade, a: DISABLED_SWATCH_OPACITY);

            try
            {
                GUI.color = Color.white;
                GUI.enabled = true;
                EditorGUI.DrawRect(position, overlay);
            }
            finally
            {
                GUI.enabled = enabled;
                GUI.color = color;
            }
        }

        private static Rect DrawLabel(
              Rect line
            , GUIContent label
            , SerializedProperty property
            , SerializedProperty drawn
            , bool header
            , bool active
        )
        {
            var color = GUI.color;

            if (active == false && GUI.enabled)
            {
                GUI.color = new Color(color.r, color.g, color.b, color.a * 0.5f);
            }

            try
            {
                if (header == false)
                {
                    return EditorGUI.PrefixLabel(line, label);
                }

                var labelRect = line;
                labelRect.width = EditorGUIUtility.labelWidth;

                var expanded = EditorGUI.Foldout(
                      position: labelRect
                    , foldout: drawn.isExpanded
                    , content: label
                    , toggleOnLabelClick: true
                );

                drawn.isExpanded = expanded;
                property.FindPropertyRelative(OverridableEditorAPI.VALUE).isExpanded = expanded;
                line.xMin += EditorGUIUtility.labelWidth + PREFIX_PADDING_RIGHT;
                return line;
            }
            finally
            {
                GUI.color = color;
            }
        }

        private static void AddItems(
              GenericMenu menu
            , IReadOnlyList<(GUIContent Content, bool IsChecked, GenericMenu.MenuFunction Callback)> items
        )
        {
            var count = items.Count;

            for (var i = 0; i < count; i++)
            {
                var item = items[i];
                menu.AddItem(item.Content, item.IsChecked, item.Callback);
            }
        }

        private static void DrawRounded(Rect rect, Color color, float radius, float borderWidth)
        {
            if (GUI.enabled == false)
            {
                color.a *= 0.5f;
            }

            GUI.DrawTexture(
                  position: rect
                , image: Texture2D.whiteTexture
                , scaleMode: ScaleMode.StretchToFill
                , alphaBlend: true
                , imageAspect: 0f
                , color: color
                , borderWidth: borderWidth
                , borderRadius: radius
            );
        }

        private static void DrawOutline(Rect rect, Color top, Color sides, float radius)
        {
            if (GUI.enabled == false)
            {
                top.a *= 0.5f;
                sides.a *= 0.5f;
            }

            var radiuses = new Vector4(radius, radius, radius, radius);
            DrawBorder(rect: rect, color: sides, widths: Vector4.one, radiuses: radiuses);
            DrawBorder(rect: rect, color: top, widths: new Vector4(0f, 1f, 0f, 0f), radiuses: radiuses);
        }

        private static void DrawBorder(Rect rect, Color color, Vector4 widths, Vector4 radiuses)
        {
            GUI.DrawTexture(
                  position: rect
                , image: Texture2D.whiteTexture
                , scaleMode: ScaleMode.StretchToFill
                , alphaBlend: true
                , imageAspect: 0f
                , color: color
                , borderWidths: widths
                , borderRadiuses: radiuses
            );
        }

        public SerializedProperty GetDrawnProperty()
        {
            var property = _serializedObject.FindProperty(_propertyPath);
            var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            var overridden = property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN);

            if (overridden.boolValue || overridden.hasMultipleDifferentValues)
            {
                return value;
            }

            if (_holder.IsInvalid())
            {
                _holder = OverridableDefaultHolder.Create(_valueType);
            }

            return _holder.Write(OverridableEditorAPI.GetShownDefault(_attribute, _valueType), value);
        }

        public float GetHeight(
              IReadOnlyList<(string label, object value)> choices = null
            , ValueChoicesAttribute choicesAttribute = null
        )
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;

            if (UsesDropdown(choices, choicesAttribute))
            {
                return lineHeight;
            }

            var drawn = GetDrawnProperty();

            if (IsHeader(drawn) == false)
            {
                var valueHeight = EditorGUI.GetPropertyHeight(
                      property: drawn
                    , label: GUIContent.none
                    , includeChildren: false
                );

                return Mathf.Max(lineHeight, valueHeight);
            }

            if (drawn.isExpanded == false)
            {
                return lineHeight;
            }

            if (IsCollection(drawn) && TryGetNativeList(drawn, out var list))
            {
                return lineHeight + EditorGUIUtility.standardVerticalSpacing + list.GetHeight();
            }

            if (IsCollection(drawn) || _hasCustomClassDrawer)
            {
                return lineHeight
                    + EditorGUIUtility.standardVerticalSpacing
                    + EditorGUI.GetPropertyHeight(property: drawn, label: GUIContent.none, includeChildren: true);
            }

            var height = lineHeight;
            using var child = drawn.Copy();
            using var end = drawn.GetEndProperty();
            var enterChildren = true;

            while (child.NextVisible(enterChildren) && SerializedProperty.EqualContents(child, end) == false)
            {
                height += EditorGUIUtility.standardVerticalSpacing
                    + EditorGUI.GetPropertyHeight(property: child, includeChildren: true);

                enterChildren = false;
            }

            return height;
        }

        public void Draw(
              Rect position
            , GUIContent label
            , IReadOnlyList<(string label, object value)> choices = null
            , ValueChoicesAttribute choicesAttribute = null
        )
        {
            var property = _serializedObject.FindProperty(_propertyPath);
            var locked = IsLocked(property);
            var overridden = property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN);
            var useValue = overridden.boolValue || overridden.hasMultipleDifferentValues;
            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var contextRect = useValue ? line : position;

            HandleContextMenu(contextRect, label.text, locked);
            label = locked ? new GUIContent(label.text) : label;
            label = new GUIContent(EditorGUI.BeginProperty(position, label, property));
            var indent = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;
            var previousMixed = EditorGUI.showMixedValue;

            try
            {
                using (new EditorGUI.DisabledScope(locked))
                {
                    var dropdown = UsesDropdown(choices, choicesAttribute);
                    var drawn = dropdown ? null : GetDrawnProperty();
                    var header = drawn != null && IsHeader(drawn);
                    var input = DrawLabel(line, label, property, drawn, header, dropdown || useValue);
                    EditorGUI.indentLevel = 0;
                    var hasChoiceMenu = choices?.Count > 0
                        && ValueChoicesEditorAPI.ListsOnlyChoices(choicesAttribute, _valueType) == false;

                    var menuRect = new Rect(line.xMax - MENU_WIDTH, line.y, MENU_WIDTH, line.height);

                    if (hasChoiceMenu)
                    {
                        input.xMax = Mathf.Max(input.x, menuRect.x - MENU_SPACING);
                    }

                    if (dropdown)
                    {
                        DrawDropdown(input, property, choices, choicesAttribute, locked);
                    }
                    else
                    {
                        var switchRect = new Rect(input.x, input.y, SWITCH_WIDTH, line.height);

                        if (DrawSwitch(switchRect, overridden.boolValue, overridden.hasMultipleDifferentValues, locked))
                        {
                            SetOverride(overridden.hasMultipleDifferentValues || overridden.boolValue == false);
                            drawn = GetDrawnProperty();
                            useValue = overridden.boolValue || overridden.hasMultipleDifferentValues;
                        }

                        input.xMin = Mathf.Min(input.xMax, input.x + SWITCH_WIDTH + SWITCH_SPACING);

                        DrawValue(position, input, property, drawn, header, useValue, locked, indent, labelWidth);

                        if (hasChoiceMenu && IsCollection(drawn))
                        {
                            var sizeRect = GetCollectionSizeRect(input, indent);
                            menuRect.y = sizeRect.y;
                            menuRect.height = sizeRect.height;
                        }
                    }

                    if (hasChoiceMenu
                        && DrawMoreButton(
                              menuRect
                            , locked ? string.Empty : ValueChoicesPropertyDrawer.CHOICES_TOOLTIP
                    ))
                    {
                        BuildChoiceMenu(choices, choicesAttribute).DropDown(menuRect);
                    }
                }

                HandleContextMenu(position, label.text, locked);
            }
            finally
            {
                EditorGUI.indentLevel = indent;
                EditorGUIUtility.labelWidth = labelWidth;
                EditorGUI.showMixedValue = previousMixed;
                EditorGUI.EndProperty();
            }
        }

        public void SetOverride(bool isOverridden)
        {
            if (TryGetEditableProperty(out var property) == false)
            {
                return;
            }

            if (isOverridden)
            {
                OverridableEditorAPI.BeginOverride(property, _attribute, _valueType);
            }
            else
            {
                OverridableEditorAPI.EndOverride(property);
            }
        }

        public void ApplyPreset(object value)
        {
            if (TryGetEditableProperty(out var property) == false)
            {
                return;
            }

            ValueChoicesPropertyDrawer.WriteValue(property.FindPropertyRelative(OverridableEditorAPI.VALUE), value);
            property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue = true;
            _serializedObject.ApplyModifiedProperties();
        }

        public GenericMenu BuildChoiceMenu(
              IReadOnlyList<(string label, object value)> choices
            , ValueChoicesAttribute choicesAttribute
        )
        {
            var menu = new GenericMenu();

            if (choices == null
                || ValueChoicesEditorAPI.ListsOnlyChoices(choicesAttribute, _valueType)
                || TryGetEditableProperty(out var property) == false
            )
            {
                return menu;
            }

            var overridden = property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue;

            var value = overridden
                ? ValueChoicesPropertyDrawer.ReadValue(
                      property.FindPropertyRelative(OverridableEditorAPI.VALUE)
                    , _valueType
                )
                : OverridableEditorAPI.GetShownDefault(_attribute, _valueType);

            var items = ValueChoicesPropertyDrawer.CreateMenuItems(
                  choices
                , value
                , OverridableEditorAPI.IsMixed(property)
                , ApplyPreset
            );

            AddItems(menu, items);
            return menu;
        }

        public GenericMenu BuildContextMenu(string label, bool shiftPressed = false)
        {
            if (TryGetEditableProperty(out var property) == false)
            {
                return new GenericMenu();
            }

            return OverridableEditorAPI.BuildPropertyContextMenu(property, Locate, shiftPressed);

            void Locate()
            {
                if (TryGetEditableProperty(out var current))
                {
                    LocateDefaultSource(current, label);
                }
            }
        }

        private bool IsHeader(SerializedProperty property)
            => UsesHeader(property, _valueType);

        private bool UsesDropdown(
              IReadOnlyList<(string label, object value)> choices
            , ValueChoicesAttribute choicesAttribute
        )
            => choices != null
                ? ValueChoicesEditorAPI.UsesDropdown(choicesAttribute, _valueType)
                : OverridableEditorAPI.GetMode(_valueType) == OverridableMode.Dropdown;

        private bool TryGetEditableProperty(out SerializedProperty property)
        {
            property = null;

            if (_cleared || _serializedObject.targetObject.IsInvalid())
            {
                return false;
            }

            _serializedObject.UpdateIfRequiredOrScript();
            property = _serializedObject.FindProperty(_propertyPath);
            return property != null && IsLocked(property) == false;
        }

        private void DrawValue(
              Rect position
            , Rect input
            , SerializedProperty property
            , SerializedProperty drawn
            , bool header
            , bool useValue
            , bool locked
            , int indent
            , float labelWidth
        )
        {
            var tooltip = useValue || locked
                ? string.Empty
                : OverridableEditorAPI.GetDefaultTooltip(_attribute, _valueType);

            EditorGUI.BeginChangeCheck();

            using (new EditorGUI.DisabledScope(useValue == false))
            {
                if (header)
                {
                    DrawHeaderValue(position, input, drawn, indent, labelWidth);
                }
                else
                {
                    input.height = position.height;
                    var content = new GUIContent(string.Empty, tooltip);
                    if (drawn.propertyType == SerializedPropertyType.Vector4 && HasCustomDrawer(_valueType) == false)
                    {
                        using var component = drawn.FindPropertyRelative("x");
                        EditorGUI.MultiPropertyField(input, s_vector4Labels, component, content);
                    }
                    else
                    {
                        EditorGUI.PropertyField(
                              position: input
                            , property: drawn
                            , label: content
                            , includeChildren: false
                        );
                    }

                    DrawDisabledColorSwatch(position: input, property: drawn, hasLabel: false);
                }
            }

            if (EditorGUI.EndChangeCheck() && useValue && property.serializedObject.hasModifiedProperties)
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            if (string.IsNullOrEmpty(tooltip) == false)
            {
                var content = new GUIContent(string.Empty, tooltip);

                if (header)
                {
                    if (IsCollection(drawn))
                    {
                        GUI.Label(input, content, GUIStyle.none);
                    }

                    var body = position;
                    body.yMin = input.yMax + EditorGUIUtility.standardVerticalSpacing;

                    if (body.height > 0f)
                    {
                        GUI.Label(body, content, GUIStyle.none);
                    }
                }
                else
                {
                    GUI.Label(input, content, GUIStyle.none);
                }
            }
        }

        private void DrawHeaderValue(Rect position, Rect input, SerializedProperty drawn, int indent, float labelWidth)
        {
            if (IsCollection(drawn))
            {
                var size = drawn.FindPropertyRelative("Array.size");
                EditorGUI.showMixedValue = size.hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel = indent;
                var sizeRect = GetCollectionSizeRect(input, indent);
                var count = EditorGUI.DelayedIntField(sizeRect, size.intValue);
                EditorGUI.indentLevel = 0;

                if (EditorGUI.EndChangeCheck())
                {
                    size.intValue = Mathf.Max(0, count);
                }

                EditorGUI.showMixedValue = false;
            }

            if (drawn.isExpanded == false)
            {
                return;
            }

            var body = position;
            body.yMin = input.yMax + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth = labelWidth;

            if (IsCollection(drawn) && TryGetNativeList(drawn, out var list))
            {
                DrawNativeList(EditorGUI.IndentedRect(body), list);
                return;
            }

            if (IsCollection(drawn) || _hasCustomClassDrawer)
            {
                EditorGUI.PropertyField(position: body, property: drawn, label: GUIContent.none, includeChildren: true);
                return;
            }

            EditorGUI.indentLevel = indent + 1;
            using var child = drawn.Copy();
            using var end = drawn.GetEndProperty();
            var enterChildren = true;

            while (child.NextVisible(enterChildren) && SerializedProperty.EqualContents(child, end) == false)
            {
                body.height = EditorGUI.GetPropertyHeight(property: child, includeChildren: true);
                EditorGUI.PropertyField(position: body, property: child, includeChildren: true);
                DrawDisabledColorSwatch(position: body, property: child, hasLabel: true);
                body.y += body.height + EditorGUIUtility.standardVerticalSpacing;
                enterChildren = false;
            }

            EditorGUI.indentLevel = 0;
        }

        private void DrawDropdown(
              Rect rect
            , SerializedProperty property
            , IReadOnlyList<(string label, object value)> choices
            , ValueChoicesAttribute choicesAttribute
            , bool locked
        )
        {
            var overridden = property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue;
            var mixed = OverridableEditorAPI.IsMixed(property);
            var popupChoices = choices == null
                ? null
                : ValueChoicesPropertyDrawer.GetPopupChoices(choicesAttribute, _valueType, choices);

            var value = ValueChoicesPropertyDrawer.ReadValue(
                  property.FindPropertyRelative(OverridableEditorAPI.VALUE)
                , _valueType
            );

            var text = OverridableEditorAPI.GetDisplayText(_valueType, value);
            var valueIndex = popupChoices == null ? -1 : ValueChoicesPropertyDrawer.GetChoiceIndex(popupChoices, value);

            if (overridden == false)
            {
                text = GetDefaultChoiceLabel(choices);
            }
            else if (valueIndex >= 0)
            {
                text = popupChoices[valueIndex].label;
            }

            if (overridden
                && mixed == false
                && popupChoices != null
                && ValueChoicesEditorAPI.ListsOnlyChoices(choicesAttribute, _valueType)
                && valueIndex < 0
            )
            {
                rect = DrawWarning(rect, text, locked);
            }

            var tooltip = overridden || mixed || locked
                ? string.Empty
                : OverridableEditorAPI.GetDefaultTooltip(_attribute, _valueType);

            if (DrawPopupButton(rect, text, tooltip, mixed) == false)
            {
                return;
            }

            var menu = new GenericMenu();

            menu.AddItem(
                  new GUIContent(GetDefaultChoiceLabel(choices))
                , mixed == false && overridden == false
                , UseDefault
            );

            menu.AddSeparator(string.Empty);

            if (popupChoices != null)
            {
                var items = ValueChoicesPropertyDrawer.CreateMenuItems(
                      popupChoices
                    , value
                    , mixed || overridden == false
                    , ApplyPreset
                );

                AddItems(menu, items);
            }
            else
            {
                var labels = OverridableEditorAPI.GetValueChoices(_valueType);
                var selected = OverridableEditorAPI.GetSelectedIndex(property, _valueType);
                var count = labels.Length;

                for (var i = 0; i < count; i++)
                {
                    var choiceIndex = i + 1;
                    menu.AddItem(new GUIContent(labels[i]), mixed == false && selected == choiceIndex, SelectEnum);

                    void SelectEnum()
                    {
                        if (TryGetEditableProperty(out var current))
                        {
                            OverridableEditorAPI.ApplyChoice(current, _valueType, choiceIndex);
                        }
                    }
                }
            }

            menu.DropDown(rect);

            void UseDefault()
            {
                SetOverride(false);
            }
        }

        private string GetDefaultChoiceLabel(IReadOnlyList<(string label, object value)> choices)
        {
            if (choices == null)
            {
                return OverridableEditorAPI.GetDefaultChoiceLabel(_attribute, _valueType);
            }

            var label = OverridableEditorAPI.GetDefaultLabel(_attribute);

            if (OverridableEditorAPI.TryGetDefaultValue(_attribute, out var value) == false)
            {
                return label;
            }

            var index = ValueChoicesPropertyDrawer.GetChoiceIndex(choices, value);
            var text = index >= 0 ? choices[index].label : OverridableEditorAPI.GetDisplayText(_valueType, value);
            return $"{label} ({text})";
        }

        private void HandleContextMenu(Rect rect, string label, bool locked)
        {
            if (Event.current.type != EventType.ContextClick || rect.Contains(Event.current.mousePosition) == false)
            {
                return;
            }

            if (locked == false && GUI.enabled)
            {
                BuildContextMenu(label, Event.current.shift).ShowAsContext();
            }

            Event.current.Use();
        }

        private void LocateDefaultSource(SerializedProperty property, string label)
        {
            if (_isListElement)
            {
                var path = property.propertyPath;
                var arrayIndex = path.LastIndexOf(".Array.data[", StringComparison.Ordinal);
                var collectionPath = path[..arrayIndex];
                var outerPath = collectionPath[..collectionPath.LastIndexOf('.')];
                var outer = property.serializedObject.FindProperty(outerPath);
                var elementIndex = int.Parse(path[(path.LastIndexOf('[') + 1)..^1]);

                var message = OverridableEditorAPI.GetElementNoDefaultMessage(
                      outer.displayName
                    , _valueType
                    , elementIndex
                );

                EditorUtility.DisplayDialog(title: "Default Value", message: message, ok: "OK");
                return;
            }

            OverridableEditorAPI.Locate(_attribute, _valueType, label);
        }

        private bool DrawSwitch(Rect rect, bool value, bool mixed, bool locked)
        {
            var id = GUIUtility.GetControlID(FocusType.Keyboard, rect);
            var evt = Event.current;
            var toggle = false;
            var tooltip = locked ? string.Empty : "Override";
            GUI.Label(rect, new GUIContent(string.Empty, tooltip), GUIStyle.none);

            switch (evt.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                {
                    if (GUI.enabled && evt.button == 0 && rect.Contains(evt.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        GUIUtility.keyboardControl = id;
                        evt.Use();
                    }

                    break;
                }
                case EventType.MouseUp:
                {
                    if (GUIUtility.hotControl == id && evt.button == 0)
                    {
                        GUIUtility.hotControl = 0;
                        toggle = GUI.enabled && rect.Contains(evt.mousePosition);
                        evt.Use();
                    }

                    break;
                }
                case EventType.KeyDown:
                {
                    var activationKey = evt.keyCode is KeyCode.Space or KeyCode.Return or KeyCode.KeypadEnter;

                    if (GUI.enabled && GUIUtility.keyboardControl == id && activationKey)
                    {
                        toggle = true;
                        evt.Use();
                    }

                    break;
                }
                case EventType.Repaint:
                {
                    PaintSwitch(rect, id, value, mixed);
                    break;
                }
            }

            if (toggle)
            {
                GUI.changed = true;
            }

            return toggle;
        }

        private void PaintSwitch(Rect rect, int id, bool value, bool mixed)
        {
            var dark = EditorGUIUtility.isProSkin;
            var background = dark ? s_darkBackground : s_lightBackground;
            var border = dark ? s_darkBorder : s_lightBorder;
            var borderTop = dark ? s_darkBorderTop : s_lightBorderTop;
            var knob = dark ? s_darkKnob : s_lightKnob;

            if (value && mixed == false)
            {
                background = dark ? s_darkOn : s_lightOn;
                knob = dark ? s_darkKnobOn : Color.white;
            }

            var track = new Rect(x: rect.x, y: rect.center.y - 6.5f, width: 24f, height: 13f);

            if (GUIUtility.keyboardControl == id && GUI.enabled)
            {
                var focusRect = new Rect(track.x - 1f, track.y - 1f, track.width + 2f, track.height + 2f);
                var focus = dark ? s_darkFocus : s_lightFocus;
                DrawRounded(rect: focusRect, color: focus, radius: 7.5f, borderWidth: 1f);
            }

            DrawRounded(rect: track, color: background, radius: 6.5f, borderWidth: 0f);
            DrawOutline(rect: track, top: borderTop, sides: border, radius: 6.5f);
            var target = value ? 13f : 2f;

            if (mixed)
            {
                target = 8f;
            }

            var x = GetSwitchPosition(target);
            var knobRect = mixed
                ? new Rect(x: track.x + x, y: track.y + 5.5f, width: 8f, height: 2f)
                : new Rect(x: track.x + x, y: track.y + 2f, width: 9f, height: 9f);

            DrawRounded(rect: knobRect, color: knob, radius: mixed ? 0f : 4.5f, borderWidth: 0f);
        }

        private float GetSwitchPosition(float target)
        {
            if (_hasSwitchPosition == false)
            {
                _hasSwitchPosition = true;
                _switchPosition = target;
                _switchTarget = target;
                _switchStartPosition = target;
            }

            if (_switchTarget != target)
            {
                _switchTarget = target;
                _switchStartPosition = _switchPosition;
                _switchStartTime = EditorApplication.timeSinceStartup;
            }

            var time = (float)(EditorApplication.timeSinceStartup - _switchStartTime) / TRANSITION_DURATION;
            _switchPosition = Mathf.Lerp(_switchStartPosition, target, Mathf.SmoothStep(from: 0f, to: 1f, t: time));

            if (time < 1f)
            {
                InternalEditorUtility.RepaintAllViews();
            }

            return _switchPosition;
        }

        private sealed class CompositeListDrawer
        {
            private static readonly Func<Rect, SerializedProperty, float> s_labelWidth
                = typeof(ReorderableList.Defaults)
                    .GetMethod("FieldLabelSize", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.CreateDelegate(
                          typeof(Func<Rect, SerializedProperty, float>)
                        , ReorderableList.defaultBehaviours
                    ) as Func<Rect, SerializedProperty, float>;

            private static readonly Func<float, float> s_padding
                = typeof(ReorderableList.Defaults)
                    .GetMethod("ElementPadding", BindingFlags.Static | BindingFlags.NonPublic)
                    ?.CreateDelegate(typeof(Func<float, float>)) as Func<float, float>;

            private readonly ReorderableList _list;

            public CompositeListDrawer(ReorderableList list)
            {
                _list = list;
            }

            public static bool IsSupported => s_labelWidth != null && s_padding != null;

            public void Draw(Rect position, int index, bool selected, bool focused)
            {
                var collection = _list.serializedProperty;

                if ((uint)index >= (uint)collection.arraySize)
                {
                    return;
                }

                using var element = collection.GetArrayElementAtIndex(index);
                var labelWidth = EditorGUIUtility.labelWidth;
                position.y += s_padding(position.height) / 2f;
                EditorGUIUtility.labelWidth = s_labelWidth(position, element);

                try
                {
                    TryDrawFullWidthComposite(position, element, new GUIContent(element.displayName));
                }
                finally
                {
                    EditorGUIUtility.labelWidth = labelWidth;
                }
            }
        }

        private sealed class CacheComparer : IEqualityComparer<(SerializedObject Object, string Path)>
        {
            public bool Equals((SerializedObject Object, string Path) x, (SerializedObject Object, string Path) y)
                => ReferenceEquals(x.Object, y.Object) && string.Equals(x.Path, y.Path, StringComparison.Ordinal);

            public int GetHashCode((SerializedObject Object, string Path) obj)
                => HashCode.Combine(RuntimeHelpers.GetHashCode(obj.Object), obj.Path);
        }
    }
}

#endif
