using System;
using System.Threading;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Creates a <see cref="UnityTask"/> whose completion is set explicitly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Behaviour:</b> the first completion wins; later completions leave <see cref="Task"/> unchanged.
    /// Awaiting a faulted <see cref="Task"/> throws the original exception instance with its original stack.
    /// An <see cref="OperationCanceledException"/> passed to <see cref="TrySetException"/> is rethrown as the same
    /// instance. <see cref="TrySetCanceled"/> completes with <c>new OperationCanceledException(token)</c>.
    /// </para>
    /// <para>
    /// <b>Thread:</b> <see cref="Task"/> resumes its awaiter on the kind of thread that created this source: the
    /// main thread when created on the main thread, a thread-pool thread otherwise. A completion from another
    /// thread kind posts the continuation to Unity's main <see cref="SynchronizationContext"/> or to the
    /// <see cref="ThreadPool"/>. <see cref="UnityTask.IsCompleted"/> returns <c>false</c> while the current thread
    /// kind differs from the creator's.
    /// </para>
    /// <para><b>Undefined behaviour:</b></para>
    /// <list type="bullet">
    /// <item><description>
    /// Awaiting <see cref="Task"/> or a copy more than once, or calling <c>GetResult</c> twice: may throw
    /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
    /// because sources are pooled.
    /// </description></item>
    /// <item><description>
    /// Calling <c>Task.GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
    /// </description></item>
    /// <item><description>
    /// Calling Unity APIs after resuming on a thread-pool thread (source created off the main thread): Unity throws
    /// or corrupts state, per Unity rules.
    /// </description></item>
    /// </list>
    /// </remarks>
    public sealed partial class UnityTaskCompletionSource
    {
        /// <summary>
        /// Completes <see cref="Task"/> successfully.
        /// </summary>
        /// <exception cref="InvalidOperationException">The source has already been completed.</exception>
        public void SetResult()
        {
            if (TrySetResult() == false)
            {
                ThrowHelper.ThrowAlreadyCompleted();
            }
        }

        /// <summary>
        /// Completes <see cref="Task"/> with <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The exception that awaiting <see cref="Task"/> rethrows unchanged.</param>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The source has already been completed.</exception>
        public void SetException(Exception exception)
        {
            if (TrySetException(exception) == false)
            {
                ThrowHelper.ThrowAlreadyCompleted();
            }
        }

        /// <summary>
        /// Completes <see cref="Task"/> with <c>new OperationCanceledException(token)</c>.
        /// </summary>
        /// <param name="token">The token stored in the thrown <see cref="OperationCanceledException"/>.</param>
        /// <exception cref="InvalidOperationException">The source has already been completed.</exception>
        public void SetCanceled(CancellationToken token = default)
        {
            if (TrySetCanceled(token) == false)
            {
                ThrowHelper.ThrowAlreadyCompleted();
            }
        }

        /// <summary>
        /// Attempts to complete <see cref="Task"/> with <c>new OperationCanceledException(token)</c>.
        /// </summary>
        /// <param name="token">The token stored in the thrown <see cref="OperationCanceledException"/>.</param>
        /// <returns><c>true</c> if this call completed the source; <c>false</c> if it was already completed.</returns>
        public bool TrySetCanceled(CancellationToken token = default)
            => TrySetException(new OperationCanceledException(token));
    }
}
