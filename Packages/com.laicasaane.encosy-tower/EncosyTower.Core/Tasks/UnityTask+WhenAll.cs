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
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WhenAll</c>; Unity: none.
        /// </para>
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
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WhenAll</c>; Unity: none.
        /// </para>
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
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WhenAll</c>; Unity: none.
        /// </para>
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
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WhenAll</c>; Unity: none.
        /// </para>
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
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WhenAll</c>; Unity: none.
        /// </para>
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
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WhenAll</c>; Unity: none.
        /// </para>
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

        /// <summary>
        /// Starts an observer on each of the first <paramref name="count"/> tasks and returns the combined task. An
        /// empty selection returns <see cref="CompletedTask"/>.
        /// </summary>
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

        /// <summary>
        /// Starts an observer on each of the first <paramref name="count"/> tasks and returns a task that holds the
        /// results in input order. An empty selection returns a task completed with an empty array.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
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

        /// <summary>
        /// Copies <paramref name="tasks"/> into an array rented from <see cref="ArrayPool{T}.Shared"/> and returns it.
        /// Only the first <paramref name="count"/> entries are valid; the caller clears them and returns the array.
        /// </summary>
        /// <typeparam name="TTask">The task type.</typeparam>
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

        /// <summary>
        /// The pooled state behind <see cref="WhenAll(UnityTask[])"/> for tasks without a result.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Protocol:</b> one <see cref="PooledIndexedUnityTaskObserver{TSink}"/> per input calls
        /// <see cref="IIndexedUnityTaskSink.Complete"/> with the input's outcome and then
        /// <see cref="IIndexedUnityTaskSink.Detach"/>. <c>_remaining</c> counts the inputs not yet detached.
        /// </para>
        /// <para>
        /// <b>Outcome:</b> <c>_signaled</c> is set once with
        /// <see cref="Interlocked.CompareExchange(ref int, int, int)"/>. The first fault wins and faults the source. If
        /// no input faults, the last detach completes the source successfully.
        /// </para>
        /// <para>
        /// <b>Pooling:</b> <c>_releases</c> counts two owners: the last detaching observer and <see cref="WaitAsync"/>.
        /// The instance returns to <c>s_pool</c> on the second release, so it is never reused while either owner can
        /// still touch it. The pool holds at most <c>MAX_POOL_SIZE</c> instances and is guarded by a lock on
        /// <c>s_pool</c>.
        /// </para>
        /// </remarks>
        private sealed class WhenAllState : IIndexedUnityTaskSink
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly Stack<WhenAllState> s_pool = new();

            private UnityTaskCompletionSource _source;
            private int _remaining;
            private int _signaled;
            private int _releases;

            private WhenAllState() { }

            /// <summary>
            /// Takes an instance from the pool or creates one, resets its source and sets <c>_remaining</c> to
            /// <paramref name="count"/>.
            /// </summary>
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
                state._releases = 0;
                return state;
            }

            /// <summary>
            /// Awaits the source and releases this instance's <c>WaitAsync</c> ownership when the await ends, including
            /// on a fault.
            /// </summary>
            internal async UnityTask WaitAsync()
            {
                try
                {
                    await _source.Task;
                }
                finally
                {
                    Release();
                }
            }

            /// <summary>
            /// Faults the source if <paramref name="exception"/> is not <c>null</c> and no other input has signaled
            /// yet.
            /// </summary>
            void IIndexedUnityTaskSink.Complete(int index, Exception exception)
            {
                if (exception != null
                    && Interlocked.CompareExchange(location1: ref _signaled, value: 1, comparand: 0) == 0
                )
                {
                    _source.TrySetException(exception);
                }
            }

            /// <summary>
            /// Counts one input as detached. The last detach completes the source successfully unless a fault signaled
            /// first, then releases the observers' ownership.
            /// </summary>
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

                Release();
            }

            /// <summary>
            /// Gives up one of the two ownerships and returns this instance to the pool on the second call.
            /// </summary>
            private void Release()
            {
                // The pool return waits for the second owner, so a late observer cannot touch a reused state.
                if (Interlocked.Increment(ref _releases) != 2)
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

        /// <summary>
        /// The pooled state behind <see cref="WhenAll{T}(UnityTask{T}[])"/> for tasks with a result.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <remarks>
        /// <para>
        /// <b>Protocol:</b> one <see cref="PooledIndexedUnityTaskObserver{T, TSink}"/> per input calls
        /// <see cref="IIndexedUnityTaskResultSink{T}.Complete"/> with the input's outcome and then
        /// <see cref="IIndexedUnityTaskResultSink{T}.Detach"/>. <c>_remaining</c> counts the inputs not yet detached.
        /// </para>
        /// <para>
        /// <b>Results:</b> a successful input stores its result in <c>_results</c> at its own index. The source is
        /// completed with that array by the last detach and the array is cleared when the instance is released for the
        /// last time.
        /// </para>
        /// <para>
        /// <b>Outcome:</b> <c>_signaled</c> is set once with
        /// <see cref="Interlocked.CompareExchange(ref int, int, int)"/>. The first fault wins and faults the source. If
        /// no input faults, the last detach completes the source successfully.
        /// </para>
        /// <para>
        /// <b>Pooling:</b> <c>_releases</c> counts two owners: the last detaching observer and <see cref="WaitAsync"/>.
        /// The instance returns to <c>s_pool</c> on the second release. The pool holds at most <c>MAX_POOL_SIZE</c>
        /// instances and is guarded by a lock on <c>s_pool</c>.
        /// </para>
        /// </remarks>
        private sealed class WhenAllState<T> : IIndexedUnityTaskResultSink<T>
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly Stack<WhenAllState<T>> s_pool = new();

            private UnityTaskCompletionSource<T[]> _source;
            private T[] _results;
            private int _remaining;
            private int _signaled;
            private int _releases;

            private WhenAllState() { }

            /// <summary>
            /// Takes an instance from the pool or creates one, resets its source, allocates a result array of
            /// <paramref name="count"/> entries and sets <c>_remaining</c> to <paramref name="count"/>.
            /// </summary>
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
                state._releases = 0;
                return state;
            }

            /// <summary>
            /// Awaits the source and releases this instance's <c>WaitAsync</c> ownership when the await ends, including
            /// on a fault.
            /// </summary>
            internal async UnityTask<T[]> WaitAsync()
            {
                try
                {
                    return await _source.Task;
                }
                finally
                {
                    Release();
                }
            }

            /// <summary>
            /// Stores the result at <paramref name="index"/> on success. On a fault, faults the source if no other
            /// input has signaled yet.
            /// </summary>
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

            /// <summary>
            /// Counts one input as detached. The last detach completes the source with the result array unless a fault
            /// signaled first, then releases the observers' ownership.
            /// </summary>
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

                Release();
            }

            /// <summary>
            /// Gives up one of the two ownerships. On the second call it drops the result array and returns this
            /// instance to the pool.
            /// </summary>
            private void Release()
            {
                // The pool return waits for the second owner, so a late observer cannot touch a reused state.
                if (Interlocked.Increment(ref _releases) != 2)
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
