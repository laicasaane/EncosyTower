// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Dictionaries/SveltoDictionary.cs

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;
using EncosyTower.Common;

namespace EncosyTower.Collections.Unsafe
{
    internal struct SharedArrayMapUnsafe<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        /// <safety>The owner pins this values-info buffer and refreshes the pointer after relocation.</safety>
        internal unsafe ArrayMapNode<TKey>* _valuesInfo;
        internal int _valuesInfoCapacity;
        /// <safety>The owner pins this values buffer and refreshes the pointer after relocation.</safety>
        internal unsafe TValue* _values;
        internal int _valuesCapacity;
        /// <safety>The owner pins this buckets buffer and refreshes the pointer after relocation.</safety>
        internal unsafe int* _buckets;
        internal int _bucketsCapacity;

        /// <safety>The owner keeps this borrowed scalar live until header disposal.</safety>
        internal unsafe ulong* _fastModBucketsMultiplier;
        /// <safety>The owner keeps this borrowed scalar live until header disposal.</safety>
        internal unsafe uint* _collisions;
        /// <safety>The owner keeps this borrowed scalar live until header disposal.</safety>
        internal unsafe int* _freeValueCellIndex;
        /// <safety>The owner keeps this borrowed scalar live until header disposal.</safety>
        internal unsafe int* _version;

        /// <safety>
        /// Every supplied pointer must address live pinned storage owned by one map, capacities must match their
        /// buffers, and the returned persistent header must have exactly one owner.
        /// </safety>
        internal static unsafe SharedArrayMapUnsafe<TKey, TValue>* Alloc(
              ArrayMapNode<TKey>* valuesInfo
            , int valuesInfoCapacity
            , TValue* values
            , int valuesCapacity
            , int* buckets
            , int bucketsCapacity
            , ulong* fastModBucketsMultiplier
            , uint* collisions
            , int* freeValueCellIndex
            , int* version
            , AllocatorStrategy allocator
        )
        {
            // SAFETY: The allocator returns writable storage for the complete unmanaged struct.
            unsafe
            {
                var data = allocator.Allocate<SharedArrayMapUnsafe<TKey, TValue>>();
                *data = new SharedArrayMapUnsafe<TKey, TValue> {
                    _valuesInfo = valuesInfo,
                    _valuesInfoCapacity = valuesInfoCapacity,
                    _values = values,
                    _valuesCapacity = valuesCapacity,
                    _buckets = buckets,
                    _bucketsCapacity = bucketsCapacity,
                    _fastModBucketsMultiplier = fastModBucketsMultiplier,
                    _collisions = collisions,
                    _freeValueCellIndex = freeValueCellIndex,
                    _version = version,
                };
                return data;
            }
        }

        /// <safety>
        /// Null is a no-op. Otherwise data must come from the matching allocator, be released once, and have no
        /// aliases used afterward.
        /// </safety>
        internal static unsafe void Free(SharedArrayMapUnsafe<TKey, TValue>* data, AllocatorStrategy allocator)
        {
            if (data == null)
            {
                return;
            }

            // SAFETY: The pointer was allocated by the matching allocator and is freed once by its owner.
            unsafe
            {
                allocator.Free(data);
            }
        }

        internal int Capacity => _valuesCapacity;

        internal int BucketCapacity => _bucketsCapacity;

        /// <safety>The owner must keep the count pointer live for the complete read.</safety>
        internal unsafe int Count
        {
            get
            {
                // SAFETY: The owning map keeps the count storage live while this header is borrowed.
                unsafe
                {
                    return *_freeValueCellIndex;
                }
            }
        }

        /// <safety>The owner must keep the version pointer live for the complete read.</safety>
        internal unsafe int Version
        {
            get
            {
                // SAFETY: The owning map keeps the version storage live while this header is borrowed.
                unsafe
                {
                    return *_version;
                }
            }
        }

