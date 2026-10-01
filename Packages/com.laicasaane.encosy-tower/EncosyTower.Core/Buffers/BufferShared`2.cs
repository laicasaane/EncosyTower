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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using CollectionsThrowHelper = EncosyTower.Collections.ThrowHelper;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Buffers
{
    public struct BufferShared<T, TNative> : IBuffer<T>, IRefIndexer<T>, IAsMemory<T>, IAsReadOnlyMemory<T>
        , IAsNativeArray<TNative>, IAsNativeSlice<TNative>, IAsNativeSliceReadOnly<TNative>
        where T : unmanaged
        where TNative : unmanaged
    {
        internal GCHandle _gcHandle;

        /// <safety>
        /// The designated owner keeps the managed array pinned. Copies only borrow this pointer and become invalid
        /// after owner resize or disposal.
        /// </safety>
        [NativeDisableUnsafePtrRestriction]
        internal unsafe void* _buffer;
        internal int _capacity;

        public BufferShared(int capacity) : this()
        {
            CollectionsThrowHelper.ThrowIfSizeNegative(capacity >= 0);
            CollectionsThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(
                UnsafeAPI.AreTypesEqualSize<T, TNative>()
            );
            Create(new T[capacity]);
        }

        public BufferShared([NotNull] T[] buffer) : this()
        {
            DebuggingThrowHelper.ThrowIfNull(buffer);
            CollectionsThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(
                UnsafeAPI.AreTypesEqualSize<T, TNative>()
            );
            Create(buffer);
        }

        /// <safety>The handle must pin the array addressed by buffer for the complete borrowed lifetime.</safety>
        internal unsafe BufferShared(GCHandle gcHandle, void* buffer, int capacity)
        {
            _gcHandle = gcHandle;

            // SAFETY: Caller provides a borrowed pointer kept pinned by gcHandle.
            unsafe
            {
                _buffer = buffer;
            }

            _capacity = capacity;
        }

        /// <safety>The designated owner must remain alive and must not resize or dispose during access.</safety>
        public readonly unsafe ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                DebuggingThrowHelper.ThrowIfNotCreated(this);
                CollectionsThrowHelper.ThrowIfIndexOutOfRangeException((uint)index < (uint)_capacity);

                // SAFETY: Created owner keeps _buffer pinned and index validation keeps access within capacity.
                unsafe
                {
                    return ref UnsafeUtility.ArrayElementAsRef<T>(_buffer, index);
                }
            }
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _gcHandle.IsAllocated;
        }

        /// <safety>The returned view must not be used after owner resize or disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator ReadOnly(BufferShared<T, TNative> buffer)
        {
            // SAFETY: The caller accepts the borrowed view lifetime returned by AsReadOnly.
            unsafe
            {
                return buffer.AsReadOnly();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Alloc(int size)
            => Alloc(size, default, memClear: true);

        public void Alloc(int size, AllocatorStrategy allocator, bool memClear = true)
        {
            ThrowHelper.ThrowIfBufferAlreadyAllocated(IsCreated == false);
            CollectionsThrowHelper.ThrowIfSizeNegative(size >= 0);
            CollectionsThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(
                UnsafeAPI.AreTypesEqualSize<T, TNative>()
            );
            Create(size == 0 ? Array.Empty<T>() : new T[size]);
        }

        /// <safety>The caller must be the designated owner and stop borrowers before resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Resize(int newSize)
        {
            // SAFETY: The caller is the designated owner and accepts invalidation of old aliases.
            unsafe
            {
                Resize(newSize, copyContent: true, memClear: true);
            }
        }

        /// <safety>The caller must be the designated owner and stop borrowers before resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Resize(int newSize, bool copyContent)
        {
            // SAFETY: The caller is the designated owner and accepts invalidation of old aliases.
            unsafe
            {
                Resize(newSize, copyContent, memClear: true);
            }
        }

        /// <safety>
        /// The caller must be the designated owner. No borrower may access old storage during or after resize.
        /// </safety>
        public unsafe void Resize(int newSize, bool copyContent, bool memClear)
        {
            CollectionsThrowHelper.ThrowIfSizeNegative(newSize >= 0);
            ThrowHelper.ThrowIfResizeUninitializedBuffer(IsCreated);

            if (newSize == _capacity)
            {
                return;
            }

            var nextHandle = default(GCHandle);

            try
            {
                var nextArray = new T[newSize];
                nextHandle = GCHandle.Alloc(nextArray, GCHandleType.Pinned);

                void* nextPointer;

                // SAFETY: nextHandle pins nextArray while its address is captured and validated.
                unsafe
                {
                    nextPointer = newSize == 0 ? null : (void*)nextHandle.AddrOfPinnedObject();
                    ThrowHelper.ThrowIfPointerMisaligned(UnsafeAPI.IsPointerAligned<TNative>(nextPointer));

                    if (copyContent)
                    {
                        var copyLength = Math.Min(_capacity, newSize);

                        if (copyLength > 0)
                        {
                            UnsafeUtility.MemCpy(
                                  (T*)nextPointer
                                , (T*)_buffer
                                , (long)copyLength * UnsafeUtility.SizeOf<T>()
                            );
                        }
                    }
                }

                var previousHandle = _gcHandle;
                _gcHandle = nextHandle;

                // SAFETY: nextHandle remains owned by this value after the pointer is published.
                unsafe
                {
                    _buffer = nextPointer;
                }

                _capacity = newSize;
                nextHandle = default;
                previousHandle.Free();
            }
            finally
            {
                if (nextHandle.IsAllocated)
                {
                    nextHandle.Free();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void FastClear() { }

        /// <safety>The designated owner must remain alive and exclusively writable for the complete clear.</safety>
        public readonly unsafe void Clear()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(this);

            if (_capacity == 0)
            {
                return;
            }

            // SAFETY: Created owner keeps _buffer pinned for exactly _capacity elements of T.
            unsafe
            {
                UnsafeUtility.MemClear((T*)_buffer, (long)_capacity * UnsafeUtility.SizeOf<T>());
            }
        }

        /// <safety>The designated owner must remain alive and writable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyFrom(ReadOnlySpan<T> source)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy.
            unsafe
            {
                CopyFrom(0, source);
            }
        }

        /// <safety>The designated owner must remain alive and writable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyFrom(ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy.
            unsafe
            {
                CopyFrom(0, source, length);
            }
        }

        /// <safety>The designated owner must remain alive and writable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy.
            unsafe
            {
                CopyFrom(destinationStartIndex, source, source.Length);
            }
        }

        /// <safety>The designated owner must remain alive and writable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy.
            unsafe
            {
                new CopyFromSpan<T>(AsSpan()).CopyFrom(destinationStartIndex, source, length);
            }
        }

        /// <safety>The designated owner must remain alive and writable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyFrom(ReadOnlySpan<T> source)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy attempt.
            unsafe
            {
                return TryCopyFrom(0, source);
            }
        }

        /// <safety>The designated owner must remain alive and writable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyFrom(ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy attempt.
            unsafe
            {
                return TryCopyFrom(0, source, length);
            }
        }

        /// <safety>The designated owner must remain alive and writable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy attempt.
            unsafe
            {
                return TryCopyFrom(destinationStartIndex, source, source.Length);
            }
        }

        /// <safety>The designated owner must remain alive and writable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy attempt.
            unsafe
            {
                return new CopyFromSpan<T>(AsSpan()).TryCopyFrom(destinationStartIndex, source, length);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyTo(Span<T> destination)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy.
            unsafe
            {
                CopyTo(0, destination);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyTo(Span<T> destination, int length)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy.
            unsafe
            {
                CopyTo(0, destination, length);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy.
            unsafe
            {
                CopyTo(sourceStartIndex, destination, destination.Length);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy.
            unsafe
            {
                new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyTo(Span<T> destination)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(0, destination);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyTo(Span<T> destination, int length)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(0, destination, length);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy attempt.
            unsafe
            {
                return TryCopyTo(sourceStartIndex, destination, destination.Length);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy attempt.
            unsafe
            {
                return new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);
            }
        }

        internal readonly T[] AsManagedArray()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(this);
            return (T[])_gcHandle.Target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly ArraySegment<T> AsArraySegment()
            => new(AsManagedArray());

        /// <safety>
        /// The designated owner must remain alive and must not resize or dispose while the span is used.
        /// </safety>
        public readonly unsafe Span<T> AsSpan()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(this);

            // SAFETY: Created owner keeps _buffer pinned for exactly _capacity elements of T.
            unsafe
            {
                return new Span<T>((T*)_buffer, _capacity);
            }
        }

        /// <safety>
        /// The designated owner must remain alive and must not resize or dispose while the span is used.
        /// </safety>
        public readonly unsafe ReadOnlySpan<T> AsReadOnlySpan()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(this);

            // SAFETY: Created owner keeps _buffer pinned for exactly _capacity elements of T.
            unsafe
            {
                return new ReadOnlySpan<T>((T*)_buffer, _capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> AsMemory()
            => AsManagedArray().AsMemory();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<T> AsReadOnlyMemory()
            => AsManagedArray().AsMemory();

        /// <safety>The returned alias must not be disposed or used after owner resize or disposal.</safety>
        public readonly unsafe NativeArray<TNative> AsNativeArray()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(this);

            // SAFETY: Created owner keeps _buffer pinned and _capacity matches equal-size TNative elements.
            unsafe
            {
                var alias = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<TNative>(
                      (TNative*)_buffer
                    , _capacity
                    , Allocator.None
                );

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                var safety = AtomicSafetyHandle.GetTempMemoryHandle();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref alias, safety);
#endif
                return alias;
            }
        }

        /// <safety>The returned alias must not be disposed or used after owner resize or disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe NativeSlice<TNative> AsNativeSlice()
        {
            // SAFETY: The caller accepts the borrowed alias lifetime returned by AsNativeArray.
            unsafe
            {
                return new(AsNativeArray());
            }
        }

        /// <safety>The returned alias must not be disposed or used after owner resize or disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe NativeSliceReadOnly<TNative> AsNativeSliceReadOnly()
        {
            // SAFETY: The caller accepts the borrowed alias lifetime returned by AsNativeArray.
            unsafe
            {
                return new(AsNativeArray());
            }
        }

        /// <safety>The returned view must not be used after owner resize or disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe ReadOnly AsReadOnly()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(this);

            // SAFETY: Result borrows the same pin and pointer from this owner value.
            unsafe
            {
                return new ReadOnly(_gcHandle, _buffer, _capacity);
            }
        }

        /// <safety>
        /// The designated owner must remain alive. TNative and UNative must have equal size, and returned aliases
        /// must not be used after owner resize or disposal.
        /// </safety>
        public readonly unsafe BufferShared<T, UNative> Reinterpret<UNative>()
            where UNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(this);
            CollectionsThrowHelper.ThrowIfTypesNotEqualSize<TNative, UNative>(
                UnsafeUtility.SizeOf<TNative>() == UnsafeUtility.SizeOf<UNative>()
            );

            // SAFETY: Managed storage remains T[]. Equal native sizes and alignment preserve element boundaries.
            unsafe
            {
                ThrowHelper.ThrowIfPointerMisaligned(UnsafeAPI.IsPointerAligned<UNative>(_buffer));
                return new BufferShared<T, UNative>(_gcHandle, _buffer, _capacity);
            }
        }

        /// <safety>The pointer must not be freed or retained past owner resize or disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly unsafe TNative* GetUnsafeBufferPointer()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(this);

            // SAFETY: Created owner keeps returned pointer pinned until resize or disposal.
            unsafe
            {
                return (TNative*)_buffer;
            }
        }

        /// <safety>
        /// The caller must be the designated owner and must stop every borrowed alias before disposal.
        /// </safety>
        public unsafe void Dispose()
        {
            if (_gcHandle.IsAllocated == false)
            {
                return;
            }

            var handle = _gcHandle;
            _gcHandle = default;

            // SAFETY: Clearing pointer prevents this owner value from reusing released pinned storage.
            unsafe
            {
                _buffer = null;
            }

            _capacity = 0;
            handle.Free();
        }

        private void Create(T[] managed)
        {
            var handle = default(GCHandle);
            var capacity = managed.Length;

            try
            {
                handle = GCHandle.Alloc(managed, GCHandleType.Pinned);
                var pointer = capacity == 0 ? IntPtr.Zero : handle.AddrOfPinnedObject();

                // SAFETY: handle pins managed while its address is captured and validated.
                unsafe
                {
                    ThrowHelper.ThrowIfPointerMisaligned(UnsafeAPI.IsPointerAligned<TNative>((void*)pointer));
                }

                _gcHandle = handle;

                // SAFETY: Published handle continues pinning managed for this owner's lifetime.
                unsafe
                {
                    _buffer = (void*)pointer;
                }

                _capacity = capacity;
                handle = default;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        public readonly struct ReadOnly : IReadOnlyBuffer<T>, IRefReadOnlyIndexer<T>, IAsReadOnlyMemory<T>
            , IAsNativeSliceReadOnly<TNative>
        {
            private readonly GCHandle _gcHandle;

            /// <safety>
            /// The owner keeps the managed array pinned. This borrowed pointer becomes invalid after owner resize
            /// or disposal.
            /// </safety>
            [NativeDisableUnsafePtrRestriction]
            private readonly unsafe void* _buffer;
            private readonly int _capacity;

            /// <safety>The handle must pin the array addressed by buffer for the complete borrowed lifetime.</safety>
            internal unsafe ReadOnly(GCHandle gcHandle, void* buffer, int capacity)
            {
                _gcHandle = gcHandle;

                // SAFETY: The caller supplies a pointer pinned by gcHandle for this view's lifetime.
                unsafe
                {
                    _buffer = buffer;
                }

                _capacity = capacity;
            }

            /// <safety>The owner must not resize or dispose while the returned reference is used.</safety>
            public unsafe ref readonly T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    DebuggingThrowHelper.ThrowIfNotCreated(this);
                    CollectionsThrowHelper.ThrowIfIndexOutOfRangeException((uint)index < (uint)_capacity);

                    // SAFETY: Borrowed handle keeps _buffer pinned and index is within capacity.
                    unsafe
                    {
                        return ref UnsafeUtility.ArrayElementAsRef<T>(_buffer, index);
                    }
                }
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _capacity;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _gcHandle.IsAllocated;
            }

            /// <safety>The returned view must not be used after owner resize or disposal.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnly(BufferShared<T, TNative> buffer)
            {
                // SAFETY: The caller accepts the borrowed view lifetime returned by AsReadOnly.
                unsafe
                {
                    return buffer.AsReadOnly();
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void CopyTo(Span<T> destination)
            {
                // SAFETY: The caller keeps the owner alive and unchanged for the complete copy.
                unsafe
                {
                    CopyTo(0, destination);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void CopyTo(Span<T> destination, int length)
            {
                // SAFETY: The caller keeps the owner alive and unchanged for the complete copy.
                unsafe
                {
                    CopyTo(0, destination, length);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void CopyTo(int sourceStartIndex, Span<T> destination)
            {
                // SAFETY: The caller keeps the owner alive and unchanged for the complete copy.
                unsafe
                {
                    CopyTo(sourceStartIndex, destination, destination.Length);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void CopyTo(int sourceStartIndex, Span<T> destination, int length)
            {
                // SAFETY: The caller keeps the owner alive and unchanged for the complete copy.
                unsafe
                {
                    new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy attempt.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool TryCopyTo(Span<T> destination)
            {
                // SAFETY: The caller keeps the owner alive and unchanged for the complete copy attempt.
                unsafe
                {
                    return TryCopyTo(0, destination);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy attempt.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool TryCopyTo(Span<T> destination, int length)
            {
                // SAFETY: The caller keeps the owner alive and unchanged for the complete copy attempt.
                unsafe
                {
                    return TryCopyTo(0, destination, length);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy attempt.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination)
            {
                // SAFETY: The caller keeps the owner alive and unchanged for the complete copy attempt.
                unsafe
                {
                    return TryCopyTo(sourceStartIndex, destination, destination.Length);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy attempt.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
            {
                // SAFETY: The caller keeps the owner alive and unchanged for the complete copy attempt.
                unsafe
                {
                    return new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);
                }
            }

            /// <safety>The owner must not resize or dispose while the returned span is used.</safety>
            public unsafe ReadOnlySpan<T> AsReadOnlySpan()
            {
                DebuggingThrowHelper.ThrowIfNotCreated(this);

                // SAFETY: Borrowed handle keeps _buffer pinned for exactly _capacity elements.
                unsafe
                {
                    return new ReadOnlySpan<T>((T*)_buffer, _capacity);
                }
            }

            public ReadOnlyMemory<T> AsReadOnlyMemory()
            {
                DebuggingThrowHelper.ThrowIfNotCreated(this);
                return ((T[])_gcHandle.Target).AsMemory();
            }

            /// <safety>The returned alias must not be used after owner resize or disposal.</safety>
            public unsafe NativeSliceReadOnly<TNative> AsNativeSliceReadOnly()
            {
                DebuggingThrowHelper.ThrowIfNotCreated(this);

                // SAFETY: Borrowed handle keeps _buffer pinned and native element sizes match.
                unsafe
                {
                    var alias = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<TNative>(
                          (TNative*)_buffer
                        , _capacity
                        , Allocator.None
                    );

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    var safety = AtomicSafetyHandle.GetTempMemoryHandle();
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref alias, safety);
#endif
                    return new NativeSliceReadOnly<TNative>(alias);
                }
            }

            /// <safety>
            /// The owner must remain alive and unchanged. TNative and U must have equal size, and the returned
            /// alias must not be used after owner resize or disposal.
            /// </safety>
            public unsafe BufferShared<T, U>.ReadOnly Reinterpret<U>()
                where U : unmanaged
            {
                DebuggingThrowHelper.ThrowIfNotCreated(this);
                CollectionsThrowHelper.ThrowIfTypesNotEqualSize<TNative, U>(
                    UnsafeUtility.SizeOf<TNative>() == UnsafeUtility.SizeOf<U>()
                );

                // SAFETY: Equal native sizes and alignment preserve the borrowed T[] element boundaries.
                unsafe
                {
                    ThrowHelper.ThrowIfPointerMisaligned(UnsafeAPI.IsPointerAligned<U>(_buffer));
                    return new BufferShared<T, U>.ReadOnly(_gcHandle, _buffer, _capacity);
                }
            }
        }
    }
}
