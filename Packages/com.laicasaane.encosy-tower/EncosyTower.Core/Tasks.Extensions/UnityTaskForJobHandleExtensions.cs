using System.Threading;
using Unity.Jobs;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Awaits <see cref="JobHandle"/> as a <see cref="UnityTask"/>.
    /// </summary>
    public static class UnityTaskForJobHandleExtensions
    {
        /// <summary>
        /// Gets an awaiter so that <paramref name="jobHandle"/> can be awaited directly as a <see cref="UnityTask"/>.
        /// </summary>
        /// <param name="jobHandle">The job to wait for.</param>
        /// <returns>An awaiter that completes after the job is completed.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> same as <c>ToUnityTask(jobHandle, UnityTaskTiming.Update)</c>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>GetAwaiter</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask.Awaiter GetAwaiter(this JobHandle jobHandle)
            => ToUnityTask(jobHandle, UnityTaskTiming.Update).GetAwaiter();

        /// <summary>
        /// Returns a task that waits one <paramref name="timing"/> tick, then completes <paramref name="jobHandle"/>.
        /// </summary>
        /// <param name="jobHandle">The job to complete.</param>
        /// <param name="timing">The player-loop phase after which the job is completed.</param>
        /// <param name="token">Checked after the job is completed.</param>
        /// <returns>A task that completes after <see cref="JobHandle.Complete"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as UniTask, the job is always completed; a cancelled <paramref name="token"/> then throws
        /// <c>new OperationCanceledException(token)</c>. Call it on the main thread.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAsyncExtensions.WaitAsync</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask WaitAsync(
              this JobHandle jobHandle
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , CancellationToken token = default
        )
        {
            await UnityTask.Yield(timing);
            jobHandle.Complete();
            token.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Returns a task that completes <paramref name="jobHandle"/> once <see cref="JobHandle.IsCompleted"/> is
        /// <c>true</c>.
        /// </summary>
        /// <param name="jobHandle">The job to wait for.</param>
        /// <param name="timing">The player-loop phase at which the job is checked.</param>
        /// <returns>A task that completes after <see cref="JobHandle.Complete"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes synchronously when the job is already done. No token is accepted, because a
        /// scheduled job cannot be cancelled. Call it on the main thread.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UnityAsyncExtensions.ToUniTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask ToUnityTask(this JobHandle jobHandle, UnityTaskTiming timing)
        {
            while (jobHandle.IsCompleted == false)
            {
                await UnityTask.Yield(timing);
            }

            jobHandle.Complete();
        }
    }
}
