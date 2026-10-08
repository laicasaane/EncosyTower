using System;
using System.Threading;

namespace EncosyTower.Tasks
{
    public static partial class UnityTaskExtensions
    {
        /// <summary>
        /// Returns a task that completes with <paramref name="task"/> or is cancelled by <paramref name="token"/>,
        /// whichever happens first.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        /// <param name="token">The token that cancels the wait without cancelling <paramref name="task"/>.</param>
        /// <returns>A task that follows <paramref name="task"/> until <paramref name="token"/> is cancelled.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> returns <paramref name="task"/> unchanged when <paramref name="token"/> cannot be
        /// cancelled. On cancellation the returned task throws <c>new OperationCanceledException(token)</c>, and
        /// <paramref name="task"/> keeps running; it is still observed, so its fault is not lost silently.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.AttachExternalCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask AttachExternalCancellationAsync(this UnityTask task, CancellationToken token)
        {
            if (token.CanBeCanceled == false)
            {
                return task;
            }

            if (token.IsCancellationRequested)
            {
                task.Forget();
                return UnityTask.FromCanceled(token);
            }

            return AttachExternalCancellationCoreAsync(task, token);
        }

        /// <summary>
        /// Returns a task that completes with <paramref name="task"/> or is cancelled by <paramref name="token"/>,
        /// whichever happens first.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to wait for.</param>
        /// <param name="token">The token that cancels the wait without cancelling <paramref name="task"/>.</param>
        /// <returns>A task that follows <paramref name="task"/> until <paramref name="token"/> is cancelled.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as <see cref="AttachExternalCancellationAsync(UnityTask, CancellationToken)"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.AttachExternalCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask<T> AttachExternalCancellationAsync<T>(this UnityTask<T> task, CancellationToken token)
        {
            if (token.CanBeCanceled == false)
            {
                return task;
            }

            if (token.IsCancellationRequested)
            {
                task.Forget();
                return UnityTask.FromCanceled<T>(token);
            }

            return AttachExternalCancellationCoreAsync(task, token);
        }

        /// <summary>
        /// Returns a task that completes with <paramref name="task"/> or throws <see cref="TimeoutException"/> after
        /// <paramref name="timeout"/>.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        /// <param name="timeout">The longest time to wait.</param>
        /// <param name="delayType">The clock that measures <paramref name="timeout"/>.</param>
        /// <param name="timing">The player-loop phase at which the timeout is checked.</param>
        /// <param name="taskCancellationTokenSource">
        /// Cancelled and disposed when the timeout elapses, to stop <paramref name="task"/>; <c>null</c> for none.
        /// </param>
        /// <returns>A task that completes when <paramref name="task"/> completes in time.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> a fault of <paramref name="task"/> is rethrown unchanged. A cancellation of
        /// <paramref name="task"/> throws a new <see cref="OperationCanceledException"/>, as UniTask does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.Timeout</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="TimeoutException"><paramref name="timeout"/> elapsed first.</exception>
        public static async UnityTask TimeoutAsync(
              this UnityTask task
            , TimeSpan timeout
            , UnityTaskDelayType delayType = UnityTaskDelayType.DeltaTime
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , CancellationTokenSource taskCancellationTokenSource = null
        )
        {
            var (isTimeout, isCanceled) = await RaceTimeoutAsync(
                  task.SuppressCancellationThrowAsync()
                , timeout
                , delayType
                , timing
                , taskCancellationTokenSource
            );

            if (isTimeout)
            {
                ThrowHelper.ThrowTimeout(timeout);
            }

            if (isCanceled)
            {
                ThrowHelper.ThrowOperationCanceled(CancellationToken.None);
            }
        }

