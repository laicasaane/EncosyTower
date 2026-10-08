using System;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Waits for an <see cref="AsyncOperation"/> by polling it instead of using its <c>completed</c> event.
    /// </summary>
    /// <remarks>
    /// Polling keeps progress reporting and the choice of player loop phase, as UniTask's <c>ToUniTask</c> does.
    /// </remarks>
    internal static class UnityTaskOperationPolling
    {
        /// <summary>
        /// Reports <see cref="AsyncOperation.progress"/> and yields once at <paramref name="timing"/> per tick until
        /// <see cref="AsyncOperation.isDone"/> is <c>true</c>, then reports the final progress.
        /// </summary>
        /// <remarks>
        /// Throws if <paramref name="token"/> is already canceled; later cancellation is handled by the yield.
        /// </remarks>
        internal static async UnityTask WaitAsync(
              AsyncOperation operation
            , IProgress<float> progress
            , UnityTaskTiming timing
            , bool cancelImmediately
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            while (operation.isDone == false)
            {
                progress?.Report(operation.progress);
                await UnityTask.Yield(timing, cancelImmediately, token);
            }

            progress?.Report(operation.progress);
        }
    }
}
