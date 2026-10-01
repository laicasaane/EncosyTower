using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Extensions;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Common;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    partial class SharedArrayMap<TKey, TValue, TValueNative>
    {
        /// <safety>The returned view must not outlive this map or survive resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnly AsReadOnly()
        {
            // SAFETY: This map owns every allocation borrowed by the returned read-only view.
            unsafe
            {
                return new(this);
            }
        }

        public readonly partial struct ReadOnly : IHasCapacity, IHasCount, ITryGetValue<TKey, TValue>, IIsCreated
        {
            private static readonly SharedArrayMap<TKey, TValue, TValueNative> s_emptyOwner = new();
            private static readonly ReadOnly s_empty = new(s_emptyOwner);

            internal readonly NativeArray<ArrayMapNode<TKey>>.ReadOnly _valuesInfo;
            internal readonly NativeArray<TValue>.ReadOnly _values;
            internal readonly NativeArray<int>.ReadOnly _buckets;

            internal readonly NativeArray<ulong>.ReadOnly _fastModBucketsMultiplier;
            internal readonly NativeArray<uint>.ReadOnly _collisions;
            internal readonly NativeArray<int>.ReadOnly _freeValueCellIndex;
            internal readonly NativeArray<int>.ReadOnly _version;
            /// <safety>The managed map owner must keep this borrowed header alive and stable.</safety>
            internal readonly unsafe SharedArrayMapUnsafe<TKey, TValueNative>* _nativeData;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            internal readonly AtomicSafetyHandle _nativeSafety;
#endif

            /// <safety>The source map must remain alive and unresized while this view is used.</safety>
            public unsafe ReadOnly(SharedArrayMap<TKey, TValue, TValueNative> map)
            {
                DebuggingThrowHelper.ThrowIfNull(map);
                map.CheckRead();

                NativeArray<ArrayMapNode<TKey>> valuesInfo;
                NativeArray<TValue> values;
                NativeArray<int> buckets;
                NativeArray<int> freeValueCellIndex;
                NativeArray<int> version;
                NativeArray<uint> collisions;
                NativeArray<ulong> fastModBucketsMultiplier;

                // SAFETY: map owns all pinned buffers and remains the designated lifetime owner.
                unsafe
                {
                    valuesInfo = map._valuesInfo.AsNativeArray();
                    values = map._values.AsNativeArray().Reinterpret<TValue>();
                    buckets = map._buckets.AsNativeArray();
                    freeValueCellIndex = map._freeValueCellIndex.AsNativeArray();
                    version = map._version.AsNativeArray();
                    collisions = map._collisions.AsNativeArray();
                    fastModBucketsMultiplier = map._fastModBucketsMultiplier.AsNativeArray();
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                var aliasSafety = map._safety;
                AtomicSafetyHandle.UseSecondaryVersion(ref aliasSafety);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref valuesInfo, aliasSafety);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref values, aliasSafety);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref buckets, aliasSafety);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref freeValueCellIndex, aliasSafety);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref version, aliasSafety);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref collisions, aliasSafety);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref fastModBucketsMultiplier, aliasSafety);
