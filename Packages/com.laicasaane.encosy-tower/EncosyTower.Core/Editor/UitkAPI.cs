#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor
{
    /// <summary>
    /// Reusable UI Toolkit helpers shared by property drawers and editor windows.
    /// </summary>
    public static class UitkAPI
    {
        public const string WARNING_ICON_CLASS_NAME = "encosy-warning-icon";
        public const string ROW_CLASS_NAME = "encosy-row";
        public const string ROW_COLLECTION_CLASS_NAME = "encosy-row--collection";
        public const string VALUE_FIELD_CLASS_NAME = "encosy-row__value-field";
        public const string VALUE_CLASS_NAME = "encosy-row__value";
        public const string PLAIN_COLLECTION_SIZE_CHOICES_CLASS_NAME = "encosy-row__plain-collection-size--choices";
        public const string LOCAL_LABEL_CLASS_NAME = "encosy-row__local-label";

        private const string ROOT_CLASS_NAME = "encosy-row-root";
        private const string MORE_CLASS_NAME = "encosy-row__more";
        private const string MORE_ICON_CLASS_NAME = "encosy-row__more-icon";
        private const string LIST_LAYOUT_CLASS = "encosy-row__list-layout";
        private const string IMGUI_LAYOUT_CLASS = "encosy-row__imgui-layout";
        private const string STYLE_SHEET_PATH
            = EditorStyleSheetPaths.ROOT + "/EncosyTower.Core/Editor.UIElements/StyleSheets/UitkRow.uss";

        private static StyleSheet s_rowStyleSheet;

        /// <summary>
        /// Adds the shared row style sheet and the layout fixes to a row root.
        /// </summary>
        /// <param name="root">The element that receives the style sheet.</param>
        public static void AddRowStyleSheets(VisualElement root)
        {
            root.AddToClassList(ROOT_CLASS_NAME);
            root.styleSheets.Add(EditorAPI.GetOrLoadAsset(ref s_rowStyleSheet, STYLE_SHEET_PATH));
            root.RegisterCallback<GeometryChangedEvent>(OnValueSubtreeGeometryChanged);
        }


        /// <summary>
        /// Creates the hidden warning icon of a choice row.
        /// </summary>
        /// <returns>
        /// A hidden <see cref="Image"/> with the USS class <see cref="WARNING_ICON_CLASS_NAME"/>.
        /// <see cref="RefreshWarning"/> shows it.
        /// </returns>
        public static Image CreateWarningIcon()
        {
            var iconName = EditorGUIUtility.isProSkin ? "d_console.warnicon" : "console.warnicon";
            var icon = new Image { image = EditorGUIUtility.IconContent(iconName).image };
            icon.AddToClassList(WARNING_ICON_CLASS_NAME);
            icon.style.width = 16f;
            icon.style.height = 16f;
            icon.style.flexShrink = 0f;
            icon.style.marginLeft = 4f;
            icon.style.display = DisplayStyle.None;
            return icon;
        }

        /// <summary>
        /// Creates the visible error icon of a row whose choices cannot be resolved.
        /// </summary>
        public static Image CreateErrorIcon(string tooltip)
        {
            var iconName = EditorGUIUtility.isProSkin ? "d_console.erroricon" : "console.erroricon";
            var icon = new Image { image = EditorGUIUtility.IconContent(iconName).image, tooltip = tooltip };
            icon.AddToClassList(WARNING_ICON_CLASS_NAME);
            icon.style.width = 16f;
            icon.style.height = 16f;
            icon.style.flexShrink = 0f;
            icon.style.marginLeft = 4f;
            return icon;
        }

        /// <summary>
        /// Shows or hides the warning icon of a choice row.
        /// </summary>
        /// <remarks>
        /// The tooltip says that the value is not one of the allowed values. It is empty when the row is locked.
        /// </remarks>
        /// <param name="icon">The icon created by <see cref="CreateWarningIcon"/>.</param>
        /// <param name="isUnlisted">Whether the stored value is not one of the choices.</param>
        /// <param name="valueText">The text of the stored value, shown in the tooltip.</param>
        /// <param name="locked">Whether the row is locked.</param>
        public static void RefreshWarning(Image icon, bool isUnlisted, string valueText, bool locked)
        {
            icon.style.display = isUnlisted ? DisplayStyle.Flex : DisplayStyle.None;
            icon.tooltip = isUnlisted && locked == false
                ? $"\"{valueText}\" is not one of the allowed values."
                : string.Empty;
        }

        public static void AlignListElementRow(VisualElement row)
        {
            _ = new ListElementAlignment(row);
        }

        /// <summary>
        /// Creates the button that opens the menu of choices.
        /// </summary>
        /// <param name="tooltip">The tooltip of the button.</param>
        /// <param name="locked">Whether the row is locked; the button is disabled then.</param>
        /// <returns>The button with a menu icon.</returns>
        public static Button CreateMenuButton(string tooltip, bool locked)
        {
            var button = new Button { tooltip = tooltip };
            button.AddToClassList(MORE_CLASS_NAME);
            button.SetEnabled(locked == false);

            var iconName = EditorGUIUtility.isProSkin ? "d__menu@2x" : "_menu@2x";

            var icon = new Image {
                image = EditorGUIUtility.IconContent(iconName).image,
                pickingMode = PickingMode.Ignore,
            };

            icon.AddToClassList(MORE_ICON_CLASS_NAME);
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

        public static void ConfigureLocalLabel(VisualElement field)
        {
            field.AddToClassList(LOCAL_LABEL_CLASS_NAME);
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

        private static void OnValueSubtreeGeometryChanged(GeometryChangedEvent evt)
        {
            NormalizeValueLayout((VisualElement)evt.currentTarget);
        }

        public static void NormalizeValueLayout(VisualElement element)
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
                        if (scope.ClassListContains(VALUE_CLASS_NAME))
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
    }
}

#endif