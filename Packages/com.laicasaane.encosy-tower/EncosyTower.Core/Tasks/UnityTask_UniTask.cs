#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        private readonly UniTask _task;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UnityTask(UniTask task)
            => _task = task;

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task.GetAwaiter().IsCompleted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task.GetAwaiter());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask(UniTask task)
            => new(task);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask AsUniTask()
            => _task;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UnityTask<T> CreateFromResult<T>(T value)
            => new(UniTask.FromResult(value));

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly UniTask.Awaiter _awaiter;

            internal Awaiter(UniTask.Awaiter awaiter)
                => _awaiter = awaiter;

            public bool IsCompleted => _awaiter.IsCompleted;

            public void GetResult()
                => _awaiter.GetResult();

            public void OnCompleted(Action continuation)
                => _awaiter.OnCompleted(continuation);

            public void UnsafeOnCompleted(Action continuation)
                => _awaiter.UnsafeOnCompleted(continuation);
        }
    }

    public readonly partial struct UnityTask<T>
    {
        private readonly UniTask<T> _task;

        internal UnityTask(UniTask<T> task)
            => _task = task;

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task.GetAwaiter().IsCompleted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task.GetAwaiter());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask<T>(UniTask<T> task)
            => new(task);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<T> AsUniTask()
            => _task;

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly UniTask<T>.Awaiter _awaiter;

            internal Awaiter(UniTask<T>.Awaiter awaiter)
                => _awaiter = awaiter;

            public bool IsCompleted => _awaiter.IsCompleted;

            public T GetResult()
                => _awaiter.GetResult();

            public void OnCompleted(Action continuation)
                => _awaiter.OnCompleted(continuation);

            public void UnsafeOnCompleted(Action continuation)
                => _awaiter.UnsafeOnCompleted(continuation);
        }
    }

    public readonly partial struct UnityTask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask Delay(
              int millisecondsDelay
            , bool ignoreTimeScale = false
            , PlayerLoopTiming delayTiming = PlayerLoopTiming.Update
            , CancellationToken cancellationToken = default
            , bool cancelImmediately = false
        )
            => new(UniTask.Delay(
                  millisecondsDelay
                , ignoreTimeScale
                , delayTiming
                , cancellationToken
                , cancelImmediately
            ));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask Delay(
              TimeSpan delayTimeSpan
            , bool ignoreTimeScale = false
            , PlayerLoopTiming delayTiming = PlayerLoopTiming.Update
            , CancellationToken cancellationToken = default
            , bool cancelImmediately = false
        )
            => new(UniTask.Delay(
                  delayTimeSpan
                , ignoreTimeScale
                , delayTiming
                , cancellationToken
                , cancelImmediately
            ));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask Delay(
              int millisecondsDelay
            , DelayType delayType
            , PlayerLoopTiming delayTiming = PlayerLoopTiming.Update
            , CancellationToken cancellationToken = default
            , bool cancelImmediately = false
        )
            => new(UniTask.Delay(
                  millisecondsDelay
                , delayType
                , delayTiming
                , cancellationToken
                , cancelImmediately
            ));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask Delay(
              TimeSpan delayTimeSpan
            , DelayType delayType
            , PlayerLoopTiming delayTiming = PlayerLoopTiming.Update
            , CancellationToken cancellationToken = default
            , bool cancelImmediately = false
        )
            => new(UniTask.Delay(
                  delayTimeSpan
                , delayType
                , delayTiming
                , cancellationToken
                , cancelImmediately
            ));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask Yield()
            => new(UniTask.Yield().ToUniTask());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask Yield(PlayerLoopTiming timing)
            => new(UniTask.Yield(timing).ToUniTask());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask Yield(
              CancellationToken cancellationToken
            , bool cancelImmediately = false
        )
            => new(UniTask.Yield(cancellationToken, cancelImmediately));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask Yield(
              PlayerLoopTiming timing
            , CancellationToken cancellationToken
            , bool cancelImmediately = false
        )
            => new(UniTask.Yield(timing, cancellationToken, cancelImmediately));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask RunOnThreadPool(
              Action action
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(UniTask.RunOnThreadPool(action, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask RunOnThreadPool(
              Action<object> action
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(UniTask.RunOnThreadPool(action, state, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask RunOnThreadPool(
              Func<UniTask> action
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(UniTask.RunOnThreadPool(action, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask RunOnThreadPool(
              Func<object, UniTask> action
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(UniTask.RunOnThreadPool(action, state, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> RunOnThreadPool<T>(
              Func<T> function
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(UniTask.RunOnThreadPool(function, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> RunOnThreadPool<T>(
              Func<UniTask<T>> function
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(UniTask.RunOnThreadPool(function, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> RunOnThreadPool<T>(
              Func<object, T> function
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(UniTask.RunOnThreadPool(function, state, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> RunOnThreadPool<T>(
              Func<object, UniTask<T>> function
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(UniTask.RunOnThreadPool(function, state, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WhenAll(params UnityTask[] tasks)
            => WhenAll(tasks, tasks?.Length ?? 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WhenAll(IEnumerable<UnityTask> tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var adapter = UniTaskEnumerable.Rent(tasks);

            try
            {
                return new(UniTask.WhenAll(adapter));
            }
            finally
            {
                adapter.Return();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WhenAll(UnityTask[] tasks, int count)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            ThrowHelper.ThrowIfCountOutOfRange((uint)count <= (uint)tasks.Length);
            var adapter = UniTaskEnumerable.Rent(tasks, count);

            try
            {
                return new(UniTask.WhenAll(adapter));
            }
            finally
            {
                adapter.Return();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T[]> WhenAll<T>(params UnityTask<T>[] tasks)
            => WhenAll(tasks, tasks?.Length ?? 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T[]> WhenAll<T>(IEnumerable<UnityTask<T>> tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var adapter = UniTaskEnumerable<T>.Rent(tasks);

            try
            {
                return new(UniTask.WhenAll(adapter));
            }
            finally
            {
                adapter.Return();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T[]> WhenAll<T>(UnityTask<T>[] tasks, int count)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            ThrowHelper.ThrowIfCountOutOfRange((uint)count <= (uint)tasks.Length);
            var adapter = UniTaskEnumerable<T>.Rent(tasks, count);

            try
            {
                return new(UniTask.WhenAll(adapter));
            }
            finally
            {
                adapter.Return();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<(bool hasResultLeft, T result)> WhenAny<T>(
              UnityTask<T> leftTask
            , UnityTask rightTask
        )
            => new(UniTask.WhenAny(leftTask.AsUniTask(), rightTask.AsUniTask()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<(int winArgumentIndex, T result)> WhenAny<T>(
            params UnityTask<T>[] tasks
        )
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var adapter = UniTaskEnumerable<T>.Rent(tasks, tasks.Length);

            try
            {
                return new(UniTask.WhenAny(adapter));
            }
            finally
            {
                adapter.Return();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<(int winArgumentIndex, T result)> WhenAny<T>(
            IEnumerable<UnityTask<T>> tasks
        )
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var adapter = UniTaskEnumerable<T>.Rent(tasks);

            try
            {
                return new(UniTask.WhenAny(adapter));
            }
            finally
            {
                adapter.Return();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<int> WhenAny(params UnityTask[] tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var adapter = UniTaskEnumerable.Rent(tasks, tasks.Length);

            try
            {
                return new(UniTask.WhenAny(adapter));
            }
            finally
            {
                adapter.Return();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<int> WhenAny(IEnumerable<UnityTask> tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var adapter = UniTaskEnumerable.Rent(tasks);

            try
            {
                return new(UniTask.WhenAny(adapter));
            }
            finally
            {
                adapter.Return();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IUniTaskAsyncEnumerable<WhenEachResult<T>> WhenEach<T>(
            params UnityTask<T>[] tasks
        )
            => UniTask.WhenEach(UniTaskEnumerable<T>.CreatePersistent(tasks));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IUniTaskAsyncEnumerable<WhenEachResult<T>> WhenEach<T>(
            IEnumerable<UnityTask<T>> tasks
        )
            => UniTask.WhenEach(UniTaskEnumerable<T>.CreatePersistent(tasks));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask NextFrameAsync(CancellationToken token)
            => new(UniTask.NextFrame(token));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntil(Func<bool> predicate, CancellationToken token)
            => new(UniTask.WaitUntil(predicate, cancellationToken: token));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntil<T>(
              T state
            , Func<T, bool> predicate
            , CancellationToken token
        )
            => new(UniTask.WaitUntil(state, predicate, cancellationToken: token));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitWhile<T>(
              T state
            , Func<T, bool> predicate
            , CancellationToken token
        )
            => new(UniTask.WaitWhile(state, predicate, cancellationToken: token));

        private sealed class UniTaskEnumerable : IEnumerable<UniTask>
        {
            private static readonly object s_poolLock = new();
            private static readonly Stack<UniTaskEnumerable> s_pool = new();

            private IEnumerable<UnityTask> _source;
            private UnityTask[] _array;
            private int _count;
            private bool _pooled;

            private UniTaskEnumerable() { }

            private UniTaskEnumerable(IEnumerable<UnityTask> source)
            {
                _source = source;
            }

            internal static UniTaskEnumerable Rent(IEnumerable<UnityTask> source)
            {
                var result = RentCore();
                result._source = source;
                return result;
            }

            internal static UniTaskEnumerable Rent(UnityTask[] array, int count)
            {
                var result = RentCore();
                result._array = array;
                result._count = count;
                return result;
            }

            internal IEnumerator<UniTask> GetEnumerator()
            {
                if (_array != null)
                {
                    return new ArrayEnumerator(_array, _count);
                }

                return new SourceEnumerator(_source.GetEnumerator());
            }

            IEnumerator<UniTask> IEnumerable<UniTask>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            internal void Return()
            {
                if (_pooled == false)
                {
                    return;
                }

                _source = null;
                _array = null;
                _count = 0;

                lock (s_poolLock)
                {
                    if (s_pool.Count < 64)
                    {
                        s_pool.Push(this);
                    }
                }
            }

            private static UniTaskEnumerable RentCore()
            {
                UniTaskEnumerable result;

                lock (s_poolLock)
                {
                    result = s_pool.Count == 0 ? new() : s_pool.Pop();
                }

                result._pooled = true;
                return result;
            }

            private sealed class ArrayEnumerator : IEnumerator<UniTask>
            {
                private readonly UnityTask[] _array;
                private readonly int _count;
                private int _index = -1;

                internal ArrayEnumerator(UnityTask[] array, int count)
                {
                    _array = array;
                    _count = count;
                }

                public UniTask Current => _array[_index].AsUniTask();
                object IEnumerator.Current => Current;
                public bool MoveNext() => ++_index < _count;
                public void Reset() => _index = -1;
                public void Dispose() { }
            }

            private sealed class SourceEnumerator : IEnumerator<UniTask>
            {
                private readonly IEnumerator<UnityTask> _source;

                internal SourceEnumerator(IEnumerator<UnityTask> source)
                {
                    _source = source;
                }

                public UniTask Current => _source.Current.AsUniTask();
                object IEnumerator.Current => Current;
                public bool MoveNext() => _source.MoveNext();
                public void Reset() => _source.Reset();
                public void Dispose() => _source.Dispose();
            }
        }

        private sealed class UniTaskEnumerable<T> : IEnumerable<UniTask<T>>
        {
            private static readonly object s_poolLock = new();
            private static readonly Stack<UniTaskEnumerable<T>> s_pool = new();

            private IEnumerable<UnityTask<T>> _source;
            private UnityTask<T>[] _array;
            private int _count;
            private bool _pooled;

            private UniTaskEnumerable() { }

            private UniTaskEnumerable(IEnumerable<UnityTask<T>> source)
            {
                _source = source;
            }

            internal static UniTaskEnumerable<T> Rent(IEnumerable<UnityTask<T>> source)
            {
                var result = RentCore();
                result._source = source;
                return result;
            }

            internal static UniTaskEnumerable<T> Rent(UnityTask<T>[] array, int count)
            {
                var result = RentCore();
                result._array = array;
                result._count = count;
                return result;
            }

            internal static UniTaskEnumerable<T> CreatePersistent(
                IEnumerable<UnityTask<T>> source
            )
                => new(source);

            internal IEnumerator<UniTask<T>> GetEnumerator()
            {
                if (_array != null)
                {
                    return new ArrayEnumerator(_array, _count);
                }

                return new SourceEnumerator(_source.GetEnumerator());
            }

            IEnumerator<UniTask<T>> IEnumerable<UniTask<T>>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            internal void Return()
            {
                if (_pooled == false)
                {
                    return;
                }

                _source = null;
                _array = null;
                _count = 0;

                lock (s_poolLock)
                {
                    if (s_pool.Count < 64)
                    {
                        s_pool.Push(this);
                    }
                }
            }

            private static UniTaskEnumerable<T> RentCore()
            {
                UniTaskEnumerable<T> result;

                lock (s_poolLock)
                {
                    result = s_pool.Count == 0 ? new() : s_pool.Pop();
                }

                result._pooled = true;
                return result;
            }

            private sealed class ArrayEnumerator : IEnumerator<UniTask<T>>
            {
                private readonly UnityTask<T>[] _array;
                private readonly int _count;
                private int _index = -1;

                internal ArrayEnumerator(UnityTask<T>[] array, int count)
                {
                    _array = array;
                    _count = count;
                }

                public UniTask<T> Current => _array[_index].AsUniTask();
                object IEnumerator.Current => Current;
                public bool MoveNext() => ++_index < _count;
                public void Reset() => _index = -1;
                public void Dispose() { }
            }

            private sealed class SourceEnumerator : IEnumerator<UniTask<T>>
            {
                private readonly IEnumerator<UnityTask<T>> _source;

                internal SourceEnumerator(IEnumerator<UnityTask<T>> source)
                {
                    _source = source;
                }

                public UniTask<T> Current => _source.Current.AsUniTask();
                object IEnumerator.Current => Current;
                public bool MoveNext() => _source.MoveNext();
                public void Reset() => _source.Reset();
                public void Dispose() => _source.Dispose();
            }
        }
    }

    public static class UnityTaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith<T>(
              this UnityTask<T> task
            , Action<T> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith<T>(
              this UnityTask<T> task
            , Func<T, UniTask> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<TR> ContinueWith<T, TR>(
              this UnityTask<T> task
            , Func<T, TR> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<TR> ContinueWith<T, TR>(
              this UnityTask<T> task
            , Func<T, UniTask<TR>> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith(
              this UnityTask task
            , Action continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith(
              this UnityTask task
            , Func<UniTask> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> ContinueWith<T>(
              this UnityTask task
            , Func<T> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> ContinueWith<T>(
              this UnityTask task
            , Func<UniTask<T>> continuationFunction
        )
            => new(task.AsUniTask().ContinueWith(continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this UnityTask task)
            => task.AsUniTask().Forget();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget<T>(this UnityTask<T> task)
            => task.AsUniTask().Forget();
    }
}

#endif
