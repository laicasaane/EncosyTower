using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using EncosyTower.Tasks;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PagePopResultTests
{
    [Test]
    public async Task F6_SingleStackPopReturnsThePoppedPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackSingle<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");

        Assert.IsTrue((await stack.PushAsync(a, context, CancellationToken.None)).Result.IsSuccess);

        var pop = await stack.PopAsync(context, CancellationToken.None);

        Assert.IsTrue(pop.Result.IsSuccess);
        AssertPopped(pop, a);
        Assert.IsFalse(stack.CurrentPage.HasValue);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task F6_MultiStackPopReturnsEachPoppedPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");

        Assert.IsTrue((await stack.PushAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await stack.PushAsync(b, context, CancellationToken.None)).IsSuccess);

        var first = await stack.PopAsync(context, CancellationToken.None);
        var second = await stack.PopAsync(context, CancellationToken.None);

        Assert.IsTrue(first.Result.IsSuccess);
        AssertPopped(first, b);
        Assert.IsTrue(second.Result.IsSuccess);
        AssertPopped(second, a);
        Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task F6_PopOnEmptyStackHasNoPoppedPageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var context = new PageContext();
        PagePopResult<TestPage> pop;

        if (multi)
        {
            using var stack = new PageStackMulti<TestPage>(flowContext);
            pop = await stack.PopAsync(context, CancellationToken.None);
        }
        else
        {
            using var stack = new PageStackSingle<TestPage>(flowContext);
            pop = await stack.PopAsync(context, CancellationToken.None);
        }

        Assert.IsTrue(pop.Result.TryGetFailure(out var error));
        Assert.AreEqual(expected: "The flow has no page for this operation.", actual: error.ToMessage());
        Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);
        Assert.IsFalse(pop.Popped.HasValue);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task F6_CompletedPopWithDetachExceptionReturnsThePoppedPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");

        Assert.IsTrue((await stack.PushAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await stack.PushAsync(b, context, CancellationToken.None)).IsSuccess);
        b.DetachBehaviour = PageHookBehaviour.Throw;

        var pop = await stack.PopAsync(context, CancellationToken.None);

        Assert.IsTrue(pop.Result.TryGetFailure(out var error));
        Assert.AreEqual(expected: "An exception occurred in the page flow: B.Detach", actual: error.ToMessage());
        Assert.AreEqual(expected: PageTransitionState.Completed, actual: error.TransitionState);
        Assert.IsTrue(pop.Result.IsApplied());
        AssertPopped(pop, b);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator F6_QueuedPopsReturnTheirOwnPoppedPages()
        => UnityTask.ToCoroutine(static async () => {
            await QueuedPopsReturnTheirOwnPoppedPagesAsync();
        });

    private static async UnityTask QueuedPopsReturnTheirOwnPoppedPagesAsync()
    {
        using var flowContext = new TestFlowContext();
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var context = new PageContext();
        var sequential = new PageContext { AsyncOperation = PageAsyncOperation.Sequential };
        var gate = new TestGate();
        var a = new TestPage("A");
        var b = new TestPage("B");
        var c = new TestPage("C") { ShowGate = gate };

        Assert.IsTrue((await stack.PushAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await stack.PushAsync(b, context, CancellationToken.None)).IsSuccess);

        var push = stack.PushAsync(c, context, CancellationToken.None);
        var firstPop = stack.PopAsync(sequential, CancellationToken.None);
        var secondPop = stack.PopAsync(sequential, CancellationToken.None);
        Success<PageFlowError> pushResult = default;
        PagePopResult<TestPage> firstPopResult = default;
        PagePopResult<TestPage> secondPopResult = default;

        try
        {
            Assert.IsTrue(stack.IsInTransition);
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
        Assert.IsTrue(firstPopResult.Result.IsSuccess);
        AssertPopped(firstPopResult, c);
        Assert.IsTrue(secondPopResult.Result.IsSuccess);
        AssertPopped(secondPopResult, b);
        Assert.IsTrue(stack.CurrentPage.TryGetValue(out var current));
        Assert.AreSame(a, current);
        Assert.AreEqual(expected: 1, actual: stack.PageCollection.Count);
        Assert.AreEqual(expected: 3, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 2, actual: flowContext.DetachCount);
        LogAssert.NoUnexpectedReceived();
    }

    private static void AssertPopped(PagePopResult<TestPage> pop, TestPage expected)
    {
        Assert.IsTrue(pop.Popped.TryGetValue(out var popped));
        Assert.AreSame(expected, popped);
    }
}
