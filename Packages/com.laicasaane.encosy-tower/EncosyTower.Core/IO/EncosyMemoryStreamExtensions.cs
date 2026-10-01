using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.IO
{
    public static class EncosyMemoryStreamExtensions
    {
        public static ReadOnlySpan<byte> AsSpan([NotNull] this MemoryStream stream)
        {
            DebuggingThrowHelper.ThrowIfNull(stream);
            if (stream.TryGetBuffer(out var buffer))
            {
                return buffer;
            }

            return Array.Empty<byte>();
        }
    }
}
