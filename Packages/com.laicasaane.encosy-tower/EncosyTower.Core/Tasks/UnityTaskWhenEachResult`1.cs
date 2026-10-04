using System;
using System.Runtime.ExceptionServices;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Holds the outcome of one input of <c>UnityTask.WhenEach</c>: either its result or the exception it threw.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    public readonly struct UnityTaskWhenEachResult<T>
    {
        private readonly T _result;
        private readonly Exception _exception;

        /// <summary>
        /// Creates a successful outcome that holds <paramref name="result"/>.
        /// </summary>
        /// <param name="result">The result of the input.</param>
        public UnityTaskWhenEachResult(T result)
        {
            _result = result;
            _exception = null;
        }

        /// <summary>
        /// Creates a failed outcome that holds <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The exception that the input threw.</param>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
        public UnityTaskWhenEachResult(Exception exception)
        {
            Debugging.ThrowHelper.ThrowIfNull(exception);
            _result = default;
            _exception = exception;
        }

        /// <summary>
        /// Gets the result of the input, or <c>default</c> when the input failed.
        /// </summary>
        public T Result => _result;

        /// <summary>
        /// Gets the exception that the input threw, or <c>null</c> when the input succeeded.
        /// </summary>
        public Exception Exception => _exception;

        /// <summary>
        /// Gets a value that indicates whether the input succeeded.
        /// </summary>
        public bool IsCompletedSuccessfully => _exception == null;

        /// <summary>
        /// Gets a value that indicates whether the input failed, including cancellation.
        /// </summary>
        public bool IsFaulted => _exception != null;

        /// <summary>
        /// Throws the original exception instance with its original stack when the input failed.
        /// </summary>
        public void TryThrow()
        {
            if (_exception != null)
            {
                ExceptionDispatchInfo.Capture(_exception).Throw();
            }
        }

        /// <summary>
        /// Returns the result, or throws the original exception instance with its original stack when the input
        /// failed.
        /// </summary>
        /// <returns>The result of the input.</returns>
        public T GetResult()
        {
            if (_exception != null)
            {
                ExceptionDispatchInfo.Capture(_exception).Throw();
            }

            return _result;
        }

        /// <summary>
        /// Returns the result as text, or the exception message in the form <c>Exception{message}</c>.
        /// </summary>
        /// <returns>The text form of this outcome.</returns>
        public override string ToString()
            => _exception == null
                ? _result?.ToString() ?? string.Empty
                : $"Exception{{{_exception.Message}}}";
    }
}
