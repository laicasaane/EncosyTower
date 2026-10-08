#if !UNITASK || ENCOSY_UNITYTASK_AWAITABLE

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Pooled task source that completes a <see cref="UnityTask{T}"/> when an <see cref="Awaitable{T}"/> completes.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <remarks>
    /// <para>
    /// <see cref="Create"/> registers the relay's single continuation on the awaitable's awaiter, because an
    /// <see cref="Awaitable"/> accepts only one continuation. When the awaitable completes, <c>Complete</c> copies the
    /// outcome into the core. The core is prepared with <see cref="UnityTaskThreadAffinity.None"/>, so the task
    /// completes on the thread that completed the awaitable.
    /// </para>
    /// <para>
    /// <c>GetResult</c> validates the token, reads the outcome and then returns the relay to the pool.
    /// </para>
    /// </remarks>
    internal sealed class AwaitableRelaySource<T> : IUnityTaskSource<T>
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<AwaitableRelaySource<T>> s_pool = new();

        private readonly Action _complete;
        private UnityTaskSourceCore<T> _core;
        private Awaitable<T> _awaitable;

        private AwaitableRelaySource()
        {
            _complete = Complete;
        }

        /// <summary>
        /// Rents a relay, registers its continuation on <paramref name="awaitable"/> and returns the task that follows
        /// it.
        /// </summary>
        /// <remarks>
        /// If the awaitable is already complete, the relay completes at once instead of registering a continuation.
        /// </remarks>
        internal static UnityTask<T> Create(Awaitable<T> awaitable)
        {
            AwaitableRelaySource<T> source;

            lock (s_poolLock)
            {
                source = s_pool.Count > 0 ? s_pool.Pop() : new();
            }

            source._core.Prepare(UnityTaskThreadAffinity.None);
            source._awaitable = awaitable;

            var token = source._core.Version;
            var awaiter = awaitable.GetAwaiter();

            if (awaiter.IsCompleted)
            {
                source.Complete();
            }
            else
            {
                awaiter.OnCompleted(source._complete);
            }

            return UnityTask.FromSource(source, token);
        }

        public UnityTaskStatus GetStatus(short token)
            => _core.GetStatus(token);

        public UnityTaskStatus UnsafeGetStatus()
            => _core.UnsafeGetStatus();

        public void OnCompleted(Action<object> continuation, object state, short token)
            => _core.OnCompleted(continuation, state, token);

        public T GetResult(short token)
        {
            _core.ValidateResultAccess(token);

            try
            {
                return _core.GetResult(token);
            }
            finally
            {
                // Return runs even when the outcome is a throw, so the relay always goes back to the pool.
                Return();
            }
        }

        void IUnityTaskSource.GetResult(short token)
            => GetResult(token);

        /// <summary>
        /// Reads the outcome of the awaitable and sets it on the core. An exception thrown by the awaitable faults the
        /// task.
        /// </summary>
        private void Complete()
        {
            var awaitable = _awaitable;
            _awaitable = null;

            T result;

            try
            {
                result = awaitable.GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                _core.TrySetException(exception);
                return;
            }

            _core.TrySetResult(result);
        }

        /// <summary>
        /// Resets the core and puts the relay back in the pool, unless the pool already holds <c>MAX_POOL_SIZE</c>
        /// relays.
        /// </summary>
        private void Return()
        {
            _core.Reset();

            lock (s_poolLock)
            {
                if (s_pool.Count < MAX_POOL_SIZE)
                {
                    s_pool.Push(this);
                }
            }
        }
    }
}

#endif
