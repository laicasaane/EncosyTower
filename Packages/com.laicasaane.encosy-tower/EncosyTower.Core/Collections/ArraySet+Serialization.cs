using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.Collections
{
    partial class ArraySet<T> : ISerializationCallbackReceiver
    {
        [SerializeField] internal T[] _serializedItems = null;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_valuesInfo.IsCreated == false)
            {
                _serializedItems = null;
                return;
            }

            var count = Count;
            var items = new T[count];

            for (var i = 0; i < count; i++)
            {
                items[i] = _values[i];
            }

            _serializedItems = items;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            var items = _serializedItems.EnsureNotNull();
            _serializedItems = null;

            ReinitializeRuntimeState(items);

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];

                if (item is null)
                {
                    continue;
                }

                var itemAdded = Add(in item);

                ThrowHelper.ThrowIfSerializedItemIsDuplicate(
                      itemAdded
                    , ThrowHelper.CollectionType.ArraySet
                    , i
                );
            }
        }

        private void ReinitializeRuntimeState(T[] items)
        {
            var capacity = items.Length;

            _version++;

            _valuesInfo = new(capacity);
            _values = new(items);
            _buckets = new(HashHelpers.GetPrime(capacity));

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
