using System;
using System.Runtime.CompilerServices;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    public static class ArraySetReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> GetItems<T>(this ArraySet<T>.ReadOnly self)
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return self._set._values.AsReadOnlySpan()[..self._set._freeValueCellIndex];
        }
    }
}
