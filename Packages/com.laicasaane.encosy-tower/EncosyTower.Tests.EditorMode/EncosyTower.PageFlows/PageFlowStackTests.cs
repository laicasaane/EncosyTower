using System;
using System.Collections;
using System.Text.RegularExpressions;
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

public sealed class PageFlowStackTests
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

    private static void AssertPages(IPageFlow flow, TestPage[] expected)
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
            CollectionAssert.AreEqual(expected, multi.PageCollection);
        }
    }

    private static void AssertFailure(Success<PageFlowError> result, string message, PageTransitionState state)
    {
        Assert.IsTrue(result.IsFailure);
        var error = result.GetFailureOrThrow();
        Assert.AreEqual(expected: message, actual: error.ToMessage());
        Assert.AreEqual(expected: state, actual: error.TransitionState);
    }

    private static async UnityTask PushWhileGatedAsync(bool multi, PageAsyncOperation operation)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var context = new PageContext { AsyncOperation = operation };
        var first = PushAsync(flow, a, context, CancellationToken.None);
        var second = default(UnityTask<Success<PageFlowError>>);
        Success<PageFlowError> firstResult = default;
        Success<PageFlowError> secondResult = default;
        InvalidOperationException thrown = null;
        var queued = false;

        try
        {
            Assert.IsFalse(gate.IsOpen);
            Assert.IsTrue(flow.IsInTransition);
            CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow" }, a.Calls);

            if (operation == PageAsyncOperation.Sequential)
            {
                second = PushAsync(flow, b, context, CancellationToken.None);
                queued = true;
                Assert.IsFalse(second.GetAwaiter().IsCompleted);
                Assert.IsEmpty(b.Calls);
                Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
            }
            else if (operation == PageAsyncOperation.Exception)
            {
                try
                {
                    await PushAsync(flow, b, context, CancellationToken.None);
                }
                catch (InvalidOperationException ex)
                {
                    thrown = ex;
                }

                Assert.IsNotNull(thrown);
                Assert.AreEqual(expected: $"{flow.GetType()} is currently in transition.", actual: thrown.Message);
                Assert.IsEmpty(b.Calls);
            }
            else
            {
                secondResult = await PushAsync(flow, b, context, CancellationToken.None);

                AssertFailure(
                      secondResult
                    , $"{flow.GetType()} is currently in transition."
                    , PageTransitionState.NotStarted
                );

                Assert.IsEmpty(b.Calls);
            }
        }
        finally
        {
            gate.Open();
            firstResult = await first;

            if (queued)
            {
                secondResult = await second;
            }
        }

        Assert.IsTrue(firstResult.IsSuccess);

        if (queued)
        {
            var expectedCalls = multi
                ? new[] { "Attach", "BeforeShow", "Show", "AfterShow", "Hide", "AfterHide" }
                : new[] { "Attach", "BeforeShow", "Show", "AfterShow", "Hide", "AfterHide", "Detach" };

            CollectionAssert.AreEqual(expectedCalls, a.Calls);
            Assert.IsTrue(secondResult.IsSuccess);
            AssertPages(flow, multi ? new[] { b, a } : new[] { b });
            CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, b.Calls);
            Assert.AreEqual(expected: 2, actual: flowContext.AttachCount);
            Assert.AreEqual(expected: 3, actual: flowContext.BeganCount);
        }
        else
        {
            CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, a.Calls);
            AssertPages(flow, new[] { a });
            Assert.IsFalse(b.HasFlowScope);
            Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
            Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        }

        Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Backend_ActivatesPageRenderingAfterPreparationAsync(bool multi, bool noTransition)
    {
        using var flowContext = new TestFlowContext();
        var backend = new TestFlowBackend();
        IPageFlow flow = multi
            ? new PageStackMulti<TestPage>(flowContext, backend)
            : new PageStackSingle<TestPage>(flowContext, backend);
        using var lifetime = (IDisposable)flow;
        var page = new TestPage("Page");
        var context = new PageContext {
            ShowOptions = noTransition ? PageTransitionOptions.NoTransition : PageTransitionOptions.Default,
        };

        var result = await PushAsync(flow, page, context, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { page }, backend.ActivatedPages);

        var expectedCalls = noTransition
            ? new[] { "Attach", "BeforeShow", "ActivatePageRendering", "AfterShow" }
            : new[] { "Attach", "BeforeShow", "ActivatePageRendering", "Show", "AfterShow" };

        CollectionAssert.AreEqual(expectedCalls, page.Calls);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task S1_PushTwoPagesRunsHooksAndBalancesMessagesAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await PushAsync(flow, b, context, CancellationToken.None)).IsSuccess);
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, b.Calls);
        Assert.AreEqual(expected: 2, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 3, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 3, actual: flowContext.EndedCount);
        Assert.AreEqual(expected: multi ? 0 : 1, actual: flowContext.DetachCount);
        Assert.AreEqual(expected: multi, actual: a.HasFlowScope);
        Assert.IsTrue(b.HasFlowScope);
        AssertPages(flow, multi ? new[] { b, a } : new[] { b });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task S2_PopEmptyStackReturnsNoPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();

        var result = await ((IPageStackStrategy<TestPage>)flow).PopAsync(context, CancellationToken.None);

        AssertFailure(result, "The flow has no page for this operation.", PageTransitionState.NotStarted);
        AssertPages(flow, Array.Empty<TestPage>());
        Assert.AreEqual(expected: 0, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 0, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task S7_CancelledPushPreservesStackAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");
        using var cancellation = new CancellationTokenSource();

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);
        var aCalls = a.Calls.ToArray();
        cancellation.Cancel();

        var result = await PushAsync(flow, b, context, cancellation.Token);

        AssertFailure(result, "The operation was cancelled before the flow changed.", PageTransitionState.NotStarted);
        Assert.IsTrue(result.GetFailureOrThrow().IsCancelled);
        AssertPages(flow, new[] { a });
        Assert.IsTrue(a.HasFlowScope);
        Assert.IsFalse(b.HasFlowScope);
        CollectionAssert.AreEqual(aCalls, a.Calls);
        Assert.IsEmpty(b.Calls);
        Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 0, actual: flowContext.DetachCount);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 1, actual: flowContext.EndedCount);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task S8_AttachRejectRollsBackPushAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B") { AttachBehaviour = PageHookBehaviour.Reject };

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);
        var aCalls = a.Calls.ToArray();
        var detachCount = flowContext.DetachCount;

        var result = await PushAsync(flow, b, context, CancellationToken.None);

        AssertFailure(result, "The page 'B' rejected attaching to the flow.", PageTransitionState.NotStarted);
        AssertPages(flow, new[] { a });
        Assert.IsTrue(a.HasFlowScope);
        Assert.IsFalse(b.HasFlowScope);
        CollectionAssert.AreEqual(aCalls, a.Calls);
        CollectionAssert.AreEqual(new[] { "Attach" }, b.Calls);
        Assert.AreEqual(expected: 2, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: detachCount + 1, actual: flowContext.DetachCount);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 1, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task S9_AttachThrowRollsBackPushAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B") { AttachBehaviour = PageHookBehaviour.Throw };

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);
        var aCalls = a.Calls.ToArray();
        var detachCount = flowContext.DetachCount;

        var result = await PushAsync(flow, b, context, CancellationToken.None);

        AssertFailure(result, "An exception occurred in the page flow: B.Attach", PageTransitionState.NotStarted);
        AssertPages(flow, new[] { a });
        Assert.IsTrue(a.HasFlowScope);
        Assert.IsFalse(b.HasFlowScope);
        CollectionAssert.AreEqual(aCalls, a.Calls);
        CollectionAssert.AreEqual(new[] { "Attach" }, b.Calls);
        Assert.AreEqual(expected: 2, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: detachCount + 1, actual: flowContext.DetachCount);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 1, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task C1_ShowExceptionFinishesPushAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B") { ShowBehaviour = PageHookBehaviour.Throw };

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);

        var result = await PushAsync(flow, b, context, CancellationToken.None);

        AssertFailure(result, "An exception occurred in the page flow: B.Show", PageTransitionState.Completed);
        Assert.IsTrue(result.IsApplied());
        AssertPages(flow, multi ? new[] { b, a } : new[] { b });
        Assert.AreEqual(expected: "AfterShow", actual: b.Calls[^1]);
        Assert.AreEqual(expected: 3, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
        Assert.IsTrue(b.HasFlowScope);
        Assert.AreEqual(expected: multi, actual: a.HasFlowScope);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task C2_AfterShowExceptionFinishesPushAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B") { AfterShowBehaviour = PageHookBehaviour.Throw };

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);

        var result = await PushAsync(flow, b, context, CancellationToken.None);

        AssertFailure(result, "An exception occurred in the page flow: B.AfterShow", PageTransitionState.Completed);
        Assert.IsTrue(result.IsApplied());
        AssertPages(flow, multi ? new[] { b, a } : new[] { b });
        Assert.AreEqual(expected: "AfterShow", actual: b.Calls[^1]);
        Assert.AreEqual(expected: 3, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
        Assert.IsTrue(b.HasFlowScope);
        Assert.AreEqual(expected: multi, actual: a.HasFlowScope);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task C3_DetachRejectedStillFinishesPopAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await PushAsync(flow, b, context, CancellationToken.None)).IsSuccess);
        var detachCount = flowContext.DetachCount;
        b.DetachBehaviour = PageHookBehaviour.Reject;

        var result = await ((IPageStackStrategy<TestPage>)flow).PopAsync(context, CancellationToken.None);

        AssertFailure(result, "The page 'B' rejected detaching from the flow.", PageTransitionState.Completed);
        Assert.IsTrue(result.IsApplied());
        AssertPages(flow, multi ? new[] { a } : Array.Empty<TestPage>());
        Assert.IsFalse(b.HasFlowScope);
        Assert.AreEqual(expected: multi, actual: a.HasFlowScope);
        Assert.AreEqual(expected: "Detach", actual: b.Calls[^1]);
        Assert.AreEqual(expected: detachCount + 1, actual: flowContext.DetachCount);
        Assert.AreEqual(expected: multi ? 5 : 4, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task C4_CancellationInBeforeShowStillFinishesPushAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");
        using var cancellation = new CancellationTokenSource();
        b.CancelOn("BeforeShow", cancellation);

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);

        var result = await PushAsync(flow, b, context, cancellation.Token);

        Assert.IsTrue(cancellation.IsCancellationRequested);
        Assert.IsTrue(result.IsSuccess);
        AssertPages(flow, multi ? new[] { b, a } : new[] { b });
        Assert.IsTrue(b.HasFlowScope);
        Assert.AreEqual(expected: multi, actual: a.HasFlowScope);
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, b.Calls);
        Assert.AreEqual(expected: 3, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task C5_LaterFailuresBecomeSuppressedErrorsAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");

        var b = new TestPage("B") {
            ShowBehaviour = PageHookBehaviour.Throw,
            AfterShowBehaviour = PageHookBehaviour.Throw,
        };

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);

        var result = await PushAsync(flow, b, context, CancellationToken.None);

        AssertFailure(result, "An exception occurred in the page flow: B.Show", PageTransitionState.Completed);
        var error = result.GetFailureOrThrow();
        Assert.AreEqual(expected: 1, actual: error.SuppressedErrors.Length);

        Assert.AreEqual(
              expected: "An exception occurred in the page flow: B.AfterShow"
            , actual: error.SuppressedErrors[0].ToMessage()
        );

        Assert.IsTrue(result.IsApplied());
        AssertPages(flow, multi ? new[] { b, a } : new[] { b });
        Assert.AreEqual(expected: "AfterShow", actual: b.Calls[^1]);
        Assert.AreEqual(expected: 3, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
        Assert.IsTrue(b.HasFlowScope);
        Assert.AreEqual(expected: multi, actual: a.HasFlowScope);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task C6_FailureAfterACancellationIsLoggedAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var a = new TestPage("A");

        var b = new TestPage("B") {
            BeforeShowBehaviour = PageHookBehaviour.ThrowCancelled,
            AfterShowBehaviour = PageHookBehaviour.Throw,
        };

        OperationCanceledException thrown = null;

        Assert.IsTrue((await PushAsync(flow, a, context, CancellationToken.None)).IsSuccess);

        try
        {
            await PushAsync(flow, b, context, CancellationToken.None);
        }
        catch (OperationCanceledException ex)
        {
            thrown = ex;
        }

        Assert.IsNotNull(thrown);
        LogAssert.Expect(LogType.Exception, new Regex(@"B\.AfterShow"));
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task C7_FailureAfterACancellationWithACallerSourceIsLoggedOnceAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var a = new TestPage("A");

        var b = new TestPage("B") {
            BeforeShowBehaviour = PageHookBehaviour.ThrowCancelled,
            AfterShowBehaviour = PageHookBehaviour.Throw,
        };

        Assert.IsTrue((await PushAsync(flow, a, new PageContext(), CancellationToken.None)).IsSuccess);

        var context = new PageContext { Abort = new PageAbortSource().Handle };
        OperationCanceledException thrown = null;
        LogAssert.Expect(LogType.Exception, new Regex(@"B\.AfterShow"));

        try
        {
            await PushAsync(flow, b, context, CancellationToken.None);
        }
        catch (OperationCanceledException ex)
        {
            thrown = ex;
        }

        Assert.IsNotNull(thrown);
        AssertPages(flow, multi ? new[] { b, a } : new[] { b });
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, b.Calls);
        Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator S3_SingleStackDropWhileShowIsGated()
        => UnityTask.ToCoroutine(static async () => {
            await PushWhileGatedAsync(multi: false, operation: PageAsyncOperation.Drop);
        });

    [UnityTest]
    public IEnumerator S3_MultiStackDropWhileShowIsGated()
        => UnityTask.ToCoroutine(static async () => {
            await PushWhileGatedAsync(multi: true, operation: PageAsyncOperation.Drop);
        });

    [UnityTest]
    public IEnumerator S5_SingleStackExceptionWhileShowIsGated()
        => UnityTask.ToCoroutine(static async () => {
            await PushWhileGatedAsync(multi: false, operation: PageAsyncOperation.Exception);
        });

    [UnityTest]
    public IEnumerator S5_MultiStackExceptionWhileShowIsGated()
        => UnityTask.ToCoroutine(static async () => {
            await PushWhileGatedAsync(multi: true, operation: PageAsyncOperation.Exception);
        });

    [UnityTest]
    public IEnumerator S6_SingleStackSequentialWhileShowIsGated()
        => UnityTask.ToCoroutine(static async () => {
            await PushWhileGatedAsync(multi: false, operation: PageAsyncOperation.Sequential);
        });

    [UnityTest]
    public IEnumerator S6_MultiStackSequentialWhileShowIsGated()
        => UnityTask.ToCoroutine(static async () => {
            await PushWhileGatedAsync(multi: true, operation: PageAsyncOperation.Sequential);
        });
}
