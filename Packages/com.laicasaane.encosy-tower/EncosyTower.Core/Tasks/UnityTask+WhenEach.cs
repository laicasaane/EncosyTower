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
        /// <c>new OperationCanceledException(token)</c>. <see cref="IUnityTaskAsyncEnumerator{T}.DisposeAsync"/>
        /// completes a pending move with <c>false</c>.
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
        /// <c>new OperationCanceledException(token)</c>. <see cref="IUnityTaskAsyncEnumerator{T}.DisposeAsync"/>
        /// completes a pending move with <c>false</c>.
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
        public static IUnityTaskAsyncEnumerable<UnityTaskWhenEachResult<T>> WhenEach<T>(
            IEnumerable<UnityTask<T>> tasks
        )
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
            private int _version;
            private int _returned;

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
                enumerator._returned = 0;
                enumerator.Current = default;
                enumerator._version = unchecked(enumerator._version + 1);

                return new EnumeratorLease(enumerator, enumerator._version);
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

                lock (_lock)
                {
                    _completed++;

                    if (_completed == _count && _queued == 0 && _pending != null)
                    {
                        pending = _pending;
                        _pending = null;
                        Cleanup();
                    }
                    else if (_disposed)
                    {
                        CleanupIfDetached();
                    }
                }

                pending?.TrySetResult(false);
            }

            private UnityTask<bool> MoveNextAsync(int version)
            {
                ThrowIfInvalidVersion(version);

                lock (_lock)
                {
                    if (_canceled)
                    {
                        return FromCanceled<bool>(_canceledToken);
                    }

                    if (_disposed)
                    {
                        return FromResult(false);
                    }

                    if (_started == false)
                    {
                        if (_token.IsCancellationRequested)
                        {
                            MarkCanceled();
                            Cleanup();
                            return FromCanceled<bool>(_canceledToken);
                        }

                        Start();
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
                        Cleanup();
                        return FromResult(false);
                    }

                    if (_token.IsCancellationRequested)
                    {
                        MarkCanceled();
                        CleanupIfDetached();
                        return FromCanceled<bool>(_canceledToken);
                    }

                    ThrowHelper.ThrowIfConcurrentMoveNext(_pending != null);

                    _pending = new();
                    return _pending.Task;
                }
            }

            private UnityTask DisposeAsync(int version)
            {
                ThrowIfInvalidVersion(version);

                UnityTaskCompletionSource<bool> pending;

                lock (_lock)
                {
                    if (_disposed)
                    {
                        return CompletedTask;
                    }

                    _disposed = true;
                    pending = _pending;
                    _pending = null;
                    CleanupIfDetached();
                }

                pending?.TrySetResult(false);
                return CompletedTask;
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
                CancellationToken token;

                lock (_lock)
                {
                    if (_disposed)
                    {
                        return;
                    }

                    MarkCanceled();
                    token = _canceledToken;
                    pending = _pending;
                    _pending = null;
                    CleanupIfDetached();
                }

                pending?.TrySetException(new OperationCanceledException(token));
            }

            private void MarkCanceled()
            {
                _canceled = true;
                _disposed = true;
                _canceledToken = _token;
            }

            private void CleanupIfDetached()
            {
                if (_started == false || _completed == _count)
                {
                    Cleanup();
                }
            }

            private void Cleanup()
            {
                if (_cleaned)
                {
                    return;
                }

                _cleaned = true;
                _registration.Dispose();
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
                Current = default;

                if (Interlocked.Exchange(ref _returned, 1) == 0)
                {
                    lock (s_pool)
                    {
                        if (s_pool.Count < MAX_POOL_SIZE)
                        {
                            s_pool.Push(this);
                        }
                    }
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

                internal EnumeratorLease(WhenEachEnumerator<T> enumerator, int version)
                {
                    _enumerator = enumerator;
                    _version = version;
                }

                public UnityTaskWhenEachResult<T> Current
                {
                    get
                    {
                        _enumerator.ThrowIfInvalidVersion(_version);
                        return _enumerator.Current;
                    }
                }

                public UnityTask<bool> MoveNextAsync()
                    => _enumerator.MoveNextAsync(_version);

                public UnityTask DisposeAsync()
                    => _enumerator.DisposeAsync(_version);
            }
        }
    }
}
