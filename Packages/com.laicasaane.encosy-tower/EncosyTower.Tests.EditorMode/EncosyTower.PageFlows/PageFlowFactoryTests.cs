using System;
using System.Collections;
using System.Collections.Generic;
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

public sealed class PageFlowFactoryTests
{
    public enum FactoryCall
    {
        StackSinglePush,
        StackMultiPush,
        ListSingleAdd,
        ListMultiAdd,
        ListSingleShow,
        ListMultiShow,
        ListMultiHide,
    }

    [TestCase(FactoryCall.StackSinglePush)]
    [TestCase(FactoryCall.StackMultiPush)]
    [TestCase(FactoryCall.ListSingleAdd)]
    [TestCase(FactoryCall.ListMultiAdd)]
    [TestCase(FactoryCall.ListSingleShow)]
    [TestCase(FactoryCall.ListMultiShow)]
    [TestCase(FactoryCall.ListMultiHide)]
    public async Task FA1_FactoryFailure_ReturnsItsErrorAndNeverReleasesAsync(FactoryCall call)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateFlow(call, flowContext);
        using var lifetime = (IDisposable)flow;
        var error = PageFlowError.CreateRejected("key");
        var probe = new Probe(error);

        var result = await CallAsync(call, flow, probe, new PageContext(), CancellationToken.None);

        AssertFailure(result, error.ToMessage());
        Assert.AreEqual(expected: 1, actual: probe.FactoryCalls);
        Assert.IsEmpty(probe.Released);
        Assert.AreEqual(expected: 0, actual: flowContext.AttachCount);
        Assert.AreEqual(expected: 0, actual: CountPages(flow));
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FA2_DroppedPush_ReleasesThePageAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateFlow(multi ? FactoryCall.StackMultiPush : FactoryCall.StackSinglePush, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageStackStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var probe = new Probe(b);
        var first = strategy.PushAsync(a, new PageContext(), CancellationToken.None);
        Success<PageFlowError> firstResult = default;
        Success<PageFlowError> secondResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);

