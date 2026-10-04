using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
using Cysharp.Threading.Tasks.CompilerServices;
#else
using UnityEngine;
#endif

namespace EncosyTower.Tasks
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct UnityTaskAsyncMethodBuilder
    {
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
        private AsyncUniTaskMethodBuilder _builder;

        private UnityTaskAsyncMethodBuilder(AsyncUniTaskMethodBuilder builder)
#else
        private Awaitable.AwaitableAsyncMethodBuilder _builder;

        private UnityTaskAsyncMethodBuilder(Awaitable.AwaitableAsyncMethodBuilder builder)
#endif
            => _builder = builder;

        public static UnityTaskAsyncMethodBuilder Create()
        {
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
            return new(AsyncUniTaskMethodBuilder.Create());
#else
            return new(Awaitable.AwaitableAsyncMethodBuilder.Create());
#endif
        }

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
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
        private AsyncUniTaskMethodBuilder<T> _builder;

        private UnityTaskAsyncMethodBuilder(AsyncUniTaskMethodBuilder<T> builder)
#else
        private Awaitable.AwaitableAsyncMethodBuilder<T> _builder;

        private UnityTaskAsyncMethodBuilder(Awaitable.AwaitableAsyncMethodBuilder<T> builder)
#endif
            => _builder = builder;

        public static UnityTaskAsyncMethodBuilder<T> Create()
        {
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
            return new(AsyncUniTaskMethodBuilder<T>.Create());
#else
            return new(Awaitable.AwaitableAsyncMethodBuilder<T>.Create());
#endif
        }

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
}
