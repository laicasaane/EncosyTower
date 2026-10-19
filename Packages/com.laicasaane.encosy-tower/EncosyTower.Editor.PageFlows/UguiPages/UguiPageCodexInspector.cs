#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.UguiPages.Codex;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UguiPages
{
    /// <summary>
    /// The custom inspector of <see cref="UguiPageCodex"/> and its derived types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The inspector has two sections. The Context section shows the rows of
    /// <c>UguiPageFlowContextSettings.All</c>. The Flow Definitions section is a
    /// <see cref="FlowDefinitionsSection"/> with the columns Identifier, Kind, Override, Sorting Layer and Order.
    /// </para>
    /// <para>
    /// The inspector finds the scope collection from the <see cref="IUguiPageCodexOnInitialize"/> component when it
    /// creates the inspector. Sorting Layer and Order are enabled only while Override is on; the row sets this when it
    /// binds. A flow definition that Sync adds starts as <see cref="PageFlowKind.PageStackSingle"/>, without override,
    /// with sorting layer 0 and order 0.
    /// </para>
    /// <para>
    /// With several objects selected, the Context section is editable and the Flow Definitions section shows only a
    /// note.
    /// </para>
    /// </remarks>
    /// <seealso cref="FlowDefinitionsSection"/>
    [CustomEditor(typeof(UguiPageCodex), true)]
    [CanEditMultipleObjects]
    public class UguiPageCodexInspector : UnityEditor.Editor
    {
        private const string FLOWS = nameof(UguiPageCodex._flows);
        private const string FLOW_CONTEXT = nameof(UguiPageCodex._flowContext);
        private const string KIND = "kind";
        private const string OVERRIDE_SORTING_LAYER = "overrideSortingLayer";
        private const string SORTING_LAYER = "sortingLayer";
        private const string SORTING_LAYER_VALUE = nameof(SerializableSortingLayer.value);
        private const string SORTING_ORDER_IN_LAYER = "sortingOrderInLayer";

        /// <inheritdoc/>
        /// <remarks>
        /// Returns the root with the Context and Flow Definitions sections.
        /// </remarks>
        public override VisualElement CreateInspectorGUI()
        {
            var root = PageFlowContextSection.CreateRoot();
            var context = serializedObject.FindProperty(FLOW_CONTEXT);

            root.Add(PageFlowContextSection.Create(context, OpenSettings, UguiPageFlowContextSettings.All));

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
                , ResetNewDefinition
            ));

            return root;
        }

        private static void OpenSettings()
        {
            UguiPageFlowSettings.Instance.OpenSettingsWindow();
        }

        /// <summary>
        /// Sets the fields of a flow definition that Sync adds, except the identifier, to their defaults.
        /// </summary>
        /// <param name="definition">The serialized flow definition.</param>
        /// <remarks>
        /// A serialized field that is added to the runtime flow definition needs its default here too.
        /// </remarks>
        internal static void ResetNewDefinition(SerializedProperty definition)
        {
            definition.FindPropertyRelative(KIND).enumValueIndex = (int)PageFlowKind.PageStackSingle;
            definition.FindPropertyRelative(OVERRIDE_SORTING_LAYER).boolValue = false;
            definition.FindPropertyRelative(SORTING_LAYER).FindPropertyRelative(SORTING_LAYER_VALUE).intValue = 0;
            definition.FindPropertyRelative(SORTING_ORDER_IN_LAYER).intValue = 0;
        }

        private static FlowDefinitionColumn[] CreateColumns()
        {
            var titles = PageFlowsViewResources.Get().Columns;

            return new FlowDefinitionColumn[] {
                new(
                      Name: FlowDefinitionsSection.IDENTIFIER_COLUMN
                    , Title: titles.Identifier
                    , Width: 16f
                    , Grow: 1.15f
                    , MakeCell: null
                    , BindCell: null
                ),
                new(
                      Name: KIND
                    , Title: titles.Kind
                    , Width: 0f
                    , Grow: 1.3f
                    , MakeCell: static () => FlowDefinitionsSection.CreateCell(new EnumField())
                    , BindCell: static (cell, definition) => cell.Q<EnumField>()
                        .BindProperty(definition.FindPropertyRelative(KIND))
                ),
                new(
                      Name: OVERRIDE_SORTING_LAYER
                    , Title: titles.Override
                    , Width: 62f
                    , Grow: 0f
                    , MakeCell: static () => FlowDefinitionsSection.CreateCenteredCell(new Toggle())
                    , BindCell: static (cell, definition) => cell.Q<Toggle>()
                        .BindProperty(definition.FindPropertyRelative(OVERRIDE_SORTING_LAYER))
                ),
                new(
                      Name: SORTING_LAYER
                    , Title: titles.SortingLayer
                    , Width: 0f
                    , Grow: 1f
                    , MakeCell: static () => FlowDefinitionsSection.CreateCell(
                          new PropertyField(property: null, label: string.Empty)
                    )
                    , BindCell: static (cell, definition) => BindWhileOverridden<PropertyField>(
                          cell
                        , definition
                        , SORTING_LAYER
                    )
                ),
                new(
                      Name: SORTING_ORDER_IN_LAYER
                    , Title: titles.Order
                    , Width: 54f
                    , Grow: 0f
                    , MakeCell: static () => FlowDefinitionsSection.CreateCell(new IntegerField())
                    , BindCell: static (cell, definition) => BindWhileOverridden<IntegerField>(
                          cell
                        , definition
                        , SORTING_ORDER_IN_LAYER
                    )
                ),
            };
        }

        /// <summary>
        /// Binds a field of a cell and enables the cell only while the flow definition overrides the sorting layer.
        /// </summary>
        /// <typeparam name="TField">The type of the field element to find in the cell.</typeparam>
        /// <param name="cell">The cell of the list.</param>
        /// <param name="definition">The serialized flow definition.</param>
        /// <param name="name">The name of the serialized field to bind.</param>
        /// <remarks>
        /// The enabled state is set when the row binds, from the value at that time.
        /// </remarks>
        private static void BindWhileOverridden<TField>(VisualElement cell, SerializedProperty definition, string name)
            where TField : VisualElement, IBindable
        {
            cell.Q<TField>().BindProperty(definition.FindPropertyRelative(name));
            cell.SetEnabled(definition.FindPropertyRelative(OVERRIDE_SORTING_LAYER).boolValue);
        }
    }
}

#endif
