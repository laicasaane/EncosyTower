using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Extensions
{
    public static partial class EncosyArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] self)
            => self.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNull]
        public static T[] EnsureNotNull<T>(this T[] self)
            => self ?? Array.Empty<T>();
    }
}
