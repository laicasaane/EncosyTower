using System;
using System.Threading;
using UnityEngine.Rendering;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Awaits <see cref="AsyncGPUReadbackRequest"/> as a <see cref="UnityTask"/>.
    /// </summary>
    public static class UnityTaskForAsyncGPUReadbackRequestExtensions
    {
        /// <summary>
        /// Returns a task that completes when <paramref name="request"/> is done, or is cancelled by
        /// <paramref name="token"/>.
        /// </summary>
        /// <param name="request">The request to wait for.</param>
        /// <param name="token">The token that cancels the wait. It does not cancel the request.</param>
        /// <returns>A task that completes with the finished request.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask<AsyncGPUReadbackRequest> WithCancellation(
              this AsyncGPUReadbackRequest request
            , CancellationToken token
        )
            => ToUnityTask(request, cancelImmediately: false, token: token);

        /// <summary>
        /// Returns a task that completes when <paramref name="request"/> is done, or is cancelled by
        /// <paramref name="token"/>.
        /// </summary>
        /// <param name="request">The request to wait for.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait. It does not cancel the request.</param>
        /// <returns>A task that completes with the finished request.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask<AsyncGPUReadbackRequest> WithCancellation(
              this AsyncGPUReadbackRequest request
            , bool cancelImmediately
            , CancellationToken token
        )
            => ToUnityTask(request, cancelImmediately: cancelImmediately, token: token);

        /// <summary>
        /// Returns a task that completes when <paramref name="request"/> is done.
        /// </summary>
        /// <param name="request">The request to wait for.</param>
        /// <param name="timing">The player-loop phase at which the request is checked.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait. It does not cancel the request.</param>
        /// <returns>A task that completes with the finished request.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks the request after each <paramref name="timing"/> tick. A request with
        /// <see cref="AsyncGPUReadbackRequest.hasError"/> throws <see cref="InvalidOperationException"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAsyncExtensions.ToUniTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask<AsyncGPUReadbackRequest> ToUnityTask(
              this AsyncGPUReadbackRequest request
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            token.ThrowIfCancellationRequested();

            while (true)
            {
                if (request.hasError)
                {
                    ThrowHelper.ThrowGPUReadbackError();
                }

                if (request.done)
                {
                    return request;
                }

                await UnityTask.Yield(timing, cancelImmediately, token);
            }
        }
    }
}
