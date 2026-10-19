using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.Logging;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.Types;

namespace EncosyTower.Tests.PageFlows;

internal sealed class FakeHistoryFlow : IPageFlowRecordable, IDisposable
{
    private static int s_serial;

    private readonly List<ISubscription> _subscriptions = new();

    private readonly ILogger _logger;

    public FakeHistoryFlow(
          [NotNull] TestFlowContext context
        , string flowId
        , PageFlowKind kind
        , params string[] listKeys
    )
    {
        FlowId = flowId;
        Kind = kind;
        ListKeys = listKeys;
        Subscriber = context.Subscriber;
        Publisher = context.Publisher;
        _logger = context.Logger;

        FlowScope = new PageFlowScope(new Id3(
              (Id<FakeHistoryFlow>)Type<FakeHistoryFlow>.Id
            , Interlocked.Increment(ref s_serial)
            , 0
        ));

        var subscriber = Subscriber.Scope(FlowScope).WithSubscriptions(_subscriptions).WithState(this);
        ShowPageMessage.Async.Subscribe(in subscriber, HandleAsync);
        HideActivePageMessage.Async.Subscribe(in subscriber, HandleAsync);
        ShowPageAtIndexMessage.Async.Subscribe(in subscriber, HandleAsync);
        HidePageAtIndexMessage.Async.Subscribe(in subscriber, HandleAsync);
    }

    public string FlowId { get; }

    public PageFlowKind Kind { get; }

    public PageFlowScope FlowScope { get; }

    public MessageSubscriber Subscriber { get; }

    public MessagePublisher Publisher { get; }

    public bool IsAlive { get; set; } = true;

    public bool IsShowingPages => Stack.Count > 0 || CurrentPageIndex >= 0 || ShownPageIndices.Count > 0;

    public List<string> Stack { get; } = new();

    public ReadOnlyMemory<string> ListKeys { get; }

    public int CurrentPageIndex { get; private set; } = -1;

    public List<int> ShownPageIndices { get; } = new();

    public List<string> Calls { get; } = new();

    public List<PageContext> Contexts { get; } = new();

    public AssetKey LastShownKey { get; private set; }

    public Queue<Success<PageFlowError>> NextResults { get; } = new();

    public TaskCompletionSource<bool> Gate { get; set; }

    private bool IsStack => Kind == PageFlowKind.PageStackSingle || Kind == PageFlowKind.PageStackMulti;

    public void Dispose()
        => _subscriptions.Unsubscribe();

    public async UnityTask<Success<PageFlowError>> ShowAsync(AssetKey assetKey, PageContext context = default)
    {
        LastShownKey = assetKey;

        await RecordCallAndWaitAsync($"Show:{assetKey}", context);

        var result = TakeNextResultOr(IsStack ? Success<PageFlowError>.Yes : PageFlowError.Undefined());

        if (result.IsApplied())
        {
            var from = Top();
            var to = new PageRef(Page: null, AssetKey: assetKey, Index: Option.None);

            if (Kind == PageFlowKind.PageStackSingle)
            {
                Stack.Clear();
                Stack.Add(assetKey.Value);
                await PublishAsync(PageNavigationKind.Show, from, to, context);
            }
            else
            {
                Stack.Add(assetKey.Value);
                await PublishAsync(PageNavigationKind.Push, from, to, context);
            }
        }

        context.Receipt?.Report(FlowId, result);
        return result;
    }

    public async UnityTask<Success<PageFlowError>> HideActiveAsync(PageContext context = default)
    {
        await RecordCallAndWaitAsync("HideActive", context);

        var result = TakeNextResultOr(CheckHideActive(context));

        if (result.IsApplied())
        {
            await ApplyHideActiveAsync(context);
        }

        context.Receipt?.Report(FlowId, result);
        return result;
    }

    public async UnityTask<Success<PageFlowError>> ShowAtAsync(int index, PageContext context = default)
    {
        await RecordCallAndWaitAsync($"ShowAt:{index}", context);

        var result = TakeNextResultOr(CheckIndex(index, IsStack == false));

        if (result.IsApplied() && (uint)index < (uint)ListKeys.Length)
        {
            var to = new PageRef(Page: null, AssetKey: ListKeys.Span[index], Index: index);

            if (Kind == PageFlowKind.PageListSingle)
            {
                var from = CurrentPageIndex >= 0
                    ? new PageRef(Page: null, AssetKey: ListKeys.Span[CurrentPageIndex], Index: CurrentPageIndex)
                    : PageRef.None;

                CurrentPageIndex = index;
                await PublishAsync(PageNavigationKind.ShowAt, from, to, context);
            }
            else
            {
                ShownPageIndices.Remove(index);
                ShownPageIndices.Add(index);
                await PublishAsync(PageNavigationKind.ShowAt, PageRef.None, to, context);
            }
        }

        context.Receipt?.Report(FlowId, result);
        return result;
    }

