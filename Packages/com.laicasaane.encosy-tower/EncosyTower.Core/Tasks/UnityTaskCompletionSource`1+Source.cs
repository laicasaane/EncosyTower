using System;

namespace EncosyTower.Tasks
{
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
    using ITaskSource = Cysharp.Threading.Tasks.IUniTaskSource;
    using TaskSourceStatus = Cysharp.Threading.Tasks.UniTaskStatus;
#else
    using ITaskSource = IUnityTaskSource;
    using TaskSourceStatus = UnityTaskStatus;
#endif

#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
    public sealed partial class UnityTaskCompletionSource<T> : Cysharp.Threading.Tasks.IUniTaskSource<T>
#else
    public sealed partial class UnityTaskCompletionSource<T> : IUnityTaskSource<T>
#endif
    {
        private UnityTaskSourceCore<T> _core;

        /// <summary>
        /// Creates a pending source whose <see cref="Task"/> resumes on the kind of thread that calls this
        /// constructor.
        /// </summary>
        public UnityTaskCompletionSource()
        {
            _core.Prepare(UnityTaskThreadContext.CurrentAffinity);
        }

        /// <summary>
        /// Gets the task that this source completes.
        /// </summary>
        public UnityTask<T> Task => UnityTask.FromSource<T>(this, _core.Version);

        /// <summary>
        /// Creates a pending source that continues awaiters inline, whatever kind of thread completes it.
        /// </summary>
        /// <returns>A pending source with <see cref="UnityTaskThreadAffinity.None"/>.</returns>
        /// <remarks>
        /// Used for the completion sources that back factory tasks. Affinity <c>None</c> disables the thread-kind check
        /// in <see cref="IUnityTaskSource.GetStatus"/>, so a completed task continues inline on the completing thread
        /// instead of being moved to the thread kind that created it.
        /// </remarks>
        internal static UnityTaskCompletionSource<T> CreateInline()
        {
            var source = new UnityTaskCompletionSource<T>();
            source._core.Prepare(UnityTaskThreadAffinity.None);
            return source;
        }

        /// <summary>
        /// Attempts to complete <see cref="Task"/> successfully with <paramref name="result"/>.
        /// </summary>
        /// <param name="result">The result that awaiting <see cref="Task"/> returns.</param>
        /// <returns><c>true</c> if this call completed the source; <c>false</c> if it was already completed.</returns>
        public bool TrySetResult(T result)
            => _core.TrySetResult(result);

        /// <summary>
        /// Attempts to complete <see cref="Task"/> with <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The exception that awaiting <see cref="Task"/> rethrows unchanged.</param>
        /// <returns><c>true</c> if this call completed the source; <c>false</c> if it was already completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
        public bool TrySetException(Exception exception)
        {
            Debugging.ThrowHelper.ThrowIfNull(exception);
            return _core.TrySetException(exception);
        }

        /// <summary>
        /// Returns the state of <see cref="Task"/> without checking the current thread kind.
        /// </summary>
        /// <returns>The current state of the source.</returns>
        public UnityTaskStatus UnsafeGetStatus()
            => _core.UnsafeGetStatus();

        /// <summary>
        /// Returns the source to the pending state so that it can be completed again.
        /// </summary>
        /// <remarks>
        /// <see cref="UnityTaskSourceCore{T}.Reset"/> advances the version, so a task created from an earlier version
        /// no longer matches. The source then captures the thread kind of the calling thread again, as the constructor
        /// does.
        /// </remarks>
        internal void Reset()
        {
            _core.Reset();
            _core.Prepare(UnityTaskThreadContext.CurrentAffinity);
        }

        TaskSourceStatus ITaskSource.GetStatus(short token)
            => (TaskSourceStatus)_core.GetStatus(token);

        TaskSourceStatus ITaskSource.UnsafeGetStatus()
            => (TaskSourceStatus)_core.UnsafeGetStatus();

        void ITaskSource.OnCompleted(Action<object> continuation, object state, short token)
            => _core.OnCompleted(continuation, state, token);

#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
        T Cysharp.Threading.Tasks.IUniTaskSource<T>.GetResult(short token)
            => _core.GetResult(token);
#else
        T IUnityTaskSource<T>.GetResult(short token)
            => _core.GetResult(token);
#endif

        void ITaskSource.GetResult(short token)
            => _core.GetResult(token);
    }
}
