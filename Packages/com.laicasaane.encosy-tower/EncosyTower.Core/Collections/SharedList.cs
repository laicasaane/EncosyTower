using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Types;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    [Serializable]
    public partial class SharedList<T> : SharedList<T, T>
        where T : unmanaged
    {
        public SharedList() : base()
        {
        }

        public SharedList(int capacity) : base(capacity)
        {
        }

        public SharedList([NotNull] params T[] source) : base(source)
        {
        }

        public SharedList(in ArraySegment<T> source) : base(source)
        {
        }

        public SharedList(in ReadOnlySpan<T> source) : base(source)
        {
        }

        public SharedList(in NativeArray<T> source) : base(source)
        {
        }

        public SharedList(in NativeSlice<T> source) : base(source)
        {
        }

        public SharedList([NotNull] ICollection<T> source) : base(source)
        {
        }

        public SharedList([NotNull] ICollection<T> source, int extraSize) : base(source, extraSize)
        {
        }

        public SharedList([NotNull] in SharedList<T, T> source) : base(source)
        {
        }
    }

    [Serializable]
    public partial class SharedList<T, TNative> : IList<T>, IReadOnlyList<T>, IIndexer<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>, IToArray<T>
        , ICopyFromSpan<T>, ITryCopyFromSpan<T>
        , ICopyToSpan<T>, ITryCopyToSpan<T>
        , IAddRangeSpan<T>, IContains<T>
        , IClearable, IDisposable
        , IIncreaseCapacity, IHasCount
        where T : unmanaged
        where TNative : unmanaged
    {
        [NonSerialized] internal BufferShared<T, TNative> _buffer;
        [NonSerialized] internal BufferShared<int> _count;
        [NonSerialized] internal BufferShared<int> _version;
        /// <safety>Owned by this list; borrowed native views become invalid after resize or disposal.</safety>
        [NonSerialized] internal unsafe SharedListUnsafe<TNative>* _nativeData;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        [NonSerialized] internal AtomicSafetyHandle _safety;
#endif

        public SharedList()
        {
            _buffer = new(0);
            _count = new(1);
            _version = new(1);
            InitializeNativeData();
        }

        public SharedList(int capacity)
        {
            _buffer = new(capacity);
            _count = new(1);
            _version = new(1);
            InitializeNativeData();
        }

        public SharedList([NotNull] params T[] source)
        {
            DebuggingThrowHelper.ThrowIfNull(source);
            _buffer = new(source);
            _count = new(1);
            _version = new(1);
            // SAFETY: This constructor exclusively owns the newly allocated count buffer.
            unsafe
            {
                _count[0] = source.Length;
            }
            InitializeNativeData();
        }

        public SharedList(in ArraySegment<T> source)
        {
            _buffer = new(source.ToArray());
            _count = new(1);
            _version = new(1);
            // SAFETY: This constructor exclusively owns the newly allocated count buffer.
            unsafe
            {
                _count[0] = source.Count;
            }
            InitializeNativeData();
        }

        public SharedList(in ReadOnlySpan<T> source)
        {
            _buffer = new(source.ToArray());
            _count = new(1);
            _version = new(1);
            // SAFETY: This constructor exclusively owns the newly allocated count buffer.
            unsafe
            {
                _count[0] = source.Length;
            }
            InitializeNativeData();
        }

        public SharedList([NotNull] ICollection<T> source)
        {
            DebuggingThrowHelper.ThrowIfNull(source);
            var buffer = new T[source.Count];
            source.CopyTo(buffer, 0);
            _buffer = new(buffer);
            _count = new(1);
            _version = new(1);
            // SAFETY: This constructor exclusively owns the newly allocated count buffer.
            unsafe
            {
                _count[0] = source.Count;
            }
            InitializeNativeData();
        }

        public SharedList([NotNull] ICollection<T> source, int extraSize)
        {
            DebuggingThrowHelper.ThrowIfNull(source);
            var buffer = new T[source.Count + extraSize];
            source.CopyTo(buffer, 0);
            _buffer = new(buffer);
            _count = new(1);
            _version = new(1);
            // SAFETY: This constructor exclusively owns the newly allocated count buffer.
            unsafe
            {
                _count[0] = source.Count;
            }
            InitializeNativeData();
        }

        public SharedList(in NativeArray<TNative> source)
        {
            _buffer = new(source.Length);
            // SAFETY: This constructor owns the destination buffer and consumes the live source synchronously.
            unsafe
            {
                _buffer.AsNativeArray().CopyFrom(source);
            }
            _count = new(1);
            _version = new(1);
            // SAFETY: This constructor exclusively owns the newly allocated count buffer.
            unsafe
            {
                _count[0] = source.Length;
            }
            InitializeNativeData();
        }

        public SharedList(in NativeSlice<TNative> source)
        {
            _buffer = new(source.Length);
            // SAFETY: This constructor owns the destination buffer and consumes the live source synchronously.
            unsafe
            {
                source.CopyTo(_buffer.AsNativeArray());
            }
            _count = new(1);
            _version = new(1);
            // SAFETY: This constructor exclusively owns the newly allocated count buffer.
            unsafe
            {
                _count[0] = source.Length;
            }
            InitializeNativeData();
        }

        ~SharedList()
        {
            Dispose();
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CountRO;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckRead();
                return _buffer.Capacity;
            }
        }

        public bool IsReadOnly => false;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelper.ThrowIfIndexIsOutOfRange(
                      (uint)index < (uint)CountRO
                    , ThrowHelper.CollectionType.SharedListWithNative
                );
                // SAFETY: The range check bounds access to this list's live owned buffer.
                unsafe
                {
                    return _buffer.AsReadOnlySpan()[index];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ThrowHelper.ThrowIfIndexIsOutOfRange(
                      (uint)index < (uint)CountRO
                    , ThrowHelper.CollectionType.SharedListWithNative
                );
                VersionRW++;
                // SAFETY: The range and write checks bound access to this list's live owned buffer.
                unsafe
                {
                    _buffer.AsSpan()[index] = value;
                }
            }
        }

        /// <safety>The returned native view must not outlive the source list or survive resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator SharedListNative<TNative>(SharedList<T, TNative> list)
        {
            DebuggingThrowHelper.ThrowIfNull(list);

            // SAFETY: The non-null list remains the designated owner of the returned view.
            // SAFETY: The header was freed first; this list now releases its remaining owned buffers.
            unsafe
            {
                return list.AsNative();
            }
        }

        public void Dispose()
        {
            if (_buffer.IsCreated == false)
            {
                return;
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckDeallocateAndThrow(_safety);
#endif

            // SAFETY: The established ownership and safety checks keep the native storage live
            // for this pointer dereference.
            unsafe
            {
                SharedListUnsafe<TNative>.Free(_nativeData, Allocator.Persistent);
                _nativeData = null;
            }

            // SAFETY: The header was freed first; this list now releases its remaining owned buffers.
            unsafe
            {
                _buffer.Dispose();
                _count.Dispose();
                _version.Dispose();
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.Release(_safety);
            _safety = default;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
            => IndexOf(item, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, int index)
            => IndexOf(item, index, CountRO - index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, int index, int count)
        {
            ThrowHelper.ThrowIfIndexIsNegative(index >= 0);
            ThrowHelper.ThrowIfCountIsNegative(count >= 0);
            ThrowHelper.ThrowIfIndexSectionIsInvalid(
                  index + count <= CountRO
                , ThrowHelper.CollectionType.SharedListWithNative
            );
            return Array.IndexOf(_buffer.AsManagedArray(), item, index, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item)
            => IndexOf(in item, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index)
            => IndexOf(in item, index, CountRO - index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index, int count)
        {
            ThrowHelper.ThrowIfIndexIsNegative(index >= 0);
            ThrowHelper.ThrowIfCountIsNegative(count >= 0);
            ThrowHelper.ThrowIfIndexSectionIsInvalid(
                  index + count <= CountRO
                , ThrowHelper.CollectionType.SharedListWithNative
            );
            return Array.IndexOf(_buffer.AsManagedArray(), item, index, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            VersionRW++;

            ref var count = ref CountRW;

            if (count == _buffer.Capacity)
            {
                AllocateMore();
            }

            // SAFETY: Capacity is available and count is a bounded index into this owned buffer.
            unsafe
            {
                _buffer.AsSpan()[count++] = item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            VersionRW++;

            ref var count = ref CountRW;

            if (count == _buffer.Capacity)
            {
                AllocateMore();
            }

            // SAFETY: Capacity is available and count is a bounded index into this owned buffer.
            unsafe
            {
                _buffer.AsSpan()[count++] = item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item)
        {
            VersionRW++;

            ref var count = ref CountRW;

            ThrowHelper.ThrowIfInsertionIndexIsOutOfRange(
                  (uint)index <= (uint)count
                , ThrowHelper.CollectionType.SharedListWithNative
            );

            if (count == _buffer.Capacity)
            {
                AllocateMore();
            }

            var buffer = _buffer.AsManagedArray();
            Array.Copy(buffer, index, buffer, index + 1, count - index);
            ++count;

            // SAFETY: The insertion checks and capacity growth bound the destination index.
            unsafe
            {
                _buffer.AsSpan()[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item)
        {
            VersionRW++;

            ref var count = ref CountRW;

            ThrowHelper.ThrowIfInsertionIndexIsOutOfRange(
                  (uint)index <= (uint)count
                , ThrowHelper.CollectionType.SharedListWithNative
            );

            if (count == _buffer.Capacity)
            {
                AllocateMore();
            }

            var buffer = _buffer.AsManagedArray();
            Array.Copy(buffer, index, buffer, index + 1, count - index);
            ++count;

            // SAFETY: The insertion checks and capacity growth bound the destination index.
            unsafe
            {
                _buffer.AsSpan()[index] = item;
            }
        }

        /// <safety>The returned reference must not outlive this list or survive resize or mutation.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T ElementAt(int index)
        {
            ThrowHelper.ThrowIfIndexIsOutOfRange(
                  (uint)index < (uint)CountRO
                , ThrowHelper.CollectionType.SharedListWithNative
            );
            // SAFETY: The range check bounds the returned reference to this live owned buffer.
            unsafe
            {
                return ref _buffer.AsSpan()[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange([NotNull] T[] items)
        {
            DebuggingThrowHelper.ThrowIfNull(items);
            AddRange(items, items.Length);
        }

        public void AddRange([NotNull] T[] items, int count)
        {
            DebuggingThrowHelper.ThrowIfNull(items);
            VersionRW++;

            if (count == 0)
            {
                return;
            }

            if (_buffer.Capacity - CountRO < count)
            {
                AllocateMore(checked(CountRO + count));
            }

            // SAFETY: Capacity and count checks bound the synchronous copy into owned storage.
            unsafe
            {
                items.AsSpan()[..count].CopyTo(_buffer.AsSpan().Slice(CountRO, count));
            }
            CountRW += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<T> items)
            => AddRange(items, items.Length);

        public void AddRange(ReadOnlySpan<T> items, int count)
        {
            VersionRW++;

            if (count == 0)
            {
                return;
            }

            if (_buffer.Capacity - CountRO < count)
            {
                AllocateMore(checked(CountRO + count));
            }

            // SAFETY: Capacity and count checks bound the synchronous copy into owned storage.
            unsafe
            {
                items[..count].CopyTo(_buffer.AsSpan().Slice(CountRO, count));
            }
            CountRW += count;
        }

        public void AddRange([NotNull] IEnumerable<T> collection)
        {
            DebuggingThrowHelper.ThrowIfNull(collection);

            if (collection is ICollection<T> c)
            {
                var count = c.Count;

                if (count > 0)
                {
                    if (_buffer.Capacity - CountRO < count)
                    {
                        AllocateMore(checked(CountRO + count));
                    }

                    c.CopyTo(_buffer.AsManagedArray(), CountRO);
                    CountRW += count;
                    VersionRW++;
                }
            }
            else
            {
                using IEnumerator<T> en = collection.GetEnumerator();

                while (en.MoveNext())
                {
                    Add(en.Current);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            var count = CountRO;
            return count > 0 && Array.IndexOf(_buffer.AsManagedArray(), item, 0, count) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            VersionRW++;
            CountRW = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] destination, int destinationIndex)
            => CopyTo(destination.AsSpan().Slice(destinationIndex, CountRO));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<T> source)
            => CopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<T> source, int length)
            => CopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => CopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The copy consumes the checked owner-backed span before this method returns.
            unsafe
            {
                new CopyFromSpan<T>(AsSpan()).CopyFrom(destinationStartIndex, source, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(ReadOnlySpan<T> source)
            => TryCopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(ReadOnlySpan<T> source, int length)
            => TryCopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => TryCopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The copy consumes the checked owner-backed span before this method returns.
            unsafe
            {
                return new CopyFromSpan<T>(AsSpan()).TryCopyFrom(destinationStartIndex, source, length);
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The copy consumes the checked owner-backed span before this method returns.
            unsafe
            {
                new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The copy consumes the checked owner-backed span before this method returns.
            unsafe
            {
                return new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);
            }
        }

        /// <safety>This list must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Enumerator GetEnumerator()
        {
            // SAFETY: The enumerator borrows this list and preserves version checks.
            unsafe
            {
                return new(AsReadOnly());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IncreaseCapacityBy(int amount)
            => IncreaseCapacityTo(_buffer.Capacity + amount);

        public int IncreaseCapacityTo(int newCapacity)
        {
            VersionRW++;

            if (newCapacity <= _buffer.Capacity)
            {
                return _buffer.Capacity;
            }

            ResizeBuffer(newCapacity);
            return _buffer.Capacity;
        }

        public bool Remove(T item)
        {
            VersionRW++;

            var index = IndexOf(item);

            if ((uint)index >= (uint)CountRO)
            {
                return false;
            }

            if (index < --CountRW)
            {
                Array.Copy(_buffer.AsManagedArray(), index + 1, _buffer.AsManagedArray(), index, CountRO - index);
            }

            if (EncosyTypeExtensions.IsUnmanaged<T>() == false)
            {
                // SAFETY: The decremented count identifies the vacated slot in owned storage.
                unsafe
                {
                    _buffer.AsSpan()[CountRO] = default;
                }
            }

            return true;
        }

        public bool Remove(in T item)
        {
            VersionRW++;

            var index = IndexOf(item);

            if ((uint)index >= (uint)CountRO)
            {
                return false;
            }

            if (index < --CountRW)
            {
                Array.Copy(_buffer.AsManagedArray(), index + 1, _buffer.AsManagedArray(), index, CountRO - index);
            }

            if (EncosyTypeExtensions.IsUnmanaged<T>() == false)
            {
                // SAFETY: The decremented count identifies the vacated slot in owned storage.
                unsafe
                {
                    _buffer.AsSpan()[CountRO] = default;
                }
            }

            return true;
        }

        public void RemoveAt(int index)
        {
            ThrowHelper.ThrowIfRemovalIndexIsOutOfRange((uint)index < (uint)CountRO);

            VersionRW++;

            if (index < --CountRW)
            {
                var buffer = _buffer.AsManagedArray();
                Array.Copy(buffer, index + 1, buffer, index, CountRO - index);
            }
        }

        public void RemoveRange(int startIndex, int length)
        {
            var count = CountRO;

            ThrowHelper.ThrowIfStartIndexIsOutOfRange((uint)startIndex < (uint)count);

            var end = startIndex + length;

            ThrowHelper.ThrowIfRemovalRangeIsOutOfRange((uint)end <= (uint)count);

            VersionRW++;

            if (length < 1)
            {
                return;
            }

            count = CountRW -= length;

            var buffer = _buffer.AsManagedArray();

            Array.Copy(buffer, startIndex + length, buffer, startIndex, count - startIndex);
        }

        public void RemoveAtSwapBack(int index)
        {
            ThrowHelper.ThrowIfRemovalIndexIsOutOfRange((uint)index < (uint)CountRO);

            VersionRW++;

            if (index < --CountRW)
            {
                // SAFETY: Removal checks and updated count bound both source and destination indices.
                unsafe
                {
                    var buffer = _buffer.AsSpan();
                    buffer[index] = buffer[CountRO];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            // SAFETY: ToArray copies the owner-backed span before this method returns.
            unsafe
            {
                return AsReadOnlySpan().ToArray();
            }
        }

        /// <safety>The returned span must not outlive this list or survive resize or mutation.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<T> AsSpan()
        {
            VersionRW++;

            // SAFETY: This list owns the live buffer for the returned mutable span lifetime.
            unsafe
            {
                return _buffer.AsSpan()[..CountRO];
            }
        }

        /// <safety>The returned span must not outlive this list or survive resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnlySpan<T> AsReadOnlySpan()
        {
            // SAFETY: This list owns the live buffer for the returned read-only span lifetime.
            unsafe
            {
                return _buffer.AsReadOnlySpan()[..CountRO];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            VersionRW++;

            if (CountRO < _buffer.Capacity)
            {
                ResizeBuffer(CountRO);
            }
        }

        /// <safety>The returned span must not outlive this list or survive another mutation.</safety>
        public unsafe Span<T> AddReplicate(int amount)
        {
            VersionRW++;

            var oldCount = CountRO;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Capacity;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            Span<T> buffer;

            // SAFETY: Growth completed and the requested range is bounded by the new capacity.
            unsafe
            {
                buffer = _buffer.AsSpan().Slice(oldCount, amount);
            }
            buffer.Fill(default);
            CountRW = newCount;

            return buffer;
        }

        /// <safety>The returned span must not outlive this list or survive another mutation.</safety>
        public unsafe Span<T> AddReplicate(T value, int amount)
        {
            VersionRW++;

            var oldCount = CountRO;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Capacity;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            Span<T> buffer;

            // SAFETY: Growth completed and the requested range is bounded by the new capacity.
            unsafe
            {
                buffer = _buffer.AsSpan().Slice(oldCount, amount);
            }
            buffer.Fill(value);
            CountRW = newCount;

            return buffer;
        }

        /// <safety>The returned span must not outlive this list or survive another mutation.</safety>
        public unsafe Span<T> AddReplicateNoInit(int amount)
        {
            VersionRW++;

            var oldCount = CountRO;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Capacity;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            Span<T> buffer;

            // SAFETY: Growth completed and the requested range is bounded by the new capacity.
            unsafe
            {
                buffer = _buffer.AsSpan().Slice(oldCount, amount);
            }
            CountRW = newCount;

            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SharedList<T, TNative> Prefill(int amount)
        {
            var list = new SharedList<T, TNative>(amount);

            // SAFETY: The new list owns the returned span and does not expose it.
            unsafe
            {
                list.AddReplicate(amount);
            }
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SharedList<T, TNative> Prefill(T value, int amount)
        {
            var list = new SharedList<T, TNative>(amount);

            // SAFETY: The new list owns the returned span and does not expose it.
            unsafe
            {
                list.AddReplicate(value, amount);
            }
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int CalcNewCapacity(int newSize)
        {
            newSize = Math.Max(4, newSize);
            return checked(((int)Math.Ceiling(newSize * 1.5f) / 4) * 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AllocateMore()
        {
            var newCapacity = CalcNewCapacity(_buffer.Capacity + 1);
            ResizeBuffer(newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AllocateMore(int newSize)
        {
            ThrowHelper.ThrowIfNewCapacityIsInvalid(newSize > _buffer.Capacity);

            var newCapacity = CalcNewCapacity(newSize);
            ResizeBuffer(newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeNativeData()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            _safety = AtomicSafetyHandle.Create();
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(_safety, true);
#endif

            // SAFETY: The managed buffer and shared counters remain pinned for the native view lifetime.
            unsafe
            {
                _nativeData = SharedListUnsafe<TNative>.Alloc(
                      _buffer.GetUnsafeBufferPointer()
                    , _buffer.Capacity
                    , _count.GetUnsafeBufferPointer()
                    , _version.GetUnsafeBufferPointer()
                    , Allocator.Persistent
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RefreshNativeData()
        {
            // SAFETY: The shared native header is live and all pointers are refreshed from still-owned storage.
            unsafe
            {
                _nativeData->_buffer = _buffer.GetUnsafeBufferPointer();
                _nativeData->_capacity = _buffer.Capacity;
                _nativeData->_count = _count.GetUnsafeBufferPointer();
                _nativeData->_version = _version.GetUnsafeBufferPointer();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeBuffer(int newCapacity)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(_safety);
#endif
            // SAFETY: This list owns the buffer and invalidated secondary aliases before relocation.
            unsafe
            {
                _buffer.Resize(newCapacity);
            }

            RefreshNativeData();
        }

        internal ref readonly int CountRO
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckRead();
                // SAFETY: The read check validates the owned scalar count buffer.
                unsafe
                {
                    return ref _count[0];
                }
            }
        }

        internal ref int CountRW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckWrite();
                // SAFETY: The write check validates the owned scalar count buffer.
                unsafe
                {
                    return ref _count[0];
                }
            }
        }

        internal ref readonly int VersionRO
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckRead();
                // SAFETY: The read check validates the owned scalar version buffer.
                unsafe
                {
                    return ref _version[0];
                }
            }
        }

        internal ref int VersionRW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckWrite();
                // SAFETY: The write check validates the owned scalar version buffer.
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

        /// <safety>This list must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            // SAFETY: The interface enumerator borrows this list and preserves version checks.
            unsafe
            {
                return GetEnumerator();
            }
        }

        /// <safety>This list must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator IEnumerable.GetEnumerator()
        {
            // SAFETY: The interface enumerator borrows this list and preserves version checks.
            unsafe
            {
                return GetEnumerator();
            }
        }
    }
}
