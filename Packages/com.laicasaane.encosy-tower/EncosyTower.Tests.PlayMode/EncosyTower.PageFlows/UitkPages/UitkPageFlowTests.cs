using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.AssetKeys;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.Loading;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    using static UitkPagesHarness;

    public sealed class UitkPageFlowTests
    {
        private const string MISSING_PAGE = "uitk-test-missing";

        private const string NO_THEME_WARNING
            = "No Theme Style Sheet set to PanelSettings , UI will not render properly";

        private readonly List<ISubscription> _errorDeliverySubscriptions = new();
        private UitkPagesHarness _harness;

        [TearDown]
        public void TearDown()
        {
            _errorDeliverySubscriptions.Unsubscribe();
            _harness?.Dispose();
            _harness = null;
        }

        [Test]
        public async Task PageStackSingle_ShowsTheNewPageAndHidesThePreviousOne()
        {
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

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
        public async Task PageStackMulti_DrawsTheTopPageLastAndPopsIt()
        {
            var stack = await CreateFlowAsync<UitkPageStackMulti>(PageFlowKind.PageStackMulti);

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
        public async Task PageListSingle_ShowsOnePageAtATime()
        {
            var list = await CreateFlowAsync<UitkPageListSingle>(PageFlowKind.PageListSingle);

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
        public async Task PageListMulti_ShowsAndHidesPagesByIndex()
        {
            var list = await CreateFlowAsync<UitkPageListMulti>(PageFlowKind.PageListMulti);

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
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

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
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

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
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

            var result = await stack.PrepoolPageAsync(
                  pageAssetKey: ELEMENT_PAGE
                , amount: 3
                , token: CancellationToken.None
            );

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var prepooled));
            Assert.AreEqual(3, prepooled);

            stack.TrimPool(pageAssetKey: ELEMENT_PAGE, amountToKeep: 1);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var trimmed));
            Assert.AreEqual(1, trimmed);
        }

        [Test]
        public async Task BehaviourPage_RendersItsUxmlAndActivatesItsGameObject()
        {
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

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
        public async Task UnsupportedAsset_ReturnsAssetLoadFailedAndShowsNothing()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

            var result = await stack.ShowPageAsync(NOT_A_PAGE, ZeroDuration, CancellationToken.None);

            Assert.IsTrue(result.TryGetFailure(out var error));
            var reason = PageAssetLoadError.UnsupportedAsset(NOT_A_PAGE, typeof(TextAsset));
            Assert.AreEqual(PageFlowError.AssetLoadFailed(NOT_A_PAGE, reason).ToMessage(), error.ToMessage());
            Assert.IsFalse(stack.CurrentPage.TryGetValue(out _));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task UxmlWithTwoPages_ReturnsAssetNotSinglePageAndShowsNothing()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

            var result = await stack.ShowPageAsync(TWO_PAGES, ZeroDuration, CancellationToken.None);

            Assert.IsTrue(result.TryGetFailure(out var error));

            Assert.AreEqual(
                  PageFlowError.AssetNotSinglePage(assetKey: TWO_PAGES, pageCount: 2).ToMessage()
                , error.ToMessage()
            );

            Assert.IsFalse(stack.CurrentPage.TryGetValue(out _));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task ContainerName_PlacesTheFlowInsideTheNamedElement()
        {
            var layout = Resources.Load<VisualTreeAsset>(LAYOUT);

            _harness = await CreateAsync(
                  layout
                , ("Screen", PageFlowKind.PageStackSingle, "")
                , ("Popup", PageFlowKind.PageStackSingle, "PopupLayer")
            );

            var screen = _harness.GetFlow<UitkPageStackSingle>("Screen");
            var popup = _harness.GetFlow<UitkPageStackSingle>("Popup");

            Assert.AreSame(_harness.Codex.CodexRoot, screen.FlowElement.parent);
            Assert.AreEqual("PopupLayer", popup.FlowElement.parent.name);
        }

        [Test]
        public async Task MissingContainer_LogsAndFallsBackToTheCodexRoot()
        {
            LogAssert.Expect(LogType.Error, new Regex("Cannot find the container 'Nope'"));

            _harness = await CreateAsync(
                  ("Screen", PageFlowKind.PageStackSingle, "")
                , ("Popup", PageFlowKind.PageStackSingle, "Nope")
            );

            var popup = _harness.GetFlow<UitkPageStackSingle>("Popup");

            Assert.AreSame(_harness.Codex.CodexRoot, popup.FlowElement.parent);
        }

        [Test]
        public async Task UIDocumentReload_ReaddsTheCodexRootAndKeepsPageState()
        {
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

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
                  ("Screen", PageFlowKind.PageStackSingle, "")
                , ("Popup", PageFlowKind.PageStackMulti, "")
            );

            Assert.AreEqual(1, _harness.Initializer.CallCount);
            Assert.IsTrue(_harness.Codex.TryGetFlowScope("Screen", out var screen));
            Assert.AreEqual(screen, _harness.Initializer.Scopes.Screen);
        }

        [Test]
        public async Task Initializer_IsNotCalledOnMismatch()
        {
            LogAssert.Expect(LogType.Error, new Regex("do not match the properties"));

            _harness = await CreateAsync(("Screen", PageFlowKind.PageStackSingle, ""));

            Assert.AreEqual(0, _harness.Initializer.CallCount);
        }

        [Test]
        public async Task Prepool_ZeroAmountReportsInvalidPrepoolAmountAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

            var result = await stack.PrepoolPageAsync(
                  pageAssetKey: ELEMENT_PAGE
                , amount: 0
                , token: CancellationToken.None
            );

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.AreEqual(PageFlowError.InvalidPrepoolAmount(amount: 0).ToMessage(), error.ToMessage());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task Prepool_UnsupportedAssetReportsAssetLoadFailedAndLeavesThePoolEmptyAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

            var result = await stack.PrepoolPageAsync(
                  pageAssetKey: NOT_A_PAGE
                , amount: 1
                , token: CancellationToken.None
            );

            Assert.IsTrue(result.TryGetFailure(out var error));
            var reason = PageAssetLoadError.UnsupportedAsset(NOT_A_PAGE, typeof(TextAsset));
            Assert.AreEqual(PageFlowError.AssetLoadFailed(NOT_A_PAGE, reason).ToMessage(), error.ToMessage());
            Assert.IsTrue(stack.TryGetPooledCount(NOT_A_PAGE, out var count));
            Assert.AreEqual(0, count);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task Prepool_MissingResourceReportsAssetLoadFailedAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

            var result = await stack.PrepoolPageAsync(
                  pageAssetKey: MISSING_PAGE
                , amount: 1
                , token: CancellationToken.None
            );

            Assert.IsTrue(result.TryGetFailure(out var error));

            StringAssert.StartsWith(
                  "Cannot load the page asset with key 'uitk-test-missing': Resources: "
                , error.ToMessage()
            );

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task Prepool_UxmlWithTwoPagesReportsAssetNotSinglePageAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);

            var result = await stack.PrepoolPageAsync(
                  pageAssetKey: TWO_PAGES
                , amount: 1
                , token: CancellationToken.None
            );

            Assert.IsTrue(result.TryGetFailure(out var error));

            Assert.AreEqual(
                  PageFlowError.AssetNotSinglePage(assetKey: TWO_PAGES, pageCount: 2).ToMessage()
                , error.ToMessage()
            );

            Assert.IsTrue(stack.TryGetPooledCount(TWO_PAGES, out var count));
            Assert.AreEqual(0, count);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PrepoolMessage_FailurePublishesPageFlowFailedMessageAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            var failures = new List<PageFlowFailedMessage>();
            var subscriptions = new List<ISubscription>();

            var subscriber = stack.Context.Subscriber.Scope(stack.Context.FlowScope)
                .WithState(failures)
                .WithSubscriptions(subscriptions);

            PageFlowFailedMessage.Subscribe(in subscriber, Handle);

            var publisher = stack.Context.Publisher.Scope(stack.Context.FlowScope);

            var publishingContext = PublishingContext.Default(
                  logger: stack.Context.Logger
                , token: CancellationToken.None
            );

            var published = await PrepoolPageMessage.Async.Publish(
                  in publisher
                , new PrepoolPageMessage(AssetKey: TWO_PAGES, Amount: 1)
                , publishingContext
            );

            subscriptions.Unsubscribe();

            Assert.IsTrue(published.IsSuccess);
            Assert.AreEqual(1, failures.Count);

            Assert.AreEqual(
                  PageFlowError.AssetNotSinglePage(assetKey: TWO_PAGES, pageCount: 2).ToMessage()
                , failures[0].Error.ToMessage()
            );

            LogAssert.NoUnexpectedReceived();

            static void Handle(List<PageFlowFailedMessage> log, PageFlowFailedMessage message)
            {
                log.Add(message);
            }
        }

        [Test]
        public async Task NavigatedSubscriber_ExceptionPublishesPageFlowFailedMessageAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            var exception = new InvalidOperationException("navigated subscriber failed");
            var failures = new List<PageFlowFailedMessage>();
            var subscriptions = _errorDeliverySubscriptions;

            var failureSubscriber = stack.Context.Subscriber.Scope(stack.Context.FlowScope)
                .WithState(failures)
                .WithSubscriptions(subscriptions);

            PageFlowFailedMessage.Subscribe(in failureSubscriber, Handle);

            var navigatedSubscriber = stack.Context.Subscriber.Scope(stack.Context.FlowScope)
                .WithState(exception)
                .WithSubscriptions(subscriptions);

            PageNavigatedMessage.Async.Subscribe(in navigatedSubscriber, Throw);

            var result = await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            subscriptions.Unsubscribe();

            Assert.IsTrue(result.IsFailure);
            Assert.IsTrue(result.IsApplied());
            Assert.IsTrue(result.GetFailureOrThrow()._error.TryGetValue(out PageFlowError.Error.Exception wrapped));
            Assert.AreSame(exception, wrapped.Ex);
            Assert.AreEqual(expected: 1, actual: failures.Count);
            Assert.AreEqual(expected: result.GetFailureOrThrow(), actual: failures[0].Error);
            LogAssert.NoUnexpectedReceived();

            static void Handle(List<PageFlowFailedMessage> log, PageFlowFailedMessage message)
            {
                log.Add(message);
            }

            static UnityTask Throw(
                  InvalidOperationException ex
                , PageNavigatedMessage.Async message
                , PublishingContext publishingContext
            )
                => throw ex;
        }

        [Test]
        public async Task NavigatedSubscriber_ExceptionWithoutFailedMessageSubscriberIsReturnedNotLoggedAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            var exception = new InvalidOperationException("navigated subscriber failed");
            var subscriptions = _errorDeliverySubscriptions;

            var navigatedSubscriber = stack.Context.Subscriber.Scope(stack.Context.FlowScope)
                .WithState(exception)
                .WithSubscriptions(subscriptions);

            PageNavigatedMessage.Async.Subscribe(in navigatedSubscriber, Throw);

            var result = await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            subscriptions.Unsubscribe();

            Assert.IsTrue(result.IsFailure);
            Assert.IsTrue(result.IsApplied());
            Assert.IsTrue(result.GetFailureOrThrow()._error.TryGetValue(out PageFlowError.Error.Exception wrapped));
            Assert.AreSame(exception, wrapped.Ex);
            LogAssert.NoUnexpectedReceived();

            static UnityTask Throw(
                  InvalidOperationException ex
                , PageNavigatedMessage.Async message
                , PublishingContext publishingContext
            )
                => throw ex;
        }

        [Test]
        public async Task PrepoolMessage_FailureWithoutFailedMessageSubscriberLogsNothingAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            var publisher = stack.Context.Publisher.Scope(stack.Context.FlowScope);

            var publishingContext = PublishingContext.Default(
                  logger: stack.Context.Logger
                , token: CancellationToken.None
            );

            var published = await PrepoolPageMessage.Async.Publish(
                  in publisher
                , new PrepoolPageMessage(AssetKey: ELEMENT_PAGE, Amount: 0)
                , publishingContext
            );

            Assert.IsTrue(published.IsSuccess);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task ShowPageMessage_WithAnAssetKeyShowsThePageAndPoolsItByThatKeyAsync()
        {
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            var key = new AssetKey(ELEMENT_PAGE);
            var receipt = new PageCallReceipt();
            var publisher = stack.Context.Publisher.Scope(stack.Context.FlowScope);

            var publishingContext = PublishingContext.Default(
                  logger: stack.Context.Logger
                , token: CancellationToken.None
            );

            var published = await ShowPageMessage.Async.Publish(
                  in publisher
                , new ShowPageMessage(key, ZeroDuration with { Receipt = receipt })
                , publishingContext
            );

            Assert.IsTrue(published.IsSuccess);
            Assert.IsTrue(receipt.IsReported);
            Assert.IsTrue(receipt.Result.IsSuccess);
            Assert.IsTrue(stack.CurrentPage.HasValue);
            Assert.IsTrue(stack.TryGetPooledCount(key, out _));
        }

        [Test]
        public async Task TrimPoolMessage_WithAnAssetKeyTrimsThatPoolAsync()
        {
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            var key = new AssetKey(ELEMENT_PAGE);

            var prepooled = await stack.PrepoolPageAsync(
                  pageAssetKey: key
                , amount: 2
                , token: CancellationToken.None
            );

            Assert.IsTrue(prepooled.IsSuccess);
            Assert.IsTrue(stack.TryGetPooledCount(key, out var before));
            Assert.AreEqual(2, before);

            var publisher = stack.Context.Publisher.Scope(stack.Context.FlowScope);

            TrimPoolMessage.Publish(in publisher, new TrimPoolMessage(AssetKey: key, AmountToKeep: 1));

            Assert.IsTrue(stack.TryGetPooledCount(key, out var after));
            Assert.AreEqual(1, after);
        }

        [Test]
        public async Task AddPageMessage_WithAnAssetKeyAddsThePageAsync()
        {
            var list = await CreateFlowAsync<UitkPageListMulti>(PageFlowKind.PageListMulti);
            var key = new AssetKey(ELEMENT_PAGE);
            var receipt = new PageCallReceipt();
            var publisher = list.Context.Publisher.Scope(list.Context.FlowScope);

            var publishingContext = PublishingContext.Default(
                  logger: list.Context.Logger
                , token: CancellationToken.None
            );

            var published = await AddPageMessage.Async.Publish(
                  in publisher
                , new AddPageMessage(key, ZeroDuration with { Receipt = receipt })
                , publishingContext
            );

            Assert.IsTrue(published.IsSuccess);
            Assert.IsTrue(receipt.IsReported);
            Assert.IsTrue(receipt.Result.IsSuccess);
            Assert.AreEqual(1, list.Pages.Count);
        }

        [Test]
        public async Task Return_DestroySucceedsAndASecondReturnReportsPageNotActiveAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            var rent = await stack.RentPageAsync(ELEMENT_PAGE, ZeroDuration, ZeroDuration, CancellationToken.None);
            Assert.IsTrue(rent.TryGetValue(out var slot));

            var first = stack.ReturnPage(
                  slot.Page
                , ZeroDuration with { ReturnOperation = PageReturnOperation.Destroy }
            );

            Assert.IsTrue(first.IsSuccess);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var count));
            Assert.AreEqual(0, count);

            var second = stack.ReturnPage(slot.Page, ZeroDuration);

            Assert.IsTrue(second.TryGetFailure(out var error));
            Assert.AreEqual(PageFlowError.PageNotActive(slot.Page).ToMessage(), error.ToMessage());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task ResourceLease_ReleaseKeepsTheAssetValidAndMissingResourceReportsFailureAsync()
        {
            var load = await PageAssetLoader.LoadAsync<VisualTreeAsset>(
                  PageLoaderStrategy.Resources
                , ELEMENT_PAGE
                , CancellationToken.None
            );

            Assert.IsTrue(load.TryGetValue(out var lease));
            var asset = lease.Asset;
            Assert.IsTrue(asset.IsValid());

            lease.Release();

            Assert.AreSame(asset, lease.Asset);
            Assert.IsTrue(lease.Asset.IsValid());

            var missing = await PageAssetLoader.LoadAsync<VisualTreeAsset>(
                  PageLoaderStrategy.Resources
                , MISSING_PAGE
                , CancellationToken.None
            );

            Assert.IsTrue(missing.TryGetError(out var error));
            StringAssert.StartsWith("Resources: ", error.ToMessage());
            LogAssert.NoUnexpectedReceived();
        }

        private async Task<T> CreateFlowAsync<T>(PageFlowKind kind)
            where T : UitkPageFlow
        {
            _harness = await CreateAsync(
                  ("Screen", kind, "")
                , ("Popup", PageFlowKind.PageStackSingle, "")
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
