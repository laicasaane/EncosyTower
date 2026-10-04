#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
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
        private const string NOTE_USS_CLASS_NAME = ROOT + "__note";
        private const string FIELD_NOTE_USS_CLASS_NAME = ROOT + "__field-note";
        private const string CONTAINER_CELL_USS_CLASS_NAME = ROOT + "__container-cell";
        private const string PICK_BUTTON_USS_CLASS_NAME = ROOT + "__pick-button";

        private const string PICK_BUTTON_TEXT = "⋯";
        private const string PICK_BUTTON_TOOLTIP = "Pick a named element of the Layout Asset.";
        private const string LAYOUT_ASSET_TOOLTIP =
            "UXML loaded into the panel root. Flow containers are picked from its named elements.";
        private const string LAYOUT_ASSET_EMPTY_NOTE = "Optional. Flows go under the codex root.";
        private const string EXISTING_UI_DOCUMENT_NOTE = "Taken from the existing UI Document";
        private const string EXISTING_PANEL_RENDERER_NOTE = "Taken from the existing Panel Renderer";
        private const string PANEL_RENDERER = "Panel Renderer";
        private const string UI_DOCUMENT = "UI Document";

        private static readonly ContextRow[] s_contextRows = {
            new(nameof(UitkPageFlowContext.warnNoSubscriber), "Warn No Subscriber"),
            new(nameof(UitkPageFlowContext.loadStrategy), "Load Strategy"),
            new(nameof(UitkPageFlowContext.logEnvironment), "Log Environment"),
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
            => new FlowDefinitionColumn[] {
                new(
                      Name: IDENTIFIER
                    , Title: "Identifier"
                    , Width: Length.Percent(30f)
                    , Stretchable: true
                    , MakeCell: null
                    , BindCell: null
                ),
                new(
                      Name: KIND
                    , Title: "Kind"
                    , Width: Length.Percent(30f)
                    , Stretchable: true
                    , MakeCell: static () => FlowDefinitionsSection.CreateCell(new EnumField())
                    , BindCell: static (cell, row) => cell.Q<EnumField>().BindProperty(row.FindPropertyRelative(KIND))
                ),
                new(
                      Name: CONTAINER_NAME
                    , Title: "Container Name"
                    , Width: Length.Percent(40f)
                    , Stretchable: true
                    , MakeCell: MakeContainerCell
                    , BindCell: BindContainerCell
                ),
            };

        private VisualElement MakeContainerCell()
        {
            var field = new TextField();
            field.textEdition.placeholder = LayoutAssetContainerModel.CODEX_ROOT_LABEL;

            var button = new Button {
                text = PICK_BUTTON_TEXT,
                tooltip = PICK_BUTTON_TOOLTIP,
            };

            button.AddToClassList(PICK_BUTTON_USS_CLASS_NAME);
            button.clicked += OnClicked;

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

            UnityEditor.PopupWindow.Show(anchor.worldBound, new ContainerPickerPopup(
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
            var section = PageFlowContextSection.CreateSection("Panel", out _);
            var existingNote = _panelMode switch {
                PanelMode.ExistingDocument => EXISTING_UI_DOCUMENT_NOTE,
                PanelMode.ExistingRenderer => EXISTING_PANEL_RENDERER_NOTE,
                _ => null,
            };

            var isExisting = existingNote != null;
            var panelSettings = new PropertyField(serializedObject.FindProperty(PANEL_SETTINGS), "Panel Settings");
            var sortingOrder = new PropertyField(serializedObject.FindProperty(SORTING_ORDER), "Sorting Order");
            var layoutAsset = new PropertyField(_layoutAsset, "Layout Asset") {
                tooltip = LAYOUT_ASSET_TOOLTIP,
            };

            panelSettings.SetEnabled(isExisting == false);
            sortingOrder.SetEnabled(isExisting == false);
            layoutAsset.SetEnabled(isExisting == false);

            section.Add(panelSettings);

            if (isExisting)
            {
                section.Add(CreateFieldNote(existingNote));
            }

            section.Add(sortingOrder);

            if (isExisting)
            {
                section.Add(CreateFieldNote(existingNote));
            }

            section.Add(layoutAsset);

            _layoutNote = CreateFieldNote(existingNote ?? string.Empty);
            section.Add(_layoutNote);

            var (componentName, componentNote) = GetPanelComponentTexts(_panelMode);
            var component = new TextField("Panel Component") {
                value = componentName,
                isReadOnly = true,
                focusable = false,
            };

            component.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            section.Add(component);
            section.Add(CreateFieldNote(componentNote));

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

            var count = containers.Items.Length;

            _layoutNote.text = containers.HasAsset == false ? LAYOUT_ASSET_EMPTY_NOTE
                : count == 1 ? "1 named element available as a container"
                : $"{count} named elements available as containers";
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

        private static Label CreateFieldNote(string text)
        {
            var label = new Label(text);
            label.AddToClassList(NOTE_USS_CLASS_NAME);
            label.AddToClassList(FIELD_NOTE_USS_CLASS_NAME);
            return label;
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
            => mode switch {
                PanelMode.AddRenderer => (PANEL_RENDERER, "Added to this GameObject at runtime."),
                PanelMode.AddDocument => (
                      UI_DOCUMENT
                    , "Added to this GameObject at runtime. Panel Renderer is used on Unity 6000.5 or newer."
                ),
                PanelMode.Forced => (
                      UI_DOCUMENT
                    , "Added at runtime. Project Settings force UI Document instead of Panel Renderer."
                ),
                PanelMode.ExistingRenderer => (PANEL_RENDERER, "Uses the existing component on this GameObject."),
                _ => (UI_DOCUMENT, "Uses the existing component on this GameObject."),
            };

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
