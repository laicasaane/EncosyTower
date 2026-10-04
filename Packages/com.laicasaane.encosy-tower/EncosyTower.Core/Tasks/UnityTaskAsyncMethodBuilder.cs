using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
using Cysharp.Threading.Tasks;
#else
using UnityEngine;
#endif

namespace EncosyTower.Tasks
{
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
    /// <summary>
    /// Builds the <see cref="UnityTask"/> returned by an <c>async</c> method. Used by the compiler; not intended
    /// for direct use.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct UnityTaskAsyncMethodBuilder
    {
        private IUnityTaskRunner _runner;
        private UnityTaskThreadAffinity _affinity;

        /// <summary>
        /// Gets the task that represents the asynchronous method.
        /// </summary>
        public UnityTask Task
        {
            get
            {
                EnsureRunner();
                return new(new UniTask(_runner, _runner.Version));
            }
        }

        /// <summary>
        /// Creates a builder for an asynchronous method.
        /// </summary>
        public static UnityTaskAsyncMethodBuilder Create()
            => default;

        /// <summary>
        /// Completes the task successfully.
        /// </summary>
        public void SetResult()
        {
            EnsureRunner();
            _runner.SetResult();
        }

        /// <summary>
        /// Completes the task with <paramref name="exception"/>, which awaiting the task rethrows unchanged.
        /// </summary>
        /// <param name="exception">The exception that faulted the asynchronous method.</param>
        public void SetException(Exception exception)
        {
            EnsureRunner();
            _runner.SetException(exception);
        }

        /// <summary>
        /// Associates the builder with a boxed state machine.
        /// </summary>
        /// <param name="stateMachine">The boxed state machine.</param>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        /// <summary>
        /// Runs the state machine until its first suspension.
        /// </summary>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine to run.</param>
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            _affinity = UnityTaskThreadContext.CurrentAffinity;
            stateMachine.MoveNext();
        }

