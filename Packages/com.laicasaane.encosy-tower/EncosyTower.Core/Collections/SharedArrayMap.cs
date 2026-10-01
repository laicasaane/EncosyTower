// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Dictionaries/SveltoDictionary.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Common;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    /// <summary>
    /// A dictionary that stores its values in a contiguous array, so the values
    /// can be iterated directly as an array, without an enumerator.
    /// Most operations perform on par with <see cref="Dictionary{TKey, TValue}"/>.
    /// Growing on add is slower, because two internal arrays must be resized.
    /// <br/>
    /// Its storage can be shared with a <see cref="SharedArrayMapNative{TKey, TValue}"/>
    /// without copying.
    /// </summary>
    /// <remarks>
    /// Not thread-safe.
    /// </remarks>
    [Serializable, DebuggerTypeProxy(typeof(SharedArrayMapDebugProxy<,>))]
    public partial class SharedArrayMap<TKey, TValue> : SharedArrayMap<TKey, TValue, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        public SharedArrayMap() : base()
        {
        }

        public SharedArrayMap(int capacity) : base(capacity)
        {
        }
    }

    /// <summary>
    /// A dictionary that stores its values in a contiguous array, so the values
    /// can be iterated directly as an array, without an enumerator.
    /// Most operations perform on par with <see cref="Dictionary{TKey, TValue}"/>.
    /// Growing on add is slower, because two internal arrays must be resized.
    /// <br/>
    /// Its storage can be shared with a <see cref="SharedArrayMapNative{TKey, TValue}"/>
    /// without copying.
    /// </summary>
    /// <remarks>
    /// Not thread-safe.
    /// </remarks>
    /// <typeparam name="TValue">
    /// The value type on the managed side.
    /// </typeparam>
    /// <typeparam name="TValueNative">
    /// The value type on the <see cref="SharedArrayMapNative{TKey, TValue}"/> side.
    /// Must be the same size as <typeparamref name="TValue"/>.
    /// </typeparam>
    [Serializable, DebuggerTypeProxy(typeof(SharedArrayMapDebugProxy<,,>))]
    public partial class SharedArrayMap<TKey, TValue, TValueNative> : IDisposable
        , ICollection<SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>>
        , IReadOnlyCollection<SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>>
        , IClearable, IIncreaseCapacity, IHasCount, ITryGetValue<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {
        [NonSerialized] internal BufferShared<ArrayMapNode<TKey>> _valuesInfo;
        [NonSerialized] internal BufferShared<TValue, TValueNative> _values;
        [NonSerialized] internal BufferShared<int> _buckets;

        [NonSerialized] internal BufferShared<ulong> _fastModBucketsMultiplier;
        [NonSerialized] internal BufferShared<uint> _collisions;
        [NonSerialized] internal BufferShared<int> _freeValueCellIndex;
        [NonSerialized] internal BufferShared<int> _version;

        // Native views share this live header but copy the values array's safety handle.
        // Resizing values releases that handle, invalidating older checked views; later
        // views use refreshed pointers/capacities and the new representative handle.
        /// <safety>Owned by this map; borrowed native views become invalid after resize or disposal.</safety>
        [NonSerialized] internal unsafe SharedArrayMapUnsafe<TKey, TValueNative>* _nativeData;
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        [NonSerialized] internal AtomicSafetyHandle _safety;
#endif

        public SharedArrayMap() : this(0)
        {
        }

        public SharedArrayMap(int capacity)
        {
            _valuesInfo = new(capacity);
            _values = new(capacity);
            _buckets = new(HashHelpers.GetPrime(capacity));
            _freeValueCellIndex = new(1);
            _version = new(1);
            _collisions = new(1);
            _fastModBucketsMultiplier = new(1);

            if (capacity > 0)
            {
                // SAFETY: This constructor exclusively owns the initialized scalar buffer.
                unsafe
                {
                    _fastModBucketsMultiplier[0] = HashHelpers.GetFastModMultiplier((uint)_buckets.Capacity);
                }
            }

            InitializeNativeData();
        }

        public SharedArrayMap([NotNull] SharedArrayMap<TKey, TValue, TValueNative> source)
        {
            DebuggingThrowHelper.ThrowIfNull(source);
            source.CheckRead();
            var capacity = source.Capacity;
            var bucketCapacity = source._buckets.Capacity;

            _valuesInfo = new(capacity);
            _values = new(capacity);
            _buckets = new(bucketCapacity);
            _freeValueCellIndex = new(1);
            _version = new(1);
            _collisions = new(1);
            _fastModBucketsMultiplier = new(1);

            // SAFETY: source passed its read check and this constructor exclusively owns each destination buffer.
            unsafe
            {
                source._valuesInfo.AsSpan().CopyTo(_valuesInfo.AsSpan());
                source._values.AsSpan().CopyTo(_values.AsSpan());
                source._buckets.AsSpan().CopyTo(_buckets.AsSpan());

                _freeValueCellIndex[0] = source.FreeValueCellIndexRO;
                _collisions[0] = source.CollisionsRO;
                _fastModBucketsMultiplier[0] = source.FastModBucketsMultiplierRO;
            }
            InitializeNativeData();
        }

        public SharedArrayMap(in SharedArrayMapNative<TKey, TValueNative> source)
        {
            DebuggingThrowHelper.ThrowIfNotCreated(source);

            var capacity = source.Capacity;
            var bucketCapacity = source.BucketCapacity;

            _valuesInfo = new(capacity);
            _values = new(capacity);
            _buckets = new(bucketCapacity);
            _freeValueCellIndex = new(1);
            _version = new(1);
            _collisions = new(1);
            _fastModBucketsMultiplier = new(1);

            int count;
            uint collisions;
            ulong fastModBucketsMultiplier;

            // SAFETY: source is live and this constructor owns all destination buffers for the synchronous copy.
            unsafe
            {
                source.CopyTo(
                      _valuesInfo.AsSpan()
                    , _values.AsNativeArray().AsSpan()
                    , _buckets.AsSpan()
                    , out count
                    , out collisions
                    , out fastModBucketsMultiplier
                );

                _freeValueCellIndex[0] = count;
                _collisions[0] = collisions;
                _fastModBucketsMultiplier[0] = fastModBucketsMultiplier;
            }
            InitializeNativeData();
        }

        ~SharedArrayMap()
        {
            // SAFETY: Finalization releases only storage still owned by this map.
            unsafe
            {
                Dispose();
            }
        }

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values.IsCreated;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!_values.IsCreated)
                {
                    return 0;
                }
                CheckRead();
                return _values.Capacity;
            }
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!_values.IsCreated)
                {
                    return 0;
                }
                return FreeValueCellIndexRO;
            }
        }

        public KeyEnumerable Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(this);
        }

        public ReadOnlyMemory<TValue> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { CheckRead(); return _values.AsArraySegment().Slice(0, FreeValueCellIndexRO); }
        }

        bool ICollection<SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>>.IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // SAFETY: GetIndex completes the owner read check before the bounded access.
                unsafe
                {
                    return _values.AsReadOnlySpan()[GetIndex(key)];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                AddValue(key, out var index);

                // SAFETY: AddValue completes the owner write checks and returns a valid index.
                unsafe
                {
                    _values.AsSpan()[index] = value;
                }
            }
        }

        /// <safety>The returned native view must not outlive the managed map or survive resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator SharedArrayMapNative<TKey, TValueNative>(
            [NotNull] SharedArrayMap<TKey, TValue, TValueNative> map
        )
        {
            DebuggingThrowHelper.ThrowIfNull(map);
            // SAFETY: The non-null map is the designated owner of the returned borrowed view.
            unsafe
            {
                return map.AsNative();
            }
        }

        public void Dispose()
        {
            if (!_valuesInfo.IsCreated)
            {
                return;
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckDeallocateAndThrow(_safety);
#endif
            // SAFETY: This map owns the live header and releases it before every owned buffer.
            unsafe
            {
                SharedArrayMapUnsafe<TKey, TValueNative>.Free(_nativeData, Allocator.Persistent);
                _nativeData = null;

                _valuesInfo.Dispose();
                _values.Dispose();
                _buckets.Dispose();

                _freeValueCellIndex.Dispose();
                _collisions.Dispose();
                _fastModBucketsMultiplier.Dispose();
                _version.Dispose();
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.Release(_safety);
            _safety = default;
#endif
        }

        /// <remarks>
        /// The map must not be modified while it is being enumerated.
        /// </remarks>
        /// <safety>This map must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe SharedArrayMapKeyValueEnumerator<TKey, TValue, TValueNative> GetEnumerator()
        {
            // SAFETY: The enumerator borrows this owner and preserves version checks.
            unsafe
            {
                return new(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, in TValue value)
        {
            var itemAdded = AddValue(key, out var index);

            ThrowHelper.ThrowIfKeyIsPresent(itemAdded);

            if (itemAdded)
            {
                // SAFETY: AddValue validated the owner and returned a bounded writable index.
                unsafe
                {
                    _values.AsSpan()[index] = value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, in TValue value)
        {
            var itemAdded = AddValue(key, out var index);

            if (itemAdded)
            {
                // SAFETY: AddValue validated the owner and returned a bounded writable index.
                unsafe
                {
                    _values.AsSpan()[index] = value;
                }
            }

            return itemAdded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, in TValue value, out int index)
        {
            var itemAdded = AddValue(key, out index);

            if (itemAdded)
            {
                // SAFETY: AddValue validated the owner and returned a bounded writable index.
                unsafe
                {
                    _values.AsSpan()[index] = value;
                }
            }

            return itemAdded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            ref var freeValueCellIndex = ref FreeValueCellIndexRW;

            if (freeValueCellIndex == 0)
            {
                return;
            }

            VersionRW++;
            freeValueCellIndex = 0;

            // Buckets cannot be FastCleared because it's important that the values are reset to 0
            // SAFETY: This map owns the bucket buffer and the write check completed above.
            unsafe
            {
                _buckets.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            return TryFindIndex(key, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue result)
        {
            if (TryFindIndex(key, out var findIndex))
            {
                // SAFETY: TryFindIndex completed the read check and returned a bounded index.
                unsafe
                {
                    result = _values.AsReadOnlySpan()[findIndex];
                }
                return true;
            }

            result = default;
            return false;
        }

        /// <safety>The returned reference must not outlive this map or survive resize or mutation.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TValue GetOrAdd(TKey key)
        {
            CheckWrite();

            // SAFETY: The write check validates owned storage; returned references inherit this map's lifetime.
            unsafe
            {
                var values = _values.AsSpan();

                if (TryFindIndex(key, out var findIndex))
                {
                    VersionRW++;
                    return ref values[findIndex];
                }

                AddValue(key, out findIndex);
                values = _values.AsSpan();

                values[findIndex] = default;

                return ref values[findIndex];
            }
        }

        /// <safety>The returned reference must not outlive this map or survive resize or mutation.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TValue GetOrAdd(TKey key, out int index)
        {
            CheckWrite();

            // SAFETY: The write check validates owned storage; returned references inherit this map's lifetime.
            unsafe
            {
                var values = _values.AsSpan();

                if (TryFindIndex(key, out index))
                {
                    VersionRW++;
                    return ref values[index];
                }

                AddValue(key, out index);
                values = _values.AsSpan();

                return ref values[index];
            }
        }

        /// <safety>The returned reference must not outlive this map or survive resize or mutation.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TValue GetValueByRef(TKey key)
        {
            var found = TryFindIndex(key, out var findIndex);

            // Burst is not able to vectorise code if throw is found, regardless if it's actually ever thrown
            ThrowHelper.ThrowIfKeyIsNotFound(found);

            VersionRW++;

            // SAFETY: The key lookup bounds the index and the returned reference inherits this map's lifetime.
            unsafe
            {
                return ref _values.AsSpan()[findIndex];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int size)
        {
            if (_values.Capacity < size)
            {
                var expandPrime = HashHelpers.ExpandPrime(size);

                VersionRW++;
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(_safety);
#endif
                // SAFETY: This map owns both buffers and bumped the secondary safety version before relocation.
                unsafe
                {
                    _values.Resize(expandPrime, true);
                    _valuesInfo.Resize(expandPrime);
                }
                RefreshNativeData();
            }

            return _values.Capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IncreaseCapacityBy(int amount)
            => EnsureCapacity(_values.Capacity + amount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IncreaseCapacityTo(int size)
            => EnsureCapacity(size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            return Remove(key, out _, out _);
        }

        public bool Remove(TKey key, out int index, out TValue value)
        {
            CheckWrite();

            // SAFETY: The write check validates every owned buffer used by this synchronous removal algorithm.
            unsafe
            {
            var buckets = _buckets.AsSpan();
            var valuesInfo = _valuesInfo.AsSpan();
            var values = _values.AsSpan();
            var fastModBucketsMultiplier = _fastModBucketsMultiplier.AsReadOnlySpan()[0];
            ref var freeValueCellIndex = ref _freeValueCellIndex.AsSpan()[0];

            var hash = key.GetHashCode();
            var bucketIndex = (int)Reduce((uint)hash, (uint)buckets.Length, fastModBucketsMultiplier);

            //find the bucket
            var indexToValueToRemove = buckets[bucketIndex] - 1;
            var itemAfterCurrentOne = -1;

            // Part one: find the key in the bucket list and update the list so that it does not
            //point anymore to the cell to remove
            while (indexToValueToRemove != -1)
            {
                ref var node = ref valuesInfo[indexToValueToRemove];

                if (node._hashcode == hash && key.Equals(node.key))
                {
                    //if the key is found and the bucket points directly to the node to remove
                    if (buckets[bucketIndex] - 1 == indexToValueToRemove)
                    {
                        //the bucket will point to the previous cell. if a previous cell exists
                        //its next pointer must be updated!
                        //<--- iteration order
                        //                      Bucket points always to the last one
                        //   ------- ------- -------
                        //   |  1  | |  2  | |  3  | //bucket cannot have next, only previous
                        //   ------- ------- -------
                        //--> insert order
                        buckets[bucketIndex] = node._previous + 1;
                    }
                    else // The previous pointer must be updated when the removed element is not the last one.
                    {
                        ThrowHelper.ThrowIfMissingLinkedListNode(
                              itemAfterCurrentOne != -1
                            , ThrowHelper.CollectionType.SharedArrayMap
                        );
                        //update the previous pointer of the item after the one to remove with the
                        //previous pointer of the item to remove
                        valuesInfo[itemAfterCurrentOne]._previous = node._previous;
                    }

                    break; // Stop here without updating indexToValueToRemove.
                }

                // A bucket always points to the last element of the list, so a missing item
                // requires backward iteration.
                itemAfterCurrentOne = indexToValueToRemove;
                indexToValueToRemove = node._previous;
            }

            if (indexToValueToRemove == -1)
            {
                index = default;
                value = default;
                return false; //not found!
            }

            VersionRW++;
            index = indexToValueToRemove; // The out parameter exposes the index of the element to remove.

            freeValueCellIndex--; //one less value to iterate
            value = values[indexToValueToRemove]; // The out parameter exposes the value of the element to remove.

            //Part two:
            //At this point nodes pointers and buckets are updated, but the _values array
            //still contains the value to delete. The map must support
            //to iterate over the values like an array, so the values array must always be up to date

            // Removing the last cell requires fewer operations because no swap is needed.
            // Otherwise, the last value cell replaces the removed value.

            var lastValueCellIndex = freeValueCellIndex;

            if (indexToValueToRemove != lastValueCellIndex)
            {
                // Transfer the last value of both arrays to the index of the value being removed.
                // The bucket pointer must be updated accordingly.
                // First, find the bucket-list index of the pointer to the cell.
                //to move
                ref var modeToMove = ref valuesInfo[lastValueCellIndex];

                var movingBucketIndex = (int)Reduce(
                      (uint)modeToMove._hashcode
                    , (uint)buckets.Length
                    , fastModBucketsMultiplier
                );

                var linkedListIterationIndex = buckets[movingBucketIndex] - 1;

                //if the key is found and the bucket points directly to the node to remove
                //it must now point to the cell where it's going to be moved (update bucket list
                //first linked list node to iterate from)
                if (linkedListIterationIndex == lastValueCellIndex)
                {
                    buckets[movingBucketIndex] = indexToValueToRemove + 1;
                }

                //find the prev element of the last element in the valuesInfo array
                while (valuesInfo[linkedListIterationIndex]._previous != -1
                    && valuesInfo[linkedListIterationIndex]._previous != lastValueCellIndex
                )
                {
                    linkedListIterationIndex = valuesInfo[linkedListIterationIndex]._previous;
                }

                // Any value whose previous node is the last value cell must point to the replacement index.
                if (valuesInfo[linkedListIterationIndex]._previous != -1)
                {
                    valuesInfo[linkedListIterationIndex]._previous = indexToValueToRemove;
                }

                //finally, actually move the values
                valuesInfo[indexToValueToRemove] = modeToMove;
                values[indexToValueToRemove] = values[lastValueCellIndex];
            }

            return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            var count = Count;

            if (count == _values.Capacity)
            {
                return;
            }

            VersionRW++;
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(_safety);
#endif
            // SAFETY: This map owns both buffers and bumped the secondary safety version before relocation.
            unsafe
            {
                _values.Resize(count);
                _valuesInfo.Resize(count);
            }
            RefreshNativeData();
        }

        // Indices are stored with an offset of 1 so that 0 represents a missing entry in the bucket list.
        //When read the offset must be offset by -1 again to be the real one. In this way
        // This avoids initializing the array to -1.

        //WARNING this method must stay stateless (not relying on states that can change, it's ok to read
        //constant states) because it will be used in multithreaded parallel code
        public bool TryFindIndex(TKey key, out int findIndex)
        {
            CheckRead();

            // SAFETY: The read check validates all owned buffers for this bounded lookup.
            unsafe
            {
            var buckets = _buckets.AsReadOnlySpan();
            var valuesInfo = _valuesInfo.AsReadOnlySpan();
            var fastModBucketsMultiplier = _fastModBucketsMultiplier.AsReadOnlySpan()[0];

            ThrowHelper.ThrowIfBucketsAreUninitialized(buckets.Length > 0, ThrowHelper.CollectionType.SharedArrayMap);

            var hash = key.GetHashCode();
            var bucketIndex = (int)Reduce((uint)hash, (uint)buckets.Length, fastModBucketsMultiplier);
            var valueIndex = buckets[bucketIndex] - 1;

            // An existing value must still be checked against the requested key.
            while (valueIndex != -1)
            {
                ref readonly var node = ref valuesInfo[valueIndex];

                if (node._hashcode == hash && key.Equals(node.key))
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(TKey key)
        {
            var found = TryFindIndex(key, out var findIndex);

            //Burst is not able to vectorise code if throw is found, regardless if it's actually ever thrown
            ThrowHelper.ThrowIfKeyIsNotFound(found);

            return findIndex;
        }

        public void Intersect<UValue, UValueNative>([NotNull] SharedArrayMap<TKey, UValue, UValueNative> otherMapKeys)
            where UValue : unmanaged
            where UValueNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNull(otherMapKeys);
            CheckWrite();

            // SAFETY: The write check validates this map's key buffer for the synchronous set operation.
            unsafe
            {
            var keys = _valuesInfo.AsSpan();

            for (var i = Count - 1; i >= 0; i--)
            {
                var tKey = keys[i].key;

                if (otherMapKeys.ContainsKey(tKey) == false)
                {
                    Remove(tKey);
                }
            }
            }
        }

        public void Exclude<UValue, UValueNative>([NotNull] SharedArrayMap<TKey, UValue, UValueNative> otherMapKeys)
            where UValue : unmanaged
            where UValueNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNull(otherMapKeys);
            CheckWrite();

            // SAFETY: The write check validates this map's key buffer for the synchronous set operation.
            unsafe
            {
            var keys = _valuesInfo.AsSpan();

            for (var i = Count - 1; i >= 0; i--)
            {
                var tKey = keys[i].key;

                if (otherMapKeys.ContainsKey(tKey))
                {
                    Remove(tKey);
                }
            }
            }
        }

        public void Union([NotNull] SharedArrayMap<TKey, TValue, TValueNative> otherMapKeys)
        {
            DebuggingThrowHelper.ThrowIfNull(otherMapKeys);

            foreach (var other in otherMapKeys)
            {
                this[other.Key] = other.Value;
            }
        }

        private bool AddValue(TKey key, out int indexSet)
        {
            CheckWrite();

            // SAFETY: The write check validates every owned buffer used by this synchronous insertion algorithm.
            unsafe
            {
            var valuesInfo = _valuesInfo.AsSpan();
            var buckets = _buckets.AsSpan();
            var fastModBucketsMultiplier = _fastModBucketsMultiplier.AsReadOnlySpan()[0];
            ref var freeValueCellIndex = ref _freeValueCellIndex.AsSpan()[0];
            ref var collisions = ref _collisions.AsSpan()[0];

            var hash = key.GetHashCode(); //IEquatable doesn't enforce the override of GetHashCode
            var bucketIndex = (int)Reduce((uint)hash, (uint)buckets.Length, fastModBucketsMultiplier);

            //buckets value -1 means it's empty
            var valueIndex = buckets[bucketIndex] - 1;

            if (valueIndex == -1)
            {
                if (ResizeIfNeeded())
                {
                    valuesInfo = _valuesInfo.AsSpan();
                }

                //create the info node at the last position and fill it with the relevant information
                valuesInfo[freeValueCellIndex] = new ArrayMapNode<TKey>(key, hash);
            }
            else //collision or already exists
            {
                var currentValueIndex = valueIndex;

                do
                {
                    //must check if the key already exists in the map
                    ref var node = ref valuesInfo[currentValueIndex];

                    if (node._hashcode == hash && key.Equals(node.key))
                    {
                        //the key already exists, simply replace the value!
                        indexSet = currentValueIndex;
                        return false;
                    }

                    currentValueIndex = node._previous;
                } while (currentValueIndex != -1); //-1 means no more values with key with the same hash

                if (ResizeIfNeeded())
                {
                    valuesInfo = _valuesInfo.AsSpan();
                }

                //oops collision!
                collisions++;

                //create a new node which previous index points to node currently pointed in the bucket (valueIndex)
                //_freeValueCellIndex = valueIndex + 1
                valuesInfo[freeValueCellIndex] = new ArrayMapNode<TKey>(key, hash, valueIndex);
                //Important: the new node is always the one that will be pointed by the bucket cell
                // Therefore, the bucket always points to the last value added.
            }

            VersionRW++;

            //item with this bucketIndex will point to the last value created
            // TODO: If the original node is assumed to be the one in the bucket,
            // the bucket would not need to be updated here. This is a small but important optimization.
            buckets[bucketIndex] = freeValueCellIndex + 1;

            indexSet = freeValueCellIndex;
            freeValueCellIndex++;

            //too many collisions
            if (collisions > buckets.Length)
            {
                if (buckets.Length < 100)
                {
                    RecomputeBuckets((int)collisions << 1);
                }
                else
                {
                    RecomputeBuckets(HashHelpers.ExpandPrime((int)collisions));
                }
            }

            return true;
            }
        }

        private void RecomputeBuckets(int newSize)
        {
            // More space is needed to reduce collisions.
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(_safety);
#endif
            // SAFETY: This map owns the bucket and scalar buffers and bumped the secondary version before resize.
            unsafe
            {
            _buckets.Resize(newSize, false);
            RefreshNativeData();

            var valuesInfo = _valuesInfo.AsSpan();
            var buckets = _buckets.AsSpan();

            ref var collisions = ref _collisions.AsSpan()[0];
            collisions = 0;

            var bucketsCapacity = (uint)buckets.Length;
            var fastModBucketsMultiplier = _fastModBucketsMultiplier.AsSpan()[0]
                = HashHelpers.GetFastModMultiplier(bucketsCapacity);

            // Redistribute the hash codes of all stored values across the new buckets.
            //length
            var freeValueCellIndex = _freeValueCellIndex.AsSpan()[0];

            for (var newValueIndex = 0; newValueIndex < freeValueCellIndex; ++newValueIndex)
            {
                //get the original hash code and find the new bucketIndex due to the new length
                ref var valueInfoNode = ref valuesInfo[newValueIndex];
                var bucketIndex = (int)Reduce((uint)valueInfoNode._hashcode, bucketsCapacity, fastModBucketsMultiplier);

                //bucketsIndex can be -1 or a next value. If it's -1 means no collisions. If there is collision,
                // Create a new node whose previous pointer targets the old node, then point the
                // old node to the new one.
                //the bucket will now points to the new one
                // This rebuilds the linked list.
                //get the current valueIndex, it's -1 if no collision happens
                var existingValueIndex = buckets[bucketIndex] - 1;

                //update the bucket index to the index of the current item that share the bucketIndex
                //(last found is always the one in the bucket)
                buckets[bucketIndex] = newValueIndex + 1;

                if (existingValueIndex == -1)
                {
                    // Nothing was indexed because the bucket was empty, so the previous pointer must be updated.
                    //values of next and previous
                    valueInfoNode._previous = -1;
                }
                else
                {
                    //oops a value was already being pointed by this cell in the new bucket list,
                    //it means there is a collision, problem
                    collisions++;
                    //the bucket will point to this value, so
                    //the previous index will be used as previous for the new value.
                    valueInfoNode._previous = existingValueIndex;
                }
            }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ResizeIfNeeded()
        {
            int freeValueCellIndex;

            // SAFETY: This map owns the scalar count buffer for the duration of the read.
            unsafe
            {
                freeValueCellIndex = _freeValueCellIndex.AsReadOnlySpan()[0];
            }

            if (freeValueCellIndex != _values.Capacity)
            {
                return false;
            }

            var expandPrime = HashHelpers.ExpandPrime(freeValueCellIndex);

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(_safety);
#endif
            // SAFETY: This map owns both buffers and bumped the secondary version before relocation.
            unsafe
            {
                _values.Resize(expandPrime, true);
                _valuesInfo.Resize(expandPrime);
            }
            RefreshNativeData();

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeNativeData()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            _safety = AtomicSafetyHandle.Create();
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(_safety, true);
#endif
            // SAFETY: This map owns every pinned buffer passed to its newly allocated native header.
            unsafe
            {
                _nativeData = SharedArrayMapUnsafe<TKey, TValueNative>.Alloc(
                      _valuesInfo.GetUnsafeBufferPointer()
                    , _valuesInfo.Capacity
                    , _values.GetUnsafeBufferPointer()
                    , _values.Capacity
                    , _buckets.GetUnsafeBufferPointer()
                    , _buckets.Capacity
                    , _fastModBucketsMultiplier.GetUnsafeBufferPointer()
                    , _collisions.GetUnsafeBufferPointer()
                    , _freeValueCellIndex.GetUnsafeBufferPointer()
                    , _version.GetUnsafeBufferPointer()
                    , Allocator.Persistent
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RefreshNativeData()
        {
            // SAFETY: The shared native header is live and all pointers are refreshed from still-owned storage.
            unsafe
            {
                _nativeData->_valuesInfo = _valuesInfo.GetUnsafeBufferPointer();
                _nativeData->_valuesInfoCapacity = _valuesInfo.Capacity;
                _nativeData->_values = _values.GetUnsafeBufferPointer();
                _nativeData->_valuesCapacity = _values.Capacity;
                _nativeData->_buckets = _buckets.GetUnsafeBufferPointer();
                _nativeData->_bucketsCapacity = _buckets.Capacity;
                _nativeData->_fastModBucketsMultiplier = _fastModBucketsMultiplier.GetUnsafeBufferPointer();
                _nativeData->_collisions = _collisions.GetUnsafeBufferPointer();
                _nativeData->_freeValueCellIndex = _freeValueCellIndex.GetUnsafeBufferPointer();
                _nativeData->_version = _version.GetUnsafeBufferPointer();
            }
        }

        internal ref readonly ulong FastModBucketsMultiplierRO
        {
            get
            {
                CheckRead();

                // SAFETY: The read check validates the owned scalar buffer.
                unsafe
                {
                    return ref _fastModBucketsMultiplier[0];
                }
            }
        }

        internal ref readonly uint CollisionsRO
        {
            get
            {
                CheckRead();

                // SAFETY: The read check validates the owned scalar buffer.
                unsafe
                {
                    return ref _collisions[0];
                }
            }
        }

        internal ref readonly int FreeValueCellIndexRO
        {
            get
            {
                CheckRead();

                // SAFETY: The read check validates the owned scalar buffer.
                unsafe
                {
                    return ref _freeValueCellIndex[0];
                }
            }
        }

        internal ref int FreeValueCellIndexRW
        {
            get
            {
                CheckWrite();

                // SAFETY: The write check validates the owned scalar buffer.
                unsafe
                {
                    return ref _freeValueCellIndex[0];
                }
            }
        }

        internal ref readonly int VersionRO
        {
            get
            {
                CheckRead();

                // SAFETY: The read check validates the owned scalar buffer.
                unsafe
                {
                    return ref _version[0];
                }
            }
        }

        internal ref int VersionRW
        {
            get
            {
                CheckWrite();

                // SAFETY: The write check validates the owned scalar buffer.
                unsafe
                {
                    return ref _version[0];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CheckRead()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckReadAndThrow(_safety);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckWrite()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(_safety);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Reduce(uint hashcode, uint N, ulong fastModBucketsMultiplier)
        {
            if (hashcode >= N) //is the condition return actually an optimization?
            {
                return Environment.Is64BitProcess
                ? HashHelpers.FastMod(hashcode, N, fastModBucketsMultiplier)
                : hashcode % N;
            }

            return hashcode;
        }

        /// <safety>This map must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator<SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>>
            IEnumerable<SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>>.GetEnumerator()
        {
            // SAFETY: The interface enumerator borrows this map and preserves version checks.
            unsafe
            {
                return GetEnumerator();
            }
        }

        /// <safety>This map must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator IEnumerable.GetEnumerator()
        {
            // SAFETY: The interface enumerator borrows this map and preserves version checks.
            unsafe
            {
                return GetEnumerator();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICollection<SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>>.Add(
            SharedArrayMapKeyValuePair<TKey, TValue, TValueNative> item
        )
        {
            TryAdd(item.Key, item.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ICollection<SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>>.Contains(
            SharedArrayMapKeyValuePair<TKey, TValue, TValueNative> item
        )
        {
            return TryGetValue(item.Key, out var value)
                && EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        void ICollection<SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>>.CopyTo(
              SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>[] array
            , int arrayIndex
        )
        {
            DebuggingThrowHelper.ThrowIfNull(array);
            ThrowHelper.ThrowIfArrayIndexIsOutOfRange((uint)arrayIndex <= (uint)array.Length, nameof(arrayIndex));
            ThrowHelper.ThrowIfDestinationArrayIsTooSmall(array.Length - arrayIndex >= Count);

            // SAFETY: The enumerator is consumed synchronously while this map cannot be modified here.
            unsafe
            {
                var enumerator = GetEnumerator();

                while (enumerator.MoveNext())
                {
                    array[arrayIndex++] = enumerator.Current;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ICollection<SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>>.Remove(
            SharedArrayMapKeyValuePair<TKey, TValue, TValueNative> item
        )
        {
            return TryGetValue(item.Key, out var value)
                && EqualityComparer<TValue>.Default.Equals(value, item.Value)
                && Remove(item.Key);
        }

        public readonly struct KeyEnumerable : IEnumerable<TKey>, IIsValid
        {
            private readonly SharedArrayMap<TKey, TValue, TValueNative> _map;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyEnumerable([NotNull] SharedArrayMap<TKey, TValue, TValueNative> map)
            {
                DebuggingThrowHelper.ThrowIfNull(map);
                _map = map;
            }

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map != null;
            }

            /// <safety>The map must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe KeyEnumerator GetEnumerator()
            {
                // SAFETY: The key enumerator borrows this map and preserves version checks.
                unsafe
                {
                    return new(_map);
                }
            }

            /// <safety>The map must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this map.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>The map must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe IEnumerator IEnumerable.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this map.
                unsafe
                {
                    return GetEnumerator();
                }
            }
        }

        public struct KeyEnumerator : IEnumerator<TKey>, IIsValid
        {
            private readonly SharedArrayMap<TKey, TValue, TValueNative> _map;
            private readonly int _version;

            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyEnumerator([NotNull] SharedArrayMap<TKey, TValue, TValueNative> map) : this()
            {
                DebuggingThrowHelper.ThrowIfNull(map);
                _map = map;
                _index = -1;
                _version = map.VersionRO;
            }

            public readonly bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map != null;
            }

            public readonly TKey Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    _map.CheckRead();

                    // SAFETY: The read and version checks validate the bounded values-info access.
                    unsafe
                    {
                        return _map._valuesInfo.AsReadOnlySpan()[_index].key;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                ThrowHelper.ThrowIfEnumeratorIsInvalid(IsValid);
                ThrowHelper.ThrowIfMapIsBeingIterated(_version == _map.VersionRO);

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

    public struct SharedArrayMapKeyValueEnumerator<TKey, TValue, TValueNative>
        : IEnumerator<SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>>, IIsValid
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {
        private readonly SharedArrayMap<TKey, TValue, TValueNative> _map;
        private readonly int _version;

        private int _index;

        public SharedArrayMapKeyValueEnumerator([NotNull] SharedArrayMap<TKey, TValue, TValueNative> map) : this()
        {
            DebuggingThrowHelper.ThrowIfNull(map);
            _map = map;
            _index = -1;
            _version = map.VersionRO;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            ThrowHelper.ThrowIfEnumeratorIsInvalid(IsValid);
            ThrowHelper.ThrowIfMapIsBeingIterated(_version == _map.VersionRO);

            if (_index >= _map.Count - 1)
            {
                return false;
            }

            ++_index;
            return true;
        }

        public readonly SharedArrayMapKeyValuePair<TKey, TValue, TValueNative> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _map.CheckRead();

                // SAFETY: The read and version checks bound access; the pair stores the managed array owner.
                unsafe
                {
                    return new(
                          _map._valuesInfo.AsReadOnlySpan()[_index].key
                        , _map._values.AsManagedArray()
                        , _index
                    );
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

    [DebuggerDisplay("[{Key}] = {Value}")]
    [DebuggerTypeProxy(typeof(SharedArrayMapKeyValuePairDebugProxy<,,>))]
    public readonly struct SharedArrayMapKeyValuePair<TKey, TValue, TValueNative> : IIsValid
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {
        private readonly TValue[] _mapValues;
        private readonly TKey _key;
        private readonly int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedArrayMapKeyValuePair(in TKey key, [NotNull] TValue[] mapValues, int index)
        {
            DebuggingThrowHelper.ThrowIfNull(mapValues);
            _mapValues = mapValues;
            _index = index;
            _key = key;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _mapValues != null;
        }

        public TKey Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _key;
        }

        public readonly ref readonly TValue Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _mapValues[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out TKey key, out TValue value)
        {
            key = Key;
            value = Value;
        }
    }

    internal sealed class SharedArrayMapKeyValuePairDebugProxy<TKey, TValue, TValueNative>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {

        private readonly SharedArrayMapKeyValuePair<TKey, TValue, TValueNative> _keyValue;

        public SharedArrayMapKeyValuePairDebugProxy(in SharedArrayMapKeyValuePair<TKey, TValue, TValueNative> keyValue)
        {
            _keyValue = keyValue;
        }

        public TKey Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _keyValue.Key;
        }

        public TValue Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _keyValue.Value;
        }
    }

    internal sealed class SharedArrayMapDebugProxy<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {

        private readonly SharedArrayMap<TKey, TValue> _map;

        public SharedArrayMapDebugProxy([NotNull] SharedArrayMap<TKey, TValue> map)
        {
            DebuggingThrowHelper.ThrowIfNull(map);
            _map = map;
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)_map.Count;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public SharedArrayMapKeyValuePair<TKey, TValue, TValue>[] Items
        {
            get
            {
                var map = _map;
                var array = new SharedArrayMapKeyValuePair<TKey, TValue, TValue>[map.Count];
                var i = 0;

                foreach (var keyValue in map)
                {
                    array[i++] = keyValue;
                }

                return array;
            }
        }
    }

    internal sealed class SharedArrayMapDebugProxy<TKey, TValue, TValueNative>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {

        private readonly SharedArrayMap<TKey, TValue, TValueNative> _map;

        public SharedArrayMapDebugProxy([NotNull] SharedArrayMap<TKey, TValue, TValueNative> map)
        {
            DebuggingThrowHelper.ThrowIfNull(map);
            _map = map;
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)_map.Count;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>[] Items
        {
            get
            {
                var map = _map;
                var array = new SharedArrayMapKeyValuePair<TKey, TValue, TValueNative>[map.Count];
                var i = 0;

                foreach (var keyValue in map)
                {
                    array[i++] = keyValue;
                }

                return array;
            }
        }
    }
}
