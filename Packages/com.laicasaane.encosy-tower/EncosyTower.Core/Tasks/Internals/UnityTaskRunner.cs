using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EncosyTower.Tasks
{
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
    using ITaskSource = Cysharp.Threading.Tasks.IUniTaskSource;
    using TaskSourceStatus = Cysharp.Threading.Tasks.UniTaskStatus;
#else
    using ITaskSource = IUnityTaskSource;
    using TaskSourceStatus = UnityTaskStatus;
#endif

    /// <summary>
    /// The task source of an <c>async UnityTask</c> method, as seen by <see cref="UnityTaskAsyncMethodBuilder"/>
    /// without its state machine type.
    /// </summary>
    internal interface IUnityTaskRunner : ITaskSource
    {
        short Version { get; }

        Action MoveNextAction { get; }

        void Prepare(UnityTaskThreadAffinity affinity);

        void SetResult();

        void SetException(Exception exception);
    }

    /// <summary>
    /// The task source of an <c>async UnityTask&lt;T&gt;</c> method, as seen by
    /// <see cref="UnityTaskAsyncMethodBuilder{T}"/> without its state machine type.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
    internal interface IUnityTaskRunner<T> : Cysharp.Threading.Tasks.IUniTaskSource<T>
#else
    internal interface IUnityTaskRunner<T> : IUnityTaskSource<T>
#endif
    {
        short Version { get; }

        Action MoveNextAction { get; }

        void Prepare(UnityTaskThreadAffinity affinity);

        void SetResult(T result);

        void SetException(Exception exception);
    }

    /// <summary>
    /// Placeholder state machine for a runner created before any <c>await</c> suspended: when the method completes or
    /// throws synchronously, or when the compiler reads <c>Task</c> first. Such a runner only carries the outcome.
    /// </summary>
    internal readonly struct UnityTaskNoStateMachine : IAsyncStateMachine
    {
        public readonly void MoveNext()
        {
        }

        public readonly void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }

    /// <summary>
    /// Pooled box that holds an async method's state machine and its <see cref="UnityTaskSourceCore{T}"/>; it is both
    /// the continuation target and the task source.
    /// </summary>
    /// <typeparam name="TStateMachine">The compiler-generated state machine type.</typeparam>
    /// <remarks>
    /// <para>
    /// The builder rents a runner on the first suspension and copies the state machine into it once.
    /// <c>MoveNextAction</c> is created once per runner, so awaits do not allocate delegates.
    /// </para>
    /// <para>
    /// <see cref="GetResult"/> validates the token, reads the outcome and then returns the runner to the pool. Under
    /// IL2CPP the return is scheduled at <see cref="UnityTaskTiming.LastPostLateUpdate"/> instead, the same delay
    /// UniTask uses to work around an IL2CPP issue with reusing a state machine box that is still on the call stack.
    /// </para>
    /// </remarks>
    internal sealed class UnityTaskRunner<TStateMachine> : IUnityTaskRunner
        where TStateMachine : IAsyncStateMachine
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<UnityTaskRunner<TStateMachine>> s_pool = new();

        private readonly Action _moveNextAction;
#if ENABLE_IL2CPP
        private readonly Action _returnAction;
#endif
        private UnityTaskSourceCore<object> _core;
        private TStateMachine _stateMachine;

        private UnityTaskRunner()
        {
            _moveNextAction = MoveNext;
#if ENABLE_IL2CPP
            _returnAction = Return;
#endif
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

        public TaskSourceStatus GetStatus(short token)
            => (TaskSourceStatus)_core.GetStatus(token);

        public TaskSourceStatus UnsafeGetStatus()
            => (TaskSourceStatus)_core.UnsafeGetStatus();

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
#if ENABLE_IL2CPP
                PlayerLoopScheduler.Schedule(UnityTaskTiming.LastPostLateUpdate, _returnAction);
#else
                Return();
#endif
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

    /// <summary>
    /// Pooled box that holds an <c>async UnityTask&lt;T&gt;</c> method's state machine and its
    /// <see cref="UnityTaskSourceCore{T}"/>. Works as <see cref="UnityTaskRunner{TStateMachine}"/> does.
    /// </summary>
    /// <typeparam name="TStateMachine">The compiler-generated state machine type.</typeparam>
    /// <typeparam name="T">The type of the result.</typeparam>
    internal sealed class UnityTaskRunner<TStateMachine, T> : IUnityTaskRunner<T>
        where TStateMachine : IAsyncStateMachine
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<UnityTaskRunner<TStateMachine, T>> s_pool = new();

        private readonly Action _moveNextAction;
#if ENABLE_IL2CPP
        private readonly Action _returnAction;
#endif
        private UnityTaskSourceCore<T> _core;
        private TStateMachine _stateMachine;

        private UnityTaskRunner()
        {
            _moveNextAction = MoveNext;
#if ENABLE_IL2CPP
            _returnAction = Return;
#endif
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

        public TaskSourceStatus GetStatus(short token)
            => (TaskSourceStatus)_core.GetStatus(token);

        public TaskSourceStatus UnsafeGetStatus()
            => (TaskSourceStatus)_core.UnsafeGetStatus();

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
#if ENABLE_IL2CPP
                PlayerLoopScheduler.Schedule(UnityTaskTiming.LastPostLateUpdate, _returnAction);
#else
                Return();
#endif
            }
        }

        void ITaskSource.GetResult(short token)
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
