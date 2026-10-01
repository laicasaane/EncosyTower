#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace UnityEngine.Tasks
{
    internal sealed class AwaitableForgetObserver
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<AwaitableForgetObserver> s_pool = new(MAX_POOL_SIZE);

        private readonly Action _continuation;
        private Awaitable.Awaiter _awaiter;

        private AwaitableForgetObserver()
        {
            _continuation = Continue;
        }

        internal static void Observe(Awaitable task)
        {
            AwaitableForgetObserver observer;

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
                observer._awaiter.OnCompleted(observer._continuation);
            }
        }

        private void Continue()
        {
            try
            {
                _awaiter.GetResult();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                ThrowHelper.LogException(exception);
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

    internal sealed class AwaitableForgetObserver<T>
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<AwaitableForgetObserver<T>> s_pool = new(MAX_POOL_SIZE);

        private readonly Action _continuation;
        private Awaitable<T>.Awaiter _awaiter;

        private AwaitableForgetObserver()
        {
            _continuation = Continue;
        }

        internal static void Observe(Awaitable<T> task)
        {
            AwaitableForgetObserver<T> observer;

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
                observer._awaiter.OnCompleted(observer._continuation);
            }
        }

        private void Continue()
        {
            try
            {
                _ = _awaiter.GetResult();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                ThrowHelper.LogException(exception);
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

    internal sealed class AwaitableRunObserver
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<AwaitableRunObserver> s_pool = new(MAX_POOL_SIZE);

        private readonly Action _continuation;
        private Awaitable.Awaiter _awaiter;

        private AwaitableRunObserver()
        {
            _continuation = Continue;
        }

        internal static void Observe(Awaitable task)
        {
            AwaitableRunObserver observer;

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
                observer._awaiter.OnCompleted(observer._continuation);
            }
        }

        private void Continue()
        {
            ExceptionDispatchInfo exception = null;

            try
            {
                _awaiter.GetResult();
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
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

            exception?.Throw();
        }
    }
}
