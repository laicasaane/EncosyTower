// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections;

using ListNativeReadOnlyUnsafeAPI = EncosyTower.Collections.Extensions.Unsafe.ListNativeReadOnlyExtensionsUnsafe;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class ListNativeReadOnlyExtensionsUnsafeTests
    {
        [Test]
        public void ReadOnlyAccessors_AliasListStorage()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var view = list.AsReadOnly();

            // SAFETY: The list remains alive for the block and the requested index is valid.
            unsafe
            {
                var buffer = ListNativeReadOnlyUnsafeAPI.GetBufferUnsafe(in view);
                ref readonly var second = ref ListNativeReadOnlyUnsafeAPI.GetItemAtUnsafe(in view, 1);

                Assert.AreEqual(1, buffer[0]);
                Assert.AreEqual(2, second);
            }
        }

        [Test]
        public void Accessors_UncreatedList_ThrowArgumentException()
        {
            var uncreated = default(ListNative<int>.ReadOnly);

            Assert.Throws<ArgumentException>(() => ListNativeReadOnlyUnsafeAPI.GetBufferUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => _ = ListNativeReadOnlyUnsafeAPI.GetItemAtUnsafe(in uncreated, 0));
        }

        [Test]
        public void GetBufferUnsafe_EmptyList_ReturnsACreatedBufferWithoutElements()
        {
            var list = new ListNative<int>(4, Allocator.Persistent);

            try
            {
                var view = list.AsReadOnly();
                var buffer = ListNativeReadOnlyUnsafeAPI.GetBufferUnsafe(in view);

                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(0, view.Count);
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void Accessors_ByteElement_RoundTripBitExact()
        {
            AssertElementRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Accessors_ThreeByteStructElement_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Accessors_LongElement_RoundTripBitExact()
        {
            AssertElementRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Accessors_StructLargerThanSixteenBytesElement_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = 0, b = 0, c = 0 }
                , new Wide { a = long.MinValue, b = 7, c = -1 }
            );
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var list = new ListNative<T>(new[] { a, b, c }, Allocator.Persistent);

            try
            {
                var view = list.AsReadOnly();
                var buffer = ListNativeReadOnlyUnsafeAPI.GetBufferUnsafe(in view);

                Assert.AreEqual(a, buffer[0], name);
                Assert.AreEqual(b, buffer[1], name);
                Assert.AreEqual(c, ListNativeReadOnlyUnsafeAPI.GetItemAtUnsafe(in view, 2), name);

                list[2] = a;

                Assert.AreEqual(a, ListNativeReadOnlyUnsafeAPI.GetItemAtUnsafe(in view, 2), name);
            }
            finally
            {
                list.Dispose();
            }
        }

        private struct Rgb
        {
            public byte r;
            public byte g;
            public byte b;
        }

        private struct Wide
        {
            public long a;
            public long b;
            public long c;
        }
    }
}
