using System;
using EncosyTower.Editor.PageFlows;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Tests.Editor.PageFlows
{
    using Row = FlowDefinitionTestAsset.Row;

    public sealed class FlowDefinitionListModelTests
    {
        private const int LIST = 1;
        private const int MULTI_PAGE_STACK = 2;

        private static readonly string[] s_scopes = { "Screen", "Popup", "FreeTop" };

        private static readonly PageFlowScopeCollectionInfo s_info = new(
              TypeName: "GamePageFlowScopes"
            , ScopeIdentifiers: s_scopes
            , Problem: ScopeCollectionProblem.None
            , InitializerInterfaceName: string.Empty
        );

        private FlowDefinitionTestAsset _asset;

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<FlowDefinitionTestAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_asset);
        }

        [Test]
        public void RowProblems_DescribeEmptyUnknownAndDuplicateIdentifiers()
        {
            var model = new FlowDefinitionListModel(new[] { "", "Overlay", "Screen", "Popup", "Screen" }, s_info);

            AssertProblem(
                  model.GetRowProblems(0)
                , "This row has no identifier, so the codex cannot match it to a scope."
                , "Choose a scope from the Identifier dropdown, or press Sync Scopes."
            );

            AssertProblem(
                  model.GetRowProblems(1)
                , "'Overlay' is not a scope of GamePageFlowScopes."
                , "Choose one of Screen, Popup, FreeTop, add a PageFlowScope property named Overlay to "
                    + "GamePageFlowScopes, or press Sync Scopes."
            );

            AssertProblem(
                  model.GetRowProblems(2)
                , "'Screen' is also used by row 5. Each scope can have only one flow."
                , "Choose another scope for one of these rows, or press Sync Scopes (it keeps the first row)."
            );

            CollectionAssert.IsEmpty(model.GetRowProblems(3));

            AssertProblem(
                  model.GetRowProblems(4)
                , "'Screen' is also used by row 3. Each scope can have only one flow."
                , "Choose another scope for one of these rows, or press Sync Scopes (it keeps the first row)."
            );
        }

        [Test]
        public void ListProblem_UsesSingularAndPluralForms()
        {
            var one = new FlowDefinitionListModel(new[] { "Screen", "Popup" }, s_info).GetListProblem();
            var two = new FlowDefinitionListModel(new[] { "Screen" }, s_info).GetListProblem();
            var none = new FlowDefinitionListModel(s_scopes, s_info).GetListProblem();

            Assert.AreEqual("FreeTop has no flow definition, so the codex cannot create that flow.", one.ToProblem());
            Assert.AreEqual(
                  "Popup, FreeTop have no flow definition, so the codex cannot create those flows."
                , two.ToProblem()
            );
            Assert.AreEqual("Press Sync Scopes.", two.ToFix());
            Assert.IsFalse(none.HasProblem);
        }

        [Test]
        public void IdentifierChoices_DisableScopesInUseAndListUnknownValueFirst()
        {
            var model = new FlowDefinitionListModel(new[] { "Screen", "Overlay" }, s_info);
            var screenRow = model.GetIdentifierChoices(0);
            var unknownRow = model.GetIdentifierChoices(1);

            Assert.AreEqual(3, screenRow.Length);
            Assert.AreEqual(Choice("Screen", isChecked: true, disabled: false, note: ""), screenRow[0]);
            Assert.AreEqual(Choice("Popup", isChecked: false, disabled: false, note: ""), screenRow[1]);

            Assert.AreEqual(4, unknownRow.Length);
            Assert.AreEqual(
                  new IdentifierChoice(
                        Value: "Overlay"
                      , Checked: true
                      , Disabled: true
                      , Note: "not a scope"
                      , SeparatorAfter: true
                  )
                , unknownRow[0]
            );
            Assert.AreEqual(Choice("Screen", isChecked: false, disabled: true, note: "in use"), unknownRow[1]);
            Assert.AreEqual(Choice("FreeTop", isChecked: false, disabled: false, note: ""), unknownRow[3]);
        }

        [Test]
        public void Sync_KeepsReassignsRemovesAndAppendsInOneUndoStep()
        {
            _asset.flows = new[] {
                new Row { identifier = "Screen", kind = 0 },
                new Row { identifier = "Screen", kind = LIST },
                new Row { identifier = "Overlay", kind = MULTI_PAGE_STACK },
                new Row { identifier = "", kind = 0 },
            };

            using var serializedObject = new SerializedObject(_asset);
            var flows = serializedObject.FindProperty(nameof(FlowDefinitionTestAsset.flows));
            var model = FlowDefinitionListModel.Create(flows, s_info);

            Assert.IsTrue(model.CanSync);

            Undo.IncrementCurrentGroup();
            model.Sync(flows, ResetRow);

            CollectionAssert.AreEqual(
                  new[] {
                      new Row { identifier = "Screen", kind = 0 },
                      new Row { identifier = "Popup", kind = LIST },
                      new Row { identifier = "FreeTop", kind = MULTI_PAGE_STACK },
                  }
                , _asset.flows
            );

            Assert.IsFalse(FlowDefinitionListModel.Create(flows, s_info).CanSync);

            Undo.PerformUndo();

            Assert.AreEqual(4, _asset.flows.Length);
            Assert.AreEqual("Overlay", _asset.flows[2].identifier);
        }

        [Test]
        public void Sync_OnEmptyList_AppendsEveryScopeWithResetRows()
        {
            using var serializedObject = new SerializedObject(_asset);
            var flows = serializedObject.FindProperty(nameof(FlowDefinitionTestAsset.flows));

            FlowDefinitionListModel.Create(flows, s_info).Sync(flows, ResetRow);

            CollectionAssert.AreEqual(
                  new[] {
                      new Row { identifier = "Screen", kind = MULTI_PAGE_STACK },
                      new Row { identifier = "Popup", kind = MULTI_PAGE_STACK },
                      new Row { identifier = "FreeTop", kind = MULTI_PAGE_STACK },
                  }
                , _asset.flows
            );
        }

        [Test]
        public void ValidList_CannotSync()
        {
            Assert.IsFalse(new FlowDefinitionListModel(s_scopes, s_info).CanSync);
        }

        [Test]
        public void MissingScopeCollection_CannotSyncAndReportsNoRowProblem()
        {
            var info = PageFlowScopeCollectionInfo.Failed(ScopeCollectionProblem.NoInitializer, typeof(IDisposable));
            var model = new FlowDefinitionListModel(new[] { "" }, info);

            Assert.IsFalse(model.CanSync);
            CollectionAssert.IsEmpty(model.GetRowProblems(0));
            Assert.AreEqual("No initializer component found", info.ToProblemText());
            StringAssert.Contains("No component on this GameObject implements IDisposable", info.ToTooltip());
        }

        private static IdentifierChoice Choice(string value, bool isChecked, bool disabled, string note)
            => new(Value: value, Checked: isChecked, Disabled: disabled, Note: note, SeparatorAfter: false);

        private static void ResetRow(SerializedProperty row)
        {
            row.FindPropertyRelative(nameof(Row.kind)).intValue = MULTI_PAGE_STACK;
        }

        private static void AssertProblem(RowProblem[] problems, string problem, string fix)
        {
            Assert.AreEqual(1, problems.Length);
            Assert.AreEqual(problem, problems[0].ToProblem());
            Assert.AreEqual(fix, problems[0].ToFix());
        }
    }
}
