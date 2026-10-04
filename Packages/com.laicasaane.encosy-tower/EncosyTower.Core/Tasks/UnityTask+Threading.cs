using System;
using System.Threading;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Runs <paramref name="action"/> on a thread-pool thread.
        /// </summary>
        /// <param name="action">The work to run on a thread-pool thread.</param>
        /// <param name="token">The token checked before and after the switch and after the work.</param>
        /// <returns>A task that completes when <paramref name="action"/> completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, switches to a thread-pool thread, checks the token, runs
        /// <paramref name="action"/>, then checks the token again. A late cancellation discards the result.
        /// Cancellation throws <c>new OperationCanceledException(token)</c>. An exception thrown by <paramref
        /// name="action"/> faults the task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> <paramref name="action"/> starts on a thread-pool thread. The awaiter resumes on the kind of
        /// thread that called this method: the main thread when called on the main thread, a thread-pool thread
        /// otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw <see
        /// cref="InvalidOperationException"/>, return a stale result, or observe another operation's result because
        /// sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs inside <paramref name="action"/>, or after resuming on a thread-pool thread (task created
        /// off the main thread): Unity throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
        public static UnityTask RunOnThreadPool(Action action, CancellationToken token = default)
        {
            Debugging.ThrowHelper.ThrowIfNull(action);
            return RunOnThreadPoolCoreAsync(action, token);
        }

        /// <summary>
        /// Runs <paramref name="action"/> on a thread-pool thread.
        /// </summary>
        /// <param name="action">The work to run on a thread-pool thread.</param>
        /// <param name="state">The state passed to <paramref name="action"/>.</param>
        /// <param name="token">The token checked before and after the switch and after the work.</param>
        /// <returns>A task that completes when <paramref name="action"/> completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, switches to a thread-pool thread, checks the token, runs
        /// <paramref name="action"/>, then checks the token again. A late cancellation discards the result.
        /// Cancellation throws <c>new OperationCanceledException(token)</c>. An exception thrown by <paramref
        /// name="action"/> faults the task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> <paramref name="action"/> starts on a thread-pool thread. The awaiter resumes on the kind of
        /// thread that called this method: the main thread when called on the main thread, a thread-pool thread
        /// otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw <see
        /// cref="InvalidOperationException"/>, return a stale result, or observe another operation's result because
        /// sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs inside <paramref name="action"/>, or after resuming on a thread-pool thread (task created
        /// off the main thread): Unity throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
        public static UnityTask RunOnThreadPool(Action<object> action, object state, CancellationToken token = default)
        {
            Debugging.ThrowHelper.ThrowIfNull(action);
            return RunOnThreadPoolCoreAsync(action, state, token);
        }

        /// <summary>
        /// Switches to a thread-pool thread, calls <paramref name="action"/> there and awaits the task it returns.
        /// </summary>
        /// <param name="action">The work to run on a thread-pool thread.</param>
        /// <param name="token">The token checked before and after the switch and after the work.</param>
        /// <returns>A task that completes when <paramref name="action"/> completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, switches to a thread-pool thread, checks the token,
        /// awaits the task that <paramref name="action"/> returns, then checks the token again. A late cancellation
        /// discards the result. Cancellation throws <c>new OperationCanceledException(token)</c>. An exception thrown
        /// by <paramref name="action"/> faults the task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> <paramref name="action"/> starts on a thread-pool thread. The awaiter resumes on the kind of
        /// thread that called this method: the main thread when called on the main thread, a thread-pool thread
        /// otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw <see
        /// cref="InvalidOperationException"/>, return a stale result, or observe another operation's result because
        /// sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs inside <paramref name="action"/>, or after resuming on a thread-pool thread (task created
        /// off the main thread): Unity throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
        public static UnityTask RunOnThreadPool(Func<UnityTask> action, CancellationToken token = default)
        {
            Debugging.ThrowHelper.ThrowIfNull(action);
            return RunOnThreadPoolCoreAsync(action, token);
        }

        /// <summary>
        /// Switches to a thread-pool thread, calls <paramref name="action"/> there and awaits the task it returns.
        /// </summary>
        /// <param name="action">The work to run on a thread-pool thread.</param>
        /// <param name="state">The state passed to <paramref name="action"/>.</param>
        /// <param name="token">The token checked before and after the switch and after the work.</param>
        /// <returns>A task that completes when <paramref name="action"/> completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, switches to a thread-pool thread, checks the token,
        /// awaits the task that <paramref name="action"/> returns, then checks the token again. A late cancellation
        /// discards the result. Cancellation throws <c>new OperationCanceledException(token)</c>. An exception thrown
        /// by <paramref name="action"/> faults the task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> <paramref name="action"/> starts on a thread-pool thread. The awaiter resumes on the kind of
        /// thread that called this method: the main thread when called on the main thread, a thread-pool thread
        /// otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw <see
        /// cref="InvalidOperationException"/>, return a stale result, or observe another operation's result because
        /// sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs inside <paramref name="action"/>, or after resuming on a thread-pool thread (task created
        /// off the main thread): Unity throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
        public static UnityTask RunOnThreadPool(
              Func<object, UnityTask> action
            , object state
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(action);
            return RunOnThreadPoolCoreAsync(action, state, token);
        }

        /// <summary>
        /// Runs <paramref name="function"/> on a thread-pool thread.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="function">The work to run on a thread-pool thread.</param>
        /// <param name="token">The token checked before and after the switch and after the work.</param>
        /// <returns>A task that completes with the result of <paramref name="function"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, switches to a thread-pool thread, checks the token, runs
        /// <paramref name="function"/>, then checks the token again. A late cancellation discards the result.
        /// Cancellation throws <c>new OperationCanceledException(token)</c>. An exception thrown by <paramref
        /// name="function"/> faults the task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> <paramref name="function"/> starts on a thread-pool thread. The awaiter resumes on the kind
        /// of thread that called this method: the main thread when called on the main thread, a thread-pool thread
        /// otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw <see
        /// cref="InvalidOperationException"/>, return a stale result, or observe another operation's result because
        /// sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs inside <paramref name="function"/>, or after resuming on a thread-pool thread (task
        /// created off the main thread): Unity throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> is <c>null</c>.</exception>
        public static UnityTask<T> RunOnThreadPool<T>(Func<T> function, CancellationToken token = default)
        {
            Debugging.ThrowHelper.ThrowIfNull(function);
            return RunOnThreadPoolCoreAsync<T>(function, token);
        }

        /// <summary>
        /// Switches to a thread-pool thread, calls <paramref name="function"/> there and awaits the task it returns.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="function">The work to run on a thread-pool thread.</param>
        /// <param name="token">The token checked before and after the switch and after the work.</param>
        /// <returns>A task that completes with the result of <paramref name="function"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, switches to a thread-pool thread, checks the token,
        /// awaits the task that <paramref name="function"/> returns, then checks the token again. A late cancellation
        /// discards the result. Cancellation throws <c>new OperationCanceledException(token)</c>. An exception thrown
        /// by <paramref name="function"/> faults the task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> <paramref name="function"/> starts on a thread-pool thread. The awaiter resumes on the kind
        /// of thread that called this method: the main thread when called on the main thread, a thread-pool thread
        /// otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw <see
        /// cref="InvalidOperationException"/>, return a stale result, or observe another operation's result because
        /// sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs inside <paramref name="function"/>, or after resuming on a thread-pool thread (task
        /// created off the main thread): Unity throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> is <c>null</c>.</exception>
        public static UnityTask<T> RunOnThreadPool<T>(Func<UnityTask<T>> function, CancellationToken token = default)
        {
            Debugging.ThrowHelper.ThrowIfNull(function);
            return RunOnThreadPoolCoreAsync<T>(function, token);
        }

        /// <summary>
        /// Runs <paramref name="function"/> on a thread-pool thread.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="function">The work to run on a thread-pool thread.</param>
        /// <param name="state">The state passed to <paramref name="function"/>.</param>
        /// <param name="token">The token checked before and after the switch and after the work.</param>
        /// <returns>A task that completes with the result of <paramref name="function"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, switches to a thread-pool thread, checks the token, runs
        /// <paramref name="function"/>, then checks the token again. A late cancellation discards the result.
        /// Cancellation throws <c>new OperationCanceledException(token)</c>. An exception thrown by <paramref
        /// name="function"/> faults the task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> <paramref name="function"/> starts on a thread-pool thread. The awaiter resumes on the kind
        /// of thread that called this method: the main thread when called on the main thread, a thread-pool thread
        /// otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw <see
        /// cref="InvalidOperationException"/>, return a stale result, or observe another operation's result because
        /// sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs inside <paramref name="function"/>, or after resuming on a thread-pool thread (task
        /// created off the main thread): Unity throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> is <c>null</c>.</exception>
        public static UnityTask<T> RunOnThreadPool<T>(
              Func<object, T> function
            , object state
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(function);
            return RunOnThreadPoolCoreAsync<T>(function, state, token);
        }

        /// <summary>
        /// Switches to a thread-pool thread, calls <paramref name="function"/> there and awaits the task it returns.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="function">The work to run on a thread-pool thread.</param>
        /// <param name="state">The state passed to <paramref name="function"/>.</param>
        /// <param name="token">The token checked before and after the switch and after the work.</param>
        /// <returns>A task that completes with the result of <paramref name="function"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> checks <paramref name="token"/>, switches to a thread-pool thread, checks the token,
        /// awaits the task that <paramref name="function"/> returns, then checks the token again. A late cancellation
        /// discards the result. Cancellation throws <c>new OperationCanceledException(token)</c>. An exception thrown
        /// by <paramref name="function"/> faults the task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> <paramref name="function"/> starts on a thread-pool thread. The awaiter resumes on the kind
        /// of thread that called this method: the main thread when called on the main thread, a thread-pool thread
        /// otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw <see
        /// cref="InvalidOperationException"/>, return a stale result, or observe another operation's result because
        /// sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs inside <paramref name="function"/>, or after resuming on a thread-pool thread (task
        /// created off the main thread): Unity throws or corrupts state, per Unity rules.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> is <c>null</c>.</exception>
        public static UnityTask<T> RunOnThreadPool<T>(
              Func<object, UnityTask<T>> function
            , object state
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(function);
            return RunOnThreadPoolCoreAsync<T>(function, state, token);
        }

        private static async UnityTask RunOnThreadPoolCoreAsync(Action action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await new ThreadPoolSwitch();
            token.ThrowIfCancellationRequested();

            action();

            token.ThrowIfCancellationRequested();
        }

        private static async UnityTask RunOnThreadPoolCoreAsync(
              Action<object> action
            , object state
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            await new ThreadPoolSwitch();
            token.ThrowIfCancellationRequested();

            action(state);

            token.ThrowIfCancellationRequested();
        }

        private static async UnityTask RunOnThreadPoolCoreAsync(Func<UnityTask> action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await new ThreadPoolSwitch();
            token.ThrowIfCancellationRequested();

            await action();

            token.ThrowIfCancellationRequested();
        }

        private static async UnityTask RunOnThreadPoolCoreAsync(
              Func<object, UnityTask> action
            , object state
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            await new ThreadPoolSwitch();
            token.ThrowIfCancellationRequested();

            await action(state);

            token.ThrowIfCancellationRequested();
        }

        private static async UnityTask<T> RunOnThreadPoolCoreAsync<T>(Func<T> function, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await new ThreadPoolSwitch();
            token.ThrowIfCancellationRequested();

            var result = function();

            token.ThrowIfCancellationRequested();
            return result;
        }

        private static async UnityTask<T> RunOnThreadPoolCoreAsync<T>(
              Func<UnityTask<T>> function
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            await new ThreadPoolSwitch();
            token.ThrowIfCancellationRequested();

            var result = await function();

            token.ThrowIfCancellationRequested();
            return result;
        }

        private static async UnityTask<T> RunOnThreadPoolCoreAsync<T>(
              Func<object, T> function
            , object state
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            await new ThreadPoolSwitch();
            token.ThrowIfCancellationRequested();

            var result = function(state);

            token.ThrowIfCancellationRequested();
            return result;
        }

        private static async UnityTask<T> RunOnThreadPoolCoreAsync<T>(
              Func<object, UnityTask<T>> function
            , object state
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            await new ThreadPoolSwitch();
            token.ThrowIfCancellationRequested();

            var result = await function(state);

            token.ThrowIfCancellationRequested();
            return result;
        }
    }
}
