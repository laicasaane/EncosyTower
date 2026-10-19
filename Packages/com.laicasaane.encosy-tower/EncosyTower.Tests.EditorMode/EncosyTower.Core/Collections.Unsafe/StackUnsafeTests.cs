using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public partial class StackUnsafeTests
    {
        [Test]
        public void Constructors_CreateEmptyAndCopiedStacksIncludingZeroCapacity()
        {
            using var stack = new StackUnsafe<int>(4, new AllocatorStrategy(Allocator.Temp));
            using var zeroCapacity = new StackUnsafe<int>(0, Allocator.Temp);
            using var copied = new StackUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.IsTrue(stack.IsCreated);
            Assert.AreEqual(0, stack.Count);
            Assert.AreEqual(4, stack.Capacity);
            Assert.IsTrue(zeroCapacity.IsCreated);
            Assert.AreEqual(0, zeroCapacity.Count);
            Assert.AreEqual(0, zeroCapacity.Capacity);
            Assert.IsTrue(copied.IsCreated);
            Assert.AreEqual(3, copied.Count);
            Assert.AreEqual(3, copied.Capacity);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, copied.ToArray());

            zeroCapacity.Push(7);

            Assert.AreEqual(1, zeroCapacity.Count);
            Assert.AreEqual(7, zeroCapacity.Peek());
        }

        [Test]
        public void Push_OverloadsGrowAndPreserveLifoOrder()
        {
            using var stack = new StackUnsafe<int>(1, Allocator.Temp);
            var second = 2;

            stack.Push(1);
            stack.Push(in second);

            for (var i = 3; i <= 20; i++)
            {
                stack.Push(i);
            }

            Assert.AreEqual(20, stack.Count);
            Assert.GreaterOrEqual(stack.Capacity, 20);

            for (var i = 20; i >= 1; i--)
            {
                Assert.AreEqual(i, stack.Pop());
            }

            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void PushRange_AppendsWithLastItemOnTop()
        {
            using var stack = new StackUnsafe<int>(1, Allocator.Temp);
            stack.Push(1);

            stack.PushRange(new[] { 2, 3, 4 });
            stack.PushRange(ReadOnlySpan<int>.Empty);

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void PopPeekAndTryMethods_HandleSuccessAndFailure()
        {
            using var stack = new StackUnsafe<int>(2, Allocator.Temp);

            Assert.IsFalse(stack.TryPop(out var emptyPop));
            Assert.IsFalse(stack.TryPeek(out var emptyPeek));
            Assert.AreEqual(0, emptyPop);
            Assert.AreEqual(0, emptyPeek);

            stack.PushRange(new[] { 10, 20 });

            Assert.AreEqual(20, stack.Peek());
            Assert.IsTrue(stack.TryPeek(out var peeked));
            Assert.AreEqual(20, peeked);
            Assert.AreEqual(2, stack.Count);
            Assert.IsTrue(stack.TryPop(out var popped));
            Assert.AreEqual(20, popped);
            Assert.AreEqual(10, stack.Pop());
            Assert.IsFalse(stack.TryPop(out emptyPop));
            Assert.AreEqual(0, emptyPop);
        }

        [Test]
        public void Clear_ResetsCountAndAllowsReuse()
        {
            using var stack = new StackUnsafe<int>(4, Allocator.Temp);
            stack.PushRange(new[] { 1, 2, 3 });

            stack.Clear();

            Assert.AreEqual(0, stack.Count);
            Assert.IsFalse(stack.TryPeek(out _));

            stack.PushRange(new[] { 4, 5 });

            CollectionAssert.AreEqual(new[] { 5, 4 }, stack.ToArray());
        }

        [Test]
        public void CopyAndTryCopy_AllOverloadsUseTopFirstOrder()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var full = new int[4];
            var partial = new int[3];
            var offset = new int[2];
            var range = new int[3];

            stack.CopyTo(full);
            stack.CopyTo(partial, 2);
            stack.CopyTo(1, offset);
            stack.CopyTo(1, range, 2);

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, full);
            CollectionAssert.AreEqual(new[] { 4, 3, 0 }, partial);
            CollectionAssert.AreEqual(new[] { 3, 2 }, offset);
            CollectionAssert.AreEqual(new[] { 3, 2, 0 }, range);
            Assert.IsTrue(stack.TryCopyTo(new int[4]));
            Assert.IsTrue(stack.TryCopyTo(new int[3], 2));
            Assert.IsTrue(stack.TryCopyTo(1, new int[3]));
            Assert.IsTrue(stack.TryCopyTo(1, new int[3], 2));

            var failedDestination = new[] { 9, 9 };
            Assert.IsFalse(stack.TryCopyTo(3, failedDestination));
            Assert.IsFalse(stack.TryCopyTo(3, failedDestination, 2));
            CollectionAssert.AreEqual(new[] { 9, 9 }, failedDestination);
        }

        [Test]
        public void CapacityMethodsGrowShrinkAndTrimWithoutChangingOrder()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            stack.EnsureCapacity(10);
            Assert.GreaterOrEqual(stack.Capacity, 10);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());

            var afterEnsure = stack.Capacity;
            stack.IncreaseCapacityBy(5);
            Assert.AreEqual(afterEnsure + 5, stack.Capacity);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());

            stack.IncreaseCapacityTo(stack.Count);
            Assert.AreEqual(stack.Count, stack.Capacity);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());

            stack.IncreaseCapacityTo(8);
            stack.Trim();

            Assert.AreEqual(stack.Count, stack.Capacity);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void ReadOnly_AllPropertiesPeekCopyAndConversionMembersWork()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = stack.AsReadOnly();
            StackUnsafe<int>.ReadOnly converted = stack;
            var full = new int[4];
            var partial = new int[3];
            var offset = new int[2];
            var range = new int[3];

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(4, readOnly.Count);
            Assert.AreEqual(4, readOnly.Capacity);
            Assert.AreEqual(4, readOnly.Peek());
            Assert.IsTrue(readOnly.TryPeek(out var peeked));
            Assert.AreEqual(4, peeked);
            Assert.AreEqual(4, converted.Peek());

            readOnly.CopyTo(full);
            readOnly.CopyTo(partial, 2);
            readOnly.CopyTo(1, offset);
            readOnly.CopyTo(1, range, 2);

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, readOnly.ToArray());
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, full);
            CollectionAssert.AreEqual(new[] { 4, 3, 0 }, partial);
            CollectionAssert.AreEqual(new[] { 3, 2 }, offset);
            CollectionAssert.AreEqual(new[] { 3, 2, 0 }, range);
            Assert.IsTrue(readOnly.TryCopyTo(new int[4]));
            Assert.IsTrue(readOnly.TryCopyTo(new int[3], 2));
            Assert.IsTrue(readOnly.TryCopyTo(1, new int[3]));
            Assert.IsTrue(readOnly.TryCopyTo(1, new int[3], 2));
            Assert.IsFalse(readOnly.TryCopyTo(3, new int[2]));
            Assert.IsFalse(readOnly.TryCopyTo(3, new int[2], 2));
        }

        [Test]
        public void ReadOnlyEmpty_TryPeekReturnsFalse()
        {
            using var stack = new StackUnsafe<int>(0, Allocator.Temp);
            var readOnly = stack.AsReadOnly();

            Assert.IsFalse(readOnly.TryPeek(out var value));
            Assert.AreEqual(0, value);
        }

        [Test]
        public void OwnerAndReadOnlyEnumerators_DirectMembersVisitTopFirstOrderAndReset()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var ownerEnumerator = stack.GetEnumerator();
            var readOnlyEnumerator = stack.AsReadOnly().GetEnumerator();

            Assert.IsTrue(ownerEnumerator.MoveNext());
            Assert.AreEqual(3, ownerEnumerator.Current);
            Assert.IsTrue(ownerEnumerator.MoveNext());
            Assert.AreEqual(2, ownerEnumerator.Current);
            ownerEnumerator.Reset();
            Assert.IsTrue(ownerEnumerator.MoveNext());
            Assert.AreEqual(3, ownerEnumerator.Current);
            ownerEnumerator.Dispose();

            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(3, readOnlyEnumerator.Current);
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(2, readOnlyEnumerator.Current);
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(1, readOnlyEnumerator.Current);
            Assert.IsFalse(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(0, readOnlyEnumerator.Current);
            readOnlyEnumerator.Reset();
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(3, readOnlyEnumerator.Current);
            readOnlyEnumerator.Dispose();
        }

        [Test]
        public void Enumerator_DoesNotInvalidateOnMutationWithinCapacity()
        {
            using var stack = new StackUnsafe<int>(8, Allocator.Temp);
            stack.PushRange(new[] { 1, 2, 3 });
            var enumerator = stack.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);

            stack.Push(4);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
            enumerator.Dispose();
        }

        [Test]
        public void Dispose_MarksNotCreatedAndDoubleDisposeIsNoOp()
        {
            var stack = new StackUnsafe<int>(4, Allocator.Temp);
            stack.Push(1);

            stack.Dispose();

            Assert.IsFalse(stack.IsCreated);
            Assert.AreEqual(0, stack.Count);
            Assert.AreEqual(0, stack.Capacity);
            Assert.DoesNotThrow(() => stack.Dispose());
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var stack = new StackUnsafe<int>(4, Allocator.TempJob);
            stack.Push(1);

            var handle = stack.Dispose(default);

            Assert.IsFalse(stack.IsCreated);
            Assert.AreEqual(0, stack.Count);
            Assert.AreEqual(0, stack.Capacity);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Validation_RejectsInvalidCapacityCopiesAndEmptyAccess()
        {
            Assert.Throws<InvalidOperationException>(
                static () => _ = new StackUnsafe<int>(-1, Allocator.Temp)
            );

            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => stack.EnsureCapacity(-1));
            Assert.Throws<InvalidOperationException>(() => stack.IncreaseCapacityTo(2));
            Assert.Throws<InvalidOperationException>(() => stack.CopyTo(4, new int[1]));

            stack.Clear();
            var readOnly = stack.AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => stack.Pop());
            Assert.Throws<InvalidOperationException>(() => stack.Peek());
            Assert.Throws<InvalidOperationException>(() => readOnly.Peek());
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAndDisposes(Allocator allocator)
        {
            var stack = new StackUnsafe<int>(4, allocator);

            try
            {
                stack.Push(1);

                Assert.IsTrue(stack.IsCreated);
                Assert.AreEqual(1, stack.Count);
            }
            finally
            {
                stack.Dispose();
            }

            Assert.IsFalse(stack.IsCreated);
            Assert.AreEqual(0, stack.Count);
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_JobHandle_AcceptedAllocator_ReleasesTheStorage(Allocator allocator)
        {
            var stack = new StackUnsafe<int>(new[] { 1, 2 }, allocator);

            var handle = stack.Dispose(default);

            Assert.IsFalse(stack.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void Dispose_DefaultStack_IsNoOp()
        {
            var stack = default(StackUnsafe<int>);

            Assert.DoesNotThrow(() => stack.Dispose());
            Assert.DoesNotThrow(() => stack.Dispose(default).Complete());
            Assert.IsFalse(stack.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_DefaultAllocatorStrategy_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new StackUnsafe<int>(4, default(AllocatorStrategy)));
            Assert.Throws<InvalidOperationException>(
                () => _ = new StackUnsafe<int>(new[] { 1 }, default(AllocatorStrategy))
            );
        }

        [Test]
        public void SpanConstructor_EmptySource_CreatesAnEmptyStackThatCanGrow()
        {
            using var stack = new StackUnsafe<int>(ReadOnlySpan<int>.Empty, Allocator.Temp);

            Assert.IsTrue(stack.IsCreated);
            Assert.AreEqual(0, stack.Count);

            stack.Push(5);

            Assert.AreEqual(5, stack.Peek());
        }

        [Test]
        public void UncreatedStack_Members_ReportAnEmptyStack()
        {
            var stack = default(StackUnsafe<int>);

            Assert.IsFalse(stack.IsCreated);
            Assert.AreEqual(0, stack.Count);
            Assert.AreEqual(0, stack.Capacity);
            Assert.IsFalse(stack.TryPop(out var popped));
            Assert.IsFalse(stack.TryPeek(out var peeked));
            Assert.AreEqual(0, popped);
            Assert.AreEqual(0, peeked);
            Assert.AreEqual(0, stack.ToArray().Length);
            Assert.DoesNotThrow(() => stack.Trim());
            Assert.DoesNotThrow(() => stack.Clear());
            Assert.IsTrue(stack.TryCopyTo(Span<int>.Empty));
            Assert.IsFalse(stack.TryCopyTo(new int[1]));
        }

        [Test]
        public void AsReadOnly_UncreatedStack_ThrowsArgumentException()
        {
            var stack = default(StackUnsafe<int>);

            Assert.Throws<ArgumentException>(() => stack.AsReadOnly());
            Assert.Throws<ArgumentException>(() => _ = (StackUnsafe<int>.ReadOnly)stack);
            Assert.Throws<ArgumentException>(() => stack.GetEnumerator());
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
        public void Push_FullStack_GrowsAndKeepsEveryElement()
        {
            var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            try
            {
                Assert.AreEqual(stack.Count, stack.Capacity);

                stack.Push(5);

                CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 }, stack.ToArray());
            }
            finally
            {
                stack.Dispose();
            }
        }

        [Test]
        public void Push_InArgumentAliasingOwnStorageOnFullStack_PushesACopy()
        {
            var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                stack.Push(in stack._buffer[0]);

                CollectionAssert.AreEqual(new[] { 1, 4, 3, 2, 1 }, stack.ToArray());
            }
            finally
            {
                stack.Dispose();
            }
        }

        [Test]
        public void PushRange_SpanOverOwnStorageOnFullStack_PushesACopy()
        {
            var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                stack.PushRange(stack._buffer.AsReadOnlySpan());

                CollectionAssert.AreEqual(new[] { 4, 3, 2, 1, 4, 3, 2, 1 }, stack.ToArray());
            }
            finally
            {
                stack.Dispose();
            }
        }

        [Test]
        public void PushRange_ExactlyFillsThenGrows_KeepsLifoOrder()
        {
            var stack = new StackUnsafe<int>(3, Allocator.Temp);

            try
            {
                stack.PushRange(new[] { 1, 2, 3 });

                Assert.AreEqual(stack.Count, stack.Capacity);

                stack.PushRange(new[] { 4 });

                CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
            }
            finally
            {
                stack.Dispose();
            }
        }

        [Test]
        public void PushRange_ZeroCapacityStack_PushesEveryItem()
        {
            using var stack = new StackUnsafe<int>(0, Allocator.Temp);

            stack.PushRange(new[] { 1, 2, 3 });

            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void TryPop_LastElement_LeavesAnEmptyStackThatCanBeRefilled()
        {
            using var stack = new StackUnsafe<int>(new[] { 1 }, Allocator.Temp);

            Assert.IsTrue(stack.TryPop(out var value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(0, stack.Count);
            Assert.IsFalse(stack.TryPop(out _));

            stack.Push(2);

            CollectionAssert.AreEqual(new[] { 2 }, stack.ToArray());
        }

        [Test]
        public void Trim_EmptyStack_ReleasesTheCapacityAndStaysUsable()
        {
            using var stack = new StackUnsafe<int>(8, Allocator.Temp);

            stack.Trim();

            Assert.IsTrue(stack.IsCreated);
            Assert.AreEqual(0, stack.Capacity);

            stack.Push(3);

            CollectionAssert.AreEqual(new[] { 3 }, stack.ToArray());
        }

        [Test]
        public void Trim_FullStack_KeepsContent()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            stack.Trim();

            Assert.AreEqual(3, stack.Capacity);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void CopyTo_InvalidRange_Throws()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<InvalidOperationException>(() => stack.CopyTo(-1, destination));
            Assert.Throws<InvalidOperationException>(() => stack.CopyTo(5, destination));
            Assert.Throws<InvalidOperationException>(() => stack.CopyTo(1, destination, 4));
            Assert.Throws<InvalidOperationException>(() => stack.CopyTo(0, destination, -1));
            Assert.Throws<InvalidOperationException>(() => stack.CopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void TryCopyTo_InvalidRange_ReturnsFalseAndKeepsDestination()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var destination = new[] { 9, 9, 9, 9 };

            Assert.IsFalse(stack.TryCopyTo(-1, destination));
            Assert.IsFalse(stack.TryCopyTo(5, destination));
            Assert.IsFalse(stack.TryCopyTo(1, destination, 4));
            Assert.IsFalse(stack.TryCopyTo(0, destination, -1));
            Assert.IsFalse(stack.TryCopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void CopyTo_EmptyStackOrEmptyDestination_CopiesNothing()
        {
            using var empty = new StackUnsafe<int>(4, Allocator.Temp);
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            empty.CopyTo(Span<int>.Empty);
            stack.CopyTo(Span<int>.Empty);
            stack.CopyTo(3, Span<int>.Empty);

            Assert.IsTrue(empty.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(stack.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(stack.TryCopyTo(3, Span<int>.Empty));
            Assert.IsFalse(empty.TryCopyTo(new int[1]));
        }

        [Test]
        public void ReadOnly_TryCopyTo_InvalidRange_ReturnsFalseAndKeepsDestination()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = stack.AsReadOnly();
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
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = stack.AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(-1, destination));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(5, destination));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(1, destination, 4));
            Assert.Throws<InvalidOperationException>(() => readOnly.CopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IncreaseCapacityBy_NegativeAmountBelowCount_Throws()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => stack.IncreaseCapacityBy(-5));
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_ZeroAmount_KeepsCapacityAndContent()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.AreEqual(4, stack.IncreaseCapacityBy(0));
            Assert.AreEqual(4, stack.Capacity);
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_AmountOverflowingInt_ThrowsOverflow()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<OverflowException>(() => stack.IncreaseCapacityBy(int.MaxValue));
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_ZeroCapacityStack_AllocatesTheRequestedCapacity()
        {
            using var stack = new StackUnsafe<int>(0, Allocator.Temp);

            var capacity = stack.IncreaseCapacityBy(6);

            Assert.AreEqual(6, capacity);
            Assert.AreEqual(6, stack.Capacity);
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void GetEnumerator_EmptyStack_YieldsNothingForEveryEnumeratorForm()
        {
            using var stack = new StackUnsafe<int>(4, Allocator.Temp);

            var structEnumerator = stack.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)stack).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)stack).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [Test]
        public void ReadOnly_Enumerator_EmptyStack_YieldsNothingForEveryEnumeratorForm()
        {
            using var stack = new StackUnsafe<int>(4, Allocator.Temp);
            var readOnly = stack.AsReadOnly();

            var structEnumerator = readOnly.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)readOnly).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)readOnly).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Alloc_AcceptedAllocator_CreatesAndFreesTheHeader(Allocator allocator)
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = StackUnsafe<int>.Alloc(4, allocator);

                try
                {
                    data->Push(1);

                    Assert.IsTrue(data->IsCreated);
                    Assert.AreEqual(1, data->Count);
                    Assert.AreEqual(1, data->Peek());
                }
                finally
                {
                    StackUnsafe<int>.Free(data);
                }
            }
        }

        [Test]
        public void Alloc_ZeroCapacity_CreatesAnEmptyHeaderThatCanGrow()
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = StackUnsafe<int>.Alloc(0, Allocator.Persistent);

                try
                {
                    Assert.IsTrue(data->IsCreated);
                    Assert.AreEqual(0, data->Capacity);

                    data->PushRange(new[] { 1, 2, 3 });

                    CollectionAssert.AreEqual(new[] { 3, 2, 1 }, data->ToArray());
                }
                finally
                {
                    StackUnsafe<int>.Free(data);
                }
            }
        }

        [Test]
        public void Alloc_SpanSource_CopiesTheSourceAndAcceptsAnEmptySource()
        {
            // SAFETY: Both headers returned by Alloc stay live until Free, which runs exactly once in finally.
            unsafe
            {
                var copy = StackUnsafe<int>.Alloc(new[] { 1, 2, 3 }, Allocator.Persistent);
                var empty = StackUnsafe<int>.Alloc(ReadOnlySpan<int>.Empty, Allocator.Persistent);

                try
                {
                    CollectionAssert.AreEqual(new[] { 3, 2, 1 }, copy->ToArray());
                    Assert.IsTrue(empty->IsCreated);
                    Assert.AreEqual(0, empty->Count);
                }
                finally
                {
                    StackUnsafe<int>.Free(copy);
                    StackUnsafe<int>.Free(empty);
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
                Assert.Throws<InvalidOperationException>(() => StackUnsafe<int>.Alloc(-1, Allocator.Temp));
            }
        }

        [Test]
        public void Free_NullPointer_DoesNothing()
        {
            // SAFETY: Free documents that a null header is ignored, so no memory is read or released.
            unsafe
            {
                Assert.DoesNotThrow(() => StackUnsafe<int>.Free(null));
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void PushRange_NonzeroOffsetOwnSpanOnFullStack_PushesOriginalSlice()
        {
            var stack = new StackUnsafe<int>(new[] { 11, 22, 33, 44 }, Allocator.Persistent);

            try
            {
                stack.PushRange(stack._buffer.AsReadOnlySpan().Slice(1, 2));

                CollectionAssert.AreEqual(new[] { 33, 22, 44, 33, 22, 11 }, stack.ToArray());
            }
            finally
            {
                stack.Dispose();
            }
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var stack = new StackUnsafe<T>(2, Allocator.Temp);

            try
            {
                stack.Push(a);
                stack.Push(in b);
                stack.PushRange(new[] { c, a });

                CollectionAssert.AreEqual(new[] { a, c, b, a }, stack.ToArray(), name);
                Assert.AreEqual(a, stack.Peek(), name);

                var copy = new T[4];
                stack.CopyTo(copy);

                CollectionAssert.AreEqual(new[] { a, c, b, a }, copy, name);
                Assert.IsTrue(stack.TryPop(out var first));
                Assert.AreEqual(a, first, name);
                Assert.AreEqual(c, stack.Pop(), name);
                Assert.AreEqual(b, stack.Pop(), name);
                Assert.AreEqual(a, stack.Pop(), name);
                Assert.AreEqual(0, stack.Count, name);
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
