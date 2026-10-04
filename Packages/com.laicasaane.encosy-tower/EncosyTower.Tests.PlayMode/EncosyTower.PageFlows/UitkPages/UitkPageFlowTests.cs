using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    using static UitkPagesHarness;

    public sealed class UitkPageFlowTests
    {
        private UitkPagesHarness _harness;

        [TearDown]
        public void TearDown()
        {
            _harness?.Dispose();
            _harness = null;
        }

        [Test]
        public async Task SinglePageStack_ShowsTheNewPageAndHidesThePreviousOne()
        {
            var stack = await CreateFlowAsync<UitkSinglePageStack>(PageFlowKind.SinglePageStack);

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var first));

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var second));

            Assert.AreNotSame(first, second);
            Assert.IsFalse(IsDisplayed(GetSlot(first)));
            Assert.IsTrue(IsDisplayed(GetSlot(second)));
            AssertDrawnLast(stack, GetSlot(second));

            await stack.HideActivePageAsync(ZeroDuration, CancellationToken.None);

            Assert.IsFalse(stack.CurrentPage.TryGetValue(out _));
            Assert.IsFalse(IsDisplayed(GetSlot(second)));
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(2, pooled);
        }

        [Test]
        public async Task MultiPageStack_DrawsTheTopPageLastAndPopsIt()
        {
            var stack = await CreateFlowAsync<UitkMultiPageStack>(PageFlowKind.MultiPageStack);

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var bottom));

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var top));

            Assert.IsTrue(IsDisplayed(GetSlot(bottom)));
            Assert.IsTrue(IsDisplayed(GetSlot(top)));
            AssertDrawnLast(stack, GetSlot(top));

            await stack.HideActivePageAsync(ZeroDuration, CancellationToken.None);

            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var current));
            Assert.AreSame(bottom, current);
            Assert.IsFalse(IsDisplayed(GetSlot(top)));
            Assert.IsTrue(IsDisplayed(GetSlot(bottom)));
        }

        [Test]
        public async Task SinglePageList_ShowsOnePageAtATime()
        {
            var list = await CreateFlowAsync<UitkSinglePageList>(PageFlowKind.SinglePageList);

            await list.AddPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            await list.AddPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            var first = list.Pages[0];
            var second = list.Pages[1];

            Assert.IsFalse(IsDisplayed(GetSlot(first)));
            Assert.IsFalse(IsDisplayed(GetSlot(second)));

            await list.ShowPageAtIndexAsync(index: 0, context: ZeroDuration, token: CancellationToken.None);
            Assert.IsTrue(IsDisplayed(GetSlot(first)));

            await list.ShowPageAtIndexAsync(index: 1, context: ZeroDuration, token: CancellationToken.None);
            Assert.IsFalse(IsDisplayed(GetSlot(first)));
            Assert.IsTrue(IsDisplayed(GetSlot(second)));
            AssertDrawnLast(list, GetSlot(second));

            await list.HideActivePageAsync(ZeroDuration, CancellationToken.None);
            Assert.IsFalse(IsDisplayed(GetSlot(second)));
        }

        [Test]
        public async Task MultiPageList_ShowsAndHidesPagesByIndex()
        {
            var list = await CreateFlowAsync<UitkMultiPageList>(PageFlowKind.MultiPageList);

            await list.AddPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            await list.AddPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            await list.ShowPageAtIndexAsync(index: 0, context: ZeroDuration, token: CancellationToken.None);
            await list.ShowPageAtIndexAsync(index: 1, context: ZeroDuration, token: CancellationToken.None);

            var first = list.Pages[0];
            var second = list.Pages[1];

            Assert.IsTrue(IsDisplayed(GetSlot(first)));
            Assert.IsTrue(IsDisplayed(GetSlot(second)));
            AssertDrawnLast(list, GetSlot(second));

            await list.HidePageAtIndexAsync(index: 0, context: ZeroDuration, token: CancellationToken.None);

            Assert.IsFalse(IsDisplayed(GetSlot(first)));
            Assert.IsTrue(IsDisplayed(GetSlot(second)));
        }

        [Test]
        public async Task Pool_ReusesAReturnedSlot()
        {
            var stack = await CreateFlowAsync<UitkSinglePageStack>(PageFlowKind.SinglePageStack);

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var first));

            await stack.HideActivePageAsync(ZeroDuration, CancellationToken.None);
            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var second));

            Assert.AreSame(first, second);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(0, pooled);
        }

        [Test]
        public async Task DestroyReturnOperation_RemovesTheSlot()
        {
            var stack = await CreateFlowAsync<UitkSinglePageStack>(PageFlowKind.SinglePageStack);

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var page));

            var slot = GetSlot(page);

            await stack.HideActivePageAsync(
                  ZeroDuration with { ReturnOperation = PageReturnOperation.Destroy }
                , CancellationToken.None
            );

            Assert.IsNull(slot.parent);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(0, pooled);
        }

        [Test]
        public async Task PrepoolAndTrim_ChangeThePooledCount()
        {
            var stack = await CreateFlowAsync<UitkSinglePageStack>(PageFlowKind.SinglePageStack);

            await stack.PrepoolPageAsync(pageAssetKey: ELEMENT_PAGE, amount: 3, token: CancellationToken.None);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var prepooled));
            Assert.AreEqual(3, prepooled);

            stack.TrimPool(pageAssetKey: ELEMENT_PAGE, amountToKeep: 1);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var trimmed));
            Assert.AreEqual(1, trimmed);
        }

        [Test]
        public async Task BehaviourPage_RendersItsUxmlAndActivatesItsGameObject()
        {
            var stack = await CreateFlowAsync<UitkSinglePageStack>(PageFlowKind.SinglePageStack);

            await stack.ShowPageAsync(BEHAVIOUR_PAGE, ZeroDuration, CancellationToken.None);

            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var page));
            Assert.IsInstanceOf<UitkPageBehaviour>(page);

            var behaviour = (UitkPageBehaviour)page;

            Assert.IsTrue(behaviour.gameObject.activeInHierarchy);
            Assert.IsNotNull(behaviour.Slot.Q<Button>("behaviour-button"));
            Assert.IsTrue(IsDisplayed(behaviour.Slot));

            await stack.HideActivePageAsync(ZeroDuration, CancellationToken.None);

            Assert.IsFalse(behaviour.gameObject.activeSelf);
        }

        [Test]
        public async Task UnsupportedAsset_LogsAnErrorAndShowsNothing()
        {
            var stack = await CreateFlowAsync<UitkSinglePageStack>(PageFlowKind.SinglePageStack);

            LogAssert.Expect(LogType.Error, new Regex("must be a VisualTreeAsset or a GameObject"));
            await stack.ShowPageAsync(NOT_A_PAGE, ZeroDuration, CancellationToken.None);

            Assert.IsFalse(stack.CurrentPage.TryGetValue(out _));
        }

        [Test]
        public async Task UxmlWithTwoPages_LogsAnErrorAndShowsNothing()
        {
            var stack = await CreateFlowAsync<UitkSinglePageStack>(PageFlowKind.SinglePageStack);

            LogAssert.Expect(LogType.Error, new Regex("must contain exactly one element"));
            await stack.ShowPageAsync(TWO_PAGES, ZeroDuration, CancellationToken.None);

            Assert.IsFalse(stack.CurrentPage.TryGetValue(out _));
        }

        [Test]
        public async Task ContainerName_PlacesTheFlowInsideTheNamedElement()
        {
            var layout = Resources.Load<VisualTreeAsset>(LAYOUT);

            _harness = await CreateAsync(
                  layout
                , ("Screen", PageFlowKind.SinglePageStack, "")
                , ("Popup", PageFlowKind.SinglePageStack, "PopupLayer")
            );

            var screen = _harness.GetFlow<UitkSinglePageStack>("Screen");
            var popup = _harness.GetFlow<UitkSinglePageStack>("Popup");

            Assert.AreSame(_harness.Codex.CodexRoot, screen.FlowElement.parent);
            Assert.AreEqual("PopupLayer", popup.FlowElement.parent.name);
        }

        [Test]
        public async Task MissingContainer_LogsAndFallsBackToTheCodexRoot()
        {
            LogAssert.Expect(LogType.Error, new Regex("Cannot find the container 'Nope'"));

            _harness = await CreateAsync(
                  ("Screen", PageFlowKind.SinglePageStack, "")
                , ("Popup", PageFlowKind.SinglePageStack, "Nope")
            );

            var popup = _harness.GetFlow<UitkSinglePageStack>("Popup");

            Assert.AreSame(_harness.Codex.CodexRoot, popup.FlowElement.parent);
        }

        [Test]
        public async Task UIDocumentReload_ReaddsTheCodexRootAndKeepsPageState()
        {
            var stack = await CreateFlowAsync<UitkSinglePageStack>(PageFlowKind.SinglePageStack);

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var page));

            var element = (TestElementPage)page;
            element.Counter = 5;

            var document = _harness.GameObject.GetComponent<UIDocument>();
            var oldRoot = document.rootVisualElement;
            var oldVersion = _harness.Codex.RootVersion;

            document.enabled = false;
            await WaitFramesAsync(2);
            document.enabled = true;

            await WaitUntilAsync(() => _harness.Codex.RootVersion > oldVersion);

            Assert.AreNotSame(oldRoot, document.rootVisualElement);
            Assert.AreSame(document.rootVisualElement, _harness.Codex.CodexRoot.parent);
            Assert.AreEqual(5, element.Counter);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var current));
            Assert.AreSame(page, current);
        }

        [Test]
        public async Task Initializer_IsCalledOnceWithTheScopes()
        {
            _harness = await CreateAsync(
                  ("Screen", PageFlowKind.SinglePageStack, "")
                , ("Popup", PageFlowKind.MultiPageStack, "")
            );

            Assert.AreEqual(1, _harness.Initializer.CallCount);
            Assert.IsTrue(_harness.Codex.TryGetFlowScope("Screen", out var screen));
            Assert.AreEqual(screen, _harness.Initializer.Scopes.Screen);
        }

        [Test]
        public async Task Initializer_IsNotCalledOnMismatch()
        {
            LogAssert.Expect(LogType.Error, new Regex("do not match the properties"));

            _harness = await CreateAsync(("Screen", PageFlowKind.SinglePageStack, ""));

            Assert.AreEqual(0, _harness.Initializer.CallCount);
        }

        private async Task<T> CreateFlowAsync<T>(PageFlowKind kind)
            where T : UitkPageFlow
        {
            _harness = await CreateAsync(
                  ("Screen", kind, "")
                , ("Popup", PageFlowKind.SinglePageStack, "")
            );

            return _harness.GetFlow<T>("Screen");
        }

        private static void AssertDrawnLast(UitkPageFlow flow, VisualElement slot)
        {
            var element = flow.FlowElement;
            var count = element.childCount;

            Assert.AreSame(flow.Blocker, element[count - 1]);
            Assert.AreSame(slot, element[count - 2]);
        }
    }
}
