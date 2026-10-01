using System;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using UnityEngine;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    partial class SharedArrayMap<TKey, TValue, TValueNative> : ISerializationCallbackReceiver
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {
        [SerializeField]
        internal SerializedMapEntry<TKey, TValue>[] _serializedEntries = null;

        internal SharedArrayMap(TValue[] valueBuffer)
        {
            DebuggingThrowHelper.ThrowIfNull(valueBuffer);

            var capacity = valueBuffer.Length;

            _valuesInfo = new(capacity);
            _values = new(valueBuffer);
            _buckets = new(HashHelpers.GetPrime(capacity));

            _fastModBucketsMultiplier = new(1);
            _collisions = new(1);
            _freeValueCellIndex = new(1);
            _version = new(1);

            if (capacity > 0)
            {
                // SAFETY: This constructor exclusively owns the initialized scalar buffer.
                unsafe
                {
                    _fastModBucketsMultiplier[0]
                        = HashHelpers.GetFastModMultiplier((uint)_buckets.Capacity);
                }
            }

            InitializeNativeData();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!_valuesInfo.IsCreated)
            {
                _serializedEntries = null;
                return;
            }

            var count = Count;
            var entries = new SerializedMapEntry<TKey, TValue>[count];
            // SAFETY: This map owns both buffers and copies their logical values synchronously.
            unsafe
            {
                var valuesInfo = _valuesInfo.AsReadOnlySpan();
                var values = _values.AsReadOnlySpan();

                for (var i = 0; i < count; i++)
                {
                    entries[i].key = valuesInfo[i].key;
                    entries[i].value = values[i];
                }
            }

            _serializedEntries = entries;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            var entries = _serializedEntries.EnsureNotNull();
            _serializedEntries = null;

            Dispose();
            InitializeRuntimeState(entries.Length);

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var itemAdded = TryAdd(entry.key, in entry.value);

                ThrowHelper.ThrowIfSerializedItemIsDuplicate(
                      itemAdded
                    , ThrowHelper.CollectionType.SharedArrayMap
                    , i
                );
            }
        }

        private void InitializeRuntimeState(int capacity)
        {
            _valuesInfo = new(capacity);
            _values = new(capacity);
            _buckets = new(HashHelpers.GetPrime(capacity));

            _fastModBucketsMultiplier = new(1);
            _collisions = new(1);
            _freeValueCellIndex = new(1);
            _version = new(1);

            if (capacity > 0)
            {
                // SAFETY: This initializer exclusively owns the rebuilt scalar buffer.
                unsafe
                {
                    _fastModBucketsMultiplier[0]
                        = HashHelpers.GetFastModMultiplier((uint)_buckets.Capacity);
                }
            }

            InitializeNativeData();
        }
    }
}
