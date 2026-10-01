#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Logging;
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

        public void Compress(ILogger logger)
        {
            lock (_scopedBrokers)
            {
#if !__ENCOSY_NO_VALIDATION__
                try
#endif
                {
                    var scopedBrokers = _scopedBrokers;
                    var scopesToRemove = _scopesToRemove.AsListFast();

                    scopesToRemove.IncreaseCapacityTo(scopedBrokers.Count);

                    foreach (var (key, broker) in scopedBrokers)
                    {
                        broker.Compress(logger);

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
#if !__ENCOSY_NO_VALIDATION__
                catch (Exception ex)
                {
                    ThrowHelper.LogException(logger, ex);
                }
#endif
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
        public void Compress(TScope scope, ILogger logger)
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

                broker.Compress(logger);

                if (broker.HasHandlers == false)
                {
                    scopedBrokers.Remove(scope);
                    broker.Dispose();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<UnityTask> TryPublishAsync(TScope scope, TMessage message, PublishingContext context)
        {
            return _scopedBrokers.TryGetValue(scope, out var broker)
                ? broker.TryPublishAsync(message, context)
                : Option.None;
        }

        public Subscription<TMessage> Subscribe(
              TScope scope
            , IHandler<TMessage> handler
            , int order
            , ArrayPool<UnityTask> taskArrayPool
            , ILogger logger
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

                return broker.Subscribe(handler, order, logger);
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
