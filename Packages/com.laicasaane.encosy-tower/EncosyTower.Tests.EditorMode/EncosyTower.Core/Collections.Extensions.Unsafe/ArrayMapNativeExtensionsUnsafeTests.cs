using System;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections;

using MapAPI = EncosyTower.Collections.Extensions.Unsafe.ArrayMapNativeExtensionsUnsafe;
using MapReadOnlyAPI = EncosyTower.Collections.Extensions.Unsafe.ArrayMapNativeReadOnlyExtensionsUnsafe;

namespace EncosyTower.Tests.Core.Collections.Extensions.Unsafe
{
    public partial class ArrayMapNativeExtensionsUnsafeTests
    {
        [Test]
        public void MutableAndReadOnlyAccessors_AliasMapStorage()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            var keys = MapAPI.GetKeysUnsafe(in map);
            var values = MapAPI.GetValuesUnsafe(in map);
            ref var secondValue = ref MapAPI.GetValueAtUnsafe(in map, 1);
            var readOnly = map.AsReadOnly();
            var readOnlyKeys = MapReadOnlyAPI.GetKeysUnsafe(in readOnly);
            var readOnlyValues = MapReadOnlyAPI.GetValuesUnsafe(in readOnly);
            ref readonly var readOnlySecondValue = ref MapReadOnlyAPI.GetValueAtUnsafe(in readOnly, 1);

            Assert.AreEqual(1, keys[0].key);
            Assert.AreEqual(1, readOnlyKeys[0].key);
            Assert.AreEqual(10, readOnlyValues[0]);

            values[0] = 11;
            secondValue = 22;

            Assert.AreEqual(11, map[1]);
            Assert.AreEqual(22, map[2]);
            Assert.AreEqual(11, readOnlyValues[0]);
            Assert.AreEqual(22, readOnlySecondValue);

            keys[0].key = 99;

            Assert.AreEqual(99, readOnlyKeys[0].key);
        }

        [Test]
        public void Accessors_UncreatedMap_ThrowArgumentException()
        {
            var map = default(ArrayMapNative<int, int>);
            var readOnly = default(ArrayMapNative<int, int>.ReadOnly);

            Assert.Throws<ArgumentException>(() => MapAPI.GetKeysUnsafe(in map));
            Assert.Throws<ArgumentException>(() => MapAPI.GetValuesUnsafe(in map));
            Assert.Throws<ArgumentException>(() => _ = MapAPI.GetValueAtUnsafe(in map, 0));
            Assert.Throws<ArgumentException>(() => MapReadOnlyAPI.GetKeysUnsafe(in readOnly));
            Assert.Throws<ArgumentException>(() => MapReadOnlyAPI.GetValuesUnsafe(in readOnly));
            Assert.Throws<ArgumentException>(() => _ = MapReadOnlyAPI.GetValueAtUnsafe(in readOnly, 0));
        }

        [Test]
        public void Accessors_DisposedMap_ThrowArgumentException()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Persistent);
            map.Add(1, 10);
            map.Dispose();

            Assert.Throws<ArgumentException>(() => MapAPI.GetKeysUnsafe(in map));
            Assert.Throws<ArgumentException>(() => MapAPI.GetValuesUnsafe(in map));
            Assert.Throws<ArgumentException>(() => _ = MapAPI.GetValueAtUnsafe(in map, 0));
        }

        [Test]
        public void Accessors_EmptyMap_ReturnCreatedViews()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Persistent);

            try
            {
                var readOnly = map.AsReadOnly();

                Assert.IsTrue(MapAPI.GetKeysUnsafe(in map).IsCreated);
                Assert.IsTrue(MapAPI.GetValuesUnsafe(in map).IsCreated);
                Assert.IsTrue(MapReadOnlyAPI.GetKeysUnsafe(in readOnly).IsCreated);
                Assert.IsTrue(MapReadOnlyAPI.GetValuesUnsafe(in readOnly).IsCreated);
            }
            finally
            {
                map.Dispose();
            }
        }

        [Test]
        public void Accessors_ByteValue_RoundTripBitExact()
        {
            AssertValueRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Accessors_ThreeByteStructValue_RoundTripBitExact()
        {
            AssertValueRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Accessors_LongValue_RoundTripBitExact()
        {
            AssertValueRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Accessors_StructLargerThanSixteenBytesValue_RoundTripBitExact()
        {
            AssertValueRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = 0, b = 0, c = 0 }
                , new Wide { a = long.MinValue, b = 7, c = -1 }
            );
        }

        private static void AssertValueRoundTrip<TValue>(TValue a, TValue b, TValue c)
            where TValue : unmanaged
        {
            var name = typeof(TValue).Name;
            var map = new ArrayMapNative<int, TValue>(4, Allocator.Persistent);

            try
            {
                map.Add(1, a);
                map.Add(2, b);

                var values = MapAPI.GetValuesUnsafe(in map);
                var readOnly = map.AsReadOnly();

                Assert.AreEqual(a, values[0], name);
                Assert.AreEqual(b, values[1], name);
                Assert.AreEqual(b, MapReadOnlyAPI.GetValueAtUnsafe(in readOnly, 1), name);

                MapAPI.GetValueAtUnsafe(in map, 1) = c;

                Assert.AreEqual(c, map[2], name);
                Assert.AreEqual(c, values[1], name);
                Assert.AreEqual(c, MapReadOnlyAPI.GetValuesUnsafe(in readOnly)[1], name);
            }
            finally
            {
                map.Dispose();
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
