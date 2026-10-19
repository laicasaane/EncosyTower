#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.UitkPages.Codex;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UitkPages
{
    /// <summary>
    /// The custom inspector of <see cref="UitkPageCodex"/> and its derived types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The inspector has three sections:
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// Context, with the rows Warn No Subscriber, Reject Duplicate Pages, Load Strategy and Log Environment.
    /// </description></item>
    /// <item><description>
    /// Flow Definitions, a <see cref="FlowDefinitionsSection"/> with the columns Identifier, Kind and Container Name.
    /// The Container Name cell has a pick button that opens a <see cref="ContainerPickerPopup"/>. The container
    /// problems of <see cref="LayoutAssetContainerModel"/> join the identifier problems of each row.
    /// </description></item>
    /// <item><description>
    /// Panel, with Panel Settings, Sorting Order, Layout Asset and a read-only row for the panel component.
    /// </description></item>
    /// </list>
    /// <para>
    /// The inspector decides the panel mode once, when it creates the inspector. If the GameObject already has a
    /// <c>PanelRenderer</c> (Unity 6000.5 or newer) or a <c>UIDocument</c>, the codex uses it, the Panel fields are
    /// disabled and a note says so. Otherwise the codex adds a <c>PanelRenderer</c> on Unity 6000.5 or newer, or a
    /// <c>UIDocument</c> on older Unity or when <see cref="UitkPageFlowSettings.forceUIDocument"/> is on.
    /// </para>
    /// <para>
    /// The layout asset that the container checks use is the one of the existing component if there is one, otherwise
    /// the Layout Asset field. A change of the field updates the note and rechecks the list. The inspector reads its
    /// state from the first selected object.
    /// </para>
    /// </remarks>
    /// <seealso cref="FlowDefinitionsSection"/>
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


        private static readonly ContextSetting[] s_contextSettings = {
            new(nameof(UitkPageFlowContext.warnNoSubscriber), "warn-no-subscriber"),
            new(nameof(UitkPageFlowContext.rejectDuplicatePages), "reject-duplicate-pages"),
            new(nameof(UitkPageFlowContext.loadStrategy), "load-strategy"),
            new(nameof(UitkPageFlowContext.logEnvironment), "log-environment"),
        };

        private SerializedProperty _flows;
        private SerializedProperty _layoutAsset;
        private PanelMode _panelMode;
        private VisualElement _flowsSection;
        private Label _layoutNote;

        /// <inheritdoc/>
        /// <remarks>
        /// Returns the root with the Context, Flow Definitions and Panel sections. The scope collection comes from
        /// the <see cref="IUitkPageCodexOnInitialize"/> component.
        /// </remarks>
        public override VisualElement CreateInspectorGUI()
        {
            var codex = (UitkPageCodex)target;

            _flows = serializedObject.FindProperty(FLOWS);
            _layoutAsset = serializedObject.FindProperty(LAYOUT_ASSET);
            _panelMode = GetPanelMode(codex);

            var root = PageFlowContextSection.CreateRoot();
            var context = serializedObject.FindProperty(FLOW_CONTEXT);

            root.Add(PageFlowContextSection.Create(context, OpenSettings, s_contextSettings));

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
                , ResetNewDefinition
                , GetContainerErrors
            );

            root.Add(_flowsSection);
            root.Add(CreatePanelSection());
            return root;
        }

        private static void OpenSettings()
        {
            UitkPageFlowSettings.Instance.OpenSettingsWindow();
        }

        /// <summary>
        /// Sets the fields of a flow definition that Sync adds, except the identifier, to their defaults.
        /// </summary>
        /// <param name="definition">The serialized flow definition.</param>
        /// <remarks>
        /// The defaults are <see cref="PageFlowKind.PageStackSingle"/> and an empty container name. A serialized field
        /// that is added to the runtime flow definition needs its default here too.
        /// </remarks>
        private static void ResetNewDefinition(SerializedProperty definition)
        {
            definition.FindPropertyRelative(KIND).enumValueIndex = (int)PageFlowKind.PageStackSingle;
            definition.FindPropertyRelative(CONTAINER_NAME).stringValue = string.Empty;
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
                    , BindCell: static (cell, definition) => cell.Q<EnumField>()
                        .BindProperty(definition.FindPropertyRelative(KIND))
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

        /// <summary>
        /// Creates a cell of the Container Name column.
        /// </summary>
        /// <remarks>
        /// The cell has a text field with the codex root as placeholder, and a pick button. The button keeps the
        /// serialized flow definition of the row in its <c>userData</c>, which <c>BindContainerCell</c> sets on each
        /// bind. The list reuses a cell for different rows.
        /// </remarks>
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
                if (button.userData is SerializedProperty definition)
                {
                    ShowContainerPicker(button, definition);
                }
            }
        }

        private static void BindContainerCell(VisualElement cell, SerializedProperty definition)
        {
            cell.Q<TextField>().BindProperty(definition.FindPropertyRelative(CONTAINER_NAME));
            cell.Q<Button>().userData = definition;
        }

        /// <summary>
        /// Opens the container picker for a flow definition.
        /// </summary>
        /// <remarks>
        /// Updates the serialized object first, so the choices see the latest values. The choices come from the
        /// effective layout asset and the other flow definitions. The popup is right-aligned with the pick button.
        /// </remarks>
        private void ShowContainerPicker(VisualElement anchor, SerializedProperty definition)
        {
            serializedObject.Update();

            var container = definition.FindPropertyRelative(CONTAINER_NAME);
            var containers = LayoutAssetContainerModel.Get(GetEffectiveLayoutAsset());
            var choices = LayoutAssetContainerModel.GetChoices(
                  containers
                , container.stringValue
                , GetOtherDefinitions(definition.propertyPath)
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

        private List<(string Identifier, string ContainerName)> GetOtherDefinitions(string definitionPath)
        {
            var count = _flows.arraySize;
            var definitions = new List<(string Identifier, string ContainerName)>(count);

            for (var i = 0; i < count; i++)
            {
                var definition = _flows.GetArrayElementAtIndex(i);

                if (string.Equals(definition.propertyPath, definitionPath, StringComparison.Ordinal))
                {
                    continue;
                }

                definitions.Add((
                      definition.FindPropertyRelative(IDENTIFIER).stringValue
                    , definition.FindPropertyRelative(CONTAINER_NAME).stringValue
                ));
            }

            return definitions;
        }

        /// <summary>
        /// Gets the container problem of a flow definition.
        /// </summary>
        /// <returns>
        /// An array with the problem from <see cref="LayoutAssetContainerModel.TryGetError"/>, checked against the
        /// effective layout asset, or an empty array when there is none.
        /// </returns>
        private FlowDefinitionError[] GetContainerErrors(SerializedProperty definition)
        {
            var name = definition.FindPropertyRelative(CONTAINER_NAME).stringValue;
            var containers = LayoutAssetContainerModel.Get(GetEffectiveLayoutAsset());

            return LayoutAssetContainerModel.TryGetError(name, containers, out var error)
                ? new[] { error }
                : Array.Empty<FlowDefinitionError>();
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

        /// <summary>
        /// Gets the layout asset that the container checks use.
        /// </summary>
        /// <remarks>
        /// The order of priority is the existing <c>PanelRenderer</c> (Unity 6000.5 or newer), the existing
        /// <c>UIDocument</c>, then the Layout Asset field.
        /// </remarks>
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

        /// <summary>
        /// Says which panel component the codex uses.
        /// </summary>
        private enum PanelMode
        {
            /// <summary>
            /// The codex adds a <c>PanelRenderer</c>.
            /// </summary>
            AddRenderer,

            /// <summary>
            /// The codex adds a <c>UIDocument</c>, because Unity is older than 6000.5.
            /// </summary>
            AddDocument,

            /// <summary>
            /// The codex adds a <c>UIDocument</c>, because <see cref="UitkPageFlowSettings.forceUIDocument"/> is on.
            /// </summary>
            Forced,

            /// <summary>
            /// The GameObject already has a <c>PanelRenderer</c>.
            /// </summary>
            ExistingRenderer,

            /// <summary>
            /// The GameObject already has a <c>UIDocument</c>.
            /// </summary>
            ExistingDocument,
        }
    }
}

#endif
