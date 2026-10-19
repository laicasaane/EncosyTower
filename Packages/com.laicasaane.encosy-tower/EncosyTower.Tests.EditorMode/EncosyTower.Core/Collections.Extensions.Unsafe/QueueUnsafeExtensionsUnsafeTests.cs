using System;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

using QueueUnsafeAPI = EncosyTower.Collections.Extensions.Unsafe.QueueUnsafeExtensionsUnsafe;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class QueueUnsafeExtensionsUnsafeTests
    {
        [Test]
        public void OwnerAndReadOnlyAccessors_ExposeWrappedRingStorage()
        {
            using var queue = new QueueUnsafe<int>(8, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2, 3, 4, 5, 6 });
            Assert.AreEqual(1, queue.Dequeue());
            Assert.AreEqual(2, queue.Dequeue());
            queue.EnqueueRange(new[] { 7, 8, 9 });
            var readOnly = queue.AsReadOnly();

            // SAFETY: The queue owns the ring buffer and the reported head stays valid for the block.
            unsafe
            {
                var buffer = QueueUnsafeAPI.GetBufferUnsafe(in queue);
                var head = QueueUnsafeAPI.GetHeadUnsafe(in queue);
                var tail = QueueUnsafeAPI.GetTailUnsafe(in queue);
                var readOnlyBuffer = QueueUnsafeAPI.GetBufferUnsafe(in readOnly);
                var readOnlyHead = QueueUnsafeAPI.GetHeadUnsafe(in readOnly);
                var readOnlyTail = QueueUnsafeAPI.GetTailUnsafe(in readOnly);

                Assert.AreEqual(2, head);
                Assert.AreEqual(1, tail);
                Assert.AreEqual(head, readOnlyHead);
                Assert.AreEqual(tail, readOnlyTail);
                Assert.AreEqual(3, readOnlyBuffer[head]);

                buffer[head] = 30;

                Assert.AreEqual(30, readOnlyBuffer[head]);
            }

            Assert.AreEqual(30, queue.Peek());
        }

        [Test]
        public void Accessors_UncreatedQueue_ThrowArgumentException()
        {
            var uncreated = default(QueueUnsafe<int>);
            var uncreatedReadOnly = default(QueueUnsafe<int>.ReadOnly);

            Assert.Throws<ArgumentException>(() => QueueUnsafeAPI.GetBufferUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => QueueUnsafeAPI.GetHeadUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => QueueUnsafeAPI.GetTailUnsafe(in uncreated));
            Assert.Throws<ArgumentException>(() => QueueUnsafeAPI.GetBufferUnsafe(in uncreatedReadOnly));
            Assert.Throws<ArgumentException>(() => QueueUnsafeAPI.GetHeadUnsafe(in uncreatedReadOnly));
            Assert.Throws<ArgumentException>(() => QueueUnsafeAPI.GetTailUnsafe(in uncreatedReadOnly));
        }

        [Test]
        public void RawPositions_EmptyQueue_AreZeroAndTheBufferIsCreated()
        {
            using var queue = new QueueUnsafe<int>(4, Allocator.Temp);
            var readOnly = queue.AsReadOnly();

            Assert.AreEqual(0, QueueUnsafeAPI.GetHeadUnsafe(in queue));
            Assert.AreEqual(0, QueueUnsafeAPI.GetTailUnsafe(in queue));
            Assert.AreEqual(0, QueueUnsafeAPI.GetHeadUnsafe(in readOnly));
            Assert.AreEqual(0, QueueUnsafeAPI.GetTailUnsafe(in readOnly));
            Assert.IsTrue(QueueUnsafeAPI.GetBufferUnsafe(in queue).IsCreated);
            Assert.IsTrue(QueueUnsafeAPI.GetBufferUnsafe(in readOnly).IsCreated);
        }

        [Test]
        public void RawPositions_FullQueueThenDequeueAndEnqueue_WrapTheRing()
        {
            using var queue = new QueueUnsafe<int>(4, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2, 3, 4 });

            Assert.AreEqual(0, QueueUnsafeAPI.GetHeadUnsafe(in queue));
            Assert.AreEqual(0, QueueUnsafeAPI.GetTailUnsafe(in queue));
            Assert.AreEqual(1, queue.Dequeue());

            queue.Enqueue(5);

            var buffer = QueueUnsafeAPI.GetBufferUnsafe(in queue);

            Assert.AreEqual(1, QueueUnsafeAPI.GetHeadUnsafe(in queue));
            Assert.AreEqual(1, QueueUnsafeAPI.GetTailUnsafe(in queue));
            Assert.AreEqual(5, buffer[0]);
            Assert.AreEqual(2, buffer[QueueUnsafeAPI.GetHeadUnsafe(in queue)]);
        }

        [Test]
        public void GetBufferUnsafe_WriteThroughTheBuffer_IsSeenByTheQueueAndTheReadOnlyView()
        {
            using var queue = new QueueUnsafe<int>(4, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2 });

            var readOnly = queue.AsReadOnly();
            var buffer = QueueUnsafeAPI.GetBufferUnsafe(in queue);
            var readOnlyBuffer = QueueUnsafeAPI.GetBufferUnsafe(in readOnly);

            buffer[1] = 20;

            Assert.AreEqual(20, readOnlyBuffer[1]);
            Assert.AreEqual(1, queue.Dequeue());
            Assert.AreEqual(20, queue.Peek());
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
            var queue = new QueueUnsafe<T>(3, Allocator.Temp);

            try
            {
                queue.Enqueue(a);
                queue.Enqueue(b);
                queue.Enqueue(c);

                var buffer = QueueUnsafeAPI.GetBufferUnsafe(in queue);
                var head = QueueUnsafeAPI.GetHeadUnsafe(in queue);

                Assert.AreEqual(a, buffer[head], name);

                buffer[head] = c;

                Assert.AreEqual(c, queue.Peek(), name);
                Assert.AreEqual(c, queue.Dequeue(), name);

                queue.Enqueue(a);

                Assert.AreEqual(1, QueueUnsafeAPI.GetHeadUnsafe(in queue), name);
                Assert.AreEqual(1, QueueUnsafeAPI.GetTailUnsafe(in queue), name);
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
