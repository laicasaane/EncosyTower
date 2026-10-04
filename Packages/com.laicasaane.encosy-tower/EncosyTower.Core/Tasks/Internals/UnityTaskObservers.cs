using System;
using System.Collections.Generic;

namespace EncosyTower.Tasks
{
    internal sealed class PooledUnityTaskObserver<TPosition, TSink>
        where TSink : class, IUnityTaskSink<TPosition>
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly Stack<PooledUnityTaskObserver<TPosition, TSink>> s_pool = new();

        private readonly Action _continuation;
        private UnityTask.Awaiter _awaiter;
        private TSink _sink;

        private PooledUnityTaskObserver()
        {
            _continuation = Invoke;
        }

        internal static void Observe(UnityTask task, TSink sink)
        {
            PooledUnityTaskObserver<TPosition, TSink> observer;

            lock (s_pool)
            {
                observer = s_pool.Count > 0 ? s_pool.Pop() : new();
            }

            observer._sink = sink;
            observer._awaiter = task.GetAwaiter();

            if (observer._awaiter.IsCompleted)
            {
                observer.Invoke();
            }
            else
            {
                observer._awaiter.UnsafeOnCompleted(observer._continuation);
            }
        }

        private void Invoke()
        {
            var sink = _sink;
            Exception exception = null;

            try
            {
                _awaiter.GetResult();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            sink.Complete(position: default, exception: exception);
            _sink = null;
            _awaiter = default;

            lock (s_pool)
            {
                if (s_pool.Count < MAX_POOL_SIZE)
                {
                    s_pool.Push(this);
                }
            }

            sink.Detach();
        }
    }

    internal sealed class PooledUnityTaskObserver<T, TPosition, TSink>
        where TSink : class, IUnityTaskResultSink<T, TPosition>
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly Stack<PooledUnityTaskObserver<T, TPosition, TSink>> s_pool = new();

        private readonly Action _continuation;
        private UnityTask<T>.Awaiter _awaiter;
        private TSink _sink;

        private PooledUnityTaskObserver()
        {
            _continuation = Invoke;
        }

        internal static void Observe(UnityTask<T> task, TSink sink)
        {
            PooledUnityTaskObserver<T, TPosition, TSink> observer;

            lock (s_pool)
            {
                observer = s_pool.Count > 0 ? s_pool.Pop() : new();
            }

            observer._sink = sink;
            observer._awaiter = task.GetAwaiter();

            if (observer._awaiter.IsCompleted)
            {
                observer.Invoke();
            }
            else
            {
                observer._awaiter.UnsafeOnCompleted(observer._continuation);
            }
        }

        private void Invoke()
        {
            var sink = _sink;
            Exception exception = null;
            var result = default(T);

            try
            {
                result = _awaiter.GetResult();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            sink.Complete(position: default, result: result, exception: exception);
            _sink = null;
            _awaiter = default;

            lock (s_pool)
            {
                if (s_pool.Count < MAX_POOL_SIZE)
                {
                    s_pool.Push(this);
                }
            }

            sink.Detach();
        }
    }

    internal sealed class PooledIndexedUnityTaskObserver<TSink>
        where TSink : class, IIndexedUnityTaskSink
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly Stack<PooledIndexedUnityTaskObserver<TSink>> s_pool = new();

        private readonly Action _continuation;
        private UnityTask.Awaiter _awaiter;
        private TSink _sink;
        private int _index;

        private PooledIndexedUnityTaskObserver()
        {
            _continuation = Invoke;
        }

        internal static void Observe(UnityTask task, TSink sink, int index)
        {
            PooledIndexedUnityTaskObserver<TSink> observer;

            lock (s_pool)
            {
                observer = s_pool.Count > 0 ? s_pool.Pop() : new();
            }

            observer._sink = sink;
            observer._index = index;
            observer._awaiter = task.GetAwaiter();

            if (observer._awaiter.IsCompleted)
            {
                observer.Invoke();
            }
            else
            {
                observer._awaiter.UnsafeOnCompleted(observer._continuation);
            }
        }

        private void Invoke()
        {
            var sink = _sink;
            var index = _index;
            Exception exception = null;

            try
            {
                _awaiter.GetResult();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            sink.Complete(index, exception);
            _sink = null;
            _index = 0;
            _awaiter = default;

            lock (s_pool)
            {
                if (s_pool.Count < MAX_POOL_SIZE)
                {
                    s_pool.Push(this);
                }
            }

            sink.Detach();
        }
    }

    internal sealed class PooledIndexedUnityTaskObserver<T, TSink>
        where TSink : class, IIndexedUnityTaskResultSink<T>
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly Stack<PooledIndexedUnityTaskObserver<T, TSink>> s_pool = new();

        private readonly Action _continuation;
        private UnityTask<T>.Awaiter _awaiter;
        private TSink _sink;
        private int _index;

        private PooledIndexedUnityTaskObserver()
        {
            _continuation = Invoke;
        }

        internal static void Observe(UnityTask<T> task, TSink sink, int index)
        {
            PooledIndexedUnityTaskObserver<T, TSink> observer;

            lock (s_pool)
            {
                observer = s_pool.Count > 0 ? s_pool.Pop() : new();
            }

            observer._sink = sink;
            observer._index = index;
            observer._awaiter = task.GetAwaiter();

            if (observer._awaiter.IsCompleted)
            {
                observer.Invoke();
            }
            else
            {
                observer._awaiter.UnsafeOnCompleted(observer._continuation);
            }
        }

        private void Invoke()
        {
            var sink = _sink;
            var index = _index;
            Exception exception = null;
            var result = default(T);

            try
            {
                result = _awaiter.GetResult();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            sink.Complete(index, result, exception);
            _sink = null;
            _index = 0;
            _awaiter = default;

            lock (s_pool)
            {
                if (s_pool.Count < MAX_POOL_SIZE)
                {
                    s_pool.Push(this);
                }
            }

            sink.Detach();
        }
    }
}
