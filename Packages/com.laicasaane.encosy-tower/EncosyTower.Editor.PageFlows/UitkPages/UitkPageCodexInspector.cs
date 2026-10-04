#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UitkPages
{
    [CustomEditor(typeof(UitkPageCodex), true)]
    [CanEditMultipleObjects]
    public class UitkPageCodexInspector : UnityEditor.Editor
    {
        private const string FLOWS = nameof(UitkPageCodex._flows);
        private const string FLOW_CONTEXT = nameof(UitkPageCodex._flowContext);
        private const string PANEL_SETTINGS = nameof(UitkPageCodex._panelSettings);
        private const string SORTING_ORDER = nameof(UitkPageCodex._sortingOrder);
        private const string LAYOUT_ASSET = nameof(UitkPageCodex._layoutAsset);
        private const string IDENTIFIER = FlowDefinitionsSection.IDENTIFIER_COLUMN;
        private const string KIND = "kind";
        private const string CONTAINER_NAME = FlowDefinitionsSection.CONTAINER_NAME_COLUMN;

        private const string ROOT = PageFlowContextSection.ROOT_USS_CLASS_NAME;
        private const string CONTAINER_CELL_USS_CLASS_NAME = ROOT + "__container-cell";
        private const string PICK_BUTTON_USS_CLASS_NAME = ROOT + "__pick-button";
        private const string PICK_DOT_USS_CLASS_NAME = ROOT + "__pick-dot";
        private const int PICK_DOT_COUNT = 3;


        private static readonly ContextRow[] s_contextRows = {
            new(nameof(UitkPageFlowContext.warnNoSubscriber), "warn-no-subscriber"),
            new(nameof(UitkPageFlowContext.loadStrategy), "load-strategy"),
            new(nameof(UitkPageFlowContext.logEnvironment), "log-environment"),
        };

        private SerializedProperty _flows;
        private SerializedProperty _layoutAsset;
        private PanelMode _panelMode;
        private VisualElement _flowsSection;
        private Label _layoutNote;

        public override VisualElement CreateInspectorGUI()
        {
            var codex = (UitkPageCodex)target;

            _flows = serializedObject.FindProperty(FLOWS);
            _layoutAsset = serializedObject.FindProperty(LAYOUT_ASSET);
            _panelMode = GetPanelMode(codex);

            var root = PageFlowContextSection.CreateRoot();
            var context = serializedObject.FindProperty(FLOW_CONTEXT);

            root.Add(PageFlowContextSection.Create(context, OpenSettings, s_contextRows));

            var info = PageFlowScopeCollectionResolver.Resolve(
                  codex
                , typeof(IUitkPageCodexOnInitialize)
                , typeof(UitkPageCodexInitializer<>)
            );

            _flowsSection = FlowDefinitionsSection.Create(
                  serializedObject
                , _flows
                , info
                , CreateColumns()
                , ResetNewRow
                , GetContainerProblems
            );

            root.Add(_flowsSection);
            root.Add(CreatePanelSection());
            return root;
        }

        private static void OpenSettings()
        {
            UitkPageFlowSettings.Instance.OpenSettingsWindow();
        }

        private static void ResetNewRow(SerializedProperty row)
        {
            row.FindPropertyRelative(KIND).enumValueIndex = (int)PageFlowKind.SinglePageStack;
            row.FindPropertyRelative(CONTAINER_NAME).stringValue = string.Empty;
        }

        private FlowDefinitionColumn[] CreateColumns()
        {
            var titles = PageFlowsViewResources.Get().Columns;

            return new FlowDefinitionColumn[] {
                new(
                      Name: IDENTIFIER
                    , Title: titles.Identifier
                    , Width: 16f
                    , Grow: 1.1f
                    , MakeCell: null
                    , BindCell: null
                ),
                new(
                      Name: KIND
                    , Title: titles.Kind
                    , Width: 0f
                    , Grow: 1.25f
                    , MakeCell: static () => FlowDefinitionsSection.CreateCell(new EnumField())
                    , BindCell: static (cell, row) => cell.Q<EnumField>().BindProperty(row.FindPropertyRelative(KIND))
                ),
                new(
                      Name: CONTAINER_NAME
                    , Title: titles.ContainerName
                    , Width: 0f
                    , Grow: 1.4f
                    , MakeCell: MakeContainerCell
                    , BindCell: BindContainerCell
                ),
            };
        }

        private VisualElement MakeContainerCell()
        {
            var field = new TextField();
            field.textEdition.placeholder = PageFlowsViewResources.Get().Picker.CodexRoot;

            var button = new Button {
                tooltip = PageFlowsViewResources.Get().Picker.Tooltip,
            };

            button.AddToClassList(PICK_BUTTON_USS_CLASS_NAME);
            button.clicked += OnClicked;

            for (var i = 0; i < PICK_DOT_COUNT; i++)
            {
                var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                dot.AddToClassList(PICK_DOT_USS_CLASS_NAME);
                button.Add(dot);
            }

            var content = new VisualElement();
            content.AddToClassList(CONTAINER_CELL_USS_CLASS_NAME);
            content.Add(field);
            content.Add(button);
            return FlowDefinitionsSection.CreateCell(content);

            void OnClicked()
            {
                if (button.userData is SerializedProperty row)
                {
                    ShowContainerPicker(button, row);
                }
            }
        }

        private static void BindContainerCell(VisualElement cell, SerializedProperty row)
        {
            cell.Q<TextField>().BindProperty(row.FindPropertyRelative(CONTAINER_NAME));
            cell.Q<Button>().userData = row;
        }

        private void ShowContainerPicker(VisualElement anchor, SerializedProperty row)
        {
            serializedObject.Update();

            var container = row.FindPropertyRelative(CONTAINER_NAME);
            var containers = LayoutAssetContainerModel.Get(GetEffectiveLayoutAsset());
            var choices = LayoutAssetContainerModel.GetChoices(
                  containers
                , container.stringValue
                , GetOtherRows(row.propertyPath)
            );

            var bounds = anchor.worldBound;
            var activator = new Rect(
                  bounds.xMax - ContainerPickerPopup.WIDTH
                , bounds.y
                , ContainerPickerPopup.WIDTH
                , bounds.height
            );

            UnityEditor.PopupWindow.Show(activator, new ContainerPickerPopup(
                  container
                , choices
                , containers.HasAsset
            ));
        }

        private List<(string Identifier, string ContainerName)> GetOtherRows(string rowPath)
        {
            var count = _flows.arraySize;
            var rows = new List<(string Identifier, string ContainerName)>(count);

            for (var i = 0; i < count; i++)
            {
                var row = _flows.GetArrayElementAtIndex(i);

                if (string.Equals(row.propertyPath, rowPath, StringComparison.Ordinal))
                {
                    continue;
                }

                rows.Add((
                      row.FindPropertyRelative(IDENTIFIER).stringValue
                    , row.FindPropertyRelative(CONTAINER_NAME).stringValue
                ));
            }

            return rows;
        }

        private RowProblem[] GetContainerProblems(SerializedProperty row, int index)
        {
            var name = row.FindPropertyRelative(CONTAINER_NAME).stringValue;
            var containers = LayoutAssetContainerModel.Get(GetEffectiveLayoutAsset());

            return LayoutAssetContainerModel.TryGetProblem(name, containers, out var problem)
                ? new[] { problem }
                : Array.Empty<RowProblem>();
        }

        private VisualElement CreatePanelSection()
        {
            var texts = PageFlowsViewResources.Get().Panel;
            var section = PageFlowContextSection.CreateSection(texts.Heading, out _);
            var existingNote = _panelMode switch {
                PanelMode.ExistingDocument => texts.ExistingDocument,
                PanelMode.ExistingRenderer => texts.ExistingRenderer,
                _ => null,
            };

            var isExisting = existingNote != null;
            var panelSettings = new PropertyField(serializedObject.FindProperty(PANEL_SETTINGS), texts.PanelSettings);
            var sortingOrder = new PropertyField(serializedObject.FindProperty(SORTING_ORDER), texts.SortingOrder);
            var layoutAsset = new PropertyField(_layoutAsset, texts.LayoutAsset) {
                tooltip = texts.LayoutAssetTooltip,
            };

            panelSettings.SetEnabled(isExisting == false);
            sortingOrder.SetEnabled(isExisting == false);
            layoutAsset.SetEnabled(isExisting == false);

            section.Add(panelSettings);

            if (isExisting)
            {
                section.Add(PageFlowContextSection.CreateFieldNote(panelSettings, existingNote));
            }

            section.Add(sortingOrder);

            if (isExisting)
            {
                section.Add(PageFlowContextSection.CreateFieldNote(sortingOrder, existingNote));
            }

            section.Add(layoutAsset);

            _layoutNote = PageFlowContextSection.CreateFieldNote(layoutAsset, existingNote ?? string.Empty);
            section.Add(_layoutNote);

            var (componentName, componentNote) = GetPanelComponentTexts(_panelMode);
            var component = new ReadOnlyField(texts.Component);
            component.SetText(componentName);

            section.Add(component);
            section.Add(PageFlowContextSection.CreateFieldNote(component, componentNote));

            UpdateLayoutNote();
            section.TrackPropertyValue(_layoutAsset, OnLayoutAssetChanged);
            return section;
        }

        private void OnLayoutAssetChanged(SerializedProperty _)
        {
            UpdateLayoutNote();
            _flowsSection?.Q<MultiColumnListView>()?.RefreshItems();
        }

        private void UpdateLayoutNote()
        {
            if (_layoutNote == null || IsExisting(_panelMode))
            {
                return;
            }

            var containers = LayoutAssetContainerModel.Get(GetEffectiveLayoutAsset());
            var texts = PageFlowsViewResources.Get().Panel;

            _layoutNote.text = containers.HasAsset
                ? texts.LayoutAssetCount(containers.Items.Length)
                : texts.LayoutAssetEmpty;
        }

        private VisualTreeAsset GetEffectiveLayoutAsset()
        {
            var codex = target as UitkPageCodex;

            if (codex.IsValid())
            {
#if UNITY_6000_5_OR_NEWER
                if (codex.TryGetComponent<PanelRenderer>(out var renderer))
                {
                    return renderer.visualTreeAsset;
                }
#endif

                if (codex.TryGetComponent<UIDocument>(out var document))
                {
                    return document.visualTreeAsset;
                }
            }

            serializedObject.UpdateIfRequiredOrScript();
            return _layoutAsset.objectReferenceValue as VisualTreeAsset;
        }

        private static bool IsExisting(PanelMode mode)
            => mode is PanelMode.ExistingDocument or PanelMode.ExistingRenderer;

        private static PanelMode GetPanelMode(UitkPageCodex codex)
        {
#if UNITY_6000_5_OR_NEWER
            if (codex.TryGetComponent<PanelRenderer>(out _))
            {
                return PanelMode.ExistingRenderer;
            }
#endif

            if (codex.TryGetComponent<UIDocument>(out _))
            {
                return PanelMode.ExistingDocument;
            }

#if UNITY_6000_5_OR_NEWER
            return UitkPageFlowSettings.Instance.forceUIDocument ? PanelMode.Forced : PanelMode.AddRenderer;
#else
            return PanelMode.AddDocument;
#endif
        }

        private static (string Name, string Note) GetPanelComponentTexts(PanelMode mode)
        {
            var texts = PageFlowsViewResources.Get().Panel;

            return mode switch {
                PanelMode.AddRenderer => (texts.PanelRenderer, texts.Added),
                PanelMode.AddDocument => (texts.UIDocument, texts.AddedDocument),
                PanelMode.Forced => (texts.UIDocument, texts.Forced),
                PanelMode.ExistingRenderer => (texts.PanelRenderer, texts.Existing),
                _ => (texts.UIDocument, texts.Existing),
            };
        }

        private enum PanelMode
        {
            AddRenderer,
            AddDocument,
            Forced,
            ExistingRenderer,
            ExistingDocument,
        }
    }
}

#endif
