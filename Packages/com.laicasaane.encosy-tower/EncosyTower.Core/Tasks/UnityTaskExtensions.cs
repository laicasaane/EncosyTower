using System;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Provides continuation and fire-and-forget operations for <see cref="UnityTask"/> and
    /// <see cref="UnityTask{T}"/>.
    /// </summary>
    public static class UnityTaskExtensions
    {
        /// <summary>
        /// Runs <paramref name="continuationFunction"/> after <paramref name="task"/> completes successfully.
        /// </summary>
        /// <typeparam name="T">The result type of <paramref name="task"/>.</typeparam>
        /// <param name="task">The antecedent task.</param>
        /// <param name="continuationFunction">The continuation to run; it receives the result of <paramref
        /// name="task"/>.</param>
        /// <returns>A task that completes when the continuation completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the continuation runs only after <paramref name="task"/> succeeds. A fault or cancellation
        /// of <paramref name="task"/> propagates unchanged, as the same exception instance, and skips the continuation.
        /// An exception thrown by the continuation faults the returned task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the continuation runs where <paramref name="task"/> resumes its awaiter. The returned task
        /// resumes its awaiter on the kind of thread that called this method: the main thread when called on the main
        /// thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the returned task or a copy more than once, calling <c>GetResult</c> twice, or awaiting <paramref
        /// name="task"/> elsewhere as well: may throw <see cref="InvalidOperationException"/>, return a stale result,
        /// or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An antecedent obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: the antecedent
        /// follows native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="continuationFunction"/> is <c>null</c>.</exception>
        public static UnityTask ContinueWith<T>(this UnityTask<T> task, Action<T> continuationFunction)
        {
            Debugging.ThrowHelper.ThrowIfNull(continuationFunction);
            return ContinueWithCoreAsync<T>(task, continuationFunction);
        }

        /// <summary>
        /// Runs <paramref name="continuationFunction"/> after <paramref name="task"/> completes successfully.
        /// </summary>
        /// <typeparam name="T">The result type of <paramref name="task"/>.</typeparam>
        /// <param name="task">The antecedent task.</param>
        /// <param name="continuationFunction">The continuation to run; it receives the result of <paramref
        /// name="task"/>.</param>
        /// <returns>A task that completes when the continuation completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the continuation runs only after <paramref name="task"/> succeeds and its returned task is
        /// awaited. A fault or cancellation of <paramref name="task"/> propagates unchanged, as the same exception
        /// instance, and skips the continuation. An exception thrown by the continuation faults the returned task with
        /// that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the continuation runs where <paramref name="task"/> resumes its awaiter. The returned task
        /// resumes its awaiter on the kind of thread that called this method: the main thread when called on the main
        /// thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the returned task or a copy more than once, calling <c>GetResult</c> twice, or awaiting <paramref
        /// name="task"/> elsewhere as well: may throw <see cref="InvalidOperationException"/>, return a stale result,
        /// or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An antecedent obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: the antecedent
        /// follows native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="continuationFunction"/> is <c>null</c>.</exception>
        public static UnityTask ContinueWith<T>(this UnityTask<T> task, Func<T, UnityTask> continuationFunction)
        {
            Debugging.ThrowHelper.ThrowIfNull(continuationFunction);
            return ContinueWithCoreAsync<T>(task, continuationFunction);
        }

        /// <summary>
        /// Runs <paramref name="continuationFunction"/> after <paramref name="task"/> completes successfully.
        /// </summary>
        /// <typeparam name="T">The result type of <paramref name="task"/>.</typeparam>
        /// <typeparam name="TR">The result type of the continuation.</typeparam>
        /// <param name="task">The antecedent task.</param>
        /// <param name="continuationFunction">The continuation to run; it receives the result of <paramref
        /// name="task"/>.</param>
        /// <returns>A task that completes with the result of the continuation.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the continuation runs only after <paramref name="task"/> succeeds. A fault or cancellation
        /// of <paramref name="task"/> propagates unchanged, as the same exception instance, and skips the continuation.
        /// An exception thrown by the continuation faults the returned task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the continuation runs where <paramref name="task"/> resumes its awaiter. The returned task
        /// resumes its awaiter on the kind of thread that called this method: the main thread when called on the main
        /// thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the returned task or a copy more than once, calling <c>GetResult</c> twice, or awaiting <paramref
        /// name="task"/> elsewhere as well: may throw <see cref="InvalidOperationException"/>, return a stale result,
        /// or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An antecedent obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: the antecedent
        /// follows native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="continuationFunction"/> is <c>null</c>.</exception>
        public static UnityTask<TR> ContinueWith<T, TR>(this UnityTask<T> task, Func<T, TR> continuationFunction)
        {
            Debugging.ThrowHelper.ThrowIfNull(continuationFunction);
            return ContinueWithCoreAsync<T, TR>(task, continuationFunction);
        }

        /// <summary>
        /// Runs <paramref name="continuationFunction"/> after <paramref name="task"/> completes successfully.
        /// </summary>
        /// <typeparam name="T">The result type of <paramref name="task"/>.</typeparam>
        /// <typeparam name="TR">The result type of the continuation.</typeparam>
        /// <param name="task">The antecedent task.</param>
        /// <param name="continuationFunction">The continuation to run; it receives the result of <paramref
        /// name="task"/>.</param>
        /// <returns>A task that completes with the result of the continuation.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the continuation runs only after <paramref name="task"/> succeeds and its returned task is
        /// awaited. A fault or cancellation of <paramref name="task"/> propagates unchanged, as the same exception
        /// instance, and skips the continuation. An exception thrown by the continuation faults the returned task with
        /// that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the continuation runs where <paramref name="task"/> resumes its awaiter. The returned task
        /// resumes its awaiter on the kind of thread that called this method: the main thread when called on the main
        /// thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the returned task or a copy more than once, calling <c>GetResult</c> twice, or awaiting <paramref
        /// name="task"/> elsewhere as well: may throw <see cref="InvalidOperationException"/>, return a stale result,
        /// or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An antecedent obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: the antecedent
        /// follows native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="continuationFunction"/> is <c>null</c>.</exception>
        public static UnityTask<TR> ContinueWith<T, TR>(
              this UnityTask<T> task
            , Func<T, UnityTask<TR>> continuationFunction
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(continuationFunction);
            return ContinueWithCoreAsync<T, TR>(task, continuationFunction);
        }

        /// <summary>
        /// Runs <paramref name="continuationFunction"/> after <paramref name="task"/> completes successfully.
        /// </summary>
        /// <param name="task">The antecedent task.</param>
        /// <param name="continuationFunction">The continuation to run; it takes no argument.</param>
        /// <returns>A task that completes when the continuation completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the continuation runs only after <paramref name="task"/> succeeds. A fault or cancellation
        /// of <paramref name="task"/> propagates unchanged, as the same exception instance, and skips the continuation.
        /// An exception thrown by the continuation faults the returned task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the continuation runs where <paramref name="task"/> resumes its awaiter. The returned task
        /// resumes its awaiter on the kind of thread that called this method: the main thread when called on the main
        /// thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the returned task or a copy more than once, calling <c>GetResult</c> twice, or awaiting <paramref
        /// name="task"/> elsewhere as well: may throw <see cref="InvalidOperationException"/>, return a stale result,
        /// or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An antecedent obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: the antecedent
        /// follows native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="continuationFunction"/> is <c>null</c>.</exception>
        public static UnityTask ContinueWith(this UnityTask task, Action continuationFunction)
        {
            Debugging.ThrowHelper.ThrowIfNull(continuationFunction);
            return ContinueWithCoreAsync(task, continuationFunction);
        }

        /// <summary>
        /// Runs <paramref name="continuationFunction"/> after <paramref name="task"/> completes successfully.
        /// </summary>
        /// <param name="task">The antecedent task.</param>
        /// <param name="continuationFunction">The continuation to run; it takes no argument.</param>
        /// <returns>A task that completes when the continuation completes.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the continuation runs only after <paramref name="task"/> succeeds and its returned task is
        /// awaited. A fault or cancellation of <paramref name="task"/> propagates unchanged, as the same exception
        /// instance, and skips the continuation. An exception thrown by the continuation faults the returned task with
        /// that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the continuation runs where <paramref name="task"/> resumes its awaiter. The returned task
        /// resumes its awaiter on the kind of thread that called this method: the main thread when called on the main
        /// thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the returned task or a copy more than once, calling <c>GetResult</c> twice, or awaiting <paramref
        /// name="task"/> elsewhere as well: may throw <see cref="InvalidOperationException"/>, return a stale result,
        /// or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An antecedent obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: the antecedent
        /// follows native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="continuationFunction"/> is <c>null</c>.</exception>
        public static UnityTask ContinueWith(this UnityTask task, Func<UnityTask> continuationFunction)
        {
            Debugging.ThrowHelper.ThrowIfNull(continuationFunction);
            return ContinueWithCoreAsync(task, continuationFunction);
        }

        /// <summary>
        /// Runs <paramref name="continuationFunction"/> after <paramref name="task"/> completes successfully.
        /// </summary>
        /// <typeparam name="T">The result type of the continuation.</typeparam>
        /// <param name="task">The antecedent task.</param>
        /// <param name="continuationFunction">The continuation to run; it takes no argument.</param>
        /// <returns>A task that completes with the result of the continuation.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the continuation runs only after <paramref name="task"/> succeeds. A fault or cancellation
        /// of <paramref name="task"/> propagates unchanged, as the same exception instance, and skips the continuation.
        /// An exception thrown by the continuation faults the returned task with that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the continuation runs where <paramref name="task"/> resumes its awaiter. The returned task
        /// resumes its awaiter on the kind of thread that called this method: the main thread when called on the main
        /// thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the returned task or a copy more than once, calling <c>GetResult</c> twice, or awaiting <paramref
        /// name="task"/> elsewhere as well: may throw <see cref="InvalidOperationException"/>, return a stale result,
        /// or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An antecedent obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: the antecedent
        /// follows native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="continuationFunction"/> is <c>null</c>.</exception>
        public static UnityTask<T> ContinueWith<T>(this UnityTask task, Func<T> continuationFunction)
        {
            Debugging.ThrowHelper.ThrowIfNull(continuationFunction);
            return ContinueWithCoreAsync<T>(task, continuationFunction);
        }

        /// <summary>
        /// Runs <paramref name="continuationFunction"/> after <paramref name="task"/> completes successfully.
        /// </summary>
        /// <typeparam name="T">The result type of the continuation.</typeparam>
        /// <param name="task">The antecedent task.</param>
        /// <param name="continuationFunction">The continuation to run; it takes no argument.</param>
        /// <returns>A task that completes with the result of the continuation.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the continuation runs only after <paramref name="task"/> succeeds and its returned task is
        /// awaited. A fault or cancellation of <paramref name="task"/> propagates unchanged, as the same exception
        /// instance, and skips the continuation. An exception thrown by the continuation faults the returned task with
        /// that instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the continuation runs where <paramref name="task"/> resumes its awaiter. The returned task
        /// resumes its awaiter on the kind of thread that called this method: the main thread when called on the main
        /// thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the returned task or a copy more than once, calling <c>GetResult</c> twice, or awaiting <paramref
        /// name="task"/> elsewhere as well: may throw <see cref="InvalidOperationException"/>, return a stale result,
        /// or observe another operation's result because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.
        /// </description></item>
        /// <item><description>
        /// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread): Unity throws
        /// or corrupts state, per Unity rules.
        /// </description></item>
        /// <item><description>
        /// An antecedent obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: the antecedent
        /// follows native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="continuationFunction"/> is <c>null</c>.</exception>
        public static UnityTask<T> ContinueWith<T>(this UnityTask task, Func<UnityTask<T>> continuationFunction)
        {
            Debugging.ThrowHelper.ThrowIfNull(continuationFunction);
            return ContinueWithCoreAsync<T>(task, continuationFunction);
        }

        /// <summary>
        /// Observes <paramref name="task"/> without awaiting it, so that its fault is reported.
        /// </summary>
        /// <param name="task">The task to observe.</param>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> a fault other than <see cref="OperationCanceledException"/> is logged once through
        /// <c>StaticLogger.LogException</c> in every build. Cancellation is silent.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the fault is logged on the thread where <paramref name="task"/> resumes its awaiter.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting <paramref name="task"/> or a copy after calling this method, or calling it twice: may throw <see
        /// cref="InvalidOperationException"/>, return a stale result, or observe another operation's result because
        /// sources are pooled.
        /// </description></item>
        /// <item><description>
        /// A task obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: the task follows
        /// native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        public static void Forget(this UnityTask task)
            => UnityTaskForgetObserver.Observe(task);

        /// <summary>
        /// Observes <paramref name="task"/> without awaiting it, so that its fault is reported.
        /// </summary>
        /// <typeparam name="T">The result type of <paramref name="task"/>; the result is discarded.</typeparam>
        /// <param name="task">The task to observe.</param>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> a fault other than <see cref="OperationCanceledException"/> is logged once through
        /// <c>StaticLogger.LogException</c> in every build. Cancellation is silent.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the fault is logged on the thread where <paramref name="task"/> resumes its awaiter.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting <paramref name="task"/> or a copy after calling this method, or calling it twice: may throw <see
        /// cref="InvalidOperationException"/>, return a stale result, or observe another operation's result because
        /// sources are pooled.
        /// </description></item>
        /// <item><description>
        /// A task obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: the task follows
        /// native-library behaviour.
        /// </description></item>
        /// </list>
        /// </remarks>
        public static void Forget<T>(this UnityTask<T> task)
            => UnityTaskForgetObserver<T>.Observe(task);

        private static async UnityTask ContinueWithCoreAsync<T>(UnityTask<T> task, Action<T> continuationFunction)
        {
            var result = await task;

            continuationFunction(result);
        }

        private static async UnityTask ContinueWithCoreAsync<T>(
              UnityTask<T> task
            , Func<T, UnityTask> continuationFunction
        )
        {
            var result = await task;

            await continuationFunction(result);
        }

        private static async UnityTask<TR> ContinueWithCoreAsync<T, TR>(
              UnityTask<T> task
            , Func<T, TR> continuationFunction
        )
        {
            var result = await task;

            return continuationFunction(result);
        }

        private static async UnityTask<TR> ContinueWithCoreAsync<T, TR>(
              UnityTask<T> task
            , Func<T, UnityTask<TR>> continuationFunction
        )
        {
            var result = await task;

            return await continuationFunction(result);
        }

        private static async UnityTask ContinueWithCoreAsync(UnityTask task, Action continuationFunction)
        {
            await task;

            continuationFunction();
        }

        private static async UnityTask ContinueWithCoreAsync(UnityTask task, Func<UnityTask> continuationFunction)
        {
            await task;

            await continuationFunction();
        }

        private static async UnityTask<T> ContinueWithCoreAsync<T>(UnityTask task, Func<T> continuationFunction)
        {
            await task;

            return continuationFunction();
        }

        private static async UnityTask<T> ContinueWithCoreAsync<T>(
              UnityTask task
            , Func<UnityTask<T>> continuationFunction
        )
        {
            await task;

            return await continuationFunction();
        }
    }
}
