using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityEngine.Tasks
{
    internal static class AwaitableCancellation
    {
        internal static CancellationTokenRegistration Register(
              CancellationToken token
            , Action<object> callback
            , object state
        )
        {
            if (ExecutionContext.IsFlowSuppressed())
            {
                return token.Register(callback, state);
            }

            using (ExecutionContext.SuppressFlow())
            {
                return token.Register(callback, state);
            }
        }
    }

    internal interface IAwaitableSink<TPosition>
    {
        void Complete(TPosition position, Exception exception);

        void Detach();
    }

    internal interface IAwaitableResultSink<T, TPosition>
    {
        void Complete(TPosition position, T result, Exception exception);

        void Detach();
    }

    internal interface IIndexedAwaitableSink
    {
        void Complete(int index, Exception exception);

        void Detach();
    }

    internal interface IIndexedAwaitableResultSink<T>
    {
        void Complete(int index, T result, Exception exception);

        void Detach();
    }

    internal readonly struct AwaitablePosition1 { }

    internal readonly struct AwaitablePosition2 { }

    internal readonly struct AwaitablePosition3 { }

    internal readonly struct AwaitablePosition4 { }

    internal readonly struct AwaitablePosition5 { }

    internal readonly struct AwaitablePosition6 { }

    internal readonly struct AwaitablePosition7 { }

    internal readonly struct AwaitablePosition8 { }

    internal readonly struct AwaitablePosition9 { }

    internal readonly struct AwaitablePosition10 { }

    internal readonly struct AwaitablePosition11 { }

    internal readonly struct AwaitablePosition12 { }

    internal readonly struct AwaitablePosition13 { }

    internal readonly struct AwaitablePosition14 { }

    internal readonly struct AwaitablePosition15 { }

    internal sealed class PooledAwaitableObserver<TPosition, TSink>
        where TSink : class, IAwaitableSink<TPosition>
    {
        private const int MAX_POOL_SIZE = 256;
        private static readonly Stack<PooledAwaitableObserver<TPosition, TSink>> s_pool = new();
        private readonly Action _continuation;
        private Awaitable.Awaiter _awaiter;
        private TSink _sink;

        private PooledAwaitableObserver()
        {
            _continuation = Invoke;
        }

        internal static void Observe(Awaitable awaitable, TSink sink)
        {
            PooledAwaitableObserver<TPosition, TSink> observer;

            lock (s_pool)
            {
                observer = s_pool.Count > 0 ? s_pool.Pop() : new PooledAwaitableObserver<TPosition, TSink>();
            }

            observer._sink = sink;
            observer._awaiter = awaitable.GetAwaiter();

            if (observer._awaiter.IsCompleted)
            {
                observer.Invoke();
            }
            else
            {
                observer._awaiter.OnCompleted(observer._continuation);
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

            sink.Complete(default, exception);
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

    internal sealed class PooledAwaitableObserver<T, TPosition, TSink>
        where TSink : class, IAwaitableResultSink<T, TPosition>
    {
        private const int MAX_POOL_SIZE = 256;
        private static readonly Stack<PooledAwaitableObserver<T, TPosition, TSink>> s_pool = new();
        private readonly Action _continuation;
        private Awaitable<T>.Awaiter _awaiter;
        private TSink _sink;

        private PooledAwaitableObserver()
        {
            _continuation = Invoke;
        }

        internal static void Observe(Awaitable<T> awaitable, TSink sink)
        {
            PooledAwaitableObserver<T, TPosition, TSink> observer;

            lock (s_pool)
            {
                observer = s_pool.Count > 0 ? s_pool.Pop() : new PooledAwaitableObserver<T, TPosition, TSink>();
            }

            observer._sink = sink;
            observer._awaiter = awaitable.GetAwaiter();

            if (observer._awaiter.IsCompleted)
            {
                observer.Invoke();
            }
            else
            {
                observer._awaiter.OnCompleted(observer._continuation);
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

            sink.Complete(default, result, exception);
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

    internal sealed class PooledIndexedAwaitableObserver<TSink>
        where TSink : class, IIndexedAwaitableSink
    {
        private const int MAX_POOL_SIZE = 256;
        private static readonly Stack<PooledIndexedAwaitableObserver<TSink>> s_pool = new();
        private readonly Action _continuation;
        private Awaitable.Awaiter _awaiter;
        private TSink _sink;
        private int _index;

        private PooledIndexedAwaitableObserver()
        {
            _continuation = Invoke;
        }

        internal static void Observe(Awaitable awaitable, TSink sink, int index)
        {
            PooledIndexedAwaitableObserver<TSink> observer;

            lock (s_pool)
            {
                observer = s_pool.Count > 0 ? s_pool.Pop() : new PooledIndexedAwaitableObserver<TSink>();
            }

            observer._sink = sink;
            observer._index = index;
            observer._awaiter = awaitable.GetAwaiter();

            if (observer._awaiter.IsCompleted)
            {
                observer.Invoke();
            }
            else
            {
                observer._awaiter.OnCompleted(observer._continuation);
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

    internal sealed class PooledIndexedAwaitableObserver<T, TSink>
        where TSink : class, IIndexedAwaitableResultSink<T>
    {
        private const int MAX_POOL_SIZE = 256;
        private static readonly Stack<PooledIndexedAwaitableObserver<T, TSink>> s_pool = new();
        private readonly Action _continuation;
        private Awaitable<T>.Awaiter _awaiter;
        private TSink _sink;
        private int _index;

        private PooledIndexedAwaitableObserver()
        {
            _continuation = Invoke;
        }

        internal static void Observe(Awaitable<T> awaitable, TSink sink, int index)
        {
            PooledIndexedAwaitableObserver<T, TSink> observer;

            lock (s_pool)
            {
                observer = s_pool.Count > 0 ? s_pool.Pop() : new PooledIndexedAwaitableObserver<T, TSink>();
            }

            observer._sink = sink;
            observer._index = index;
            observer._awaiter = awaitable.GetAwaiter();

            if (observer._awaiter.IsCompleted)
            {
                observer.Invoke();
            }
            else
            {
                observer._awaiter.OnCompleted(observer._continuation);
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
