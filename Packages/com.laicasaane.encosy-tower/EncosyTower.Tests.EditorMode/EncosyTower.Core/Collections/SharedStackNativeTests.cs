using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from System.Collections.Generic.Stack<T> semantics against SharedStackNative<T>.
    public partial class SharedStackNativeTests
    {
        [Test]
        public void OwnerPush_AfterViewCreationIsVisible()
        {
            using var stack = new SharedStack<int>(8);
            stack.Push(1);
            SharedStackNative<int> view;

            // SAFETY: stack remains the live owner of the borrowed native view.
            unsafe
            {
                view = stack.AsNative();
            }

            stack.Push(2);

            Assert.AreEqual(2, view.Count);
            CollectionAssert.AreEqual(new[] { 2, 1 }, view.ToArray());
        }

        [Test]
        public void DefaultImplicitConversionAndValueInOperations_ExposeSharedLifoState()
        {
            SharedStackNative<int> empty = default;
            using var stack = new SharedStack<int, int>(8);
            SharedStackNative<int> view;

            // SAFETY: stack remains the live owner of the borrowed implicit native view.
            unsafe
            {
                view = stack;
            }
            var two = 2;

            view.Push(1);
            view.Push(in two);

            Assert.IsFalse(empty.IsCreated);
            Assert.IsTrue(view.IsCreated);
            Assert.AreEqual(2, view.Count);
            Assert.GreaterOrEqual(view.Capacity, 8);
            Assert.AreEqual(2, view.Peek());
            Assert.IsTrue(view.TryPeek(out var peeked));
            Assert.AreEqual(2, peeked);
            Assert.IsTrue(view.TryPop(out var popped));
            Assert.AreEqual(2, popped);
            Assert.AreEqual(1, view.Pop());
            Assert.IsFalse(view.TryPeek(out peeked));
            Assert.IsFalse(view.TryPop(out popped));
            Assert.AreEqual(0, peeked);
            Assert.AreEqual(0, popped);
        }

        [Test]
        public void CopyToAndTryCopyTo_NativeAndReadOnlyOverloadsUseTopFirstOrder()
        {
            using var stack = new SharedStack<int>(new[] { 1, 2, 3, 4 }.AsSpan());
            SharedStackNative<int> view;

            // SAFETY: stack remains the live owner of the borrowed native view.
            unsafe
            {
                view = stack.AsNative();
            }
            var full = new int[4];
            var partial = new int[2];
            var offset = new int[2];
            var explicitLength = new int[3];

            view.CopyTo(full);
            view.CopyTo(partial.AsSpan(), 2);
            view.CopyTo(1, offset);
            view.CopyTo(1, explicitLength, 2);

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, view.ToArray());
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, full);
            CollectionAssert.AreEqual(new[] { 4, 3 }, partial);
            CollectionAssert.AreEqual(new[] { 3, 2 }, offset);
            CollectionAssert.AreEqual(new[] { 3, 2, 0 }, explicitLength);
            Assert.IsTrue(view.TryCopyTo(full));
            Assert.IsTrue(view.TryCopyTo(partial, 2));
            Assert.IsTrue(view.TryCopyTo(1, offset));
            Assert.IsTrue(view.TryCopyTo(1, explicitLength, 2));
            Assert.IsFalse(view.TryCopyTo(3, partial));

            SharedStackNative<int>.ReadOnly readOnly;

            // SAFETY: stack remains the live owner of the borrowed read-only alias.
            unsafe
            {
                readOnly = view.AsReadOnly();
            }
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

            view.Clear();
            Assert.IsFalse(readOnly.TryPeek(out peeked));
            Assert.AreEqual(0, peeked);
        }

        [Test]
        public void NativeAndReadOnlyEnumerators_MoveResetAndDisposeDirectly()
        {
            using var stack = new SharedStack<int>(new[] { 1, 2 }.AsSpan());

            // SAFETY: stack remains live and unmodified while both borrowed enumerators are consumed.
            unsafe
            {
                var view = stack.AsNative();
                var ownerEnumerator = view.GetEnumerator();

                Assert.IsTrue(ownerEnumerator.MoveNext());
                Assert.AreEqual(2, ownerEnumerator.Current);
                ownerEnumerator.Reset();
                Assert.IsTrue(ownerEnumerator.MoveNext());
                ownerEnumerator.Dispose();

                var readOnlyEnumerator = view.AsReadOnly().GetEnumerator();
                Assert.IsTrue(readOnlyEnumerator.MoveNext());
                Assert.AreEqual(2, readOnlyEnumerator.Current);
                readOnlyEnumerator.Reset();
                Assert.IsTrue(readOnlyEnumerator.MoveNext());
                readOnlyEnumerator.Dispose();
            }
        }

        [Test]
        public void Clear_ResetsSharedCountAndAllowsReuse()
        {
            using var stack = new SharedStack<int>(new[] { 1, 2 }.AsSpan());
            SharedStackNative<int> view;

            // SAFETY: stack remains the live owner of the borrowed native view.
            unsafe
            {
                view = stack.AsNative();
            }

            view.Clear();
            view.Push(3);

            CollectionAssert.AreEqual(new[] { 3 }, stack.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ViewCreatedBeforeOwnerGrow_ThrowsOnNextAccess()
        {
            using var stack = new SharedStack<int>(2);
            stack.Push(1);
            stack.Push(2);
            SharedStackNative<int> staleView;

            // SAFETY: This test intentionally retains the borrowed view across owner growth.
            unsafe
            {
                staleView = stack.AsNative();
            }

            stack.Push(3);

            Assert.Catch(() => _ = staleView.Count);
        }

        [Test]
        public void ViewCreatedAfterOwnerGrow_CanMutateOwner()
        {
            using var stack = new SharedStack<int>(2);
            stack.PushRange(new[] { 1, 2, 3 });
            SharedStackNative<int> view;

            // SAFETY: stack remains live and stable after growth while the refreshed view is used.
            unsafe
            {
                view = stack.AsNative();
            }

            view.Push(4);

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void FullView_PushThrowsBecauseCapacityIsImmutable()
        {
            using var stack = new SharedStack<int>(new[] { 1, 2 }.AsSpan());
            SharedStackNative<int> view;

            // SAFETY: stack remains live while the fixed-capacity borrowed view is exercised.
            unsafe
            {
                view = stack.AsNative();
            }

            Assert.Throws<InvalidOperationException>(() => view.Push(3));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Dispose_ReleasesOwnerAndNativeHeader()
        {
            var stack = new SharedStack<int>(4);
            stack.Push(1);
            SharedStackNative<int> view;

            // SAFETY: This test intentionally retains the borrowed view across owner disposal.
            unsafe
            {
                view = stack.AsNative();
            }

            stack.Dispose();

            Assert.IsFalse(stack.IsCreated);
            Assert.Catch(() => _ = view.Count);
            Assert.DoesNotThrow(() => stack.Dispose());
        }

        [Test]
        public unsafe void Constructor_BorrowedHeader_ViewsTheHeaderAndAcceptsANullHeader()
        {
            using var storage = new SharedStackStorage<int>(4);
            var header = storage.CreateHeader();

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            var safety = AtomicSafetyHandle.Create();
            var view = new SharedStackNative<int>(&header, safety);
            var nullView = new SharedStackNative<int>(null, safety);
#else
            var view = new SharedStackNative<int>(&header);
            var nullView = new SharedStackNative<int>(null);
#endif

            try
            {
                Assert.IsTrue(view.IsCreated);
                Assert.IsFalse(nullView.IsCreated);
                Assert.AreEqual(4, view.Capacity);
                Assert.AreEqual(0, view.Count);

                view.Push(5);

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
        public unsafe void PopAndPeek_EmptyStack_ThrowWhileTryMethodsReturnFalse()
        {
            using var stack = new SharedStack<int>(4);
            var view = stack.AsNative();
            var readOnly = view.AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => view.Pop());
            Assert.Throws<InvalidOperationException>(() => view.Peek());
            Assert.Throws<InvalidOperationException>(() => readOnly.Peek());
            Assert.IsFalse(view.TryPop(out var popped));
            Assert.IsFalse(view.TryPeek(out var peeked));
            Assert.IsFalse(readOnly.TryPeek(out var readOnlyPeeked));
            Assert.AreEqual(0, popped);
            Assert.AreEqual(0, peeked);
            Assert.AreEqual(0, readOnlyPeeked);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ZeroCapacityStack_PushAndPop_Throw()
        {
            using var stack = new SharedStack<int>(0);
            var view = stack.AsNative();

            Assert.IsTrue(view.IsCreated);
            Assert.AreEqual(0, view.Capacity);
            Assert.Throws<InvalidOperationException>(() => view.Push(1));
            Assert.Throws<InvalidOperationException>(() => view.Pop());
            Assert.Throws<InvalidOperationException>(() => view.Peek());
            Assert.AreEqual(0, view.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Push_ExactlyFullThenOnePast_ThrowsAndKeepsTheContent()
        {
            using var stack = new SharedStack<int>(3);
            var view = stack.AsNative();
            var value = 5;

            view.Push(1);
            view.Push(2);
            view.Push(3);

            Assert.AreEqual(3, view.Count);
            Assert.AreEqual(3, view.Capacity);
            Assert.Throws<InvalidOperationException>(() => view.Push(4));
            Assert.Throws<InvalidOperationException>(() => view.Push(in value));
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());
        }

        [Test]
        public unsafe void Push_AfterPopFromAFullStack_ReusesTheSlot()
        {
            using var stack = new SharedStack<int>(3);
            var view = stack.AsNative();

            view.Push(1);
            view.Push(2);
            view.Push(3);

            Assert.AreEqual(3, view.Pop());

            view.Push(4);

            Assert.AreEqual(3, view.Count);
            CollectionAssert.AreEqual(new[] { 4, 2, 1 }, view.ToArray());
            Assert.AreEqual(4, view.Peek());
        }

        [Test]
        public unsafe void SingleSlotStack_RepeatedPushAndPop_KeepsTheLifoOrder()
        {
            using var stack = new SharedStack<int>(1);
            var view = stack.AsNative();

            for (var i = 0; i < 5; i++)
            {
                view.Push(i);

                Assert.AreEqual(i, view.Peek());
                Assert.IsTrue(view.TryPop(out var value));
                Assert.AreEqual(i, value);
                Assert.AreEqual(0, view.Count);
            }
        }

        [Test]
        public unsafe void Clear_EmptyAndFilledStack_ResetsTheCountAndStaysUsable()
        {
            using var stack = new SharedStack<int>(3);
            var view = stack.AsNative();

            view.Clear();

            Assert.AreEqual(0, view.Count);
            Assert.AreEqual(0, view.ToArray().Length);

            view.Push(1);
            view.Push(2);
            view.Push(3);
            view.Pop();
            view.Push(4);
            view.Clear();

            Assert.AreEqual(0, stack.Count);
            Assert.IsFalse(view.TryPeek(out _));

            view.Push(5);
            view.Push(6);

            CollectionAssert.AreEqual(new[] { 6, 5 }, view.ToArray());
        }

        [Test]
        public unsafe void CopyTo_EverySourceStart_CopiesTheTopFirstWindow()
        {
            using var stack = new SharedStack<int>(new[] { 1, 2, 3, 4, 5 }.AsSpan());
            var view = stack.AsNative();
            var expected = new[] { 5, 4, 3, 2, 1 };

            for (var start = 0; start <= expected.Length; start++)
            {
                var destination = new int[expected.Length - start];

                view.CopyTo(start, destination);

                CollectionAssert.AreEqual(expected.AsSpan(start).ToArray(), destination, $"start {start}");
            }

            var partial = new int[2];

            view.CopyTo(1, partial, 2);

            CollectionAssert.AreEqual(new[] { 4, 3 }, partial);
            CollectionAssert.AreEqual(expected, view.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void CopyTo_InvalidRange_Throws()
        {
            using var stack = new SharedStack<int>(new[] { 1, 2, 3, 4 }.AsSpan());
            var view = stack.AsNative();
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
            using var stack = new SharedStack<int>(new[] { 1, 2, 3, 4 }.AsSpan());
            var view = stack.AsNative();
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
        public unsafe void CopyTo_EmptyStackOrEmptyDestination_CopiesNothing()
        {
            using var empty = new SharedStack<int>(4);
            using var stack = new SharedStack<int>(new[] { 1, 2, 3 }.AsSpan());
            var emptyView = empty.AsNative();
            var view = stack.AsNative();

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
            var stack = new SharedStack<int>(new[] { 1, 2, 3 }.AsSpan());
            var view = stack.AsNative();
            var value = 9;

            stack.Dispose();

            Assert.Catch(() => _ = view.Count);
            Assert.Catch(() => _ = view.Capacity);
            Assert.Catch(() => view.Push(1));
            Assert.Catch(() => view.Push(in value));
            Assert.Catch(() => view.Pop());
            Assert.Catch(() => view.TryPop(out _));
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
            SharedStackNative<int> notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new SharedStackNative<int>.ReadOnly(notCreated));
        }

        [Test]
        public unsafe void ReadOnly_EmptyStack_ReturnsEmptyResults()
        {
            using var stack = new SharedStack<int>(4);
            var readOnly = stack.AsNative().AsReadOnly();
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
        public unsafe void ReadOnly_FilledStack_CopiesAndEnumeratesTopFirst()
        {
            using var stack = new SharedStack<int>(new[] { 1, 2, 3, 4 }.AsSpan());
            var readOnly = stack.AsNative().AsReadOnly();
            var visited = new List<int>();

            foreach (var item in readOnly)
            {
                visited.Add(item);
            }

            var copy = new int[3];

            readOnly.CopyTo(1, copy);

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, visited);
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, readOnly.ToArray());
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, copy);
            Assert.AreEqual(4, readOnly.Peek());
        }

        [Test]
        public unsafe void ReadOnly_ElementShapes_RoundTripBitExact()
        {
            using var small = new SharedStack<Rgb>(new[] { new Rgb { r = 1, g = 2, b = 3 } }.AsSpan());
            using var wide = new SharedStack<Wide>(
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
            var stack = new SharedStack<int>(new[] { 1, 2, 3 }.AsSpan());
            var readOnly = stack.AsNative().AsReadOnly();

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
            SharedStackNative<int>.ReadOnly notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new SharedStackNative<int>.Enumerator(null, in notCreated));
        }

        [Test]
        public unsafe void Enumerator_EmptyStack_YieldsNothingForEveryEnumeratorForm()
        {
            using var stack = new SharedStack<int>(4);
            var view = stack.AsNative();

            var structEnumerator = view.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)view).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)view).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [Test]
        public unsafe void Enumerator_ModifiedStack_MoveNextAndResetThrow()
        {
            using var stack = new SharedStack<int>(4);
            stack.Push(1);
            stack.Push(2);
            var view = stack.AsNative();
            var enumerator = view.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            view.Push(3);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
        }

        [Test]
        public unsafe void Enumerator_Exhausted_StaysFinishedUntilReset()
        {
            using var stack = new SharedStack<int>(new[] { 1, 2 }.AsSpan());
            var enumerator = stack.AsNative().GetEnumerator();

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

        private static unsafe void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            using var stack = new SharedStack<T>(4);
            var view = stack.AsNative();

            view.Push(a);
            view.Push(in b);
            view.Push(c);

            CollectionAssert.AreEqual(new[] { c, b, a }, stack.ToArray(), name);
            Assert.AreEqual(c, view.Peek(), name);
            Assert.AreEqual(c, view.Pop(), name);

            var copy = new T[2];
            view.CopyTo(copy);

            CollectionAssert.AreEqual(new[] { b, a }, copy, name);
            Assert.IsTrue(view.TryPop(out var second), name);
            Assert.AreEqual(b, second, name);
            Assert.IsTrue(view.TryPeek(out var third), name);
            Assert.AreEqual(a, third, name);
            Assert.AreEqual(a, view.AsReadOnly().Peek(), name);
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
