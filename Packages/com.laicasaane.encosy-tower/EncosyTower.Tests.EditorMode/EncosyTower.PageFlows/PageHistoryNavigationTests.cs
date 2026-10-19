using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.History;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageHistoryNavigationTests
{
    private readonly List<IDisposable> _disposables = new();
    private TestFlowContext _context;

    [SetUp]
    public void SetUp()
    {
        _context = new TestFlowContext();
    }

    [TearDown]
    public void TearDown()
    {
        for (var i = _disposables.Count - 1; i >= 0; i--)
        {
            _disposables[i].Dispose();
        }

        _disposables.Clear();
        _context.Dispose();
        _context = null;
    }

    [Test]
    public void ReplayLearnOnlyRequiresNoSuppressedFailure()
    {
        var error = PageFlowError.SubscriberNotFound(typeof(PageHistoryNavigationTests))
            .WithTransitionState(PageTransitionState.Completed);

        Success<PageFlowError> unheard = error;

        Success<PageFlowError> failed = error.WithSuppressed(
              PageFlowError.Exception(new InvalidOperationException("navigated subscriber failed"))
        );

        Assert.IsTrue(PageHistoryReplay.IsLearnOnly(unheard));
        Assert.IsFalse(PageHistoryReplay.IsLearnOnly(failed));
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task N1_BackAndForward_NewShowClearsForwardAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: "Show:A", actual: screen.Calls[^1]);
        CollectionAssert.AreEqual(new[] { "A" }, screen.Stack);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);

        Assert.IsTrue((await history.GoForwardAsync(CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: "Show:B", actual: screen.Calls[^1]);
        CollectionAssert.AreEqual(new[] { "B" }, screen.Stack);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("C")).IsSuccess);
        Assert.AreEqual(expected: 2, actual: history.Entries.Count);
        Assert.AreEqual(expected: "A", actual: history.Entries[0].To.AssetKey.Value);
        Assert.AreEqual(expected: "C", actual: history.Entries[1].To.AssetKey.Value);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);
        Assert.IsFalse(history.CanGoForward);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: "Show:A", actual: screen.Calls[^1]);
        CollectionAssert.AreEqual(new[] { "A" }, screen.Stack);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: "HideActive", actual: screen.Calls[^1]);
        Assert.IsEmpty(screen.Stack);
        Assert.AreEqual(expected: -1, actual: history.CurrentIndex);
        var callCount = screen.Calls.Count;
        var result = await history.GoBackAsync(CancellationToken.None);
        Assert.IsTrue(result.IsFailure);

        Assert.AreEqual(
              expected: PageHistoryError.EnumCase.NothingToGoBack
            , actual: result.GetFailureOrDefault().GetEnumCase()
        );

        Assert.AreEqual(expected: callCount, actual: screen.Calls.Count);
        Assert.AreEqual(expected: -1, actual: history.CurrentIndex);
    }

    [Test]
    public async Task N2_CallLogRecordsTheReplayedBackAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        var log = Track(new PageHistoryLog());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue(log.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: 2, actual: history.Entries.Count);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);
        Assert.AreEqual(expected: 3, actual: log.Entries.Count);
        Assert.AreEqual(expected: PageNavigationKind.Show, actual: log.Entries[2].Kind);
        Assert.AreEqual(expected: "A", actual: log.Entries[2].To.AssetKey.Value);
    }

    [Test]
    public async Task N3_BackAcrossTwoFlowsAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var popup = Fake("Popup", PageFlowKind.PageStackMulti);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(new IPageFlowRecordable[] { screen, popup }).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await popup.ShowAsync("P")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        CollectionAssert.AreEqual(new[] { "Show:A", "Show:B", "Show:A" }, screen.Calls);
        CollectionAssert.AreEqual(new[] { "Show:P" }, popup.Calls);
        CollectionAssert.AreEqual(new[] { "A" }, screen.Stack);
        CollectionAssert.AreEqual(new[] { "P" }, popup.Stack);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        CollectionAssert.AreEqual(new[] { "Show:A", "Show:B", "Show:A" }, screen.Calls);
        CollectionAssert.AreEqual(new[] { "Show:P", "HideActive" }, popup.Calls);
        CollectionAssert.AreEqual(new[] { "A" }, screen.Stack);
        Assert.IsEmpty(popup.Stack);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        CollectionAssert.AreEqual(new[] { "Show:A", "Show:B", "Show:A", "HideActive" }, screen.Calls);
        CollectionAssert.AreEqual(new[] { "Show:P", "HideActive" }, popup.Calls);
        Assert.IsEmpty(screen.Stack);
        Assert.IsEmpty(popup.Stack);
        Assert.AreEqual(expected: -1, actual: history.CurrentIndex);
    }

    [Test]
    public async Task N4_FailedAndAbortedReplaysKeepTheIndexAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);

        screen.NextResults.Enqueue(PageFlowError.CreateRejected("A"));
        var failedResult = await history.GoBackAsync(CancellationToken.None);
        Assert.IsTrue(failedResult.IsFailure);

        Assert.AreEqual(
              expected: PageHistoryError.EnumCase.ReplayFailed
            , actual: failedResult.GetFailureOrDefault().GetEnumCase()
        );

        Assert.IsTrue(failedResult.GetFailureOrDefault().TryGetValue(out PageHistoryError.ReplayFailed failed));
        StringAssert.Contains("reported that its creation failed", failed.Error.ToMessage());
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);
        CollectionAssert.AreEqual(new[] { "B" }, screen.Stack);

        screen.NextResults.Enqueue(PageFlowError.Aborted(new PageAbortOrigin.Caller(), PageAbortMode.Immediate));
        var abortedResult = await history.GoBackAsync(CancellationToken.None);
        Assert.IsTrue(abortedResult.IsFailure);

        Assert.AreEqual(
              expected: PageHistoryError.EnumCase.ReplayAborted
            , actual: abortedResult.GetFailureOrDefault().GetEnumCase()
        );

        Assert.IsTrue(abortedResult.GetFailureOrDefault().TryGetValue(out PageHistoryError.ReplayAborted aborted));
        Assert.IsTrue(aborted.Error.IsAborted);
        Assert.IsFalse(aborted.Error.IsWithdrawn);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);
        CollectionAssert.AreEqual(new[] { "B" }, screen.Stack);

        screen.NextResults.Enqueue(PageFlowError.Withdrawn(new PageAbortOrigin.Caller()));
        var withdrawnResult = await history.GoBackAsync(CancellationToken.None);
        Assert.IsTrue(withdrawnResult.IsFailure);

        Assert.AreEqual(
              expected: PageHistoryError.EnumCase.ReplayAborted
            , actual: withdrawnResult.GetFailureOrDefault().GetEnumCase()
        );

        Assert.IsTrue(withdrawnResult.GetFailureOrDefault().TryGetValue(out PageHistoryError.ReplayAborted withdrawn));
        Assert.IsTrue(withdrawn.Error.IsWithdrawn);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);
        CollectionAssert.AreEqual(new[] { "B" }, screen.Stack);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: "Show:A", actual: screen.Calls[^1]);
        CollectionAssert.AreEqual(new[] { "A" }, screen.Stack);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);
        Assert.AreEqual(expected: 2, actual: history.Entries.Count);
    }

    [Test]
    public async Task N7_MissingFlowIsFlowNotFoundAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        screen.IsAlive = false;
        var back = await history.GoBackAsync(CancellationToken.None);
        Assert.IsTrue(back.IsFailure);

        Assert.AreEqual(
              expected: PageHistoryError.EnumCase.FlowNotFound
            , actual: back.GetFailureOrDefault().GetEnumCase()
        );

        Assert.IsTrue(back.GetFailureOrDefault().TryGetValue(out PageHistoryError.FlowNotFound dead));
        Assert.AreEqual(expected: "Screen", actual: dead.FlowId);
        CollectionAssert.AreEqual(new[] { "Show:A" }, screen.Calls);
        CollectionAssert.AreEqual(new[] { "A" }, screen.Stack);
        Assert.AreEqual(expected: 1, actual: history.Entries.Count);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);
    }

    [Test]
    public async Task N8_ListNavigatorsReplayAsync()
    {
        var tabs = Fake("Tabs", PageFlowKind.PageListSingle, "A", "B", "C");
        var panels = Fake("Panels", PageFlowKind.PageListMulti, "A", "B");
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(new IPageFlowRecordable[] { tabs, panels }).IsSuccess);
        Assert.IsTrue((await tabs.ShowAtAsync(index: 0)).IsSuccess);
        Assert.IsTrue((await tabs.ShowAtAsync(index: 2)).IsSuccess);
        CollectionAssert.AreEqual(new[] { "ShowAt:0", "ShowAt:2" }, tabs.Calls);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        CollectionAssert.AreEqual(new[] { "ShowAt:0", "ShowAt:2", "ShowAt:0" }, tabs.Calls);
        Assert.AreEqual(expected: 0, actual: tabs.CurrentPageIndex);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);

        Assert.IsTrue((await history.GoForwardAsync(CancellationToken.None)).IsSuccess);
        CollectionAssert.AreEqual(new[] { "ShowAt:0", "ShowAt:2", "ShowAt:0", "ShowAt:2" }, tabs.Calls);
        Assert.AreEqual(expected: 2, actual: tabs.CurrentPageIndex);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);

        Assert.IsTrue((await panels.ShowAtAsync(index: 1)).IsSuccess);
        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        CollectionAssert.AreEqual(new[] { "ShowAt:1", "HideAt:1" }, panels.Calls);
        Assert.IsEmpty(panels.ShownPageIndices);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);

        Assert.IsTrue((await history.GoForwardAsync(CancellationToken.None)).IsSuccess);
        CollectionAssert.AreEqual(new[] { "ShowAt:1", "HideAt:1", "ShowAt:1" }, panels.Calls);
        CollectionAssert.AreEqual(new[] { 1 }, panels.ShownPageIndices);
        Assert.AreEqual(expected: 2, actual: history.CurrentIndex);
        CollectionAssert.AreEqual(new[] { "ShowAt:0", "ShowAt:2", "ShowAt:0", "ShowAt:2" }, tabs.Calls);
    }

    [Test]
    public async Task N9_ReplayUsesTheUndoneEntryContextAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory { ReplayOperation = PageAsyncOperation.Drop });
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        var showContext = new PageContext { ShowOptions = PageTransitionOptions.ZeroDuration };
        Assert.IsTrue((await screen.ShowAsync("B", showContext)).IsSuccess);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: "Show:A", actual: screen.Calls[^1]);
        var replayed = screen.Contexts[screen.Contexts.Count - 1];
        Assert.AreEqual(expected: PageTransitionOptions.ZeroDuration, actual: replayed.ShowOptions);
        Assert.AreSame(history, replayed.IssuedBy);
        Assert.AreEqual(expected: PageAsyncOperation.Drop, actual: replayed.AsyncOperation);
        Assert.IsNotNull(replayed.Receipt);
    }

    [Test]
    public async Task N10_BusyRefusesASecondReplayAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);

        screen.Gate = new TaskCompletionSource<bool>();
        var first = history.GoBackAsync(CancellationToken.None);
        var second = await history.GoBackAsync(CancellationToken.None);

        var indexWhileBusy = history.CurrentIndex;
        screen.Gate.SetResult(true);
        var firstResult = await first;
        Assert.IsTrue(second.IsFailure);
        Assert.AreEqual(expected: PageHistoryError.EnumCase.Busy, actual: second.GetFailureOrDefault().GetEnumCase());
        Assert.AreEqual(expected: 1, actual: indexWhileBusy);
        Assert.IsTrue(firstResult.IsSuccess);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);
        Assert.AreEqual(expected: 2, actual: history.Entries.Count);
        CollectionAssert.AreEqual(new[] { "Show:A", "Show:B", "Show:A" }, screen.Calls);
        CollectionAssert.AreEqual(new[] { "A" }, screen.Stack);
    }

    [Test]
    public async Task N11_CustomNavigatorFactoryReplaysBackAndForwardAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        CountingNavigator navigator = null;
        var history = Track(new PageHistory { NavigatorFactory = CreateNavigator });
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        Assert.IsNotNull(navigator);
        Assert.AreEqual(expected: 1, actual: navigator.BackCount);
        Assert.AreEqual(expected: 0, actual: navigator.ForwardCount);
        Assert.AreEqual(expected: "Show:A", actual: screen.Calls[^1]);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);

        Assert.IsTrue((await history.GoForwardAsync(CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: 1, actual: navigator.BackCount);
        Assert.AreEqual(expected: 1, actual: navigator.ForwardCount);
        Assert.AreEqual(expected: "Show:B", actual: screen.Calls[^1]);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);

        IPageHistoryNavigator CreateNavigator(IPageFlowRecordable flow)
        {
            navigator = new CountingNavigator(flow);
            return navigator;
        }
    }

    [Test]
    public async Task N12_ReplayWithNoAnsweringFlowReturnsReplayUnansweredAsync()
    {
        var screen = new FakeHistoryFlow(_context, "Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);

        screen.Dispose();
        var result = await history.GoBackAsync(CancellationToken.None);

        Assert.IsTrue(result.IsFailure);

        Assert.AreEqual(
              expected: PageHistoryError.EnumCase.ReplayFailed
            , actual: result.GetFailureOrDefault().GetEnumCase()
        );

        Assert.IsTrue(result.GetFailureOrDefault().TryGetValue(out PageHistoryError.ReplayFailed failed));

        Assert.AreEqual(
              expected: PageFlowError.ReplayUnanswered("Screen").ToMessage()
            , actual: failed.Error.ToMessage()
        );

        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task N13_ReplayWithAThrowingHandlerReturnsReplayFailedWithTheExceptionAsync()
    {
        var screen = new FakeHistoryFlow(_context, "Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);

        screen.Dispose();

        var exception = new InvalidOperationException("replay handler failed");
        var subscriptions = new List<ISubscription>();

        var subscriber = _context.Subscriber.Scope(screen.FlowScope)
            .WithState(exception)
            .WithSubscriptions(subscriptions);

        ShowPageMessage.Async.Subscribe(in subscriber, Throw);

        var result = await history.GoBackAsync(CancellationToken.None);

        subscriptions.Unsubscribe();

        Assert.IsTrue(result.IsFailure);

        Assert.AreEqual(
              expected: PageHistoryError.EnumCase.ReplayFailed
            , actual: result.GetFailureOrDefault().GetEnumCase()
        );

        Assert.IsTrue(result.GetFailureOrDefault().TryGetValue(out PageHistoryError.ReplayFailed failed));
        Assert.IsTrue(failed.Error._error.TryGetValue(out PageFlowError.Error.Exception wrapped));
        Assert.AreSame(exception, wrapped.Ex);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);
        LogAssert.NoUnexpectedReceived();

        static UnityTask Throw(
              InvalidOperationException ex
            , ShowPageMessage.Async message
            , PublishingContext publishingContext
        )
            => throw ex;
    }

    [Test]
    public async Task N14_ReplayShowCarriesTheAssetKeyThroughShowPageMessageAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var key = new AssetKey("Pages/Screens/a");

        var result = await PageHistoryReplay.ShowAsync(screen, key, new PageContext(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expected: key, actual: screen.LastShownKey);
        Assert.AreEqual(expected: "Show:Pages/Screens/a", actual: screen.Calls[^1]);
        CollectionAssert.AreEqual(new[] { "Pages/Screens/a" }, screen.Stack);
    }

    private FakeHistoryFlow Fake(string flowId, PageFlowKind kind, params string[] listKeys)
        => Track(new FakeHistoryFlow(_context, flowId, kind, listKeys));

    private T Track<T>(T disposable)
        where T : IDisposable
    {
        _disposables.Add(disposable);
        return disposable;
    }

    private sealed class CountingNavigator : IPageHistoryNavigator
    {
        private readonly PageStackSingleNavigator _inner;

        public CountingNavigator(IPageFlowRecordable flow)
        {
            _inner = new PageStackSingleNavigator(flow);
        }

        public int BackCount { get; private set; }

        public int ForwardCount { get; private set; }

        public UnityTask<Success<PageFlowError>> BackAsync(
              PageHistoryEntry entry
            , PageAsyncOperation operation
            , CancellationToken token
        )
        {
            BackCount++;
            return _inner.BackAsync(entry, operation, token);
        }

        public UnityTask<Success<PageFlowError>> ForwardAsync(
              PageHistoryEntry entry
            , PageAsyncOperation operation
            , CancellationToken token
        )
        {
            ForwardCount++;
            return _inner.ForwardAsync(entry, operation, token);
        }
    }
}
