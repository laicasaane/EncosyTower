#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EncosyTower.Editor
{
    /// <summary>
    /// Reusable IMGUI helpers shared by property drawers.
    /// </summary>
    public static class ImguiAPI
    {
        public const float MENU_WIDTH = 12f;
        public const float MENU_SPACING = 2f;
        public const float COLLECTION_SIZE_WIDTH = 48f;

        public static readonly GUIContent[] VECTOR4_LABELS = {
            new("X"),
            new("Y"),
            new("Z"),
            new("W"),
        };

        private static readonly Dictionary<Type, bool> s_customDrawers = new();
        private static readonly ConditionalWeakTable<ReorderableList, CompositeListDrawer> s_compositeLists = new();

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

        /// <summary>
        /// Draws the warning icon of an exclusive dropdown at the right end of a rectangle.
        /// </summary>
        /// <param name="rect">The rectangle of the dropdown.</param>
        /// <param name="valueText">The text of the stored value, shown in the tooltip.</param>
        /// <param name="locked">Whether the row is locked; the tooltip is empty then.</param>
        /// <returns>The rectangle without the space of the icon.</returns>
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

        /// <summary>
        /// Draws an error icon at the right end of a rectangle.
        /// </summary>
        /// <returns>The rectangle without the space of the icon.</returns>
        public static Rect DrawError(Rect rect, string tooltip)
        {
            const float ICON_SIZE = 16f;
            const float ICON_SPACING = 4f;
            var iconName = EditorGUIUtility.isProSkin ? "d_console.erroricon" : "console.erroricon";
            var content = new GUIContent(EditorGUIUtility.IconContent(iconName).image, tooltip);
            var iconRect = new Rect(rect.xMax - ICON_SIZE, rect.y, ICON_SIZE, ICON_SIZE);
            GUI.Label(iconRect, content, GUIStyle.none);
            rect.width = Mathf.Max(0f, rect.width - ICON_SIZE - ICON_SPACING);
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

        /// <summary>
        /// Gets the list that Unity draws for a collection property.
        /// </summary>
        /// <remarks>
        /// Unity builds the list when it measures the property height, so the method asks for the height first.
        /// </remarks>
        /// <param name="property">The collection property.</param>
        /// <param name="list">Receives the list, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when Unity drew a list.</returns>
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

        /// <summary>
        /// Draws a vector or rect value on one full-width line.
        /// </summary>
        /// <param name="position">The rectangle of the field.</param>
        /// <param name="property">The value property.</param>
        /// <param name="label">The label of the field.</param>
        /// <returns>
        /// <see langword="true"/> when the value was drawn; <see langword="false"/> when it is not a Vector2,
        /// Vector2Int, Vector4, Rect or RectInt, or when that type has a custom drawer.
        /// </returns>
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
                    EditorGUI.MultiPropertyField(input, VECTOR4_LABELS, component, GUIContent.none);
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

        /// <summary>
        /// Checks whether a type has a custom property drawer.
        /// </summary>
        /// <remarks>
        /// A drawer counts when its target is the type or its generic definition, or a base type when the drawer
        /// is for children. The result is cached for each type until the domain reloads. The rows use it to decide
        /// whether Unity's default Vector4 and Quaternion fields are safe to use.
        /// </remarks>
        /// <param name="valueType">The type of the value.</param>
        /// <returns><see langword="true"/> when a drawer applies to the type.</returns>
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

        public static bool UsesFullWidthComposite(SerializedProperty property)
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

        public static bool IsCollection(SerializedProperty property)
            => property.isArray && property.propertyType != SerializedPropertyType.String;

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
    }
}

#endif
