using System.Threading;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Awaits <see cref="CancellationToken"/> cancellation as a <see cref="UnityTask"/>.
    /// </summary>
    public static class EncosyCancellationTokenExtensions
    {
        /// <summary>
        /// Returns a task that completes successfully when <paramref name="token"/> is cancelled.
        /// </summary>
        /// <param name="token">The token to observe.</param>
        /// <returns>A task that completes from the token's cancellation callback. It never throws.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> same as <c>UnityTask.WaitUntilCanceledAsync(token, completeImmediately: true)</c>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenExtensions.WaitUntilCanceled</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask WaitUntilCanceledAsync(this CancellationToken token)
            => UnityTask.WaitUntilCanceledAsync(token, UnityTaskTiming.Update, completeImmediately: true);
    }
}