        /// <summary>
        /// Schedules the state machine to continue when <paramref name="awaiter"/> completes.
        /// </summary>
        /// <typeparam name=\"TAwaiter\">The type of the awaiter.</typeparam>
        /// <typeparam name=\"TStateMachine\">The type of the state machine.</typeparam>
        /// <param name=\"awaiter\">The awaiter to wait on.</param>
        /// <param name=\"stateMachine\">The state machine to continue.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsureRunner(ref stateMachine);
            awaiter.OnCompleted(_runner.MoveNextAction);
        }

        /// <summary>
        /// Schedules the state machine to continue when <paramref name="awaiter"/> completes, without
        /// flowing the execution context.
        /// </summary>
        /// <typeparam name=\"TAwaiter\">The type of the awaiter.</typeparam>
        /// <typeparam name=\"TStateMachine\">The type of the state machine.</typeparam>
        /// <param name=\"awaiter\">The awaiter to wait on.</param>
        /// <param name=\"stateMachine\">The state machine to continue.</param>
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

        private void EnsureRunner()
        {
            if (_runner != null)
            {
                return;
            }

            var runner = UnityTaskRunner<UnityTaskNoStateMachine>.Rent();
            runner.Prepare(_affinity);
            _runner = runner;
        }

        private void EnsureRunner<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            if (_runner != null)
            {
                return;
            }

            var runner = UnityTaskRunner<TStateMachine>.Rent();
            runner.Prepare(_affinity);
            _runner = runner;
            runner.SetStateMachine(ref stateMachine);
        }
    }

    /// <summary>
    /// Builds the <see cref="UnityTask{T}"/> returned by an <c>async</c> method. Used by the compiler; not
    /// intended for direct use.
    /// </summary>
    /// <typeparam name="T">The result type of the task.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct UnityTaskAsyncMethodBuilder<T>
    {
        private IUnityTaskRunner<T> _runner;
        private UnityTaskThreadAffinity _affinity;

        /// <summary>
        /// Gets the task that represents the asynchronous method.
        /// </summary>
        public UnityTask<T> Task
        {
            get
            {
                EnsureRunner();
                return new(new UniTask<T>(_runner, _runner.Version));
            }
        }

        /// <summary>
        /// Creates a builder for an asynchronous method.
        /// </summary>
        public static UnityTaskAsyncMethodBuilder<T> Create()
            => default;

        /// <summary>
        /// Completes the task successfully.
        /// </summary>
        /// <param name="result">The result of the task.</param>
        public void SetResult(T result)
        {
            EnsureRunner();
            _runner.SetResult(result);
        }

        /// <summary>
        /// Completes the task with <paramref name="exception"/>, which awaiting the task rethrows unchanged.
        /// </summary>
        /// <param name="exception">The exception that faulted the asynchronous method.</param>
        public void SetException(Exception exception)
        {
            EnsureRunner();
            _runner.SetException(exception);
        }

        /// <summary>
        /// Associates the builder with a boxed state machine.
        /// </summary>
        /// <param name="stateMachine">The boxed state machine.</param>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        /// <summary>
        /// Runs the state machine until its first suspension.
        /// </summary>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine to run.</param>
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            _affinity = UnityTaskThreadContext.CurrentAffinity;
            stateMachine.MoveNext();
        }

        /// <summary>
        /// Schedules the state machine to continue when <paramref name="awaiter"/> completes.
        /// </summary>
        /// <typeparam name=\"TAwaiter\">The type of the awaiter.</typeparam>
        /// <typeparam name=\"TStateMachine\">The type of the state machine.</typeparam>
        /// <param name=\"awaiter\">The awaiter to wait on.</param>
        /// <param name=\"stateMachine\">The state machine to continue.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsureRunner(ref stateMachine);
            awaiter.OnCompleted(_runner.MoveNextAction);
        }

        /// <summary>
        /// Schedules the state machine to continue when <paramref name="awaiter"/> completes, without
        /// flowing the execution context.
        /// </summary>
        /// <typeparam name=\"TAwaiter\">The type of the awaiter.</typeparam>
        /// <typeparam name=\"TStateMachine\">The type of the state machine.</typeparam>
        /// <param name=\"awaiter\">The awaiter to wait on.</param>
        /// <param name=\"stateMachine\">The state machine to continue.</param>
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

        private void EnsureRunner()
        {
            if (_runner != null)
            {
                return;
            }

            var runner = UnityTaskRunner<UnityTaskNoStateMachine, T>.Rent();
            runner.Prepare(_affinity);
            _runner = runner;
        }

        private void EnsureRunner<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            if (_runner != null)
            {
                return;
            }

            var runner = UnityTaskRunner<TStateMachine, T>.Rent();
            runner.Prepare(_affinity);
            _runner = runner;
            runner.SetStateMachine(ref stateMachine);
        }
    }
