using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Common;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    // Adapted from Unity.Collections.Tests/NativeHashSetTests.cs against the raw Unsafe header.
    public partial class ArraySetUnsafeTests
    {
        [Test]
        public void Constructor_CreatesValidSet()
        {
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);

            Assert.IsTrue(set.IsCreated);
            Assert.AreEqual(0, set.Count);
            Assert.GreaterOrEqual(set.Capacity, 4);
        }

        [Test]
        public void DefaultSet_IsNotCreated()
        {
            ArraySetUnsafe<int> set = default;

            Assert.IsFalse(set.IsCreated);
        }

        [Test]
        public void Add_StoresUniqueValues()
        {
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);

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
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);
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
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);
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
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);

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
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);
            var before = set.Capacity;

            set.EnsureCapacity(before + 100);

            Assert.Greater(set.Capacity, before);
        }

        [Test]
        public void Trim_ShrinksCapacityToCount()
        {
            using var set = new ArraySetUnsafe<int>(64, Allocator.Temp);
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
            using var set = new ArraySetUnsafe<int>(8, Allocator.Temp);

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
        public void CopyConstructor_DeepCopiesContent()
        {
            using var source = new ArraySetUnsafe<int>(8, Allocator.Temp);
            source.Add(1);
            source.Add(2);

            using var copy = new ArraySetUnsafe<int>(source, Allocator.Temp);

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
            using var source = new ArraySetUnsafe<int>(2, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                source.Add(i);
            }

            using var copy = new ArraySetUnsafe<int>(source, Allocator.Temp);

            Assert.AreEqual(N, copy.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.IsTrue(copy.Contains(i), $"missing {i} after copy");
            }
        }

        [Test]
        public void Intersect_KeepsOnlyShared()
        {
            using var a = new ArraySetUnsafe<int>(8, Allocator.Temp);
            using var b = new ArraySetUnsafe<int>(8, Allocator.Temp);

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
            using var a = new ArraySetUnsafe<int>(8, Allocator.Temp);
            using var b = new ArraySetUnsafe<int>(8, Allocator.Temp);

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
            using var a = new ArraySetUnsafe<int>(8, Allocator.Temp);
            using var b = new ArraySetUnsafe<int>(8, Allocator.Temp);

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
            using var set = new ArraySetUnsafe<int>(8, Allocator.Temp);
            set.Add(1);

            var reinterpreted = set.Reinterpret<uint>();

            Assert.AreEqual(set.Count, reinterpreted.Count);
            Assert.IsTrue(reinterpreted.Contains(1u));
        }

        [Test]
        public void AsReadOnly_ReflectsContent()
        {
            using var set = new ArraySetUnsafe<int>(8, Allocator.Temp);
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
            var set = new ArraySetUnsafe<int>(4, Allocator.Temp);
            Assert.IsTrue(set.IsCreated);

            set.Dispose();

            Assert.IsFalse(set.IsCreated);
        }

        [Test]
        public void Collisions_ManyValues_AllRetrievable()
        {
            const int N = 500;
            using var set = new ArraySetUnsafe<int>(2, Allocator.Temp);

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
        public void ValueOverloadsItemsAndCapacityOperations_MutateExpectedState()
        {
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);
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
        public void ReadOnly_ConstructionPropertiesContainsAndAllCopyOverloadsReflectSource()
        {
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);
            set.Add(1);
            set.Add(2);
            set.Add(3);
            set.Add(4);
            var direct = new ArraySetUnsafe<int>.ReadOnly(set);
            var fromOwner = set.AsReadOnly();
            var expected = new[] { 1, 2, 3, 4 };
            var destination = new int[set.Count];

            Assert.IsTrue(direct.IsCreated);
            Assert.AreEqual(set.Capacity, direct.Capacity);
            Assert.AreEqual(set.Count, direct.Count);
            Assert.AreEqual(4, direct.Items.Length);
            Assert.AreEqual(1, direct.Items[0]);
            Assert.AreEqual(2, fromOwner.Items[1]);
            Assert.IsTrue(direct.Contains(1));
            var two = 2;
            var missing = 99;
            Assert.IsTrue(direct.Contains(in two));
            Assert.IsFalse(direct.Contains(in missing));

            direct.CopyTo(destination.AsSpan());
            CollectionAssert.AreEqual(expected, destination);

            Array.Clear(destination, 0, destination.Length);
            direct.CopyTo(destination.AsSpan(), 2);
            direct.CopyTo(2, destination.AsSpan(2));
            CollectionAssert.AreEqual(expected, destination);

            Array.Clear(destination, 0, destination.Length);
            direct.CopyTo(1, destination.AsSpan(), 2);
            CollectionAssert.AreEqual(expected.AsSpan(1, 2).ToArray(), destination.AsSpan(0, 2).ToArray());

            Assert.IsTrue(direct.TryCopyTo(destination.AsSpan()));
            Assert.IsTrue(direct.TryCopyTo(destination.AsSpan(), 2));
            Assert.IsTrue(direct.TryCopyTo(1, destination.AsSpan(0, 3)));
            Assert.IsTrue(direct.TryCopyTo(1, destination.AsSpan(), 2));
            Assert.IsFalse(direct.TryCopyTo(new int[3].AsSpan(), set.Count));
            Assert.IsFalse(direct.TryCopyTo(3, new int[2].AsSpan()));
            Assert.IsFalse(default(ArraySetUnsafe<int>.ReadOnly).IsCreated);
        }

        [Test]
        public void Enumerators_DirectConstructionMovementCurrentResetDisposeAndValidity()
        {
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);
            set.Add(7);
            var direct = new ArraySetUnsafeEnumerator<int>(in set);

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
            var readOnlyDirect = new ArraySetUnsafeReadOnlyEnumerator<int>(in readOnly);
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

            Assert.IsFalse(default(ArraySetUnsafeEnumerator<int>).IsValid);
            Assert.IsFalse(default(ArraySetUnsafeReadOnlyEnumerator<int>).IsValid);
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var set = new ArraySetUnsafe<int>(4, Allocator.TempJob);
            set.Add(1);

            var handle = set.Dispose(default);

            Assert.IsFalse(set.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void AllocCapacityAndFree_OwnsLiveHeader()
        {
            // SAFETY: The pointer is freed exactly once in finally and is never used after release.
            unsafe
            {
                ArraySetUnsafe<int>* data = null;

                try
                {
                    data = ArraySetUnsafe<int>.Alloc(4, Allocator.Temp);

                    Assert.AreNotEqual(IntPtr.Zero, (IntPtr)data);

                    data->Add(1);

                    Assert.IsTrue(data->IsCreated);
                    Assert.GreaterOrEqual(data->Capacity, 4);
                    Assert.AreEqual(1, data->Count);
                    Assert.IsTrue(data->Contains(1));
                }
                finally
                {
                    if (data != null)
                    {
                        ArraySetUnsafe<int>.Free(data);
                        data = null;
                    }
                }
            }
        }

        [Test]
        public void AllocSourceAndFree_DeepCopiesLiveHeader()
        {
            using var source = new ArraySetUnsafe<int>(4, Allocator.Temp);
            source.Add(1);

            // SAFETY: The copied pointer is freed exactly once in finally and does not escape the test.
            unsafe
            {
                ArraySetUnsafe<int>* data = null;

                try
                {
                    data = ArraySetUnsafe<int>.Alloc(in source, Allocator.Temp);

                    Assert.AreNotEqual(IntPtr.Zero, (IntPtr)data);

                    data->Add(2);

                    Assert.IsTrue(data->Contains(1));
                    Assert.IsTrue(data->Contains(2));
                    Assert.IsFalse(source.Contains(2));
                }
                finally
                {
                    if (data != null)
                    {
                        ArraySetUnsafe<int>.Free(data);
                        data = null;
                    }
                }
            }
        }

        [Test]
        public void HashCollisionsAndRemovalReuse_PreserveRemainingValues()
        {
            using var set = new ArraySetUnsafe<CollisionValue>(2, Allocator.Temp);
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

        [TestCase(false)]
        [TestCase(true)]
        [Category("Task045X2Regression")]
        public void Reinterpret_DisposeBorrowedView_LeavesEveryOwnerBufferUsable(bool scheduled)
        {
            var owner = new ArraySetUnsafe<int>(2, Allocator.Persistent);

            try
            {
                owner.Add(17);
                owner.Add(29);
                var view = owner.Reinterpret<uint>();

                if (scheduled)
                {
                    view.Dispose(default(Unity.Jobs.JobHandle)).Complete();
                }
                else
                {
                    view.Dispose();
                }

                Assert.IsFalse(view.IsCreated);
                Assert.AreEqual(0, view.Count);
                Assert.AreEqual(2, owner.Count);
                CollectionAssert.AreEquivalent(new[] { 17, 29 }, owner.Items.ToArray());
                owner.Add(43);
                Assert.IsTrue(owner.Contains(43));
                Assert.AreEqual(3, owner.Count);
            }
            finally
            {
                owner.Dispose();
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void Reinterpret_ChangeBorrowedCapacity_RejectsWithoutChangingOwner()
        {
            var owner = new ArraySetUnsafe<int>(4, Allocator.Persistent);

            try
            {
                owner.Add(17);
                owner.Add(29);
                var view = owner.Reinterpret<uint>();

                try
                {
                    Assert.AreEqual(4, view.EnsureCapacity(4));
                    Assert.Throws<InvalidOperationException>(() => view.EnsureCapacity(8));
                    Assert.Throws<InvalidOperationException>(() => view.Trim());
                    Assert.AreEqual(4, view.Capacity);
                    Assert.AreEqual(2, view.Count);
                    Assert.AreEqual(4, owner.Capacity);
                    Assert.AreEqual(2, owner.Count);
                    CollectionAssert.AreEquivalent(new[] { 17, 29 }, owner.Items.ToArray());
                }
                finally
                {
                    view.Dispose();
                }

                owner.Add(43);
                Assert.IsTrue(owner.Contains(43));
                Assert.AreEqual(3, owner.Count);
            }
            finally
            {
                owner.Dispose();
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void Reinterpret_CollisionBucketGrowthRejected_DoesNotInsertPhantomEntry()
        {
            var owner = new ArraySetUnsafe<CollisionValue>(2, Allocator.Persistent);

            try
            {
                owner.EnsureCapacity(20);

                for (var i = 1; i <= 4; i++)
                {
                    owner.Add(new CollisionValue(i));
                }

                var view = owner.Reinterpret<CollisionValue>();

                try
                {
                    Assert.Throws<InvalidOperationException>(() => view.Add(new CollisionValue(5)));
                    Assert.AreEqual(4, view.Count);
                    Assert.AreEqual(4, owner.Count);
                    Assert.IsFalse(view.Contains(new CollisionValue(5)));
                    Assert.IsFalse(owner.Contains(new CollisionValue(5)));

                    for (var i = 1; i <= 4; i++)
                    {
                        Assert.IsTrue(owner.Contains(new CollisionValue(i)));
                    }
                }
                finally
                {
                    view.Dispose();
                }

                Assert.IsTrue(owner.Add(new CollisionValue(5)));
                Assert.AreEqual(5, owner.Count);
                Assert.IsTrue(owner.Contains(new CollisionValue(5)));
            }
            finally
            {
                owner.Dispose();
            }
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
        public void Constructor_AcceptedAllocator_CreatesAndDisposes(Allocator allocator)
        {
            var set = new ArraySetUnsafe<int>(4, allocator);

            try
            {
                Assert.IsTrue(set.Add(1));
                Assert.IsTrue(set.IsCreated);
                Assert.AreEqual(1, set.Count);
            }
            finally
            {
                set.Dispose();
            }

            Assert.IsFalse(set.IsCreated);
            Assert.AreEqual(0, set.Count);
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_JobHandle_AcceptedAllocator_ReleasesTheStorage(Allocator allocator)
        {
            var set = new ArraySetUnsafe<int>(4, allocator);
            set.Add(1);

            var handle = set.Dispose(default);

            Assert.IsFalse(set.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
            Assert.DoesNotThrow(() => set.Dispose(default).Complete());
        }

        [Test]
        public void Dispose_DefaultOrAlreadyDisposedSet_IsNoOp()
        {
            var uncreated = default(ArraySetUnsafe<int>);
            var disposed = new ArraySetUnsafe<int>(4, Allocator.Persistent);
            disposed.Dispose();

            Assert.DoesNotThrow(() => uncreated.Dispose());
            Assert.DoesNotThrow(() => disposed.Dispose());
            Assert.IsFalse(uncreated.IsCreated);
            Assert.IsFalse(disposed.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_NegativeCapacity_Throws()
        {
            // The two buffer allocations that precede the bucket allocation hold no memory for a negative capacity.
            Assert.Catch(() => _ = new ArraySetUnsafe<int>(-1, Allocator.Temp));
        }

        [Test]
        public void Constructor_ZeroCapacity_CreatesAUsableSet()
        {
            const int N = 50;
            using var set = new ArraySetUnsafe<int>(0, Allocator.Temp);

            Assert.IsTrue(set.IsCreated);
            Assert.AreEqual(0, set.Count);
            Assert.IsFalse(set.Contains(1));

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
        public void CopyConstructor_NotCreatedOrDisposedSource_ThrowsArgumentException()
        {
            var uncreated = default(ArraySetUnsafe<int>);
            var disposed = new ArraySetUnsafe<int>(4, Allocator.Persistent);
            disposed.Dispose();

            Assert.Throws<ArgumentException>(() => _ = new ArraySetUnsafe<int>(uncreated, Allocator.Temp));
            Assert.Throws<ArgumentException>(() => _ = new ArraySetUnsafe<int>(disposed, Allocator.Temp));
        }

        [Test]
        public void CopyConstructor_EmptySource_CreatesAnIndependentUsableSet()
        {
            using var source = new ArraySetUnsafe<int>(0, Allocator.Temp);
            using var copy = new ArraySetUnsafe<int>(source, Allocator.Temp);

            Assert.IsTrue(copy.IsCreated);
            Assert.AreEqual(0, copy.Count);

            copy.Add(1);

            Assert.IsTrue(copy.Contains(1));
            Assert.AreEqual(0, source.Count);
        }

        [Test]
        public void Items_EmptyOrUncreatedSet_IsEmpty()
        {
            using var empty = new ArraySetUnsafe<int>(4, Allocator.Temp);
            var uncreated = default(ArraySetUnsafe<int>);

            Assert.AreEqual(0, empty.Items.Length);
            Assert.AreEqual(0, uncreated.Items.Length);
        }

        [Test]
        public void Values_ByteElement_RoundTripBitExact()
        {
            AssertSetRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Values_ThreeByteStruct_RoundTripBitExact()
        {
            AssertSetRoundTrip(
                  new RgbValue { r = 1, g = 2, b = 3 }
                , new RgbValue { r = 0, g = 0, b = 0 }
                , new RgbValue { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Values_LongElement_RoundTripBitExact()
        {
            AssertSetRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Values_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertSetRoundTrip(
                  new WideValue { a = 1, b = -2, c = long.MaxValue }
                , new WideValue { a = 0, b = 0, c = 0 }
                , new WideValue { a = long.MinValue, b = 7, c = -1 }
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Contains_UncreatedSet_Throws()
        {
            var set = default(ArraySetUnsafe<int>);
            var value = 1;

            Assert.Throws<InvalidOperationException>(() => set.Contains(1));
            Assert.Throws<InvalidOperationException>(() => set.Contains(in value));
        }

        [Test]
        public void Add_ExactlyFullThenOnePast_KeepsEveryValue()
        {
            using var set = new ArraySetUnsafe<int>(3, Allocator.Temp);

            for (var i = 0; i < set.Capacity; i++)
            {
                Assert.IsTrue(set.Add(i));
            }

            Assert.AreEqual(set.Count, set.Capacity);

            var past = set.Capacity;

            Assert.IsTrue(set.Add(past));

            for (var i = 0; i <= past; i++)
            {
                Assert.IsTrue(set.Contains(i), $"missing {i}");
            }
        }

        [Test]
        public void AddValue_NewAndExistingValues_ReportIndexAndGrowPastCapacity()
        {
            using var set = new ArraySetUnsafe<int>(2, Allocator.Temp);
            var first = 5;
            var other = 6;

            Assert.IsTrue(set.AddValue(in first, out var firstIndex));
            Assert.AreEqual(0, firstIndex);
            Assert.IsFalse(set.AddValue(in first, out var existingIndex));
            Assert.AreEqual(0, existingIndex);
            Assert.IsTrue(set.AddValue(in other, out var otherIndex));
            Assert.AreEqual(1, otherIndex);
            Assert.AreEqual(2, set.Count);

            for (var i = 10; i < 30; i++)
            {
                var value = i;

                Assert.IsTrue(set.AddValue(in value, out var index));
                Assert.AreEqual(i - 8, index);
            }

            Assert.AreEqual(22, set.Count);
        }

        [TestCase(3u)]
        [TestCase(7u)]
        [TestCase(101u)]
        [TestCase(1103u)]
        [TestCase(1u)]
        public void Reduce_AnyHashcode_EqualsModulo(uint bucketCount)
        {
            var multiplier = HashHelpers.GetFastModMultiplier(bucketCount);
            var hashcodes = new[] {
                0u,
                1u,
                bucketCount - 1,
                bucketCount,
                bucketCount + 1,
                bucketCount * 2,
                (uint)int.MaxValue,
                (uint)int.MaxValue + 1u,
                uint.MaxValue - 1u,
                uint.MaxValue,
            };

            foreach (var hashcode in hashcodes)
            {
                Assert.AreEqual(
                      hashcode % bucketCount
                    , ArraySetUnsafe<int>.Reduce(hashcode, bucketCount, multiplier)
                    , $"{hashcode} % {bucketCount}"
                );
            }
        }

        [Test]
        public void Remove_EmptySet_ReturnsFalse()
        {
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);
            var value = 1;

            Assert.IsFalse(set.Remove(1));
            Assert.IsFalse(set.Remove(in value));
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void Remove_EveryValue_MatchesHashSet()
        {
            const int COUNT = 6;

            for (var removed = 1; removed <= COUNT; removed++)
            {
                using var set = new ArraySetUnsafe<int>(COUNT, Allocator.Temp);
                var expected = new HashSet<int>();

                for (var i = 1; i <= COUNT; i++)
                {
                    set.Add(i);
                    expected.Add(i);
                }

                Assert.AreEqual(expected.Remove(removed), set.Remove(removed), $"removed {removed}");
                Assert.AreEqual(expected.Count, set.Count, $"removed {removed}");
                CollectionAssert.AreEquivalent(expected, set.Items.ToArray(), $"removed {removed}");

                foreach (var value in expected)
                {
                    Assert.IsTrue(set.Contains(value), $"removed {removed}, missing {value}");
                }
            }
        }

        [Test]
        public void Remove_CollidingValuesInEveryOrder_KeepsTheRemainingValuesReachable()
        {
            const int COUNT = 5;

            for (var removed = 0; removed < COUNT; removed++)
            {
                using var set = new ArraySetUnsafe<CollisionValue>(2, Allocator.Temp);

                for (var i = 0; i < COUNT; i++)
                {
                    Assert.IsTrue(set.Add(new CollisionValue(i)));
                }

                Assert.IsTrue(set.Remove(new CollisionValue(removed)));
                Assert.IsFalse(set.Remove(new CollisionValue(removed)));
                Assert.AreEqual(COUNT - 1, set.Count);

                for (var i = 0; i < COUNT; i++)
                {
                    Assert.AreEqual(i != removed, set.Contains(new CollisionValue(i)), $"removed {removed}, value {i}");
                }
            }
        }

        [Test]
        public void Clear_EmptySet_KeepsTheSetUsable()
        {
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);

            set.Clear();
            set.Clear();

            Assert.AreEqual(0, set.Count);
            Assert.IsTrue(set.Add(1));
        }

        [Test]
        public void EnsureCapacityAndIncreaseCapacity_NotLargerThanCapacity_KeepCapacityAndContent()
        {
            using var set = new ArraySetUnsafe<int>(8, Allocator.Temp);
            set.Add(1);
            var capacity = set.Capacity;

            Assert.AreEqual(capacity, set.EnsureCapacity(0));
            Assert.AreEqual(capacity, set.EnsureCapacity(capacity));
            Assert.AreEqual(capacity, set.IncreaseCapacityTo(1));
            Assert.AreEqual(capacity, set.IncreaseCapacityBy(0));
            Assert.AreEqual(capacity, set.Capacity);
            Assert.IsTrue(set.Contains(1));
        }

        [Test]
        public void Trim_EmptySet_ReleasesTheCapacityAndStaysUsable()
        {
            using var set = new ArraySetUnsafe<int>(8, Allocator.Temp);

            set.Trim();

            Assert.AreEqual(0, set.Capacity);
            Assert.IsTrue(set.IsCreated);
            Assert.IsTrue(set.Add(3));
            Assert.IsTrue(set.Contains(3));
        }

        [Test]
        public void SetOperations_WithEmptyOrSameSet_MatchHashSet()
        {
            using var set = new ArraySetUnsafe<int>(8, Allocator.Temp);
            using var empty = new ArraySetUnsafe<int>(0, Allocator.Temp);
            set.Add(1);
            set.Add(2);
            set.Add(3);

            set.Union(in empty);
            set.Exclude(in empty);

            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, set.Items.ToArray());

            set.Union(in set);
            set.Intersect(in set);

            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, set.Items.ToArray());

            set.Intersect(in empty);

            Assert.AreEqual(0, set.Count);

            empty.Union(in set);

            Assert.AreEqual(0, empty.Count);

            set.Add(4);
            set.Add(5);
            set.Add(6);
            set.Exclude(in set);

            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void Reinterpret_UncreatedSet_ReturnsAnUncreatedView()
        {
            var set = default(ArraySetUnsafe<int>);

            var view = set.Reinterpret<uint>();

            Assert.IsFalse(view.IsCreated);
            Assert.AreEqual(0, view.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_DifferentSize_Throws()
        {
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => set.Reinterpret<ushort>());
            Assert.Throws<InvalidOperationException>(() => set.Reinterpret<long>());
        }

        [Test]
        public void Reinterpret_SameSizeValues_ExposeTheBitsOfEveryValue()
        {
            using var ints = new ArraySetUnsafe<int>(4, Allocator.Temp);
            using var longs = new ArraySetUnsafe<long>(4, Allocator.Temp);
            ints.Add(-1);
            ints.Add(int.MinValue);
            longs.Add(-1L);
            longs.Add(long.MinValue);

            // The views are never disposed: they share the owners' buffers, so only the owners release them.
            var uintView = ints.Reinterpret<uint>();
            var ulongView = longs.Reinterpret<ulong>();

            CollectionAssert.AreEquivalent(new[] { uint.MaxValue, 2147483648u }, uintView.Items.ToArray());
            CollectionAssert.AreEquivalent(new[] { ulong.MaxValue, 9223372036854775808ul }, ulongView.Items.ToArray());
        }

        [Test]
        public void GetEnumerator_EmptySet_YieldsNothing()
        {
            using var set = new ArraySetUnsafe<int>(4, Allocator.Temp);

            var enumerator = set.GetEnumerator();

            Assert.IsTrue(enumerator.IsValid);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void GetEnumerator_UncreatedSet_MoveNextThrows()
        {
            var set = default(ArraySetUnsafe<int>);

            var enumerator = set.GetEnumerator();

            Assert.IsFalse(enumerator.IsValid);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public void DebugProxy_EmptyAndFilledSet_ReportCountAndItems()
        {
            using var empty = new ArraySetUnsafe<int>(4, Allocator.Temp);
            using var filled = new ArraySetUnsafe<int>(4, Allocator.Temp);
            filled.Add(7);
            filled.Add(8);

            var emptyProxy = new ArraySetUnsafeDebugProxy<int>(in empty);
            var filledProxy = new ArraySetUnsafeDebugProxy<int>(in filled);

            Assert.AreEqual(0u, emptyProxy.Count);
            Assert.AreEqual(0, emptyProxy.Items.Length);
            Assert.AreEqual(2u, filledProxy.Count);
            CollectionAssert.AreEquivalent(new[] { 7, 8 }, filledProxy.Items);
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Alloc_AcceptedAllocator_CreatesAndFreesTheHeader(Allocator allocator)
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = ArraySetUnsafe<int>.Alloc(4, allocator);

                try
                {
                    data->Add(1);

                    Assert.IsTrue(data->IsCreated);
                    Assert.AreEqual(1, data->Count);
                    Assert.IsTrue(data->Contains(1));
                }
                finally
                {
                    ArraySetUnsafe<int>.Free(data);
                }
            }
        }

        [Test]
        public void Alloc_ZeroCapacityAndEmptySource_CreateUsableHeaders()
        {
            using var source = new ArraySetUnsafe<int>(0, Allocator.Temp);

            // SAFETY: Both headers returned by Alloc stay live until Free, which runs exactly once in finally.
            unsafe
            {
                var zero = ArraySetUnsafe<int>.Alloc(0, Allocator.Persistent);
                var copy = ArraySetUnsafe<int>.Alloc(in source, Allocator.Persistent);

                try
                {
                    Assert.IsTrue(zero->Add(1));
                    Assert.IsTrue(copy->Add(2));
                    Assert.IsTrue(zero->Contains(1));
                    Assert.IsTrue(copy->Contains(2));
                    Assert.AreEqual(0, source.Count);
                }
                finally
                {
                    ArraySetUnsafe<int>.Free(zero);
                    ArraySetUnsafe<int>.Free(copy);
                }
            }
        }

        [Test]
        public void Alloc_InvalidArgument_Throws()
        {
            var uncreated = default(ArraySetUnsafe<int>);

            // The header is allocated before the constructor validates the source (suspected defect D-17), so this
            // call leaks one header. Allocator.Temp is used so that Unity releases it without a leak report.
            // SAFETY: The call throws before it returns a header, so no pointer is used.
            unsafe
            {
                Assert.Throws<ArgumentException>(() => ArraySetUnsafe<int>.Alloc(in uncreated, Allocator.Temp));
            }
        }

        [Test]
        public void Free_NullPointer_DoesNothing()
        {
            // SAFETY: Free documents that a null header is ignored, so no memory is read or released.
            unsafe
            {
                Assert.DoesNotThrow(() => ArraySetUnsafe<int>.Free(null));
            }
        }

        private static void AssertSetRoundTrip<T>(T a, T b, T c)
            where T : unmanaged, IEquatable<T>
        {
            var name = typeof(T).Name;
            var set = new ArraySetUnsafe<T>(1, Allocator.Temp);

            try
            {
                Assert.IsTrue(set.Add(a), name);
                Assert.IsTrue(set.Add(in b), name);
                Assert.IsTrue(set.Add(c), name);
                Assert.IsFalse(set.Add(a), name);
                Assert.AreEqual(3, set.Count, name);
                CollectionAssert.AreEquivalent(new[] { a, b, c }, set.Items.ToArray(), name);
                Assert.IsTrue(set.Contains(a), name);
                Assert.IsTrue(set.Contains(in b), name);
                Assert.IsTrue(set.Contains(c), name);
                Assert.IsTrue(set.Remove(b), name);
                CollectionAssert.AreEquivalent(new[] { a, c }, set.Items.ToArray(), name);
                Assert.IsFalse(set.Contains(b), name);

                set.Clear();

                Assert.AreEqual(0, set.Count, name);
                Assert.IsTrue(set.Add(b), name);
                CollectionAssert.AreEqual(new[] { b }, set.Items.ToArray(), name);
            }
            finally
            {
                set.Dispose();
            }
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
