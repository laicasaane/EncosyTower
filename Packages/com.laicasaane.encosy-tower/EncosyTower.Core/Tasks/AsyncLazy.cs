using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Runs an asynchronous operation once, on first use, and lets any number of callers await its outcome.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Behaviour:</b> the first read of <see cref="Task"/>, <see cref="GetAwaiter"/> or <see cref="Start"/> starts
    /// the operation. Every read of <see cref="Task"/> returns a new task, so the outcome can be awaited many times and
    /// by several callers at once. A fault is rethrown as the same exception instance to every caller.
    /// </para>
    /// <para>
    /// <b>Thread:</b> each returned task resumes on the kind of thread that read <see cref="Task"/>.
    /// </para>
    /// <para>
    /// The name matches <c>Cysharp.Threading.Tasks.AsyncLazy</c>. A file that imports both namespaces refers to this
    /// type by its full name.
    /// </para>
    /// <para>
    /// <b>Counterparts:</b> UniTask: <c>Cysharp.Threading.Tasks.AsyncLazy</c>; Unity: none.
    /// </para>
    /// </remarks>
    public sealed class AsyncLazy
    {
        private readonly object _lock = new();
        private readonly List<UnityTaskCompletionSource> _waiters = new();
        private Func<UnityTask> _factory;
        private UnityTask _task;
        private ExceptionDispatchInfo _exception;
        private bool _started;
        private bool _completed;

        /// <summary>
        /// Creates a lazy operation that calls <paramref name="factory"/> on first use.
        /// </summary>
        /// <param name="factory">The method that starts the operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
        public AsyncLazy(Func<UnityTask> factory)
        {
            Debugging.ThrowHelper.ThrowIfNull(factory);
            _factory = factory;
        }

        /// <summary>
        /// Creates a lazy operation around a task that already exists, so there is no factory to call.
        /// </summary>
        /// <param name="task">The task whose outcome the lazy operation shares.</param>
        internal AsyncLazy(UnityTask task)
        {
            _task = task;
        }

        /// <summary>
        /// Gets a new task that completes with the outcome of the operation, starting it when needed.
        /// </summary>
        public UnityTask Task
        {
            get
            {
                Start();

                lock (_lock)
                {
                    if (_completed && _exception == null)
                    {
                        return UnityTask.CompletedTask;
                    }

                    if (_completed)
                    {
                        return UnityTask.FromException(_exception.SourceException);
                    }

                    var waiter = new UnityTaskCompletionSource();
                    _waiters.Add(waiter);
                    return waiter.Task;
                }
            }
        }

        /// <summary>
        /// Returns an awaiter for a new <see cref="Task"/>.
        /// </summary>
        /// <returns>An awaiter for the operation.</returns>
        public UnityTask.Awaiter GetAwaiter()
            => Task.GetAwaiter();

        /// <summary>
        /// Starts the operation if it has not started yet.
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_started)
                {
                    return;
                }

                _started = true;
            }

            RunAsync().Forget();
        }

        /// <summary>
        /// Runs the operation, then completes every waiter with its outcome.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Called once, by the first <see cref="Start"/>. It awaits the task from the factory, or the stored task
        /// for an instance created from a task, and keeps a fault as an <see cref="ExceptionDispatchInfo"/>.
        /// </para>
        /// <para>
        /// The outcome and the waiter list are published under the lock, and the waiters are completed after it is
        /// released. A <see cref="Task"/> read before that point adds a waiter; a read after it returns a
        /// completed task.
        /// </para>
        /// </remarks>
        private async UnityTaskVoid RunAsync()
        {
            Exception exception = null;

            try
            {
                var factory = _factory;
                _factory = null;

                await (factory == null ? _task : factory());
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            UnityTaskCompletionSource[] waiters;

            lock (_lock)
            {
                _exception = exception == null ? null : ExceptionDispatchInfo.Capture(exception);
                _completed = true;
                _task = default;
                waiters = _waiters.ToArray();
                _waiters.Clear();
            }

            var count = waiters.Length;

            for (var i = 0; i < count; i++)
            {
                if (exception == null)
                {
                    waiters[i].TrySetResult();
                }
                else
                {
                    waiters[i].TrySetException(exception);
                }
            }
        }
    }
}
