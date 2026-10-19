using System;
using System.Collections.Generic;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections
{
    public class SharedArraySetNativeTests
    {
        [Test]
        public void NativeAndReadOnlyViews_ReflectOwnerMutations()
        {
            using var owner = new SharedArraySet<int>(8);
            owner.Add(1);

            // SAFETY: owner remains live while all native and read-only aliases are used.
            unsafe
            {
                var view = owner.AsNative();
                Assert.IsTrue(view.Add(2));
                Assert.IsFalse(view.Add(2));

                var readOnly = view.AsReadOnly();
                SharedArraySetNative<int>.ReadOnly converted = view;

                Assert.IsTrue(view.IsCreated);
                Assert.AreEqual(2, owner.Count);
                Assert.IsTrue(owner.Contains(2));
                Assert.IsTrue(readOnly.Contains(1));
                Assert.IsTrue(converted.Contains(2));

                view.Remove(1);

                Assert.IsFalse(owner.Contains(1));
                Assert.AreEqual(1, readOnly.Count);
            }
        }

        [Test]
        public void NativeSetOperations_MutateOwnerStorage()
        {
            using var targetOwner = CreateSet(1, 2, 3);
            using var otherOwner = CreateSet(2, 3, 4);
            // SAFETY: Both owners remain live and stable while their native aliases interact.
            unsafe
            {
                var target = targetOwner.AsNative();
                var other = otherOwner.AsNative();

                target.Intersect(in other);

                Assert.AreEqual(2, targetOwner.Count);
                Assert.IsTrue(targetOwner.Contains(2));
                Assert.IsTrue(targetOwner.Contains(3));

                target.Exclude(in other);

                Assert.AreEqual(0, targetOwner.Count);

                target.Union(in other);

                Assert.AreEqual(3, targetOwner.Count);
                Assert.IsTrue(targetOwner.Contains(4));
            }
        }

        [Test]
        public void NativeEnumerationAndCopy_UseCurrentOwnerContents()
        {
            using var owner = CreateSet(7, 9);
            Span<int> copied = stackalloc int[2];

            // SAFETY: owner remains live and unmodified while aliases and enumeration are consumed.
            unsafe
            {
                var view = owner.AsNative();
                view.AsReadOnly().CopyTo(copied);

                CollectionAssert.AreEquivalent(new[] { 7, 9 }, copied.ToArray());

                var enumerated = new List<int>();

                foreach (var value in view)
                {
                    enumerated.Add(value);
                }

                CollectionAssert.AreEquivalent(new[] { 7, 9 }, enumerated);
            }
        }

        private static SharedArraySet<int> CreateSet(params int[] values)
        {
            var set = new SharedArraySet<int>(Math.Max(values.Length, 1));

            foreach (var value in values)
            {
                set.Add(value);
            }

            return set;
        }

        [Test]
        public unsafe void Constructor_DefaultMapView_IsNotCreated()
        {
            SharedArrayMapNative<int, int> notCreatedMap = default;
            var set = new SharedArraySetNative<int>(in notCreatedMap);

            Assert.IsFalse(set.IsCreated);
            Assert.IsFalse(set.AsReadOnly().IsCreated);
            Assert.IsFalse(set.AsMap().IsCreated);
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        [Test]
        public unsafe void ReadOnlyConstructor_NullHeader_IsNotCreated()
        {
            var safety = AtomicSafetyHandle.Create();

            try
            {
                var readOnly = new SharedArraySetNative<int>.ReadOnly(null, safety);

                Assert.IsFalse(readOnly.IsCreated);
            }
            finally
            {
                AtomicSafetyHandle.Release(safety);
            }
        }
#endif

        [Test]
        public unsafe void Items_CountBelowCapacity_LengthIsCountAndSeesLaterWrites()
        {
            using var owner = new SharedArraySet<int>(8);
            owner.Add(10);
            owner.Add(20);
            var view = owner.AsNative();

            var items = view.Items;

            Assert.AreEqual(2, items.Length);
            Assert.GreaterOrEqual(view.Capacity, 8);
            CollectionAssert.AreEqual(new[] { 10, 20 }, items.ToArray());

            view.Add(30);

            Assert.AreEqual(3, view.Items.Length);
            Assert.AreEqual(30, view.Items[2]);
        }

        [Test]
        public unsafe void Items_ElementShapes_RoundTripBitExact()
        {
            using var small = new SharedArraySet<RgbValue>(4);
            using var wide = new SharedArraySet<WideValue>(4);
            using var longs = new SharedArraySet<long>(4);
            using var bytes = new SharedArraySet<byte>(4);
            var smallView = small.AsNative();
            var wideView = wide.AsNative();
            var longView = longs.AsNative();
            var byteView = bytes.AsNative();

            Assert.IsTrue(smallView.Add(new RgbValue { r = 1, g = 2, b = 3 }));
            Assert.IsTrue(smallView.Add(new RgbValue { r = 250, g = 251, b = 252 }));
            Assert.IsFalse(smallView.Add(new RgbValue { r = 250, g = 251, b = 252 }));
            Assert.IsTrue(wideView.Add(new WideValue { a = long.MinValue, b = -1L, c = long.MaxValue }));
            Assert.IsTrue(longView.Add(long.MaxValue));
            Assert.IsTrue(longView.Add(long.MinValue));
            Assert.IsTrue(byteView.Add(255));
            Assert.IsTrue(byteView.Add(0));

            Assert.AreEqual(2, smallView.Items.Length);
            Assert.AreEqual(3, smallView.Items[0].b);
            Assert.AreEqual(250, smallView.Items[1].r);
            Assert.IsTrue(smallView.Contains(new RgbValue { r = 1, g = 2, b = 3 }));
            Assert.IsFalse(smallView.Contains(new RgbValue { r = 1, g = 2, b = 4 }));
            Assert.AreEqual(long.MinValue, wideView.Items[0].a);
            Assert.AreEqual(long.MaxValue, wideView.Items[0].c);
            CollectionAssert.AreEqual(new[] { long.MaxValue, long.MinValue }, longView.Items.ToArray());
            CollectionAssert.AreEqual(new byte[] { 255, 0 }, byteView.Items.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Items_SliceUsedAfterOwnerDispose_Throws()
        {
            var owner = CreateSet(1, 2);
            var view = owner.AsNative();
            var items = view.Items;

            owner.Dispose();

            Assert.Catch(() => _ = items[0]);
            Assert.Catch(() => _ = view.Items);
        }

        [Test]
        public unsafe void EmptySet_Queries_ReturnEmptyResults()
        {
            using var owner = new SharedArraySet<int>(4);
            var view = owner.AsNative();
            var enumerator = view.GetEnumerator();
            var missing = 1;

            Assert.IsTrue(view.IsCreated);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, view.Count);
            Assert.AreEqual(4, view.Capacity);
            Assert.AreEqual(0, view.Items.Length);
            Assert.IsFalse(view.Contains(1));
            Assert.IsFalse(view.Contains(in missing));
            Assert.IsFalse(view.Remove(1));
            Assert.IsFalse(view.Remove(in missing));
            Assert.DoesNotThrow(() => view.Clear());
            Assert.AreEqual(0, view.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Add_ExactlyFullThenOnePast_ThrowsAndKeepsTheContent()
        {
            using var owner = CreateSet(1, 2, 3);
            var view = owner.AsNative();
            var value = 4;

            Assert.AreEqual(3, view.Count);
            Assert.IsFalse(view.Add(2));
            Assert.Throws<InvalidOperationException>(() => view.Add(4));
            Assert.Throws<InvalidOperationException>(() => view.Add(in value));
            Assert.AreEqual(3, owner.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, view.Items.ToArray());
        }

        [Test]
        public unsafe void Add_NewDuplicateAndInOverloads_ReportWhetherTheValueWasAdded()
        {
            using var owner = new SharedArraySet<int>(4);
            var view = owner.AsNative();
            var two = 2;

            Assert.IsTrue(view.Add(1));
            Assert.IsFalse(view.Add(1));
            Assert.IsTrue(view.Add(in two));
            Assert.IsFalse(view.Add(in two));
            Assert.AreEqual(2, owner.Count);
            Assert.IsTrue(owner.Contains(1));
            Assert.IsTrue(owner.Contains(2));
        }

        [Test]
        public unsafe void Remove_FirstValue_MovesTheLastValueIntoTheHole()
        {
            using var owner = CreateSet(1, 2, 3, 4);
            var view = owner.AsNative();
            var two = 2;

            Assert.IsTrue(view.Remove(1));

            CollectionAssert.AreEqual(new[] { 4, 2, 3 }, view.Items.ToArray());
            Assert.IsFalse(view.Contains(1));
            Assert.IsTrue(view.Contains(4));
            Assert.IsTrue(view.Remove(in two));
            Assert.IsFalse(view.Remove(in two));
            Assert.AreEqual(2, owner.Count);
        }

        [Test]
        public unsafe void Remove_LastRemainingValue_LeavesAnEmptySetThatAcceptsNewValues()
        {
            using var owner = new SharedArraySet<int>(2);
            owner.Add(1);
            var view = owner.AsNative();

            Assert.IsTrue(view.Remove(1));
            Assert.AreEqual(0, view.Count);
            Assert.IsTrue(view.Add(1));
            Assert.IsTrue(view.Add(2));
            Assert.AreEqual(2, owner.Count);
        }

        [Test]
        public unsafe void SetOperations_EmptyOperands_ProduceTheSetResult()
        {
            using var owner = CreateSet(1, 2);
            using var emptyOwner = new SharedArraySet<int>(4);
            var view = owner.AsNative();
            var empty = emptyOwner.AsNative();

            view.Exclude(in empty);
            view.Union(in empty);

            Assert.AreEqual(2, view.Count);
            Assert.IsTrue(view.Contains(1));
            Assert.IsTrue(view.Contains(2));

            view.Intersect(in empty);

            Assert.AreEqual(0, view.Count);

            empty.Intersect(in view);
            empty.Exclude(in view);
            empty.Union(in empty);

            Assert.AreEqual(0, empty.Count);
        }

        [Test]
        public unsafe void SetOperations_DefaultOperand_Throws()
        {
            using var owner = CreateSet(1);
            var view = owner.AsNative();
            SharedArraySetNative<int> notCreated = default;

            Assert.Throws<ArgumentException>(() => view.Intersect(in notCreated));
            Assert.Throws<ArgumentException>(() => view.Exclude(in notCreated));
            Assert.Throws<ArgumentException>(() => view.Union(in notCreated));
        }

        [Test]
        public unsafe void Enumerators_FromDefaultSet_Throw()
        {
            SharedArraySetNative<int> notCreated = default;
            SharedArraySetNative<int>.ReadOnly notCreatedReadOnly = default;

            Assert.Throws<ArgumentException>(() => _ = new SharedArraySetNativeEnumerator<int>(in notCreated));
            Assert.Throws<ArgumentException>(
                () => _ = new SharedArraySetNativeReadOnlyEnumerator<int>(in notCreatedReadOnly)
            );
        }

        [Test]
        public unsafe void Enumerator_ExhaustedAndModified_BehavesLikeTheMapEnumerator()
        {
            using var owner = CreateSet(7, 8);
            var view = owner.AsNative();
            var enumerator = view.GetEnumerator();

            Assert.IsTrue(enumerator.IsValid);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(7, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(8, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(7, enumerator.Current);

            view.Remove(8);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Members_AfterOwnerDispose_Throw()
        {
            var owner = CreateSet(1, 2);
            var other = CreateSet(1);
            var view = owner.AsNative();
            var otherView = other.AsNative();
            var enumerator = view.GetEnumerator();
            var value = 9;

            try
            {
                owner.Dispose();

                Assert.Catch(() => _ = view.Capacity);
                Assert.Catch(() => _ = view.Count);
                Assert.Catch(() => _ = view.Items);
                Assert.Catch(() => view.GetEnumerator());
                Assert.Catch(() => view.Add(5));
                Assert.Catch(() => view.Add(in value));
                Assert.Catch(() => view.Clear());
                Assert.Catch(() => view.Contains(1));
                Assert.Catch(() => view.Contains(in value));
                Assert.Catch(() => view.Remove(1));
                Assert.Catch(() => view.Remove(in value));
                Assert.Catch(() => view.Intersect(in otherView));
                Assert.Catch(() => view.Exclude(in otherView));
                Assert.Catch(() => view.Union(in otherView));
                Assert.Catch(() => enumerator.MoveNext());
            }
            finally
            {
                other.Dispose();
            }
        }

        [Test]
        public unsafe void ReadOnly_EmptySet_ReturnsEmptyResults()
        {
            using var owner = new SharedArraySet<int>(4);
            var readOnly = owner.AsNative().AsReadOnly();
            var enumerator = readOnly.GetEnumerator();
            var missing = 1;

            Assert.IsTrue(readOnly.IsCreated);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, readOnly.Count);
            Assert.AreEqual(4, readOnly.Capacity);
            Assert.AreEqual(0, readOnly.Items.Length);
            Assert.IsFalse(readOnly.Contains(1));
            Assert.IsFalse(readOnly.Contains(in missing));
            Assert.IsTrue(readOnly.TryCopyTo(Span<int>.Empty));
            Assert.IsFalse(readOnly.TryCopyTo(new int[1]));
            Assert.DoesNotThrow(() => readOnly.CopyTo(Span<int>.Empty));
        }

        [Test]
        public unsafe void ReadOnly_CopyTo_EverySourceStart_CopiesTheInsertionOrderedWindow()
        {
            using var owner = CreateSet(1, 2, 3, 4, 5);
            var readOnly = owner.AsNative().AsReadOnly();
            var expected = new[] { 1, 2, 3, 4, 5 };

            for (var start = 0; start <= expected.Length; start++)
            {
                var destination = new int[expected.Length - start];

                readOnly.CopyTo(start, destination);

                CollectionAssert.AreEqual(expected.AsSpan(start).ToArray(), destination, $"start {start}");
            }

            var partial = new int[2];

            readOnly.CopyTo(1, partial, 2);

            CollectionAssert.AreEqual(new[] { 2, 3 }, partial);
        }

        [Test]
        public unsafe void ReadOnly_CopyToInvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            using var owner = CreateSet(1, 2, 3, 4);
            var readOnly = owner.AsNative().AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(-1, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(5, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(0, destination, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(0, new int[2], 3));
            Assert.IsFalse(readOnly.TryCopyTo(-1, destination));
            Assert.IsFalse(readOnly.TryCopyTo(5, destination));
            Assert.IsFalse(readOnly.TryCopyTo(0, destination, 5));
            Assert.IsFalse(readOnly.TryCopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public unsafe void ReadOnly_ElementShapes_ConvertAndCopyBitExact()
        {
            using var small = new SharedArraySet<RgbValue>(4);
            using var wide = new SharedArraySet<WideValue>(4);
            small.Add(new RgbValue { r = 1, g = 2, b = 3 });
            wide.Add(new WideValue { a = long.MinValue, b = 0, c = long.MaxValue });
            SharedArraySetNative<RgbValue> smallView = small;
            SharedArraySetNative<WideValue>.ReadOnly wideView = wide.AsNative();

            var copy = new WideValue[1];
            wideView.CopyTo(copy);

            Assert.IsTrue(smallView.IsCreated);
            Assert.AreEqual(2, smallView.Items[0].g);
            Assert.AreEqual(long.MinValue, copy[0].a);
            Assert.AreEqual(long.MaxValue, copy[0].c);
            Assert.IsTrue(wideView.Contains(new WideValue { a = long.MinValue, b = 0, c = long.MaxValue }));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ReadOnly_Members_AfterOwnerDispose_Throw()
        {
            var owner = CreateSet(1, 2, 3);
            var readOnly = owner.AsNative().AsReadOnly();
            var enumerator = readOnly.GetEnumerator();
            var value = 1;

            owner.Dispose();

            Assert.Catch(() => _ = readOnly.Capacity);
            Assert.Catch(() => _ = readOnly.Count);
            Assert.Catch(() => _ = readOnly.Items);
            Assert.Catch(() => readOnly.GetEnumerator());
            Assert.Catch(() => readOnly.CopyTo(new int[3]));
            Assert.Catch(() => readOnly.TryCopyTo(new int[3]));
            Assert.Catch(() => readOnly.Contains(1));
            Assert.Catch(() => readOnly.Contains(in value));
            Assert.Catch(() => enumerator.MoveNext());
        }

        private struct RgbValue : IEquatable<RgbValue>
        {
            public byte r;
            public byte g;
            public byte b;

            public bool Equals(RgbValue other)
                => r == other.r && g == other.g && b == other.b;

            public override bool Equals(object obj)
                => obj is RgbValue other && Equals(other);

            public override int GetHashCode()
                => r | (g << 8) | (b << 16);
        }

        private struct WideValue : IEquatable<WideValue>
        {
            public long a;
            public long b;
            public long c;

            public bool Equals(WideValue other)
                => a == other.a && b == other.b && c == other.c;

            public override bool Equals(object obj)
                => obj is WideValue other && Equals(other);

            public override int GetHashCode()
                => HashCode.Combine(a, b, c);
        }
    }
}
