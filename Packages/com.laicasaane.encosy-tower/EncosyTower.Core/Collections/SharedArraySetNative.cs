// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Dictionaries/SveltoDictionary.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        /// <safety>The returned native view borrows the shared set allocation and must not outlive the set.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe SharedArraySetNative<T> AsNative()
        {
            // SAFETY: This set owns the backing map for the complete returned-view lifetime.
            unsafe
            {
                return new(_map.AsNative());
            }
        }

        /// <safety>The returned native view must not outlive the source set or survive resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator SharedArraySetNative<T>([NotNull] SharedArraySet<T> set)
        {
            DebuggingThrowHelper.ThrowIfNull(set);

            // SAFETY: The non-null set remains the designated owner of the returned view.
            unsafe
            {
                return set.AsNative();
            }
        }
    }

    /// <summary>
    /// A fixed-capacity native view over the storage of a <see cref="SharedArraySet{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>Not thread-safe.</para>
    /// <para>Only the owning <see cref="SharedArraySet{T}"/> can grow the storage.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    [DebuggerTypeProxy(typeof(SharedArraySetNativeDebugProxy<>))]
    public readonly partial struct SharedArraySetNative<T> : IClearable, IHasCapacity, IHasCount, IIsCreated
        where T : unmanaged, IEquatable<T>
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <safety>The managed set owner must keep this borrowed map header alive and stable.</safety>
        [NativeDisableUnsafePtrRestriction]
        internal readonly unsafe SharedArrayMapUnsafe<T, T>* m_Data;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        internal readonly AtomicSafetyHandle m_Safety;
#endif
#pragma warning restore IDE1006 // Naming Styles

        /// <safety>The native map must remain backed by one live managed owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe SharedArraySetNative(in SharedArrayMapNative<T, T> map)
        {
            // SAFETY: The wrapper borrows the validated map header and shares its safety handle.
            unsafe
            {
                m_Data = map.m_Data;
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            m_Safety = map.m_Safety;
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
                // SAFETY: The returned slice inherits this set view's managed-owner lifetime.
                unsafe
                {
                    return AsMap().Values;
                }
            }
        }

        /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe SharedArraySetNativeEnumerator<T> GetEnumerator()
        {
            // SAFETY: The enumerator borrows this set view and its managed-owner lifetime.
            unsafe
            {
                return new(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T value)
        {
            // SAFETY: The temporary map alias is consumed synchronously by the checked value operation.
            unsafe
            {
                var map = AsMap();
                return map.TryAdd(value, in value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(in T value)
        {
            // SAFETY: The temporary map alias is consumed synchronously by the checked value operation.
            unsafe
            {
                var map = AsMap();
                return map.TryAdd(value, in value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            // SAFETY: The temporary map alias is consumed synchronously by the checked value operation.
            unsafe
            {
                var map = AsMap();
                map.Clear();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T value)
        {
            // SAFETY: The temporary map alias is consumed synchronously by the checked value operation.
            unsafe
            {
                var map = AsMap();
                return map.Remove(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T value)
        {
            // SAFETY: The temporary map alias is consumed synchronously by the checked value operation.
            unsafe
            {
                var map = AsMap();
                return map.Remove(value);
            }
        }

        public void Intersect(in SharedArraySetNative<T> otherSet)
        {
            // SAFETY: Both temporary map aliases are consumed synchronously by the two-view checked operation.
            unsafe
            {
                var map = AsMap();
                var otherMap = otherSet.AsMap();
                map.Intersect(in otherMap);
            }
        }

        public void Exclude(in SharedArraySetNative<T> otherSet)
        {
            // SAFETY: Both temporary map aliases are consumed synchronously by the two-view checked operation.
            unsafe
            {
                var map = AsMap();
                var otherMap = otherSet.AsMap();
                map.Exclude(in otherMap);
            }
        }

        public void Union(in SharedArraySetNative<T> otherSet)
        {
            // SAFETY: Both temporary map aliases are consumed synchronously by the two-view checked operation.
            unsafe
            {
                var map = AsMap();
                var otherMap = otherSet.AsMap();
                map.Union(in otherMap);
            }
        }

        /// <safety>The returned map alias must not outlive the managed set owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe SharedArrayMapNative<T, T> AsMap()
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

    public struct SharedArraySetNativeEnumerator<T> : IEnumerator<T>, IIsValid
        where T : unmanaged, IEquatable<T>
    {
        private SharedArrayMapNativeKeyValueEnumerator<T, T> _enumerator;

        /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
        public unsafe SharedArraySetNativeEnumerator(in SharedArraySetNative<T> set) : this()
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

    internal sealed class SharedArraySetNativeDebugProxy<T>
        where T : unmanaged, IEquatable<T>
    {
        private readonly SharedArraySetNative<T> _set;

        public SharedArraySetNativeDebugProxy(in SharedArraySetNative<T> set)
        {
            DebuggingThrowHelper.ThrowIfNotCreated(set);
            _set = set;
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)_set.Count;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var set = _set;
                var array = new T[set.Count];
                var i = 0;

                foreach (var value in set)
                {
                    array[i++] = value;
                }

                return array;
            }
        }
    }
}
