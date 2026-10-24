using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.History;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageHistoryRecordingTests
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
    public void DefaultPageRefIsNoneAndInvalid()
    {
        Assert.IsTrue(default(PageRef) == PageRef.None);
        Assert.IsFalse(PageRef.None.IsValid);
    }

    [Test]
    public async Task R1_MatchingHideMovesBack_UnmatchedHideIsIgnoredAsync()
    {
        var popup = Fake("Popup", PageFlowKind.PageStackMulti);
        Assert.IsTrue((await popup.ShowAsync("Q")).IsSuccess);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(popup).IsSuccess);

        Assert.IsTrue((await popup.HideActiveAsync()).IsSuccess);
        Assert.AreEqual(expected: 0, actual: history.Entries.Count);
        Assert.AreEqual(expected: -1, actual: history.CurrentIndex);

        Assert.IsTrue((await popup.ShowAsync("P")).IsSuccess);
        Assert.AreEqual(expected: 1, actual: history.Entries.Count);
        Assert.AreEqual(expected: PageNavigationKind.Push, actual: history.Entries[0].Kind);
        Assert.AreEqual(expected: "P", actual: history.Entries[0].To.AssetKey.Value);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);

        Assert.IsTrue((await popup.HideActiveAsync()).IsSuccess);
        Assert.AreEqual(expected: 1, actual: history.Entries.Count);
        Assert.AreEqual(expected: -1, actual: history.CurrentIndex);
        Assert.IsTrue(history.CanGoForward);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task R2_BackStackRecordsShowsAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        var issuer = new object();
        var firstReport = new PageCallReport();
        var secondReport = new PageCallReport();
        var firstContext = new PageContext { IssuedBy = issuer, Report = firstReport };
        var secondContext = new PageContext { IssuedBy = issuer, Report = secondReport };

        Assert.IsTrue((await screen.ShowAsync("A", firstContext)).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B", secondContext)).IsSuccess);
        Assert.AreEqual(expected: 2, actual: history.Entries.Count);

        var first = history.Entries[0];
        Assert.AreEqual(expected: "Screen", actual: first.FlowId);
        Assert.AreEqual(expected: PageNavigationKind.Show, actual: first.Kind);
        Assert.AreEqual(expected: PageRef.None, actual: first.From);
        Assert.AreEqual(expected: "A", actual: first.To.AssetKey.Value);
        Assert.IsFalse(first.To.Index.HasValue);
        Assert.IsNull(first.To.Page);
        Assert.IsNull(first.Context.IssuedBy);
        Assert.IsNull(first.Context.Report);

        var second = history.Entries[1];
        Assert.AreEqual(expected: "Screen", actual: second.FlowId);
        Assert.AreEqual(expected: PageNavigationKind.Show, actual: second.Kind);
        Assert.AreEqual(expected: first.To, actual: second.From);
        Assert.AreEqual(expected: "B", actual: second.To.AssetKey.Value);
        Assert.IsFalse(second.To.Index.HasValue);
        Assert.IsNull(second.To.Page);
        Assert.IsNull(second.Context.IssuedBy);
        Assert.IsNull(second.Context.Report);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);
        Assert.IsTrue(history.CanGoBack);
        Assert.IsFalse(history.CanGoForward);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task R3_OneNavigationHistoryPerFlowAsync()
    {
        var x = Fake("X", PageFlowKind.PageStackSingle);
        var y = Fake("Y", PageFlowKind.PageStackSingle);
        var h1 = Track(new PageHistory());
        var h2 = Track(new PageHistory());
        var log = Track(new PageHistoryLog());
        var h3 = Track(new PageHistory());
        Assert.IsTrue(h1.Attach(x).IsSuccess);

        var rejected = h2.Attach(new[] { y, x });
        Assert.IsTrue(rejected.IsFailure);
        var error = rejected.GetFailureOrDefault();
        Assert.AreEqual(expected: PageHistoryError.EnumCase.FlowAlreadyCovered, actual: error.GetEnumCase());
        Assert.IsTrue(error.TryGetValue(out PageHistoryError.FlowAlreadyCovered value));
        Assert.AreEqual(expected: "X", actual: value.FlowId);
        Assert.IsFalse(h2.IsAttached);
        Assert.IsTrue(log.Attach(x).IsSuccess);
        Assert.IsTrue(h3.Attach(y).IsSuccess);

        h1.Detach();
        Assert.IsTrue(h2.Attach(x).IsSuccess);
        Assert.IsTrue((await x.ShowAsync("A")).IsSuccess);
        Assert.AreEqual(expected: 0, actual: h1.Entries.Count);
        Assert.AreEqual(expected: 1, actual: h2.Entries.Count);
        Assert.AreEqual(expected: "A", actual: h2.Entries[0].To.AssetKey.Value);
        Assert.AreEqual(expected: 1, actual: log.Entries.Count);
        Assert.AreEqual(expected: "A", actual: log.Entries[0].To.AssetKey.Value);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task R4_MaxEntriesDropsTheOldestAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory { MaxEntries = 3 });
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        var keys = new[] { "A", "B", "C", "D", "E" };
        var count = keys.Length;

        for (var i = 0; i < count; i++)
        {
            Assert.IsTrue((await screen.ShowAsync(keys[i])).IsSuccess);
        }

        Assert.AreEqual(expected: 3, actual: history.Entries.Count);
        Assert.AreEqual(expected: "C", actual: history.Entries[0].To.AssetKey.Value);
        Assert.AreEqual(expected: "D", actual: history.Entries[1].To.AssetKey.Value);
        Assert.AreEqual(expected: "E", actual: history.Entries[2].To.AssetKey.Value);
        Assert.AreEqual(expected: 2, actual: history.CurrentIndex);

        history.MaxEntries = 2;
        Assert.AreEqual(expected: 2, actual: history.Entries.Count);
        Assert.AreEqual(expected: "D", actual: history.Entries[0].To.AssetKey.Value);
        Assert.AreEqual(expected: "E", actual: history.Entries[1].To.AssetKey.Value);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task R5_ClearKeepsTheHistoryAttachedAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);
        Assert.AreEqual(expected: 2, actual: history.Entries.Count);

        history.Clear();
        Assert.AreEqual(expected: 0, actual: history.Entries.Count);
        Assert.AreEqual(expected: -1, actual: history.CurrentIndex);
        Assert.IsTrue(history.IsAttached);

        Assert.IsTrue((await screen.ShowAsync("C")).IsSuccess);
        Assert.AreEqual(expected: 1, actual: history.Entries.Count);
        Assert.AreEqual(expected: "C", actual: history.Entries[0].To.AssetKey.Value);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task R6_CallLogRecordsHidesAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var log = Track(new PageHistoryLog());
        Assert.IsTrue(log.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.HideActiveAsync()).IsSuccess);
        Assert.AreEqual(expected: 2, actual: log.Entries.Count);
        Assert.AreEqual(expected: PageNavigationKind.Show, actual: log.Entries[0].Kind);
        Assert.AreEqual(expected: PageRef.None, actual: log.Entries[0].From);
        Assert.AreEqual(expected: "A", actual: log.Entries[0].To.AssetKey.Value);
        Assert.AreEqual(expected: PageNavigationKind.Hide, actual: log.Entries[1].Kind);
        Assert.AreEqual(expected: log.Entries[0].To, actual: log.Entries[1].From);
        Assert.AreEqual(expected: PageRef.None, actual: log.Entries[1].To);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task R7_HistoriesIgnoreTheirOwnCallsAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        var log = Track(new PageHistoryLog());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue(log.Attach(screen).IsSuccess);
        var ownContext = new PageContext { IssuedBy = history };

        Assert.IsTrue((await screen.ShowAsync("A", ownContext)).IsSuccess);
        Assert.AreEqual(expected: 0, actual: history.Entries.Count);
        Assert.AreEqual(expected: 1, actual: log.Entries.Count);
        Assert.AreEqual(expected: "A", actual: log.Entries[0].To.AssetKey.Value);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task R8_AttachRefusesInvalidRequestsAsync()
    {
        var x = Fake("X", PageFlowKind.PageStackSingle);
        var x1 = Fake("X", PageFlowKind.PageStackSingle);
        var x2 = Fake("X", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        var emptyHistory = Track(new PageHistory());
        var duplicateHistory = Track(new PageHistory());
        Assert.IsTrue(history.Attach(x).IsSuccess);

        var alreadyAttached = history.Attach(x);
        Assert.IsTrue(alreadyAttached.IsFailure);

        Assert.AreEqual(
              expected: PageHistoryError.EnumCase.AlreadyAttached
            , actual: alreadyAttached.GetFailureOrDefault().GetEnumCase()
        );

        var noFlow = emptyHistory.Attach(Array.Empty<IPageFlowRecordable>());
        Assert.IsTrue(noFlow.IsFailure);

        Assert.AreEqual(
              expected: PageHistoryError.EnumCase.NoFlowToAttach
            , actual: noFlow.GetFailureOrDefault().GetEnumCase()
        );

        Assert.IsFalse(emptyHistory.IsAttached);
        LogAssert.Expect(LogType.Warning, new Regex("appears more than once"));
        Assert.IsTrue(duplicateHistory.Attach(new[] { x1, x2 }).IsSuccess);
        Assert.IsTrue(duplicateHistory.IsAttached);

        Assert.IsTrue((await x2.ShowAsync("Ignored")).IsSuccess);
        Assert.AreEqual(expected: 0, actual: duplicateHistory.Entries.Count);
        Assert.IsTrue((await x1.ShowAsync("Accepted")).IsSuccess);
        Assert.AreEqual(expected: 1, actual: duplicateHistory.Entries.Count);
        Assert.AreEqual(expected: "X", actual: duplicateHistory.Entries[0].FlowId);
        Assert.AreEqual(expected: "Accepted", actual: duplicateHistory.Entries[0].To.AssetKey.Value);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task R9_StrategyExceptionIsLoggedAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory(new ThrowingHistory()));
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        LogAssert.Expect(LogType.Exception, new Regex("ThrowingHistory"));
        var result = await screen.ShowAsync("A");
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expected: 0, actual: history.Entries.Count);
        Assert.AreEqual(expected: -1, actual: history.CurrentIndex);
        CollectionAssert.AreEqual(new[] { "A" }, screen.Stack);
        LogAssert.NoUnexpectedReceived();
    }

    private FakeHistoryFlow Fake(string flowId, PageFlowKind kind, params string[] listKeys)
        => Track(new FakeHistoryFlow(_context, flowId, kind, listKeys));

    private T Track<T>(T disposable)
        where T : IDisposable
    {
        _disposables.Add(disposable);
        return disposable;
    }

    private sealed class ThrowingHistory : IPageHistoryRecordingStrategy
    {
        public PageHistoryChange OnNavigated(
              in PageNavigation navigation
            , ReadOnlySpan<PageHistoryEntry> entries
            , int currentIndex
        )
            => throw new InvalidOperationException("ThrowingHistory");
    }
}
