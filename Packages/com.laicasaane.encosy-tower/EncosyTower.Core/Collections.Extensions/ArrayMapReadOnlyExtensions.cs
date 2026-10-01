using System;
using System.Runtime.CompilerServices;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    public static class ArrayMapReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TValue> GetValues<TKey, TValue>(this ArrayMap<TKey, TValue>.ReadOnly self)
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return self._map._values.AsReadOnlySpan()[..self._map._freeValueCellIndex];
        }
    }
}
