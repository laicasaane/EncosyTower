using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from Unity.Collections.Tests/NativeQueueTests.cs.
    public partial class SharedQueueTests
    {
        [Test]
        public void Constructors_CreateExpectedContent()
        {
            using var empty = new SharedQueue<int>();
            using var capacity = new SharedQueue<int>(8);
            using var span = new SharedQueue<int>(new[] { 1, 2, 3 }.AsSpan());
            using var collection = new SharedQueue<int>(new List<int> { 4, 5, 6 });

            Assert.AreEqual(0, empty.Count);
            Assert.AreEqual(0, capacity.Count);
            Assert.GreaterOrEqual(capacity.Capacity, 8);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, span.ToArray());
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, collection.ToArray());
        }

        [Test]
        public void GenericConstructors_CreateExpectedContent()
        {
            using var empty = new SharedQueue<int, uint>();
            using var capacity = new SharedQueue<int, uint>(8);
            using var span = new SharedQueue<int, uint>(new[] { 1, 2, 3 }.AsSpan());
            using var collection = new SharedQueue<int, uint>(new List<int> { 4, 5, 6 });

            Assert.IsTrue(empty.IsCreated);
            Assert.AreEqual(0, empty.Count);
            Assert.GreaterOrEqual(capacity.Capacity, 8);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, span.ToArray());
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, collection.ToArray());
        }

        [Test]
        public void EnqueueAndDequeue_GrowAndPreserveFifoOrder()
        {
            const int N = 200;
            using var queue = new SharedQueue<int>(2);

            for (var i = 0; i < N; i++)
            {
                queue.Enqueue(i);
            }

            Assert.AreEqual(N, queue.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.AreEqual(i, queue.Dequeue());
            }
        }

        [Test]
        public void ValueInAndTryOperations_ReturnExpectedValues()
        {
            using var queue = new SharedQueue<int, int>(4);
            var two = 2;

            queue.Enqueue(1);
            queue.Enqueue(in two);

            Assert.AreEqual(1, queue.Peek());
            Assert.IsTrue(queue.TryPeek(out var peeked));
            Assert.AreEqual(1, peeked);
            Assert.IsTrue(queue.TryDequeue(out var dequeued));
            Assert.AreEqual(1, dequeued);
            Assert.AreEqual(2, queue.Dequeue());
            Assert.IsFalse(queue.TryPeek(out peeked));
            Assert.IsFalse(queue.TryDequeue(out dequeued));
            Assert.AreEqual(0, peeked);
            Assert.AreEqual(0, dequeued);
        }

        [Test]
        public void EnqueueRange_WrappedQueueRetainsLinearOrder()
        {
            using var queue = new SharedQueue<int>(8);
            queue.EnqueueRange(new[] { 0, 1, 2, 3, 4, 5 });

            for (var i = 0; i < 4; i++)
            {
                queue.Dequeue();
            }

            queue.EnqueueRange(new[] { 6, 7, 8, 9 });

            CollectionAssert.AreEqual(new[] { 4, 5, 6, 7, 8, 9 }, queue.ToArray());
        }

        [Test]
        public void Clear_ResetsCountAndAllowsReuse()
        {
            using var queue = new SharedQueue<int>(new[] { 1, 2, 3 }.AsSpan());

            queue.Clear();

            Assert.AreEqual(0, queue.Count);
            queue.Enqueue(4);
            Assert.AreEqual(4, queue.Dequeue());
        }

        [Test]
        public void Trim_ShrinksCapacityToCountWithoutChangingOrder()
        {
            using var queue = new SharedQueue<int>(16);
            queue.EnqueueRange(new[] { 1, 2, 3 });

            queue.Trim();

            Assert.AreEqual(queue.Count, queue.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());
        }

        [Test]
        public void CopyToAndTryCopyTo_AllOwnerAndReadOnlyOverloadsPreserveFifoOrder()
        {
            using var queue = new SharedQueue<int, int>(new[] { 1, 2, 3, 4 }.AsSpan());
            var ownerFull = new int[4];
            var ownerPartial = new int[2];
            var ownerOffset = new int[2];
            var ownerLength = new int[3];

            queue.CopyTo(ownerFull);
            queue.CopyTo(ownerPartial.AsSpan(), 2);
            queue.CopyTo(1, ownerOffset);
            queue.CopyTo(1, ownerLength, 2);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, ownerFull);
            CollectionAssert.AreEqual(new[] { 1, 2 }, ownerPartial);
            CollectionAssert.AreEqual(new[] { 2, 3 }, ownerOffset);
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, ownerLength);
            Assert.IsTrue(queue.TryCopyTo(ownerFull));
            Assert.IsTrue(queue.TryCopyTo(ownerPartial, 2));
            Assert.IsTrue(queue.TryCopyTo(1, ownerOffset));
            Assert.IsTrue(queue.TryCopyTo(1, ownerLength, 2));
            Assert.IsFalse(queue.TryCopyTo(3, ownerPartial));

            // SAFETY: queue remains the live owner while both borrowed read-only aliases are exercised.
            unsafe
            {
                SharedQueue<int, int>.ReadOnly readOnly = queue;
                var readOnlyFull = new int[4];
                var readOnlyPartial = new int[2];
                var readOnlyOffset = new int[2];
                var readOnlyLength = new int[3];

                Assert.IsTrue(readOnly.IsCreated);
                Assert.AreEqual(4, readOnly.Count);
                Assert.GreaterOrEqual(readOnly.Capacity, 4);
                Assert.AreEqual(1, readOnly.Peek());
                Assert.IsTrue(readOnly.TryPeek(out var peeked));
                Assert.AreEqual(1, peeked);
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, readOnly.ToArray());

                readOnly.CopyTo(readOnlyFull);
                readOnly.CopyTo(readOnlyPartial.AsSpan(), 2);
                readOnly.CopyTo(1, readOnlyOffset);
                readOnly.CopyTo(1, readOnlyLength, 2);

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, readOnlyFull);
                CollectionAssert.AreEqual(new[] { 1, 2 }, readOnlyPartial);
                CollectionAssert.AreEqual(new[] { 2, 3 }, readOnlyOffset);
                CollectionAssert.AreEqual(new[] { 2, 3, 0 }, readOnlyLength);
                Assert.IsTrue(readOnly.TryCopyTo(readOnlyFull));
                Assert.IsTrue(readOnly.TryCopyTo(readOnlyPartial, 2));
                Assert.IsTrue(readOnly.TryCopyTo(1, readOnlyOffset));
                Assert.IsTrue(readOnly.TryCopyTo(1, readOnlyLength, 2));
                Assert.IsFalse(readOnly.TryCopyTo(3, readOnlyPartial));

                var nativeReadOnly = readOnly.AsNative();
                Assert.AreEqual(4, nativeReadOnly.Count);

                queue.Clear();
                Assert.IsFalse(readOnly.TryPeek(out peeked));
                Assert.AreEqual(0, peeked);
            }
        }

        [Test]
        public void CapacityConversionsEnumeratorsAndDispose_WorkDirectly()
        {
            var queue = new SharedQueue<int, int>(new[] { 1, 2 }.AsSpan());

            try
            {
                var initialCapacity = queue.Capacity;
                queue.IncreaseCapacityBy(2);
                Assert.GreaterOrEqual(queue.Capacity, initialCapacity + 2);

                var increased = queue.Capacity;
                queue.IncreaseCapacityTo(increased + 2);
                Assert.GreaterOrEqual(queue.Capacity, increased + 2);

                // SAFETY: queue remains alive and unmodified while its aliases and enumerators are used.
                unsafe
                {
                    SharedQueueNative<int> native = queue;
                    Assert.AreEqual(2, native.Count);

                    var ownerEnumerator = queue.GetEnumerator();
                    Assert.IsTrue(ownerEnumerator.MoveNext());
                    Assert.AreEqual(1, ownerEnumerator.Current);
                    ownerEnumerator.Reset();
                    Assert.IsTrue(ownerEnumerator.MoveNext());
                    ownerEnumerator.Dispose();

                    var readOnlyEnumerator = queue.AsReadOnly().GetEnumerator();
                    Assert.IsTrue(readOnlyEnumerator.MoveNext());
                    Assert.AreEqual(1, readOnlyEnumerator.Current);
                    readOnlyEnumerator.Reset();
                    Assert.IsTrue(readOnlyEnumerator.MoveNext());
                    readOnlyEnumerator.Dispose();
                }
            }
            finally
            {
                queue.Dispose();
            }

            Assert.IsFalse(queue.IsCreated);
            Assert.DoesNotThrow(() => queue.Dispose());
        }

        [Test]
        public void EnqueueRange_EmptyInput_LeavesTheQueueUnchanged()
        {
            using var zeroCapacity = new SharedQueue<int>();
            using var queue = new SharedQueue<int>(new[] { 1, 2, 3 }.AsSpan());

            zeroCapacity.EnqueueRange(ReadOnlySpan<int>.Empty);
            zeroCapacity.EnqueueRange(Array.Empty<int>());
            queue.EnqueueRange(ReadOnlySpan<int>.Empty);

            Assert.AreEqual(0, zeroCapacity.Count);
            Assert.AreEqual(0, zeroCapacity.Capacity);
            Assert.AreEqual(3, queue.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());
        }

        [Test]
        public void EnqueueRange_ZeroCapacityQueue_GrowsAndKeepsFifoOrder()
        {
            using var queue = new SharedQueue<int>();
            var items = new int[50];

            for (var i = 0; i < items.Length; i++)
            {
                items[i] = i * 3;
            }

            queue.EnqueueRange(items);

            Assert.AreEqual(items.Length, queue.Count);
            Assert.GreaterOrEqual(queue.Capacity, items.Length);
            CollectionAssert.AreEqual(items, queue.ToArray());
        }

        [Test]
        public void EnqueueRange_WrapAtEveryHeadAndLength_MatchesQueueOfT()
        {
            const int CAPACITY = 8;

            for (var head = 0; head < CAPACITY; head++)
            {
                for (var length = 0; length <= CAPACITY; length++)
                {
                    using var queue = new SharedQueue<int>(CAPACITY);
                    var model = new Queue<int>();
                    var rotation = new int[head];
                    var items = new int[length];
                    var message = $"head {head} length {length}";

                    for (var i = 0; i < head; i++)
                    {
                        rotation[i] = -(i + 1);
                    }

                    for (var i = 0; i < length; i++)
                    {
                        items[i] = 100 + i;
                        model.Enqueue(items[i]);
                    }

                    queue.EnqueueRange(rotation);

                    for (var i = 0; i < head; i++)
                    {
                        queue.Dequeue();
                    }

                    queue.EnqueueRange(items);

                    Assert.AreEqual(CAPACITY, queue.Capacity, message);
                    Assert.AreEqual(length, queue.Count, message);
                    CollectionAssert.AreEqual(model.ToArray(), queue.ToArray(), message);

                    while (model.Count > 0)
                    {
                        Assert.AreEqual(model.Dequeue(), queue.Dequeue(), message);
                    }

                    Assert.AreEqual(0, queue.Count, message);
                }
            }
        }

        [Test]
        public void EnqueueRange_PastTheCapacityWhileWrapped_KeepsFifoOrder()
        {
            using var queue = new SharedQueue<int>(8);
            var model = new Queue<int>();
            var items = new int[10];

            for (var i = 0; i < 6; i++)
            {
                queue.Enqueue(i);
                model.Enqueue(i);
            }

            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(model.Dequeue(), queue.Dequeue());
            }

            for (var i = 0; i < 4; i++)
            {
                queue.Enqueue(10 + i);
                model.Enqueue(10 + i);
            }

            for (var i = 0; i < items.Length; i++)
            {
                items[i] = 100 + i;
                model.Enqueue(items[i]);
            }

            queue.EnqueueRange(items);

            Assert.Greater(queue.Capacity, 8);
            Assert.AreEqual(model.Count, queue.Count);
            CollectionAssert.AreEqual(model.ToArray(), queue.ToArray());

            while (model.Count > 0)
            {
                Assert.AreEqual(model.Dequeue(), queue.Dequeue());
            }
        }

        [Test]
        public void EnqueueRange_ElementSizes_RoundTripBitExact()
        {
            AssertEnqueueRangeRoundTrip(new byte[] { 0, 255, 128 });
            AssertEnqueueRangeRoundTrip(new[] { long.MinValue, -1L, long.MaxValue });
            AssertEnqueueRangeRoundTrip(new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 0, b = 128 } });
            AssertEnqueueRangeRoundTrip(new[] { new Wide { a = long.MinValue, b = -1, c = long.MaxValue } });
        }

        [Test]
        public void AsNative_ViewAndOwnerShareTheRing()
        {
            using var queue = new SharedQueue<int>(8);

            // SAFETY: queue outlives the native view, which is used before any resize.
            unsafe
            {
                var native = queue.AsNative();

                queue.Enqueue(1);
                native.Enqueue(2);

                Assert.AreEqual(2, queue.Count);
                Assert.AreEqual(2, native.Count);
                CollectionAssert.AreEqual(new[] { 1, 2 }, queue.ToArray());
                CollectionAssert.AreEqual(new[] { 1, 2 }, native.ToArray());
                Assert.AreEqual(1, native.Dequeue());
                Assert.AreEqual(1, queue.Count);
                Assert.AreEqual(2, queue.Peek());

                queue.Clear();

                Assert.AreEqual(0, native.Count);
            }
        }

        [Test]
        public void AsReadOnly_ViewsSeeLaterEnqueuesAndDequeues()
        {
            using var queue = new SharedQueue<int>(8);

            // SAFETY: queue outlives the views, which are used before any resize.
            unsafe
            {
                var view = queue.AsReadOnly();
                var nativeReadOnly = view.AsNative();

                queue.Enqueue(5);
                queue.Enqueue(6);

                Assert.AreEqual(2, view.Count);
                Assert.AreEqual(2, nativeReadOnly.Count);
                Assert.AreEqual(5, view.Peek());
                Assert.AreEqual(5, queue.Dequeue());
                Assert.AreEqual(1, nativeReadOnly.Count);
                Assert.AreEqual(6, view.Peek());
                CollectionAssert.AreEqual(new[] { 6 }, view.ToArray());
            }
        }

        [Test]
        public void GetEnumerator_EmptyQueues_YieldNothing()
        {
            using var zeroCapacity = new SharedQueue<int>();
            using var withCapacity = new SharedQueue<int>(4);
            var count = 0;

            // SAFETY: The queues outlive the views and their enumerators and are not modified while they run.
            unsafe
            {
                var zeroView = zeroCapacity.AsReadOnly();
                var capacityView = withCapacity.AsReadOnly();

                foreach (var value in zeroCapacity)
                {
                    count++;
                }

                foreach (var value in withCapacity)
                {
                    count++;
                }

                foreach (var value in (IEnumerable<int>)zeroCapacity)
                {
                    count++;
                }

                foreach (var value in (IEnumerable)withCapacity)
                {
                    count++;
                }

                foreach (var value in zeroView)
                {
                    count++;
                }

                foreach (var value in capacityView)
                {
                    count++;
                }

                foreach (var value in (IEnumerable<int>)zeroView)
                {
                    count++;
                }

                foreach (var value in (IEnumerable)capacityView)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ReadOnlyAsNative_ViewSeesLaterEnqueues()
        {
            using var queue = new SharedQueue<int>(8);

            // SAFETY: queue outlives the views, which are used before any resize.
            unsafe
            {
                var view = queue.AsReadOnly();
                var native = view.AsNative();

                Assert.IsTrue(native.IsCreated);
                Assert.AreEqual(0, native.Count);

                queue.Enqueue(7);

                Assert.AreEqual(1, native.Count);
                CollectionAssert.AreEqual(new[] { 7 }, native.ToArray());
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Dispose_QueueWithLiveNativeView_InvalidatesTheView()
        {
            var queue = new SharedQueue<int>(new[] { 1, 2 }.AsSpan());

            // SAFETY: This test intentionally retains a borrowed alias across owner disposal to verify checks.
            unsafe
            {
                var native = queue.AsNative();

                queue.Dispose();

                Assert.Catch(() => _ = native.Count);
                Assert.DoesNotThrow(() => queue.Dispose());
            }
        }

        [Test]
        public void Dispose_QueueState_ReportsNotCreatedAndEmpty()
        {
            var queue = new SharedQueue<int>(new[] { 1, 2 }.AsSpan());
            var zeroCapacity = new SharedQueue<int>();

            queue.Dispose();
            zeroCapacity.Dispose();

            Assert.IsFalse(queue.IsCreated);
            Assert.IsFalse(zeroCapacity.IsCreated);
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(0, queue.Capacity);
            Assert.AreEqual(0, zeroCapacity.Count);
            Assert.DoesNotThrow(() => zeroCapacity.Dispose());
        }

        [Test]
        public void InternalState_FollowsTheRing()
        {
            using var queue = new SharedQueue<int>(4);

            Assert.AreEqual(0, queue.HeadRO);
            Assert.AreEqual(0, queue.TailRO);
            Assert.AreEqual(0, queue.CountRO);

            var version = queue.VersionRO;

            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);

            Assert.AreEqual(0, queue.HeadRO);
            Assert.AreEqual(3, queue.TailRO);
            Assert.AreEqual(3, queue.CountRO);
            Assert.AreNotEqual(version, queue.VersionRO);

            Assert.AreEqual(1, queue.Dequeue());

            Assert.AreEqual(1, queue.HeadRO);
            Assert.AreEqual(2, queue.CountRO);

            queue.Enqueue(4);
            queue.Enqueue(5);

            Assert.AreEqual(1, queue.HeadRO);
            Assert.AreEqual(1, queue.TailRO);
            Assert.AreEqual(4, queue.CountRO);

            version = queue.VersionRO;

            _ = queue.Peek();
            _ = queue.ToArray();
            _ = queue.Count;

            Assert.AreEqual(version, queue.VersionRO);

            queue.VersionRW++;

            Assert.AreEqual(version + 1, queue.VersionRO);

            queue.Clear();

            Assert.AreEqual(0, queue.HeadRO);
            Assert.AreEqual(0, queue.TailRO);
            Assert.AreEqual(0, queue.CountRO);

            queue.HeadRW = 2;
            queue.TailRW = 2;
            queue.CountRW = 0;

            queue.Enqueue(9);

            Assert.AreEqual(9, queue.Dequeue());
        }

        private static void AssertEnqueueRangeRoundTrip<T>(T[] values)
            where T : unmanaged
        {
            using var queue = new SharedQueue<T>();

            queue.EnqueueRange(values);

            SpanBytesAssert.AreEqual(values, queue.ToArray());

            var first = queue.Dequeue();

            queue.EnqueueRange(new[] { first });

            var rotated = new T[values.Length];

            values.AsSpan(1).CopyTo(rotated);
            rotated[^1] = values[0];

            SpanBytesAssert.AreEqual(rotated, queue.ToArray());
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
