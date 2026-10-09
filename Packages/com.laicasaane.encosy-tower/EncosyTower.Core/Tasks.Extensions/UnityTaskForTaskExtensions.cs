using System;
using System.Threading;
using System.Threading.Tasks;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Converts <see cref="Task"/> and <see cref="Task{TResult}"/> to <see cref="UnityTask"/>.
    /// </summary>
    public static class UnityTaskForTaskExtensions
    {
        /// <summary>
        /// Returns a <see cref="UnityTask"/> that completes with the outcome of <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task to convert.</param>
        /// <returns>A task that completes when <paramref name="task"/> completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> a faulted <paramref name="task"/> rethrows its first inner exception, or the
        /// <see cref="AggregateException"/> when it has none. A canceled <paramref name="task"/> throws
        /// <see cref="OperationCanceledException"/>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.AsUniTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is <c>null</c>.</exception>
        public static UnityTask AsUnityTask(this Task task)
        {
            Debugging.ThrowHelper.ThrowIfNull(task);

            var source = new UnityTaskCompletionSource();

            task.ContinueWith(
                  static (completed, state) => Complete(completed, (UnityTaskCompletionSource)state)
                , source
                , CancellationToken.None
                , TaskContinuationOptions.ExecuteSynchronously
                , TaskScheduler.Default
            );

            return source.Task;
        }

        /// <summary>
        /// Returns a <see cref="UnityTask{T}"/> that completes with the outcome of <paramref name="task"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to convert.</param>
        /// <returns>A task that completes with the result of <paramref name="task"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as <see cref="AsUnityTask(Task)"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.AsUniTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is <c>null</c>.</exception>
        public static UnityTask<T> AsUnityTask<T>(this Task<T> task)
        {
            Debugging.ThrowHelper.ThrowIfNull(task);

            var source = new UnityTaskCompletionSource<T>();

            task.ContinueWith(
                  static (completed, state) => Complete(completed, (UnityTaskCompletionSource<T>)state)
                , source
                , CancellationToken.None
                , TaskContinuationOptions.ExecuteSynchronously
                , TaskScheduler.Default
            );

            return source.Task;
        }

        /// <summary>
        /// Copies the outcome of the finished <paramref name="task"/> to <paramref name="source"/>.
        /// </summary>
        /// <param name="task">The finished task.</param>
        /// <param name="source">The source to complete.</param>
        /// <remarks>
        /// A fault is unwrapped to its first inner exception; a cancellation completes the source as cancelled; any
        /// other state completes it successfully. The continuation runs synchronously on the thread that finished the
        /// task.
        /// </remarks>
        private static void Complete(Task task, UnityTaskCompletionSource source)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                {
                    source.TrySetException(task.Exception.InnerException ?? task.Exception);
                    break;
                }

                case TaskStatus.Canceled:
                {
                    source.TrySetCanceled();
                    break;
                }

                default:
                {
                    source.TrySetResult();
                    break;
                }
            }
        }

        /// <summary>
        /// Copies the outcome of the finished <paramref name="task"/> to <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The finished task.</param>
        /// <param name="source">The source to complete.</param>
        /// <remarks>
        /// Handles faults and cancellation as the overload without a result does, and completes the source with the
        /// result of the task otherwise.
        /// </remarks>
        private static void Complete<T>(Task<T> task, UnityTaskCompletionSource<T> source)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                {
                    source.TrySetException(task.Exception.InnerException ?? task.Exception);
                    break;
                }

                case TaskStatus.Canceled:
                {
                    source.TrySetCanceled();
                    break;
                }

                default:
                {
                    source.TrySetResult(task.Result);
                    break;
                }
            }
        }
    }
}
