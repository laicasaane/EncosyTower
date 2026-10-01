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
using EncosyTower.Collections;
using Unity.Collections;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Buffers
{
    /// <summary>
    /// Provides resizable storage backed by one pinned managed array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Struct copies borrow the current pin and do not follow later resize or disposal state.
    /// </para>
    /// <para>
    /// Only one designated owner may resize or dispose the pin. Borrowed copies must not resize or dispose.
    /// </para>
    /// <para>This type is not thread-safe.</para>
    /// </remarks>
    /// <typeparam name="T">The unmanaged element type.</typeparam>
    public struct BufferShared<T> : IBuffer<T>, IRefIndexer<T>, IAsMemory<T>, IAsReadOnlyMemory<T>
        , IAsNativeArray<T>, IAsNativeSlice<T>, IAsNativeSliceReadOnly<T>
        where T : unmanaged
    {
        private BufferShared<T, T> _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BufferShared(int capacity)
        {
            _buffer = new(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BufferShared([NotNull] T[] buffer)
        {
            _buffer = new(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BufferShared(BufferShared<T, T> buffer)
        {
            DebuggingThrowHelper.ThrowIfNotCreated(buffer);
            _buffer = buffer;
        }

        /// <safety>The designated owner must remain alive and must not resize or dispose during access.</safety>
        public readonly unsafe ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // SAFETY: The caller accepts the direct pinned-array lifetime contract above.
                unsafe
                {
                    return ref _buffer[index];
                }
            }
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Capacity;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.IsCreated;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BufferShared<T>(BufferShared<T, T> buffer)
            => new(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BufferShared<T, T>(BufferShared<T> buffer)
            => buffer._buffer;

        /// <safety>The returned view must not be used after owner resize or disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator ReadOnly(BufferShared<T> buffer)
        {
            // SAFETY: The caller accepts the borrowed view lifetime returned by AsReadOnly.
            unsafe
            {
                return buffer.AsReadOnly();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Alloc(int size)
            => _buffer.Alloc(size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Alloc(int size, AllocatorStrategy allocator, bool memClear = true)
            => _buffer.Alloc(size, allocator, memClear);

        /// <safety>The caller must be the designated owner and stop borrowers before resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Resize(int newSize)
        {
            // SAFETY: The caller is the designated owner and accepts invalidation of old aliases.
            unsafe
            {
                _buffer.Resize(newSize);
            }
        }

        /// <safety>The caller must be the designated owner and stop borrowers before resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Resize(int newSize, bool copyContent)
        {
            // SAFETY: The caller is the designated owner and accepts invalidation of old aliases.
            unsafe
            {
                _buffer.Resize(newSize, copyContent);
            }
        }

        /// <safety>The caller must be the designated owner and stop borrowers before resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Resize(int newSize, bool copyContent, bool memClear)
        {
            // SAFETY: The caller is the designated owner and accepts invalidation of old aliases.
            unsafe
            {
                _buffer.Resize(newSize, copyContent, memClear);
            }
        }

        /// <summary>
        /// Performs no operation because unmanaged elements require no reference clearing.
        /// </summary>
        /// <remarks>
        /// Existing element values and buffer capacity remain unchanged. Use <see cref="Clear"/> to zero the storage.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void FastClear() { }

        /// <safety>The designated owner must remain alive and exclusively writable by the caller.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void Clear()
        {
            // SAFETY: The caller accepts the direct pinned-array lifetime contract above.
            unsafe
            {
                _buffer.Clear();
            }
        }

        /// <safety>The designated owner must remain alive and exclusively writable by the caller.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyFrom(ReadOnlySpan<T> source)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy.
            unsafe
            {
                _buffer.CopyFrom(source);
            }
        }

        /// <safety>The designated owner must remain alive and exclusively writable by the caller.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyFrom(ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy.
            unsafe
            {
                _buffer.CopyFrom(source, length);
            }
        }

        /// <safety>The designated owner must remain alive and exclusively writable by the caller.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy.
            unsafe
            {
                _buffer.CopyFrom(destinationStartIndex, source);
            }
        }

        /// <safety>The designated owner must remain alive and exclusively writable by the caller.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy.
            unsafe
            {
                _buffer.CopyFrom(destinationStartIndex, source, length);
            }
        }

        /// <safety>The designated owner must remain alive and exclusively writable by the caller.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyFrom(ReadOnlySpan<T> source)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy attempt.
            unsafe
            {
                return _buffer.TryCopyFrom(source);
            }
        }

        /// <safety>The designated owner must remain alive and exclusively writable by the caller.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyFrom(ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy attempt.
            unsafe
            {
                return _buffer.TryCopyFrom(source, length);
            }
        }

        /// <safety>The designated owner must remain alive and exclusively writable by the caller.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy attempt.
            unsafe
            {
                return _buffer.TryCopyFrom(destinationStartIndex, source);
            }
        }

        /// <safety>The designated owner must remain alive and exclusively writable by the caller.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            // SAFETY: The caller keeps designated-owner storage live and writable for the complete copy attempt.
            unsafe
            {
                return _buffer.TryCopyFrom(destinationStartIndex, source, length);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyTo(Span<T> destination)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy.
            unsafe
            {
                _buffer.CopyTo(destination);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyTo(Span<T> destination, int length)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy.
            unsafe
            {
                _buffer.CopyTo(destination, length);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy.
            unsafe
            {
                _buffer.CopyTo(sourceStartIndex, destination);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void CopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy.
            unsafe
            {
                _buffer.CopyTo(sourceStartIndex, destination, length);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyTo(Span<T> destination)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy attempt.
            unsafe
            {
                return _buffer.TryCopyTo(destination);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyTo(Span<T> destination, int length)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy attempt.
            unsafe
            {
                return _buffer.TryCopyTo(destination, length);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy attempt.
            unsafe
            {
                return _buffer.TryCopyTo(sourceStartIndex, destination);
            }
        }

        /// <safety>The owner must remain alive and readable for the complete copy attempt.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            // SAFETY: The caller keeps owner storage live and readable for the complete copy attempt.
            unsafe
            {
                return _buffer.TryCopyTo(sourceStartIndex, destination, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly T[] AsManagedArray()
            => _buffer.AsManagedArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly ArraySegment<T> AsArraySegment()
            => _buffer.AsArraySegment();

        /// <safety>The owner must remain alive and must not resize or dispose while the span is used.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe Span<T> AsSpan()
        {
            // SAFETY: The caller accepts the direct pinned-array lifetime contract above.
            unsafe
            {
                return _buffer.AsSpan();
            }
        }

        /// <safety>The owner must remain alive and must not resize or dispose while the span is used.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe ReadOnlySpan<T> AsReadOnlySpan()
        {
            // SAFETY: The caller accepts the direct pinned-array lifetime contract above.
            unsafe
            {
                return _buffer.AsReadOnlySpan();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> AsMemory()
            => _buffer.AsMemory();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<T> AsReadOnlyMemory()
            => _buffer.AsReadOnlyMemory();

        /// <safety>The alias must not be disposed or used after resize or owner disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe NativeArray<T> AsNativeArray()
        {
            // SAFETY: The caller accepts the borrowed alias lifetime returned by the inner buffer.
            unsafe
            {
                return _buffer.AsNativeArray();
            }
        }

        /// <safety>The alias must not be disposed or used after resize or owner disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe NativeSlice<T> AsNativeSlice()
        {
            // SAFETY: The caller accepts the borrowed alias lifetime returned by the inner buffer.
            unsafe
            {
                return _buffer.AsNativeSlice();
            }
        }

        /// <safety>The alias must not be disposed or used after resize or owner disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe NativeSliceReadOnly<T> AsNativeSliceReadOnly()
        {
            // SAFETY: The caller accepts the borrowed alias lifetime returned by the inner buffer.
            unsafe
            {
                return _buffer.AsNativeSliceReadOnly();
            }
        }

        /// <safety>The returned view must not be used after owner resize or disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe ReadOnly AsReadOnly()
        {
            // SAFETY: The caller accepts the borrowed view lifetime returned by the constructor.
            unsafe
            {
                return new(this);
            }
        }

        /// <safety>
        /// The owner must remain alive. T and U must have equal size, and returned aliases must not be used after
        /// owner resize or disposal.
        /// </safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe BufferShared<T, U> Reinterpret<U>()
            where U : unmanaged
        {
            // SAFETY: Managed storage remains T[] and the inner buffer validates U size and alignment.
            unsafe
            {
                return _buffer.Reinterpret<U>();
            }
        }

        /// <safety>The pointer must not be freed or retained past resize or owner disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly unsafe T* GetUnsafeBufferPointer()
        {
            // SAFETY: The caller accepts the direct pinned-array lifetime contract above.
            unsafe
            {
                return _buffer.GetUnsafeBufferPointer();
            }
        }

        /// <safety>The caller must be the designated owner and stop every borrowed alias before disposal.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Dispose()
        {
            // SAFETY: The caller accepts the designated-owner disposal contract above.
            unsafe
            {
                _buffer.Dispose();
            }
        }

        /// <summary>
        /// Provides a non-owning, read-only view over pinned <typeparamref name="T"/> storage.
        /// </summary>
        /// <remarks>
        /// The view borrows one pinned array. It does not follow owner resize and must not be used
        /// after resize or disposal.
        /// </remarks>
        public readonly struct ReadOnly : IReadOnlyBuffer<T>, IRefReadOnlyIndexer<T>, IAsReadOnlyMemory<T>
            , IAsNativeSliceReadOnly<T>
        {
            private readonly BufferShared<T, T>.ReadOnly _buffer;

            /// <safety>The owner must remain alive and unchanged while this view is used.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal unsafe ReadOnly(BufferShared<T> buffer)
            {
                // SAFETY: The caller accepts this view's borrowed lifetime.
                unsafe
                {
                    _buffer = buffer._buffer.AsReadOnly();
                }
            }

            /// <safety>The owner must not resize or dispose while the referenced element is borrowed.</safety>
            public unsafe ref readonly T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    // SAFETY: The caller accepts this view's borrowed lifetime.
                    unsafe
                    {
                        return ref _buffer[index];
                    }
                }
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.Capacity;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.IsCreated;
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void CopyTo(Span<T> destination)
            {
                // SAFETY: The caller accepts this view's borrowed lifetime for the complete copy.
                unsafe
                {
                    _buffer.CopyTo(destination);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void CopyTo(Span<T> destination, int length)
            {
                // SAFETY: The caller accepts this view's borrowed lifetime for the complete copy.
                unsafe
                {
                    _buffer.CopyTo(destination, length);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void CopyTo(int sourceStartIndex, Span<T> destination)
            {
                // SAFETY: The caller accepts this view's borrowed lifetime for the complete copy.
                unsafe
                {
                    _buffer.CopyTo(sourceStartIndex, destination);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void CopyTo(int sourceStartIndex, Span<T> destination, int length)
            {
                // SAFETY: The caller accepts this view's borrowed lifetime for the complete copy.
                unsafe
                {
                    _buffer.CopyTo(sourceStartIndex, destination, length);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy attempt.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool TryCopyTo(Span<T> destination)
            {
                // SAFETY: The caller accepts this view's borrowed lifetime for the complete copy attempt.
                unsafe
                {
                    return _buffer.TryCopyTo(destination);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy attempt.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool TryCopyTo(Span<T> destination, int length)
            {
                // SAFETY: The caller accepts this view's borrowed lifetime for the complete copy attempt.
                unsafe
                {
                    return _buffer.TryCopyTo(destination, length);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy attempt.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination)
            {
                // SAFETY: The caller accepts this view's borrowed lifetime for the complete copy attempt.
                unsafe
                {
                    return _buffer.TryCopyTo(sourceStartIndex, destination);
                }
            }

            /// <safety>The owner must remain alive and unchanged for the complete copy attempt.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
            {
                // SAFETY: The caller accepts this view's borrowed lifetime for the complete copy attempt.
                unsafe
                {
                    return _buffer.TryCopyTo(sourceStartIndex, destination, length);
                }
            }

            /// <safety>The owner must not resize or dispose while the returned span is used.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe ReadOnlySpan<T> AsReadOnlySpan()
            {
                // SAFETY: The caller accepts this view's borrowed lifetime.
                unsafe
                {
                    return _buffer.AsReadOnlySpan();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlyMemory<T> AsReadOnlyMemory()
                => _buffer.AsReadOnlyMemory();

            /// <safety>The alias must not be disposed or used after resize or owner disposal.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe NativeSliceReadOnly<T> AsNativeSliceReadOnly()
            {
                // SAFETY: The caller accepts this view's borrowed lifetime.
                unsafe
                {
                    return _buffer.AsNativeSliceReadOnly();
                }
            }

            /// <safety>
            /// The owner must remain alive and unchanged. T and U must have equal size, and the returned alias must
            /// not be used after owner resize or disposal.
            /// </safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe BufferShared<T, U>.ReadOnly Reinterpret<U>()
                where U : unmanaged
            {
                // SAFETY: The inner view validates native size and alignment while preserving managed T.
                unsafe
                {
                    return _buffer.Reinterpret<U>();
                }
            }
        }
    }
}
