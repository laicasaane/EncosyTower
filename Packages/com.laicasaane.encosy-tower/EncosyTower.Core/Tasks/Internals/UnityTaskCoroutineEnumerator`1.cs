using System;
using System.Collections;
using System.Runtime.ExceptionServices;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Enumerator that lets a Unity coroutine wait for a <see cref="UnityTask{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <remarks>
    /// <para>
    /// The first <see cref="MoveNext"/> starts <c>RunAsync</c>, an <c>async UnityTaskVoid</c> method that awaits the
    /// task and then sets the completed flag. <see cref="MoveNext"/> returns <c>true</c> until that flag is set.
    /// </para>
    /// <para>
    /// The result goes to the result handler when there is one. If the task throws, the exception goes to the exception
    /// handler when there is one. Otherwise it is stored and rethrown from <see cref="MoveNext"/>.
    /// </para>
    /// </remarks>
    internal sealed class UnityTaskCoroutineEnumerator<T> : IEnumerator
    {
        private readonly UnityTask<T> _task;
        private readonly Action<T> _resultHandler;
        private readonly Action<Exception> _exceptionHandler;
        private ExceptionDispatchInfo _exception;
        private bool _started;
        private bool _completed;

        internal UnityTaskCoroutineEnumerator(
              UnityTask<T> task
            , Action<T> resultHandler
            , Action<Exception> exceptionHandler
        )
        {
            _task = task;
            _resultHandler = resultHandler;
            _exceptionHandler = exceptionHandler;
        }

        public object Current => null;

        public bool MoveNext()
        {
            if (_started == false)
            {
                _started = true;
                RunAsync().Forget();
            }

            _exception?.Throw();
            return _completed == false;
        }

        public void Reset()
        {
        }

        /// <summary>
        /// Awaits the task, passes its result to the result handler, routes a failure to the exception handler or to
        /// the stored exception, and sets the completed flag.
        /// </summary>
        private async UnityTaskVoid RunAsync()
        {
            try
            {
                var result = await _task;
                _resultHandler?.Invoke(result);
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
