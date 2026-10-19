// Adapted from System.Collections.Generic.Stack<T> storage semantics.

using System;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections;

using StackNativeUnsafeAPI = EncosyTower.Collections.Extensions.Unsafe.StackNativeExtensionsUnsafe;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class StackNativeExtensionsUnsafeTests
    {
        [Test]
        public void RawAccessors_AliasStackStorage()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var readOnly = stack.AsReadOnly();

            // SAFETY: The stack owns the aliased buffer for the block and both indices are valid.
            unsafe
            {
                var buffer = StackNativeUnsafeAPI.GetBufferUnsafe(in stack);
                ref var second = ref StackNativeUnsafeAPI.GetItemAtUnsafe(in stack, 1);
                var readOnlyBuffer = StackNativeUnsafeAPI.GetBufferUnsafe(in readOnly);
                ref readonly var readOnlySecond = ref StackNativeUnsafeAPI.GetItemAtUnsafe(in readOnly, 1);

                buffer[0] = 10;
                second = 20;

                Assert.AreEqual(10, readOnlyBuffer[0]);
                Assert.AreEqual(20, readOnlySecond);
            }

            CollectionAssert.AreEqual(new[] { 3, 20, 10 }, stack.ToArray());
        }

        [Test]
        public void Accessors_UncreatedOrDisposedStack_ThrowArgumentException()
        {
            var uncreated = default(StackNative<int>);
            var uncreatedReadOnly = default(StackNative<int>.ReadOnly);
            var disposed = new StackNative<int>(new[] { 1, 2 }, Allocator.Persistent);
            disposed.Dispose();

            Assert.Throws<ArgumentException>(() => StackNativeUnsafeAPI.GetBufferUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => _ = StackNativeUnsafeAPI.GetItemAtUnsafe(in uncreated, 0));
            Assert.Throws<ArgumentException>(() => StackNativeUnsafeAPI.GetBufferUnsafe(in uncreatedReadOnly));
            Assert.Throws<ArgumentException>(() => _ = StackNativeUnsafeAPI.GetItemAtUnsafe(in uncreatedReadOnly, 0));
            Assert.Throws<ArgumentException>(() => StackNativeUnsafeAPI.GetBufferUnsafe(in disposed));
            Assert.Throws<ArgumentException>(() => _ = StackNativeUnsafeAPI.GetItemAtUnsafe(in disposed, 0));
        }

        [Test]
        public void GetBufferUnsafe_EmptyStack_ReturnsACreatedBufferWithoutElements()
        {
            var stack = new StackNative<int>(4, Allocator.Persistent);

            try
            {
                var readOnly = stack.AsReadOnly();

                Assert.IsTrue(StackNativeUnsafeAPI.GetBufferUnsafe(in stack).IsCreated);
                Assert.IsTrue(StackNativeUnsafeAPI.GetBufferUnsafe(in readOnly).IsCreated);
                Assert.AreEqual(0, stack.Count);
            }
            finally
            {
                stack.Dispose();
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
            var stack = new StackNative<T>(new[] { a, b, c }, Allocator.Persistent);

            try
            {
                var readOnly = stack.AsReadOnly();
                var buffer = StackNativeUnsafeAPI.GetBufferUnsafe(in stack);

                Assert.AreEqual(a, buffer[0], name);
                Assert.AreEqual(b, StackNativeUnsafeAPI.GetItemAtUnsafe(in stack, 1), name);
                Assert.AreEqual(c, StackNativeUnsafeAPI.GetItemAtUnsafe(in readOnly, 2), name);

                StackNativeUnsafeAPI.GetItemAtUnsafe(in stack, 2) = a;

                Assert.AreEqual(a, stack.Peek(), name);
                Assert.AreEqual(a, buffer[2], name);
            }
            finally
            {
                stack.Dispose();
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
