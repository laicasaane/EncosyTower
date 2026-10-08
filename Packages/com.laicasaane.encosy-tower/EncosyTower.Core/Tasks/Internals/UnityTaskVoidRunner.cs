using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// The box of an <c>async UnityTaskVoid</c> method, as seen by <see cref="UnityTaskVoidMethodBuilder"/> without its
    /// state machine type.
    /// </summary>
    internal interface IUnityTaskVoidRunner
    {
        Action MoveNextAction { get; }

        void Return();
    }

    /// <summary>
    /// Pooled box that holds the state machine of an <c>async UnityTaskVoid</c> method and is the continuation target
    /// of its awaits.
    /// </summary>
    /// <typeparam name="TStateMachine">The compiler-generated state machine type.</typeparam>
    /// <remarks>
    /// <para>
    /// A void method has no task source, so this runner does not depend on the task backend. The builder rents it on
    /// the first suspension and copies the state machine into it once. <c>MoveNextAction</c> is created once per
    /// runner, so awaits do not allocate delegates.
    /// </para>
    /// <para>
    /// The builder calls <see cref="Return"/> when the method completes or throws. Under IL2CPP the return is scheduled
    /// at <see cref="UnityTaskTiming.LastPostLateUpdate"/> instead, as
    /// <see cref="UnityTaskRunner{TStateMachine}"/> does.
    /// </para>
    /// </remarks>
    internal sealed class UnityTaskVoidRunner<TStateMachine> : IUnityTaskVoidRunner
        where TStateMachine : IAsyncStateMachine
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<UnityTaskVoidRunner<TStateMachine>> s_pool = new();

        private readonly Action _moveNextAction;
#if ENABLE_IL2CPP
        private readonly Action _returnAction;
#endif
        private TStateMachine _stateMachine;

        private UnityTaskVoidRunner()
        {
            _moveNextAction = MoveNext;
#if ENABLE_IL2CPP
            _returnAction = ReturnCore;
#endif
        }

        public Action MoveNextAction => _moveNextAction;

        public static UnityTaskVoidRunner<TStateMachine> Rent()
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

        /// <summary>
        /// Gives the runner back to the pool, either at once or, under IL2CPP, at <c>LastPostLateUpdate</c>.
        /// </summary>
        public void Return()
        {
#if ENABLE_IL2CPP
            PlayerLoopScheduler.Schedule(UnityTaskTiming.LastPostLateUpdate, _returnAction);
#else
            ReturnCore();
#endif
        }

        private void MoveNext()
            => _stateMachine.MoveNext();

        /// <summary>
        /// Clears the state machine and puts the runner back in the pool, unless the pool already holds
        /// <c>MAX_POOL_SIZE</c> runners.
        /// </summary>
        private void ReturnCore()
        {
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
