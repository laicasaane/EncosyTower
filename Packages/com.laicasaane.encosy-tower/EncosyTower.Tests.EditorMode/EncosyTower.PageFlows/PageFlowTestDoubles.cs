using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.Logging;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.Types;

namespace EncosyTower.Tests.PageFlows;

internal sealed class TestFlowContext : IPageFlowContext, IDisposable
{
    private static readonly PageFlowScope s_flowScope = new(new Id3(
          x: (Id<TestFlowContext>)Type<TestFlowContext>.Id
        , y: 0
        , z: 0
    ));

    private readonly Messenger _messenger = new(ArrayPool<UnityTask>.Shared);

    public TestFlowContext(bool subscribe = true, bool warnNoSubscriber = false)
    {
        WarnNoSubscriber = warnNoSubscriber;

        if (subscribe == false)
        {
            return;
        }

        SubscribeAttachAndDetach();
        SubscribeTransition();
    }

    public MessageSubscriber Subscriber => _messenger.Subscriber;

    public MessagePublisher Publisher => _messenger.Publisher;

    public PageFlowScope FlowScope => s_flowScope;

    public Option<IPageFlowScopeCollectionApplier> FlowScopeCollectionApplier => default;

    public Option<IPageOwnershipTracker> OwnershipTracker { get; set; }

    public bool WarnNoSubscriber { get; set; }

    public bool RejectDuplicatePages { get; set; }

    public ILogger Logger => DevLogger.Default;

    public int AttachCount { get; private set; }

    public int DetachCount { get; private set; }

    public int BeganCount { get; private set; }

    public int EndedCount { get; private set; }

    public List<string> TransitionLog { get; } = new();

    public IPageFlow LastTransitionFlow { get; private set; }

    public void SubscribeAttachAndDetach()
    {
        var subscriber = Subscriber.Scope(FlowScope).WithState(this);
        AttachPageMessage.Async.Subscribe(in subscriber, HandleAsync);
        DetachPageMessage.Async.Subscribe(in subscriber, HandleAsync);
    }

    public void SubscribeTransition()
    {
        var subscriber = Subscriber.Scope(FlowScope).WithState(this);
        PageTransitionBeganMessage.Async.Subscribe(in subscriber, HandleAsync);
        PageTransitionEndedMessage.Async.Subscribe(in subscriber, HandleAsync);
    }

    private static UnityTask HandleAsync(
          TestFlowContext owner
        , AttachPageMessage.Async message
        , PublishingContext context
    )
    {
        owner.AttachCount++;
        return UnityTask.CompletedTask;
    }

    private static UnityTask HandleAsync(
          TestFlowContext owner
        , DetachPageMessage.Async message
        , PublishingContext context
    )
    {
        owner.DetachCount++;
        return UnityTask.CompletedTask;
    }

    private static UnityTask HandleAsync(
          TestFlowContext owner
        , PageTransitionBeganMessage.Async message
        , PublishingContext context
    )
    {
        var began = (PageTransitionBeganMessage)message;

        owner.BeganCount++;
        owner.LastTransitionFlow = began.Flow;
        owner.TransitionLog.Add($"Began {began.Page} {began.Transition} {GetName(began.OtherPage)}");
        return UnityTask.CompletedTask;
    }

    private static UnityTask HandleAsync(
          TestFlowContext owner
        , PageTransitionEndedMessage.Async message
        , PublishingContext context
    )
    {
        var ended = (PageTransitionEndedMessage)message;

        owner.EndedCount++;
        owner.LastTransitionFlow = ended.Flow;

        owner.TransitionLog.Add(
            $"Ended {ended.Page} {ended.Transition} {GetName(ended.OtherPage)} {ended.TransitionState}"
        );

        return UnityTask.CompletedTask;
    }

    private static string GetName(Option<IPage> page)
        => page.TryGetValue(out var value) ? value.ToString() : "-";

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messenger.Dispose();
        }
    }
}

internal sealed class TestFlowBackend : IPageFlowBackend
{
    public List<IPage> ActivatedPages { get; } = new();

    public void ActivatePageRendering(IPage page)
    {
        ActivatedPages.Add(page);

        if (page is TestPage testPage)
        {
            testPage.Calls.Add("ActivatePageRendering");
        }
    }
}

