// https://gitlab.com/tertle/com.bovinelabs.core/-/blob/100ed3191ffa18e79e508fcc80f75f562507803e/BovineLabs.Core/Collections/UnsafeArray.cs

// MIT License
//
// Copyright (c) 2025 Timothy Raines
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Internal;

namespace EncosyTower.Collections.Unsafe
{
    [DebuggerTypeProxy(typeof(ArrayUnsafe<>.UnsafeArrayDebugView))]
    [DebuggerDisplay("Length = {Length}")]
    public struct ArrayUnsafe<T> : IDisposable, IEnumerable<T>, IEquatable<ArrayUnsafe<T>>
        , ICopyToSpan<T>, ITryCopyToSpan<T>
        , ICopyFromSpan<T>, ITryCopyFromSpan<T>
        , IIsCreated, IHasLength, IIndexer<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>
#if UNITY_COLLECTIONS
        , INativeDisposable
#endif
        where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        private unsafe void* _buffer;

        private Allocator _allocatorLabel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ArrayUnsafe(
              int length
            , Allocator allocator
            , NativeArrayOptions options = NativeArrayOptions.ClearMemory
        )
        {
            Allocate(length, allocator, out this);

            if (options != NativeArrayOptions.ClearMemory)
            {
                return;
            }

            UnsafeUtility.MemClear(_buffer, Length * (long)UnsafeUtility.SizeOf<T>());
        }

        public int Length { get; private set; }

