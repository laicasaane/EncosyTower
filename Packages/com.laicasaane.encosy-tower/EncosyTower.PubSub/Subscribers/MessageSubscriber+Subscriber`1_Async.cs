#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;
using EncosyTower.Tasks;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub
{
    partial class MessageSubscriber
    {
        partial struct Subscriber<TScope>
        {
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<UnityTask> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                TrySubscribe(new HandlerFunc<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TMessage, UnityTask> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                TrySubscribe(new HandlerFuncMessage<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<UnityTask> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                if (TrySubscribe(new HandlerFunc<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TMessage, UnityTask> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                if (TrySubscribe(new HandlerFuncMessage<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<PublishingContext, UnityTask> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                TrySubscribe(new ContextualHandlerFunc<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TMessage, PublishingContext, UnityTask> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                TrySubscribe(new ContextualHandlerFuncMessage<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<PublishingContext, UnityTask> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                if (TrySubscribe(new ContextualHandlerFunc<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TMessage, PublishingContext, UnityTask> handler
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
                          new ContextualHandlerFuncMessage<TMessage>(handler)
                        , order
                        , out var subscription
                        , logger
                    )
                )
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings_Async();
        }
    }
}
