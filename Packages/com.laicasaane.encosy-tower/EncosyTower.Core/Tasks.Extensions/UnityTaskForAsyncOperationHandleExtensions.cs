#if UNITY_ADDRESSABLES

using System;
using System.Threading;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Awaits Addressables <see cref="AsyncOperationHandle"/> and <see cref="AsyncOperationHandle{TObject}"/> as a
    /// <see cref="UnityTask"/>.
    /// </summary>
    public static class UnityTaskForAsyncOperationHandleExtensions
    {
        /// <summary>
        /// Returns a task that completes when <paramref name="handle"/> is done, or is cancelled by
        /// <paramref name="token"/>.
        /// </summary>
        /// <param name="handle">The operation handle to wait for.</param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes when the operation is done.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>AddressablesAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask WithCancellation(
              this AsyncOperationHandle handle
            , CancellationToken token
        )
        {
            return ToUnityTask(
                  handle
                , progress: null
                , cancelImmediately: false
                , autoReleaseWhenCanceled: false
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="handle"/> is done, or is cancelled by
        /// <paramref name="token"/>.
        /// </summary>
        /// <param name="handle">The operation handle to wait for.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes when the operation is done.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>AddressablesAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask WithCancellation(
              this AsyncOperationHandle handle
            , bool cancelImmediately
            , CancellationToken token
        )
        {
            return ToUnityTask(
                  handle
                , progress: null
                , cancelImmediately: cancelImmediately
                , autoReleaseWhenCanceled: false
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="handle"/> is done, or is cancelled by
        /// <paramref name="token"/>.
        /// </summary>
        /// <param name="handle">The operation handle to wait for.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="autoReleaseWhenCanceled">
        /// <c>true</c> to release <paramref name="handle"/> when the wait is cancelled.
        /// </param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes when the operation is done.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>AddressablesAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask WithCancellation(
              this AsyncOperationHandle handle
            , bool cancelImmediately
            , bool autoReleaseWhenCanceled
            , CancellationToken token
        )
        {
            return ToUnityTask(
                  handle
                , progress: null
                , cancelImmediately: cancelImmediately
                , autoReleaseWhenCanceled: autoReleaseWhenCanceled
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="handle"/> is done.
        /// </summary>
        /// <param name="handle">The operation handle to wait for.</param>
        /// <param name="progress">Receives the download progress on each check; <c>null</c> for none.</param>
        /// <param name="timing">The player-loop phase at which the operation is checked.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="autoReleaseWhenCanceled">
        /// <c>true</c> to release <paramref name="handle"/> when the wait is cancelled.
        /// </param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes when the operation is done.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes synchronously when the operation is already done or the handle is not valid,
        /// such as a handle that was released automatically. Otherwise checks it after each
        /// <paramref name="timing"/> tick of the shared Encosy player-loop scheduler. A failed operation throws its
        /// <see cref="AsyncOperationHandle.OperationException"/>. Cancellation throws
        /// <c>new OperationCanceledException(token)</c> and leaves the operation running.
        /// </para>
        /// <para>
        /// <b>Thread:</b> call it on the main thread. The awaiter resumes on the main thread.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>AddressablesAsyncExtensions.ToUniTask</c>; Addressables:
        /// <see cref="AsyncOperationHandle.Task"/>, which reports no progress, cannot be cancelled and has no
        /// player-loop phase choice.
        /// </para>
        /// </remarks>
        public static async UnityTask ToUnityTask(
              this AsyncOperationHandle handle
            , IProgress<float> progress = null
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , bool autoReleaseWhenCanceled = false
            , CancellationToken token = default
        )
        {
            token.ThrowIfCancellationRequested();

            try
            {
                while (handle.IsDone == false)
                {
                    progress?.Report(handle.GetDownloadStatus().Percent);
                    await UnityTask.Yield(timing, cancelImmediately, token);
                }
            }
            catch (OperationCanceledException) when (autoReleaseWhenCanceled)
            {
                if (handle.IsValid())
                {
                    handle.Release();
                }

                throw;
            }

            if (handle.IsValid() == false)
            {
                return;
            }

            progress?.Report(handle.GetDownloadStatus().Percent);

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                ThrowHelper.ThrowOperationHandleFailed(handle.OperationException);
            }
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="handle"/> is done, or is cancelled by
        /// <paramref name="token"/>.
        /// </summary>
        /// <typeparam name="T">The type of the operation result.</typeparam>
        /// <param name="handle">The operation handle to wait for.</param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes with the operation result.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>AddressablesAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException"><paramref name="handle"/> is not valid.</exception>
        public static UnityTask<T> WithCancellation<T>(
              this AsyncOperationHandle<T> handle
            , CancellationToken token
        )
        {
            return ToUnityTask(
                  handle
                , progress: null
                , cancelImmediately: false
                , autoReleaseWhenCanceled: false
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="handle"/> is done, or is cancelled by
        /// <paramref name="token"/>.
        /// </summary>
        /// <typeparam name="T">The type of the operation result.</typeparam>
        /// <param name="handle">The operation handle to wait for.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes with the operation result.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>AddressablesAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException"><paramref name="handle"/> is not valid.</exception>
        public static UnityTask<T> WithCancellation<T>(
              this AsyncOperationHandle<T> handle
            , bool cancelImmediately
            , CancellationToken token
        )
        {
            return ToUnityTask(
                  handle
                , progress: null
                , cancelImmediately: cancelImmediately
                , autoReleaseWhenCanceled: false
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="handle"/> is done, or is cancelled by
        /// <paramref name="token"/>.
        /// </summary>
        /// <typeparam name="T">The type of the operation result.</typeparam>
        /// <param name="handle">The operation handle to wait for.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="autoReleaseWhenCanceled">
        /// <c>true</c> to release <paramref name="handle"/> when the wait is cancelled.
        /// </param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes with the operation result.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>AddressablesAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException"><paramref name="handle"/> is not valid.</exception>
        public static UnityTask<T> WithCancellation<T>(
              this AsyncOperationHandle<T> handle
            , bool cancelImmediately
            , bool autoReleaseWhenCanceled
            , CancellationToken token
        )
        {
            return ToUnityTask(
                  handle
                , progress: null
                , cancelImmediately: cancelImmediately
                , autoReleaseWhenCanceled: autoReleaseWhenCanceled
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="handle"/> is done.
        /// </summary>
        /// <typeparam name="T">The type of the operation result.</typeparam>
        /// <param name="handle">The operation handle to wait for.</param>
        /// <param name="progress">Receives the download progress on each check; <c>null</c> for none.</param>
        /// <param name="timing">The player-loop phase at which the operation is checked.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="autoReleaseWhenCanceled">
        /// <c>true</c> to release <paramref name="handle"/> when the wait is cancelled.
        /// </param>
        /// <param name="token">The token that cancels the wait. It does not cancel the operation.</param>
        /// <returns>A task that completes with <see cref="AsyncOperationHandle{TObject}.Result"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes synchronously when the operation is already done. Otherwise checks it after
        /// each <paramref name="timing"/> tick of the shared Encosy player-loop scheduler. A failed operation throws
        /// its <see cref="AsyncOperationHandle{TObject}.OperationException"/>; a handle released during the wait
        /// throws <see cref="InvalidOperationException"/>. Cancellation throws
        /// <c>new OperationCanceledException(token)</c> and leaves the operation running.
        /// </para>
        /// <para>
        /// <b>Thread:</b> call it on the main thread. The awaiter resumes on the main thread.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>AddressablesAsyncExtensions.ToUniTask</c>; Addressables:
        /// <see cref="AsyncOperationHandle{TObject}.Task"/>, which reports no progress, cannot be cancelled and has
        /// no player-loop phase choice.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException"><paramref name="handle"/> is not valid.</exception>
        public static UnityTask<T> ToUnityTask<T>(
              this AsyncOperationHandle<T> handle
            , IProgress<float> progress = null
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , bool autoReleaseWhenCanceled = false
            , CancellationToken token = default
        )
        {
            if (handle.IsValid() == false)
            {
                ThrowHelper.ThrowInvalidOperationHandle();
            }

            return ToUnityTaskCoreAsync(handle, progress, timing, cancelImmediately, autoReleaseWhenCanceled, token);
        }

        private static async UnityTask<T> ToUnityTaskCoreAsync<T>(
              AsyncOperationHandle<T> handle
            , IProgress<float> progress
            , UnityTaskTiming timing
            , bool cancelImmediately
            , bool autoReleaseWhenCanceled
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            try
            {
                while (handle.IsDone == false)
                {
                    progress?.Report(handle.GetDownloadStatus().Percent);
                    await UnityTask.Yield(timing, cancelImmediately, token);
                }
            }
            catch (OperationCanceledException) when (autoReleaseWhenCanceled)
            {
                if (handle.IsValid())
                {
                    handle.Release();
                }

                throw;
            }

            if (handle.IsValid() == false)
            {
                ThrowHelper.ThrowInvalidOperationHandle();
            }

            progress?.Report(handle.GetDownloadStatus().Percent);

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                ThrowHelper.ThrowOperationHandleFailed(handle.OperationException);
            }

            return handle.Result;
        }
    }
}

#endif