        public readonly unsafe bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (IntPtr)_buffer != IntPtr.Zero;
        }

        public readonly unsafe T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeUtility.ReadArrayElement<T>(_buffer, index);

            [WriteAccessRequired]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => UnsafeUtility.WriteArrayElement(_buffer, index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ArrayUnsafe<T> left, ArrayUnsafe<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ArrayUnsafe<T> left, ArrayUnsafe<T> right)
            => !left.Equals(right);

        [WriteAccessRequired]
        public unsafe void Dispose()
        {
            ThrowIfAlreadyDisposed(IsCreated);
            ThrowIfInvalidAllocator(_allocatorLabel != Allocator.Invalid);

            if (_allocatorLabel > Allocator.None)
            {
                UnsafeUtility.FreeTracked(_buffer, _allocatorLabel);
                _allocatorLabel = Allocator.Invalid;
            }

            _buffer = null;
        }

        public unsafe JobHandle Dispose(JobHandle inputDeps)
        {
            ThrowIfInvalidAllocator(_allocatorLabel != Allocator.Invalid);
            ThrowIfAlreadyDisposed(IsCreated);

            if (_allocatorLabel > Allocator.None)
            {
                var jobHandle = new UnsafeArrayDisposeJob
                {
                    Data = new UnsafeArrayDispose
                    {
                        Buffer = _buffer,
                        AllocatorLabel = _allocatorLabel,
                    },
                }.Schedule(inputDeps);

                _buffer = null;
                _allocatorLabel = Allocator.Invalid;
                return jobHandle;
            }

            _buffer = null;
            return inputDeps;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void* GetUnsafePtr()
            => _buffer;

        /// <summary>
        /// Creates a non-owning view over existing memory. <see cref="Dispose()"/> on a view
        /// frees nothing (the <see cref="Allocator.None"/> path already handles that).
        /// </summary>
        internal static unsafe ArrayUnsafe<T> ConvertExistingData(void* buffer, int length)
        {
            var array = default(ArrayUnsafe<T>);
            array._buffer = buffer;
            array.Length = length;
            array._allocatorLabel = Allocator.None;
            return array;
        }

        /// <summary>
        /// Reinterprets the element type. <typeparamref name="T"/> and <typeparamref name="U"/>
        /// must have equal size. The result is a non-owning VIEW (<see cref="Allocator.None"/>),
        /// so the reinterpreted copy can never double-free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe ArrayUnsafe<U> Reinterpret<U>()
            where U : unmanaged
        {
            ThrowIfTypesNotEqualSize<U>(UnsafeUtility.SizeOf<T>() == UnsafeUtility.SizeOf<U>());

            return ArrayUnsafe<U>.ConvertExistingData(_buffer, Length);
        }

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source)
            => CopyFrom(0, source);

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source, int length)
            => CopyFrom(0, source, length);

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => CopyFrom(destinationStartIndex, source, source.Length);

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => new CopyFromSpan<T>(AsSpan()).CopyFrom(destinationStartIndex, source, length);

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source)
            => TryCopyFrom(0, source);

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source, int length)
            => TryCopyFrom(0, source, length);

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => TryCopyFrom(destinationStartIndex, source, source.Length);

        [WriteAccessRequired]
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
        public readonly T[] ToArray()
        {
            var dst = new T[Length];
            CopyTo(dst);
            return dst;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan()
        {
            unsafe
            {
                return new Span<T>(_buffer, Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
        {
            unsafe
            {
                return new ReadOnlySpan<T>(_buffer, Length);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool Equals(ArrayUnsafe<T> other)
            => _buffer == other._buffer && Length == other.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj != null && obj is ArrayUnsafe<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override unsafe int GetHashCode()
            => ((int)_buffer * 397) ^ Length;

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckAllocateArguments(int length, Allocator allocator)
        {
            ThrowIfAllocatorNotSupported(allocator > Allocator.None);
            ThrowIfCustomAllocator(allocator < Allocator.FirstUserIndex);
            ThrowIfAllocateLengthNegative(length >= 0);
        }

        private static unsafe void Allocate(int length, Allocator allocator, out ArrayUnsafe<T> array)
        {
            var size = UnsafeUtility.SizeOf<T>() * (long)length;
            CheckAllocateArguments(length, allocator);
            array = default;
            IsUnmanagedAndThrow();
            array._buffer = UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<T>(), allocator, 0);
            array.Length = length;
            array._allocatorLabel = allocator;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [BurstDiscard]
        private static void IsUnmanagedAndThrow()
            => ThrowIfNotUnmanaged(UnsafeUtility.IsUnmanaged<T>());

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfAlreadyDisposed([DoesNotReturnIf(false)] bool isCreated)
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ObjectDisposedException CreateException()
                => new("The UnsafeArray is already disposed.");
        }

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfInvalidAllocator([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("The UnsafeArray can not be Disposed because it was not allocated with a valid allocator.");
        }

        [HideInCallstack, StackTraceHidden, Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowIfAllocatorNotSupported([DoesNotReturnIf(false)] bool isSupported)
        {
            if (isSupported == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new("Allocator must be Temp, TempJob or Persistent", "allocator");
        }

        [HideInCallstack, StackTraceHidden, Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowIfCustomAllocator([DoesNotReturnIf(false)] bool isBuiltIn)
        {
            if (isBuiltIn == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new("Use CollectionHelper.CreateUnsafeArray for custom allocator", "allocator");
        }

        [HideInCallstack, StackTraceHidden, Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowIfAllocateLengthNegative([DoesNotReturnIf(false)] bool isZeroOrPositive)
        {
            if (isZeroOrPositive == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException()
                => new("length", "Length must be >= 0");
        }

        [HideInCallstack, StackTraceHidden, Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowIfTypesNotEqualSize<U>([DoesNotReturnIf(false)] bool areEqual)
            where U : unmanaged
        {
            if (areEqual == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new(
                    $"size of type '{typeof(U).FullName}' must be equal to " +
                    $"size of type '{typeof(T).FullName}'"
                );
        }

        [HideInCallstack, StackTraceHidden, Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowIfNotUnmanaged([DoesNotReturnIf(false)] bool isUnmanaged)
        {
            if (isUnmanaged == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new($"{typeof(T)} used in UnsafeArray<{typeof(T)}> must be unmanaged (contain no managed types).");
        }

        [ExcludeFromDocs]
        public struct Enumerator : IEnumerator<T>
        {
            private ArrayUnsafe<T> _array;
            private int _index;

            public Enumerator(ref ArrayUnsafe<T> array)
            {
                _array = array;
                _index = -1;
            }

            public readonly T Current => _array[_index];

            readonly object IEnumerator.Current => Current;

            public readonly void Dispose() { }

            public bool MoveNext()
            {
                ++_index;
                return _index < _array.Length;
            }

            public void Reset() => _index = -1;
        }

#pragma warning disable IDE1006 // Naming Styles
        private struct UnsafeArrayDisposeJob : IJob
        {
            internal UnsafeArrayDispose Data;

            public void Execute() => Data.Dispose();
        }

        private struct UnsafeArrayDispose
        {
            [NativeDisableUnsafePtrRestriction]
            internal unsafe void* Buffer;

            internal Allocator AllocatorLabel;

            public readonly unsafe void Dispose() => UnsafeUtility.FreeTracked(Buffer, AllocatorLabel);
        }

        private sealed class UnsafeArrayDebugView
        {
            private ArrayUnsafe<T> array;

            public UnsafeArrayDebugView(ArrayUnsafe<T> array)
            {
                this.array = array;
            }

            public T[] Items => array.ToArray();
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
