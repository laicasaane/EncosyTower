#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Tasks;
using UnityEngine.Pool;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class MessageBroker<TMessage> : ICompressibleMessageBroker
    {
        private readonly List<int> _ordering = new(1);
        private readonly Dictionary<int, ArrayMap<DelegateId, IHandler<TMessage>>> _orderToHandlerMap = new(1);

        private ArrayPool<UnityTask> _taskArrayPool;
        private long _refCount;

        public ArrayPool<UnityTask> TaskArrayPool
        {
            get => _taskArrayPool;
            set
            {
                DebuggingThrowHelper.ThrowIfNull(value);
                _taskArrayPool = value;
            }
        }

        public bool HasHandlers
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ordering.Count > 0;
        }

        public bool IsCached => _refCount > 0;

        public Subscription<TMessage> Subscribe([NotNull] IHandler<TMessage> handler, int order, ILogger logger)
        {
            DebuggingThrowHelper.ThrowIfNull(handler);

            lock (_orderToHandlerMap)
            {
#if __ENCOSY_VALIDATION__
                try
#endif
                {
                    var orderToHandlerMap = _orderToHandlerMap;
                    var ordering = _ordering;

                    if (ordering.Contains(order) == false)
                    {
                        ordering.Add(order);
                        ordering.Sort(Comparer<int>.Default);
                    }

                    if (orderToHandlerMap.TryGetValue(order, out var handlerMap) == false)
                    {
                        handlerMap = new ArrayMap<DelegateId, IHandler<TMessage>>();
                        orderToHandlerMap.Add(order, handlerMap);
                    }

                    if (handlerMap.TryAdd(handler.Id, handler))
                    {
                        return new Subscription<TMessage>(this, handler, order);
                    }
                }
#if __ENCOSY_VALIDATION__
                catch (Exception ex)
                {
                    ThrowHelper.LogException(logger, ex);
                }
#endif

                return Subscription<TMessage>.None;
            }
        }

        public void Dispose()
        {
            lock (_orderToHandlerMap)
            {
                var orderToHandlerMap = _orderToHandlerMap;

                foreach (var kvp in orderToHandlerMap)
                {
                    kvp.Value?.Dispose();
                }

                orderToHandlerMap.Clear();
            }
        }

        public void Clear()
        {
            lock (_orderToHandlerMap)
            {
                _ordering.Clear();
                _orderToHandlerMap.Clear();
            }
        }

        public bool RemoveHandler(IHandler<TMessage> expected, int order)
        {
            lock (_orderToHandlerMap)
            {
                if (_orderToHandlerMap.TryGetValue(order, out var handlerMap) == false)
                {
                    return false;
                }

                if (expected == null
                    || handlerMap.TryGetValue(expected.Id, out var current) == false
                    || ReferenceEquals(current, expected) == false
                )
                {
                    return false;
                }

                handlerMap.Remove(expected.Id);

                if (handlerMap.Count < 1)
                {
                    _orderToHandlerMap.Remove(order);
                    _ordering.Remove(order);
                }

                return true;
            }
        }

        public void OnCache()
        {
            checked
            {
                _refCount++;
            }
        }

        public void OnUncache()
        {
            _refCount = Math.Max(0, _refCount - 1);
        }

        /// <summary>
        /// Remove empty handler groups to optimize performance.
        /// </summary>
        public void Compress(ILogger logger)
        {
            lock (_orderToHandlerMap)
            {
#if __ENCOSY_VALIDATION__
                try
#endif
                {
                    var orderToHandlerMap = _orderToHandlerMap;
                    var ordering = _ordering;
                    var orders = ordering.AsListFast().AsSpan();

                    for (var i = orders.Length - 1; i >= 0; i--)
                    {
                        var order = orders[i];

                        if (orderToHandlerMap.TryGetValue(order, out var handlerMap) == false)
                        {
                            continue;
                        }

                        if (handlerMap.Count > 0)
                        {
                            continue;
                        }

                        orderToHandlerMap.Remove(order);
                        ordering.RemoveAt(i);
                    }
                }
#if __ENCOSY_VALIDATION__
                catch (Exception ex)
                {
                    ThrowHelper.LogException(logger, ex);
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<UnityTask> TryPublishAsync(TMessage message, PublishingContext context)
        {
            return HasHandlers ? PublishAsync(message, context) : Option.None;
        }

        private async UnityTask PublishAsync(TMessage message, PublishingContext context)
        {
            var handlerGroupList = GetHandlerGroupList(context.Logger);

#if __ENCOSY_VALIDATION__
            try
#endif
            {
                var taskArrayPool = TaskArrayPool;
                var handlerGroups = handlerGroupList.AsListFast();
                var length = handlerGroups.Count;

                for (var i = 0; i < length; i++)
                {
                    if (context.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    await PublishAsync(handlerGroups[i], message, context, taskArrayPool);

                    if (context.Token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
#if __ENCOSY_VALIDATION__
            catch (Exception ex)
            {
                ThrowHelper.LogException(context.Logger, ex);
            }
            finally
#endif
            {
                Dispose(handlerGroupList);
            }
        }

        private List<HandlerGroup> GetHandlerGroupList(ILogger logger)
        {
            var orderToHandlerMap = _orderToHandlerMap;
            var orders = _ordering.AsListFast().AsSpan();
            var handlerGroupList = GetHandlerGroupList();

#if __ENCOSY_VALIDATION__
            try
#endif
            {
                for (var i = orders.Length - 1; i >= 0; i--)
                {
                    var order = orders[i];

                    if (orderToHandlerMap.TryGetValue(order, out var handlerMap) == false
                        || handlerMap.Count < 1
                    )
                    {
                        continue;
                    }

                    var handlerArray = handlerMap.GetValues();
                    var handlerList = GetHandlerList();
                    var handlerSpan = handlerList.AsListFast().AddReplicateNoInit(handlerArray.Length);
                    handlerArray.CopyTo(handlerSpan);
                    handlerGroupList.Add(new HandlerGroup(order, handlerList));
                }
            }
#if __ENCOSY_VALIDATION__
            catch (Exception ex)
            {
                ThrowHelper.LogException(logger, ex);
            }
#endif

            return handlerGroupList;
        }

        private static void Dispose(List<HandlerGroup> handlerGroupList)
        {
            if (handlerGroupList == null)
            {
                return;
            }

            var handlerGroups = handlerGroupList.AsListFast().AsSpan();
            var handlerGroupCount = handlerGroups.Length;

            for (var i = 0; i < handlerGroupCount; i++)
            {
                ref var handlerGroup = ref handlerGroups[i];
                var handlers = handlerGroup.Handlers;

                if (handlers == null) continue;

                Release(handlers);

                handlerGroup = default;
            }

            Release(handlerGroupList);
        }

        private async UnityTask PublishAsync(
              HandlerGroup handlerGroup
            , TMessage message
            , PublishingContext context
            , ArrayPool<UnityTask> taskArrayPool
        )
        {
            var handlerList = handlerGroup.Handlers.AsListFast();

            if (handlerList.Count < 1)
            {
                return;
            }

            var tasks = taskArrayPool.Rent(handlerList.Count);

            try
            {
                GetTasks(handlerList.List, handlerGroup.Order, message, context, tasks);

                await UnityTask.WhenAll(tasks, handlerList.Count);
            }
#if __ENCOSY_VALIDATION__
            catch (Exception ex)
            {
                ThrowHelper.LogException(context.Logger, ex);
            }
#endif
            finally
            {
                taskArrayPool.Return(tasks, true);
            }
        }

        private void GetTasks(
              List<IHandler<TMessage>> handlerList
            , int order
            , TMessage message
            , PublishingContext context
            , UnityTask[] resultTasks
        )
        {
            var handlerListFast = handlerList.AsListFast();
            var handlers = handlerListFast.AsReadOnlySpan();
            var handlersLength = handlerListFast.Count;

            for (var i = 0; i < handlersLength; i++)
            {
#if __ENCOSY_VALIDATION__
                try
#endif
                {
                    var handler = handlers[i];

                    if (handler == null)
                    {
                        resultTasks[i] = UnityTask.CompletedTask;
                        continue;
                    }

                    var result = handler.Handle(message, context);
                    ThrowHelper.ThrowIfHandlerResultIsInvalid(result.IsValid, handler.GetType());

                    if (result.TryGetValue(out var task))
                    {
                        resultTasks[i] = task;
                        continue;
                    }

                    resultTasks[i] = UnityTask.CompletedTask;

                    if (RemoveHandler(handler, order))
                    {
                        handler.Dispose();
                    }

                    if (result.TryGetError(out var error))
                    {
                        ThrowHelper.LogHandlerError(error, context.Logger);
                    }
                }
#if __ENCOSY_VALIDATION__
                catch (Exception ex)
                {
                    resultTasks[i] = UnityTask.CompletedTask;
                    ThrowHelper.LogException(context.Logger, ex);
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<HandlerGroup> GetHandlerGroupList()
            => ListPool<HandlerGroup>.Get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Release(List<HandlerGroup> item)
            => ListPool<HandlerGroup>.Release(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<IHandler<TMessage>> GetHandlerList()
            => ListPool<IHandler<TMessage>>.Get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Release(List<IHandler<TMessage>> item)
            => ListPool<IHandler<TMessage>>.Release(item);

        private readonly struct HandlerGroup
        {
            public readonly int Order;
            public readonly List<IHandler<TMessage>> Handlers;

            public HandlerGroup(int order, List<IHandler<TMessage>> handlers)
            {
                Order = order;
                Handlers = handlers;
            }
        }
    }
}
