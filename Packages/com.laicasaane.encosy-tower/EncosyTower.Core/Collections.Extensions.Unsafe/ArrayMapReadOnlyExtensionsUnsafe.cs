using System.Runtime.CompilerServices;
using EncosyTower.Buffers;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Unsafe
{
    public static class ArrayMapReadOnlyExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BufferManaged<ArrayMapNode<TKey>>.ReadOnly GetKeysUnsafe<TKey, TValue>(
            this in ArrayMap<TKey, TValue>.ReadOnly self
        )
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return self._map._valuesInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BufferManaged<TValue>.ReadOnly GetValuesUnsafe<TKey, TValue>(
            this in ArrayMap<TKey, TValue>.ReadOnly self
        )
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return self._map._values;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly TValue GetValueAtUnsafe<TKey, TValue>(
              this in ArrayMap<TKey, TValue>.ReadOnly self
            , int index
        )
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return ref self._map._values[index];
        }
    }
}
