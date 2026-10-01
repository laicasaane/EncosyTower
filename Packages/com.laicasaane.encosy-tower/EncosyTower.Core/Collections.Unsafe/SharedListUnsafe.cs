using System;
using EncosyTower.Buffers;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Collections.Unsafe
{
    internal struct SharedListUnsafe<T>
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
        internal static unsafe SharedListUnsafe<T>* Alloc(
              T* buffer
            , int capacity
            , int* count
            , int* version
            , AllocatorStrategy allocator
        )
        {
            // SAFETY: The allocator returns writable storage for the complete unmanaged struct.
            unsafe
            {
                var data = allocator.Allocate<SharedListUnsafe<T>>();
                *data = new SharedListUnsafe<T> {
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
        internal static unsafe void Free(SharedListUnsafe<T>* data, AllocatorStrategy allocator)
        {
            if (data == null)
            {
                return;
            }

            // SAFETY: The pointer was allocated by the matching allocator and is freed once by its owner.
            unsafe
            {
                allocator.Free(data);
            }
        }

        internal readonly int Capacity => _capacity;

        /// <safety>The owner must keep the count pointer live for the complete read.</safety>
        internal readonly unsafe int Count
        {
            get
            {
                // SAFETY: The owning shared list keeps the count storage live while the header is borrowed.
                unsafe
                {
                    return *_count;
                }
            }
        }

        /// <safety>The owner must keep the version pointer live for the complete read.</safety>
        internal readonly unsafe int Version
        {
            get
            {
                // SAFETY: The owning shared list keeps the version storage live while the header is borrowed.
                unsafe
                {
                    return *_version;
                }
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe T this[int index]
        {
            get
            {
                // SAFETY: The caller keeps all borrowed pointers live; the index check bounds the access.
                unsafe
                {
                    ThrowHelper.ThrowIfIndexIsOutOfRange(
                          (uint)index < (uint)Count
                        , ThrowHelper.CollectionType.SharedListUnsafe
                    );

                    return _buffer[index];
                }
            }
            set
            {
                // SAFETY: The caller keeps all borrowed pointers live; the index check bounds the access.
                unsafe
                {
                    ThrowHelper.ThrowIfIndexIsOutOfRange(
                          (uint)index < (uint)Count
                        , ThrowHelper.CollectionType.SharedListUnsafe
                    );

                    IncrementVersion();
                    _buffer[index] = value;
                }
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void Add(T item)
        {
            // SAFETY: The caller keeps all borrowed pointers live; the capacity check bounds the append.
            unsafe
            {
                IncrementVersion();

                var count = Count;
                ThrowHelper.ThrowIfCapacityIsImmutable(
                      count < _capacity
                    , ThrowHelper.CollectionType.SharedListUnsafe
                );

                _buffer[count] = item;
                *_count = count + 1;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void Add(in T item)
        {
            // SAFETY: The caller keeps all borrowed pointers live; the capacity check bounds the append.
            unsafe
            {
                IncrementVersion();

                var count = Count;
                ThrowHelper.ThrowIfCapacityIsImmutable(
                      count < _capacity
                    , ThrowHelper.CollectionType.SharedListUnsafe
                );

                _buffer[count] = item;
                *_count = count + 1;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void Insert(int index, T item)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete insertion.
            unsafe
            {
                IncrementVersion();

                var count = Count;
                ThrowHelper.ThrowIfInsertionIndexIsOutOfRange(
                      (uint)index <= (uint)count
                    , ThrowHelper.CollectionType.SharedListUnsafe
                );
                ThrowHelper.ThrowIfCapacityIsImmutable(
                      count < _capacity
                    , ThrowHelper.CollectionType.SharedListUnsafe
                );

                GetBufferSpan()[index..count].CopyTo(GetBufferSpan()[(index + 1)..]);
                SetCount(count + 1);
                GetBufferSpan()[index] = item;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void Insert(int index, in T item)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete insertion.
            unsafe
            {
                Insert(index, item);
            }
        }

        /// <safety>The owner must remain live while the returned reference is used.</safety>
        internal unsafe ref T ElementAt(int index)
        {
            // SAFETY: The caller keeps all borrowed pointers live; the index check bounds the returned reference.
            unsafe
            {
                ThrowHelper.ThrowIfIndexIsOutOfRange(
                      (uint)index < (uint)Count
                    , ThrowHelper.CollectionType.SharedListUnsafe
                );

                return ref _buffer[index];
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void AddRange(ReadOnlySpan<T> items)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete append.
            unsafe
            {
                AddRange(items, items.Length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void AddRange(ReadOnlySpan<T> items, int count)
        {
            // SAFETY: The caller keeps all borrowed pointers live; the capacity check bounds the append.
            unsafe
            {
                IncrementVersion();

                if (count == 0)
                {
                    return;
                }

                var currentCount = Count;
                ThrowHelper.ThrowIfCapacityIsImmutable(
                      _capacity - currentCount >= count
                    , ThrowHelper.CollectionType.SharedListUnsafe
                );

                items[..count].CopyTo(GetBufferSpan().Slice(currentCount, count));
                SetCount(currentCount + count);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void AddRange(in NativeSlice<T> items)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete append.
            unsafe
            {
                AddRange(in items, items.Length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void AddRange(in NativeSlice<T> items, int count)
        {
            // SAFETY: The caller keeps all borrowed pointers live; the capacity check bounds the append.
            unsafe
            {
                IncrementVersion();

                if (count == 0)
                {
                    return;
                }

                var currentCount = Count;
                ThrowHelper.ThrowIfCapacityIsImmutable(
                      _capacity - currentCount >= count
                    , ThrowHelper.CollectionType.SharedListUnsafe
                );

                var destination = ConvertFrom(GetBufferSpan().Slice(currentCount, count));

                items.Slice(0, count).CopyTo(destination);
                SetCount(currentCount + count);

                return;
            }

            static NativeArray<T> ConvertFrom(Span<T> source)
            {
                var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray(source, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                var safety = AtomicSafetyHandle.GetTempMemoryHandle();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, safety);
#endif

                return array;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy.</safety>
        internal unsafe void CopyFrom(ReadOnlySpan<T> source)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyFrom(0, source);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy.</safety>
        internal unsafe void CopyFrom(ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyFrom(0, source, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy.</safety>
        internal unsafe void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyFrom(destinationStartIndex, source, source.Length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy.</safety>
        internal unsafe void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                new CopyFromSpan<T>(AsSpan()).CopyFrom(destinationStartIndex, source, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy attempt.</safety>
        internal unsafe bool TryCopyFrom(ReadOnlySpan<T> source)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyFrom(0, source);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy attempt.</safety>
        internal unsafe bool TryCopyFrom(ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyFrom(0, source, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy attempt.</safety>
        internal unsafe bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyFrom(destinationStartIndex, source, source.Length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy attempt.</safety>
        internal unsafe bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return new CopyFromSpan<T>(AsSpan()).TryCopyFrom(destinationStartIndex, source, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy.</safety>
        internal unsafe void CopyTo(Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyTo(0, destination);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy.</safety>
        internal unsafe void CopyTo(Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyTo(0, destination, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy.</safety>
        internal unsafe void CopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                CopyTo(sourceStartIndex, destination, destination.Length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy.</safety>
        internal unsafe void CopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy attempt.</safety>
        internal unsafe bool TryCopyTo(Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(0, destination);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy attempt.</safety>
        internal unsafe bool TryCopyTo(Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(0, destination, length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy attempt.</safety>
        internal unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(sourceStartIndex, destination, destination.Length);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy attempt.</safety>
        internal unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy attempt.
            unsafe
            {
                return new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);
            }
        }

        /// <safety>The owner must keep count and version pointers live.</safety>
        internal unsafe void Clear()
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete clear.
            unsafe
            {
                IncrementVersion();
                SetCount(0);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void RemoveAt(int index)
        {
            // SAFETY: The caller keeps all borrowed pointers live; range checks bound the move.
            unsafe
            {
                var count = Count;
                ThrowHelper.ThrowIfRemovalIndexIsOutOfRange((uint)index < (uint)count);

                IncrementVersion();

                count--;
                SetCount(count);

                if (index < count)
                {
                    GetBufferSpan().Slice(index + 1, count - index).CopyTo(GetBufferSpan()[index..]);
                }
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void RemoveRange(int startIndex, int length)
        {
            // SAFETY: The caller keeps all borrowed pointers live; range checks bound the move.
            unsafe
            {
                var count = Count;
                ThrowHelper.ThrowIfStartIndexIsOutOfRange((uint)startIndex < (uint)count);

                var end = startIndex + length;
                ThrowHelper.ThrowIfRemovalRangeIsOutOfRange((uint)end <= (uint)count);

                IncrementVersion();

                if (length < 1)
                {
                    return;
                }

                count -= length;
                SetCount(count);
                GetBufferSpan().Slice(end, count - startIndex).CopyTo(GetBufferSpan()[startIndex..]);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid list state.</safety>
        internal unsafe void RemoveAtSwapBack(int index)
        {
            // SAFETY: The caller keeps all borrowed pointers live; the index check bounds the swap.
            unsafe
            {
                var count = Count;
                ThrowHelper.ThrowIfRemovalIndexIsOutOfRange((uint)index < (uint)count);

                IncrementVersion();

                count--;
                SetCount(count);

                if (index < count)
                {
                    GetBufferSpan()[index] = GetBufferSpan()[count];
                }
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy.</safety>
        internal readonly unsafe T[] ToArray()
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                return AsReadOnlySpan().ToArray();
            }
        }

        /// <safety>The owner must remain live and unchanged while the returned span is used.</safety>
        internal unsafe Span<T> AsSpan()
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the returned span.
            unsafe
            {
                IncrementVersion();
                return GetBufferSpan()[..Count];
            }
        }

        /// <safety>The owner must remain live and unchanged while the returned span is used.</safety>
        internal readonly unsafe ReadOnlySpan<T> AsReadOnlySpan()
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the returned span.
            unsafe
            {
                return GetBufferSpan()[..Count];
            }
        }

        /// <safety>The owner must remain live and unchanged while the returned span is used.</safety>
        internal unsafe Span<T> AddReplicate(int amount)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the returned span.
            unsafe
            {
                var buffer = AddReplicateNoInit(amount);
                buffer.Clear();
                return buffer;
            }
        }

        /// <safety>The owner must remain live and unchanged while the returned span is used.</safety>
        internal unsafe Span<T> AddReplicate(T value, int amount)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the returned span.
            unsafe
            {
                var buffer = AddReplicateNoInit(amount);
                buffer.Fill(value);
                return buffer;
            }
        }

        /// <safety>The owner must remain live and unchanged while the returned span is used.</safety>
        internal unsafe Span<T> AddReplicateNoInit(int amount)
        {
            // SAFETY: The caller keeps all borrowed pointers live; the capacity check bounds the returned span.
            unsafe
            {
                IncrementVersion();

                var oldCount = Count;
                var newCount = amount + oldCount;
                ThrowHelper.ThrowIfCapacityIsImmutable(
                      newCount <= _capacity
                    , ThrowHelper.CollectionType.SharedListUnsafe
                );

                var result = GetBufferSpan().Slice(oldCount, amount);
                SetCount(newCount);
                return result;
            }
        }

        /// <safety>The owner must keep the version pointer live for the complete update.</safety>
        internal unsafe void IncrementVersion()
        {
            // SAFETY: The owning shared list keeps the version storage live while the header is borrowed.
            unsafe
            {
                (*_version)++;
            }
        }

        /// <safety>The owner must keep the buffer live and unchanged while the returned span is used.</safety>
        private readonly unsafe Span<T> GetBufferSpan()
        {
            // SAFETY: The owner pins a buffer with exactly the capacity recorded in this live header.
            unsafe
            {
                return new Span<T>(_buffer, _capacity);
            }
        }

        /// <safety>The owner must keep the count pointer live for the complete update.</safety>
        private unsafe void SetCount(int count)
        {
            // SAFETY: The owning shared list keeps the count storage live while the header is borrowed.
            unsafe
            {
                *_count = count;
            }
        }
    }
}
