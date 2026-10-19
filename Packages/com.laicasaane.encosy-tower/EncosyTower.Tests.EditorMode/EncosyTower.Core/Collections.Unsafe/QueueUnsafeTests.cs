using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public partial class QueueUnsafeTests
    {
        [Test]
        public void Constructors_CreateEmptyAndCopiedQueuesIncludingZeroCapacity()
        {
            using var queue = new QueueUnsafe<int>(4, new AllocatorStrategy(Allocator.Temp));
            using var zeroCapacity = new QueueUnsafe<int>(0, Allocator.Temp);
            using var copied = new QueueUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.IsTrue(queue.IsCreated);
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(4, queue.Capacity);
            Assert.IsTrue(zeroCapacity.IsCreated);
            Assert.AreEqual(0, zeroCapacity.Count);
            Assert.AreEqual(0, zeroCapacity.Capacity);
            Assert.IsTrue(copied.IsCreated);
            Assert.AreEqual(3, copied.Count);
            Assert.AreEqual(3, copied.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, copied.ToArray());

            zeroCapacity.Enqueue(7);

            Assert.AreEqual(1, zeroCapacity.Count);
            Assert.AreEqual(7, zeroCapacity.Peek());
        }

        [Test]
        public void Enqueue_OverloadsGrowAndPreserveFifoOrder()
        {
            using var queue = new QueueUnsafe<int>(1, Allocator.Temp);
            var second = 2;

            queue.Enqueue(1);
            queue.Enqueue(in second);

            for (var i = 3; i <= 20; i++)
            {
                queue.Enqueue(i);
            }

            Assert.AreEqual(20, queue.Count);
            Assert.GreaterOrEqual(queue.Capacity, 20);

            for (var i = 1; i <= 20; i++)
            {
                Assert.AreEqual(i, queue.Dequeue());
            }

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void EnqueueRange_WrappedGrowthPreservesFifoOrder()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4, 5, 6 }, Allocator.Temp);

            Assert.AreEqual(1, queue.Dequeue());
            Assert.AreEqual(2, queue.Dequeue());
            Assert.AreEqual(3, queue.Dequeue());
            queue.EnqueueRange(new[] { 7, 8, 9 });
            queue.EnqueueRange(ReadOnlySpan<int>.Empty);
            queue.EnqueueRange(new[] { 10, 11, 12 });

            CollectionAssert.AreEqual(new[] { 4, 5, 6, 7, 8, 9, 10, 11, 12 }, queue.ToArray());
            Assert.GreaterOrEqual(queue.Capacity, queue.Count);
        }

        [Test]
        public void DequeuePeekAndTryMethods_HandleSuccessAndFailure()
        {
            using var queue = new QueueUnsafe<int>(2, Allocator.Temp);

            Assert.IsFalse(queue.TryDequeue(out var emptyDequeue));
            Assert.IsFalse(queue.TryPeek(out var emptyPeek));
            Assert.AreEqual(0, emptyDequeue);
            Assert.AreEqual(0, emptyPeek);

            queue.EnqueueRange(new[] { 10, 20 });

            Assert.AreEqual(10, queue.Peek());
            Assert.IsTrue(queue.TryPeek(out var peeked));
            Assert.AreEqual(10, peeked);
            Assert.AreEqual(2, queue.Count);
            Assert.IsTrue(queue.TryDequeue(out var dequeued));
            Assert.AreEqual(10, dequeued);
            Assert.AreEqual(20, queue.Dequeue());
            Assert.IsFalse(queue.TryDequeue(out emptyDequeue));
            Assert.AreEqual(0, emptyDequeue);
        }

        [Test]
        public void Clear_ResetsRingAndAllowsReuse()
        {
            using var queue = new QueueUnsafe<int>(4, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2, 3 });
            Assert.AreEqual(1, queue.Dequeue());

            queue.Clear();

            Assert.AreEqual(0, queue.Count);
            Assert.IsFalse(queue.TryPeek(out _));

            queue.EnqueueRange(new[] { 4, 5 });

            CollectionAssert.AreEqual(new[] { 4, 5 }, queue.ToArray());
        }

        [Test]
        public void CopyAndTryCopy_AllOverloadsLinearizeWrappedContent()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4, 5, 6 }, Allocator.Temp);
            Assert.AreEqual(1, queue.Dequeue());
            Assert.AreEqual(2, queue.Dequeue());
            queue.Enqueue(7);
            queue.Enqueue(8);

            var full = new int[6];
            var partial = new int[3];
            var offset = new int[2];
            var range = new int[3];

            queue.CopyTo(full);
            queue.CopyTo(partial, 2);
            queue.CopyTo(2, offset);
            queue.CopyTo(1, range, 2);

            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6, 7, 8 }, queue.ToArray());
            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6, 7, 8 }, full);
            CollectionAssert.AreEqual(new[] { 3, 4, 0 }, partial);
            CollectionAssert.AreEqual(new[] { 5, 6 }, offset);
            CollectionAssert.AreEqual(new[] { 4, 5, 0 }, range);
            Assert.IsTrue(queue.TryCopyTo(new int[6]));
            Assert.IsTrue(queue.TryCopyTo(new int[3], 2));
            Assert.IsTrue(queue.TryCopyTo(2, new int[3]));
            Assert.IsTrue(queue.TryCopyTo(1, new int[3], 2));

            var failedDestination = new[] { 9, 9 };
            Assert.IsFalse(queue.TryCopyTo(5, failedDestination));
            Assert.IsFalse(queue.TryCopyTo(5, failedDestination, 2));
            CollectionAssert.AreEqual(new[] { 9, 9 }, failedDestination);
        }

        [Test]
        public void CapacityMethodsGrowShrinkAndTrimWithoutChangingOrder()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            queue.EnsureCapacity(10);
            Assert.GreaterOrEqual(queue.Capacity, 10);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());

            var afterEnsure = queue.Capacity;
            queue.IncreaseCapacityBy(5);
            Assert.AreEqual(afterEnsure + 5, queue.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());

            queue.IncreaseCapacityTo(queue.Count);
            Assert.AreEqual(queue.Count, queue.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());

            queue.IncreaseCapacityTo(8);
            queue.Trim();

            Assert.AreEqual(queue.Count, queue.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());
        }

        [Test]
        public void ReadOnly_AllPropertiesPeekCopyAndConversionMembersWork()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4, 5, 6 }, Allocator.Temp);
            Assert.AreEqual(1, queue.Dequeue());
            Assert.AreEqual(2, queue.Dequeue());
            queue.Enqueue(7);
            queue.Enqueue(8);
            var readOnly = queue.AsReadOnly();
            QueueUnsafe<int>.ReadOnly converted = queue;
            var full = new int[6];
            var partial = new int[3];
            var offset = new int[2];
            var range = new int[3];

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(6, readOnly.Count);
            Assert.AreEqual(6, readOnly.Capacity);
            Assert.AreEqual(3, readOnly.Peek());
            Assert.IsTrue(readOnly.TryPeek(out var peeked));
            Assert.AreEqual(3, peeked);
            Assert.AreEqual(3, converted.Peek());

            readOnly.CopyTo(full);
            readOnly.CopyTo(partial, 2);
            readOnly.CopyTo(2, offset);
            readOnly.CopyTo(1, range, 2);

            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6, 7, 8 }, readOnly.ToArray());
            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6, 7, 8 }, full);
            CollectionAssert.AreEqual(new[] { 3, 4, 0 }, partial);
            CollectionAssert.AreEqual(new[] { 5, 6 }, offset);
            CollectionAssert.AreEqual(new[] { 4, 5, 0 }, range);
            Assert.IsTrue(readOnly.TryCopyTo(new int[6]));
            Assert.IsTrue(readOnly.TryCopyTo(new int[3], 2));
            Assert.IsTrue(readOnly.TryCopyTo(2, new int[3]));
            Assert.IsTrue(readOnly.TryCopyTo(1, new int[3], 2));
            Assert.IsFalse(readOnly.TryCopyTo(5, new int[2]));
            Assert.IsFalse(readOnly.TryCopyTo(5, new int[2], 2));
        }

        [Test]
        public void ReadOnlyEmpty_TryPeekReturnsFalse()
        {
            using var queue = new QueueUnsafe<int>(0, Allocator.Temp);
            var readOnly = queue.AsReadOnly();

            Assert.IsFalse(readOnly.TryPeek(out var value));
            Assert.AreEqual(0, value);
        }

        [Test]
        public void OwnerAndReadOnlyEnumerators_DirectMembersVisitFifoOrderAndReset()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var ownerEnumerator = queue.GetEnumerator();
            var readOnlyEnumerator = queue.AsReadOnly().GetEnumerator();

            Assert.IsTrue(ownerEnumerator.MoveNext());
            Assert.AreEqual(1, ownerEnumerator.Current);
            Assert.IsTrue(ownerEnumerator.MoveNext());
            Assert.AreEqual(2, ownerEnumerator.Current);
            ownerEnumerator.Reset();
            Assert.IsTrue(ownerEnumerator.MoveNext());
            Assert.AreEqual(1, ownerEnumerator.Current);
            ownerEnumerator.Dispose();

            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(1, readOnlyEnumerator.Current);
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(2, readOnlyEnumerator.Current);
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(3, readOnlyEnumerator.Current);
            Assert.IsFalse(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(0, readOnlyEnumerator.Current);
            readOnlyEnumerator.Reset();
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(1, readOnlyEnumerator.Current);
            readOnlyEnumerator.Dispose();
        }

        [Test]
        public void Enumerator_DoesNotInvalidateOnMutationWithinCapacity()
        {
            using var queue = new QueueUnsafe<int>(8, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2, 3 });
            var enumerator = queue.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);

            queue.Enqueue(4);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
            enumerator.Dispose();
        }

        [Test]
        public void Dispose_MarksNotCreatedAndDoubleDisposeIsNoOp()
        {
            var queue = new QueueUnsafe<int>(4, Allocator.Temp);
            queue.Enqueue(1);

            queue.Dispose();

            Assert.IsFalse(queue.IsCreated);
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(0, queue.Capacity);
            Assert.DoesNotThrow(() => queue.Dispose());
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var queue = new QueueUnsafe<int>(4, Allocator.TempJob);
            queue.Enqueue(1);

            var handle = queue.Dispose(default);

            Assert.IsFalse(queue.IsCreated);
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(0, queue.Capacity);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Validation_RejectsInvalidCapacityCopiesAndEmptyAccess()
        {
            Assert.Throws<InvalidOperationException>(
                static () => _ = new QueueUnsafe<int>(-1, Allocator.Temp)
            );

            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => queue.EnsureCapacity(-1));
            Assert.Throws<InvalidOperationException>(() => queue.IncreaseCapacityTo(2));
            Assert.Throws<InvalidOperationException>(() => queue.CopyTo(4, new int[1]));

            queue.Clear();
            var readOnly = queue.AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
            Assert.Throws<InvalidOperationException>(() => queue.Peek());
            Assert.Throws<InvalidOperationException>(() => readOnly.Peek());
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAndDisposes(Allocator allocator)
        {
            var queue = new QueueUnsafe<int>(4, allocator);

            try
            {
                queue.Enqueue(1);

                Assert.IsTrue(queue.IsCreated);
                Assert.AreEqual(1, queue.Count);
            }
            finally
            {
                queue.Dispose();
            }

            Assert.IsFalse(queue.IsCreated);
            Assert.AreEqual(0, queue.Count);
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_JobHandle_AcceptedAllocator_ReleasesTheStorage(Allocator allocator)
        {
            var queue = new QueueUnsafe<int>(new[] { 1, 2 }, allocator);

            var handle = queue.Dispose(default);

            Assert.IsFalse(queue.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void Dispose_DefaultQueue_IsNoOp()
        {
            var queue = default(QueueUnsafe<int>);

            Assert.DoesNotThrow(() => queue.Dispose());
            Assert.DoesNotThrow(() => queue.Dispose(default).Complete());
            Assert.IsFalse(queue.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_DefaultAllocatorStrategy_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new QueueUnsafe<int>(4, default(AllocatorStrategy)));
            Assert.Throws<InvalidOperationException>(
                () => _ = new QueueUnsafe<int>(new[] { 1 }, default(AllocatorStrategy))
            );
        }

        [Test]
        public void SpanConstructor_EmptySource_CreatesAnEmptyQueueThatCanGrow()
        {
            using var queue = new QueueUnsafe<int>(ReadOnlySpan<int>.Empty, Allocator.Temp);

            Assert.IsTrue(queue.IsCreated);
            Assert.AreEqual(0, queue.Count);

            queue.Enqueue(5);

            Assert.AreEqual(5, queue.Peek());
        }

        [Test]
        public void UncreatedQueue_Members_ReportAnEmptyQueue()
        {
            var queue = default(QueueUnsafe<int>);

            Assert.IsFalse(queue.IsCreated);
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(0, queue.Capacity);
            Assert.IsFalse(queue.TryDequeue(out var dequeued));
            Assert.IsFalse(queue.TryPeek(out var peeked));
            Assert.AreEqual(0, dequeued);
            Assert.AreEqual(0, peeked);
            Assert.AreEqual(0, queue.ToArray().Length);
            Assert.DoesNotThrow(() => queue.Trim());
            Assert.DoesNotThrow(() => queue.Clear());
            Assert.IsTrue(queue.TryCopyTo(Span<int>.Empty));
            Assert.IsFalse(queue.TryCopyTo(new int[1]));
        }

        [Test]
        public void AsReadOnly_UncreatedQueue_ThrowsArgumentException()
        {
            var queue = default(QueueUnsafe<int>);

            Assert.Throws<ArgumentException>(() => queue.AsReadOnly());
            Assert.Throws<ArgumentException>(() => _ = (QueueUnsafe<int>.ReadOnly)queue);
            Assert.Throws<ArgumentException>(() => queue.GetEnumerator());
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

        [Test]
        public void Enqueue_FullWrappedQueue_GrowsAndKeepsFifoOrder()
        {
            var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            try
            {
                Assert.AreEqual(1, queue.Dequeue());
                Assert.AreEqual(2, queue.Dequeue());

                queue.Enqueue(5);
                queue.Enqueue(6);

                Assert.AreEqual(queue.Count, queue.Capacity);

                queue.Enqueue(7);

                CollectionAssert.AreEqual(new[] { 3, 4, 5, 6, 7 }, queue.ToArray());
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        public void Enqueue_InArgumentAliasingOwnStorageOnFullQueue_AppendsACopy()
        {
            var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                queue.Enqueue(in queue._buffer[0]);

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 1 }, queue.ToArray());
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        public void EnqueueRange_SpanOverOwnStorageOnFullQueue_AppendsACopy()
        {
            var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                queue.EnqueueRange(queue._buffer.AsReadOnlySpan());

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 1, 2, 3, 4 }, queue.ToArray());
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        public void EnqueueRange_ExactlyFillsTheRingThenGrows_KeepsFifoOrder()
        {
            var queue = new QueueUnsafe<int>(3, Allocator.Temp);

            try
            {
                queue.EnqueueRange(new[] { 1, 2, 3 });

                Assert.AreEqual(queue.Count, queue.Capacity);
                Assert.AreEqual(1, queue.Dequeue());

                queue.EnqueueRange(new[] { 4, 5 });

                CollectionAssert.AreEqual(new[] { 2, 3, 4, 5 }, queue.ToArray());
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        public void EnqueueRange_ZeroCapacityQueue_AddsEveryItem()
        {
            using var queue = new QueueUnsafe<int>(0, Allocator.Temp);

            queue.EnqueueRange(new[] { 1, 2, 3 });

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());
        }

        [Test]
        public void TryDequeue_LastElement_LeavesAnEmptyQueueThatCanBeRefilled()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1 }, Allocator.Temp);

            Assert.IsTrue(queue.TryDequeue(out var value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(0, queue.Count);
            Assert.IsFalse(queue.TryDequeue(out _));

            queue.Enqueue(2);

            CollectionAssert.AreEqual(new[] { 2 }, queue.ToArray());
        }

        [Test]
        public void Trim_EmptyQueue_ReleasesTheCapacityAndStaysUsable()
        {
            using var queue = new QueueUnsafe<int>(8, Allocator.Temp);

            queue.Trim();

            Assert.IsTrue(queue.IsCreated);
            Assert.AreEqual(0, queue.Capacity);

            queue.Enqueue(3);

            CollectionAssert.AreEqual(new[] { 3 }, queue.ToArray());
        }

        [Test]
        public void Trim_FullQueue_KeepsContent()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            queue.Trim();

            Assert.AreEqual(3, queue.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());
        }

        [Test]
        public void Trim_WrappedQueue_LinearizesAndKeepsFifoOrder()
        {
            var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4, 5, 6 }, Allocator.Temp);

            try
            {
                for (var i = 0; i < 4; i++)
                {
                    queue.Dequeue();
                }

                queue.Enqueue(7);
                queue.Enqueue(8);

                CollectionAssert.AreEqual(new[] { 5, 6, 7, 8 }, queue.ToArray());

                queue.Trim();

                Assert.AreEqual(4, queue.Capacity);
                CollectionAssert.AreEqual(new[] { 5, 6, 7, 8 }, queue.ToArray());
                Assert.AreEqual(5, queue.Peek());
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void CopyTo_InvalidRange_Throws()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<InvalidOperationException>(() => queue.CopyTo(-1, destination));
            Assert.Throws<InvalidOperationException>(() => queue.CopyTo(5, destination));
            Assert.Throws<InvalidOperationException>(() => queue.CopyTo(1, destination, 4));
            Assert.Throws<InvalidOperationException>(() => queue.CopyTo(0, destination, -1));
            Assert.Throws<InvalidOperationException>(() => queue.CopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void TryCopyTo_InvalidRange_ReturnsFalseAndKeepsDestination()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var destination = new[] { 9, 9, 9, 9 };

            Assert.IsFalse(queue.TryCopyTo(-1, destination));
            Assert.IsFalse(queue.TryCopyTo(5, destination));
            Assert.IsFalse(queue.TryCopyTo(1, destination, 4));
            Assert.IsFalse(queue.TryCopyTo(0, destination, -1));
            Assert.IsFalse(queue.TryCopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void CopyTo_EmptyQueueOrEmptyDestination_CopiesNothing()
        {
            using var empty = new QueueUnsafe<int>(4, Allocator.Temp);
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            empty.CopyTo(Span<int>.Empty);
            queue.CopyTo(Span<int>.Empty);
            queue.CopyTo(3, Span<int>.Empty);

            Assert.IsTrue(empty.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(queue.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(queue.TryCopyTo(3, Span<int>.Empty));
            Assert.IsFalse(empty.TryCopyTo(new int[1]));
        }

        [Test]
        public void CopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5 };
            var queue = new QueueUnsafe<int>(8, Allocator.Temp);

            try
            {
                queue.EnqueueRange(seed);
                queue.CopyTo(queue._buffer.AsSpan()[1..6]);

                var expected = new int[8];
                Array.Copy(seed, expected, seed.Length);
                Array.Copy(expected, 0, expected, 1, 5);

                var actual = queue._buffer.AsReadOnlySpan()[..6].ToArray();

                CollectionAssert.AreEqual(expected.AsSpan(0, 6).ToArray(), actual);
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        public void ReadOnly_TryCopyTo_InvalidRange_ReturnsFalseAndKeepsDestination()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = queue.AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.IsFalse(readOnly.TryCopyTo(-1, destination));
            Assert.IsFalse(readOnly.TryCopyTo(5, destination));
            Assert.IsFalse(readOnly.TryCopyTo(1, destination, 4));
            Assert.IsFalse(readOnly.TryCopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnly_CopyTo_InvalidRange_Throws()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = queue.AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(-1, destination));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(5, destination));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(1, destination, 4));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void AsReadOnly_OwnerWriteThroughTheSharedBuffer_IsSeenByTheView()
        {
            var queue = new QueueUnsafe<int>(8, Allocator.Temp);

            try
            {
                queue.EnqueueRange(new[] { 1, 2 });

                var readOnly = queue.AsReadOnly();

                queue._buffer[0] = 10;

                Assert.AreEqual(10, readOnly.Peek());
                Assert.IsTrue(readOnly.TryPeek(out var peeked));
                Assert.AreEqual(10, peeked);
                CollectionAssert.AreEqual(new[] { 10, 2 }, readOnly.ToArray());
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IncreaseCapacityBy_NegativeAmountBelowCount_Throws()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => queue.IncreaseCapacityBy(-5));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, queue.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_ZeroAmount_KeepsCapacityAndContent()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.AreEqual(4, queue.IncreaseCapacityBy(0));
            Assert.AreEqual(4, queue.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, queue.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_AmountOverflowingInt_ThrowsOverflow()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<OverflowException>(() => queue.IncreaseCapacityBy(int.MaxValue));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, queue.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_WrappedQueue_KeepsFifoOrder()
        {
            var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            try
            {
                queue.Dequeue();
                queue.Dequeue();
                queue.Enqueue(5);
                queue.Enqueue(6);

                queue.IncreaseCapacityBy(4);

                Assert.AreEqual(8, queue.Capacity);
                CollectionAssert.AreEqual(new[] { 3, 4, 5, 6 }, queue.ToArray());
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        public void GetEnumerator_EmptyQueue_YieldsNothingForEveryEnumeratorForm()
        {
            using var queue = new QueueUnsafe<int>(4, Allocator.Temp);

            var structEnumerator = queue.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)queue).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)queue).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [Test]
        public void ReadOnly_Enumerator_EmptyQueue_YieldsNothingForEveryEnumeratorForm()
        {
            using var queue = new QueueUnsafe<int>(4, Allocator.Temp);
            var readOnly = queue.AsReadOnly();

            var structEnumerator = readOnly.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)readOnly).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)readOnly).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [Test]
        public void Enumerator_WrappedQueue_VisitsFifoOrder()
        {
            var queue = new QueueUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            try
            {
                queue.Dequeue();
                queue.Dequeue();
                queue.Enqueue(5);

                var visited = new List<int>();

                foreach (var item in queue)
                {
                    visited.Add(item);
                }

                CollectionAssert.AreEqual(new[] { 3, 4, 5 }, visited);
            }
            finally
            {
                queue.Dispose();
            }
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Alloc_AcceptedAllocator_CreatesAndFreesTheHeader(Allocator allocator)
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = QueueUnsafe<int>.Alloc(4, allocator);

                try
                {
                    data->Enqueue(1);

                    Assert.IsTrue(data->IsCreated);
                    Assert.AreEqual(1, data->Count);
                    Assert.AreEqual(1, data->Peek());
                }
                finally
                {
                    QueueUnsafe<int>.Free(data);
                }
            }
        }

        [Test]
        public void Alloc_ZeroCapacity_CreatesAnEmptyHeaderThatCanGrow()
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = QueueUnsafe<int>.Alloc(0, Allocator.Persistent);

                try
                {
                    Assert.IsTrue(data->IsCreated);
                    Assert.AreEqual(0, data->Capacity);

                    data->EnqueueRange(new[] { 1, 2, 3 });

                    CollectionAssert.AreEqual(new[] { 1, 2, 3 }, data->ToArray());
                }
                finally
                {
                    QueueUnsafe<int>.Free(data);
                }
            }
        }

        [Test]
        public void Alloc_SpanSource_CopiesTheSourceAndAcceptsAnEmptySource()
        {
            // SAFETY: Both headers returned by Alloc stay live until Free, which runs exactly once in finally.
            unsafe
            {
                var copy = QueueUnsafe<int>.Alloc(new[] { 1, 2, 3 }, Allocator.Persistent);
                var empty = QueueUnsafe<int>.Alloc(ReadOnlySpan<int>.Empty, Allocator.Persistent);

                try
                {
                    CollectionAssert.AreEqual(new[] { 1, 2, 3 }, copy->ToArray());
                    Assert.IsTrue(empty->IsCreated);
                    Assert.AreEqual(0, empty->Count);
                }
                finally
                {
                    QueueUnsafe<int>.Free(copy);
                    QueueUnsafe<int>.Free(empty);
                }
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Alloc_InvalidArgument_Throws()
        {
            // The header is allocated before the constructor validates the capacity (suspected defect D-17), so
            // this call leaks one header. Allocator.Temp is used so that Unity releases it without a leak report.
            // SAFETY: The call throws before it returns a header, so no pointer is used.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => QueueUnsafe<int>.Alloc(-1, Allocator.Temp));
            }
        }

        [Test]
        public void Free_NullPointer_DoesNothing()
        {
            // SAFETY: Free documents that a null header is ignored, so no memory is read or released.
            unsafe
            {
                Assert.DoesNotThrow(() => QueueUnsafe<int>.Free(null));
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void EnqueueRange_WrappedOwnSubspanOnFullQueue_AppendsOriginalSliceAfterGrowth()
        {
            var queue = new QueueUnsafe<int>(new[] { 11, 22, 33, 44 }, Allocator.Persistent);

            try
            {
                Assert.AreEqual(11, queue.Dequeue());
                Assert.AreEqual(22, queue.Dequeue());
                queue.Enqueue(55);
                queue.Enqueue(66);
                queue.EnqueueRange(queue._buffer.AsReadOnlySpan().Slice(1, 2));

                CollectionAssert.AreEqual(new[] { 33, 44, 55, 66, 66, 33 }, queue.ToArray());
            }
            finally
            {
                queue.Dispose();
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void EnqueueRange_OwnSubspanOverlapsSplitDestination_CopiesOriginalValues()
        {
            var queue = new QueueUnsafe<int>(8, Allocator.Persistent);

            try
            {
                queue.EnqueueRange(new[] { 11, 22, 33, 44, 55, 66 });

                for (var i = 0; i < 4; i++)
                {
                    queue.Dequeue();
                }

                queue._buffer[6] = 77;
                queue.EnqueueRange(queue._buffer.AsReadOnlySpan().Slice(4, 3));

                Assert.AreEqual(8, queue.Capacity);
                CollectionAssert.AreEqual(new[] { 55, 66, 55, 66, 77 }, queue.ToArray());
            }
            finally
            {
                queue.Dispose();
            }
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var queue = new QueueUnsafe<T>(2, Allocator.Temp);

            try
            {
                queue.Enqueue(a);
                queue.Enqueue(in b);

                Assert.AreEqual(a, queue.Dequeue(), name);

                queue.EnqueueRange(new[] { c, a });

                CollectionAssert.AreEqual(new[] { b, c, a }, queue.ToArray(), name);
                Assert.AreEqual(b, queue.Peek(), name);

                var copy = new T[3];
                queue.CopyTo(copy);

                CollectionAssert.AreEqual(new[] { b, c, a }, copy, name);
                Assert.IsTrue(queue.TryDequeue(out var first));
                Assert.AreEqual(b, first, name);
                Assert.AreEqual(c, queue.Dequeue(), name);
                Assert.AreEqual(a, queue.Dequeue(), name);
                Assert.AreEqual(0, queue.Count, name);
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
