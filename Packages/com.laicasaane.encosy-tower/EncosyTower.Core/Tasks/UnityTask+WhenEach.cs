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
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
        public static IUnityTaskAsyncEnumerable<UnityTaskWhenEachResult<T>> WhenEach<T>(IEnumerable<UnityTask<T>> tasks)
        {
            Debugging.ThrowHelper.ThrowIfNull(tasks);
            return new WhenEachEnumerable<T>(tasks);
        }

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
                    enumerator._version = unchecked(enumerator._version + 1);

                    return new EnumeratorLease(enumerator, enumerator._version);
                }
            }

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

                pending?.TrySetResult(true);
            }

            void IUnityTaskResultSink<T, UnityTaskPosition1>.Detach()
            {
                UnityTaskCompletionSource<bool> pending = null;
                CancellationTokenRegistration registration = default;
                var cleaned = false;

                lock (_lock)
                {
                    _completed++;

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

            FINISH_CLEANUP:
                FinishCleanup(registration);
                return result;
            }

            private void Dispose(int version)
            {
                UnityTaskCompletionSource<bool> pending = null;
                CancellationTokenRegistration registration = default;
                var cleaned = false;
                bool returnToPool;

                lock (_lock)
                {
                    ThrowIfInvalidVersion(version);

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

            private void MarkCanceled()
            {
                _canceled = true;
                _disposed = true;
                _canceledToken = _token;
            }

            private bool CleanupIfDetached(out CancellationTokenRegistration registration)
            {
                if (_started == false || _completed == _count)
                {
                    return Cleanup(out registration);
                }

                registration = default;
                return false;
            }

            private bool Cleanup(out CancellationTokenRegistration registration)
            {
                if (_cleaned)
                {
                    registration = default;
                    return false;
                }

                _cleaned = true;
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

            private UnityTaskWhenEachResult<T> GetCurrent(int version)
            {
                lock (_lock)
                {
                    ThrowIfInvalidVersion(version);
                    return Current;
                }
            }

            private void ThrowIfInvalidVersion(int version)
            {
                if (version != _version)
                {
                    ThrowHelper.ThrowEnumeratorDisposed();
                }
            }

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
