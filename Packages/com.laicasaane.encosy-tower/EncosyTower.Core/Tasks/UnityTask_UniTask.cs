#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        private readonly UniTask _task;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UnityTask(UniTask task)
            => _task = task;

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task.GetAwaiter().IsCompleted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task.GetAwaiter());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask(UniTask task)
            => new(task);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask AsUniTask()
            => _task;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UnityTask<T> CreateFromResult<T>(T value)
            => new(UniTask.FromResult(value));

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly UniTask.Awaiter _awaiter;

            internal Awaiter(UniTask.Awaiter awaiter)
                => _awaiter = awaiter;

            public bool IsCompleted => _awaiter.IsCompleted;

            public void GetResult()
                => _awaiter.GetResult();

            public void OnCompleted(Action continuation)
                => _awaiter.OnCompleted(continuation);

            public void UnsafeOnCompleted(Action continuation)
                => _awaiter.UnsafeOnCompleted(continuation);
        }
    }

    public readonly partial struct UnityTask<T>
    {
        private readonly UniTask<T> _task;

        internal UnityTask(UniTask<T> task)
            => _task = task;

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task.GetAwaiter().IsCompleted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task.GetAwaiter());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask<T>(UniTask<T> task)
            => new(task);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<T> AsUniTask()
            => _task;

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly UniTask<T>.Awaiter _awaiter;

            internal Awaiter(UniTask<T>.Awaiter awaiter)
                => _awaiter = awaiter;

            public bool IsCompleted => _awaiter.IsCompleted;

            public T GetResult()
                => _awaiter.GetResult();

            public void OnCompleted(Action continuation)
                => _awaiter.OnCompleted(continuation);

            public void UnsafeOnCompleted(Action continuation)
                => _awaiter.UnsafeOnCompleted(continuation);
        }
    }

    public static class UnityTaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith<T>(
              this UnityTask<T> task
            , Action<T> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith<T>(
              this UnityTask<T> task
            , Func<T, UniTask> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<TR> ContinueWith<T, TR>(
              this UnityTask<T> task
            , Func<T, TR> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<TR> ContinueWith<T, TR>(
              this UnityTask<T> task
            , Func<T, UniTask<TR>> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith(
              this UnityTask task
            , Action continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith(
              this UnityTask task
            , Func<UniTask> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> ContinueWith<T>(
              this UnityTask task
            , Func<T> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> ContinueWith<T>(
              this UnityTask task
            , Func<UniTask<T>> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this UnityTask task)
            => task.AsUniTask().Forget();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget<T>(this UnityTask<T> task)
            => task.AsUniTask().Forget();
    }
}

#endif