#endif

                _valuesInfo = valuesInfo.AsReadOnly();
                _values = values.AsReadOnly();
                _buckets = buckets.AsReadOnly();
                _freeValueCellIndex = freeValueCellIndex.AsReadOnly();
                _version = version.AsReadOnly();
                _collisions = collisions.AsReadOnly();
                _fastModBucketsMultiplier = fastModBucketsMultiplier.AsReadOnly();
                // SAFETY: The read-only view borrows the live map header without taking ownership.
                unsafe
                {
                    _nativeData = map._nativeData;
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                _nativeSafety = aliasSafety;
#endif
            }

            public static ReadOnly Empty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => s_empty;
            }

            public readonly bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _valuesInfo.IsCreated && _values.IsCreated && _buckets.IsCreated;
            }

            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _values.Length;
            }

            public readonly int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _freeValueCellIndex[0];
            }

            public readonly KeyEnumerable Keys
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(this);
            }

            /// <safety>The returned slice must not outlive the managed map owner or survive resize.</safety>
            public readonly unsafe NativeSliceReadOnly<TValue> Values
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _values.Slice(0, _freeValueCellIndex[0]);
            }

            internal readonly int Version
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _version[0];
            }

            public TValue this[TKey key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _values[GetIndex(key)];
            }

            /// <safety>The returned alias must not outlive the managed map owner.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnly(SharedArrayMap<TKey, TValue, TValueNative> map)
            {
                // SAFETY: A non-null map remains the designated owner of the returned alias.
                unsafe
                {
                    return map is not null ? map.AsReadOnly() : Empty;
                }
            }

            /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly unsafe SharedArrayMapReadOnlyKeyValueEnumerator<TKey, TValue, TValueNative> GetEnumerator()
            {
                // SAFETY: The enumerator borrows this read-only view and preserves version checks.
                unsafe
                {
                    return new(this);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool ContainsKey(TKey key)
            {
                return TryFindIndex(key, out _);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryGetValue(TKey key, out TValue result)
            {
                if (TryFindIndex(key, out var findIndex))
                {
                    result = _values[findIndex];
                    return true;
                }

                result = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly int GetIndex(TKey key)
            {
                var found = TryFindIndex(key, out var findIndex);

                //Burst is not able to vectorise code if throw is found, regardless if it's actually ever thrown
                ThrowHelper.ThrowIfKeyIsNotFound(found);

                return findIndex;
            }

            // Indices are stored with an offset of 1 so that 0 represents a missing entry in the bucket list.
            //When read the offset must be offset by -1 again to be the real one. In this way
            // This avoids initializing the array to -1.

            //WARNING this method must stay stateless (not relying on states that can change, it's ok to read
            //constant states) because it will be used in multithreaded parallel code
            public readonly bool TryFindIndex(TKey key, out int findIndex)
            {
                ThrowHelper.ThrowIfBucketsAreUninitialized(
                      _buckets.Length > 0
                    , ThrowHelper.CollectionType.SharedArrayMap
                );

                var hash = key.GetHashCode();
                var bucketIndex = (int)Reduce((uint)hash, (uint)_buckets.Length, _fastModBucketsMultiplier[0]);
                var valueIndex = _buckets[bucketIndex] - 1;

                // An existing value must still be checked against the requested key.
                while (valueIndex != -1)
                {
                    //Comparer<TKey>.default needs to create a new comparer, so it is much slower
                    //than assuming that Equals is implemented through IEquatable
                    var node = _valuesInfo[valueIndex];

                    if (node._hashcode == hash && node.key.Equals(key))
                    {
                        //this is the one
                        findIndex = valueIndex;
                        return true;
                    }

                    valueIndex = node._previous;
                }

                findIndex = 0;
                return false;
            }

            public readonly struct KeyEnumerable : IEnumerable<TKey>, IIsValid
            {
                private readonly ReadOnly _map;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public KeyEnumerable(in ReadOnly map)
                {
                    DebuggingThrowHelper.ThrowIfNotCreated(map);
                    _map = map;
                }

                public bool IsValid
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _map.IsCreated;
                }

                /// <safety>The managed owner must remain alive and unmodified during enumeration.</safety>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public unsafe KeyEnumerator GetEnumerator()
                {
                    // SAFETY: The key enumerator borrows this map view and preserves version checks.
                    unsafe
                    {
                        return new(_map);
                    }
                }

                /// <safety>The map must remain alive and unmodified during enumeration.</safety>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                unsafe IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
                {
                    // SAFETY: The interface enumerator borrows this map view.
                    unsafe
                    {
                        return GetEnumerator();
                    }
                }

                /// <safety>The map must remain alive and unmodified during enumeration.</safety>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                unsafe IEnumerator IEnumerable.GetEnumerator()
                {
                    // SAFETY: The interface enumerator borrows this map view.
                    unsafe
                    {
                        return GetEnumerator();
                    }
                }
            }

            public struct KeyEnumerator : IEnumerator<TKey>, IIsValid
            {
                private readonly ReadOnly _map;
                private readonly int _version;

                private int _index;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public KeyEnumerator(in ReadOnly map) : this()
                {
                    DebuggingThrowHelper.ThrowIfNotCreated(map);
                    _map = map;
                    _index = -1;
                    _version = map.Version;
                }

                public readonly bool IsValid
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _map.IsCreated;
                }

                public readonly TKey Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _map._valuesInfo.AsReadOnlySpan()[_index].key;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    ThrowHelper.ThrowIfEnumeratorIsInvalid(IsValid);
                    ThrowHelper.ThrowIfMapIsBeingIterated(_version == _map.Version);

                    if (_index < _map.Count - 1)
                    {
                        ++_index;
                        return true;
                    }

                    return false;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset()
                {
                    _index = -1;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly void Dispose()
                {
                }

                readonly object IEnumerator.Current => Current;
            }
        }
    }

    public struct SharedArrayMapReadOnlyKeyValueEnumerator<TKey, TValue, TValueNative>
        : IEnumerator<SharedArrayMapReadOnlyKeyValuePair<TKey, TValue, TValueNative>>, IIsValid
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {
        private readonly SharedArrayMap<TKey, TValue, TValueNative>.ReadOnly _map;
        private readonly int _version;

        private int _index;

        public SharedArrayMapReadOnlyKeyValueEnumerator(
            in SharedArrayMap<TKey, TValue, TValueNative>.ReadOnly map
        ) : this()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(map);
            _map = map;
            _index = -1;
            _version = map.Version;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.IsCreated;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            ThrowHelper.ThrowIfEnumeratorIsInvalid(IsValid);
            ThrowHelper.ThrowIfMapIsBeingIterated(_version == _map.Version);

            if (_index >= _map.Count - 1)
            {
                return false;
            }

            ++_index;
            return true;
        }

        public readonly SharedArrayMapReadOnlyKeyValuePair<TKey, TValue, TValueNative> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // SAFETY: Enumerator checks bound the index and the pair inherits the map owner's lifetime.
                unsafe
                {
                    return new(_map._valuesInfo.AsReadOnlySpan()[_index].key, _map._values, _index);
                }
            }
        }

        readonly object IEnumerator.Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Current;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
        }
    }

    public readonly struct SharedArrayMapReadOnlyKeyValuePair<TKey, TValue, TValueNative> : IIsValid
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {
        private readonly NativeArray<TValue>.ReadOnly _mapValues;
        private readonly TKey _key;
        private readonly int _index;

        /// <safety>The native array owner must remain alive and stable while this borrowed pair is used.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe SharedArrayMapReadOnlyKeyValuePair(in TKey key, NativeArray<TValue>.ReadOnly mapValues, int index)
        {
            _mapValues = mapValues;
            _index = index;
            _key = key;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _mapValues.IsCreated;
        }

        public TKey Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _key;
        }

        /// <safety>The returned reference must not outlive the native array owner.</safety>
        public readonly unsafe ref readonly TValue Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // SAFETY: Construction preserved the native array and bounded index owner contract.
                unsafe
                {
                    return ref _mapValues.AsReadOnlySpan()[_index];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out TKey key, out TValue value)
        {
            key = Key;

            // SAFETY: The value is copied immediately while this borrowed pair remains valid.
            unsafe
            {
                value = Value;
            }
        }
    }
}
