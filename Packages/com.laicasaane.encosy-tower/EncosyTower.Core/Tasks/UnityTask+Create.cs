using System;
using System.Threading;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Calls <paramref name="factory"/> and returns its task.
        /// </summary>
        /// <param name="factory">The method that creates the task.</param>
        /// <returns>The task returned by <paramref name="factory"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> calls <paramref name="factory"/> synchronously. Use it to run an async lambda in place.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Create</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public static UnityTask CreateAsync(Func<UnityTask> factory)
        {
            Debugging.ThrowHelper.ThrowIfNull(factory);
            return factory();
        }

        /// <summary>
        /// Calls <paramref name="factory"/> with <paramref name="token"/> and returns its task.
        /// </summary>
        /// <param name="factory">The method that creates the task.</param>
        /// <param name="token">The token passed to <paramref name="factory"/>.</param>
        /// <returns>The task returned by <paramref name="factory"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Create</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public static UnityTask CreateAsync(Func<CancellationToken, UnityTask> factory, CancellationToken token)
        {
            Debugging.ThrowHelper.ThrowIfNull(factory);
            return factory(token);
        }

        /// <summary>
        /// Calls <paramref name="factory"/> with <paramref name="state"/> and returns its task.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to <paramref name="factory"/>.</typeparam>
        /// <param name="state">The state passed to <paramref name="factory"/>.</param>
        /// <param name="factory">The method that creates the task.</param>
        /// <returns>The task returned by <paramref name="factory"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Create</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public static UnityTask CreateAsync<TState>(TState state, Func<TState, UnityTask> factory)
        {
            Debugging.ThrowHelper.ThrowIfNull(factory);
            return factory(state);
        }

        /// <summary>
        /// Calls <paramref name="factory"/> and returns its task.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="factory">The method that creates the task.</param>
        /// <returns>The task returned by <paramref name="factory"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Create</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public static UnityTask<T> CreateAsync<T>(Func<UnityTask<T>> factory)
        {
            Debugging.ThrowHelper.ThrowIfNull(factory);
            return factory();
        }

        /// <summary>
        /// Returns a task that calls <paramref name="factory"/> when it is first awaited or queried.
        /// </summary>
        /// <param name="factory">The method that creates the task.</param>
        /// <returns>A task that runs <paramref name="factory"/> lazily.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the first read of <c>IsCompleted</c>, <c>Status</c> or the awaiter calls
        /// <paramref name="factory"/>; later reads use the task it returned. An exception thrown by
        /// <paramref name="factory"/> is thrown from that first read.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Defer</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public static UnityTask DeferAsync(Func<UnityTask> factory)
        {
            Debugging.ThrowHelper.ThrowIfNull(factory);
            return FromSource(new UnityTaskDeferSource(factory), 0);
        }

        /// <summary>
        /// Returns a task that calls <paramref name="factory"/> when it is first awaited or queried.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="factory">The method that creates the task.</param>
        /// <returns>A task that runs <paramref name="factory"/> lazily.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as <see cref="DeferAsync(Func{UnityTask})"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Defer</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public static UnityTask<T> DeferAsync<T>(Func<UnityTask<T>> factory)
        {
            Debugging.ThrowHelper.ThrowIfNull(factory);
            return FromSource<T>(new UnityTaskDeferSource<T>(factory), 0);
        }

        /// <summary>
        /// Returns a task that calls <paramref name="factory"/> with <paramref name="state"/> when it is first
        /// awaited or queried.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to <paramref name="factory"/>.</typeparam>
        /// <param name="state">The state passed to <paramref name="factory"/>.</param>
        /// <param name="factory">The method that creates the task.</param>
        /// <returns>A task that runs <paramref name="factory"/> lazily.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as <see cref="DeferAsync(Func{UnityTask})"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Defer</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public static UnityTask DeferAsync<TState>(TState state, Func<TState, UnityTask> factory)
        {
            Debugging.ThrowHelper.ThrowIfNull(factory);
            return DeferAsync(Invoke);

            UnityTask Invoke()
                => factory(state);
        }

        /// <summary>
        /// Returns a task that calls <paramref name="factory"/> with <paramref name="state"/> when it is first
        /// awaited or queried.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to <paramref name="factory"/>.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="state">The state passed to <paramref name="factory"/>.</param>
        /// <param name="factory">The method that creates the task.</param>
        /// <returns>A task that runs <paramref name="factory"/> lazily.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as <see cref="DeferAsync(Func{UnityTask})"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Defer</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public static UnityTask<TResult> DeferAsync<TState, TResult>(
              TState state
            , Func<TState, UnityTask<TResult>> factory
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(factory);
            return DeferAsync<TResult>(Invoke);

            UnityTask<TResult> Invoke()
                => factory(state);
        }

        /// <summary>
        /// Returns a task that completes only when <paramref name="token"/> is cancelled.
        /// </summary>
        /// <param name="token">The token that cancels the task.</param>
        /// <returns>A task that completes with <c>new OperationCanceledException(token)</c> when
        /// <paramref name="token"/> is cancelled; it never completes when the token cannot be cancelled.</returns>
        /// <remarks>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Never</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask NeverAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return FromCanceled(token);
            }

            var source = new UnityTaskCompletionSource();

            if (token.CanBeCanceled)
            {
                return NeverCoreAsync(source, token);
            }

            return source.Task;
        }

        /// <summary>
        /// Returns a task that completes only when <paramref name="token"/> is cancelled.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="token">The token that cancels the task.</param>
        /// <returns>A task that completes with <c>new OperationCanceledException(token)</c> when
        /// <paramref name="token"/> is cancelled; it never completes when the token cannot be cancelled.</returns>
        /// <remarks>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Never</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask<T> NeverAsync<T>(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return FromCanceled<T>(token);
            }

            var source = new UnityTaskCompletionSource<T>();

            if (token.CanBeCanceled)
            {
                return NeverCoreAsync(source, token);
            }

            return source.Task;
        }

        /// <summary>
        /// Waits for <paramref name="token"/> to be cancelled, then throws <see cref="OperationCanceledException"/>.
        /// </summary>
        /// <param name="source">The source whose task is awaited; the token registration completes it.</param>
        /// <param name="token">The token that ends the wait.</param>
        /// <returns>A task that is cancelled with <paramref name="token"/>.</returns>
        /// <remarks>
        /// Used by <c>NeverAsync</c> when the token can be cancelled. Without a cancellable token the source's own task
        /// is returned and never completes.
        /// </remarks>
        private static async UnityTask NeverCoreAsync(UnityTaskCompletionSource source, CancellationToken token)
        {
            using (token.Register(static state => ((UnityTaskCompletionSource)state).TrySetResult(), source))
            {
                await source.Task;
            }

            ThrowHelper.ThrowOperationCanceled(token);
        }

        /// <summary>
        /// Waits for <paramref name="token"/> to be cancelled, then throws <see cref="OperationCanceledException"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result that never arrives.</typeparam>
        /// <param name="source">The source whose task is awaited; the token registration completes it.</param>
        /// <param name="token">The token that ends the wait.</param>
        /// <returns>A task that is cancelled with <paramref name="token"/>.</returns>
        private static async UnityTask<T> NeverCoreAsync<T>(
              UnityTaskCompletionSource<T> source
            , CancellationToken token
        )
        {
            using (token.Register(
                  static state => ((UnityTaskCompletionSource<T>)state).TrySetResult(result: default)
                , source
            ))
            {
                await source.Task;
            }

            ThrowHelper.ThrowOperationCanceled(token);
            return default;
        }
    }
}
