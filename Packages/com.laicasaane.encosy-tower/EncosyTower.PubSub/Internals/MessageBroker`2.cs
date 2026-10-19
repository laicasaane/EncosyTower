using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class MessageBroker<TScope, TMessage> : ICompressibleMessageBroker, IScopedMessageBroker<TScope>
    {
        private readonly ArrayMap<TScope, MessageBroker<TMessage>> _scopedBrokers = new();
        private readonly List<TScope> _scopesToRemove = new();

        public bool HasHandlers
        {
            get
            {
                lock (_scopedBrokers)
                {
                    var scopedBrokers = _scopedBrokers;

                    foreach (var (_, broker) in scopedBrokers)
                    {
                        if (broker.HasHandlers)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

        public void Dispose()
        {
            lock (_scopedBrokers)
            {
                var scopedBrokers = _scopedBrokers;

                var brokers = scopedBrokers.GetValues();

                foreach (var broker in brokers)
                {
                    broker?.Dispose();
                }

                scopedBrokers.Dispose();
            }
        }

        public void Compress()
        {
            lock (_scopedBrokers)
            {
                var scopedBrokers = _scopedBrokers;
                var scopesToRemove = _scopesToRemove.AsListFast();

                scopesToRemove.IncreaseCapacityTo(scopedBrokers.Count);

                foreach (var (key, broker) in scopedBrokers)
                {
                    broker.Compress();

                    if (broker.HasHandlers == false)
                    {
                        broker.Dispose();
                        scopesToRemove.Add(key);
                    }
                }

                var scopes = scopesToRemove.AsSpan();

                foreach (var scope in scopes)
                {
                    scopedBrokers.Remove(scope);
                }
            }
        }

        public void Clear(TScope scope)
        {
            lock (_scopedBrokers)
            {
                if (_scopedBrokers.TryGetValue(scope, out var broker))
                {
                    broker.Clear();
                }
            }
        }

        /// <summary>
        /// Remove empty handler groups to optimize performance.
        /// </summary>
        public void Compress(TScope scope)
        {
            lock (_scopedBrokers)
            {
                var scopedBrokers = _scopedBrokers;

                if (scopedBrokers.TryGetValue(scope, out var broker) == false)
                {
                    return;
                }

                if (broker.IsCached)
                {
                    return;
                }

                broker.Compress();

                if (broker.HasHandlers == false)
                {
                    scopedBrokers.Remove(scope);
                    broker.Dispose();
                }
            }
        }

        public bool HasHandlersFor(TScope scope)
        {
            lock (_scopedBrokers)
            {
                return _scopedBrokers.TryGetValue(scope, out var broker) && broker.HasHandlers;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<UnityTask<Success<PublishError>>> TryGetPublishTask(
              TScope scope
            , TMessage message
            , PublishingContext context
        )
        {
            return _scopedBrokers.TryGetValue(scope, out var broker)
                ? broker.TryGetPublishTask(message, context)
                : Option.None;
        }

        public Subscription<TMessage> Subscribe(
              TScope scope
            , IHandler<TMessage> handler
            , int order
            , ArrayPool<UnityTask> taskArrayPool
        )
        {
            lock (_scopedBrokers)
            {
                var scopedBrokers = _scopedBrokers;

                if (scopedBrokers.TryGetValue(scope, out var broker) == false)
                {
                    scopedBrokers[scope] = broker = new MessageBroker<TMessage>();
                    broker.TaskArrayPool = taskArrayPool;
                }

                return broker.Subscribe(handler, order);
            }
        }

        public MessageBroker<TMessage> Cache(TScope scope, ArrayPool<UnityTask> taskArrayPool)
        {
            lock (_scopedBrokers)
            {
                var scopedBrokers = _scopedBrokers;

                if (scopedBrokers.TryGetValue(scope, out var broker) == false)
                {
                    scopedBrokers[scope] = broker = new MessageBroker<TMessage>();
                    broker.TaskArrayPool = taskArrayPool;
                }

                broker.OnCache();
                return broker;
            }
        }
    }
}
