using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Common;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    partial class SharedList<T, TNative>
    {
        /// <safety>The returned view must not outlive this list or survive a resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnly AsReadOnly()
        {
            // SAFETY: This list owns every allocation borrowed by the returned read-only view.
            unsafe
            {
                return new(this);
            }
        }

        public readonly partial struct ReadOnly : IReadOnlyList<T>, IToArray<T>, IReadOnlyIndexer<T>
            , IAsReadOnlySpan<T>, ICopyToSpan<T>, ITryCopyToSpan<T>, IHasCapacity, IHasCount, IIsCreated
        {
            private static readonly SharedList<T, TNative> s_emptyOwner = new();
            private static readonly ReadOnly s_empty = new(s_emptyOwner);

            internal readonly NativeArray<T>.ReadOnly _buffer;
            internal readonly NativeArray<int>.ReadOnly _count;
            internal readonly NativeArray<int>.ReadOnly _version;
            /// <safety>The managed list owner must keep this borrowed header alive and stable.</safety>
            internal readonly unsafe SharedListUnsafe<TNative>* _nativeData;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            internal readonly AtomicSafetyHandle _nativeSafety;
#endif

            /// <safety>The source list must remain alive and unresized while this view is used.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe ReadOnly(SharedList<T, TNative> list)
            {
                DebuggingThrowHelper.ThrowIfNull(list);
                list.CheckRead();

                NativeArray<T> buffer;
                NativeArray<int> count;
                NativeArray<int> version;

                // SAFETY: list owns the pinned buffers and remains the designated lifetime owner.
                unsafe
                {
                    buffer = list._buffer.AsNativeArray().Reinterpret<T>();
                    count = list._count.AsNativeArray();
                    version = list._version.AsNativeArray();
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                var aliasSafety = list._safety;
                AtomicSafetyHandle.UseSecondaryVersion(ref aliasSafety);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref buffer, aliasSafety);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref count, aliasSafety);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref version, aliasSafety);
#endif

                _buffer = buffer.AsReadOnly();
                _count = count.AsReadOnly();
                _version = version.AsReadOnly();
                // SAFETY: The read-only view borrows the live list header without taking ownership.
                unsafe
                {
                    _nativeData = list._nativeData;
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                _nativeSafety = aliasSafety;
#endif
            }

            /// <safety>All borrowed arrays and the header must share one live owner lifetime.</safety>
            private unsafe ReadOnly(
                  NativeArray<T>.ReadOnly buffer
                , NativeArray<int>.ReadOnly count
                , NativeArray<int>.ReadOnly version
                , SharedListUnsafe<TNative>* nativeData
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                , AtomicSafetyHandle nativeSafety
#endif
            )
            {
                _buffer = buffer;
                _count = count;
                _version = version;
                // SAFETY: The constructor receives the live header pointer from its owning list view.
                unsafe
                {
                    _nativeData = nativeData;
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                _nativeSafety = nativeSafety;
#endif
            }

            public static ReadOnly Empty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => s_empty;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.IsCreated && _count.IsCreated && _version.IsCreated;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _count[0];
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.Length;
            }

            public bool IsReadOnly => true;

            internal int Version
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _version[0];
            }

            /// <safety>This view's owner must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe Enumerator GetEnumerator()
            {
                // SAFETY: The enumerator borrows this view and preserves its version checks.
                unsafe
                {
                    return new(this);
                }
            }

            public T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ThrowHelper.ThrowIfIndexIsOutOfRange(
                          (uint)index < (uint)Count
                        , ThrowHelper.CollectionType.SharedListWithNativeReadOnly
                    );
                    // SAFETY: The checked index is read before the owner can mutate or dispose the backing buffer.
                    unsafe
                    {
                        return _buffer.AsReadOnlySpan()[index];
                    }
                }
            }

            /// <safety>The source list must remain alive and unresized while the alias is used.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnly(SharedList<T, TNative> list)
            {
                // SAFETY: A non-null list is the designated owner for the returned alias.
                unsafe
                {
                    return list is not null ? list.AsReadOnly() : Empty;
                }
            }

            /// <safety>The span must not outlive the read-only view's owner.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnlySpan<T>(in ReadOnly list)
            {
                DebuggingThrowHelper.ThrowIfNotCreated(list);

                // SAFETY: The caller accepts the borrowed span lifetime.
                unsafe
                {
                    return list.AsReadOnlySpan();
                }
            }

            /// <safety>The returned span must not outlive the read-only view's owner.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe ReadOnlySpan<T> AsReadOnlySpan()
            {
                // SAFETY: The caller accepts the backing owner's lifetime for the returned span.
                unsafe
                {
                    return _buffer.AsReadOnlySpan()[..Count];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(T[] destination, int destinationIndex)
                => CopyTo(destination.AsSpan().Slice(destinationIndex, Count));

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
                // SAFETY: The copy consumes the borrowed span before this method returns.
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
                // SAFETY: The copy consumes the borrowed span before this method returns.
                unsafe
                {
                    return new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T[] ToArray()
            {
                // SAFETY: ToArray copies the borrowed span before this method returns.
                unsafe
                {
                    return AsReadOnlySpan().ToArray();
                }
            }

            /// <safety>The returned alias must not outlive this view's owner or survive owner resize.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe SharedList<U, TNative>.ReadOnly Reinterpret<U>()
                where U : unmanaged
            {
                // SAFETY: The reinterpreted arrays preserve the original allocation and safety-handle lifetime.
                unsafe
                {
                    return new SharedList<U, TNative>.ReadOnly(
                          _buffer.Reinterpret<U>()
                        , _count
                        , _version
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                        , _nativeData
                        , _nativeSafety
#else
                        , _nativeData
#endif
                    );
                }
            }

            /// <safety>This view's owner must remain alive and unmodified during enumeration.</safety>
            unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this view for its checked lifetime.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>This view's owner must remain alive and unmodified during enumeration.</safety>
            unsafe IEnumerator IEnumerable.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this view for its checked lifetime.
                unsafe
                {
                    return GetEnumerator();
                }
            }
        }
    }
}
