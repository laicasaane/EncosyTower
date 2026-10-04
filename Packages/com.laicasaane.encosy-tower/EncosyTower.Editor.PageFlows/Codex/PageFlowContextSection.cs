#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    internal readonly record struct ContextRow(string FieldName, string Label, string GroupHeading = null);

    internal static class PageFlowContextSection
    {
        public const string ROOT_USS_CLASS_NAME = "encosy-page-flows";
        public const string SECTION_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__section";
        public const string SECTION_HEADER_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__section-header";
        public const string HEADING_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__heading";
        public const string GROUP_HEADING_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__group-heading";
        public const string HEADER_BUTTON_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__header-button";
        public const string NOTE_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__note";
        public const string FIELD_NOTE_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__field-note";

        private const string AUTO_INITIALIZE_ON_AWAKE = "autoInitializeOnAwake";

        public static VisualElement Create(
              SerializedProperty context
            , Action openSettings
            , IReadOnlyList<ContextRow> rows
        )
        {
            var section = CreateSection("Context", out var header);
            var openButton = new Button(openSettings) { text = "Open Project Settings" };
            openButton.AddToClassList(HEADER_BUTTON_USS_CLASS_NAME);
            header.Add(openButton);

            var autoInitialize = context.FindPropertyRelative(AUTO_INITIALIZE_ON_AWAKE);
            section.Add(new PropertyField(autoInitialize, "Auto Initialize On Awake"));

            var count = rows.Count;

            for (var i = 0; i < count; i++)
            {
                var row = rows[i];

                if (string.IsNullOrEmpty(row.GroupHeading) == false)
                {
                    var heading = new Label(row.GroupHeading);
                    heading.AddToClassList(GROUP_HEADING_USS_CLASS_NAME);
                    section.Add(heading);
                }

                section.Add(new PropertyField(context.FindPropertyRelative(row.FieldName), row.Label));
            }

            return section;
        }

        public static VisualElement CreateSection(string title, out VisualElement header)
        {
            var section = new VisualElement();
            section.AddToClassList(SECTION_USS_CLASS_NAME);

            header = new VisualElement();
            header.AddToClassList(SECTION_HEADER_USS_CLASS_NAME);
            section.Add(header);

            var heading = new Label(title);
            heading.AddToClassList(HEADING_USS_CLASS_NAME);
            header.Add(heading);

            return section;
        }

        public static Label CreateFieldNote(VisualElement field, string text)
        {
            var note = new Label(text);
            note.AddToClassList(NOTE_USS_CLASS_NAME);
            note.AddToClassList(FIELD_NOTE_USS_CLASS_NAME);

            VisualElement trackedInput = null;
            field.RegisterCallback<GeometryChangedEvent>(OnFieldGeometryChanged);
            return note;

            void OnFieldGeometryChanged(GeometryChangedEvent _)
            {
                var input = field.Q(className: BaseField<int>.inputUssClassName);
                var parent = note.parent;

                if (input == null || parent == null)
                {
                    return;
                }

                if (trackedInput != input)
                {
                    trackedInput?.UnregisterCallback<GeometryChangedEvent>(OnFieldGeometryChanged);
                    trackedInput = input;
                    trackedInput.RegisterCallback<GeometryChangedEvent>(OnFieldGeometryChanged);
                }

                var parentStyle = parent.resolvedStyle;
                var contentLeft = parent.worldBound.xMin + parentStyle.borderLeftWidth + parentStyle.paddingLeft;
                note.style.marginLeft = input.worldBound.xMin - contentLeft;
            }
        }

        public static VisualElement CreateRoot()
        {
            var root = new VisualElement();
            root.AddToClassList(ROOT_USS_CLASS_NAME);
            PageFlowCodexStyleSheetPaths.AddTo(root);
            return root;
        }
    }
}

#endif
