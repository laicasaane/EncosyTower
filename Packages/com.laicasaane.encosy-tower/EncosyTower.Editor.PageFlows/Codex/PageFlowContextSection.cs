#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Describes one property row of the Context section.
    /// </summary>
    /// <param name="FieldName">
    /// The name of the serialized field in the flow context struct. Use <c>nameof</c> of the runtime field.
    /// </param>
    /// <param name="TextKey">
    /// The key of the row label. The label is the text <c>context__{TextKey}</c> of the editor text resources.
    /// </param>
    /// <param name="GroupTextKey">
    /// The key of a group heading that appears before this row, or <see langword="null"/> for none.
    /// </param>
    internal readonly record struct ContextSetting(string FieldName, string TextKey, string GroupTextKey = null);

    /// <summary>
    /// Builds the parts that the page flow inspectors share: the Context section, plain sections, field notes and the
    /// root element with the style sheets.
    /// </summary>
    internal static class PageFlowContextSection
    {
        /// <summary>
        /// The USS class of the root element. Every other USS class of this module is named
        /// <c>encosy-page-flows__element</c>.
        /// </summary>
        public const string ROOT_USS_CLASS_NAME = "encosy-page-flows";
        public const string SECTION_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__section";
        public const string SECTION_HEADER_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__section-header";
        public const string HEADING_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__heading";
        public const string GROUP_HEADING_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__group-heading";
        public const string HEADER_BUTTON_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__header-button";
        public const string NOTE_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__note";
        public const string FIELD_NOTE_USS_CLASS_NAME = ROOT_USS_CLASS_NAME + "__field-note";

        private const string AUTO_INITIALIZE_ON_AWAKE = "autoInitializeOnAwake";

        /// <summary>
        /// Creates the Context section.
        /// </summary>
        /// <param name="context">
        /// The serialized flow context struct. It must contain <c>autoInitializeOnAwake</c> and every field named in
        /// <paramref name="settings"/>.
        /// </param>
        /// <param name="openSettings">Runs when the user presses the Open Project Settings button.</param>
        /// <param name="settings">
        /// The rows after the fixed Auto Initialize On Awake row, in display order.
        /// </param>
        /// <returns>The section.</returns>
        /// <remarks>
        /// The rows are property fields, so Undo and editing several objects work through Unity property binding.
        /// </remarks>
        public static VisualElement Create(
              SerializedProperty context
            , Action openSettings
            , IReadOnlyList<ContextSetting> settings
        )
        {
            var texts = PageFlowsViewResources.Get().Context;
            var section = CreateSection(texts.Heading, out var header);
            var openButton = new Button(openSettings) { text = texts.OpenSettings };
            openButton.AddToClassList(HEADER_BUTTON_USS_CLASS_NAME);
            header.Add(openButton);

            var autoInitialize = context.FindPropertyRelative(AUTO_INITIALIZE_ON_AWAKE);
            section.Add(new PropertyField(autoInitialize, texts.AutoInitialize));

            var count = settings.Count;

            for (var i = 0; i < count; i++)
            {
                var setting = settings[i];

                if (string.IsNullOrEmpty(setting.GroupTextKey) == false)
                {
                    var heading = new Label(texts.GetText(setting.GroupTextKey));
                    heading.AddToClassList(GROUP_HEADING_USS_CLASS_NAME);
                    section.Add(heading);
                }

                var label = texts.GetText(setting.TextKey);
                section.Add(new PropertyField(context.FindPropertyRelative(setting.FieldName), label));
            }

            return section;
        }

        /// <summary>
        /// Creates an empty section with a header.
        /// </summary>
        /// <param name="title">The heading text.</param>
        /// <param name="header">The header row. Its first child is the title; callers add buttons and icons.</param>
        /// <returns>The section.</returns>
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

        /// <summary>
        /// Creates a note that lines up under the input of a field.
        /// </summary>
        /// <param name="field">The field that the note belongs to.</param>
        /// <param name="text">The text of the note.</param>
        /// <returns>The note.</returns>
        /// <remarks>
        /// The caller must add the note to the same parent as <paramref name="field"/>, right after it. The note keeps
        /// its left margin equal to the left edge of the input of the field, and updates it when the geometry changes.
        /// The callbacks stay as long as the elements.
        /// </remarks>
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

        /// <summary>
        /// Creates the root element of an inspector, with the module class and style sheets.
        /// </summary>
        /// <returns>The root element.</returns>
        /// <seealso cref="PageFlowCodexStyleSheetPaths.AddTo"/>
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
