#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    internal sealed record FlowDefinitionColumn(
          string Name
        , string Title
        , float Width
        , float Grow
        , Func<VisualElement> MakeCell
        , Action<VisualElement, SerializedProperty> BindCell
    );

    internal sealed class FlowDefinitionsSection
    {
        public const string IDENTIFIER_COLUMN = FlowDefinitionListModel.IDENTIFIER;
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
        private readonly Action<SerializedProperty> _resetNewRow;
        private readonly Func<SerializedProperty, int, RowProblem[]> _getExtraRowProblems;
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
            , Action<SerializedProperty> resetNewRow
            , Func<SerializedProperty, int, RowProblem[]> getExtraRowProblems
        )
        {
            _flows = flows;
            _info = info;
            _resetNewRow = resetNewRow;
            _getExtraRowProblems = getExtraRowProblems;

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

        public static VisualElement Create(
              SerializedObject serializedObject
            , SerializedProperty flows
            , in PageFlowScopeCollectionInfo info
            , IReadOnlyList<FlowDefinitionColumn> columns
            , Action<SerializedProperty> resetNewRow
            , Func<SerializedProperty, int, RowProblem[]> getExtraRowProblems = null
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

            _ = new FlowDefinitionsSection(section, header, flows, info, columns, resetNewRow, getExtraRowProblems);
            return section;
        }

        public static VisualElement CreateCell(VisualElement content)
        {
            var cell = new VisualElement();
            cell.AddToClassList(CELL_USS_CLASS_NAME);
            cell.Add(content);
            return cell;
        }

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
                if (input.userData is int row)
                {
                    ShowIdentifierMenu(input, row);
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

        private void ShowIdentifierMenu(VisualElement anchor, int row)
        {
            var choices = _model.GetIdentifierChoices(row);
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
                if ((uint)row >= (uint)_flows.arraySize)
                {
                    return;
                }

                _flows.serializedObject.Update();
                _flows.GetArrayElementAtIndex(row).FindPropertyRelative(IDENTIFIER_COLUMN).stringValue = (string)value;
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
            var problems = GetRowProblems(index);

            icon.visible = problems.Length > 0;
            icon.tooltip = problems.Length > 0 ? ProblemText.ToTooltip(problems) : string.Empty;
        }

        private RowProblem[] GetRowProblems(int index)
        {
            if (_model == null || (uint)index >= (uint)_flows.arraySize)
            {
                return Array.Empty<RowProblem>();
            }

            var problems = _model.GetRowProblems(index);

            if (_getExtraRowProblems == null)
            {
                return problems;
            }

            var extra = _getExtraRowProblems(_flows.GetArrayElementAtIndex(index), index);

            if (extra == null || extra.Length < 1)
            {
                return problems;
            }

            var combined = new RowProblem[problems.Length + extra.Length];
            problems.CopyTo(combined, 0);
            extra.CopyTo(combined, problems.Length);
            return combined;
        }

        private bool HasProblemInColumn(int index, string columnName)
        {
            var problems = GetRowProblems(index);

            for (var i = 0; i < problems.Length; i++)
            {
                var problemColumn = problems[i].IsIdentifierProblem ? IDENTIFIER_COLUMN : CONTAINER_NAME_COLUMN;

                if (problemColumn == columnName)
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
            _model.Sync(_flows, _resetNewRow);
            Refresh();
        }

        private void Refresh()
        {
            _flows.serializedObject.UpdateIfRequiredOrScript();
            _model = FlowDefinitionListModel.Create(_flows, _info);

            var listProblem = _model.GetListProblem();
            _listWarning.style.display = listProblem.HasProblem ? DisplayStyle.Flex : DisplayStyle.None;
            _listWarning.tooltip = listProblem.HasProblem
                ? ProblemText.ToTooltip(listProblem.ToProblem(), listProblem.ToFix())
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
