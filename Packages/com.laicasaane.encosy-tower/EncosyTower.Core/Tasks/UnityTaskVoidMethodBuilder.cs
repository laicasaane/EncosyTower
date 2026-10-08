using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// The async method builder of <see cref="UnityTaskVoid"/>. Used by the compiler; do not call it directly.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct UnityTaskVoidMethodBuilder
    {
        private IUnityTaskVoidRunner _runner;

        /// <summary>Gets <c>default(UnityTaskVoid)</c>.</summary>
        public UnityTaskVoid Task => default;

        /// <summary>Creates a builder.</summary>
        /// <returns>A new builder.</returns>
        public static UnityTaskVoidMethodBuilder Create()
            => default;

        /// <summary>Completes the method and returns its pooled state.</summary>
        public void SetResult()
            => ReturnRunner();

        /// <summary>Completes the method, returns its pooled state, and logs <paramref name="exception"/>
        /// unless it is an <see cref="OperationCanceledException"/>.</summary>
        /// <param name="exception">The exception thrown by the method.</param>
        public void SetException(Exception exception)
        {
            ReturnRunner();

            if (exception is OperationCanceledException == false)
            {
                ThrowHelper.LogUnobservedException(exception);
            }
        }

        /// <summary>Not used.</summary>
        /// <param name="stateMachine">Ignored.</param>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        /// <summary>Runs the method until its first suspension.</summary>
        /// <typeparam name="TStateMachine">The compiler-generated state machine type.</typeparam>
        /// <param name="stateMachine">The state machine.</param>
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => stateMachine.MoveNext();

        /// <summary>Schedules the state machine to continue when <paramref name="awaiter"/> completes.</summary>
        /// <typeparam name="TAwaiter">The awaiter type.</typeparam>
        /// <typeparam name="TStateMachine">The compiler-generated state machine type.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsureRunner(ref stateMachine);
            awaiter.OnCompleted(_runner.MoveNextAction);
        }

        /// <summary>Schedules the state machine to continue when <paramref name="awaiter"/> completes.</summary>
        /// <typeparam name="TAwaiter">The awaiter type.</typeparam>
        /// <typeparam name="TStateMachine">The compiler-generated state machine type.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
              ref TAwaiter awaiter
            , ref TStateMachine stateMachine
        )
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsureRunner(ref stateMachine);
            awaiter.UnsafeOnCompleted(_runner.MoveNextAction);
        }

        /// <summary>
        /// Creates the runner for the first suspension, if none exists, and copies the state machine into it.
        /// </summary>
        /// <typeparam name="TStateMachine">The compiler-generated state machine type.</typeparam>
        /// <param name="stateMachine">The state machine of the asynchronous method.</param>
        /// <remarks>
        /// Later suspensions find the runner and return without copying, so the state machine is boxed once. A void
        /// method that never suspends never rents a runner.
        /// </remarks>
        private void EnsureRunner<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            if (_runner != null)
            {
                return;
            }

            var runner = UnityTaskVoidRunner<TStateMachine>.Rent();
            _runner = runner;
            runner.SetStateMachine(ref stateMachine);
        }

        /// <summary>
        /// Gives the runner back to the pool and forgets it. Does nothing when the method never suspended.
        /// </summary>
        private void ReturnRunner()
        {
            if (_runner == null)
            {
                return;
            }

            _runner.Return();
            _runner = null;
        }
    }
}
