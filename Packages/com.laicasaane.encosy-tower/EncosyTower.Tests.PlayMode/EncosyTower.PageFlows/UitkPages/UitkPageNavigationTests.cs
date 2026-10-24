using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.History;
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

    public sealed class UitkPageNavigationTests
    {
        private const string NO_THEME_WARNING
            = "No Theme Style Sheet set to PanelSettings , UI will not render properly";

        private UitkPagesHarness _harness;
        private NavigationRecorder _recorder;
        private PageHistory _history;

        [TearDown]
        public void TearDown()
        {
            TestElementPage.ThrowOnReturnToPool = null;
            _history?.Dispose();
            _history = null;
            _recorder?.Dispose();
            _recorder = null;
            _harness?.Dispose();
            _harness = null;
            TestElementPage.AbortOnAttach = null;
            TestElementPage.AttachDelayFrames = 0;
        }

        [Test]
        public async Task PageStackSingle_PublishesShowAndHideAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var screen = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            var firstContext = CreateContextWithReport(out var r1);
            var firstResult = await screen.ShowPageAsync(ELEMENT_PAGE, firstContext, CancellationToken.None);

            Assert.IsTrue(firstResult.IsSuccess);
            AssertReported(r1, firstResult);
            Assert.IsTrue(screen.CurrentPage.TryGetValue(out var firstPage));
            Assert.AreEqual(expected: 1, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[0]
                , kind: PageNavigationKind.Show
                , fromKey: null
                , fromIndex: Option.None
                , toKey: ELEMENT_PAGE
                , toIndex: Option.None
            );

            Assert.AreSame(firstPage, _recorder.Items[0].To.Page);
            var secondContext = CreateContextWithReport(out var r2);
            var secondResult = await screen.ShowPageAsync(BEHAVIOUR_PAGE, secondContext, CancellationToken.None);

            Assert.IsTrue(secondResult.IsSuccess);
            AssertReported(r2, secondResult);
            Assert.IsTrue(screen.CurrentPage.TryGetValue(out var secondPage));
            Assert.AreEqual(expected: 2, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[1]
                , kind: PageNavigationKind.Show
                , fromKey: ELEMENT_PAGE
                , fromIndex: Option.None
                , toKey: BEHAVIOUR_PAGE
                , toIndex: Option.None
            );

            Assert.AreSame(firstPage, _recorder.Items[1].From.Page);
            Assert.AreSame(secondPage, _recorder.Items[1].To.Page);
            var hideContext = CreateContextWithReport(out var r3);
            var hideResult = await screen.HideActivePageAsync(hideContext, CancellationToken.None);

            Assert.IsTrue(hideResult.IsSuccess);
            AssertReported(r3, hideResult);
            Assert.AreEqual(expected: 3, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[2]
                , kind: PageNavigationKind.Hide
                , fromKey: BEHAVIOUR_PAGE
                , fromIndex: Option.None
                , toKey: null
                , toIndex: Option.None
            );

            Assert.AreSame(secondPage, _recorder.Items[2].From.Page);
            Assert.IsNull(_recorder.Items[2].To.Page);
            Assert.IsFalse(screen.CurrentPage.HasValue);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackMulti_PublishesPushAndPopAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var screen = await CreateFlowAsync<UitkPageStackMulti>(PageFlowKind.PageStackMulti);
            var firstContext = CreateContextWithReport(out var r1);
            var firstResult = await screen.ShowPageAsync(ELEMENT_PAGE, firstContext, CancellationToken.None);

            Assert.IsTrue(firstResult.IsSuccess);
            AssertReported(r1, firstResult);
            Assert.IsTrue(screen.CurrentPage.TryGetValue(out var firstPage));
            Assert.AreEqual(expected: 1, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[0]
                , kind: PageNavigationKind.Push
                , fromKey: null
                , fromIndex: Option.None
                , toKey: ELEMENT_PAGE
                , toIndex: Option.None
            );

            Assert.AreSame(firstPage, _recorder.Items[0].To.Page);
            var secondContext = CreateContextWithReport(out var r2);
            var secondResult = await screen.ShowPageAsync(BEHAVIOUR_PAGE, secondContext, CancellationToken.None);

            Assert.IsTrue(secondResult.IsSuccess);
            AssertReported(r2, secondResult);
            Assert.IsTrue(screen.CurrentPage.TryGetValue(out var secondPage));
            Assert.AreEqual(expected: 2, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[1]
                , kind: PageNavigationKind.Push
                , fromKey: ELEMENT_PAGE
                , fromIndex: Option.None
                , toKey: BEHAVIOUR_PAGE
                , toIndex: Option.None
            );

            Assert.AreSame(firstPage, _recorder.Items[1].From.Page);
            Assert.AreSame(secondPage, _recorder.Items[1].To.Page);
            var hideContext = CreateContextWithReport(out var r3);
            var hideResult = await screen.HideActivePageAsync(hideContext, CancellationToken.None);

            Assert.IsTrue(hideResult.IsSuccess);
            AssertReported(r3, hideResult);
            Assert.AreEqual(expected: 3, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[2]
                , kind: PageNavigationKind.Pop
                , fromKey: BEHAVIOUR_PAGE
                , fromIndex: Option.None
                , toKey: ELEMENT_PAGE
                , toIndex: Option.None
            );

            Assert.AreSame(secondPage, _recorder.Items[2].From.Page);
            Assert.AreSame(_recorder.Items[0].To.Page, _recorder.Items[2].To.Page);
            Assert.IsTrue(screen.CurrentPage.TryGetValue(out var currentPage));
            Assert.AreSame(firstPage, currentPage);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageListSingle_PublishesShowAtAndHideAtAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var screen = await CreateFlowAsync<UitkPageListSingle>(PageFlowKind.PageListSingle);
            var addFirstContext = CreateContextWithReport(out var r1);
            var addFirstResult = await screen.AddPageAsync(ELEMENT_PAGE, addFirstContext, CancellationToken.None);

            Assert.IsTrue(addFirstResult.IsSuccess);
            AssertReported(r1, addFirstResult);
            Assert.IsEmpty(_recorder.Items);
            var addSecondContext = CreateContextWithReport(out var r2);
            var addSecondResult = await screen.AddPageAsync(BEHAVIOUR_PAGE, addSecondContext, CancellationToken.None);

            Assert.IsTrue(addSecondResult.IsSuccess);
            AssertReported(r2, addSecondResult);
            Assert.IsEmpty(_recorder.Items);
            var showFirstContext = CreateContextWithReport(out var r3);

            var showFirstResult = await screen.ShowPageAtIndexAsync(
                  index: 0
                , context: showFirstContext
                , token: CancellationToken.None
            );

            Assert.IsTrue(showFirstResult.IsSuccess);
            AssertReported(r3, showFirstResult);
            Assert.AreEqual(expected: 1, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[0]
                , kind: PageNavigationKind.ShowAt
                , fromKey: null
                , fromIndex: Option.None
                , toKey: ELEMENT_PAGE
                , toIndex: 0
            );

            Assert.AreSame(screen.Pages[0], _recorder.Items[0].To.Page);
            var showSecondContext = CreateContextWithReport(out var r4);

            var showSecondResult = await screen.ShowPageAtIndexAsync(
                  index: 1
                , context: showSecondContext
                , token: CancellationToken.None
            );

            Assert.IsTrue(showSecondResult.IsSuccess);
            AssertReported(r4, showSecondResult);
            Assert.AreEqual(expected: 2, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[1]
                , kind: PageNavigationKind.ShowAt
                , fromKey: ELEMENT_PAGE
                , fromIndex: 0
                , toKey: BEHAVIOUR_PAGE
                , toIndex: 1
            );

            Assert.AreSame(screen.Pages[0], _recorder.Items[1].From.Page);
            Assert.AreSame(screen.Pages[1], _recorder.Items[1].To.Page);
            var hideContext = CreateContextWithReport(out var r5);
            var hideResult = await screen.HideActivePageAsync(hideContext, CancellationToken.None);

            Assert.IsTrue(hideResult.IsSuccess);
            AssertReported(r5, hideResult);
            Assert.AreEqual(expected: 3, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[2]
                , kind: PageNavigationKind.HideAt
                , fromKey: BEHAVIOUR_PAGE
                , fromIndex: 1
                , toKey: ELEMENT_PAGE
                , toIndex: 0
            );

            Assert.AreSame(screen.Pages[1], _recorder.Items[2].From.Page);
            Assert.AreSame(screen.Pages[0], _recorder.Items[2].To.Page);
            Assert.IsTrue(screen.CurrentPage.TryGetValue(out var currentPage));
            Assert.AreSame(screen.Pages[0], currentPage);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageListMulti_PublishesShowAtAndHideAtAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var screen = await CreateFlowAsync<UitkPageListMulti>(PageFlowKind.PageListMulti);
            var addFirstContext = CreateContextWithReport(out var r1);
            var addFirstResult = await screen.AddPageAsync(ELEMENT_PAGE, addFirstContext, CancellationToken.None);

            Assert.IsTrue(addFirstResult.IsSuccess);
            AssertReported(r1, addFirstResult);
            Assert.IsEmpty(_recorder.Items);
            var addSecondContext = CreateContextWithReport(out var r2);
            var addSecondResult = await screen.AddPageAsync(BEHAVIOUR_PAGE, addSecondContext, CancellationToken.None);

            Assert.IsTrue(addSecondResult.IsSuccess);
            AssertReported(r2, addSecondResult);
            Assert.IsEmpty(_recorder.Items);
            var showContext = CreateContextWithReport(out var r3);

            var showResult = await screen.ShowPageAtIndexAsync(
                  index: 1
                , context: showContext
                , token: CancellationToken.None
            );

            Assert.IsTrue(showResult.IsSuccess);
            AssertReported(r3, showResult);
            Assert.AreEqual(expected: 1, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[0]
                , kind: PageNavigationKind.ShowAt
                , fromKey: null
                , fromIndex: Option.None
                , toKey: BEHAVIOUR_PAGE
                , toIndex: 1
            );

            Assert.AreSame(screen.Pages[1], _recorder.Items[0].To.Page);
            var hideContext = CreateContextWithReport(out var r4);

            var hideResult = await screen.HidePageAtIndexAsync(
                  index: 1
                , context: hideContext
                , token: CancellationToken.None
            );

            Assert.IsTrue(hideResult.IsSuccess);
            AssertReported(r4, hideResult);
            Assert.AreEqual(expected: 2, actual: _recorder.Items.Count);

            AssertNavigation(
                  navigation: _recorder.Items[1]
                , kind: PageNavigationKind.HideAt
                , fromKey: BEHAVIOUR_PAGE
                , fromIndex: 1
                , toKey: null
                , toIndex: Option.None
            );

            Assert.AreSame(screen.Pages[1], _recorder.Items[1].From.Page);
            Assert.IsNull(_recorder.Items[1].To.Page);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task OutOfRangeShow_PublishesNothingAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var screen = await CreateFlowAsync<UitkPageListMulti>(PageFlowKind.PageListMulti);
            var addContext = CreateContextWithReport(out var r1);
            var addResult = await screen.AddPageAsync(ELEMENT_PAGE, addContext, CancellationToken.None);

            Assert.IsTrue(addResult.IsSuccess);
            AssertReported(r1, addResult);
            Assert.IsEmpty(_recorder.Items);
            var showContext = CreateContextWithReport(out var r2);

            var showResult = await screen.ShowPageAtIndexAsync(
                  index: 5
                , context: showContext
                , token: CancellationToken.None
            );

            Assert.IsFalse(showResult.IsSuccess);
            Assert.IsTrue(showResult.TryGetFailure(out var error));
            StringAssert.StartsWith("Index '5' is out of range", error.ToMessage());
            AssertReported(r2, showResult);
            Assert.AreEqual(error.ToMessage(), r2.Result.GetFailureOrDefault().ToMessage());
            Assert.IsEmpty(_recorder.Items);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task CancelledShow_PublishesNothingAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var screen = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            using var source = new CancellationTokenSource();
            source.Cancel();
            var context = CreateContextWithReport(out var report);
            var result = await screen.ShowPageAsync(ELEMENT_PAGE, context, source.Token);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsCancelled);
            AssertReported(report, result);
            Assert.IsTrue(report.Result.GetFailureOrDefault().IsCancelled);
            Assert.IsEmpty(_recorder.Items);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task AbortedShow_PublishesNothingAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var screen = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            TestElementPage.AbortOnAttach = PageAbortMode.Immediate;
            var context = CreateContextWithReport(out var report);
            var result = await screen.ShowPageAsync(ELEMENT_PAGE, context, CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsAborted);
            Assert.AreEqual(PageTransitionState.NotStarted, error.TransitionState);
            AssertReported(report, result);
            Assert.IsTrue(report.Result.GetFailureOrDefault().IsAborted);
            Assert.AreEqual(PageTransitionState.NotStarted, report.Result.GetFailureOrDefault().TransitionState);
            Assert.IsEmpty(_recorder.Items);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task WithdrawnShow_PublishesNothingAsync()
        {
            var screen = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            await screen.PrepoolPageAsync(BEHAVIOUR_PAGE, 1, CancellationToken.None);
            TestElementPage.AttachDelayFrames = 3;

            var source = new PageAbortSource();
            var firstContext = CreateContextWithReport(out var r1);
            var first = screen.ShowPageAsync(ELEMENT_PAGE, firstContext, CancellationToken.None);
            await WaitUntilAsync(() => screen.IsInTransition);

            var secondContext = CreateContextWithReport(out var r2) with {
                AsyncOperation = PageAsyncOperation.Sequential,
                Abort = source.Handle,
            };

            var second = screen.ShowPageAsync(BEHAVIOUR_PAGE, secondContext, CancellationToken.None);
            await WaitFramesAsync(1);
            source.Abort(PageAbortMode.Reverse);

            var secondResult = await second;
            var firstResult = await first;

            Assert.IsTrue(secondResult.TryGetFailure(out var error));
            Assert.IsTrue(error.IsWithdrawn);
            Assert.AreEqual(PageTransitionState.NotStarted, error.TransitionState);
            AssertReported(r2, secondResult);
            Assert.IsTrue(firstResult.IsSuccess);
            AssertReported(r1, firstResult);
            Assert.AreEqual(1, _recorder.Items.Count);
            AssertNavigation(
                  navigation: _recorder.Items[0]
                , kind: PageNavigationKind.Show
                , fromKey: null
                , fromIndex: Option.None
                , toKey: ELEMENT_PAGE
                , toIndex: Option.None
            );
        }

        [Test]
        public async Task PageStackSingle_BackReRentsThePooledScreenAsync()
        {
            var screen = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            _history = new PageHistory();
            Assert.IsTrue(_history.Attach(_harness.Codex).IsSuccess);

            await screen.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            await screen.ShowPageAsync(BEHAVIOUR_PAGE, ZeroDuration, CancellationToken.None);

            Assert.IsTrue(screen.TryGetPooledCount(ELEMENT_PAGE, out var pooledBefore));
            Assert.AreEqual(1, pooledBefore);

            var result = await _history.GoBackAsync(CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(screen.CurrentPage.TryGetValue(out var current));
            Assert.IsInstanceOf<TestElementPage>(current);
            Assert.IsTrue(screen.TryGetPooledCount(ELEMENT_PAGE, out var pooledElement));
            Assert.AreEqual(0, pooledElement);
            Assert.IsTrue(screen.TryGetPooledCount(BEHAVIOUR_PAGE, out var pooledBehaviour));
            Assert.AreEqual(1, pooledBehaviour);
            Assert.AreEqual(0, _history.CurrentIndex);
            Assert.AreSame(_history, _recorder.Items[_recorder.Items.Count - 1].IssuedBy);
        }

        [Test]
        public async Task PageStackSingle_BackReportsTheExceptionOfAThrowingPageAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var screen = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            _history = new PageHistory();
            Assert.IsTrue(_history.Attach(_harness.Codex).IsSuccess);

            await screen.ShowPageAsync(BEHAVIOUR_PAGE, ZeroDuration, CancellationToken.None);
            await screen.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            var thrown = new InvalidOperationException("replay-hook-failed");
            TestElementPage.ThrowOnReturnToPool = thrown;

            var result = await GoBackWithThrowingPageAsync();

            AssertReplayFailed(result, thrown);
            Assert.AreEqual(1, _history.CurrentIndex);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackSingle_BackFromTheFirstPageReportsTheHideExceptionAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var screen = await CreateFlowAsync<UitkPageStackSingle>(PageFlowKind.PageStackSingle);
            _history = new PageHistory();
            Assert.IsTrue(_history.Attach(_harness.Codex).IsSuccess);

            await screen.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            var thrown = new InvalidOperationException("replay-hook-failed");
            TestElementPage.ThrowOnReturnToPool = thrown;

            var result = await GoBackWithThrowingPageAsync();

            AssertReplayFailed(result, thrown);
            Assert.AreEqual(0, _history.CurrentIndex);
            LogAssert.NoUnexpectedReceived();
        }

        private async Task<T> CreateFlowAsync<T>(PageFlowKind kind)
            where T : UitkPageFlow
        {
            _harness = await CreateAsync(("Screen", kind, ""), ("Popup", PageFlowKind.PageStackSingle, ""));

            var flow = _harness.GetFlow<T>("Screen");
            _recorder = new NavigationRecorder(flow);
            return flow;
        }

        private async Task<Success<PageHistoryError>> GoBackWithThrowingPageAsync()
        {
            try
            {
                return await _history.GoBackAsync(CancellationToken.None);
            }
            finally
            {
                TestElementPage.ThrowOnReturnToPool = null;
            }
        }

        private static void AssertReplayFailed(Success<PageHistoryError> result, Exception thrown)
        {
            Assert.IsTrue(result.TryGetFailure(out var historyError));
            Assert.AreEqual(PageHistoryError.EnumCase.ReplayFailed, historyError.GetEnumCase());
            Assert.IsTrue(historyError.TryGetValue(out PageHistoryError.ReplayFailed failed));
            Assert.IsTrue(failed.Error._error.TryGetValue(out PageFlowError.Error.Exception wrapped));
            Assert.AreSame(thrown, wrapped.Ex);
        }

        private static PageContext CreateContextWithReport(out PageCallReport report)
        {
            report = new PageCallReport();
            return ZeroDuration with { Report = report };
        }

        private static void AssertReported(PageCallReport report, in Success<PageFlowError> result)
        {
            Assert.IsTrue(report.IsReported);
            Assert.AreEqual("Screen", report.FlowId);
            Assert.AreEqual(result.IsSuccess, report.Result.IsSuccess);
        }

        private static void AssertNavigation(
              PageNavigation navigation
            , PageNavigationKind kind
            , string fromKey
            , Option<int> fromIndex
            , string toKey
            , Option<int> toIndex
        )
        {
            Assert.AreEqual("Screen", navigation.FlowId);
            Assert.AreEqual(kind, navigation.Kind);
            Assert.AreEqual(fromKey ?? string.Empty, navigation.From.AssetKey.Value);
            Assert.IsTrue(Option.Equals(fromIndex, navigation.From.Index));
            Assert.AreEqual(toKey ?? string.Empty, navigation.To.AssetKey.Value);
            Assert.IsTrue(Option.Equals(toIndex, navigation.To.Index));
            Assert.IsNull(navigation.IssuedBy);
        }

        private sealed class NavigationRecorder : IDisposable
        {
            private readonly List<ISubscription> _subscriptions = new();

            public NavigationRecorder(UitkPageFlow flow)
            {
                var subscriber = flow.Context.Subscriber
                    .Scope(flow.Context.FlowScope)
                    .WithSubscriptions(_subscriptions)
                    .WithState(this);

                PageNavigatedMessage.Async.Subscribe(in subscriber, HandleAsync);
            }

            public List<PageNavigation> Items { get; } = new();

            public void Dispose()
                => _subscriptions.Unsubscribe();

            private static UnityTask HandleAsync(
                  NavigationRecorder recorder
                , PageNavigatedMessage.Async msg
                , PublishingContext _
            )
            {
                recorder.Items.Add(((PageNavigatedMessage)msg).Navigation);
                return UnityTask.CompletedTask;
            }
        }
    }
}
