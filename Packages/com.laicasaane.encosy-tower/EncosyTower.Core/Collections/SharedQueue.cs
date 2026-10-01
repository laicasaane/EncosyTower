using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    [Serializable]
    public partial class SharedQueue<T> : SharedQueue<T, T>
        where T : unmanaged
    {
        public SharedQueue()
            : base()
        {
        }

        public SharedQueue(int capacity)
            : base(capacity)
        {
        }

        public SharedQueue(ReadOnlySpan<T> source)
            : base(source)
        {
        }

        public SharedQueue([NotNull] ICollection<T> source)
            : base(source)
        {
        }
    }

    [Serializable]
    public partial class SharedQueue<T, TNative>
        : IReadOnlyCollection<T>
        , IHasCapacity, IIncreaseCapacity
        , IHasCount
        , IClearable
        , IDisposable
        , ICopyToSpan<T>
        , ITryCopyToSpan<T>
        where T : unmanaged
        where TNative : unmanaged
    {
        [NonSerialized] internal BufferShared<T, TNative> _buffer;
        [NonSerialized] internal BufferShared<int> _head;
        [NonSerialized] internal BufferShared<int> _tail;
        [NonSerialized] internal BufferShared<int> _count;
        [NonSerialized] internal BufferShared<int> _version;
        /// <safety>Owned by this queue; borrowed native views become invalid after resize or disposal.</safety>
        [NonSerialized] internal unsafe SharedQueueUnsafe<TNative>* _nativeData;
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        [NonSerialized] internal AtomicSafetyHandle _safety;
#endif

        public SharedQueue()
        {
            _buffer = new(0);
            _head = new(1);
            _tail = new(1);
            _count = new(1);
            _version = new(1);
            InitializeNativeData();
        }

        public SharedQueue(int capacity)
        {
            ThrowHelper.ThrowIfCapacityIsInvalid(capacity >= 0);
            _buffer = new(capacity);
            _head = new(1);
            _tail = new(1);
            _count = new(1);
            _version = new(1);
            InitializeNativeData();
        }

        public SharedQueue(ReadOnlySpan<T> source)
            : this(source.Length)
        {
            // SAFETY: This owner keeps its pinned buffer live for the construction copy.
            unsafe
            {
                source.CopyTo(_buffer.AsSpan());
            }
            CountRW = source.Length;
            TailRW = CountRO == Capacity ? 0 : CountRO;
            RefreshNativeData();
        }

        public SharedQueue([NotNull] ICollection<T> source)
            : this(GetCount(source))
        {
            source.CopyTo(_buffer.AsManagedArray(), 0);
            CountRW = source.Count;
            TailRW = CountRO == Capacity ? 0 : CountRO;
            RefreshNativeData();
        }

        ~SharedQueue()
            => Dispose();

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.IsCreated;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.IsCreated ? CountRO : 0;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!_buffer.IsCreated)
                {
                    return 0;
                }
                CheckRead();
                return _buffer.Capacity;
            }
        }

        public void Enqueue(T item)
        {
            VersionRW++;
            if (CountRO == Capacity)
            {
                AllocateMore(checked(CountRO + 1));
            }

            // SAFETY: This owner keeps its pinned buffer live and TailRO is within capacity.
            unsafe
            {
                _buffer.AsSpan()[TailRO] = item;
            }
            MoveNext(ref _tail, Capacity);
            CountRW++;
        }

        public void Enqueue(in T item)
        {
            VersionRW++;
            if (CountRO == Capacity)
            {
                AllocateMore(checked(CountRO + 1));
            }

            // SAFETY: This owner keeps its pinned buffer live and TailRO is within capacity.
            unsafe
            {
                _buffer.AsSpan()[TailRO] = item;
            }
            MoveNext(ref _tail, Capacity);
            CountRW++;
        }

        public void EnqueueRange(ReadOnlySpan<T> items)
        {
            if (items.Length == 0)
            {
                return;
            }

            VersionRW++;
            var required = checked(CountRO + items.Length);
            if (required > Capacity)
            {
                AllocateMore(required);
            }

            var first = Math.Min(items.Length, Capacity - TailRO);
            // SAFETY: Capacity checks bound both writes into this owner's live pinned buffer.
            unsafe
            {
                items[..first].CopyTo(_buffer.AsSpan().Slice(TailRO, first));
            }
            if (items.Length > first)
            {
                // SAFETY: Capacity checks bound this wrapped write into the live pinned buffer.
                unsafe
                {
                    items[first..].CopyTo(_buffer.AsSpan()[..(items.Length - first)]);
                }
            }

            TailRW += items.Length;
            if (TailRO >= Capacity)
            {
                TailRW -= Capacity;
            }

            CountRW += items.Length;
        }

        public T Dequeue()
        {
            ThrowHelper.ThrowIfEmpty(CountRO > 0, ThrowHelper.CollectionType.SharedQueue);
            T result;
            // SAFETY: This owner keeps its pinned buffer live and HeadRO is within the occupied ring.
            unsafe
            {
                result = _buffer.AsReadOnlySpan()[HeadRO];
            }
            MoveNext(ref _head, Capacity);
            CountRW--;
            VersionRW++;
            return result;
        }

        public bool TryDequeue(out T value)
        {
            if (CountRO == 0)
            {
                value = default;
                return false;
            }
            value = Dequeue();
            return true;
        }

        public T Peek()
        {
            ThrowHelper.ThrowIfEmpty(CountRO > 0, ThrowHelper.CollectionType.SharedQueue);
            // SAFETY: This owner keeps its pinned buffer live and HeadRO is within the occupied ring.
            unsafe
            {
                return _buffer.AsReadOnlySpan()[HeadRO];
            }
        }

        public bool TryPeek(out T value)
        {
            if (CountRO == 0)
            {
                value = default;
                return false;
            }
            value = Peek();
            return true;
        }

        public void Clear()
        {
            HeadRW = 0;
            TailRW = 0;
            CountRW = 0;
            VersionRW++;
        }

        public T[] ToArray()
        {
            var result = new T[Count];
            CopyLinearTo(result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> destination)
            => CopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> destination, int length)
            => CopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<T> destination)
            => CopyTo(sourceStartIndex, destination, destination.Length);

        public void CopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            var count = Count;
            ThrowHelper.ThrowIfSourceStartIndexIsInvalid((uint)sourceStartIndex <= (uint)count);
            ThrowHelper.ThrowIfSourceLengthIsInvalid((uint)length <= (uint)(count - sourceStartIndex));
            ThrowHelper.ThrowIfDestinationLengthIsInvalid((uint)length <= (uint)destination.Length);
            CopyRingTo(sourceStartIndex, destination, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<T> destination)
            => TryCopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<T> destination, int length)
            => TryCopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(int sourceStartIndex, Span<T> destination)
            => TryCopyTo(sourceStartIndex, destination, destination.Length);

        public bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IncreaseCapacityBy(int amount)
            => IncreaseCapacityTo(checked(Capacity + amount));

        public int IncreaseCapacityTo(int capacity)
        {
            ThrowHelper.ThrowIfCapacityBelowCount(capacity >= Count);

            if (capacity > Capacity)
            {
                ResizeBuffer(capacity);
            }

            return Capacity;
        }

        public void Trim()
        {
            if (Capacity > Count)
            {
                ResizeBuffer(Count);
            }
        }

        /// <safety>The returned alias must not outlive this queue.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnly AsReadOnly()
        {
            // SAFETY: The caller accepts the returned view's borrowed owner lifetime.
            unsafe
            {
                return new(this);
            }
        }

        /// <safety>This queue must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Enumerator GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return new(AsReadOnly());
            }
        }

        /// <safety>This queue must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return GetEnumerator();
            }
        }

        /// <safety>This queue must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator IEnumerable.GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return GetEnumerator();
            }
        }

        /// <safety>The returned native view must not outlive this queue or survive resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe SharedQueueNative<TNative> AsNative()
        {
            CheckRead();
            // SAFETY: This queue owns the live header and buffers borrowed by the returned view.
            unsafe
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                var safety = _safety;
                AtomicSafetyHandle.UseSecondaryVersion(ref safety);
                return new SharedQueueNative<TNative>(_nativeData, safety);
#else
                return new SharedQueueNative<TNative>(_nativeData);
#endif
            }
        }

        /// <safety>The returned native view must not outlive the source queue or survive resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator SharedQueueNative<TNative>(SharedQueue<T, TNative> queue)
        {
            DebuggingThrowHelper.ThrowIfNull(queue);

            // SAFETY: The caller accepts the returned view's borrowed owner lifetime.
            unsafe
            {
                return queue.AsNative();
            }
        }

        public void Dispose()
        {
            if (!_buffer.IsCreated)
            {
                return;
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckDeallocateAndThrow(_safety);
#endif
            // SAFETY: This queue owns the header and releases it before every buffer.
            unsafe
            {
                SharedQueueUnsafe<TNative>.Free(_nativeData, Allocator.Persistent);
                _nativeData = null;
            }
            // SAFETY: The header was freed first; this queue now releases its remaining owned buffers.
            unsafe
            {
                _buffer.Dispose();
                _head.Dispose();
                _tail.Dispose();
                _count.Dispose();
                _version.Dispose();
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.Release(_safety);
            _safety = default;
#endif
        }

        private void InitializeNativeData()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            _safety = AtomicSafetyHandle.Create();
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(_safety, true);
#endif
            // SAFETY: This queue owns every pinned buffer passed to its newly allocated header.
            unsafe
            {
                _nativeData = SharedQueueUnsafe<TNative>.Alloc(
                      _buffer.GetUnsafeBufferPointer()
                    , _buffer.Capacity
                    , _head.GetUnsafeBufferPointer()
                    , _tail.GetUnsafeBufferPointer()
                    , _count.GetUnsafeBufferPointer()
                    , _version.GetUnsafeBufferPointer()
                    , Allocator.Persistent
                );
            }
        }

        private void RefreshNativeData()
        {
            // SAFETY: The header remains live and all pointers are refreshed from current pinned storage.
            unsafe
            {
                _nativeData->_buffer = _buffer.GetUnsafeBufferPointer();
                _nativeData->_capacity = _buffer.Capacity;
                _nativeData->_head = _head.GetUnsafeBufferPointer();
                _nativeData->_tail = _tail.GetUnsafeBufferPointer();
                _nativeData->_count = _count.GetUnsafeBufferPointer();
                _nativeData->_version = _version.GetUnsafeBufferPointer();
            }
        }

        private void AllocateMore(int newSize)
        {
            newSize = Math.Max(4, newSize);
            var capacity = checked(((int)Math.Ceiling(newSize * 1.5f) / 4) * 4);

            Linearize();
            ResizeBuffer(capacity);
        }

        private void ResizeBuffer(int newCapacity)
        {
            Linearize();
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(_safety);
#endif
            // SAFETY: This class is the designated owner and has invalidated old aliases before resize.
            unsafe
            {
                _buffer.Resize(newCapacity);
            }
            RefreshNativeData();
            TailRW = Count == Capacity ? 0 : Count;
        }

        private void Linearize()
        {
            if (Count == 0 || HeadRO == 0)
            {
                return;
            }

            var values = ToArray();
            // SAFETY: This owner keeps its resized pinned buffer live for the complete copy.
            unsafe
            {
                values.AsSpan().CopyTo(_buffer.AsSpan());
            }
            HeadRW = 0;
            TailRW = Count == Capacity ? 0 : Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CopyLinearTo(Span<T> destination)
            => CopyRingTo(0, destination, Count);

        private void CopyRingTo(int sourceStartIndex, Span<T> destination, int length)
        {
            if (length == 0)
            {
                return;
            }

            var capacity = Capacity;
            var start = HeadRO + sourceStartIndex;
            if (start >= capacity)
            {
                start -= capacity;
            }

            var first = Math.Min(length, capacity - start);
            // SAFETY: Validated ring ranges bound both reads from this owner's live pinned buffer.
            unsafe
            {
                _buffer.AsReadOnlySpan().Slice(start, first).CopyTo(destination);
            }

            if (length > first)
            {
                // SAFETY: Validated ring ranges bound this wrapped read from the live pinned buffer.
                unsafe
                {
                    _buffer.AsReadOnlySpan()[..(length - first)].CopyTo(destination[first..]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetCount([NotNull] ICollection<T> source)
        {
            DebuggingThrowHelper.ThrowIfNull(source);
            return source.Count;
        }

        private static void MoveNext(ref BufferShared<int> index, int capacity)
        {
            var value = index[0] + 1;
            if (value == capacity)
            {
                value = 0;
            }

            index[0] = value;
        }

        internal ref readonly int HeadRO
        {
            get
            {
                CheckRead();
                // SAFETY: This owner keeps the head scalar pinned for the returned internal reference.
                unsafe
                {
                    return ref _head[0];
                }
            }
        }

        internal ref int HeadRW
        {
            get
            {
                CheckWrite();
                // SAFETY: This owner keeps the head scalar pinned for the returned internal reference.
                unsafe
                {
                    return ref _head[0];
                }
            }
        }

        internal ref readonly int TailRO
        {
            get
            {
                CheckRead();
                // SAFETY: This owner keeps the tail scalar pinned for the returned internal reference.
                unsafe
                {
                    return ref _tail[0];
                }
            }
        }

        internal ref int TailRW
        {
            get
            {
                CheckWrite();
                // SAFETY: This owner keeps the tail scalar pinned for the returned internal reference.
                unsafe
                {
                    return ref _tail[0];
                }
            }
        }

        internal ref readonly int CountRO
        {
            get
            {
                CheckRead();
                // SAFETY: This owner keeps the count scalar pinned for the returned internal reference.
                unsafe
                {
                    return ref _count[0];
                }
            }
        }

        internal ref int CountRW
        {
            get
            {
                CheckWrite();
                // SAFETY: This owner keeps the count scalar pinned for the returned internal reference.
                unsafe
                {
                    return ref _count[0];
                }
            }
        }

        internal ref readonly int VersionRO
        {
            get
            {
                CheckRead();
                // SAFETY: This owner keeps the version scalar pinned for the returned internal reference.
                unsafe
                {
                    return ref _version[0];
                }
            }
        }

        internal ref int VersionRW
        {
            get
            {
                CheckWrite();
                // SAFETY: This owner keeps the version scalar pinned for the returned internal reference.
                unsafe
                {
                    return ref _version[0];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckRead()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckReadAndThrow(_safety);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckWrite()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(_safety);
#endif
        }
    }
}
