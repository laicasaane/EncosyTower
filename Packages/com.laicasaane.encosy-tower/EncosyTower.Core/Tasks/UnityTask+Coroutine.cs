using System;
using System.Collections;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Returns a coroutine enumerator that calls <paramref name="taskFactory"/> on its first <c>MoveNext</c> and
        /// runs until the returned task completes.
        /// </summary>
        /// <param name="taskFactory">The method that creates the task.</param>
        /// <returns>An enumerator for <c>StartCoroutine</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> an exception of the task is rethrown from <c>MoveNext</c>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.ToCoroutine</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="taskFactory"/> is <c>null</c>.</exception>
        public static IEnumerator ToCoroutine(Func<UnityTask> taskFactory)
        {
            Debugging.ThrowHelper.ThrowIfNull(taskFactory);
            return new UnityTaskCoroutineEnumerator(taskFactory);
        }
    }
}
