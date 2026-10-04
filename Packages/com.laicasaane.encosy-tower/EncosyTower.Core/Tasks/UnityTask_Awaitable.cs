#if !UNITASK || ENCOSY_UNITYTASK_AWAITABLE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Debugging;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;
using UnityEngine;
using UnityEngine.Tasks;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        private readonly Awaitable _task;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UnityTask(Awaitable task)
            => _task = task;

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task == null || _task.GetAwaiter().IsCompleted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask(Awaitable task)
        {
            DebuggingThrowHelper.ThrowIfNull(task);
            return new(task);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable AsAwaitable()
            => _task ?? Awaitables.GetCompleted();

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly Awaitable _task;

            internal Awaiter(Awaitable task)
                => _task = task;

            public bool IsCompleted => _task == null || _task.GetAwaiter().IsCompleted;

            public void GetResult()
            {
                if (_task != null)
                {
                    _task.GetAwaiter().GetResult();
                }
            }

            public void OnCompleted(Action continuation)
            {
                if (_task == null)
                {
                    continuation();
                    return;
                }

                _task.GetAwaiter().OnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
                => OnCompleted(continuation);
        }
    }

    public readonly partial struct UnityTask<T>
    {
        private readonly Awaitable<T> _task;

        internal UnityTask(Awaitable<T> task)
            => _task = task;

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task == null || _task.GetAwaiter().IsCompleted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
            => new(_task);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityTask<T>(Awaitable<T> task)
        {
            DebuggingThrowHelper.ThrowIfNull(task);
            return new(task);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<T> AsAwaitable()
            => _task ?? Awaitables.GetCompleted<T>();

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly Awaitable<T> _task;

            internal Awaiter(Awaitable<T> task)
                => _task = task;

            public bool IsCompleted => _task == null || _task.GetAwaiter().IsCompleted;

            public T GetResult()
                => _task == null ? default : _task.GetAwaiter().GetResult();

            public void OnCompleted(Action continuation)
            {
                if (_task == null)
                {
                    continuation();
                    return;
                }

                _task.GetAwaiter().OnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
                => OnCompleted(continuation);
        }
    }

    public readonly partial struct UnityTask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask RunOnThreadPool(
              Action action
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(Awaitables.RunOnThreadPool(action, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask RunOnThreadPool(
              Action<object> action
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(Awaitables.RunOnThreadPool(action, state, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask RunOnThreadPool(
              Func<Awaitable> action
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(Awaitables.RunOnThreadPool(action, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask RunOnThreadPool(
              Func<object, Awaitable> action
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(Awaitables.RunOnThreadPool(action, state, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> RunOnThreadPool<T>(
              Func<T> function
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(Awaitables.RunOnThreadPool(function, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> RunOnThreadPool<T>(
              Func<Awaitable<T>> function
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(Awaitables.RunOnThreadPool(function, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> RunOnThreadPool<T>(
              Func<object, T> function
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(Awaitables.RunOnThreadPool(function, state, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> RunOnThreadPool<T>(
              Func<object, Awaitable<T>> function
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
            => new(Awaitables.RunOnThreadPool(function, state, configureAwait, cancellationToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WhenAll(params UnityTask[] tasks)
            => WhenAll(tasks, tasks?.Length ?? 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WhenAll(IEnumerable<UnityTask> tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var adapter = AwaitableEnumerable.Rent(tasks);
            return new(WhenAllAndReturn(adapter));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WhenAll(UnityTask[] tasks, int count)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            global::EncosyTower.Tasks.ThrowHelper.ThrowIfCountOutOfRange((uint)count <= (uint)tasks.Length);
            var adapter = AwaitableEnumerable.Rent(tasks, count);
            return new(WhenAllAndReturn(adapter));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T[]> WhenAll<T>(params UnityTask<T>[] tasks)
            => WhenAll(tasks, tasks?.Length ?? 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T[]> WhenAll<T>(IEnumerable<UnityTask<T>> tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var adapter = AwaitableEnumerable<T>.Rent(tasks);
            return new(WhenAllAndReturn(adapter));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T[]> WhenAll<T>(UnityTask<T>[] tasks, int count)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            global::EncosyTower.Tasks.ThrowHelper.ThrowIfCountOutOfRange((uint)count <= (uint)tasks.Length);
            var adapter = AwaitableEnumerable<T>.Rent(tasks, count);
            return new(WhenAllAndReturn(adapter));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<(bool hasResultLeft, T result)> WhenAny<T>(UnityTask<T> leftTask, UnityTask rightTask)
            => new(Awaitables.WhenAny(leftTask.AsAwaitable(), rightTask.AsAwaitable()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<(int winArgumentIndex, T result)> WhenAny<T>(params UnityTask<T>[] tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var adapter = AwaitableEnumerable<T>.Rent(tasks, tasks.Length);

            try
            {
                return new(Awaitables.WhenAny(adapter));
            }
            finally
            {
                adapter.Return();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<(int winArgumentIndex, T result)> WhenAny<T>(IEnumerable<UnityTask<T>> tasks)
        {
            DebuggingThrowHelper.ThrowIfNull(tasks);
            var adapter = AwaitableEnumerable<T>.Rent(tasks);

            try
            {
                return new(Awaitables.WhenAny(adapter));
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
            var adapter = AwaitableEnumerable.Rent(tasks, tasks.Length);

            try
            {
                return new(Awaitables.WhenAny(adapter));
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
            var adapter = AwaitableEnumerable.Rent(tasks);

            try
            {
                return new(Awaitables.WhenAny(adapter));
            }
            finally
            {
                adapter.Return();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAwaitableAsyncEnumerable<AwaitableWhenEachResult<T>> WhenEach<T>(params UnityTask<T>[] tasks)
            => Awaitables.WhenEach(AwaitableEnumerable<T>.CreatePersistent(tasks));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAwaitableAsyncEnumerable<AwaitableWhenEachResult<T>> WhenEach<T>(IEnumerable<UnityTask<T>> tasks)
            => Awaitables.WhenEach(AwaitableEnumerable<T>.CreatePersistent(tasks));

        private static async Awaitable WhenAllAndReturn(AwaitableEnumerable adapter)
        {
            try
            {
                await Awaitables.WhenAll(adapter);
            }
            finally
            {
                adapter.Return();
            }
        }

        private static async Awaitable<T[]> WhenAllAndReturn<T>(AwaitableEnumerable<T> adapter)
        {
            try
            {
                return await Awaitables.WhenAll(adapter);
            }
            finally
            {
                adapter.Return();
            }
        }

        private sealed class AwaitableEnumerable : IEnumerable<Awaitable>
        {
            private static readonly object s_poolLock = new();
            private static readonly Stack<AwaitableEnumerable> s_pool = new();

            private IEnumerable<UnityTask> _source;
            private UnityTask[] _array;
            private int _count;
            private bool _pooled;

            private AwaitableEnumerable() { }

            internal static AwaitableEnumerable Rent(IEnumerable<UnityTask> source)
            {
                var result = RentCore();
                result._source = source;
                return result;
            }

            internal static AwaitableEnumerable Rent(UnityTask[] array, int count)
            {
                var result = RentCore();
                result._array = array;
                result._count = count;
                return result;
            }

            internal IEnumerator<Awaitable> GetEnumerator()
            {
                if (_array != null)
                {
                    return new ArrayEnumerator(_array, _count);
                }

                return new SourceEnumerator(_source.GetEnumerator());
            }

            IEnumerator<Awaitable> IEnumerable<Awaitable>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

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

            private static AwaitableEnumerable RentCore()
            {
                AwaitableEnumerable result;

                lock (s_poolLock)
                {
                    result = s_pool.Count == 0 ? new() : s_pool.Pop();
                }

                result._pooled = true;
                return result;
            }

            private sealed class ArrayEnumerator : IEnumerator<Awaitable>
            {
                private readonly UnityTask[] _array;
                private readonly int _count;
                private int _index = -1;

                internal ArrayEnumerator(UnityTask[] array, int count)
                {
                    _array = array;
                    _count = count;
                }

                public Awaitable Current => _array[_index].AsAwaitable();

                object IEnumerator.Current => Current;

                public bool MoveNext()
                    => ++_index < _count;

                public void Reset()
                    => _index = -1;

                public void Dispose() { }
            }

            private sealed class SourceEnumerator : IEnumerator<Awaitable>
            {
                private readonly IEnumerator<UnityTask> _source;

                internal SourceEnumerator(IEnumerator<UnityTask> source)
                {
                    _source = source;
                }

                public Awaitable Current => _source.Current.AsAwaitable();

                object IEnumerator.Current => Current;

                public bool MoveNext()
                    => _source.MoveNext();

                public void Reset()
                    => _source.Reset();

                public void Dispose()
                    => _source.Dispose();
            }
        }

        private sealed class AwaitableEnumerable<T> : IEnumerable<Awaitable<T>>
        {
            private static readonly object s_poolLock = new();
            private static readonly Stack<AwaitableEnumerable<T>> s_pool = new();

            private IEnumerable<UnityTask<T>> _source;
            private UnityTask<T>[] _array;
            private int _count;
            private bool _pooled;

            private AwaitableEnumerable() { }

            private AwaitableEnumerable(IEnumerable<UnityTask<T>> source)
            {
                _source = source;
            }

            internal static AwaitableEnumerable<T> Rent(IEnumerable<UnityTask<T>> source)
            {
                var result = RentCore();
                result._source = source;
                return result;
            }

            internal static AwaitableEnumerable<T> Rent(UnityTask<T>[] array, int count)
            {
                var result = RentCore();
                result._array = array;
                result._count = count;
                return result;
            }

            internal static AwaitableEnumerable<T> CreatePersistent(IEnumerable<UnityTask<T>> source)
                => new(source);

            internal IEnumerator<Awaitable<T>> GetEnumerator()
            {
                if (_array != null)
                {
                    return new ArrayEnumerator(_array, _count);
                }

                return new SourceEnumerator(_source.GetEnumerator());
            }

            IEnumerator<Awaitable<T>> IEnumerable<Awaitable<T>>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

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

            private static AwaitableEnumerable<T> RentCore()
            {
                AwaitableEnumerable<T> result;

                lock (s_poolLock)
                {
                    result = s_pool.Count == 0 ? new() : s_pool.Pop();
                }

                result._pooled = true;
                return result;
            }

            private sealed class ArrayEnumerator : IEnumerator<Awaitable<T>>
            {
                private readonly UnityTask<T>[] _array;
                private readonly int _count;
                private int _index = -1;

                internal ArrayEnumerator(UnityTask<T>[] array, int count)
                {
                    _array = array;
                    _count = count;
                }

                public Awaitable<T> Current => _array[_index].AsAwaitable();

                object IEnumerator.Current => Current;

                public bool MoveNext()
                    => ++_index < _count;

                public void Reset()
                    => _index = -1;

                public void Dispose() { }
            }

            private sealed class SourceEnumerator : IEnumerator<Awaitable<T>>
            {
                private readonly IEnumerator<UnityTask<T>> _source;

                internal SourceEnumerator(IEnumerator<UnityTask<T>> source)
                {
                    _source = source;
                }

                public Awaitable<T> Current => _source.Current.AsAwaitable();

                object IEnumerator.Current => Current;

                public bool MoveNext()
                    => _source.MoveNext();

                public void Reset()
                    => _source.Reset();

                public void Dispose()
                    => _source.Dispose();
            }
        }
    }

    public static class UnityTaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith<T>(this UnityTask<T> task, Action<T> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith<T>(this UnityTask<T> task, Func<T, Awaitable> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<TR> ContinueWith<T, TR>(this UnityTask<T> task, Func<T, TR> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<TR> ContinueWith<T, TR>(
              this UnityTask<T> task
            , Func<T, Awaitable<TR>> continuationFunction
        )
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith(this UnityTask task, Action continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask ContinueWith(this UnityTask task, Func<Awaitable> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> ContinueWith<T>(this UnityTask task, Func<T> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> ContinueWith<T>(this UnityTask task, Func<Awaitable<T>> continuationFunction)
            => new(Awaitables.ContinueWith(task.AsAwaitable(), continuationFunction));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this UnityTask task)
            => Awaitables.Forget(task.AsAwaitable());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget<T>(this UnityTask<T> task)
            => Awaitables.Forget(task.AsAwaitable());
    }
}

#endif
