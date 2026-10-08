using System;
using System.Collections;
using System.Runtime.ExceptionServices;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Enumerator that lets a Unity coroutine wait for a <see cref="UnityTask"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The first <see cref="MoveNext"/> starts <c>RunAsync</c>, an <c>async UnityTaskVoid</c> method that awaits the
    /// task and then sets the completed flag. <see cref="MoveNext"/> returns <c>true</c> until that flag is set.
    /// </para>
    /// <para>
    /// If the task throws, the exception goes to the exception handler when there is one. Otherwise it is stored and
    /// rethrown from <see cref="MoveNext"/>. When a task factory was given instead of a task, the first
    /// <see cref="MoveNext"/> calls it.
    /// </para>
    /// </remarks>
    internal sealed class UnityTaskCoroutineEnumerator : IEnumerator
    {
        private readonly Action<Exception> _exceptionHandler;
        private UnityTask _task;
        private Func<UnityTask> _taskFactory;
        private ExceptionDispatchInfo _exception;
        private bool _started;
        private bool _completed;

        internal UnityTaskCoroutineEnumerator(UnityTask task, Action<Exception> exceptionHandler)
        {
            _task = task;
            _exceptionHandler = exceptionHandler;
        }

        internal UnityTaskCoroutineEnumerator(Func<UnityTask> taskFactory)
        {
            _taskFactory = taskFactory;
        }

        public object Current => null;

        public bool MoveNext()
        {
            if (_started == false)
            {
                _started = true;

                if (_taskFactory != null)
                {
                    _task = _taskFactory();
                    _taskFactory = null;
                }

                RunAsync().Forget();
            }

            _exception?.Throw();
            return _completed == false;
        }

        public void Reset()
        {
        }

        /// <summary>
        /// Awaits the task, routes a failure to the exception handler or to the stored exception, and sets the
        /// completed flag.
        /// </summary>
        private async UnityTaskVoid RunAsync()
        {
            try
            {
                await _task;
            }
            catch (Exception exception)
            {
                if (_exceptionHandler == null)
                {
                    _exception = ExceptionDispatchInfo.Capture(exception);
                }
                else
                {
                    _exceptionHandler(exception);
                }
            }
            finally
            {
                _completed = true;
            }
        }
    }
}
