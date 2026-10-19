// Adapted from Unity.Collections.Tests/UnsafeRingQueueTests.cs.

using System;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections;

using QueueNativeUnsafeAPI = EncosyTower.Collections.Extensions.Unsafe.QueueNativeExtensionsUnsafe;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class QueueNativeExtensionsUnsafeTests
    {
        [Test]
        public void RawAccessors_ExposeWrappedRingStorage()
        {
            using var queue = new QueueNative<int>(8, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2, 3, 4, 5, 6 });
            queue.Dequeue();
            queue.Dequeue();
            queue.EnqueueRange(new[] { 7, 8, 9 });
            var readOnly = queue.AsReadOnly();

            // SAFETY: The queue owns the ring buffer for the block and the raw head index is valid.
            unsafe
            {
                var buffer = QueueNativeUnsafeAPI.GetBufferUnsafe(in queue);
                var head = QueueNativeUnsafeAPI.GetHeadUnsafe(in queue);
                var tail = QueueNativeUnsafeAPI.GetTailUnsafe(in queue);
                var readOnlyBuffer = QueueNativeUnsafeAPI.GetBufferUnsafe(in readOnly);

                Assert.AreEqual(2, head);
                Assert.AreEqual(1, tail);
                Assert.AreEqual(head, QueueNativeUnsafeAPI.GetHeadUnsafe(in readOnly));
                Assert.AreEqual(tail, QueueNativeUnsafeAPI.GetTailUnsafe(in readOnly));
                Assert.AreEqual(3, readOnlyBuffer[head]);

                buffer[head] = 30;
            }

            Assert.AreEqual(30, queue.Peek());
        }

        [Test]
        public void Accessors_UncreatedOrDisposedQueue_ThrowArgumentException()
        {
            var uncreated = default(QueueNative<int>);
            var uncreatedReadOnly = default(QueueNative<int>.ReadOnly);
            var disposed = new QueueNative<int>(4, Allocator.Persistent);
            disposed.Dispose();

            Assert.Throws<ArgumentException>(() => QueueNativeUnsafeAPI.GetBufferUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => QueueNativeUnsafeAPI.GetHeadUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => QueueNativeUnsafeAPI.GetTailUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => QueueNativeUnsafeAPI.GetBufferUnsafe(in uncreatedReadOnly));
            Assert.Throws<ArgumentException>(() => QueueNativeUnsafeAPI.GetHeadUnsafe(in uncreatedReadOnly));
            Assert.Throws<ArgumentException>(() => QueueNativeUnsafeAPI.GetTailUnsafe(in uncreatedReadOnly));
            Assert.Throws<ArgumentException>(() => QueueNativeUnsafeAPI.GetBufferUnsafe(in disposed));
            Assert.Throws<ArgumentException>(() => QueueNativeUnsafeAPI.GetHeadUnsafe(in disposed));
            Assert.Throws<ArgumentException>(() => QueueNativeUnsafeAPI.GetTailUnsafe(in disposed));
        }

        [Test]
        public void RawPositions_EmptyQueue_AreZeroAndTheBufferIsCreated()
        {
            var queue = new QueueNative<int>(4, Allocator.Persistent);

            try
            {
                var readOnly = queue.AsReadOnly();

                Assert.AreEqual(0, QueueNativeUnsafeAPI.GetHeadUnsafe(in queue));
                Assert.AreEqual(0, QueueNativeUnsafeAPI.GetTailUnsafe(in queue));
                Assert.AreEqual(0, QueueNativeUnsafeAPI.GetHeadUnsafe(in readOnly));
                Assert.AreEqual(0, QueueNativeUnsafeAPI.GetTailUnsafe(in readOnly));
                Assert.IsTrue(QueueNativeUnsafeAPI.GetBufferUnsafe(in queue).IsCreated);
                Assert.IsTrue(QueueNativeUnsafeAPI.GetBufferUnsafe(in readOnly).IsCreated);
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        public void RawPositions_FullQueueThenDequeueAndEnqueue_WrapTheRing()
        {
            var queue = new QueueNative<int>(4, Allocator.Persistent);

            try
            {
                queue.EnqueueRange(new[] { 1, 2, 3, 4 });

                Assert.AreEqual(0, QueueNativeUnsafeAPI.GetHeadUnsafe(in queue));
                Assert.AreEqual(0, QueueNativeUnsafeAPI.GetTailUnsafe(in queue));

                Assert.AreEqual(1, queue.Dequeue());
                queue.Enqueue(5);

                var buffer = QueueNativeUnsafeAPI.GetBufferUnsafe(in queue);

                Assert.AreEqual(1, QueueNativeUnsafeAPI.GetHeadUnsafe(in queue));
                Assert.AreEqual(1, QueueNativeUnsafeAPI.GetTailUnsafe(in queue));
                Assert.AreEqual(5, buffer[0]);
                Assert.AreEqual(2, buffer[QueueNativeUnsafeAPI.GetHeadUnsafe(in queue)]);
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        public void GetBufferUnsafe_WriteThroughTheBuffer_IsSeenByTheQueueAndTheReadOnlyView()
        {
            var queue = new QueueNative<int>(4, Allocator.Persistent);

            try
            {
                queue.EnqueueRange(new[] { 1, 2 });

                var readOnly = queue.AsReadOnly();
                var buffer = QueueNativeUnsafeAPI.GetBufferUnsafe(in queue);
                var readOnlyBuffer = QueueNativeUnsafeAPI.GetBufferUnsafe(in readOnly);

                buffer[1] = 20;

                Assert.AreEqual(20, readOnlyBuffer[1]);
                Assert.AreEqual(1, queue.Dequeue());
                Assert.AreEqual(20, queue.Peek());
            }
            finally
            {
                queue.Dispose();
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
            var queue = new QueueNative<T>(3, Allocator.Persistent);

            try
            {
                queue.Enqueue(a);
                queue.Enqueue(b);
                queue.Enqueue(c);

                var buffer = QueueNativeUnsafeAPI.GetBufferUnsafe(in queue);
                var head = QueueNativeUnsafeAPI.GetHeadUnsafe(in queue);

                Assert.AreEqual(a, buffer[head], name);

                buffer[head] = c;

                Assert.AreEqual(c, queue.Peek(), name);
                Assert.AreEqual(c, queue.Dequeue(), name);

                queue.Enqueue(a);

                Assert.AreEqual(1, QueueNativeUnsafeAPI.GetHeadUnsafe(in queue), name);
                Assert.AreEqual(1, QueueNativeUnsafeAPI.GetTailUnsafe(in queue), name);
                Assert.AreEqual(a, buffer[0], name);
                CollectionAssert.AreEqual(new[] { b, c, a }, queue.ToArray(), name);
            }
            finally
            {
                queue.Dispose();
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
