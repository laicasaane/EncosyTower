using System;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Returns a task that completes with <c>true</c> when this task is cancelled instead of throwing.
        /// </summary>
        /// <returns>A task that completes with <c>true</c> when this task was cancelled; otherwise
        /// <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> awaits this task. An <see cref="OperationCanceledException"/> becomes <c>true</c>; any
        /// other exception is rethrown unchanged.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.SuppressCancellationThrow</c>; Unity: none.
        /// </para>
        /// </remarks>
        public async UnityTask<bool> SuppressCancellationThrowAsync()
        {
            try
            {
                await this;
                return false;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
        }
    }

    public readonly partial struct UnityTask<T>
    {
        /// <summary>
        /// Returns a task that reports cancellation of this task as a value instead of throwing.
        /// </summary>
        /// <returns>A task that completes with <c>(true, default)</c> when this task was cancelled; otherwise
        /// <c>(false, result)</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> awaits this task. An <see cref="OperationCanceledException"/> becomes
        /// <c>isCanceled = true</c>; any other exception is rethrown unchanged.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.SuppressCancellationThrow</c>; Unity: none.
        /// </para>
        /// </remarks>
        public async UnityTask<(bool isCanceled, T result)> SuppressCancellationThrowAsync()
        {
            try
            {
                return (false, await this);
            }
            catch (OperationCanceledException)
            {
                return (true, default);
            }
        }
    }
}
