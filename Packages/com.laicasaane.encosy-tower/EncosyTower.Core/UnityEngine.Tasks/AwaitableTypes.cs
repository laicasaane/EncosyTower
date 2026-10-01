using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace UnityEngine.Tasks
{
    public enum AwaitableDelayType
    {
        DeltaTime,
        UnscaledDeltaTime,
        Realtime,
    }

    public enum AwaitablePlayerLoopTiming
    {
        Initialization,
        LastInitialization,
        EarlyUpdate,
        LastEarlyUpdate,
        FixedUpdate,
        LastFixedUpdate,
        PreUpdate,
        LastPreUpdate,
        Update,
        LastUpdate,
        PreLateUpdate,
        LastPreLateUpdate,
        PostLateUpdate,
        LastPostLateUpdate,
        TimeUpdate,
        LastTimeUpdate,
    }

    public interface IAwaitableAsyncEnumerable<out T>
    {
        IAwaitableAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token = default);
    }

    public interface IAwaitableAsyncEnumerator<out T>
    {
        T Current { get; }

        Awaitable<bool> MoveNextAsync();

        Awaitable DisposeAsync();
    }

    public readonly struct AwaitableWhenEachResult<T>
    {
        private readonly T _result;
        private readonly Exception _exception;

        public AwaitableWhenEachResult(T result)
        {
            _result = result;
            _exception = null;
        }

        public AwaitableWhenEachResult(Exception exception)
        {
            EncosyTower.Debugging.ThrowHelper.ThrowIfNull(exception);
            _result = default;
            _exception = exception;
        }

        public T Result => _result;

        public Exception Exception => _exception;

        public bool IsCompletedSuccessfully => _exception == null;

        public bool IsFaulted => _exception != null;

        public void TryThrow()
        {
            if (_exception != null)
            {
                ExceptionDispatchInfo.Capture(_exception).Throw();
            }
        }

        public T GetResult()
        {
            if (_exception != null)
            {
                ExceptionDispatchInfo.Capture(_exception).Throw();
            }

            return _result;
        }

        public override string ToString()
            => _exception == null
                ? _result?.ToString() ?? string.Empty
                : $"Exception{{{_exception.Message}}}";
    }
}
