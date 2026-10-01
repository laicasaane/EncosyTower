using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    public static class ArraySetExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> GetItems<T>([NotNull] this ArraySet<T> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return self._values.AsSpan()[..self._freeValueCellIndex];
        }
    }
}
