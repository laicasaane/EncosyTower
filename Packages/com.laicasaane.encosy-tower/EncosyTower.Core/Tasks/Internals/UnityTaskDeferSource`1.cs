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

    /// <summary>
    /// Task source that delays calling a <see cref="UnityTask{T}"/> factory until the task is first observed.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <remarks>
    /// <para>
    /// The factory runs once, on the first <c>GetStatus</c>, <c>OnCompleted</c> or <c>GetResult</c>. The source is not
    /// pooled, so the token is always 0 and is not checked. <c>UnsafeGetStatus</c> does not run the factory and reports
    /// <see cref="UnityTaskStatus.Pending"/> until it has run.
    /// </para>
    /// <para>
    /// The continuation is stored and called through the cached <c>_invoke</c> delegate once the created task
    /// completes.
    /// </para>
    /// </remarks>
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
    internal sealed class UnityTaskDeferSource<T> : Cysharp.Threading.Tasks.IUniTaskSource<T>
#else
    internal sealed class UnityTaskDeferSource<T> : IUnityTaskSource<T>
#endif
    {
        private readonly Action _invoke;
        private Func<UnityTask<T>> _factory;
        private UnityTask<T> _task;
        private Action<object> _continuation;
        private object _state;

        internal UnityTaskDeferSource(Func<UnityTask<T>> factory)
        {
            _factory = factory;
            _invoke = Invoke;
        }

        public TaskSourceStatus GetStatus(short token)
            => (TaskSourceStatus)Start().Status;

        public TaskSourceStatus UnsafeGetStatus()
            => _factory == null ? (TaskSourceStatus)_task.Status : (TaskSourceStatus)UnityTaskStatus.Pending;

        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            _continuation = continuation;
            _state = state;
            Start().GetAwaiter().UnsafeOnCompleted(_invoke);
        }

        public T GetResult(short token)
            => Start().GetAwaiter().GetResult();

        void ITaskSource.GetResult(short token)
            => GetResult(token);

        /// <summary>
        /// Calls the factory the first time and caches the task it returns; later calls return the cached task.
        /// </summary>
        private UnityTask<T> Start()
        {
            var factory = _factory;

            if (factory != null)
            {
                // Clear the factory before calling it so it runs only once.
                _factory = null;
                _task = factory();
            }

            return _task;
        }

        /// <summary>
        /// Clears the stored continuation and state, then calls the continuation.
        /// </summary>
        private void Invoke()
        {
            var continuation = _continuation;
            var state = _state;

            _continuation = null;
            _state = null;
            continuation(state);
        }
    }
}
