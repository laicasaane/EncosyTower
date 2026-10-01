using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Common;
using Unity.Collections.LowLevel.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    partial class SharedArray<T, TNative>
    {
        partial struct ReadOnly
        {
            /// <safety>The returned native view must not outlive the source array.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe SharedArrayNative<TNative>.ReadOnly AsNative()
            {
                // SAFETY: The source array remains the designated owner of the returned native view.
                unsafe
                {
                    return _array.AsNative().AsReadOnly();
                }
            }
        }
    }

    partial struct SharedArrayNative<T>
    {
        /// <safety>The returned alias must not outlive the managed array owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnly AsReadOnly()
        {
            // SAFETY: The returned view borrows this array's live buffer and safety handle.
            unsafe
            {
                return new(
                      m_Buffer
                    , m_Length
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                    , m_Safety
#endif
                );
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        [NativeContainer]
        [NativeContainerIsReadOnly]
        public readonly struct ReadOnly : IReadOnlyList<T>, IReadOnlyIndexer<T>
            , IAsReadOnlySpan<T>, IToArray<T>, ICopyToSpan<T>, ITryCopyToSpan<T>
            , IHasLength, IIsCreated
        {
#pragma warning disable IDE1006 // Naming Styles
            /// <safety>The managed array owner must keep this borrowed buffer alive and stable.</safety>
            [NativeDisableUnsafePtrRestriction]
            internal readonly unsafe T* m_Buffer;
            internal readonly int m_Length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            internal readonly AtomicSafetyHandle m_Safety;
#endif
#pragma warning restore IDE1006 // Naming Styles

            /// <safety>The pointer, length, and safety handle must come from the same live owner.</safety>
            internal unsafe ReadOnly(
                  T* buffer
                , int length
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                , AtomicSafetyHandle safety
#endif
            )
            {
                // SAFETY: The constructor receives bounded borrowed storage from the owning shared array.
                unsafe
                {
                    m_Buffer = buffer;
                }

                m_Length = length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                m_Safety = safety;
#endif
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                    return AtomicSafetyHandle.IsDefaultValue(m_Safety) == false;
#else
                    // SAFETY: Pointer comparison only observes borrowed-storage state.
                    unsafe
                    {
                        return m_Buffer != null;
                    }
#endif
                }
            }

            public int Length
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Length;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Length;
            }

            public bool IsReadOnly => true;

            public T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    CheckRead();
                    ThrowHelper.ThrowIfIndexIsOutOfRange(
                          (uint)index < (uint)m_Length
                        , ThrowHelper.CollectionType.SharedArrayNative
                    );

                    // SAFETY: Read access and bounds are validated before reading borrowed storage.
                    unsafe
                    {
                        return m_Buffer[index];
                    }
                }
            }

            /// <safety>The returned alias must not outlive the managed owner.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnly(in SharedArrayNative<T> array)
            {
                DebuggingThrowHelper.ThrowIfNotCreated(array);

                // SAFETY: The caller accepts the managed-owner lifetime inherited by the alias.
                unsafe
                {
                    return array.AsReadOnly();
                }
            }

            /// <safety>The returned span must not outlive the managed owner.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnlySpan<T>(in ReadOnly array)
            {
                DebuggingThrowHelper.ThrowIfNotCreated(array);

                // SAFETY: The caller accepts the borrowed span lifetime.
                unsafe
                {
                    return array.AsReadOnlySpan();
                }
            }

            /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe Enumerator GetEnumerator()
            {
                // SAFETY: The enumerator borrows this checked read-only view.
                unsafe
                {
                    return new(this);
                }
            }

            /// <safety>The returned span must not outlive the managed owner.</safety>
            public unsafe ReadOnlySpan<T> AsReadOnlySpan()
            {
                CheckRead();

                // SAFETY: Read access validates borrowed storage for exactly m_Length elements.
                unsafe
                {
                    return new ReadOnlySpan<T>(m_Buffer, m_Length);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(T[] destination, int destinationIndex)
                => CopyTo(destination.AsSpan().Slice(destinationIndex, Length));

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

            /// <safety>The returned alias must not outlive the managed owner.</safety>
            public unsafe SharedArrayNative<U>.ReadOnly Reinterpret<U>()
                where U : unmanaged
            {
                CheckRead();
                ThrowHelper.ThrowIfTypesNotEqualSize<T, U>(UnsafeAPI.AreTypesEqualSize<T, U>());

                // SAFETY: Equal sizes preserve element boundaries and the result borrows this view.
                unsafe
                {
                    return new SharedArrayNative<U>.ReadOnly(
                          (U*)m_Buffer
                        , m_Length
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                        , m_Safety
#endif
                    );
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void CheckRead()
            {
                DebuggingThrowHelper.ThrowIfNotCreated(this);

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            }

            /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
            unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this checked read-only view.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
            unsafe IEnumerator IEnumerable.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this checked read-only view.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            public struct Enumerator : IEnumerator<T>
            {
                private ReadOnly _array;
                private int _index;

                internal Enumerator(ReadOnly array)
                {
                    _array = array;
                    _index = -1;
                }

                public readonly T Current => _array[_index];

                readonly object IEnumerator.Current => Current;

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
        }
    }
}
