using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.Collections
{
    partial class ArrayMap<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        internal SerializedMapEntry<TKey, TValue>[] _serializedEntries = null;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_valuesInfo.IsCreated == false)
            {
                _serializedEntries = null;
                return;
            }

            var count = Count;
            var entries = new SerializedMapEntry<TKey, TValue>[count];

            for (var i = 0; i < count; i++)
            {
                entries[i].key = _valuesInfo[i].key;
                entries[i].value = _values[i];
            }

            _serializedEntries = entries;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            var entries = _serializedEntries.EnsureNotNull();
            _serializedEntries = null;

            ReinitializeRuntimeState(entries.Length);

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];

                if (entry.key is null)
                {
                    continue;
                }

                var itemAdded = TryAdd(entry.key, in entry.value);

                ThrowHelper.ThrowIfSerializedItemIsDuplicate(
                      itemAdded
                    , ThrowHelper.CollectionType.ArrayMap
                    , i
                );
            }
        }

        private void ReinitializeRuntimeState(int capacity)
        {
            _version++;

            _valuesInfo = default;
            _valuesInfo.Alloc(capacity);
            _values = default;
            _values.Alloc(capacity);
            _buckets = default;
            _buckets.Alloc(HashHelpers.GetPrime(capacity));

            _fastModBucketsMultiplier = default;
            _collisions = default;
            _freeValueCellIndex = default;

            if (capacity > 0)
            {
                _fastModBucketsMultiplier = HashHelpers.GetFastModMultiplier((uint)_buckets.Capacity);
            }
        }
    }
}
