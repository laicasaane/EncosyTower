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
    // Adapted from System.Collections.Generic.Stack<T> semantics.
    public partial class StackNativeTests
    {
        [Test]
        public void ConstructorsAndInPush_CreateExpectedContent()
        {
            using var capacity = new StackNative<int>(4, Allocator.Temp);
            using var source = new StackNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var four = 4;

            source.Push(in four);

            Assert.IsTrue(capacity.IsCreated);
            Assert.AreEqual(0, capacity.Count);
            Assert.GreaterOrEqual(capacity.Capacity, 4);
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, source.ToArray());
        }

        [Test]
        public void PushAndPop_PreserveLifoOrder()
        {
            using var stack = new StackNative<int>(4, Allocator.Temp);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);

            Assert.AreEqual(3, stack.Pop());
            Assert.AreEqual(2, stack.Pop());
            Assert.AreEqual(1, stack.Pop());
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void EmptyStack_PopAndPeekThrowWhileTryMethodsReturnFalse()
        {
            using var stack = new StackNative<int>(0, Allocator.Temp);

            Assert.Catch(() => stack.Pop());
            Assert.Catch(() => stack.Peek());
            Assert.IsFalse(stack.TryPop(out var popped));
            Assert.IsFalse(stack.TryPeek(out var peeked));
            Assert.AreEqual(0, popped);
            Assert.AreEqual(0, peeked);
        }

        [Test]
        public void Push_GrowsPastInitialCapacity()
        {
            const int N = 200;
            using var stack = new StackNative<int>(2, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                stack.Push(i);
            }

            Assert.AreEqual(N, stack.Count);

            for (var i = N - 1; i >= 0; i--)
            {
                Assert.AreEqual(i, stack.Pop());
            }
        }

        [Test]
        public void PushRange_AppendsValuesWithLastValueOnTop()
        {
            using var stack = new StackNative<int>(2, Allocator.Temp);
            stack.Push(1);

            stack.PushRange(new[] { 2, 3, 4 });

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void PeekAndTryOperations_ObserveTopWithoutRemovingIt()
        {
            using var stack = new StackNative<int>(2, Allocator.Temp);
            stack.Push(10);
            stack.Push(20);

            Assert.AreEqual(20, stack.Peek());
            Assert.IsTrue(stack.TryPeek(out var peeked));
            Assert.AreEqual(20, peeked);
            Assert.AreEqual(2, stack.Count);

            Assert.IsTrue(stack.TryPop(out var popped));
            Assert.AreEqual(20, popped);
            Assert.AreEqual(10, stack.Peek());
        }

        [Test]
        public void ToArrayAndCopyTo_ReturnTopFirstOrder()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var full = new int[5];
            var partial = new int[3];

            stack.CopyTo(full);
            stack.CopyTo(1, partial);

            CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 }, stack.ToArray());
            CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 }, full);
            CollectionAssert.AreEqual(new[] { 4, 3, 2 }, partial);
        }

        [Test]
        public void CopyToAndTryCopyTo_AllOverloadsUseTopFirstOrder()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var full = new int[4];
            var partial = new int[2];
            var offset = new int[2];
            var explicitLength = new int[3];

            stack.CopyTo(full);
            stack.CopyTo(partial.AsSpan(), 2);
            stack.CopyTo(1, offset);
            stack.CopyTo(1, explicitLength, 2);

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, full);
            CollectionAssert.AreEqual(new[] { 4, 3 }, partial);
            CollectionAssert.AreEqual(new[] { 3, 2 }, offset);
            CollectionAssert.AreEqual(new[] { 3, 2, 0 }, explicitLength);

            Assert.IsTrue(stack.TryCopyTo(full));
            Assert.IsTrue(stack.TryCopyTo(partial, 2));
            Assert.IsTrue(stack.TryCopyTo(1, offset));
            Assert.IsTrue(stack.TryCopyTo(1, explicitLength, 2));
            Assert.IsFalse(stack.TryCopyTo(3, partial));
        }

        [Test]
        public void ReadOnly_ConversionPropertiesPeekCopiesAndEnumerationExposeTopFirstValues()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            StackNative<int>.ReadOnly readOnly = stack;
            var direct = stack.AsReadOnly();
            var full = new int[4];
            var partial = new int[2];
            var offset = new int[2];
            var explicitLength = new int[3];

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(readOnly.Count, direct.Count);
            Assert.AreEqual(4, readOnly.Count);
            Assert.GreaterOrEqual(readOnly.Capacity, 4);
            Assert.AreEqual(4, readOnly.Peek());
            Assert.IsTrue(readOnly.TryPeek(out var peeked));
            Assert.AreEqual(4, peeked);
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, readOnly.ToArray());

            readOnly.CopyTo(full);
            readOnly.CopyTo(partial.AsSpan(), 2);
            readOnly.CopyTo(1, offset);
            readOnly.CopyTo(1, explicitLength, 2);

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, full);
            CollectionAssert.AreEqual(new[] { 4, 3 }, partial);
            CollectionAssert.AreEqual(new[] { 3, 2 }, offset);
            CollectionAssert.AreEqual(new[] { 3, 2, 0 }, explicitLength);
            Assert.IsTrue(readOnly.TryCopyTo(full));
            Assert.IsTrue(readOnly.TryCopyTo(partial, 2));
            Assert.IsTrue(readOnly.TryCopyTo(1, offset));
            Assert.IsTrue(readOnly.TryCopyTo(1, explicitLength, 2));
            Assert.IsFalse(readOnly.TryCopyTo(3, partial));

            var enumerator = readOnly.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Current);
            enumerator.Reset();
            Assert.IsTrue(enumerator.MoveNext());
            enumerator.Dispose();

            stack.Clear();
            Assert.IsFalse(readOnly.TryPeek(out peeked));
            Assert.AreEqual(0, peeked);
        }

        [Test]
        public void CapacityAndOwnerEnumeratorMembers_WorkDirectly()
        {
            using var stack = new StackNative<int>(new[] { 1, 2 }, Allocator.Temp);
            var initialCapacity = stack.Capacity;

            stack.EnsureCapacity(initialCapacity + 2);
            Assert.GreaterOrEqual(stack.Capacity, initialCapacity + 2);

            var ensured = stack.Capacity;
            stack.IncreaseCapacityBy(2);
            Assert.GreaterOrEqual(stack.Capacity, ensured + 2);

            var increased = stack.Capacity;
            stack.IncreaseCapacityTo(increased + 2);
            Assert.GreaterOrEqual(stack.Capacity, increased + 2);

            stack.Trim();
            Assert.AreEqual(stack.Count, stack.Capacity);

            var enumerator = stack.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);
            enumerator.Reset();
            Assert.IsTrue(enumerator.MoveNext());
            enumerator.Dispose();
        }

        [Test]
        public void Clear_ResetsCountAndAllowsReuse()
        {
            using var stack = new StackNative<int>(4, Allocator.Temp);
            stack.PushRange(new[] { 1, 2, 3 });

            stack.Clear();

            Assert.AreEqual(0, stack.Count);
            Assert.IsFalse(stack.TryPeek(out _));

            stack.Push(4);

            Assert.AreEqual(4, stack.Pop());
        }

        [Test]
        public void Enumerator_VisitsValuesFromTopToBottom()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var visited = new int[stack.Count];
            var index = 0;

            foreach (var value in stack)
            {
                visited[index++] = value;
            }

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, visited);
        }

        [Test]
        public void Enumerator_MutationDuringIteration_Throws()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var enumerator = stack.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());

            stack.Push(4);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var stack = new StackNative<int>(4, Allocator.Temp);
            Assert.IsTrue(stack.IsCreated);

            stack.Dispose();

            Assert.IsFalse(stack.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void UseAfterDispose_Throws()
        {
            var stack = new StackNative<int>(4, Allocator.Temp);
            stack.Push(1);
            stack.Dispose();

            Assert.Catch(() => _ = stack.Count);
        }

        [Test]
        public void DoubleDispose_DoesNotThrow()
        {
            var stack = new StackNative<int>(4, Allocator.Temp);
            stack.Dispose();

            Assert.DoesNotThrow(() => stack.Dispose());
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var stack = new StackNative<int>(4, Allocator.TempJob);
            stack.Push(1);

            var handle = stack.Dispose(default);

            Assert.IsFalse(stack.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAddsAndDisposes(Allocator allocator)
        {
            var stack = new StackNative<int>(4, allocator);

            try
            {
                stack.Push(10);

                Assert.IsTrue(stack.IsCreated);
                Assert.AreEqual(1, stack.Count);
                Assert.AreEqual(10, stack.Peek());
            }
            finally
            {
                stack.Dispose();
            }

            Assert.IsFalse(stack.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_InvalidArguments_Throw()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new StackNative<int>(-1, Allocator.Temp));
            Assert.Throws<InvalidOperationException>(() => _ = new StackNative<int>(4, default(AllocatorStrategy)));
            Assert.Throws<InvalidOperationException>(
                () => _ = new StackNative<int>(new[] { 1 }, default(AllocatorStrategy))
            );
        }

        [Test]
        public void SpanConstructor_EmptySource_CreatesAnEmptyStackThatCanGrow()
        {
            using var stack = new StackNative<int>(ReadOnlySpan<int>.Empty, Allocator.Temp);

            Assert.IsTrue(stack.IsCreated);
            Assert.AreEqual(0, stack.Count);

            stack.Push(1);

            CollectionAssert.AreEqual(new[] { 1 }, stack.ToArray());
        }

        [Test]
        public void Push_FullStack_GrowsAndKeepsLifoOrder()
        {
            var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            try
            {
                Assert.AreEqual(stack.Count, stack.Capacity);

                stack.Push(5);

                CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 }, stack.ToArray());
                Assert.AreEqual(5, stack.Peek());
            }
            finally
            {
                stack.Dispose();
            }
        }

        [Test]
        public void Push_ZeroCapacityStack_GrowsOnTheFirstItem()
        {
            using var stack = new StackNative<int>(0, Allocator.Temp);
            var second = 2;

            stack.Push(1);
            stack.Push(in second);

            CollectionAssert.AreEqual(new[] { 2, 1 }, stack.ToArray());
        }

        [Test]
        public void PushRange_EmptyItemsOrZeroCapacity_BehaveLikeAnEmptyAppend()
        {
            using var stack = new StackNative<int>(0, Allocator.Temp);

            stack.PushRange(ReadOnlySpan<int>.Empty);

            Assert.AreEqual(0, stack.Count);

            stack.PushRange(new[] { 1, 2, 3 });
            stack.PushRange(ReadOnlySpan<int>.Empty);

            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void PushRange_ExactlyFillsTheCapacityThenGrows_KeepsLifoOrder()
        {
            var stack = new StackNative<int>(3, Allocator.Temp);

            try
            {
                stack.PushRange(new[] { 1, 2, 3 });

                Assert.AreEqual(stack.Count, stack.Capacity);
                Assert.AreEqual(3, stack.Pop());

                stack.PushRange(new[] { 4, 5 });

                CollectionAssert.AreEqual(new[] { 5, 4, 2, 1 }, stack.ToArray());
            }
            finally
            {
                stack.Dispose();
            }
        }

        [Test]
        public void TryPop_LastElement_LeavesAnEmptyStackThatCanBeRefilled()
        {
            using var stack = new StackNative<int>(new[] { 1 }, Allocator.Temp);

            Assert.IsTrue(stack.TryPop(out var value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(0, stack.Count);
            Assert.IsFalse(stack.TryPop(out _));

            stack.Push(2);
            stack.Push(3);

            CollectionAssert.AreEqual(new[] { 3, 2 }, stack.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void PopAndPeek_EmptyStack_ThrowInvalidOperation()
        {
            using var stack = new StackNative<int>(4, Allocator.Temp);
            var readOnly = stack.AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => stack.Pop());
            Assert.Throws<InvalidOperationException>(() => stack.Peek());
            Assert.Throws<InvalidOperationException>(() => readOnly.Peek());
            Assert.IsFalse(readOnly.TryPeek(out var peeked));
            Assert.AreEqual(0, peeked);
        }

        [Test]
        public void EnsureCapacity_NotLargerThanCapacity_ReturnsTheCurrentCapacity()
        {
            using var stack = new StackNative<int>(4, Allocator.Temp);
            stack.PushRange(new[] { 1, 2 });

            Assert.AreEqual(4, stack.EnsureCapacity(0));
            Assert.AreEqual(4, stack.EnsureCapacity(4));
            Assert.AreEqual(4, stack.Capacity);
            CollectionAssert.AreEqual(new[] { 2, 1 }, stack.ToArray());
        }

        [Test]
        public void EnsureCapacity_ZeroCapacityStack_AllocatesAtLeastTheRequestedCapacity()
        {
            using var stack = new StackNative<int>(0, Allocator.Temp);

            var capacity = stack.EnsureCapacity(6);

            Assert.GreaterOrEqual(capacity, 6);
            Assert.AreEqual(capacity, stack.Capacity);
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void EnsureCapacity_NegativeSize_Throws()
        {
            using var stack = new StackNative<int>(new[] { 1, 2 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => stack.EnsureCapacity(-1));
            CollectionAssert.AreEqual(new[] { 2, 1 }, stack.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_ZeroAmount_KeepsCapacityAndContent()
        {
            using var stack = new StackNative<int>(4, Allocator.Temp);
            stack.PushRange(new[] { 1, 2 });

            Assert.AreEqual(4, stack.IncreaseCapacityBy(0));
            Assert.AreEqual(4, stack.IncreaseCapacityTo(4));
            CollectionAssert.AreEqual(new[] { 2, 1 }, stack.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_FullStack_GrowsAndKeepsLifoOrder()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            var capacity = stack.IncreaseCapacityBy(4);

            Assert.GreaterOrEqual(capacity, 8);
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
            Assert.AreEqual(4, stack.Peek());
        }

        [Test]
        public void IncreaseCapacityBy_AmountOverflowingInt_ThrowsOverflow()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<OverflowException>(() => stack.IncreaseCapacityBy(int.MaxValue));
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IncreaseCapacity_BelowCount_Throws()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => stack.IncreaseCapacityBy(-5));
            Assert.Throws<InvalidOperationException>(() => stack.IncreaseCapacityTo(2));
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void Trim_EmptyStack_ReleasesTheCapacityAndStaysUsable()
        {
            using var stack = new StackNative<int>(8, Allocator.Temp);

            stack.Trim();

            Assert.IsTrue(stack.IsCreated);
            Assert.AreEqual(0, stack.Capacity);

            stack.Push(3);

            CollectionAssert.AreEqual(new[] { 3 }, stack.ToArray());
        }

        [Test]
        public void Trim_FullAndPartiallyFilledStack_KeepContent()
        {
            using var full = new StackNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            using var partial = new StackNative<int>(8, Allocator.Temp);
            partial.PushRange(new[] { 1, 2, 3 });

            full.Trim();
            partial.Trim();

            Assert.AreEqual(3, full.Capacity);
            Assert.AreEqual(3, partial.Capacity);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, full.ToArray());
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, partial.ToArray());
        }

        [Test]
        public void Clear_EmptyStackAndToArray_AreNoOpsThatKeepTheStackUsable()
        {
            using var stack = new StackNative<int>(0, Allocator.Temp);

            stack.Clear();

            Assert.IsTrue(stack.IsCreated);
            Assert.AreEqual(0, stack.Count);
            Assert.AreEqual(0, stack.ToArray().Length);

            stack.Push(1);

            CollectionAssert.AreEqual(new[] { 1 }, stack.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void CopyTo_InvalidRange_Throws()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
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
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
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
            using var empty = new StackNative<int>(4, Allocator.Temp);
            using var stack = new StackNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            empty.CopyTo(Span<int>.Empty);
            stack.CopyTo(Span<int>.Empty);
            stack.CopyTo(3, Span<int>.Empty);

            Assert.IsTrue(empty.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(stack.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(stack.TryCopyTo(3, Span<int>.Empty));
            Assert.IsFalse(empty.TryCopyTo(new int[1]));
        }

        [Test]
        public void CopyTo_EverySourceStart_MatchesTheTopFirstSlice()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var expected = new[] { 5, 4, 3, 2, 1 };

            for (var start = 0; start <= expected.Length; start++)
            {
                var destination = new int[expected.Length - start];

                stack.CopyTo(start, destination);

                CollectionAssert.AreEqual(expected.AsSpan(start).ToArray(), destination, $"start {start}");
            }

            var partial = new int[2];

            stack.CopyTo(1, partial, 2);

            CollectionAssert.AreEqual(new[] { 4, 3 }, partial);
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
            var stack = new StackNative<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var value = 9;

            stack.Dispose();

            Assert.Catch(() => _ = stack.Count);
            Assert.Catch(() => _ = stack.Capacity);
            Assert.Catch(() => stack.Push(1));
            Assert.Catch(() => stack.Push(in value));
            Assert.Catch(() => stack.PushRange(new[] { 1 }));
            Assert.Catch(() => stack.EnsureCapacity(8));
            Assert.Catch(() => stack.IncreaseCapacityBy(1));
            Assert.Catch(() => stack.IncreaseCapacityTo(8));
            Assert.Catch(() => stack.Trim());
            Assert.Catch(() => stack.Pop());
            Assert.Catch(() => stack.TryPop(out _));
            Assert.Catch(() => stack.Peek());
            Assert.Catch(() => stack.TryPeek(out _));
            Assert.Catch(() => stack.Clear());
            Assert.Catch(() => stack.ToArray());
            Assert.Catch(() => stack.CopyTo(new int[3]));
            Assert.Catch(() => stack.CopyTo(0, new int[3], 1));
            Assert.Catch(() => stack.TryCopyTo(new int[3]));
            Assert.Catch(() => stack.TryCopyTo(0, new int[3], 1));
            Assert.Catch(() => stack.GetEnumerator());
            Assert.Catch(() => stack.AsReadOnly());
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_AcceptedAllocator_MarksNotCreatedAndIsRepeatable(Allocator allocator)
        {
            var stack = new StackNative<int>(4, allocator);
            stack.Push(1);

            stack.Dispose();

            Assert.IsFalse(stack.IsCreated);
            Assert.DoesNotThrow(() => stack.Dispose());
        }

        [Test]
        public void Dispose_DefaultStack_IsNoOp()
        {
            StackNative<int> stack = default;

            Assert.DoesNotThrow(() => stack.Dispose());

            var handle = stack.Dispose(default);

            Assert.DoesNotThrow(() => handle.Complete());
            Assert.IsFalse(stack.IsCreated);
        }

        [Test]
        public void DisposeJob_PersistentAllocator_CompletesAndMarksNotCreated()
        {
            var stack = new StackNative<int>(4, Allocator.Persistent);
            stack.Push(1);

            var handle = stack.Dispose(default);

            Assert.IsFalse(stack.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void NativeDispose_NullHeader_IsNoOp()
        {
            Assert.DoesNotThrow(() => default(StackNativeDispose).Dispose());
        }

        [Test]
        public unsafe void NativeDispose_AllocatedHeader_ReleasesIt()
        {
            var data = StackUnsafe<int>.Alloc(4, Allocator.Persistent);
            data->Push(1);

            Assert.AreEqual(1, data->Count);

            var dispose = new StackNativeDispose { m_Data = data };
            dispose.Dispose();
        }

        [Test]
        public void ReadOnly_Constructor_FromDefaultStack_Throws()
        {
            StackNative<int> notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new StackNative<int>.ReadOnly(notCreated));
            Assert.Throws<ArgumentException>(() => notCreated.AsReadOnly());
            Assert.Throws<ArgumentException>(() => _ = (StackNative<int>.ReadOnly)notCreated);
        }

        [Test]
        public void ReadOnly_EmptyStack_ReturnsEmptyResults()
        {
            using var stack = new StackNative<int>(4, Allocator.Temp);
            var readOnly = stack.AsReadOnly();
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
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = stack.AsReadOnly();
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
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = stack.AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.IsFalse(readOnly.TryCopyTo(-1, destination));
            Assert.IsFalse(readOnly.TryCopyTo(5, destination));
            Assert.IsFalse(readOnly.TryCopyTo(1, destination, 4));
            Assert.IsFalse(readOnly.TryCopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void ReadOnly_EverySourceStart_CopiesTheTopFirstSlice()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var readOnly = stack.AsReadOnly();
            var expected = new[] { 5, 4, 3, 2, 1 };

            for (var start = 0; start <= expected.Length; start++)
            {
                var destination = new int[expected.Length - start];

                readOnly.CopyTo(start, destination);

                CollectionAssert.AreEqual(expected.AsSpan(start).ToArray(), destination, $"start {start}");
            }

            Assert.IsTrue(readOnly.TryPeek(out var top));
            Assert.AreEqual(5, top);
        }

        [Test]
        public void ReadOnly_ElementShapes_RoundTripBitExact()
        {
            using var small = new StackNative<Rgb>(new[] { new Rgb { r = 1, g = 2, b = 3 } }, Allocator.Temp);
            using var wide = new StackNative<Wide>(
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
            var stack = new StackNative<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var readOnly = stack.AsReadOnly();

            stack.Dispose();

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
            StackNative<int>.ReadOnly notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new StackNative<int>.Enumerator(null, in notCreated));
        }

        [Test]
        public void Enumerator_EmptyStack_YieldsNothingForEveryEnumeratorForm()
        {
            using var stack = new StackNative<int>(4, Allocator.Temp);

            var structEnumerator = stack.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)stack).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)stack).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [Test]
        public void Enumerator_ModifiedStack_MoveNextAndResetThrow()
        {
            using var stack = new StackNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var enumerator = stack.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            stack.Clear();

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
        }

        [Test]
        public void Enumerator_Exhausted_StaysFinishedUntilReset()
        {
            using var stack = new StackNative<int>(new[] { 1, 2 }, Allocator.Temp);
            var enumerator = stack.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var stack = new StackNative<T>(1, Allocator.Temp);

            try
            {
                stack.Push(a);
                stack.Push(in b);
                stack.PushRange(new[] { c });

                CollectionAssert.AreEqual(new[] { c, b, a }, stack.ToArray(), name);
                Assert.AreEqual(c, stack.Peek(), name);
                Assert.AreEqual(c, stack.Pop(), name);

                var copy = new T[2];
                stack.CopyTo(copy);

                CollectionAssert.AreEqual(new[] { b, a }, copy, name);
                Assert.IsTrue(stack.TryPop(out var second), name);
                Assert.AreEqual(b, second, name);
                Assert.IsTrue(stack.TryPeek(out var third), name);
                Assert.AreEqual(a, third, name);
                Assert.AreEqual(a, stack.AsReadOnly().Peek(), name);
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

#endif
