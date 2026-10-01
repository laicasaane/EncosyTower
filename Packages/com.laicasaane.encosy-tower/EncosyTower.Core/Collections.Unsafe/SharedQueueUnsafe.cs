using System;
using EncosyTower.Buffers;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Collections.Unsafe
{
    internal partial struct SharedQueueUnsafe<T>
        where T : unmanaged
    {
        /// <safety>The owner pins this buffer and refreshes the pointer after relocation.</safety>
        internal unsafe T* _buffer;
        internal int _capacity;
        /// <safety>The owner keeps this borrowed scalar live until header disposal.</safety>
        internal unsafe int* _head;
        /// <safety>The owner keeps this borrowed scalar live until header disposal.</safety>
        internal unsafe int* _tail;
        /// <safety>The owner keeps this borrowed scalar live until header disposal.</safety>
        internal unsafe int* _count;
        /// <safety>The owner keeps this borrowed scalar live until header disposal.</safety>
        internal unsafe int* _version;

        /// <safety>
        /// Every supplied pointer must address live pinned owner storage, capacity must match buffer length, and
        /// the returned persistent header must have exactly one owner.
        /// </safety>
        internal static unsafe SharedQueueUnsafe<T>* Alloc(
              T* buffer
            , int capacity
            , int* head
            , int* tail
            , int* count
            , int* version
            , AllocatorStrategy allocator
        )
        {
            // SAFETY: The allocator returns storage for a header whose pointers are all supplied by the
            // owning shared object.
            unsafe
            {
                var data = allocator.Allocate<SharedQueueUnsafe<T>>();
                *data = new SharedQueueUnsafe<T>
                {
                    _buffer = buffer,
                    _capacity = capacity,
                    _head = head,
                    _tail = tail,
                    _count = count,
                    _version = version,
                };
                return data;
            }
        }

        /// <safety>
        /// Null is a no-op. Otherwise data must come from the matching allocator, be released once, and have no
        /// aliases used afterward.
        /// </safety>
        internal static unsafe void Free(SharedQueueUnsafe<T>* data, AllocatorStrategy allocator)
        {
            if (data == null)
            {
                return;
            }
            // SAFETY: data is the single header allocated by Alloc and is freed after the owner stops exposing it.
            unsafe
            {
                allocator.Free(data);
            }
        }

        internal int Capacity => _capacity;

        /// <safety>The owner must keep the count pointer live for the complete read.</safety>
        internal unsafe int Count
        {
            get
            {
                // SAFETY: The owning collection keeps the native header live while this property reads its state.
                unsafe
                {
                    return *_count;
                }
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe void Enqueue(T item)
        {
            // SAFETY: The owning shared queue pins the buffer and validates the fixed-capacity contract
            // before forwarding.
            unsafe
            {
                var count = *_count;
                ThrowHelper.ThrowIfCapacityIsImmutable(count < _capacity, ThrowHelper.CollectionType.SharedQueueUnsafe);
                UnsafeUtility.ArrayElementAsRef<T>(_buffer, *_tail) = item;
                MoveNext(_tail);
                *_count = count + 1;
                (*_version)++;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe void Enqueue(in T item)
        {
            // SAFETY: The owning shared queue pins the buffer and validates the fixed-capacity contract
            // before forwarding.
            unsafe
            {
                var count = *_count;
                ThrowHelper.ThrowIfCapacityIsImmutable(count < _capacity, ThrowHelper.CollectionType.SharedQueueUnsafe);
                UnsafeUtility.ArrayElementAsRef<T>(_buffer, *_tail) = item;
                MoveNext(_tail);
                *_count = count + 1;
                (*_version)++;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe T Dequeue()
        {
            // SAFETY: The shared owner pins the backing storage and the count check bounds the head index.
            unsafe
            {
                ThrowHelper.ThrowIfEmpty(*_count > 0, ThrowHelper.CollectionType.SharedQueueUnsafe);
                var result = UnsafeUtility.ArrayElementAsRef<T>(_buffer, *_head);
                MoveNext(_head);
                (*_count)--;
                (*_version)++;
                return result;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe bool TryDequeue(out T result)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete attempt.
            unsafe
            {
                if (Count == 0)
                {
                    result = default;
                    return false;
                }

                result = Dequeue();
                return true;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe T Peek()
        {
            // SAFETY: The shared owner pins the backing storage and the count check bounds the head index.
            unsafe
            {
                ThrowHelper.ThrowIfEmpty(*_count > 0, ThrowHelper.CollectionType.SharedQueueUnsafe);
                return UnsafeUtility.ArrayElementAsRef<T>(_buffer, *_head);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe bool TryPeek(out T result)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete attempt.
            unsafe
            {
                if (Count == 0)
                {
                    result = default;
                    return false;
                }

                result = Peek();
                return true;
            }
        }

        /// <safety>The owner must keep head, tail, count, and version pointers live.</safety>
        internal unsafe void Clear()
        {
            // SAFETY: The owner supplies live scalar pointers for the lifetime of the header.
            unsafe
            {
                *_head = *_tail = *_count = 0;
                (*_version)++;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe T[] ToArray()
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                var result = new T[Count];
                CopyLinearTo(result);
                return result;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe void CopyTo(Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyTo(0, destination);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe void CopyTo(Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyTo(0, destination, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe void CopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyTo(sourceStartIndex, destination, destination.Length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe void CopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                var count = Count;
                ThrowHelper.ThrowIfSourceStartIndexIsInvalid((uint)sourceStartIndex <= (uint)count);
                ThrowHelper.ThrowIfSourceLengthIsInvalid((uint)length <= (uint)(count - sourceStartIndex));
                ThrowHelper.ThrowIfDestinationLengthIsInvalid((uint)length <= (uint)destination.Length);
                CopyRingTo(sourceStartIndex, destination, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe bool TryCopyTo(Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(0, destination);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe bool TryCopyTo(Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(0, destination, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(sourceStartIndex, destination, destination.Length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        internal unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                var count = Count;
                if (
                    (uint)sourceStartIndex > (uint)count
                    || (uint)length > (uint)(count - sourceStartIndex)
                    || (uint)length > (uint)destination.Length
                )
                {
                    return false;
                }

                CopyRingTo(sourceStartIndex, destination, length);
                return true;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid ring state.</safety>
        private unsafe void CopyLinearTo(Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyRingTo(0, destination, Count);
            }
        }

        /// <safety>The caller must provide a valid range over live ring storage.</safety>
        private unsafe void CopyRingTo(int sourceStartIndex, Span<T> destination, int length)
        {
            if (length == 0)
            {
                return;
            }
            // SAFETY: The caller-validated range and the ring pointers describe a live, bounded sequence
            // in the pinned buffer.
            unsafe
            {
                var start = *_head + sourceStartIndex;
                if (start >= _capacity)
                {
                    start -= _capacity;
                }

                var first = Math.Min(length, _capacity - start);
                new ReadOnlySpan<T>(_buffer + start, first).CopyTo(destination);
                if (length > first)
                {
                    new ReadOnlySpan<T>(_buffer, length - first).CopyTo(destination[first..]);
                }
            }
        }

        /// <safety>The pointer must identify one live ring-index scalar and ring capacity must be valid.</safety>
        private unsafe void MoveNext(int* index)
        {
            // SAFETY: index points to one of the live scalar fields owned by this header.
            unsafe
            {
                if (++(*index) == _capacity)
                {
                    *index = 0;
                }
            }
        }
    }
}
