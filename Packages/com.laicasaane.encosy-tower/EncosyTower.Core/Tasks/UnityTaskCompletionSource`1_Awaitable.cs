#if !UNITASK || ENCOSY_UNITYTASK_AWAITABLE

using System;
using UnityEngine;

namespace EncosyTower.Tasks
{
    public sealed partial class UnityTaskCompletionSource<T>
    {
        private readonly AwaitableCompletionSource<T> _source = new();
        private UnityTask<T> _task;

        /// <summary>
        /// Creates a pending source whose <see cref="Task"/> resumes on the kind of thread that calls this
        /// constructor.
        /// </summary>
        public UnityTaskCompletionSource()
            : this(relay: true)
        {
        }

        private UnityTaskCompletionSource(bool relay)
        {
            _task = relay ? new(RelayAsync(_source.Awaitable)) : new(_source.Awaitable);
        }

        /// <summary>
        /// Gets the task that this source completes.
        /// </summary>
        public UnityTask<T> Task => _task;

        /// <summary>
        /// Attempts to complete <see cref="Task"/> successfully with <paramref name="result"/>.
        /// </summary>
        /// <param name="result">The result that awaiting <see cref="Task"/> returns.</param>
        /// <returns><c>true</c> if this call completed the source; <c>false</c> if it was already completed.</returns>
        public bool TrySetResult(T result)
            => _source.TrySetResult(result);

        /// <summary>
        /// Attempts to complete <see cref="Task"/> with <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The exception that awaiting <see cref="Task"/> rethrows unchanged.</param>
        /// <returns><c>true</c> if this call completed the source; <c>false</c> if it was already completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
        public bool TrySetException(Exception exception)
        {
            Debugging.ThrowHelper.ThrowIfNull(exception);
            return _source.TrySetException(exception);
        }

        internal static UnityTaskCompletionSource<T> CreateInline()
            => new(relay: false);

        internal void Reset()
        {
            _source.Reset();
            _task = new(RelayAsync(_source.Awaitable));
        }

        private static async Awaitable<T> RelayAsync(Awaitable<T> inner)
            => await inner;
    }
}

#endif