#else
    /// <summary>
    /// Builds the <see cref="UnityTask"/> returned by an <c>async</c> method. Used by the compiler; not intended
    /// for direct use.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct UnityTaskAsyncMethodBuilder
    {
        private Awaitable.AwaitableAsyncMethodBuilder _builder;

        private UnityTaskAsyncMethodBuilder(Awaitable.AwaitableAsyncMethodBuilder builder)
            => _builder = builder;

        /// <summary>
        /// Creates a builder for an asynchronous method.
        /// </summary>
        public static UnityTaskAsyncMethodBuilder Create()
            => new(Awaitable.AwaitableAsyncMethodBuilder.Create());

        /// <summary>
        /// Gets the task that represents the asynchronous method.
        /// </summary>
        public UnityTask Task
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_builder.Task);
        }

        /// <summary>
        /// Completes the task successfully.
        /// </summary>
        public void SetResult()
            => _builder.SetResult();

        /// <summary>
        /// Completes the task with <paramref name="exception"/>, which awaiting the task rethrows unchanged.
        /// </summary>
        /// <param name="exception">The exception that faulted the asynchronous method.</param>
        public void SetException(Exception exception)
            => _builder.SetException(exception);

        /// <summary>
        /// Associates the builder with a boxed state machine.
        /// </summary>
        /// <param name="stateMachine">The boxed state machine.</param>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
            => _builder.SetStateMachine(stateMachine);

        /// <summary>
        /// Runs the state machine until its first suspension.
        /// </summary>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine to run.</param>
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => _builder.Start(ref stateMachine);

        /// <summary>
        /// Schedules the state machine to continue when <paramref name="awaiter"/> completes.
        /// </summary>
        /// <typeparam name=\"TAwaiter\">The type of the awaiter.</typeparam>
        /// <typeparam name=\"TStateMachine\">The type of the state machine.</typeparam>
        /// <param name=\"awaiter\">The awaiter to wait on.</param>
        /// <param name=\"stateMachine\">The state machine to continue.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
              ref TAwaiter awaiter
            , ref TStateMachine stateMachine
        )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);

        /// <summary>
        /// Schedules the state machine to continue when <paramref name="awaiter"/> completes, without
        /// flowing the execution context.
        /// </summary>
        /// <typeparam name=\"TAwaiter\">The type of the awaiter.</typeparam>
        /// <typeparam name=\"TStateMachine\">The type of the state machine.</typeparam>
        /// <param name=\"awaiter\">The awaiter to wait on.</param>
        /// <param name=\"stateMachine\">The state machine to continue.</param>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
              ref TAwaiter awaiter
            , ref TStateMachine stateMachine
        )
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }

    /// <summary>
    /// Builds the <see cref="UnityTask{T}"/> returned by an <c>async</c> method. Used by the compiler; not
    /// intended for direct use.
    /// </summary>
    /// <typeparam name="T">The result type of the task.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct UnityTaskAsyncMethodBuilder<T>
    {
        private Awaitable.AwaitableAsyncMethodBuilder<T> _builder;

        private UnityTaskAsyncMethodBuilder(Awaitable.AwaitableAsyncMethodBuilder<T> builder)
            => _builder = builder;

        /// <summary>
        /// Creates a builder for an asynchronous method.
        /// </summary>
        public static UnityTaskAsyncMethodBuilder<T> Create()
            => new(Awaitable.AwaitableAsyncMethodBuilder<T>.Create());

        /// <summary>
        /// Gets the task that represents the asynchronous method.
        /// </summary>
        public UnityTask<T> Task
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_builder.Task);
        }

        /// <summary>
        /// Completes the task successfully.
        /// </summary>
        /// <param name="result">The result of the task.</param>
        public void SetResult(T result)
            => _builder.SetResult(result);

        /// <summary>
        /// Completes the task with <paramref name="exception"/>, which awaiting the task rethrows unchanged.
        /// </summary>
        /// <param name="exception">The exception that faulted the asynchronous method.</param>
        public void SetException(Exception exception)
            => _builder.SetException(exception);

        /// <summary>
        /// Associates the builder with a boxed state machine.
        /// </summary>
        /// <param name="stateMachine">The boxed state machine.</param>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
            => _builder.SetStateMachine(stateMachine);

        /// <summary>
        /// Runs the state machine until its first suspension.
        /// </summary>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine to run.</param>
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => _builder.Start(ref stateMachine);

        /// <summary>
        /// Schedules the state machine to continue when <paramref name="awaiter"/> completes.
        /// </summary>
        /// <typeparam name=\"TAwaiter\">The type of the awaiter.</typeparam>
        /// <typeparam name=\"TStateMachine\">The type of the state machine.</typeparam>
        /// <param name=\"awaiter\">The awaiter to wait on.</param>
        /// <param name=\"stateMachine\">The state machine to continue.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
              ref TAwaiter awaiter
            , ref TStateMachine stateMachine
        )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);

        /// <summary>
        /// Schedules the state machine to continue when <paramref name="awaiter"/> completes, without
        /// flowing the execution context.
        /// </summary>
        /// <typeparam name=\"TAwaiter\">The type of the awaiter.</typeparam>
        /// <typeparam name=\"TStateMachine\">The type of the state machine.</typeparam>
        /// <param name=\"awaiter\">The awaiter to wait on.</param>
        /// <param name=\"stateMachine\">The state machine to continue.</param>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
              ref TAwaiter awaiter
            , ref TStateMachine stateMachine
        )
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }
#endif
}
