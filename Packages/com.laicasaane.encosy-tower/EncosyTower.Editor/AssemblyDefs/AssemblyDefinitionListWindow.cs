#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor.UIElements;
using EncosyTower.Search;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.AssemblyDefs
{
    internal class AssemblyDefinitionListWindow : EditorWindow
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/AssemblyDefs";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(AssemblyDefinitionListWindow);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.tss";

        private static readonly string[] s_folders1 = new[] { "Assets" };
        private static readonly string[] s_folders2 = new[] { "Assets", "Packages" };

        private readonly List<AssemblyDefinitionEntry> _items = new();

        private AssemblyDefinitionEntryCollectionAsset _asset;
        private SerializedObject _serializedObject;
        private SimpleTableView<AssemblyDefinitionEntry> _table;
        private Toggle _includePackagesToggle;

        [MenuItem("Encosy Tower/Assembly Definitions", priority = 65_83_00_00)]
        public static void OpenWindow()
        {
            var window = GetWindow<AssemblyDefinitionListWindow>();
            window.titleContent = new GUIContent("Assembly Definitions");
            window.ShowPopup();
        }

        private void CreateGUI()
        {
            rootVisualElement.WithEditorStyleSheet(THEME_STYLE_SHEET);

            var toolbar = new Toolbar();
            rootVisualElement.Add(toolbar);

            toolbar.Add(new ToolbarButton(Refresh) {
                text = "Refresh",
                name = "refresh-button",
            });

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(_includePackagesToggle = new ToolbarToggle {
                text = "Include Packages",
                tooltip = "Include all embedded packages",
                name = "include-packages-toggle",
                value = true,
            });

            _includePackagesToggle.RegisterValueChangedCallback(IncludePackagesToggle_OnValueChanged);

            var searchField = new ToolbarSearchField {
                name = "search-field"
            };

            searchField.RegisterValueChangedCallback(SearchField_OnValueChanged);

            toolbar.Add(searchField);

            CreateTable();
            Refresh();
        }

        private void Refresh()
        {
            FindItems(_items, _includePackagesToggle.value);

            _asset.items.Clear();
            _asset.items.AddRange(_items);
        }

        private void IncludePackagesToggle_OnValueChanged(ChangeEvent<bool> evt)
        {
            Refresh();
        }

        private void SearchField_OnValueChanged(ChangeEvent<string> evt)
        {
            _table.Unbind();

            var dest = _asset.items;
            dest.Clear();

            FuzzySearchAPI.Search(_items, evt.newValue, dest, new SearchValidator());

            _table.Bind(_serializedObject);
        }

        private void CreateTable()
        {
            var table = _table = new SimpleTableView<AssemblyDefinitionEntry>() {
                bindingPath = nameof(AssemblyDefinitionEntryCollectionAsset.items),
                showBoundCollectionSize = false,
            }.WithEditorStyleSheets();

            rootVisualElement.Add(table);

            var column = table.AddColumn(" ", 10);
            column.maxWidth = 10;

            column = table.AddColumn("Edit References"
                , 200
                , TableColumn_MakeCell_References
                , TableColumn_BindCell_References
                , header: new(AssemblyDefinitionEntry.CompareNames)
            );

            column.resizable = true;
            column.sortable = true;

            column = table.AddColumn("Locate Asset"
                , 200
                , TableColumn_MakeCell_Path
                , TableColumn_BindCell_Path
                , header: new(AssemblyDefinitionEntry.ComparePaths)
            );

            column.resizable = true;
            column.sortable = true;

            _asset = CreateInstance<AssemblyDefinitionEntryCollectionAsset>();
            _asset.items = _table.Items;

            _serializedObject = new SerializedObject(_asset);
            _table.Bind(_serializedObject);
        }

        private static Button TableColumn_MakeCell_References()
        {
            var button = new Button();
            button.AddToClassList("asset-button");

            button.clicked += () => {
                if (button.userData is not AssemblyDefinitionEntry item || item.asset.IsInvalid())
                {
                    return;
                }

                var infoResult = AssemblyDefinitionAPI.TryGetInfo(item.asset.text);

                if (infoResult.TryGetValue(out var assemblyDef) == false)
                {
                    EditorUtility.DisplayDialog(item.name, infoResult.GetErrorOrThrow().ToString(), "I understand");
                    return;
                }

                var dataResult = AssemblyDefinitionAPI.TryGetData(item.path, item.asset, assemblyDef);

                if (dataResult.TryGetValue(out var data) == false)
                {
                    EditorUtility.DisplayDialog(item.name, dataResult.GetErrorOrThrow().ToString(), "I understand");
                    return;
                }

                AssemblyReferenceWindow.OpenWindow(data);
            };

            return button;
        }

        private static void TableColumn_BindCell_References(VisualElement element, AssemblyDefinitionEntry item)
        {
            if (element is not Button button || item is null)
            {
                return;
            }

            button.text = item.asset.name;
            button.userData = item;
        }

        private static Label TableColumn_MakeCell_Path()
        {
            var label = new Label();
            label.AddToClassList("asset-path");

            label.RegisterCallback<PointerDownEvent>(_ => {
                if (label.userData is AssemblyDefinitionAsset asset && asset.IsValid())
                {
                    EditorGUIUtility.PingObject(asset);
                }
            });

            return label;
        }

        private static void TableColumn_BindCell_Path(VisualElement element, AssemblyDefinitionEntry item)
        {
            if (element is not Label label || item is null)
            {
                return;
            }

            label.text = item.path;
            label.userData = item.asset;
        }

        private static void FindItems(List<AssemblyDefinitionEntry> result, bool includePackages)
        {
            result.Clear();

            var folders = includePackages ? s_folders2 : s_folders1;
            var guids = AssetDatabase.FindAssets($"t:{nameof(AssemblyDefinitionAsset)}", folders).AsSpan();

            if (guids.Length < 1)
            {
                result = default;
                return;
            }

            var items = new List<AssemblyDefinitionEntry>(guids.Length);

            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);

                if (package != null && package.source != PackageSource.Embedded)
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(path);

                if (asset.IsInvalid())
                {
                    continue;
                }

                items.Add(new AssemblyDefinitionEntry {
                    asset = asset,
                    name = asset.name,
                    path = path,
                });
            }

            items.Sort(Sort);
            result.AddRange(items);
            return;

            static int Sort(AssemblyDefinitionEntry x, AssemblyDefinitionEntry y)
                => StringComparer.OrdinalIgnoreCase.Compare(x.path, y.path);
        }

        [Serializable]
        private class AssemblyDefinitionEntry
        {
            public AssemblyDefinitionAsset asset;
            public string name;
            public string path;

            public static int CompareNames(AssemblyDefinitionEntry a, AssemblyDefinitionEntry b)
                => string.Compare(a.asset.name, b.asset.name, StringComparison.Ordinal);

            public static int ComparePaths(AssemblyDefinitionEntry a, AssemblyDefinitionEntry b)
                => string.Compare(a.path, b.path, StringComparison.Ordinal);
        }

        private class AssemblyDefinitionEntryCollectionAsset : ScriptableObject
        {
            public List<AssemblyDefinitionEntry> items;
        }

        private readonly struct SearchValidator : ISearchValidator<AssemblyDefinitionEntry>
        {
            public bool IsSearchable(AssemblyDefinitionEntry item)
                => item.path.IsNotEmpty();

            public string GetSearchableString(AssemblyDefinitionEntry item)
                => item.name;
        }
    }
}

#endif
