using System;
using System.Collections.Generic;
using System.Threading;
using EncosyTower.UnityExtensions;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Returns a task that completes when <paramref name="predicate"/> returns <c>true</c>, checking it at
        /// <paramref name="timing"/>.
        /// </summary>
        /// <param name="predicate">The condition to wait for.</param>
        /// <param name="timing">The player-loop phase after which the predicate is evaluated again.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when <paramref name="predicate"/> returns <c>true</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, then evaluates <paramref name="predicate"/> immediately;
        /// when it returns <c>true</c>, the task completes synchronously. Otherwise the predicate is evaluated again
        /// after each <paramref name="timing"/> tick. An exception thrown by the predicate faults the task with that
        /// instance. Cancellation throws <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitUntil</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public static UnityTask WaitUntil(
              Func<bool> predicate
            , UnityTaskTiming timing
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(predicate);

            return WaitConditionCoreAsync(
                  predicate: predicate
                , expected: true
                , timing: timing
                , cancelImmediately: cancelImmediately
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="predicate"/> returns <c>true</c> for
        /// <paramref name="state"/>, checking it at <paramref name="timing"/>.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to <paramref name="predicate"/>.</typeparam>
        /// <param name="state">The state passed to <paramref name="predicate"/>.</param>
        /// <param name="predicate">The condition to wait for.</param>
        /// <param name="timing">The player-loop phase after which the predicate is evaluated again.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when <paramref name="predicate"/> returns <c>true</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as
        /// <see cref="WaitUntil(Func{bool}, UnityTaskTiming, bool, CancellationToken)"/>, with
        /// <paramref name="state"/> passed to every predicate call.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitUntil</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public static UnityTask WaitUntil<TState>(
              TState state
            , Func<TState, bool> predicate
            , UnityTaskTiming timing
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(predicate);

            return WaitConditionCoreAsync(
                  state: state
                , predicate: predicate
                , expected: true
                , timing: timing
                , cancelImmediately: cancelImmediately
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="predicate"/> returns <c>false</c>.
        /// </summary>
        /// <param name="predicate">The condition that keeps the wait going while it returns <c>true</c>.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when <paramref name="predicate"/> returns <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as
        /// <see cref="WaitWhile(Func{bool}, UnityTaskTiming, bool, CancellationToken)"/> with
        /// <see cref="UnityTaskTiming.Update"/>.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitWhile</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public static UnityTask WaitWhile(Func<bool> predicate, CancellationToken token = default)
        {
            Debugging.ThrowHelper.ThrowIfNull(predicate);

            return WaitConditionCoreAsync(
                  predicate: predicate
                , expected: false
                , timing: UnityTaskTiming.Update
                , cancelImmediately: false
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="predicate"/> returns <c>false</c>, checking it at
        /// <paramref name="timing"/>.
        /// </summary>
        /// <param name="predicate">The condition that keeps the wait going while it returns <c>true</c>.</param>
        /// <param name="timing">The player-loop phase after which the predicate is evaluated again.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when <paramref name="predicate"/> returns <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, then evaluates <paramref name="predicate"/> immediately;
        /// when it returns <c>false</c>, the task completes synchronously. Otherwise the predicate is evaluated again
        /// after each <paramref name="timing"/> tick. An exception thrown by the predicate faults the task with that
        /// instance. Cancellation throws <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitWhile</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public static UnityTask WaitWhile(
              Func<bool> predicate
            , UnityTaskTiming timing
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(predicate);

            return WaitConditionCoreAsync(
                  predicate: predicate
                , expected: false
                , timing: timing
                , cancelImmediately: cancelImmediately
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes when <paramref name="predicate"/> returns <c>false</c> for
        /// <paramref name="state"/>, checking it at <paramref name="timing"/>.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to <paramref name="predicate"/>.</typeparam>
        /// <param name="state">The state passed to <paramref name="predicate"/>.</param>
        /// <param name="predicate">The condition that keeps the wait going while it returns <c>true</c>.</param>
        /// <param name="timing">The player-loop phase after which the predicate is evaluated again.</param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when <paramref name="predicate"/> returns <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> as
        /// <see cref="WaitWhile(Func{bool}, UnityTaskTiming, bool, CancellationToken)"/>, with
        /// <paramref name="state"/> passed to every predicate call.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitWhile</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public static UnityTask WaitWhile<TState>(
              TState state
            , Func<TState, bool> predicate
            , UnityTaskTiming timing
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(predicate);

            return WaitConditionCoreAsync(
                  state: state
                , predicate: predicate
                , expected: false
                , timing: timing
                , cancelImmediately: cancelImmediately
                , token: token
            );
        }

        /// <summary>
        /// Returns a task that completes successfully when <paramref name="token"/> is cancelled.
        /// </summary>
        /// <param name="token">The token to observe.</param>
        /// <param name="timing">The player-loop phase at which the token is polled.</param>
        /// <param name="completeImmediately">
        /// <c>true</c> to complete the task from the token's cancellation callback; <c>false</c> to poll the token at
        /// <paramref name="timing"/>.
        /// </param>
        /// <returns>A task that completes when <paramref name="token"/> is cancelled. It never throws
        /// <see cref="OperationCanceledException"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes synchronously when <paramref name="token"/> is already cancelled. A token that
        /// can never be cancelled produces a task that never completes.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitUntilCanceled</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask WaitUntilCanceledAsync(
              CancellationToken token
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , bool completeImmediately = false
        )
        {
            if (token.IsCancellationRequested)
            {
                return CompletedTask;
            }

            if (completeImmediately)
            {
                return WaitUntilCanceledByCallbackAsync(token);
            }

            return WaitUntilCanceledByPollingAsync(token, timing);
        }

        /// <summary>
        /// Returns a task that completes with the new value when <paramref name="monitorFunction"/> returns a value
        /// different from its first result.
        /// </summary>
        /// <typeparam name="T">The type of the monitored object.</typeparam>
        /// <typeparam name="TValue">The type of the monitored value.</typeparam>
        /// <param name="target">The object passed to <paramref name="monitorFunction"/>.</param>
        /// <param name="monitorFunction">Reads the value to monitor from <paramref name="target"/>.</param>
        /// <param name="timing">The player-loop phase at which the value is read again.</param>
        /// <param name="equalityComparer">
        /// The comparer for the values, or <c>null</c> for <see cref="EqualityComparer{T}.Default"/>.
        /// </param>
        /// <param name="cancelImmediately">
        /// <c>true</c> to complete the task as soon as <paramref name="token"/> is cancelled; <c>false</c> to check
        /// the token when the scheduler runs.
        /// </param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes with the first value that differs from the initial value.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> reads the initial value when called, then reads again after each
        /// <paramref name="timing"/> tick. When <paramref name="target"/> is a <see cref="UnityEngine.Object"/> that
        /// is destroyed, the task completes with <c>new OperationCanceledException(token)</c>. An exception thrown by
        /// <paramref name="monitorFunction"/> faults the task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.WaitUntilValueChanged</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="target"/> or <paramref name="monitorFunction"/> is <c>null</c>.
        /// </exception>
        public static UnityTask<TValue> WaitUntilValueChangedAsync<T, TValue>(
              T target
            , Func<T, TValue> monitorFunction
            , UnityTaskTiming timing = UnityTaskTiming.Update
            , IEqualityComparer<TValue> equalityComparer = null
            , bool cancelImmediately = false
            , CancellationToken token = default
        )
            where T : class
        {
            Debugging.ThrowHelper.ThrowIfNull(target);
            Debugging.ThrowHelper.ThrowIfNull(monitorFunction);

            return WaitUntilValueChangedCoreAsync(
                  target
                , monitorFunction
                , timing
                , equalityComparer ?? EqualityComparer<TValue>.Default
                , cancelImmediately
                , token
            );
        }

        /// <summary>
        /// Polls <paramref name="predicate"/> once per player-loop pass at <paramref name="timing"/> until it returns
        /// <paramref name="expected"/>.
        /// </summary>
        /// <param name="predicate">The condition to poll.</param>
        /// <param name="expected">The value of <paramref name="predicate"/> that ends the wait.</param>
        /// <param name="timing">The player-loop phase at which the condition is checked.</param>
        /// <param name="cancelImmediately"><c>true</c> to cancel as soon as <paramref name="token"/> is cancelled;
        /// <c>false</c> to observe it on the next pass.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when the condition has the expected value.</returns>
        /// <remarks>
        /// The token is checked first and the condition is read before the first yield, so a condition that already
        /// holds completes without a suspension. The <c>WaitUntil</c> overloads pass <c>true</c> for
        /// <paramref name="expected"/> and the <c>WaitWhile</c> overloads pass <c>false</c>.
        /// </remarks>
        private static async UnityTask WaitConditionCoreAsync(
              Func<bool> predicate
            , bool expected
            , UnityTaskTiming timing
            , bool cancelImmediately
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            while (predicate() != expected)
            {
                await Yield(timing, cancelImmediately, token);
            }
        }

        /// <summary>
        /// Polls <paramref name="predicate"/> with <paramref name="state"/> once per player-loop pass at
        /// <paramref name="timing"/> until it returns <paramref name="expected"/>.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the predicate.</typeparam>
        /// <param name="state">The state passed to <paramref name="predicate"/> on each poll.</param>
        /// <param name="predicate">The condition to poll.</param>
        /// <param name="expected">The value of <paramref name="predicate"/> that ends the wait.</param>
        /// <param name="timing">The player-loop phase at which the condition is checked.</param>
        /// <param name="cancelImmediately"><c>true</c> to cancel as soon as <paramref name="token"/> is cancelled;
        /// <c>false</c> to observe it on the next pass.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that completes when the condition has the expected value.</returns>
        /// <remarks>
        /// Behaves like the overload without state.
        /// </remarks>
        private static async UnityTask WaitConditionCoreAsync<TState>(
              TState state
            , Func<TState, bool> predicate
            , bool expected
            , UnityTaskTiming timing
            , bool cancelImmediately
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            while (predicate(state) != expected)
            {
                await Yield(timing, cancelImmediately, token);
            }
        }

        /// <summary>
        /// Completes when <paramref name="token"/> is cancelled, by registering a callback that completes a source.
        /// </summary>
        /// <param name="token">The token to wait on.</param>
        /// <returns>A task that succeeds when the token is cancelled.</returns>
        /// <remarks>
        /// The callback runs on the thread that cancels the token. The source captured the thread kind of the caller,
        /// so the task resumes on that kind of thread. The registration is disposed when the wait ends.
        /// </remarks>
        private static async UnityTask WaitUntilCanceledByCallbackAsync(CancellationToken token)
        {
            var source = new UnityTaskCompletionSource();

            using (token.Register(static state => ((UnityTaskCompletionSource)state).TrySetResult(), source))
            {
                await source.Task;
            }
        }

        /// <summary>
        /// Completes when <paramref name="token"/> is cancelled, by checking it once per pass at
        /// <paramref name="timing"/>.
        /// </summary>
        /// <param name="token">The token to wait on.</param>
        /// <param name="timing">The player-loop phase at which the token is checked.</param>
        /// <returns>A task that succeeds on the first pass after the token is cancelled.</returns>
        private static async UnityTask WaitUntilCanceledByPollingAsync(CancellationToken token, UnityTaskTiming timing)
        {
            while (token.IsCancellationRequested == false)
            {
                await Yield(timing);
            }
        }

        /// <summary>
        /// Polls <paramref name="monitorFunction"/> on <paramref name="target"/> once per pass until its value differs
        /// from the value it had at the start.
        /// </summary>
        /// <typeparam name="T">The type of the monitored object.</typeparam>
        /// <typeparam name="TValue">The type of the monitored value.</typeparam>
        /// <param name="target">The object to monitor.</param>
        /// <param name="monitorFunction">The function that reads the value from <paramref name="target"/>.</param>
        /// <param name="timing">The player-loop phase at which the value is read.</param>
        /// <param name="equalityComparer">The comparer that decides whether the value changed.</param>
        /// <param name="cancelImmediately"><c>true</c> to cancel as soon as <paramref name="token"/> is cancelled;
        /// <c>false</c> to observe it on the next pass.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>A task that returns the first value that differs from the initial value.</returns>
        /// <remarks>
        /// If <paramref name="target"/> is a destroyed <c>UnityEngine.Object</c> when a pass resumes, the task is
        /// cancelled with <paramref name="token"/>.
        /// </remarks>
        private static async UnityTask<TValue> WaitUntilValueChangedCoreAsync<T, TValue>(
              T target
            , Func<T, TValue> monitorFunction
            , UnityTaskTiming timing
            , IEqualityComparer<TValue> equalityComparer
            , bool cancelImmediately
            , CancellationToken token
        )
            where T : class
        {
            token.ThrowIfCancellationRequested();

            var initial = monitorFunction(target);

            while (true)
            {
                await Yield(timing, cancelImmediately, token);

                // A destroyed Unity object has no valid value to read, so the wait ends as cancelled.
                if (target is UnityEngine.Object unityObject && unityObject.IsInvalid())
                {
                    ThrowHelper.ThrowOperationCanceled(token);
                }

                var current = monitorFunction(target);

                if (equalityComparer.Equals(initial, current) == false)
                {
                    return current;
                }
            }
        }
    }
}
