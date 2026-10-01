#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;
using EncosyTower.Tasks;

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

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return default;
                }
#endif

                lock (_publisher._messageBrokers)
                {
                    var brokers = _publisher._messageBrokers;

                    if (brokers.TryGet<MessageBroker<TScope, TMessage>>(out var scopedBroker) == false)
                    {
                        scopedBroker = new MessageBroker<TScope, TMessage>();

                        if (brokers.TryAdd(scopedBroker) == false)
                        {
#if __ENCOSY_VALIDATION__
                            ThrowHelper.LogErrorUnexpectedBrokerRegistration<TScope, TMessage>(logger);
#endif

                            scopedBroker.Dispose();
                            return default;
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
                PublishAsync(message, context).Forget();
            }

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (_publisher == null)
                {
                    ThrowHelper.LogErrorInvalidPublisherAccess(GetType(), logger);

                    return false;
                }

                if (Scope != null)
                {
                    return true;
                }

                ThrowHelper.LogExceptionInvalidScope(logger);
                return false;
            }

            private bool Validate<TMessage>(TMessage message, ILogger logger)
            {
                if (_publisher == null)
                {
                    ThrowHelper.LogErrorInvalidPublisherAccess(GetType(), logger);

                    return false;
                }

                if (Scope == null)
                {
                    ThrowHelper.LogExceptionInvalidScope(logger);
                    return false;
                }

                if (message != null)
                {
                    return true;
                }

                ThrowHelper.LogExceptionMessageIsNull(logger);
                return false;
            }
#endif
        }
    }
}
