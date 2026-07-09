using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace EncosyTower.Collections.Unsafe
{
    [DebuggerDisplay("Value = {Value}")]
    public struct ReferenceUnsafe<T> : IDisposable, IEquatable<ReferenceUnsafe<T>>
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
        private unsafe void* _data;

        private Allocator _allocatorLabel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReferenceUnsafe(
              Allocator allocator
            , NativeArrayOptions options = NativeArrayOptions.ClearMemory
        )
        {
            Allocate(allocator, out this);

            if (options != NativeArrayOptions.ClearMemory)
            {
                return;
            }

            UnsafeUtility.MemClear(_data, UnsafeUtility.SizeOf<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReferenceUnsafe(T value, Allocator allocator)
        {
            Allocate(allocator, out this);
            *(T*)_data = value;
        }

        public readonly unsafe bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (IntPtr)_data != IntPtr.Zero;
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsCreated ? 1 : 0;
        }

        public readonly unsafe T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => *(T*)_data;

            [WriteAccessRequired]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => *(T*)_data = value;
        }

        public readonly unsafe ref T ValueAsRef
        {
            [WriteAccessRequired]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref UnsafeUtility.AsRef<T>(_data);
        }

        public readonly unsafe T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfIndexOutOfRange(index);
                return *(T*)_data;
            }

            [WriteAccessRequired]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ThrowIfIndexOutOfRange(index);
                *(T*)_data = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ReferenceUnsafe<T> left, ReferenceUnsafe<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ReferenceUnsafe<T> left, ReferenceUnsafe<T> right)
            => !left.Equals(right);

        [WriteAccessRequired]
        public unsafe void Dispose()
        {
            ThrowIfAlreadyDisposed(IsCreated);
            ThrowIfInvalidAllocator(_allocatorLabel != Allocator.Invalid);

            if (_allocatorLabel > Allocator.None)
            {
                UnsafeUtility.FreeTracked(_data, _allocatorLabel);
                _allocatorLabel = Allocator.Invalid;
            }

            _data = null;
        }

        public unsafe JobHandle Dispose(JobHandle inputDeps)
        {
            ThrowIfInvalidAllocator(_allocatorLabel != Allocator.Invalid);
            ThrowIfAlreadyDisposed(IsCreated);

            if (_allocatorLabel > Allocator.None)
            {
                var jobHandle = new UnsafeReferenceDisposeJob
                {
                    Data = new UnsafeReferenceDispose
                    {
                        Data = _data,
                        AllocatorLabel = _allocatorLabel,
                    },
                }.Schedule(inputDeps);

                _data = null;
                _allocatorLabel = Allocator.Invalid;
                return jobHandle;
            }

            _data = null;
            return inputDeps;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void* GetUnsafePtr()
            => _data;

        /// <summary>
        /// Creates a non-owning view over existing memory (e.g. an element of a pinned
        /// managed array). <see cref="Dispose()"/> on a view frees nothing.
        /// </summary>
        internal static unsafe ReferenceUnsafe<T> ConvertExistingData(void* data)
        {
            var reference = default(ReferenceUnsafe<T>);
            reference._data = data;
            reference._allocatorLabel = Allocator.None;
            return reference;
        }

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReferenceUnsafe<T> reference)
            => Copy(this, reference);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(ReferenceUnsafe<T> reference)
            => Copy(reference, this);

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
        public readonly bool Equals(ReferenceUnsafe<T> other)
            => Value.Equals(other.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj != null && obj is ReferenceUnsafe<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(ReferenceUnsafe<T> dst, ReferenceUnsafe<T> src)
        {
            ThrowIfSourceNotCreated(src.IsCreated);
            ThrowIfDestinationNotCreated(dst.IsCreated);

            UnsafeUtility.MemCpy(dst._data, src._data, UnsafeUtility.SizeOf<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan()
        {
            unsafe
            {
                return new Span<T>(_data, 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
        {
            unsafe
            {
                return new ReadOnlySpan<T>(_data, 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe ReadOnly AsReadOnly()
            => new(_data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnly(ReferenceUnsafe<T> reference)
            => reference.AsReadOnly();

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckAllocateArguments(Allocator allocator)
        {
            ThrowIfAllocatorNotSupported(allocator > Allocator.None);
            ThrowIfCustomAllocator(allocator < Allocator.FirstUserIndex);
        }

        private static unsafe void Allocate(Allocator allocator, out ReferenceUnsafe<T> reference)
        {
            CheckAllocateArguments(allocator);
            reference = default;
            IsUnmanagedAndThrow();
            reference._allocatorLabel = allocator;
            reference._data = UnsafeUtility.MallocTracked(
                  UnsafeUtility.SizeOf<T>()
                , UnsafeUtility.AlignOf<T>()
                , allocator
                , 0
            );
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
                => new("The UnsafeReference is already disposed.");
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
                => new("The UnsafeReference can not be Disposed because it was not allocated with a valid allocator.");
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
                => new("Custom allocator is not supported by UnsafeReference", "allocator");
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
                => new($"{typeof(T)} used in UnsafeReference<{typeof(T)}> must be unmanaged (contain no managed types).");
        }

        [HideInCallstack, StackTraceHidden, Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowIfSourceNotCreated([DoesNotReturnIf(false)] bool isCreated)
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("The source UnsafeReference is not created.");
        }

        [HideInCallstack, StackTraceHidden, Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowIfDestinationNotCreated([DoesNotReturnIf(false)] bool isCreated)
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("The destination UnsafeReference is not created.");
        }

        [HideInCallstack, StackTraceHidden, Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowIfIndexOutOfRange(int index)
        {
            if (index != 0)
            {
                throw CreateException(index);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static IndexOutOfRangeException CreateException(int index)
                => new($"Index {index} is out of range of the UnsafeReference which only contains 1 element.");
        }

        /// <summary> A read-only alias for the value of an UnsafeReference. Does not have its own allocated storage. </summary>
        public readonly struct ReadOnly : IIsCreated
        {
            [NativeDisableUnsafePtrRestriction]
            private readonly unsafe void* _data;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal unsafe ReadOnly(void* data)
            {
                _data = data;
            }

            public unsafe bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (IntPtr)_data != IntPtr.Zero;
            }

            public unsafe T Value
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => *(T*)_data;
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        private struct UnsafeReferenceDisposeJob : IJob
        {
            internal UnsafeReferenceDispose Data;

            public readonly void Execute() => Data.Dispose();
        }

        private struct UnsafeReferenceDispose
        {
            [NativeDisableUnsafePtrRestriction]
            internal unsafe void* Data;

            internal Allocator AllocatorLabel;

            public readonly unsafe void Dispose() => UnsafeUtility.FreeTracked(Data, AllocatorLabel);
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
