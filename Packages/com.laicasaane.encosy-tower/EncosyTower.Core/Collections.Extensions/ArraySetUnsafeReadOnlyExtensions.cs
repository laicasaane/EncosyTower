using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class ArraySetUnsafeReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> GetItems<T>(this in ArraySetUnsafe<T>.ReadOnly self)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return self._values.AsReadOnlySpan()[..self._freeValueCellIndex];
        }
    }
}
