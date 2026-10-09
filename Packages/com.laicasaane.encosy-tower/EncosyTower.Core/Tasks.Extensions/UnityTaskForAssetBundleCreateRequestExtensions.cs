#if UNITY_ASSETBUNDLE

using System;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Awaits <see cref="AssetBundleCreateRequest"/> as a <see cref="UnityTask"/>.
    /// </summary>
    public static class UnityTaskForAssetBundleCreateRequestExtensions
    {
        /// <summary>
        /// Gets an awaiter so that <paramref name="operation"/> can be awaited directly as a <see cref="UnityTask"/>.
        /// </summary>
        /// <param name="operation">The operation to wait for.</param>
        /// <returns>An awaiter that completes with the loaded asset bundle.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> same as <c>ToUnityTask</c> with default arguments.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>GetAwaiter</c>; Unity: the inherited
        /// <c>AsyncOperationAwaitableExtensions.GetAwaiter</c>, which this more specific overload takes precedence
        /// over and which returns no result.
        /// </para>
        /// </remarks>
        public static UnityTask<AssetBundle>.Awaiter GetAwaiter(this AssetBundleCreateRequest operation)
            => ToUnityTask(operation).GetAwaiter();

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
        public static UnityTask<AssetBundle> WithCancellation(
              this AssetBundleCreateRequest operation
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
        public static UnityTask<AssetBundle> WithCancellation(
              this AssetBundleCreateRequest operation
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
        public static UnityTask<AssetBundle> ToUnityTask(
              this AssetBundleCreateRequest operation
            , IProgress<float> progress = null
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(operation);
            return ToUnityTaskCoreAsync(operation, progress, timing, cancelImmediately, token);
        }

        private static async UnityTask<AssetBundle> ToUnityTaskCoreAsync(
              AssetBundleCreateRequest operation
            , IProgress<float> progress
            , UnityTaskTiming timing
            , bool cancelImmediately
            , CancellationToken token
        )
        {
            await UnityTaskOperationPolling.WaitAsync(operation, progress, timing, cancelImmediately, token);
            return operation.assetBundle;
        }
    }
}

#endif
