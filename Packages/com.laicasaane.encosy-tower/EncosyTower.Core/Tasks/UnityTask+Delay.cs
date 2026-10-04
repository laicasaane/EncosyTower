using System;
using System.Threading;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Returns a task that completes at the next run of the <paramref name="timing"/> player-loop phase.
        /// </summary>
        /// <param name="timing">The player-loop phase at which the task completes.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that never completes synchronously unless <paramref name="token"/> is already
        /// cancelled.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> scheduled on the shared Encosy player-loop scheduler. A request made while that phase is
        /// running resumes in the next frame. In the Editor outside Play Mode, every phase is run from
        /// <c>EditorApplication.update</c>. When <paramref name="token"/> is already cancelled, the task completes
        /// cancelled when created. Cancellation throws <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity
        /// throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        public static UnityTask Yield(
              UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
            => UnityTaskDelayPromise.Create(
                  UnityTaskDelayPromise.DelayKind.Yield
                , TimeSpan.Zero
                , UnityTaskDelayType.DeltaTime
                , timing
                , cancelImmediately
                , token
            );

        /// <summary>
        /// Returns a task that completes after <paramref name="millisecondsDelay"/> milliseconds.
        /// </summary>
        /// <param name="millisecondsDelay">The delay in milliseconds. Must not be negative.</param>
        /// <param name="delayType">The clock that the delay measures.</param>
        /// <param name="timing">The player-loop phase at which the delay is checked.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the delay.</param>
        /// <returns>A task that completes when the delay has elapsed.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checked on each tick of the shared Encosy player-loop scheduler.
        /// <see cref="UnityTaskDelayType.DeltaTime"/> subtracts <c>Time.deltaTime</c> each tick,
        /// <see cref="UnityTaskDelayType.UnscaledDeltaTime"/> subtracts <c>Time.unscaledDeltaTime</c>, and
        /// <see cref="UnityTaskDelayType.Realtime"/> compares <c>Time.realtimeSinceStartupAsDouble</c>. In the Editor
        /// outside Play Mode, every phase is run from <c>EditorApplication.update</c> and delays use real time.
        /// When <paramref name="token"/> is already cancelled, the task completes cancelled when created.
        /// Cancellation throws <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// A negative <paramref name="millisecondsDelay"/>: development builds (<c>UNITY_EDITOR</c>, <c>DEBUG</c>,
        /// <c>ENCOSY_RUNTIME_CHECKS</c>) throw <see cref="ArgumentOutOfRangeException"/>. Release builds complete at
        /// the first scheduler tick.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity
        /// throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="millisecondsDelay"/> is negative (development builds).
        /// </exception>
        public static UnityTask Delay(
              int millisecondsDelay
            , UnityTaskDelayType delayType = UnityTaskDelayType.DeltaTime
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            ThrowHelper.ThrowIfDelayNegative(millisecondsDelay >= 0, nameof(millisecondsDelay));

            return UnityTaskDelayPromise.Create(
                  UnityTaskDelayPromise.DelayKind.Delay
                , TimeSpan.FromMilliseconds(millisecondsDelay)
                , delayType
                , timing
                , cancelImmediately
                , token
            );
        }

        /// <summary>
        /// Returns a task that completes after <paramref name="delay"/>.
        /// </summary>
        /// <param name="delay">The delay. Must not be negative.</param>
        /// <param name="delayType">The clock that the delay measures.</param>
        /// <param name="timing">The player-loop phase at which the delay is checked.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the delay.</param>
        /// <returns>A task that completes when the delay has elapsed.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checked on each tick of the shared Encosy player-loop scheduler.
        /// <see cref="UnityTaskDelayType.DeltaTime"/> subtracts <c>Time.deltaTime</c> each tick,
        /// <see cref="UnityTaskDelayType.UnscaledDeltaTime"/> subtracts <c>Time.unscaledDeltaTime</c>, and
        /// <see cref="UnityTaskDelayType.Realtime"/> compares <c>Time.realtimeSinceStartupAsDouble</c>. In the Editor
        /// outside Play Mode, every phase is run from <c>EditorApplication.update</c> and delays use real time.
        /// When <paramref name="token"/> is already cancelled, the task completes cancelled when created.
        /// Cancellation throws <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// A negative <paramref name="delay"/>: development builds (<c>UNITY_EDITOR</c>, <c>DEBUG</c>,
        /// <c>ENCOSY_RUNTIME_CHECKS</c>) throw <see cref="ArgumentOutOfRangeException"/>. Release builds complete at
        /// the first scheduler tick.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity
        /// throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="delay"/> is negative (development builds).
        /// </exception>
        public static UnityTask Delay(
              TimeSpan delay
            , UnityTaskDelayType delayType = UnityTaskDelayType.DeltaTime
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            ThrowHelper.ThrowIfDelayNegative(delay >= TimeSpan.Zero, nameof(delay));

            return UnityTaskDelayPromise.Create(
                  UnityTaskDelayPromise.DelayKind.Delay
                , delay
                , delayType
                , timing
                , cancelImmediately
                , token
            );
        }

        /// <summary>
        /// Returns a task that completes at <see cref="UnityTaskTiming.Update"/> of a later frame.
        /// </summary>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes in a later frame.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes when <c>Time.frameCount</c> exceeds the frame count captured when the task was
        /// created. In the Editor outside Play Mode, completes on the next <c>EditorApplication.update</c> tick. The
        /// scheduler checks <paramref name="token"/> on each tick; cancellation throws
        /// <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity
        /// throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        public static UnityTask NextFrameAsync(CancellationToken token = default)
            => UnityTaskDelayPromise.Create(
                  kind: UnityTaskDelayPromise.DelayKind.NextFrame
                , delay: TimeSpan.Zero
                , delayType: UnityTaskDelayType.DeltaTime
                , timing: UnityTaskTiming.Update
                , cancelImmediately: false
                , token: token
            );

        /// <summary>
        /// Returns a task that completes when <paramref name="predicate"/> returns <c>true</c>.
        /// </summary>
        /// <param name="predicate">The condition to wait for.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when <paramref name="predicate"/> returns <c>true</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, then evaluates <paramref name="predicate"/> immediately;
        /// when it returns <c>true</c>, the task completes synchronously. Otherwise the predicate is evaluated again
        /// after each <see cref="UnityTaskTiming.Update"/> tick. An exception thrown by the predicate faults the task
        /// with that instance. Cancellation throws <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity
        /// throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public static UnityTask WaitUntil(Func<bool> predicate, CancellationToken token = default)
        {
            Debugging.ThrowHelper.ThrowIfNull(predicate);
            return WaitUntilCoreAsync(predicate, token);
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="predicate"/> returns <c>true</c> for
        /// <paramref name="state"/>.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to <paramref name="predicate"/>.</typeparam>
        /// <param name="state">The state passed to <paramref name="predicate"/>.</param>
        /// <param name="predicate">The condition to wait for.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when <paramref name="predicate"/> returns <c>true</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, then evaluates <paramref name="predicate"/> immediately;
        /// when it returns <c>true</c>, the task completes synchronously. Otherwise the predicate is evaluated again
        /// after each <see cref="UnityTaskTiming.Update"/> tick. An exception thrown by the predicate faults the task
        /// with that instance. Cancellation throws <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity
        /// throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public static UnityTask WaitUntil<TState>(
              TState state
            , Func<TState, bool> predicate
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(predicate);
            return WaitUntilCoreAsync(state, predicate, token);
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="predicate"/> returns <c>false</c> for
        /// <paramref name="state"/>.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to <paramref name="predicate"/>.</typeparam>
        /// <param name="state">The state passed to <paramref name="predicate"/>.</param>
        /// <param name="predicate">The condition that keeps the wait going while it returns <c>true</c>.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when <paramref name="predicate"/> returns <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, then evaluates <paramref name="predicate"/> immediately;
        /// when it returns <c>false</c>, the task completes synchronously. Otherwise the predicate is evaluated again
        /// after each <see cref="UnityTaskTiming.Update"/> tick. An exception thrown by the predicate faults the task
        /// with that instance. Cancellation throws <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity
        /// throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public static UnityTask WaitWhile<TState>(
              TState state
            , Func<TState, bool> predicate
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(predicate);
            return WaitWhileCoreAsync(state, predicate, token);
        }

        private static async UnityTask WaitUntilCoreAsync(Func<bool> predicate, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            while (predicate() == false)
            {
                await Yield(timing: UnityTaskTiming.Update, token: token);
            }
        }

        private static async UnityTask WaitUntilCoreAsync<TState>(
              TState state
            , Func<TState, bool> predicate
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            while (predicate(state) == false)
            {
                await Yield(timing: UnityTaskTiming.Update, token: token);
            }
        }

        private static async UnityTask WaitWhileCoreAsync<TState>(
              TState state
            , Func<TState, bool> predicate
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            while (predicate(state))
            {
                await Yield(timing: UnityTaskTiming.Update, token: token);
            }
        }
    }
}
