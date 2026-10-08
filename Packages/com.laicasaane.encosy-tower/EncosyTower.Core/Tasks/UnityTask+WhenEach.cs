using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Tasks;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Creates an asynchronous sequence that yields the outcome of each task in <paramref name="tasks"/> in
        /// completion order.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <param name="tasks">The tasks to observe.</param>
        /// <returns>A lazy sequence of outcomes in completion order.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> lazy until the first <see cref="IUnityTaskAsyncEnumerator{T}.MoveNextAsync"/>. Yields
        /// results in completion order; a fault or cancellation of an input appears as
        /// <see cref="UnityTaskWhenEachResult{T}.Exception"/> and does not end the sequence. Cancelling the
        /// enumeration token makes the pending and every later <c>MoveNextAsync</c> throw
        /// <c>new OperationCanceledException(token)</c>, until the sequence has ended or the enumerator is disposed.
        /// <see cref="IUnityTaskAsyncEnumerator{T}.DisposeAsync"/> completes a pending move with <c>false</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> each <c>MoveNextAsync</c> awaiter resumes on the kind of thread that called it: the main
        /// thread when called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting a returned task or a copy more than once, calling <c>GetResult</c> twice, or passing the same
        /// task (or a copy) more than once: may throw <see cref="InvalidOperationException"/>, return a stale
        /// result, never complete, or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>MoveNextAsync</c> again before the previous call completes: development builds
        /// (<c>UNITY_EDITOR</c>, <c>DEBUG</c>, <c>ENCOSY_RUNTIME_CHECKS</c>) throw
        /// <see cref="InvalidOperationException"/>. Release builds may lose a result or never complete.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread: Unity throws or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An input obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: that input
        /// follows native-library behaviour.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WhenEach</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        public static IUnityTaskAsyncEnumerable<UnityTaskWhenEachResult<T>> WhenEach<T>(params UnityTask<T>[] tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);
            return new WhenEachEnumerable<T>(tasks);
        }

        /// <summary>
        /// Creates an asynchronous sequence that yields the outcome of each task in <paramref name="tasks"/> in
        /// completion order.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <param name="tasks">The tasks to observe. The collection is enumerated at the first move.</param>
        /// <returns>A lazy sequence of outcomes in completion order.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> lazy until the first <see cref="IUnityTaskAsyncEnumerator{T}.MoveNextAsync"/>. Yields
        /// results in completion order; a fault or cancellation of an input appears as
        /// <see cref="UnityTaskWhenEachResult{T}.Exception"/> and does not end the sequence. Cancelling the
        /// enumeration token makes the pending and every later <c>MoveNextAsync</c> throw
        /// <c>new OperationCanceledException(token)</c>, until the sequence has ended or the enumerator is disposed.
        /// <see cref="IUnityTaskAsyncEnumerator{T}.DisposeAsync"/> completes a pending move with <c>false</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> each <c>MoveNextAsync</c> awaiter resumes on the kind of thread that called it: the main
        /// thread when called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting a returned task or a copy more than once, calling <c>GetResult</c> twice, or passing the same
        /// task (or a copy) more than once: may throw <see cref="InvalidOperationException"/>, return a stale
        /// result, never complete, or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>MoveNextAsync</c> again before the previous call completes: development builds
        /// (<c>UNITY_EDITOR</c>, <c>DEBUG</c>, <c>ENCOSY_RUNTIME_CHECKS</c>) throw
        /// <see cref="InvalidOperationException"/>. Release builds may lose a result or never complete.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread: Unity throws or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An input obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: that input
        /// follows native-library behaviour.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WhenEach</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        public static IUnityTaskAsyncEnumerable<UnityTaskWhenEachResult<T>> WhenEach<T>(IEnumerable<UnityTask<T>> tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);
            return new WhenEachEnumerable<T>(tasks);
        }

        /// <summary>
        /// The sequence returned by <see cref="WhenEach{T}(IEnumerable{UnityTask{T}})"/>. Each call to
        /// <see cref="GetAsyncEnumerator"/> rents a new <see cref="WhenEachEnumerator{T}"/> over the same inputs.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <remarks>
        /// Holds the input sequence only. Nothing is read from it until the first <c>MoveNextAsync</c> of an
        /// enumerator.
        /// </remarks>
        private sealed class WhenEachEnumerable<T> : IUnityTaskAsyncEnumerable<UnityTaskWhenEachResult<T>>
        {
            private readonly IEnumerable<UnityTask<T>> _tasks;

            internal WhenEachEnumerable(IEnumerable<UnityTask<T>> tasks)
            {
                _tasks = tasks;
            }

            public IUnityTaskAsyncEnumerator<UnityTaskWhenEachResult<T>> GetAsyncEnumerator(
                CancellationToken token = default
            )
                => WhenEachEnumerator<T>.Rent(_tasks, token);
        }

        /// <summary>
        /// The pooled state of one enumeration started by <see cref="WhenEachEnumerable{T}"/>. It observes every input
        /// and yields their outcomes in completion order.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <remarks>
        /// <para>
        /// <b>Lease:</b> callers never hold this object. <see cref="Rent"/> returns an <see cref="EnumeratorLease"/>
        /// that holds this instance and the <c>_version</c> it had when rented. Every call through a lease checks the
        /// version under <c>_lock</c> and throws once the instance has been returned to the pool and rented again.
        /// </para>
        /// <para>
        /// <b>Start:</b> nothing is read or observed until the first <c>MoveNextAsync</c> calls <see cref="Start"/>. It
        /// reads the inputs into <c>_tasks</c>, rents <c>_ring</c> with one slot per input, registers the cancellation
        /// token, and attaches one <see cref="PooledUnityTaskObserver{T, TPosition, TSink}"/> per input.
        /// </para>
        /// <para>
        /// <b>Delivery:</b> all state is guarded by <c>_lock</c>. An input's outcome goes straight to the pending
        /// <c>MoveNextAsync</c> source when there is one, and is otherwise appended to the ring buffer (<c>_head</c> is
        /// the oldest entry, <c>_queued</c> the number of entries). <c>_completed</c> counts the inputs detached so far
        /// and <c>_count</c> is the number of inputs. The sequence ends when <c>_completed</c> equals <c>_count</c> and
        /// the ring is empty.
        /// </para>
        /// <para>
        /// <b>Cancellation and disposal:</b> <see cref="MarkCanceled"/> and disposal both set <c>_disposed</c>, after
        /// which completions are dropped. Cancellation also sets <c>_canceled</c> so that every later
        /// <c>MoveNextAsync</c> returns a canceled task.
        /// </para>
        /// <para>
        /// <b>Cleanup:</b> <see cref="Cleanup"/> returns the rented arrays and clears the references. It runs once,
        /// guarded by <c>_cleaned</c>, and only when no observer can still call in: before <see cref="Start"/>, or
        /// after every input has detached. <see cref="FinishCleanup"/> then disposes the cancellation registration
        /// outside the lock.
        /// </para>
        /// <para>
        /// <b>Pool return:</b> <see cref="ClaimReturn"/> allows the single return to <c>s_pool</c> only when
        /// <c>_cleaned</c> and <c>_released</c> are set, <c>_registrationPending</c> is clear and <c>_returned</c> is
        /// not yet set. <c>_released</c> is set by disposal of the lease. The pool holds at most <c>MAX_POOL_SIZE</c>
        /// instances.
        /// </para>
        /// </remarks>
        private sealed class WhenEachEnumerator<T> : IUnityTaskResultSink<T, UnityTaskPosition1>
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly Stack<WhenEachEnumerator<T>> s_pool = new();

            private readonly object _lock = new();
            private IEnumerable<UnityTask<T>> _source;
            private CancellationToken _token;
            private CancellationToken _canceledToken;
            private UnityTask<T>[] _tasks;
            private UnityTaskWhenEachResult<T>[] _ring;
            private UnityTaskCompletionSource<bool> _pending;
            private CancellationTokenRegistration _registration;
            private int _count;
            private int _completed;
            private int _head;
            private int _queued;
            private bool _ownsTasks;
            private bool _started;
            private bool _disposed;
            private bool _canceled;
            private bool _cleaned;
            private bool _registrationPending;
            private bool _released;
            private bool _returned;
            private int _version;

            private WhenEachEnumerator() { }

            private UnityTaskWhenEachResult<T> Current { get; set; }

            /// <summary>
            /// Takes an instance from the pool or creates one, resets all of its state under its lock, advances
            /// <c>_version</c> and returns a new <see cref="EnumeratorLease"/> for that version.
            /// </summary>
            internal static IUnityTaskAsyncEnumerator<UnityTaskWhenEachResult<T>> Rent(
                  IEnumerable<UnityTask<T>> source
                , CancellationToken token
            )
            {
                WhenEachEnumerator<T> enumerator;

                lock (s_pool)
                {
                    enumerator = s_pool.Count > 0 ? s_pool.Pop() : new();
                }

                lock (enumerator._lock)
                {
                    enumerator._source = source;
                    enumerator._token = token;
                    enumerator._canceledToken = default;
                    enumerator._tasks = null;
                    enumerator._ring = null;
                    enumerator._pending = null;
                    enumerator._registration = default;
                    enumerator._count = 0;
                    enumerator._completed = 0;
                    enumerator._head = 0;
                    enumerator._queued = 0;
                    enumerator._ownsTasks = false;
                    enumerator._started = false;
                    enumerator._disposed = false;
                    enumerator._canceled = false;
                    enumerator._cleaned = false;
                    enumerator._registrationPending = false;
                    enumerator._released = false;
                    enumerator._returned = false;
                    enumerator.Current = default;
                    // A new version makes leases from the previous use of this instance fail.
                    enumerator._version = unchecked(enumerator._version + 1);

                    return new EnumeratorLease(enumerator, enumerator._version);
                }
            }

            /// <summary>
            /// Delivers the outcome of one input: to the pending <c>MoveNextAsync</c> if there is one, otherwise to the
            /// ring buffer. Outcomes arriving after disposal or cancellation are dropped.
            /// </summary>
            void IUnityTaskResultSink<T, UnityTaskPosition1>.Complete(
                  UnityTaskPosition1 position
                , T result
                , Exception exception
            )
            {
                UnityTaskCompletionSource<bool> pending = null;

                lock (_lock)
                {
                    if (_disposed)
                    {
                        return;
                    }

                    var outcome = exception == null
                        ? new UnityTaskWhenEachResult<T>(result)
                        : new UnityTaskWhenEachResult<T>(exception);

                    // A waiting MoveNextAsync takes the outcome directly; otherwise it is queued.
                    if (_pending != null)
                    {
                        Current = outcome;
                        pending = _pending;
                        _pending = null;
                    }
                    else
                    {
                        _ring[(_head + _queued) % _ring.Length] = outcome;
                        _queued++;
                    }
                }

                // Completed outside the lock so the continuation does not run while it is held.
                pending?.TrySetResult(true);
            }

            /// <summary>
            /// Counts one input as detached. When it was the last input, nothing is queued and a <c>MoveNextAsync</c>
            /// is waiting, ends that call with <c>false</c> and cleans up. When the enumerator is already disposed,
            /// cleans up once all inputs have detached.
            /// </summary>
            void IUnityTaskResultSink<T, UnityTaskPosition1>.Detach()
            {
                UnityTaskCompletionSource<bool> pending = null;
                CancellationTokenRegistration registration = default;
                var cleaned = false;

                lock (_lock)
                {
                    _completed++;

                    // The last input detached with nothing queued, so the sequence ends for the waiting caller.
                    if (_completed == _count && _queued == 0 && _pending != null)
                    {
                        pending = _pending;
                        _pending = null;
                        cleaned = Cleanup(out registration);
                    }
                    else if (_disposed)
                    {
                        cleaned = CleanupIfDetached(out registration);
                    }
                }

                try
                {
                    pending?.TrySetResult(false);
                }
                finally
                {
                    if (cleaned)
                    {
                        FinishCleanup(registration);
                    }
                }
            }

            /// <summary>
            /// Implements <c>MoveNextAsync</c> for the lease of <paramref name="version"/>. It returns, in order: a
            /// canceled task after cancellation, <c>false</c> after disposal, the next queued outcome, <c>false</c>
            /// when all inputs have completed, a canceled task when the token is canceled, or a task completed by the
            /// next arriving outcome.
            /// </summary>
            private UnityTask<bool> MoveNextAsync(int version)
            {
                UnityTask<bool> result;
                CancellationTokenRegistration registration;

                lock (_lock)
                {
                    ThrowIfInvalidVersion(version);

                    if (_canceled)
                    {
                        return FromCanceled<bool>(_canceledToken);
                    }

                    if (_disposed)
                    {
                        return FromResult(false);
                    }

                    // Canceled before the first call: nothing was started, so cleanup runs at once.
                    if (_started == false && _token.IsCancellationRequested)
                    {
                        MarkCanceled();
                        result = FromCanceled<bool>(_canceledToken);

                        if (Cleanup(out registration))
                        {
                            goto FINISH_CLEANUP;
                        }

                        return result;
                    }

                    if (_started == false)
                    {
                        Start();

                        if (_canceled)
                        {
                            return FromCanceled<bool>(_canceledToken);
                        }
                    }

                    if (_queued > 0)
                    {
                        Current = _ring[_head];
                        _ring[_head] = default;
                        _head = (_head + 1) % _ring.Length;
                        _queued--;
                        return FromResult(true);
                    }

                    if (_completed == _count)
                    {
                        result = FromResult(false);

                        if (Cleanup(out registration))
                        {
                            goto FINISH_CLEANUP;
                        }

                        return result;
                    }

                    if (_token.IsCancellationRequested)
                    {
                        MarkCanceled();
                        result = FromCanceled<bool>(_canceledToken);

                        if (CleanupIfDetached(out registration))
                        {
                            goto FINISH_CLEANUP;
                        }

                        return result;
                    }

                    ThrowHelper.ThrowIfConcurrentMoveNext(_pending != null);

                    _pending = new();
                    return _pending.Task;
                }

            // Cleanup finishes after the lock is released; the cancellation callback takes the same lock.
            FINISH_CLEANUP:
                FinishCleanup(registration);
                return result;
            }

            /// <summary>
            /// Disposes the lease of <paramref name="version"/>. Ends a pending <c>MoveNextAsync</c> with <c>false</c>,
            /// marks the enumerator released and cleans up once every input has detached.
            /// </summary>
            private void Dispose(int version)
            {
                UnityTaskCompletionSource<bool> pending = null;
                CancellationTokenRegistration registration = default;
                var cleaned = false;
                bool returnToPool;

                lock (_lock)
                {
                    ThrowIfInvalidVersion(version);

                    // Released only means the owner is done; the pool return still waits for cleanup.
                    _released = true;

                    if (_disposed == false)
                    {
                        _disposed = true;
                        pending = _pending;
                        _pending = null;
                        cleaned = CleanupIfDetached(out registration);
                    }

                    returnToPool = ClaimReturn();
                }

                try
                {
                    pending?.TrySetResult(false);
                }
                finally
                {
                    if (cleaned)
                    {
                        FinishCleanup(registration);
                    }
                    else if (returnToPool)
                    {
                        ReturnToPool();
                    }
                }
            }

            /// <summary>
            /// Reads the inputs, rents the ring buffer, registers the cancellation token and starts observing every
            /// input. An <see cref="UnityTask{T}"/> array is used in place; any other sequence is copied into a rented
            /// array that <see cref="Cleanup"/> returns. Called once, under the lock, by the first
            /// <c>MoveNextAsync</c>.
            /// </summary>
            private void Start()
            {
                _started = true;

                if (_source is UnityTask<T>[] array)
                {
                    _tasks = array;
                    _count = array.Length;
                }
                else
                {
                    _tasks = Materialize(_source, out _count);
                    _ownsTasks = true;
                }

                if (_count == 0)
                {
                    _ring = Array.Empty<UnityTaskWhenEachResult<T>>();
                    return;
                }

                _ring = ArrayPool<UnityTaskWhenEachResult<T>>.Shared.Rent(_count);

                if (_token.CanBeCanceled)
                {
                    _registration = AwaitableCancellation.Register(
                          _token
                        , static state => ((WhenEachEnumerator<T>)state).CancelEnumeration()
                        , this
                    );
                }

                for (var i = 0; i < _count; i++)
                {
                    PooledUnityTaskObserver<T, UnityTaskPosition1, WhenEachEnumerator<T>>.Observe(_tasks[i], this);
                }
            }

            /// <summary>
            /// Runs when the enumeration token is canceled. Marks the enumerator canceled, ends the pending
            /// <c>MoveNextAsync</c> with an <see cref="OperationCanceledException"/> and cleans up once every input has
            /// detached.
            /// </summary>
            private void CancelEnumeration()
            {
                UnityTaskCompletionSource<bool> pending;
                CancellationTokenRegistration registration;
                CancellationToken token;
                bool cleaned;

                lock (_lock)
                {
                    if (_disposed || _cleaned)
                    {
                        return;
                    }

                    MarkCanceled();
                    token = _canceledToken;
                    pending = _pending;
                    _pending = null;
                    cleaned = CleanupIfDetached(out registration);
                }

                try
                {
                    pending?.TrySetException(new OperationCanceledException(token));
                }
                finally
                {
                    if (cleaned)
                    {
                        FinishCleanup(registration);
                    }
                }
            }

            /// <summary>
            /// Marks the enumerator canceled and disposed and keeps the token for the
            /// <see cref="OperationCanceledException"/> that later calls report.
            /// </summary>
            private void MarkCanceled()
            {
                _canceled = true;
                _disposed = true;
                _canceledToken = _token;
            }

            /// <summary>
            /// Runs <see cref="Cleanup"/> if no observer can still call in: the enumerator never started or all inputs
            /// have detached. Returns whether this call performed the cleanup.
            /// </summary>
            private bool CleanupIfDetached(out CancellationTokenRegistration registration)
            {
                if (_started == false || _completed == _count)
                {
                    return Cleanup(out registration);
                }

                registration = default;
                return false;
            }

            /// <summary>
            /// Returns the rented arrays and clears the references, once. Returns <c>true</c> and hands out the
            /// cancellation registration when this call performed the cleanup; the caller must pass it to
            /// <see cref="FinishCleanup"/> after leaving the lock.
            /// </summary>
            private bool Cleanup(out CancellationTokenRegistration registration)
            {
                if (_cleaned)
                {
                    registration = default;
                    return false;
                }

                _cleaned = true;
                // Blocks the pool return until FinishCleanup has disposed the registration.
                _registrationPending = true;
                registration = _registration;
                _registration = default;

                if (_ring is { Length: > 0 })
                {
                    Array.Clear(_ring, 0, _ring.Length);
                    ArrayPool<UnityTaskWhenEachResult<T>>.Shared.Return(_ring);
                }

                _ring = null;

                if (_ownsTasks)
                {
                    Array.Clear(_tasks, 0, _count);
                    ArrayPool<UnityTask<T>>.Shared.Return(_tasks);
                    _tasks = null;
                }

                _source = null;
                _token = default;
                return true;
            }

            /// <summary>
            /// Disposes the cancellation registration outside the lock, clears <c>_registrationPending</c> and returns
            /// the instance to the pool if <see cref="ClaimReturn"/> allows it.
            /// </summary>
            private void FinishCleanup(CancellationTokenRegistration registration)
            {
                registration.Dispose();

                bool returnToPool;

                lock (_lock)
                {
                    _registrationPending = false;
                    returnToPool = ClaimReturn();
                }

                if (returnToPool)
                {
                    ReturnToPool();
                }
            }

            /// <summary>
            /// Returns <c>true</c> exactly once, when cleanup is done, the lease is disposed and no registration is
            /// pending. Must be called under the lock.
            /// </summary>
            private bool ClaimReturn()
            {
                if (_cleaned == false || _released == false || _registrationPending || _returned)
                {
                    return false;
                }

                _returned = true;
                Current = default;
                return true;
            }

            /// <summary>
            /// Pushes this instance to <c>s_pool</c> unless the pool is full. Called outside <c>_lock</c> after
            /// <see cref="ClaimReturn"/> returned <c>true</c>.
            /// </summary>
            private void ReturnToPool()
            {
                lock (s_pool)
                {
                    if (s_pool.Count < MAX_POOL_SIZE)
                    {
                        s_pool.Push(this);
                    }
                }
            }

            /// <summary>
            /// Returns the outcome last delivered by <c>MoveNextAsync</c> for the lease of <paramref name="version"/>.
            /// </summary>
            private UnityTaskWhenEachResult<T> GetCurrent(int version)
            {
                lock (_lock)
                {
                    ThrowIfInvalidVersion(version);
                    return Current;
                }
            }

            /// <summary>
            /// Throws when <paramref name="version"/> is not the version this instance was last rented with.
            /// </summary>
            private void ThrowIfInvalidVersion(int version)
            {
                if (version != _version)
                {
                    ThrowHelper.ThrowEnumeratorDisposed();
                }
            }

            /// <summary>
            /// The enumerator object that callers receive. It forwards to the pooled
            /// <see cref="WhenEachEnumerator{T}"/> with the version it was created for, so a stale lease cannot reach
            /// an instance that was returned and rented again.
            /// </summary>
            /// <remarks>
            /// <c>_disposed</c> is set once with <see cref="Interlocked.Exchange(ref int, int)"/>, so only the first
            /// <c>DisposeAsync</c> reaches <see cref="WhenEachEnumerator{T}.Dispose"/>. After that,
            /// <c>MoveNextAsync</c> returns <c>false</c> and <c>Current</c> returns <c>default</c> without touching the
            /// pooled instance.
            /// </remarks>
            private sealed class EnumeratorLease : IUnityTaskAsyncEnumerator<UnityTaskWhenEachResult<T>>
            {
                private readonly WhenEachEnumerator<T> _enumerator;
                private readonly int _version;
                private int _disposed;

                internal EnumeratorLease(WhenEachEnumerator<T> enumerator, int version)
                {
                    _enumerator = enumerator;
                    _version = version;
                }

                public UnityTaskWhenEachResult<T> Current
                    => Volatile.Read(ref _disposed) == 0 ? _enumerator.GetCurrent(_version) : default;

                public UnityTask<bool> MoveNextAsync()
                    => Volatile.Read(ref _disposed) == 0 ? _enumerator.MoveNextAsync(_version) : FromResult(false);

                public UnityTask DisposeAsync()
                {
                    if (Interlocked.Exchange(location1: ref _disposed, value: 1) == 0)
                    {
                        _enumerator.Dispose(_version);
                    }

                    return CompletedTask;
                }
            }
        }
    }
}
