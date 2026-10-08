#if !UNITASK || ENCOSY_UNITYTASK_AWAITABLE

using System;
using System.Runtime.ExceptionServices;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Task source that lets a <see cref="UnityTask"/> be awaited more than once by caching the outcome of the inner
    /// source.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Until the first <c>GetResult</c>, every member forwards to the inner source with the original token. Only one
    /// continuation can wait before completion, so no extra state is needed for that period.
    /// </para>
    /// <para>
    /// The first <c>GetResult</c> stores the outcome and the status, then drops the inner source, which returns to its
    /// pool. Later reads are served from the stored outcome and do not touch the inner source. This mirrors
    /// UniTask's <c>MemoizeSource</c>.
    /// </para>
    /// </remarks>
    internal sealed class UnityTaskMemoizeSource : IUnityTaskSource
    {
        private IUnityTaskSource _source;
        private ExceptionDispatchInfo _exception;
        private UnityTaskStatus _status;

        internal UnityTaskMemoizeSource(IUnityTaskSource source)
        {
            _source = source;
        }

        public UnityTaskStatus GetStatus(short token)
            => _source == null ? _status : _source.GetStatus(token);

        public UnityTaskStatus UnsafeGetStatus()
            => _source == null ? _status : _source.UnsafeGetStatus();

        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            if (_source == null)
            {
                // After memoization the task is already complete, so the continuation runs at once.
                continuation(state);
            }
            else
            {
                _source.OnCompleted(continuation, state, token);
            }
        }

        /// <summary>
        /// Reads the inner source on the first call and caches the outcome; later calls replay the cached outcome.
        /// </summary>
        public void GetResult(short token)
        {
            if (_source == null)
            {
                _exception?.Throw();
                return;
            }

            try
            {
                _source.GetResult(token);
                _status = UnityTaskStatus.Succeeded;
            }
            catch (Exception exception)
            {
                _exception = ExceptionDispatchInfo.Capture(exception);
                _status = exception is OperationCanceledException ? UnityTaskStatus.Canceled : UnityTaskStatus.Faulted;

                throw;
            }
            finally
            {
                // The inner source has returned to its pool in its own GetResult, so it must not be used again.
                _source = null;
            }
        }
    }
}

#endif
