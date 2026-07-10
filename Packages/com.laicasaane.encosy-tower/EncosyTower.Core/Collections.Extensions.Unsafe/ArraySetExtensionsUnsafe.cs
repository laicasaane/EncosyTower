using System.Runtime.CompilerServices;
using EncosyTower.Buffers;

namespace EncosyTower.Collections.Unsafe
{
    public static class ArraySetExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedBuffer<ArrayMapNode<T>> GetNodesUnsafe<T>(this ArraySet<T> self)
            => self._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedBuffer<T> GetItemsUnsafe<T>(this ArraySet<T> self)
            => self._values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetItemAtUnsafe<T>(this ArraySet<T> self, int index)
            => ref self._values[index];
    }

    public static class ArraySetReadOnlyExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedBuffer<ArrayMapNode<T>>.ReadOnly GetNodesUnsafe<T>(this in ArraySet<T>.ReadOnly self)
            => self._set._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedBuffer<T>.ReadOnly GetItemsUnsafe<T>(this in ArraySet<T>.ReadOnly self)
            => self._set._values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T GetItemAtUnsafe<T>(this in ArraySet<T>.ReadOnly self, int index)
            => ref self._set._values[index];
    }
}
