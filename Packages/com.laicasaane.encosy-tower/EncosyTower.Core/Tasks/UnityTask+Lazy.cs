using System;
using System.Threading;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Creates an <see cref="AsyncLazy"/> that calls <paramref name="factory"/> on first use.
        /// </summary>
        /// <param name="factory">The method that starts the operation.</param>
        /// <returns>A new lazy operation.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Lazy</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public static AsyncLazy Lazy(Func<UnityTask> factory)
            => new(factory);

        /// <summary>
        /// Creates an <see cref="AsyncLazy{T}"/> that calls <paramref name="factory"/> on first use.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="factory">The method that starts the operation.</param>
        /// <returns>A new lazy operation.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Lazy</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public static AsyncLazy<T> Lazy<T>(Func<UnityTask<T>> factory)
            => new(factory);

        /// <summary>
        /// Calls <paramref name="asyncCoroutine"/> with the destroy token of <paramref name="monoBehaviour"/> and
        /// returns its task.
        /// </summary>
        /// <param name="monoBehaviour">The behaviour whose <c>destroyCancellationToken</c> is passed.</param>
        /// <param name="asyncCoroutine">The asynchronous method to run.</param>
        /// <returns>The task returned by <paramref name="asyncCoroutine"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the token is cancelled when <paramref name="monoBehaviour"/> is destroyed. Call it on the
        /// main thread.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAsyncExtensions.StartAsyncCoroutine</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncCoroutine"/> is <c>null</c>.</exception>
        public static UnityTask StartAsyncCoroutine(
              MonoBehaviour monoBehaviour
            , Func<CancellationToken, UnityTask> asyncCoroutine
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncCoroutine);
            return asyncCoroutine(monoBehaviour.AssumeValid().destroyCancellationToken);
        }
    }
}
