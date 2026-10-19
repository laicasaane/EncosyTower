#if DISABLE_ENCOSY_CHECKS || !(((UNITY_EDITOR || DEBUG) && !DISABLE_ENCOSY_EDITOR_CHECKS) || ((ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) && !DISABLE_ENCOSY_RUNTIME_CHECKS))
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.PubSub.Internals;
using EncosyTower.Tasks;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub
{
    public partial struct CachedPublisher<TScope, TMessage> : IDisposable, IIsCreated
#if ENCOSY_PUBSUB_RELAX_MODE
        where TMessage
#else
        where TMessage : IMessage
#endif
    {
        internal MessageBroker<TMessage> _broker;
        internal InterceptorBrokers _interceptorBrokers;
        internal Func<TMessage> _createFunc;
        internal TScope _scope;

        internal CachedPublisher(
              [NotNull] MessageBroker<TMessage> broker
            , [NotNull] InterceptorBrokers interceptorBrokers
            , [NotNull] Func<TMessage> createFunc
            , TScope scope
        )
        {
            DebuggingThrowHelper.ThrowIfNull(broker);
            DebuggingThrowHelper.ThrowIfNull(interceptorBrokers);
            DebuggingThrowHelper.ThrowIfNull(createFunc);

            _broker = broker;
            _interceptorBrokers = interceptorBrokers;
            _createFunc = createFunc;
            _scope = scope;
        }

        public readonly bool IsCreated => _broker != null;

        public readonly MessageInterceptors Interceptors => new(_interceptorBrokers);

        public void Dispose()
        {
            _broker?.OnUncache();
            _broker = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Publish(PublishingContext context = default)
        {
            ThrowHelper.ThrowIfCachedPublisherNotCreated(_broker != null, typeof(CachedPublisher<TScope, TMessage>));

            Publish(_createFunc(), context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Publish(TMessage message, PublishingContext context = default)
        {
            PublishObserver.ForgetAndLog(PublishAsync(message, context), context.Logger);
        }

        /// <remarks>
        /// PubSub never logs a failure it returns; the caller logs each failed result it reacts to.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnityTask<Success<PublishError>> PublishAsync(PublishingContext context = default)
        {
            ThrowHelper.ThrowIfCachedPublisherNotCreated(_broker != null, typeof(CachedPublisher<TScope, TMessage>));

            return PublishAsync(_createFunc(), context);
        }

        /// <remarks>
        /// PubSub never logs a failure it returns; the caller logs each failed result it reacts to.
        /// </remarks>
#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly UnityTask<Success<PublishError>> PublishAsync(
              TMessage message
            , PublishingContext context = default
        )
        {
            ThrowHelper.ThrowIfCachedPublisherNotCreated(_broker != null, typeof(CachedPublisher<TScope, TMessage>));
            ThrowHelper.ThrowIfMessageNull(message);

            if (_interceptorBrokers.HasInterceptors)
            {
                var publisher = InterceptablePublisher<TScope, TMessage>.Rent();
                publisher.scope = _scope;
                publisher.message = message;
                publisher.context = context;
                publisher.cachedPublisher = this;

                _interceptorBrokers.GetInterceptors<TScope, TMessage>(publisher.Interceptors, publisher.ObjectStack);

                return ContinueAsync(publisher);
            }
            else
            {
                return PublishAsyncInternal(message, context);
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
        internal readonly UnityTask<Success<PublishError>> PublishCoreAsync(TMessage message, PublishingContext context)
        {
            ThrowHelper.ThrowIfCachedPublisherNotCreated(_broker != null, typeof(CachedPublisher<TScope, TMessage>));
            ThrowHelper.ThrowIfMessageNull(message);

            return PublishAsyncInternal(message, context);
        }

        private readonly UnityTask<Success<PublishError>> PublishAsyncInternal(
              TMessage message
            , PublishingContext context
        )
        {
            if (_broker.TryGetPublishTask(message, context).TryGetValue(out var task))
            {
                return task;
            }

            if (context.Strategy == PublishingStrategy.WaitForSubscriber)
            {
                return WaitThenPublishAsync(message, context);
            }

            Success<PublishError> result = PublishError.CreateSubscriberNotFound<TScope, TMessage>();
            return UnityTask.FromResult(result);
        }

        private readonly async UnityTask<Success<PublishError>> WaitThenPublishAsync(
              TMessage message
            , PublishingContext context
        )
        {
            await UnityTask.WaitUntil(_broker, static x => x.HasHandlers, context.Token);

            context.Token.ThrowIfCancellationRequested();

            if (_broker.TryGetPublishTask(message, context).TryGetValue(out var task))
            {
                return await task;
            }

            return PublishError.CreateSubscriberNotFound<TScope, TMessage>();
        }
    }
}
