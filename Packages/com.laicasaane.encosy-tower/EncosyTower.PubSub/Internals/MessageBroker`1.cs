using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
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

        public Subscription<TMessage> Subscribe([NotNull] IHandler<TMessage> handler, int order)
        {
            DebuggingThrowHelper.ThrowIfNull(handler);

            lock (_orderToHandlerMap)
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
        public void Compress()
        {
            lock (_orderToHandlerMap)
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<UnityTask<Success<PublishError>>> TryGetPublishTask(TMessage message, PublishingContext context)
        {
            return HasHandlers ? PublishAsync(message, context) : Option.None;
        }

        private async UnityTask<Success<PublishError>> PublishAsync(TMessage message, PublishingContext context)
        {
            var handlerGroupList = GetHandlerGroupList();
            Success<PublishError> result = Success.Yes;

            try
            {
                CollectHandlerGroups(handlerGroupList);

                var taskArrayPool = TaskArrayPool;
                var handlerGroups = handlerGroupList.AsListFast();
                var length = handlerGroups.Count;
                var token = context.Token;

                token.ThrowIfCancellationRequested();

                for (var i = 0; i < length; i++)
                {
                    var groupResult = await PublishAsync(handlerGroups[i], message, context, taskArrayPool);

                    result = PublishError.Merge(result, groupResult);
                    token.ThrowIfCancellationRequested();
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                ThrowHelper.LogDisplacedFailure(result, context.Logger);
                throw;
            }
            finally
            {
                Dispose(handlerGroupList);
            }
        }

        private void CollectHandlerGroups(List<HandlerGroup> handlerGroupList)
        {
            var orderToHandlerMap = _orderToHandlerMap;
            var orders = _ordering.AsListFast().AsSpan();

            for (var i = orders.Length - 1; i >= 0; i--)
            {
                var order = orders[i];

                if (orderToHandlerMap.TryGetValue(order, out var handlerMap) == false || handlerMap.Count < 1)
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

        private async UnityTask<Success<PublishError>> PublishAsync(
              HandlerGroup handlerGroup
            , TMessage message
            , PublishingContext context
            , ArrayPool<UnityTask> taskArrayPool
        )
        {
            var handlerList = handlerGroup.Handlers.AsListFast();
            var count = handlerList.Count;

            if (count < 1)
            {
                return Success.Yes;
            }

            var resultPool = ArrayPool<Success<PublishError>>.Shared;
            var tasks = taskArrayPool.Rent(count);
            var results = resultPool.Rent(count);
            Success<PublishError> failure = Success.Yes;
            ExceptionDispatchInfo cancelled = null;

            try
            {
                GetTasks(handlerList.List, handlerGroup.Order, message, context, tasks, results, ref cancelled);

                for (var i = 0; i < count; i++)
                {
                    failure = PublishError.Merge(failure, results[i]);

                    try
                    {
                        await tasks[i];
                    }
                    catch (OperationCanceledException ex)
                    {
                        cancelled ??= ExceptionDispatchInfo.Capture(ex);
                    }
                    catch (Exception ex)
                    {
                        failure = PublishError.Merge(failure, PublishError.CreateHandlerFaulted(ex));
                    }
                }

                if (cancelled != null)
                {
                    ThrowHelper.LogDisplacedFailure(failure, context.Logger);
                    cancelled.Throw();
                }

                return failure;
            }
            finally
            {
                taskArrayPool.Return(tasks, clearArray: true);
                resultPool.Return(results, clearArray: true);
            }
        }

        private void GetTasks(
              List<IHandler<TMessage>> handlerList
            , int order
            , TMessage message
            , PublishingContext context
            , UnityTask[] resultTasks
            , Success<PublishError>[] results
            , ref ExceptionDispatchInfo cancelled
        )
        {
            var handlerListFast = handlerList.AsListFast();
            var handlers = handlerListFast.AsReadOnlySpan();
            var handlersLength = handlerListFast.Count;

            for (var i = 0; i < handlersLength; i++)
            {
                var handler = handlers[i];

                resultTasks[i] = UnityTask.CompletedTask;
                results[i] = Success.Yes;

                if (handler == null)
                {
                    continue;
                }

                Result<UnityTask, StateUnavailableError> result;

                try
                {
                    result = handler.Handle(message, context);
                }
                catch (OperationCanceledException ex)
                {
                    cancelled ??= ExceptionDispatchInfo.Capture(ex);
                    continue;
                }
                catch (Exception ex)
                {
                    results[i] = PublishError.CreateHandlerFaulted(ex);
                    continue;
                }

                ThrowHelper.ThrowIfHandlerResultIsInvalid(result.IsValid, handler.GetType());

                if (result.TryGetValue(out var task))
                {
                    resultTasks[i] = task;
                    continue;
                }

                if (RemoveHandler(handler, order))
                {
                    handler.Dispose();
                }

                if (result.TryGetError(out var error))
                {
                    results[i] = PublishError.CreateStateUnavailable(error.StateType);
                }
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
