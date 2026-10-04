#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE

using System;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Tasks
{
    public sealed partial class UnityTaskCompletionSource : IUniTaskSource
    {
        private UnityTaskSourceCore<object> _core;

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
        public UnityTask Task => new(new UniTask(this, _core.Version));

        /// <summary>
        /// Attempts to complete <see cref="Task"/> successfully.
        /// </summary>
        /// <returns><c>true</c> if this call completed the source; <c>false</c> if it was already completed.</returns>
        public bool TrySetResult()
            => _core.TrySetResult(null);

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

        internal static UnityTaskCompletionSource CreateInline()
        {
            var source = new UnityTaskCompletionSource();
            source._core.Prepare(UnityTaskThreadAffinity.None);
            return source;
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

        void IUniTaskSource.GetResult(short token)
            => _core.GetResult(token);
    }
}

#endif
