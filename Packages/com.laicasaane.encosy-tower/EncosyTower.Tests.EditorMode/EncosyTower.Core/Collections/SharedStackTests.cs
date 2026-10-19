using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from System.Collections.Generic.Stack<T> semantics.
    public partial class SharedStackTests
    {
        [Test]
        public void Constructors_CreateExpectedContent()
        {
            using var empty = new SharedStack<int>();
            using var capacity = new SharedStack<int>(8);
            using var span = new SharedStack<int>(new[] { 1, 2, 3 }.AsSpan());
            using var collection = new SharedStack<int>(new List<int> { 4, 5, 6 });

            Assert.AreEqual(0, empty.Count);
            Assert.AreEqual(0, capacity.Count);
            Assert.GreaterOrEqual(capacity.Capacity, 8);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, span.ToArray());
            CollectionAssert.AreEqual(new[] { 6, 5, 4 }, collection.ToArray());
        }

        [Test]
        public void GenericConstructors_CreateExpectedContent()
        {
            using var empty = new SharedStack<int, uint>();
            using var capacity = new SharedStack<int, uint>(8);
            using var span = new SharedStack<int, uint>(new[] { 1, 2, 3 }.AsSpan());
            using var collection = new SharedStack<int, uint>(new List<int> { 4, 5, 6 });

            Assert.IsTrue(empty.IsCreated);
            Assert.AreEqual(0, empty.Count);
            Assert.GreaterOrEqual(capacity.Capacity, 8);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, span.ToArray());
            CollectionAssert.AreEqual(new[] { 6, 5, 4 }, collection.ToArray());
        }

        [Test]
        public void PushAndPop_GrowAndPreserveLifoOrder()
        {
            const int N = 200;
            using var stack = new SharedStack<int>(2);

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
        public void ValueInAndTryOperations_ReturnExpectedValues()
        {
            using var stack = new SharedStack<int, int>(4);
            var two = 2;

            stack.Push(1);
            stack.Push(in two);

            Assert.AreEqual(2, stack.Peek());
            Assert.IsTrue(stack.TryPeek(out var peeked));
            Assert.AreEqual(2, peeked);
            Assert.IsTrue(stack.TryPop(out var popped));
            Assert.AreEqual(2, popped);
            Assert.AreEqual(1, stack.Pop());
            Assert.IsFalse(stack.TryPeek(out peeked));
            Assert.IsFalse(stack.TryPop(out popped));
            Assert.AreEqual(0, peeked);
            Assert.AreEqual(0, popped);
        }

        [Test]
        public void PushRange_PlacesLastValueOnTop()
        {
            using var stack = new SharedStack<int>(2);
            stack.Push(1);

            stack.PushRange(new[] { 2, 3, 4 });

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void Clear_ResetsCountAndAllowsReuse()
        {
            using var stack = new SharedStack<int>(new[] { 1, 2, 3 }.AsSpan());

            stack.Clear();

            Assert.AreEqual(0, stack.Count);
            stack.Push(4);
            Assert.AreEqual(4, stack.Pop());
        }

        [Test]
        public void Trim_ShrinksCapacityToCountWithoutChangingOrder()
        {
            using var stack = new SharedStack<int>(16);
            stack.PushRange(new[] { 1, 2, 3 });

            stack.Trim();

            Assert.AreEqual(stack.Count, stack.Capacity);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void CopyToAndTryCopyTo_AllOwnerAndReadOnlyOverloadsUseTopFirstOrder()
        {
            using var stack = new SharedStack<int, int>(new[] { 1, 2, 3, 4 }.AsSpan());
            var ownerFull = new int[4];
            var ownerPartial = new int[2];
            var ownerOffset = new int[2];
            var ownerLength = new int[3];

            stack.CopyTo(ownerFull);
            stack.CopyTo(ownerPartial.AsSpan(), 2);
            stack.CopyTo(1, ownerOffset);
            stack.CopyTo(1, ownerLength, 2);

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, ownerFull);
            CollectionAssert.AreEqual(new[] { 4, 3 }, ownerPartial);
            CollectionAssert.AreEqual(new[] { 3, 2 }, ownerOffset);
            CollectionAssert.AreEqual(new[] { 3, 2, 0 }, ownerLength);
            Assert.IsTrue(stack.TryCopyTo(ownerFull));
            Assert.IsTrue(stack.TryCopyTo(ownerPartial, 2));
            Assert.IsTrue(stack.TryCopyTo(1, ownerOffset));
            Assert.IsTrue(stack.TryCopyTo(1, ownerLength, 2));
            Assert.IsFalse(stack.TryCopyTo(3, ownerPartial));

            // SAFETY: stack remains the live owner while both borrowed read-only aliases are exercised.
            unsafe
            {
                SharedStack<int, int>.ReadOnly readOnly = stack;
                var readOnlyFull = new int[4];
                var readOnlyPartial = new int[2];
                var readOnlyOffset = new int[2];
                var readOnlyLength = new int[3];

                Assert.IsTrue(readOnly.IsCreated);
                Assert.AreEqual(4, readOnly.Count);
                Assert.GreaterOrEqual(readOnly.Capacity, 4);
                Assert.AreEqual(4, readOnly.Peek());
                Assert.IsTrue(readOnly.TryPeek(out var peeked));
                Assert.AreEqual(4, peeked);
                CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, readOnly.ToArray());

                readOnly.CopyTo(readOnlyFull);
                readOnly.CopyTo(readOnlyPartial.AsSpan(), 2);
                readOnly.CopyTo(1, readOnlyOffset);
                readOnly.CopyTo(1, readOnlyLength, 2);

                CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, readOnlyFull);
                CollectionAssert.AreEqual(new[] { 4, 3 }, readOnlyPartial);
                CollectionAssert.AreEqual(new[] { 3, 2 }, readOnlyOffset);
                CollectionAssert.AreEqual(new[] { 3, 2, 0 }, readOnlyLength);
                Assert.IsTrue(readOnly.TryCopyTo(readOnlyFull));
                Assert.IsTrue(readOnly.TryCopyTo(readOnlyPartial, 2));
                Assert.IsTrue(readOnly.TryCopyTo(1, readOnlyOffset));
                Assert.IsTrue(readOnly.TryCopyTo(1, readOnlyLength, 2));
                Assert.IsFalse(readOnly.TryCopyTo(3, readOnlyPartial));

                var nativeReadOnly = readOnly.AsNative();
                Assert.AreEqual(4, nativeReadOnly.Count);

                stack.Clear();
                Assert.IsFalse(readOnly.TryPeek(out peeked));
                Assert.AreEqual(0, peeked);
            }
        }

        [Test]
        public void CapacityConversionsEnumeratorsAndDispose_WorkDirectly()
        {
            var stack = new SharedStack<int, int>(new[] { 1, 2 }.AsSpan());

            try
            {
                var initialCapacity = stack.Capacity;
                stack.IncreaseCapacityBy(2);
                Assert.GreaterOrEqual(stack.Capacity, initialCapacity + 2);

                var increased = stack.Capacity;
                stack.IncreaseCapacityTo(increased + 2);
                Assert.GreaterOrEqual(stack.Capacity, increased + 2);

                // SAFETY: stack remains alive and unmodified while its aliases and enumerators are used.
                unsafe
                {
                    SharedStackNative<int> native = stack;
                    Assert.AreEqual(2, native.Count);

                    var ownerEnumerator = stack.GetEnumerator();
                    Assert.IsTrue(ownerEnumerator.MoveNext());
                    Assert.AreEqual(2, ownerEnumerator.Current);
                    ownerEnumerator.Reset();
                    Assert.IsTrue(ownerEnumerator.MoveNext());
                    ownerEnumerator.Dispose();

                    var readOnlyEnumerator = stack.AsReadOnly().GetEnumerator();
                    Assert.IsTrue(readOnlyEnumerator.MoveNext());
                    Assert.AreEqual(2, readOnlyEnumerator.Current);
                    readOnlyEnumerator.Reset();
                    Assert.IsTrue(readOnlyEnumerator.MoveNext());
                    readOnlyEnumerator.Dispose();
                }
            }
            finally
            {
                stack.Dispose();
            }

            Assert.IsFalse(stack.IsCreated);
            Assert.DoesNotThrow(() => stack.Dispose());
        }

        [Test]
        public void PushRange_EmptyInput_LeavesTheStackUnchanged()
        {
            using var zeroCapacity = new SharedStack<int>();
            using var stack = new SharedStack<int>(new[] { 1, 2, 3 }.AsSpan());

            zeroCapacity.PushRange(ReadOnlySpan<int>.Empty);
            zeroCapacity.PushRange(Array.Empty<int>());
            stack.PushRange(ReadOnlySpan<int>.Empty);

            Assert.AreEqual(0, zeroCapacity.Count);
            Assert.AreEqual(0, zeroCapacity.Capacity);
            Assert.AreEqual(3, stack.Count);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public void PushRange_ZeroCapacityStack_GrowsAndKeepsLifoOrder()
        {
            using var stack = new SharedStack<int>();
            var items = new int[50];
            var expected = new int[items.Length];

            for (var i = 0; i < items.Length; i++)
            {
                items[i] = i * 3;
                expected[items.Length - 1 - i] = items[i];
            }

            stack.PushRange(items);

            Assert.AreEqual(items.Length, stack.Count);
            Assert.GreaterOrEqual(stack.Capacity, items.Length);
            CollectionAssert.AreEqual(expected, stack.ToArray());
        }

        [Test]
        public void PushRange_AtEveryFillLevelAndLength_MatchesStackOfT()
        {
            const int CAPACITY = 8;

            for (var fill = 0; fill <= CAPACITY; fill++)
            {
                for (var length = 0; length <= 10; length++)
                {
                    using var stack = new SharedStack<int>(CAPACITY);
                    var model = new Stack<int>();
                    var items = new int[length];
                    var message = $"fill {fill} length {length}";

                    for (var i = 0; i < fill; i++)
                    {
                        stack.Push(-(i + 1));
                        model.Push(-(i + 1));
                    }

                    for (var i = 0; i < length; i++)
                    {
                        items[i] = 100 + i;
                        model.Push(items[i]);
                    }

                    stack.PushRange(items);

                    Assert.AreEqual(model.Count, stack.Count, message);

                    if (fill + length <= CAPACITY)
                    {
                        Assert.AreEqual(CAPACITY, stack.Capacity, message);
                    }

                    CollectionAssert.AreEqual(model.ToArray(), stack.ToArray(), message);

                    while (model.Count > 0)
                    {
                        Assert.AreEqual(model.Pop(), stack.Pop(), message);
                    }

                    Assert.AreEqual(0, stack.Count, message);
                }
            }
        }

        [Test]
        public void PushRange_ElementSizes_RoundTripBitExact()
        {
            AssertPushRangeRoundTrip(new byte[] { 0, 255, 128 });
            AssertPushRangeRoundTrip(new[] { long.MinValue, -1L, long.MaxValue });
            AssertPushRangeRoundTrip(new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 0, b = 128 } });
            AssertPushRangeRoundTrip(new[] { new Wide { a = long.MinValue, b = -1, c = long.MaxValue } });
        }

        [Test]
        public void AsNative_ViewAndOwnerShareTheStack()
        {
            using var stack = new SharedStack<int>(8);

            // SAFETY: stack outlives the native view, which is used before any resize.
            unsafe
            {
                var native = stack.AsNative();

                stack.Push(1);
                native.Push(2);

                Assert.AreEqual(2, stack.Count);
                Assert.AreEqual(2, native.Count);
                CollectionAssert.AreEqual(new[] { 2, 1 }, stack.ToArray());
                CollectionAssert.AreEqual(new[] { 2, 1 }, native.ToArray());
                Assert.AreEqual(2, native.Pop());
                Assert.AreEqual(1, stack.Count);
                Assert.AreEqual(1, stack.Peek());

                stack.Clear();

                Assert.AreEqual(0, native.Count);
            }
        }

        [Test]
        public void AsReadOnly_ViewsSeeLaterPushesAndPops()
        {
            using var stack = new SharedStack<int>(8);

            // SAFETY: stack outlives the views, which are used before any resize.
            unsafe
            {
                var view = stack.AsReadOnly();
                var nativeReadOnly = view.AsNative();

                stack.Push(5);
                stack.Push(6);

                Assert.AreEqual(2, view.Count);
                Assert.AreEqual(2, nativeReadOnly.Count);
                Assert.AreEqual(6, view.Peek());
                Assert.AreEqual(6, stack.Pop());
                Assert.AreEqual(1, nativeReadOnly.Count);
                Assert.AreEqual(5, view.Peek());
                CollectionAssert.AreEqual(new[] { 5 }, view.ToArray());
            }
        }

        [Test]
        public void GetEnumerator_EmptyStacks_YieldNothing()
        {
            using var zeroCapacity = new SharedStack<int>();
            using var withCapacity = new SharedStack<int>(4);
            var count = 0;

            // SAFETY: The stacks outlive the views and their enumerators and are not modified while they run.
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
        public void ReadOnlyAsNative_ViewSeesLaterPushes()
        {
            using var stack = new SharedStack<int>(8);

            // SAFETY: stack outlives the views, which are used before any resize.
            unsafe
            {
                var view = stack.AsReadOnly();
                var native = view.AsNative();

                Assert.IsTrue(native.IsCreated);
                Assert.AreEqual(0, native.Count);

                stack.Push(7);

                Assert.AreEqual(1, native.Count);
                CollectionAssert.AreEqual(new[] { 7 }, native.ToArray());
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Dispose_StackWithLiveNativeView_InvalidatesTheView()
        {
            var stack = new SharedStack<int>(new[] { 1, 2 }.AsSpan());

            // SAFETY: This test intentionally retains a borrowed alias across owner disposal to verify checks.
            unsafe
            {
                var native = stack.AsNative();

                stack.Dispose();

                Assert.Catch(() => _ = native.Count);
                Assert.DoesNotThrow(() => stack.Dispose());
            }
        }

        [Test]
        public void Dispose_StackState_ReportsNotCreatedAndEmpty()
        {
            var stack = new SharedStack<int>(new[] { 1, 2 }.AsSpan());
            var zeroCapacity = new SharedStack<int>();

            stack.Dispose();
            zeroCapacity.Dispose();

            Assert.IsFalse(stack.IsCreated);
            Assert.IsFalse(zeroCapacity.IsCreated);
            Assert.AreEqual(0, stack.Count);
            Assert.AreEqual(0, stack.Capacity);
            Assert.AreEqual(0, zeroCapacity.Count);
            Assert.DoesNotThrow(() => zeroCapacity.Dispose());
        }

        [Test]
        public void InternalState_FollowsTheStack()
        {
            using var stack = new SharedStack<int>(4);

            Assert.AreEqual(0, stack.CountRO);

            var version = stack.VersionRO;

            stack.Push(1);
            stack.Push(2);

            Assert.AreEqual(2, stack.CountRO);
            Assert.AreNotEqual(version, stack.VersionRO);

            version = stack.VersionRO;

            _ = stack.Peek();
            _ = stack.ToArray();
            _ = stack.Count;

            Assert.AreEqual(version, stack.VersionRO);

            stack.VersionRW++;

            Assert.AreEqual(version + 1, stack.VersionRO);

            stack.CountRW = 1;

            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(1, stack.Peek());

            stack.Clear();

            Assert.AreEqual(0, stack.CountRO);
        }

        private static void AssertPushRangeRoundTrip<T>(T[] values)
            where T : unmanaged
        {
            using var stack = new SharedStack<T>();
            var expected = (T[])values.Clone();

            Array.Reverse(expected);
            stack.PushRange(values);

            SpanBytesAssert.AreEqual(expected, stack.ToArray());

            var top = stack.Pop();

            SpanBytesAssert.AreEqual(new[] { values[^1] }, new[] { top });
            SpanBytesAssert.AreEqual(expected.AsSpan(1).ToArray(), stack.ToArray());
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
