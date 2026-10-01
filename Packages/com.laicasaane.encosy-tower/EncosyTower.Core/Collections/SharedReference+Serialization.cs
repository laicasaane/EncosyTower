using EncosyTower.Collections.Unsafe;
using UnityEngine;

namespace EncosyTower.Collections
{
    partial class SharedReference<T, TNative> : ISerializationCallbackReceiver
        where T : unmanaged
        where TNative : unmanaged
    {
        [SerializeField] internal T _serializedValue;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // SAFETY: This owner remains alive and unchanged for the immediate value copy.
            unsafe
            {
                _serializedValue = _buffer.IsCreated == false
                    ? default
                    : ValueRO;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());

            // SAFETY: Deserialization owns the current runtime state and stops all aliases before rebuilding it.
            unsafe
            {
                Dispose();
            }

            Initialize(_serializedValue);
        }
    }
}
