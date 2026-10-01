#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Runtime.CompilerServices;
using EncosyTower.PubSub.Internals;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub
{
    partial class MessagePublisher
    {
        partial struct Publisher<TScope>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask PublishAsync<TMessage>(PublishingContext context = default)
#if ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
                return PublishAsync(new TMessage(), context);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UnityTask PublishAsync<TMessage>(TMessage message, PublishingContext context = default)
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(message, context.Logger) == false)
                {
                    return UnityTask.CompletedTask;
                }
#endif

                if (_publisher._interceptorBrokers.HasInterceptors)
                {
                    var publisher = InterceptablePublisher<TScope, TMessage>.Rent();
                    publisher.scope = Scope;
                    publisher.message = message;
                    publisher.context = context;
                    publisher.publisher = this;

                    _publisher._interceptorBrokers.GetInterceptors<TScope, TMessage>(
                          publisher.Interceptors
                        , publisher.ObjectStack
                    );

                    return ContinueAsync(publisher.PublishAsync(), publisher);
                }
                else
                {
                    return PublishAsyncInternal(Scope, message, context);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static async UnityTask ContinueAsync(
                      UnityTask task
                    , InterceptablePublisher<TScope, TMessage> publisher
                )
                {
                    try
                    {
                        await task;
                    }
                    finally
                    {
                        publisher.Return();
                    }
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            internal UnityTask PublishCoreAsync<TMessage>(TScope scope, TMessage message, PublishingContext context)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(message, context.Logger) == false)
                {
                    return UnityTask.CompletedTask;
                }
#endif

                return PublishAsyncInternal(scope, message, context);
            }

            private UnityTask PublishAsyncInternal<TMessage>(TScope scope, TMessage message, PublishingContext context)
            {
                if (TryGetMessageBroker<TMessage>(_publisher, out var messageBroker)
                    && messageBroker.TryPublishAsync(scope, message, context).TryGetValue(out var task)
                )
                {
                    return task;
                }

                if (context.Strategy == PublishingStrategy.WaitForSubscriber)
                {
                    return WaitThenPublishAsync(_publisher, scope, message, context);
                }
#if __ENCOSY_VALIDATION__
                else
                {
                    ThrowHelper.LogWarningNoSubscriber<TScope, TMessage>(scope, context);
                }
#endif

                return UnityTask.CompletedTask;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool TryGetMessageBroker<TMessage>(
                  MessagePublisher publisher
                , out MessageBroker<TScope, TMessage> messageBroker
            )
            {
                return publisher._messageBrokers.TryGet(out messageBroker);
            }

            private static async UnityTask WaitThenPublishAsync<TMessage>(
                  MessagePublisher publisher
                , TScope scope
                , TMessage message
                , PublishingContext context
            )
            {
                await UnityTask.WaitUntil(publisher, static x => HasHandlers(x), context.Token);

                if (context.Token.IsCancellationRequested)
                {
                    return;
                }

                if (TryGetMessageBroker<TMessage>(publisher, out var broker)
                    && broker.TryPublishAsync(scope, message, context).TryGetValue(out var task)
                )
                {
                    await task;
                    return;
                }

#if __ENCOSY_VALIDATION__

                {
                    ThrowHelper.LogErrorFailedWaitThenPublish<TScope, TMessage>(scope, context);
                }
#endif

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool HasHandlers(MessagePublisher publisher)
                {
                    return publisher._messageBrokers.TryGet(out MessageBroker<TScope, TMessage> broker)
                        && broker.HasHandlers;
                }
            }

        }
    }
}
