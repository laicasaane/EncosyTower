using System.Threading.Tasks;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Converts <see cref="ValueTask"/> and <see cref="ValueTask{TResult}"/> to <see cref="UnityTask"/>.
    /// </summary>
    public static class UnityTaskForValueTaskExtensions
    {
        /// <summary>
        /// Returns a <see cref="UnityTask"/> that awaits <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The value task to convert. It is consumed.</param>
        /// <returns>A task that completes when <paramref name="task"/> completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Thread:</b> awaiting <paramref name="task"/> resumes on the captured synchronization context, as a plain
        /// <c>await</c> of a <see cref="ValueTask"/> does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.AsUniTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask AsUnityTask(this ValueTask task)
            => await task;

        /// <summary>
        /// Returns a <see cref="UnityTask{T}"/> that awaits <paramref name="task"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The value task to convert. It is consumed.</param>
        /// <returns>A task that completes with the result of <paramref name="task"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Thread:</b> awaiting <paramref name="task"/> resumes on the captured synchronization context, as a plain
        /// <c>await</c> of a <see cref="ValueTask{TResult}"/> does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.AsUniTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask<T> AsUnityTask<T>(this ValueTask<T> task)
            => await task;
    }
}
