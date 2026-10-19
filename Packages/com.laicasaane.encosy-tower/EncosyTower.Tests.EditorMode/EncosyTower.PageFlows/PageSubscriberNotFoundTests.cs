using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.Flows;
using EncosyTower.PageFlows.History;
using EncosyTower.PageFlows.PubSub;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageSubscriberNotFoundTests
{
    private readonly List<IDisposable> _disposables = new();
    private TestFlowContext _historyContext;

    [SetUp]
    public void SetUp()
    {
        _historyContext = new TestFlowContext();
    }

    [TearDown]
    public void TearDown()
    {
        for (var i = _disposables.Count - 1; i >= 0; i--)
        {
            _disposables[i].Dispose();
        }

        _disposables.Clear();
        _historyContext.Dispose();
        _historyContext = null;
    }

    [Test]
    public async Task SettingOff_UnheardNotificationsSucceedAsync()
    {
        using var flowContext = new TestFlowContext(subscribe: false, warnNoSubscriber: false);
        using var stack = new PageStackSingle<TestPage>(flowContext);

        var push = await stack.PushAsync(new TestPage("A"), new PageContext(), CancellationToken.None);

        Assert.IsTrue(push.Result.IsSuccess);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task SettingOn_UnheardAttach_IsReturnedAndThePageStaysShownAsync()
    {
        using var flowContext = new TestFlowContext(subscribe: false, warnNoSubscriber: true);
        flowContext.SubscribeTransition();

        using var stack = new PageStackSingle<TestPage>(flowContext);
        var page = new TestPage("A");

        var push = await stack.PushAsync(page, new PageContext(), CancellationToken.None);

        AssertSoftOnly(push.Result, typeof(AttachPageMessage), suppressedCount: 0);
        Assert.IsTrue(stack.CurrentPage.TryGetValue(out var current));
        Assert.AreSame(page, current);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task SettingOn_TwoUnheard_FirstIsPrimaryAndTheRestIsSuppressedAsync()
    {
        using var flowContext = new TestFlowContext(subscribe: false, warnNoSubscriber: true);
        flowContext.SubscribeAttachAndDetach();

        using var stack = new PageStackSingle<TestPage>(flowContext);

        var push = await stack.PushAsync(new TestPage("A"), new PageContext(), CancellationToken.None);

        AssertSoftOnly(push.Result, typeof(PageTransitionBeganMessage), suppressedCount: 1);
        Assert.IsTrue(AllSuppressedAreSoft(push.Result));
        StringAssert.Contains(nameof(PageTransitionEndedMessage), GetSuppressedMessage(push.Result, index: 0));
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task SettingOn_UnheardDetach_IsReturnedAndThePageIsPoppedAsync()
    {
        using var flowContext = new TestFlowContext(subscribe: false, warnNoSubscriber: true);
        flowContext.SubscribeTransition();

        using var stack = new PageStackSingle<TestPage>(flowContext);

        var push = await stack.PushAsync(new TestPage("A"), new PageContext(), CancellationToken.None);
        Assert.IsTrue(push.Result.IsApplied());

        var pop = await stack.PopAsync(new PageContext(), CancellationToken.None);

        AssertSoftOnly(pop.Result, typeof(DetachPageMessage), suppressedCount: 0);
        Assert.IsTrue(pop.Popped.HasValue);
        Assert.IsFalse(stack.CurrentPage.HasValue);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task SettingOn_RealFailureStaysPrimaryAndTheUnheardAreSuppressedAsync()
    {
        using var flowContext = new TestFlowContext(subscribe: false, warnNoSubscriber: true);
        flowContext.SubscribeAttachAndDetach();

        using var stack = new PageStackSingle<TestPage>(flowContext);
        var page = new TestPage("A") { ShowBehaviour = PageHookBehaviour.Throw };

        var push = await stack.PushAsync(page, new PageContext(), CancellationToken.None);

        Assert.IsTrue(push.Result.IsFailure);
        Assert.IsFalse(push.Result.GetFailureOrDefault().IsSubscriberNotFound);
        Assert.IsTrue(GetSuppressedCount(push.Result) > 0);
        Assert.IsTrue(AllSuppressedAreSoft(push.Result));
    }

    [Test]
    public async Task SettingOn_PageStackMultiPush_ReturnsTheUnheardNotificationAsync()
    {
        using var flowContext = new TestFlowContext(subscribe: false, warnNoSubscriber: true);
        flowContext.SubscribeTransition();

        using var stack = new PageStackMulti<TestPage>(flowContext);

        var push = await stack.PushAsync(new TestPage("A"), new PageContext(), CancellationToken.None);

        AssertSoftOnly(push, typeof(AttachPageMessage), suppressedCount: 0);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task SettingOn_ListAddAndRemove_ReturnTheUnheardNotificationsAsync()
    {
        using var flowContext = new TestFlowContext(subscribe: false, warnNoSubscriber: true);
        using var list = new PageListMulti<TestPage>(flowContext);
        var page = new TestPage("A");

        var add = await list.AddAsync(page, new PageContext(), CancellationToken.None);

        AssertSoftOnly(add, typeof(AttachPageMessage), suppressedCount: 0);

        var remove = await list.RemoveAsync(page, new PageContext(), CancellationToken.None);

        AssertSoftOnly(remove, typeof(DetachPageMessage), suppressedCount: 0);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task SettingOn_ReplayMovesTheHistoryPastAnUnheardNotificationAsync()
    {
        var screen = Fake("Screen", PageFlowKind.PageStackSingle);
        var history = Track(new PageHistory());
        Assert.IsTrue(history.Attach(screen).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("A")).IsSuccess);
        Assert.IsTrue((await screen.ShowAsync("B")).IsSuccess);

        screen.NextResults.Enqueue(CreateSoftResult());

        var result = await history.GoBackAsync(CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { "A" }, screen.Stack);
        Assert.AreEqual(expected: 0, actual: history.CurrentIndex);
    }

    private static Success<PageFlowError> CreateSoftResult()
        => PageFlowError.SubscriberNotFound(typeof(PageNavigatedMessage))
            .WithTransitionState(PageTransitionState.Completed);

    private static void AssertSoftOnly(in Success<PageFlowError> result, Type messageType, int suppressedCount)
    {
        Assert.IsTrue(result.IsFailure);
        Assert.IsTrue(result.IsApplied());

        var error = result.GetFailureOrDefault();

        Assert.IsTrue(error.IsSubscriberNotFound);
        StringAssert.Contains(messageType.Name, error.ToMessage());
        Assert.AreEqual(expected: suppressedCount, actual: error.SuppressedErrors.Length);
    }

    private static int GetSuppressedCount(in Success<PageFlowError> result)
        => result.GetFailureOrDefault().SuppressedErrors.Length;

    private static string GetSuppressedMessage(in Success<PageFlowError> result, int index)
        => result.GetFailureOrDefault().SuppressedErrors[index].ToMessage();

    private static bool AllSuppressedAreSoft(in Success<PageFlowError> result)
    {
        var suppressed = result.GetFailureOrDefault().SuppressedErrors;
        var count = suppressed.Length;

        for (var i = 0; i < count; i++)
        {
            if (suppressed[i].IsSubscriberNotFound == false)
            {
                return false;
            }
        }

        return true;
    }

    private FakeHistoryFlow Fake(string flowId, PageFlowKind kind)
        => Track(new FakeHistoryFlow(_historyContext, flowId, kind));

    private T Track<T>(T disposable)
        where T : IDisposable
    {
        _disposables.Add(disposable);
        return disposable;
    }
}
