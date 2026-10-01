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
    partial class SharedArraySet<T>
    {
        partial struct ReadOnly
        {
            /// <safety>The returned native view borrows the shared set allocation and must not
            /// outlive the set.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe SharedArraySetNative<T>.ReadOnly AsNative()
            {
                // SAFETY: The managed set remains the designated owner of both borrowed aliases.
                unsafe
                {
                    return _set.AsNative().AsReadOnly();
                }
            }
        }
    }

    partial struct SharedArraySetNative<T>
    {
        /// <safety>The returned alias must not outlive the managed set owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnly AsReadOnly()
        {
            // SAFETY: The returned view borrows this shared set's live header and safety handle.
            unsafe
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                return new(m_Data, m_Safety);
#else
                return new(m_Data);
#endif
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        [NativeContainer]
        [NativeContainerIsReadOnly]
        public readonly struct ReadOnly : IHasCapacity, IHasCount, IIsCreated
        {
#pragma warning disable IDE1006 // Naming Styles
            /// <safety>The managed set owner must keep this borrowed map header alive and stable.</safety>
            [NativeDisableUnsafePtrRestriction]
            internal readonly unsafe SharedArrayMapUnsafe<T, T>* m_Data;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            internal readonly AtomicSafetyHandle m_Safety;
#endif
#pragma warning restore IDE1006 // Naming Styles

            /// <safety>The pointer and safety handle must come from the same live managed owner.</safety>
            internal unsafe ReadOnly(
                  SharedArrayMapUnsafe<T, T>* data
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                , AtomicSafetyHandle safety
#endif
            )
            {
                // SAFETY: The constructor receives a borrowed live header from the owning shared set.
                unsafe
                {
                    m_Data = data;
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                m_Safety = safety;
#endif
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    // SAFETY: The temporary map view is consumed before this property returns.
                    unsafe
                    {
                        return AsMap().IsCreated;
                    }
                }
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    // SAFETY: The temporary map view is consumed before this property returns.
                    unsafe
                    {
                        return AsMap().Capacity;
                    }
                }
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    // SAFETY: The temporary map view is consumed before this property returns.
                    unsafe
                    {
                        return AsMap().Count;
                    }
                }
            }

            /// <safety>The returned slice must not outlive the managed set owner or survive resize.</safety>
            public unsafe NativeSliceReadOnly<T> Items
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    // SAFETY: The returned slice inherits this view's managed-owner lifetime.
                    unsafe
                    {
                        return AsMap().Values;
                    }
                }
            }

            /// <safety>The returned alias must not outlive the managed set owner.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnly(SharedArraySetNative<T> set)
            {
                // SAFETY: The caller accepts the managed-owner lifetime inherited by the alias.
                unsafe
                {
                    return set.AsReadOnly();
                }
            }

            /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe SharedArraySetNativeReadOnlyEnumerator<T> GetEnumerator()
            {
                // SAFETY: The enumerator borrows this set view and its managed-owner lifetime.
                unsafe
                {
                    return new(this);
                }
            }

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
                // SAFETY: The copy consumes the borrowed slice before this method returns.
                unsafe
                {
                    new CopyToSpan<T>(Items.AsSpan()).CopyTo(sourceStartIndex, destination, length);
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
                // SAFETY: The copy consumes the borrowed slice before this method returns.
                unsafe
                {
                    return new CopyToSpan<T>(Items.AsSpan()).TryCopyTo(sourceStartIndex, destination, length);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(T value)
            {
                // SAFETY: The temporary map alias is consumed synchronously by the checked lookup.
                unsafe
                {
                    return AsMap().ContainsKey(value);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(in T value)
            {
                // SAFETY: The temporary map alias is consumed synchronously by the checked lookup.
                unsafe
                {
                    return AsMap().ContainsKey(value);
                }
            }

            /// <safety>The returned map alias must not outlive the managed set owner.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal unsafe SharedArrayMapNative<T, T>.ReadOnly AsMap()
            {
                // SAFETY: The set and map views use the same borrowed header and safety handle.
                unsafe
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                    return new(m_Data, m_Safety);
#else
                    return new(m_Data);
#endif
                }
            }
        }
    }

    public struct SharedArraySetNativeReadOnlyEnumerator<T> : IEnumerator<T>, IIsValid
        where T : unmanaged, IEquatable<T>
    {
        private SharedArrayMapNativeReadOnlyKeyValueEnumerator<T, T> _enumerator;

        /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
        public unsafe SharedArraySetNativeReadOnlyEnumerator(in SharedArraySetNative<T>.ReadOnly set) : this()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(set);
            // SAFETY: The enumerator borrows the set's map view and preserves version checks.
            unsafe
            {
                _enumerator = set.AsMap().GetEnumerator();
            }
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enumerator.IsValid;
        }

        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enumerator.Current.Key;
        }

        readonly object IEnumerator.Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Current;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
            => _enumerator.MoveNext();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
            => _enumerator.Reset();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
            => _enumerator.Dispose();
    }
}
