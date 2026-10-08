using System;
using System.Threading;

namespace EncosyTower.Tasks
{
    public static partial class UnityTaskExtensions
    {
        /// <summary>
        /// Returns a token that is cancelled when <paramref name="task"/> completes in any state.
        /// </summary>
        /// <param name="task">The task to observe. It is consumed; a fault other than cancellation is logged.</param>
        /// <returns>A token cancelled after <paramref name="task"/> completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenExtensions.ToCancellationToken</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static CancellationToken ToCancellationToken(this UnityTask task)
        {
            var source = new CancellationTokenSource();
            CancelWhenCompletedAsync(task, source).Forget();
            return source.Token;
        }

        /// <summary>
        /// Returns a token that is cancelled when <paramref name="task"/> completes or <paramref name="linkToken"/> is
        /// cancelled.
        /// </summary>
        /// <param name="task">The task to observe. It is consumed.</param>
        /// <param name="linkToken">A token whose cancellation also cancels the returned token.</param>
        /// <returns>A linked token.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenExtensions.ToCancellationToken</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static CancellationToken ToCancellationToken(this UnityTask task, CancellationToken linkToken)
        {
            if (linkToken.IsCancellationRequested)
            {
                task.Forget();
                return linkToken;
            }

            if (linkToken.CanBeCanceled == false)
            {
                return ToCancellationToken(task);
            }

            var source = CancellationTokenSource.CreateLinkedTokenSource(linkToken);
            CancelWhenCompletedAsync(task, source).Forget();
            return source.Token;
        }

        /// <summary>
        /// Returns a token that is cancelled when <paramref name="task"/> completes in any state.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to observe. It is consumed and its result is discarded.</param>
        /// <returns>A token cancelled after <paramref name="task"/> completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenExtensions.ToCancellationToken</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static CancellationToken ToCancellationToken<T>(this UnityTask<T> task)
            => ToCancellationToken(task.AsUnityTask());

        /// <summary>
        /// Returns a token that is cancelled when <paramref name="task"/> completes or <paramref name="linkToken"/> is
        /// cancelled.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to observe. It is consumed and its result is discarded.</param>
        /// <param name="linkToken">A token whose cancellation also cancels the returned token.</param>
        /// <returns>A linked token.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenExtensions.ToCancellationToken</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static CancellationToken ToCancellationToken<T>(this UnityTask<T> task, CancellationToken linkToken)
            => ToCancellationToken(task.AsUnityTask(), linkToken);

        /// <summary>
        /// Awaits <paramref name="task"/>, then cancels and disposes <paramref name="source"/>.
        /// </summary>
        /// <param name="task">The task whose completion triggers the cancellation.</param>
        /// <param name="source">The source to cancel and dispose when the task ends.</param>
        /// <remarks>
        /// The source is cancelled whatever the outcome of the task. A cancellation of the task is ignored; any other
        /// exception is logged as unobserved before the source is cancelled.
        /// </remarks>
        private static async UnityTaskVoid CancelWhenCompletedAsync(UnityTask task, CancellationTokenSource source)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                if (exception is OperationCanceledException == false)
                {
                    ThrowHelper.LogUnobservedException(exception);
                }
            }

            source.Cancel();
            source.Dispose();
        }
    }
}
