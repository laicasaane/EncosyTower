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
    public partial class SharedStack<T> : SharedStack<T, T>
        where T : unmanaged
    {
        public SharedStack()
            : base()
        {
        }

        public SharedStack(int capacity)
            : base(capacity)
        {
        }

        public SharedStack(ReadOnlySpan<T> source)
            : base(source)
        {
        }

        public SharedStack([NotNull] ICollection<T> source)
            : base(source)
        {
        }
    }

    [Serializable]
    public partial class SharedStack<T, TNative>
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
        [NonSerialized] internal BufferShared<int> _count;
        [NonSerialized] internal BufferShared<int> _version;
        /// <safety>Owned by this stack; borrowed native views become invalid after resize or disposal.</safety>
        [NonSerialized] internal unsafe SharedStackUnsafe<TNative>* _nativeData;
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        [NonSerialized] internal AtomicSafetyHandle _safety;
#endif

        public SharedStack()
        {
            _buffer = new(0);
            _count = new(1);
            _version = new(1);
            InitializeNativeData();
        }

        public SharedStack(int capacity)
        {
            ThrowHelper.ThrowIfCapacityIsInvalid(capacity >= 0);
            _buffer = new(capacity);
            _count = new(1);
            _version = new(1);
            InitializeNativeData();
        }

        public SharedStack(ReadOnlySpan<T> source)
            : this(source.Length)
        {
            // SAFETY: This owner keeps its pinned buffer live for the construction copy.
            unsafe
            {
                source.CopyTo(_buffer.AsSpan());
            }
            CountRW = source.Length;
            RefreshNativeData();
        }

        public SharedStack([NotNull] ICollection<T> source)
            : this(GetCount(source))
        {
            source.CopyTo(_buffer.AsManagedArray(), 0);
            CountRW = source.Count;
            RefreshNativeData();
        }

        ~SharedStack()
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

        public void Push(T item)
        {
            VersionRW++;
            if (CountRO == Capacity)
            {
                AllocateMore(checked(CountRO + 1));
            }

            var index = CountRO;
            // SAFETY: This owner keeps its pinned buffer live and index is within capacity.
            unsafe
            {
                _buffer.AsSpan()[index] = item;
            }
            CountRW = index + 1;
        }

        public void Push(in T item)
        {
            VersionRW++;
            if (CountRO == Capacity)
            {
                AllocateMore(checked(CountRO + 1));
            }

            var index = CountRO;
            // SAFETY: This owner keeps its pinned buffer live and index is within capacity.
            unsafe
            {
                _buffer.AsSpan()[index] = item;
            }
            CountRW = index + 1;
        }

        public void PushRange(ReadOnlySpan<T> items)
        {
            if (items.Length == 0)
            {
                return;
            }

            VersionRW++;
            var old = Count;
            var required = checked(old + items.Length);

            if (required > Capacity)
            {
                AllocateMore(required);
            }

            // SAFETY: Capacity checks bound the append into this owner's live pinned buffer.
            unsafe
            {
                items.CopyTo(_buffer.AsSpan()[old..]);
            }
            CountRW = required;
        }

        public T Pop()
        {
            ThrowHelper.ThrowIfEmpty(Count > 0, ThrowHelper.CollectionType.SharedStack);
            VersionRW++;
            // SAFETY: Count validates the top index in this owner's live pinned buffer.
            unsafe
            {
                return _buffer.AsReadOnlySpan()[--CountRW];
            }
        }

        public bool TryPop(out T value)
        {
            if (Count == 0)
            {
                value = default;
                return false;
            }
            value = Pop();
            return true;
        }

        public T Peek()
        {
            ThrowHelper.ThrowIfEmpty(Count > 0, ThrowHelper.CollectionType.SharedStack);
            // SAFETY: Count validates the top index in this owner's live pinned buffer.
            unsafe
            {
                return _buffer.AsReadOnlySpan()[Count - 1];
            }
        }

        public bool TryPeek(out T value)
        {
            if (Count == 0)
            {
                value = default;
                return false;
            }
            value = Peek();
            return true;
        }

        public void Clear()
        {
            CountRW = 0;
            VersionRW++;
        }

        public T[] ToArray()
        {
            var result = new T[Count];
            CopyTopFirstTo(0, result, result.Length);
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
            CopyTopFirstTo(sourceStartIndex, destination, length);
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

            CopyTopFirstTo(sourceStartIndex, destination, length);
            return true;
        }

        private void CopyTopFirstTo(int sourceStartIndex, Span<T> destination, int length)
        {
            ReadOnlySpan<T> span;

            // SAFETY: This owner keeps its pinned buffer live for the complete bounded copy.
            unsafe
            {
                span = _buffer.AsReadOnlySpan();
            }
            var top = Count - 1 - sourceStartIndex;
            for (var i = 0; i < length; i++)
            {
                destination[i] = span[top - i];
            }
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

        /// <safety>The returned alias must not outlive this stack.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnly AsReadOnly()
        {
            // SAFETY: The caller accepts the returned view's borrowed owner lifetime.
            unsafe
            {
                return new(this);
            }
        }

        /// <safety>This stack must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Enumerator GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return new(AsReadOnly());
            }
        }

        /// <safety>This stack must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return GetEnumerator();
            }
        }

        /// <safety>This stack must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator IEnumerable.GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return GetEnumerator();
            }
        }

        /// <safety>The returned native view must not outlive this stack or survive resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe SharedStackNative<TNative> AsNative()
        {
            CheckRead();
            // SAFETY: This stack owns the live header and buffers borrowed by the returned view.
            unsafe
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                var safety = _safety;
                AtomicSafetyHandle.UseSecondaryVersion(ref safety);
                return new SharedStackNative<TNative>(_nativeData, safety);
#else
                return new SharedStackNative<TNative>(_nativeData);
#endif
            }
        }

        /// <safety>The returned native view must not outlive the source stack or survive resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator SharedStackNative<TNative>(SharedStack<T, TNative> stack)
        {
            DebuggingThrowHelper.ThrowIfNull(stack);
            // SAFETY: The non-null stack remains the designated owner of the returned view.
            unsafe
            {
                return stack.AsNative();
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
            // SAFETY: This stack owns the header and releases it before every buffer.
            unsafe
            {
                SharedStackUnsafe<TNative>.Free(_nativeData, Allocator.Persistent);
                _nativeData = null;
            }
            // SAFETY: The header was freed first; this stack now releases its remaining owned buffers.
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

        private void InitializeNativeData()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            _safety = AtomicSafetyHandle.Create();
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(_safety, true);
#endif
            // SAFETY: This stack owns every pinned buffer passed to its newly allocated header.
            unsafe
            {
                _nativeData = SharedStackUnsafe<TNative>.Alloc(
                      _buffer.GetUnsafeBufferPointer()
                    , _buffer.Capacity
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
                _nativeData->_count = _count.GetUnsafeBufferPointer();
                _nativeData->_version = _version.GetUnsafeBufferPointer();
            }
        }

        private void AllocateMore(int newSize)
        {
            newSize = Math.Max(4, newSize);
            var capacity = checked(((int)Math.Ceiling(newSize * 1.5f) / 4) * 4);
            ResizeBuffer(capacity);
        }

        private void ResizeBuffer(int capacity)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(_safety);
#endif
            // SAFETY: This stack owns the buffer and invalidated secondary aliases before relocation.
            unsafe
            {
                _buffer.Resize(capacity);
            }
            RefreshNativeData();
        }

        internal ref readonly int CountRO
        {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetCount([NotNull] ICollection<T> source)
        {
            DebuggingThrowHelper.ThrowIfNull(source);
            return source.Count;
        }
    }
}
