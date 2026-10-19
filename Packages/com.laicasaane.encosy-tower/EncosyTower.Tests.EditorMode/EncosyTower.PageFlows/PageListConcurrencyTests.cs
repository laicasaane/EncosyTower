using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageListConcurrencyTests
{
    private const string IN_TRANSITION_A = "The page 'A' is part of the running transition.";
    private const string BEING_REMOVED_A = "The page 'A' is already being removed from the flow.";

    private static IPageFlow CreateList(bool multi, TestFlowContext context)
        => multi ? new PageListMulti<TestPage>(context) : new PageListSingle<TestPage>(context);

    private static async Task AddPagesAsync(IPageListStrategy<TestPage> list, params TestPage[] pages)
    {
        var count = pages.Length;

        for (var index = 0; index < count; index++)
        {
            Assert.IsTrue((await list.AddAsync(pages[index], new PageContext(), CancellationToken.None)).IsSuccess);
        }
    }

    private static void AssertPages(IPageFlow flow, params TestPage[] expected)
    {
        var pages = ((IHasPages<TestPage>)flow).Pages;
        Assert.AreEqual(expected: expected.Length, actual: pages.Count);
        var count = expected.Length;

        for (var index = 0; index < count; index++)
        {
            Assert.AreSame(expected[index], pages[index]);
        }
    }

    private static void AssertFailure(Success<PageFlowError> result, string message)
    {
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(expected: message, actual: result.GetFailureOrThrow().ToMessage());
    }

    private static int CountCalls(TestPage page, string call)
    {
        var count = 0;
        var calls = page.Calls;

        for (var index = 0; index < calls.Count; index++)
        {
            if (calls[index] == call)
            {
                count++;
            }
        }

        return count;
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task LC1_RemoveOfThePageBeingShown_FailsAtOnceAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };

        await AddPagesAsync(list, a);

        var show = list.ShowAsync(a, new PageContext(), CancellationToken.None);
        Success<PageFlowError> removeResult = default;
        Success<PageFlowError> showResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            removeResult = await list.RemoveAsync(a, new PageContext(), CancellationToken.None);
            Assert.IsTrue(flow.IsInTransition);
        }
        finally
        {
            gate.Open();
            showResult = await show;
        }

        AssertFailure(removeResult, IN_TRANSITION_A);
        Assert.IsTrue(showResult.IsSuccess);
        AssertPages(flow, a);
        Assert.AreEqual(expected: 0, actual: flowContext.DetachCount);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task LC2_RemoveOfThePageBeingHidden_FailsAtOnceAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListSingle<TestPage>(flowContext);
        var gate = new TestGate();
        var a = new TestPage("A");
        var b = new TestPage("B") { ShowGate = gate };

        await AddPagesAsync(flow, a, b);
        Assert.IsTrue((await flow.ShowAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);

        var show = flow.ShowAsync(b, new PageContext(), CancellationToken.None);
        Success<PageFlowError> removeResult = default;
        Success<PageFlowError> showResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            removeResult = await flow.RemoveAsync(a, new PageContext(), CancellationToken.None);
        }
        finally
        {
            gate.Open();
            showResult = await show;
        }

        AssertFailure(removeResult, IN_TRANSITION_A);
        Assert.IsTrue(showResult.IsSuccess);
        AssertPages(flow, a, b);
        AssertCurrent(flow, b);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task LC3_RemoveOfAnotherPageDuringATransition_SucceedsAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");

        await AddPagesAsync(list, a, b);

        var show = list.ShowAsync(a, new PageContext(), CancellationToken.None);
        Success<PageFlowError> removeResult = default;
        Success<PageFlowError> showResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            removeResult = await list.RemoveAsync(b, new PageContext(), CancellationToken.None);
        }
        finally
        {
            gate.Open();
            showResult = await show;
        }

        Assert.IsTrue(removeResult.IsSuccess);
        Assert.IsTrue(showResult.IsSuccess);
        AssertPages(flow, a);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task LC4_SecondRemove_FailsWithPageBeingRemovedAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { DetachGate = gate };

        await AddPagesAsync(list, a);

        var firstRemove = list.RemoveAsync(a, new PageContext(), CancellationToken.None);
        Success<PageFlowError> firstResult = default;
        Success<PageFlowError> secondResult = default;

        try
        {
            secondResult = await list.RemoveAsync(a, new PageContext(), CancellationToken.None);
        }
        finally
        {
            gate.Open();
            firstResult = await firstRemove;
        }

        AssertFailure(secondResult, BEING_REMOVED_A);
        Assert.IsTrue(firstResult.IsSuccess);
        AssertPages(flow);
        Assert.AreEqual(expected: 1, actual: CountCalls(a, "Detach"));
        Assert.AreEqual(expected: 1, actual: flowContext.DetachCount);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task LC5_RemoveByIndex_BindsToThePageAtCallTimeAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A");
        var b = new TestPage("B") { DetachGate = gate };
        var c = new TestPage("C");

        await AddPagesAsync(list, a, b, c);

        var removeByIndex = list.RemoveAsync(1, new PageContext(), CancellationToken.None);
        Success<PageFlowError> byIndexResult = default;
        Success<PageFlowError> byPageResult = default;

        try
        {
            byPageResult = await list.RemoveAsync(a, new PageContext(), CancellationToken.None);
        }
        finally
        {
            gate.Open();
            byIndexResult = await removeByIndex;
        }

        Assert.IsTrue(byPageResult.IsSuccess);
        Assert.IsTrue(byIndexResult.IsSuccess);
        AssertPages(flow, c);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task LC6_ShowByIndex_BindsToThePageAtCallTimeAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListMulti<TestPage>(flowContext);
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var c = new TestPage("C");
        var sequential = new PageContext { AsyncOperation = PageAsyncOperation.Sequential };

        await AddPagesAsync(flow, a, b, c);

        var showA = flow.ShowAsync(a, new PageContext(), CancellationToken.None);
        var showByIndex = flow.ShowAsync(2, sequential, CancellationToken.None);
        Success<PageFlowError> removeResult = default;
        Success<PageFlowError> showAResult = default;
        Success<PageFlowError> showByIndexResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            Assert.IsFalse(showByIndex.GetAwaiter().IsCompleted);
            removeResult = await flow.RemoveAsync(b, new PageContext(), CancellationToken.None);
        }
        finally
        {
            gate.Open();
            showAResult = await showA;
            showByIndexResult = await showByIndex;
        }

        Assert.IsTrue(removeResult.IsSuccess);
        Assert.IsTrue(showAResult.IsSuccess);
        Assert.IsTrue(showByIndexResult.IsSuccess);
        AssertPages(flow, a, c);
        CollectionAssert.Contains(c.Calls, "Show");
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task LC7_ShowByIndex_PageRemovedWhileWaiting_ReturnsPageNotFoundAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListMulti<TestPage>(flowContext);
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var sequential = new PageContext { AsyncOperation = PageAsyncOperation.Sequential };

        await AddPagesAsync(flow, a, b);

        var showA = flow.ShowAsync(a, new PageContext(), CancellationToken.None);
        var showByIndex = flow.ShowAsync(1, sequential, CancellationToken.None);
        Success<PageFlowError> removeResult = default;
        Success<PageFlowError> showAResult = default;
        Success<PageFlowError> showByIndexResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            Assert.IsFalse(showByIndex.GetAwaiter().IsCompleted);
            removeResult = await flow.RemoveAsync(b, new PageContext(), CancellationToken.None);
        }
        finally
        {
            gate.Open();
            showAResult = await showA;
            showByIndexResult = await showByIndex;
        }

        Assert.IsTrue(removeResult.IsSuccess);
        Assert.IsTrue(showAResult.IsSuccess);
        AssertFailure(showByIndexResult, "The page 'B' is not in the flow.");
        AssertPages(flow, a);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task LC8_AddNeverWaitsAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");

        await AddPagesAsync(list, a);

        var show = list.ShowAsync(a, new PageContext(), CancellationToken.None);
        Success<PageFlowError> addResult = default;
        Success<PageFlowError> showResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            addResult = await list.AddAsync(b, new PageContext(), CancellationToken.None);
            Assert.IsTrue(flow.IsInTransition);
        }
        finally
        {
            gate.Open();
            showResult = await show;
        }

        Assert.IsTrue(addResult.IsSuccess);
        Assert.IsTrue(showResult.IsSuccess);
        AssertPages(flow, a, b);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task LC9_HideActive_PageNoLongerCurrent_ReturnsPageNotActiveAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListSingle<TestPage>(flowContext);
        var gate = new TestGate();
        var a = new TestPage("A");
        var b = new TestPage("B") { ShowGate = gate };
        var hideContext = new PageContext { AsyncOperation = PageAsyncOperation.Sequential, DefaultIndex = -1 };

        await AddPagesAsync(flow, a, b);
        Assert.IsTrue((await flow.ShowAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);

        var show = flow.ShowAsync(b, new PageContext(), CancellationToken.None);
        var hide = flow.HideAsync(hideContext, CancellationToken.None);
        Success<PageFlowError> showResult = default;
        Success<PageFlowError> hideResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            Assert.IsFalse(hide.GetAwaiter().IsCompleted);
        }
        finally
        {
            gate.Open();
            showResult = await show;
            hideResult = await hide;
        }

        Assert.IsTrue(showResult.IsSuccess);
        AssertFailure(hideResult, "The page 'A' is not an active page of this flow.");
        AssertCurrent(flow, b);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task LC10_ShowOfAPageBeingRemoved_FailsAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { DetachGate = gate };

        await AddPagesAsync(list, a);

        var remove = list.RemoveAsync(a, new PageContext(), CancellationToken.None);
        Success<PageFlowError> showResult = default;
        Success<PageFlowError> removeResult = default;

        try
        {
            showResult = await list.ShowAsync(a, new PageContext(), CancellationToken.None);
        }
        finally
        {
            gate.Open();
            removeResult = await remove;
        }

        AssertFailure(showResult, BEING_REMOVED_A);
        Assert.IsTrue(removeResult.IsSuccess);
        AssertPages(flow);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task LC11_RemoveAll_LeavesAPageBeingRemovedToItsCallAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { DetachGate = gate };
        var b = new TestPage("B");

        await AddPagesAsync(list, a, b);

        var remove = list.RemoveAsync(a, new PageContext(), CancellationToken.None);
        Success<PageFlowError> removeAllResult = default;
        Success<PageFlowError> removeResult = default;

        try
        {
            removeAllResult = await list.RemoveAllAsync(new PageContext(), CancellationToken.None);
            AssertPages(flow, a);
        }
        finally
        {
            gate.Open();
            removeResult = await remove;
        }

        Assert.IsTrue(removeAllResult.IsSuccess);
        Assert.IsTrue(removeResult.IsSuccess);
        AssertPages(flow);
        Assert.AreEqual(expected: 1, actual: CountCalls(a, "Detach"));
        Assert.AreEqual(expected: 1, actual: CountCalls(b, "Detach"));
        LogAssert.NoUnexpectedReceived();
    }

    private static void AssertCurrent(PageListSingle<TestPage> flow, TestPage expected)
    {
        Assert.IsTrue(flow.CurrentPage.TryGetValue(out var current));
        Assert.AreSame(expected, current);
    }
}
