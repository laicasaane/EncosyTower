using System;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

using ListUnsafeAPI = EncosyTower.Collections.Extensions.Unsafe.ListUnsafeExtensionsUnsafe;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class ListUnsafeExtensionsUnsafeTests
    {
        [Test]
        public void MutableAccessors_AliasListStorage()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            // SAFETY: The list remains alive and both raw indices are inside its allocated storage.
            unsafe
            {
                var buffer = ListUnsafeAPI.GetBufferUnsafe(in list);
                ref var second = ref ListUnsafeAPI.GetItemAtUnsafe(in list, 1);
                buffer[0] = 10;
                second = 20;
            }

            CollectionAssert.AreEqual(new[] { 10, 20, 3 }, list.ToArray());
        }

        [Test]
        public void Accessors_UncreatedList_ThrowArgumentException()
        {
            var uncreated = default(ListUnsafe<int>);

            Assert.Throws<ArgumentException>(() => ListUnsafeAPI.GetBufferUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => _ = ListUnsafeAPI.GetItemAtUnsafe(in uncreated, 0));
        }

        [Test]
        public void GetBufferUnsafe_EmptyList_ReturnsACreatedBufferWithoutElements()
        {
            using var list = new ListUnsafe<int>(4, Allocator.Temp);

            var buffer = ListUnsafeAPI.GetBufferUnsafe(in list);

            Assert.IsTrue(buffer.IsCreated);
            Assert.AreEqual(4, buffer.Capacity);
            Assert.AreEqual(0, list.Count);
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
            var list = new ListUnsafe<T>(new[] { a, b, c }, Allocator.Temp);

            try
            {
                var buffer = ListUnsafeAPI.GetBufferUnsafe(in list);

                Assert.AreEqual(a, buffer[0], name);
                Assert.AreEqual(b, buffer[1], name);
                Assert.AreEqual(c, ListUnsafeAPI.GetItemAtUnsafe(in list, 2), name);

                ListUnsafeAPI.GetItemAtUnsafe(in list, 2) = a;
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
