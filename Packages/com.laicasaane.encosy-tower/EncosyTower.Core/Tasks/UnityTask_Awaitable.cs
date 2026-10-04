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
        private readonly Awaitable _task;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UnityTask(Awaitable task)
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
            get => _task == null || _task.GetAwaiter().IsCompleted;
        }

        /// <summary>
        /// Converts a native <see cref="Awaitable"/> to a <see cref="UnityTask"/>.
        /// </summary>
        /// <param name="task">The native task to wrap.</param>
        /// <remarks>
        /// <b>Interop:</b> a task converted from <see cref="Awaitable"/> follows native <c>Awaitable</c> behaviour;
        /// the <see cref="UnityTask"/> behaviour contract does not apply to it.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask(Awaitable task)
        {
            DebuggingThrowHelper.ThrowIfNull(task);
            return new(task);
        }

        /// <summary>
        /// Returns an awaiter for this task.
        /// </summary>
        /// <returns>An awaiter for this task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task);

        /// <summary>
        /// Returns the native <see cref="Awaitable"/> that backs this task, or a completed awaitable for
        /// <c>default(UnityTask)</c>.
        /// </summary>
        /// <returns>The backing native task.</returns>
        /// <remarks>
        /// <b>Interop:</b> the native task follows native <c>Awaitable</c> behaviour; the <see cref="UnityTask"/>
        /// behaviour contract does not apply to operations performed on it.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable AsAwaitable()
            => _task ?? Awaitables.GetCompleted();

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
            private readonly Awaitable _task;

            internal Awaiter(Awaitable task)
                => _task = task;

            /// <summary>
            /// Gets a value that indicates whether the task has completed and its result can be read on the current
            /// thread without suspending.
            /// </summary>
            public bool IsCompleted => _task == null || _task.GetAwaiter().IsCompleted;

            /// <summary>
            /// Ends the wait; throws the original exception instance when the task faulted or was cancelled.
            /// </summary>
            public void GetResult()
            {
                if (_task != null)
                {
                    _task.GetAwaiter().GetResult();
                }
            }

            /// <summary>
            /// Schedules <paramref name="continuation"/> to run when the task completes.
            /// </summary>
            /// <param name="continuation">The action to run on completion.</param>
            public void OnCompleted(Action continuation)
            {
                if (_task == null)
                {
                    continuation();
                    return;
                }

                _task.GetAwaiter().OnCompleted(continuation);
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
        private readonly Awaitable<T> _task;

        internal UnityTask(Awaitable<T> task)
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
            get => _task == null || _task.GetAwaiter().IsCompleted;
        }

        /// <summary>
        /// Converts a native <see cref="Awaitable{T}"/> to a <see cref="UnityTask{T}"/>.
        /// </summary>
        /// <param name="task">The native task to wrap.</param>
        /// <remarks>
        /// <b>Interop:</b> a task converted from <see cref="Awaitable{T}"/> follows native <c>Awaitable</c>
        /// behaviour; the <see cref="UnityTask{T}"/> behaviour contract does not apply to it.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask<T>(Awaitable<T> task)
        {
            DebuggingThrowHelper.ThrowIfNull(task);
            return new(task);
        }

        /// <summary>
        /// Returns an awaiter for this task.
        /// </summary>
        /// <returns>An awaiter for this task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task);

        /// <summary>
        /// Returns the native <see cref="Awaitable{T}"/> that backs this task, or a completed awaitable for
        /// <c>default(UnityTask&lt;T&gt;)</c>.
        /// </summary>
        /// <returns>The backing native task.</returns>
        /// <remarks>
        /// <b>Interop:</b> the native task follows native <c>Awaitable</c> behaviour; the <see cref="UnityTask{T}"/>
        /// behaviour contract does not apply to operations performed on it.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<T> AsAwaitable()
            => _task ?? Awaitables.GetCompleted<T>();

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
            private readonly Awaitable<T> _task;

            internal Awaiter(Awaitable<T> task)
                => _task = task;

            /// <summary>
            /// Gets a value that indicates whether the task has completed and its result can be read on the current
            /// thread without suspending.
            /// </summary>
            public bool IsCompleted => _task == null || _task.GetAwaiter().IsCompleted;

            /// <summary>
            /// Returns the result; throws the original exception instance when the task faulted or was cancelled.
            /// </summary>
            /// <returns>The result of the task, or <c>default</c> for <c>default(UnityTask&lt;T&gt;)</c>.</returns>
            public T GetResult()
                => _task == null ? default : _task.GetAwaiter().GetResult();

            /// <summary>
            /// Schedules <paramref name="continuation"/> to run when the task completes.
            /// </summary>
            /// <param name="continuation">The action to run on completion.</param>
            public void OnCompleted(Action continuation)
            {
                if (_task == null)
                {
                    continuation();
                    return;
                }

                _task.GetAwaiter().OnCompleted(continuation);
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
}

#endif
