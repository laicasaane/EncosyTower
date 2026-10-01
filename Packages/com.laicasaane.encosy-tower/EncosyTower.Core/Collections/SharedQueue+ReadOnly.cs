using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    public partial class SharedQueue<T, TNative>
        where T : unmanaged
        where TNative : unmanaged
    {
        public readonly struct ReadOnly : IReadOnlyCollection<T>, IHasCapacity, IHasCount, IIsCreated, ITryCopyToSpan<T>
        {
            internal readonly SharedQueue<T, TNative> _queue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ReadOnly(SharedQueue<T, TNative> queue)
            {
                DebuggingThrowHelper.ThrowIfNull(queue);
                _queue = queue;
            }

            public readonly bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _queue != null && _queue.IsCreated;
            }

            public readonly int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _queue?.Count ?? 0;
            }

            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _queue?.Capacity ?? 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly T Peek()
                => _queue.Peek();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryPeek(out T value)
                => _queue.TryPeek(out value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly T[] ToArray()
                => _queue.ToArray();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(Span<T> destination)
                => _queue.CopyTo(destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(Span<T> destination, int length)
                => _queue.CopyTo(destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(int sourceStartIndex, Span<T> destination)
                => _queue.CopyTo(sourceStartIndex, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(int sourceStartIndex, Span<T> destination, int length)
                => _queue.CopyTo(sourceStartIndex, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(Span<T> destination)
                => _queue.TryCopyTo(destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(Span<T> destination, int length)
                => _queue.TryCopyTo(destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination)
                => _queue.TryCopyTo(sourceStartIndex, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
                => _queue.TryCopyTo(sourceStartIndex, destination, length);

            /// <safety>The queue must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly unsafe Enumerator GetEnumerator()
            {
                // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
                unsafe
                {
                    return new(this);
                }
            }

            /// <safety>The queue must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this queue view.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>The queue must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe IEnumerator IEnumerable.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this queue view.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>The returned native alias must not outlive the queue or survive resize.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly unsafe SharedQueueNative<TNative>.ReadOnly AsNative()
            {
                // SAFETY: The managed queue owns both aliases for the complete borrowed lifetime.
                unsafe
                {
                    return _queue.AsNative().AsReadOnly();
                }
            }

            /// <safety>The returned alias must not outlive the source queue.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnly(SharedQueue<T, TNative> queue)
            {
                DebuggingThrowHelper.ThrowIfNull(queue);

                // SAFETY: The non-null queue remains the designated owner of the returned alias.
                unsafe
                {
                    return new(queue);
                }
            }
        }
    }
}
