#if !UNITASK || ENCOSY_UNITYTASK_AWAITABLE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Debugging;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;
using UnityEngine;
using UnityEngine.Tasks;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        private readonly Awaitable _task;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UnityTask(Awaitable task)
            => _task = task;

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task == null || _task.GetAwaiter().IsCompleted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask(Awaitable task)
        {
            DebuggingThrowHelper.ThrowIfNull(task);
            return new(task);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable AsAwaitable()
            => _task ?? Awaitables.GetCompleted();

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly Awaitable _task;

            internal Awaiter(Awaitable task)
                => _task = task;

            public bool IsCompleted => _task == null || _task.GetAwaiter().IsCompleted;

            public void GetResult()
            {
                if (_task != null)
                {
                    _task.GetAwaiter().GetResult();
                }
            }

            public void OnCompleted(Action continuation)
            {
                if (_task == null)
                {
                    continuation();
                    return;
                }

                _task.GetAwaiter().OnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
                => OnCompleted(continuation);
        }
    }

    public readonly partial struct UnityTask<T>
    {
        private readonly Awaitable<T> _task;

        internal UnityTask(Awaitable<T> task)
            => _task = task;

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task == null || _task.GetAwaiter().IsCompleted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask<T>(Awaitable<T> task)
        {
            DebuggingThrowHelper.ThrowIfNull(task);
            return new(task);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<T> AsAwaitable()
            => _task ?? Awaitables.GetCompleted<T>();

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly Awaitable<T> _task;

            internal Awaiter(Awaitable<T> task)
                => _task = task;

            public bool IsCompleted => _task == null || _task.GetAwaiter().IsCompleted;

            public T GetResult()
                => _task == null ? default : _task.GetAwaiter().GetResult();

            public void OnCompleted(Action continuation)
            {
                if (_task == null)
                {
                    continuation();
                    return;
                }

                _task.GetAwaiter().OnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
                => OnCompleted(continuation);
        }
    }

    public static class UnityTaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith<T>(this UnityTask<T> task, Action<T> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith<T>(this UnityTask<T> task, Func<T, Awaitable> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<TR> ContinueWith<T, TR>(this UnityTask<T> task, Func<T, TR> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<TR> ContinueWith<T, TR>(
              this UnityTask<T> task
            , Func<T, Awaitable<TR>> continuationFunction
        )
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith(this UnityTask task, Action continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith(this UnityTask task, Func<Awaitable> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> ContinueWith<T>(this UnityTask task, Func<T> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> ContinueWith<T>(this UnityTask task, Func<Awaitable<T>> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this UnityTask task)
            => Awaitables.Forget(task.AsAwaitable());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget<T>(this UnityTask<T> task)
            => Awaitables.Forget(task.AsAwaitable());
    }
}

#endif
