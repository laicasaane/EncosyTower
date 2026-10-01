using EncosyTower.CodeGen;
using EncosyTower.Processing;
using EncosyTower.Tasks;
using UnityEngine;

namespace Samples.Processing
{
    [Processing(ApiMode.Sync, State = StateMode.Stateful)]
    public readonly partial record struct RefreshRequest;

    [Processing(ApiMode.Async, State = StateMode.Stateful)]
    public readonly partial record struct GetCountRequest : IRequest<int>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(OrderScope))]
    public readonly partial record struct SubmitOrderRequest;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(SessionScope))]
    public readonly partial record struct RefreshSessionRequest;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(OrderScope))]
    [Processing(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(SessionScope))]
    public readonly partial record struct GetPageRequest : IRequest<int>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageView))]
    public readonly partial record struct RefreshViewRequest;

    public readonly struct OrderScope
    {
    }

    public sealed class SessionScope
    {
    }

    public sealed class PageView : ScriptableObject
    {
    }

    public sealed class ProcessingSamples
    {
        public void Use(
              OrderScope orderScope
            , SessionScope sessionScope
            , Processor customProcessor
            , PageView pageView
        )
        {
            var orderHub = GlobalProcessor.Instance.Scope(orderScope);
            var statefulOrderHub = orderHub.WithState(this);
            var sessionHub = customProcessor.Scope(sessionScope);
            var statefulSessionHub = sessionHub.WithState(this);

            GetPageRequest.Register(in statefulOrderHub, ProcessOrder);
            GetPageRequest.Register(in orderHub, this, ProcessOrder);
            _ = GetPageRequest.Process(in orderHub, new GetPageRequest());

            SubmitOrderRequest.Register(in statefulOrderHub, ProcessOrder);
            RefreshSessionRequest.Register(in statefulSessionHub, ProcessSession);

            RefreshRequest.Register(this, ProcessRefresh);
            RefreshRequest.Process(new RefreshRequest());

            var customGlobalHub = customProcessor.Global();
            var statefulCustomGlobalHub = customGlobalHub.WithState(this);
            RefreshRequest.Register(in statefulCustomGlobalHub, ProcessRefresh);

            var viewHub = customProcessor.UnityScope(pageView);
            var statefulViewHub = viewHub.WithState(this);
            RefreshViewRequest.Register(in statefulViewHub, ProcessView);

            GetPageRequest.Async.Register(in statefulSessionHub, ProcessPageAsync);
            _ = GetPageRequest.Async.Process(in sessionHub, new GetPageRequest());
            _ = GetPageRequest.Async.TryProcess(in sessionHub, new GetPageRequest());
        }

        private static void ProcessRefresh(ProcessingSamples state, RefreshRequest request)
        {
        }

        private static void ProcessOrder(ProcessingSamples state, SubmitOrderRequest request)
        {
        }

        private static int ProcessOrder(
              ProcessingSamples state
            , GetPageRequest request
            , ProcessingContext context
        )
            => default;

        private static void ProcessSession(ProcessingSamples state, RefreshSessionRequest request)
        {
        }

        private static void ProcessView(ProcessingSamples state, RefreshViewRequest request)
        {
        }

        private static UnityTask<int> ProcessPageAsync(
              ProcessingSamples state
            , GetPageRequest.Async request
            , ProcessingContext context
        )
            => default;
    }
}
