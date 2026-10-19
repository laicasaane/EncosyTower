#if DISABLE_ENCOSY_CHECKS || !(((UNITY_EDITOR || DEBUG) && !DISABLE_ENCOSY_EDITOR_CHECKS) || ((ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) && !DISABLE_ENCOSY_RUNTIME_CHECKS))
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.PubSub.Internals;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub
{
    partial class MessagePublisher
    {
        partial struct Publisher<TScope>
        {
            /// <remarks>
            /// PubSub never logs a failure it returns; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask<Success<PublishError>> PublishAsync<TMessage>(PublishingContext context = default)
#if ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
                return PublishAsync(new TMessage(), context);
            }

            /// <remarks>
            /// PubSub never logs a failure it returns; the caller logs each failed result it reacts to.
            /// </remarks>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UnityTask<Success<PublishError>> PublishAsync<TMessage>(
                  TMessage message
                , PublishingContext context = default
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowHelper.ThrowIfPublisherNotCreated(_publisher != null, typeof(Publisher<TScope>));
                ThrowHelper.ThrowIfScopeNull(Scope);
                ThrowHelper.ThrowIfMessageNull(message);

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

                    return ContinueAsync(publisher);
                }
                else
                {
                    return PublishAsyncInternal(Scope, message, context);
                }

                static async UnityTask<Success<PublishError>> ContinueAsync(
                      InterceptablePublisher<TScope, TMessage> publisher
                )
                {
                    var logger = publisher.context.Logger;

                    try
                    {
                        await publisher.PublishAsync();
                        return publisher._result;
                    }
                    catch
                    {
                        ThrowHelper.LogDisplacedFailure(publisher._result, logger);
                        throw;
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
            internal UnityTask<Success<PublishError>> PublishCoreAsync<TMessage>(
                  TScope scope
                , TMessage message
                , PublishingContext context
            )
            {
                ThrowHelper.ThrowIfPublisherNotCreated(_publisher != null, typeof(Publisher<TScope>));
                ThrowHelper.ThrowIfScopeNull(Scope);
                ThrowHelper.ThrowIfMessageNull(message);

                return PublishAsyncInternal(scope, message, context);
            }

            private UnityTask<Success<PublishError>> PublishAsyncInternal<TMessage>(
                  TScope scope
                , TMessage message
                , PublishingContext context
            )
            {
                if (TryGetMessageBroker<TMessage>(_publisher, out var messageBroker)
                    && messageBroker.TryGetPublishTask(scope, message, context).TryGetValue(out var task)
                )
                {
                    return task;
                }

                if (context.Strategy == PublishingStrategy.WaitForSubscriber)
                {
                    return WaitThenPublishAsync(_publisher, scope, message, context);
                }

                Success<PublishError> result = PublishError.CreateSubscriberNotFound<TScope, TMessage>();
                return UnityTask.FromResult(result);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool TryGetMessageBroker<TMessage>(
                  MessagePublisher publisher
                , out MessageBroker<TScope, TMessage> messageBroker
            )
            {
                return publisher._messageBrokers.TryGet(out messageBroker);
            }

            private static async UnityTask<Success<PublishError>> WaitThenPublishAsync<TMessage>(
                  MessagePublisher publisher
                , TScope scope
                , TMessage message
                , PublishingContext context
            )
            {
                await UnityTask.WaitUntil(
                      (publisher, scope)
                    , static x => x.publisher._messageBrokers.TryGet(out MessageBroker<TScope, TMessage> scopedBroker)
                        && scopedBroker.HasHandlersFor(x.scope)
                    , context.Token
                );

                context.Token.ThrowIfCancellationRequested();

                if (TryGetMessageBroker<TMessage>(publisher, out var broker)
                    && broker.TryGetPublishTask(scope, message, context).TryGetValue(out var task)
                )
                {
                    return await task;
                }

                return PublishError.CreateSubscriberNotFound<TScope, TMessage>();
            }
        }
    }
}
