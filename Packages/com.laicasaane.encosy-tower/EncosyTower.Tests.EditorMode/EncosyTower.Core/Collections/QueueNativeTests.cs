#if UNITY_COLLECTIONS

using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Buffers;
using EncosyTower.Collections;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from Unity.Collections.Tests/NativeQueueTests.cs and UnsafeRingQueueTests.cs.
    public partial class QueueNativeTests
    {
        [Test]
        public void ConstructorsAndInEnqueue_CreateExpectedContent()
        {
            using var capacity = new QueueNative<int>(4, Allocator.Temp);
            using var source = new QueueNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var four = 4;

            source.Enqueue(in four);

            Assert.IsTrue(capacity.IsCreated);
            Assert.AreEqual(0, capacity.Count);
            Assert.GreaterOrEqual(capacity.Capacity, 4);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, source.ToArray());
        }

        [Test]
        public void EnqueueAndDequeue_PreserveFifoOrderWhileGrowing()
        {
            const int N = 200;
            using var queue = new QueueNative<int>(2, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                queue.Enqueue(i);
            }

            Assert.AreEqual(N, queue.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.AreEqual(i, queue.Dequeue());
            }

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void EmptyQueue_DequeueAndPeekThrowWhileTryMethodsReturnFalse()
        {
            using var queue = new QueueNative<int>(0, Allocator.Temp);

            Assert.Catch(() => queue.Dequeue());
            Assert.Catch(() => queue.Peek());
            Assert.IsFalse(queue.TryDequeue(out var dequeued));
            Assert.IsFalse(queue.TryPeek(out var peeked));
            Assert.AreEqual(0, dequeued);
            Assert.AreEqual(0, peeked);
        }

        [Test]
        public void EnqueueRange_WrapsAndGrowsWithoutChangingOrder()
        {
            using var queue = new QueueNative<int>(8, Allocator.Temp);
            queue.EnqueueRange(new[] { 0, 1, 2, 3, 4, 5 });

            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(i, queue.Dequeue());
            }

            queue.EnqueueRange(new[] { 6, 7, 8 });
            queue.EnqueueRange(new[] { 9, 10, 11, 12, 13 });

            CollectionAssert.AreEqual(
                new[] { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 },
                queue.ToArray()
            );
        }

        [Test]
        public void PeekAndTryOperations_ObserveFrontWithoutRemovingIt()
        {
            using var queue = new QueueNative<int>(2, Allocator.Temp);
            queue.Enqueue(10);
            queue.Enqueue(20);

            Assert.AreEqual(10, queue.Peek());
            Assert.IsTrue(queue.TryPeek(out var peeked));
            Assert.AreEqual(10, peeked);
            Assert.AreEqual(2, queue.Count);

            Assert.IsTrue(queue.TryDequeue(out var dequeued));
            Assert.AreEqual(10, dequeued);
            Assert.AreEqual(20, queue.Peek());
        }

        [Test]
        public void Clear_ResetsCountAndAllowsReuse()
        {
            using var queue = new QueueNative<int>(4, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2, 3 });

            queue.Clear();

            Assert.AreEqual(0, queue.Count);
            Assert.IsFalse(queue.TryPeek(out _));

            queue.Enqueue(4);

            Assert.AreEqual(4, queue.Dequeue());
        }

        [Test]
        public void ToArrayAndCopyTo_LinearizeWrappedContent()
        {
            using var queue = new QueueNative<int>(8, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2, 3, 4, 5, 6 });

            for (var i = 0; i < 4; i++)
            {
                queue.Dequeue();
            }

            queue.EnqueueRange(new[] { 7, 8, 9, 10 });

            var full = new int[6];
            var partial = new int[3];
            queue.CopyTo(full);
            queue.CopyTo(2, partial);

            CollectionAssert.AreEqual(new[] { 5, 6, 7, 8, 9, 10 }, queue.ToArray());
            CollectionAssert.AreEqual(new[] { 5, 6, 7, 8, 9, 10 }, full);
            CollectionAssert.AreEqual(new[] { 7, 8, 9 }, partial);
        }

        [Test]
        public void CopyToAndTryCopyTo_AllOverloadsCopyOrReturnFalse()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var full = new int[4];
            var partial = new int[2];
            var offset = new int[2];
            var explicitLength = new int[3];

            queue.CopyTo(full);
            queue.CopyTo(partial.AsSpan(), 2);
            queue.CopyTo(1, offset);
            queue.CopyTo(1, explicitLength, 2);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full);
            CollectionAssert.AreEqual(new[] { 1, 2 }, partial);
            CollectionAssert.AreEqual(new[] { 2, 3 }, offset);
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, explicitLength);

            Assert.IsTrue(queue.TryCopyTo(full));
            Assert.IsTrue(queue.TryCopyTo(partial, 2));
            Assert.IsTrue(queue.TryCopyTo(1, offset));
            Assert.IsTrue(queue.TryCopyTo(1, explicitLength, 2));
            Assert.IsFalse(queue.TryCopyTo(3, partial));
        }

        [Test]
        public void ReadOnly_ConversionPropertiesPeekCopiesAndEnumerationExposeOwnerValues()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            QueueNative<int>.ReadOnly readOnly = queue;
            var direct = queue.AsReadOnly();
            var full = new int[4];
            var partial = new int[2];
            var offset = new int[2];
            var explicitLength = new int[3];

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(readOnly.Count, direct.Count);
            Assert.AreEqual(4, readOnly.Count);
            Assert.GreaterOrEqual(readOnly.Capacity, 4);
            Assert.AreEqual(1, readOnly.Peek());
            Assert.IsTrue(readOnly.TryPeek(out var peeked));
            Assert.AreEqual(1, peeked);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, readOnly.ToArray());

            readOnly.CopyTo(full);
            readOnly.CopyTo(partial.AsSpan(), 2);
            readOnly.CopyTo(1, offset);
            readOnly.CopyTo(1, explicitLength, 2);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full);
            CollectionAssert.AreEqual(new[] { 1, 2 }, partial);
            CollectionAssert.AreEqual(new[] { 2, 3 }, offset);
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, explicitLength);
            Assert.IsTrue(readOnly.TryCopyTo(full));
            Assert.IsTrue(readOnly.TryCopyTo(partial, 2));
            Assert.IsTrue(readOnly.TryCopyTo(1, offset));
            Assert.IsTrue(readOnly.TryCopyTo(1, explicitLength, 2));
            Assert.IsFalse(readOnly.TryCopyTo(3, partial));

            var enumerator = readOnly.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
            enumerator.Reset();
            Assert.IsTrue(enumerator.MoveNext());
            enumerator.Dispose();

            queue.Clear();
            Assert.IsFalse(readOnly.TryPeek(out peeked));
            Assert.AreEqual(0, peeked);
        }

        [Test]
        public void CapacityAndOwnerEnumeratorMembers_WorkDirectly()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2 }, Allocator.Temp);
            var initialCapacity = queue.Capacity;

            queue.EnsureCapacity(initialCapacity + 2);
            Assert.GreaterOrEqual(queue.Capacity, initialCapacity + 2);

            var ensured = queue.Capacity;
            queue.IncreaseCapacityBy(2);
            Assert.GreaterOrEqual(queue.Capacity, ensured + 2);

            var increased = queue.Capacity;
            queue.IncreaseCapacityTo(increased + 2);
            Assert.GreaterOrEqual(queue.Capacity, increased + 2);

            queue.Trim();
            Assert.AreEqual(queue.Count, queue.Capacity);

            var enumerator = queue.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
            enumerator.Reset();
            Assert.IsTrue(enumerator.MoveNext());
            enumerator.Dispose();
        }

        [Test]
        public void Enumerator_VisitsWrappedContentInFifoOrder()
        {
            using var queue = new QueueNative<int>(8, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2, 3, 4, 5, 6 });
            queue.Dequeue();
            queue.Dequeue();
            queue.EnqueueRange(new[] { 7, 8, 9 });
            var visited = new int[queue.Count];
            var index = 0;

            foreach (var value in queue)
            {
                visited[index++] = value;
            }

            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6, 7, 8, 9 }, visited);
        }

        [Test]
        public void Enumerator_MutationDuringIteration_Throws()
        {
            using var queue = new QueueNative<int>(8, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2, 3 });
            var enumerator = queue.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());

            queue.Enqueue(4);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var queue = new QueueNative<int>(4, Allocator.Temp);
            Assert.IsTrue(queue.IsCreated);

            queue.Dispose();

            Assert.IsFalse(queue.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void UseAfterDispose_Throws()
        {
            var queue = new QueueNative<int>(4, Allocator.Temp);
            queue.Enqueue(1);
            queue.Dispose();

            Assert.Catch(() => _ = queue.Count);
        }

        [Test]
        public void DoubleDispose_DoesNotThrow()
        {
            var queue = new QueueNative<int>(4, Allocator.Temp);
            queue.Dispose();

            Assert.DoesNotThrow(() => queue.Dispose());
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var queue = new QueueNative<int>(4, Allocator.TempJob);
            queue.Enqueue(1);

            var handle = queue.Dispose(default);

            Assert.IsFalse(queue.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAddsAndDisposes(Allocator allocator)
        {
            var queue = new QueueNative<int>(4, allocator);

            try
            {
                queue.Enqueue(10);

                Assert.IsTrue(queue.IsCreated);
                Assert.AreEqual(1, queue.Count);
                Assert.AreEqual(10, queue.Peek());
            }
            finally
            {
                queue.Dispose();
            }

            Assert.IsFalse(queue.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_InvalidArguments_Throw()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new QueueNative<int>(-1, Allocator.Temp));
            Assert.Throws<InvalidOperationException>(() => _ = new QueueNative<int>(4, default(AllocatorStrategy)));
            Assert.Throws<InvalidOperationException>(
                () => _ = new QueueNative<int>(new[] { 1 }, default(AllocatorStrategy))
            );
        }

        [Test]
        public void SpanConstructor_EmptySource_CreatesAnEmptyQueueThatCanGrow()
        {
            using var queue = new QueueNative<int>(ReadOnlySpan<int>.Empty, Allocator.Temp);

            Assert.IsTrue(queue.IsCreated);
            Assert.AreEqual(0, queue.Count);

            queue.Enqueue(1);

            CollectionAssert.AreEqual(new[] { 1 }, queue.ToArray());
        }

        [Test]
        public void Enqueue_FullWrappedQueue_GrowsAndKeepsFifoOrder()
        {
            var queue = new QueueNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

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
        public void Enqueue_ZeroCapacityQueue_GrowsOnTheFirstItem()
        {
            using var queue = new QueueNative<int>(0, Allocator.Temp);
            var second = 2;

            queue.Enqueue(1);
            queue.Enqueue(in second);

            CollectionAssert.AreEqual(new[] { 1, 2 }, queue.ToArray());
        }

        [Test]
        public void EnqueueRange_EmptyItemsOrZeroCapacity_BehaveLikeAnEmptyAppend()
        {
            using var queue = new QueueNative<int>(0, Allocator.Temp);

            queue.EnqueueRange(ReadOnlySpan<int>.Empty);

            Assert.AreEqual(0, queue.Count);

            queue.EnqueueRange(new[] { 1, 2, 3 });
            queue.EnqueueRange(ReadOnlySpan<int>.Empty);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());
        }

        [Test]
        public void EnqueueRange_ExactlyFillsTheRingThenGrows_KeepsFifoOrder()
        {
            var queue = new QueueNative<int>(3, Allocator.Temp);

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
        public void EnqueueRange_WrappingAppendWithSpareCapacity_KeepsFifoOrder()
        {
            using var queue = new QueueNative<int>(6, Allocator.Temp);

            queue.EnqueueRange(new[] { 1, 2, 3, 4, 5 });
            queue.Dequeue();
            queue.Dequeue();
            queue.Dequeue();
            queue.EnqueueRange(new[] { 6, 7, 8 });

            Assert.AreEqual(6, queue.Capacity);
            CollectionAssert.AreEqual(new[] { 4, 5, 6, 7, 8 }, queue.ToArray());
        }

        [Test]
        public void TryDequeue_LastElement_LeavesAnEmptyQueueThatCanBeRefilled()
        {
            using var queue = new QueueNative<int>(new[] { 1 }, Allocator.Temp);

            Assert.IsTrue(queue.TryDequeue(out var value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(0, queue.Count);
            Assert.IsFalse(queue.TryDequeue(out _));

            queue.Enqueue(2);
            queue.Enqueue(3);

            CollectionAssert.AreEqual(new[] { 2, 3 }, queue.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void DequeueAndPeek_EmptyQueue_ThrowInvalidOperation()
        {
            using var queue = new QueueNative<int>(4, Allocator.Temp);
            var readOnly = queue.AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
            Assert.Throws<InvalidOperationException>(() => queue.Peek());
            Assert.Throws<InvalidOperationException>(() => readOnly.Peek());
            Assert.IsFalse(readOnly.TryPeek(out var peeked));
            Assert.AreEqual(0, peeked);
        }

        [Test]
        public void EnsureCapacity_NotLargerThanCapacity_ReturnsTheCurrentCapacity()
        {
            using var queue = new QueueNative<int>(4, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2 });

            Assert.AreEqual(4, queue.EnsureCapacity(0));
            Assert.AreEqual(4, queue.EnsureCapacity(4));
            Assert.AreEqual(4, queue.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2 }, queue.ToArray());
        }

        [Test]
        public void EnsureCapacity_ZeroCapacityQueue_AllocatesAtLeastTheRequestedCapacity()
        {
            using var queue = new QueueNative<int>(0, Allocator.Temp);

            var capacity = queue.EnsureCapacity(6);

            Assert.GreaterOrEqual(capacity, 6);
            Assert.AreEqual(capacity, queue.Capacity);
            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void EnsureCapacity_NegativeSize_Throws()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => queue.EnsureCapacity(-1));
            CollectionAssert.AreEqual(new[] { 1, 2 }, queue.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_ZeroAmount_KeepsCapacityAndContent()
        {
            using var queue = new QueueNative<int>(4, Allocator.Temp);
            queue.EnqueueRange(new[] { 1, 2 });

            Assert.AreEqual(4, queue.IncreaseCapacityBy(0));
            Assert.AreEqual(4, queue.IncreaseCapacityTo(4));
            CollectionAssert.AreEqual(new[] { 1, 2 }, queue.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_WrappedQueue_KeepsFifoOrder()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            queue.Dequeue();
            queue.Dequeue();
            queue.Enqueue(5);
            queue.Enqueue(6);

            var capacity = queue.IncreaseCapacityBy(4);

            Assert.GreaterOrEqual(capacity, 8);
            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6 }, queue.ToArray());
            Assert.AreEqual(3, queue.Peek());
        }

        [Test]
        public void IncreaseCapacityBy_AmountOverflowingInt_ThrowsOverflow()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<OverflowException>(() => queue.IncreaseCapacityBy(int.MaxValue));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, queue.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IncreaseCapacity_BelowCount_Throws()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => queue.IncreaseCapacityBy(-5));
            Assert.Throws<InvalidOperationException>(() => queue.IncreaseCapacityTo(2));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, queue.ToArray());
        }

        [Test]
        public void Trim_EmptyQueue_ReleasesTheCapacityAndStaysUsable()
        {
            using var queue = new QueueNative<int>(8, Allocator.Temp);

            queue.Trim();

            Assert.IsTrue(queue.IsCreated);
            Assert.AreEqual(0, queue.Capacity);

            queue.Enqueue(3);

            CollectionAssert.AreEqual(new[] { 3 }, queue.ToArray());
        }

        [Test]
        public void Trim_FullQueue_KeepsContent()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            queue.Trim();

            Assert.AreEqual(3, queue.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());
        }

        [Test]
        public void Trim_WrappedQueue_LinearizesAndKeepsFifoOrder()
        {
            var queue = new QueueNative<int>(new[] { 1, 2, 3, 4, 5, 6 }, Allocator.Temp);

            try
            {
                for (var i = 0; i < 4; i++)
                {
                    queue.Dequeue();
                }

                queue.Enqueue(7);
                queue.Enqueue(8);

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
        public void Clear_EmptyQueueAndToArray_AreNoOpsThatKeepTheQueueUsable()
        {
            using var queue = new QueueNative<int>(0, Allocator.Temp);

            queue.Clear();

            Assert.IsTrue(queue.IsCreated);
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(0, queue.ToArray().Length);

            queue.Enqueue(1);

            CollectionAssert.AreEqual(new[] { 1 }, queue.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void CopyTo_InvalidRange_Throws()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
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
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
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
            using var empty = new QueueNative<int>(4, Allocator.Temp);
            using var queue = new QueueNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            empty.CopyTo(Span<int>.Empty);
            queue.CopyTo(Span<int>.Empty);
            queue.CopyTo(3, Span<int>.Empty);

            Assert.IsTrue(empty.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(queue.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(queue.TryCopyTo(3, Span<int>.Empty));
            Assert.IsFalse(empty.TryCopyTo(new int[1]));
        }

        [Test]
        public void CopyTo_WrappedQueueWithSourceStart_MatchesFifoSlice()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4, 5, 6 }, Allocator.Temp);

            for (var i = 0; i < 4; i++)
            {
                queue.Dequeue();
            }

            queue.Enqueue(7);
            queue.Enqueue(8);

            var expected = new[] { 5, 6, 7, 8 };

            for (var start = 0; start <= expected.Length; start++)
            {
                var destination = new int[expected.Length - start];

                queue.CopyTo(start, destination);

                CollectionAssert.AreEqual(expected.AsSpan(start).ToArray(), destination, $"start {start}");
            }

            var partial = new int[2];

            queue.CopyTo(1, partial, 2);

            CollectionAssert.AreEqual(new[] { 6, 7 }, partial);
        }

        [Test]
        public unsafe void CopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5 };
            var queue = new QueueNative<int>(8, Allocator.Temp);

            try
            {
                queue.EnqueueRange(seed);
                queue.CopyTo(queue.m_Data->_buffer.AsSpan()[1..6]);

                var expected = new int[8];
                Array.Copy(seed, expected, seed.Length);
                Array.Copy(expected, 0, expected, 1, 5);

                var actual = queue.m_Data->_buffer.AsReadOnlySpan()[..6].ToArray();

                CollectionAssert.AreEqual(expected.AsSpan(0, 6).ToArray(), actual);
            }
            finally
            {
                queue.Dispose();
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

        [Test]
        [TestRequiresCollectionChecks]
        public void Members_AfterOwnerDispose_Throw()
        {
            var queue = new QueueNative<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var value = 9;

            queue.Dispose();

            Assert.Catch(() => _ = queue.Count);
            Assert.Catch(() => _ = queue.Capacity);
            Assert.Catch(() => queue.Enqueue(1));
            Assert.Catch(() => queue.Enqueue(in value));
            Assert.Catch(() => queue.EnqueueRange(new[] { 1 }));
            Assert.Catch(() => queue.EnsureCapacity(8));
            Assert.Catch(() => queue.IncreaseCapacityBy(1));
            Assert.Catch(() => queue.IncreaseCapacityTo(8));
            Assert.Catch(() => queue.Trim());
            Assert.Catch(() => queue.Dequeue());
            Assert.Catch(() => queue.TryDequeue(out _));
            Assert.Catch(() => queue.Peek());
            Assert.Catch(() => queue.TryPeek(out _));
            Assert.Catch(() => queue.Clear());
            Assert.Catch(() => queue.ToArray());
            Assert.Catch(() => queue.CopyTo(new int[3]));
            Assert.Catch(() => queue.CopyTo(0, new int[3], 1));
            Assert.Catch(() => queue.TryCopyTo(new int[3]));
            Assert.Catch(() => queue.TryCopyTo(0, new int[3], 1));
            Assert.Catch(() => queue.GetEnumerator());
            Assert.Catch(() => queue.AsReadOnly());
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_AcceptedAllocator_MarksNotCreatedAndIsRepeatable(Allocator allocator)
        {
            var queue = new QueueNative<int>(4, allocator);
            queue.Enqueue(1);

            queue.Dispose();

            Assert.IsFalse(queue.IsCreated);
            Assert.DoesNotThrow(() => queue.Dispose());
        }

        [Test]
        public void Dispose_DefaultQueue_IsNoOp()
        {
            QueueNative<int> queue = default;

            Assert.DoesNotThrow(() => queue.Dispose());

            var handle = queue.Dispose(default);

            Assert.DoesNotThrow(() => handle.Complete());
            Assert.IsFalse(queue.IsCreated);
        }

        [Test]
        public void DisposeJob_PersistentAllocator_CompletesAndMarksNotCreated()
        {
            var queue = new QueueNative<int>(4, Allocator.Persistent);
            queue.Enqueue(1);

            var handle = queue.Dispose(default);

            Assert.IsFalse(queue.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void NativeDispose_NullHeader_IsNoOp()
        {
            Assert.DoesNotThrow(() => default(QueueNativeDispose).Dispose());
        }

        [Test]
        public unsafe void NativeDispose_AllocatedHeader_ReleasesIt()
        {
            var data = QueueUnsafe<int>.Alloc(4, Allocator.Persistent);
            data->Enqueue(1);

            Assert.AreEqual(1, data->Count);

            var dispose = new QueueNativeDispose { m_Data = data };
            dispose.Dispose();
        }

        [Test]
        public void ReadOnly_Constructor_FromDefaultQueue_Throws()
        {
            QueueNative<int> notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new QueueNative<int>.ReadOnly(notCreated));
            Assert.Throws<ArgumentException>(() => notCreated.AsReadOnly());
            Assert.Throws<ArgumentException>(() => _ = (QueueNative<int>.ReadOnly)notCreated);
        }

        [Test]
        public void ReadOnly_EmptyQueue_ReturnsEmptyResults()
        {
            using var queue = new QueueNative<int>(4, Allocator.Temp);
            var readOnly = queue.AsReadOnly();
            var enumerator = readOnly.GetEnumerator();

            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, readOnly.Count);
            Assert.AreEqual(0, readOnly.ToArray().Length);
            Assert.IsFalse(readOnly.TryPeek(out _));
            Assert.IsTrue(readOnly.TryCopyTo(Span<int>.Empty));
            Assert.IsFalse(readOnly.TryCopyTo(new int[1]));
            Assert.DoesNotThrow(() => readOnly.CopyTo(Span<int>.Empty));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnly_CopyTo_InvalidRange_Throws()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = queue.AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(-1, destination));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(5, destination));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(1, destination, 4));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void ReadOnly_TryCopyTo_InvalidRange_ReturnsFalseAndKeepsDestination()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = queue.AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.IsFalse(readOnly.TryCopyTo(-1, destination));
            Assert.IsFalse(readOnly.TryCopyTo(5, destination));
            Assert.IsFalse(readOnly.TryCopyTo(1, destination, 4));
            Assert.IsFalse(readOnly.TryCopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void ReadOnly_WrappedQueue_CopiesAndEnumeratesInFifoOrder()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3, 4, 5, 6 }, Allocator.Temp);

            for (var i = 0; i < 4; i++)
            {
                queue.Dequeue();
            }

            queue.Enqueue(7);
            queue.Enqueue(8);

            var readOnly = queue.AsReadOnly();
            var visited = new List<int>();

            foreach (var item in readOnly)
            {
                visited.Add(item);
            }

            var copy = new int[3];

            readOnly.CopyTo(1, copy);

            CollectionAssert.AreEqual(new[] { 5, 6, 7, 8 }, visited);
            CollectionAssert.AreEqual(new[] { 5, 6, 7, 8 }, readOnly.ToArray());
            CollectionAssert.AreEqual(new[] { 6, 7, 8 }, copy);
            Assert.IsTrue(readOnly.TryPeek(out var front));
            Assert.AreEqual(5, front);
        }

        [Test]
        public void ReadOnly_ElementShapes_RoundTripBitExact()
        {
            using var small = new QueueNative<Rgb>(new[] { new Rgb { r = 1, g = 2, b = 3 } }, Allocator.Temp);
            using var wide = new QueueNative<Wide>(
                  new[] { new Wide { a = long.MinValue, b = 0, c = long.MaxValue } }
                , Allocator.Temp
            );
            var smallView = small.AsReadOnly();
            var wideView = wide.AsReadOnly();

            Assert.AreEqual(3, smallView.Peek().b);
            Assert.AreEqual(2, smallView.ToArray()[0].g);
            Assert.AreEqual(long.MinValue, wideView.Peek().a);

            var copy = new Wide[1];
            wideView.CopyTo(copy);

            Assert.AreEqual(long.MaxValue, copy[0].c);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnly_Members_AfterOwnerDispose_Throw()
        {
            var queue = new QueueNative<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var readOnly = queue.AsReadOnly();

            queue.Dispose();

            Assert.Catch(() => _ = readOnly.Count);
            Assert.Catch(() => _ = readOnly.Capacity);
            Assert.Catch(() => readOnly.Peek());
            Assert.Catch(() => readOnly.TryPeek(out _));
            Assert.Catch(() => readOnly.ToArray());
            Assert.Catch(() => readOnly.CopyTo(new int[3]));
            Assert.Catch(() => readOnly.TryCopyTo(new int[3]));
            Assert.Catch(() => readOnly.GetEnumerator());
        }

        [Test]
        public unsafe void Enumerator_ConstructedFromDefaultReadOnly_Throws()
        {
            QueueNative<int>.ReadOnly notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new QueueNative<int>.Enumerator(null, in notCreated));
        }

        [Test]
        public void Enumerator_EmptyQueue_YieldsNothingForEveryEnumeratorForm()
        {
            using var queue = new QueueNative<int>(4, Allocator.Temp);

            var structEnumerator = queue.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)queue).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)queue).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [Test]
        public void Enumerator_ModifiedQueue_MoveNextAndResetThrow()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var enumerator = queue.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            queue.Clear();

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
        }

        [Test]
        public void Enumerator_Exhausted_StaysFinishedUntilReset()
        {
            using var queue = new QueueNative<int>(new[] { 1, 2 }, Allocator.Temp);
            var enumerator = queue.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var queue = new QueueNative<T>(1, Allocator.Temp);

            try
            {
                queue.Enqueue(a);
                queue.Enqueue(in b);
                queue.EnqueueRange(new[] { c });

                CollectionAssert.AreEqual(new[] { a, b, c }, queue.ToArray(), name);
                Assert.AreEqual(a, queue.Peek(), name);
                Assert.AreEqual(a, queue.Dequeue(), name);

                var copy = new T[2];
                queue.CopyTo(copy);

                CollectionAssert.AreEqual(new[] { b, c }, copy, name);
                Assert.IsTrue(queue.TryDequeue(out var second), name);
                Assert.AreEqual(b, second, name);
                Assert.IsTrue(queue.TryPeek(out var third), name);
                Assert.AreEqual(c, third, name);
                Assert.AreEqual(c, queue.AsReadOnly().Peek(), name);
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

#endif
