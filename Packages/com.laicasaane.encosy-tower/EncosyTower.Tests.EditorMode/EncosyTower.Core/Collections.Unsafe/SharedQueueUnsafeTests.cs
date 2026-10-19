using System;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public partial class SharedQueueUnsafeTests
    {
        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Alloc_AcceptedAllocator_CreatesAndFreesTheHeader(Allocator allocator)
        {
            using var storage = new SharedQueueStorage<int>(4);

            // SAFETY: The header borrows storage, which outlives it; Free runs exactly once in finally.
            unsafe
            {
                var data = SharedQueueUnsafe<int>.Alloc(
                      storage.buffer
                    , storage.capacity
                    , storage.head
                    , storage.tail
                    , storage.count
                    , storage.version
                    , allocator
                );

                try
                {
                    data->Enqueue(7);

                    Assert.AreEqual(4, data->Capacity);
                    Assert.AreEqual(1, data->Count);
                    Assert.AreEqual(1, storage.Count);
                    Assert.AreEqual(7, data->Peek());
                    Assert.AreEqual(7, data->Dequeue());
                    Assert.AreEqual(0, storage.Count);
                }
                finally
                {
                    SharedQueueUnsafe<int>.Free(data, allocator);
                }
            }
        }

        [Test]
        public void Alloc_ZeroCapacity_CreatesAHeaderOfAnEmptyFixedQueue()
        {
            using var storage = new SharedQueueStorage<int>(0);

            // SAFETY: The header borrows storage, which outlives it; Free runs exactly once in finally.
            unsafe
            {
                var data = SharedQueueUnsafe<int>.Alloc(
                      storage.buffer
                    , storage.capacity
                    , storage.head
                    , storage.tail
                    , storage.count
                    , storage.version
                    , Allocator.Persistent
                );

                try
                {
                    Assert.AreEqual(0, data->Capacity);
                    Assert.AreEqual(0, data->Count);
                    Assert.AreEqual(0, data->ToArray().Length);
                    Assert.IsFalse(data->TryDequeue(out _));
                    Assert.IsFalse(data->TryPeek(out _));
                    Assert.IsTrue(data->TryCopyTo(Span<int>.Empty));
                }
                finally
                {
                    SharedQueueUnsafe<int>.Free(data, Allocator.Persistent);
                }
            }
        }

        [Test]
        public void Free_NullPointer_DoesNothingForEveryStrategy()
        {
            // SAFETY: Free documents that a null header is ignored, so no memory is read or released.
            unsafe
            {
                SharedQueueUnsafe<int>.Free(null, default(AllocatorStrategy));
                SharedQueueUnsafe<int>.Free(null, Allocator.Temp);
                SharedQueueUnsafe<int>.Free(null, Allocator.Persistent);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ZeroCapacityQueue_EnqueueAndDequeue_Throw()
        {
            using var storage = new SharedQueueStorage<int>(0);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => header.Enqueue(1));
                Assert.Throws<InvalidOperationException>(() => header.Dequeue());
                Assert.Throws<InvalidOperationException>(() => header.Peek());
                Assert.AreEqual(0, storage.Count);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Enqueue_ExactlyFullThenOnePast_ThrowsAndKeepsContentAndVersion()
        {
            using var storage = new SharedQueueStorage<int>(3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Enqueue(1);
                header.Enqueue(2);
                header.Enqueue(in storage.buffer[0]);

                Assert.AreEqual(3, header.Count);
                Assert.AreEqual(3, storage.Version);
                Assert.Throws<InvalidOperationException>(() => header.Enqueue(4));
                Assert.Throws<InvalidOperationException>(() =>
                {
                    var item = 5;
                    header.Enqueue(in item);
                });
                Assert.AreEqual(3, storage.Count);
                Assert.AreEqual(3, storage.Version);
                CollectionAssert.AreEqual(new[] { 1, 2, 1 }, header.ToArray());
            }
        }

        [Test]
        public void Enqueue_AfterDequeueFromAFullQueue_WrapsTheRing()
        {
            using var storage = new SharedQueueStorage<int>(3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Enqueue(1);
                header.Enqueue(2);
                header.Enqueue(3);

                Assert.AreEqual(1, header.Dequeue());

                header.Enqueue(4);

                Assert.AreEqual(1, storage.Tail);
                Assert.AreEqual(1, storage.Head);
                Assert.AreEqual(3, storage.Count);
                CollectionAssert.AreEqual(new[] { 2, 3, 4 }, header.ToArray());
            }
        }

        [Test]
        public void SingleSlotQueue_RepeatedEnqueueAndDequeue_KeepsHeadAndTailInRange()
        {
            using var storage = new SharedQueueStorage<int>(1);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                for (var i = 0; i < 5; i++)
                {
                    header.Enqueue(i);

                    Assert.AreEqual(i, header.Peek());
                    Assert.AreEqual(i, header.Dequeue());
                    Assert.AreEqual(0, storage.Head);
                    Assert.AreEqual(0, storage.Tail);
                    Assert.AreEqual(0, storage.Count);
                }
            }
        }

        [Test]
        public void DequeueAndPeek_FilledQueue_ReturnFifoOrderAndBumpTheVersionOnDequeue()
        {
            using var storage = new SharedQueueStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Enqueue(1);
                header.Enqueue(2);

                var versionAfterEnqueues = storage.Version;

                Assert.AreEqual(1, header.Peek());
                Assert.IsTrue(header.TryPeek(out var peeked));
                Assert.AreEqual(1, peeked);
                Assert.AreEqual(versionAfterEnqueues, storage.Version);
                Assert.AreEqual(1, header.Dequeue());
                Assert.AreEqual(versionAfterEnqueues + 1, storage.Version);
                Assert.IsTrue(header.TryDequeue(out var dequeued));
                Assert.AreEqual(2, dequeued);
                Assert.AreEqual(0, header.Count);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void DequeueAndPeek_EmptyQueue_Throw()
        {
            using var storage = new SharedQueueStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => header.Dequeue());
                Assert.Throws<InvalidOperationException>(() => header.Peek());
                Assert.IsFalse(header.TryDequeue(out var dequeued));
                Assert.IsFalse(header.TryPeek(out var peeked));
                Assert.AreEqual(0, dequeued);
                Assert.AreEqual(0, peeked);
            }
        }

        [Test]
        public void Clear_WrappedQueue_ResetsHeadTailAndCountAndStaysUsable()
        {
            using var storage = new SharedQueueStorage<int>(3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Clear();

                Assert.AreEqual(0, header.Count);

                header.Enqueue(1);
                header.Enqueue(2);
                header.Dequeue();
                header.Enqueue(3);
                header.Enqueue(4);
                header.Clear();

                Assert.AreEqual(0, storage.Head);
                Assert.AreEqual(0, storage.Tail);
                Assert.AreEqual(0, storage.Count);

                header.Enqueue(5);

                CollectionAssert.AreEqual(new[] { 5 }, header.ToArray());
            }
        }

        [Test]
        public void CopyTo_WrappedQueue_CopiesEveryWindowInFifoOrder()
        {
            using var storage = new SharedQueueStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Enqueue(1);
                header.Enqueue(2);
                header.Enqueue(3);
                header.Enqueue(4);
                header.Dequeue();
                header.Dequeue();
                header.Enqueue(5);
                header.Enqueue(6);

                var all = new int[4];
                var fromOne = new int[3];
                var window = new int[2];

                header.CopyTo(all);
                header.CopyTo(1, fromOne);
                header.CopyTo(2, window);

                CollectionAssert.AreEqual(new[] { 3, 4, 5, 6 }, all);
                CollectionAssert.AreEqual(new[] { 4, 5, 6 }, fromOne);
                CollectionAssert.AreEqual(new[] { 5, 6 }, window);
                Assert.IsTrue(header.TryCopyTo(1, window, 2));
                CollectionAssert.AreEqual(new[] { 4, 5 }, window);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void CopyTo_InvalidRange_Throws()
        {
            using var storage = new SharedQueueStorage<int>(4);
            var header = storage.CreateHeader();
            var destination = new[] { 9, 9, 9, 9 };

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Enqueue(1);
                header.Enqueue(2);
                header.Enqueue(3);
                header.Enqueue(4);

                Assert.Throws<InvalidOperationException>(() => header.CopyTo(-1, destination));
                Assert.Throws<InvalidOperationException>(() => header.CopyTo(5, destination));
                Assert.Throws<InvalidOperationException>(() => header.CopyTo(1, destination, 4));
                Assert.Throws<InvalidOperationException>(() => header.CopyTo(0, destination, -1));
                Assert.Throws<InvalidOperationException>(() => header.CopyTo(0, new int[2], 3));
                CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
            }
        }

        [Test]
        public void TryCopyTo_InvalidRange_ReturnsFalseAndKeepsDestination()
        {
            using var storage = new SharedQueueStorage<int>(4);
            var header = storage.CreateHeader();
            var destination = new[] { 9, 9, 9, 9 };

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Enqueue(1);
                header.Enqueue(2);
                header.Enqueue(3);
                header.Enqueue(4);

                Assert.IsFalse(header.TryCopyTo(-1, destination));
                Assert.IsFalse(header.TryCopyTo(5, destination));
                Assert.IsFalse(header.TryCopyTo(1, destination, 4));
                Assert.IsFalse(header.TryCopyTo(0, destination, -1));
                Assert.IsFalse(header.TryCopyTo(0, new int[2], 3));
                CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
            }
        }

        [Test]
        public void CopyTo_EmptyQueueOrEmptyDestination_CopiesNothing()
        {
            using var storage = new SharedQueueStorage<int>(4);
            using var empty = new SharedQueueStorage<int>(4);
            var header = storage.CreateHeader();
            var emptyHeader = empty.CreateHeader();

            // SAFETY: both storages own the blocks their headers borrow and outlive every call below.
            unsafe
            {
                header.Enqueue(1);
                header.Enqueue(2);

                header.CopyTo(Span<int>.Empty);
                header.CopyTo(2, Span<int>.Empty);
                emptyHeader.CopyTo(Span<int>.Empty);

                Assert.IsTrue(header.TryCopyTo(Span<int>.Empty));
                Assert.IsTrue(header.TryCopyTo(2, Span<int>.Empty));
                Assert.IsTrue(emptyHeader.TryCopyTo(Span<int>.Empty));
                Assert.IsFalse(emptyHeader.TryCopyTo(new int[1]));
            }
        }

        [Test]
        public void Elements_ByteElement_RoundTripBitExact()
        {
            AssertElementRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Elements_ThreeByteStruct_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Elements_LongElement_RoundTripBitExact()
        {
            AssertElementRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Elements_StructLargerThanSixteenBytes_RoundTripBitExact()
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
            using var storage = new SharedQueueStorage<T>(3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Enqueue(a);
                header.Enqueue(in b);
                header.Enqueue(c);

                Assert.AreEqual(a, header.Dequeue(), name);

                header.Enqueue(a);

                CollectionAssert.AreEqual(new[] { b, c, a }, header.ToArray(), name);
                Assert.AreEqual(b, header.Peek(), name);

                var copy = new T[3];
                header.CopyTo(copy);

                CollectionAssert.AreEqual(new[] { b, c, a }, copy, name);
                Assert.IsTrue(header.TryDequeue(out var first), name);
                Assert.AreEqual(b, first, name);
                Assert.AreEqual(c, header.Dequeue(), name);
                Assert.AreEqual(a, header.Dequeue(), name);
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
