using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace UnityEngine.Tasks
{
    public static partial class Awaitables
    {
        private static Awaitable StartWhenAll(Awaitable[] tasks, int count)
        {
            if (count == 0)
            {
                return CompletedTask;
            }

            for (var i = 0; i < count; i++)
            {
                DebuggingThrowHelper.ThrowIfNull(tasks[i]);
            }

            var state = WhenAllState.Rent(count);

            for (var i = 0; i < count; i++)
            {
                PooledIndexedAwaitableObserver<WhenAllState>.Observe(tasks[i], state, i);
            }

            return state.WaitAsync();
        }

        private static Awaitable StartWhenAll(IEnumerable<Awaitable> tasks)
        {
            var buffer = Materialize(tasks, out var count);

            try
            {
                return StartWhenAll(buffer, count);
            }
            finally
            {
                Array.Clear(buffer, 0, count);
                ArrayPool<Awaitable>.Shared.Return(buffer);
            }
        }

        private static Awaitable StartWhenAll(Awaitable task1, Awaitable task2)
        {
            DebuggingThrowHelper.ThrowIfNull(task1);
            DebuggingThrowHelper.ThrowIfNull(task2);
            var state = WhenAllState.Rent(2);
            PooledIndexedAwaitableObserver<WhenAllState>.Observe(task1, state, 0);
            PooledIndexedAwaitableObserver<WhenAllState>.Observe(task2, state, 1);
            return state.WaitAsync();
        }

        private static Awaitable StartWhenAll(Awaitable task1, Awaitable task2, Awaitable task3)
        {
            DebuggingThrowHelper.ThrowIfNull(task1);
            DebuggingThrowHelper.ThrowIfNull(task2);
            DebuggingThrowHelper.ThrowIfNull(task3);
            var state = WhenAllState.Rent(3);
            PooledIndexedAwaitableObserver<WhenAllState>.Observe(task1, state, 0);
            PooledIndexedAwaitableObserver<WhenAllState>.Observe(task2, state, 1);
            PooledIndexedAwaitableObserver<WhenAllState>.Observe(task3, state, 2);
            return state.WaitAsync();
        }

        private static Awaitable StartWhenAll(Awaitable task1, Awaitable task2, Awaitable task3, Awaitable task4)
        {
            DebuggingThrowHelper.ThrowIfNull(task1);
            DebuggingThrowHelper.ThrowIfNull(task2);
            DebuggingThrowHelper.ThrowIfNull(task3);
            DebuggingThrowHelper.ThrowIfNull(task4);
            var state = WhenAllState.Rent(4);
            PooledIndexedAwaitableObserver<WhenAllState>.Observe(task1, state, 0);
            PooledIndexedAwaitableObserver<WhenAllState>.Observe(task2, state, 1);
            PooledIndexedAwaitableObserver<WhenAllState>.Observe(task3, state, 2);
            PooledIndexedAwaitableObserver<WhenAllState>.Observe(task4, state, 3);
            return state.WaitAsync();
        }

        private static Awaitable<T[]> StartWhenAll<T>(Awaitable<T>[] tasks, int count)
        {
            if (count == 0)
            {
                return FromResult(Array.Empty<T>());
            }

            for (var i = 0; i < count; i++)
            {
                DebuggingThrowHelper.ThrowIfNull(tasks[i]);
            }

            var state = WhenAllState<T>.Rent(count);

            for (var i = 0; i < count; i++)
            {
                PooledIndexedAwaitableObserver<T, WhenAllState<T>>.Observe(tasks[i], state, i);
            }

            return state.WaitAsync();
        }

        private static Awaitable<T[]> StartWhenAll<T>(IEnumerable<Awaitable<T>> tasks)
        {
            var buffer = Materialize(tasks, out var count);

            try
            {
                return StartWhenAll(buffer, count);
            }
            finally
            {
                Array.Clear(buffer, 0, count);
                ArrayPool<Awaitable<T>>.Shared.Return(buffer);
            }
        }

        private static Awaitable<T> StartWhenAll<T>(Awaitable<T> task1, Awaitable task2)
        {
            DebuggingThrowHelper.ThrowIfNull(task1);
            DebuggingThrowHelper.ThrowIfNull(task2);
            var state = MixedWhenAllState<T>.Rent();
            PooledAwaitableObserver<T, AwaitablePosition1, MixedWhenAllState<T>>.Observe(task1, state);
            PooledAwaitableObserver<AwaitablePosition2, MixedWhenAllState<T>>.Observe(task2, state);
            return state.WaitAsync();
        }

        private sealed class WhenAllState : IIndexedAwaitableSink
        {
            private const int MAX_POOL_SIZE = 256;
            private static readonly Stack<WhenAllState> s_pool = new();
            private readonly AwaitableCompletionSource _source = new();
            private int _remaining;
            private int _signaled;
            private int _consumed;
            private int _detached;
            private int _returned;

            private WhenAllState() { }

            internal static WhenAllState Rent(int count)
            {
                WhenAllState state;

                lock (s_pool)
                {
                    state = s_pool.Count > 0 ? s_pool.Pop() : new WhenAllState();
                }

                state._remaining = count;
                state._signaled = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async Awaitable WaitAsync()
            {
                try
                {
                    await _source.Awaitable;
                }
                finally
                {
                    Volatile.Write(ref _consumed, 1);
                    TryRecycle();
                }
            }

            void IIndexedAwaitableSink.Complete(int index, Exception exception)
            {
                if (exception != null && Interlocked.CompareExchange(ref _signaled, 1, 0) == 0)
                {
                    _source.TrySetException(exception);
                }
            }

            void IIndexedAwaitableSink.Detach()
            {
                if (Interlocked.Decrement(ref _remaining) != 0)
                {
                    return;
                }

                if (Interlocked.CompareExchange(ref _signaled, 1, 0) == 0)
                {
                    _source.TrySetResult();
                }

                Volatile.Write(ref _detached, 1);
                TryRecycle();
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

        private sealed class WhenAllState<T> : IIndexedAwaitableResultSink<T>
        {
            private const int MAX_POOL_SIZE = 256;
            private static readonly Stack<WhenAllState<T>> s_pool = new();
            private readonly AwaitableCompletionSource<T[]> _source = new();
            private T[] _results;
            private int _remaining;
            private int _signaled;
            private int _consumed;
            private int _detached;
            private int _returned;

            private WhenAllState() { }

            internal static WhenAllState<T> Rent(int count)
            {
                WhenAllState<T> state;

                lock (s_pool)
                {
                    state = s_pool.Count > 0 ? s_pool.Pop() : new WhenAllState<T>();
                }

                state._results = new T[count];
                state._remaining = count;
                state._signaled = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async Awaitable<T[]> WaitAsync()
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
                if (exception == null)
                {
                    _results[index] = result;
                }
                else if (Interlocked.CompareExchange(ref _signaled, 1, 0) == 0)
                {
                    _source.TrySetException(exception);
                }
            }

            void IIndexedAwaitableResultSink<T>.Detach()
            {
                if (Interlocked.Decrement(ref _remaining) != 0)
                {
                    return;
                }

                if (Interlocked.CompareExchange(ref _signaled, 1, 0) == 0)
                {
                    _source.TrySetResult(_results);
                }

                Volatile.Write(ref _detached, 1);
                TryRecycle();
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
                _results = null;

                lock (s_pool)
                {
                    if (s_pool.Count < MAX_POOL_SIZE)
                    {
                        s_pool.Push(this);
                    }
                }
            }
        }

        private sealed class MixedWhenAllState<T>
            : IAwaitableResultSink<T, AwaitablePosition1>
            , IAwaitableSink<AwaitablePosition2>
        {
            private const int MAX_POOL_SIZE = 256;
            private static readonly Stack<MixedWhenAllState<T>> s_pool = new();
            private readonly AwaitableCompletionSource<T> _source = new();
            private T _result;
            private int _remaining;
            private int _signaled;
            private int _consumed;
            private int _detached;
            private int _returned;

            private MixedWhenAllState() { }

            internal static MixedWhenAllState<T> Rent()
            {
                MixedWhenAllState<T> state;

                lock (s_pool)
                {
                    state = s_pool.Count > 0 ? s_pool.Pop() : new MixedWhenAllState<T>();
                }

                state._result = default;
                state._remaining = 2;
                state._signaled = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async Awaitable<T> WaitAsync()
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
            {
                if (exception == null)
                {
                    _result = result;
                }
                else
                {
                    SignalFailure(exception);
                }
            }

            void IAwaitableSink<AwaitablePosition2>.Complete(AwaitablePosition2 position, Exception exception)
            {
                if (exception != null)
                {
                    SignalFailure(exception);
                }
            }

            void IAwaitableResultSink<T, AwaitablePosition1>.Detach()
                => Detach();

            void IAwaitableSink<AwaitablePosition2>.Detach()
                => Detach();

            private void SignalFailure(Exception exception)
            {
                if (Interlocked.CompareExchange(ref _signaled, 1, 0) == 0)
                {
                    _source.TrySetException(exception);
                }
            }

            private void Detach()
            {
                if (Interlocked.Decrement(ref _remaining) != 0)
                {
                    return;
                }

                if (Interlocked.CompareExchange(ref _signaled, 1, 0) == 0)
                {
                    _source.TrySetResult(_result);
                }

                Volatile.Write(ref _detached, 1);
                TryRecycle();
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
                _result = default;

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
