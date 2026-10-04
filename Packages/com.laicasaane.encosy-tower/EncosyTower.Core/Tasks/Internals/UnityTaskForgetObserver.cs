using System;
using System.Collections.Generic;

namespace EncosyTower.Tasks
{
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
