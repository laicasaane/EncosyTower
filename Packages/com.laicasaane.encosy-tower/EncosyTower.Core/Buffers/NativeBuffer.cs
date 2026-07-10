// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/DualMemorySupport/NativeStrategy.cs

// MIT License
//
// Copyright (c) 2015-2020 Sebastiano Mandalà
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#if UNITY_COLLECTIONS
#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Debugging;
using EncosyTower.Types;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace EncosyTower.Buffers
{
    /// <summary>
    /// A buffer backed by a <see cref="NativeArray{T}"/>.
    /// <br/>
    /// <see cref="NativeBuffer{T}"/> abstracts the handling of native memory so that
    /// data structures can use it interchangeably with <see cref="ManagedBuffer{T}"/>
    /// and <see cref="UnsafeBuffer{T}"/> through the <see cref="IBuffer{T}"/> contract.
    /// </summary>
    public struct NativeBuffer<T> : IBuffer<T>, IRefIndexer<T>
        , IAsNativeSlice<T>, IAsNativeSliceReadOnly<T>
        , INativeDisposable
        where T : unmanaged
    {
#if __ENCOSY_VALIDATION__
        static NativeBuffer()
        {
            ThrowHelper.ThrowIfNotUnmanagedType<T>(EncosyTypeExtensions.IsUnmanaged<T>());
        }
#endif

        internal NativeReference<AllocatorStrategy> _nativeAllocator;

#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        internal NativeArray<T> _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBuffer(int size, AllocatorStrategy allocatorStrategy, bool clear = true) : this()
        {
            ThrowIfInvalidAllocatorStrategy(allocatorStrategy.IsValid);

            Alloc(size, allocatorStrategy, clear);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBuffer(NativeReference<AllocatorStrategy> nativeAllocator, NativeArray<T> buffer)
        {
            _nativeAllocator = nativeAllocator;
            _buffer = buffer;
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.IsCreated;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if __ENCOSY_VALIDATION__
                ThrowHelper.ThrowIfIndexOutOfRangeException((uint)index < (uint)_buffer.Length);
#endif

                unsafe
                {
                    return ref UnsafeUtility.ArrayElementAsRef<T>(_buffer.GetUnsafePtr(), index);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Alloc(int newCapacity, AllocatorStrategy allocatorStrategy, bool memClear = true)
        {
            ThrowIfBufferAlreadyAllocated(_buffer.IsCreated);

            if (allocatorStrategy.TryGetAllocatorHandle(out var handle))
            {
                Alloc(newCapacity, handle, memClear);
                return;
            }

            if (allocatorStrategy.TryGetAllocator(out var allocator))
            {
                Alloc(newCapacity, allocator, memClear);
                return;
            }

            ThrowIfInvalidAllocatorStrategy(allocatorStrategy.IsValid);
        }

        private void Alloc(int newCapacity, AllocatorManager.AllocatorHandle allocator, bool memClear = true)
        {
            _nativeAllocator = new(allocator, NativeArrayOptions.UninitializedMemory) {
                Value = allocator
            };

            _buffer = memClear
                ? NativeArray.Create<T>(newCapacity, allocator)
                : NativeArray.CreateFast<T>(newCapacity, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize)
            => Resize(newSize, true, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize, bool copyContent)
            => Resize(newSize, copyContent, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize, bool copyContent, bool memClear)
        {
            ThrowIfResizeUninitializedBuffer(_nativeAllocator.IsCreated && _buffer.IsCreated);

            var capacity = Capacity;

            if (newSize == capacity)
            {
                return;
            }

            var allocatorStrategy = _nativeAllocator.Value;

            if (allocatorStrategy.TryGetAllocatorHandle(out var handle))
            {
                Resize(newSize, copyContent, memClear, handle);
                return;
            }

            if (allocatorStrategy.TryGetAllocator(out var allocator))
            {
                Resize(newSize, copyContent, memClear, allocator);
                return;
            }

            ThrowIfInvalidAllocatorStrategy(allocatorStrategy.IsValid);
        }

        private void Resize(int newSize, bool copyContent, bool memClear, AllocatorManager.AllocatorHandle allocator)
        {
            var oldBuffer = _buffer;
            var oldLength = oldBuffer.Length;
            var newBuffer = memClear
                ? NativeArray.Create<T>(newSize, allocator)
                : NativeArray.CreateFast<T>(newSize, allocator);

            if (copyContent)
            {
                var copyLength = math.min(oldLength, newSize);
                oldBuffer.AsReadOnlySpan()[..copyLength].CopyTo(newBuffer.AsSpan()[..copyLength]);
            }

            oldBuffer.Dispose();
            _buffer = newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void FastClear() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
        {
            unsafe
            {
                UnsafeUtility.MemClear(_buffer.GetUnsafePtr(), (long)_buffer.Length * UnsafeUtility.SizeOf<T>());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source)
            => CopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source, int length)
            => CopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => CopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => new CopyFromSpan<T>(AsSpan()).CopyFrom(destinationStartIndex, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source)
            => TryCopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source, int length)
            => TryCopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => TryCopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => new CopyFromSpan<T>(AsSpan()).TryCopyFrom(destinationStartIndex, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> destination)
            => CopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> destination, int length)
            => CopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(int sourceStartIndex, Span<T> destination)
            => CopyTo(sourceStartIndex, destination, destination.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(int sourceStartIndex, Span<T> destination, int length)
            => new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination)
            => TryCopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination, int length)
            => TryCopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination)
            => TryCopyTo(sourceStartIndex, destination, destination.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
            => new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly NativeArray<T> AsNativeArray()
            => _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSlice<T> AsNativeSlice()
            => new(_buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSliceReadOnly<T> AsNativeSliceReadOnly()
            => new(_buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan()
            => _buffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
            => _buffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnly AsReadOnly()
            => this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeBuffer<U> Reinterpret<U>()
            where U : unmanaged
            => new(_nativeAllocator, _buffer.Reinterpret<U>());

        public void Dispose()
        {
            ThrowIfAlreadyDisposed(_buffer.IsCreated);

            if (_buffer.IsCreated)
            {
                _buffer.Dispose();
            }

            if (_nativeAllocator.IsCreated)
            {
                _nativeAllocator.Dispose();
            }

            _buffer = default;
            _nativeAllocator = default;
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            ThrowIfAlreadyDisposed(_buffer.IsCreated);

            if (_buffer.IsCreated && _nativeAllocator.IsCreated)
            {
                inputDeps = JobHandle.CombineDependencies(
                      _buffer.Dispose(inputDeps)
                    , _nativeAllocator.Dispose(inputDeps)
                );
            }
            else
            {
                if (_buffer.IsCreated)
                {
                    inputDeps = _buffer.Dispose(inputDeps);
                }

                if (_nativeAllocator.IsCreated)
                {
                    inputDeps = _nativeAllocator.Dispose(inputDeps);
                }
            }

            _buffer = default;
            _nativeAllocator = default;

            return inputDeps;
        }

        public readonly struct ReadOnly : IReadOnlyBuffer<T>, IRefReadOnlyIndexer<T>
            , IAsNativeSliceReadOnly<T>
        {
            internal readonly NativeReference<AllocatorStrategy>.ReadOnly _nativeAllocator;

#if UNITY_BURST
            [Unity.Burst.NoAlias]
#endif
            internal readonly NativeArray<T>.ReadOnly _buffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ReadOnly(NativeBuffer<T> buffer)
            {
                _nativeAllocator = buffer._nativeAllocator;
                _buffer = buffer._buffer.AsReadOnly();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ReadOnly(
                  NativeReference<AllocatorStrategy>.ReadOnly nativeAllocator
                , NativeArray<T>.ReadOnly buffer
            )
            {
                _nativeAllocator = nativeAllocator;
                _buffer = buffer;
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.Length;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.IsCreated;
            }

            public ref readonly T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
#if __ENCOSY_VALIDATION__
                    ThrowHelper.ThrowIfIndexOutOfRangeException((uint)index < (uint)_buffer.Length);
#endif

                    unsafe
                    {
                        return ref UnsafeUtility.ArrayElementAsRef<T>(_buffer.GetUnsafeReadOnlyPtr(), index);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(Span<T> destination)
                => CopyTo(0, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(Span<T> destination, int length)
                => CopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(int sourceStartIndex, Span<T> destination)
                => CopyTo(sourceStartIndex, destination, destination.Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(int sourceStartIndex, Span<T> destination, int length)
                => new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(Span<T> destination)
                => TryCopyTo(0, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(Span<T> destination, int length)
                => TryCopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination)
                => TryCopyTo(sourceStartIndex, destination, destination.Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
                => new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal NativeArray<T>.ReadOnly AsNativeArray()
                => _buffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeSliceReadOnly<T> AsNativeSliceReadOnly()
                => new(_buffer);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<T> AsReadOnlySpan()
                => _buffer.AsReadOnlySpan();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeBuffer<U>.ReadOnly Reinterpret<U>()
                where U : unmanaged
                => new(_nativeAllocator, _buffer.Reinterpret<U>());

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(NativeBuffer<T> buffer)
                => new(buffer);
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfInvalidAllocatorStrategy([DoesNotReturnIf(false)] bool validStrategy)
        {
            if (validStrategy == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new(
                    "Allocator strategy must be either Unity.Collections.Allocator " +
                    "or Unity.Collections.AllocatorManager.AllocatorHandle"
                );
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfBufferAlreadyAllocated([DoesNotReturnIf(true)] bool isCreated)
        {
            if (isCreated)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot allocate an already allocated buffer.");
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfResizeUninitializedBuffer([DoesNotReturnIf(false)] bool isCreated)
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot resize an uninitialized buffer.");
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfAlreadyDisposed([DoesNotReturnIf(false)] bool isCreated)
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot dispose an already disposed buffer.");
        }
    }
}

#endif
