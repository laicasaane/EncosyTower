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
            private int _version;
            private int _returned;

            private WhenEachEnumerator() { }

            internal static IAwaitableAsyncEnumerator<AwaitableWhenEachResult<T>> Rent(
                  IEnumerable<Awaitable<T>> source
                , CancellationToken token
            )
            {
                WhenEachEnumerator<T> enumerator;

                lock (s_pool)
                {
                    enumerator = s_pool.Count > 0 ? s_pool.Pop() : new WhenEachEnumerator<T>();
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

            private AwaitableWhenEachResult<T> Current { get; set; }

            private Awaitable<bool> MoveNextAsync(int version)
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

                    if (_pending != null)
                    {
                        throw new InvalidOperationException(
                            "MoveNextAsync cannot be called again before the previous call completes."
                        );
                    }

                    _pending = new();
                    return _pending.Awaitable;
                }
            }

            private Awaitable DisposeAsync(int version)
            {
                ThrowIfInvalidVersion(version);
                AwaitableCompletionSource<bool> pending;

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
                    throw new ObjectDisposedException(nameof(WhenEachEnumerator<T>));
                }
            }

            private sealed class EnumeratorLease
                : IAwaitableAsyncEnumerator<AwaitableWhenEachResult<T>>
            {
                private readonly WhenEachEnumerator<T> _enumerator;
                private readonly int _version;

                internal EnumeratorLease(WhenEachEnumerator<T> enumerator, int version)
                {
                    _enumerator = enumerator;
                    _version = version;
                }

                public AwaitableWhenEachResult<T> Current
                {
                    get
                    {
                        _enumerator.ThrowIfInvalidVersion(_version);
                        return _enumerator.Current;
                    }
                }

                public Awaitable<bool> MoveNextAsync()
                    => _enumerator.MoveNextAsync(_version);

                public Awaitable DisposeAsync()
                    => _enumerator.DisposeAsync(_version);
            }
        }
    }
}
