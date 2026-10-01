#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;
using EncosyTower.UnityExtensions;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub
{
    public static partial class MessageSubscriberExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MessageSubscriber.Subscriber<TScope> WithSubscriptions<TScope>(
              this in MessageSubscriber.Subscriber<TScope> subscriber
            , ICollection<ISubscription> subscriptions
        )
        {
            var resolvedSubscriptions = subscriptions ?? EmptySubscriptions.Default;

            if (subscriber.IsCreated == false)
            {
                return new MessageSubscriber.Subscriber<TScope>(
                      subscriber._subscriber
                    , subscriber.Scope
                    , resolvedSubscriptions
                    , invalid: true
                );
            }

            return new MessageSubscriber.Subscriber<TScope>(
                  subscriber._subscriber
                , subscriber.Scope
                , resolvedSubscriptions
            );
        }
    }

    partial class MessageSubscriber
    {
        public readonly partial struct Subscriber<TScope> : IIsCreated
        {
            internal readonly MessageSubscriber _subscriber;
            internal readonly ICollection<ISubscription> _subscriptions;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Subscriber([NotNull] MessageSubscriber subscriber, [NotNull] TScope scope)
            {
                DebuggingThrowHelper.ThrowIfNull(subscriber);
                DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(scope);

                _subscriber = subscriber;
                _subscriptions = EmptySubscriptions.Default;
                Scope = scope;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Subscriber(
                  [NotNull] MessageSubscriber subscriber
                , [NotNull] TScope scope
                , [NotNull] ICollection<ISubscription> subscriptions
            )
            {
                DebuggingThrowHelper.ThrowIfNull(subscriber);
                DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(scope);
                DebuggingThrowHelper.ThrowIfNull(subscriptions);

                _subscriber = subscriber;
                _subscriptions = subscriptions;
                Scope = scope;
            }

            public bool IsCreated => _subscriber != null;

            public TScope Scope { get; }

            public ICollection<ISubscription> Subscriptions => _subscriptions ?? EmptySubscriptions.Default;

            public MessageInterceptors Interceptors => _subscriber?.Interceptors ?? default;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Subscriber<TNewScope> WithScope<TNewScope>()
                where TNewScope : struct
            {
                if (IsCreated == false)
                {
                    return new Subscriber<TNewScope>(_subscriber, default, Subscriptions, invalid: true);
                }

                return _subscriber.Scope<TNewScope>().WithSubscriptions(Subscriptions);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Subscriber<TNewScope> WithScope<TNewScope>([NotNull] TNewScope scope)
            {
                DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(scope);

                if (IsCreated == false)
                {
                    return new Subscriber<TNewScope>(_subscriber, scope, Subscriptions, invalid: true);
                }

                return _subscriber.Scope(scope).WithSubscriptions(Subscriptions);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Subscriber<GlobalScope> WithGlobalScope()
            {
                if (IsCreated == false)
                {
                    return new Subscriber<GlobalScope>(_subscriber, default, Subscriptions, invalid: true);
                }

                return _subscriber.Global().WithSubscriptions(Subscriptions);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnitySubscriber<TNewScope> WithUnityScope<TNewScope>([NotNull] TNewScope scope)
                where TNewScope : UnityEngine.Object
            {
                DebuggingThrowHelper.ThrowIfUnityObjectInvalid(scope);

                if (IsCreated == false)
                {
                    var subscriber = new Subscriber<UnityEntityId<TNewScope>>(
                          _subscriber
                        , scope
                        , Subscriptions
                        , invalid: true
                    );

                    return new UnitySubscriber<TNewScope>(subscriber);
                }

                return _subscriber.UnityScope(scope).WithSubscriptions(Subscriptions);
            }

            internal Subscriber(
                  MessageSubscriber subscriber
                , TScope scope
                , ICollection<ISubscription> subscriptions
                , bool invalid
            )
            {
                _subscriber = subscriber;
                _subscriptions = subscriptions;
                Scope = scope;
            }

            /// <summary>
            /// Remove empty handler groups to optimize performance.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Compress<TMessage>(ILogger logger = null)
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Compress<TScope, TMessage>(Scope, logger);
            }

            public void Clear(ILogger logger = null)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Clear(Scope);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                TrySubscribe(new HandlerAction<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action<TMessage> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                TrySubscribe(new HandlerActionMessage<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Subscribe<TMessage>(
                  [NotNull] Action handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                if (TrySubscribe(new HandlerAction<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Subscribe<TMessage>(
                  [NotNull] Action<TMessage> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                if (TrySubscribe(new HandlerActionMessage<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action<PublishingContext> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                TrySubscribe(new ContextualHandlerAction<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action<TMessage, PublishingContext> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                TrySubscribe(
                      new ContextualHandlerActionMessage<TMessage>(handler)
                    , order
                    , out var subscription
                    , logger
                );
                return subscription;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Subscribe<TMessage>(
                  [NotNull] Action<PublishingContext> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                if (TrySubscribe(new ContextualHandlerAction<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Subscribe<TMessage>(
                  [NotNull] Action<TMessage, PublishingContext> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                if (TrySubscribe(
                          new ContextualHandlerActionMessage<TMessage>(handler)
                        , order
                        , out var subscription
                        , logger
                    )
                )
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            internal bool TrySubscribe<TMessage>(
                  IHandler<TMessage> handler
                , int order
                , out Subscription<TMessage> subscription
                , ILogger logger
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    subscription = Subscription<TMessage>.None;
                    return false;
                }
#endif

                if (_subscriber.TrySubscribe(handler, order, Scope, out subscription, logger))
                {
                    Subscriptions.Add(subscription);
                    return true;
                }

                return false;
            }

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (IsCreated)
                {
                    return true;
                }

                ThrowHelper.LogErrorInvalidSubscriberAccess(GetType(), logger);

                return false;
            }
#endif
        }
    }
}
