#if DISABLE_ENCOSY_CHECKS || !(((UNITY_EDITOR || DEBUG) && !DISABLE_ENCOSY_EDITOR_CHECKS) || ((ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) && !DISABLE_ENCOSY_RUNTIME_CHECKS))
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
        partial struct UnitySubscriber<TScope, TState>
        {
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TState, UnityTask> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                ThrowHelper.ThrowIfUnitySubscriberNotCreated(IsCreated, typeof(UnitySubscriber<TScope, TState>));

                _subscriber.TrySubscribe(
                      new StatefulHandlerFunc<TState, TMessage>(State, handler)
                    , order
                    , out var subscription
                );
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TState, TMessage, UnityTask> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                ThrowHelper.ThrowIfUnitySubscriberNotCreated(IsCreated, typeof(UnitySubscriber<TScope, TState>));

                _subscriber.TrySubscribe(
                      new StatefulHandlerFuncMessage<TState, TMessage>(State, handler)
                    , order
                    , out var subscription
                );
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TState, UnityTask> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                ThrowHelper.ThrowIfUnitySubscriberNotCreated(IsCreated, typeof(UnitySubscriber<TScope, TState>));

                if (_subscriber.TrySubscribe(
                          new StatefulHandlerFunc<TState, TMessage>(State, handler)
                        , order
                        , out var subscription
                    )
                )
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TState, TMessage, UnityTask> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                ThrowHelper.ThrowIfUnitySubscriberNotCreated(IsCreated, typeof(UnitySubscriber<TScope, TState>));

                if (_subscriber.TrySubscribe(
                          new StatefulHandlerFuncMessage<TState, TMessage>(State, handler)
                        , order
                        , out var subscription
                    )
                )
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TState, PublishingContext, UnityTask> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                ThrowHelper.ThrowIfUnitySubscriberNotCreated(IsCreated, typeof(UnitySubscriber<TScope, TState>));

                _subscriber.TrySubscribe(
                      new StatefulContextualHandlerFunc<TState, TMessage>(State, handler)
                    , order
                    , out var subscription
                );
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TState, TMessage, PublishingContext, UnityTask> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                ThrowHelper.ThrowIfUnitySubscriberNotCreated(IsCreated, typeof(UnitySubscriber<TScope, TState>));

                _subscriber.TrySubscribe(
                      new StatefulContextualHandlerFuncMessage<TState, TMessage>(State, handler)
                    , order
                    , out var subscription
                );
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TState, PublishingContext, UnityTask> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                ThrowHelper.ThrowIfUnitySubscriberNotCreated(IsCreated, typeof(UnitySubscriber<TScope, TState>));

                if (_subscriber.TrySubscribe(
                          new StatefulContextualHandlerFunc<TState, TMessage>(State, handler)
                        , order
                        , out var subscription
                    )
                )
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TState, TMessage, PublishingContext, UnityTask> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(handler);

                ThrowHelper.ThrowIfUnitySubscriberNotCreated(IsCreated, typeof(UnitySubscriber<TScope, TState>));

                if (_subscriber.TrySubscribe(
                          new StatefulContextualHandlerFuncMessage<TState, TMessage>(State, handler)
                        , order
                        , out var subscription
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
