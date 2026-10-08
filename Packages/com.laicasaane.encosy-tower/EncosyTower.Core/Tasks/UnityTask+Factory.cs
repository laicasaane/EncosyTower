using System;
using System.Threading;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Gets a task that has already completed successfully.
        /// </summary>
        /// <remarks>
        /// <para><b>Behaviour:</b> returns <c>default(UnityTask)</c>, which completes inline on any thread.</para>
        /// <para><b>Thread:</b> the awaiter continues inline on the awaiting thread.</para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.CompletedTask</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask CompletedTask => default;

        /// <summary>
        /// Returns a task that has already completed successfully.
        /// </summary>
        /// <returns><c>default(UnityTask)</c>.</returns>
        /// <remarks>
        /// <para><b>Behaviour:</b> completes inline on any thread.</para>
        /// <para><b>Thread:</b> the awaiter continues inline on the awaiting thread.</para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.CompletedTask</c> and <c>UniTask.FromResult</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static UnityTask GetCompleted()
            => default;

        /// <summary>
        /// Returns a task that has already completed successfully with <c>default(T)</c>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <returns>A completed task whose result is <c>default(T)</c>.</returns>
        /// <remarks>
        /// <para><b>Behaviour:</b> completes inline on any thread.</para>
        /// <para><b>Thread:</b> the awaiter continues inline on the awaiting thread.</para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.CompletedTask</c> and <c>UniTask.FromResult</c>; Unity: none.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend a result-carrying task wraps <c>UniTask.FromResult</c>; on the
        /// <c>Awaitable</c> backend the result is stored in the task itself.
        /// </para>
        /// </remarks>
        public static UnityTask<T> GetCompleted<T>()
            => FromResult<T>(default);

        /// <summary>
        /// Returns a task that has already completed successfully with <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="value">The result of the task.</param>
        /// <returns>A completed task whose result is <paramref name="value"/>.</returns>
        /// <remarks>
        /// <para><b>Behaviour:</b> completes inline on any thread.</para>
        /// <para><b>Thread:</b> the awaiter continues inline on the awaiting thread.</para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.CompletedTask</c> and <c>UniTask.FromResult</c>; Unity: none.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend a result-carrying task wraps <c>UniTask.FromResult</c>; on the
        /// <c>Awaitable</c> backend the result is stored in the task itself.
        /// </para>
        /// </remarks>
        public static UnityTask<T> GetCompleted<T>(T value)
            => FromResult(value);

        /// <summary>
        /// Returns a task that has already completed successfully with <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="value">The result of the task.</param>
        /// <returns>A completed task whose result is <paramref name="value"/>.</returns>
        /// <remarks>
        /// <para><b>Behaviour:</b> completes inline on any thread.</para>
        /// <para><b>Thread:</b> the awaiter continues inline on the awaiting thread.</para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.FromResult</c>; Unity: a completed
        /// <see cref="UnityEngine.AwaitableCompletionSource{T}"/> (<c>SetResult</c>).
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend this returns <c>UniTask.FromResult</c>; on the <c>Awaitable</c>
        /// backend the result is stored in the task itself. Neither allocates.
        /// </para>
        /// </remarks>
        public static UnityTask<T> FromResult<T>(T value)
            => CreateFromResult(value);

        /// <summary>
        /// Returns a task that has already completed with <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The exception that awaiting the task rethrows unchanged.</param>
        /// <returns>A faulted task, or a canceled task when <paramref name="exception"/> is an
        /// <see cref="OperationCanceledException"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes inline on any thread. Awaiting the task throws the original instance of
        /// <paramref name="exception"/> with its original stack. An <see cref="OperationCanceledException"/> is not
        /// converted and keeps its derived type, message and token.
        /// </para>
        /// <para><b>Thread:</b> the awaiter continues inline on the awaiting thread.</para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.FromException</c>; Unity: a completed
        /// <see cref="UnityEngine.AwaitableCompletionSource"/> (<c>SetException</c>).
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
        public static UnityTask FromException(Exception exception)
        {
            Debugging.ThrowHelper.ThrowIfNull(exception);

            var source = UnityTaskCompletionSource.CreateInline();
            source.TrySetException(exception);
            return source.Task;
        }

        /// <summary>
        /// Returns a task that has already completed with <paramref name="exception"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="exception">The exception that awaiting the task rethrows unchanged.</param>
        /// <returns>A faulted task, or a canceled task when <paramref name="exception"/> is an
        /// <see cref="OperationCanceledException"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> completes inline on any thread. Awaiting the task throws the original instance of
        /// <paramref name="exception"/> with its original stack. An <see cref="OperationCanceledException"/> is not
        /// converted and keeps its derived type, message and token.
        /// </para>
        /// <para><b>Thread:</b> the awaiter continues inline on the awaiting thread.</para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.FromException</c>; Unity: a completed
        /// <see cref="UnityEngine.AwaitableCompletionSource"/> (<c>SetException</c>).
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
        public static UnityTask<T> FromException<T>(Exception exception)
        {
            Debugging.ThrowHelper.ThrowIfNull(exception);

            var source = UnityTaskCompletionSource<T>.CreateInline();
            source.TrySetException(exception);
            return source.Task;
        }

        /// <summary>
        /// Returns a task that has already completed with <c>new OperationCanceledException(token)</c>.
        /// </summary>
        /// <param name="token">The token stored in the thrown <see cref="OperationCanceledException"/>.</param>
        /// <returns>A canceled task.</returns>
        /// <remarks>
        /// <para><b>Behaviour:</b> completes inline on any thread, as <see cref="FromException"/> does.</para>
        /// <para><b>Thread:</b> the awaiter continues inline on the awaiting thread.</para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.FromCanceled</c>; Unity: a completed
        /// <see cref="UnityEngine.AwaitableCompletionSource"/> (<c>SetCanceled</c>), which does not keep a token.
        /// </para>
        /// </remarks>
        public static UnityTask FromCanceled(CancellationToken token = default)
            => FromException(new OperationCanceledException(token));

        /// <summary>
        /// Returns a task that has already completed with <c>new OperationCanceledException(token)</c>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="token">The token stored in the thrown <see cref="OperationCanceledException"/>.</param>
        /// <returns>A canceled task.</returns>
        /// <remarks>
        /// <para><b>Behaviour:</b> completes inline on any thread, as <see cref="FromException{T}"/> does.</para>
        /// <para><b>Thread:</b> the awaiter continues inline on the awaiting thread.</para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.FromCanceled</c>; Unity: a completed
        /// <see cref="UnityEngine.AwaitableCompletionSource"/> (<c>SetCanceled</c>), which does not keep a token.
        /// </para>
        /// </remarks>
        public static UnityTask<T> FromCanceled<T>(CancellationToken token = default)
            => FromException<T>(new OperationCanceledException(token));
    }
}
