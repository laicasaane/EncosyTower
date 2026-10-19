// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections;

using ListNativeUnsafeAPI = EncosyTower.Collections.Extensions.Unsafe.ListNativeExtensionsUnsafe;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class ListNativeExtensionsUnsafeTests
    {
        [Test]
        public void MutableAccessors_AliasListStorage()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            // SAFETY: The list owns the aliased buffer for the block and both indices are valid.
            unsafe
            {
                var buffer = ListNativeUnsafeAPI.GetBufferUnsafe(in list);
                ref var second = ref ListNativeUnsafeAPI.GetItemAtUnsafe(in list, 1);
                buffer[0] = 10;
                second = 20;
            }

            CollectionAssert.AreEqual(new[] { 10, 20, 3 }, list.ToArray());
        }

        [Test]
        public void Accessors_UncreatedOrDisposedList_ThrowArgumentException()
        {
            var uncreated = default(ListNative<int>);
            var disposed = new ListNative<int>(new[] { 1, 2 }, Allocator.Persistent);
            disposed.Dispose();

            Assert.Throws<ArgumentException>(() => ListNativeUnsafeAPI.GetBufferUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => _ = ListNativeUnsafeAPI.GetItemAtUnsafe(in uncreated, 0));
            Assert.Throws<ArgumentException>(() => ListNativeUnsafeAPI.GetBufferUnsafe(in disposed));
            Assert.Throws<ArgumentException>(() => _ = ListNativeUnsafeAPI.GetItemAtUnsafe(in disposed, 0));
        }

        [Test]
        public void GetBufferUnsafe_EmptyList_ReturnsACreatedBufferWithoutElements()
        {
            var list = new ListNative<int>(4, Allocator.Persistent);

            try
            {
                var buffer = ListNativeUnsafeAPI.GetBufferUnsafe(in list);

                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(0, list.Count);
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
                var buffer = ListNativeUnsafeAPI.GetBufferUnsafe(in list);

                Assert.AreEqual(a, buffer[0], name);
                Assert.AreEqual(b, buffer[1], name);
                Assert.AreEqual(c, ListNativeUnsafeAPI.GetItemAtUnsafe(in list, 2), name);

                ListNativeUnsafeAPI.GetItemAtUnsafe(in list, 2) = a;
                buffer[0] = c;

                CollectionAssert.AreEqual(new[] { c, b, a }, list.ToArray(), name);
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
