namespace EncosyTower.Tasks
{
    public static partial class UnityTaskExtensions
    {
        /// <summary>
        /// Wraps <paramref name="task"/> in an <see cref="AsyncLazy"/> whose outcome can be awaited many times.
        /// </summary>
        /// <param name="task">The task to wrap. It is consumed by the lazy operation.</param>
        /// <returns>A lazy operation that awaits <paramref name="task"/> on first use.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.ToAsyncLazy</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static AsyncLazy ToAsyncLazy(this UnityTask task)
            => new(task);

        /// <summary>
        /// Wraps <paramref name="task"/> in an <see cref="AsyncLazy{T}"/> whose result can be awaited many times.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to wrap. It is consumed by the lazy operation.</param>
        /// <returns>A lazy operation that awaits <paramref name="task"/> on first use.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.ToAsyncLazy</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static AsyncLazy<T> ToAsyncLazy<T>(this UnityTask<T> task)
            => new(task);
    }
}
