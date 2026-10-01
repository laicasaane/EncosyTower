using EncosyTower.Collections.Extensions;
using UnityEngine;

namespace EncosyTower.Collections
{
    partial class SharedList<T, TNative> : ISerializationCallbackReceiver
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
            _count = new(1);
            _version = new(1);

            // SAFETY: This list exclusively owns the newly allocated scalar count buffer.
            unsafe
            {
                _count[0] = items.Length;
            }
            InitializeNativeData();
        }
    }
}
