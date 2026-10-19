using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    public sealed class UitkPageReviewFixTests
    {
        private const string NO_THEME_WARNING
            = "No Theme Style Sheet set to PanelSettings , UI will not render properly";

        private readonly List<ISubscription> _subscriptions = new();
        private UitkPagesHarness _harness;

        [TearDown]
        public void TearDown()
        {
            _subscriptions.Unsubscribe();
            _harness?.Dispose();
            _harness = null;
            TestElementPage.AttachDelayFrames = 0;
            TestElementPage.CancelOnShow = null;
        }

        [Test]
        public async Task F1_PageListSingleShowingTheShownIndexKeepsThePageShownAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var list = await CreateFlowAsync<UitkPageListSingle>(PageFlowKind.PageListSingle);
            var log = Subscribe(list);

            Assert.IsTrue((await list.AddPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);

            var page = list.Pages[0];
            var first = await list.ShowPageAtIndexAsync(index: 0, context: ZeroDuration, token: CancellationToken.None);

            Assert.IsTrue(first.IsSuccess);
            Assert.AreEqual(expected: 1, actual: log.Navigated.Count);

            var second = await list.ShowPageAtIndexAsync(
                  index: 0
                , context: ZeroDuration
                , token: CancellationToken.None
            );

            Assert.IsTrue(second.TryGetFailure(out var error));
            Assert.AreEqual(expected: PageFlowError.AlreadyShown(index: 0).ToMessage(), actual: error.ToMessage());
            Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);
            Assert.IsTrue(list.CurrentPage.TryGetValue(out var current));
            Assert.AreSame(page, current);
            Assert.IsTrue(IsDisplayed(GetSlot(page)));
            Assert.IsFalse(list.IsInTransition);
            Assert.AreEqual(expected: 1, actual: log.Navigated.Count);
            Assert.AreEqual(expected: 1, actual: log.Failed.Count);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task F2_PageStackSinglePushCancelledFromShowPoolsTheDisplacedPageAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            var log = Subscribe(stack);

            Assert.IsTrue((await stack.ShowPageAsync(BEHAVIOUR_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var displaced));

            using var cancellation = new CancellationTokenSource();
            TestElementPage.CancelOnShow = cancellation;
            OperationCanceledException thrown = null;

            try
            {
                await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, cancellation.Token);
            }
            catch (OperationCanceledException ex)
            {
                thrown = ex;
            }

            Assert.IsNotNull(thrown);
            Assert.IsTrue(cancellation.IsCancellationRequested);
            Assert.IsFalse(stack.IsInTransition);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var current));
            Assert.IsInstanceOf<TestElementPage>(current);
            Assert.AreNotSame(displaced, current);
            Assert.IsTrue(IsDisplayed(GetSlot(current)));
            Assert.IsTrue(stack.TryGetPooledCount(BEHAVIOUR_PAGE, out var pooledDisplaced));
            Assert.AreEqual(expected: 1, actual: pooledDisplaced);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var pooledCurrent));
            Assert.AreEqual(expected: 0, actual: pooledCurrent);
            Assert.AreEqual(expected: 1, actual: log.Navigated.Count);
            Assert.IsEmpty(log.Failed);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task F6_PageStackMultiQueuedHidesPoolAndReportTheirOwnPagesAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateFlowAsync<UitkPageStackMulti>(PageFlowKind.PageStackMulti);
            var log = Subscribe(stack);

            Assert.IsTrue((await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var a));
            Assert.IsTrue((await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var b));

            TestElementPage.AttachDelayFrames = 3;
            var push = stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            await WaitUntilAsync(() => stack.IsInTransition);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var c));

            var sequential = ZeroDuration with { AsyncOperation = PageAsyncOperation.Sequential };
            var firstHide = stack.HideActivePageAsync(sequential, CancellationToken.None);
            var secondHide = stack.HideActivePageAsync(sequential, CancellationToken.None);

            Assert.IsTrue((await push).IsSuccess);
            Assert.IsTrue((await firstHide).IsSuccess);
            Assert.IsTrue((await secondHide).IsSuccess);

            Assert.IsFalse(stack.IsInTransition);
            Assert.AreEqual(expected: 1, actual: stack.PageCollection.Count);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var current));
            Assert.AreSame(a, current);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(expected: 2, actual: pooled);
            Assert.AreEqual(expected: 5, actual: log.Navigated.Count);

            var firstPop = log.Navigated[3];
            var secondPop = log.Navigated[4];

            Assert.AreEqual(expected: PageNavigationKind.Pop, actual: firstPop.Kind);
            Assert.AreSame(c, firstPop.From.Page);
            Assert.AreSame(b, firstPop.To.Page);
            Assert.AreEqual(expected: PageNavigationKind.Pop, actual: secondPop.Kind);
            Assert.AreSame(b, secondPop.From.Page);
            Assert.AreSame(a, secondPop.To.Page);
            Assert.IsEmpty(log.Failed);
            LogAssert.NoUnexpectedReceived();
        }

        private static void Handle(MessageLog log, PageFlowFailedMessage message)
            => log.Failed.Add(message);

        private static UnityTask HandleAsync(MessageLog log, PageNavigatedMessage.Async message, PublishingContext _)
        {
            log.Navigated.Add(((PageNavigatedMessage)message).Navigation);
            return UnityTask.CompletedTask;
        }

        private async Task<T> CreateFlowAsync<T>(PageFlowKind kind)
            where T : UitkPageFlow
        {
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
