#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE

using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        private readonly UniTask _task;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UnityTask(UniTask task)
            => _task = task;

        /// <summary>
        /// Gets a value that indicates whether the task has completed and can be awaited on the current thread without
        /// suspending.
        /// </summary>
        /// <remarks>
        /// <b>Thread:</b> returns <c>false</c> when the task is done but the current thread kind (main or thread pool)
        /// differs from the kind of thread that created it; awaiting it then resumes on the creator's thread kind.
        /// </remarks>
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task.GetAwaiter().IsCompleted;
        }

        /// <summary>
        /// Converts a native <see cref="UniTask"/> to a <see cref="UnityTask"/>.
        /// </summary>
        /// <param name="task">The native task to wrap.</param>
        /// <remarks>
        /// <b>Interop:</b> a task converted from <see cref="UniTask"/> follows native UniTask behaviour; the
        /// <see cref="UnityTask"/> behaviour contract does not apply to it.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask(UniTask task)
            => new(task);

        /// <summary>
        /// Returns an awaiter for this task.
        /// </summary>
        /// <returns>An awaiter for this task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task.GetAwaiter());

        /// <summary>
        /// Returns the native <see cref="UniTask"/> that backs this task.
        /// </summary>
        /// <returns>The backing native task.</returns>
        /// <remarks>
        /// <b>Interop:</b> the native task follows native UniTask behaviour; the <see cref="UnityTask"/> behaviour
        /// contract does not apply to operations performed on it.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask AsUniTask()
            => _task;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UnityTask<T> CreateFromResult<T>(T value)
            => new(UniTask.FromResult(value));

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
            private readonly UniTask.Awaiter _awaiter;

            internal Awaiter(UniTask.Awaiter awaiter)
                => _awaiter = awaiter;

            /// <summary>
            /// Gets a value that indicates whether the task has completed and its result can be read on the current
            /// thread without suspending.
            /// </summary>
            public bool IsCompleted => _awaiter.IsCompleted;

            /// <summary>
            /// Ends the wait; throws the original exception instance when the task faulted or was cancelled.
            /// </summary>
            public void GetResult()
                => _awaiter.GetResult();

            /// <summary>
            /// Schedules <paramref name="continuation"/> to run when the task completes.
            /// </summary>
            /// <param name="continuation">The action to run on completion.</param>
            public void OnCompleted(Action continuation)
                => _awaiter.OnCompleted(continuation);

            /// <summary>
            /// Schedules <paramref name="continuation"/> to run when the task completes, without flowing the execution
            /// context.
            /// </summary>
            /// <param name="continuation">The action to run on completion.</param>
            public void UnsafeOnCompleted(Action continuation)
                => _awaiter.UnsafeOnCompleted(continuation);
        }
    }

    public readonly partial struct UnityTask<T>
    {
        private readonly UniTask<T> _task;

        internal UnityTask(UniTask<T> task)
            => _task = task;

        /// <summary>
        /// Gets a value that indicates whether the task has completed and can be awaited on the current thread without
        /// suspending.
        /// </summary>
        /// <remarks>
        /// <b>Thread:</b> returns <c>false</c> when the task is done but the current thread kind (main or thread pool)
        /// differs from the kind of thread that created it; awaiting it then resumes on the creator's thread kind.
        /// </remarks>
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task.GetAwaiter().IsCompleted;
        }

        /// <summary>
        /// Converts a native <see cref="UniTask{T}"/> to a <see cref="UnityTask{T}"/>.
        /// </summary>
        /// <param name="task">The native task to wrap.</param>
        /// <remarks>
        /// <b>Interop:</b> a task converted from <see cref="UniTask{T}"/> follows native UniTask behaviour; the
        /// <see cref="UnityTask{T}"/> behaviour contract does not apply to it.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask<T>(UniTask<T> task)
            => new(task);

        /// <summary>
        /// Returns an awaiter for this task.
        /// </summary>
        /// <returns>An awaiter for this task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task.GetAwaiter());

        /// <summary>
        /// Returns the native <see cref="UniTask{T}"/> that backs this task.
        /// </summary>
        /// <returns>The backing native task.</returns>
        /// <remarks>
        /// <b>Interop:</b> the native task follows native UniTask behaviour; the <see cref="UnityTask{T}"/> behaviour
        /// contract does not apply to operations performed on it.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<T> AsUniTask()
            => _task;

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
            private readonly UniTask<T>.Awaiter _awaiter;

            internal Awaiter(UniTask<T>.Awaiter awaiter)
                => _awaiter = awaiter;

            /// <summary>
            /// Gets a value that indicates whether the task has completed and its result can be read on the current
            /// thread without suspending.
            /// </summary>
            public bool IsCompleted => _awaiter.IsCompleted;

            /// <summary>
            /// Returns the result; throws the original exception instance when the task faulted or was cancelled.
            /// </summary>
            /// <returns>The result of the task.</returns>
            public T GetResult()
                => _awaiter.GetResult();

            /// <summary>
            /// Schedules <paramref name="continuation"/> to run when the task completes.
            /// </summary>
            /// <param name="continuation">The action to run on completion.</param>
            public void OnCompleted(Action continuation)
                => _awaiter.OnCompleted(continuation);

            /// <summary>
            /// Schedules <paramref name="continuation"/> to run when the task completes, without flowing the execution
            /// context.
            /// </summary>
            /// <param name="continuation">The action to run on completion.</param>
            public void UnsafeOnCompleted(Action continuation)
                => _awaiter.UnsafeOnCompleted(continuation);
        }
    }
}

#endif
