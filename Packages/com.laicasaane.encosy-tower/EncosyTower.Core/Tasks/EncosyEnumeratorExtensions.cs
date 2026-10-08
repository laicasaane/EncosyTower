using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Runs coroutine enumerators as <see cref="UnityTask"/>.
    /// </summary>
    public static class EncosyEnumeratorExtensions
    {
        // Reads the private seconds field of WaitForSeconds; GetSeconds returns 0 when the field is not found.
        private static readonly FieldInfo s_waitForSecondsField = typeof(WaitForSeconds).GetField(
              "m_Seconds"
            , BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        /// <summary>
        /// Returns a task that runs <paramref name="enumerator"/> on the shared Encosy player-loop scheduler.
        /// </summary>
        /// <param name="enumerator">The coroutine to run.</param>
        /// <param name="timing">The player-loop phase at which the coroutine advances.</param>
        /// <param name="token">The token that stops the coroutine between steps.</param>
        /// <returns>A task that completes when <paramref name="enumerator"/> ends.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> runs without a <see cref="MonoBehaviour"/>. Nested enumerators (including
        /// <see cref="CustomYieldInstruction"/>) run in place; an <see cref="AsyncOperation"/> is awaited;
        /// <see cref="WaitForSeconds"/> waits its seconds; <see cref="WaitForFixedUpdate"/> waits for
        /// <see cref="UnityTaskTiming.LastFixedUpdate"/>; <see cref="WaitForEndOfFrame"/> waits for
        /// <see cref="UnityTaskTiming.LastPostLateUpdate"/>; anything else waits one <paramref name="timing"/> tick. An
        /// exception thrown by the coroutine faults the task.
        /// </para>
        /// <para>
        /// <b>Thread:</b> call it on the main thread.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>EnumeratorAsyncExtensions.ToUniTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="enumerator"/> is <c>null</c>.</exception>
        public static UnityTask ToUnityTask(
              this IEnumerator enumerator
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(enumerator);
            return RunAsync(enumerator, timing, token);
        }

        /// <summary>
        /// Returns a task that runs <paramref name="enumerator"/> until it ends or <paramref name="token"/> is
        /// cancelled.
        /// </summary>
        /// <param name="enumerator">The coroutine to run.</param>
        /// <param name="token">The token that stops the coroutine between steps.</param>
        /// <returns>A task that completes when <paramref name="enumerator"/> ends.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as <see cref="ToUnityTask(IEnumerator, UnityTaskTiming, CancellationToken)"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>EnumeratorAsyncExtensions.WithCancellation</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask WithCancellation(this IEnumerator enumerator, CancellationToken token)
            => ToUnityTask(enumerator, UnityTaskTiming.Update, token);

        /// <summary>
        /// Returns a task that runs <paramref name="enumerator"/> as a coroutine of
        /// <paramref name="coroutineRunner"/>.
        /// </summary>
        /// <param name="enumerator">The coroutine to run.</param>
        /// <param name="coroutineRunner">The behaviour that runs the coroutine.</param>
        /// <returns>A task that completes when the coroutine ends.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> uses Unity's coroutine rules. An exception in the coroutine is logged by Unity and the
        /// task never completes; the task also never completes when <paramref name="coroutineRunner"/> stops it.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>EnumeratorAsyncExtensions.ToUniTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="enumerator"/> is <c>null</c>.</exception>
        public static UnityTask ToUnityTask(this IEnumerator enumerator, MonoBehaviour coroutineRunner)
        {
            Debugging.ThrowHelper.ThrowIfNull(enumerator);

            var source = new UnityTaskCompletionSource();

            coroutineRunner.AssumeValid().StartCoroutine(RunCoroutine(enumerator, source));
            return source.Task;
        }

        /// <summary>
        /// Runs <paramref name="enumerator"/> as a Unity coroutine and completes <paramref name="source"/> when it
        /// ends.
        /// </summary>
        /// <param name="enumerator">The enumerator that the coroutine runner yields.</param>
        /// <param name="source">The source to complete.</param>
        /// <returns>The enumerator that is passed to <c>StartCoroutine</c>.</returns>
        private static IEnumerator RunCoroutine(IEnumerator enumerator, UnityTaskCompletionSource source)
        {
            yield return enumerator;
            source.TrySetResult();
        }

        /// <summary>
        /// Steps <paramref name="enumerator"/> on the player loop, waiting for each yielded object before the next
        /// step.
        /// </summary>
        /// <param name="enumerator">The enumerator to run.</param>
        /// <param name="timing">The player-loop phase at which the waits resume.</param>
        /// <param name="token">The token that cancels the run.</param>
        /// <returns>A task that completes when the enumerator has no more steps.</returns>
        /// <remarks>
        /// The first <c>MoveNext</c> runs synchronously in the call. The token is checked before it and after each
        /// wait.
        /// </remarks>
        private static async UnityTask RunAsync(IEnumerator enumerator, UnityTaskTiming timing, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            while (enumerator.MoveNext())
            {
                await WaitForCurrentAsync(enumerator.Current, timing, token);
                token.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Waits for one object yielded by an enumerator, as the Unity coroutine scheduler would.
        /// </summary>
        /// <param name="current">The object yielded by <c>IEnumerator.Current</c>.</param>
        /// <param name="timing">The player-loop phase at which the wait resumes.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when the yielded object is done.</returns>
        /// <remarks>
        /// <para>
        /// A nested <see cref="IEnumerator"/> is run to the end, an <c>AsyncOperation</c> is awaited, and a
        /// <c>WaitForSeconds</c> becomes a delay of its seconds. <c>WaitForFixedUpdate</c> resumes at
        /// <see cref="UnityTaskTiming.LastFixedUpdate"/> and <c>WaitForEndOfFrame</c> at
        /// <see cref="UnityTaskTiming.LastPostLateUpdate"/>.
        /// </para>
        /// <para>
        /// Any other object, including <c>null</c>, waits one pass at <paramref name="timing"/>.
        /// </para>
        /// </remarks>
        private static async UnityTask WaitForCurrentAsync(
              object current
            , UnityTaskTiming timing
            , CancellationToken token
        )
        {
            switch (current)
            {
                case IEnumerator nested:
                {
                    await RunAsync(nested, timing, token);
                    break;
                }

                case AsyncOperation operation:
                {
                    await operation.ToUnityTask(timing: timing, token: token);
                    break;
                }

                case WaitForSeconds waitForSeconds:
                {
                    var delay = TimeSpan.FromSeconds(GetSeconds(waitForSeconds));
                    await UnityTask.Delay(delay, timing: timing, token: token);
                    break;
                }

                case WaitForFixedUpdate:
                {
                    await UnityTask.Yield(UnityTaskTiming.LastFixedUpdate, token: token);
                    break;
                }

                case WaitForEndOfFrame:
                {
                    await UnityTask.Yield(UnityTaskTiming.LastPostLateUpdate, token: token);
                    break;
                }

                default:
                {
                    await UnityTask.Yield(timing, token: token);
                    break;
                }
            }
        }

        /// <summary>
        /// Reads the seconds that <paramref name="waitForSeconds"/> was created with.
        /// </summary>
        /// <param name="waitForSeconds">The yield instruction to read.</param>
        /// <returns>The seconds, or <c>0</c> when the private field cannot be read.</returns>
        private static float GetSeconds(WaitForSeconds waitForSeconds)
            => s_waitForSecondsField?.GetValue(waitForSeconds) is float seconds ? seconds : 0f;
    }
}
