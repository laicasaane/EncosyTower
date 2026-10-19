#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UitkPages
{
    /// <summary>
    /// A popup that picks the element of the layout asset in which a flow definition places its flow.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="UitkPageCodexInspector"/> opens it with <c>PopupWindow.Show</c>. The content is a UI Toolkit tree
    /// from <see cref="CreateGUI"/>; <see cref="OnGUI"/> is empty on purpose.
    /// </para>
    /// <para>
    /// The first item is always the codex root, followed by a separator. A search field filters the other items by
    /// label or path, ignoring case. Without a layout asset, a row says so instead of the list. Each item shows its
    /// path and its note; no item is disabled.
    /// </para>
    /// <para>
    /// Picking an item writes the container name with <c>Update</c>, a change of the value and
    /// <c>ApplyModifiedProperties</c>, which is one Undo step, then closes the window. The popup keeps the serialized
    /// property for its lifetime.
    /// </para>
    /// </remarks>
    internal sealed class ContainerPickerPopup : PopupWindowContent
    {
        /// <summary>
        /// The width of the popup in pixels. The inspector uses it to align the popup with the pick button.
        /// </summary>
        public const float WIDTH = 300f;

        private const string ROOT = PageFlowContextSection.ROOT_USS_CLASS_NAME;
        private const string PICKER_USS_CLASS_NAME = ROOT + "__picker";
        private const string SEARCH_USS_CLASS_NAME = ROOT + "__picker-search";
        private const string ITEM_USS_CLASS_NAME = ROOT + "__picker-item";
        private const string CHECK_USS_CLASS_NAME = ROOT + "__picker-check";
        private const string NAME_USS_CLASS_NAME = ROOT + "__picker-name";
        private const string NOTE_USS_CLASS_NAME = ROOT + "__picker-note";
        private const string EMPTY_USS_CLASS_NAME = ROOT + "__picker-empty";
        private const string SEPARATOR_USS_CLASS_NAME = ROOT + "__picker-separator";
        private const string CHECK_MARK = "✓";
        private const float ITEM_HEIGHT = 20f;
        private const float CHROME_HEIGHT = 36f;
        private const float MAX_HEIGHT = 360f;

        private readonly SerializedProperty _container;
        private readonly ContainerChoice[] _choices;
        private readonly bool _hasAsset;

        private ScrollView _items;

        /// <summary>
        /// Creates the popup.
        /// </summary>
        /// <param name="container">The string property of the container name to write.</param>
        /// <param name="choices">
        /// The choices to list. The first choice must be the Codex root: it is always shown first, above the separator,
        /// and is never filtered.
        /// </param>
        /// <param name="hasAsset">Whether a layout asset is available.</param>
        public ContainerPickerPopup(SerializedProperty container, ContainerChoice[] choices, bool hasAsset)
        {
            _container = container;
            _choices = choices;
            _hasAsset = hasAsset;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The height grows with the number of choices, with room for at least two rows, up to a maximum of 360 pixels.
        /// </remarks>
        public override Vector2 GetWindowSize()
        {
            var rows = Math.Max(_choices.Length, 2);
            return new Vector2(WIDTH, Mathf.Min(rows * ITEM_HEIGHT + CHROME_HEIGHT, MAX_HEIGHT));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Draws nothing. The content comes from <see cref="CreateGUI"/>.
        /// </remarks>
        public override void OnGUI(Rect rect)
        {
        }

        /// <inheritdoc/>
        public override VisualElement CreateGUI()
        {
            var size = GetWindowSize();
            var root = new VisualElement();
            root.AddToClassList(ROOT);
            root.AddToClassList(PICKER_USS_CLASS_NAME);
            root.style.width = size.x;
            root.style.height = size.y;
            PageFlowCodexStyleSheetPaths.AddTo(root);

            var search = new ToolbarSearchField();
            search.AddToClassList(SEARCH_USS_CLASS_NAME);
            search.RegisterValueChangedCallback(OnSearchChanged);
            root.Add(search);

            _items = new ScrollView(ScrollViewMode.Vertical);
            root.Add(_items);

            Draw(string.Empty);
            search.schedule.Execute(search.Focus);
            return root;
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            Draw(evt.newValue ?? string.Empty);
        }

        /// <summary>
        /// Rebuilds the items for the text of the search field. It runs on every change of that text.
        /// </summary>
        /// <param name="query">The search text. Empty keeps all items.</param>
        private void Draw(string query)
        {
            _items.Clear();
            _items.Add(CreateItem(_choices[0]));

            var separator = new VisualElement();
            separator.AddToClassList(SEPARATOR_USS_CLASS_NAME);
            _items.Add(separator);

            if (_hasAsset == false)
            {
                _items.Add(CreateEmpty(PageFlowsViewResources.Get().Picker.NoLayoutAsset));
                return;
            }

            var count = 0;

            for (var i = 1; i < _choices.Length; i++)
            {
                var choice = _choices[i];

                if (Matches(choice, query) == false)
                {
                    continue;
                }

                _items.Add(CreateItem(choice));
                count++;
            }

            if (count < 1)
            {
                _items.Add(CreateEmpty(PageFlowsViewResources.Get().Picker.NoMatch));
            }
        }

        private VisualElement CreateItem(ContainerChoice choice)
        {
            var item = new Button(OnClicked);
            item.AddToClassList(ITEM_USS_CLASS_NAME);
            item.tooltip = choice.Path;

            var check = new Label(choice.Checked ? CHECK_MARK : string.Empty);
            check.AddToClassList(CHECK_USS_CLASS_NAME);
            item.Add(check);

            var name = new Label(choice.Label);
            name.AddToClassList(NAME_USS_CLASS_NAME);
            item.Add(name);

            var noteText = GetNoteText(choice);

            if (string.IsNullOrEmpty(noteText) == false)
            {
                var note = new Label(noteText);
                note.AddToClassList(NOTE_USS_CLASS_NAME);
                item.Add(note);
            }

            return item;

            void OnClicked()
            {
                Select(choice.Value);
            }
        }

        private void Select(string value)
        {
            _container.serializedObject.Update();
            _container.stringValue = value;
            _container.serializedObject.ApplyModifiedProperties();
            editorWindow.Close();
        }

        private static VisualElement CreateEmpty(string text)
        {
            var label = new Label(text);
            label.AddToClassList(EMPTY_USS_CLASS_NAME);
            return label;
        }

        private static string GetNoteText(ContainerChoice choice)
        {
            if (string.IsNullOrEmpty(choice.Note))
            {
                return choice.Path;
            }

            return string.IsNullOrEmpty(choice.Path) ? choice.Note : $"{choice.Path} · {choice.Note}";
        }

        private static bool Matches(ContainerChoice choice, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return true;
            }

            return choice.Label.Contains(query, StringComparison.OrdinalIgnoreCase)
                || choice.Path.Contains(query, StringComparison.OrdinalIgnoreCase);
        }
    }
}

#endif
