using System;
using System.Collections;
using System.Threading.Tasks;

namespace EncosyTower.Tasks
{
    public static partial class UnityTaskExtensions
    {
        /// <summary>
        /// Returns a <see cref="Task"/> that completes with the outcome of <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task to convert.</param>
        /// <returns>A task that completes when <paramref name="task"/> completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> consumes <paramref name="task"/>. A fault keeps its exception instance; a cancellation
        /// produces a canceled <see cref="Task"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.AsTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static Task AsTask(this UnityTask task)
        {
            var source = new TaskCompletionSource<object>();
            CompleteTaskAsync(task, source).Forget();
            return source.Task;
        }

        /// <summary>
        /// Returns a <see cref="Task{TResult}"/> that completes with the outcome of <paramref name="task"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to convert.</param>
        /// <returns>A task that completes with the result of <paramref name="task"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as <see cref="AsTask(UnityTask)"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.AsTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static Task<T> AsTask<T>(this UnityTask<T> task)
        {
            var source = new TaskCompletionSource<T>();
            CompleteTaskAsync(task, source).Forget();
            return source.Task;
        }

        /// <summary>
        /// Returns a <see cref="ValueTask"/> that completes with the outcome of <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task to convert.</param>
        /// <returns>A value task backed by <see cref="AsTask(UnityTask)"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskValueTaskExtensions.AsValueTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static ValueTask AsValueTask(this UnityTask task)
            => new(task.AsTask());

        /// <summary>
        /// Returns a <see cref="ValueTask{TResult}"/> that completes with the outcome of <paramref name="task"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to convert.</param>
        /// <returns>A value task backed by <see cref="AsTask{T}(UnityTask{T})"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskValueTaskExtensions.AsValueTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static ValueTask<T> AsValueTask<T>(this UnityTask<T> task)
            => new(task.AsTask());

        /// <summary>
        /// Returns a coroutine enumerator that runs until <paramref name="task"/> completes.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        /// <param name="exceptionHandler">
        /// Receives the exception of <paramref name="task"/>; <c>null</c> to rethrow it from <c>MoveNext</c>.
        /// </param>
        /// <returns>An enumerator for <c>StartCoroutine</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the first <c>MoveNext</c> starts awaiting <paramref name="task"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.ToCoroutine</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static IEnumerator ToCoroutine(this UnityTask task, Action<Exception> exceptionHandler = null)
            => new UnityTaskCoroutineEnumerator(task, exceptionHandler);

        /// <summary>
        /// Returns a coroutine enumerator that runs until <paramref name="task"/> completes.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to wait for.</param>
        /// <param name="resultHandler">Receives the result of <paramref name="task"/>; <c>null</c> for none.</param>
        /// <param name="exceptionHandler">
        /// Receives the exception of <paramref name="task"/>; <c>null</c> to rethrow it from <c>MoveNext</c>.
        /// </param>
        /// <returns>An enumerator for <c>StartCoroutine</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the first <c>MoveNext</c> starts awaiting <paramref name="task"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.ToCoroutine</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static IEnumerator ToCoroutine<T>(
              this UnityTask<T> task
            , Action<T> resultHandler = null
            , Action<Exception> exceptionHandler = null
        )
            => new UnityTaskCoroutineEnumerator<T>(task, resultHandler, exceptionHandler);

        /// <summary>
        /// Completes <paramref name="source"/> with the outcome of <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task to await.</param>
        /// <param name="source">The source behind the <c>Task</c> returned to the caller.</param>
        /// <remarks>
        /// A cancellation becomes a cancelled source that keeps the token of the exception; any other exception faults
        /// the source. Nothing escapes this method, so it is started with <c>Forget</c>.
        /// </remarks>
        private static async UnityTaskVoid CompleteTaskAsync(UnityTask task, TaskCompletionSource<object> source)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException exception)
            {
                source.TrySetCanceled(exception.CancellationToken);
                return;
            }
            catch (Exception exception)
            {
                source.TrySetException(exception);
                return;
            }

            source.TrySetResult(null);
        }

        /// <summary>
        /// Completes <paramref name="source"/> with the result of <paramref name="task"/> or its exception.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to await.</param>
        /// <param name="source">The source behind the <c>Task&lt;T&gt;</c> returned to the caller.</param>
        /// <remarks>
        /// Handles cancellation and other exceptions as the overload without a result does.
        /// </remarks>
        private static async UnityTaskVoid CompleteTaskAsync<T>(UnityTask<T> task, TaskCompletionSource<T> source)
        {
            T result;

            try
            {
                result = await task;
            }
            catch (OperationCanceledException exception)
            {
                source.TrySetCanceled(exception.CancellationToken);
                return;
            }
            catch (Exception exception)
            {
                source.TrySetException(exception);
                return;
            }

            source.TrySetResult(result);
        }
    }
}
