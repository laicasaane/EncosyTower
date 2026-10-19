using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using EncosyTower.PageFlows.Internals;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageAbortContractTests
{
    [Test]
    public void AC1_DefaultHandleIsInvalid()
    {
        var handle = default(PageAbortHandle);
        Assert.IsFalse(handle.IsValid);
        Assert.IsFalse(handle.IsRequested);
        Assert.AreEqual(expected: CancellationToken.None, actual: handle.Token);

        var result = handle.Request(PageAbortMode.Reverse);
        Assert.IsTrue(result.TryGetFailure(out var error));
        Assert.AreEqual(expected: PageAbortError.EnumCase.InvalidHandle, actual: error.GetEnumCase());
        Assert.IsTrue(error.TryGetValue(out PageAbortError.InvalidHandle _));
    }

    [Test]
    public void AC2_UnboundSourceRejectsAbortWithoutCancelling()
    {
        var source = new PageAbortSource();
        Assert.IsTrue(source.Handle.IsValid);

        var result = source.Abort(PageAbortMode.Reverse);
        Assert.IsTrue(result.TryGetFailure(out var error));
        Assert.AreEqual(expected: PageAbortError.EnumCase.InvalidHandle, actual: error.GetEnumCase());
        Assert.IsFalse(source.IsAbortRequested);
        Assert.IsFalse(source.Handle.IsRequested);
        Assert.IsFalse(source.Handle.Token.IsCancellationRequested);
    }

    [Test]
    public void AC3_AbortedErrorPreservesOriginModeAndUndoneState()
    {
        var error = PageFlowError.Aborted(new PageAbortOrigin.Caller(), PageAbortMode.Reverse);
        Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);
        AssertAborted(error);

        var undone = error.WithTransitionState(PageTransitionState.Undone);
        Assert.AreEqual(expected: PageTransitionState.Undone, actual: undone.TransitionState);
        AssertAborted(undone);
        Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);

        static void AssertAborted(PageFlowError error)
        {
            Assert.AreEqual(
                  expected: "The operation was aborted by Caller in Reverse mode."
                , actual: error.ToMessage()
            );

            Assert.IsTrue(error.IsAborted);
            Assert.IsFalse(error.IsWithdrawn);
            Assert.IsFalse(error.IsCancelled);
            Assert.IsTrue(error.TryGetAbort(out var origin, out var mode));
            Assert.AreEqual(expected: PageAbortOrigin.EnumCase.Caller, actual: origin.GetEnumCase());
            Assert.IsTrue(origin.TryGetValue(out PageAbortOrigin.Caller _));
            Assert.AreEqual(expected: PageAbortMode.Reverse, actual: mode);

            Success<PageFlowError> result = error;
            Assert.IsTrue(result.IsFailure);
            Assert.IsFalse(result.IsApplied());
        }
    }

    [Test]
    public void AC4_WithdrawnErrorReportsFlowAndDefaultReverseMode()
    {
        var error = PageFlowError.Withdrawn(new PageAbortOrigin.Flow());
        Assert.AreEqual(expected: "The operation was withdrawn by Flow before it started.", actual: error.ToMessage());
        Assert.IsTrue(error.IsAborted);
        Assert.IsTrue(error.IsWithdrawn);
        Assert.IsTrue(error.TryGetAbort(out var origin, out var mode));
        Assert.AreEqual(expected: PageAbortOrigin.EnumCase.Flow, actual: origin.GetEnumCase());
        Assert.IsTrue(origin.TryGetValue(out PageAbortOrigin.Flow _));
        Assert.AreEqual(expected: PageAbortMode.Reverse, actual: mode);
    }

    [Test]
    public void AC5_AllFourIdleFlowsRejectAbort()
    {
        using var context = new TestFlowContext();
        using var singleStack = new PageStackSingle<TestPage>(context);
        using var multiStack = new PageStackMulti<TestPage>(context);
        using var singleList = new PageListSingle<TestPage>(context);
        using var multiList = new PageListMulti<TestPage>(context);
        IPageFlow[] flows = { singleStack, multiStack, singleList, multiList };
        var count = flows.Length;

        for (var i = 0; i < count; i++)
        {
            var flow = flows[i];
            var result = flow.AbortCurrent(PageAbortMode.Immediate);
            Assert.IsTrue(result.TryGetFailure(out var error), flow.GetType().Name);

            Assert.AreEqual(
                  expected: PageAbortError.EnumCase.FlowIsIdle
                , actual: error.GetEnumCase()
                , message: flow.GetType().Name
            );

            Assert.IsTrue(error.TryGetValue(out PageAbortError.FlowIsIdle _), flow.GetType().Name);
            Assert.IsFalse(flow.IsInTransition, flow.GetType().Name);
        }
    }

    [Test]
    public void AC6_SetAsideErrorsAreLoggedOnceWhenACallWithoutReaderEnds()
    {
        var registry = new PageAbortRegistry();
        var context = new PageContext();
        var state = registry.BeginCall(ref context, PageAbortCallKind.Abortable, DevLogger.Default);
        state.SetAside(PageFlowError.NoPage());

        LogAssert.Expect(LogType.Error, "The flow has no page for this operation.");
        registry.EndCall(state, Success.Yes);
        registry.EndCall(state, Success.Yes);

        Assert.IsNull(state.SetAsideErrors);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void AC7_SetAsideErrorsAreKeptForTheReaderAndNotLogged()
    {
        var registry = new PageAbortRegistry();
        var context = new PageContext();
        var state = registry.BeginCall(ref context, PageAbortCallKind.Abortable, DevLogger.Default);
        var error = PageFlowError.NoPage();
        state.MarkSetAsideReader();
        state.SetAside(error);

        registry.EndCall(state, Success.Yes);

        Assert.IsTrue(state.HasSetAsideReader);
        Assert.IsNotNull(state.SetAsideErrors);
        Assert.AreEqual(expected: 1, actual: state.SetAsideErrors.Count);
        Assert.AreEqual(expected: error.ToMessage(), actual: state.SetAsideErrors[0].ToMessage());
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void AC8_SetAsideSuppressedErrorsMovesTheUndoErrors()
    {
        var registry = new PageAbortRegistry();
        var context = new PageContext();
        var state = registry.BeginCall(ref context, PageAbortCallKind.Abortable, DevLogger.Default);
        var undoError = PageFlowError.NoPage();
        var earlierError = PageFlowError.AlreadyShown(index: 2);
        state.MarkSetAsideReader();
        state.AddSuppressedError(undoError);
        state.SetAside(earlierError);

        state.SetAsideSuppressedErrors();

        Assert.IsNull(state.SuppressedErrors);
        Assert.IsNotNull(state.SetAsideErrors);
        Assert.AreEqual(expected: 2, actual: state.SetAsideErrors.Count);
        Assert.AreEqual(expected: earlierError.ToMessage(), actual: state.SetAsideErrors[0].ToMessage());
        Assert.AreEqual(expected: undoError.ToMessage(), actual: state.SetAsideErrors[1].ToMessage());
        registry.EndCall(state, Success.Yes);
        LogAssert.NoUnexpectedReceived();
    }
}
