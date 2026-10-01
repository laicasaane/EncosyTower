using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Common;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Internal;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    public readonly partial struct SharedArrayNative<T> : IEnumerable<T>, IIsCreated, IHasLength, IIndexer<T>
        , ICopyToSpan<T>, ITryCopyToSpan<T>, ICopyFromSpan<T>, ITryCopyFromSpan<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>, IAsNativeSlice<T>, IClearable, IToArray<T>
        where T : unmanaged
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <safety>The managed owner pins this borrowed buffer until disposal.</safety>
        [NativeDisableUnsafePtrRestriction]
        internal readonly unsafe T* m_Buffer;
        internal readonly int m_Length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        internal readonly AtomicSafetyHandle m_Safety;
#endif
#pragma warning restore IDE1006 // Naming Styles

        /// <safety>The pointer must address length live elements owned by one managed shared array.</safety>
        internal unsafe SharedArrayNative(
              T* buffer
            , int length
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            , AtomicSafetyHandle safety
#endif
        )
        {
            // SAFETY: The caller supplies live bounded storage for this borrowed view.
            unsafe
            {
                m_Buffer = buffer;
            }

            m_Length = length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            m_Safety = safety;
#endif
        }

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

            [WriteAccessRequired]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CheckWrite();
                ThrowHelper.ThrowIfIndexIsOutOfRange(
                      (uint)index < (uint)m_Length
                    , ThrowHelper.CollectionType.SharedArrayNative
                );

                // SAFETY: Write access and bounds are validated before writing borrowed storage.
                unsafe
                {
                    m_Buffer[index] = value;
                }
            }
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Length;
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

        [WriteAccessRequired]
        /// <safety>The owner must remain alive while the returned reference is used.</safety>
        public unsafe ref T ElementAt(int index)
        {
            CheckWrite();
            ThrowHelper.ThrowIfIndexIsOutOfRange(
                  (uint)index < (uint)m_Length
                , ThrowHelper.CollectionType.SharedArrayNative
            );

            // SAFETY: Write access and bounds are validated before returning borrowed element reference.
            unsafe
            {
                return ref m_Buffer[index];
            }
        }

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<T> source)
        {
            // SAFETY: This view remains live for the complete checked copy.
            unsafe
            {
                CopyFrom(0, source);
            }
        }

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<T> source, int length)
        {
            // SAFETY: This view remains live for the complete checked copy.
            unsafe
            {
                CopyFrom(0, source, length);
            }
        }

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
        {
            // SAFETY: This view remains live for the complete checked copy.
            unsafe
            {
                CopyFrom(destinationStartIndex, source, source.Length);
            }
        }

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            CheckWrite();

            // SAFETY: CheckWrite validates the live buffer for the complete bounded copy.
            unsafe
            {
                new CopyFromSpan<T>(AsSpan()).CopyFrom(destinationStartIndex, source, length);
            }
        }

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(ReadOnlySpan<T> source)
        {
            // SAFETY: This view remains live for the complete checked copy attempt.
            unsafe
            {
                return TryCopyFrom(0, source);
            }
        }

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(ReadOnlySpan<T> source, int length)
        {
            // SAFETY: This view remains live for the complete checked copy attempt.
            unsafe
            {
                return TryCopyFrom(0, source, length);
            }
        }

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
        {
            // SAFETY: This view remains live for the complete checked copy attempt.
            unsafe
            {
                return TryCopyFrom(destinationStartIndex, source, source.Length);
            }
        }

        [WriteAccessRequired]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            CheckWrite();

            // SAFETY: CheckWrite validates the live buffer for the complete bounded copy attempt.
            unsafe
            {
                return new CopyFromSpan<T>(AsSpan()).TryCopyFrom(destinationStartIndex, source, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> destination)
        {
            // SAFETY: This view remains live for the complete checked copy.
            unsafe
            {
                CopyTo(0, destination);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> destination, int length)
        {
            // SAFETY: This view remains live for the complete checked copy.
            unsafe
            {
                CopyTo(0, destination, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: This view remains live for the complete checked copy.
            unsafe
            {
                CopyTo(sourceStartIndex, destination, destination.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            CheckRead();

            // SAFETY: CheckRead validates the live buffer for the complete bounded copy.
            unsafe
            {
                new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<T> destination)
        {
            // SAFETY: This view remains live for the complete checked copy attempt.
            unsafe
            {
                return TryCopyTo(0, destination);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<T> destination, int length)
        {
            // SAFETY: This view remains live for the complete checked copy attempt.
            unsafe
            {
                return TryCopyTo(0, destination, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(int sourceStartIndex, Span<T> destination)
        {
            // SAFETY: This view remains live for the complete checked copy attempt.
            unsafe
            {
                return TryCopyTo(sourceStartIndex, destination, destination.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            CheckRead();

            // SAFETY: CheckRead validates the live buffer for the complete bounded copy attempt.
            unsafe
            {
                return new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);
            }
        }

        [WriteAccessRequired]
        public void Clear()
        {
            CheckWrite();

            if (m_Length == 0)
            {
                return;
            }

            // SAFETY: Write access validates borrowed storage for exactly m_Length elements.
            unsafe
            {
                UnsafeUtility.MemClear(m_Buffer, (long)m_Length * UnsafeUtility.SizeOf<T>());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            var result = new T[m_Length];
            CopyTo(result);
            return result;
        }

        [WriteAccessRequired]
        /// <safety>The owner must remain alive while the returned span is used.</safety>
        public unsafe Span<T> AsSpan()
        {
            CheckWrite();

            // SAFETY: Write access validates borrowed storage for exactly m_Length elements.
            unsafe
            {
                return new Span<T>(m_Buffer, m_Length);
            }
        }

        /// <safety>The owner must remain alive while the returned span is used.</safety>
        public unsafe ReadOnlySpan<T> AsReadOnlySpan()
        {
            CheckRead();

            // SAFETY: Read access validates borrowed storage for exactly m_Length elements.
            unsafe
            {
                return new ReadOnlySpan<T>(m_Buffer, m_Length);
            }
        }

        [WriteAccessRequired]
        /// <safety>The returned alias must not be used after owner disposal.</safety>
        public unsafe NativeSlice<T> AsNativeSlice()
        {
            CheckWrite();

            // SAFETY: Write access validates borrowed storage and the alias uses Allocator.None.
            unsafe
            {
                var alias = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                      m_Buffer
                    , m_Length
                    , Allocator.None
                );

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref alias, m_Safety);
#endif
                return new NativeSlice<T>(alias);
            }
        }

        /// <safety>The owner must remain alive and T and U must have equal size.</safety>
        public unsafe SharedArrayNative<U> Reinterpret<U>()
            where U : unmanaged
        {
            CheckRead();
            ThrowHelper.ThrowIfTypesNotEqualSize<T, U>(UnsafeUtility.SizeOf<T>() == UnsafeUtility.SizeOf<U>());

            // SAFETY: Equal-size validation preserves element boundaries and the result borrows identical storage.
            unsafe
            {
                return new SharedArrayNative<U>(
                      (U*)m_Buffer
                    , m_Length
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                    , m_Safety
#endif
                );
            }
        }

        /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Enumerator GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return new(this);
            }
        }

        /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return GetEnumerator();
            }
        }

        /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator IEnumerable.GetEnumerator()
        {
            // SAFETY: The caller accepts the enumerator's borrowed owner lifetime.
            unsafe
            {
                return GetEnumerator();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckWrite()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(this);

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
        }

        [ExcludeFromDocs]
        public struct Enumerator : IEnumerator<T>
        {
            private SharedArrayNative<T> _array;
            private int _index;

            internal Enumerator(SharedArrayNative<T> array)
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
