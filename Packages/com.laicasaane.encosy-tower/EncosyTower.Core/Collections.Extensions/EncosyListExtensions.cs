using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EncosyTower.Collections.Extensions
{
    public static partial class EncosyListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IList<T> list)
            => list == null || list.Count < 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IReadOnlyList<T> list)
            => list == null || list.Count < 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this List<T> list)
            => list == null ? default : CollectionsMarshal.AsSpan(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T> list)
            => list == null ? default : CollectionsMarshal.AsSpan(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListFast<T> AsListFast<T>(this List<T> list)
            => list;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListFast<T>.ReadOnly AsReadOnly<T>(this List<T> list)
            => list is not null ? list : ListFast<T>.ReadOnly.Empty;
    }
}

