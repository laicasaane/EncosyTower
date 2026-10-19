using System;
using System.Buffers;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageTransitionMessageTests
{
    private static IPageFlow CreateStack(bool multi, TestFlowContext context)
        => multi ? new PageStackMulti<TestPage>(context) : new PageStackSingle<TestPage>(context);

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

    [TestCase(false)]
    [TestCase(true)]
    public async Task TM1_PushOnEmpty_PublishesOnePairForTheShownPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var a = new TestPage("A");

        var result = await PushAsync(flow, a, new PageContext(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { "Began A Show -", "Ended A Show - Completed" }, flowContext.TransitionLog);
        Assert.AreSame(flow, flowContext.LastTransitionFlow);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task TM2_PushOverPage_PublishesOnePairPerPageWithPartnerAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var a = new TestPage("A");
        var b = new TestPage("B");

        Assert.IsTrue((await PushAsync(flow, a, new PageContext(), CancellationToken.None)).IsSuccess);
        flowContext.TransitionLog.Clear();

        var result = await PushAsync(flow, b, new PageContext(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);

        CollectionAssert.AreEqual(
              new[] { "Began A Hide B", "Began B Show A", "Ended A Hide B Completed", "Ended B Show A Completed" }
            , flowContext.TransitionLog
        );

        Assert.AreSame(flow, flowContext.LastTransitionFlow);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task TM3_PopLastPage_PublishesHideWithNoPartnerAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var a = new TestPage("A");

        Assert.IsTrue((await PushAsync(flow, a, new PageContext(), CancellationToken.None)).IsSuccess);
        flowContext.TransitionLog.Clear();

        var result = await ((IPageStackStrategy<TestPage>)flow).PopAsync(new PageContext(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { "Began A Hide -", "Ended A Hide - Completed" }, flowContext.TransitionLog);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator TM4_SingleStackAbortedPush_EndsBothPagesUndone()
        => UnityTask.ToCoroutine(static async () => {
            await AbortedPushEndsBothPagesUndoneAsync(multi: false);
        });

    [UnityTest]
    public IEnumerator TM4_MultiStackAbortedPush_EndsBothPagesUndone()
        => UnityTask.ToCoroutine(static async () => {
            await AbortedPushEndsBothPagesUndoneAsync(multi: true);
        });

    [Test]
    public async Task TM5_ListShowAll_PublishesOnePairPerPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var list = new PageListMulti<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");

        Assert.IsTrue((await list.AddAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.AddAsync(b, context, CancellationToken.None)).IsSuccess);

        var result = await list.ShowAllAsync(new[] { a, b }, 2, context, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);

        CollectionAssert.AreEqual(
              new[] { "Began A Show -", "Began B Show -", "Ended A Show - Completed", "Ended B Show - Completed" }
            , flowContext.TransitionLog
        );

        Assert.AreSame(list, flowContext.LastTransitionFlow);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator TM6_PopAllAbort_EndsEveryPageUndone()
        => UnityTask.ToCoroutine(static async () => {
            await PopAllAbortEndsEveryPageUndoneAsync();
        });

    private static async UnityTask AbortedPushEndsBothPagesUndoneAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var gate = new TestGate();
        var transition = new TestTransition { ShowGate = gate };
        var a = new TestPage("A");
        var b = new TestPage("B") { PageTransition = transition };

        Assert.IsTrue((await PushAsync(flow, a, new PageContext(), CancellationToken.None)).IsSuccess);
        flowContext.TransitionLog.Clear();

        var began = flowContext.BeganCount;
        var ended = flowContext.EndedCount;
        var pending = PushAsync(flow, b, new PageContext(), CancellationToken.None);
        Success<PageFlowError> result = default;

        try
        {
            await UnityTask.WaitUntil(
                  state: transition
                , predicate: static current => current.Value == 0.5f
                , timing: UnityTaskTiming.Update
                , token: CancellationToken.None
            );

            Assert.IsTrue(flow.AbortCurrent(PageAbortMode.Reverse).IsSuccess);
        }
        finally
        {
            gate.Open();
            result = await pending;
        }

        var error = result.GetFailureOrThrow();

        Assert.IsTrue(error.IsAborted);
        Assert.AreEqual(expected: PageTransitionState.Undone, actual: error.TransitionState);

        CollectionAssert.AreEqual(
              new[] { "Began A Hide B", "Began B Show A", "Ended A Hide B Undone", "Ended B Show A Undone" }
            , flowContext.TransitionLog
        );

        Assert.AreEqual(expected: 2, actual: flowContext.BeganCount - began);
        Assert.AreEqual(expected: 2, actual: flowContext.EndedCount - ended);
        LogAssert.NoUnexpectedReceived();
    }

    private static async UnityTask PopAllAbortEndsEveryPageUndoneAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var gateA = new TestGate();
        var gateB = new TestGate();
        var a = new TestPage("A");
        var b = new TestPage("B");

        Assert.IsTrue((await stack.PushAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await stack.PushAsync(b, new PageContext(), CancellationToken.None)).IsSuccess);

        a.HideGate = gateA;
        b.HideGate = gateB;
        flowContext.TransitionLog.Clear();

        var began = flowContext.BeganCount;
        var ended = flowContext.EndedCount;
        var pending = stack.PopAllAsync(new PageContext(), CancellationToken.None);
        PagePopAllResult<TestPage> pop = default;

        try
        {
            Assert.IsTrue(stack.IsInTransition);
            Assert.IsTrue(stack.AbortCurrent(PageAbortMode.Reverse).IsSuccess);
        }
        finally
        {
            gateA.Open();
            gateB.Open();
            pop = await pending;
        }

        try
        {
            Assert.IsTrue(pop.Result.GetFailureOrThrow().IsAborted);
            Assert.AreEqual(expected: 2, actual: flowContext.BeganCount - began);
            Assert.AreEqual(expected: 2, actual: flowContext.EndedCount - ended);

            var log = flowContext.TransitionLog;
            var endedEntries = 0;

            for (var i = 0; i < log.Count; i++)
            {
                if (log[i].StartsWith("Ended ", StringComparison.Ordinal) == false)
                {
                    continue;
                }

                endedEntries++;
                StringAssert.EndsWith(" Undone", log[i]);
            }

            Assert.AreEqual(expected: 2, actual: endedEntries);
            LogAssert.NoUnexpectedReceived();
        }
        finally
        {
            if (pop.Pages != null)
            {
                ArrayPool<TestPage>.Shared.Return(array: pop.Pages, clearArray: true);
            }
        }
    }
}
