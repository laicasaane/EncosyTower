using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace UnityEngine.Tasks
{
    public static partial class Awaitables
    {
        public static Awaitable<(bool hasResultLeft, T result)> WhenAny<T>(Awaitable<T> leftTask, Awaitable rightTask)
        {
            DebuggingThrowHelper.ThrowIfNull(leftTask);
            DebuggingThrowHelper.ThrowIfNull(rightTask);
            var state = WhenAnyLeftRightState<T>.Rent();
            PooledAwaitableObserver<T, AwaitablePosition1, WhenAnyLeftRightState<T>>.Observe(leftTask, state);
            PooledAwaitableObserver<AwaitablePosition2, WhenAnyLeftRightState<T>>.Observe(rightTask, state);
            return state.WaitAsync();
        }

        public static Awaitable<(int winArgumentIndex, T result)> WhenAny<T>(params Awaitable<T>[] tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            return StartWhenAny(tasks, tasks.Length);
        }

        public static Awaitable<(int winArgumentIndex, T result)> WhenAny<T>(IEnumerable<Awaitable<T>> tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var buffer = Materialize(tasks, out var count);

            try
            {
                return StartWhenAny(buffer, count);
            }
            finally
            {
                Array.Clear(buffer, 0, count);
                ArrayPool<Awaitable<T>>.Shared.Return(buffer);
            }
        }

        public static Awaitable<int> WhenAny(params Awaitable[] tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            return StartWhenAny(tasks, tasks.Length);
        }

        public static Awaitable<int> WhenAny(IEnumerable<Awaitable> tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var buffer = Materialize(tasks, out var count);

            try
            {
                return StartWhenAny(buffer, count);
            }
            finally
            {
                Array.Clear(buffer, 0, count);
                ArrayPool<Awaitable>.Shared.Return(buffer);
            }
        }

        private static Awaitable<int> StartWhenAny(Awaitable[] tasks, int count)
        {
            ThrowHelper.ThrowIfWhenAnyEmpty(count);

            for (var i = 0; i < count; i++)
            {
                DebuggingThrowHelper.ThrowIfNull(tasks[i]);
            }

            var state = WhenAnyState.Rent(count);

            for (var i = 0; i < count; i++)
            {
                PooledIndexedAwaitableObserver<WhenAnyState>.Observe(tasks[i], state, i);
            }

            return state.WaitAsync();
        }

        private static Awaitable<(int winArgumentIndex, T result)> StartWhenAny<T>(Awaitable<T>[] tasks, int count)
        {
            ThrowHelper.ThrowIfWhenAnyEmpty(count);

            for (var i = 0; i < count; i++)
            {
                DebuggingThrowHelper.ThrowIfNull(tasks[i]);
            }

            var state = WhenAnyState<T>.Rent(count);

            for (var i = 0; i < count; i++)
            {
                PooledIndexedAwaitableObserver<T, WhenAnyState<T>>.Observe(tasks[i], state, i);
            }

            return state.WaitAsync();
        }

        private static TTask[] Materialize<TTask>(IEnumerable<TTask> tasks, out int count)
        {
            var buffer = ArrayPool<TTask>.Shared.Rent(
                tasks is ICollection<TTask> collection ? Math.Max(collection.Count, 1) : 8
            );
            count = 0;

            foreach (var task in tasks)
            {
                if (count == buffer.Length)
                {
                    var next = ArrayPool<TTask>.Shared.Rent(buffer.Length * 2);
                    Array.Copy(buffer, next, count);
                    Array.Clear(buffer, 0, count);
                    ArrayPool<TTask>.Shared.Return(buffer);
                    buffer = next;
                }

                buffer[count++] = task;
            }

            return buffer;
        }

        private sealed class WhenAnyState : IIndexedAwaitableSink
        {
            private const int MAX_POOL_SIZE = 256;
            private static readonly Stack<WhenAnyState> s_pool = new();
            private readonly AwaitableCompletionSource<int> _source = new();
            private int _remaining;
            private int _won;
            private int _consumed;
            private int _detached;
            private int _returned;

            private WhenAnyState() { }

            internal static WhenAnyState Rent(int count)
            {
                WhenAnyState state;

                lock (s_pool)
                {
                    state = s_pool.Count > 0 ? s_pool.Pop() : new WhenAnyState();
                }

                state._remaining = count;
                state._won = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async Awaitable<int> WaitAsync()
            {
                try
                {
                    return await _source.Awaitable;
                }
                finally
                {
                    Volatile.Write(ref _consumed, 1);
                    TryRecycle();
                }
            }

            void IIndexedAwaitableSink.Complete(int index, Exception exception)
            {
                if (Interlocked.CompareExchange(ref _won, 1, 0) != 0)
                {
                    return;
                }

                if (exception == null)
                {
                    _source.TrySetResult(index);
                }
                else
                {
                    _source.TrySetException(exception);
                }
            }

            void IIndexedAwaitableSink.Detach()
                => Detach();

            private void Detach()
            {
                if (Interlocked.Decrement(ref _remaining) == 0)
                {
                    Volatile.Write(ref _detached, 1);
                    TryRecycle();
                }
            }

            private void TryRecycle()
            {
                if (Volatile.Read(ref _consumed) == 0
                    || Volatile.Read(ref _detached) == 0
                    || Interlocked.Exchange(ref _returned, 1) != 0
                )
                {
                    return;
                }

                _source.Reset();

                lock (s_pool)
                {
                    if (s_pool.Count < MAX_POOL_SIZE)
                    {
                        s_pool.Push(this);
                    }
                }
            }
        }

        private sealed class WhenAnyState<T> : IIndexedAwaitableResultSink<T>
        {
            private const int MAX_POOL_SIZE = 256;
            private static readonly Stack<WhenAnyState<T>> s_pool = new();
            private readonly AwaitableCompletionSource<(int, T)> _source = new();
            private int _remaining;
            private int _won;
            private int _consumed;
            private int _detached;
            private int _returned;

            private WhenAnyState() { }

            internal static WhenAnyState<T> Rent(int count)
            {
                WhenAnyState<T> state;

                lock (s_pool)
                {
                    state = s_pool.Count > 0 ? s_pool.Pop() : new WhenAnyState<T>();
                }

                state._remaining = count;
                state._won = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async Awaitable<(int winArgumentIndex, T result)> WaitAsync()
            {
                try
                {
                    return await _source.Awaitable;
                }
                finally
                {
                    Volatile.Write(ref _consumed, 1);
                    TryRecycle();
                }
            }

            void IIndexedAwaitableResultSink<T>.Complete(int index, T result, Exception exception)
            {
                if (Interlocked.CompareExchange(ref _won, 1, 0) != 0)
                {
                    return;
                }

                if (exception == null)
                {
                    _source.TrySetResult((index, result));
                }
                else
                {
                    _source.TrySetException(exception);
                }
            }

            void IIndexedAwaitableResultSink<T>.Detach()
                => Detach();

            private void Detach()
            {
                if (Interlocked.Decrement(ref _remaining) == 0)
                {
                    Volatile.Write(ref _detached, 1);
                    TryRecycle();
                }
            }

            private void TryRecycle()
            {
                if (Volatile.Read(ref _consumed) == 0
                    || Volatile.Read(ref _detached) == 0
                    || Interlocked.Exchange(ref _returned, 1) != 0
                )
                {
                    return;
                }

                _source.Reset();

                lock (s_pool)
                {
                    if (s_pool.Count < MAX_POOL_SIZE)
                    {
                        s_pool.Push(this);
                    }
                }
            }
        }

        private sealed class WhenAnyLeftRightState<T>
            : IAwaitableResultSink<T, AwaitablePosition1>
            , IAwaitableSink<AwaitablePosition2>
        {
            private const int MAX_POOL_SIZE = 256;
            private static readonly Stack<WhenAnyLeftRightState<T>> s_pool = new();
            private readonly AwaitableCompletionSource<(bool, T)> _source = new();
            private int _remaining;
            private int _won;
            private int _consumed;
            private int _detached;
            private int _returned;

            private WhenAnyLeftRightState() { }

            internal static WhenAnyLeftRightState<T> Rent()
            {
                WhenAnyLeftRightState<T> state;

                lock (s_pool)
                {
                    state = s_pool.Count > 0 ? s_pool.Pop() : new WhenAnyLeftRightState<T>();
                }

                state._remaining = 2;
                state._won = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async Awaitable<(bool hasResultLeft, T result)> WaitAsync()
            {
                try
                {
                    return await _source.Awaitable;
                }
                finally
                {
                    Volatile.Write(ref _consumed, 1);
                    TryRecycle();
                }
            }

            void IAwaitableResultSink<T, AwaitablePosition1>.Complete(
                  AwaitablePosition1 position
                , T result
                , Exception exception
            )
                => Complete(true, result, exception);

            void IAwaitableSink<AwaitablePosition2>.Complete(AwaitablePosition2 position, Exception exception)
                => Complete(false, default, exception);

            void IAwaitableResultSink<T, AwaitablePosition1>.Detach()
                => Detach();

            void IAwaitableSink<AwaitablePosition2>.Detach()
                => Detach();

            private void Complete(bool hasResultLeft, T result, Exception exception)
            {
                if (Interlocked.CompareExchange(ref _won, 1, 0) != 0)
                {
                    return;
                }

                if (exception == null)
                {
                    _source.TrySetResult((hasResultLeft, result));
                }
                else
                {
                    _source.TrySetException(exception);
                }
            }

            private void Detach()
            {
                if (Interlocked.Decrement(ref _remaining) == 0)
                {
                    Volatile.Write(ref _detached, 1);
                    TryRecycle();
                }
            }

            private void TryRecycle()
            {
                if (Volatile.Read(ref _consumed) == 0
                    || Volatile.Read(ref _detached) == 0
                    || Interlocked.Exchange(ref _returned, 1) != 0
                )
                {
                    return;
                }

                _source.Reset();

                lock (s_pool)
                {
                    if (s_pool.Count < MAX_POOL_SIZE)
                    {
                        s_pool.Push(this);
                    }
                }
            }
        }
    }
}
