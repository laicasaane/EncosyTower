using System;
using System.Threading;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Cancellation helpers that match UniTask's <c>CancellationToken</c> and <c>CancellationTokenSource</c>
    /// extensions, as static methods so that they never clash with UniTask in the same file.
    /// </summary>
    public static class UnityTaskCancellation
    {
        /// <summary>
        /// Cancels <paramref name="source"/> after <paramref name="millisecondsDelay"/> milliseconds measured on the
        /// player loop.
        /// </summary>
        /// <param name="source">The source to cancel.</param>
        /// <param name="millisecondsDelay">The delay in milliseconds. Must not be negative.</param>
        /// <param name="delayType">The clock that measures the delay.</param>
        /// <param name="timing">The player-loop phase at which the delay is checked.</param>
        /// <returns>A handle; disposing it stops the timer without cancelling <paramref name="source"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenSourceExtensions.CancelAfterSlim</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public static IDisposable CancelAfterSlim(
              CancellationTokenSource source
            , int millisecondsDelay
            , UnityTaskDelayType delayType = UnityTaskDelayType.DeltaTime
            , UnityTaskTiming timing = UnityTaskTiming.Update
        )
            => CancelAfterSlim(source, TimeSpan.FromMilliseconds(millisecondsDelay), delayType, timing);

        /// <summary>
        /// Cancels <paramref name="source"/> after <paramref name="delay"/> measured on the player loop.
        /// </summary>
        /// <param name="source">The source to cancel.</param>
        /// <param name="delay">The delay. Must not be negative.</param>
        /// <param name="delayType">The clock that measures the delay.</param>
        /// <param name="timing">The player-loop phase at which the delay is checked.</param>
        /// <returns>A handle; disposing it stops the timer without cancelling <paramref name="source"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenSourceExtensions.CancelAfterSlim</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public static IDisposable CancelAfterSlim(
              CancellationTokenSource source
            , TimeSpan delay
            , UnityTaskDelayType delayType = UnityTaskDelayType.DeltaTime
            , UnityTaskTiming timing = UnityTaskTiming.Update
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(source);

            return PlayerLoopTimer.StartNew(
                  interval: delay
                , periodic: false
                , delayType: delayType
                , timing: timing
                , token: source.Token
                , timerCallback: static state => ((CancellationTokenSource)state).Cancel()
                , state: source
            );
        }

        /// <summary>
        /// Cancels <paramref name="source"/> when <paramref name="component"/>'s game object is destroyed.
        /// </summary>
        /// <param name="source">The source to cancel.</param>
        /// <param name="component">The component whose game object is observed.</param>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> adds a hidden component to the game object on first use.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenSourceExtensions.RegisterRaiseCancelOnDestroy</c>; Unity:
        /// none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public static void RegisterRaiseCancelOnDestroy(CancellationTokenSource source, Component component)
            => RegisterRaiseCancelOnDestroy(source, component.AssumeValid().gameObject);

        /// <summary>
        /// Cancels <paramref name="source"/> when <paramref name="gameObject"/> is destroyed.
        /// </summary>
        /// <param name="source">The source to cancel.</param>
        /// <param name="gameObject">The game object to observe.</param>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> adds a hidden component to <paramref name="gameObject"/> on first use.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenSourceExtensions.RegisterRaiseCancelOnDestroy</c>; Unity:
        /// none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public static void RegisterRaiseCancelOnDestroy(CancellationTokenSource source, GameObject gameObject)
        {
            Debugging.ThrowHelper.ThrowIfNull(source);
            UnityTaskDestroyTrigger.Register(gameObject.AssumeValid(), source);
        }

        /// <summary>
        /// Registers <paramref name="callback"/> on <paramref name="token"/> without capturing the current
        /// <see cref="ExecutionContext"/>.
        /// </summary>
        /// <param name="token">The token to observe.</param>
        /// <param name="callback">The action to run on cancellation.</param>
        /// <returns>The registration.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenExtensions.RegisterWithoutCaptureExecutionContext</c>;
        /// Unity: none.
        /// </para>
        /// </remarks>
        public static CancellationTokenRegistration RegisterWithoutCaptureExecutionContext(
              CancellationToken token
            , Action callback
        )
        {
            // SuppressFlow throws when the flow is already suppressed, so it is called only when the flow is not.
            if (ExecutionContext.IsFlowSuppressed())
            {
                return token.Register(callback, useSynchronizationContext: false);
            }

            using (ExecutionContext.SuppressFlow())
            {
                return token.Register(callback, useSynchronizationContext: false);
            }
        }

        /// <summary>
        /// Registers <paramref name="callback"/> with <paramref name="state"/> on <paramref name="token"/> without
        /// capturing the current <see cref="ExecutionContext"/>.
        /// </summary>
        /// <param name="token">The token to observe.</param>
        /// <param name="callback">The action to run on cancellation.</param>
        /// <param name="state">The state passed to <paramref name="callback"/>.</param>
        /// <returns>The registration.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenExtensions.RegisterWithoutCaptureExecutionContext</c>;
        /// Unity: none.
        /// </para>
        /// </remarks>
        public static CancellationTokenRegistration RegisterWithoutCaptureExecutionContext(
              CancellationToken token
            , Action<object> callback
            , object state
        )
        {
            // SuppressFlow throws when the flow is already suppressed, so it is called only when the flow is not.
            if (ExecutionContext.IsFlowSuppressed())
            {
                return token.Register(callback, state, useSynchronizationContext: false);
            }

            using (ExecutionContext.SuppressFlow())
            {
                return token.Register(callback, state, useSynchronizationContext: false);
            }
        }

        /// <summary>
        /// Disposes <paramref name="disposable"/> when <paramref name="token"/> is cancelled.
        /// </summary>
        /// <param name="disposable">The object to dispose.</param>
        /// <param name="token">The token to observe.</param>
        /// <returns>The registration.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>CancellationTokenExtensions.AddTo</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="disposable"/> is <c>null</c>.</exception>
        public static CancellationTokenRegistration AddTo(IDisposable disposable, CancellationToken token)
        {
            Debugging.ThrowHelper.ThrowIfNull(disposable);

            return RegisterWithoutCaptureExecutionContext(
                  token
                , static state => ((IDisposable)state).Dispose()
                , disposable
            );
        }

        /// <summary>
        /// Returns whether <paramref name="exception"/> is an <see cref="OperationCanceledException"/>.
        /// </summary>
        /// <param name="exception">The exception to test.</param>
        /// <returns><c>true</c> for an <see cref="OperationCanceledException"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>ExceptionExtensions.IsOperationCanceledException</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static bool IsOperationCanceledException(Exception exception)
            => exception is OperationCanceledException;
    }
}
