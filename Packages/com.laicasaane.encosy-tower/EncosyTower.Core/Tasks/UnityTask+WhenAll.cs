using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Creates a task that completes when every task in <paramref name="tasks"/> has completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <returns>A task that completes when every input has completed.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes when every input succeeds. The first observed fault or cancellation completes
        /// the task immediately and is rethrown as the same instance. Later inputs are still observed and their faults
        /// are discarded. An empty input completes successfully.
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
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        public static UnityTask WhenAll(params UnityTask[] tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);
            return StartWhenAll(tasks, tasks.Length);
        }

        /// <summary>
        /// Creates a task that completes when every task in <paramref name="tasks"/> has completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <returns>A task that completes when every input has completed.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes when every input succeeds. The first observed fault or cancellation completes
        /// the task immediately and is rethrown as the same instance. Later inputs are still observed and their faults
        /// are discarded. An empty input completes successfully.
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
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        public static UnityTask WhenAll(IEnumerable<UnityTask> tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);

            var buffer = Materialize(tasks, out var count);

            try
            {
                return StartWhenAll(buffer, count);
            }
            finally
            {
                Array.Clear(buffer, 0, count);
                ArrayPool<UnityTask>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Creates a task that completes when the first <paramref name="count"/> tasks in <paramref name="tasks"/> have
        /// completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <param name="count">The number of leading entries of <paramref name="tasks"/> to wait for.</param>
        /// <returns>A task that completes when every selected input has completed.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes when every input succeeds. The first observed fault or cancellation completes
        /// the task immediately and is rethrown as the same instance. Later inputs are still observed and their faults
        /// are discarded. An empty input completes successfully. Entries at or after <paramref name="count"/> are not
        /// observed.
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
        /// A <paramref name="count"/> that is negative or greater than the length of <paramref name="tasks"/>:
        /// development builds (<c>UNITY_EDITOR</c>, <c>DEBUG</c>, <c>ENCOSY_RUNTIME_CHECKS</c>) throw <see
        /// cref="ArgumentOutOfRangeException"/>. Release builds may throw <see cref="IndexOutOfRangeException"/> or
        /// complete with wrong results.
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is out of range (development
        /// builds).</exception>
        public static UnityTask WhenAll(UnityTask[] tasks, int count)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);
            ThrowHelper.ThrowIfCountOutOfRange((uint)count <= (uint)tasks.Length);
            return StartWhenAll(tasks, count);
        }

        /// <summary>
        /// Creates a task that completes with the results of every task in <paramref name="tasks"/>.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <returns>A task whose result holds every input result in input order.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes when every input succeeds, with results in input order. The first observed fault
        /// or cancellation completes the task immediately and is rethrown as the same instance. Later inputs are still
        /// observed and their faults are discarded. An empty input completes successfully with an empty array.
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
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        public static UnityTask<T[]> WhenAll<T>(params UnityTask<T>[] tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);
            return StartWhenAll(tasks, tasks.Length);
        }

        /// <summary>
        /// Creates a task that completes with the results of every task in <paramref name="tasks"/>.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <returns>A task whose result holds every input result in input order.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes when every input succeeds, with results in input order. The first observed fault
        /// or cancellation completes the task immediately and is rethrown as the same instance. Later inputs are still
        /// observed and their faults are discarded. An empty input completes successfully with an empty array.
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
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        public static UnityTask<T[]> WhenAll<T>(IEnumerable<UnityTask<T>> tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);

            var buffer = Materialize(tasks, out var count);

            try
            {
                return StartWhenAll(buffer, count);
            }
            finally
            {
                Array.Clear(buffer, 0, count);
                ArrayPool<UnityTask<T>>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Creates a task that completes with the results of the first <paramref name="count"/> tasks in <paramref
        /// name="tasks"/>.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <param name="count">The number of leading entries of <paramref name="tasks"/> to wait for.</param>
        /// <returns>A task whose result holds every selected input result in input order.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes when every input succeeds, with results in input order. The first observed fault
        /// or cancellation completes the task immediately and is rethrown as the same instance. Later inputs are still
        /// observed and their faults are discarded. An empty input completes successfully with an empty array. Entries
        /// at or after <paramref name="count"/> are not observed.
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
        /// A <paramref name="count"/> that is negative or greater than the length of <paramref name="tasks"/>:
        /// development builds (<c>UNITY_EDITOR</c>, <c>DEBUG</c>, <c>ENCOSY_RUNTIME_CHECKS</c>) throw <see
        /// cref="ArgumentOutOfRangeException"/>. Release builds may throw <see cref="IndexOutOfRangeException"/> or
        /// complete with wrong results.
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is out of range (development
        /// builds).</exception>
        public static UnityTask<T[]> WhenAll<T>(UnityTask<T>[] tasks, int count)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);
            ThrowHelper.ThrowIfCountOutOfRange((uint)count <= (uint)tasks.Length);
            return StartWhenAll(tasks, count);
        }

        private static UnityTask StartWhenAll(UnityTask[] tasks, int count)
        {
            if (count == 0)
            {
                return CompletedTask;
            }

            var state = WhenAllState.Rent(count);

            for (var i = 0; i < count; i++)
            {
                PooledIndexedUnityTaskObserver<WhenAllState>.Observe(tasks[i], state, i);
            }

            return state.WaitAsync();
        }

        private static UnityTask<T[]> StartWhenAll<T>(UnityTask<T>[] tasks, int count)
        {
            if (count == 0)
            {
                return FromResult(Array.Empty<T>());
            }

            var state = WhenAllState<T>.Rent(count);

            for (var i = 0; i < count; i++)
            {
                PooledIndexedUnityTaskObserver<T, WhenAllState<T>>.Observe(tasks[i], state, i);
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

        private sealed class WhenAllState : IIndexedUnityTaskSink
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly Stack<WhenAllState> s_pool = new();

            private UnityTaskCompletionSource _source;
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
                state._signaled = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async UnityTask WaitAsync()
            {
                try
                {
                    await _source.Task;
                }
                finally
                {
                    Volatile.Write(location: ref _consumed, value: 1);
                    TryRecycle();
                }
            }

            void IIndexedUnityTaskSink.Complete(int index, Exception exception)
            {
                if (exception != null
                    && Interlocked.CompareExchange(location1: ref _signaled, value: 1, comparand: 0) == 0
                )
                {
                    _source.TrySetException(exception);
                }
            }

            void IIndexedUnityTaskSink.Detach()
            {
                if (Interlocked.Decrement(ref _remaining) != 0)
                {
                    return;
                }

                if (Interlocked.CompareExchange(location1: ref _signaled, value: 1, comparand: 0) == 0)
                {
                    _source.TrySetResult();
                }

                Volatile.Write(location: ref _detached, value: 1);
                TryRecycle();
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

        private sealed class WhenAllState<T> : IIndexedUnityTaskResultSink<T>
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly Stack<WhenAllState<T>> s_pool = new();

            private UnityTaskCompletionSource<T[]> _source;
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

                state._results = new T[count];
                state._remaining = count;
                state._signaled = 0;
                state._consumed = 0;
                state._detached = 0;
                state._returned = 0;
                return state;
            }

            internal async UnityTask<T[]> WaitAsync()
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
                if (exception == null)
                {
                    _results[index] = result;
                }
                else if (Interlocked.CompareExchange(location1: ref _signaled, value: 1, comparand: 0) == 0)
                {
                    _source.TrySetException(exception);
                }
            }

            void IIndexedUnityTaskResultSink<T>.Detach()
            {
                if (Interlocked.Decrement(ref _remaining) != 0)
                {
                    return;
                }

                if (Interlocked.CompareExchange(location1: ref _signaled, value: 1, comparand: 0) == 0)
                {
                    _source.TrySetResult(_results);
                }

                Volatile.Write(location: ref _detached, value: 1);
                TryRecycle();
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
    }
}
