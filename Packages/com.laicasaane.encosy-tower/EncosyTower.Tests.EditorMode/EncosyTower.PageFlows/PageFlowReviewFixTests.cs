using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.Flows;
using EncosyTower.PageFlows.History;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageFlowReviewFixTests
{
    private const string DETACH = "Detach";

    [TestCase(RemoveAllFlowKind.MultiStack)]
    [TestCase(RemoveAllFlowKind.SingleList)]
    [TestCase(RemoveAllFlowKind.MultiList)]
    public async Task F5_RemoveAllDetachExceptionStillRemovesEveryPageAsync(RemoveAllFlowKind kind)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateRemoveAllFlow(kind, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var pages = new[] { new TestPage("A"), new TestPage("B"), new TestPage("C") };

        await AddPagesAsync(flow, pages, context);
        await ShowSecondPageOnSingleListAsync(flow, context);
        pages[1].DetachBehaviour = PageHookBehaviour.Throw;

        var result = await RemoveAllAsync(flow, context, CancellationToken.None);

        AssertFailure(result, "An exception occurred in the page flow: B.Detach", PageTransitionState.Completed);
        Assert.IsTrue(result.IsApplied());
        AssertNoPages(flow);
        Assert.AreEqual(expected: 3, actual: flowContext.DetachCount);
        AssertEveryPageDetachedOnce(pages);
        LogAssert.NoUnexpectedReceived();

        Success<PageFlowError> again;

        if (flow is IPageStackMulti<TestPage> stack)
        {
            again = (await stack.PopAsync(context, CancellationToken.None)).Result;
        }
        else
        {
            again = await RemoveAllAsync(flow, context, CancellationToken.None);
        }

        AssertFailure(again, "The flow has no page for this operation.", PageTransitionState.NotStarted);
        Assert.AreEqual(expected: 3, actual: flowContext.DetachCount);
        AssertEveryPageDetachedOnce(pages);
    }

    [TestCase(RemoveAllFlowKind.MultiStack)]
    [TestCase(RemoveAllFlowKind.SingleList)]
    [TestCase(RemoveAllFlowKind.MultiList)]
    public async Task F5_RemoveAllSecondDetachExceptionIsSuppressedAsync(RemoveAllFlowKind kind)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateRemoveAllFlow(kind, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var pages = new[] { new TestPage("A"), new TestPage("B"), new TestPage("C") };

        await AddPagesAsync(flow, pages, context);
        await ShowSecondPageOnSingleListAsync(flow, context);
        pages[0].DetachBehaviour = PageHookBehaviour.Throw;
        pages[1].DetachBehaviour = PageHookBehaviour.Throw;

        var result = await RemoveAllAsync(flow, context, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);

        var error = result.GetFailureOrThrow();

        Assert.AreEqual(expected: PageTransitionState.Completed, actual: error.TransitionState);
        Assert.AreEqual(expected: 1, actual: error.SuppressedErrors.Length);

        CollectionAssert.AreEquivalent(
              new[] {
                  "An exception occurred in the page flow: A.Detach",
                  "An exception occurred in the page flow: B.Detach",
              }
            , new[] { error.ToMessage(), error.SuppressedErrors[0].ToMessage() }
        );

        AssertNoPages(flow);
        AssertEveryPageDetachedOnce(pages);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(RemoveAllFlowKind.MultiStack)]
    [TestCase(RemoveAllFlowKind.SingleList)]
    [TestCase(RemoveAllFlowKind.MultiList)]
    public async Task F5_RemoveAllDetachCancellationRethrowsAfterRemovingEveryPageAsync(RemoveAllFlowKind kind)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateRemoveAllFlow(kind, flowContext);
        using var lifetime = (IDisposable)flow;
        using var cancellation = new CancellationTokenSource();
        var context = new PageContext();
        var pages = new[] { new TestPage("A"), new TestPage("B"), new TestPage("C") };
        OperationCanceledException thrown = null;

        await AddPagesAsync(flow, pages, context);
        await ShowSecondPageOnSingleListAsync(flow, context);
        pages[1].DetachBehaviour = PageHookBehaviour.ThrowCancelled;
        pages[1].CancelOn(DETACH, cancellation);

        try
        {
            await RemoveAllAsync(flow, context, cancellation.Token);
        }
        catch (OperationCanceledException ex)
        {
            thrown = ex;
        }

        Assert.IsNotNull(thrown);
        Assert.IsTrue(cancellation.IsCancellationRequested);
        AssertNoPages(flow);
        Assert.AreEqual(expected: 3, actual: flowContext.DetachCount);
        AssertEveryPageDetachedOnce(pages);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(RemoveAllFlowKind.MultiStack)]
    [TestCase(RemoveAllFlowKind.SingleList)]
    [TestCase(RemoveAllFlowKind.MultiList)]
    public async Task F10_RemoveAllReportsADetachRejectionAsync(RemoveAllFlowKind kind)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateRemoveAllFlow(kind, flowContext);
        using var lifetime = (IDisposable)flow;
        var context = new PageContext();
        var pages = new[] { new TestPage("A"), new TestPage("B"), new TestPage("C") };

        await AddPagesAsync(flow, pages, context);
        await ShowSecondPageOnSingleListAsync(flow, context);
        pages[1].DetachBehaviour = PageHookBehaviour.Reject;

        var result = await RemoveAllAsync(flow, context, CancellationToken.None);

        AssertFailure(result, "The page 'B' rejected detaching from the flow.", PageTransitionState.Completed);
        Assert.AreEqual(expected: 0, actual: result.GetFailureOrThrow().SuppressedErrors.Length);
        AssertNoPages(flow);
        Assert.AreEqual(expected: 3, actual: flowContext.DetachCount);
        AssertEveryPageDetachedOnce(pages);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator F9_SingleListDroppedShowRemovesAddedPage()
        => UnityTask.ToCoroutine(static async () => {
            await DroppedShowRemovesAddedPageAsync(multi: false);
        });

    [UnityTest]
    public IEnumerator F9_MultiListDroppedShowRemovesAddedPage()
        => UnityTask.ToCoroutine(static async () => {
            await DroppedShowRemovesAddedPageAsync(multi: true);
        });

    [Test]
    public async Task F3_ReattachKeepsHistoryAsync()
    {
        using var flowContext = new TestFlowContext();
        using var screen = new FakeHistoryFlow(flowContext, "Screen", PageFlowKind.PageStackSingle);
        using var history = new PageHistory();

        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);
        history.Detach();

        using var rebuilt = new FakeHistoryFlow(flowContext, "Screen", PageFlowKind.PageStackSingle);
        Assert.IsTrue(history.Attach(rebuilt).IsSuccess);

        Assert.AreEqual(expected: 2, actual: history.Entries.Count);
        Assert.AreEqual(expected: "A", actual: history.Entries[0].To.AssetKey.Value);
        Assert.AreEqual(expected: "B", actual: history.Entries[1].To.AssetKey.Value);
        Assert.AreEqual(expected: 1, actual: history.CurrentIndex);
        Assert.IsEmpty(rebuilt.Calls);

        Assert.IsTrue((await history.GoBackAsync(CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: "Show:A", actual: rebuilt.Calls[^1]);
        CollectionAssert.AreEqual(new[] { "A" }, rebuilt.Stack);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);
        Assert.AreEqual(expected: 2, actual: history.Entries.Count);
    }

    [UnityTest]
    public IEnumerator F4_MultiStackRemoveAllDetachesPagePushedWhileQueued()
        => UnityTask.ToCoroutine(static async () => {
            await RemoveAllDetachesPagePushedWhileQueuedAsync();
        });

    [UnityTest]
    public IEnumerator F4_SingleListRemoveAllDetachesPageAddedWhileQueued()
        => UnityTask.ToCoroutine(static async () => {
            await RemoveAllDetachesPageAddedWhileQueuedAsync(multi: false);
        });

    [UnityTest]
    public IEnumerator F4_MultiListRemoveAllDetachesPageAddedWhileQueued()
        => UnityTask.ToCoroutine(static async () => {
            await RemoveAllDetachesPageAddedWhileQueuedAsync(multi: true);
        });

    [UnityTest]
    public IEnumerator F4_SingleListRemoveAllSkipsPageRemovedWhileQueued()
        => UnityTask.ToCoroutine(static async () => {
            await RemoveAllSkipsPageRemovedWhileQueuedAsync(multi: false);
        });

    [UnityTest]
    public IEnumerator F4_MultiListRemoveAllSkipsPageRemovedWhileQueued()
        => UnityTask.ToCoroutine(static async () => {
            await RemoveAllSkipsPageRemovedWhileQueuedAsync(multi: true);
        });

    [UnityTest]
    public IEnumerator F7_SingleStackQueuedPopOnEmptiedStackReturnsNoPage()
        => UnityTask.ToCoroutine(static async () => {
            await QueuedPopOnEmptiedStackReturnsNoPageAsync(multi: false);
        });

    [UnityTest]
    public IEnumerator F7_MultiStackQueuedPopOnEmptiedStackReturnsNoPage()
        => UnityTask.ToCoroutine(static async () => {
            await QueuedPopOnEmptiedStackReturnsNoPageAsync(multi: true);
        });

    [TestCase(false)]
    [TestCase(true)]
    public async Task F8_ListDisposedDuringAttachDetachesThePageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var a = new TestPage("A") { HookAction = DisposeOnAttach };

        var result = await list.AddAsync(a, new PageContext(), CancellationToken.None);

        AssertFailure(result, "The operation was cancelled before the flow changed.", PageTransitionState.NotStarted);
        Assert.IsTrue(result.GetFailureOrThrow().IsCancelled);
        AssertListPages(flow);
        CollectionAssert.AreEqual(new[] { "Attach", DETACH }, a.Calls);
        Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 1, actual: flowContext.DetachCount);
        Assert.IsFalse(a.HasFlowScope);
        LogAssert.NoUnexpectedReceived();

        void DisposeOnAttach(string hook, PageContext received)
        {
            if (hook == "Attach")
            {
                lifetime.Dispose();
            }
        }
    }

    private static async UnityTask DroppedShowRemovesAddedPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var dropContext = new PageContext { AsyncOperation = PageAsyncOperation.Drop };
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var c = new TestPage("C");

        Assert.IsTrue((await list.AddAsync(a, context, CancellationToken.None)).IsSuccess);

        var pending = list.ShowAsync(index: 0, context: context, token: CancellationToken.None);
        Success<PageFlowError> pendingResult = default;
        Success<PageFlowError> result = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            var attachCount = flowContext.AttachCount;
            var detachCount = flowContext.DetachCount;

            result = await list.ShowAsync(c, dropContext, CancellationToken.None);

            Assert.AreEqual(expected: attachCount + 1, actual: flowContext.AttachCount);
            Assert.AreEqual(expected: detachCount + 1, actual: flowContext.DetachCount);
        }
        finally
        {
            gate.Open();
            pendingResult = await pending;
        }

        AssertFailure(result, $"{flow.GetType()} is currently in transition.", PageTransitionState.NotStarted);
        Assert.IsFalse(result.IsApplied());
        CollectionAssert.AreEqual(new[] { "Attach", DETACH }, c.Calls);
        Assert.IsFalse(c.HasFlowScope);
        Assert.IsTrue(pendingResult.IsSuccess);
        AssertListPages(flow, a);
        Assert.IsTrue(a.HasFlowScope);

        if (flow is PageListSingle<TestPage> single)
        {
            Assert.IsTrue(single.CurrentPage.TryGetValue(out var current));
            Assert.AreSame(a, current);
        }

        LogAssert.NoUnexpectedReceived();
    }

    private static async UnityTask RemoveAllDetachesPagePushedWhileQueuedAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var sequential = new PageContext { AsyncOperation = PageAsyncOperation.Sequential };
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var first = stack.PushAsync(a, new PageContext(), CancellationToken.None);
        var second = stack.PushAsync(b, sequential, CancellationToken.None);
        var removeAll = stack.RemoveAllAsync(sequential, CancellationToken.None);
        Success<PageFlowError> firstResult = default;
        Success<PageFlowError> secondResult = default;
        Success<PageFlowError> removeAllResult = default;

        try
        {
            Assert.IsTrue(stack.IsInTransition);
            Assert.IsFalse(second.GetAwaiter().IsCompleted);
            Assert.IsFalse(removeAll.GetAwaiter().IsCompleted);
        }
        finally
        {
            gate.Open();
            firstResult = await first;
            secondResult = await second;
            removeAllResult = await removeAll;
        }

        Assert.IsTrue(firstResult.IsSuccess);
        Assert.IsTrue(secondResult.IsSuccess);
        Assert.IsTrue(removeAllResult.IsSuccess);
        AssertNoPages(stack);
        Assert.AreEqual(expected: 2, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 2, actual: flowContext.DetachCount);
        AssertEveryPageDetachedOnce(new[] { a, b });
        LogAssert.NoUnexpectedReceived();
    }

    private static async UnityTask RemoveAllDetachesPageAddedWhileQueuedAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var sequential = new PageContext { AsyncOperation = PageAsyncOperation.Sequential };
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var c = new TestPage("C");

        Assert.IsTrue((await list.AddAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.AddAsync(b, context, CancellationToken.None)).IsSuccess);

        var show = list.ShowAsync(index: 0, context: context, token: CancellationToken.None);
        var removeAll = list.RemoveAllAsync(sequential, CancellationToken.None);
        Success<PageFlowError> showResult = default;
        Success<PageFlowError> removeAllResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            Assert.IsFalse(removeAll.GetAwaiter().IsCompleted);
            Assert.IsTrue((await list.AddAsync(c, context, CancellationToken.None)).IsSuccess);
        }
        finally
        {
            gate.Open();
            showResult = await show;
            removeAllResult = await removeAll;
        }

        Assert.IsTrue(showResult.IsSuccess);
        Assert.IsTrue(removeAllResult.IsSuccess);
        AssertNoPages(flow);
        Assert.AreEqual(expected: 3, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 3, actual: flowContext.DetachCount);
        AssertEveryPageDetachedOnce(new[] { a, b, c });
        LogAssert.NoUnexpectedReceived();
    }

    private static async UnityTask RemoveAllSkipsPageRemovedWhileQueuedAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateList(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var context = new PageContext();
        var sequential = new PageContext { AsyncOperation = PageAsyncOperation.Sequential };
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var c = new TestPage("C");

        Assert.IsTrue((await list.AddAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.AddAsync(b, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await list.AddAsync(c, context, CancellationToken.None)).IsSuccess);

        var show = list.ShowAsync(index: 0, context: context, token: CancellationToken.None);
        var removeAll = list.RemoveAllAsync(sequential, CancellationToken.None);
        Success<PageFlowError> showResult = default;
        Success<PageFlowError> removeAllResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            Assert.IsFalse(removeAll.GetAwaiter().IsCompleted);
            Assert.IsTrue((await list.RemoveAsync(c, context, CancellationToken.None)).IsSuccess);
        }
        finally
        {
            gate.Open();
            showResult = await show;
            removeAllResult = await removeAll;
        }

        Assert.IsTrue(showResult.IsSuccess);
        Assert.IsTrue(removeAllResult.IsSuccess);
        AssertNoPages(flow);
        Assert.AreEqual(expected: 3, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 3, actual: flowContext.DetachCount);
        AssertEveryPageDetachedOnce(new[] { a, b, c });
        LogAssert.NoUnexpectedReceived();
    }

    private static async UnityTask QueuedPopOnEmptiedStackReturnsNoPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateStack(multi, flowContext);
        using var lifetime = (IDisposable)flow;
        var stack = (IPageStackStrategy<TestPage>)flow;
        var sequential = new PageContext { AsyncOperation = PageAsyncOperation.Sequential };
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var push = stack.PushAsync(a, new PageContext(), CancellationToken.None);
        var firstPop = stack.PopAsync(sequential, CancellationToken.None);
        var secondPop = stack.PopAsync(sequential, CancellationToken.None);
        Success<PageFlowError> pushResult = default;
        Success<PageFlowError> firstPopResult = default;
        Success<PageFlowError> secondPopResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);
            Assert.IsFalse(firstPop.GetAwaiter().IsCompleted);
            Assert.IsFalse(secondPop.GetAwaiter().IsCompleted);
        }
        finally
        {
            gate.Open();
            pushResult = await push;
            firstPopResult = await firstPop;
            secondPopResult = await secondPop;
        }

        Assert.IsTrue(pushResult.IsSuccess);
        Assert.IsTrue(firstPopResult.IsSuccess);
        AssertFailure(secondPopResult, "The flow has no page for this operation.", PageTransitionState.NotStarted);
        AssertNoPages(flow);
        Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 1, actual: flowContext.DetachCount);
        Assert.AreEqual(expected: 2, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 2, actual: flowContext.EndedCount);
        AssertEveryPageDetachedOnce(new[] { a });
        LogAssert.NoUnexpectedReceived();
    }

    private static IPageFlow CreateStack(bool multi, TestFlowContext context)
        => multi ? new PageStackMulti<TestPage>(context) : new PageStackSingle<TestPage>(context);

    private static IPageFlow CreateList(bool multi, TestFlowContext context)
        => multi ? new PageListMulti<TestPage>(context) : new PageListSingle<TestPage>(context);

    private static IPageFlow CreateRemoveAllFlow(RemoveAllFlowKind kind, TestFlowContext context)
        => kind switch {
            RemoveAllFlowKind.MultiStack => new PageStackMulti<TestPage>(context),
            RemoveAllFlowKind.SingleList => new PageListSingle<TestPage>(context),
            _ => new PageListMulti<TestPage>(context),
        };

    private static async UnityTask AddPagesAsync(IPageFlow flow, TestPage[] pages, PageContext context)
    {
        var count = pages.Length;

        for (var index = 0; index < count; index++)
        {
            var page = pages[index];

            var result = flow is IPageStackMulti<TestPage> stack
                ? await stack.PushAsync(page, context, CancellationToken.None)
                : await ((IPageListStrategy<TestPage>)flow).AddAsync(page, context, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
        }
    }

    private static async UnityTask ShowSecondPageOnSingleListAsync(IPageFlow flow, PageContext context)
    {
        if (flow is PageListSingle<TestPage> single)
        {
            var result = await single.ShowAsync(index: 1, context: context, token: CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(single.CurrentPage.HasValue);
        }
    }

    private static UnityTask<Success<PageFlowError>> RemoveAllAsync(
          IPageFlow flow
        , PageContext context
        , CancellationToken token
    )
        => flow is IPageStackMulti<TestPage> stack
            ? stack.RemoveAllAsync(context, token)
            : ((IPageListStrategy<TestPage>)flow).RemoveAllAsync(context, token);

    private static void AssertFailure(Success<PageFlowError> result, string message, PageTransitionState state)
    {
        Assert.IsTrue(result.IsFailure);
        var error = result.GetFailureOrThrow();
        Assert.AreEqual(expected: message, actual: error.ToMessage());
        Assert.AreEqual(expected: state, actual: error.TransitionState);
    }

    private static void AssertNoPages(IPageFlow flow)
    {
        Assert.IsFalse(flow.IsInTransition);

        if (flow is IHasCurrentPage<TestPage> hasCurrentPage)
        {
            Assert.IsFalse(hasCurrentPage.CurrentPage.HasValue);
        }

        switch (flow)
        {
            case PageStackMulti<TestPage> stack:
            {
                Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
                break;
            }

            case IHasPages<TestPage> list:
            {
                Assert.AreEqual(expected: 0, actual: list.Pages.Count);
                break;
            }
        }
    }

    private static void AssertListPages(IPageFlow flow, params TestPage[] expected)
    {
        Assert.IsFalse(flow.IsInTransition);
        var pages = ((IHasPages<TestPage>)flow).Pages;
        Assert.AreEqual(expected: expected.Length, actual: pages.Count);
        CollectionAssert.AreEqual(expected, pages);
    }

    private static void AssertEveryPageDetachedOnce(TestPage[] pages)
    {
        var count = pages.Length;

        for (var index = 0; index < count; index++)
        {
            var page = pages[index];
            Assert.AreEqual(expected: 1, actual: page.Calls.FindAll(static call => call == DETACH).Count);
            Assert.IsFalse(page.HasFlowScope);
        }
    }

    public enum RemoveAllFlowKind
    {
        MultiStack,
        SingleList,
        MultiList,
    }
}