        /// <safety>The owner must keep values-info storage live and index must be valid.</safety>
        internal unsafe TKey KeyAt(int index)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the indexed read.
            unsafe
            {
                return GetValuesInfoSpan()[index].key;
            }
        }

        /// <safety>The owner must remain live while the returned reference is used and index must be valid.</safety>
        internal unsafe ref readonly TValue ValueAt(int index)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the returned reference.
            unsafe
            {
                return ref GetValuesSpan()[index];
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe TValue this[TKey key]
        {
            get
            {
                // SAFETY: The caller accepts this header's borrowed-pointer contract for the indexed read.
                unsafe
                {
                    return GetValuesSpan()[GetIndex(key)];
                }
            }
            set
            {
                // SAFETY: The caller accepts this header's borrowed-pointer contract for the map update.
                unsafe
                {
                    AddValue(key, out var index);
                    GetValuesSpan()[index] = value;
                }
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe void Add(TKey key, in TValue value)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the map update.
            unsafe
            {
                var itemAdded = AddValue(key, out var index);
                ThrowHelper.ThrowIfKeyIsPresent(itemAdded);

                if (itemAdded)
                {
                    GetValuesSpan()[index] = value;
                }
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe bool TryAdd(TKey key, in TValue value)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the map update.
            unsafe
            {
                return TryAdd(key, in value, out _);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe bool TryAdd(TKey key, in TValue value, out int index)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the map update.
            unsafe
            {
                var itemAdded = AddValue(key, out index);

                if (itemAdded)
                {
                    GetValuesSpan()[index] = value;
                }

                return itemAdded;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe void Clear()
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete clear.
            unsafe
            {
                if (Count == 0)
                {
                    return;
                }

                IncrementVersion();
                SetCount(0);
                GetBucketsSpan().Clear();
            }
        }

        /// <safety>The owner must keep every borrowed pointer live for the complete copy.</safety>
        internal unsafe void CopyTo(
              Span<ArrayMapNode<TKey>> valuesInfo
            , Span<TValue> values
            , Span<int> buckets
            , out int count
            , out uint collisions
            , out ulong fastModBucketsMultiplier
        )
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the complete copy.
            unsafe
            {
                GetValuesInfoSpan().CopyTo(valuesInfo);
                GetValuesSpan().CopyTo(values);
                GetBucketsSpan().CopyTo(buckets);
                count = Count;
                collisions = (uint)GetCollisions();
                fastModBucketsMultiplier = GetFastModBucketsMultiplier();
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe bool ContainsKey(TKey key)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the lookup.
            unsafe
            {
                return TryFindIndex(key, out _);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe bool TryGetValue(TKey key, out TValue result)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the lookup.
            unsafe
            {
                if (TryFindIndex(key, out var findIndex))
                {
                    result = GetValuesSpan()[findIndex];
                    return true;
                }

                result = default;
                return false;
            }
        }

        /// <safety>The owner must remain live while the returned reference is used.</safety>
        internal unsafe ref TValue GetOrAdd(TKey key)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the returned reference.
            unsafe
            {
                var values = GetValuesSpan();

                if (TryFindIndex(key, out var findIndex))
                {
                    IncrementVersion();
                    return ref values[findIndex];
                }

                AddValue(key, out findIndex);
                values[findIndex] = default;
                return ref values[findIndex];
            }
        }

        /// <safety>The owner must remain live while the returned reference is used.</safety>
        internal unsafe ref TValue GetOrAdd(TKey key, out int index)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the returned reference.
            unsafe
            {
                var values = GetValuesSpan();

                if (TryFindIndex(key, out index))
                {
                    IncrementVersion();
                    return ref values[index];
                }

                AddValue(key, out index);
                return ref values[index];
            }
        }

        /// <safety>The owner must remain live while the returned reference is used.</safety>
        internal unsafe ref TValue GetValueByRef(TKey key)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the returned reference.
            unsafe
            {
                var found = TryFindIndex(key, out var findIndex);
                ThrowHelper.ThrowIfKeyIsNotFound(found);

                IncrementVersion();
                return ref GetValuesSpan()[findIndex];
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe bool Remove(TKey key)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the removal.
            unsafe
            {
                return Remove(key, out _, out _);
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe bool Remove(TKey key, out int index, out TValue value)
        {
            // SAFETY: The caller keeps all borrowed map storage live for the complete removal.
            unsafe
            {
                var buckets = GetBucketsSpan();
                var valuesInfo = GetValuesInfoSpan();
                var values = GetValuesSpan();
                var fastModBucketsMultiplier = GetFastModBucketsMultiplier();

                var hash = key.GetHashCode();
                var bucketIndex = (int)Reduce((uint)hash, (uint)buckets.Length, fastModBucketsMultiplier);
                var indexToValueToRemove = buckets[bucketIndex] - 1;
                var itemAfterCurrentOne = -1;

                while (indexToValueToRemove != -1)
                {
                    ref var node = ref valuesInfo[indexToValueToRemove];

                    if (node._hashcode == hash && key.Equals(node.key))
                    {
                        if (buckets[bucketIndex] - 1 == indexToValueToRemove)
                        {
                            buckets[bucketIndex] = node._previous + 1;
                        }
                        else
                        {
                            ThrowHelper.ThrowIfMissingLinkedListNode(
                                  itemAfterCurrentOne != -1
                                , ThrowHelper.CollectionType.SharedArrayMapUnsafe
                            );
                            valuesInfo[itemAfterCurrentOne]._previous = node._previous;
                        }

                        break;
                    }

                    itemAfterCurrentOne = indexToValueToRemove;
                    indexToValueToRemove = node._previous;
                }

                if (indexToValueToRemove == -1)
                {
                    index = default;
                    value = default;
                    return false;
                }

                IncrementVersion();
                index = indexToValueToRemove;

                var lastValueCellIndex = Count - 1;
                SetCount(lastValueCellIndex);
                value = values[indexToValueToRemove];

                if (indexToValueToRemove != lastValueCellIndex)
                {
                    ref var nodeToMove = ref valuesInfo[lastValueCellIndex];
                    var movingBucketIndex = (int)Reduce(
                          (uint)nodeToMove._hashcode
                        , (uint)buckets.Length
                        , fastModBucketsMultiplier
                    );
                    var linkedListIterationIndex = buckets[movingBucketIndex] - 1;

                    if (linkedListIterationIndex == lastValueCellIndex)
                    {
                        buckets[movingBucketIndex] = indexToValueToRemove + 1;
                    }

                    while (valuesInfo[linkedListIterationIndex]._previous != -1
                        && valuesInfo[linkedListIterationIndex]._previous != lastValueCellIndex
                    )
                    {
                        linkedListIterationIndex = valuesInfo[linkedListIterationIndex]._previous;
                    }

                    if (valuesInfo[linkedListIterationIndex]._previous != -1)
                    {
                        valuesInfo[linkedListIterationIndex]._previous = indexToValueToRemove;
                    }

                    valuesInfo[indexToValueToRemove] = nodeToMove;
                    values[indexToValueToRemove] = values[lastValueCellIndex];
                }

                return true;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe bool TryFindIndex(TKey key, out int findIndex)
        {
            // SAFETY: The caller keeps all borrowed map storage live for the complete lookup.
            unsafe
            {
                var buckets = GetBucketsSpan();
                var valuesInfo = GetValuesInfoSpan();

                ThrowHelper.ThrowIfBucketsAreUninitialized(
                      buckets.Length > 0
                    , ThrowHelper.CollectionType.SharedArrayMapUnsafe
                );

                var hash = key.GetHashCode();
                var bucketIndex = (int)Reduce((uint)hash, (uint)buckets.Length, GetFastModBucketsMultiplier());
                var valueIndex = buckets[bucketIndex] - 1;

                while (valueIndex != -1)
                {
                    ref readonly var node = ref valuesInfo[valueIndex];

                    if (node._hashcode == hash && key.Equals(node.key))
                    {
                        findIndex = valueIndex;
                        return true;
                    }

                    valueIndex = node._previous;
                }

                findIndex = 0;
                return false;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        internal unsafe int GetIndex(TKey key)
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the lookup.
            unsafe
            {
                var found = TryFindIndex(key, out var findIndex);
                ThrowHelper.ThrowIfKeyIsNotFound(found);
                return findIndex;
            }
        }

        /// <safety>Both map headers and all borrowed storage must remain live for the complete operation.</safety>
        internal unsafe void Intersect<UValue>(SharedArrayMapUnsafe<TKey, UValue>* otherMapKeys)
            where UValue : unmanaged
        {
            // SAFETY: The caller supplies two live borrowed headers with valid map state.
            unsafe
            {
                for (var i = Count - 1; i >= 0; i--)
                {
                    var key = GetValuesInfoSpan()[i].key;

                    if (otherMapKeys->ContainsKey(key) == false)
                    {
                        Remove(key);
                    }
                }
            }
        }

        /// <safety>Both map headers and all borrowed storage must remain live for the complete operation.</safety>
        internal unsafe void Exclude<UValue>(SharedArrayMapUnsafe<TKey, UValue>* otherMapKeys)
            where UValue : unmanaged
        {
            // SAFETY: The caller supplies two live borrowed headers with valid map state.
            unsafe
            {
                for (var i = Count - 1; i >= 0; i--)
                {
                    var key = GetValuesInfoSpan()[i].key;

                    if (otherMapKeys->ContainsKey(key))
                    {
                        Remove(key);
                    }
                }
            }
        }

        /// <safety>Both map headers and all borrowed storage must remain live for the complete operation.</safety>
        internal unsafe void Union(SharedArrayMapUnsafe<TKey, TValue>* otherMap)
        {
            // SAFETY: The caller supplies a live borrowed header whose occupied range is contiguous.
            unsafe
            {
                var count = otherMap->Count;
                for (var i = 0; i < count; i++)
                {
                    this[otherMap->KeyAt(i)] = otherMap->ValueAt(i);
                }
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        private unsafe bool AddValue(TKey key, out int indexSet)
        {
            // SAFETY: The caller keeps all borrowed map storage live for the complete insertion.
            unsafe
            {
                var valuesInfo = GetValuesInfoSpan();
                var buckets = GetBucketsSpan();
                var count = Count;
                var collisions = GetCollisions();
                var hash = key.GetHashCode();
                var bucketIndex = (int)Reduce((uint)hash, (uint)buckets.Length, GetFastModBucketsMultiplier());
                var valueIndex = buckets[bucketIndex] - 1;

                if (valueIndex == -1)
                {
                    ResizeIfNeeded();
                    valuesInfo[count] = new ArrayMapNode<TKey>(key, hash);
                }
                else
                {
                    var currentValueIndex = valueIndex;

                    do
                    {
                        ref var node = ref valuesInfo[currentValueIndex];

                        if (node._hashcode == hash && key.Equals(node.key))
                        {
                            indexSet = currentValueIndex;
                            return false;
                        }

                        currentValueIndex = node._previous;
                    } while (currentValueIndex != -1);

                    ResizeIfNeeded();
                    collisions++;
                    valuesInfo[count] = new ArrayMapNode<TKey>(key, hash, valueIndex);
                }

                IncrementVersion();
                buckets[bucketIndex] = count + 1;
                indexSet = count;
                SetCount(count + 1);
                SetCollisions(collisions);

                if (collisions > buckets.Length)
                {
                    RecomputeBuckets();
                }

                return true;
            }
        }

        /// <safety>The owner must keep every borrowed pointer live and provide valid map state.</safety>
        private unsafe void RecomputeBuckets()
        {
            // SAFETY: The caller keeps all borrowed map storage live for the complete bucket rebuild.
            unsafe
            {
                var valuesInfo = GetValuesInfoSpan();
                var buckets = GetBucketsSpan();
                buckets.Clear();

                var collisions = 0;
                var bucketsCapacity = (uint)buckets.Length;
                var fastModBucketsMultiplier = HashHelpers.GetFastModMultiplier(bucketsCapacity);
                SetFastModBucketsMultiplier(fastModBucketsMultiplier);

                var count = Count;

                for (var newValueIndex = 0; newValueIndex < count; ++newValueIndex)
                {
                    ref var valueInfoNode = ref valuesInfo[newValueIndex];
                    var bucketIndex = (int)Reduce(
                          (uint)valueInfoNode._hashcode
                        , bucketsCapacity
                        , fastModBucketsMultiplier
                    );
                    var existingValueIndex = buckets[bucketIndex] - 1;
                    buckets[bucketIndex] = newValueIndex + 1;

                    if (existingValueIndex == -1)
                    {
                        valueInfoNode._previous = -1;
                    }
                    else
                    {
                        collisions++;
                        valueInfoNode._previous = existingValueIndex;
                    }
                }

                SetCollisions(collisions);
            }
        }

        /// <safety>The owner must keep the count pointer live for the complete capacity check.</safety>
        private unsafe void ResizeIfNeeded()
        {
            // SAFETY: The caller accepts this header's borrowed-pointer contract for the count read.
            unsafe
            {
                ThrowHelper.ThrowIfCapacityIsImmutable(
                      Count != _valuesCapacity
                    , ThrowHelper.CollectionType.SharedArrayMapUnsafe
                );
            }
        }

        /// <safety>The owner must keep values-info storage live and unchanged while the span is used.</safety>
        private unsafe Span<ArrayMapNode<TKey>> GetValuesInfoSpan()
        {
            // SAFETY: The owner pins a values-info buffer matching the recorded capacity.
            unsafe
            {
                return new Span<ArrayMapNode<TKey>>(_valuesInfo, _valuesInfoCapacity);
            }
        }

        /// <safety>The owner must keep values storage live and unchanged while the span is used.</safety>
        private unsafe Span<TValue> GetValuesSpan()
        {
            // SAFETY: The owner pins a values buffer matching the recorded capacity.
            unsafe
            {
                return new Span<TValue>(_values, _valuesCapacity);
            }
        }

        /// <safety>The owner must keep bucket storage live and unchanged while the span is used.</safety>
        private unsafe Span<int> GetBucketsSpan()
        {
            // SAFETY: The owner pins a bucket buffer matching the recorded capacity.
            unsafe
            {
                return new Span<int>(_buckets, _bucketsCapacity);
            }
        }

        /// <safety>The owner must keep the multiplier pointer live for the complete read.</safety>
        private unsafe ulong GetFastModBucketsMultiplier()
        {
            // SAFETY: The owner keeps the multiplier storage live while the header is borrowed.
            unsafe
            {
                return *_fastModBucketsMultiplier;
            }
        }

        /// <safety>The owner must keep the multiplier pointer live for the complete write.</safety>
        private unsafe void SetFastModBucketsMultiplier(ulong value)
        {
            // SAFETY: The owner keeps the multiplier storage live while the header is borrowed.
            unsafe
            {
                *_fastModBucketsMultiplier = value;
            }
        }

        /// <safety>The owner must keep the collision pointer live for the complete read.</safety>
        private unsafe int GetCollisions()
        {
            // SAFETY: The owner keeps the collision storage live while the header is borrowed.
            unsafe
            {
                return (int)*_collisions;
            }
        }

        /// <safety>The owner must keep the collision pointer live for the complete write.</safety>
        private unsafe void SetCollisions(int value)
        {
            // SAFETY: The owner keeps the collision storage live while the header is borrowed.
            unsafe
            {
                *_collisions = (uint)value;
            }
        }

        /// <safety>The owner must keep the count pointer live for the complete write.</safety>
        private unsafe void SetCount(int count)
        {
            // SAFETY: The owner keeps the count storage live while the header is borrowed.
            unsafe
            {
                *_freeValueCellIndex = count;
            }
        }

        /// <safety>The owner must keep the version pointer live for the complete update.</safety>
        private unsafe void IncrementVersion()
        {
            // SAFETY: The owner keeps the version storage live while the header is borrowed.
            unsafe
            {
                (*_version)++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Reduce(uint hashcode, uint length, ulong fastModBucketsMultiplier)
        {
            if (hashcode >= length)
            {
                return Environment.Is64BitProcess
                    ? HashHelpers.FastMod(hashcode, length, fastModBucketsMultiplier)
                    : hashcode % length;
            }

            return hashcode;
        }
    }
}
