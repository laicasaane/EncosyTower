using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageListDuplicateTests
{
    private const string FLOW_LEVEL
        = "The page 'A' is already in the flow; duplicate pages are rejected at the Flow level.";

    private const string PAGE_LEVEL
        = "The page 'A' is already in the flow; duplicate pages are rejected at the Page level.";

    private static IPageFlow CreateList(bool multi, TestFlowContext context)
        => multi ? new PageListMulti<TestPage>(context) : new PageListSingle<TestPage>(context);

    private static void AssertFailure(Success<PageFlowError> result, string message)
    {
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(expected: message, actual: result.GetFailureOrThrow().ToMessage());
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task DU1_DuplicatesAllowedByDefault_AddsTwiceAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var a = new TestPage("A");

        Assert.IsTrue((await list.AddAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.AddAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);

        Assert.AreEqual(expected: 2, actual: ((IHasPages<TestPage>)flow).Pages.Count);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task DU2_FlowRejects_ReturnsPageAlreadyAddedAtFlowLevelAsync(bool multi)
    {
        using var flowContext = new TestFlowContext { RejectDuplicatePages = true };
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var a = new TestPage("A");

        Assert.IsTrue((await list.AddAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);

        var result = await list.AddAsync(a, new PageContext(), CancellationToken.None);

        AssertFailure(result, FLOW_LEVEL);
        CollectionAssert.AreEqual(new[] { "Attach" }, a.Calls);
        Assert.AreEqual(expected: 1, actual: ((IHasPages<TestPage>)flow).Pages.Count);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task DU3_FlowAllowsPageRejects_RejectsAtPageLevelAsync(bool multi)
    {
        using var flowContext = new TestFlowContext { RejectDuplicatePages = false };
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;

        var a = new TestPage("A") {
            PageOptions = new PageOptions { rejectDuplicatePages = new(value: true, isOverridden: true) },
        };

        Assert.IsTrue((await list.AddAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);

        var result = await list.AddAsync(a, new PageContext(), CancellationToken.None);

        AssertFailure(result, PAGE_LEVEL);
        CollectionAssert.AreEqual(new[] { "Attach" }, a.Calls);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task DU4_FlowRejectsPageAllows_FlowWinsAsync(bool multi)
    {
        using var flowContext = new TestFlowContext { RejectDuplicatePages = true };
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;

        var a = new TestPage("A") {
            PageOptions = new PageOptions { rejectDuplicatePages = new(value: false, isOverridden: true) },
        };

        Assert.IsTrue((await list.AddAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);

        var result = await list.AddAsync(a, new PageContext(), CancellationToken.None);

        AssertFailure(result, FLOW_LEVEL);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task DU5_ShowOfAListedPage_IsNotADuplicateAsync(bool multi)
    {
        using var flowContext = new TestFlowContext { RejectDuplicatePages = true };
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var a = new TestPage("A");

        Assert.IsTrue((await list.AddAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);

        var result = await list.ShowAsync(a, new PageContext(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expected: 1, actual: ((IHasPages<TestPage>)flow).Pages.Count);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task DU6_ConcurrentAddOfOnePage_SecondIsRejectedAsync(bool multi)
    {
        using var flowContext = new TestFlowContext { RejectDuplicatePages = true };
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { AttachGate = gate };
        var first = list.AddAsync(a, new PageContext(), CancellationToken.None);
        Success<PageFlowError> firstResult = default;
        Success<PageFlowError> secondResult = default;

        try
        {
            secondResult = await list.AddAsync(a, new PageContext(), CancellationToken.None);
        }
        finally
        {
            gate.Open();
            firstResult = await first;
        }

        AssertFailure(secondResult, FLOW_LEVEL);
        Assert.IsTrue(firstResult.IsSuccess);
        Assert.AreEqual(expected: 1, actual: ((IHasPages<TestPage>)flow).Pages.Count);
        Assert.AreSame(a, ((IHasPages<TestPage>)flow).Pages[0]);
        CollectionAssert.AreEqual(new[] { "Attach" }, a.Calls);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task DU7_FactoryAddRejectedWhileAnotherAddIsInFlight_DoesNotReleaseThePageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext { RejectDuplicatePages = true };
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { AttachGate = gate };
        var released = new List<TestPage>();
        var first = list.AddAsync(a, new PageContext(), CancellationToken.None);
        Success<PageFlowError> firstResult = default;
        Success<PageFlowError> secondResult = default;

        try
        {
            secondResult = await list.AddAsync(Create, Release, new PageContext(), CancellationToken.None);
            Assert.IsEmpty(released);
        }
        finally
        {
            gate.Open();
            firstResult = await first;
        }

        AssertFailure(secondResult, FLOW_LEVEL);
        Assert.IsTrue(firstResult.IsSuccess);
        Assert.IsEmpty(released);
        Assert.AreEqual(expected: 1, actual: ((IHasPages<TestPage>)flow).Pages.Count);
        Assert.AreSame(a, ((IHasPages<TestPage>)flow).Pages[0]);
        LogAssert.NoUnexpectedReceived();

        UnityTask<Result<TestPage, PageFlowError>> Create(CancellationToken token)
            => UnityTask.FromResult(Result<TestPage, PageFlowError>.Succeed(a));

        void Release(TestPage page)
        {
            released.Add(page);
        }
    }
}
