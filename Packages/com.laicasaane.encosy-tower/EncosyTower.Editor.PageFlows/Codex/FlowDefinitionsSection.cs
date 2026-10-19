#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Describes one data column of the flow definition list.
    /// </summary>
    /// <param name="Name">
    /// The column name, which is also the name of the serialized field that the column shows. The column named
    /// <see cref="FlowDefinitionsSection.CONTAINER_NAME_COLUMN"/> receives the container problems.
    /// </param>
    /// <param name="Title">The header text.</param>
    /// <param name="Width">The fixed width in pixels.</param>
    /// <param name="Grow">The share of the remaining width. <c>0</c> keeps the width fixed.</param>
    /// <param name="MakeCell">
    /// Creates one cell. Build it with <see cref="FlowDefinitionsSection.CreateCell"/> or
    /// <see cref="FlowDefinitionsSection.CreateCenteredCell"/>. <see langword="null"/> for the identifier column, which
    /// the section builds itself.
    /// </param>
    /// <param name="BindCell">
    /// Binds a cell to the flow definition, which is an element of the serialized array. It runs on every row bind, so
    /// it must be safe to run again. <see langword="null"/> for the identifier column.
    /// </param>
    internal sealed record FlowDefinitionColumn(
          string Name
        , string Title
        , float Width
        , float Grow
        , Func<VisualElement> MakeCell
        , Action<VisualElement, SerializedProperty> BindCell
    );

    /// <summary>
    /// Builds the Flow Definitions section of the codex inspectors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The uGUI and UI Toolkit codex inspectors share it. The section has a header with a warning icon for the
    /// list-level error and a Sync Scopes button, a read-only Scope Collection row, and a reorderable list bound to the
    /// serialized array. The list has the identifier column, the columns of the inspector, and a status column.
    /// </para>
    /// <para>
    /// <see cref="Create"/> builds everything. The section lives through the callbacks it registers, so the caller does
    /// not keep it. With several objects selected, the section shows only a note.
    /// </para>
    /// <para>
    /// A refresh builds a new <see cref="FlowDefinitionListModel"/> after <c>UpdateIfRequiredOrScript</c>, so Undo,
    /// Redo and external edits are picked up. It runs on creation, on every change of the array and after Sync. The
    /// problems of a row are read when the row binds. A row shows an invalid style on the cell of the column that
    /// <see cref="FlowDefinitionError.IEnumCase.IsIdentifierProblem"/> selects, and the status icon lists every problem
    /// of the row as a tooltip.
    /// </para>
    /// <para>
    /// List cells use Unity property binding. The dropdown of the identifier column writes with
    /// <c>Update</c>, a change of the value and <c>ApplyModifiedProperties</c>, which is one Undo step. Sync is one
    /// Undo step too.
    /// </para>
    /// </remarks>
    /// <seealso cref="FlowDefinitionListModel"/>
    /// <seealso cref="FlowDefinitionError"/>
    /// <seealso cref="PageFlowScopeCollectionInfo"/>
    internal sealed class FlowDefinitionsSection
    {
        /// <summary>
        /// The name of the identifier column. It equals <see cref="FlowDefinitionListModel.IDENTIFIER"/>.
        /// </summary>
        public const string IDENTIFIER_COLUMN = FlowDefinitionListModel.IDENTIFIER;

        /// <summary>
        /// The name of the column that shows the problems other than identifier problems.
        /// </summary>
        /// <remarks>
        /// A codex without a column of this name still shows the status icon, but no invalid cell for these problems.
        /// </remarks>
        public const string CONTAINER_NAME_COLUMN = "containerName";

        private const string ROOT = PageFlowContextSection.ROOT_USS_CLASS_NAME;
        private const string WARNING_USS_CLASS_NAME = ROOT + "__warning";
        private const string NOTE_USS_CLASS_NAME = ROOT + "__note";
        private const string LIST_USS_CLASS_NAME = ROOT + "__list";
        private const string CELL_USS_CLASS_NAME = ROOT + "__cell";
        private const string CENTERED_CELL_USS_CLASS_NAME = ROOT + "__cell--centered";
        private const string INVALID_CELL_USS_CLASS_NAME = ROOT + "__cell--invalid";
        private const string STATUS_USS_CLASS_NAME = ROOT + "__status";
        private const string STATUS_ICON_USS_CLASS_NAME = ROOT + "__status-icon";
        private const string POPUP_USS_CLASS_NAME = ROOT + "__popup";
        private const string WARNING_ICON = "console.warnicon.sml";
        private const float ROW_HEIGHT = 22f;
        private const float STATUS_WIDTH = 22f;

        private readonly SerializedProperty _flows;
        private readonly PageFlowScopeCollectionInfo _info;
        private readonly Action<SerializedProperty> _resetNewDefinition;
        private readonly Func<SerializedProperty, FlowDefinitionError[]> _getExtraErrors;
        private readonly Image _listWarning;
        private readonly Button _syncButton;
        private readonly MultiColumnListView _list;
        private readonly IReadOnlyList<FlowDefinitionColumn> _columns;

        private FlowDefinitionListModel _model;

        private FlowDefinitionsSection(
              VisualElement section
            , VisualElement header
            , SerializedProperty flows
            , in PageFlowScopeCollectionInfo info
            , IReadOnlyList<FlowDefinitionColumn> columns
            , Action<SerializedProperty> resetNewDefinition
            , Func<SerializedProperty, FlowDefinitionError[]> getExtraErrors
        )
        {
            _flows = flows;
            _info = info;
            _resetNewDefinition = resetNewDefinition;
            _getExtraErrors = getExtraErrors;

            _listWarning = CreateWarningIcon();
            header.Add(_listWarning);

            _syncButton = new Button(OnSync) { text = PageFlowsViewResources.Get().Flows.Sync };
            _syncButton.AddToClassList(PageFlowContextSection.HEADER_BUTTON_USS_CLASS_NAME);
            header.Add(_syncButton);

            section.Add(CreateScopeCollectionRow(info));

            _list = new MultiColumnListView {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = false,
                showBoundCollectionSize = false,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                fixedItemHeight = ROW_HEIGHT,
                selectionType = SelectionType.Single,
                horizontalScrollingEnabled = false,
            };

            _columns = columns;
            _list.AddToClassList(LIST_USS_CLASS_NAME);
            AddColumns(columns);
            _list.Q<ScrollView>().contentViewport.RegisterCallback<GeometryChangedEvent>(OnListGeometryChanged);
            section.Add(_list);

            _list.BindProperty(flows);
            section.TrackPropertyValue(flows, OnFlowsChanged);
            Refresh();
        }

        /// <summary>
        /// Creates the Flow Definitions section.
        /// </summary>
        /// <param name="serializedObject">
        /// The inspected object. Only <see cref="SerializedObject.isEditingMultipleObjects"/> is read from it.
        /// </param>
        /// <param name="flows">The serialized array of flow definitions.</param>
        /// <param name="info">The result of the scope collection lookup.</param>
        /// <param name="columns">
        /// The columns of the list. The first one must be the identifier column, named
        /// <see cref="IDENTIFIER_COLUMN"/>.
        /// </param>
        /// <param name="resetNewDefinition">
        /// Sets the fields other than the identifier of a flow definition that Sync adds to their defaults.
        /// </param>
        /// <param name="getExtraErrors">
        /// Returns more errors for a flow definition, for example container errors. They follow the identifier errors.
        /// Can be <see langword="null"/>.
        /// </param>
        /// <returns>
        /// The section. With several objects selected, it holds only a note.
        /// </returns>
        public static VisualElement Create(
              SerializedObject serializedObject
            , SerializedProperty flows
            , in PageFlowScopeCollectionInfo info
            , IReadOnlyList<FlowDefinitionColumn> columns
            , Action<SerializedProperty> resetNewDefinition
            , Func<SerializedProperty, FlowDefinitionError[]> getExtraErrors = null
        )
        {
            var flowsTexts = PageFlowsViewResources.Get().Flows;
            var section = PageFlowContextSection.CreateSection(flowsTexts.Heading, out var header);

            if (serializedObject.isEditingMultipleObjects)
            {
                var note = new Label(flowsTexts.MultiEdit);
                note.AddToClassList(NOTE_USS_CLASS_NAME);
                section.Add(note);
                return section;
            }

            _ = new FlowDefinitionsSection(section, header, flows, info, columns, resetNewDefinition, getExtraErrors);
            return section;
        }

        /// <summary>
        /// Wraps content in the standard cell of the list.
        /// </summary>
        /// <param name="content">The content of the cell.</param>
        /// <returns>The cell.</returns>
        public static VisualElement CreateCell(VisualElement content)
        {
            var cell = new VisualElement();
            cell.AddToClassList(CELL_USS_CLASS_NAME);
            cell.Add(content);
            return cell;
        }

        /// <summary>
        /// Wraps content in the standard cell of the list and centers it, for example a toggle.
        /// </summary>
        /// <param name="content">The content of the cell.</param>
        /// <returns>The cell.</returns>
        public static VisualElement CreateCenteredCell(VisualElement content)
        {
            var cell = CreateCell(content);
            cell.AddToClassList(CENTERED_CELL_USS_CLASS_NAME);
            return cell;
        }

        private static Image CreateWarningIcon()
        {
            var icon = new Image {
                image = EditorGUIUtility.IconContent(WARNING_ICON).image,
                focusable = true,
                pickingMode = PickingMode.Position,
            };

            icon.AddToClassList(WARNING_USS_CLASS_NAME);
            return icon;
        }

        private static VisualElement CreateScopeCollectionRow(in PageFlowScopeCollectionInfo info)
        {
            var field = new ReadOnlyField(PageFlowsViewResources.Get().Flows.ScopeCollection);
            field.AddToClassList(BaseField<string>.alignedFieldUssClassName);

            if (info.IsValid)
            {
                field.SetText(info.TypeName);
            }
            else
            {
                var icon = CreateWarningIcon();
                icon.tooltip = info.ToTooltip();
                field.SetText(info.ToProblemText(), icon);
            }

            return field;
        }

        private void AddColumns(IReadOnlyList<FlowDefinitionColumn> columns)
        {
            var count = columns.Count;

            for (var i = 0; i < count; i++)
            {
                var source = columns[i];

                if (source.Name == IDENTIFIER_COLUMN)
                {
                    _list.columns.Add(CreateIdentifierColumn(source));
                    continue;
                }

                _list.columns.Add(new Column {
                    name = source.Name,
                    title = source.Title,
                    width = source.Width,
                    stretchable = false,
                    resizable = false,
                    makeCell = source.MakeCell,
                    bindCell = Bind,
                    unbindCell = static (element, _) => element.Unbind(),
                });

                void Bind(VisualElement element, int index)
                {
                    BindCell(source, element, index);
                }
            }

            _list.columns.Add(new Column {
                name = "status",
                title = string.Empty,
                width = STATUS_WIDTH,
                minWidth = STATUS_WIDTH,
                maxWidth = STATUS_WIDTH,
                stretchable = false,
                resizable = false,
                makeCell = MakeStatusCell,
                bindCell = BindStatusCell,
                unbindCell = UnbindManualCell,
            });
        }

        /// <summary>
        /// Creates the identifier column.
        /// </summary>
        /// <remarks>
        /// While the scope collection is not found, the cells are text fields that the user types in. Otherwise the
        /// cells are popups built by hand, not <c>PopupField</c>, because the choices differ per row: they can include
        /// a disabled current value that is not a scope, and "in use" notes. Code binds the popup cells, so they are
        /// not unbound.
        /// </remarks>
        private Column CreateIdentifierColumn(FlowDefinitionColumn source)
        {
            if (_info.IsValid == false)
            {
                return new Column {
                    name = source.Name,
                    title = source.Title,
                    width = source.Width,
                    stretchable = false,
                    resizable = false,
                    makeCell = static () => CreateCell(new TextField()),
                    bindCell = BindIdentifierTextCell,
                    unbindCell = static (element, _) => element.Unbind(),
                };
            }

            return new Column {
                name = source.Name,
                title = source.Title,
                width = source.Width,
                stretchable = false,
                resizable = false,
                makeCell = MakeIdentifierPopupCell,
                bindCell = BindIdentifierPopupCell,
                unbindCell = UnbindManualCell,
            };
        }

        /// <summary>
        /// Sizes the columns to the width of the list.
        /// </summary>
        /// <remarks>
        /// The fixed width of every column and the width of the status column are subtracted from the width of the
        /// viewport. The rest is divided between the columns by their <see cref="FlowDefinitionColumn.Grow"/>. Widths
        /// are rounded down. Horizontal scrolling is off.
        /// </remarks>
        private void OnListGeometryChanged(GeometryChangedEvent evt)
        {
            var available = evt.newRect.width - STATUS_WIDTH;
            var totalGrow = 0f;
            var count = _columns.Count;

            for (var i = 0; i < count; i++)
            {
                var column = _columns[i];
                available -= column.Width;
                totalGrow += column.Grow;
            }

            available = Mathf.Max(available, 0f);

            for (var i = 0; i < count; i++)
            {
                var source = _columns[i];
                var share = totalGrow > 0f ? available * source.Grow / totalGrow : 0f;
                _list.columns[source.Name].width = Mathf.Floor(source.Width + share);
            }
        }

        private void BindCell(FlowDefinitionColumn column, VisualElement element, int index)
        {
            if ((uint)index >= (uint)_flows.arraySize)
            {
                return;
            }

            column.BindCell?.Invoke(element, _flows.GetArrayElementAtIndex(index));
            SetInvalid(element, HasProblemInColumn(index, column.Name));
        }

        private void BindIdentifierTextCell(VisualElement element, int index)
        {
            if ((uint)index >= (uint)_flows.arraySize)
            {
                return;
            }

            var property = _flows.GetArrayElementAtIndex(index).FindPropertyRelative(IDENTIFIER_COLUMN);
            element.Q<TextField>().BindProperty(property);
        }

        private VisualElement MakeIdentifierPopupCell()
        {
            var field = new VisualElement();
            field.AddToClassList(POPUP_USS_CLASS_NAME);
            field.AddToClassList(BaseField<string>.ussClassName);
            field.AddToClassList(BaseField<string>.noLabelVariantUssClassName);
            field.AddToClassList(BasePopupField<string, string>.ussClassName);
            field.AddToClassList(PopupField<string>.ussClassName);

            var input = new VisualElement();
            input.AddToClassList(BaseField<string>.inputUssClassName);
            input.AddToClassList(BasePopupField<string, string>.inputUssClassName);
            input.AddToClassList(PopupField<string>.inputUssClassName);
            input.AddManipulator(new Clickable(OnClicked));
            field.Add(input);

            var text = new TextElement { pickingMode = PickingMode.Ignore };
            text.AddToClassList(BasePopupField<string, string>.textUssClassName);
            input.Add(text);

            var arrow = new VisualElement { pickingMode = PickingMode.Ignore };
            arrow.AddToClassList(BasePopupField<string, string>.arrowUssClassName);
            input.Add(arrow);

            return CreateCell(field);

            void OnClicked()
            {
                if (input.userData is int definitionIndex)
                {
                    ShowIdentifierMenu(input, definitionIndex);
                }
            }
        }

        private void BindIdentifierPopupCell(VisualElement element, int index)
        {
            if ((uint)index >= (uint)_flows.arraySize)
            {
                return;
            }

            var input = element.Q(className: BaseField<string>.inputUssClassName);
            var text = element.Q<TextElement>(className: BasePopupField<string, string>.textUssClassName);
            var identifier = _flows.GetArrayElementAtIndex(index).FindPropertyRelative(IDENTIFIER_COLUMN).stringValue;

            input.userData = index;
            text.text = identifier;
            SetInvalid(element, HasProblemInColumn(index, IDENTIFIER_COLUMN));
        }

        /// <summary>
        /// Shows the identifier choices of a row as a menu.
        /// </summary>
        /// <remarks>
        /// An item reads <c>value</c> or <c>value (note)</c>. Picking an item writes the identifier as one Undo step.
        /// A pick is ignored when the row no longer exists.
        /// </remarks>
        private void ShowIdentifierMenu(VisualElement anchor, int definitionIndex)
        {
            var choices = _model.GetIdentifierChoices(definitionIndex);
            var menu = new GenericMenu();

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                var text = string.IsNullOrEmpty(choice.Note) ? choice.Value : $"{choice.Value} ({choice.Note})";
                var content = new GUIContent(text);

                if (choice.Disabled)
                {
                    menu.AddDisabledItem(content, choice.Checked);
                }
                else
                {
                    menu.AddItem(content, choice.Checked, OnSelected, choice.Value);
                }

                if (choice.SeparatorAfter)
                {
                    menu.AddSeparator(string.Empty);
                }
            }

            menu.DropDown(anchor.worldBound);

            void OnSelected(object value)
            {
                if ((uint)definitionIndex >= (uint)_flows.arraySize)
                {
                    return;
                }

                _flows.serializedObject.Update();
                var definition = _flows.GetArrayElementAtIndex(definitionIndex);
                definition.FindPropertyRelative(IDENTIFIER_COLUMN).stringValue = (string)value;
                _flows.serializedObject.ApplyModifiedProperties();
            }
        }

        private VisualElement MakeStatusCell()
        {
            var cell = new VisualElement();
            cell.AddToClassList(STATUS_USS_CLASS_NAME);

            var icon = CreateWarningIcon();
            icon.AddToClassList(STATUS_ICON_USS_CLASS_NAME);
            cell.Add(icon);
            return cell;
        }

        private void BindStatusCell(VisualElement element, int index)
        {
            var icon = element.Q<Image>();
            var errors = GetErrors(index);

            icon.visible = errors.Length > 0;
            icon.tooltip = errors.Length > 0 ? TooltipAPI.ToTooltip(errors) : string.Empty;
        }

        /// <summary>
        /// Gets all problems of a row.
        /// </summary>
        /// <returns>
        /// The identifier errors of the model, then the extra errors of the inspector. Empty while there is no model,
        /// or when the index is stale.
        /// </returns>
        private FlowDefinitionError[] GetErrors(int index)
        {
            if (_model == null || (uint)index >= (uint)_flows.arraySize)
            {
                return Array.Empty<FlowDefinitionError>();
            }

            var errors = _model.GetErrors(index);

            if (_getExtraErrors == null)
            {
                return errors;
            }

            var extra = _getExtraErrors(_flows.GetArrayElementAtIndex(index));

            if (extra == null || extra.Length < 1)
            {
                return errors;
            }

            var combined = new FlowDefinitionError[errors.Length + extra.Length];
            errors.CopyTo(combined, 0);
            extra.CopyTo(combined, errors.Length);
            return combined;
        }

        /// <summary>
        /// Gets whether a row has a problem that belongs to a column.
        /// </summary>
        /// <remarks>
        /// An identifier problem belongs to <see cref="IDENTIFIER_COLUMN"/>. Every other problem belongs to
        /// <see cref="CONTAINER_NAME_COLUMN"/>.
        /// </remarks>
        private bool HasProblemInColumn(int index, string columnName)
        {
            var errors = GetErrors(index);

            for (var i = 0; i < errors.Length; i++)
            {
                var errorColumn = errors[i].IsIdentifierProblem ? IDENTIFIER_COLUMN : CONTAINER_NAME_COLUMN;

                if (errorColumn == columnName)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnFlowsChanged(SerializedProperty _)
        {
            Refresh();
        }

        private void OnSync()
        {
            _flows.serializedObject.Update();
            _model.Sync(_flows, _resetNewDefinition);
            Refresh();
        }

        /// <summary>
        /// Validates the list again and updates the section.
        /// </summary>
        /// <remarks>
        /// Updates the serialized object, builds a new model, then updates the header warning, the Sync button and its
        /// tooltip, and the rows of the list. It runs on creation, on every change of the array and after Sync.
        /// </remarks>
        private void Refresh()
        {
            _flows.serializedObject.UpdateIfRequiredOrScript();
            _model = FlowDefinitionListModel.Create(_flows, _info);

            var definitionsError = _model.GetDefinitionsError();
            _listWarning.style.display = definitionsError.HasProblem ? DisplayStyle.Flex : DisplayStyle.None;
            _listWarning.tooltip = definitionsError.HasProblem
                ? TooltipAPI.ToTooltip(definitionsError.ToProblem(), definitionsError.ToFix())
                : string.Empty;

            var canSync = _model.CanSync;
            _syncButton.SetEnabled(canSync);
            _syncButton.tooltip = GetSyncTooltip(canSync);
            _list.RefreshItems();
        }

        private string GetSyncTooltip(bool canSync)
        {
            var flows = PageFlowsViewResources.Get().Flows;

            if (_info.IsValid == false)
            {
                return flows.SyncNoCollection;
            }

            return canSync ? flows.SyncRule(_info.TypeName) : flows.SyncMatches(_info.TypeName);
        }

        private static void UnbindManualCell(VisualElement element, int index)
        {
        }

        private static void SetInvalid(VisualElement element, bool invalid)
            => element.EnableInClassList(INVALID_CELL_USS_CLASS_NAME, invalid);
    }
}

#endif