internal sealed class TestPage
    : IPageOnAttachToFlowAsync
    , IPageOnDetachFromFlowAsync
    , IPageOnBeforeShowAsync
    , IPageOnShowAsync
    , IPageOnHideAsync
    , IPageOnAfterShow
    , IPageOnAfterHide
    , IPageNeedsFlowScope
    , IPageHasTransition
    , IPageHasOptions
{
    private const string ATTACH = "Attach";
    private const string DETACH = "Detach";
    private const string BEFORE_SHOW = "BeforeShow";
    private const string SHOW = "Show";
    private const string HIDE = "Hide";
    private const string AFTER_SHOW = "AfterShow";
    private const string AFTER_HIDE = "AfterHide";

    private readonly Dictionary<string, CancellationTokenSource> _cancelAfter = new(StringComparer.Ordinal);

    public TestPage(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public List<string> Calls { get; } = new();

    public IPageTransition PageTransition { get; set; }

    public PageOptions PageOptions { get; set; }

    public Action<string, PageContext> HookAction { get; set; }

    public PageHookBehaviour AttachBehaviour { get; set; }

    public TestGate AttachGate { get; set; }

    public PageHookBehaviour DetachBehaviour { get; set; }

    public TestGate DetachGate { get; set; }

    public PageHookBehaviour BeforeShowBehaviour { get; set; }

    public PageHookBehaviour ShowBehaviour { get; set; }

    public TestGate ShowGate { get; set; }

    public PageHookBehaviour HideBehaviour { get; set; }

    public TestGate HideGate { get; set; }

    public PageHookBehaviour AfterShowBehaviour { get; set; }

    public PageHookBehaviour AfterHideBehaviour { get; set; }

    public bool HasFlowScope { get; private set; }

    public PageFlowScope FlowScope
    {
        set => HasFlowScope = value != default;
    }

    public void CancelOn(string hook, CancellationTokenSource source)
    {
        _cancelAfter[hook] = source;
    }

    public UnityTask<bool> OnAttachToFlowAsync(IPageFlow flow, PageContext context, CancellationToken token)
        => AttachGate == null
            ? UnityTask.FromResult(InvokeHook(ATTACH, AttachBehaviour, context))
            : RunGatedHookAsync(AttachGate, ATTACH, AttachBehaviour, context, token);

    public UnityTask<bool> OnDetachFromFlowAsync(IPageFlow flow, PageContext context, CancellationToken token)
        => DetachGate == null
            ? UnityTask.FromResult(InvokeHook(DETACH, DetachBehaviour, context))
            : RunGatedHookAsync(DetachGate, DETACH, DetachBehaviour, context, token);

    public UnityTask OnBeforeShowAsync(PageContext context, CancellationToken token)
    {
        InvokeHook(BEFORE_SHOW, BeforeShowBehaviour, context);
        return UnityTask.CompletedTask;
    }

    public async UnityTask OnShowAsync(PageContext context, CancellationToken token)
    {
        if (ShowGate != null)
        {
            await ShowGate.WaitAsync(token);
        }

        InvokeHook(SHOW, ShowBehaviour, context);
    }

    public UnityTask OnHideAsync(PageContext context, CancellationToken token)
    {
        InvokeHook(HIDE, HideBehaviour, context);
        return HideGate == null ? UnityTask.CompletedTask : HideGate.WaitAsync(token);
    }

    public void OnAfterShow(PageContext context)
    {
        InvokeHook(AFTER_SHOW, AfterShowBehaviour, context);
    }

    public void OnAfterHide(PageContext context)
    {
        InvokeHook(AFTER_HIDE, AfterHideBehaviour, context);
    }

    public override string ToString()
        => Name;

    private async UnityTask<bool> RunGatedHookAsync(
          TestGate gate
        , string hook
        , PageHookBehaviour behaviour
        , PageContext context
        , CancellationToken token
    )
    {
        await gate.WaitAsync(token);
        return InvokeHook(hook, behaviour, context);
    }

    private bool InvokeHook(string hook, PageHookBehaviour behaviour, PageContext context)
    {
        Calls.Add(hook);
        HookAction?.Invoke(hook, context);

        if (_cancelAfter.TryGetValue(hook, out var source))
        {
            source.Cancel();
        }

        switch (behaviour)
        {
            case PageHookBehaviour.Throw:
            {
                ThrowHelper.ThrowHookException(Name, hook);
                break;
            }

            case PageHookBehaviour.ThrowCancelled:
            {
                ThrowHelper.ThrowHookCancelledException();
                break;
            }
        }

        return behaviour != PageHookBehaviour.Reject;
    }
}

internal enum PageHookBehaviour
{
    Succeed,
    Reject,
    Throw,
    ThrowCancelled,
}

internal static class ThrowHelper
{
    [UnityEngine.HideInCallstack, StackTraceHidden, DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
    internal static void ThrowHookException(string name, string hook)
        => throw new InvalidOperationException($"{name}.{hook}");

    [UnityEngine.HideInCallstack, StackTraceHidden, DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
    internal static void ThrowHookCancelledException()
        => throw new OperationCanceledException();
}

internal sealed class TestGate
{
    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;
    }

    public UnityTask WaitAsync(CancellationToken token)
        => UnityTask.WaitUntil(() => IsOpen, token);
}

public sealed class TestTransition : IPageTransition
{
    private const string SHOW = "Show";
    private const string HIDE = "Hide";

    public bool ForceRunShow { get; set; }

    public bool ForceRunHide { get; set; }

    public List<string> Calls { get; } = new();

    public float Value { get; set; }

    public float ShowStartValue { get; private set; }

    public float HideStartValue { get; private set; }

    public PageTransitionContext LastShowContext { get; private set; }

    public PageTransitionContext LastHideContext { get; private set; }

    public Action<string, PageTransitionContext> HookAction { get; set; }

    internal TestGate ShowGate { get; set; }

    internal TestGate HideGate { get; set; }

    public UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token)
    {
        var hook = $"Before{context.Transition}";
        Calls.Add(hook);
        HookAction?.Invoke(hook, context);
        return UnityTask.CompletedTask;
    }

    public void OnAfterTransition(PageTransitionContext context)
    {
        var hook = $"After{context.Transition}";
        Calls.Add(hook);
        HookAction?.Invoke(hook, context);
    }

    public async UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
    {
        ShowStartValue = Value;
        LastShowContext = context;
        Calls.Add(SHOW);
        HookAction?.Invoke(SHOW, context);

        if (ShowGate != null)
        {
            Value = 0.5f;
            await ShowGate.WaitAsync(token);
        }

        Value = 1f;
    }

    public async UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token)
    {
        HideStartValue = Value;
        LastHideContext = context;
        Calls.Add(HIDE);
        HookAction?.Invoke(HIDE, context);

        if (HideGate != null)
        {
            Value = 0.5f;
            await HideGate.WaitAsync(token);
        }

        Value = 0f;
    }
}
