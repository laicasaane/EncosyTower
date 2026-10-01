using EncosyTower.CodeGen;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using UnityEngine;

namespace Samples.PubSub
{
    [PubSub(ApiMode.Sync, State = StateMode.Stateful)]
    public readonly partial record struct RefreshMessage;

    [PubSub(ApiMode.Async, State = StateMode.Stateful)]
    public readonly partial record struct LoadMessage;

    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(OrderScope))]
    public readonly partial record struct SubmitOrderMessage;

    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(SessionScope))]
    public readonly partial record struct RefreshSessionMessage;

    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(OrderScope))]
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(SessionScope))]
    public readonly partial record struct PageChangedMessage;

    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageView))]
    public readonly partial record struct RefreshViewMessage;

    public readonly struct OrderScope
    {
    }

    public sealed class SessionScope
    {
    }

    public sealed class PageView : ScriptableObject
    {
    }

    public sealed class PubSubSamples
    {
        public void Use(
              OrderScope orderScope
            , SessionScope sessionScope
            , Messenger customMessenger
            , PageView pageView
        )
        {
            var orderPublisher = customMessenger.Publisher.Scope(orderScope);
            var orderSubscriber = customMessenger.Subscriber.Scope(orderScope);
            var statefulOrderSubscriber = orderSubscriber.WithState(this);
            var sessionPublisher = customMessenger.Publisher.Scope(sessionScope);
            var sessionSubscriber = customMessenger.Subscriber.Scope(sessionScope);
            var statefulSessionSubscriber = sessionSubscriber.WithState(this);

            PageChangedMessage.Subscribe(in statefulOrderSubscriber, HandleOrderPageChanged);
            PageChangedMessage.Publish(in orderPublisher, new PageChangedMessage());

            SubmitOrderMessage.Subscribe(in statefulOrderSubscriber, HandleOrderSubmitted);
            _ = SubmitOrderMessage.Cache(in orderPublisher, static () => new SubmitOrderMessage());
            RefreshSessionMessage.Subscribe(in statefulSessionSubscriber, HandleSessionRefresh);

            RefreshMessage.Subscribe(this, HandleRefresh);
            RefreshMessage.Publish(new RefreshMessage());

            var customGlobalPublisher = customMessenger.Publisher.Global();
            var customGlobalSubscriber = customMessenger.Subscriber.Global();
            var statefulCustomGlobalSubscriber = customGlobalSubscriber.WithState(this);
            RefreshMessage.Subscribe(in statefulCustomGlobalSubscriber, HandleRefresh);
            RefreshMessage.Publish(in customGlobalPublisher, new RefreshMessage());

            var viewPublisher = customMessenger.Publisher.UnityScope(pageView);
            var viewSubscriber = customMessenger.Subscriber.UnityScope(pageView);
            var statefulViewSubscriber = viewSubscriber.WithState(this);
            RefreshViewMessage.Subscribe(in statefulViewSubscriber, HandleViewRefresh);
            RefreshViewMessage.Publish(in viewPublisher, new RefreshViewMessage());

            LoadMessage.Async.Subscribe(this, HandleLoadAsync);
            _ = LoadMessage.Async.Publish();

            PageChangedMessage.Async.Subscribe(in statefulSessionSubscriber, HandlePageChangedAsync);
            _ = PageChangedMessage.Async.Publish(in sessionPublisher, new PageChangedMessage.Async());
        }

        private static void HandleRefresh(PubSubSamples state, RefreshMessage message)
        {
        }

        private static void HandleOrderSubmitted(PubSubSamples state, SubmitOrderMessage message)
        {
        }

        private static void HandleOrderPageChanged(
              PubSubSamples state
            , PageChangedMessage message
            , PublishingContext context
        )
        {
        }

        private static void HandleSessionRefresh(PubSubSamples state, RefreshSessionMessage message)
        {
        }

        private static void HandleViewRefresh(PubSubSamples state, RefreshViewMessage message)
        {
        }

        private static UnityTask HandleLoadAsync(
              PubSubSamples state
            , LoadMessage.Async message
            , PublishingContext context
        )
            => default;

        private static UnityTask HandlePageChangedAsync(
              PubSubSamples state
            , PageChangedMessage.Async message
            , PublishingContext context
        )
            => default;
    }
}
