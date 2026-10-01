using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    public static class ArrayMapExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TValue> GetValues<TKey, TValue>([NotNull] this ArrayMap<TKey, TValue> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return self._values.AsSpan()[..self._freeValueCellIndex];
        }
    }
}
