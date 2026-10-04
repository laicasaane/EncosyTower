#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Tasks
{
    internal struct UnityTaskSourceCore<T>
    {
        private static readonly Action<object> s_completedSentinel = static _ => { };

        private T _result;
        private ExceptionDispatchInfo _error;
        private bool _errorIsCancellation;
        private int _completed;
        private short _version;
        private UnityTaskThreadAffinity _affinity;
        private Action<object> _continuation;
        private object _state;

        public readonly short Version => _version;

        public void Prepare(UnityTaskThreadAffinity affinity)
        {
            _affinity = affinity;
        }

        public bool TrySetResult(T result)
        {
            if (Interlocked.Increment(ref _completed) != 1)
            {
                return false;
            }

            _result = result;
            SignalCompletion();
            return true;
        }

        public bool TrySetException(Exception exception)
        {
            if (Interlocked.Increment(ref _completed) != 1)
            {
                return false;
            }

            _error = ExceptionDispatchInfo.Capture(exception);
            _errorIsCancellation = exception is OperationCanceledException;
            SignalCompletion();
            return true;
        }

        public UniTaskStatus GetStatus(short token)
        {
            ValidateToken(token);

            if (UnityTaskThreadContext.Matches(_affinity) == false)
            {
                return UniTaskStatus.Pending;
            }

            return UnsafeGetStatus();
        }

        public UniTaskStatus UnsafeGetStatus()
        {
            if (Volatile.Read(ref _continuation) == null || Volatile.Read(ref _completed) == 0)
            {
                return UniTaskStatus.Pending;
            }

            if (_error == null)
            {
                return UniTaskStatus.Succeeded;
            }

            return _errorIsCancellation ? UniTaskStatus.Canceled : UniTaskStatus.Faulted;
        }

        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            Debugging.ThrowHelper.ThrowIfNull(continuation);
            ValidateToken(token);

            var previous = Volatile.Read(ref _continuation);

            if (previous == null)
            {
                _state = state;
                previous = Interlocked.CompareExchange(ref _continuation, continuation, null);
            }

            if (previous == null)
            {
                return;
            }

            if (ReferenceEquals(previous, s_completedSentinel) == false)
            {
                ThrowHelper.ThrowContinuationAlreadyRegistered();
            }

            UnityTaskThreadContext.Run(_affinity, continuation, state);
        }

        public void ValidateResultAccess(short token)
        {
            ValidateToken(token);

            if (Volatile.Read(ref _completed) == 0)
            {
                ThrowHelper.ThrowNotCompleted();
            }
        }

        public T GetResult(short token)
        {
            ValidateResultAccess(token);
            _error?.Throw();
            return _result;
        }

        public void Reset()
        {
            unchecked
            {
                _version += 1;
            }

            _result = default;
            _error = null;
            _errorIsCancellation = false;
            _completed = 0;
            _affinity = UnityTaskThreadAffinity.None;
            _continuation = null;
            _state = null;
        }

        private readonly void ValidateToken(short token)
        {
            if (token != _version)
            {
                ThrowHelper.ThrowTokenMismatch();
            }
        }

        private void SignalCompletion()
        {
            if (Volatile.Read(ref _continuation) == null
                && Interlocked.CompareExchange(ref _continuation, s_completedSentinel, null) == null
            )
            {
                return;
            }

            UnityTaskThreadContext.Run(_affinity, _continuation, _state);
        }
    }
}

#endif
