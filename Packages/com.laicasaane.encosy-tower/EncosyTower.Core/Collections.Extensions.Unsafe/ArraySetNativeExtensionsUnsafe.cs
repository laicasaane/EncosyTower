#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;

namespace EncosyTower.Collections.Extensions.Unsafe
{
    public static class ArraySetNativeExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBuffer<ArrayMapNode<T>> GetNodesUnsafe<T>(this in ArraySetNative<T> self)
            where T : unmanaged, IEquatable<T>
            => self._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBuffer<T> GetItemsUnsafe<T>(this in ArraySetNative<T> self)
            where T : unmanaged, IEquatable<T>
            => self._values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetItemAtUnsafe<T>(this in ArraySetNative<T> self, int index)
            where T : unmanaged, IEquatable<T>
            => ref self._values[index];
    }

    public static class ArraySetNativeReadOnlyExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBuffer<ArrayMapNode<T>>.ReadOnly GetNodesUnsafe<T>(this in ArraySetNative<T>.ReadOnly self)
            where T : unmanaged, IEquatable<T>
            => self._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBuffer<T>.ReadOnly GetItemsUnsafe<T>(this in ArraySetNative<T>.ReadOnly self)
            where T : unmanaged, IEquatable<T>
            => self._values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T GetItemAtUnsafe<T>(this in ArraySetNative<T>.ReadOnly self, int index)
            where T : unmanaged, IEquatable<T>
            => ref self._values[index];
    }
}

#endif
