using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

    public sealed class UitkPageErrorDeliveryTests
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
            TestElementPage.AbortOnAttach = null;
            TestElementPage.ThrowOnAttach = null;
            TestElementPage.CancelOnAttach = null;
            TestElementPage.CancelOnShow = null;
            TestElementPage.Events.Clear();
        }

        [Test]
        public async Task G1_AbortWithAHookFailure_PageAbortedMessageCarriesTheErrorAsync()
        {
            await CreateHarnessAsync(PageFlowKind.PageStackMulti);
            var stack = _harness.GetFlow<UitkPageStackMulti>("Popup");
            var log = Subscribe(stack);
            var exception = new InvalidOperationException("attach failed");
            var report = new PageCallReport();
            var context = ZeroDuration with { Report = report };
            TestElementPage.AbortOnAttach = PageAbortMode.Reverse;
            TestElementPage.ThrowOnAttach = exception;

            var result = await stack.ShowPageAsync(ELEMENT_PAGE, context, CancellationToken.None);

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsAborted);
            Assert.IsFalse(error.IsWithdrawn);
            Assert.IsFalse(error.IsCancelled);
            Assert.IsFalse(result.IsApplied());
            Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);
            Assert.IsTrue(error.TryGetAbort(out var origin, out var mode));
            Assert.IsTrue(origin.TryGetValue(out PageAbortOrigin.Page pageOrigin));
            Assert.IsInstanceOf<TestElementPage>(pageOrigin.Source);
            Assert.AreEqual(expected: PageAbortMode.Reverse, actual: mode);
            AssertSingleSuppressedException(error, exception);
            AssertReport(report, "Popup", error);

            Assert.AreEqual(expected: 1, actual: log.Aborted.Count);
            var message = log.Aborted[0];
            Assert.AreEqual(expected: origin, actual: message.Origin);
            Assert.AreEqual(expected: mode, actual: message.Mode);
            Assert.AreEqual(expected: error, actual: message.Error);
            AssertSingleSuppressedException(message.Error, exception);
            Assert.AreEqual(expected: context, actual: message.Context);
            Assert.AreSame(report, message.Context.Report);
            Assert.IsEmpty(log.Failed);
            Assert.IsFalse(stack.CurrentPage.TryGetValue(out _));
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(expected: 1, actual: pooled);
            Assert.IsFalse(stack.IsInTransition);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task G2_CancelledAddWithAFailedCleanup_PublishesTheFailureAsync()
        {
            await CreateHarnessAsync(PageFlowKind.PageListMulti);
            var list = _harness.GetFlow<UitkPageListMulti>("Popup");
            var log = Subscribe(list);
            var exception = new InvalidOperationException("detach subscriber failed");
            var report = new PageCallReport();
            var context = ZeroDuration with { Report = report };
            using var cancellation = new CancellationTokenSource();
            TestElementPage.CancelOnAttach = cancellation;

            var subscriber = list.Context.Subscriber.Scope(list.Context.FlowScope)
                .WithState(exception)
                .WithSubscriptions(_subscriptions);

            DetachPageMessage.Async.Subscribe(in subscriber, FailDetachAsync);

            var result = await list.AddPageAsync(ELEMENT_PAGE, context, cancellation.Token);

            Assert.IsTrue(cancellation.IsCancellationRequested);
            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsCancelled);
            Assert.IsFalse(result.IsApplied());
            AssertSingleSuppressedException(error, exception);
            AssertReport(report, "Popup", error);
            Assert.AreEqual(expected: 1, actual: log.Failed.Count);
            Assert.AreEqual(expected: error, actual: log.Failed[0].Error);
            AssertSingleSuppressedException(log.Failed[0].Error, exception);
            Assert.AreEqual(expected: context, actual: log.Failed[0].Context);
            Assert.AreSame(report, log.Failed[0].Context.Report);
            Assert.IsEmpty(log.Aborted);
            Assert.AreEqual(expected: 0, actual: list.Pages.Count);
            Assert.IsTrue(list.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(expected: 1, actual: pooled);
            Assert.IsFalse(list.IsInTransition);
            LogAssert.NoUnexpectedReceived();

            static UnityTask FailDetachAsync(
                  InvalidOperationException ex
                , DetachPageMessage.Async message
                , PublishingContext publishingContext
            )
                => throw ex;
        }

        [Test]
        public async Task G2_PlainCancel_PublishesNothingAsync()
        {
            await CreateHarnessAsync(PageFlowKind.PageListMulti);
            var list = _harness.GetFlow<UitkPageListMulti>("Popup");
            var log = Subscribe(list);
            var report = new PageCallReport();
            var context = ZeroDuration with { Report = report };
            using var cancellation = new CancellationTokenSource();
            TestElementPage.CancelOnAttach = cancellation;

            var result = await list.AddPageAsync(ELEMENT_PAGE, context, cancellation.Token);

            Assert.IsTrue(cancellation.IsCancellationRequested);
            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsCancelled);
            Assert.IsFalse(error.HasSuppressedErrors);
            Assert.AreEqual(expected: 0, actual: error.SuppressedErrors.Length);
            Assert.IsFalse(result.IsApplied());
            AssertReport(report, "Popup", error);
            Assert.IsEmpty(log.Failed);
            Assert.IsEmpty(log.Aborted);
            Assert.AreEqual(expected: 0, actual: list.Pages.Count);
            Assert.IsTrue(list.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(expected: 1, actual: pooled);
            Assert.IsFalse(list.IsInTransition);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task G3_ThrownCancel_ReportAndListenerGetTheFailureItWonOverAsync()
        {
            await AssertThrownShowCancellationAsync(hasReport: true, hasListener: true);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task G3_ThrownCancel_WithoutReportOrListener_LogsTheFailureOnceAsync()
        {
            await AssertThrownShowCancellationAsync(hasReport: false, hasListener: false);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task G3_ThrownCancel_WithoutReport_ListenerGetsItAndNothingIsLoggedAsync()
        {
            await AssertThrownShowCancellationAsync(hasReport: false, hasListener: true);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task G3_HideActiveThrownCancel_ReportCarriesTheFailureAsync()
        {
            await CreateHarnessAsync(PageFlowKind.PageStackMulti);
            var stack = _harness.GetFlow<UitkPageStackMulti>("Popup");
            Assert.IsTrue((await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var page));
            var element = (TestElementPage)page;
            element.HideDelayFrames = 3;

            var log = Subscribe(stack);
            var exception = new InvalidOperationException("began subscriber failed");
            SubscribeBeganFailure(stack, exception);
            var report = new PageCallReport();
            var context = ZeroDuration with { Report = report };
            using var cancellation = new CancellationTokenSource();
            OperationCanceledException thrown = null;

            var pending = stack.HideActivePageAsync(context, cancellation.Token);
            await WaitFramesAsync(1);
            cancellation.Cancel();

            try
            {
                await pending;
            }
            catch (OperationCanceledException ex)
            {
                thrown = ex;
            }

            Assert.IsNotNull(thrown);
            AssertCancelledReportAndMessage(report, context, log, exception);
            CollectionAssert.Contains(TestElementPage.Events, $"HideCancelled:{element.Counter}");
            Assert.IsFalse(stack.CurrentPage.TryGetValue(out _));
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(expected: 1, actual: pooled);
            Assert.IsFalse(stack.IsInTransition);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task G3_ListShowAtThrownCancel_ReportCarriesTheFailureAsync()
        {
            await CreateHarnessAsync(PageFlowKind.PageListMulti);
            var list = _harness.GetFlow<UitkPageListMulti>("Popup");
            Assert.IsTrue((await list.AddPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None)).IsSuccess);
            var page = list.Pages[0];
            var log = Subscribe(list);
            var exception = new InvalidOperationException("began subscriber failed");
            SubscribeBeganFailure(list, exception);
            var report = new PageCallReport();
            var context = ZeroDuration with { Report = report };
            using var cancellation = new CancellationTokenSource();
            TestElementPage.CancelOnShow = cancellation;
            OperationCanceledException thrown = null;

            try
            {
                await list.ShowPageAtIndexAsync(index: 0, context: context, token: cancellation.Token);
            }
            catch (OperationCanceledException ex)
            {
                thrown = ex;
            }

            Assert.IsNotNull(thrown);
            Assert.IsTrue(cancellation.IsCancellationRequested);
            AssertCancelledReportAndMessage(report, context, log, exception);
            Assert.AreEqual(expected: 1, actual: list.Pages.Count);
            Assert.AreSame(page, list.Pages[0]);
            Assert.IsFalse(list.IsInTransition);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task G4_NavigatedSubscriberFailure_ReachesTheReportAsync()
        {
            await CreateHarnessAsync(PageFlowKind.PageStackMulti);
            var stack = _harness.GetFlow<UitkPageStackSingle>("Screen");
            var log = Subscribe(stack);
            var exception = new InvalidOperationException("navigated subscriber failed");
            var report = new PageCallReport();
            var context = ZeroDuration with { Report = report };

            var subscriber = stack.Context.Subscriber.Scope(stack.Context.FlowScope)
                .WithState(exception)
                .WithSubscriptions(_subscriptions);

            PageNavigatedMessage.Async.Subscribe(in subscriber, FailNavigatedAsync);

            var result = await stack.ShowPageAsync(ELEMENT_PAGE, context, CancellationToken.None);

            Assert.IsTrue(result.TryGetFailure(out var error));
            AssertException(error, exception);
            Assert.IsFalse(error.HasSuppressedErrors);
            Assert.AreEqual(expected: PageTransitionState.Completed, actual: error.TransitionState);
            Assert.IsTrue(result.IsApplied());
            AssertReport(report, "Screen", error);
            Assert.IsTrue(report.Result.IsApplied());
            Assert.AreEqual(expected: 1, actual: log.Failed.Count);
            Assert.AreEqual(expected: error, actual: log.Failed[0].Error);
            Assert.AreEqual(expected: context, actual: log.Failed[0].Context);
            Assert.AreSame(report, log.Failed[0].Context.Report);
            Assert.IsEmpty(log.Aborted);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var page));
            Assert.IsInstanceOf<TestElementPage>(page);
            Assert.IsTrue(IsDisplayed(GetSlot(page)));
            Assert.IsFalse(stack.IsInTransition);
            LogAssert.NoUnexpectedReceived();

            static UnityTask FailNavigatedAsync(
                  InvalidOperationException ex
                , PageNavigatedMessage.Async message
                , PublishingContext publishingContext
            )
                => throw ex;
        }

        [Test]
        public async Task G5_SecondFlowFailure_ReachesListenersWhileTheReportKeepsTheFirstAsync()
        {
            await CreateHarnessAsync(PageFlowKind.PageStackMulti);
            var screen = _harness.GetFlow<UitkPageStackSingle>("Screen");
            var popup = _harness.GetFlow<UitkPageStackMulti>("Popup");
            var log = Subscribe(popup);
            var report = new PageCallReport();
            var context = ZeroDuration with { Report = report };

            var first = await screen.ShowPageAsync(ELEMENT_PAGE, context, CancellationToken.None);
            var second = await popup.ShowPageAsync(TWO_PAGES, context, CancellationToken.None);

            Assert.IsTrue(first.IsSuccess);
            Assert.IsTrue(report.IsReported);
            Assert.AreEqual(expected: "Screen", actual: report.FlowId);
            Assert.IsTrue(report.Result.IsSuccess);
            Assert.AreEqual(expected: first, actual: report.Result);
            Assert.IsTrue(second.TryGetFailure(out var error));

            Assert.AreEqual(
                  expected: PageFlowError.AssetNotSinglePage(assetKey: TWO_PAGES, pageCount: 2).ToMessage()
                , actual: error.ToMessage()
            );

            Assert.IsFalse(error.HasSuppressedErrors);
            Assert.IsFalse(second.IsApplied());
            Assert.AreEqual(expected: 1, actual: log.Failed.Count);
            Assert.AreEqual(expected: error, actual: log.Failed[0].Error);
            Assert.AreEqual(expected: context, actual: log.Failed[0].Context);
            Assert.AreSame(report, log.Failed[0].Context.Report);
            Assert.IsEmpty(log.Aborted);
            Assert.IsTrue(screen.CurrentPage.TryGetValue(out _));
            Assert.IsFalse(popup.CurrentPage.TryGetValue(out _));
            Assert.IsFalse(screen.IsInTransition);
            Assert.IsFalse(popup.IsInTransition);
            LogAssert.NoUnexpectedReceived();
        }

        private static void AssertException(PageFlowError error, Exception exception)
        {
            Assert.IsTrue(error._error.TryGetValue(out PageFlowError.Error.Exception wrapped));
            Assert.AreSame(exception, wrapped.Ex);
            Assert.AreEqual(expected: PageFlowError.Exception(exception).ToMessage(), actual: error.ToMessage());
        }

        private static void AssertSingleSuppressedException(PageFlowError error, Exception exception)
        {
            Assert.IsTrue(error.HasSuppressedErrors);
            Assert.AreEqual(expected: 1, actual: error.SuppressedErrors.Length);
            var suppressed = error.SuppressedErrors[0];
            AssertException(suppressed, exception);
            Assert.IsFalse(suppressed.HasSuppressedErrors);
        }

        private static void AssertReport(PageCallReport report, string flowId, PageFlowError error)
        {
            Assert.IsTrue(report.IsReported);
            Assert.AreEqual(expected: flowId, actual: report.FlowId);
            Assert.IsTrue(report.Result.TryGetFailure(out var reported));
            Assert.AreEqual(expected: error, actual: reported);
        }

        private static void AssertCancelledReportAndMessage(
              PageCallReport report
            , PageContext context
            , MessageLog log
            , Exception exception
        )
        {
            Assert.IsTrue(report.Result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsCancelled);
            AssertSingleSuppressedException(error, exception);
            AssertReport(report, "Popup", error);
            Assert.AreEqual(expected: 1, actual: log.Failed.Count);
            Assert.AreEqual(expected: error, actual: log.Failed[0].Error);
            AssertSingleSuppressedException(log.Failed[0].Error, exception);
            Assert.AreEqual(expected: context, actual: log.Failed[0].Context);
            Assert.AreSame(report, log.Failed[0].Context.Report);
            Assert.IsEmpty(log.Aborted);
        }

        private async Task CreateHarnessAsync(PageFlowKind popupKind)
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);

            _harness = await CreateAsync(("Screen", PageFlowKind.PageStackSingle, ""), ("Popup", popupKind, ""));
        }

        private async Task AssertThrownShowCancellationAsync(bool hasReport, bool hasListener)
        {
            await CreateHarnessAsync(PageFlowKind.PageStackMulti);
            var stack = _harness.GetFlow<UitkPageStackMulti>("Popup");
            var log = Subscribe(stack, hasListener);
            var exception = new InvalidOperationException("began subscriber failed");
            SubscribeBeganFailure(stack, exception);
            var report = hasReport ? new PageCallReport() : null;
            var context = ZeroDuration with { Report = report };
            using var cancellation = new CancellationTokenSource();
            TestElementPage.CancelOnShow = cancellation;
            OperationCanceledException thrown = null;

            if (hasReport == false && hasListener == false)
            {
                LogAssert.Expect(
                      LogType.Exception
                    , new Regex(@"^InvalidOperationException: began subscriber failed(?:\r?\n|$)")
                );
            }

            try
            {
                await stack.ShowPageAsync(ELEMENT_PAGE, context, cancellation.Token);
            }
            catch (OperationCanceledException ex)
            {
                thrown = ex;
            }

            Assert.IsNotNull(thrown);
            Assert.IsTrue(cancellation.IsCancellationRequested);

            if (hasReport)
            {
                AssertCancelledReportAndMessage(report, context, log, exception);
            }
            else if (hasListener)
            {
                Assert.AreEqual(expected: 1, actual: log.Failed.Count);
                var message = log.Failed[0];
                Assert.IsTrue(message.Error.IsCancelled);
                AssertSingleSuppressedException(message.Error, exception);
                Assert.AreEqual(expected: context, actual: message.Context);
                Assert.IsNull(message.Context.Report);
            }
            else
            {
                Assert.IsEmpty(log.Failed);
            }

            Assert.IsEmpty(log.Aborted);
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var page));
            Assert.IsInstanceOf<TestElementPage>(page);
            Assert.AreEqual(expected: 1, actual: stack.PageCollection.Count);
            Assert.IsFalse(stack.IsInTransition);
        }

        private MessageLog Subscribe(UitkPageFlow flow, bool hasFailureListener = true)
        {
            var log = new MessageLog();

            var subscriber = flow.Context.Subscriber.Scope(flow.Context.FlowScope)
                .WithState(log)
                .WithSubscriptions(_subscriptions);

            PageAbortedMessage.Subscribe(in subscriber, HandleAborted);

            if (hasFailureListener)
            {
                PageFlowFailedMessage.Subscribe(in subscriber, HandleFailed);
            }

            return log;

            static void HandleAborted(MessageLog messages, PageAbortedMessage message)
                => messages.Aborted.Add(message);

            static void HandleFailed(MessageLog messages, PageFlowFailedMessage message)
                => messages.Failed.Add(message);
        }

        private void SubscribeBeganFailure(UitkPageFlow flow, InvalidOperationException exception)
        {
            var subscriber = flow.Context.Subscriber.Scope(flow.Context.FlowScope)
                .WithState(exception)
                .WithSubscriptions(_subscriptions);

            PageTransitionBeganMessage.Async.Subscribe(in subscriber, FailBeganAsync);

            static UnityTask FailBeganAsync(
                  InvalidOperationException ex
                , PageTransitionBeganMessage.Async message
                , PublishingContext publishingContext
            )
                => throw ex;
        }

        private sealed class MessageLog
        {
            public readonly List<PageAbortedMessage> Aborted = new();
            public readonly List<PageFlowFailedMessage> Failed = new();
        }
    }
}
