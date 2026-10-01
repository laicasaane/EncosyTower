using System;
using System.Runtime.CompilerServices;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class ArrayMapNativeReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TValue> GetValues<TKey, TValue>(this in ArrayMapNative<TKey, TValue>.ReadOnly self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return self.AsValuesReadOnlySpan();
        }
    }
}
