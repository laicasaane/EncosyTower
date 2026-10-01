// https://github.com/stella3d/SharedArray

// MIT License
//
// Copyright(c) 2020 Stella Cannefax
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify,
// merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
// OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.CompilerServices.Exposed;
using EncosyTower.Buffers;
using EncosyTower.Common;
using EncosyTower.Collections.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    [Serializable]
    public partial class SharedArray<T> : SharedArray<T, T>
        where T : unmanaged
    {
        public SharedArray(int size) : base(size)
        {
        }

        public SharedArray([NotNull] T[] source) : base(source)
        {
        }

        public SharedArray(in ArraySegment<T> source) : base(source)
        {
        }

        public SharedArray(ReadOnlySpan<T> source) : base(source)
        {
        }

        public SharedArray([NotNull] ICollection<T> source) : base(source)
        {
        }

        public SharedArray([NotNull] ICollection<T> source, int extraSize) : base(source, extraSize)
        {
        }
    }

    [Serializable]
    public partial class SharedArray<T, TNative> : IDisposable, IClearable, IEnumerable<T>, IIndexer<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>, IAsMemory<T>, IAsReadOnlyMemory<T>
        , IAsNativeArray<TNative>, IAsNativeSlice<TNative>, IHasLength
        where T : unmanaged
        where TNative : unmanaged
    {
        [NonSerialized] internal BufferShared<T, TNative> _buffer;
        [NonSerialized] internal int _version;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        [NonSerialized] private AtomicSafetyHandle _safety;
#endif

        protected SharedArray()
        {
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());
            Initialize(Array.Empty<T>());
        }

        public SharedArray(int size)
        {
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());
            ThrowHelper.ThrowIfSizeNegative(size >= 0);
            Initialize(size == 0 ? Array.Empty<T>() : new T[size]);
        }

        public SharedArray([NotNull] T[] source)
        {
            DebuggingThrowHelper.ThrowIfNull(source);
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());
            Initialize(source);
        }

        public SharedArray(in ArraySegment<T> source) : this(source.ToArray())
        {
        }

        public SharedArray(ReadOnlySpan<T> source) : this(source.ToArray())
        {
        }

        public SharedArray(in NativeArray<TNative> source)
        {
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());

            var managed = new T[source.Length];
            Initialize(managed);

            // SAFETY: This owner keeps its fixed pinned storage live for the complete construction copy.
            unsafe
            {
                AsNativeArray().CopyFrom(source);
            }
        }

        public SharedArray(in NativeSlice<TNative> source)
        {
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());

            var managed = new T[source.Length];
            Initialize(managed);

            // SAFETY: This owner keeps its fixed pinned storage live for the complete construction copy.
            unsafe
            {
                source.CopyTo(AsNativeArray());
            }
        }

        public SharedArray([NotNull] ICollection<T> source)
        {
            DebuggingThrowHelper.ThrowIfNull(source);
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());

            var managed = new T[source.Count];
            source.CopyTo(managed, 0);
            Initialize(managed);
        }

        public SharedArray([NotNull] ICollection<T> source, int extraSize)
        {
            DebuggingThrowHelper.ThrowIfNull(source);
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());

            var managed = new T[source.Count + extraSize];
            source.CopyTo(managed, 0);
            Initialize(managed);
        }

        ~SharedArray()
        {
            // SAFETY: Finalization owns the remaining buffer and no managed caller can use this instance afterward.
            unsafe
            {
                Dispose();
            }
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckRead();
                return _buffer.Capacity;
            }
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckRead();
                ThrowHelper.ThrowIfIndexIsOutOfRange(
                      (uint)index < (uint)_buffer.Capacity
                    , ThrowHelper.CollectionType.SharedArray
                );
                // SAFETY: This owner keeps fixed pinned storage live; the index check bounds the read.
                unsafe
                {
                    return _buffer[index];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CheckWrite();
                ThrowHelper.ThrowIfIndexIsOutOfRange(
                      (uint)index < (uint)_buffer.Capacity
                    , ThrowHelper.CollectionType.SharedArray
                );
                _version++;

                // SAFETY: This owner keeps fixed pinned storage live; the index check bounds the write.
                unsafe
                {
                    _buffer[index] = value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T[]([NotNull] SharedArray<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return self.AsManagedArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ArraySegment<T>([NotNull] SharedArray<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return self.AsArraySegment();
        }

        /// <safety>The returned span must not be used after array disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator Span<T>([NotNull] SharedArray<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);

            // SAFETY: The caller accepts the borrowed span lifetime returned by AsSpan.
            unsafe
            {
                return self.AsSpan();
            }
        }

        /// <safety>The returned span must not be used after array disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator ReadOnlySpan<T>([NotNull] SharedArray<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);

            // SAFETY: The caller accepts the borrowed span lifetime returned by AsReadOnlySpan.
            unsafe
            {
                return self.AsReadOnlySpan();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Memory<T>([NotNull] SharedArray<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return self.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>([NotNull] SharedArray<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return self.AsReadOnlyMemory();
        }

        /// <safety>The returned alias must not be disposed or used after array disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator NativeArray<TNative>([NotNull] SharedArray<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);

            // SAFETY: The caller accepts the borrowed alias lifetime returned by AsNativeArray.
            unsafe
            {
                return self.AsNativeArray();
            }
        }

        /// <safety>The returned alias must not be used after array disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator NativeSlice<TNative>([NotNull] SharedArray<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);

            // SAFETY: The caller accepts the borrowed alias lifetime returned by AsNativeSlice.
            unsafe
            {
                return self.AsNativeSlice();
            }
        }

        /// <safety>The returned view must not be used after array disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator SharedArrayNative<TNative>([NotNull] SharedArray<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);

            // SAFETY: The caller accepts the borrowed view lifetime returned by AsNative.
            unsafe
            {
                return self.AsNative();
            }
        }

        /// <safety>The array must remain alive while the returned reference is used.</safety>
        public unsafe ref T GetPinnableReference()
        {
            CheckWrite();
            _version++;

            // SAFETY: This owner keeps fixed pinned storage live; Capacity determines whether element zero exists.
            unsafe
            {
                if (_buffer.Capacity > 0)
                {
                    return ref _buffer[0];
                }

                return ref UnsafeExposed.NullRef<T>();
            }
        }

        public void Clear()
        {
            CheckWrite();

            // SAFETY: This owner keeps fixed pinned storage live and CheckWrite validates exclusive access.
            unsafe
            {
                _buffer.Clear();
            }
        }

        /// <safety>The array must remain alive and unchanged while the returned enumerator is used.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Enumerator GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return new(this);
            }
        }

        public void Dispose()
        {
            if (_buffer.IsCreated == false)
            {
                return;
            }

            _version++;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckDeallocateAndThrow(_safety);
#endif

            // SAFETY: This class owns the live pin and has passed its deallocation checks.
            unsafe
            {
                _buffer.Dispose();
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.Release(_safety);
            _safety = default;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] AsManagedArray()
        {
            CheckWrite();
            return _buffer.AsManagedArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegment<T> AsArraySegment()
        {
            CheckWrite();
            return _buffer.AsArraySegment();
        }

        /// <safety>The array must remain alive while the returned span is used.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<T> AsSpan()
        {
            CheckWrite();

            // SAFETY: This owner keeps fixed pinned storage live and CheckWrite validates access.
            unsafe
            {
                return _buffer.AsSpan();
            }
        }

        /// <safety>The array must remain alive while the returned span is used.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnlySpan<T> AsReadOnlySpan()
        {
            CheckRead();

            // SAFETY: This owner keeps fixed pinned storage live and CheckRead validates access.
            unsafe
            {
                return _buffer.AsReadOnlySpan();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
        {
            CheckWrite();
            return _buffer.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
        {
            CheckRead();
            return _buffer.AsReadOnlyMemory();
        }

        /// <safety>The returned alias must not be disposed or used after array disposal.</safety>
        public unsafe NativeArray<TNative> AsNativeArray()
        {
            CheckNativeAliasAccess();

            NativeArray<TNative> alias;

            // SAFETY: This owner keeps fixed pinned storage live for the returned borrowed alias.
            unsafe
            {
                alias = _buffer.AsNativeArray();
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            var aliasSafety = _safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref aliasSafety);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref alias, aliasSafety);
#endif
            return alias;
        }

        /// <safety>The returned slice must not outlive this array owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe NativeSlice<TNative> AsNativeSlice()
        {
            // SAFETY: The caller accepts the borrowed alias lifetime returned by AsNativeArray.
            unsafe
            {
                return new(AsNativeArray());
            }
        }

        /// <safety>The returned view must not be used after array disposal.</safety>
        public unsafe SharedArrayNative<TNative> AsNative()
        {
            CheckNativeAliasAccess();

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            var aliasSafety = _safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref aliasSafety);
#endif

            // SAFETY: Owner keeps the pinned buffer live for the returned fixed-length borrowed view.
            unsafe
            {
                return new SharedArrayNative<TNative>(
                      _buffer.GetUnsafeBufferPointer()
                    , _buffer.Capacity
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                    , aliasSafety
#endif
                );
            }
        }

        /// <safety>The returned pointer must not outlive this array owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe TNative* GetUnsafeBufferPointer()
        {
            CheckWrite();

            // SAFETY: This owner keeps fixed pinned storage live for the returned pointer.
            unsafe
            {
                return _buffer.GetUnsafeBufferPointer();
            }
        }

        internal void Initialize(T[] managed)
        {
            _version++;
            var buffer = new BufferShared<T, TNative>(managed);

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            var safety = default(AtomicSafetyHandle);
#endif

            try
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                safety = AtomicSafetyHandle.Create();
                AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(safety, true);
#endif
                _buffer = buffer;
                buffer = default;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                _safety = safety;
                safety = default;
#endif
            }
            finally
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                if (AtomicSafetyHandle.IsDefaultValue(safety) == false)
                {
                    AtomicSafetyHandle.Release(safety);
                }
#endif
                // SAFETY: buffer is the only owner of any unpublished pin remaining after the transaction.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckRead()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(_buffer);

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckReadAndThrow(_safety);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckWrite()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(_buffer);

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(_safety);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckNativeAliasAccess()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(_buffer);

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(_safety);
#endif
        }

        /// <safety>This array must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return GetEnumerator();
            }
        }

        /// <safety>This array must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator IEnumerable.GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return GetEnumerator();
            }
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly SharedArray<T, TNative> _sharedArray;
            private readonly T[] _managed;
            private readonly int _version;
            private readonly int _length;
            private int _index;
            private Option<T> _current;

            public Enumerator([NotNull] SharedArray<T, TNative> sharedArray)
            {
                DebuggingThrowHelper.ThrowIfNull(sharedArray);
                sharedArray.CheckRead();

                _sharedArray = sharedArray;
                _managed = sharedArray._buffer.AsManagedArray();
                _version = sharedArray._version;
                _length = sharedArray._buffer.Capacity;
                _index = -1;
                _current = Option.None;
            }

            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current.GetValueOrThrow();
            }

            public bool MoveNext()
            {
                if (_version == _sharedArray._version && (uint)(_index + 1) < (uint)_length)
                {
                    _index++;
                    _current = _managed[_index];
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                ThrowHelper.ThrowIfCollectionWasModified(
                      _version == _sharedArray._version
                    , ThrowHelper.CollectionType.SharedArray
                );

                _index = _length + 1;
                _current = Option.None;
                return false;
            }

            void IEnumerator.Reset()
            {
                ThrowHelper.ThrowIfCollectionWasModified(
                      _version == _sharedArray._version
                    , ThrowHelper.CollectionType.SharedArray
                );

                _index = -1;
                _current = Option.None;
            }

            readonly object IEnumerator.Current
            {
                get
                {
                    ThrowHelper.ThrowIfEnumeratorOperationIsInvalid(
                          (uint)_index < (uint)_length
                        , ThrowHelper.CollectionType.SharedArray
                    );

                    return Current;
                }
            }

            public readonly void Dispose()
            {
            }
        }
    }
}
