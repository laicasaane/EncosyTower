using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageFlowAbortTests
{
    [UnityTest]
    public IEnumerator AB1_SingleStackReversesGatedShow()
        => UnityTask.ToCoroutine(static async () => {
            await AbortGatedShowAsync(multi: false, mode: PageAbortMode.Reverse);
        });

    [UnityTest]
    public IEnumerator AB1_MultiStackReversesGatedShow()
        => UnityTask.ToCoroutine(static async () => {
            await AbortGatedShowAsync(multi: true, mode: PageAbortMode.Reverse);
        });

    [UnityTest]
    public IEnumerator AB2_SingleStackImmediatelyUndoesGatedShow()
        => UnityTask.ToCoroutine(static async () => {
            await AbortGatedShowAsync(multi: false, mode: PageAbortMode.Immediate);
        });

    [UnityTest]
    public IEnumerator AB2_MultiStackImmediatelyUndoesGatedShow()
        => UnityTask.ToCoroutine(static async () => {
            await AbortGatedShowAsync(multi: true, mode: PageAbortMode.Immediate);
        });

    [TestCase(false)]
    [TestCase(true)]
    public async Task AB3_AttachAbortRestoresStackBeforeTransitionAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var a = new TestPage("A");
        var b = new TestPage("B");
        var context = new PageContext();
        Success<PageAbortError> request = default;
        b.HookAction = RequestOnAttach;
        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);
        var counts = MessageCounts.Capture(flowContext);

        var result = await PushAsync(flow, b, context, CancellationToken.None);

        Assert.IsTrue(request.IsSuccess);
        AssertPageAbort(result, b, PageAbortMode.Reverse, PageTransitionState.NotStarted);
        AssertMessageDelta(flowContext, counts, attach: 1, detach: 1, began: 0, ended: 0);
        CollectionAssert.AreEqual(new[] { "Attach", "Detach" }, b.Calls);
        AssertStackPages(flow, a);
        Assert.IsFalse(b.HasFlowScope);
        Assert.IsTrue(a.HasFlowScope);
        LogAssert.NoUnexpectedReceived();

        void RequestOnAttach(string hook, PageContext received)
        {
            if (hook == "Attach")
            {
                request = received.Abort.Request(PageAbortMode.Reverse);
            }
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task AB4_AfterShowAbortSkipsForwardAfterHideAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var a = new TestPage("A");
        var b = new TestPage("B") { HookAction = RequestOnAfterShow };
        var context = new PageContext();
        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);

        var result = await PushAsync(flow, b, context, CancellationToken.None);

        AssertPageAbort(result, b, PageAbortMode.Reverse, PageTransitionState.Undone);

        CollectionAssert.AreEqual(
              new[] { "Attach", "BeforeShow", "Show", "AfterShow", "Hide", "BeforeShow", "Show", "AfterShow" }
            , a.Calls
        );

        CollectionAssert.AreEqual(
              new[] { "Attach", "BeforeShow", "Show", "AfterShow", "Hide", "AfterHide", "Detach" }
            , b.Calls
        );

        Assert.AreEqual(expected: 3, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 3, actual: flowContext.EndedCount);
        Assert.AreEqual(expected: 1, actual: flowContext.DetachCount);
        AssertStackPages(flow, a);
        Assert.IsTrue(a.HasFlowScope);
        Assert.IsFalse(b.HasFlowScope);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task AB5_TransitionHideAbortKeepsPageAttachedAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var transition = new TestTransition();
        var b = new TestPage("B") { PageTransition = transition };
        Success<PageAbortError> request = default;

        if (multi)
        {
            Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);
        }

        Assert.IsTrue((await PushAsync(flow, b, context, CancellationToken.None)).IsSuccess);
        var counts = MessageCounts.Capture(flowContext);
        transition.HookAction = RequestOnHide;

        var result = await ((IPageStackStrategy<TestPage>)flow).PopAsync(context, CancellationToken.None);

        Assert.IsTrue(request.IsSuccess);
        var error = AssertAborted(result, PageAbortMode.Reverse, PageTransitionState.Undone);
        Assert.IsTrue(error.TryGetAbort(origin: out var origin, mode: out _));
        Assert.AreEqual(expected: PageAbortOrigin.EnumCase.Transition, actual: origin.GetEnumCase());
        Assert.IsTrue(origin.TryGetValue(out PageAbortOrigin.Transition payload));
        Assert.AreSame(transition, payload.Source);
        AssertMessageDelta(flowContext, counts, attach: 0, detach: 0, began: multi ? 2 : 1, ended: multi ? 2 : 1);
        AssertStackPages(flow, multi ? new[] { b, a } : new[] { b });
        Assert.IsTrue(b.HasFlowScope);
        AssertCallsEndWith(b.Calls, "Hide", "BeforeShow", "Show", "AfterShow");
        AssertCallsEndWith(transition.Calls, "BeforeHide", "Hide", "BeforeShow", "Show", "AfterShow");
        LogAssert.NoUnexpectedReceived();

        void RequestOnHide(string hook, PageTransitionContext received)
        {
            if (hook == "Hide")
            {
                request = received.Abort.Request(PageAbortMode.Reverse);
            }
        }
    }

    [TestCase(FlowKind.SingleStack)]
    [TestCase(FlowKind.MultiStack)]
    [TestCase(FlowKind.SingleList)]
    [TestCase(FlowKind.MultiList)]
    public async Task AB6_IdleFlowRejectsAbortBeforeAndAfterSuccessAsync(FlowKind kind)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateFlow(kind, flowContext);
        using var lifetime = (IDisposable)flow;
        AssertAbortError(flow.AbortCurrent(PageAbortMode.Reverse), PageAbortError.EnumCase.FlowIsIdle);
        var a = new TestPage("A");
        var context = new PageContext();

        var result = flow is IPageStackStrategy<TestPage>
            ? await PushAsync(flow, a, context, CancellationToken.None)
            : await ((IPageListStrategy<TestPage>)flow).ShowAsync(a, context, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        AssertAbortError(flow.AbortCurrent(PageAbortMode.Reverse), PageAbortError.EnumCase.FlowIsIdle);
        Assert.IsFalse(flow.IsInTransition);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task AB6_DetachAfterEndRejectsAbortAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackSingle<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");
        Success<PageAbortError> request = default;
        a.HookAction = RequestOnDetach;
        Assert.IsTrue((await stack.PushAsync(a, context, CancellationToken.None)).Result.IsSuccess);

        var result = await stack.PushAsync(b, context, CancellationToken.None);

        Assert.IsTrue(result.Result.IsSuccess);
        AssertAbortError(request, PageAbortError.EnumCase.TransitionAlreadyEnded);
        AssertStackPages(stack, b);
        LogAssert.NoUnexpectedReceived();

        void RequestOnDetach(string hook, PageContext received)
        {
            if (hook == "Detach")
            {
                request = stack.AbortCurrent(PageAbortMode.Reverse);
            }
        }
    }

    [Test]
    public async Task AB7_FirstAbortOriginAndModeWinThroughUndoAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");
        Success<PageAbortError> r1 = default;
        Success<PageAbortError> r2 = default;
        Success<PageAbortError> r3 = default;
        Success<PageAbortError> r4 = default;
        b.HookAction = RequestRepeatedly;
        Assert.IsTrue((await stack.PushAsync(a, context, CancellationToken.None)).IsSuccess);

        var result = await stack.PushAsync(b, context, CancellationToken.None);

        Assert.IsTrue(r1.IsSuccess);
        AssertAlreadyRequested(r2, b);
        AssertAlreadyRequested(r3, b);
        AssertAlreadyRequested(r4, b);
        AssertPageAbort(result, b, PageAbortMode.Reverse, PageTransitionState.Undone);
        AssertStackPages(stack, a);
        LogAssert.NoUnexpectedReceived();

        void RequestRepeatedly(string hook, PageContext received)
        {
            if (hook == "AfterShow")
            {
                r1 = received.Abort.Request(PageAbortMode.Reverse);
                r2 = stack.AbortCurrent(PageAbortMode.Immediate);
                r3 = received.Abort.Request(PageAbortMode.Immediate);
            }
            else if (hook == "AfterHide")
            {
                r4 = received.Abort.Request(PageAbortMode.Immediate);
            }
        }
    }

    [Test]
    public async Task AB8_CompletedFailedKeptAndInvalidHandlesReportTheirOutcomesAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var source1 = new PageAbortSource();
        var source2 = new PageAbortSource();
        var a = new TestPage("A");
        var b = new TestPage("B") { AttachBehaviour = PageHookBehaviour.Reject };
        var c = new TestPage("C") { HookAction = KeepHandle };
        var kept = default(PageAbortHandle);

        Assert.IsTrue((await stack.PushAsync(
              a
            , new PageContext { Abort = source1.Handle }
            , CancellationToken.None
        )).IsSuccess);

        AssertAbortError(source1.Abort(PageAbortMode.Reverse), PageAbortError.EnumCase.OperationCompleted);

        var rejected = await stack.PushAsync(b, new PageContext { Abort = source2.Handle }, CancellationToken.None);

        Assert.IsTrue(rejected.IsFailure);
        var failed = AssertAbortError(source2.Abort(PageAbortMode.Reverse), PageAbortError.EnumCase.OperationFailed);
        Assert.IsTrue(failed.TryGetValue(out PageAbortError.OperationFailed payload));

        Assert.AreEqual(expected: "The page 'B' rejected attaching to the flow.", actual: payload.Failure.ToMessage());

        Assert.IsTrue((await stack.PushAsync(c, new PageContext(), CancellationToken.None)).IsSuccess);
        Assert.IsTrue(kept.IsValid);
        AssertAbortError(kept.Request(PageAbortMode.Reverse), PageAbortError.EnumCase.OperationCompleted);
        var invalid = default(PageAbortHandle).Request(PageAbortMode.Reverse);
        AssertAbortError(invalid, PageAbortError.EnumCase.InvalidHandle);
        AssertAbortError(new PageAbortSource().Abort(PageAbortMode.Reverse), PageAbortError.EnumCase.InvalidHandle);
        AssertStackPages(stack, c, a);
        LogAssert.NoUnexpectedReceived();

        void KeepHandle(string hook, PageContext received)
        {
            if (hook == "AfterShow")
            {
                kept = received.Abort;
            }
        }
    }

    [UnityTest]
    public IEnumerator AB9_SingleStackWithdrawsQueuedPush()
        => UnityTask.ToCoroutine(static async () => {
            await WithdrawQueuedPushAsync(multi: false);
        });

    [UnityTest]
    public IEnumerator AB9_MultiStackWithdrawsQueuedPush()
        => UnityTask.ToCoroutine(static async () => {
            await WithdrawQueuedPushAsync(multi: true);
        });

    [Test]
    public async Task AB10_FailureRecoveryRefusesAbortAndFinishesForwardAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var b = new TestPage("B") {
            ShowBehaviour = PageHookBehaviour.Throw,
            HookAction = RequestOnAfterShow,
        };

        Success<PageAbortError> request = default;

        var result = await stack.PushAsync(b, new PageContext(), CancellationToken.None);

        var error = AssertAbortError(request, PageAbortError.EnumCase.RecoveringFromFailure);
        Assert.IsTrue(error.TryGetValue(out PageAbortError.RecoveringFromFailure payload));
        Assert.AreEqual(expected: PageTransitionState.Completed, actual: payload.Failure.TransitionState);
        StringAssert.StartsWith("An exception occurred in the page flow: B.Show", payload.Failure.ToMessage());
        Assert.IsTrue(result.IsFailure);
        var failure = result.GetFailureOrThrow();
        Assert.AreEqual(expected: PageTransitionState.Completed, actual: failure.TransitionState);
        Assert.AreEqual(expected: "An exception occurred in the page flow: B.Show", actual: failure.ToMessage());
        Assert.IsFalse(failure.IsAborted);
        AssertStackPages(stack, b);
        LogAssert.NoUnexpectedReceived();

        void RequestOnAfterShow(string hook, PageContext received)
        {
            if (hook == "AfterShow")
            {
                request = received.Abort.Request(PageAbortMode.Reverse);
            }
        }
    }

    [Test]
    public async Task AB11_RemoveAllIsNotAbortableAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");
        Success<PageAbortError> flowRequest = default;
        Success<PageAbortError> pageRequest = default;
        a.HookAction = RequestOnDetach;
        Assert.IsTrue((await stack.PushAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await stack.PushAsync(b, context, CancellationToken.None)).IsSuccess);

        var result = await stack.RemoveAllAsync(context, CancellationToken.None);

        AssertAbortError(flowRequest, PageAbortError.EnumCase.OperationNotAbortable);
        AssertAbortError(pageRequest, PageAbortError.EnumCase.OperationNotAbortable);
        Assert.IsTrue(result.IsSuccess);
        AssertStackPages(stack);
        Assert.IsFalse(a.HasFlowScope);
        Assert.IsFalse(b.HasFlowScope);
        LogAssert.NoUnexpectedReceived();

        void RequestOnDetach(string hook, PageContext received)
        {
            if (hook == "Detach")
            {
                flowRequest = stack.AbortCurrent(PageAbortMode.Reverse);
                pageRequest = received.Abort.Request(PageAbortMode.Reverse);
            }
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task AB12_UndoExceptionIsCarriedAndRestorationFinishesAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B") {
            HookAction = RequestOnAfterShow,
            HideBehaviour = PageHookBehaviour.Throw,
        };

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);

        var result = await PushAsync(flow, b, context, CancellationToken.None);

        AssertPageAbort(result, b, PageAbortMode.Reverse, PageTransitionState.Undone);
        AssertSingleSuppressed(result.GetFailureOrThrow(), "B.Hide");

        CollectionAssert.AreEqual(
              new[] { "Attach", "BeforeShow", "Show", "AfterShow", "Hide", "AfterHide", "Detach" }
            , b.Calls
        );

        Assert.AreEqual(expected: 3, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 3, actual: flowContext.EndedCount);
        AssertStackPages(flow, a);
        Assert.IsFalse(b.HasFlowScope);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task AB14_ListShowAbortPreservesPagesAndCurrentPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B") { HookAction = RequestOnAfterShow };
        Assert.IsTrue((await list.AddAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.AddAsync(b, context, CancellationToken.None)).IsSuccess);

        if (multi == false)
        {
            Assert.IsTrue((await list.ShowAsync(index: 0, context: context, token: CancellationToken.None)).IsSuccess);
        }

        var counts = MessageCounts.Capture(flowContext);
        var result = await list.ShowAsync(index: 1, context: context, token: CancellationToken.None);

        AssertPageAbort(result, b, PageAbortMode.Reverse, PageTransitionState.Undone);
        AssertListPages(flow, a, b);
        AssertMessageDelta(flowContext, counts, attach: 0, detach: 0, began: multi ? 1 : 2, ended: multi ? 1 : 2);
        Assert.IsTrue(b.HasFlowScope);
        AssertCallsEndWith(b.Calls, "BeforeShow", "Show", "AfterShow", "Hide", "AfterHide");

        if (multi == false)
        {
            AssertCurrentPage(flow, a);
            AssertCallsEndWith(a.Calls, "Hide", "BeforeShow", "Show", "AfterShow");
        }

        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task AB15_ListHideAbortRestoresHiddenPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext { DefaultIndex = 0 };
        var a = new TestPage("A");
        var b = new TestPage("B");
        Assert.IsTrue((await list.AddAsync(a, context, CancellationToken.None)).IsSuccess);

        if (multi == false)
        {
            Assert.IsTrue((await list.AddAsync(b, context, CancellationToken.None)).IsSuccess);
        }

        var index = multi ? 0 : 1;
        var hidden = multi ? a : b;
        Assert.IsTrue((await list.ShowAsync(index, context, CancellationToken.None)).IsSuccess);
        hidden.HookAction = RequestOnHide;
        var counts = MessageCounts.Capture(flowContext);

        var result = multi
            ? await ((PageListMulti<TestPage>)flow).HideAsync(index: 0, context: context, token: CancellationToken.None)
            : await ((PageListSingle<TestPage>)flow).HideAsync(context, CancellationToken.None);

        AssertPageAbort(result, hidden, PageAbortMode.Reverse, PageTransitionState.Undone);
        AssertCallsEndWith(hidden.Calls, "Hide", "BeforeShow", "Show", "AfterShow");
        AssertMessageDelta(flowContext, counts, attach: 0, detach: 0, began: multi ? 1 : 2, ended: multi ? 1 : 2);
        AssertListPages(flow, multi ? new[] { a } : new[] { a, b });
        Assert.IsTrue(hidden.HasFlowScope);

        if (multi == false)
        {
            AssertCurrentPage(flow, b);
        }

        LogAssert.NoUnexpectedReceived();

        static void RequestOnHide(string hook, PageContext received)
        {
            if (hook == "Hide")
            {
                Assert.IsTrue(received.Abort.Request(PageAbortMode.Reverse).IsSuccess);
            }
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task AB16_AddThenShowAbortRemovesIncomingPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var c = new TestPage("C") { HookAction = RequestBeforeShow };
        Assert.IsTrue((await list.AddAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.ShowAsync(index: 0, context: context, token: CancellationToken.None)).IsSuccess);
        var counts = MessageCounts.Capture(flowContext);

        var result = await list.ShowAsync(c, context, CancellationToken.None);

        AssertPageAbort(result, c, PageAbortMode.Immediate, PageTransitionState.Undone);
        AssertListPages(flow, a);
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Hide", "AfterHide", "Detach" }, c.Calls);
        AssertMessageDelta(flowContext, counts, attach: 1, detach: 1, began: multi ? 1 : 2, ended: multi ? 1 : 2);
        Assert.IsFalse(c.HasFlowScope);
        Assert.IsTrue(a.HasFlowScope);

        if (multi == false)
        {
            AssertCurrentPage(flow, a);
        }

        LogAssert.NoUnexpectedReceived();

        static void RequestBeforeShow(string hook, PageContext received)
        {
            if (hook == "BeforeShow")
            {
                Assert.IsTrue(received.Abort.Request(PageAbortMode.Immediate).IsSuccess);
            }
        }
    }

    [Test]
    public async Task AB17_ReusedCallerSourceLogsErrorAndRemainsBoundToFirstCallAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var source = new PageAbortSource();
        var context = new PageContext { Abort = source.Handle };
        var a = new TestPage("A");
        var b = new TestPage("B");
        Assert.IsTrue((await stack.PushAsync(a, context, CancellationToken.None)).IsSuccess);

        LogAssert.Expect(
              LogType.Error
            , "The PageAbortSource in PageContext.Abort is already bound to another call; this call runs without it."
        );

        var result = await stack.PushAsync(b, context, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        AssertAbortError(source.Abort(PageAbortMode.Reverse), PageAbortError.EnumCase.OperationCompleted);
        AssertStackPages(stack, b, a);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task AB18_ListAddAndRemoveRejectAbortAsNotAbortableAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var requests = new List<Success<PageAbortError>>();
        var a = new TestPage("A") { HookAction = RequestOnAttach };
        var b = new TestPage("B") { HookAction = RequestOnDetach };

        Assert.IsTrue((await list.AddAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.AddAsync(b, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.RemoveAsync(b, context, CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: 4, actual: requests.Count);
        var count = requests.Count;

        for (var i = 0; i < count; i++)
        {
            AssertAbortError(requests[i], PageAbortError.EnumCase.OperationNotAbortable);
        }

        AssertListPages(flow, a);
        Assert.IsTrue(a.HasFlowScope);
        Assert.IsFalse(b.HasFlowScope);
        LogAssert.NoUnexpectedReceived();

        void RequestOnAttach(string hook, PageContext received)
        {
            if (hook == "Attach")
            {
                RecordRequests(received);
            }
        }

        void RequestOnDetach(string hook, PageContext received)
        {
            if (hook == "Detach")
            {
                RecordRequests(received);
            }
        }

        void RecordRequests(PageContext received)
        {
            requests.Add(flow.AbortCurrent(PageAbortMode.Reverse));
            requests.Add(received.Abort.Request(PageAbortMode.Reverse));
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task AB19_AttachExceptionAfterAcceptedAbortKeepsAbortedOutcomeAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B") { AttachBehaviour = PageHookBehaviour.Throw };
        Success<PageAbortError> request = default;
        b.HookAction = RequestOnAttach;
        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);
        var counts = MessageCounts.Capture(flowContext);

        var result = await PushAsync(flow, b, context, CancellationToken.None);

        Assert.IsTrue(request.IsSuccess);
        AssertPageAbort(result, b, PageAbortMode.Reverse, PageTransitionState.NotStarted);
        AssertSingleSuppressed(result.GetFailureOrThrow(), "B.Attach");
        CollectionAssert.AreEqual(new[] { "Attach" }, b.Calls);
        AssertMessageDelta(flowContext, counts, attach: 1, detach: 1, began: 0, ended: 0);
        AssertStackPages(flow, a);
        Assert.IsFalse(b.HasFlowScope);
        Assert.IsTrue(a.HasFlowScope);
        LogAssert.NoUnexpectedReceived();

        void RequestOnAttach(string hook, PageContext received)
        {
            if (hook == "Attach")
            {
                request = received.Abort.Request(PageAbortMode.Reverse);
            }
        }
    }

    [Test]
    public async Task AB20_KeptPageContextGetsIndependentAbortInAnotherFlowAsync()
    {
        using var flowContext1 = new TestFlowContext();
        using var flowContext2 = new TestFlowContext();
        using var s1 = new PageStackMulti<TestPage>(flowContext1);
        using var s2 = new PageStackMulti<TestPage>(flowContext2);
        var source = new PageAbortSource();
        var kept = new PageContext();
        var b = new TestPage("B") { HookAction = KeepContext };
        var c = new TestPage("C") { HookAction = RequestOnAfterShow };
        Success<PageAbortError> request = default;

        Assert.IsTrue((await s1.PushAsync(
              b
            , new PageContext { Abort = source.Handle }
            , CancellationToken.None
        )).IsSuccess);

        Assert.IsTrue(kept.Abort.IsValid);
        var result = await s2.PushAsync(c, kept, CancellationToken.None);

        Assert.IsTrue(request.IsSuccess);
        AssertPageAbort(result, c, PageAbortMode.Reverse, PageTransitionState.Undone);
        AssertStackPages(s2);
        AssertStackPages(s1, b);
        Assert.IsTrue(b.HasFlowScope);
        Assert.IsFalse(c.HasFlowScope);
        AssertAbortError(source.Abort(PageAbortMode.Reverse), PageAbortError.EnumCase.OperationCompleted);
        AssertAbortError(kept.Abort.Request(PageAbortMode.Reverse), PageAbortError.EnumCase.OperationCompleted);
        LogAssert.NoUnexpectedReceived();

        void KeepContext(string hook, PageContext received)
        {
            if (hook == "AfterShow")
            {
                kept = received;
            }
        }

        void RequestOnAfterShow(string hook, PageContext received)
        {
            if (hook == "AfterShow")
            {
                request = received.Abort.Request(PageAbortMode.Reverse);
            }
        }
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task AB21_UndoCancellationRethrowsOnlyForCancelledCallerAsync(bool multi, bool cancelCaller)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        using var cancellation = new CancellationTokenSource();
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B") {
            HookAction = RequestOnAfterShow,
            HideBehaviour = PageHookBehaviour.ThrowCancelled,
        };

        if (cancelCaller)
        {
            b.CancelOn("Hide", cancellation);
        }

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);
        Success<PageFlowError> result = default;
        OperationCanceledException thrown = null;

        try
        {
            result = await PushAsync(flow, b, context, cancellation.Token);
        }
        catch (OperationCanceledException ex)
        {
            thrown = ex;
        }

        Assert.AreEqual(expected: cancelCaller, actual: cancellation.IsCancellationRequested);

        if (cancelCaller)
        {
            Assert.IsNotNull(thrown);
        }
        else
        {
            Assert.IsNull(thrown);
            AssertPageAbort(result, b, PageAbortMode.Reverse, PageTransitionState.Undone);

            CollectionAssert.AreEqual(
                  new[] { "Attach", "BeforeShow", "Show", "AfterShow", "Hide", "AfterHide", "Detach" }
                , b.Calls
            );

            Assert.IsFalse(b.HasFlowScope);
        }

        Assert.IsFalse(flow.IsInTransition);
        AssertStackPages(flow, a);
        Assert.AreEqual(expected: 3, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 3, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    private static IPageFlow CreateStack(bool multi, TestFlowContext context)
        => multi ? new PageStackMulti<TestPage>(context) : new PageStackSingle<TestPage>(context);

    private static IPageFlow CreateList(bool multi, TestFlowContext context)
        => multi ? new PageListMulti<TestPage>(context) : new PageListSingle<TestPage>(context);

    private static IPageFlow CreateFlow(FlowKind kind, TestFlowContext context)
        => kind switch {
            FlowKind.SingleStack => new PageStackSingle<TestPage>(context),
            FlowKind.MultiStack => new PageStackMulti<TestPage>(context),
            FlowKind.SingleList => new PageListSingle<TestPage>(context),
            _ => new PageListMulti<TestPage>(context),
        };

    private static async UnityTask<Success<PageFlowError>> PushAsync(
          IPageFlow flow
        , TestPage page
        , PageContext context
        , CancellationToken token
    )
    {
        if (flow is PageStackSingle<TestPage> single)
        {
            var push = await single.PushAsync(page, context, token);
            return push.Result;
        }

        return await ((PageStackMulti<TestPage>)flow).PushAsync(page, context, token);
    }

    private static async UnityTask AbortGatedShowAsync(bool multi, PageAbortMode mode)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var a = new TestPage("A");
        var gate = new TestGate();
        var transition = new TestTransition { ShowGate = gate };
        var b = new TestPage("B") { PageTransition = transition };
        var source = new PageAbortSource();
        Assert.IsTrue((await PushAsync(flow, a, new PageContext(), CancellationToken.None)).IsSuccess);
        var counts = MessageCounts.Capture(flowContext);
        var context = new PageContext { Abort = source.Handle };
        var pending = PushAsync(flow, b, context, CancellationToken.None);
        Success<PageFlowError> result = default;

        try
        {
            await UnityTask.WaitUntil(
                  state: transition
                , predicate: static current => current.Value == 0.5f
                , timing: UnityTaskTiming.Update
                , token: CancellationToken.None
            );

            Assert.IsFalse(gate.IsOpen);
            Assert.IsTrue(flow.IsInTransition);
            Assert.IsTrue(source.Abort(mode).IsSuccess);
        }
        finally
        {
            gate.Open();
            result = await pending;
        }

        var error = AssertAborted(result, mode, PageTransitionState.Undone);
        AssertCallerOrigin(error);
        Assert.AreEqual(expected: 0.5f, actual: transition.HideStartValue);
        Assert.AreEqual(expected: 0f, actual: transition.Value);

        Assert.AreEqual(
              expected: mode == PageAbortMode.Immediate
            , actual: transition.LastHideContext.HideOptions.Contains(PageTransitionOptions.ZeroDuration)
        );

        CollectionAssert.AreEqual(new[] { "BeforeShow", "Show", "BeforeHide", "Hide", "AfterHide" }, transition.Calls);
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "Hide", "AfterHide", "Detach" }, b.Calls);
        AssertMessageDelta(flowContext, counts, attach: 1, detach: 1, began: 2, ended: 2);
        AssertStackPages(flow, a);
        Assert.IsFalse(b.HasFlowScope);
        Assert.IsTrue(a.HasFlowScope);
        AssertOperationAborted(source.Abort(PageAbortMode.Immediate), mode);
        LogAssert.NoUnexpectedReceived();
    }

    private static async UnityTask WithdrawQueuedPushAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var source = new PageAbortSource();
        var context = new PageContext { AsyncOperation = PageAsyncOperation.Sequential, Abort = source.Handle };
        var first = PushAsync(flow, a, new PageContext(), CancellationToken.None);
        var second = PushAsync(flow, b, context, CancellationToken.None);
        Success<PageFlowError> firstResult = default;
        Success<PageFlowError> secondResult = default;
        var secondConsumed = false;

        try
        {
            await UnityTask.NextFrameAsync(CancellationToken.None);
            Assert.IsFalse(gate.IsOpen);
            Assert.IsTrue(flow.IsInTransition);
            Assert.IsTrue(source.Abort(PageAbortMode.Reverse).IsSuccess);
            secondConsumed = true;
            secondResult = await second;
            var error = AssertAborted(secondResult, PageAbortMode.Reverse, PageTransitionState.NotStarted);
            Assert.IsTrue(error.IsWithdrawn);
            AssertCallerOrigin(error);
            Assert.IsEmpty(b.Calls);
            Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
            Assert.IsFalse(b.HasFlowScope);
        }
        finally
        {
            gate.Open();
            firstResult = await first;

            if (secondConsumed == false)
            {
                secondResult = await second;
            }
        }

        Assert.IsTrue(firstResult.IsSuccess);
        Assert.IsTrue(secondResult.GetFailureOrThrow().IsWithdrawn);
        AssertStackPages(flow, a);
        AssertOperationAborted(source.Abort(PageAbortMode.Reverse), PageAbortMode.Reverse);
        LogAssert.NoUnexpectedReceived();
    }

    private static void RequestOnAfterShow(string hook, PageContext context)
    {
        if (hook == "AfterShow")
        {
            Assert.IsTrue(context.Abort.Request(PageAbortMode.Reverse).IsSuccess);
        }
    }

    private static PageFlowError AssertAborted(
          Success<PageFlowError> result
        , PageAbortMode mode
        , PageTransitionState state
    )
    {
        Assert.IsTrue(result.IsFailure);
        var error = result.GetFailureOrThrow();
        Assert.IsTrue(error.IsAborted);
        Assert.IsFalse(error.IsCancelled);
        Assert.IsFalse(result.IsApplied());
        Assert.AreEqual(expected: state, actual: error.TransitionState);
        Assert.IsTrue(error.TryGetAbort(origin: out _, mode: out var actualMode));
        Assert.AreEqual(expected: mode, actual: actualMode);
        return error;
    }

    private static void AssertPageAbort(
          Success<PageFlowError> result
        , TestPage page
        , PageAbortMode mode
        , PageTransitionState state
    )
    {
        var error = AssertAborted(result, mode, state);
        Assert.IsFalse(error.IsWithdrawn);
        Assert.IsTrue(error.TryGetAbort(origin: out var origin, mode: out _));
        AssertPageOrigin(origin, page);
    }

    private static void AssertSingleSuppressed(PageFlowError error, string message)
    {
        var suppressed = error.SuppressedErrors;

        Assert.AreEqual(expected: 1, actual: suppressed.Length);
        StringAssert.Contains(message, suppressed[0].ToMessage());
    }

    private static void AssertCallerOrigin(PageFlowError error)
    {
        Assert.IsTrue(error.TryGetAbort(origin: out var origin, mode: out _));
        Assert.AreEqual(expected: PageAbortOrigin.EnumCase.Caller, actual: origin.GetEnumCase());
        Assert.IsTrue(origin.TryGetValue(out PageAbortOrigin.Caller _));
    }

    private static void AssertPageOrigin(PageAbortOrigin origin, TestPage page)
    {
        Assert.AreEqual(expected: PageAbortOrigin.EnumCase.Page, actual: origin.GetEnumCase());
        Assert.IsTrue(origin.TryGetValue(out PageAbortOrigin.Page payload));
        Assert.AreSame(page, payload.Source);
    }

    private static PageAbortError AssertAbortError(Success<PageAbortError> result, PageAbortError.EnumCase expected)
    {
        Assert.IsTrue(result.TryGetFailure(out var error));
        Assert.AreEqual(expected: expected, actual: error.GetEnumCase());
        return error;
    }

    private static void AssertAlreadyRequested(Success<PageAbortError> result, TestPage page)
    {
        var error = AssertAbortError(result, PageAbortError.EnumCase.AbortAlreadyRequested);
        Assert.IsTrue(error.TryGetValue(out PageAbortError.AbortAlreadyRequested payload));
        AssertPageOrigin(payload.Origin, page);
        Assert.AreEqual(expected: PageAbortMode.Reverse, actual: payload.Mode);
    }

    private static void AssertOperationAborted(Success<PageAbortError> result, PageAbortMode mode)
    {
        var error = AssertAbortError(result, PageAbortError.EnumCase.OperationAborted);
        Assert.IsTrue(error.TryGetValue(out PageAbortError.OperationAborted payload));
        Assert.AreEqual(expected: PageAbortOrigin.EnumCase.Caller, actual: payload.Origin.GetEnumCase());
        Assert.IsTrue(payload.Origin.TryGetValue(out PageAbortOrigin.Caller _));
        Assert.AreEqual(expected: mode, actual: payload.Mode);
    }

    private static void AssertStackPages(IPageFlow flow, params TestPage[] expected)
    {
        Assert.IsFalse(flow.IsInTransition);
        var current = ((IHasCurrentPage<TestPage>)flow).CurrentPage;
        Assert.AreEqual(expected: expected.Length > 0, actual: current.HasValue);

        if (expected.Length > 0)
        {
            Assert.AreSame(expected[0], current.GetValueOrThrow());
        }

        if (flow is PageStackMulti<TestPage> multi)
        {
            Assert.AreEqual(expected: expected.Length, actual: multi.PageCollection.Count);
            CollectionAssert.AreEqual(expected, multi.PageCollection);
        }
    }

    private static void AssertListPages(IPageFlow flow, params TestPage[] expected)
    {
        Assert.IsFalse(flow.IsInTransition);

        if (flow is PageListSingle<TestPage> single)
        {
            Assert.AreEqual(expected: expected.Length, actual: single.Pages.Count);
            CollectionAssert.AreEqual(expected, single.Pages);
        }
        else
        {
            var multi = (PageListMulti<TestPage>)flow;
            Assert.AreEqual(expected: expected.Length, actual: multi.Pages.Count);
            CollectionAssert.AreEqual(expected, multi.Pages);
        }
    }

    private static void AssertCurrentPage(IPageFlow flow, TestPage expected)
    {
        var current = ((IHasCurrentPage<TestPage>)flow).CurrentPage;
        Assert.IsTrue(current.TryGetValue(out var page));
        Assert.AreSame(expected, page);
    }

    private static void AssertCallsEndWith(List<string> calls, params string[] expected)
    {
        Assert.GreaterOrEqual(calls.Count, expected.Length);
        CollectionAssert.AreEqual(expected, calls.GetRange(calls.Count - expected.Length, expected.Length));
    }

    private static void AssertMessageDelta(
          TestFlowContext context
        , MessageCounts before
        , int attach
        , int detach
        , int began
        , int ended
    )
    {
        Assert.AreEqual(expected: attach, actual: context.AttachCount - before.Attach);
        Assert.AreEqual(expected: detach, actual: context.DetachCount - before.Detach);
        Assert.AreEqual(expected: began, actual: context.BeganCount - before.Began);
        Assert.AreEqual(expected: ended, actual: context.EndedCount - before.Ended);
    }

    public enum FlowKind
    {
        SingleStack,
        MultiStack,
        SingleList,
        MultiList,
    }

    private readonly record struct MessageCounts(int Attach, int Detach, int Began, int Ended)
    {
        internal static MessageCounts Capture(TestFlowContext context)
            => new(context.AttachCount, context.DetachCount, context.BeganCount, context.EndedCount);
    }
}
