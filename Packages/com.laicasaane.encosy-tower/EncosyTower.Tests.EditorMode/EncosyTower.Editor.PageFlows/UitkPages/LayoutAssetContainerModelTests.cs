using System;
using EncosyTower.Editor.PageFlows;
using EncosyTower.Editor.PageFlows.UitkPages;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.PageFlows.UitkPages
{
    public sealed class LayoutAssetContainerModelTests
    {
        private const string ASSET_PATH = "Packages/com.laicasaane.encosy-tower/EncosyTower.Tests.EditorMode/"
            + "EncosyTower.Editor.PageFlows/UitkPages/TestAssets/layout-containers.uxml";

        private const string ASSET_NAME = "layout-containers";

        private VisualTreeAsset _asset;

        [SetUp]
        public void SetUp()
        {
            LayoutAssetContainerModel.ClearCache();
            _asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ASSET_PATH);
            Assert.That(_asset, Is.Not.Null);
        }

        [TearDown]
        public void TearDown()
        {
            LayoutAssetContainerModel.ClearCache();
        }

        [Test]
        public void Get_NoAsset_ReturnsEmpty()
        {
            var containers = LayoutAssetContainerModel.Get(null);

            Assert.IsFalse(containers.HasAsset);
            Assert.IsEmpty(containers.Items);
        }

        [Test]
        public void Get_CollectsNamedElementsWithHierarchyPaths()
        {
            var containers = LayoutAssetContainerModel.Get(_asset);

            Assert.IsTrue(containers.HasAsset);
            Assert.AreEqual(ASSET_NAME, containers.AssetName);
            CollectionAssert.AreEqual(
                  new[] {
                      new LayoutAssetContainer("safe-area", "safe-area"),
                      new LayoutAssetContainer("content", "safe-area / content"),
                      new LayoutAssetContainer("overlay-root", "safe-area / content / overlay-root"),
                      new LayoutAssetContainer("content", "VisualElement / content"),
                      new LayoutAssetContainer("template-host", "template-host"),
                      new LayoutAssetContainer("popup-layer", "template-host / popup-layer"),
                  }
                , containers.Items
            );
        }

        [Test]
        public void CountOf_DuplicateName_ReturnsCountAndFirstPath()
        {
            var containers = LayoutAssetContainerModel.Get(_asset);

            Assert.AreEqual(2, containers.CountOf("content", out var firstPath));
            Assert.AreEqual("safe-area / content", firstPath);
            Assert.AreEqual(0, containers.CountOf("missing", out var missingPath));
            Assert.IsNull(missingPath);
        }

        [Test]
        public void Get_SameContentHash_ReturnsCachedContainers()
        {
            var first = LayoutAssetContainerModel.Get(_asset);
            var second = LayoutAssetContainerModel.Get(_asset);

            Assert.AreSame(first, second);
        }

        [Test]
        public void Get_ChangedContentHash_RebuildsContainers()
        {
            var originalHash = _asset.contentHash;
            var first = LayoutAssetContainerModel.Get(_asset);

            try
            {
                _asset.contentHash = originalHash + 1;
                var second = LayoutAssetContainerModel.Get(_asset);

                Assert.AreNotSame(first, second);
                CollectionAssert.AreEqual(first.Items, second.Items);
            }
            finally
            {
                _asset.contentHash = originalHash;
            }
        }

        [Test]
        public void TryGetProblem_EmptyOrUniqueName_ReportsNothing()
        {
            var containers = LayoutAssetContainerModel.Get(_asset);

            Assert.IsFalse(LayoutAssetContainerModel.TryGetProblem(string.Empty, containers, out _));
            Assert.IsFalse(LayoutAssetContainerModel.TryGetProblem(null, LayoutAssetContainers.Empty, out _));
            Assert.IsFalse(LayoutAssetContainerModel.TryGetProblem("overlay-root", containers, out _));
        }

        [Test]
        public void TryGetProblem_NameNotInLayout_DescribesTheFallback()
        {
            var containers = LayoutAssetContainerModel.Get(_asset);

            Assert.IsTrue(LayoutAssetContainerModel.TryGetProblem("missing", containers, out var problem));
            Assert.AreEqual(RowProblemKind.ContainerNotInLayout, problem.Kind);
            Assert.IsFalse(problem.IsIdentifierProblem);
            Assert.AreEqual(
                  "'missing' is not an element of Layout Asset layout-containers. Unless code creates it before "
                    + "the codex initializes, the flow goes under the codex root."
                , problem.ToProblem()
            );
            Assert.AreEqual(
                  "Press ⋯ and pick an element, or name an element of layout-containers 'missing'."
                , problem.ToFix()
            );
        }

        [Test]
        public void TryGetProblem_NameMatchesMany_NamesTheFirstPath()
        {
            var containers = LayoutAssetContainerModel.Get(_asset);

            Assert.IsTrue(LayoutAssetContainerModel.TryGetProblem("content", containers, out var problem));
            Assert.AreEqual(RowProblemKind.ContainerMatchesMany, problem.Kind);
            Assert.AreEqual(2, problem.MatchCount);
            Assert.AreEqual(
                  "'content' matches 2 elements of Layout Asset layout-containers; the first one "
                    + "(safe-area / content) is used."
                , problem.ToProblem()
            );
            Assert.AreEqual(
                  "Give the intended element a unique name in layout-containers, then pick it with ⋯."
                , problem.ToFix()
            );
        }

        [Test]
        public void TryGetProblem_WithoutLayoutAsset_DescribesTheRuntimeRequirement()
        {
            Assert.IsTrue(LayoutAssetContainerModel.TryGetProblem(
                  "content"
                , LayoutAssetContainers.Empty
                , out var problem
            ));

            Assert.AreEqual(RowProblemKind.ContainerWithoutLayout, problem.Kind);
            Assert.AreEqual(
                  "No Layout Asset is assigned, so the container 'content' must be created by code before the "
                    + "codex initializes; otherwise the flow goes under the codex root."
                , problem.ToProblem()
            );
            Assert.AreEqual(
                  "Assign a Layout Asset that contains an element named 'content', or clear the field to use the "
                    + "codex root."
                , problem.ToFix()
            );
        }

        [Test]
        public void GetChoices_StartsWithCodexRootAndChecksTheFirstMatch()
        {
            var containers = LayoutAssetContainerModel.Get(_asset);
            var choices = LayoutAssetContainerModel.GetChoices(containers, "content", otherRows: null);

            Assert.AreEqual(containers.Items.Length + 1, choices.Length);
            Assert.AreEqual(string.Empty, choices[0].Value);
            Assert.AreEqual(LayoutAssetContainerModel.CODEX_ROOT_LABEL, choices[0].Label);
            Assert.IsFalse(choices[0].Checked);
            Assert.IsTrue(choices[2].Checked);
            Assert.IsFalse(choices[4].Checked);
            Assert.AreEqual(1, Array.FindAll(choices, static choice => choice.Checked).Length);
        }

        [Test]
        public void GetChoices_EmptyCurrent_ChecksCodexRoot()
        {
            var choices = LayoutAssetContainerModel.GetChoices(LayoutAssetContainers.Empty, string.Empty, null);

            Assert.AreEqual(1, choices.Length);
            Assert.IsTrue(choices[0].Checked);
        }

        [Test]
        public void GetChoices_NotesWhichOtherRowsUseEachContainer()
        {
            var containers = LayoutAssetContainerModel.Get(_asset);
            var otherRows = new[] {
                ("Popup", "content"),
                (string.Empty, "content"),
                ("FreeTop", "overlay-root"),
            };

            var choices = LayoutAssetContainerModel.GetChoices(containers, string.Empty, otherRows);

            Assert.AreEqual(string.Empty, choices[1].Note);
            Assert.AreEqual("used by Popup, (no identifier)", choices[2].Note);
            Assert.AreEqual("used by FreeTop", choices[3].Note);
            Assert.AreEqual("safe-area / content / overlay-root", choices[3].Path);
        }
    }
}
