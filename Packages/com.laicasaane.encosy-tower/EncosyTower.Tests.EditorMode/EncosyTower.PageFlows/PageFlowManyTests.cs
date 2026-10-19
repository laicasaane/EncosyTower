using System;
using System.Buffers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageFlowManyTests
{
    private const string PREFIX = "An exception occurred in the page flow: ";

    private static readonly string[] s_shownCalls = { "Attach", "BeforeShow", "Show", "AfterShow" };

    private static readonly string[] s_stackedCalls = {
        "Attach", "BeforeShow", "Show", "AfterShow", "Hide", "AfterHide",
    };

    private static async UnityTask PushAsync(PageStackMulti<TestPage> stack, params TestPage[] pages)
    {
        for (var i = 0; i < pages.Length; i++)
        {
            var result = await stack.PushAsync(pages[i], new PageContext(), CancellationToken.None);
            Assert.IsTrue(result.IsSuccess);
        }
    }

    private static void ReturnPages(PagePopAllResult<TestPage> pop)
    {
        if (pop.Pages != null)
        {
            ArrayPool<TestPage>.Shared.Return(array: pop.Pages, clearArray: true);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Backend_ShowAllActivatesEachPageBeforeItsShowCallbackAsync(bool noTransition)
    {
        using var flowContext = new TestFlowContext();
        var backend = new TestFlowBackend();
        using var flow = new PageListMulti<TestPage>(flowContext, backend);
        var first = new TestPage("First");
        var second = new TestPage("Second");
        var context = new PageContext {
            ShowOptions = noTransition ? PageTransitionOptions.NoTransition : PageTransitionOptions.Default,
        };

        Assert.IsTrue((await flow.AddAsync(first, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await flow.AddAsync(second, context, CancellationToken.None)).IsSuccess);
        Assert.IsEmpty(backend.ActivatedPages);

        var pages = new[] { first, second };
        var result = await flow.ShowAllAsync(pages, pages.Length, context, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { first, second }, backend.ActivatedPages);

        var expectedCalls = noTransition
            ? new[] { "Attach", "BeforeShow", "ActivatePageRendering", "AfterShow" }
            : new[] { "Attach", "BeforeShow", "ActivatePageRendering", "Show", "AfterShow" };

        CollectionAssert.AreEqual(expectedCalls, first.Calls);
        CollectionAssert.AreEqual(expectedCalls, second.Calls);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task M1_PopAllStartsEveryHideBeforeAnyEndsAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var gateA = new TestGate();
        var gateB = new TestGate();
        var a = new TestPage("A");
        var b = new TestPage("B");

        await PushAsync(stack, a, b);

        a.HideGate = gateA;
        b.HideGate = gateB;

        var begin = flowContext.BeganCount;
        var end = flowContext.EndedCount;
        var pending = stack.PopAllAsync(new PageContext(), CancellationToken.None);
        PagePopAllResult<TestPage> pop = default;

        try
        {
            Assert.IsTrue(stack.IsInTransition);
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            CollectionAssert.AreEqual(Concat(s_stackedCalls, "Hide"), a.Calls);
            CollectionAssert.AreEqual(Concat(s_shownCalls, "Hide"), b.Calls);
            Assert.AreEqual(expected: begin + 2, actual: flowContext.BeganCount);
            Assert.AreEqual(expected: end, actual: flowContext.EndedCount);
        }
        finally
        {
            gateA.Open();
            gateB.Open();
            pop = await pending;
        }

        try
        {
            Assert.IsTrue(pop.Result.IsSuccess);
            Assert.IsNull(pop.Thrown);
            Assert.AreEqual(expected: 2, actual: pop.Count);
            Assert.AreSame(b, pop.Pages[0]);
            Assert.AreSame(a, pop.Pages[1]);
            Assert.IsFalse(stack.IsInTransition);
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            CollectionAssert.AreEqual(Concat(s_stackedCalls, "Hide", "AfterHide", "Detach"), a.Calls);
            CollectionAssert.AreEqual(Concat(s_shownCalls, "Hide", "AfterHide", "Detach"), b.Calls);
            Assert.AreEqual(expected: 2, actual: flowContext.DetachCount);
            Assert.AreEqual(expected: end + 2, actual: flowContext.EndedCount);
            Assert.IsFalse(a.HasFlowScope);
            Assert.IsFalse(b.HasFlowScope);
            LogAssert.NoUnexpectedReceived();
        }
        finally
        {
            ReturnPages(pop);
        }
    }

    [Test]
    public async Task M2_PopAllOnEmptyStackReturnsNoPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);

        var pop = await stack.PopAllAsync(new PageContext(), CancellationToken.None);

        AssertNoPage(pop.Result);
        Assert.IsNull(pop.Pages);
        Assert.AreEqual(expected: 0, actual: pop.Count);
        Assert.AreEqual(expected: 0, actual: flowContext.BeganCount);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task M3_ListTransitionOfNoPageReturnsNoPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var list = new PageListMulti<TestPage>(flowContext);
        var pages = new TestPage[1];

        AssertNoPage(await list.HideAllAsync(pages, 0, new PageContext(), CancellationToken.None));
        AssertNoPage(await list.ShowAllAsync(pages, 0, new PageContext(), CancellationToken.None));
        Assert.AreEqual(expected: 0, actual: flowContext.BeganCount);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task M4_AbortCurrentAbortsEveryPageAndRestoresStackAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var gateA = new TestGate();
        var gateB = new TestGate();
        var a = new TestPage("A");
        var b = new TestPage("B");

        await PushAsync(stack, a, b);

        a.HideGate = gateA;
        b.HideGate = gateB;

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
            Assert.IsTrue(pop.Result.IsFailure);
            var error = pop.Result.GetFailureOrThrow();
            Assert.IsTrue(error.IsAborted);
            Assert.AreEqual(expected: PageTransitionState.Undone, actual: error.TransitionState);
            Assert.IsFalse(pop.Result.IsApplied());
            Assert.IsNull(pop.Pages);
            Assert.IsFalse(stack.IsInTransition);
            CollectionAssert.AreEqual(new[] { b, a }, stack.PageCollection);

            CollectionAssert.AreEqual(
                  Concat(s_stackedCalls, "Hide", "BeforeShow", "Show", "AfterShow")
                , a.Calls
            );

            CollectionAssert.AreEqual(Concat(s_shownCalls, "Hide", "BeforeShow", "Show", "AfterShow"), b.Calls);
            Assert.AreEqual(expected: 0, actual: flowContext.DetachCount);
            Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
            Assert.IsTrue(a.HasFlowScope);
            Assert.IsTrue(b.HasFlowScope);
            LogAssert.NoUnexpectedReceived();
        }
        finally
        {
            ReturnPages(pop);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task M5_FailuresMergeAsSuppressedErrorsAsync(bool inHideCallback)
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var a = new TestPage("A");
        var b = new TestPage("B");
        var behaviour = PageHookBehaviour.Throw;
        var hook = inHideCallback ? "Hide" : "AfterHide";

        await PushAsync(stack, a, b);

        if (inHideCallback)
        {
            a.HideBehaviour = behaviour;
            b.HideBehaviour = behaviour;
        }
        else
        {
            a.AfterHideBehaviour = behaviour;
            b.AfterHideBehaviour = behaviour;
        }

        var pop = await stack.PopAllAsync(new PageContext(), CancellationToken.None);

        try
        {
            Assert.IsTrue(pop.Result.IsFailure);
            Assert.IsNull(pop.Thrown);

            var error = pop.Result.GetFailureOrThrow();
            Assert.AreEqual(expected: $"{PREFIX}B.{hook}", actual: error.ToMessage());
            Assert.AreEqual(expected: PageTransitionState.Completed, actual: error.TransitionState);
            Assert.AreEqual(expected: 1, actual: error.SuppressedErrors.Length);
            Assert.AreEqual(expected: $"{PREFIX}A.{hook}", actual: error.SuppressedErrors[0].ToMessage());
            Assert.IsTrue(pop.Result.IsApplied());
            Assert.AreEqual(expected: 2, actual: pop.Count);
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            Assert.AreEqual(expected: 2, actual: flowContext.DetachCount);
            Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
            LogAssert.NoUnexpectedReceived();
        }
        finally
        {
            ReturnPages(pop);
        }
    }

    [Test]
    public async Task M10_PopAllReportsADetachRejectionAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var a = new TestPage("A");
        var b = new TestPage("B");

        await PushAsync(stack, a, b);

        b.DetachBehaviour = PageHookBehaviour.Reject;

        var pop = await stack.PopAllAsync(new PageContext(), CancellationToken.None);

        try
        {
            Assert.IsTrue(pop.Result.IsFailure);
            Assert.IsNull(pop.Thrown);

            var error = pop.Result.GetFailureOrThrow();
            Assert.AreEqual(expected: "The page 'B' rejected detaching from the flow.", actual: error.ToMessage());
            Assert.AreEqual(expected: PageTransitionState.Completed, actual: error.TransitionState);
            Assert.IsTrue(pop.Result.IsApplied());
            Assert.AreEqual(expected: 2, actual: pop.Count);
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            Assert.AreEqual(expected: 2, actual: flowContext.DetachCount);
            LogAssert.NoUnexpectedReceived();
        }
        finally
        {
            ReturnPages(pop);
        }
    }

    [Test]
    public async Task M6_CancelledCallDetachesEveryPageAndRethrowsAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        using var source = new CancellationTokenSource();
        var gateA = new TestGate();
        var gateB = new TestGate();
        var a = new TestPage("A");
        var b = new TestPage("B");

        await PushAsync(stack, a, b);

        a.HideGate = gateA;
        b.HideGate = gateB;

        var pending = stack.PopAllAsync(new PageContext(), source.Token);
        PagePopAllResult<TestPage> pop = default;

        try
        {
            Assert.IsTrue(stack.IsInTransition);
            source.Cancel();
        }
        finally
        {
            gateA.Open();
            gateB.Open();
            pop = await pending;
        }

        try
        {
            Assert.IsNotNull(pop.Thrown);
            Assert.IsInstanceOf<OperationCanceledException>(pop.Thrown.SourceException);
            Assert.AreEqual(expected: 2, actual: pop.Count);
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            Assert.AreEqual(expected: 2, actual: flowContext.DetachCount);
            Assert.IsFalse(stack.IsInTransition);
        }
        finally
        {
            ReturnPages(pop);
        }
    }

    [Test]
    public async Task M11_CancelledPopAllLogsTheFailureItWonOverOnceAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        using var source = new CancellationTokenSource();
        var a = new TestPage("A");
        var b = new TestPage("B");
        var gate = new TestGate();

        await PushAsync(stack, a, b);

        a.HideBehaviour = PageHookBehaviour.Throw;
        b.HideGate = gate;

        var context = new PageContext { Abort = new PageAbortSource().Handle };
        var pending = stack.PopAllAsync(context, source.Token);
        PagePopAllResult<TestPage> pop = default;
        LogAssert.Expect(LogType.Exception, new Regex(@"A\.Hide"));

        try
        {
            try
            {
                Assert.IsTrue(stack.IsInTransition);
                Assert.AreEqual(expected: "Hide", actual: b.Calls[^1]);
                CollectionAssert.Contains(a.Calls, "Hide");
                source.Cancel();
            }
            finally
            {
                gate.Open();
                pop = await pending;
            }

            Assert.IsNotNull(pop.Thrown);
            Assert.IsInstanceOf<OperationCanceledException>(pop.Thrown.SourceException);
            Assert.AreEqual(expected: 2, actual: pop.Count);
            Assert.AreSame(b, pop.Pages[0]);
            Assert.AreSame(a, pop.Pages[1]);
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            Assert.AreEqual(expected: 2, actual: flowContext.DetachCount);
            Assert.IsFalse(stack.IsInTransition);
            Assert.IsFalse(a.HasFlowScope);
            Assert.IsFalse(b.HasFlowScope);
            Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
            LogAssert.NoUnexpectedReceived();
        }
        finally
        {
            ReturnPages(pop);
        }
    }

    [Test]
    public async Task M9_DetachCancelledDuringCancelledCallStillReturnsEveryPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        using var source = new CancellationTokenSource();
        var gateA = new TestGate();
        var gateB = new TestGate();
        var a = new TestPage("A");
        var b = new TestPage("B");

        await PushAsync(stack, a, b);

        a.HideGate = gateA;
        b.HideGate = gateB;
        a.DetachBehaviour = PageHookBehaviour.ThrowCancelled;
        b.DetachBehaviour = PageHookBehaviour.ThrowCancelled;

        var pending = stack.PopAllAsync(new PageContext(), source.Token);
        PagePopAllResult<TestPage> pop = default;

        try
        {
            source.Cancel();
        }
        finally
        {
            gateA.Open();
            gateB.Open();
            pop = await pending;
        }

        try
        {
            Assert.IsNotNull(pop.Thrown);
            Assert.IsInstanceOf<OperationCanceledException>(pop.Thrown.SourceException);
            Assert.AreEqual(expected: 2, actual: pop.Count);
            Assert.AreSame(b, pop.Pages[0]);
            Assert.AreSame(a, pop.Pages[1]);
            Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
            Assert.IsFalse(stack.IsInTransition);
        }
        finally
        {
            ReturnPages(pop);
        }
    }

    [Test]
    public async Task M7_ListHidesShownPagesThenShowsAllPagesInParallelAsync()
    {
        using var flowContext = new TestFlowContext();
        using var list = new PageListMulti<TestPage>(flowContext);
        var t0 = new TestTransition();
        var t1 = new TestTransition();
        var t2 = new TestTransition();
        var p0 = new TestPage("P0") { PageTransition = t0 };
        var p1 = new TestPage("P1") { PageTransition = t1 };
        var p2 = new TestPage("P2") { PageTransition = t2 };
        var context = new PageContext();

        Assert.IsTrue((await list.AddAsync(p0, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.AddAsync(p1, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.AddAsync(p2, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.ShowAsync(p0, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.ShowAsync(p1, context, CancellationToken.None)).IsSuccess);

        var hideGate0 = new TestGate();
        var hideGate1 = new TestGate();
        t0.HideGate = hideGate0;
        t1.HideGate = hideGate1;

        var shown = new[] { p0, p1 };
        var hiding = list.HideAllAsync(shown, 2, context, CancellationToken.None);
        Success<PageFlowError> hidden;

        try
        {
            Assert.IsTrue(list.IsInTransition);
            CollectionAssert.AreEqual(new[] { "BeforeShow", "Show", "AfterShow", "BeforeHide", "Hide" }, t0.Calls);
            CollectionAssert.AreEqual(new[] { "BeforeShow", "Show", "AfterShow", "BeforeHide", "Hide" }, t1.Calls);
        }
        finally
        {
            hideGate0.Open();
            hideGate1.Open();
            hidden = await hiding;
        }

        Assert.IsTrue(hidden.IsSuccess);
        Assert.IsFalse(list.IsInTransition);
        Assert.AreEqual(expected: "AfterHide", actual: t0.Calls[^1]);
        Assert.AreEqual(expected: "AfterHide", actual: t1.Calls[^1]);

        var showGate0 = new TestGate();
        var showGate1 = new TestGate();
        var showGate2 = new TestGate();
        t0.ShowGate = showGate0;
        t1.ShowGate = showGate1;
        t2.ShowGate = showGate2;

        var every = new[] { p0, p1, p2 };
        var showing = list.ShowAllAsync(every, 3, context, CancellationToken.None);
        Success<PageFlowError> shownResult;

        try
        {
            Assert.IsTrue(list.IsInTransition);
            Assert.AreEqual(expected: "Show", actual: t0.Calls[^1]);
            Assert.AreEqual(expected: "Show", actual: t1.Calls[^1]);
            CollectionAssert.AreEqual(new[] { "BeforeShow", "Show" }, t2.Calls);
        }
        finally
        {
            showGate0.Open();
            showGate1.Open();
            showGate2.Open();
            shownResult = await showing;
        }

        Assert.IsTrue(shownResult.IsSuccess);
        Assert.AreEqual(expected: 3, actual: list.Pages.Count);
        Assert.AreEqual(expected: 0, actual: flowContext.DetachCount);
        CollectionAssert.AreEqual(new[] { "BeforeShow", "Show", "AfterShow" }, t2.Calls);
        Assert.AreEqual(expected: "AfterShow", actual: p2.Calls[^1]);
        Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task M8_ListFailsWhenSelectedPageIsNotListedAsync()
    {
        using var flowContext = new TestFlowContext();
        using var list = new PageListMulti<TestPage>(flowContext);
        var listed = new TestPage("Listed");
        var stranger = new TestPage("Stranger");

        Assert.IsTrue((await list.AddAsync(listed, new PageContext(), CancellationToken.None)).IsSuccess);

        var result = await list.ShowAllAsync(new[] { listed, stranger }, 2, new PageContext(), CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: result.GetFailureOrThrow().TransitionState);
        Assert.IsFalse(list.IsInTransition);
        CollectionAssert.AreEqual(new[] { "Attach" }, listed.Calls);
        Assert.AreEqual(expected: 0, actual: flowContext.BeganCount);
    }

    private static string[] Concat(string[] first, params string[] second)
    {
        var result = new string[first.Length + second.Length];

        Array.Copy(first, result, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);

        return result;
    }

    private static void AssertNoPage(Success<PageFlowError> result)
    {
        Assert.IsTrue(result.IsFailure);

        var error = result.GetFailureOrThrow();

        Assert.AreEqual(expected: "The flow has no page for this operation.", actual: error.ToMessage());
        Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);
    }
}
