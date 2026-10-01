using System;

namespace EncosyTower.Collections
{
    [Serializable]
    internal struct SerializedMapEntry<TKey, TValue>
    {
        public TKey key;
        public TValue value;
    }
}
