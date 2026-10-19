using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageFlowListTests
{
    private static IPageFlow CreateList(bool multi, TestFlowContext context)
        => multi ? new PageListMulti<TestPage>(context) : new PageListSingle<TestPage>(context);

    private static async Task AddPagesAsync(IPageListStrategy<TestPage> flow, TestPage[] pages, PageContext context)
    {
        var count = pages.Length;

        for (var index = 0; index < count; index++)
        {
            Assert.IsTrue((await flow.AddAsync(pages[index], context, CancellationToken.None)).IsSuccess);
        }
    }

    private static void AssertList(IPageFlow flow, TestPage[] expected, TestPage current)
    {
        Assert.IsFalse(flow.IsInTransition);
        var pages = ((IHasPages<TestPage>)flow).Pages;
        Assert.AreEqual(expected: expected.Length, actual: pages.Count);
        var count = expected.Length;

        for (var index = 0; index < count; index++)
        {
            Assert.AreSame(expected[index], pages[index]);
            Assert.IsTrue(pages[index].HasFlowScope);
        }

        if (flow is PageListSingle<TestPage> single)
        {
            Assert.AreEqual(expected: current != null, actual: single.CurrentPage.HasValue);

            if (current != null)
            {
                Assert.AreSame(current, single.CurrentPage.GetValueOrThrow());
            }
        }
    }

    private static void AssertFailure(
          Success<PageFlowError> result
        , string message
        , PageTransitionState state = PageTransitionState.NotStarted
    )
    {
        Assert.IsTrue(result.IsFailure);
        var error = result.GetFailureOrThrow();
        Assert.AreEqual(expected: message, actual: error.ToMessage());
        Assert.AreEqual(expected: state, actual: error.TransitionState);
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
            ? new PageListMulti<TestPage>(flowContext, backend)
            : new PageListSingle<TestPage>(flowContext, backend);
        using var lifetime = (IDisposable)flow;
        var page = new TestPage("Page");
        var context = new PageContext {
            ShowOptions = noTransition ? PageTransitionOptions.NoTransition : PageTransitionOptions.Default,
        };

        var strategy = (IPageListStrategy<TestPage>)flow;
        Assert.IsTrue((await strategy.AddAsync(page, context, CancellationToken.None)).IsSuccess);
        Assert.IsEmpty(backend.ActivatedPages);

        var result = await strategy.ShowAsync(index: 0, context: context, token: CancellationToken.None);

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
    public async Task L1_ShowIndexSelectsSecondPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");
        var c = new TestPage("C");
        var pages = new[] { a, b, c };

        await AddPagesAsync(strategy, pages, context);

        var result = await strategy.ShowAsync(index: 1, context: context, token: CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        AssertList(flow, pages, b);
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, b.Calls);
        CollectionAssert.AreEqual(new[] { "Attach" }, a.Calls);
        CollectionAssert.AreEqual(new[] { "Attach" }, c.Calls);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 1, actual: flowContext.EndedCount);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task L2_OutOfRangeShowReturnsExactMessageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");
        var c = new TestPage("C");
        var pages = new[] { a, b, c };

        await AddPagesAsync(strategy, pages, context);

        var result = await strategy.ShowAsync(index: 5, context: context, token: CancellationToken.None);

        AssertFailure(result, "Index '5' is out of range; the flow has 3 pages.");
        AssertList(flow: flow, expected: pages, current: null);
        Assert.AreEqual(expected: 0, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 0, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task L3_ShowCurrentIndexReturnsAlreadyShownAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListSingle<TestPage>(flowContext);
        var context = new PageContext();
        var pages = new[] { new TestPage("A"), new TestPage("B"), new TestPage("C") };

        await AddPagesAsync(flow, pages, context);
        Assert.IsTrue((await flow.ShowAsync(index: 1, context: context, token: CancellationToken.None)).IsSuccess);
        var calls = pages[1].Calls.ToArray();

        var result = await flow.ShowAsync(index: 1, context: context, token: CancellationToken.None);

        AssertFailure(result, "The page at index '1' is already showing.");
        AssertList(flow, pages, pages[1]);
        CollectionAssert.AreEqual(calls, pages[1].Calls);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 1, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task L4_HideDefaultIndexReturnsDefaultIndexPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListSingle<TestPage>(flowContext);
        var context = new PageContext { DefaultIndex = 0 };
        var a = new TestPage("A");

        Assert.IsTrue((await flow.AddAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await flow.ShowAsync(index: 0, context: context, token: CancellationToken.None)).IsSuccess);
        var calls = a.Calls.ToArray();

        var result = await flow.HideAsync(context, CancellationToken.None);

        AssertFailure(result, "The page at index '0' is the default page and cannot be hidden.");
        AssertList(flow, new[] { a }, a);
        CollectionAssert.AreEqual(calls, a.Calls);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 1, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task L5_DetachRejectedKeepsPageInListAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var a = new TestPage("A") { DetachBehaviour = PageHookBehaviour.Reject };

        Assert.IsTrue((await strategy.AddAsync(a, context, CancellationToken.None)).IsSuccess);

        var result = await strategy.RemoveAsync(a, context, CancellationToken.None);

        AssertFailure(result, "The page 'A' rejected detaching from the flow.");
        AssertList(flow: flow, expected: new[] { a }, current: null);
        CollectionAssert.AreEqual(new[] { "Attach", "Detach" }, a.Calls);
        Assert.AreEqual(expected: 0, actual: flowContext.DetachCount);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task L6_RemoveMissingPageReturnsPageNotFoundAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");
        var c = new TestPage("C");
        var pages = new[] { a, b, c };

        await AddPagesAsync(strategy, pages, context);
        var d = new TestPage("D");

        var result = await strategy.RemoveAsync(d, context, CancellationToken.None);

        AssertFailure(result, "The page 'D' is not in the flow.");
        AssertList(flow: flow, expected: pages, current: null);
        Assert.IsEmpty(d.Calls);
        Assert.IsFalse(d.HasFlowScope);
        Assert.AreEqual(expected: 0, actual: flowContext.DetachCount);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task L7_RemoveAllDetachesThreePagesAndClearsCurrentPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");
        var c = new TestPage("C");
        var pages = new[] { a, b, c };

        await AddPagesAsync(strategy, pages, context);

        Assert.IsTrue((await strategy.ShowAsync(index: 1, context: context, token: CancellationToken.None)).IsSuccess);

        var result = await strategy.RemoveAllAsync(context, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        AssertList(flow: flow, expected: Array.Empty<TestPage>(), current: null);
        Assert.AreEqual(expected: 3, actual: flowContext.DetachCount);
        var count = pages.Length;

        for (var index = 0; index < count; index++)
        {
            var page = pages[index];
            Assert.IsFalse(page.HasFlowScope);
            Assert.AreEqual(expected: "Detach", actual: page.Calls[^1]);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task L8_AttachRejectedLeavesListEmptyAndUndoesAttachAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var a = new TestPage("A") { AttachBehaviour = PageHookBehaviour.Reject };

        var result = await strategy.AddAsync(a, context, CancellationToken.None);

        AssertFailure(result, "The page 'A' rejected attaching to the flow.");
        AssertList(flow: flow, expected: Array.Empty<TestPage>(), current: null);
        Assert.IsFalse(a.HasFlowScope);
        CollectionAssert.AreEqual(new[] { "Attach" }, a.Calls);
        Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 1, actual: flowContext.DetachCount);
        Assert.AreEqual(expected: 0, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 0, actual: flowContext.EndedCount);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task C5_ShowExceptionStillSelectsRequestedPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B") { ShowBehaviour = PageHookBehaviour.Throw };
        var pages = new[] { a, b };

        await AddPagesAsync(strategy, pages, context);
        Assert.IsTrue((await strategy.ShowAsync(index: 0, context: context, token: CancellationToken.None)).IsSuccess);

        var result = await strategy.ShowAsync(index: 1, context: context, token: CancellationToken.None);

        AssertFailure(result, "An exception occurred in the page flow: B.Show", PageTransitionState.Completed);
        Assert.IsTrue(result.IsApplied());
        AssertList(flow, pages, b);
        Assert.AreEqual(expected: "AfterShow", actual: b.Calls[^1]);
        Assert.AreEqual(expected: multi ? 2 : 3, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task C6_BeforeShowCancellationRethrowsAfterFinishingTransitionAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var a = new TestPage("A") { BeforeShowBehaviour = PageHookBehaviour.ThrowCancelled };
        OperationCanceledException thrown = null;

        Assert.IsTrue((await strategy.AddAsync(a, context, CancellationToken.None)).IsSuccess);

        try
        {
            await strategy.ShowAsync(index: 0, context: context, token: CancellationToken.None);
        }
        catch (OperationCanceledException ex)
        {
            thrown = ex;
        }

        Assert.IsNotNull(thrown);
        AssertList(flow, new[] { a }, a);
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, a.Calls);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: flowContext.BeganCount, actual: flowContext.EndedCount);
        LogAssert.NoUnexpectedReceived();
    }
}
