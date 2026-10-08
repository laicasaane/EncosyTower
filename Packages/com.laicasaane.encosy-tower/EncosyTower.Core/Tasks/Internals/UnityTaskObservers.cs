using System;
using System.Collections.Generic;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Pooled observer that reports the outcome of one <see cref="UnityTask"/> input of a combinator to a sink,
    /// without allocating a closure.
    /// </summary>
    /// <typeparam name="TPosition">
    /// The <c>UnityTaskPositionN</c> tag type that tells the sink which input completed.
    /// </typeparam>
    /// <typeparam name="TSink">
    /// The sink type, which implements <see cref="IUnityTaskSink{TPosition}"/> for the tag.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// <c>Observe</c> takes an observer from the pool and completes inline when the task is already complete. Otherwise
    /// it registers the cached <c>_continuation</c> delegate with <c>UnsafeOnCompleted</c>, so no closure is
    /// allocated.
    /// </para>
    /// <para>
    /// <c>Invoke</c> runs on the thread that completes the task. It reads the outcome, calls <c>Complete</c> on the
    /// sink, returns the observer to the pool and calls <c>Detach</c> last, so the sink can release itself after every
    /// observer is gone.
    /// </para>
    /// <para>
    /// <typeparamref name="TPosition"/> is only a tag. A fixed-arity sink implements the interface once per tag, so
    /// the call <c>Complete(default, ...)</c> reaches the right implementation without an index.
    /// </para>
    /// </remarks>
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

        /// <summary>
        /// Binds an observer to <paramref name="sink"/> and starts observing <paramref name="task"/>.
        /// </summary>
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

        /// <summary>
        /// Reads the task's outcome, reports it to the sink, returns the observer to the pool and detaches from the
        /// sink.
        /// </summary>
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

            // Last, after the observer is back in the pool: the sink may release itself once no observer is left.
            sink.Detach();
        }
    }

    /// <summary>
    /// Pooled observer that reports the outcome of one <see cref="UnityTask{T}"/> input of a combinator to a sink,
    /// without allocating a closure.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <typeparam name="TPosition">
    /// The <c>UnityTaskPositionN</c> tag type that tells the sink which input completed.
    /// </typeparam>
    /// <typeparam name="TSink">
    /// The sink type, which implements <see cref="IUnityTaskResultSink{T, TPosition}"/> for the tag.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// <c>Observe</c> takes an observer from the pool and completes inline when the task is already complete. Otherwise
    /// it registers the cached <c>_continuation</c> delegate with <c>UnsafeOnCompleted</c>, so no closure is
    /// allocated.
    /// </para>
    /// <para>
    /// <c>Invoke</c> runs on the thread that completes the task. It reads the outcome, calls <c>Complete</c> on the
    /// sink, returns the observer to the pool and calls <c>Detach</c> last, so the sink can release itself after every
    /// observer is gone.
    /// </para>
    /// <para>
    /// <typeparamref name="TPosition"/> is only a tag. A fixed-arity sink implements the interface once per tag, so
    /// the call <c>Complete(default, ...)</c> reaches the right implementation without an index.
    /// </para>
    /// </remarks>
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

        /// <summary>
        /// Binds an observer to <paramref name="sink"/> and starts observing <paramref name="task"/>.
        /// </summary>
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

        /// <summary>
        /// Reads the task's outcome, reports it to the sink, returns the observer to the pool and detaches from the
        /// sink.
        /// </summary>
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

            // Last, after the observer is back in the pool: the sink may release itself once no observer is left.
            sink.Detach();
        }
    }

    /// <summary>
    /// Pooled observer that reports the outcome of one <see cref="UnityTask"/> input of a combinator over a sequence
    /// to a sink, together with the input's index.
    /// </summary>
    /// <typeparam name="TSink">The sink type, which implements <see cref="IIndexedUnityTaskSink"/>.</typeparam>
    /// <remarks>
    /// <para>
    /// <c>Observe</c> takes an observer from the pool and completes inline when the task is already complete. Otherwise
    /// it registers the cached <c>_continuation</c> delegate with <c>UnsafeOnCompleted</c>, so no closure is
    /// allocated.
    /// </para>
    /// <para>
    /// <c>Invoke</c> runs on the thread that completes the task. It reads the outcome, calls <c>Complete</c> on the
    /// sink, returns the observer to the pool and calls <c>Detach</c> last, so the sink can release itself after every
    /// observer is gone.
    /// </para>
    /// </remarks>
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

        /// <summary>
        /// Binds an observer to <paramref name="sink"/> and <paramref name="index"/> and starts observing
        /// <paramref name="task"/>.
        /// </summary>
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

        /// <summary>
        /// Reads the task's outcome, reports it to the sink, returns the observer to the pool and detaches from the
        /// sink.
        /// </summary>
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

            // Last, after the observer is back in the pool: the sink may release itself once no observer is left.
            sink.Detach();
        }
    }

    /// <summary>
    /// Pooled observer that reports the outcome of one <see cref="UnityTask{T}"/> input of a combinator over a
    /// sequence to a sink, together with the input's index.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <typeparam name="TSink">
    /// The sink type, which implements <see cref="IIndexedUnityTaskResultSink{T}"/>.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// <c>Observe</c> takes an observer from the pool and completes inline when the task is already complete. Otherwise
    /// it registers the cached <c>_continuation</c> delegate with <c>UnsafeOnCompleted</c>, so no closure is
    /// allocated.
    /// </para>
    /// <para>
    /// <c>Invoke</c> runs on the thread that completes the task. It reads the outcome, calls <c>Complete</c> on the
    /// sink, returns the observer to the pool and calls <c>Detach</c> last, so the sink can release itself after every
    /// observer is gone.
    /// </para>
    /// </remarks>
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

        /// <summary>
        /// Binds an observer to <paramref name="sink"/> and <paramref name="index"/> and starts observing
        /// <paramref name="task"/>.
        /// </summary>
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

        /// <summary>
        /// Reads the task's outcome, reports it to the sink, returns the observer to the pool and detaches from the
        /// sink.
        /// </summary>
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

            // Last, after the observer is back in the pool: the sink may release itself once no observer is left.
            sink.Detach();
        }
    }
}
