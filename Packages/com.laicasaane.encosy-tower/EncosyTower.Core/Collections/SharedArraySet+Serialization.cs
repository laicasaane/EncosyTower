using System;
using EncosyTower.Collections.Extensions;
using UnityEngine;

namespace EncosyTower.Collections
{
    partial class SharedArraySet<T> : ISerializationCallbackReceiver
        where T : unmanaged, IEquatable<T>
    {
        [SerializeField] internal T[] _serializedItems = null;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_map == null)
            {
                _serializedItems = null;
                return;
            }

            var items = new T[_map.Count];
            _map.Values.Span.CopyTo(items);
            _serializedItems = items;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            var items = _serializedItems.EnsureNotNull();
            _serializedItems = null;

            Dispose();

            _map = new(items);

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var itemAdded = Add(in item);

                ThrowHelper.ThrowIfSerializedItemIsDuplicate(
                      itemAdded
                    , ThrowHelper.CollectionType.SharedArraySet
                    , i
                );
            }
        }
    }
}
