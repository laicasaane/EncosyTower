using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace UnityEngine.Tasks
{
    public static partial class Awaitables
    {
        public static IAwaitableAsyncEnumerable<AwaitableWhenEachResult<T>> WhenEach<T>(IEnumerable<Awaitable<T>> tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            return new WhenEachEnumerable<T>(tasks);
        }

        public static IAwaitableAsyncEnumerable<AwaitableWhenEachResult<T>> WhenEach<T>(params Awaitable<T>[] tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            return new WhenEachEnumerable<T>(tasks);
        }

        private sealed class WhenEachEnumerable<T>
            : IAwaitableAsyncEnumerable<AwaitableWhenEachResult<T>>
        {
            private readonly IEnumerable<Awaitable<T>> _tasks;

            internal WhenEachEnumerable(IEnumerable<Awaitable<T>> tasks)
            {
                _tasks = tasks;
            }

            public IAwaitableAsyncEnumerator<AwaitableWhenEachResult<T>> GetAsyncEnumerator(
                CancellationToken token = default
            )
                => WhenEachEnumerator<T>.Rent(_tasks, token);
        }

        private sealed class WhenEachEnumerator<T>
            : IAwaitableResultSink<T, AwaitablePosition1>
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly Stack<WhenEachEnumerator<T>> s_pool = new();

            private readonly object _lock = new();
            private IEnumerable<Awaitable<T>> _source;
            private CancellationToken _token;
            private CancellationToken _canceledToken;
            private Awaitable<T>[] _tasks;
            private AwaitableWhenEachResult<T>[] _ring;
            private AwaitableCompletionSource<bool> _pending;
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

            private AwaitableWhenEachResult<T> Current { get; set; }

            internal static IAwaitableAsyncEnumerator<AwaitableWhenEachResult<T>> Rent(
                  IEnumerable<Awaitable<T>> source
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

            void IAwaitableResultSink<T, AwaitablePosition1>.Complete(
                  AwaitablePosition1 position
                , T result
                , Exception exception
            )
            {
                AwaitableCompletionSource<bool> pending = null;

                lock (_lock)
                {
                    if (_disposed)
                    {
                        return;
                    }

                    var outcome = exception == null
                        ? new AwaitableWhenEachResult<T>(result)
                        : new AwaitableWhenEachResult<T>(exception);

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

            void IAwaitableResultSink<T, AwaitablePosition1>.Detach()
            {
                AwaitableCompletionSource<bool> pending = null;
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

                pending?.TrySetResult(false);

                if (cleaned)
                {
                    FinishCleanup(registration);
                }
            }

            private Awaitable<bool> MoveNextAsync(int version)
            {
                Awaitable<bool> result;
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

                    if (_pending != null)
                    {
                        throw new InvalidOperationException(
                            "MoveNextAsync cannot be called again before the previous call completes."
                        );
                    }

                    _pending = new();
                    return _pending.Awaitable;
                }

            FINISH_CLEANUP:
                FinishCleanup(registration);
                return result;
            }

            private void DisposeAsync(int version)
            {
                AwaitableCompletionSource<bool> pending = null;
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

                pending?.TrySetResult(false);

                if (cleaned)
                {
                    FinishCleanup(registration);
                }
                else if (returnToPool)
                {
                    ReturnToPool();
                }
            }

            private void Start()
            {
                _started = true;

                if (_source is Awaitable<T>[] array)
                {
                    _tasks = array;
                    _count = array.Length;
                }
                else
                {
                    _tasks = Materialize(_source, out _count);
                    _ownsTasks = true;
                }

                for (var i = 0; i < _count; i++)
                {
                    DebuggingThrowHelper.ThrowIfNull(_tasks[i]);
                }

                if (_count == 0)
                {
                    _ring = Array.Empty<AwaitableWhenEachResult<T>>();
                    return;
                }

                _ring = ArrayPool<AwaitableWhenEachResult<T>>.Shared.Rent(_count);

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
                    PooledAwaitableObserver<T, AwaitablePosition1, WhenEachEnumerator<T>>.Observe(_tasks[i], this);
                }
            }

            private void CancelEnumeration()
            {
                AwaitableCompletionSource<bool> pending;
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

                pending?.TrySetException(new OperationCanceledException(token));

                if (cleaned)
                {
                    FinishCleanup(registration);
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
                    ArrayPool<AwaitableWhenEachResult<T>>.Shared.Return(_ring);
                }

                _ring = null;

                if (_ownsTasks)
                {
                    Array.Clear(_tasks, 0, _count);
                    ArrayPool<Awaitable<T>>.Shared.Return(_tasks);
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

            private AwaitableWhenEachResult<T> GetCurrent(int version)
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
                    throw new ObjectDisposedException(nameof(WhenEachEnumerator<T>));
                }
            }

            private sealed class EnumeratorLease
                : IAwaitableAsyncEnumerator<AwaitableWhenEachResult<T>>
            {
                private readonly WhenEachEnumerator<T> _enumerator;
                private readonly int _version;
                private int _disposed;

                internal EnumeratorLease(WhenEachEnumerator<T> enumerator, int version)
                {
                    _enumerator = enumerator;
                    _version = version;
                }

                public AwaitableWhenEachResult<T> Current
                    => Volatile.Read(ref _disposed) == 0 ? _enumerator.GetCurrent(_version) : default;

                public Awaitable<bool> MoveNextAsync()
                    => Volatile.Read(ref _disposed) == 0 ? _enumerator.MoveNextAsync(_version) : FromResult(false);

                public Awaitable DisposeAsync()
                {
                    if (Interlocked.Exchange(location1: ref _disposed, value: 1) == 0)
                    {
                        _enumerator.DisposeAsync(_version);
                    }

                    return CompletedTask;
                }
            }
        }
    }
}
