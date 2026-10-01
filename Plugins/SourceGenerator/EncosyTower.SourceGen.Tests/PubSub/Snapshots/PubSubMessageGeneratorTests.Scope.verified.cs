#pragma warning disable 0219

using EncosyTower.PubSub;
using g__S = global::System;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__ST = global::System.Threading;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ET = global::EncosyTower.Common;
using g__ETL = global::EncosyTower.Logging;
using g__ETPS = global::EncosyTower.PubSub;
using g__ETUE = global::EncosyTower.UnityExtensions;
using g__ETT = global::EncosyTower.Tasks;

namespace TestProject
{


partial class Message
{
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.CachedPublisher<g__ET.GlobalScope, global::TestProject.Message> Cache(in g__ETPS.MessagePublisher.Publisher<g__ET.GlobalScope> publisher, [g__SDCA.NotNull] g__S.Func<global::TestProject.Message> factory, g__ETL.ILogger logger = null)
    {
        return publisher.Cache(factory, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Publish(in g__ETPS.MessagePublisher.Publisher<g__ET.GlobalScope> publisher, g__ETPS.PublishingContext context = default)
    {
        publisher.Publish<global::TestProject.Message>(context);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Publish(in g__ETPS.MessagePublisher.Publisher<g__ET.GlobalScope> publisher, global::TestProject.Message message, g__ETPS.PublishingContext context = default)
    {
        publisher.Publish(message, context);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Action handler, int order = 0, g__ETL.ILogger logger = null)
    {
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Action handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
    {
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Action<global::TestProject.Message> handler, int order = 0, g__ETL.ILogger logger = null)
    {
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Action<global::TestProject.Message> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
    {
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Action<g__ETPS.PublishingContext> handler, int order = 0, g__ETL.ILogger logger = null)
    {
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Action<g__ETPS.PublishingContext> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
    {
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Action<global::TestProject.Message, g__ETPS.PublishingContext> handler, int order = 0, g__ETL.ILogger logger = null)
    {
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Action<global::TestProject.Message, g__ETPS.PublishingContext> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
    {
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Action<TState> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Action<TState> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Action<TState, g__ETPS.PublishingContext> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Action<TState, g__ETPS.PublishingContext> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message, g__ETPS.PublishingContext> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message, g__ETPS.PublishingContext> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        return statefulSubscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        statefulSubscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        return statefulSubscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        statefulSubscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, g__ETPS.PublishingContext> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        return statefulSubscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, g__ETPS.PublishingContext> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        statefulSubscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message, g__ETPS.PublishingContext> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        return statefulSubscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message, g__ETPS.PublishingContext> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        statefulSubscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }


    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.CachedPublisher<g__ET.GlobalScope, global::TestProject.Message> Cache([g__SDCA.NotNull] g__S.Func<global::TestProject.Message> factory, g__ETL.ILogger logger = null)
    {
        var publisher = g__ETPS.GlobalMessenger.Publisher.Global();
        return publisher.Cache(factory, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Publish(g__ETPS.PublishingContext context = default)
    {
        var publisher = g__ETPS.GlobalMessenger.Publisher.Global();
        publisher.Publish<global::TestProject.Message>(context);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Publish(global::TestProject.Message message, g__ETPS.PublishingContext context = default)
    {
        var publisher = g__ETPS.GlobalMessenger.Publisher.Global();
        publisher.Publish(message, context);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe([g__SDCA.NotNull] g__S.Action handler, int order = 0, g__ETL.ILogger logger = null)
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe([g__SDCA.NotNull] g__S.Action handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe([g__SDCA.NotNull] g__S.Action<global::TestProject.Message> handler, int order = 0, g__ETL.ILogger logger = null)
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe([g__SDCA.NotNull] g__S.Action<global::TestProject.Message> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe([g__SDCA.NotNull] g__S.Action<g__ETPS.PublishingContext> handler, int order = 0, g__ETL.ILogger logger = null)
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe([g__SDCA.NotNull] g__S.Action<g__ETPS.PublishingContext> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe([g__SDCA.NotNull] g__S.Action<global::TestProject.Message, g__ETPS.PublishingContext> handler, int order = 0, g__ETL.ILogger logger = null)
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        return subscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe([g__SDCA.NotNull] g__S.Action<global::TestProject.Message, g__ETPS.PublishingContext> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        subscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        return statefulSubscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        statefulSubscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        return statefulSubscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        statefulSubscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, g__ETPS.PublishingContext> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        return statefulSubscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, g__ETPS.PublishingContext> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        statefulSubscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__ETPS.ISubscription Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message, g__ETPS.PublishingContext> handler, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        return statefulSubscriber.Subscribe<global::TestProject.Message>(handler, order, logger);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static void Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Action<TState, global::TestProject.Message, g__ETPS.PublishingContext> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        where TState : class
    {
        var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
        var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
        statefulSubscriber.Subscribe<global::TestProject.Message>(handler, unsubscribeToken, order, logger);
    }


    public readonly partial struct Async
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETT.UnityTask Publish(in g__ETPS.MessagePublisher.Publisher<g__ET.GlobalScope> publisher, g__ETPS.PublishingContext context = default)
        {
            return publisher.PublishAsync(new Async(new global::TestProject.Message()), context);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETT.UnityTask Publish(in g__ETPS.MessagePublisher.Publisher<g__ET.GlobalScope> publisher, Async message, g__ETPS.PublishingContext context = default)
        {
            return publisher.PublishAsync(message, context);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Func<g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
        {
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Func<g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        {
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Func<Async, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
        {
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Func<Async, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        {
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Func<g__ETPS.PublishingContext, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
        {
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Func<g__ETPS.PublishingContext, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        {
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Func<Async, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
        {
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] g__S.Func<Async, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        {
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Func<TState, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Func<TState, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Func<TState, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Func<TState, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope, TState> subscriber, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            return statefulSubscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            statefulSubscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            return statefulSubscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            statefulSubscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            return statefulSubscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            statefulSubscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            return statefulSubscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>(in g__ETPS.MessageSubscriber.Subscriber<g__ET.GlobalScope> subscriber, [g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            statefulSubscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }


        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETT.UnityTask Publish(g__ETPS.PublishingContext context = default)
        {
            var publisher = g__ETPS.GlobalMessenger.Publisher.Global();
            return publisher.PublishAsync(new Async(new global::TestProject.Message()), context);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETT.UnityTask Publish(Async message, g__ETPS.PublishingContext context = default)
        {
            var publisher = g__ETPS.GlobalMessenger.Publisher.Global();
            return publisher.PublishAsync(message, context);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe([g__SDCA.NotNull] g__S.Func<g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe([g__SDCA.NotNull] g__S.Func<g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe([g__SDCA.NotNull] g__S.Func<Async, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe([g__SDCA.NotNull] g__S.Func<Async, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe([g__SDCA.NotNull] g__S.Func<g__ETPS.PublishingContext, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe([g__SDCA.NotNull] g__S.Func<g__ETPS.PublishingContext, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe([g__SDCA.NotNull] g__S.Func<Async, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            return subscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe([g__SDCA.NotNull] g__S.Func<Async, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            subscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            return statefulSubscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            statefulSubscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            return statefulSubscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            statefulSubscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            return statefulSubscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            statefulSubscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ETPS.ISubscription Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            return statefulSubscriber.Subscribe<Async>(handler, order, logger);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<TState>([g__SDCA.NotNull] TState state, [g__SDCA.NotNull] g__S.Func<TState, Async, g__ETPS.PublishingContext, g__ETT.UnityTask> handler, g__ST.CancellationToken unsubscribeToken, int order = 0, g__ETL.ILogger logger = null)
            where TState : class
        {
            var subscriber = g__ETPS.GlobalMessenger.Subscriber.Global();
            var statefulSubscriber = g__ETPS.MessageSubscriberExtensions.WithState(in subscriber, state);
            statefulSubscriber.Subscribe<Async>(handler, unsubscribeToken, order, logger);
        }

    }
}


}