        /// <summary>
        /// Returns a task that completes with the result of <paramref name="task"/> or throws
        /// <see cref="TimeoutException"/> after <paramref name="timeout"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to wait for.</param>
        /// <param name="timeout">The longest time to wait.</param>
        /// <param name="delayType">The clock that measures <paramref name="timeout"/>.</param>
        /// <param name="timing">The player-loop phase at which the timeout is checked.</param>
        /// <param name="taskCancellationTokenSource">
        /// Cancelled and disposed when the timeout elapses, to stop <paramref name="task"/>; <c>null</c> for none.
        /// </param>
        /// <returns>A task that completes with the result of <paramref name="task"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as the non-generic <c>TimeoutAsync</c>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.Timeout</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="TimeoutException"><paramref name="timeout"/> elapsed first.</exception>
        public static async UnityTask<T> TimeoutAsync<T>(
              this UnityTask<T> task
            , TimeSpan timeout
            , UnityTaskDelayType delayType = UnityTaskDelayType.DeltaTime
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , CancellationTokenSource taskCancellationTokenSource = null
        )
        {
            var (isTimeout, outcome) = await RaceTimeoutAsync(
                  task.SuppressCancellationThrowAsync()
                , timeout
                , delayType
                , timing
                , taskCancellationTokenSource
            );

            if (isTimeout)
            {
                ThrowHelper.ThrowTimeout(timeout);
            }

            if (outcome.isCanceled)
            {
                ThrowHelper.ThrowOperationCanceled(CancellationToken.None);
            }

            return outcome.result;
        }

        /// <summary>
        /// Returns a task that completes with <c>true</c> when <paramref name="timeout"/> elapses before
        /// <paramref name="task"/> completes.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        /// <param name="timeout">The longest time to wait.</param>
        /// <param name="delayType">The clock that measures <paramref name="timeout"/>.</param>
        /// <param name="timing">The player-loop phase at which the timeout is checked.</param>
        /// <param name="taskCancellationTokenSource">
        /// Cancelled and disposed when the timeout elapses, to stop <paramref name="task"/>; <c>null</c> for none.
        /// </param>
        /// <returns><c>true</c> on timeout or when <paramref name="task"/> was cancelled; otherwise
        /// <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> a fault of <paramref name="task"/> also produces <c>true</c>, as UniTask does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.TimeoutWithoutException</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask<bool> TimeoutWithoutExceptionAsync(
              this UnityTask task
            , TimeSpan timeout
            , UnityTaskDelayType delayType = UnityTaskDelayType.DeltaTime
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , CancellationTokenSource taskCancellationTokenSource = null
        )
        {
            try
            {
                var (isTimeout, isCanceled) = await RaceTimeoutAsync(
                      task.SuppressCancellationThrowAsync()
                    , timeout
                    , delayType
                    , timing
                    , taskCancellationTokenSource
                );

                return isTimeout || isCanceled;
            }
            catch (Exception exception) when (exception is OperationCanceledException == false)
            {
                return true;
            }
        }

