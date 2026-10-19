using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from Unity.Collections.Tests/NativeHashSetTests.cs against the Native forwarder.
    public partial class ArraySetNativeTests
    {
        [Test]
        public void Constructor_CreatesValidSet()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);

            Assert.IsTrue(set.IsCreated);
            Assert.AreEqual(0, set.Count);
            Assert.GreaterOrEqual(set.Capacity, 4);
        }

        [Test]
        public void DefaultSet_IsNotCreated()
        {
            ArraySetNative<int> set = default;

            Assert.IsFalse(set.IsCreated);
        }

        [Test]
        public void Add_StoresUniqueValues()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);

            Assert.IsTrue(set.Add(1));
            Assert.IsTrue(set.Add(2));
            Assert.IsFalse(set.Add(1));

            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains(1));
            Assert.IsTrue(set.Contains(2));
            Assert.IsFalse(set.Contains(3));
        }

        [Test]
        public void Remove_RemovesValue()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            set.Add(1);
            set.Add(2);
            set.Add(3);

            Assert.IsTrue(set.Remove(2));
            Assert.IsFalse(set.Remove(99));

            Assert.AreEqual(2, set.Count);
            Assert.IsFalse(set.Contains(2));
            Assert.IsTrue(set.Contains(1));
            Assert.IsTrue(set.Contains(3));
        }

        [Test]
        public void Clear_ResetsCount()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            set.Add(1);
            set.Add(2);

            set.Clear();

            Assert.AreEqual(0, set.Count);
            Assert.IsFalse(set.Contains(1));

            set.Add(5);
            Assert.AreEqual(1, set.Count);
            Assert.IsTrue(set.Contains(5));
        }

        [Test]
        public void Grow_PastInitialCapacity_KeepsAllValues()
        {
            const int N = 200;
            using var set = new ArraySetNative<int>(4, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                Assert.IsTrue(set.Add(i));
            }

            Assert.AreEqual(N, set.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.IsTrue(set.Contains(i), $"missing {i}");
            }
        }

        [Test]
        public void EnsureCapacity_GrowsCapacity()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            var before = set.Capacity;

            set.EnsureCapacity(before + 100);

            Assert.Greater(set.Capacity, before);
        }

        [Test]
        public void Trim_ShrinksCapacityToCount()
        {
            using var set = new ArraySetNative<int>(64, Allocator.Temp);
            set.Add(1);
            set.Add(2);

            set.Trim();

            Assert.AreEqual(2, set.Capacity);
            Assert.IsTrue(set.Contains(1));
            Assert.IsTrue(set.Contains(2));
        }

        [Test]
        public void Enumerator_VisitsAllValues()
        {
            using var set = new ArraySetNative<int>(8, Allocator.Temp);

            for (var i = 0; i < 5; i++)
            {
                set.Add(i * 10);
            }

            var seen = new HashSet<int>();

            foreach (var value in set)
            {
                seen.Add(value);
            }

            CollectionAssert.AreEquivalent(new[] { 0, 10, 20, 30, 40 }, seen);
        }

        [Test]
        public void Enumerator_ModifyDuringIteration_Throws()
        {
            using var set = new ArraySetNative<int>(8, Allocator.Temp);
            set.Add(1);
            set.Add(2);

            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var value in set)
                {
                    set.Add(value + 100);
                }
            });
        }

        [Test]
        public void CopyConstructor_DeepCopiesContent()
        {
            using var source = new ArraySetNative<int>(8, Allocator.Temp);
            source.Add(1);
            source.Add(2);

            using var copy = new ArraySetNative<int>(source, Allocator.Temp);

            Assert.AreEqual(source.Count, copy.Count);
            Assert.IsTrue(copy.Contains(1));
            Assert.IsTrue(copy.Contains(2));

            copy.Add(3);
            Assert.IsFalse(source.Contains(3));
        }

        [Test]
        public void CopyConstructor_AfterBucketGrowth_AllValuesResolve()
        {
            const int N = 500;
            using var source = new ArraySetNative<int>(2, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                source.Add(i);
            }

            using var copy = new ArraySetNative<int>(source, Allocator.Temp);

            Assert.AreEqual(N, copy.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.IsTrue(copy.Contains(i), $"missing {i} after copy");
            }
        }

        [Test]
        public void Intersect_KeepsOnlyShared()
        {
            using var a = new ArraySetNative<int>(8, Allocator.Temp);
            using var b = new ArraySetNative<int>(8, Allocator.Temp);

            a.Add(1); a.Add(2); a.Add(3);
            b.Add(2); b.Add(3); b.Add(4);

            a.Intersect(in b);

            Assert.AreEqual(2, a.Count);
            Assert.IsFalse(a.Contains(1));
            Assert.IsTrue(a.Contains(2));
            Assert.IsTrue(a.Contains(3));
        }

        [Test]
        public void Exclude_RemovesShared()
        {
            using var a = new ArraySetNative<int>(8, Allocator.Temp);
            using var b = new ArraySetNative<int>(8, Allocator.Temp);

            a.Add(1); a.Add(2); a.Add(3);
            b.Add(2); b.Add(3);

            a.Exclude(in b);

            Assert.AreEqual(1, a.Count);
            Assert.IsTrue(a.Contains(1));
            Assert.IsFalse(a.Contains(2));
        }

        [Test]
        public void Union_MergesValues()
        {
            using var a = new ArraySetNative<int>(8, Allocator.Temp);
            using var b = new ArraySetNative<int>(8, Allocator.Temp);

            a.Add(1); a.Add(2);
            b.Add(2); b.Add(3);

            a.Union(in b);

            Assert.AreEqual(3, a.Count);
            Assert.IsTrue(a.Contains(1));
            Assert.IsTrue(a.Contains(2));
            Assert.IsTrue(a.Contains(3));
        }

        [Test]
        public void Reinterpret_ReusesUnderlyingValues()
        {
            using var set = new ArraySetNative<int>(8, Allocator.Temp);
            set.Add(1);

            var reinterpreted = set.Reinterpret<uint>();

            Assert.AreEqual(set.Count, reinterpreted.Count);
            Assert.IsTrue(reinterpreted.Contains(1u));
        }

        [Test]
        public void AsReadOnly_ReflectsContent()
        {
            using var set = new ArraySetNative<int>(8, Allocator.Temp);
            set.Add(1);
            set.Add(2);

            var readOnly = set.AsReadOnly();

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(2, readOnly.Count);

            var buffer = new int[readOnly.Count];
            readOnly.CopyTo(buffer, readOnly.Count);
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, buffer);
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var set = new ArraySetNative<int>(4, Allocator.Temp);
            Assert.IsTrue(set.IsCreated);

            set.Dispose();

            Assert.IsFalse(set.IsCreated);
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var set = new ArraySetNative<int>(4, Allocator.TempJob);
            set.Add(1);

            var handle = set.Dispose(default);

            Assert.IsFalse(set.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void DoubleDispose_DoesNotThrow()
        {
            var set = new ArraySetNative<int>(4, Allocator.Temp);
            set.Dispose();

            Assert.DoesNotThrow(() => set.Dispose());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void UseAfterDispose_Throws()
        {
            var set = new ArraySetNative<int>(4, Allocator.Temp);
            set.Add(1);
            set.Dispose();

            Assert.Catch(() => set.Contains(1));
        }

        [Test]
        public void Collisions_ManyValues_AllRetrievable()
        {
            const int N = 500;
            using var set = new ArraySetNative<int>(2, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                set.Add(i);
            }

            for (var i = 0; i < N; i++)
            {
                Assert.IsTrue(set.Contains(i), $"missing {i}");
            }
        }

        [Test]
        public void ValueOverloadsAndCapacityOperations_MutateExpectedState()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            var two = 2;
            var missing = 99;

            Assert.IsTrue(set.Add(1));
            Assert.IsTrue(set.Add(in two));
            Assert.IsFalse(set.Add(in two));
            Assert.AreEqual(2, set.Items.Length);
            Assert.AreEqual(1, set.Items[0]);
            Assert.IsTrue(set.Contains(1));
            Assert.IsTrue(set.Contains(in two));
            Assert.IsFalse(set.Contains(in missing));
            Assert.IsTrue(set.Remove(1));
            Assert.IsTrue(set.Remove(in two));
            Assert.IsFalse(set.Remove(in missing));

            var initialCapacity = set.Capacity;
            set.EnsureCapacity(initialCapacity + 4);
            Assert.GreaterOrEqual(set.Capacity, initialCapacity + 4);

            var ensuredCapacity = set.Capacity;
            set.IncreaseCapacityBy(4);
            Assert.GreaterOrEqual(set.Capacity, ensuredCapacity + 4);

            var increasedCapacity = set.Capacity;
            set.IncreaseCapacityTo(increasedCapacity + 4);
            Assert.GreaterOrEqual(set.Capacity, increasedCapacity + 4);
        }

        [Test]
        public void ReadOnly_PropertiesContainsAndAllCopyOverloadsReflectOwner()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            set.Add(1);
            set.Add(2);
            set.Add(3);
            set.Add(4);
            var readOnly = set.AsReadOnly();
            var expected = new[] { 1, 2, 3, 4 };
            var destination = new int[set.Count];

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(set.Capacity, readOnly.Capacity);
            Assert.AreEqual(set.Count, readOnly.Count);
            Assert.AreEqual(4, readOnly.Items.Length);
            Assert.AreEqual(1, readOnly.Items[0]);
            Assert.IsTrue(readOnly.Contains(1));
            var two = 2;
            var missing = 99;
            Assert.IsTrue(readOnly.Contains(in two));
            Assert.IsFalse(readOnly.Contains(in missing));

            readOnly.CopyTo(destination.AsSpan());
            CollectionAssert.AreEqual(expected, destination);

            Array.Clear(destination, 0, destination.Length);
            readOnly.CopyTo(destination.AsSpan(), 2);
            readOnly.CopyTo(2, destination.AsSpan(2));
            CollectionAssert.AreEqual(expected, destination);

            Array.Clear(destination, 0, destination.Length);
            readOnly.CopyTo(1, destination.AsSpan(), 2);
            CollectionAssert.AreEqual(expected.AsSpan(1, 2).ToArray(), destination.AsSpan(0, 2).ToArray());

            Assert.IsTrue(readOnly.TryCopyTo(destination.AsSpan()));
            Assert.IsTrue(readOnly.TryCopyTo(destination.AsSpan(), 2));
            Assert.IsTrue(readOnly.TryCopyTo(1, destination.AsSpan(0, 3)));
            Assert.IsTrue(readOnly.TryCopyTo(1, destination.AsSpan(), 2));
            Assert.IsFalse(readOnly.TryCopyTo(new int[3].AsSpan(), set.Count));
            Assert.IsFalse(readOnly.TryCopyTo(3, new int[2].AsSpan()));
            Assert.IsFalse(default(ArraySetNative<int>.ReadOnly).IsCreated);
        }

        [Test]
        public void Enumerators_DirectConstructionMovementCurrentResetDisposeAndValidity()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            set.Add(7);
            var direct = new ArraySetNativeEnumerator<int>(in set);

            Assert.IsTrue(direct.IsValid);
            Assert.IsTrue(direct.MoveNext());
            Assert.AreEqual(7, direct.Current);
            Assert.IsFalse(direct.MoveNext());
            direct.Reset();
            Assert.IsTrue(direct.MoveNext());
            IEnumerator interfaceEnumerator = direct;
            Assert.AreEqual(7, interfaceEnumerator.Current);
            direct.Dispose();

            var ownerEnumerator = set.GetEnumerator();
            Assert.IsTrue(ownerEnumerator.MoveNext());
            ownerEnumerator.Dispose();

            var readOnly = set.AsReadOnly();
            var readOnlyDirect = new ArraySetNativeReadOnlyEnumerator<int>(in readOnly);
            Assert.IsTrue(readOnlyDirect.IsValid);
            Assert.IsTrue(readOnlyDirect.MoveNext());
            Assert.AreEqual(7, readOnlyDirect.Current);
            Assert.IsFalse(readOnlyDirect.MoveNext());
            readOnlyDirect.Reset();
            Assert.IsTrue(readOnlyDirect.MoveNext());
            IEnumerator readOnlyInterfaceEnumerator = readOnlyDirect;
            Assert.AreEqual(7, readOnlyInterfaceEnumerator.Current);
            readOnlyDirect.Dispose();

            var readOnlyEnumerator = readOnly.GetEnumerator();
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            readOnlyEnumerator.Dispose();

            Assert.IsFalse(default(ArraySetNativeEnumerator<int>).IsValid);
            Assert.IsFalse(default(ArraySetNativeReadOnlyEnumerator<int>).IsValid);
        }

        [Test]
        public void HashCollisionsAndRemovalReuse_PreserveRemainingValues()
        {
            using var set = new ArraySetNative<CollisionValue>(2, Allocator.Temp);
            var first = new CollisionValue(1);
            var second = new CollisionValue(2);
            var third = new CollisionValue(3);

            Assert.IsTrue(set.Add(first));
            Assert.IsTrue(set.Add(second));
            Assert.IsTrue(set.Add(third));
            Assert.IsFalse(set.Add(second));
            Assert.IsTrue(set.Remove(second));
            Assert.IsTrue(set.Add(new CollisionValue(4)));
            Assert.IsTrue(set.Contains(first));
            Assert.IsTrue(set.Contains(third));
            Assert.IsTrue(set.Contains(new CollisionValue(4)));
            Assert.IsFalse(set.Contains(second));
        }

        private readonly struct CollisionValue : IEquatable<CollisionValue>
        {
            private readonly int _value;

            public CollisionValue(int value)
            {
                _value = value;
            }

            public bool Equals(CollisionValue other)
                => _value == other._value;

            public override bool Equals(object obj)
                => obj is CollisionValue other && Equals(other);

            public override int GetHashCode()
                => 1;
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAddsAndDisposes(Allocator allocator)
        {
            var set = new ArraySetNative<int>(4, allocator);

            try
            {
                Assert.IsTrue(set.Add(10));

                Assert.IsTrue(set.IsCreated);
                Assert.AreEqual(1, set.Count);
                Assert.IsTrue(set.Contains(10));
            }
            finally
            {
                set.Dispose();
            }

            Assert.IsFalse(set.IsCreated);
        }

        [Test]
        public void Constructor_ZeroCapacity_CreatesEmptySetAndGrowsOnAdd()
        {
            var set = new ArraySetNative<int>(0, Allocator.Temp);

            try
            {
                Assert.IsTrue(set.IsCreated);
                Assert.AreEqual(0, set.Count);
                Assert.AreEqual(0, set.Capacity);

                Assert.IsTrue(set.Add(1));
                Assert.IsTrue(set.Add(2));

                Assert.AreEqual(2, set.Count);
                Assert.GreaterOrEqual(set.Capacity, 2);
                Assert.IsTrue(set.Contains(1));
                Assert.IsTrue(set.Contains(2));
            }
            finally
            {
                set.Dispose();
            }
        }

        [Test]
        public void CopyConstructor_EmptySource_CreatesIndependentEmptySet()
        {
            using var source = new ArraySetNative<int>(4, Allocator.Temp);
            using var copy = new ArraySetNative<int>(source, Allocator.Temp);

            Assert.IsTrue(copy.IsCreated);
            Assert.AreEqual(0, copy.Count);

            copy.Add(1);

            Assert.AreEqual(1, copy.Count);
            Assert.AreEqual(0, source.Count);
            Assert.IsFalse(source.Contains(1));
        }

        [Test]
        public void CopyConstructor_FromDefaultSource_Throws()
        {
            ArraySetNative<int> source = default;

            Assert.Throws<ArgumentException>(() => _ = new ArraySetNative<int>(source, Allocator.Temp));
        }

        [Test]
        public void EmptySet_Queries_ReturnEmptyResults()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            var enumerator = set.GetEnumerator();
            var missing = 1;

            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, set.Items.Length);
            Assert.IsFalse(set.Contains(1));
            Assert.IsFalse(set.Contains(in missing));
            Assert.IsFalse(set.Remove(1));
            Assert.IsFalse(set.Remove(in missing));
            Assert.DoesNotThrow(() => set.Clear());
            Assert.AreEqual(0, set.Count);

            var capacity = set.Capacity;

            Assert.AreEqual(capacity, set.EnsureCapacity(capacity - 1));
            Assert.AreEqual(capacity, set.EnsureCapacity(capacity));
            Assert.AreEqual(capacity, set.Capacity);
        }

        [Test]
        public void SetOperations_EmptyOperands_ProduceTheSetResult()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            using var empty = new ArraySetNative<int>(4, Allocator.Temp);

            set.Add(1);
            set.Add(2);

            set.Exclude(in empty);
            set.Union(in empty);

            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains(1));
            Assert.IsTrue(set.Contains(2));

            set.Intersect(in empty);

            Assert.AreEqual(0, set.Count);
            Assert.IsFalse(set.Contains(1));

            empty.Intersect(in set);
            empty.Exclude(in set);
            empty.Union(in empty);

            Assert.AreEqual(0, empty.Count);
        }

        [Test]
        public void SetOperations_DefaultOperand_Throws()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            ArraySetNative<int> notCreated = default;

            Assert.Throws<ArgumentException>(() => set.Intersect(in notCreated));
            Assert.Throws<ArgumentException>(() => set.Exclude(in notCreated));
            Assert.Throws<ArgumentException>(() => set.Union(in notCreated));
        }

        [Test]
        public void Trim_EmptySet_ShrinksToZeroAndStaysUsable()
        {
            var set = new ArraySetNative<int>(8, Allocator.Temp);

            try
            {
                set.Trim();

                Assert.IsTrue(set.IsCreated);
                Assert.AreEqual(0, set.Capacity);
                Assert.AreEqual(0, set.Count);

                Assert.IsTrue(set.Add(1));

                Assert.AreEqual(1, set.Count);
                Assert.IsTrue(set.Contains(1));
            }
            finally
            {
                set.Dispose();
            }
        }

        [Test]
        public void Items_CountBelowCapacity_LengthIsCountAndSeesLaterWrites()
        {
            using var set = new ArraySetNative<int>(8, Allocator.Temp);
            set.Add(10);
            set.Add(20);

            var items = set.Items;

            Assert.AreEqual(2, items.Length);
            Assert.GreaterOrEqual(set.Capacity, 8);
            CollectionAssert.AreEqual(new[] { 10, 20 }, items.ToArray());

            set.GetValueRefAt(1) = 99;

            Assert.AreEqual(99, items[1]);
        }

        [Test]
        public void Items_ElementShapes_RoundTripBitExact()
        {
            using var small = new ArraySetNative<RgbValue>(4, Allocator.Temp);
            using var wide = new ArraySetNative<WideValue>(4, Allocator.Temp);
            using var longs = new ArraySetNative<long>(4, Allocator.Temp);
            using var bytes = new ArraySetNative<byte>(4, Allocator.Temp);

            small.Add(new RgbValue { r = 1, g = 2, b = 3 });
            small.Add(new RgbValue { r = 250, g = 251, b = 252 });
            wide.Add(new WideValue { a = long.MinValue, b = -1L, c = long.MaxValue });
            longs.Add(long.MaxValue);
            longs.Add(long.MinValue);
            bytes.Add(255);
            bytes.Add(0);

            var smallItems = small.Items;
            var wideItems = wide.Items;

            Assert.AreEqual(2, smallItems.Length);
            Assert.AreEqual(1, smallItems[0].r);
            Assert.AreEqual(3, smallItems[0].b);
            Assert.AreEqual(250, smallItems[1].r);
            Assert.AreEqual(252, smallItems[1].b);
            Assert.AreEqual(long.MinValue, wideItems[0].a);
            Assert.AreEqual(-1L, wideItems[0].b);
            Assert.AreEqual(long.MaxValue, wideItems[0].c);
            CollectionAssert.AreEqual(new[] { long.MaxValue, long.MinValue }, longs.Items.ToArray());
            CollectionAssert.AreEqual(new byte[] { 255, 0 }, bytes.Items.ToArray());
            Assert.IsTrue(small.Contains(new RgbValue { r = 250, g = 251, b = 252 }));
            Assert.IsFalse(small.Contains(new RgbValue { r = 250, g = 251, b = 253 }));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Items_SliceUsedAfterOwnerDispose_Throws()
        {
            var set = new ArraySetNative<int>(4, Allocator.Persistent);
            set.Add(10);
            var items = set.Items;

            set.Dispose();

            Assert.Catch(() => _ = items[0]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Members_AfterOwnerDispose_Throw()
        {
            var set = new ArraySetNative<int>(4, Allocator.Persistent);
            var other = new ArraySetNative<int>(4, Allocator.Persistent);
            var value = 1;

            try
            {
                set.Add(1);
                set.Dispose();

                Assert.Catch(() => _ = set.Capacity);
                Assert.Catch(() => _ = set.Count);
                Assert.Catch(() => _ = set.Items);
                Assert.Catch(() => set.GetEnumerator());
                Assert.Catch(() => set.Add(2));
                Assert.Catch(() => set.Add(in value));
                Assert.Catch(() => set.Clear());
                Assert.Catch(() => set.Contains(1));
                Assert.Catch(() => set.Contains(in value));
                Assert.Catch(() => set.EnsureCapacity(8));
                Assert.Catch(() => set.IncreaseCapacityBy(1));
                Assert.Catch(() => set.IncreaseCapacityTo(8));
                Assert.Catch(() => set.Remove(1));
                Assert.Catch(() => set.Remove(in value));
                Assert.Catch(() => set.Trim());
                Assert.Catch(() => set.Intersect(in other));
                Assert.Catch(() => set.Exclude(in other));
                Assert.Catch(() => set.Union(in other));
                Assert.Catch(() => set.AddValue(in value, out _));
                Assert.Catch(() => set.GetValueRefAt(0));
                Assert.Catch(() => set.BumpVersion());
                Assert.Catch(() => set.AsValuesSpan());
            }
            finally
            {
                other.Dispose();
            }
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_AcceptedAllocator_MarksNotCreatedAndIsRepeatable(Allocator allocator)
        {
            var set = new ArraySetNative<int>(4, allocator);
            set.Add(1);

            set.Dispose();

            Assert.IsFalse(set.IsCreated);
            Assert.DoesNotThrow(() => set.Dispose());
        }

        [Test]
        public void Dispose_DefaultSet_IsNoOp()
        {
            ArraySetNative<int> set = default;

            Assert.DoesNotThrow(() => set.Dispose());

            var handle = set.Dispose(default);

            Assert.DoesNotThrow(() => handle.Complete());
            Assert.IsFalse(set.IsCreated);
        }

        [Test]
        public void DisposeJob_PersistentAllocator_CompletesAndMarksNotCreated()
        {
            var set = new ArraySetNative<int>(4, Allocator.Persistent);
            set.Add(1);

            var handle = set.Dispose(default);

            Assert.IsFalse(set.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void Reinterpret_SameSizeViews_ShareTheSetAndReinterpretBits()
        {
            var set = new ArraySetNative<int>(4, Allocator.Temp);

            try
            {
                var emptyView = set.Reinterpret<uint>();

                Assert.IsTrue(emptyView.IsCreated);
                Assert.AreEqual(0, emptyView.Count);

                set.Add(-1);
                set.Add(0x3F800000);

                var unsignedView = set.Reinterpret<uint>();
                var floatView = set.Reinterpret<float>();

                Assert.AreEqual(2, unsignedView.Count);
                Assert.IsTrue(unsignedView.Contains(uint.MaxValue));
                Assert.IsTrue(floatView.Contains(1f));

                Assert.IsTrue(unsignedView.Add(7u));

                Assert.AreEqual(3, set.Count);
                Assert.IsTrue(set.Contains(7));
                Assert.AreEqual(3, floatView.Count);
            }
            finally
            {
                set.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_DifferentSize_Throws()
        {
            var bytes = new ArraySetNative<byte>(4, Allocator.Temp);
            var ints = new ArraySetNative<int>(4, Allocator.Temp);

            try
            {
                Assert.Throws<InvalidOperationException>(() => bytes.Reinterpret<long>());
                Assert.Throws<InvalidOperationException>(() => ints.Reinterpret<short>());
                Assert.Throws<InvalidOperationException>(() => ints.Reinterpret<long>());
            }
            finally
            {
                bytes.Dispose();
                ints.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_ViewUsedAfterOwnerDispose_Throws()
        {
            var set = new ArraySetNative<int>(4, Allocator.Persistent);
            set.Add(1);
            var view = set.Reinterpret<uint>();

            set.Dispose();

            Assert.Catch(() => _ = view.Count);
            Assert.Catch(() => view.Contains(1u));
        }

        [Test]
        public void Add_FullSet_GrowsAndKeepsValues()
        {
            using var set = new ArraySetNative<int>(2, Allocator.Temp);

            Assert.IsTrue(set.Add(10));
            Assert.IsTrue(set.Add(20));
            Assert.AreEqual(2, set.Capacity);

            Assert.IsTrue(set.Add(30));
            Assert.IsFalse(set.Add(20));

            Assert.AreEqual(3, set.Count);
            Assert.GreaterOrEqual(set.Capacity, 3);
            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, set.Items.ToArray());
        }

        [Test]
        public void Remove_FirstValue_MovesTheLastValueIntoTheHole()
        {
            using var set = new ArraySetNative<int>(8, Allocator.Temp);
            set.Add(1);
            set.Add(2);
            set.Add(3);
            set.Add(4);

            Assert.IsTrue(set.Remove(1));

            Assert.AreEqual(3, set.Count);
            CollectionAssert.AreEqual(new[] { 4, 2, 3 }, set.Items.ToArray());
            Assert.IsFalse(set.Contains(1));
            Assert.IsTrue(set.Contains(4));
        }

        [Test]
        public void Remove_LastRemainingValue_LeavesAnEmptySetThatAcceptsNewValues()
        {
            using var set = new ArraySetNative<int>(2, Allocator.Temp);
            set.Add(1);

            Assert.IsTrue(set.Remove(1));
            Assert.AreEqual(0, set.Count);

            Assert.IsTrue(set.Add(1));
            Assert.IsTrue(set.Add(2));
            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public void AddValue_NewAndExistingValue_ReportsWhetherTheValueWasAdded()
        {
            var set = new ArraySetNative<int>(2, Allocator.Temp);

            try
            {
                Assert.IsTrue(set.AddValue(1, out var first));
                Assert.IsTrue(set.AddValue(2, out var second));
                Assert.IsTrue(set.AddValue(3, out var third));
                Assert.IsFalse(set.AddValue(2, out var again));

                Assert.AreEqual(0, first);
                Assert.AreEqual(1, second);
                Assert.AreEqual(2, third);
                Assert.AreEqual(second, again);
                Assert.AreEqual(3, set.Count);
                Assert.GreaterOrEqual(set.Capacity, 3);
            }
            finally
            {
                set.Dispose();
            }
        }

        [Test]
        public void GetValueRefAt_WritesThroughToTheStoredValue()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            set.Add(5);
            set.Add(6);

            ref var second = ref set.GetValueRefAt(1);
            second = 66;

            Assert.AreEqual(66, set.Items[1]);
            Assert.AreEqual(5, set.Items[0]);
            Assert.AreEqual(66, set.GetValueRefAt(1));
        }

        [Test]
        public void BumpVersion_InvalidatesRunningEnumerators()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            set.Add(1);
            set.Add(2);

            var enumerator = set.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            set.BumpVersion();

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public void AsValuesSpan_CoversTheCountAndAliasesTheStorage()
        {
            using var set = new ArraySetNative<int>(8, Allocator.Temp);

            Assert.AreEqual(0, set.AsValuesSpan().Length);

            set.Add(10);
            set.Add(20);
            set.Add(30);

            var span = set.AsValuesSpan();

            Assert.AreEqual(3, span.Length);
            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, span.ToArray());

            span[1] = 99;
            set.GetValueRefAt(2) = 33;

            Assert.AreEqual(99, set.Items[1]);
            Assert.AreEqual(33, span[2]);
        }

        [Test]
        public void AsValuesSpan_ElementShapes_RoundTripBitExact()
        {
            using var small = new ArraySetNative<RgbValue>(4, Allocator.Temp);
            using var wide = new ArraySetNative<WideValue>(4, Allocator.Temp);

            small.Add(new RgbValue { r = 7, g = 8, b = 9 });
            wide.Add(new WideValue { a = 1L, b = long.MinValue, c = long.MaxValue });

            var smallSpan = small.AsValuesSpan();
            var wideSpan = wide.AsValuesSpan();

            Assert.AreEqual(1, smallSpan.Length);
            Assert.AreEqual(7, smallSpan[0].r);
            Assert.AreEqual(8, smallSpan[0].g);
            Assert.AreEqual(9, smallSpan[0].b);
            Assert.AreEqual(1, wideSpan.Length);
            Assert.AreEqual(1L, wideSpan[0].a);
            Assert.AreEqual(long.MinValue, wideSpan[0].b);
            Assert.AreEqual(long.MaxValue, wideSpan[0].c);
        }

        [Test]
        public void Enumerators_FromDefaultSet_Throw()
        {
            ArraySetNative<int> notCreated = default;
            ArraySetNative<int>.ReadOnly notCreatedReadOnly = default;

            Assert.Throws<ArgumentException>(() => _ = new ArraySetNativeEnumerator<int>(in notCreated));
            Assert.Throws<ArgumentException>(
                () => _ = new ArraySetNativeReadOnlyEnumerator<int>(in notCreatedReadOnly)
            );
        }

        [Test]
        public void Enumerators_EmptySet_MoveNextReturnsFalse()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            var readOnly = set.AsReadOnly();

            var ownerEnumerator = new ArraySetNativeEnumerator<int>(in set);
            var readOnlyEnumerator = new ArraySetNativeReadOnlyEnumerator<int>(in readOnly);

            Assert.IsTrue(ownerEnumerator.IsValid);
            Assert.IsFalse(ownerEnumerator.MoveNext());
            Assert.IsTrue(readOnlyEnumerator.IsValid);
            Assert.IsFalse(readOnlyEnumerator.MoveNext());
        }

        [Test]
        public void NativeDispose_NullHeader_IsNoOp()
        {
            Assert.DoesNotThrow(() => default(ArraySetNativeDispose).Dispose());
        }

        [Test]
        public unsafe void NativeDispose_AllocatedHeader_ReleasesIt()
        {
            var data = ArraySetUnsafe<int>.Alloc(4, Allocator.Persistent);
            data->Add(1);

            Assert.IsTrue(data->IsCreated);
            Assert.AreEqual(1, data->Count);

            var dispose = new ArraySetNativeDispose { m_Data = data };
            dispose.Dispose();
        }

        [Test]
        public void ReadOnly_EmptySet_ReturnsEmptyResults()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            var readOnly = set.AsReadOnly();
            var enumerator = readOnly.GetEnumerator();
            var missing = 1;

            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, readOnly.Count);
            Assert.AreEqual(0, readOnly.Items.Length);
            Assert.AreEqual(0, readOnly.AsValuesReadOnlySpan().Length);
            Assert.IsFalse(readOnly.Contains(1));
            Assert.IsFalse(readOnly.Contains(in missing));
        }

        [Test]
        public void ReadOnly_ConstructorFromDefaultSet_Throws()
        {
            ArraySetNative<int> notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new ArraySetNative<int>.ReadOnly(notCreated));
            Assert.Throws<ArgumentException>(() => notCreated.AsReadOnly());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnly_Members_AfterOwnerDispose_Throw()
        {
            var set = new ArraySetNative<int>(4, Allocator.Persistent);
            set.Add(1);
            var readOnly = set.AsReadOnly();
            var value = 1;

            set.Dispose();

            Assert.Catch(() => _ = readOnly.Capacity);
            Assert.Catch(() => _ = readOnly.Count);
            Assert.Catch(() => _ = readOnly.Items);
            Assert.Catch(() => readOnly.GetEnumerator());
            Assert.Catch(() => readOnly.Contains(1));
            Assert.Catch(() => readOnly.Contains(in value));
            Assert.Catch(() => readOnly.AsValuesReadOnlySpan());
        }

        [Test]
        public void ReadOnly_UncheckedVersionAndValueAt_ReflectTheSet()
        {
            using var set = new ArraySetNative<int>(4, Allocator.Temp);
            var readOnly = set.AsReadOnly();
            var initialVersion = readOnly.UncheckedVersion;

            set.Add(7);
            set.Add(8);

            Assert.AreNotEqual(initialVersion, readOnly.UncheckedVersion);
            Assert.AreEqual(7, readOnly.ValueAt(0));
            Assert.AreEqual(8, readOnly.ValueAt(1));

            var versionBeforeBump = readOnly.UncheckedVersion;

            set.BumpVersion();

            Assert.AreNotEqual(versionBeforeBump, readOnly.UncheckedVersion);
        }

        [Test]
        public void ReadOnly_ItemsAndSpan_ElementShapesRoundTripAndSeeWrites()
        {
            using var set = new ArraySetNative<RgbValue>(8, Allocator.Temp);
            set.Add(new RgbValue { r = 1, g = 2, b = 3 });
            set.Add(new RgbValue { r = 4, g = 5, b = 6 });

            var readOnly = set.AsReadOnly();
            var span = readOnly.AsValuesReadOnlySpan();

            Assert.AreEqual(2, span.Length);
            Assert.AreEqual(3, span[0].b);
            Assert.AreEqual(4, span[1].r);
            Assert.AreEqual(2, readOnly.Items.Length);
            Assert.AreEqual(6, readOnly.Items[1].b);

            set.GetValueRefAt(0).g = 22;

            Assert.AreEqual(22, span[0].g);
            Assert.AreEqual(22, readOnly.ValueAt(0).g);
        }

        [Test]
        public void ReadOnly_ModifiedDuringEnumeration_Throws()
        {
            using var set = new ArraySetNative<int>(8, Allocator.Temp);
            set.Add(1);
            set.Add(2);

            var readOnly = set.AsReadOnly();
            var enumerator = readOnly.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            set.Add(3);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
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
