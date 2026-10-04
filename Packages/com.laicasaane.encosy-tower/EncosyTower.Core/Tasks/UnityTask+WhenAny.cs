using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Creates a task that completes when either <paramref name="leftTask"/> or <paramref name="rightTask"/>
        /// completes.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <param name="leftTask">The task that produces a result.</param>
        /// <param name="rightTask">The task without a result.</param>
        /// <returns>A task whose result tells whether <paramref name="leftTask"/> won and, if so, its result.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the first observed completion wins; a winner's fault or cancellation is rethrown as the
        /// same instance. Losers are observed and not cancelled, and their faults are discarded. When several inputs
        /// are already complete, <paramref name="leftTask"/> wins.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, calling <c>GetResult</c> twice, or passing the same task (or a
        /// copy) more than once to a combinator: may throw <see cref="InvalidOperationException"/>, return a stale
        /// result, never complete, or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An input obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: that input follows
        /// native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        public static UnityTask<(bool hasResultLeft, T result)> WhenAny<T>(
              UnityTask<T> leftTask
            , UnityTask rightTask
        )
        {
            var state = WhenAnyLeftRightState<T>.Rent();
            PooledUnityTaskObserver<T, UnityTaskPosition1, WhenAnyLeftRightState<T>>.Observe(leftTask, state);
            PooledUnityTaskObserver<UnityTaskPosition2, WhenAnyLeftRightState<T>>.Observe(rightTask, state);
            return state.WaitAsync();
        }

        /// <summary>
        /// Creates a task that completes when any task in <paramref name="tasks"/> completes.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <param name="tasks">The tasks to wait for. Must not be empty.</param>
        /// <returns>A task whose result holds the index and the result of the winning input.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the first observed completion wins; a winner's fault or cancellation is rethrown as the
        /// same instance. Losers are observed and not cancelled, and their faults are discarded. When several inputs
        /// are already complete, the lowest index wins.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, calling <c>GetResult</c> twice, or passing the same task (or a
        /// copy) more than once to a combinator: may throw <see cref="InvalidOperationException"/>, return a stale
        /// result, never complete, or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// An empty <paramref name="tasks"/>: development builds (<c>UNITY_EDITOR</c>, <c>DEBUG</c>,
        /// <c>ENCOSY_RUNTIME_CHECKS</c>) throw <see cref="ArgumentException"/>. Release builds return a task that never
        /// completes.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An input obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: that input follows
        /// native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tasks"/> is empty (development builds).</exception>
        public static UnityTask<(int winArgumentIndex, T result)> WhenAny<T>(params UnityTask<T>[] tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);
            return StartWhenAny(tasks, tasks.Length);
        }

        /// <summary>
        /// Creates a task that completes when any task in <paramref name="tasks"/> completes.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <param name="tasks">The tasks to wait for. Must not be empty.</param>
        /// <returns>A task whose result holds the index and the result of the winning input.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the first observed completion wins; a winner's fault or cancellation is rethrown as the
        /// same instance. Losers are observed and not cancelled, and their faults are discarded. When several inputs
        /// are already complete, the lowest index wins.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, calling <c>GetResult</c> twice, or passing the same task (or a
        /// copy) more than once to a combinator: may throw <see cref="InvalidOperationException"/>, return a stale
        /// result, never complete, or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// An empty <paramref name="tasks"/>: development builds (<c>UNITY_EDITOR</c>, <c>DEBUG</c>,
        /// <c>ENCOSY_RUNTIME_CHECKS</c>) throw <see cref="ArgumentException"/>. Release builds return a task that never
        /// completes.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An input obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: that input follows
        /// native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tasks"/> is empty (development builds).</exception>
        public static UnityTask<(int winArgumentIndex, T result)> WhenAny<T>(IEnumerable<UnityTask<T>> tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);

            var buffer = Materialize(tasks, out var count);

            try
            {
                return StartWhenAny(buffer, count);
            }
            finally
            {
                Array.Clear(buffer, 0, count);
                ArrayPool<UnityTask<T>>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Creates a task that completes when any task in <paramref name="tasks"/> completes.
        /// </summary>
        /// <param name="tasks">The tasks to wait for. Must not be empty.</param>
        /// <returns>A task whose result is the index of the winning input.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the first observed completion wins; a winner's fault or cancellation is rethrown as the
        /// same instance. Losers are observed and not cancelled, and their faults are discarded. When several inputs
        /// are already complete, the lowest index wins.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, calling <c>GetResult</c> twice, or passing the same task (or a
        /// copy) more than once to a combinator: may throw <see cref="InvalidOperationException"/>, return a stale
        /// result, never complete, or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// An empty <paramref name="tasks"/>: development builds (<c>UNITY_EDITOR</c>, <c>DEBUG</c>,
        /// <c>ENCOSY_RUNTIME_CHECKS</c>) throw <see cref="ArgumentException"/>. Release builds return a task that never
        /// completes.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An input obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: that input follows
        /// native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tasks"/> is empty (development builds).</exception>
        public static UnityTask<int> WhenAny(params UnityTask[] tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);
            return StartWhenAny(tasks, tasks.Length);
        }

        /// <summary>
        /// Creates a task that completes when any task in <paramref name="tasks"/> completes.
        /// </summary>
        /// <param name="tasks">The tasks to wait for. Must not be empty.</param>
        /// <returns>A task whose result is the index of the winning input.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the first observed completion wins; a winner's fault or cancellation is rethrown as the
        /// same instance. Losers are observed and not cancelled, and their faults are discarded. When several inputs
        /// are already complete, the lowest index wins.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, calling <c>GetResult</c> twice, or passing the same task (or a
        /// copy) more than once to a combinator: may throw <see cref="InvalidOperationException"/>, return a stale
        /// result, never complete, or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// An empty <paramref name="tasks"/>: development builds (<c>UNITY_EDITOR</c>, <c>DEBUG</c>,
        /// <c>ENCOSY_RUNTIME_CHECKS</c>) throw <see cref="ArgumentException"/>. Release builds return a task that never
        /// completes.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An input obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: that input follows
        /// native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tasks"/> is empty (development builds).</exception>
        public static UnityTask<int> WhenAny(IEnumerable<UnityTask> tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);

            var buffer = Materialize(tasks, out var count);

            try
            {
                return StartWhenAny(buffer, count);
            }
            finally
            {
                Array.Clear(buffer, 0, count);
                ArrayPool<UnityTask>.Shared.Return(buffer);
            }
        }

        private static UnityTask<int> StartWhenAny(UnityTask[] tasks, int count)
        {
            ThrowHelper.ThrowIfWhenAnyEmpty(count == 0);

            var state = WhenAnyState.Rent(count);

            for (var i = 0; i < count; i++)
            {
                PooledIndexedUnityTaskObserver<WhenAnyState>.Observe(tasks[i], state, i);
            }

            return state.WaitAsync();
        }

        private static UnityTask<(int winArgumentIndex, T result)> StartWhenAny<T>(UnityTask<T>[] tasks, int count)
        {
            ThrowHelper.ThrowIfWhenAnyEmpty(count == 0);

            var state = WhenAnyState<T>.Rent(count);

            for (var i = 0; i < count; i++)
            {
                PooledIndexedUnityTaskObserver<T, WhenAnyState<T>>.Observe(tasks[i], state, i);
            }

            return state.WaitAsync();
        }

        private sealed class WhenAnyState : IIndexedUnityTaskSink
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly Stack<WhenAnyState> s_pool = new();

            private UnityTaskCompletionSource<int> _source;
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
                    state = s_pool.Count > 0 ? s_pool.Pop() : new();
                }

                if (state._source == null)
                {
                    state._source = new();
                }
                else
                {
                    state._source.Reset();
                }

                state._remaining = count;
                state._won = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async UnityTask<int> WaitAsync()
            {
                try
                {
                    return await _source.Task;
                }
                finally
                {
                    Volatile.Write(location: ref _consumed, value: 1);
                    TryRecycle();
                }
            }

            void IIndexedUnityTaskSink.Complete(int index, Exception exception)
            {
                if (Interlocked.CompareExchange(location1: ref _won, value: 1, comparand: 0) != 0)
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

            void IIndexedUnityTaskSink.Detach()
                => Detach();

            private void Detach()
            {
                if (Interlocked.Decrement(ref _remaining) == 0)
                {
                    Volatile.Write(location: ref _detached, value: 1);
                    TryRecycle();
                }
            }

            private void TryRecycle()
            {
                if (Volatile.Read(ref _consumed) == 0
                    || Volatile.Read(ref _detached) == 0
                    || Interlocked.Exchange(location1: ref _returned, value: 1) != 0
                )
                {
                    return;
                }

                lock (s_pool)
                {
                    if (s_pool.Count < MAX_POOL_SIZE)
                    {
                        s_pool.Push(this);
                    }
                }
            }
        }

        private sealed class WhenAnyState<T> : IIndexedUnityTaskResultSink<T>
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly Stack<WhenAnyState<T>> s_pool = new();

            private UnityTaskCompletionSource<(int winArgumentIndex, T result)> _source;
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
                    state = s_pool.Count > 0 ? s_pool.Pop() : new();
                }

                if (state._source == null)
                {
                    state._source = new();
                }
                else
                {
                    state._source.Reset();
                }

                state._remaining = count;
                state._won = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async UnityTask<(int winArgumentIndex, T result)> WaitAsync()
            {
                try
                {
                    return await _source.Task;
                }
                finally
                {
                    Volatile.Write(location: ref _consumed, value: 1);
                    TryRecycle();
                }
            }

            void IIndexedUnityTaskResultSink<T>.Complete(int index, T result, Exception exception)
            {
                if (Interlocked.CompareExchange(location1: ref _won, value: 1, comparand: 0) != 0)
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

            void IIndexedUnityTaskResultSink<T>.Detach()
                => Detach();

            private void Detach()
            {
                if (Interlocked.Decrement(ref _remaining) == 0)
                {
                    Volatile.Write(location: ref _detached, value: 1);
                    TryRecycle();
                }
            }

            private void TryRecycle()
            {
                if (Volatile.Read(ref _consumed) == 0
                    || Volatile.Read(ref _detached) == 0
                    || Interlocked.Exchange(location1: ref _returned, value: 1) != 0
                )
                {
                    return;
                }

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
            : IUnityTaskResultSink<T, UnityTaskPosition1>
            , IUnityTaskSink<UnityTaskPosition2>
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly Stack<WhenAnyLeftRightState<T>> s_pool = new();

            private UnityTaskCompletionSource<(bool hasResultLeft, T result)> _source;
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
                    state = s_pool.Count > 0 ? s_pool.Pop() : new();
                }

                if (state._source == null)
                {
                    state._source = new();
                }
                else
                {
                    state._source.Reset();
                }

                state._remaining = 2;
                state._won = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async UnityTask<(bool hasResultLeft, T result)> WaitAsync()
            {
                try
                {
                    return await _source.Task;
                }
                finally
                {
                    Volatile.Write(location: ref _consumed, value: 1);
                    TryRecycle();
                }
            }

            void IUnityTaskResultSink<T, UnityTaskPosition1>.Complete(
                  UnityTaskPosition1 position
                , T result
                , Exception exception
            )
                => Complete(hasResultLeft: true, result: result, exception: exception);

            void IUnityTaskSink<UnityTaskPosition2>.Complete(UnityTaskPosition2 position, Exception exception)
                => Complete(hasResultLeft: false, result: default, exception: exception);

            void IUnityTaskResultSink<T, UnityTaskPosition1>.Detach()
                => Detach();

            void IUnityTaskSink<UnityTaskPosition2>.Detach()
                => Detach();

            private void Complete(bool hasResultLeft, T result, Exception exception)
            {
                if (Interlocked.CompareExchange(location1: ref _won, value: 1, comparand: 0) != 0)
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
                    Volatile.Write(location: ref _detached, value: 1);
                    TryRecycle();
                }
            }

            private void TryRecycle()
            {
                if (Volatile.Read(ref _consumed) == 0
                    || Volatile.Read(ref _detached) == 0
                    || Interlocked.Exchange(location1: ref _returned, value: 1) != 0
                )
                {
                    return;
                }

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
