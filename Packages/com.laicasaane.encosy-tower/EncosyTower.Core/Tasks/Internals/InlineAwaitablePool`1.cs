#if !UNITASK || ENCOSY_UNITYTASK_AWAITABLE

using System.Collections.Generic;
using UnityEngine;

namespace EncosyTower.Tasks
{
    internal static class InlineAwaitablePool<T>
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_lock = new();
        private static readonly Stack<AwaitableCompletionSource<T>> s_pool = new();

        internal static Awaitable<T> FromResult(T value)
        {
            AwaitableCompletionSource<T> source;

            lock (s_lock)
            {
                source = s_pool.Count > 0 ? s_pool.Pop() : new();
            }

            source.SetResult(value);

            var awaitable = source.Awaitable;
            source.Reset();

            lock (s_lock)
            {
                if (s_pool.Count < MAX_POOL_SIZE)
                {
                    s_pool.Push(source);
                }
            }

            return awaitable;
        }
    }
}

#endif