    public async UnityTask<Success<PageFlowError>> HideAtAsync(int index, PageContext context = default)
    {
        await RecordCallAndWaitAsync($"HideAt:{index}", context);

        var result = TakeNextResultOr(CheckIndex(index, Kind == PageFlowKind.PageListMulti));

        if (result.IsApplied() && (uint)index < (uint)ListKeys.Length)
        {
            ShownPageIndices.Remove(index);
            var from = new PageRef(Page: null, AssetKey: ListKeys.Span[index], Index: index);
            await PublishAsync(PageNavigationKind.HideAt, from, PageRef.None, context);
        }

        context.Receipt?.Report(FlowId, result);
        return result;
    }

    private async UnityTask RecordCallAndWaitAsync(string call, PageContext context)
    {
        Calls.Add(call);
        Contexts.Add(context);

        if (Gate != null)
        {
            await Gate.Task;
        }
    }

    private Success<PageFlowError> TakeNextResultOr(in Success<PageFlowError> fallback)
        => NextResults.Count > 0 ? NextResults.Dequeue() : fallback;

    private Success<PageFlowError> CheckHideActive(PageContext context)
    {
        if (IsStack)
        {
            return Stack.Count > 0 ? Success<PageFlowError>.Yes : PageFlowError.NoPage();
        }

        if (Kind != PageFlowKind.PageListSingle)
        {
            return PageFlowError.Undefined();
        }

        if (CurrentPageIndex < 0)
        {
            return PageFlowError.NoPage();
        }

        if (CurrentPageIndex == context.DefaultIndex)
        {
            return PageFlowError.CannotHideDefaultPage(CurrentPageIndex);
        }

        return Success<PageFlowError>.Yes;
    }

    private Success<PageFlowError> CheckIndex(int index, bool kindHasOperation)
    {
        if (kindHasOperation == false)
        {
            return PageFlowError.Undefined();
        }

        return (uint)index < (uint)ListKeys.Length
            ? Success<PageFlowError>.Yes
            : PageFlowError.IndexOutOfRange(index, ListKeys.Length);
    }

    private async UnityTask ApplyHideActiveAsync(PageContext context)
    {
        if (IsStack)
        {
            if (Stack.Count < 1)
            {
                return;
            }

            var from = Top();
            Stack.RemoveAt(Stack.Count - 1);

            var kind = Kind == PageFlowKind.PageStackSingle ? PageNavigationKind.Hide : PageNavigationKind.Pop;
            var to = Kind == PageFlowKind.PageStackSingle ? PageRef.None : Top();
            await PublishAsync(kind, from, to, context);
            return;
        }

        if (Kind != PageFlowKind.PageListSingle || CurrentPageIndex < 0)
        {
            return;
        }

        var hidden = CurrentPageIndex;
        var defaultIndex = context.DefaultIndex;
        var showsDefault = (uint)defaultIndex < (uint)ListKeys.Length && defaultIndex != hidden;
        CurrentPageIndex = showsDefault ? defaultIndex : -1;

        var hiddenRef = new PageRef(Page: null, AssetKey: ListKeys.Span[hidden], Index: hidden);

        var defaultRef = showsDefault
            ? new PageRef(Page: null, AssetKey: ListKeys.Span[defaultIndex], Index: defaultIndex)
            : PageRef.None;

        await PublishAsync(PageNavigationKind.HideAt, hiddenRef, defaultRef, context);
    }

    private PageRef Top()
        => Stack.Count > 0
            ? new PageRef(Page: null, AssetKey: Stack[^1], Index: Option.None)
            : PageRef.None;

    private async UnityTask PublishAsync(PageNavigationKind kind, PageRef from, PageRef to, PageContext context)
    {
        var navigation = new PageNavigation(FlowId, kind, from, to, context, context.IssuedBy);
        var message = new PageNavigatedMessage(navigation);
        var publisher = Publisher.Scope(FlowScope);

        var publishingContext = PublishingContext.Default(logger: _logger, token: CancellationToken.None);

        try
        {
            var published = await PageNavigatedMessage.Async.Publish(in publisher, message, publishingContext);

            if (published.TryGetFailure(out var publishError) && publishError.IsSubscriberNotFound == false)
            {
                publishError.Log(_logger);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogException(ex);
        }
    }

    private static UnityTask HandleAsync(
          FakeHistoryFlow flow
        , ShowPageMessage.Async msg
        , PublishingContext _
    )
    {
        var message = (ShowPageMessage)msg;
        return flow.ShowAsync(message.AssetKey, message.Context).AsUnityTask();
    }

    private static UnityTask HandleAsync(
          FakeHistoryFlow flow
        , HideActivePageMessage.Async msg
        , PublishingContext _
    )
    {
        var message = (HideActivePageMessage)msg;
        return flow.HideActiveAsync(message.Context).AsUnityTask();
    }

    private static UnityTask HandleAsync(
          FakeHistoryFlow flow
        , ShowPageAtIndexMessage.Async msg
        , PublishingContext _
    )
    {
        var message = (ShowPageAtIndexMessage)msg;
        return flow.ShowAtAsync(message.Index, message.Context).AsUnityTask();
    }

    private static UnityTask HandleAsync(
          FakeHistoryFlow flow
        , HidePageAtIndexMessage.Async msg
        , PublishingContext _
    )
    {
        var message = (HidePageAtIndexMessage)msg;
        return flow.HideAtAsync(message.Index, message.Context).AsUnityTask();
    }
}
