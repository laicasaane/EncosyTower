using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Loading;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

using ILogger = EncosyTower.Logging.ILogger;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageFlowErrorTests
{
    public static IEnumerable<TestCaseData> ErrorCases
    {
        get
        {
            var page = new TestPage("A");
            var index = 5;
            var count = 3;
            var assetKey = "page/main";
            var flowId = "Screen";
            var exception = new InvalidOperationException("hook failed");

            yield return new TestCaseData(PageFlowError.Undefined(), "An unknown page flow error occurred.")
                .SetName("ErrorContract_Undefined");

            yield return new TestCaseData(PageFlowError.NoPage(), "The flow has no page for this operation.")
                .SetName("ErrorContract_NoPage");

            yield return new TestCaseData(PageFlowError.PageNotFound(page), "The page 'A' is not in the flow.")
                .SetName("ErrorContract_PageNotFound");

            yield return new TestCaseData(
                  PageFlowError.IndexOutOfRange(index, count)
                , "Index '5' is out of range; the flow has 3 pages."
            ).SetName("ErrorContract_IndexOutOfRange");

            yield return new TestCaseData(
                  PageFlowError.AlreadyShown(index)
                , "The page at index '5' is already showing."
            ).SetName("ErrorContract_AlreadyShown");

            yield return new TestCaseData(
                  PageFlowError.CannotHideDefaultPage(index)
                , "The page at index '5' is the default page and cannot be hidden."
            ).SetName("ErrorContract_DefaultIndexPage");

            yield return new TestCaseData(
                  PageFlowError.InTransition(typeof(PageFlowErrorTests))
                , "EncosyTower.Tests.PageFlows.PageFlowErrorTests is currently in transition."
            ).SetName("ErrorContract_InTransition");

            yield return new TestCaseData(
                  PageFlowError.Cancelled()
                , "The operation was cancelled before the flow changed."
            ).SetName("ErrorContract_Cancelled");

            yield return new TestCaseData(
                  PageFlowError.AttachRejected(page)
                , "The page 'A' rejected attaching to the flow."
            ).SetName("ErrorContract_AttachRejected");

            yield return new TestCaseData(
                  PageFlowError.DetachRejected(page)
                , "The page 'A' rejected detaching from the flow."
            ).SetName("ErrorContract_DetachRejected");

            yield return new TestCaseData(
                  PageFlowError.CreateRejected(assetKey)
                , "The page created from asset key 'page/main' reported that its creation failed."
            ).SetName("ErrorContract_CreateRejected");

            var assetLoadReason = PageAssetLoadError.UnsupportedAsset(assetKey, typeof(TextAsset));

            yield return new TestCaseData(
                  PageFlowError.AssetLoadFailed(assetKey, assetLoadReason)
                , "Cannot load the page asset with key 'page/main': " + assetLoadReason.ToMessage()
            ).SetName("ErrorContract_AssetLoadFailed");

            yield return new TestCaseData(
                  PageFlowError.Exception(exception)
                , "An exception occurred in the page flow: hook failed"
            ).SetName("ErrorContract_Exception");

            yield return new TestCaseData(
                  PageFlowError.PageOwnedByAnotherFlow(page)
                , "The page 'A' is already attached to another flow."
            ).SetName("ErrorContract_PageOwnedByAnotherFlow");

            yield return new TestCaseData(
                  PageFlowError.AssetNotSinglePage(assetKey, count)
                , "The asset 'page/main' must hold exactly one page but holds 3."
            ).SetName("ErrorContract_AssetNotSinglePage");

            yield return new TestCaseData(
                  PageFlowError.PageBehaviourMissing(assetKey)
                , "The page of asset 'page/main' has no valid page behaviour."
            ).SetName("ErrorContract_PageBehaviourMissing");

            yield return new TestCaseData(
                  PageFlowError.ReplayUnanswered(flowId)
                , "No page flow answered the replayed call on the flow 'Screen'."
            ).SetName("ErrorContract_ReplayUnanswered");

            yield return new TestCaseData(
                  PageFlowError.SubscriberStateUnavailable(typeof(PageFlowErrorTests))
                , "The subscriber state of type `EncosyTower.Tests.PageFlows.PageFlowErrorTests` is not alive anymore."
            ).SetName("ErrorContract_SubscriberStateUnavailable");

            yield return new TestCaseData(
                  PageFlowError.ReplayNotSupported("PageListSingleNavigator", PageNavigationKind.Pop, flowId)
                , "The navigator 'PageListSingleNavigator' cannot replay a Pop entry on the flow 'Screen'."
            ).SetName("ErrorContract_ReplayNotSupported");

            yield return new TestCaseData(
                  PageFlowError.PageAlreadyAdded(page, PageSettingLevel.Flow)
                , "The page 'A' is already in the flow; duplicate pages are rejected at the Flow level."
            ).SetName("ErrorContract_PageAlreadyAdded");

            yield return new TestCaseData(
                  PageFlowError.PageInTransition(page)
                , "The page 'A' is part of the running transition."
            ).SetName("ErrorContract_PageInTransition");

            yield return new TestCaseData(
                  PageFlowError.PageBeingRemoved(page)
                , "The page 'A' is already being removed from the flow."
            ).SetName("ErrorContract_PageBeingRemoved");
        }
    }

    [TestCaseSource(nameof(ErrorCases))]
    public async Task ErrorMessageCancellationAndTransitionStateArePreservedAsync(PageFlowError error, string message)
    {
        Assert.AreEqual(expected: message, actual: error.ToMessage());
        Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);

        var expectedCancelled = message == "The operation was cancelled before the flow changed.";
        Assert.AreEqual(expected: expectedCancelled, actual: error.IsCancelled);

        var completed = error.WithTransitionState(PageTransitionState.Completed);
        Assert.AreEqual(expected: PageTransitionState.Completed, actual: completed.TransitionState);
        Assert.AreEqual(expected: message, actual: completed.ToMessage());
        Assert.AreEqual(expected: expectedCancelled, actual: completed.IsCancelled);
        Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);

        Success<PageFlowError> notStartedResult = error;
        Success<PageFlowError> completedResult = completed;
        Assert.IsTrue(notStartedResult.IsFailure);
        Assert.IsTrue(completedResult.IsFailure);
        Assert.IsFalse(notStartedResult.IsApplied());
        Assert.IsTrue(completedResult.IsApplied());
        await Task.CompletedTask;
    }

    [Test]
    public void AssetLoadFailedIncludesTheAssetLoadReason()
    {
        var reason = PageAssetLoadError.UnsupportedAsset("k", typeof(TextAsset));

        Assert.AreEqual(
              "Cannot load the page asset with key 'k': " + reason.ToMessage()
            , PageFlowError.AssetLoadFailed("k", reason).ToMessage()
        );
    }

    [Test]
    public async Task SuccessYesIsAppliedAsync()
    {
        Success<PageFlowError> result = Success.Yes;
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.IsApplied());
        await Task.CompletedTask;
    }

    [Test]
    public void WithSuppressedReturnsACopyAndLeavesTheOriginalUnchanged()
    {
        var original = PageFlowError.NoPage();
        var first = original.WithSuppressed(PageFlowError.Cancelled());
        var second = first.WithSuppressed(PageFlowError.AlreadyShown(2));

        Assert.AreEqual(expected: 0, actual: original.SuppressedErrors.Length);
        Assert.AreEqual(expected: 1, actual: first.SuppressedErrors.Length);
        Assert.AreEqual(expected: 2, actual: second.SuppressedErrors.Length);
        Assert.AreEqual(expected: original.ToMessage(), actual: second.ToMessage());
        Assert.IsTrue(first.SuppressedErrors[0].IsCancelled);
        Assert.IsTrue(second.SuppressedErrors[0].IsCancelled);
        Assert.AreEqual(
              expected: "The page at index '2' is already showing."
            , actual: second.SuppressedErrors[1].ToMessage()
        );
    }

    [Test]
    public void WithTransitionStateAndWithAbortModeKeepTheSuppressedErrors()
    {
        var error = PageFlowError.Withdrawn(new PageAbortOrigin.Caller())
            .WithSuppressed(PageFlowError.Cancelled())
            .WithSuppressed(PageFlowError.NoPage());

        var completed = error.WithTransitionState(PageTransitionState.Completed);
        var immediate = error.WithAbortMode(PageAbortMode.Immediate);
        var expectedSecond = error.SuppressedErrors[1].ToMessage();

        Assert.AreEqual(expected: PageTransitionState.Completed, actual: completed.TransitionState);
        Assert.AreEqual(expected: 2, actual: completed.SuppressedErrors.Length);
        Assert.IsTrue(completed.SuppressedErrors[0].IsCancelled);
        Assert.AreEqual(expected: expectedSecond, actual: completed.SuppressedErrors[1].ToMessage());

        Assert.IsTrue(immediate.TryGetAbort(origin: out _, mode: out var mode));
        Assert.AreEqual(expected: PageAbortMode.Immediate, actual: mode);
        Assert.AreEqual(expected: 2, actual: immediate.SuppressedErrors.Length);
        Assert.IsTrue(immediate.SuppressedErrors[0].IsCancelled);
        Assert.AreEqual(expected: expectedSecond, actual: immediate.SuppressedErrors[1].ToMessage());
    }

    [Test]
    public void LogWritesThePrimaryErrorFirstThenEachSuppressedErrorInOrder()
    {
        var logger = new CapturingLogger();

        var error = PageFlowError.NoPage()
            .WithSuppressed(PageFlowError.Cancelled())
            .WithSuppressed(PageFlowError.Exception(new InvalidOperationException("hook failed")))
            .WithSuppressed(PageFlowError.AlreadyShown(2));

        error.Log(logger);

        CollectionAssert.AreEqual(
              new[] {
                  "The flow has no page for this operation.",
                  "The operation was cancelled before the flow changed.",
                  "An exception occurred in the page flow: hook failed",
                  "The page at index '2' is already showing.",
              }
            , logger.Messages
        );
    }

    [Test]
    public void SubscriberNotFoundLogsAsAWarning()
    {
        var error = PageFlowError.SubscriberNotFound(typeof(PageFlowErrorTests));

        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("No subscriber received"));
        error.Log(EncosyTower.Logging.DevLogger.Default);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void HasSuppressedErrorsIsTrueOnlyWithASuppressedError()
    {
        var error = PageFlowError.NoPage();

        Assert.IsFalse(error.HasSuppressedErrors);
        Assert.IsTrue(error.WithSuppressed(PageFlowError.Cancelled()).HasSuppressedErrors);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void LogSuppressedWritesOnlyTheSuppressedErrorsInOrder()
    {
        var logger = new CapturingLogger();

        var error = PageFlowError.NoPage()
            .WithSuppressed(PageFlowError.Cancelled())
            .WithSuppressed(PageFlowError.Exception(new InvalidOperationException("hook failed")))
            .WithSuppressed(PageFlowError.AlreadyShown(index: 2));

        error.LogSuppressed(logger);

        CollectionAssert.AreEqual(
              new[] {
                  "The operation was cancelled before the flow changed.",
                  "An exception occurred in the page flow: hook failed",
                  "The page at index '2' is already showing.",
              }
            , logger.Messages
        );

        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void LogSuppressedLogsNestedSuppressedErrors()
    {
        var logger = new CapturingLogger();

        var error = PageFlowError.Cancelled()
            .WithSuppressed(PageFlowError.NoPage().WithSuppressed(PageFlowError.AlreadyShown(index: 2)));

        error.LogSuppressed(logger);

        CollectionAssert.AreEqual(
              new[] {
                  "The flow has no page for this operation.",
                  "The page at index '2' is already showing.",
              }
            , logger.Messages
        );

        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void LogSuppressedWithoutSuppressedErrorsLogsNothing()
    {
        var logger = new CapturingLogger();

        PageFlowError.NoPage().LogSuppressed(logger);

        Assert.IsEmpty(logger.Messages);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void HasSuppressedFailureIgnoresCancelsAndUnheardMessages()
    {
        var cancelled = PageFlowError.Cancelled();
        var unheard = PageFlowError.SubscriberNotFound(typeof(PageFlowErrorTests));
        var failure = PageFlowError.Exception(new InvalidOperationException("hook failed"));

        Assert.IsFalse(cancelled.WithSuppressed(cancelled).HasSuppressedFailure());
        Assert.IsFalse(cancelled.WithSuppressed(unheard).HasSuppressedFailure());
        Assert.IsTrue(cancelled.WithSuppressed(failure).HasSuppressedFailure());
        Assert.IsTrue(cancelled.WithSuppressed(unheard.WithSuppressed(failure)).HasSuppressedFailure());
        LogAssert.NoUnexpectedReceived();
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<string> Messages { get; } = new();

        public void LogException(Exception value)
        {
            Messages.Add(value.Message);
        }

        public void LogInfo(object message)
        {
            Messages.Add(message.ToString());
        }

        public void LogInfoFormat(string format, params object[] args)
        {
            Messages.Add(string.Format(format, args));
        }

        public void LogWarning(object message)
        {
            Messages.Add(message.ToString());
        }

        public void LogWarningFormat(string format, params object[] args)
        {
            Messages.Add(string.Format(format, args));
        }

        public void LogError(object message)
        {
            Messages.Add(message.ToString());
        }

        public void LogErrorFormat(string format, params object[] args)
        {
            Messages.Add(string.Format(format, args));
        }
    }
}
