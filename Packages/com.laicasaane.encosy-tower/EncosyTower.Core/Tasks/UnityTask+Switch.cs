using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Returns an awaitable that continues on the main thread at <paramref name="timing"/>.
        /// </summary>
        /// <param name="timing">The player-loop phase at which the continuation runs.</param>
        /// <param name="token">The token checked when the continuation resumes.</param>
        /// <returns>An awaitable that switches to the main thread.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> when awaited on the main thread, continues synchronously. Otherwise the continuation is
        /// scheduled on the shared Encosy player-loop scheduler at <paramref name="timing"/>. When
        /// <paramref name="token"/> is cancelled at resumption, the await throws
        /// <c>OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.SwitchToMainThread</c>; Unity:
        /// <see cref="UnityEngine.Awaitable.MainThreadAsync"/>, which completes immediately on the main thread.
        /// </para>
        /// </remarks>
        public static SwitchToMainThreadAwaitable SwitchToMainThreadAsync(
              UnityTaskTiming timing = UnityTaskTiming.Update
            , CancellationToken token = default
        )
            => new(timing, token);

        /// <summary>
        /// Returns an awaitable that continues on a thread-pool thread.
        /// </summary>
        /// <returns>An awaitable that switches to the thread pool.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> always queues the continuation to the <see cref="ThreadPool"/>, even when awaited on a
        /// thread-pool thread.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.SwitchToThreadPool</c>; Unity:
        /// <see cref="UnityEngine.Awaitable.BackgroundThreadAsync"/>, which completes immediately on a background
        /// thread, whereas this method always queues.
        /// </para>
        /// </remarks>
        public static SwitchToThreadPoolAwaitable SwitchToThreadPoolAsync()
            => default;

        /// <summary>
        /// Returns a scope whose asynchronous disposal switches back to the main thread.
        /// </summary>
        /// <param name="timing">The player-loop phase at which the continuation runs after disposal.</param>
        /// <param name="token">The token checked when the continuation resumes.</param>
        /// <returns>A scope for <c>await using</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> <c>await using (UnityTask.ReturnToMainThread()) { … }</c> runs the block on the current
        /// thread, then continues on the main thread as <see cref="SwitchToMainThreadAsync"/> does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.ReturnToMainThread</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static ReturnToMainThreadScope ReturnToMainThread(
              UnityTaskTiming timing = UnityTaskTiming.Update
            , CancellationToken token = default
        )
            => new(timing, token);

        /// <summary>
        /// Runs <paramref name="action"/> on the main thread at <paramref name="timing"/>.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="timing">The player-loop phase at which the action runs.</param>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> always schedules <paramref name="action"/> on the shared Encosy player-loop scheduler,
        /// even when called on the main thread. An exception thrown by <paramref name="action"/> is logged.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Post</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
        public static void Post(Action action, UnityTaskTiming timing = UnityTaskTiming.Update)
        {
            Debugging.ThrowHelper.ThrowIfNull(action);
            PlayerLoopScheduler.Schedule(timing, action);
        }

        /// <summary>
        /// Awaitable returned by <see cref="SwitchToMainThreadAsync"/>.
        /// </summary>
        public readonly struct SwitchToMainThreadAwaitable
        {
            private readonly UnityTaskTiming _timing;
            private readonly CancellationToken _token;

            internal SwitchToMainThreadAwaitable(UnityTaskTiming timing, CancellationToken token)
            {
                _timing = timing;
                _token = token;
            }

            /// <summary>
            /// Returns an awaiter for this awaitable.
            /// </summary>
            /// <returns>An awaiter for this awaitable.</returns>
            public Awaiter GetAwaiter()
                => new(_timing, _token);

            /// <summary>
            /// Awaits a switch to the main thread.
            /// </summary>
            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                private readonly UnityTaskTiming _timing;
                private readonly CancellationToken _token;

                internal Awaiter(UnityTaskTiming timing, CancellationToken token)
                {
                    _timing = timing;
                    _token = token;
                }

                /// <summary>
                /// Gets a value that indicates whether the current thread is the main thread.
                /// </summary>
                public bool IsCompleted
                    => UnityTaskThreadContext.CurrentAffinity != UnityTaskThreadAffinity.ThreadPool;

                /// <summary>
                /// Ends the wait; throws <see cref="OperationCanceledException"/> when the token is cancelled.
                /// </summary>
                public void GetResult()
                    => _token.ThrowIfCancellationRequested();

                /// <summary>
                /// Schedules <paramref name="continuation"/> on the main thread.
                /// </summary>
                /// <param name="continuation">The action to run on the main thread.</param>
                public void OnCompleted(Action continuation)
                    => PlayerLoopScheduler.Schedule(_timing, continuation);

                /// <summary>
                /// Schedules <paramref name="continuation"/> on the main thread.
                /// </summary>
                /// <param name="continuation">The action to run on the main thread.</param>
                public void UnsafeOnCompleted(Action continuation)
                    => PlayerLoopScheduler.Schedule(_timing, continuation);
            }
        }

        /// <summary>
        /// Awaitable returned by <see cref="SwitchToThreadPoolAsync"/>.
        /// </summary>
        public readonly struct SwitchToThreadPoolAwaitable
        {
            /// <summary>
            /// Returns an awaiter for this awaitable.
            /// </summary>
            /// <returns>An awaiter for this awaitable.</returns>
            public Awaiter GetAwaiter()
                => default;

            /// <summary>
            /// Awaits a switch to the thread pool.
            /// </summary>
            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                /// <summary>
                /// Always <c>false</c>, so every await switches.
                /// </summary>
                public bool IsCompleted => false;

                /// <summary>
                /// Ends the wait.
                /// </summary>
                public void GetResult()
                {
                }

                /// <summary>
                /// Queues <paramref name="continuation"/> to the thread pool.
                /// </summary>
                /// <param name="continuation">The action to run on the thread pool.</param>
                public void OnCompleted(Action continuation)
                    => ThreadPool.UnsafeQueueUserWorkItem(static state => ((Action)state)(), continuation);

                /// <summary>
                /// Queues <paramref name="continuation"/> to the thread pool.
                /// </summary>
                /// <param name="continuation">The action to run on the thread pool.</param>
                public void UnsafeOnCompleted(Action continuation)
                    => ThreadPool.UnsafeQueueUserWorkItem(static state => ((Action)state)(), continuation);
            }
        }

        /// <summary>
        /// Scope returned by <see cref="ReturnToMainThread"/>.
        /// </summary>
        public readonly struct ReturnToMainThreadScope
        {
            private readonly UnityTaskTiming _timing;
            private readonly CancellationToken _token;

            internal ReturnToMainThreadScope(UnityTaskTiming timing, CancellationToken token)
            {
                _timing = timing;
                _token = token;
            }

            /// <summary>
            /// Returns an awaitable that switches to the main thread.
            /// </summary>
            /// <returns>An awaitable that switches to the main thread.</returns>
            public SwitchToMainThreadAwaitable DisposeAsync()
                => new(_timing, _token);
        }
    }
}
