using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageListAddCancellationTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task AddCancelledDuringAcceptedAttachDetachesThePageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();

        var flow = multi
            ? (IPageFlow)new PageListMulti<TestPage>(flowContext)
            : new PageListSingle<TestPage>(flowContext);

        using var lifetime = (IDisposable)flow;
        using var cancellation = new CancellationTokenSource();
        var list = (IPageListStrategy<TestPage>)flow;
        var a = new TestPage("A");
        a.CancelOn("Attach", cancellation);

        var result = await list.AddAsync(a, new PageContext(), cancellation.Token);

        Assert.IsTrue(result.TryGetFailure(out var error));
        Assert.IsTrue(error.IsCancelled);
        Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);
        Assert.IsFalse(flow.IsInTransition);
        Assert.AreEqual(expected: 0, actual: ((IHasPages<TestPage>)flow).Pages.Count);
        CollectionAssert.AreEqual(new[] { "Attach", "Detach" }, a.Calls);
        Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: flowContext.AttachCount, actual: flowContext.DetachCount);
        Assert.IsFalse(a.HasFlowScope);
        LogAssert.NoUnexpectedReceived();
    }
}
