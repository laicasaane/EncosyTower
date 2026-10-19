using System;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

using ListUnsafeReadOnlyAPI = EncosyTower.Collections.Extensions.Unsafe.ListUnsafeReadOnlyExtensionsUnsafe;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class ListUnsafeReadOnlyExtensionsUnsafeTests
    {
        [Test]
        public void ReadOnlyAccessors_AliasListStorage()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var readOnly = list.AsReadOnly();

            // SAFETY: The owner remains alive and index one is inside the borrowed read-only storage.
            unsafe
            {
                var buffer = ListUnsafeReadOnlyAPI.GetBufferUnsafe(in readOnly);
                ref readonly var second = ref ListUnsafeReadOnlyAPI.GetItemAtUnsafe(in readOnly, 1);

                Assert.AreEqual(1, buffer[0]);
                Assert.AreEqual(2, second);

                list[0] = 10;
                list[1] = 20;

                Assert.AreEqual(10, buffer[0]);
                Assert.AreEqual(20, second);
            }
        }

        [Test]
        public void Accessors_UncreatedList_ThrowArgumentException()
        {
            var uncreated = default(ListUnsafe<int>.ReadOnly);

            Assert.Throws<ArgumentException>(() => ListUnsafeReadOnlyAPI.GetBufferUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => _ = ListUnsafeReadOnlyAPI.GetItemAtUnsafe(in uncreated, 0));
        }

        [Test]
        public void GetBufferUnsafe_EmptyList_ReturnsACreatedBufferWithoutElements()
        {
            using var list = new ListUnsafe<int>(4, Allocator.Temp);
            var readOnly = list.AsReadOnly();

            var buffer = ListUnsafeReadOnlyAPI.GetBufferUnsafe(in readOnly);

            Assert.IsTrue(buffer.IsCreated);
            Assert.AreEqual(4, buffer.Capacity);
            Assert.AreEqual(0, readOnly.Count);
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
                var readOnly = list.AsReadOnly();
                var buffer = ListUnsafeReadOnlyAPI.GetBufferUnsafe(in readOnly);

                Assert.AreEqual(a, buffer[0], name);
                Assert.AreEqual(b, buffer[1], name);
                Assert.AreEqual(c, ListUnsafeReadOnlyAPI.GetItemAtUnsafe(in readOnly, 2), name);

                list[2] = a;

                Assert.AreEqual(a, ListUnsafeReadOnlyAPI.GetItemAtUnsafe(in readOnly, 2), name);
                Assert.AreEqual(a, buffer[2], name);
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
