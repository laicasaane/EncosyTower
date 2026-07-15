// https://gitlab.com/tertle/com.bovinelabs.core/-/blob/
// 100ed3191ffa18e79e508fcc80f75f562507803e/BovineLabs.Core/Collections/UnsafeArray.cs

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

#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Debugging;
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
        public ArrayUnsafe(
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

            // SAFETY: Allocate produced a writable buffer of exactly Length unmanaged elements.
            unsafe
            {
                UnsafeUtility.MemClear(_buffer, Length * (long)UnsafeUtility.SizeOf<T>());
            }
        }

        public int Length { get; private set; }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // SAFETY: Reading the pointer field only observes the allocation state.
                unsafe
                {
                    return (IntPtr)_buffer != IntPtr.Zero;
                }
            }
        }

        /// <safety>
        /// The index must be within the allocated length and the array must remain created for the access.
        /// </safety>
        public readonly unsafe T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelper.ThrowIfIndexIsOutOfRange(
                      (uint)index < (uint)Length
                    , ThrowHelper.CollectionType.ArrayUnsafe
                );
                // SAFETY: The caller must uphold the manually allocated buffer's bounds and lifetime.
                unsafe
                {
                    return UnsafeUtility.ReadArrayElement<T>(_buffer, index);
                }
            }

            [WriteAccessRequired]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ThrowHelper.ThrowIfIndexIsOutOfRange(
                      (uint)index < (uint)Length
                    , ThrowHelper.CollectionType.ArrayUnsafe
                );
                // SAFETY: The caller must uphold the manually allocated buffer's bounds and lifetime.
                unsafe
                {
                    UnsafeUtility.WriteArrayElement(_buffer, index, value);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ArrayUnsafe<T> left, ArrayUnsafe<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ArrayUnsafe<T> left, ArrayUnsafe<T> right)
            => !left.Equals(right);

        [WriteAccessRequired]
        public void Dispose()
        {
            ThrowHelper.ThrowIfUnsafeCollectionIsDisposed(
                IsCreated,
                ThrowHelper.CollectionType.ArrayUnsafe
            );
            ThrowHelper.ThrowIfUnsafeCollectionAllocatorIsInvalid(
                _allocatorLabel != Allocator.Invalid,
                ThrowHelper.CollectionType.ArrayUnsafe
            );

            if (_allocatorLabel > Allocator.None)
            {
                // SAFETY: The allocator and creation checks establish ownership of this buffer.
                unsafe
                {
                    UnsafeUtility.FreeTracked(_buffer, _allocatorLabel);
                }
                _allocatorLabel = Allocator.Invalid;
            }

            // SAFETY: The owned allocation has been released, so clearing the pointer is a local state update.
            unsafe
            {
                _buffer = null;
            }
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            ThrowHelper.ThrowIfUnsafeCollectionAllocatorIsInvalid(
                _allocatorLabel != Allocator.Invalid,
                ThrowHelper.CollectionType.ArrayUnsafe
            );
            ThrowHelper.ThrowIfUnsafeCollectionIsDisposed(
                IsCreated,
                ThrowHelper.CollectionType.ArrayUnsafe
            );

            if (_allocatorLabel > Allocator.None)
            {
                // SAFETY: The scheduled job receives the live buffer and takes ownership of its release.
                unsafe
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
            }

            // SAFETY: No allocation remains to release, so clearing the pointer is a local state update.
            unsafe
            {
                _buffer = null;
            }
            return inputDeps;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <safety>
        /// The returned pointer is valid only while this array remains created; callers must
        /// uphold its lifetime and bounds.
        /// </safety>
        public readonly unsafe void* GetUnsafePtr()
        {
            // SAFETY: This method intentionally exposes the owned allocation without extending its lifetime.
            unsafe
            {
                return _buffer;
            }
        }

        /// <summary>
        /// Creates a non-owning view over existing memory. <see cref="Dispose()"/> on a view
        /// frees nothing (the <see cref="Allocator.None"/> path already handles that).
        /// </summary>
        internal static unsafe ArrayUnsafe<T> ConvertExistingData(void* buffer, int length)
        {
            var array = default(ArrayUnsafe<T>);
            // SAFETY: The caller supplies a valid non-owning buffer and length for the view.
            unsafe
            {
                array._buffer = buffer;
            }
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
        /// <safety>
        /// T and U must have equal size, and the returned view must not outlive the original allocation.
        /// </safety>
        public readonly unsafe ArrayUnsafe<U> Reinterpret<U>()
            where U : unmanaged
        {
            ThrowIfTypesNotEqualSize<U>(UnsafeUtility.SizeOf<T>() == UnsafeUtility.SizeOf<U>());

            // SAFETY: Equal-size validation preserves element boundaries and the result borrows this buffer.
            unsafe
            {
                return ArrayUnsafe<U>.ConvertExistingData(_buffer, Length);
            }
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
            // SAFETY: The established ownership and safety checks keep the native storage live
            // for this pointer dereference.
            unsafe
            {
                return new Span<T>(_buffer, Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
        {
            // SAFETY: The established ownership and safety checks keep the native storage live
            // for this pointer dereference.
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
        public readonly bool Equals(ArrayUnsafe<T> other)
        {
            // SAFETY: Pointer comparison does not dereference memory.
            unsafe
            {
                return _buffer == other._buffer && Length == other.Length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj != null && obj is ArrayUnsafe<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
            // SAFETY: Pointer conversion is used only to compute an identity hash; no memory is dereferenced.
            unsafe
            {
                return ((int)_buffer * 397) ^ Length;
            }
        }

        [Conditional("__ENCOSY_VALIDATION__")]
        private static void CheckAllocateArguments(int length, Allocator allocator)
        {
            ThrowIfAllocatorNotSupported(allocator > Allocator.None);
            ThrowIfCustomAllocator(allocator < Allocator.FirstUserIndex);
            ThrowIfAllocateLengthNegative(length >= 0);
        }

        private static void Allocate(int length, Allocator allocator, out ArrayUnsafe<T> array)
        {
            // SAFETY: The validated allocator and unmanaged constraint establish the allocation size and alignment.
            unsafe
            {
                var size = UnsafeUtility.SizeOf<T>() * (long)length;
                CheckAllocateArguments(length, allocator);
                array = default;
                IsUnmanagedAndThrow();
                array._buffer = UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<T>(), allocator, 0);
                array.Length = length;
                array._allocatorLabel = allocator;
            }
        }

        [Conditional("__ENCOSY_VALIDATION__")]
#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        private static void IsUnmanagedAndThrow()
            => ThrowHelper.ThrowIfUnsafeCollectionTypeIsManaged<T>(
                UnsafeUtility.IsUnmanaged<T>(),
                ThrowHelper.CollectionType.ArrayUnsafe
            );

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
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

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
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

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
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

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
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

            public readonly T Current
                => _array[_index];

            readonly object IEnumerator.Current
                => Current;

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                ++_index;
                return _index < _array.Length;
            }

            public void Reset()
                => _index = -1;
        }

#pragma warning disable IDE1006 // Naming Styles
        private struct UnsafeArrayDisposeJob : IJob
        {
            internal UnsafeArrayDispose Data;

            public void Execute()
                => Data.Dispose();
        }

        private struct UnsafeArrayDispose
        {
            [NativeDisableUnsafePtrRestriction]
            internal unsafe void* Buffer;

            internal Allocator AllocatorLabel;

            public readonly void Dispose()
            {
                // SAFETY: The job owns Buffer and receives the matching allocator label from Dispose(JobHandle).
                unsafe
                {
                    UnsafeUtility.FreeTracked(Buffer, AllocatorLabel);
                }
            }
        }

        private sealed class UnsafeArrayDebugView
        {
            private ArrayUnsafe<T> array;

            public UnsafeArrayDebugView(ArrayUnsafe<T> array)
            {
                this.array = array;
            }

            public T[] Items
                => array.ToArray();
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
