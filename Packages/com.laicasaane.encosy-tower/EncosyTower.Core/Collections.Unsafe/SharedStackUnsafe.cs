using System;
using EncosyTower.Buffers;

namespace EncosyTower.Collections.Unsafe
{
    internal partial struct SharedStackUnsafe<T>
        where T : unmanaged
    {
        /// <safety>The owner pins this buffer and refreshes the pointer after relocation.</safety>
        internal unsafe T* _buffer;
        internal int _capacity;
        /// <safety>The owner keeps this borrowed scalar live until header disposal.</safety>
        internal unsafe int* _count;
        /// <safety>The owner keeps this borrowed scalar live until header disposal.</safety>
        internal unsafe int* _version;

        /// <safety>
        /// Every supplied pointer must address live pinned owner storage, capacity must match buffer length, and
        /// the returned persistent header must have exactly one owner.
        /// </safety>
        internal static unsafe SharedStackUnsafe<T>* Alloc(
              T* buffer
            , int capacity
            , int* count
            , int* version
            , AllocatorStrategy allocator
        )
        {
            // SAFETY: The allocator returns storage for a header whose pointers are supplied by the owning
            // shared stack.
            unsafe
            {
                var data = allocator.Allocate<SharedStackUnsafe<T>>();
                *data = new SharedStackUnsafe<T>
                {
                    _buffer = buffer,
                    _capacity = capacity,
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
        internal static unsafe void Free(SharedStackUnsafe<T>* data, AllocatorStrategy allocator)
        {
            if (data == null)
            {
                return;
            }
            // SAFETY: data is the single header allocated by Alloc and is freed exactly once by its owner.
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

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe void Push(T item)
        {
            // SAFETY: The owner pins the buffer and the fixed-capacity check bounds the write.
            unsafe
            {
                var count = *_count;
                ThrowHelper.ThrowIfCapacityIsImmutable(count < _capacity, ThrowHelper.CollectionType.SharedStackUnsafe);
                _buffer[count] = item;
                *_count = count + 1;
                (*_version)++;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe void Push(in T item)
        {
            // SAFETY: The owner pins the buffer and the fixed-capacity check bounds the write.
            unsafe
            {
                var count = *_count;
                ThrowHelper.ThrowIfCapacityIsImmutable(count < _capacity, ThrowHelper.CollectionType.SharedStackUnsafe);
                _buffer[count] = item;
                *_count = count + 1;
                (*_version)++;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe T Pop()
        {
            // SAFETY: The count check bounds the returned top element in the pinned buffer.
            unsafe
            {
                ThrowHelper.ThrowIfEmpty(*_count > 0, ThrowHelper.CollectionType.SharedStackUnsafe);
                var result = _buffer[--(*_count)];
                (*_version)++;
                return result;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe bool TryPop(out T result)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete attempt.
            unsafe
            {
                if (Count == 0)
                {
                    result = default;
                    return false;
                }

                result = Pop();
                return true;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe T Peek()
        {
            // SAFETY: The count check bounds the top element in the pinned buffer.
            unsafe
            {
                ThrowHelper.ThrowIfEmpty(*_count > 0, ThrowHelper.CollectionType.SharedStackUnsafe);
                return _buffer[*_count - 1];
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
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

        /// <safety>The owner must keep count and version pointers live.</safety>
        internal unsafe void Clear()
        {
            // SAFETY: The owner supplies live scalar pointers for the lifetime of the header.
            unsafe
            {
                *_count = 0;
                (*_version)++;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe T[] ToArray()
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                var result = new T[Count];
                CopyTopFirstTo(0, result, result.Length);
                return result;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe void CopyTo(Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyTo(0, destination);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe void CopyTo(Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyTo(0, destination, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe void CopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyTo(sourceStartIndex, destination, destination.Length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe void CopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                var count = Count;
                ThrowHelper.ThrowIfSourceStartIndexIsInvalid((uint)sourceStartIndex <= (uint)count);
                ThrowHelper.ThrowIfSourceLengthIsInvalid((uint)length <= (uint)(count - sourceStartIndex));
                ThrowHelper.ThrowIfDestinationLengthIsInvalid((uint)length <= (uint)destination.Length);
                CopyTopFirstTo(sourceStartIndex, destination, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe bool TryCopyTo(Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(0, destination);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe bool TryCopyTo(Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(0, destination, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
        internal unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(sourceStartIndex, destination, destination.Length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid stack state.</safety>
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

                CopyTopFirstTo(sourceStartIndex, destination, length);
                return true;
            }
        }

        /// <safety>The caller must provide a valid range over live stack storage.</safety>
        private unsafe void CopyTopFirstTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The caller-validated range bounds each top-first read in the pinned buffer.
            unsafe
            {
                var top = *_count - 1 - sourceStartIndex;
                for (var i = 0; i < length; i++)
                {
                    destination[i] = _buffer[top - i];
                }
            }
        }
    }
}
