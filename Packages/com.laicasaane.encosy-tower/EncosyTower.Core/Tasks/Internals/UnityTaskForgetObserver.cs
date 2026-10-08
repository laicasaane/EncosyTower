using System;
using System.Collections.Generic;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Pooled observer behind <c>Forget()</c> for a <see cref="UnityTask"/>: it awaits the task once, so the pooled
    /// source is read and returned, and logs a fault that nobody else observes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Observe</c> completes inline when the task is already complete. Otherwise it registers the cached
    /// <c>_continuation</c> delegate with <c>UnsafeOnCompleted</c>, so no closure is allocated.
    /// </para>
    /// <para>
    /// The continuation runs on the thread that completes the task. An <see cref="OperationCanceledException"/> is
    /// ignored; any other exception goes to <c>ThrowHelper.LogUnobservedException</c>. The observer returns itself to
    /// the pool in a <c>finally</c> block.
    /// </para>
    /// </remarks>
    internal sealed class UnityTaskForgetObserver
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<UnityTaskForgetObserver> s_pool = new(MAX_POOL_SIZE);

        private readonly Action _continuation;
        private UnityTask.Awaiter _awaiter;

        private UnityTaskForgetObserver()
        {
            _continuation = Continue;
        }

        /// <summary>
        /// Starts observing <paramref name="task"/>; completes inline when it is already complete, else registers the
        /// cached continuation.
        /// </summary>
        internal static void Observe(UnityTask task)
        {
            UnityTaskForgetObserver observer;

            lock (s_poolLock)
            {
                observer = s_pool.Count == 0 ? new() : s_pool.Pop();
            }

            observer._awaiter = task.GetAwaiter();

            if (observer._awaiter.IsCompleted)
            {
                observer.Continue();
            }
            else
            {
                observer._awaiter.UnsafeOnCompleted(observer._continuation);
            }
        }

        /// <summary>
        /// Reads the outcome, logs a fault other than <see cref="OperationCanceledException"/> and returns the
        /// observer to the pool.
        /// </summary>
        private void Continue()
        {
            try
            {
                _awaiter.GetResult();
            }
            catch (Exception exception)
            {
                if (exception is OperationCanceledException == false)
                {
                    ThrowHelper.LogUnobservedException(exception);
                }
            }
            finally
            {
                _awaiter = default;

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

    /// <summary>
    /// Pooled observer behind <c>Forget()</c> for a <see cref="UnityTask{T}"/>; the result is read and discarded.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <remarks>
    /// Works as <see cref="UnityTaskForgetObserver"/> does.
    /// </remarks>
    internal sealed class UnityTaskForgetObserver<T>
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<UnityTaskForgetObserver<T>> s_pool = new(MAX_POOL_SIZE);

        private readonly Action _continuation;
        private UnityTask<T>.Awaiter _awaiter;

        private UnityTaskForgetObserver()
        {
            _continuation = Continue;
        }

        /// <summary>
        /// Starts observing <paramref name="task"/>; completes inline when it is already complete, else registers the
        /// cached continuation.
        /// </summary>
        internal static void Observe(UnityTask<T> task)
        {
            UnityTaskForgetObserver<T> observer;

            lock (s_poolLock)
            {
                observer = s_pool.Count == 0 ? new() : s_pool.Pop();
            }

            observer._awaiter = task.GetAwaiter();

            if (observer._awaiter.IsCompleted)
            {
                observer.Continue();
            }
            else
            {
                observer._awaiter.UnsafeOnCompleted(observer._continuation);
            }
        }

        /// <summary>
        /// Reads the outcome, logs a fault other than <see cref="OperationCanceledException"/> and returns the
        /// observer to the pool.
        /// </summary>
        private void Continue()
        {
            try
            {
                _awaiter.GetResult();
            }
            catch (Exception exception)
            {
                if (exception is OperationCanceledException == false)
                {
                    ThrowHelper.LogUnobservedException(exception);
                }
            }
            finally
            {
                _awaiter = default;

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
}
