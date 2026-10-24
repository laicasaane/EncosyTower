using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    using static UitkPagesHarness;

    public sealed class UitkPageManyTests
    {
        private const string NO_THEME_WARNING
            = "No Theme Style Sheet set to PanelSettings , UI will not render properly";

        private readonly List<ISubscription> _subscriptions = new();
        private UitkPagesHarness _harness;

        private static void Handle(MessageLog log, PageFlowFailedMessage message)
            => log.Failed.Add(message);

        private static UnityTask HandleAsync(MessageLog log, PageNavigatedMessage.Async message, PublishingContext _)
        {
            log.Navigated.Add(((PageNavigatedMessage)message).Navigation);
            return UnityTask.CompletedTask;
        }

        [SetUp]
        public void SetUp()
        {
            TestElementPage.Events.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _subscriptions.Unsubscribe();
            _harness?.Dispose();
            _harness = null;
            TestElementPage.Events.Clear();
        }

        [Test]
        public async Task M1_StackHidesEveryPageTogetherThenPoolsThemAsync()
        {
            var stack = await CreateFlowAsync<UitkPageStackMulti>(PageFlowKind.PageStackMulti);
            var log = Subscribe(stack);
            var (bottom, top) = await ShowTwoPagesAsync(stack);

            bottom.HideDelayFrames = 6;
            top.HideDelayFrames = 1;
            TestElementPage.Events.Clear();

            var result = await stack.HideAllPagesAsync(ZeroDuration, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);

            CollectionAssert.AreEqual(
                  new[] { "Hide:2", "Hide:1", "AfterHide:2", "AfterHide:1", "Return:2", "Return:1" }
                , TestElementPage.Events
            );

            Assert.IsFalse(stack.IsInTransition);
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(expected: 2, actual: pooled);

            Assert.AreEqual(expected: 4, actual: log.Navigated.Count);
            AssertPop(log.Navigated[2], from: top, to: bottom);
            AssertPop(log.Navigated[3], from: bottom, to: null);
            Assert.IsEmpty(log.Failed);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task M2_StackWithoutPageReturnsNoPageAsync()
        {
            var stack = await CreateFlowAsync<UitkPageStackMulti>(PageFlowKind.PageStackMulti);
            var log = Subscribe(stack);

            var result = await stack.HideAllPagesAsync(ZeroDuration, CancellationToken.None);

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.AreEqual(expected: PageFlowError.NoPage().ToMessage(), actual: error.ToMessage());
            Assert.IsEmpty(log.Navigated);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task M3_AbortCurrentAbortsEveryPageTransitionAsync()
        {
            var stack = await CreateFlowAsync<UitkPageStackMulti>(PageFlowKind.PageStackMulti);
            var log = Subscribe(stack);
            var (bottom, top) = await ShowTwoPagesAsync(stack);

            bottom.HideDelayFrames = 6;
            top.HideDelayFrames = 6;
            TestElementPage.Events.Clear();

            var pending = stack.HideAllPagesAsync(ZeroDuration, CancellationToken.None);

            await WaitUntilAsync(() => TestElementPage.Events.Count >= 2);
            Assert.IsTrue(stack.IsInTransition);
            CollectionAssert.AreEqual(new[] { "Hide:2", "Hide:1" }, TestElementPage.Events);
            Assert.IsTrue(stack.AbortCurrent(PageAbortMode.Reverse).IsSuccess);

            var result = await pending;

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsAborted);
            Assert.AreEqual(expected: PageTransitionState.Undone, actual: error.TransitionState);
            CollectionAssert.Contains(TestElementPage.Events, "HideCancelled:1");
            CollectionAssert.Contains(TestElementPage.Events, "HideCancelled:2");
            CollectionAssert.DoesNotContain(TestElementPage.Events, "AfterHide:1");
            CollectionAssert.DoesNotContain(TestElementPage.Events, "AfterHide:2");
            Assert.IsFalse(stack.IsInTransition);
            Assert.AreEqual(expected: 2, actual: stack.PageCollection.Count);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var current));
            Assert.AreSame(top, current);
            Assert.IsTrue(IsDisplayed(GetSlot(top)));
            Assert.IsTrue(IsDisplayed(GetSlot(bottom)));
            Assert.AreEqual(expected: 2, actual: log.Navigated.Count);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task M4_MessageHidesEveryPageAndReportsOnceAsync()
        {
            var stack = await CreateFlowAsync<UitkPageStackMulti>(PageFlowKind.PageStackMulti);
            await ShowTwoPagesAsync(stack);

            var report = new PageCallReport();
            var publisher = stack.Context.Publisher.Scope(stack.Context.FlowScope);
            var context = ZeroDuration with { Report = report };

            var published = await HideAllPagesMessage.Async.Publish(
                  in publisher
                , new HideAllPagesMessage(context)
                , PublishingContext.Default()
            );

            Assert.IsTrue(published.IsSuccess);
            Assert.IsTrue(report.IsReported);
            Assert.IsTrue(report.Result.IsSuccess);
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task M5_ListHidesShownPagesThenShowsAllPagesAsync()
        {
            var list = await CreateFlowAsync<UitkPageListMulti>(PageFlowKind.PageListMulti);
            var log = Subscribe(list);

            for (var i = 0; i < 3; i++)
            {
                Assert.IsTrue((await list.AddPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);
                ((TestElementPage)list.Pages[i]).Counter = i;
            }

            Assert.IsTrue((await list.ShowPageAtIndexAsync(0, ZeroDuration, CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await list.ShowPageAtIndexAsync(1, ZeroDuration, CancellationToken.None)).IsSuccess);

            ((TestElementPage)list.Pages[0]).HideDelayFrames = 5;
            ((TestElementPage)list.Pages[1]).HideDelayFrames = 1;
            TestElementPage.Events.Clear();
            log.Navigated.Clear();

            var hidden = await list.HideAllPagesAsync(ZeroDuration, CancellationToken.None);

            Assert.IsTrue(hidden.IsSuccess);
            CollectionAssert.AreEqual(
                  new[] { "Hide:0", "Hide:1", "AfterHide:0", "AfterHide:1" }
                , TestElementPage.Events
            );
            Assert.AreEqual(expected: 3, actual: list.Pages.Count);
            Assert.IsFalse(IsDisplayed(GetSlot(list.Pages[0])));
            Assert.IsFalse(IsDisplayed(GetSlot(list.Pages[1])));
            Assert.IsFalse(IsDisplayed(GetSlot(list.Pages[2])));
            Assert.AreEqual(expected: 2, actual: log.Navigated.Count);
            AssertNavigation(log.Navigated[0], PageNavigationKind.HideAt, index: 0);
            AssertNavigation(log.Navigated[1], PageNavigationKind.HideAt, index: 1);

            var shownNone = await list.HideAllPagesAsync(ZeroDuration, CancellationToken.None);

            Assert.IsTrue(shownNone.TryGetFailure(out var noPage));
            Assert.AreEqual(expected: PageFlowError.NoPage().ToMessage(), actual: noPage.ToMessage());

            log.Navigated.Clear();

            var shown = await list.ShowAllPagesAsync(ZeroDuration, CancellationToken.None);

            Assert.IsTrue(shown.IsSuccess);
            Assert.AreEqual(expected: 3, actual: list.Pages.Count);
            Assert.IsTrue(IsDisplayed(GetSlot(list.Pages[0])));
            Assert.IsTrue(IsDisplayed(GetSlot(list.Pages[1])));
            Assert.IsTrue(IsDisplayed(GetSlot(list.Pages[2])));
            Assert.AreEqual(expected: 3, actual: log.Navigated.Count);
            AssertNavigation(log.Navigated[0], PageNavigationKind.ShowAt, index: 0);
            AssertNavigation(log.Navigated[1], PageNavigationKind.ShowAt, index: 1);
            AssertNavigation(log.Navigated[2], PageNavigationKind.ShowAt, index: 2);

            var shownAll = await list.ShowAllPagesAsync(ZeroDuration, CancellationToken.None);

            Assert.IsTrue(shownAll.TryGetFailure(out var allShown));
            Assert.AreEqual(expected: PageFlowError.NoPage().ToMessage(), actual: allShown.ToMessage());
            Assert.IsFalse(list.IsInTransition);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task M6_ListMessagesHideAndShowEveryPageAsync()
        {
            var list = await CreateFlowAsync<UitkPageListMulti>(PageFlowKind.PageListMulti);

            Assert.IsTrue((await list.AddPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await list.AddPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);

            var publisher = list.Context.Publisher.Scope(list.Context.FlowScope);
            var showReport = new PageCallReport();
            var hideReport = new PageCallReport();

            await ShowAllPagesMessage.Async.Publish(
                  in publisher
                , new ShowAllPagesMessage(ZeroDuration with { Report = showReport })
                , PublishingContext.Default()
            );

            Assert.IsTrue(showReport.Result.IsSuccess);
            Assert.IsTrue(IsDisplayed(GetSlot(list.Pages[0])));
            Assert.IsTrue(IsDisplayed(GetSlot(list.Pages[1])));

            await HideAllPagesMessage.Async.Publish(
                  in publisher
                , new HideAllPagesMessage(ZeroDuration with { Report = hideReport })
                , PublishingContext.Default()
            );

            Assert.IsTrue(hideReport.Result.IsSuccess);
            Assert.IsFalse(IsDisplayed(GetSlot(list.Pages[0])));
            Assert.IsFalse(IsDisplayed(GetSlot(list.Pages[1])));
            Assert.AreEqual(expected: 2, actual: list.Pages.Count);
            LogAssert.NoUnexpectedReceived();
        }

        private static async Task<(TestElementPage Bottom, TestElementPage Top)> ShowTwoPagesAsync(
            UitkPageStackMulti stack
        )
        {
            Assert.IsTrue((await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var bottom));
            Assert.IsTrue((await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var top));

            var bottomPage = (TestElementPage)bottom;
            var topPage = (TestElementPage)top;

            bottomPage.Counter = 1;
            topPage.Counter = 2;

            return (bottomPage, topPage);
        }

        private static void AssertPop(PageNavigation navigation, IUitkPage from, IUitkPage to)
        {
            Assert.AreEqual(expected: PageNavigationKind.Pop, actual: navigation.Kind);
            Assert.AreSame(from, navigation.From.Page);
            Assert.AreSame(to, navigation.To.Page);
        }

        private static void AssertNavigation(PageNavigation navigation, PageNavigationKind kind, int index)
        {
            Assert.AreEqual(expected: kind, actual: navigation.Kind);

            var page = kind == PageNavigationKind.ShowAt ? navigation.To : navigation.From;

            Assert.IsTrue(page.Index.TryGetValue(out var actual));
            Assert.AreEqual(expected: index, actual: actual);
        }

        private async Task<T> CreateFlowAsync<T>(PageFlowKind kind)
            where T : UitkPageFlow
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            _harness = await CreateAsync(("Screen", kind, ""), ("Popup", PageFlowKind.PageStackSingle, ""));
            return _harness.GetFlow<T>("Screen");
        }

        private MessageLog Subscribe(UitkPageFlow flow)
        {
            var log = new MessageLog();

            var subscriber = flow.Context.Subscriber.Scope(flow.Context.FlowScope)
                .WithState(log)
                .WithSubscriptions(_subscriptions);

            PageFlowFailedMessage.Subscribe(in subscriber, Handle);
            PageNavigatedMessage.Async.Subscribe(in subscriber, HandleAsync);
            return log;
        }

        private sealed class MessageLog
        {
            public readonly List<PageFlowFailedMessage> Failed = new();
            public readonly List<PageNavigation> Navigated = new();
        }
    }
}
