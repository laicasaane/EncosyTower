using System;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections;

using SetAPI = EncosyTower.Collections.Extensions.Unsafe.ArraySetNativeExtensionsUnsafe;
using SetReadOnlyAPI = EncosyTower.Collections.Extensions.Unsafe.ArraySetNativeReadOnlyExtensionsUnsafe;

namespace EncosyTower.Tests.Core.Collections.Extensions.Unsafe
{
    public partial class ArraySetNativeExtensionsUnsafeTests
    {
        [Test]
        public void MutableAndReadOnlyAccessors_AliasSetStorage()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            set.Add(10);
            set.Add(20);

            var nodes = SetAPI.GetNodesUnsafe(in set);
            var items = SetAPI.GetItemsUnsafe(in set);
            ref var secondItem = ref SetAPI.GetItemAtUnsafe(in set, 1);
            var readOnly = set.AsReadOnly();
            var readOnlyNodes = SetReadOnlyAPI.GetNodesUnsafe(in readOnly);
            var readOnlyItems = SetReadOnlyAPI.GetItemsUnsafe(in readOnly);
            ref readonly var readOnlySecondItem = ref SetReadOnlyAPI.GetItemAtUnsafe(in readOnly, 1);

            Assert.AreEqual(10, nodes[0].key);
            Assert.AreEqual(10, readOnlyNodes[0].key);
            Assert.AreEqual(10, readOnlyItems[0]);

            items[0] = 11;
            secondItem = 22;
            nodes[0].key = 99;

            Assert.AreEqual(11, set.Items[0]);
            Assert.AreEqual(22, set.Items[1]);
            Assert.AreEqual(11, readOnlyItems[0]);
            Assert.AreEqual(22, readOnlySecondItem);
            Assert.AreEqual(99, readOnlyNodes[0].key);
        }

        [Test]
        public void Accessors_UncreatedSet_ThrowArgumentException()
        {
            var set = default(ArraySetNative<int>);
            var readOnly = default(ArraySetNative<int>.ReadOnly);

            Assert.Throws<ArgumentException>(() => SetAPI.GetNodesUnsafe(in set));
            Assert.Throws<ArgumentException>(() => SetAPI.GetItemsUnsafe(in set));
            Assert.Throws<ArgumentException>(() => _ = SetAPI.GetItemAtUnsafe(in set, 0));
            Assert.Throws<ArgumentException>(() => SetReadOnlyAPI.GetNodesUnsafe(in readOnly));
            Assert.Throws<ArgumentException>(() => SetReadOnlyAPI.GetItemsUnsafe(in readOnly));
            Assert.Throws<ArgumentException>(() => _ = SetReadOnlyAPI.GetItemAtUnsafe(in readOnly, 0));
        }

        [Test]
        public void Accessors_DisposedSet_ThrowArgumentException()
        {
            var set = new ArraySetNative<int>(4, Allocator.Persistent);
            set.Add(1);
            set.Dispose();

            Assert.Throws<ArgumentException>(() => SetAPI.GetNodesUnsafe(in set));
            Assert.Throws<ArgumentException>(() => SetAPI.GetItemsUnsafe(in set));
            Assert.Throws<ArgumentException>(() => _ = SetAPI.GetItemAtUnsafe(in set, 0));
        }

        [Test]
        public void Accessors_EmptySet_ReturnCreatedViews()
        {
            var set = new ArraySetNative<int>(4, Allocator.Persistent);

            try
            {
                var readOnly = set.AsReadOnly();

                Assert.IsTrue(SetAPI.GetNodesUnsafe(in set).IsCreated);
                Assert.IsTrue(SetAPI.GetItemsUnsafe(in set).IsCreated);
                Assert.IsTrue(SetReadOnlyAPI.GetNodesUnsafe(in readOnly).IsCreated);
                Assert.IsTrue(SetReadOnlyAPI.GetItemsUnsafe(in readOnly).IsCreated);
            }
            finally
            {
                set.Dispose();
            }
        }

        [Test]
        public void Accessors_ByteItem_RoundTripBitExact()
        {
            AssertItemRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Accessors_ThreeByteStructItem_RoundTripBitExact()
        {
            AssertItemRoundTrip(
                  new RgbItem { r = 1, g = 2, b = 3 }
                , new RgbItem { r = 0, g = 0, b = 0 }
                , new RgbItem { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Accessors_LongItem_RoundTripBitExact()
        {
            AssertItemRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Accessors_StructLargerThanSixteenBytesItem_RoundTripBitExact()
        {
            AssertItemRoundTrip(
                  new WideItem { a = 1, b = -2, c = long.MaxValue }
                , new WideItem { a = 0, b = 0, c = 0 }
                , new WideItem { a = long.MinValue, b = 7, c = -1 }
            );
        }

        private static void AssertItemRoundTrip<T>(T a, T b, T c)
            where T : unmanaged, IEquatable<T>
        {
            var name = typeof(T).Name;
            var set = new ArraySetNative<T>(4, Allocator.Persistent);

            try
            {
                set.Add(a);
                set.Add(b);

                var items = SetAPI.GetItemsUnsafe(in set);
                var readOnly = set.AsReadOnly();

                Assert.AreEqual(a, items[0], name);
                Assert.AreEqual(b, items[1], name);
                Assert.AreEqual(b, SetReadOnlyAPI.GetItemAtUnsafe(in readOnly, 1), name);

                SetAPI.GetItemAtUnsafe(in set, 1) = c;

                Assert.AreEqual(c, items[1], name);
                Assert.AreEqual(c, SetReadOnlyAPI.GetItemsUnsafe(in readOnly)[1], name);
                Assert.AreEqual(a, SetAPI.GetNodesUnsafe(in set)[0].key, name);
            }
            finally
            {
                set.Dispose();
            }
        }

        private struct RgbItem : IEquatable<RgbItem>
        {
            public byte r;
            public byte g;
            public byte b;

            public bool Equals(RgbItem other)
                => r == other.r && g == other.g && b == other.b;

            public override bool Equals(object obj)
                => obj is RgbItem other && Equals(other);

            public override int GetHashCode()
                => r | (g << 8) | (b << 16);
        }

        private struct WideItem : IEquatable<WideItem>
        {
            public long a;
            public long b;
            public long c;

            public bool Equals(WideItem other)
                => a == other.a && b == other.b && c == other.c;

            public override bool Equals(object obj)
                => obj is WideItem other && Equals(other);

            public override int GetHashCode()
                => HashCode.Combine(a, b, c);
        }
    }
}