            secondResult = await strategy.PushAsync(
                  probe.CreateAsync
                , probe.Release
                , new PageContext()
                , CancellationToken.None
            );
        }
        finally
        {
            gate.Open();
            firstResult = await first;
        }

        Assert.IsTrue(firstResult.IsSuccess);
        AssertFailure(secondResult, $"{flow.GetType()} is currently in transition.");
        CollectionAssert.AreEqual(new[] { b }, probe.Released);
        Assert.IsEmpty(b.Calls);
        Assert.IsFalse(IsInFlow(flow, b));
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(FactoryCall.StackSinglePush)]
    [TestCase(FactoryCall.StackMultiPush)]
    [TestCase(FactoryCall.ListSingleShow)]
    [TestCase(FactoryCall.ListMultiShow)]
    [TestCase(FactoryCall.ListMultiHide)]
    public async Task FA3_AbortWhileTheFactoryRuns_ReleasesThePageAsync(FactoryCall call)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateFlow(call, flowContext);
        using var lifetime = (IDisposable)flow;
        var b = new TestPage("B");
        var probe = new Probe(b);
        probe.OnCreate = AbortFlow;

        var result = await CallAsync(call, flow, probe, new PageContext(), CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.IsTrue(result.GetFailureOrThrow().IsAborted);
        CollectionAssert.AreEqual(new[] { b }, probe.Released);
        Assert.IsFalse(IsInFlow(flow, b));
        Assert.IsEmpty(b.Calls);
        LogAssert.NoUnexpectedReceived();

        void AbortFlow()
        {
            flow.AbortCurrent(PageAbortMode.Reverse);
        }
    }

    [UnityTest]
    public IEnumerator FA4_SingleStackAbortDuringTheShowTransition_ReleasesThePage()
        => UnityTask.ToCoroutine(static async () => {
            await AbortDuringTheShowTransitionAsync(multi: false);
        });

    [UnityTest]
    public IEnumerator FA4_MultiStackAbortDuringTheShowTransition_ReleasesThePage()
        => UnityTask.ToCoroutine(static async () => {
            await AbortDuringTheShowTransitionAsync(multi: true);
        });

    [TestCase(FactoryCall.StackSinglePush)]
    [TestCase(FactoryCall.StackMultiPush)]
    [TestCase(FactoryCall.ListSingleAdd)]
    [TestCase(FactoryCall.ListMultiAdd)]
    [TestCase(FactoryCall.ListSingleShow)]
    [TestCase(FactoryCall.ListMultiShow)]
    public async Task FA5_CancelledAfterTheFactory_ReleasesThePageAsync(FactoryCall call)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateFlow(call, flowContext);
        using var lifetime = (IDisposable)flow;
        using var cancellation = new CancellationTokenSource();
        var b = new TestPage("B");
        var probe = new Probe(b);
        probe.OnCreate = CancelCaller;

        var result = await CallAsync(call, flow, probe, new PageContext(), cancellation.Token);

        Assert.IsTrue(result.IsFailure);
        Assert.IsTrue(result.GetFailureOrThrow().IsCancelled);
        CollectionAssert.AreEqual(new[] { b }, probe.Released);
        Assert.IsFalse(IsInFlow(flow, b));
        LogAssert.NoUnexpectedReceived();

        void CancelCaller()
        {
            cancellation.Cancel();
        }
    }

    [TestCase(FactoryCall.StackSinglePush)]
    [TestCase(FactoryCall.StackMultiPush)]
    [TestCase(FactoryCall.ListSingleAdd)]
    [TestCase(FactoryCall.ListMultiAdd)]
    [TestCase(FactoryCall.ListSingleShow)]
    [TestCase(FactoryCall.ListMultiShow)]
    [TestCase(FactoryCall.ListMultiHide)]
    public async Task FA6_SuccessfulCalls_NeverReleaseAsync(FactoryCall call)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateFlow(call, flowContext);
        using var lifetime = (IDisposable)flow;
        var a = new TestPage("A");
        var probe = new Probe(a);

        if (call == FactoryCall.ListMultiHide)
        {
            var list = (IPageListStrategy<TestPage>)flow;

            Assert.IsTrue((await list.AddAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await list.ShowAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);
        }

        var result = await CallAsync(call, flow, probe, new PageContext(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsEmpty(probe.Released);
        Assert.IsTrue(IsInFlow(flow, a));
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FA7_ExceptionOperationWhileInTransition_ReleasesAndThrowsAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateFlow(multi ? FactoryCall.StackMultiPush : FactoryCall.StackSinglePush, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageStackStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var probe = new Probe(b);
        var context = new PageContext { AsyncOperation = PageAsyncOperation.Exception };
        var first = strategy.PushAsync(a, new PageContext(), CancellationToken.None);
        InvalidOperationException thrown = null;

        try
        {
            Assert.IsTrue(flow.IsInTransition);

            try
            {
                await strategy.PushAsync(probe.CreateAsync, probe.Release, context, CancellationToken.None);
            }
            catch (InvalidOperationException ex)
            {
                thrown = ex;
            }
        }
        finally
        {
            gate.Open();
            await first;
        }

        Assert.IsNotNull(thrown);
        Assert.AreEqual(expected: $"{flow.GetType()} is currently in transition.", actual: thrown.Message);
        CollectionAssert.AreEqual(new[] { b }, probe.Released);
        Assert.IsFalse(IsInFlow(flow, b));
        Assert.IsEmpty(b.Calls);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FA8_FactoryReturningAListedDuplicate_IsNotReleasedAsync(bool multi)
    {
        using var flowContext = new TestFlowContext { RejectDuplicatePages = true };
        var call = multi ? FactoryCall.ListMultiAdd : FactoryCall.ListSingleAdd;
        var flow = CreateFlow(call, flowContext);
        using var lifetime = (IDisposable)flow;
        var list = (IPageListStrategy<TestPage>)flow;
        var a = new TestPage("A");
        var probe = new Probe(a);

        Assert.IsTrue((await list.AddAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);

        var result = await CallAsync(call, flow, probe, new PageContext(), CancellationToken.None);

        AssertFailure(
              result
            , "The page 'A' is already in the flow; duplicate pages are rejected at the Flow level."
        );

        Assert.IsEmpty(probe.Released);
        Assert.AreEqual(expected: 1, actual: CountPages(flow));
        Assert.IsTrue(IsInFlow(flow, a));
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FA9_ThrowingReleaseOnAFailure_IsSuppressedUnderThePrimaryFailureAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateFlow(multi ? FactoryCall.StackMultiPush : FactoryCall.StackSinglePush, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageStackStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var probe = new Probe(b) { ThrowOnRelease = true };
        var first = strategy.PushAsync(a, new PageContext(), CancellationToken.None);
        Success<PageFlowError> firstResult = default;
        Success<PageFlowError> secondResult = default;

        try
        {
            Assert.IsTrue(flow.IsInTransition);

            secondResult = await strategy.PushAsync(
                  probe.CreateAsync
                , probe.Release
                , new PageContext()
                , CancellationToken.None
            );
        }
        finally
        {
            gate.Open();
            firstResult = await first;
        }

        Assert.IsTrue(firstResult.IsSuccess);
        AssertFailure(secondResult, $"{flow.GetType()} is currently in transition.");

        AssertSingleSuppressed(secondResult, "An exception occurred in the page flow: B.Release");

        CollectionAssert.AreEqual(new[] { b }, probe.Released);
        LogAssert.NoUnexpectedReceived();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FA10_ThrowingReleaseOnTheThrowPath_IsLoggedAndTheOriginalExceptionPropagatesAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateFlow(multi ? FactoryCall.StackMultiPush : FactoryCall.StackSinglePush, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageStackStrategy<TestPage>)flow;
        var gate = new TestGate();
        var a = new TestPage("A") { ShowGate = gate };
        var b = new TestPage("B");
        var probe = new Probe(b) { ThrowOnRelease = true };
        var context = new PageContext { AsyncOperation = PageAsyncOperation.Exception };
        var first = strategy.PushAsync(a, new PageContext(), CancellationToken.None);
        InvalidOperationException thrown = null;

        try
        {
            Assert.IsTrue(flow.IsInTransition);

            try
            {
                await strategy.PushAsync(probe.CreateAsync, probe.Release, context, CancellationToken.None);
            }
            catch (InvalidOperationException ex)
            {
                thrown = ex;
            }
        }
        finally
        {
            gate.Open();
            await first;
        }

        Assert.IsNotNull(thrown);
        Assert.AreEqual(expected: $"{flow.GetType()} is currently in transition.", actual: thrown.Message);
        CollectionAssert.AreEqual(new[] { b }, probe.Released);
        LogAssert.Expect(LogType.Exception, new Regex(@"B\.Release"));
        LogAssert.NoUnexpectedReceived();
    }

    private static async UnityTask AbortDuringTheShowTransitionAsync(bool multi)
    {
        using var flowContext = new TestFlowContext();
        var flow = CreateFlow(multi ? FactoryCall.StackMultiPush : FactoryCall.StackSinglePush, flowContext);
        using var lifetime = (IDisposable)flow;
        var strategy = (IPageStackStrategy<TestPage>)flow;
        var gate = new TestGate();
        var transition = new TestTransition { ShowGate = gate };
        var a = new TestPage("A");
        var b = new TestPage("B") { PageTransition = transition };
        var probe = new Probe(b);

        Assert.IsTrue((await strategy.PushAsync(a, new PageContext(), CancellationToken.None)).IsSuccess);

        var pending = strategy.PushAsync(probe.CreateAsync, probe.Release, new PageContext(), CancellationToken.None);
        Success<PageFlowError> result = default;

        try
        {
            await UnityTask.WaitUntil(
                  state: transition
                , predicate: static current => current.Value == 0.5f
                , timing: UnityTaskTiming.Update
                , token: CancellationToken.None
            );

            Assert.IsTrue(flow.AbortCurrent(PageAbortMode.Reverse).IsSuccess);
        }
        finally
        {
            gate.Open();
            result = await pending;
        }

        var error = result.GetFailureOrThrow();

        Assert.IsTrue(error.IsAborted);
        Assert.AreEqual(expected: PageTransitionState.Undone, actual: error.TransitionState);
        CollectionAssert.AreEqual(new[] { b }, probe.Released);
        Assert.IsFalse(IsInFlow(flow, b));
        Assert.IsTrue(IsInFlow(flow, a));
        Assert.AreEqual(expected: 1, actual: CountPages(flow));
        LogAssert.NoUnexpectedReceived();
    }

    private static IPageFlow CreateFlow(FactoryCall call, TestFlowContext context)
        => call switch {
            FactoryCall.StackSinglePush => new PageStackSingle<TestPage>(context),
            FactoryCall.StackMultiPush => new PageStackMulti<TestPage>(context),
            FactoryCall.ListSingleAdd or FactoryCall.ListSingleShow => new PageListSingle<TestPage>(context),
            _ => new PageListMulti<TestPage>(context),
        };

    private static UnityTask<Success<PageFlowError>> CallAsync(
          FactoryCall call
        , IPageFlow flow
        , Probe probe
        , PageContext context
        , CancellationToken token
    )
    {
        switch (call)
        {
            case FactoryCall.StackSinglePush:
            case FactoryCall.StackMultiPush:
            {
                return ((IPageStackStrategy<TestPage>)flow).PushAsync(probe.CreateAsync, probe.Release, context, token);
            }

            case FactoryCall.ListSingleAdd:
            case FactoryCall.ListMultiAdd:
            {
                return ((IPageListStrategy<TestPage>)flow).AddAsync(probe.CreateAsync, probe.Release, context, token);
            }

            case FactoryCall.ListSingleShow:
            case FactoryCall.ListMultiShow:
            {
                return ((IPageListStrategy<TestPage>)flow).ShowAsync(probe.CreateAsync, probe.Release, context, token);
            }

            default:
            {
                return ((IPageListMulti<TestPage>)flow).HideAsync(probe.CreateAsync, probe.Release, context, token);
            }
        }
    }

    private static int CountPages(IPageFlow flow)
    {
        if (flow is PageStackSingle<TestPage> single)
        {
            return single.CurrentPage.HasValue ? 1 : 0;
        }

        if (flow is PageStackMulti<TestPage> multi)
        {
            return multi.PageCollection.Count;
        }

        return ((IHasPages<TestPage>)flow).Pages.Count;
    }

    private static bool IsInFlow(IPageFlow flow, TestPage page)
    {
        if (flow is PageStackSingle<TestPage> single)
        {
            return single.CurrentPage.TryGetValue(out var current) && ReferenceEquals(current, page);
        }

        if (flow is PageStackMulti<TestPage> multi)
        {
            foreach (var item in multi.PageCollection)
            {
                if (ReferenceEquals(item, page))
                {
                    return true;
                }
            }

            return false;
        }

        var pages = ((IHasPages<TestPage>)flow).Pages;
        var count = pages.Count;

        for (var i = 0; i < count; i++)
        {
            if (ReferenceEquals(pages[i], page))
            {
                return true;
            }
        }

        return false;
    }

    private static void AssertSingleSuppressed(Success<PageFlowError> result, string message)
    {
        var suppressed = result.GetFailureOrThrow().SuppressedErrors;

        Assert.AreEqual(expected: 1, actual: suppressed.Length);
        Assert.AreEqual(expected: message, actual: suppressed[0].ToMessage());
    }

    private static void AssertFailure(Success<PageFlowError> result, string message)
    {
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(expected: message, actual: result.GetFailureOrThrow().ToMessage());
    }

    private sealed class Probe
    {
        private readonly Result<TestPage, PageFlowError> _result;

        public Probe(TestPage page)
        {
            _result = Result<TestPage, PageFlowError>.Succeed(page);
        }

        public Probe(PageFlowError error)
        {
            _result = Result<TestPage, PageFlowError>.Err(error);
        }

        public Action OnCreate { get; set; }

        public bool ThrowOnRelease { get; set; }

        public int FactoryCalls { get; private set; }

        public List<TestPage> Released { get; } = new();

        public UnityTask<Result<TestPage, PageFlowError>> CreateAsync(CancellationToken token)
        {
            FactoryCalls++;
            OnCreate?.Invoke();
            return UnityTask.FromResult(_result);
        }

        public void Release(TestPage page)
        {
            Released.Add(page);

            if (ThrowOnRelease)
            {
                ThrowHelper.ThrowHookException(page.Name, "Release");
            }
        }
    }
}
