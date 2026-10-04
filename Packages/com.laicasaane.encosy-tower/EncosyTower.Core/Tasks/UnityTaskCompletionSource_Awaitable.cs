#if !UNITASK || ENCOSY_UNITYTASK_AWAITABLE

using System;
using UnityEngine;

namespace EncosyTower.Tasks
{
    public sealed partial class UnityTaskCompletionSource
    {
        private readonly AwaitableCompletionSource _source = new();
        private UnityTask _task;

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
        public UnityTask Task => _task;

        /// <summary>
        /// Attempts to complete <see cref="Task"/> successfully.
        /// </summary>
        /// <returns><c>true</c> if this call completed the source; <c>false</c> if it was already completed.</returns>
        public bool TrySetResult()
            => _source.TrySetResult();

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

        internal static UnityTaskCompletionSource CreateInline()
            => new(relay: false);

        private static async Awaitable RelayAsync(Awaitable inner)
        {
            await inner;
        }

        internal void Reset()
        {
            _source.Reset();
            _task = new(RelayAsync(_source.Awaitable));
        }
    }
}

#endif
