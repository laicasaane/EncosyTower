using System.Threading;
using System.Threading.Tasks;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PagePushWithoutSubscriberTests
{
    [Test]
    public async Task PageStackSinglePush_WithoutSubscriber_SucceedsAndLogsNothingAsync()
    {
        using var flowContext = new TestFlowContext(subscribe: false);
        using var stack = new PageStackSingle<TestPage>(flowContext);
        var context = new PageContext();

        var push = await stack.PushAsync(new TestPage("A"), context, CancellationToken.None);

        Assert.IsTrue(push.Result.IsSuccess);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task PageStackMultiPush_WithoutSubscriber_SucceedsAndLogsNothingAsync()
    {
        using var flowContext = new TestFlowContext(subscribe: false);
        using var stack = new PageStackMulti<TestPage>(flowContext);
        var context = new PageContext();

        var push = await stack.PushAsync(new TestPage("A"), context, CancellationToken.None);

        Assert.IsTrue(push.IsSuccess);
        LogAssert.NoUnexpectedReceived();
    }
}
