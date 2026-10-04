#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Tasks
{
    internal interface IUnityTaskRunner : IUniTaskSource
    {
        short Version { get; }

        Action MoveNextAction { get; }

        void Prepare(UnityTaskThreadAffinity affinity);

        void SetResult();

        void SetException(Exception exception);
    }

    internal interface IUnityTaskRunner<T> : IUniTaskSource<T>
    {
        short Version { get; }

        Action MoveNextAction { get; }

        void Prepare(UnityTaskThreadAffinity affinity);

        void SetResult(T result);

        void SetException(Exception exception);
    }

    internal struct UnityTaskNoStateMachine : IAsyncStateMachine
    {
        public void MoveNext() { }

        public void SetStateMachine(IAsyncStateMachine stateMachine) { }
    }

    internal sealed class UnityTaskRunner<TStateMachine> : IUnityTaskRunner
        where TStateMachine : IAsyncStateMachine
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<UnityTaskRunner<TStateMachine>> s_pool = new();

        private readonly Action _moveNextAction;
        private UnityTaskSourceCore<object> _core;
        private TStateMachine _stateMachine;

        private UnityTaskRunner()
        {
            _moveNextAction = MoveNext;
        }

        public short Version => _core.Version;

        public Action MoveNextAction => _moveNextAction;

        public static UnityTaskRunner<TStateMachine> Rent()
        {
            lock (s_poolLock)
            {
                return s_pool.Count > 0 ? s_pool.Pop() : new();
            }
        }

        public void SetStateMachine(ref TStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Prepare(UnityTaskThreadAffinity affinity)
            => _core.Prepare(affinity);

        public void SetResult()
            => _core.TrySetResult(null);

        public void SetException(Exception exception)
            => _core.TrySetException(exception);

        public UniTaskStatus GetStatus(short token)
            => _core.GetStatus(token);

        public UniTaskStatus UnsafeGetStatus()
            => _core.UnsafeGetStatus();

        public void OnCompleted(Action<object> continuation, object state, short token)
            => _core.OnCompleted(continuation, state, token);

        public void GetResult(short token)
        {
            _core.ValidateResultAccess(token);

            try
            {
                _core.GetResult(token);
            }
            finally
            {
                Return();
            }
        }

        private void MoveNext()
            => _stateMachine.MoveNext();

        private void Return()
        {
            _core.Reset();
            _stateMachine = default;

            lock (s_poolLock)
            {
                if (s_pool.Count < MAX_POOL_SIZE)
                {
                    s_pool.Push(this);
                }
            }
        }
    }

    internal sealed class UnityTaskRunner<TStateMachine, T> : IUnityTaskRunner<T>
        where TStateMachine : IAsyncStateMachine
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<UnityTaskRunner<TStateMachine, T>> s_pool = new();

        private readonly Action _moveNextAction;
        private UnityTaskSourceCore<T> _core;
        private TStateMachine _stateMachine;

        private UnityTaskRunner()
        {
            _moveNextAction = MoveNext;
        }

        public short Version => _core.Version;

        public Action MoveNextAction => _moveNextAction;

        public static UnityTaskRunner<TStateMachine, T> Rent()
        {
            lock (s_poolLock)
            {
                return s_pool.Count > 0 ? s_pool.Pop() : new();
            }
        }

        public void SetStateMachine(ref TStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Prepare(UnityTaskThreadAffinity affinity)
            => _core.Prepare(affinity);

        public void SetResult(T result)
            => _core.TrySetResult(result);

        public void SetException(Exception exception)
            => _core.TrySetException(exception);

        public UniTaskStatus GetStatus(short token)
            => _core.GetStatus(token);

        public UniTaskStatus UnsafeGetStatus()
            => _core.UnsafeGetStatus();

        public void OnCompleted(Action<object> continuation, object state, short token)
            => _core.OnCompleted(continuation, state, token);

        public T GetResult(short token)
        {
            _core.ValidateResultAccess(token);

            try
            {
                return _core.GetResult(token);
            }
            finally
            {
                Return();
            }
        }

        void IUniTaskSource.GetResult(short token)
            => GetResult(token);

        private void MoveNext()
            => _stateMachine.MoveNext();

        private void Return()
        {
            _core.Reset();
            _stateMachine = default;

            lock (s_poolLock)
            {
                if (s_pool.Count < MAX_POOL_SIZE)
                {
                    s_pool.Push(this);
                }
            }
        }
    }
}

#endif
