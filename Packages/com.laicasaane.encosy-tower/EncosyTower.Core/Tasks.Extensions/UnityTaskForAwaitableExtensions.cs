using UnityEngine;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Converts <see cref="Awaitable"/> and <see cref="Awaitable{T}"/> to <see cref="UnityTask"/> on both backends.
    /// </summary>
    public static class UnityTaskForAwaitableExtensions
    {
        /// <summary>
        /// Returns a <see cref="UnityTask"/> that completes with the outcome of <paramref name="awaitable"/>.
        /// </summary>
        /// <param name="awaitable">The awaitable to convert. It is consumed.</param>
        /// <returns>A task that completes when <paramref name="awaitable"/> completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Interop:</b> on the UniTask backend the awaiter resumes on the kind of thread that called this method;
        /// on the <c>Awaitable</c> backend it resumes on the thread that completes <paramref name="awaitable"/>, as the
        /// implicit conversion does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAwaitableExtensions.AsUniTask</c>; Unity: none.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend an <c>async UnityTask</c> method awaits the awaitable; on the
        /// <c>Awaitable</c> backend this uses the implicit conversion's pooled relay.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="awaitable"/> is <c>null</c>.</exception>
        public static UnityTask AsUnityTask(this Awaitable awaitable)
        {
            Debugging.ThrowHelper.ThrowIfNull(awaitable);
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
            return RelayAsync(awaitable);
#else
            return awaitable;
#endif
        }

        /// <summary>
        /// Returns a <see cref="UnityTask{T}"/> that completes with the outcome of <paramref name="awaitable"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="awaitable">The awaitable to convert. It is consumed.</param>
        /// <returns>A task that completes with the result of <paramref name="awaitable"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Interop:</b> on the UniTask backend the awaiter resumes on the kind of thread that called this method;
        /// on the <c>Awaitable</c> backend it resumes on the thread that completes <paramref name="awaitable"/>, as the
        /// implicit conversion does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAwaitableExtensions.AsUniTask</c>; Unity: none.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend an <c>async UnityTask</c> method awaits the awaitable; on the
        /// <c>Awaitable</c> backend this uses the implicit conversion's pooled relay.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="awaitable"/> is <c>null</c>.</exception>
        public static UnityTask<T> AsUnityTask<T>(this Awaitable<T> awaitable)
        {
            Debugging.ThrowHelper.ThrowIfNull(awaitable);
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
            return RelayAsync(awaitable);
#else
            return awaitable;
#endif
        }

#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
        private static async UnityTask RelayAsync(Awaitable awaitable)
        {
            await awaitable;
        }

        private static async UnityTask<T> RelayAsync<T>(Awaitable<T> awaitable)
            => await awaitable;
#endif
    }
}
