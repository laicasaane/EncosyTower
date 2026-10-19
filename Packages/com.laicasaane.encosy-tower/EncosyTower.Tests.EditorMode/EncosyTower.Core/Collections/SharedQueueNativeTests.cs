using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from Unity.Collections.Tests/NativeQueueTests.cs against SharedQueueNative<T>.
    public partial class SharedQueueNativeTests
    {
        [Test]
        public void OwnerEnqueue_AfterViewCreationIsVisible()
        {
            using var queue = new SharedQueue<int>(8);
            queue.Enqueue(1);

            // SAFETY: queue remains live while its borrowed native view is used.
            unsafe
            {
                var view = queue.AsNative();

                queue.Enqueue(2);

                Assert.AreEqual(2, view.Count);
                CollectionAssert.AreEqual(new[] { 1, 2 }, view.ToArray());
            }
        }

        [Test]
        public void DefaultImplicitConversionAndValueInOperations_ExposeSharedFifoState()
        {
            SharedQueueNative<int> empty = default;
            using var queue = new SharedQueue<int, int>(8);

            // SAFETY: queue remains live while its borrowed implicit native view is used.
            unsafe
            {
                SharedQueueNative<int> view = queue;
                var two = 2;

                view.Enqueue(1);
                view.Enqueue(in two);

                Assert.IsFalse(empty.IsCreated);
                Assert.IsTrue(view.IsCreated);
                Assert.AreEqual(2, view.Count);
                Assert.GreaterOrEqual(view.Capacity, 8);
                Assert.AreEqual(1, view.Peek());
                Assert.IsTrue(view.TryPeek(out var peeked));
                Assert.AreEqual(1, peeked);
                Assert.IsTrue(view.TryDequeue(out var dequeued));
                Assert.AreEqual(1, dequeued);
                Assert.AreEqual(2, view.Dequeue());
                Assert.IsFalse(view.TryPeek(out peeked));
                Assert.IsFalse(view.TryDequeue(out dequeued));
                Assert.AreEqual(0, peeked);
                Assert.AreEqual(0, dequeued);
            }
        }

        [Test]
        public void CopyToAndTryCopyTo_NativeAndReadOnlyOverloadsPreserveFifoOrder()
        {
            using var queue = new SharedQueue<int>(new[] { 1, 2, 3, 4 }.AsSpan());

            // SAFETY: queue remains live while native and read-only aliases are used.
            unsafe
            {
                var view = queue.AsNative();
                var full = new int[4];
                var partial = new int[2];
                var offset = new int[2];
                var explicitLength = new int[3];

                view.CopyTo(full);
                view.CopyTo(partial.AsSpan(), 2);
                view.CopyTo(1, offset);
                view.CopyTo(1, explicitLength, 2);

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, view.ToArray());
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full);
                CollectionAssert.AreEqual(new[] { 1, 2 }, partial);
                CollectionAssert.AreEqual(new[] { 2, 3 }, offset);
                CollectionAssert.AreEqual(new[] { 2, 3, 0 }, explicitLength);
                Assert.IsTrue(view.TryCopyTo(full));
                Assert.IsTrue(view.TryCopyTo(partial, 2));
                Assert.IsTrue(view.TryCopyTo(1, offset));
                Assert.IsTrue(view.TryCopyTo(1, explicitLength, 2));
                Assert.IsFalse(view.TryCopyTo(3, partial));

                var readOnly = view.AsReadOnly();
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

                view.Clear();
                Assert.IsFalse(readOnly.TryPeek(out peeked));
                Assert.AreEqual(0, peeked);
            }
        }

        [Test]
        public void NativeAndReadOnlyEnumerators_MoveResetAndDisposeDirectly()
        {
            using var queue = new SharedQueue<int>(new[] { 1, 2 }.AsSpan());

            // SAFETY: queue remains live and unmodified while both borrowed enumerators are consumed.
            unsafe
            {
                var view = queue.AsNative();
                var ownerEnumerator = view.GetEnumerator();

                Assert.IsTrue(ownerEnumerator.MoveNext());
                Assert.AreEqual(1, ownerEnumerator.Current);
                ownerEnumerator.Reset();
                Assert.IsTrue(ownerEnumerator.MoveNext());
                ownerEnumerator.Dispose();

                var readOnlyEnumerator = view.AsReadOnly().GetEnumerator();
                Assert.IsTrue(readOnlyEnumerator.MoveNext());
                Assert.AreEqual(1, readOnlyEnumerator.Current);
                readOnlyEnumerator.Reset();
                Assert.IsTrue(readOnlyEnumerator.MoveNext());
                readOnlyEnumerator.Dispose();
            }
        }

        [Test]
        public void Clear_ResetsSharedCountAndAllowsReuse()
        {
            using var queue = new SharedQueue<int>(new[] { 1, 2 }.AsSpan());

            // SAFETY: queue remains live while its borrowed native view is used.
            unsafe
            {
                var view = queue.AsNative();

                view.Clear();
                view.Enqueue(3);

                CollectionAssert.AreEqual(new[] { 3 }, queue.ToArray());
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ViewCreatedBeforeOwnerGrow_ThrowsOnNextAccess()
        {
            using var queue = new SharedQueue<int>(2);
            queue.Enqueue(1);
            queue.Enqueue(2);

            // SAFETY: This test intentionally uses a borrowed view after owner growth to verify safety checks.
            unsafe
            {
                var staleView = queue.AsNative();

                queue.Enqueue(3);

                Assert.Catch(() => _ = staleView.Count);
            }
        }

        [Test]
        public void ViewCreatedAfterOwnerGrow_CanMutateOwner()
        {
            using var queue = new SharedQueue<int>(2);
            queue.EnqueueRange(new[] { 1, 2, 3 });

            // SAFETY: queue remains live and stable after growth while the refreshed view is used.
            unsafe
            {
                var view = queue.AsNative();

                view.Enqueue(4);

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, queue.ToArray());
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void FullView_EnqueueThrowsBecauseCapacityIsImmutable()
        {
            using var queue = new SharedQueue<int>(new[] { 1, 2 }.AsSpan());

            // SAFETY: queue remains live while the fixed-capacity borrowed view rejects growth.
            unsafe
            {
                var view = queue.AsNative();

                Assert.Throws<InvalidOperationException>(() => view.Enqueue(3));
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Dispose_ReleasesOwnerAndNativeHeader()
        {
            var queue = new SharedQueue<int>(4);
            queue.Enqueue(1);

            // SAFETY: This test intentionally keeps a borrowed view past owner disposal to verify checks.
            unsafe
            {
                var view = queue.AsNative();

                queue.Dispose();

                Assert.IsFalse(queue.IsCreated);
                Assert.Catch(() => _ = view.Count);
                Assert.DoesNotThrow(() => queue.Dispose());
            }
        }

        [Test]
        public unsafe void Constructor_BorrowedHeader_ViewsTheHeaderAndAcceptsANullHeader()
        {
            using var storage = new SharedQueueStorage<int>(4);
            var header = storage.CreateHeader();

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            var safety = AtomicSafetyHandle.Create();
            var view = new SharedQueueNative<int>(&header, safety);
            var nullView = new SharedQueueNative<int>(null, safety);
#else
            var view = new SharedQueueNative<int>(&header);
            var nullView = new SharedQueueNative<int>(null);
#endif

            try
            {
                Assert.IsTrue(view.IsCreated);
                Assert.IsFalse(nullView.IsCreated);
                Assert.AreEqual(4, view.Capacity);
                Assert.AreEqual(0, view.Count);

                view.Enqueue(5);

                Assert.AreEqual(1, storage.Count);
                Assert.AreEqual(5, storage.buffer[0]);
                Assert.AreEqual(5, view.Peek());
            }
            finally
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                AtomicSafetyHandle.Release(safety);
#endif
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void DequeueAndPeek_EmptyQueue_ThrowWhileTryMethodsReturnFalse()
        {
            using var queue = new SharedQueue<int>(4);
            var view = queue.AsNative();
            var readOnly = view.AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => view.Dequeue());
            Assert.Throws<InvalidOperationException>(() => view.Peek());
            Assert.Throws<InvalidOperationException>(() => readOnly.Peek());
            Assert.IsFalse(view.TryDequeue(out var dequeued));
            Assert.IsFalse(view.TryPeek(out var peeked));
            Assert.IsFalse(readOnly.TryPeek(out var readOnlyPeeked));
            Assert.AreEqual(0, dequeued);
            Assert.AreEqual(0, peeked);
            Assert.AreEqual(0, readOnlyPeeked);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ZeroCapacityQueue_EnqueueAndDequeue_Throw()
        {
            using var queue = new SharedQueue<int>(0);
            var view = queue.AsNative();

            Assert.IsTrue(view.IsCreated);
            Assert.AreEqual(0, view.Capacity);
            Assert.Throws<InvalidOperationException>(() => view.Enqueue(1));
            Assert.Throws<InvalidOperationException>(() => view.Dequeue());
            Assert.Throws<InvalidOperationException>(() => view.Peek());
            Assert.AreEqual(0, view.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Enqueue_ExactlyFullThenOnePast_ThrowsAndKeepsTheContent()
        {
            using var queue = new SharedQueue<int>(3);
            var view = queue.AsNative();
            var value = 5;

            view.Enqueue(1);
            view.Enqueue(2);
            view.Enqueue(3);

            Assert.AreEqual(3, view.Count);
            Assert.AreEqual(3, view.Capacity);
            Assert.Throws<InvalidOperationException>(() => view.Enqueue(4));
            Assert.Throws<InvalidOperationException>(() => view.Enqueue(in value));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, queue.ToArray());
        }

        [Test]
        public unsafe void Enqueue_AfterDequeueFromAFullQueue_WrapsTheRing()
        {
            using var queue = new SharedQueue<int>(3);
            var view = queue.AsNative();

            view.Enqueue(1);
            view.Enqueue(2);
            view.Enqueue(3);

            Assert.AreEqual(1, view.Dequeue());

            view.Enqueue(4);

            Assert.AreEqual(3, view.Count);
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, view.ToArray());
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, queue.ToArray());
            Assert.AreEqual(2, view.Peek());
        }

        [Test]
        public unsafe void SingleSlotQueue_RepeatedEnqueueAndDequeue_KeepsTheFifoOrder()
        {
            using var queue = new SharedQueue<int>(1);
            var view = queue.AsNative();

            for (var i = 0; i < 5; i++)
            {
                view.Enqueue(i);

                Assert.AreEqual(i, view.Peek());
                Assert.IsTrue(view.TryDequeue(out var value));
                Assert.AreEqual(i, value);
                Assert.AreEqual(0, view.Count);
            }
        }

        [Test]
        public unsafe void Clear_EmptyAndWrappedQueue_ResetsTheCountAndStaysUsable()
        {
            using var queue = new SharedQueue<int>(3);
            var view = queue.AsNative();

            view.Clear();

            Assert.AreEqual(0, view.Count);
            Assert.AreEqual(0, view.ToArray().Length);

            view.Enqueue(1);
            view.Enqueue(2);
            view.Enqueue(3);
            view.Dequeue();
            view.Enqueue(4);
            view.Clear();

            Assert.AreEqual(0, queue.Count);
            Assert.IsFalse(view.TryPeek(out _));

            view.Enqueue(5);
            view.Enqueue(6);

            CollectionAssert.AreEqual(new[] { 5, 6 }, view.ToArray());
        }

        [Test]
        public unsafe void CopyTo_WrappedQueue_CopiesEveryWindowInFifoOrder()
        {
            using var queue = new SharedQueue<int>(4);
            var view = queue.AsNative();

            view.Enqueue(1);
            view.Enqueue(2);
            view.Enqueue(3);
            view.Enqueue(4);
            view.Dequeue();
            view.Dequeue();
            view.Enqueue(5);
            view.Enqueue(6);

            var expected = new[] { 3, 4, 5, 6 };

            for (var start = 0; start <= expected.Length; start++)
            {
                var destination = new int[expected.Length - start];

                view.CopyTo(start, destination);

                CollectionAssert.AreEqual(expected.AsSpan(start).ToArray(), destination, $"start {start}");
            }

            var partial = new int[2];

            view.CopyTo(1, partial, 2);

            CollectionAssert.AreEqual(new[] { 4, 5 }, partial);
            CollectionAssert.AreEqual(expected, view.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void CopyTo_InvalidRange_Throws()
        {
            using var queue = new SharedQueue<int>(new[] { 1, 2, 3, 4 }.AsSpan());
            var view = queue.AsNative();
            var readOnly = view.AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<InvalidOperationException>(() => view.CopyTo(-1, destination));
            Assert.Throws<InvalidOperationException>(() => view.CopyTo(5, destination));
            Assert.Throws<InvalidOperationException>(() => view.CopyTo(1, destination, 4));
            Assert.Throws<InvalidOperationException>(() => view.CopyTo(0, destination, -1));
            Assert.Throws<InvalidOperationException>(() => view.CopyTo(0, new int[2], 3));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(-1, destination));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(5, destination));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public unsafe void TryCopyTo_InvalidRange_ReturnsFalseAndKeepsDestination()
        {
            using var queue = new SharedQueue<int>(new[] { 1, 2, 3, 4 }.AsSpan());
            var view = queue.AsNative();
            var readOnly = view.AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.IsFalse(view.TryCopyTo(-1, destination));
            Assert.IsFalse(view.TryCopyTo(5, destination));
            Assert.IsFalse(view.TryCopyTo(1, destination, 4));
            Assert.IsFalse(view.TryCopyTo(0, destination, -1));
            Assert.IsFalse(view.TryCopyTo(0, new int[2], 3));
            Assert.IsFalse(readOnly.TryCopyTo(-1, destination));
            Assert.IsFalse(readOnly.TryCopyTo(5, destination));
            Assert.IsFalse(readOnly.TryCopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public unsafe void CopyTo_EmptyQueueOrEmptyDestination_CopiesNothing()
        {
            using var empty = new SharedQueue<int>(4);
            using var queue = new SharedQueue<int>(new[] { 1, 2, 3 }.AsSpan());
            var emptyView = empty.AsNative();
            var view = queue.AsNative();

            emptyView.CopyTo(Span<int>.Empty);
            view.CopyTo(Span<int>.Empty);
            view.CopyTo(3, Span<int>.Empty);

            Assert.IsTrue(emptyView.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(view.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(view.TryCopyTo(3, Span<int>.Empty));
            Assert.IsFalse(emptyView.TryCopyTo(new int[1]));
        }

        [Test]
        public unsafe void Elements_ByteElement_RoundTripBitExact()
        {
            AssertElementRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public unsafe void Elements_ThreeByteStruct_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public unsafe void Elements_LongElement_RoundTripBitExact()
        {
            AssertElementRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public unsafe void Elements_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = 0, b = 0, c = 0 }
                , new Wide { a = long.MinValue, b = 7, c = -1 }
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Members_AfterOwnerDispose_Throw()
        {
            var queue = new SharedQueue<int>(new[] { 1, 2, 3 }.AsSpan());
            var view = queue.AsNative();
            var value = 9;

            queue.Dispose();

            Assert.Catch(() => _ = view.Count);
            Assert.Catch(() => _ = view.Capacity);
            Assert.Catch(() => view.Enqueue(1));
            Assert.Catch(() => view.Enqueue(in value));
            Assert.Catch(() => view.Dequeue());
            Assert.Catch(() => view.TryDequeue(out _));
            Assert.Catch(() => view.Peek());
            Assert.Catch(() => view.TryPeek(out _));
            Assert.Catch(() => view.Clear());
            Assert.Catch(() => view.ToArray());
            Assert.Catch(() => view.CopyTo(new int[3]));
            Assert.Catch(() => view.CopyTo(0, new int[3], 1));
            Assert.Catch(() => view.TryCopyTo(new int[3]));
            Assert.Catch(() => view.TryCopyTo(0, new int[3], 1));
            Assert.Catch(() => view.GetEnumerator());
        }

        [Test]
        public unsafe void ReadOnly_Constructor_FromDefaultView_Throws()
        {
            SharedQueueNative<int> notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new SharedQueueNative<int>.ReadOnly(notCreated));
        }

        [Test]
        public unsafe void ReadOnly_EmptyQueue_ReturnsEmptyResults()
        {
            using var queue = new SharedQueue<int>(4);
            var readOnly = queue.AsNative().AsReadOnly();
            var enumerator = readOnly.GetEnumerator();

            Assert.IsTrue(readOnly.IsCreated);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, readOnly.Count);
            Assert.AreEqual(4, readOnly.Capacity);
            Assert.AreEqual(0, readOnly.ToArray().Length);
            Assert.IsFalse(readOnly.TryPeek(out _));
            Assert.IsTrue(readOnly.TryCopyTo(Span<int>.Empty));
            Assert.IsFalse(readOnly.TryCopyTo(new int[1]));
            Assert.DoesNotThrow(() => readOnly.CopyTo(Span<int>.Empty));
        }

        [Test]
        public unsafe void ReadOnly_WrappedQueue_CopiesAndEnumeratesInFifoOrder()
        {
            using var queue = new SharedQueue<int>(4);
            var view = queue.AsNative();

            view.Enqueue(1);
            view.Enqueue(2);
            view.Enqueue(3);
            view.Enqueue(4);
            view.Dequeue();
            view.Dequeue();
            view.Enqueue(5);
            view.Enqueue(6);

            var readOnly = view.AsReadOnly();
            var visited = new List<int>();

            foreach (var item in readOnly)
            {
                visited.Add(item);
            }

            var copy = new int[3];

            readOnly.CopyTo(1, copy);

            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6 }, visited);
            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6 }, readOnly.ToArray());
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, copy);
            Assert.AreEqual(3, readOnly.Peek());
        }

        [Test]
        public unsafe void ReadOnly_ElementShapes_RoundTripBitExact()
        {
            using var small = new SharedQueue<Rgb>(new[] { new Rgb { r = 1, g = 2, b = 3 } }.AsSpan());
            using var wide = new SharedQueue<Wide>(
                  new[] { new Wide { a = long.MinValue, b = 0, c = long.MaxValue } }.AsSpan()
            );
            var smallView = small.AsNative().AsReadOnly();
            var wideView = wide.AsNative().AsReadOnly();

            Assert.AreEqual(3, smallView.Peek().b);
            Assert.AreEqual(2, smallView.ToArray()[0].g);
            Assert.AreEqual(long.MinValue, wideView.Peek().a);

            var copy = new Wide[1];
            wideView.CopyTo(copy);

            Assert.AreEqual(long.MaxValue, copy[0].c);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ReadOnly_Members_AfterOwnerDispose_Throw()
        {
            var queue = new SharedQueue<int>(new[] { 1, 2, 3 }.AsSpan());
            var readOnly = queue.AsNative().AsReadOnly();

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
            SharedQueueNative<int>.ReadOnly notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new SharedQueueNative<int>.Enumerator(null, in notCreated));
        }

        [Test]
        public unsafe void Enumerator_EmptyQueue_YieldsNothingForEveryEnumeratorForm()
        {
            using var queue = new SharedQueue<int>(4);
            var view = queue.AsNative();

            var structEnumerator = view.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)view).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)view).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [Test]
        public unsafe void Enumerator_ModifiedQueue_MoveNextAndResetThrow()
        {
            using var queue = new SharedQueue<int>(4);
            queue.Enqueue(1);
            queue.Enqueue(2);
            var view = queue.AsNative();
            var enumerator = view.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            view.Enqueue(3);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
        }

        [Test]
        public unsafe void Enumerator_Exhausted_StaysFinishedUntilReset()
        {
            using var queue = new SharedQueue<int>(new[] { 1, 2 }.AsSpan());
            var enumerator = queue.AsNative().GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
        }

        private static unsafe void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            using var queue = new SharedQueue<T>(4);
            var view = queue.AsNative();

            view.Enqueue(a);
            view.Enqueue(in b);
            view.Enqueue(c);

            CollectionAssert.AreEqual(new[] { a, b, c }, queue.ToArray(), name);
            Assert.AreEqual(a, view.Peek(), name);
            Assert.AreEqual(a, view.Dequeue(), name);

            var copy = new T[2];
            view.CopyTo(copy);

            CollectionAssert.AreEqual(new[] { b, c }, copy, name);
            Assert.IsTrue(view.TryDequeue(out var second), name);
            Assert.AreEqual(b, second, name);
            Assert.IsTrue(view.TryPeek(out var third), name);
            Assert.AreEqual(c, third, name);
            Assert.AreEqual(c, view.AsReadOnly().Peek(), name);
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
