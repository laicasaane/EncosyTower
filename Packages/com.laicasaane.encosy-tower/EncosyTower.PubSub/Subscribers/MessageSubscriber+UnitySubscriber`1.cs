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
        public static MessageSubscriber.UnitySubscriber<TScope> WithSubscriptions<TScope>(
              this in MessageSubscriber.UnitySubscriber<TScope> subscriber
            , ICollection<ISubscription> subscriptions
        )
            where TScope : UnityEngine.Object
        {
            return new MessageSubscriber.UnitySubscriber<TScope>(
                subscriber._subscriber.WithSubscriptions(subscriptions)
            );
        }
    }

    partial class MessageSubscriber
    {
        public readonly partial struct UnitySubscriber<TScope> : IIsCreated
            where TScope : UnityEngine.Object
        {
            internal readonly Subscriber<UnityEntityId<TScope>> _subscriber;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnitySubscriber([NotNull] MessageSubscriber subscriber, [NotNull] TScope scope)
            {
                DebuggingThrowHelper.ThrowIfNull(subscriber);
                DebuggingThrowHelper.ThrowIfUnityObjectInvalid(scope);

                _subscriber = new(subscriber, scope);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnitySubscriber(in Subscriber<UnityEntityId<TScope>> subscriber)
            {
                _subscriber = subscriber;
            }

            public bool IsCreated => _subscriber.IsCreated;

            public UnityEntityId<TScope> Scope => _subscriber.Scope;

            public ICollection<ISubscription> Subscriptions => _subscriber.Subscriptions;

            public MessageInterceptors Interceptors => _subscriber.Interceptors;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Subscriber<TNewScope> WithScope<TNewScope>()
                where TNewScope : struct
            {
                return _subscriber.WithScope<TNewScope>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Subscriber<TNewScope> WithScope<TNewScope>([NotNull] TNewScope scope)
            {
                DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(scope);

                return _subscriber.WithScope(scope);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Subscriber<GlobalScope> WithGlobalScope()
            {
                return _subscriber.WithGlobalScope();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnitySubscriber<TNewScope> WithUnityScope<TNewScope>([NotNull] TNewScope scope)
                where TNewScope : UnityEngine.Object
            {
                DebuggingThrowHelper.ThrowIfUnityObjectInvalid(scope);

                return _subscriber.WithUnityScope(scope);
            }

            /// <summary>
            /// Remove empty handler groups to optimize performance.
            /// </summary>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

                _subscriber.Compress<TMessage>(logger);
            }

            public void Clear(ILogger logger = null)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Clear(logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe<TMessage>(handler, order, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe(handler, order, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<TMessage>(handler, unsubscribeToken, order, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe(handler, unsubscribeToken, order, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe<TMessage>(handler, order, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe(handler, order, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<TMessage>(handler, unsubscribeToken, order, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe(handler, unsubscribeToken, order, logger);
            }

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (IsCreated)
                {
                    return true;
                }

                ThrowHelper.LogErrorInvalidUnitySubscriberAccess(GetType(), logger);

                return false;
            }
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings();
        }
    }
}
