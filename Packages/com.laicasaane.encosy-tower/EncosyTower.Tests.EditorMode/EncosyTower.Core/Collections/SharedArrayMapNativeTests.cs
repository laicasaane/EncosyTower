using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from Unity.Collections.Tests/NativeHashMapTests.cs against the
    // SharedArrayMapNative view (a fixed-capacity window onto the owner's shared buffers).
    public partial class SharedArrayMapNativeTests
    {
        [Test]
        public void AsNative_ReflectsOwnerContent()
        {
            using var map = new SharedArrayMap<int, int>(8);
            map.Add(1, 10);
            map.Add(2, 20);

            SharedArrayMapNative<int, int> view;

            // SAFETY: map remains alive and unmodified while its borrowed native view is consumed.
            unsafe
            {
                view = map.AsNative();
            }

            Assert.IsTrue(view.IsCreated);
            Assert.AreEqual(2, view.Count);
            Assert.IsTrue(view.ContainsKey(1));
            Assert.IsTrue(view.TryGetValue(2, out var value));
            Assert.AreEqual(20, value);
        }

        [Test]
        public void ViewAdd_WithinCapacity_OwnerObservesChange()
        {
            using var map = new SharedArrayMap<int, int>(8);
            map.Add(1, 10);

            SharedArrayMapNative<int, int> view;

            // SAFETY: map remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = map.AsNative();
            }
            view.Add(2, 20);

            Assert.AreEqual(2, view.Count);
            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(20, map[2]);
        }

        [Test]
        public void MutableView_AllMembersMutateAndReportOwnerStorage()
        {
            using var map = new SharedArrayMap<int, int>(16);
            SharedArrayMapNative<int, int> view;

            // SAFETY: map remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = map.AsNative();
            }
            var value = 10;

            view.Add(1, in value);

            Assert.IsTrue(view.IsCreated);
            Assert.GreaterOrEqual(view.Capacity, 16);
            Assert.AreEqual(1, view.Count);
            // SAFETY: map remains alive and structurally unchanged while its value slice is consumed.
            unsafe
            {
                Assert.AreEqual(1, view.Values.Length);
                Assert.AreEqual(10, view.Values[0]);
            }
            Assert.AreEqual(10, view[1]);

            view[1] = 11;
            view[2] = 20;

            Assert.AreEqual(11, map[1]);
            Assert.AreEqual(20, map[2]);
            Assert.IsTrue(view.TryAdd(3, 30));
            Assert.IsFalse(view.TryAdd(3, 31));
            Assert.IsTrue(view.TryAdd(4, 40, out var fourthIndex));
            Assert.AreEqual(3, fourthIndex);
            Assert.IsFalse(view.TryAdd(4, 41, out var existingIndex));
            Assert.AreEqual(fourthIndex, existingIndex);

            int sixthIndex;

            // SAFETY: map owns the referenced values and no structural mutation occurs between each use.
            unsafe
            {
                view.GetOrAdd(5) = 50;
                view.GetOrAdd(6, out sixthIndex) = 60;
            }

            Assert.AreEqual(5, sixthIndex);
            Assert.AreEqual(50, map[5]);
            Assert.AreEqual(60, map[6]);

            // SAFETY: map owns the referenced value and is not structurally modified while it is used.
            unsafe
            {
                ref var first = ref view.GetValueByRef(1);
                first = 12;
            }

            Assert.AreEqual(12, map[1]);
            Assert.IsTrue(view.ContainsKey(2));
            Assert.IsFalse(view.ContainsKey(99));
            Assert.IsTrue(view.TryGetValue(3, out var foundValue));
            Assert.AreEqual(30, foundValue);
            Assert.IsFalse(view.TryGetValue(99, out var missingValue));
            Assert.AreEqual(0, missingValue);
            Assert.IsTrue(view.TryFindIndex(4, out var foundIndex));
            Assert.AreEqual(fourthIndex, foundIndex);
            Assert.IsFalse(view.TryFindIndex(99, out _));
            Assert.AreEqual(sixthIndex, view.GetIndex(6));

            Assert.IsTrue(view.Remove(2));
            Assert.IsFalse(view.Remove(99));
            Assert.IsTrue(view.Remove(3, out var removedIndex, out var removedValue));
            Assert.GreaterOrEqual(removedIndex, 0);
            Assert.AreEqual(30, removedValue);
            Assert.IsFalse(view.Remove(99, out _, out _));

            view.Clear();

            Assert.AreEqual(0, view.Count);
            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void SetOperations_IntersectExcludeAndUnionMutateOwnerStorage()
        {
            using var intersectedOwner = CreateMap(1, 2, 3);
            using var intersectKeysOwner = new SharedArrayMap<int, long>(8);
            intersectKeysOwner.Add(2, 20);
            intersectKeysOwner.Add(3, 30);
            intersectKeysOwner.Add(4, 40);
            SharedArrayMapNative<int, int> intersected;
            SharedArrayMapNative<int, long> intersectKeys;

            // SAFETY: Both owners remain alive while their borrowed native views are consumed.
            unsafe
            {
                intersected = intersectedOwner.AsNative();
                intersectKeys = intersectKeysOwner.AsNative();
            }

            intersected.Intersect(in intersectKeys);

            Assert.AreEqual(2, intersectedOwner.Count);
            Assert.IsTrue(intersectedOwner.ContainsKey(2));
            Assert.IsTrue(intersectedOwner.ContainsKey(3));

            using var excludedOwner = CreateMap(1, 2, 3);
            using var excludeKeysOwner = new SharedArrayMap<int, long>(8);
            excludeKeysOwner.Add(2, 20);
            excludeKeysOwner.Add(3, 30);
            SharedArrayMapNative<int, int> excluded;
            SharedArrayMapNative<int, long> excludeKeys;

            // SAFETY: Both owners remain alive while their borrowed native views are consumed.
            unsafe
            {
                excluded = excludedOwner.AsNative();
                excludeKeys = excludeKeysOwner.AsNative();
            }

            excluded.Exclude(in excludeKeys);

            Assert.AreEqual(1, excludedOwner.Count);
            Assert.IsTrue(excludedOwner.ContainsKey(1));

            using var unionOwner = CreateMap(1, 2);
            using var unionSourceOwner = new SharedArrayMap<int, int>(8);
            unionSourceOwner.Add(2, 200);
            unionSourceOwner.Add(3, 300);
            SharedArrayMapNative<int, int> union;
            SharedArrayMapNative<int, int> unionSource;

            // SAFETY: Both owners remain alive while their borrowed native views are consumed.
            unsafe
            {
                union = unionOwner.AsNative();
                unionSource = unionSourceOwner.AsNative();
            }

            union.Union(in unionSource);

            Assert.AreEqual(3, unionOwner.Count);
            Assert.AreEqual(10, unionOwner[1]);
            Assert.AreEqual(200, unionOwner[2]);
            Assert.AreEqual(300, unionOwner[3]);
        }

        [Test]
        public void ReadOnly_ViewConversionPropertiesAndLookupsReflectOwner()
        {
            using var map = new SharedArrayMap<int, int>(8);
            map.Add(1, 10);
            map.Add(2, 20);
            SharedArrayMapNative<int, int> view;
            SharedArrayMapNative<int, int>.ReadOnly direct;
            SharedArrayMapNative<int, int>.ReadOnly converted;

            // SAFETY: map remains alive and structurally unchanged while its borrowed aliases are consumed.
            unsafe
            {
                view = map.AsNative();
                direct = view.AsReadOnly();
                converted = view;
            }

            Assert.IsTrue(direct.IsCreated);
            Assert.AreEqual(view.Capacity, direct.Capacity);
            Assert.AreEqual(2, direct.Count);
            Assert.IsTrue(direct.Keys.IsValid);
            Assert.AreEqual(2, direct.Values.Length);
            Assert.AreEqual(10, direct.Values[0]);
            Assert.AreEqual(10, direct[1]);
            Assert.IsTrue(direct.ContainsKey(2));
            Assert.IsFalse(direct.ContainsKey(99));
            Assert.IsTrue(direct.TryGetValue(2, out var foundValue));
            Assert.AreEqual(20, foundValue);
            Assert.IsFalse(direct.TryGetValue(99, out var missingValue));
            Assert.AreEqual(0, missingValue);
            Assert.IsTrue(direct.TryFindIndex(2, out var foundIndex));
            Assert.AreEqual(1, foundIndex);
            Assert.IsFalse(direct.TryFindIndex(99, out _));
            Assert.AreEqual(0, direct.GetIndex(1));
            Assert.AreEqual(10, converted[1]);

            var version = direct.Version;
            // SAFETY: map owns the referenced value and is not structurally modified while it is used.
            unsafe
            {
                view.GetValueByRef(1) = 11;
            }

            Assert.Greater(direct.Version, version);
            Assert.AreEqual(11, direct[1]);
        }

        [Test]
        public void EnumeratorsAndPairs_DirectConstructionAndLifecycleExposeExpectedValues()
        {
            using var map = new SharedArrayMap<int, int>(8);
            map.Add(7, 70);

            // SAFETY: map and values remain alive and structurally unchanged while all borrowed views are consumed.
            unsafe
            {
            var view = map.AsNative();
            var keysFromView = view.Keys;
            var keys = new SharedArrayMapNative<int, int>.KeyEnumerable(in view);
            var keyEnumerator = new SharedArrayMapNative<int, int>.KeyEnumerator(in view);

            var keyFromViewEnumerator = keysFromView.GetEnumerator();
            Assert.IsTrue(keyFromViewEnumerator.MoveNext());
            Assert.AreEqual(7, keyFromViewEnumerator.Current);
            Assert.IsTrue(keyEnumerator.IsValid);
            Assert.IsTrue(keyEnumerator.MoveNext());
            Assert.AreEqual(7, keyEnumerator.Current);
            Assert.IsFalse(keyEnumerator.MoveNext());

            var keysEnumerator = keys.GetEnumerator();
            Assert.IsTrue(keysEnumerator.MoveNext());
            Assert.AreEqual(7, keysEnumerator.Current);

            var ownerEnumerator = new SharedArrayMapNativeKeyValueEnumerator<int, int>(view);
            Assert.IsTrue(ownerEnumerator.IsValid);
            Assert.IsTrue(ownerEnumerator.MoveNext());
            AssertPair(ownerEnumerator.Current, 7, 70);
            Assert.IsFalse(ownerEnumerator.MoveNext());
            ownerEnumerator.Reset();
            Assert.IsTrue(ownerEnumerator.MoveNext());
            IEnumerator ownerInterface = ownerEnumerator;
            Assert.IsInstanceOf<SharedArrayMapNativeKeyValuePair<int, int>>(ownerInterface.Current);
            ownerEnumerator.Dispose();

            var viewEnumerator = view.GetEnumerator();
            Assert.IsTrue(viewEnumerator.MoveNext());
            viewEnumerator.Dispose();

            using var values = new NativeArray<int>(new[] { 80, 90 }, Allocator.Temp);
            var pairKey = 8;
            var pair = new SharedArrayMapNativeKeyValuePair<int, int>(in pairKey, values, 1);
            AssertPair(pair, 8, 90);

            var readOnly = view.AsReadOnly();
            var readOnlyKeys = new SharedArrayMapNative<int, int>.ReadOnly.KeyEnumerable(in readOnly);
            var readOnlyKeyEnumerator =
                new SharedArrayMapNative<int, int>.ReadOnly.KeyEnumerator(in readOnly);

            Assert.IsTrue(readOnlyKeys.IsValid);
            Assert.IsTrue(readOnlyKeyEnumerator.IsValid);
            Assert.IsTrue(readOnlyKeyEnumerator.MoveNext());
            Assert.AreEqual(7, readOnlyKeyEnumerator.Current);
            Assert.IsFalse(readOnlyKeyEnumerator.MoveNext());
            readOnlyKeyEnumerator.Reset();
            Assert.IsTrue(readOnlyKeyEnumerator.MoveNext());
            readOnlyKeyEnumerator.Dispose();

            var readOnlyKeysEnumerator = readOnlyKeys.GetEnumerator();
            Assert.IsTrue(readOnlyKeysEnumerator.MoveNext());
            readOnlyKeysEnumerator.Dispose();

            IEnumerable<int> genericKeys = readOnlyKeys;
            var genericKeysEnumerator = genericKeys.GetEnumerator();
            Assert.IsTrue(genericKeysEnumerator.MoveNext());
            Assert.AreEqual(7, genericKeysEnumerator.Current);
            genericKeysEnumerator.Dispose();

            IEnumerable nonGenericKeys = readOnlyKeys;
            var nonGenericKeysEnumerator = nonGenericKeys.GetEnumerator();
            Assert.IsTrue(nonGenericKeysEnumerator.MoveNext());
            Assert.AreEqual(7, nonGenericKeysEnumerator.Current);

            var readOnlyEnumerator =
                new SharedArrayMapNativeReadOnlyKeyValueEnumerator<int, int>(in readOnly);
            Assert.IsTrue(readOnlyEnumerator.IsValid);
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            AssertPair(readOnlyEnumerator.Current, 7, 70);
            Assert.IsFalse(readOnlyEnumerator.MoveNext());
            readOnlyEnumerator.Reset();
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            IEnumerator readOnlyInterface = readOnlyEnumerator;
            Assert.IsInstanceOf<SharedArrayMapNativeReadOnlyKeyValuePair<int, int>>(readOnlyInterface.Current);
            readOnlyEnumerator.Dispose();

            var readOnlyViewEnumerator = readOnly.GetEnumerator();
            Assert.IsTrue(readOnlyViewEnumerator.MoveNext());
            readOnlyViewEnumerator.Dispose();

            var readOnlyPair = new SharedArrayMapNativeReadOnlyKeyValuePair<int, int>(
                  in pairKey
                , values.AsReadOnly()
                , 0
            );
            AssertPair(readOnlyPair, 8, 80);

            Assert.IsFalse(default(SharedArrayMapNative<int, int>.KeyEnumerator).IsValid);
            Assert.IsFalse(default(SharedArrayMapNative<int, int>).IsCreated);
            Assert.IsFalse(default(SharedArrayMapNativeKeyValueEnumerator<int, int>).IsValid);
            Assert.IsFalse(default(SharedArrayMapNative<int, int>.ReadOnly).IsCreated);
            Assert.IsFalse(default(SharedArrayMapNative<int, int>.ReadOnly.KeyEnumerable).IsValid);
            Assert.IsFalse(default(SharedArrayMapNative<int, int>.ReadOnly.KeyEnumerator).IsValid);
            Assert.IsFalse(default(SharedArrayMapNativeReadOnlyKeyValueEnumerator<int, int>).IsValid);
            Assert.IsFalse(default(SharedArrayMapNativeReadOnlyKeyValuePair<int, int>).IsValid);
            }
        }

        [Test]
        public void CurrentViewAliasesOwnerAndRefreshedPostGrowViewUsesNewStorage()
        {
            using var map = new SharedArrayMap<int, int>(2);
            map.Add(0, 0);
            SharedArrayMapNative<int, int> current;

            // SAFETY: map remains alive while its current borrowed native view is consumed.
            unsafe
            {
                current = map.AsNative();
            }

            current[0] = 10;

            Assert.AreEqual(10, map[0]);

            for (var i = 1; i < 64; i++)
            {
                map.Add(i, i);
            }

            SharedArrayMapNative<int, int> refreshed;

            // SAFETY: map remains alive while the refreshed borrowed native view is consumed.
            unsafe
            {
                refreshed = map.AsNative();
            }
            refreshed[63] = 630;

            Assert.AreEqual(map.Capacity, refreshed.Capacity);
            Assert.AreEqual(630, map[63]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void FixedCapacityView_CannotGrowOwnerStorage()
        {
            using var map = new SharedArrayMap<int, int>(2);
            SharedArrayMapNative<int, int> view;

            // SAFETY: map remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = map.AsNative();
            }

            for (var i = 0; i < view.Capacity; i++)
            {
                view.Add(i, i * 10);
            }

            var capacity = map.Capacity;
            var count = map.Count;

            Assert.Throws<InvalidOperationException>(() => view.Add(count, count * 10));
            Assert.AreEqual(capacity, map.Capacity);
            Assert.AreEqual(count, map.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void OwnerDispose_InvalidatesMutableAndReadOnlyViews()
        {
            var map = new SharedArrayMap<int, int>(4);
            map.Add(1, 10);
            SharedArrayMapNative<int, int> view;
            SharedArrayMapNative<int, int>.ReadOnly readOnly;

            // SAFETY: map remains alive until this test deliberately invalidates both borrowed views.
            unsafe
            {
                view = map.AsNative();
                readOnly = view.AsReadOnly();
            }

            map.Dispose();

            Assert.Catch(() => _ = view.Count);
            Assert.Catch(() => _ = readOnly.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ViewCreatedBeforeOwnerResize_ThrowsOnWrite()
        {
            // The view copies the owner buffer's safety handle at AsNative() time. Growing
            // the owner past capacity re-pins the buffers and releases that handle, so a
            // write through the stale view must throw.
            var map = new SharedArrayMap<int, int>(2);

            try
            {
                map.Add(0, 0);

                SharedArrayMapNative<int, int> staleView;

                // SAFETY: The view is intentionally retained across growth to verify safety-handle invalidation.
                unsafe
                {
                    staleView = map.AsNative();
                }

                for (var i = 1; i < 64; i++)
                {
                    map.Add(i, i); // forces growth -> re-pin -> old handle released
                }

                Assert.Catch(() => staleView.Add(999, 999));
            }
            finally
            {
                map.Dispose();
            }
        }

        private static SharedArrayMap<int, int> CreateMap(params int[] keys)
        {
            var map = new SharedArrayMap<int, int>(8);

            for (var i = 0; i < keys.Length; i++)
            {
                map.Add(keys[i], keys[i] * 10);
            }

            return map;
        }

        private static void AssertPair(SharedArrayMapNativeKeyValuePair<int, int> pair, int key, int value)
        {
            Assert.AreEqual(key, pair.Key);
            Assert.AreEqual(value, pair.Value);

            pair.Deconstruct(out var actualKey, out var actualValue);

            Assert.AreEqual(key, actualKey);
            Assert.AreEqual(value, actualValue);
        }

        private static void AssertPair(SharedArrayMapNativeReadOnlyKeyValuePair<int, int> pair, int key, int value)
        {
            Assert.IsTrue(pair.IsValid);
            Assert.AreEqual(key, pair.Key);
            Assert.AreEqual(value, pair.Value);

            pair.Deconstruct(out var actualKey, out var actualValue);

            Assert.AreEqual(key, actualKey);
            Assert.AreEqual(value, actualValue);
        }

        [Test]
        public void ViewAdd_CollisionChurnPastBucketCount_DoesNotThrowAndPreservesContent()
        {
            using var map = new SharedArrayMap<CollidingKey, int>(8);
            SharedArrayMapNative<CollidingKey, int> view;

            // SAFETY: map remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = map.AsNative();
            }

            view.Add(new CollidingKey(1), 10);
            view.Add(new CollidingKey(2), 20); // cumulative collision 1

            for (var i = 0; i < 11; i++) // collisions 2..12; the add at i == 10 crosses 11
            {
                view.Add(new CollidingKey(3), 30);
                Assert.IsTrue(view.Remove(new CollidingKey(3)));
            }

            Assert.AreEqual(2, view.Count);
            Assert.IsTrue(view.TryGetValue(new CollidingKey(1), out var value1));
            Assert.AreEqual(10, value1);
            Assert.IsTrue(view.TryGetValue(new CollidingKey(2), out var value2));
            Assert.AreEqual(20, value2);

            view.Add(new CollidingKey(4), 40);

            Assert.AreEqual(3, view.Count);
            Assert.AreEqual(40, view[new CollidingKey(4)]);
            Assert.AreEqual(3, map.Count);
        }

        [Test]
        public unsafe void Constructor_BorrowedHeader_ViewsTheHeaderAndAcceptsANullHeader()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            var safety = AtomicSafetyHandle.Create();
            var view = new SharedArrayMapNative<int, int>(&header, safety);
            var nullView = new SharedArrayMapNative<int, int>(null, safety);
            var nullReadOnly = new SharedArrayMapNative<int, int>.ReadOnly(null, safety);
#else
            var view = new SharedArrayMapNative<int, int>(&header);
            var nullView = new SharedArrayMapNative<int, int>(null);
            var nullReadOnly = new SharedArrayMapNative<int, int>.ReadOnly(null);
#endif

            try
            {
                Assert.IsTrue(view.IsCreated);
                Assert.IsFalse(nullView.IsCreated);
                Assert.IsFalse(nullReadOnly.IsCreated);
                Assert.AreEqual(4, view.Capacity);
                Assert.AreEqual(3, view.BucketCapacity);
                Assert.AreEqual(0, view.Count);

                var version = view.Version;

                view.Add(1, 10);

                Assert.AreEqual(1, view.Count);
                Assert.AreEqual(1, storage.Count);
                Assert.AreNotEqual(version, view.Version);
                Assert.AreEqual(10, view[1]);
            }
            finally
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                AtomicSafetyHandle.Release(safety);
#endif
            }
        }

        [Test]
        public unsafe void InternalBuffers_CoverTheCapacityAndExposeTheOwnerEntries()
        {
            using var owner = CreateMap(1, 2);
            var view = owner.AsNative();

            Assert.AreEqual(view.Capacity, view._values.Length);
            Assert.AreEqual(view.Capacity, view._valuesInfo.Length);
            Assert.GreaterOrEqual(view.BucketCapacity, 1);
            Assert.AreEqual(1, view._valuesInfo[0].key);
            Assert.AreEqual(2, view._valuesInfo[1].key);
            Assert.AreEqual(10, view._values[0]);
            Assert.AreEqual(20, view._values[1]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void InternalMembers_AfterOwnerDispose_Throw()
        {
            var owner = CreateMap(1, 2);
            var view = owner.AsNative();

            owner.Dispose();

            Assert.Catch(() => _ = view.Version);
            Assert.Catch(() => _ = view.BucketCapacity);
            Assert.Catch(() => _ = view._values);
            Assert.Catch(() => _ = view._valuesInfo);
        }

        [Test]
        public unsafe void Values_CountBelowCapacity_LengthIsCountAndSeesLaterWrites()
        {
            using var owner = CreateMap(1, 2);
            var view = owner.AsNative();

            var values = view.Values;

            Assert.AreEqual(2, values.Length);
            Assert.GreaterOrEqual(view.Capacity, 8);
            CollectionAssert.AreEqual(new[] { 10, 20 }, values.ToArray());

            view.GetValueByRef(2) = 99;

            Assert.AreEqual(99, values[1]);
            Assert.AreEqual(99, owner[2]);
        }

        [Test]
        public unsafe void Values_ElementShapes_RoundTripBitExact()
        {
            using var small = new SharedArrayMap<int, Rgb>(4);
            using var wide = new SharedArrayMap<int, Wide>(4);
            using var longs = new SharedArrayMap<long, long>(4);
            small.Add(1, new Rgb { r = 1, g = 2, b = 3 });
            small.Add(2, new Rgb { r = 250, g = 251, b = 252 });
            wide.Add(1, new Wide { a = long.MinValue, b = -1L, c = long.MaxValue });
            longs.Add(long.MaxValue, long.MinValue);
            var smallView = small.AsNative();
            var wideView = wide.AsNative();
            var longView = longs.AsNative();

            Assert.AreEqual(2, smallView.Values.Length);
            Assert.AreEqual(3, smallView.Values[0].b);
            Assert.AreEqual(250, smallView.Values[1].r);
            Assert.AreEqual(252, smallView[2].b);
            Assert.AreEqual(long.MinValue, wideView.Values[0].a);
            Assert.AreEqual(-1L, wideView[1].b);
            Assert.AreEqual(long.MaxValue, wideView[1].c);
            Assert.AreEqual(long.MinValue, longView[long.MaxValue]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Values_SliceUsedAfterOwnerDispose_Throws()
        {
            var owner = CreateMap(1, 2);
            var view = owner.AsNative();
            var values = view.Values;

            owner.Dispose();

            Assert.Catch(() => _ = values[0]);
            Assert.Catch(() => _ = view.Values);
        }

        [Test]
        public unsafe void EmptyMap_Queries_ReturnEmptyResults()
        {
            using var owner = new SharedArrayMap<int, int>(4);
            var view = owner.AsNative();
            var keyCount = 0;

            foreach (var key in view.Keys)
            {
                keyCount++;
            }

            var enumerator = view.GetEnumerator();

            Assert.AreEqual(0, keyCount);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, view.Count);
            Assert.AreEqual(0, view.Values.Length);
            Assert.IsFalse(view.ContainsKey(1));
            Assert.IsFalse(view.TryGetValue(1, out var value));
            Assert.AreEqual(0, value);
            Assert.IsFalse(view.TryFindIndex(1, out _));
            Assert.IsFalse(view.Remove(1));
            Assert.IsFalse(view.Remove(1, out var index, out var removed));
            Assert.AreEqual(0, index);
            Assert.AreEqual(0, removed);
            Assert.DoesNotThrow(() => view.Clear());
            Assert.AreEqual(0, view.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void MissingKey_IndexerGetIndexAndGetValueByRef_ThrowKeyNotFound()
        {
            using var owner = CreateMap(1);
            var view = owner.AsNative();

            Assert.Throws<KeyNotFoundException>(() => _ = view[5]);
            Assert.Throws<KeyNotFoundException>(() => view.GetIndex(5));
            Assert.Throws<KeyNotFoundException>(() => view.GetValueByRef(5));
            Assert.AreEqual(0, view.GetIndex(1));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Add_ExactlyFullThenOnePast_ThrowsAndKeepsTheContent()
        {
            using var owner = new SharedArrayMap<int, int>(3);
            owner.Add(1, 10);
            owner.Add(2, 20);
            owner.Add(3, 30);
            var view = owner.AsNative();
            var value = 40;

            Assert.AreEqual(3, view.Count);
            Assert.Throws<InvalidOperationException>(() => view.Add(4, 40));
            Assert.Throws<InvalidOperationException>(() => view.Add(4, in value));
            Assert.Throws<InvalidOperationException>(() => view.TryAdd(4, 40));
            Assert.Throws<InvalidOperationException>(() => view.TryAdd(4, 40, out _));
            Assert.Throws<InvalidOperationException>(() => view[4] = 40);
            Assert.Throws<InvalidOperationException>(() => view.GetOrAdd(4));
            Assert.Throws<InvalidOperationException>(() => view.GetOrAdd(4, out _));
            Assert.IsFalse(view.TryAdd(2, 99));
            view[2] = 21;

            Assert.AreEqual(21, owner[2]);
            Assert.AreEqual(3, owner.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Add_DuplicateKey_ThrowsAndKeepsTheValue()
        {
            using var owner = CreateMap(1);
            var view = owner.AsNative();

            Assert.Throws<InvalidOperationException>(() => view.Add(1, 20));
            Assert.AreEqual(10, owner[1]);
        }

        [Test]
        public unsafe void GetOrAdd_ExistingAndMissingKeys_ReturnReferencesIntoTheOwnerValues()
        {
            using var owner = CreateMap(1);
            var view = owner.AsNative();

            ref var existing = ref view.GetOrAdd(1);
            existing = 11;

            ref var added = ref view.GetOrAdd(2, out var index);
            added = 22;

            ref var again = ref view.GetOrAdd(2);

            Assert.AreEqual(1, index);
            Assert.AreEqual(22, again);
            Assert.AreEqual(11, owner[1]);
            Assert.AreEqual(22, owner[2]);
            Assert.AreEqual(2, owner.Count);
        }

        [Test]
        public unsafe void Remove_FirstKey_MovesTheLastEntryIntoTheHole()
        {
            using var owner = CreateMap(1, 2, 3, 4);
            var view = owner.AsNative();

            Assert.IsTrue(view.Remove(1, out var index, out var removed));

            Assert.AreEqual(0, index);
            Assert.AreEqual(10, removed);
            Assert.AreEqual(3, view.Count);
            CollectionAssert.AreEqual(new[] { 40, 20, 30 }, view.Values.ToArray());
            Assert.AreEqual(0, view.GetIndex(4));
            Assert.AreEqual(1, view.GetIndex(2));
            Assert.IsFalse(view.ContainsKey(1));
            Assert.IsFalse(view.Remove(1));
        }

        [Test]
        public unsafe void Remove_LastRemainingKey_LeavesAnEmptyMapThatAcceptsNewKeys()
        {
            using var owner = new SharedArrayMap<int, int>(2);
            owner.Add(1, 10);
            var view = owner.AsNative();

            Assert.IsTrue(view.Remove(1));
            Assert.AreEqual(0, view.Count);

            view.Add(1, 11);
            view.Add(2, 22);

            Assert.AreEqual(2, owner.Count);
            Assert.AreEqual(11, owner[1]);
            Assert.AreEqual(22, owner[2]);
        }

        [Test]
        public unsafe void SetOperations_EmptyOperands_ProduceTheSetResult()
        {
            using var owner = CreateMap(1, 2);
            using var emptyOwner = new SharedArrayMap<int, int>(4);
            using var emptyLongOwner = new SharedArrayMap<int, long>(4);
            var view = owner.AsNative();
            var empty = emptyOwner.AsNative();
            var emptyLong = emptyLongOwner.AsNative();

            view.Exclude(in empty);
            view.Union(in empty);
            view.Exclude(in emptyLong);

            Assert.AreEqual(2, view.Count);
            Assert.AreEqual(10, view[1]);
            Assert.AreEqual(20, view[2]);

            view.Intersect(in emptyLong);

            Assert.AreEqual(0, view.Count);

            empty.Intersect(in view);
            empty.Exclude(in view);
            empty.Union(in empty);

            Assert.AreEqual(0, empty.Count);
        }

        [Test]
        public unsafe void SetOperations_DefaultOperand_Throws()
        {
            using var owner = CreateMap(1);
            var view = owner.AsNative();
            SharedArrayMapNative<int, int> notCreated = default;
            SharedArrayMapNative<int, long> notCreatedLongs = default;

            Assert.Throws<ArgumentException>(() => view.Intersect(in notCreatedLongs));
            Assert.Throws<ArgumentException>(() => view.Exclude(in notCreatedLongs));
            Assert.Throws<ArgumentException>(() => view.Union(in notCreated));
        }

        [Test]
        public unsafe void CopyTo_FilledMap_CopiesEveryBufferAndReportsTheState()
        {
            using var owner = CreateMap(1, 2);
            var view = owner.AsNative();
            var valuesInfo = new ArrayMapNode<int>[view.Capacity];
            var values = new int[view.Capacity];
            var buckets = new int[view.BucketCapacity];

            view.CopyTo(valuesInfo, values, buckets, out var count, out _, out _);

            Assert.AreEqual(2, count);
            Assert.AreEqual(1, valuesInfo[0].key);
            Assert.AreEqual(2, valuesInfo[1].key);
            Assert.AreEqual(10, values[0]);
            Assert.AreEqual(20, values[1]);
        }

        [Test]
        public unsafe void CopyTo_EmptyMapAndLargerDestination_ReportsZero()
        {
            using var owner = new SharedArrayMap<int, int>(4);
            var view = owner.AsNative();

            view.CopyTo(
                  new ArrayMapNode<int>[view.Capacity + 3]
                , new int[view.Capacity + 3]
                , new int[view.BucketCapacity + 3]
                , out var count
                , out var collisions
                , out _
            );

            Assert.AreEqual(0, count);
            Assert.AreEqual(0u, collisions);
        }

        [Test]
        public unsafe void CopyTo_DestinationShorterThanTheStorage_ThrowsArgumentException()
        {
            using var owner = CreateMap(1);
            var view = owner.AsNative();
            var capacity = view.Capacity;
            var bucketCapacity = view.BucketCapacity;

            var info = new ArrayMapNode<int>[capacity];
            var values = new int[capacity];
            var buckets = new int[bucketCapacity];
            var shortInfo = new ArrayMapNode<int>[capacity - 1];
            var shortValues = new int[capacity - 1];
            var shortBuckets = new int[bucketCapacity - 1];

            Assert.Throws<ArgumentException>(
                () => view.CopyTo(shortInfo, values, buckets, out _, out _, out _)
            );
            Assert.Throws<ArgumentException>(
                () => view.CopyTo(info, shortValues, buckets, out _, out _, out _)
            );
            Assert.Throws<ArgumentException>(
                () => view.CopyTo(info, values, shortBuckets, out _, out _, out _)
            );
        }

        [Test]
        public unsafe void Keys_ModifiedDuringEnumeration_Throws()
        {
            using var owner = CreateMap(1, 2);
            var view = owner.AsNative();
            var enumerator = view.Keys.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            view.Add(3, 30);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public unsafe void EnumeratorsAndKeyEnumerable_FromDefaultView_Throw()
        {
            SharedArrayMapNative<int, int> notCreated = default;
            SharedArrayMapNative<int, int>.ReadOnly notCreatedReadOnly = default;

            Assert.Throws<ArgumentException>(
                () => _ = new SharedArrayMapNativeKeyValueEnumerator<int, int>(notCreated)
            );
            Assert.Throws<ArgumentException>(
                () => _ = new SharedArrayMapNativeReadOnlyKeyValueEnumerator<int, int>(in notCreatedReadOnly)
            );
            Assert.Throws<ArgumentException>(() => _ = new SharedArrayMapNative<int, int>.KeyEnumerable(in notCreated));
            Assert.Throws<ArgumentException>(() => _ = new SharedArrayMapNative<int, int>.KeyEnumerator(in notCreated));
            Assert.Throws<ArgumentException>(
                () => _ = new SharedArrayMapNative<int, int>.ReadOnly.KeyEnumerable(in notCreatedReadOnly)
            );
            Assert.Throws<ArgumentException>(
                () => _ = new SharedArrayMapNative<int, int>.ReadOnly.KeyEnumerator(in notCreatedReadOnly)
            );
        }

        [Test]
        public unsafe void KeyValueEnumerator_EmptyMapAndExhausted_StaysFinishedUntilReset()
        {
            using var empty = new SharedArrayMap<int, int>(4);
            using var owner = CreateMap(7);
            var emptyView = empty.AsNative();
            var view = owner.AsNative();
            var emptyEnumerator = emptyView.GetEnumerator();
            var enumerator = view.GetEnumerator();

            Assert.IsFalse(emptyEnumerator.MoveNext());
            Assert.IsTrue(enumerator.MoveNext());
            AssertPair(enumerator.Current, 7, 70);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
        }

        [Test]
        public unsafe void KeyValueEnumerator_ModifiedMap_MoveNextThrows()
        {
            using var owner = CreateMap(1, 2);
            var view = owner.AsNative();
            var enumerator = view.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            view.Add(3, 30);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public unsafe void KeyValuePair_FromANativeArray_ExposesTheKeyAndTheIndexedValue()
        {
            using var values = new NativeArray<int>(new[] { 80, 90 }, Allocator.Temp);
            using var wide = new NativeArray<Wide>(
                  new[] { new Wide { a = long.MinValue, b = 0, c = long.MaxValue } }
                , Allocator.Temp
            );
            var key = 8;
            var wideKey = 9;
            var pair = new SharedArrayMapNativeKeyValuePair<int, int>(in key, values, 0);
            var wideValue = new SharedArrayMapNativeKeyValuePair<int, Wide>(in wideKey, wide, 0);
            var readOnlyPair = new SharedArrayMapNativeReadOnlyKeyValuePair<int, int>(in key, values.AsReadOnly(), 1);

            AssertPair(pair, 8, 80);
            Assert.AreEqual(long.MinValue, wideValue.Value.a);
            Assert.AreEqual(long.MaxValue, wideValue.Value.c);
            AssertPair(readOnlyPair, 8, 90);
            Assert.IsFalse(default(SharedArrayMapNativeReadOnlyKeyValuePair<int, int>).IsValid);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void KeyValuePair_IndexOutsideTheArrayOrDisposedArray_Throws()
        {
            var values = new NativeArray<int>(new[] { 80, 90 }, Allocator.Persistent);
            var key = 8;
            var pair = new SharedArrayMapNativeKeyValuePair<int, int>(in key, values, 2);
            var negativePair = new SharedArrayMapNativeKeyValuePair<int, int>(in key, values, -1);
            var disposedPair = new SharedArrayMapNativeKeyValuePair<int, int>(in key, values, 0);

            Assert.Throws<IndexOutOfRangeException>(() => _ = pair.Value);
            Assert.Throws<IndexOutOfRangeException>(() => _ = negativePair.Value);

            values.Dispose();

            Assert.Catch(() => _ = disposedPair.Value);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Members_AfterOwnerDispose_Throw()
        {
            var owner = CreateMap(1, 2);
            var other = CreateMap(1);
            var otherLongs = new SharedArrayMap<int, long>(4);
            var view = owner.AsNative();
            var otherView = other.AsNative();
            var otherLongView = otherLongs.AsNative();
            var enumerator = view.GetEnumerator();
            var value = 9;

            try
            {
                owner.Dispose();

                Assert.Catch(() => _ = view.Capacity);
                Assert.Catch(() => _ = view.Count);
                Assert.Catch(() => _ = view[1]);
                Assert.Catch(() => view[1] = 5);
                Assert.Catch(() => view.GetEnumerator());
                Assert.Catch(() => view.Keys.GetEnumerator());
                Assert.Catch(() => view.Add(5, 50));
                Assert.Catch(() => view.Add(5, in value));
                Assert.Catch(() => view.TryAdd(5, 50));
                Assert.Catch(() => view.TryAdd(5, 50, out _));
                Assert.Catch(() => view.Clear());
                Assert.Catch(() => view.ContainsKey(1));
                Assert.Catch(() => view.TryGetValue(1, out _));
                Assert.Catch(() => view.GetOrAdd(1));
                Assert.Catch(() => view.GetOrAdd(1, out _));
                Assert.Catch(() => view.GetValueByRef(1));
                Assert.Catch(() => view.Remove(1));
                Assert.Catch(() => view.Remove(1, out _, out _));
                Assert.Catch(() => view.TryFindIndex(1, out _));
                Assert.Catch(() => view.GetIndex(1));
                Assert.Catch(() => view.Intersect(in otherLongView));
                Assert.Catch(() => view.Exclude(in otherLongView));
                Assert.Catch(() => view.Union(in otherView));
                Assert.Catch(() => enumerator.MoveNext());
            }
            finally
            {
                other.Dispose();
                otherLongs.Dispose();
            }
        }

        [Test]
        public unsafe void ReadOnly_EmptyMap_ReturnsEmptyResults()
        {
            using var owner = new SharedArrayMap<int, int>(4);
            var readOnly = owner.AsNative().AsReadOnly();
            var keyCount = 0;

            foreach (var key in readOnly.Keys)
            {
                keyCount++;
            }

            var enumerator = readOnly.GetEnumerator();

            Assert.AreEqual(0, keyCount);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(0, readOnly.Count);
            Assert.AreEqual(4, readOnly.Capacity);
            Assert.AreEqual(0, readOnly.Values.Length);
            Assert.IsFalse(readOnly.ContainsKey(1));
            Assert.IsFalse(readOnly.TryGetValue(1, out _));
            Assert.IsFalse(readOnly.TryFindIndex(1, out _));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ReadOnly_MissingKey_IndexerAndGetIndexThrowKeyNotFound()
        {
            using var owner = CreateMap(1);
            var readOnly = owner.AsNative().AsReadOnly();

            Assert.Throws<KeyNotFoundException>(() => _ = readOnly[5]);
            Assert.Throws<KeyNotFoundException>(() => readOnly.GetIndex(5));
            Assert.AreEqual(0, readOnly.GetIndex(1));
        }

        [Test]
        public unsafe void ReadOnly_Conversion_FromDefaultView_Throws()
        {
            SharedArrayMapNative<int, int> notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = (SharedArrayMapNative<int, int>.ReadOnly)notCreated);
        }

        [Test]
        public unsafe void ReadOnly_ValuesAndBuffers_LengthIsCountAndElementShapesRoundTrip()
        {
            using var small = new SharedArrayMap<int, Rgb>(4);
            small.Add(1, new Rgb { r = 1, g = 2, b = 3 });
            using var owner = CreateMap(1, 2);
            var smallView = small.AsNative().AsReadOnly();
            var readOnly = owner.AsNative().AsReadOnly();

            Assert.AreEqual(2, readOnly.Values.Length);
            CollectionAssert.AreEqual(new[] { 10, 20 }, readOnly.Values.ToArray());
            Assert.AreEqual(readOnly.Capacity, readOnly._values.Length);
            Assert.AreEqual(1, readOnly._valuesInfo[0].key);
            Assert.AreEqual(1, smallView.Values.Length);
            Assert.AreEqual(3, smallView.Values[0].b);
            Assert.AreEqual(2, smallView[1].g);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ReadOnly_Members_AfterOwnerDispose_Throw()
        {
            var owner = CreateMap(1, 2);
            var readOnly = owner.AsNative().AsReadOnly();
            var enumerator = readOnly.GetEnumerator();
            var values = readOnly.Values;

            owner.Dispose();

            Assert.Catch(() => _ = readOnly.Capacity);
            Assert.Catch(() => _ = readOnly.Count);
            Assert.Catch(() => _ = readOnly.Version);
            Assert.Catch(() => _ = readOnly.Values);
            Assert.Catch(() => _ = values[0]);
            Assert.Catch(() => _ = readOnly[1]);
            Assert.Catch(() => readOnly.GetEnumerator());
            Assert.Catch(() => readOnly.Keys.GetEnumerator());
            Assert.Catch(() => readOnly.ContainsKey(1));
            Assert.Catch(() => readOnly.TryGetValue(1, out _));
            Assert.Catch(() => readOnly.GetIndex(1));
            Assert.Catch(() => readOnly.TryFindIndex(1, out _));
            Assert.Catch(() => _ = readOnly._values);
            Assert.Catch(() => _ = readOnly._valuesInfo);
            Assert.Catch(() => enumerator.MoveNext());
        }

        [Test]
        public unsafe void ReadOnly_ModifiedDuringEnumeration_Throws()
        {
            using var owner = CreateMap(1, 2);
            var view = owner.AsNative();
            var readOnly = view.AsReadOnly();
            var enumerator = readOnly.GetEnumerator();
            var keyEnumerator = readOnly.Keys.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(keyEnumerator.MoveNext());

            view.Add(3, 30);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => keyEnumerator.MoveNext());
        }

        [Test]
        public unsafe void ElementShapes_OwnerViewsRoundTripBitExactThroughTheView()
        {
            using var bytes = new SharedArrayMap<int, byte>(4);
            var view = bytes.AsNative();

            view.Add(1, 0xA5);
            view[2] = 0xFF;
            view.GetOrAdd(3) = 0x00;

            Assert.AreEqual(0xA5, bytes[1]);
            Assert.AreEqual(0xFF, bytes[2]);
            Assert.AreEqual(0x00, bytes[3]);
            CollectionAssert.AreEqual(new byte[] { 0xA5, 0xFF, 0x00 }, view.Values.ToArray());
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
