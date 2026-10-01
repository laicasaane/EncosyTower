using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.Collections.Extensions;
using EncosyTower.Vaults;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub.Internals
{
    using AnyMessage = IInterceptorBroker.AnyMessage;

    internal sealed class InterceptorBrokers : IDisposable
    {
        private readonly SingletonVault<IInterceptorBroker> _brokers = new();
        private bool _hasInterceptors;

        public bool HasInterceptors => _hasInterceptors;

        public void Dispose()
        {
            lock (_brokers)
            {
                _brokers.Dispose();
                _hasInterceptors = false;
            }
        }

        public void GetInterceptors<TScope, TMessage>(List<object> result, Stack<object> stack)
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            var resultFast = result.AsListFast();

            if (HasInterceptors == false)
            {
                return;
            }

            if (TryGetBroker<AnyMessage>(out var brokerAnyMessage))
            {
                resultFast.AddRange(brokerAnyMessage.Interceptors.AsReadOnlySpan());
            }

            if (TryGetBroker<TMessage>(out var brokerTMessage) == false)
            {
                return;
            }

            var interceptors = brokerTMessage.Interceptors.AsReadOnlySpan();
            var length = interceptors.Length;

            for (var i = length - 1; i >= 0; i--)
            {
                var interceptor = interceptors[i];

                if (interceptor is IScopedMessageInterceptor<TScope, TMessage> or IMessageInterceptor<TMessage>)
                {
                    stack.Push(interceptor);
                }
            }

            if (stack.Count < 1)
            {
                return;
            }

            resultFast.IncreaseCapacityBy(stack.Count);

            while (stack.TryPop(out var obj))
            {
                resultFast.Add(obj);
            }
        }

        public void AddInterceptor([NotNull] IMessageInterceptor interceptor)
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<AnyMessage>();
                broker.Add(interceptor);
                _hasInterceptors = true;
            }
        }

        public void AddInterceptor([NotNull] IScopedMessageInterceptor interceptor)
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<AnyMessage>();
                broker.Add(interceptor);
                _hasInterceptors = true;
            }
        }

        public void AddInterceptor<TMessage>([NotNull] IMessageInterceptor<TMessage> interceptor)
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<TMessage>();
                broker.Add(interceptor);
                _hasInterceptors = true;
            }
        }

        public void AddInterceptor<TScope, TMessage>([NotNull] IScopedMessageInterceptor<TScope, TMessage> interceptor)
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<TMessage>();
                broker.Add(interceptor);
                _hasInterceptors = true;
            }
        }

        public void RemoveInterceptor([NotNull] IMessageInterceptor interceptor)
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<AnyMessage>();
                broker.Remove(interceptor);
                _hasInterceptors = broker.Interceptors.Count > 0;
            }
        }

        public void RemoveInterceptor([NotNull] IScopedMessageInterceptor interceptor)
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<AnyMessage>();
                broker.Remove(interceptor);
                _hasInterceptors = broker.Interceptors.Count > 0;
            }
        }

        public void RemoveInterceptor<TMessage>([NotNull] IMessageInterceptor<TMessage> interceptor)
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<TMessage>();
                broker.Remove(interceptor);
                _hasInterceptors = broker.Interceptors.Count > 0;
            }
        }

        public void RemoveInterceptor<TScope, TMessage>(
            [NotNull] IScopedMessageInterceptor<TScope, TMessage> interceptor
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<TMessage>();
                broker.Remove(interceptor);
                _hasInterceptors = broker.Interceptors.Count > 0;
            }
        }

        public void RemoveInterceptor([NotNull] Predicate<IMessageInterceptor> predicate)
        {
            DebuggingThrowHelper.ThrowIfNull(predicate);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<AnyMessage>();
                broker.Remove(predicate);
                _hasInterceptors = broker.Interceptors.Count > 0;
            }
        }

        public void RemoveInterceptor([NotNull] Predicate<IScopedMessageInterceptor> predicate)
        {
            DebuggingThrowHelper.ThrowIfNull(predicate);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<AnyMessage>();
                broker.Remove(predicate);
                _hasInterceptors = broker.Interceptors.Count > 0;
            }
        }

        public void RemoveInterceptor<TMessage>([NotNull] Predicate<IMessageInterceptor<TMessage>> predicate)
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(predicate);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<TMessage>();
                broker.Remove(predicate);
                _hasInterceptors = broker.Interceptors.Count > 0;
            }
        }

        public void RemoveInterceptor<TScope, TMessage>(
            [NotNull] Predicate<IScopedMessageInterceptor<TScope, TMessage>> predicate
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(predicate);

            lock (_brokers)
            {
                var broker = GetOrCreateBroker<TMessage>();
                broker.Remove(predicate);
                _hasInterceptors = broker.Interceptors.Count > 0;
            }
        }

        private InterceptorBroker<TMessage> GetOrCreateBroker<TMessage>()
        {
            if (_brokers.TryGet(out InterceptorBroker<TMessage> broker) == false)
            {
                broker = new InterceptorBroker<TMessage>();
                var result = _brokers.TryAdd(broker);
                ThrowHelper.ThrowIfFailedToRegisterBroker<TMessage>(result);
            }

            return broker;
        }

        private bool TryGetBroker<TMessage>(out InterceptorBroker<TMessage> broker)
        {
            return _brokers.TryGet(out broker);
        }
    }
}
