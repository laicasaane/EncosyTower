using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class ArrayMapUnsafeReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TValue> GetValues<TKey, TValue>(this in ArrayMapUnsafe<TKey, TValue>.ReadOnly self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return self.Values;
        }
    }
}
