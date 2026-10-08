using System;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Awaits <see cref="ResourceRequest"/> as a <see cref="UnityTask"/>.
    /// </summary>
    public static class EncosyResourceRequestExtensions
    {
        /// <summary>
        /// Returns a task that completes when <paramref name="operation"/> is done, or is cancelled by
        /// <paramref name="token"/>.
        /// </summary>
        /// <param name="operation">The operation to wait for.</param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes with the operation result.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask<UnityEngine.Object> WithCancellation(
              this ResourceRequest operation
            , CancellationToken token
        )
            => ToUnityTask(operation, progress: null, cancelImmediately: false, token: token);

        /// <summary>
        /// Returns a task that completes when <paramref name="operation"/> is done, or is cancelled by
        /// <paramref name="token"/>.
        /// </summary>
        /// <param name="operation">The operation to wait for.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes with the operation result.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask<UnityEngine.Object> WithCancellation(
              this ResourceRequest operation
            , bool cancelImmediately
            , CancellationToken token
        )
            => ToUnityTask(operation, progress: null, cancelImmediately: cancelImmediately, token: token);

        /// <summary>
        /// Returns a task that completes when <paramref name="operation"/> is done.
        /// </summary>
        /// <param name="operation">The operation to wait for.</param>
        /// <param name="progress">Receives the operation progress on each check; <c>null</c> for none.</param>
        /// <param name="timing">The player-loop phase at which the operation is checked.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes with the operation result.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes synchronously when the operation is already done. Otherwise checks it after
        /// each <paramref name="timing"/> tick of the shared Encosy player-loop scheduler. Cancellation throws
        /// <c>new OperationCanceledException(token)</c> and leaves the operation running.
        /// </para>
        /// <para>
        /// <b>Thread:</b> call it on the main thread. The awaiter resumes on the main thread.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAsyncExtensions.ToUniTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
        public static UnityTask<UnityEngine.Object> ToUnityTask(
              this ResourceRequest operation
            , IProgress<float> progress = null
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(operation);
            return ToUnityTaskCoreAsync(operation, progress, timing, cancelImmediately, token);
        }

        private static async UnityTask<UnityEngine.Object> ToUnityTaskCoreAsync(
              ResourceRequest operation
            , IProgress<float> progress
            , UnityTaskTiming timing
            , bool cancelImmediately
            , CancellationToken token
        )
        {
            await UnityTaskOperationPolling.WaitAsync(operation, progress, timing, cancelImmediately, token);
            return operation.asset;
        }
    }
}
