using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PubSub;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    using static UitkPagesHarness;

    public sealed class UitkPageAbortTests
    {
        private const string NO_THEME_WARNING
            = "No Theme Style Sheet set to PanelSettings , UI will not render properly";

        private readonly List<ISubscription> _subscriptions = new();
        private UitkPagesHarness _harness;

        private static void Handle(MessageLog log, PageAbortedMessage message)
            => log.Aborted.Add(message);

        private static void Handle(MessageLog log, PageFlowFailedMessage message)
            => log.Failed.Add(message);

        private static void AssertAbortedShow(
              Success<PageFlowError> result
            , UitkPageStackMulti stack
            , MessageLog log
            , PageAbortOrigin.EnumCase expectedOrigin
            , PageAbortMode expectedMode
        )
        {
            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsAborted);
            Assert.IsFalse(error.IsWithdrawn);
            Assert.IsFalse(error.IsCancelled);
            Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);
            Assert.IsTrue(error.TryGetAbort(out var origin, out var mode));
            Assert.AreEqual(expected: expectedOrigin, actual: origin.GetEnumCase());
            Assert.AreEqual(expected: expectedMode, actual: mode);

            Assert.AreEqual(expected: 1, actual: log.Aborted.Count);
            Assert.AreEqual(expected: expectedOrigin, actual: log.Aborted[0].Origin.GetEnumCase());
            Assert.AreEqual(expected: expectedMode, actual: log.Aborted[0].Mode);
            Assert.AreEqual(expected: error.ToMessage(), actual: log.Aborted[0].Error.ToMessage());
            Assert.IsFalse(log.Aborted[0].Error.HasSuppressedErrors);
            Assert.IsEmpty(log.Failed);

            Assert.IsFalse(stack.CurrentPage.TryGetValue(out _));
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(expected: 1, actual: pooled);
            Assert.IsFalse(stack.IsInTransition);
        }

        [TearDown]
        public void TearDown()
        {
            TestElementPage.AbortOnCreate = null;

            var count = _subscriptions.Count;

            for (var i = 0; i < count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
            _harness?.Dispose();
            _harness = null;
        }

        [Test]
        public async Task CallerAbortWhileRenting_ReturnsThePageAndPublishesCallerAbortAsync()
        {
            var stack = await CreateStackAsync();
            var log = Subscribe(stack);
            var source = new PageAbortSource();

            var show = stack.ShowPageAsync(
                  ELEMENT_PAGE
                , ZeroDuration with { Abort = source.Handle }
                , CancellationToken.None
            );

            var request = source.Abort(PageAbortMode.Reverse);
            Assert.IsTrue(request.IsSuccess);
            var result = await show;

            AssertAbortedShow(result, stack, log, PageAbortOrigin.EnumCase.Caller, PageAbortMode.Reverse);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageAbortInOnCreate_ReturnsThePageAndPublishesPageAbortAsync()
        {
            var stack = await CreateStackAsync();
            var log = Subscribe(stack);
            TestElementPage.AbortOnCreate = PageAbortMode.Reverse;

            var result = await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            AssertAbortedShow(result, stack, log, PageAbortOrigin.EnumCase.Page, PageAbortMode.Reverse);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task AbortMessageWhileRenting_ReturnsThePageAndPublishesFlowAbortAsync()
        {
            var stack = await CreateStackAsync();
            var log = Subscribe(stack);
            var show = stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            var publisher = stack.Context.Publisher.Scope(stack.Context.FlowScope);
            AbortPageMessage.Publish(in publisher, new AbortPageMessage(PageAbortMode.Immediate));
            var result = await show;

            AssertAbortedShow(result, stack, log, PageAbortOrigin.EnumCase.Flow, PageAbortMode.Immediate);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task AbortMessageWhileIdle_KeepsThePageAndPublishesNoFailureAsync()
        {
            var stack = await CreateStackAsync();
            var log = Subscribe(stack);
            var show = await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.IsTrue(show.IsSuccess);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var page));

            var publisher = stack.Context.Publisher.Scope(stack.Context.FlowScope);
            AbortPageMessage.Publish(in publisher, new AbortPageMessage(PageAbortMode.Reverse));
            var request = stack.AbortCurrent(PageAbortMode.Reverse);

            Assert.IsTrue(request.TryGetFailure(out var error));
            Assert.AreEqual(expected: PageAbortError.EnumCase.FlowIsIdle, actual: error.GetEnumCase());
            Assert.IsEmpty(log.Aborted);
            Assert.IsEmpty(log.Failed);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var current));
            Assert.AreSame(page, current);
            Assert.IsFalse(stack.IsInTransition);
            LogAssert.NoUnexpectedReceived();
        }

        private async Task<UitkPageStackMulti> CreateStackAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);

            _harness = await CreateAsync(
                  ("Screen", PageFlowKind.PageStackSingle, "")
                , ("Popup", PageFlowKind.PageStackMulti, "")
            );

            return _harness.GetFlow<UitkPageStackMulti>("Popup");
        }

        private MessageLog Subscribe(UitkPageStackMulti stack)
        {
            var log = new MessageLog();

            var subscriber = stack.Context.Subscriber.Scope(stack.Context.FlowScope)
                .WithState(log)
                .WithSubscriptions(_subscriptions);

            PageAbortedMessage.Subscribe(in subscriber, Handle);
            PageFlowFailedMessage.Subscribe(in subscriber, Handle);
            return log;
        }

        private sealed class MessageLog
        {
            public readonly List<PageAbortedMessage> Aborted = new();
            public readonly List<PageFlowFailedMessage> Failed = new();
        }
    }
}
