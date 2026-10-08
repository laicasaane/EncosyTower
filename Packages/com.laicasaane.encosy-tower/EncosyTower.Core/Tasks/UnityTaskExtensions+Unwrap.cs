using System.Threading.Tasks;

namespace EncosyTower.Tasks
{
    public static partial class UnityTaskExtensions
    {
        /// <summary>
        /// Returns a task that awaits <paramref name="task"/> and then the task it produced.
        /// </summary>
        /// <typeparam name="T">The type of the inner result.</typeparam>
        /// <param name="task">The task that produces another task.</param>
        /// <returns>A task that completes with the result of the inner task.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.Unwrap</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask<T> UnwrapAsync<T>(this UnityTask<UnityTask<T>> task)
            => await await task;

        /// <summary>
        /// Returns a task that awaits <paramref name="task"/> and then the task it produced.
        /// </summary>
        /// <param name="task">The task that produces another task.</param>
        /// <returns>A task that completes when the inner task completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.Unwrap</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask UnwrapAsync(this UnityTask<UnityTask> task)
            => await await task;

        /// <summary>
        /// Returns a task that awaits <paramref name="task"/> and then the task it produced.
        /// </summary>
        /// <typeparam name="T">The type of the inner result.</typeparam>
        /// <param name="task">The task that produces another task.</param>
        /// <returns>A task that completes with the result of the inner task.</returns>
        /// <remarks>
        /// <para>
        /// <b>Thread:</b> awaiting <paramref name="task"/> resumes on the captured synchronization context, as a plain
        /// <c>await</c> of a <see cref="Task{TResult}"/> does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.Unwrap</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask<T> UnwrapAsync<T>(this Task<UnityTask<T>> task)
            => await await task;

        /// <summary>
        /// Returns a task that awaits <paramref name="task"/> and then the task it produced.
        /// </summary>
        /// <param name="task">The task that produces another task.</param>
        /// <returns>A task that completes when the inner task completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Thread:</b> awaiting <paramref name="task"/> resumes on the captured synchronization context, as a plain
        /// <c>await</c> of a <see cref="Task{TResult}"/> does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.Unwrap</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask UnwrapAsync(this Task<UnityTask> task)
            => await await task;

        /// <summary>
        /// Returns a task that awaits <paramref name="task"/> and then the <see cref="Task{TResult}"/> it produced.
        /// </summary>
        /// <typeparam name="T">The type of the inner result.</typeparam>
        /// <param name="task">The task that produces another task.</param>
        /// <returns>A task that completes with the result of the inner task.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.Unwrap</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask<T> UnwrapAsync<T>(this UnityTask<Task<T>> task)
            => await await task;

        /// <summary>
        /// Returns a task that awaits <paramref name="task"/> and then the <see cref="Task"/> it produced.
        /// </summary>
        /// <param name="task">The task that produces another task.</param>
        /// <returns>A task that completes when the inner task completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.Unwrap</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask UnwrapAsync(this UnityTask<Task> task)
            => await await task;
    }
}
