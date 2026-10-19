using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub
{
    partial class MessagePublisher
    {
        public readonly partial struct Publisher<TScope> : IIsCreated
        {
            internal readonly MessagePublisher _publisher;

            public bool IsCreated => _publisher != null;

            public TScope Scope { get; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Publisher<TNewScope> WithScope<TNewScope>()
                where TNewScope : struct
            {
                if (IsCreated == false)
                {
                    return new Publisher<TNewScope>(_publisher, default, invalid: true);
                }

                return _publisher.Scope<TNewScope>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Publisher<TNewScope> WithScope<TNewScope>([NotNull] TNewScope scope)
            {
                DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(scope);

                if (IsCreated == false)
                {
                    return new Publisher<TNewScope>(_publisher, scope, invalid: true);
                }

                return _publisher.Scope(scope);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Publisher<GlobalScope> WithGlobalScope()
            {
                if (IsCreated == false)
                {
                    return new Publisher<GlobalScope>(_publisher, default, invalid: true);
                }

                return _publisher.Global();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityPublisher<TNewScope> WithUnityScope<TNewScope>([NotNull] TNewScope scope)
                where TNewScope : UnityEngine.Object
            {
                DebuggingThrowHelper.ThrowIfUnityObjectInvalid(scope);

                if (IsCreated == false)
                {
                    return new UnityPublisher<TNewScope>(_publisher, scope, invalid: true);
                }

                return _publisher.UnityScope(scope);
            }

            internal Publisher([NotNull] MessagePublisher publisher, [NotNull] TScope scope)
            {
                DebuggingThrowHelper.ThrowIfNull(publisher);
                DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(scope);

                _publisher = publisher;
                Scope = scope;
            }

            internal Publisher(MessagePublisher publisher, TScope scope, bool invalid)
            {
                _publisher = publisher;
                Scope = scope;
            }

            public CachedPublisher<TScope, TMessage> Cache<TMessage>(
                  [NotNull] Func<TMessage> createFunc
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(createFunc);

                ThrowHelper.ThrowIfPublisherNotCreated(_publisher != null, typeof(Publisher<TScope>));
                ThrowHelper.ThrowIfScopeNull(Scope);

                lock (_publisher._messageBrokers)
                {
                    var brokers = _publisher._messageBrokers;

                    if (brokers.TryGet<MessageBroker<TScope, TMessage>>(out var scopedBroker) == false)
                    {
                        scopedBroker = new MessageBroker<TScope, TMessage>();

                        if (brokers.TryAdd(scopedBroker) == false)
                        {
                            scopedBroker.Dispose();
                            ThrowHelper.ThrowIfFailedToRegisterBroker<TMessage>(false);
                        }
                    }

                    return new CachedPublisher<TScope, TMessage>(
                          scopedBroker.Cache(Scope, _publisher._taskArrayPool)
                        , _publisher._interceptorBrokers
                        , createFunc
                        , Scope
                    );
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Publish<TMessage>(PublishingContext context = default)
#if ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
                Publish(new TMessage(), context);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Publish<TMessage>(TMessage message, PublishingContext context = default)
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                PublishObserver.ForgetAndLog(PublishAsync(message, context), context.Logger);
            }
        }
    }
}
