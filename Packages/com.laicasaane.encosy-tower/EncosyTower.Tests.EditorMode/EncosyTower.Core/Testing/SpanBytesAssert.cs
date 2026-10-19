using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Testing
{
    public static class SpanBytesAssert
    {
        public static void AreEqual<T>(T[] expected, T[] actual)
            where T : unmanaged
            => AssertBytes(new ReadOnlySpan<T>(expected), new ReadOnlySpan<T>(actual));

        public static void AreEqual<T>(T[] expected, ReadOnlySpan<T> actual)
            where T : unmanaged
            => AssertBytes(new ReadOnlySpan<T>(expected), actual);

        public static void AreEqual<T>(T[] expected, Span<T> actual)
            where T : unmanaged
            => AssertBytes(new ReadOnlySpan<T>(expected), actual);

        private static void AssertBytes<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual)
            where T : unmanaged
        {
            Assert.AreEqual(expected.Length, actual.Length);
            CollectionAssert.AreEqual(
                MemoryMarshal.AsBytes(expected).ToArray(),
                MemoryMarshal.AsBytes(actual).ToArray()
            );
        }
    }
}
