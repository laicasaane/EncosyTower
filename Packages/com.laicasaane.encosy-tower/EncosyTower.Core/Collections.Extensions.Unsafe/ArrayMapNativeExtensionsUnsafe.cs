#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;

namespace EncosyTower.Collections.Extensions.Unsafe
{
    public static class ArrayMapNativeExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBuffer<ArrayMapNode<TKey>> GetKeysUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue> self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => self._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBuffer<TValue> GetValuesUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue> self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => self._values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue GetValueAtUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue> self, int index)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => ref self._values[index];
    }

    public static class ArrayMapNativeReadOnlyExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBuffer<ArrayMapNode<TKey>>.ReadOnly GetKeysUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue>.ReadOnly self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => self._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBuffer<TValue>.ReadOnly GetValuesUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue>.ReadOnly self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => self._values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly TValue GetValueAtUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue>.ReadOnly self, int index)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => ref self._values[index];
    }
}

#endif
