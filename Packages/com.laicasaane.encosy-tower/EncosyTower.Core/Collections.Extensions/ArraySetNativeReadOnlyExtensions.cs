using System;
using System.Runtime.CompilerServices;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class ArraySetNativeReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> GetItems<T>(this in ArraySetNative<T>.ReadOnly self)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return self.AsValuesReadOnlySpan();
        }
    }
}
