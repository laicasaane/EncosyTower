using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace UnityEngine.Tasks
{
    public static partial class Awaitables
    {
        private static readonly Action<Task, object> s_taskContinuation = CompleteTask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable GetCompleted()
            => CompletedTask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> GetCompleted<T>()
            => FromResult<T>(default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> GetCompleted<T>(T value)
            => FromResult(value);

        public static async Awaitable WaitUntil([NotNull] Func<bool> condition, CancellationToken token = default)
        {
            DebuggingThrowHelper.ThrowIfNull(condition);
            token.ThrowIfCancellationRequested();

            while (condition() == false)
            {
                await Yield(AwaitablePlayerLoopTiming.Update, token);
            }
        }

        public static async Awaitable WaitUntil<TState>(
              [NotNull] TState state
            , [NotNull] Func<TState, bool> condition
            , CancellationToken token = default
        )
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            DebuggingThrowHelper.ThrowIfNull(condition);
            token.ThrowIfCancellationRequested();

            while (condition(state) == false)
            {
                await Yield(AwaitablePlayerLoopTiming.Update, token);
            }
        }

        public static async Awaitable WaitWhile([NotNull] Func<bool> condition, CancellationToken token = default)
        {
            DebuggingThrowHelper.ThrowIfNull(condition);
            token.ThrowIfCancellationRequested();

            while (condition())
            {
                await Yield(AwaitablePlayerLoopTiming.Update, token);
            }
        }

        public static async Awaitable WaitWhile<TState>(
              [NotNull] TState state
            , [NotNull] Func<TState, bool> condition
            , CancellationToken token = default
        )
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            DebuggingThrowHelper.ThrowIfNull(condition);
            token.ThrowIfCancellationRequested();

            while (condition(state))
            {
                await Yield(AwaitablePlayerLoopTiming.Update, token);
            }
        }

        public static Awaitable WhenAll([NotNull] params Awaitable[] awaitables)
        {
            DebuggingThrowHelper.ThrowIfNull(awaitables);
            return StartWhenAll(awaitables, awaitables.Length);
        }

        public static Awaitable WhenAll([NotNull] IEnumerable<Awaitable> awaitables)
        {
            DebuggingThrowHelper.ThrowIfNull(awaitables);
            return StartWhenAll(awaitables);
        }

        public static Awaitable WhenAll([NotNull] Awaitable[] awaitables, int count)
        {
            DebuggingThrowHelper.ThrowIfNull(awaitables);
            ThrowHelper.ThrowIfCountOutOfRange(count, awaitables.Length);
            return StartWhenAll(awaitables, count);
        }

        public static Awaitable<T[]> WhenAll<T>([NotNull] params Awaitable<T>[] awaitables)
        {
            DebuggingThrowHelper.ThrowIfNull(awaitables);
            return StartWhenAll(awaitables, awaitables.Length);
        }

        public static Awaitable<T[]> WhenAll<T>([NotNull] IEnumerable<Awaitable<T>> awaitables)
        {
            DebuggingThrowHelper.ThrowIfNull(awaitables);
            return StartWhenAll(awaitables);
        }

        public static Awaitable<T[]> WhenAll<T>([NotNull] Awaitable<T>[] awaitables, int count)
        {
            DebuggingThrowHelper.ThrowIfNull(awaitables);
            ThrowHelper.ThrowIfCountOutOfRange(count, awaitables.Length);
            return StartWhenAll(awaitables, count);
        }

        public static Awaitable WhenAll([NotNull] Awaitable awaitable1, [NotNull] Awaitable awaitable2)
            => StartWhenAll(awaitable1, awaitable2);

        public static Awaitable WhenAll(
              [NotNull] Awaitable awaitable1
            , [NotNull] Awaitable awaitable2
            , [NotNull] Awaitable awaitable3
        )
            => StartWhenAll(awaitable1, awaitable2, awaitable3);

        public static Awaitable WhenAll(
              [NotNull] Awaitable awaitable1
            , [NotNull] Awaitable awaitable2
            , [NotNull] Awaitable awaitable3
            , [NotNull] Awaitable awaitable4
        )
            => StartWhenAll(awaitable1, awaitable2, awaitable3, awaitable4);

        public static Awaitable<T> WhenAll<T>([NotNull] Awaitable awaitable1, [NotNull] Awaitable<T> awaitable2)
            => StartWhenAll(awaitable2, awaitable1);

        public static Awaitable<T> WhenAll<T>([NotNull] Awaitable<T> awaitable1, [NotNull] Awaitable awaitable2)
            => StartWhenAll(awaitable1, awaitable2);

        public static void Forget([NotNull] Awaitable self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            AwaitableForgetObserver.Observe(self);
        }

        public static void Forget<T>([NotNull] Awaitable<T> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            AwaitableForgetObserver<T>.Observe(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run([NotNull] Awaitable self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            AwaitableRunObserver.Observe(self);
        }

        public static void Run([NotNull] IEnumerable<Awaitable> list)
        {
            DebuggingThrowHelper.ThrowIfNull(list);

            foreach (var item in list)
            {
                DebuggingThrowHelper.ThrowIfNull(item);
                AwaitableRunObserver.Observe(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WithContinuation([NotNull] Awaitable self, [NotNull] Action continuation)
            => self.ContinueWith(continuation);

        public static Awaitable AsAwaitable([NotNull] this Task task, bool useCurrentSynchronizationContext = true)
        {
            DebuggingThrowHelper.ThrowIfNull(task);
            var completionSource = new AwaitableCompletionSource();
            task.ContinueWith(
                  s_taskContinuation
                , completionSource
                , CancellationToken.None
                , TaskContinuationOptions.ExecuteSynchronously
                , GetTaskScheduler(useCurrentSynchronizationContext)
            );
            return completionSource.Awaitable;
        }

        public static Awaitable<T> AsAwaitable<T>(
              [NotNull] this Task<T> task
            , bool useCurrentSynchronizationContext = true
        )
        {
            DebuggingThrowHelper.ThrowIfNull(task);
            var completionSource = new AwaitableCompletionSource<T>();
            task.ContinueWith(
                  TaskContinuation<T>.s_callback
                , completionSource
                , CancellationToken.None
                , TaskContinuationOptions.ExecuteSynchronously
                , GetTaskScheduler(useCurrentSynchronizationContext)
            );
            return completionSource.Awaitable;
        }

        private static TaskScheduler GetTaskScheduler(bool useCurrentSynchronizationContext)
            => useCurrentSynchronizationContext
                ? TaskScheduler.FromCurrentSynchronizationContext()
                : TaskScheduler.Default;

        private static void CompleteTask(Task task, object state)
        {
            var completionSource = (AwaitableCompletionSource)state;

            try
            {
                task.GetAwaiter().GetResult();
                completionSource.TrySetResult();
            }
            catch (Exception exception)
            {
                completionSource.TrySetException(exception);
            }
        }

        private static class TaskContinuation<T>
        {
            internal static readonly Action<Task<T>, object> s_callback = Complete;

            private static void Complete(Task<T> task, object state)
            {
                var completionSource = (AwaitableCompletionSource<T>)state;

                try
                {
                    completionSource.TrySetResult(task.GetAwaiter().GetResult());
                }
                catch (Exception exception)
                {
                    completionSource.TrySetException(exception);
                }
            }
        }
    }
}
