#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE

using System;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Tasks
{
    public sealed partial class UnityTaskCompletionSource<T> : IUniTaskSource<T>
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
        public UnityTask<T> Task => new(new UniTask<T>(this, _core.Version));

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

        internal void Reset()
        {
            _core.Reset();
            _core.Prepare(UnityTaskThreadContext.CurrentAffinity);
        }

        UniTaskStatus IUniTaskSource.GetStatus(short token)
            => _core.GetStatus(token);

        UniTaskStatus IUniTaskSource.UnsafeGetStatus()
            => _core.UnsafeGetStatus();

        void IUniTaskSource.OnCompleted(Action<object> continuation, object state, short token)
            => _core.OnCompleted(continuation, state, token);

        T IUniTaskSource<T>.GetResult(short token)
            => _core.GetResult(token);

        void IUniTaskSource.GetResult(short token)
            => _core.GetResult(token);
    }
}

#endif
