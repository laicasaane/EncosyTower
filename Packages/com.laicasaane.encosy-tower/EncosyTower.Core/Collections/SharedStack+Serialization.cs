using System;
using EncosyTower.Collections.Extensions;
using UnityEngine;

namespace EncosyTower.Collections
{
    partial class SharedStack<T, TNative> : ISerializationCallbackReceiver
        where T : unmanaged
        where TNative : unmanaged
    {
        [SerializeField] internal T[] _serializedItems = null;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!_buffer.IsCreated)
            {
                _serializedItems = null;
                return;
            }

            var items = ToArray();
            Array.Reverse(items);
            _serializedItems = items;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            var items = _serializedItems.EnsureNotNull();
            _serializedItems = null;

            Dispose();

            _buffer = new(items);
            _count = new(1);
            _version = new(1);
            // SAFETY: This owner keeps its rebuilt count buffer pinned while initializing stack state.
            unsafe
            {
                _count[0] = items.Length;
            }
            InitializeNativeData();
        }
    }
}
