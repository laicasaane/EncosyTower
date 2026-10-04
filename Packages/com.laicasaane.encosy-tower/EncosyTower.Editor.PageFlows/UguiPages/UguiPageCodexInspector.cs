#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UguiPages
{
    [CustomEditor(typeof(UguiPageCodex), true)]
    [CanEditMultipleObjects]
    public class UguiPageCodexInspector : UnityEditor.Editor
    {
        private const string FLOWS = nameof(UguiPageCodex._flows);
        private const string FLOW_CONTEXT = nameof(UguiPageCodex._flowContext);
        private const string KIND = "kind";
        private const string OVERRIDE_SORTING_LAYER = "overrideSortingLayer";
        private const string SORTING_LAYER = "sortingLayer";
        private const string SORTING_ORDER_IN_LAYER = "sortingOrderInLayer";

        public override VisualElement CreateInspectorGUI()
        {
            var root = PageFlowContextSection.CreateRoot();
            var context = serializedObject.FindProperty(FLOW_CONTEXT);

            root.Add(PageFlowContextSection.Create(context, OpenSettings, UguiPageFlowContextRows.Rows));

            var info = PageFlowScopeCollectionResolver.Resolve(
                  (UguiPageCodex)target
                , typeof(IUguiPageCodexOnInitialize)
                , typeof(UguiPageCodexInitializer<>)
            );

            root.Add(FlowDefinitionsSection.Create(
                  serializedObject
                , serializedObject.FindProperty(FLOWS)
                , info
                , CreateColumns()
                , ResetNewRow
            ));

            return root;
        }

        private static void OpenSettings()
        {
            UguiPageFlowSettings.Instance.OpenSettingsWindow();
        }

        private static void ResetNewRow(SerializedProperty row)
        {
            row.FindPropertyRelative(KIND).enumValueIndex = (int)PageFlowKind.SinglePageStack;
            row.FindPropertyRelative(OVERRIDE_SORTING_LAYER).boolValue = false;
            row.FindPropertyRelative(SORTING_ORDER_IN_LAYER).intValue = 0;
        }

        private static FlowDefinitionColumn[] CreateColumns()
            => new FlowDefinitionColumn[] {
                new(
                      Name: FlowDefinitionsSection.IDENTIFIER_COLUMN
                    , Title: "Identifier"
                    , Width: 16f
                    , Grow: 1.15f
                    , MakeCell: null
                    , BindCell: null
                ),
                new(
                      Name: KIND
                    , Title: "Kind"
                    , Width: 0f
                    , Grow: 1.3f
                    , MakeCell: static () => FlowDefinitionsSection.CreateCell(new EnumField())
                    , BindCell: static (cell, row) => cell.Q<EnumField>().BindProperty(row.FindPropertyRelative(KIND))
                ),
                new(
                      Name: OVERRIDE_SORTING_LAYER
                    , Title: "Override"
                    , Width: 62f
                    , Grow: 0f
                    , MakeCell: static () => FlowDefinitionsSection.CreateCenteredCell(new Toggle())
                    , BindCell: static (cell, row) => cell.Q<Toggle>()
                        .BindProperty(row.FindPropertyRelative(OVERRIDE_SORTING_LAYER))
                ),
                new(
                      Name: SORTING_LAYER
                    , Title: "Sorting Layer"
                    , Width: 0f
                    , Grow: 1f
                    , MakeCell: static () => FlowDefinitionsSection.CreateCell(
                          new PropertyField(property: null, label: string.Empty)
                    )
                    , BindCell: static (cell, row) => BindWhileOverridden<PropertyField>(cell, row, SORTING_LAYER)
                ),
                new(
                      Name: SORTING_ORDER_IN_LAYER
                    , Title: "Order"
                    , Width: 54f
                    , Grow: 0f
                    , MakeCell: static () => FlowDefinitionsSection.CreateCell(new IntegerField())
                    , BindCell: static (cell, row) => BindWhileOverridden<IntegerField>(
                          cell
                        , row
                        , SORTING_ORDER_IN_LAYER
                    )
                ),
            };

        private static void BindWhileOverridden<TField>(VisualElement cell, SerializedProperty row, string name)
            where TField : VisualElement, IBindable
        {
            cell.Q<TField>().BindProperty(row.FindPropertyRelative(name));
            cell.SetEnabled(row.FindPropertyRelative(OVERRIDE_SORTING_LAYER).boolValue);
        }
    }
}

#endif
