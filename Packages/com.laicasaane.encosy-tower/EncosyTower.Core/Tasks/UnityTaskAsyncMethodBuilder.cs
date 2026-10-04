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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct UnityTaskAsyncMethodBuilder
    {
        private IUnityTaskRunner _runner;
        private UnityTaskThreadAffinity _affinity;

        public static UnityTaskAsyncMethodBuilder Create()
            => default;

        public UnityTask Task
        {
            get
            {
                EnsureRunner();
                return new(new UniTask(_runner, _runner.Version));
            }
        }

        public void SetResult()
        {
            EnsureRunner();
            _runner.SetResult();
        }

        public void SetException(Exception exception)
        {
            EnsureRunner();
            _runner.SetException(exception);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            _affinity = UnityTaskThreadContext.CurrentAffinity;
            stateMachine.MoveNext();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
              ref TAwaiter awaiter
            , ref TStateMachine stateMachine
        )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsureRunner(ref stateMachine);
            awaiter.OnCompleted(_runner.MoveNextAction);
        }

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

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct UnityTaskAsyncMethodBuilder<T>
    {
        private IUnityTaskRunner<T> _runner;
        private UnityTaskThreadAffinity _affinity;

        public static UnityTaskAsyncMethodBuilder<T> Create()
            => default;

        public UnityTask<T> Task
        {
            get
            {
                EnsureRunner();
                return new(new UniTask<T>(_runner, _runner.Version));
            }
        }

        public void SetResult(T result)
        {
            EnsureRunner();
            _runner.SetResult(result);
        }

        public void SetException(Exception exception)
        {
            EnsureRunner();
            _runner.SetException(exception);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            _affinity = UnityTaskThreadContext.CurrentAffinity;
            stateMachine.MoveNext();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
              ref TAwaiter awaiter
            , ref TStateMachine stateMachine
        )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsureRunner(ref stateMachine);
            awaiter.OnCompleted(_runner.MoveNextAction);
        }

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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct UnityTaskAsyncMethodBuilder
    {
        private Awaitable.AwaitableAsyncMethodBuilder _builder;

        private UnityTaskAsyncMethodBuilder(Awaitable.AwaitableAsyncMethodBuilder builder)
            => _builder = builder;

        public static UnityTaskAsyncMethodBuilder Create()
            => new(Awaitable.AwaitableAsyncMethodBuilder.Create());

        public UnityTask Task
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_builder.Task);
        }

        public void SetResult()
            => _builder.SetResult();

        public void SetException(Exception exception)
            => _builder.SetException(exception);

        public void SetStateMachine(IAsyncStateMachine stateMachine)
            => _builder.SetStateMachine(stateMachine);

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => _builder.Start(ref stateMachine);

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
              ref TAwaiter awaiter
            , ref TStateMachine stateMachine
        )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
              ref TAwaiter awaiter
            , ref TStateMachine stateMachine
        )
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct UnityTaskAsyncMethodBuilder<T>
    {
        private Awaitable.AwaitableAsyncMethodBuilder<T> _builder;

        private UnityTaskAsyncMethodBuilder(Awaitable.AwaitableAsyncMethodBuilder<T> builder)
            => _builder = builder;

        public static UnityTaskAsyncMethodBuilder<T> Create()
            => new(Awaitable.AwaitableAsyncMethodBuilder<T>.Create());

        public UnityTask<T> Task
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_builder.Task);
        }

        public void SetResult(T result)
            => _builder.SetResult(result);

        public void SetException(Exception exception)
            => _builder.SetException(exception);

        public void SetStateMachine(IAsyncStateMachine stateMachine)
            => _builder.SetStateMachine(stateMachine);

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => _builder.Start(ref stateMachine);

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
              ref TAwaiter awaiter
            , ref TStateMachine stateMachine
        )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);

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
