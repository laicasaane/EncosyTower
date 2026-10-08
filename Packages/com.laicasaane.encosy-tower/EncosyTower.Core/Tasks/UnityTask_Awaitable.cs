#if !UNITASK || ENCOSY_UNITYTASK_AWAITABLE

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tasks;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        private readonly IUnityTaskSource _source;
        private readonly short _token;

        /// <summary>
        /// Creates a task that reads <paramref name="source"/> through <paramref name="token"/>.
        /// </summary>
        /// <param name="source">The source that produces the outcome; <c>null</c> means it already succeeded.</param>
        /// <param name="token">The version of <paramref name="source"/> that this task was created with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UnityTask(IUnityTaskSource source, short token)
        {
            _source = source;
            _token = token;
        }

        /// <summary>
        /// Gets a value that indicates whether the task has completed and can be awaited on the current thread without
        /// suspending.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Thread:</b> returns <c>false</c> when the task is done but the current thread kind (main or thread pool)
        /// differs from the kind of thread that created it; awaiting it then resumes on the creator's thread kind.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Status</c> (through its awaiter); Unity:
        /// <see cref="UnityEngine.Awaitable.IsCompleted"/>.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend this reads <c>UniTask.Awaiter.IsCompleted</c>; on the
        /// <c>Awaitable</c> backend it reads the EncosyTower task source, never <c>Awaitable.IsCompleted</c>.
        /// </para>
        /// </remarks>
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source == null || _source.GetStatus(_token) != UnityTaskStatus.Pending;
        }

        /// <summary>
        /// Gets the current state of the task.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Thread:</b> returns <see cref="UnityTaskStatus.Pending"/> when the task is done but the current thread
        /// kind (main or thread pool) differs from the kind of thread that created it, as <see cref="IsCompleted"/>
        /// does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Status</c>; Unity: none, because
        /// <see cref="UnityEngine.Awaitable"/> exposes only <c>IsCompleted</c>.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend this maps <c>UniTask.Status</c> (same values); on the
        /// <c>Awaitable</c> backend it reads the EncosyTower task source.
        /// </para>
        /// </remarks>
        public UnityTaskStatus Status
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source == null ? UnityTaskStatus.Succeeded : _source.GetStatus(_token);
        }

        /// <summary>
        /// Converts a native <see cref="Awaitable"/> to a <see cref="UnityTask"/>.
        /// </summary>
        /// <param name="task">The native task to wrap.</param>
        /// <remarks>
        /// <para>
        /// <b>Interop:</b> a pooled relay observes <paramref name="task"/> once and completes the returned task on the
        /// thread that completes <paramref name="task"/>; the thread-kind rule of the <see cref="UnityTask"/>
        /// behaviour contract does not apply to it.
        /// </para>
        /// <para>
        /// <b>Backends:</b> only the selected backend's conversion exists: from <c>UniTask</c> on the UniTask backend,
        /// from <c>Awaitable</c> on the <c>Awaitable</c> backend.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is <c>null</c>.</exception>
        public static implicit operator UnityTask(Awaitable task)
        {
            DebuggingThrowHelper.ThrowIfNull(task);
            return AwaitableRelaySource.Create(task);
        }

        /// <summary>
        /// Returns a task that has already succeeded with <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="value">The result of the completed task.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UnityTask<T> CreateFromResult<T>(T value)
            => new(value);

        /// <summary>
        /// Creates a task that reads <paramref name="source"/> through <paramref name="token"/>.
        /// </summary>
        /// <param name="source">The source that produces the outcome.</param>
        /// <param name="token">The version of <paramref name="source"/> that this task was created with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UnityTask FromSource(IUnityTaskSource source, short token)
            => new(source, token);

        /// <summary>
        /// Creates a task that reads <paramref name="source"/> through <paramref name="token"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="source">The source that produces the result.</param>
        /// <param name="token">The version of <paramref name="source"/> that this task was created with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UnityTask<T> FromSource<T>(IUnityTaskSource<T> source, short token)
            => new(source, token);

        /// <summary>
        /// Returns an awaiter for this task.
        /// </summary>
        /// <returns>An awaiter for this task.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.GetAwaiter</c>; Unity:
        /// <see cref="UnityEngine.Awaitable.GetAwaiter"/>.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend the awaiter wraps <c>UniTask.Awaiter</c>; on the <c>Awaitable</c>
        /// backend it reads the EncosyTower task source directly.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_source, _token);

        /// <summary>
        /// Returns a native <see cref="Awaitable"/> that completes when this task completes.
        /// </summary>
        /// <returns>A completed awaitable for <c>default(UnityTask)</c>; otherwise a relay that awaits this
        /// task.</returns>
        /// <remarks>
        /// <b>Interop:</b> awaiting the relay consumes this task. The native awaitable follows native
        /// <c>Awaitable</c> behaviour; the <see cref="UnityTask"/> behaviour contract does not apply to operations
        /// performed on it.
        /// </remarks>
        public Awaitable AsAwaitable()
            => _source == null ? Awaitables.GetCompleted() : RelayAsync(this);

        /// <summary>
        /// Returns a task that can be awaited more than once and returns the same outcome each time.
        /// </summary>
        /// <returns>A task that stores the outcome of this task.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the first await consumes this task; later awaits return the stored result or rethrow the
        /// stored exception instance. Do not await this task directly after calling this method.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the returned task from two continuations before it completes: may throw
        /// <see cref="InvalidOperationException"/>, because only one continuation can wait on the underlying task.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Preserve</c>; Unity: none.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend this calls <c>UniTask.Preserve</c>; on the <c>Awaitable</c> backend
        /// an EncosyTower memoizing source with the same rules wraps the task.
        /// </para>
        /// </remarks>
        public UnityTask Preserve()
            => _source == null ? this : new(new UnityTaskMemoizeSource(_source), _token);

        /// <summary>
        /// Returns the state of the task in parentheses, such as <c>(Succeeded)</c>.
        /// </summary>
        /// <returns>The state text.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.ToString</c>; Unity: none.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend this calls <c>UniTask.ToString</c>; on the <c>Awaitable</c> backend
        /// it formats the task source status the same way.
        /// </para>
        /// </remarks>
        public override string ToString()
            => _source == null ? "()" : $"({_source.UnsafeGetStatus()})";

        /// <summary>
        /// Awaits <paramref name="task"/> inside an <c>async Awaitable</c> method, so the returned <c>Awaitable</c>
        /// completes when the task does.
        /// </summary>
        /// <param name="task">The task to await.</param>
        /// <returns>An <c>Awaitable</c> that completes with the outcome of <paramref name="task"/>.</returns>
        /// <remarks>
        /// The task is awaited through its <see cref="UnityTask.Awaiter"/>, which registers a single continuation with
        /// the task source. Awaiting the returned <c>Awaitable</c> therefore consumes <paramref name="task"/>.
        /// </remarks>
        private static async Awaitable RelayAsync(UnityTask task)
        {
            await task;
        }

        /// <summary>
        /// Awaits a <see cref="UnityTask"/>.
        /// </summary>
        /// <remarks>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Calling <see cref="GetResult"/> twice, or awaiting the task or a copy more than once: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <see cref="GetResult"/> before completion: may throw or block indefinitely.
        /// </description></item>
        /// </list>
        /// </remarks>
        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly IUnityTaskSource _source;
            private readonly short _token;

            internal Awaiter(IUnityTaskSource source, short token)
            {
                _source = source;
                _token = token;
            }

            /// <summary>
            /// Gets a value that indicates whether the task has completed and its result can be read on the current
            /// thread without suspending.
            /// </summary>
            public bool IsCompleted => _source == null || _source.GetStatus(_token) != UnityTaskStatus.Pending;

            /// <summary>
            /// Ends the wait; throws the original exception instance when the task faulted or was cancelled.
            /// </summary>
            public void GetResult()
            {
                if (_source != null)
                {
                    _source.GetResult(_token);
                }
            }

            /// <summary>
            /// Schedules <paramref name="continuation"/> to run when the task completes.
            /// </summary>
            /// <param name="continuation">The action to run on completion.</param>
            public void OnCompleted(Action continuation)
            {
                if (_source == null)
                {
                    continuation();
                    return;
                }

                _source.OnCompleted(UnityTaskContinuation.s_invoke, continuation, _token);
            }

            /// <summary>
            /// Schedules <paramref name="continuation"/> to run when the task completes, without flowing the execution
            /// context.
            /// </summary>
            /// <param name="continuation">The action to run on completion.</param>
            public void UnsafeOnCompleted(Action continuation)
                => OnCompleted(continuation);
        }
    }

    public readonly partial struct UnityTask<T>
    {
        private readonly IUnityTaskSource<T> _source;
        private readonly T _result;
        private readonly short _token;

        /// <summary>
        /// Creates a task that has already succeeded with <paramref name="result"/> and has no source.
        /// </summary>
        /// <param name="result">The result of the completed task.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UnityTask(T result)
        {
            _source = null;
            _result = result;
            _token = 0;
        }

        /// <summary>
        /// Creates a task that reads <paramref name="source"/> through <paramref name="token"/>.
        /// </summary>
        /// <param name="source">The source that produces the result.</param>
        /// <param name="token">The version of <paramref name="source"/> that this task was created with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UnityTask(IUnityTaskSource<T> source, short token)
        {
            _source = source;
            _result = default;
            _token = token;
        }

        /// <summary>
        /// Gets a value that indicates whether the task has completed and can be awaited on the current thread without
        /// suspending.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Thread:</b> returns <c>false</c> when the task is done but the current thread kind (main or thread pool)
        /// differs from the kind of thread that created it; awaiting it then resumes on the creator's thread kind.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Status</c> (through its awaiter); Unity:
        /// <see cref="UnityEngine.Awaitable.IsCompleted"/>.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend this reads <c>UniTask.Awaiter.IsCompleted</c>; on the
        /// <c>Awaitable</c> backend it reads the EncosyTower task source, never <c>Awaitable.IsCompleted</c>.
        /// </para>
        /// </remarks>
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source == null || _source.GetStatus(_token) != UnityTaskStatus.Pending;
        }

        /// <summary>
        /// Gets the current state of the task.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Thread:</b> returns <see cref="UnityTaskStatus.Pending"/> when the task is done but the current thread
        /// kind (main or thread pool) differs from the kind of thread that created it, as <see cref="IsCompleted"/>
        /// does.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Status</c>; Unity: none, because
        /// <see cref="UnityEngine.Awaitable"/> exposes only <c>IsCompleted</c>.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend this maps <c>UniTask.Status</c> (same values); on the
        /// <c>Awaitable</c> backend it reads the EncosyTower task source.
        /// </para>
        /// </remarks>
        public UnityTaskStatus Status
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source == null ? UnityTaskStatus.Succeeded : _source.GetStatus(_token);
        }

        /// <summary>
        /// Converts a native <see cref="Awaitable{T}"/> to a <see cref="UnityTask{T}"/>.
        /// </summary>
        /// <param name="task">The native task to wrap.</param>
        /// <remarks>
        /// <para>
        /// <b>Interop:</b> a pooled relay observes <paramref name="task"/> once and completes the returned task on the
        /// thread that completes <paramref name="task"/>; the thread-kind rule of the <see cref="UnityTask{T}"/>
        /// behaviour contract does not apply to it.
        /// </para>
        /// <para>
        /// <b>Backends:</b> only the selected backend's conversion exists: from <c>UniTask</c> on the UniTask backend,
        /// from <c>Awaitable</c> on the <c>Awaitable</c> backend.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is <c>null</c>.</exception>
        public static implicit operator UnityTask<T>(Awaitable<T> task)
        {
            DebuggingThrowHelper.ThrowIfNull(task);
            return AwaitableRelaySource<T>.Create(task);
        }

        /// <summary>
        /// Returns an awaiter for this task.
        /// </summary>
        /// <returns>An awaiter for this task.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.GetAwaiter</c>; Unity:
        /// <see cref="UnityEngine.Awaitable.GetAwaiter"/>.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend the awaiter wraps <c>UniTask.Awaiter</c>; on the <c>Awaitable</c>
        /// backend it reads the EncosyTower task source directly.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_source, _result, _token);

        /// <summary>
        /// Returns a native <see cref="Awaitable{T}"/> that completes with the outcome of this task.
        /// </summary>
        /// <returns>A completed awaitable for a task without a source; otherwise a relay that awaits this
        /// task.</returns>
        /// <remarks>
        /// <b>Interop:</b> awaiting the relay consumes this task. The native awaitable follows native
        /// <c>Awaitable</c> behaviour; the <see cref="UnityTask{T}"/> behaviour contract does not apply to operations
        /// performed on it.
        /// </remarks>
        public Awaitable<T> AsAwaitable()
            => _source == null ? Awaitables.GetCompleted(_result) : RelayAsync(this);

        /// <summary>
        /// Returns a task that can be awaited more than once and returns the same outcome each time.
        /// </summary>
        /// <returns>A task that stores the outcome of this task.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> the first await consumes this task; later awaits return the stored result or rethrow the
        /// stored exception instance. Do not await this task directly after calling this method.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting the returned task from two continuations before it completes: may throw
        /// <see cref="InvalidOperationException"/>, because only one continuation can wait on the underlying task.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Preserve</c>; Unity: none.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend this calls <c>UniTask.Preserve</c>; on the <c>Awaitable</c> backend
        /// an EncosyTower memoizing source with the same rules wraps the task.
        /// </para>
        /// </remarks>
        public UnityTask<T> Preserve()
            => _source == null ? this : new(new UnityTaskMemoizeSource<T>(_source), _token);

        /// <summary>
        /// Returns the state of the task in parentheses, such as <c>(Succeeded)</c>, or the result for a task that
        /// completed when it was created.
        /// </summary>
        /// <returns>The state text.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.ToString</c>; Unity: none.
        /// </para>
        /// <para>
        /// <b>Backends:</b> on the UniTask backend this calls <c>UniTask.ToString</c>; on the <c>Awaitable</c> backend
        /// it formats the task source status the same way.
        /// </para>
        /// </remarks>
        public override string ToString()
            => _source == null ? _result?.ToString() : $"({_source.UnsafeGetStatus()})";

        /// <summary>
        /// Awaits <paramref name="task"/> inside an <c>async Awaitable&lt;T&gt;</c> method, so the returned
        /// <c>Awaitable&lt;T&gt;</c> completes with the result of the task.
        /// </summary>
        /// <param name="task">The task to await.</param>
        /// <returns>An <c>Awaitable&lt;T&gt;</c> that completes with the outcome of <paramref name="task"/>.</returns>
        /// <remarks>
        /// Awaiting the returned <c>Awaitable</c> consumes <paramref name="task"/>, as for the non-generic overload.
        /// </remarks>
        private static async Awaitable<T> RelayAsync(UnityTask<T> task)
            => await task;

        /// <summary>
        /// Awaits a <see cref="UnityTask{T}"/>.
        /// </summary>
        /// <remarks>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Calling <see cref="GetResult"/> twice, or awaiting the task or a copy more than once: may throw
        /// <see cref="InvalidOperationException"/>, return a stale result, or observe another operation's result
        /// because sources are pooled.
        /// </description></item>
        /// <item><description>
        /// Calling <see cref="GetResult"/> before completion: may throw or block indefinitely.
        /// </description></item>
        /// </list>
        /// </remarks>
        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly IUnityTaskSource<T> _source;
            private readonly T _result;
            private readonly short _token;

            internal Awaiter(IUnityTaskSource<T> source, T result, short token)
            {
                _source = source;
                _result = result;
                _token = token;
            }

            /// <summary>
            /// Gets a value that indicates whether the task has completed and its result can be read on the current
            /// thread without suspending.
            /// </summary>
            public bool IsCompleted => _source == null || _source.GetStatus(_token) != UnityTaskStatus.Pending;

            /// <summary>
            /// Returns the result; throws the original exception instance when the task faulted or was cancelled.
            /// </summary>
            /// <returns>The result of the task, or <c>default</c> for <c>default(UnityTask&lt;T&gt;)</c>.</returns>
            public T GetResult()
                => _source == null ? _result : _source.GetResult(_token);

            /// <summary>
            /// Schedules <paramref name="continuation"/> to run when the task completes.
            /// </summary>
            /// <param name="continuation">The action to run on completion.</param>
            public void OnCompleted(Action continuation)
            {
                if (_source == null)
                {
                    continuation();
                    return;
                }

                _source.OnCompleted(UnityTaskContinuation.s_invoke, continuation, _token);
            }

            /// <summary>
            /// Schedules <paramref name="continuation"/> to run when the task completes, without flowing the execution
            /// context.
            /// </summary>
            /// <param name="continuation">The action to run on completion.</param>
            public void UnsafeOnCompleted(Action continuation)
                => OnCompleted(continuation);
        }
    }

    /// <summary>
    /// Holds the delegate that task awaiters pass to <see cref="IUnityTaskSource.OnCompleted"/>.
    /// </summary>
    /// <remarks>
    /// <c>s_invoke</c> is a static lambda that casts its state to <see cref="Action"/> and invokes it. The awaiter
    /// passes the continuation as the state, so registering a continuation allocates no closure.
    /// </remarks>
    internal static class UnityTaskContinuation
    {
        internal static readonly Action<object> s_invoke = static state => ((Action)state)();
    }
}

#endif
