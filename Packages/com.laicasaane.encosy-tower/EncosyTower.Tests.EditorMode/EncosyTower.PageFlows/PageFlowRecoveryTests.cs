using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Flows;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageFlowRecoveryTests
{
    [Test]
    public async Task PageStackSingleOwnershipTrackerRejectsAnotherFlowUntilReleasedAsync()
    {
        var tracker = new PageOwnershipTracker();

        using var flowContext1 = new TestFlowContext {
            OwnershipTracker = Option.Some<IPageOwnershipTracker>(tracker),
        };

        using var flowContext2 = new TestFlowContext {
            OwnershipTracker = Option.Some<IPageOwnershipTracker>(tracker),
        };

        using var stack1 = new PageStackSingle<TestPage>(flowContext1);
        using var stack2 = new PageStackSingle<TestPage>(flowContext2);
        var context = new PageContext();
        var a = new TestPage("A");

        Assert.IsTrue((await stack1.PushAsync(a, context, CancellationToken.None)).Result.IsSuccess);

        var push = await stack2.PushAsync(a, context, CancellationToken.None);

        Assert.IsTrue(push.Result.IsFailure);

        Assert.AreEqual(
              expected: "The page 'A' is already attached to another flow."
            , actual: push.Result.GetFailureOrDefault().ToMessage()
        );

        Assert.AreEqual(
              expected: PageTransitionState.NotStarted
            , actual: push.Result.GetFailureOrDefault().TransitionState
        );

        Assert.IsFalse(stack2.CurrentPage.HasValue);
        Assert.AreEqual(expected: 1, actual: a.Calls.FindAll(static call => call == "Attach").Count);
        Assert.IsTrue((await stack1.PopAsync(context, CancellationToken.None)).Result.IsSuccess);
        Assert.IsTrue((await stack2.PushAsync(a, context, CancellationToken.None)).Result.IsSuccess);
    }

    [Test]
    public async Task PageStackSingleAttachExceptionRecoversAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageStackSingle<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B") { AttachBehaviour = PageHookBehaviour.Throw };
        var c = new TestPage("C");

        Assert.IsTrue((await flow.PushAsync(a, context, CancellationToken.None)).Result.IsSuccess);

        var push = await flow.PushAsync(b, context, CancellationToken.None);

        Assert.IsTrue(push.Result.IsFailure);
        Assert.AreEqual(
              expected: "An exception occurred in the page flow: B.Attach"
            , actual: push.Result.GetFailureOrDefault().ToMessage()
        );

        Assert.IsFalse(flow.IsInTransition);
        Assert.IsTrue(flow.CurrentPage.TryGetValue(out var current));
        Assert.AreSame(a, current);
        CollectionAssert.AreEqual(new[] { "Attach" }, b.Calls);

        Assert.IsTrue((await flow.PushAsync(c, context, CancellationToken.None)).Result.IsSuccess);
        Assert.IsFalse(flow.IsInTransition);
        Assert.IsTrue(flow.CurrentPage.TryGetValue(out current));
        Assert.AreSame(c, current);
    }

    [Test]
    public async Task PageStackSingleThrowingAttachSubscriberReturnsTheExceptionAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageStackSingle<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");
        var exception = new InvalidOperationException("attach subscriber failed");
        var subscriptions = new List<ISubscription>();

        var subscriber = flowContext.Subscriber.Scope(flowContext.FlowScope)
            .WithState(exception)
            .WithSubscriptions(subscriptions);

        AttachPageMessage.Async.Subscribe(in subscriber, Throw);

        var push = await flow.PushAsync(a, context, CancellationToken.None);

        subscriptions.Unsubscribe();

        Assert.IsTrue(push.Result.TryGetFailure(out var error));
        Assert.IsTrue(error._error.TryGetValue(out PageFlowError.Error.Exception wrapped));
        Assert.AreSame(exception, wrapped.Ex);
        Assert.IsFalse(flow.IsInTransition);
        Assert.IsFalse(flow.CurrentPage.HasValue);
        CollectionAssert.IsEmpty(a.Calls);
        Assert.IsFalse(a.HasFlowScope);
        LogAssert.NoUnexpectedReceived();

        static UnityTask Throw(
              InvalidOperationException ex
            , AttachPageMessage.Async message
            , PublishingContext publishingContext
        )
            => throw ex;
    }

    [Test]
    public async Task PageStackSingleAttachSubscriberCancellingTheCallerDeliversTheUndoDetachAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageStackSingle<TestPage>(flowContext);
        using var cancellation = new CancellationTokenSource();
        var context = new PageContext();
        var a = new TestPage("A");
        var subscriptions = new List<ISubscription>();

        var subscriber = flowContext.Subscriber.Scope(flowContext.FlowScope)
            .WithState(cancellation)
            .WithSubscriptions(subscriptions);

        AttachPageMessage.Async.Subscribe(in subscriber, Cancel);

        OperationCanceledException thrown = null;

        try
        {
            await flow.PushAsync(a, context, cancellation.Token);
        }
        catch (OperationCanceledException ex)
        {
            thrown = ex;
        }

        subscriptions.Unsubscribe();

        Assert.IsNotNull(thrown);
        Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 1, actual: flowContext.DetachCount);
        Assert.IsFalse(flow.IsInTransition);
        Assert.IsFalse(flow.CurrentPage.HasValue);
        Assert.IsFalse(a.HasFlowScope);
        LogAssert.NoUnexpectedReceived();

        static UnityTask Cancel(
              CancellationTokenSource source
            , AttachPageMessage.Async message
            , PublishingContext publishingContext
        )
        {
            source.Cancel();
            return UnityTask.CompletedTask;
        }
    }

    [Test]
    public async Task PageStackMultiShowExceptionReturnsCompletedFailureAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageStackMulti<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A") { ShowBehaviour = PageHookBehaviour.Throw };

        var result = await flow.PushAsync(a, context, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);

        Assert.AreEqual(
              expected: "An exception occurred in the page flow: A.Show"
            , actual: result.GetFailureOrDefault().ToMessage()
        );

        Assert.AreEqual(expected: PageTransitionState.Completed, actual: result.GetFailureOrDefault().TransitionState);
        Assert.IsFalse(flow.IsInTransition);
        Assert.AreEqual(expected: 1, actual: flow.PageCollection.Count);
        CollectionAssert.AreEqual(new[] { a }, flow.PageCollection);
        Assert.IsTrue(flow.CurrentPage.TryGetValue(out var current));
        Assert.AreSame(a, current);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 1, actual: flowContext.EndedCount);
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, a.Calls);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task PageStackMultiPopExceptionRecoversAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageStackMulti<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");

        Assert.IsTrue((await flow.PushAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await flow.PushAsync(b, context, CancellationToken.None)).IsSuccess);
        b.DetachBehaviour = PageHookBehaviour.Throw;

        var result = (await flow.PopAsync(context, CancellationToken.None)).Result;

        Assert.IsTrue(result.IsFailure);

        Assert.AreEqual(
              expected: "An exception occurred in the page flow: B.Detach"
            , actual: result.GetFailureOrDefault().ToMessage()
        );

        Assert.AreEqual(expected: PageTransitionState.Completed, actual: result.GetFailureOrDefault().TransitionState);
        Assert.IsFalse(flow.IsInTransition);
        Assert.AreEqual(expected: 1, actual: flow.PageCollection.Count);
        CollectionAssert.AreEqual(new[] { a }, flow.PageCollection);
        Assert.IsTrue(flow.CurrentPage.TryGetValue(out var current));
        Assert.AreSame(a, current);
        Assert.IsFalse(b.HasFlowScope);
        Assert.AreEqual(expected: 1, actual: flowContext.DetachCount);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task PageListMultiRemoveAllDetachesEveryPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListMulti<TestPage>(flowContext);
        var context = new PageContext();
        var pages = new[] { new TestPage("A"), new TestPage("B"), new TestPage("C") };
        var count = pages.Length;

        for (var index = 0; index < count; index++)
        {
            var page = pages[index];
            Assert.IsTrue((await flow.AddAsync(page, context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue(page.HasFlowScope);
        }

        Assert.AreEqual(expected: 3, actual: flowContext.AttachCount);
        Assert.IsTrue((await flow.RemoveAllAsync(context, CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: 3, actual: flowContext.DetachCount);
        Assert.AreEqual(expected: 0, actual: flow.Pages.Count);
        Assert.IsFalse(flow.IsInTransition);

        for (var index = 0; index < count; index++)
        {
            var page = pages[index];
            CollectionAssert.Contains(page.Calls, "Detach");
            Assert.IsFalse(page.HasFlowScope);
        }
    }

    [Test]
    public async Task PageListSingleRemoveAllDetachesEveryPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListSingle<TestPage>(flowContext);
        var context = new PageContext();
        var pages = new[] { new TestPage("A"), new TestPage("B"), new TestPage("C") };
        var count = pages.Length;

        for (var index = 0; index < count; index++)
        {
            var page = pages[index];
            Assert.IsTrue((await flow.AddAsync(page, context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue(page.HasFlowScope);
        }

        Assert.IsTrue((await flow.ShowAsync(index: 1, context: context, token: CancellationToken.None)).IsSuccess);
        Assert.IsTrue(flow.CurrentPage.TryGetValue(out var current));
        Assert.AreSame(pages[1], current);
        Assert.AreEqual(expected: 3, actual: flowContext.AttachCount);

        Assert.IsTrue((await flow.RemoveAllAsync(context, CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: 3, actual: flowContext.DetachCount);
        Assert.AreEqual(expected: 0, actual: flow.Pages.Count);
        Assert.IsFalse(flow.CurrentPage.HasValue);
        Assert.IsFalse(flow.IsInTransition);

        for (var index = 0; index < count; index++)
        {
            var page = pages[index];
            CollectionAssert.Contains(page.Calls, "Detach");
            Assert.IsFalse(page.HasFlowScope);
        }
    }

    [Test]
    public async Task PageStackMultiRemoveAllKeepsPagesWhenCancelledAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageStackMulti<TestPage>(flowContext);
        using var cancellation = new CancellationTokenSource();
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");
        var c = new TestPage("C");

        Assert.IsTrue((await flow.PushAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await flow.PushAsync(b, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await flow.PushAsync(c, context, CancellationToken.None)).IsSuccess);
        cancellation.Cancel();

        Assert.IsTrue((await flow.RemoveAllAsync(context, cancellation.Token)).IsFailure);
        Assert.AreEqual(expected: 3, actual: flow.PageCollection.Count);
        CollectionAssert.AreEqual(new[] { c, b, a }, flow.PageCollection);
        Assert.IsFalse(flow.IsInTransition);
        Assert.AreEqual(expected: 0, actual: flowContext.DetachCount);
    }

    [Test]
    public async Task PageListSingleShowsNewPageOnEmptyListAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListSingle<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");

        Assert.IsTrue((await flow.ShowAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: 1, actual: flow.Pages.Count);
        Assert.AreSame(a, flow.Pages[0]);
        Assert.IsTrue(flow.CurrentPage.TryGetValue(out var current));
        Assert.AreSame(a, current);
        Assert.IsFalse(flow.IsInTransition);
        Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 1, actual: flowContext.EndedCount);
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, a.Calls);
    }

    [Test]
    public async Task PageListMultiShowsNewPageOnEmptyListAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListMulti<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");

        Assert.IsTrue((await flow.ShowAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.AreEqual(expected: 1, actual: flow.Pages.Count);
        Assert.AreSame(a, flow.Pages[0]);
        Assert.IsFalse(flow.IsInTransition);
        Assert.AreEqual(expected: 1, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 1, actual: flowContext.EndedCount);
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, a.Calls);
    }

    [Test]
    public async Task PageListSingleAlreadyShowingReturnsFailureAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListSingle<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A");
        var b = new TestPage("B");

        Assert.IsTrue((await flow.AddAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await flow.AddAsync(b, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await flow.ShowAsync(index: 1, context: context, token: CancellationToken.None)).IsSuccess);

        var result = await flow.ShowAsync(index: 1, context: context, token: CancellationToken.None);

        Assert.IsTrue(result.IsFailure);

        Assert.AreEqual(
              expected: "The page at index '1' is already showing."
            , actual: result.GetFailureOrDefault().ToMessage()
        );

        Assert.IsTrue(flow.CurrentPage.TryGetValue(out var current));
        Assert.AreSame(b, current);
        Assert.IsFalse(flow.IsInTransition);
        Assert.AreEqual(expected: 1, actual: flowContext.BeganCount);
        Assert.AreEqual(expected: 1, actual: flowContext.EndedCount);
        CollectionAssert.AreEqual(new[] { "Attach", "BeforeShow", "Show", "AfterShow" }, b.Calls);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task PageListMultiDetachVetoKeepsPageAsync()
    {
        using var flowContext = new TestFlowContext();
        using var flow = new PageListMulti<TestPage>(flowContext);
        var context = new PageContext();
        var a = new TestPage("A") { DetachBehaviour = PageHookBehaviour.Reject };

        Assert.IsTrue((await flow.AddAsync(a, context, CancellationToken.None)).IsSuccess);
        Assert.IsTrue((await flow.RemoveAsync(a, context, CancellationToken.None)).IsFailure);
        Assert.AreEqual(expected: 1, actual: flow.Pages.Count);
        Assert.AreSame(a, flow.Pages[0]);
        Assert.IsTrue(a.HasFlowScope);
        Assert.AreEqual(expected: 0, actual: flowContext.DetachCount);
        CollectionAssert.AreEqual(new[] { "Attach", "Detach" }, a.Calls);
    }
}
