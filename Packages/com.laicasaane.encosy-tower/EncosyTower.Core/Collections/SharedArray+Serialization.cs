using EncosyTower.Collections.Extensions;
using EncosyTower.Collections.Unsafe;
using UnityEngine;

namespace EncosyTower.Collections
{
    partial class SharedArray<T, TNative> : ISerializationCallbackReceiver
        where T : unmanaged
        where TNative : unmanaged
    {
        [SerializeField] internal T[] _serializedItems = null;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // SAFETY: Serialization copies the owner-backed span before this callback returns.
            unsafe
            {
                _serializedItems = _buffer.IsCreated ? AsReadOnlySpan().ToArray() : null;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());

            var items = _serializedItems.EnsureNotNull();
            _serializedItems = null;

            Dispose();
            Initialize(items);
        }
    }
}
