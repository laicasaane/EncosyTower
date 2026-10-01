using EncosyTower.Collections.Extensions;
using UnityEngine;

namespace EncosyTower.Collections
{
    partial class SharedQueue<T, TNative> : ISerializationCallbackReceiver
        where T : unmanaged
        where TNative : unmanaged
    {
        [SerializeField] internal T[] _serializedItems = null;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _serializedItems = _buffer.IsCreated ? ToArray() : null;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            var items = _serializedItems.EnsureNotNull();
            _serializedItems = null;

            Dispose();

            _buffer = new(items);
            _head = new(1);
            _tail = new(1);
            _count = new(1);
            _version = new(1);
            // SAFETY: This owner keeps its rebuilt scalar buffers pinned while initializing queue state.
            unsafe
            {
                _count[0] = items.Length;
                _tail[0] = items.Length == _buffer.Capacity ? 0 : items.Length;
            }
            InitializeNativeData();
        }
    }
}
