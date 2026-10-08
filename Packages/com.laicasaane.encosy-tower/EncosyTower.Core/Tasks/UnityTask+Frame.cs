using System;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Returns a task that completes at the <paramref name="timing"/> player-loop phase of a later frame.
        /// </summary>
        /// <param name="timing">The player-loop phase at which the frame is checked.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes in a later frame.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes when <c>Time.frameCount</c> exceeds the frame count captured when the task was
        /// created. In the Editor outside Play Mode, completes on the next <c>EditorApplication.update</c> tick. When
        /// <paramref name="token"/> is already cancelled, the task completes cancelled when created. Cancellation
        /// throws <c>new OperationCanceledException(token)</c>.
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
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.NextFrame</c>; Unity:
        /// <see cref="UnityEngine.Awaitable.NextFrameAsync"/>, which resumes on the next frame with no player-loop
        /// phase choice.
        /// </para>
        /// </remarks>
        public static UnityTask NextFrameAsync(
              UnityTaskTiming timing
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
            => UnityTaskDelayPromise.Create(
                  kind: UnityTaskDelayPromise.DelayKind.NextFrame
                , delay: TimeSpan.Zero
                , delayType: UnityTaskDelayType.DeltaTime
                , timing: timing
                , cancelImmediately: cancelImmediately
                , token: token
            );

        /// <summary>
        /// Returns a task that completes after <paramref name="delayFrameCount"/> frames.
        /// </summary>
        /// <param name="delayFrameCount">The number of frames to wait. Must not be negative.</param>
        /// <param name="timing">The player-loop phase at which the frame count is checked.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when the frames have passed.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes when <c>Time.frameCount</c> has advanced by at least
        /// <paramref name="delayFrameCount"/> from the frame captured when the task was created. A count of
        /// <c>0</c> behaves as <see cref="Yield"/>. In the Editor outside Play Mode, each
        /// <c>EditorApplication.update</c> tick counts as one frame. Cancellation throws
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
        /// A negative <paramref name="delayFrameCount"/>: development builds (<c>UNITY_EDITOR</c>, <c>DEBUG</c>,
        /// <c>ENCOSY_RUNTIME_CHECKS</c>) throw <see cref="ArgumentOutOfRangeException"/>. Release builds complete at
        /// the first scheduler tick.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.DelayFrame</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="delayFrameCount"/> is negative (development builds).
        /// </exception>
        public static UnityTask DelayFrameAsync(
              int delayFrameCount
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            ThrowHelper.ThrowIfDelayNegative(delayFrameCount >= 0, nameof(delayFrameCount));

            if (delayFrameCount <= 0)
            {
                return Yield(timing, cancelImmediately, token);
            }

            return UnityTaskDelayPromise.Create(
                  kind: UnityTaskDelayPromise.DelayKind.DelayFrame
                , delay: TimeSpan.Zero
                , delayFrames: delayFrameCount
                , delayType: UnityTaskDelayType.DeltaTime
                , timing: timing
                , cancelImmediately: cancelImmediately
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes at the end of the current frame, after rendering.
        /// </summary>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes at the end of the frame.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> awaits <c>Awaitable.EndOfFrameAsync</c>, as UniTask does on Unity 2023.1 and later.
        /// Unity resumes end-of-frame waits only while the Player loop renders frames; in the Editor outside Play
        /// Mode the task does not complete until a frame is rendered. Cancellation throws an
        /// <see cref="OperationCanceledException"/>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise. Call it from the main thread.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitForEndOfFrame</c>; Unity:
        /// <see cref="UnityEngine.Awaitable.EndOfFrameAsync"/>, which resumes after all Unity subsystems have run for
        /// the current frame; this method awaits it on both backends.
        /// </para>
        /// </remarks>
        public static async UnityTask WaitForEndOfFrameAsync(CancellationToken token = default)
        {
            await Awaitable.EndOfFrameAsync(token);
        }

        /// <summary>
        /// Returns a task that completes after the fixed-update phase of the player loop.
        /// </summary>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes at <see cref="UnityTaskTiming.LastFixedUpdate"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> same as <c>Yield(UnityTaskTiming.LastFixedUpdate, cancelImmediately, token)</c>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitForFixedUpdate</c>; Unity:
        /// <see cref="UnityEngine.Awaitable.FixedUpdateAsync"/>, which resumes on the next fixed-update frame.
        /// </para>
        /// </remarks>
        public static UnityTask WaitForFixedUpdateAsync(
              bool cancelImmediately = false
            , CancellationToken token = default
        )
            => Yield(UnityTaskTiming.LastFixedUpdate, cancelImmediately, token);

        /// <summary>
        /// Returns a task that completes after <paramref name="duration"/> seconds.
        /// </summary>
        /// <param name="duration">The delay in seconds. Must not be negative.</param>
        /// <param name="ignoreTimeScale">
        /// <c>true</c> to measure <see cref="UnityTaskDelayType.UnscaledDeltaTime"/>; <c>false</c> to measure
        /// <see cref="UnityTaskDelayType.DeltaTime"/>.
        /// </param>
        /// <param name="timing">The player-loop phase at which the delay is checked.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the delay.</param>
        /// <returns>A task that completes when the delay has elapsed.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> same as
        /// <see cref="Delay(TimeSpan, UnityTaskDelayType, UnityTaskTiming, bool, CancellationToken)"/> with
        /// <c>TimeSpan.FromSeconds(duration)</c>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitForSeconds</c>; Unity:
        /// <see cref="UnityEngine.Awaitable.WaitForSecondsAsync"/>, which waits scaled seconds only.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="duration"/> is negative (development builds).
        /// </exception>
        public static UnityTask WaitForSecondsAsync(
              float duration
            , bool ignoreTimeScale = false
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            ThrowHelper.ThrowIfDelayNegative(duration >= 0f, nameof(duration));

            return Delay(
                  TimeSpan.FromSeconds(duration)
                , ignoreTimeScale ? UnityTaskDelayType.UnscaledDeltaTime : UnityTaskDelayType.DeltaTime
                , timing
                , cancelImmediately
                , token
            );
        }

        /// <summary>
        /// Returns a task that completes after <paramref name="duration"/> seconds.
        /// </summary>
        /// <param name="duration">The delay in seconds. Must not be negative.</param>
        /// <param name="ignoreTimeScale">
        /// <c>true</c> to measure <see cref="UnityTaskDelayType.UnscaledDeltaTime"/>; <c>false</c> to measure
        /// <see cref="UnityTaskDelayType.DeltaTime"/>.
        /// </param>
        /// <param name="timing">The player-loop phase at which the delay is checked.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the delay.</param>
        /// <returns>A task that completes when the delay has elapsed.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> same as
        /// <see cref="Delay(TimeSpan, UnityTaskDelayType, UnityTaskTiming, bool, CancellationToken)"/> with
        /// <c>TimeSpan.FromSeconds(duration)</c>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitForSeconds</c>; Unity:
        /// <see cref="UnityEngine.Awaitable.WaitForSecondsAsync"/>, which waits scaled seconds only.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="duration"/> is negative (development builds).
        /// </exception>
        public static UnityTask WaitForSecondsAsync(
              int duration
            , bool ignoreTimeScale = false
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            ThrowHelper.ThrowIfDelayNegative(duration >= 0, nameof(duration));

            return Delay(
                  TimeSpan.FromSeconds(duration)
                , ignoreTimeScale ? UnityTaskDelayType.UnscaledDeltaTime : UnityTaskDelayType.DeltaTime
                , timing
                , cancelImmediately
                , token
            );
        }
    }
}