        /// <summary>
        /// Returns a task that completes with <c>(true, default)</c> when <paramref name="timeout"/> elapses before
        /// <paramref name="task"/> completes; otherwise with <c>(false, result)</c>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to wait for.</param>
        /// <param name="timeout">The longest time to wait.</param>
        /// <param name="delayType">The clock that measures <paramref name="timeout"/>.</param>
        /// <param name="timing">The player-loop phase at which the timeout is checked.</param>
        /// <param name="taskCancellationTokenSource">
        /// Cancelled and disposed when the timeout elapses, to stop <paramref name="task"/>; <c>null</c> for none.
        /// </param>
        /// <returns>The timeout flag and the result of <paramref name="task"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> a cancellation or fault of <paramref name="task"/> also produces
        /// <c>(true, default)</c>, as UniTask does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.TimeoutWithoutException</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static async UnityTask<(bool isTimeout, T result)> TimeoutWithoutExceptionAsync<T>(
              this UnityTask<T> task
            , TimeSpan timeout
            , UnityTaskDelayType delayType = UnityTaskDelayType.DeltaTime
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , CancellationTokenSource taskCancellationTokenSource = null
        )
        {
            try
            {
                var (isTimeout, outcome) = await RaceTimeoutAsync(
                      task.SuppressCancellationThrowAsync()
                    , timeout
                    , delayType
                    , timing
                    , taskCancellationTokenSource
                );

                if (isTimeout || outcome.isCanceled)
                {
                    return (true, default);
                }

                return (false, outcome.result);
            }
            catch (Exception exception) when (exception is OperationCanceledException == false)
            {
                return (true, default);
            }
        }

        /// <summary>
        /// Completes when <paramref name="task"/> completes, or throws when <paramref name="token"/> is cancelled
        /// first.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        /// <param name="token">The token that abandons the wait.</param>
        /// <returns>A task that follows <paramref name="task"/> unless the token wins.</returns>
        /// <remarks>
        /// Races <paramref name="task"/> against a source that the token registration completes. If the source wins,
        /// the method throws <see cref="OperationCanceledException"/> for <paramref name="token"/>. It does not cancel
        /// <paramref name="task"/>, which keeps running. The registration is disposed when the race ends.
        /// </remarks>
        private static async UnityTask AttachExternalCancellationCoreAsync(UnityTask task, CancellationToken token)
        {
            var cancellation = new UnityTaskCompletionSource();

            using (token.Register(static state => ((UnityTaskCompletionSource)state).TrySetResult(), cancellation))
            {
                var winArgumentIndex = await UnityTask.WhenAny(task, cancellation.Task);

                if (winArgumentIndex != 0)
                {
                    ThrowHelper.ThrowOperationCanceled(token);
                }
            }
        }

        /// <summary>
        /// Completes with the result of <paramref name="task"/>, or throws when <paramref name="token"/> is cancelled
        /// first.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to wait for.</param>
        /// <param name="token">The token that abandons the wait.</param>
        /// <returns>A task that follows <paramref name="task"/> unless the token wins.</returns>
        /// <remarks>
        /// Works as the overload for <see cref="UnityTask"/> does: <paramref name="task"/> is not cancelled when the
        /// token wins.
        /// </remarks>
        private static async UnityTask<T> AttachExternalCancellationCoreAsync<T>(
              UnityTask<T> task
            , CancellationToken token
        )
        {
            var cancellation = new UnityTaskCompletionSource();

            using (token.Register(static state => ((UnityTaskCompletionSource)state).TrySetResult(), cancellation))
            {
                var (hasResultLeft, result) = await UnityTask.WhenAny(task, cancellation.Task);

                if (hasResultLeft == false)
                {
                    ThrowHelper.ThrowOperationCanceled(token);
                }

                return result;
            }
        }

        /// <summary>
        /// Races <paramref name="task"/> against a delay of <paramref name="timeout"/> and reports which one finished
        /// first.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="task">The task to wait for; the caller passes it with cancellation suppressed.</param>
        /// <param name="timeout">The time after which the delay wins.</param>
        /// <param name="delayType">The clock that measures <paramref name="timeout"/>.</param>
        /// <param name="timing">The player-loop phase at which the delay is checked.</param>
        /// <param name="taskCancellationTokenSource">The source to cancel and dispose when the delay wins; may be
        /// <c>null</c>.</param>
        /// <returns>
        /// <c>isTimeout</c> is <c>true</c> when the delay finished first, with a default result; otherwise <c>false</c>
        /// with the result of <paramref name="task"/>.
        /// </returns>
        /// <remarks>
        /// The delay has its own cancellation source. Whichever side loses, or if the race throws, the delay is
        /// cancelled and its source is disposed, so no timer outlives the race. When the delay wins,
        /// <paramref name="taskCancellationTokenSource"/> is cancelled and disposed to ask the task to stop.
        /// </remarks>
        private static async UnityTask<(bool isTimeout, TResult result)> RaceTimeoutAsync<TResult>(
              UnityTask<TResult> task
            , TimeSpan timeout
            , UnityTaskDelayType delayType
            , UnityTaskTiming timing
            , CancellationTokenSource taskCancellationTokenSource
        )
        {
            var delayCancellation = new CancellationTokenSource();
            var delay = UnityTask.Delay(timeout, delayType, timing, token: delayCancellation.Token);

            int winArgumentIndex;
            TResult result;

            try
            {
                (winArgumentIndex, result, _) = await UnityTask.WhenAny(task, delay.SuppressCancellationThrowAsync());
            }
            catch
            {
                delayCancellation.Cancel();
                delayCancellation.Dispose();
                throw;
            }

            if (winArgumentIndex == 1)
            {
                delayCancellation.Dispose();

                if (taskCancellationTokenSource != null)
                {
                    taskCancellationTokenSource.Cancel();
                    taskCancellationTokenSource.Dispose();
                }

                return (true, default);
            }

            delayCancellation.Cancel();
            delayCancellation.Dispose();
            return (false, result);
        }
    }
}
