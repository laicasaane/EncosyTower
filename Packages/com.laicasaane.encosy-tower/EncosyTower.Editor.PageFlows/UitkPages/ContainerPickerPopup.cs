#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UitkPages
{
    internal sealed class ContainerPickerPopup : PopupWindowContent
    {
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

        public ContainerPickerPopup(SerializedProperty container, ContainerChoice[] choices, bool hasAsset)
        {
            _container = container;
            _choices = choices;
            _hasAsset = hasAsset;
        }

        public override Vector2 GetWindowSize()
        {
            var rows = Math.Max(_choices.Length, 2);
            return new Vector2(WIDTH, Mathf.Min(rows * ITEM_HEIGHT + CHROME_HEIGHT, MAX_HEIGHT));
        }

        public override void OnGUI(Rect rect)
        {
        }

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
