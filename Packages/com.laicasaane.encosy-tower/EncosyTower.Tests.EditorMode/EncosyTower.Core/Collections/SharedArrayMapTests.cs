using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from Unity.Collections.Tests/NativeHashMapTests.cs against the managed
    // SharedArrayMap owner.
    public partial class SharedArrayMapTests
    {
        [Test]
        public void Constructor_IsEmpty()
        {
            using var map = new SharedArrayMap<int, int>(8);

            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void Add_StoresValues()
        {
            using var map = new SharedArrayMap<int, int>(8);

            map.Add(1, 10);
            map.Add(2, 20);

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(20, map[2]);
        }

        [Test]
        public void TryAdd_ReturnsFalseOnDuplicate()
        {
            using var map = new SharedArrayMap<int, int>(8);

            Assert.IsTrue(map.TryAdd(1, 10));
            Assert.IsFalse(map.TryAdd(1, 20));
            Assert.AreEqual(10, map[1]);
        }

        [Test]
        public void ContainsKey_ReflectsContent()
        {
            using var map = new SharedArrayMap<int, int>(8);
            map.Add(1, 10);

            Assert.IsTrue(map.ContainsKey(1));
            Assert.IsFalse(map.ContainsKey(2));
        }

        [Test]
        public void TryGetValue_ReturnsExpected()
        {
            using var map = new SharedArrayMap<int, int>(8);
            map.Add(1, 10);

            Assert.IsTrue(map.TryGetValue(1, out var value));
            Assert.AreEqual(10, value);
            Assert.IsFalse(map.TryGetValue(99, out _));
        }

        [Test]
        public void GetOrAdd_AddsWhenMissing()
        {
            using var map = new SharedArrayMap<int, int>(8);

            // SAFETY: The map owns the referenced value for the duration of this block.
            unsafe
            {
                ref var slot = ref map.GetOrAdd(1);
                slot = 42;
            }

            Assert.AreEqual(42, map[1]);
        }

        [Test]
        public void Indexer_Set_AddsOrOverwrites()
        {
            using var map = new SharedArrayMap<int, int>(8);

            map[1] = 10;
            Assert.AreEqual(10, map[1]);

            map[1] = 99;
            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(99, map[1]);
        }

        [Test]
        public void Remove_RemovesKey()
        {
            using var map = new SharedArrayMap<int, int>(8);
            map.Add(1, 10);
            map.Add(2, 20);

            Assert.IsTrue(map.Remove(1));
            Assert.IsFalse(map.Remove(99));

            Assert.AreEqual(1, map.Count);
            Assert.IsFalse(map.ContainsKey(1));
            Assert.AreEqual(20, map[2]);
        }

        [Test]
        public void Clear_ResetsCount()
        {
            using var map = new SharedArrayMap<int, int>(8);
            map.Add(1, 10);
            map.Add(2, 20);

            map.Clear();

            Assert.AreEqual(0, map.Count);
            Assert.IsFalse(map.ContainsKey(1));
        }

        [Test]
        public void Grow_PastInitialCapacity_KeepsAllEntries()
        {
            const int N = 200;
            using var map = new SharedArrayMap<int, int>(4);

            for (var i = 0; i < N; i++)
            {
                map.Add(i, i * 3);
            }

            Assert.AreEqual(N, map.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.AreEqual(i * 3, map[i], $"key {i}");
            }
        }

        [Test]
        public void ConstructorsAndConversions_AllOwnerFormsCopyExpectedStorage()
        {
            using var convenienceDefault = new SharedArrayMap<int, int>();
            using var convenienceCapacity = new SharedArrayMap<int, int>(4);
            using var genericDefault = new SharedArrayMap<int, int, uint>();
            using var genericCapacity = new SharedArrayMap<int, int, uint>(4);
            genericCapacity.Add(1, -1);

            using var ownerCopy = new SharedArrayMap<int, int, uint>(genericCapacity);
            SharedArrayMapNative<int, uint> native;
            SharedArrayMapNative<int, uint> converted;
            SharedArrayMap<int, int, uint> nativeCopy;

            // SAFETY: genericCapacity remains alive while its borrowed native views are created and copied.
            unsafe
            {
                native = genericCapacity.AsNative();
                converted = genericCapacity;
                nativeCopy = new SharedArrayMap<int, int, uint>(in native);
            }

            using (nativeCopy)
            {
                Assert.AreEqual(0, convenienceDefault.Count);
                Assert.GreaterOrEqual(convenienceCapacity.Capacity, 4);
                Assert.AreEqual(0, genericDefault.Count);
                Assert.GreaterOrEqual(genericCapacity.Capacity, 4);
                Assert.AreEqual(-1, ownerCopy[1]);
                Assert.AreEqual(uint.MaxValue, native[1]);
                Assert.AreEqual(uint.MaxValue, converted[1]);
                Assert.AreEqual(-1, nativeCopy[1]);

                ownerCopy[1] = 10;
                nativeCopy[1] = 20;

                Assert.AreEqual(-1, genericCapacity[1]);
            }
        }

        [Test]
        public void OwnerMembers_OverloadsReferencesFindAndCapacityOperationsWork()
        {
            using var map = new SharedArrayMap<int, int>(4);

            Assert.IsTrue(map.TryAdd(1, 10, out var firstIndex));
            Assert.AreEqual(0, firstIndex);
            Assert.IsFalse(map.TryAdd(1, 20, out var existingIndex));
            Assert.AreEqual(0, existingIndex);

            int secondIndex;

            // SAFETY: The map owns the referenced values and no structural mutation occurs between each use.
            unsafe
            {
                map.GetOrAdd(2, out secondIndex) = 20;
                Assert.AreEqual(1, secondIndex);
                map.GetOrAdd(2, out var repeatedIndex) = 21;
                Assert.AreEqual(secondIndex, repeatedIndex);

                ref var first = ref map.GetValueByRef(1);
                first = 11;
            }

            Assert.IsTrue(map.TryFindIndex(1, out var foundIndex));
            Assert.AreEqual(firstIndex, foundIndex);
            Assert.IsFalse(map.TryFindIndex(99, out _));
            Assert.AreEqual(secondIndex, map.GetIndex(2));
            // SAFETY: The map remains alive and unmodified while the temporary key enumerator is inspected.
            unsafe
            {
                Assert.IsTrue(map.Keys.GetEnumerator().IsValid);
            }
            Assert.AreEqual(2, map.Values.Length);

            var initialCapacity = map.Capacity;
            map.EnsureCapacity(initialCapacity + 4);
            Assert.GreaterOrEqual(map.Capacity, initialCapacity + 4);

            var ensuredCapacity = map.Capacity;
            map.IncreaseCapacityBy(4);
            Assert.GreaterOrEqual(map.Capacity, ensuredCapacity + 4);

            map.IncreaseCapacityTo(map.Capacity + 4);
            Assert.Greater(map.Capacity, ensuredCapacity);

            Assert.IsTrue(map.Remove(1, out var removedIndex, out var removedValue));
            Assert.AreEqual(firstIndex, removedIndex);
            Assert.AreEqual(11, removedValue);
            Assert.IsFalse(map.Remove(99, out _, out _));

            map.Trim();

            Assert.AreEqual(map.Count, map.Capacity);
            Assert.AreEqual(21, map[2]);
        }

        [Test]
        public void SetOperations_IntersectExcludeAndUnionMutateExpectedKeys()
        {
            using var intersected = CreateMap(1, 2, 3);
            using var intersectKeys = new SharedArrayMap<int, long>(4);
            intersectKeys.Add(2, 20);
            intersectKeys.Add(3, 30);
            intersectKeys.Add(4, 40);

            intersected.Intersect(intersectKeys);

            Assert.AreEqual(2, intersected.Count);
            Assert.IsTrue(intersected.ContainsKey(2));
            Assert.IsTrue(intersected.ContainsKey(3));

            using var excluded = CreateMap(1, 2, 3);
            using var excludeKeys = new SharedArrayMap<int, long>(4);
            excludeKeys.Add(2, 20);
            excludeKeys.Add(3, 30);

            excluded.Exclude(excludeKeys);

            Assert.AreEqual(1, excluded.Count);
            Assert.IsTrue(excluded.ContainsKey(1));

            using var union = CreateMap(1, 2);
            using var unionSource = new SharedArrayMap<int, int>(4);
            unionSource.Add(2, 200);
            unionSource.Add(3, 300);

            union.Union(unionSource);

            Assert.AreEqual(3, union.Count);
            Assert.AreEqual(10, union[1]);
            Assert.AreEqual(200, union[2]);
            Assert.AreEqual(300, union[3]);
        }

        [Test]
        public void ReadOnly_ConstructionConversionViewsAndMembersReflectOwner()
        {
            using var map = new SharedArrayMap<int, int, int>(4);
            map.Add(1, 10);
            map.Add(2, 20);

            SharedArrayMap<int, int, int>.ReadOnly direct;
            SharedArrayMap<int, int, int>.ReadOnly fromOwner;
            SharedArrayMap<int, int, int>.ReadOnly converted;

            // SAFETY: map remains alive and unmodified while its read-only aliases are created and consumed.
            unsafe
            {
                direct = new SharedArrayMap<int, int, int>.ReadOnly(map);
                fromOwner = map.AsReadOnly();
                converted = map;
            }
            SharedArrayMap<int, int, int>.ReadOnly nullConverted = null;
            var empty = SharedArrayMap<int, int, int>.ReadOnly.Empty;
            SharedArrayMapNative<int, int>.ReadOnly native;

            // SAFETY: map remains alive and unmodified while the native read-only alias is consumed.
            unsafe
            {
                native = direct.AsNative();
            }

            Assert.IsTrue(direct.IsCreated);
            Assert.AreEqual(map.Capacity, direct.Capacity);
            Assert.AreEqual(2, direct.Count);
            Assert.IsTrue(direct.Keys.IsValid);
            Assert.AreEqual(2, direct.Values.Length);
            Assert.AreEqual(10, direct[1]);
            Assert.IsTrue(direct.ContainsKey(2));
            Assert.IsFalse(direct.ContainsKey(3));
            Assert.IsTrue(direct.TryGetValue(2, out var value));
            Assert.AreEqual(20, value);
            Assert.IsFalse(direct.TryGetValue(3, out _));
            Assert.IsTrue(direct.TryFindIndex(2, out var index));
            Assert.AreEqual(1, index);
            Assert.IsFalse(direct.TryFindIndex(3, out _));
            Assert.AreEqual(0, direct.GetIndex(1));
            Assert.AreEqual(20, fromOwner[2]);
            Assert.AreEqual(10, converted[1]);
            Assert.IsFalse(empty.IsCreated);
            Assert.AreEqual(0, empty.Count);
            Assert.IsFalse(nullConverted.IsCreated);
            Assert.AreEqual(0, nullConverted.Count);
            Assert.IsTrue(native.IsCreated);
            Assert.AreEqual(20, native[2]);
        }

        [Test]
        public void CollectionInterfaces_DispatchEveryExplicitMember()
        {
            using var source = new SharedArrayMap<int, int>(4);
            source.Add(1, 10);
            SharedArrayMapKeyValuePair<int, int, int> pair;

            // SAFETY: source remains alive and unmodified while its enumerator and borrowed pair are consumed.
            unsafe
            {
                var sourceEnumerator = source.GetEnumerator();
                Assert.IsTrue(sourceEnumerator.MoveNext());
                pair = sourceEnumerator.Current;
                sourceEnumerator.Dispose();
            }

            using var target = new SharedArrayMap<int, int>(4);
            ICollection<SharedArrayMapKeyValuePair<int, int, int>> collection = target;

            Assert.IsFalse(collection.IsReadOnly);

            collection.Add(pair);

            Assert.IsTrue(collection.Contains(pair));
            Assert.AreEqual(1, collection.Count);
            var destination = new SharedArrayMapKeyValuePair<int, int, int>[1];
            collection.CopyTo(destination, 0);
            AssertPair(destination[0], 1, 10);
            Assert.IsTrue(collection.Remove(pair));
            Assert.IsFalse(collection.Remove(pair));

            collection.Add(pair);
            collection.Clear();

            Assert.AreEqual(0, collection.Count);

            IEnumerable<SharedArrayMapKeyValuePair<int, int, int>> genericEnumerable = source;
            var genericEnumerator = genericEnumerable.GetEnumerator();
            Assert.IsTrue(genericEnumerator.MoveNext());
            Assert.AreEqual(1, genericEnumerator.Current.Key);
            genericEnumerator.Dispose();

            IEnumerable enumerable = source;
            var enumerator = enumerable.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsInstanceOf<SharedArrayMapKeyValuePair<int, int, int>>(enumerator.Current);

            IReadOnlyCollection<SharedArrayMapKeyValuePair<int, int, int>> readOnlyCollection = source;
            Assert.AreEqual(1, readOnlyCollection.Count);

        }

        [Test]
        public void CollectionInterface_PairOperationsRequireMatchingValue()
        {
            using var map = new SharedArrayMap<int, int>(4);
            map.Add(1, 0);
            map.Add(2, 20);

            using var mismatchedMap = new SharedArrayMap<int, int>(1);
            mismatchedMap.Add(1, 10);

            var matchingPair = GetFirstPair(map);
            var mismatchedPair = GetFirstPair(mismatchedMap);
            ICollection<SharedArrayMapKeyValuePair<int, int, int>> collection = map;

            Assert.IsTrue(collection.Contains(matchingPair));
            Assert.IsFalse(collection.Contains(mismatchedPair));
            Assert.IsFalse(collection.Remove(mismatchedPair));
            Assert.AreEqual(0, map[1]);
            Assert.IsTrue(collection.Remove(matchingPair));
            Assert.IsFalse(map.ContainsKey(1));
        }

        [Test]
        public void CollectionInterface_CopyToPreservesEnumerationOrderAndDestinationSentinels()
        {
            using var map = new SharedArrayMap<int, int>(4);
            map.Add(7, 0);
            map.Add(8, 80);

            ICollection<SharedArrayMapKeyValuePair<int, int, int>> collection = map;
            var destination = new SharedArrayMapKeyValuePair<int, int, int>[4];

            collection.CopyTo(destination, 1);

            Assert.IsFalse(destination[0].IsValid);
            AssertPair(destination[1], 7, 0);
            AssertPair(destination[2], 8, 80);
            Assert.IsFalse(destination[3].IsValid);
        }

        [Test]
        public void CollectionInterface_CopyToValidatesArgumentsAndAllowsEmptyEndIndex()
        {
            using var map = new SharedArrayMap<int, int>(1);
            map.Add(1, 10);
            ICollection<SharedArrayMapKeyValuePair<int, int, int>> collection = map;

            var nullException = Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, -1));
            var negativeException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                collection.CopyTo(new SharedArrayMapKeyValuePair<int, int, int>[1], -1)
            );
            var pastEndException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                collection.CopyTo(new SharedArrayMapKeyValuePair<int, int, int>[1], 2)
            );

            Assert.AreEqual("array", nullException.ParamName);
            Assert.AreEqual("arrayIndex", negativeException.ParamName);
            Assert.AreEqual("arrayIndex", pastEndException.ParamName);
            Assert.Throws<ArgumentException>(() =>
                collection.CopyTo(new SharedArrayMapKeyValuePair<int, int, int>[1], 1)
            );

            using var emptyMap = new SharedArrayMap<int, int>();
            ICollection<SharedArrayMapKeyValuePair<int, int, int>> emptyCollection = emptyMap;
            var emptyDestination = new SharedArrayMapKeyValuePair<int, int, int>[1];

            Assert.DoesNotThrow(() => emptyCollection.CopyTo(emptyDestination, emptyDestination.Length));
        }

        [Test]
        public void Enumerators_DirectConstructionMovementCurrentResetAndDispose()
        {
            using var map = new SharedArrayMap<int, int, int>(4);
            map.Add(7, 70);

            // SAFETY: map remains alive and structurally unchanged while every borrowed view is consumed.
            unsafe
            {
                var keys = new SharedArrayMap<int, int, int>.KeyEnumerable(map);
            Assert.IsTrue(keys.IsValid);

            var keyEnumerator = new SharedArrayMap<int, int, int>.KeyEnumerator(map);
            Assert.IsTrue(keyEnumerator.IsValid);
            Assert.IsTrue(keyEnumerator.MoveNext());
            Assert.AreEqual(7, keyEnumerator.Current);
            Assert.IsFalse(keyEnumerator.MoveNext());
            keyEnumerator.Reset();
            Assert.IsTrue(keyEnumerator.MoveNext());
            keyEnumerator.Dispose();

            var keysEnumerator = keys.GetEnumerator();
            Assert.IsTrue(keysEnumerator.MoveNext());
            keysEnumerator.Dispose();

            IEnumerable<int> genericKeys = keys;
            var genericKeysEnumerator = genericKeys.GetEnumerator();
            Assert.IsTrue(genericKeysEnumerator.MoveNext());
            Assert.AreEqual(7, genericKeysEnumerator.Current);
            genericKeysEnumerator.Dispose();

            IEnumerable nonGenericKeys = keys;
            var nonGenericKeysEnumerator = nonGenericKeys.GetEnumerator();
            Assert.IsTrue(nonGenericKeysEnumerator.MoveNext());
            Assert.AreEqual(7, nonGenericKeysEnumerator.Current);

            var ownerEnumerator = new SharedArrayMapKeyValueEnumerator<int, int, int>(map);
            Assert.IsTrue(ownerEnumerator.IsValid);
            Assert.IsTrue(ownerEnumerator.MoveNext());
            AssertPair(ownerEnumerator.Current, 7, 70);
            Assert.IsFalse(ownerEnumerator.MoveNext());
            ownerEnumerator.Reset();
            Assert.IsTrue(ownerEnumerator.MoveNext());
            ownerEnumerator.Dispose();

            var mapEnumerator = map.GetEnumerator();
            Assert.IsTrue(mapEnumerator.MoveNext());
            mapEnumerator.Dispose();

            var readOnly = map.AsReadOnly();
            var readOnlyKeys = new SharedArrayMap<int, int, int>.ReadOnly.KeyEnumerable(in readOnly);
            Assert.IsTrue(readOnlyKeys.IsValid);

            var readOnlyKeyEnumerator =
                new SharedArrayMap<int, int, int>.ReadOnly.KeyEnumerator(in readOnly);
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

            IEnumerable<int> genericReadOnlyKeys = readOnlyKeys;
            var genericReadOnlyKeysEnumerator = genericReadOnlyKeys.GetEnumerator();
            Assert.IsTrue(genericReadOnlyKeysEnumerator.MoveNext());
            Assert.AreEqual(7, genericReadOnlyKeysEnumerator.Current);
            genericReadOnlyKeysEnumerator.Dispose();

            IEnumerable nonGenericReadOnlyKeys = readOnlyKeys;
            var nonGenericReadOnlyKeysEnumerator = nonGenericReadOnlyKeys.GetEnumerator();
            Assert.IsTrue(nonGenericReadOnlyKeysEnumerator.MoveNext());
            Assert.AreEqual(7, nonGenericReadOnlyKeysEnumerator.Current);

            var readOnlyEnumerator =
                new SharedArrayMapReadOnlyKeyValueEnumerator<int, int, int>(in readOnly);
            Assert.IsTrue(readOnlyEnumerator.IsValid);
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            AssertPair(readOnlyEnumerator.Current, 7, 70);
            Assert.IsFalse(readOnlyEnumerator.MoveNext());
            readOnlyEnumerator.Reset();
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            IEnumerator readOnlyInterface = readOnlyEnumerator;
            Assert.IsInstanceOf<SharedArrayMapReadOnlyKeyValuePair<int, int, int>>(readOnlyInterface.Current);
            readOnlyEnumerator.Dispose();

            var viewEnumerator = readOnly.GetEnumerator();
            Assert.IsTrue(viewEnumerator.MoveNext());
            viewEnumerator.Dispose();

            Assert.IsFalse(default(SharedArrayMap<int, int, int>.KeyEnumerable).IsValid);
            Assert.IsFalse(default(SharedArrayMap<int, int, int>.KeyEnumerator).IsValid);
            Assert.IsFalse(default(SharedArrayMapKeyValueEnumerator<int, int, int>).IsValid);
            Assert.IsFalse(default(SharedArrayMap<int, int, int>.ReadOnly.KeyEnumerable).IsValid);
            Assert.IsFalse(default(SharedArrayMap<int, int, int>.ReadOnly.KeyEnumerator).IsValid);
                Assert.IsFalse(default(SharedArrayMapReadOnlyKeyValueEnumerator<int, int, int>).IsValid);
            }
        }

        [Test]
        public void KeyValuePairs_PublicConstructorsPropertiesValidityAndDeconstructReflectStorage()
        {
            var sharedValues = new[] { 10, 20 };
            using var nativeValues = new NativeArray<int>(new[] { 30, 40 }, Allocator.Temp);
            var key = 7;
            var pair = new SharedArrayMapKeyValuePair<int, int, int>(in key, sharedValues, 1);
            SharedArrayMapReadOnlyKeyValuePair<int, int, int> readOnlyPair;

            // SAFETY: nativeValues remains allocated and unmodified while readOnlyPair is consumed.
            unsafe
            {
                readOnlyPair = new SharedArrayMapReadOnlyKeyValuePair<int, int, int>(
                      in key
                    , nativeValues.AsReadOnly()
                    , 0
                );
            }

            AssertPair(pair, 7, 20);
            AssertPair(readOnlyPair, 7, 30);
            Assert.IsFalse(default(SharedArrayMapKeyValuePair<int, int, int>).IsValid);
            Assert.IsFalse(default(SharedArrayMapReadOnlyKeyValuePair<int, int, int>).IsValid);
        }

        [Test]
        public void Dispose_IsIdempotent()
        {
            var map = new SharedArrayMap<int, int>(4);
            map.Add(1, 10);

            Assert.DoesNotThrow(() => map.Dispose());
            Assert.DoesNotThrow(() => map.Dispose());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void MissingKey_OnEveryLookupThatThrows_ThrowsKeyNotFound()
        {
            using var zeroCapacity = new SharedArrayMap<int, int>();
            using var map = new SharedArrayMap<int, int>(4);

            map.Add(1, 10);

            Assert.Throws<KeyNotFoundException>(() => _ = zeroCapacity[1]);
            Assert.Throws<KeyNotFoundException>(() => _ = map[2]);
            Assert.Throws<KeyNotFoundException>(() => zeroCapacity.GetIndex(1));
            Assert.Throws<KeyNotFoundException>(() => map.GetIndex(2));

            // SAFETY: The lambdas only provoke the lookup failure; no reference escapes.
            unsafe
            {
                Assert.Throws<KeyNotFoundException>(() => _ = zeroCapacity.GetValueByRef(1));
                Assert.Throws<KeyNotFoundException>(() => _ = map.GetValueByRef(2));
            }

            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void TryAdd_ZeroCapacityMap_GrowsAndStoresEntries()
        {
            using var map = new SharedArrayMap<int, int>();

            Assert.AreEqual(0, map.Capacity);
            Assert.IsTrue(map.TryAdd(1, 10));
            Assert.IsTrue(map.TryAdd(2, 20, out var index));
            Assert.AreEqual(1, index);
            Assert.IsFalse(map.TryAdd(2, 99));
            Assert.IsFalse(map.TryAdd(2, 99, out var existingIndex));
            Assert.AreEqual(1, existingIndex);
            Assert.Greater(map.Capacity, 0);
            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(20, map[2]);
        }

        [Test]
        public void Lookups_ZeroCapacityMap_FindNothing()
        {
            using var map = new SharedArrayMap<int, int>();

            Assert.IsFalse(map.ContainsKey(1));
            Assert.IsFalse(map.TryGetValue(1, out var value));
            Assert.AreEqual(0, value);
            Assert.IsFalse(map.TryFindIndex(1, out var index));
            Assert.AreEqual(0, index);
            Assert.IsFalse(map.Remove(1));
            Assert.IsFalse(map.Remove(1, out var removedIndex, out var removedValue));
            Assert.AreEqual(0, removedIndex);
            Assert.AreEqual(0, removedValue);
            Assert.AreEqual(0, map.Count);
            Assert.AreEqual(0, map.Values.Length);
        }

        [Test]
        public void Add_ExtremeKeys_AreFoundAgainAndRemovable()
        {
            var keys = new[] { 0, -1, 1, int.MinValue, int.MaxValue, 101, -101 };
            using var map = new SharedArrayMap<int, int>();

            for (var i = 0; i < keys.Length; i++)
            {
                map.Add(keys[i], i);
            }

            Assert.AreEqual(keys.Length, map.Count);

            for (var i = 0; i < keys.Length; i++)
            {
                Assert.AreEqual(i, map[keys[i]], $"key {keys[i]}");
            }

            for (var i = 0; i < keys.Length; i++)
            {
                Assert.IsTrue(map.Remove(keys[i]), $"key {keys[i]}");
                Assert.IsFalse(map.ContainsKey(keys[i]), $"key {keys[i]}");
            }

            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void Clear_EmptyMaps_KeepTheMapsUsable()
        {
            using var zeroCapacity = new SharedArrayMap<int, int>();
            using var withCapacity = new SharedArrayMap<int, int>(4);

            zeroCapacity.Clear();
            withCapacity.Clear();

            Assert.AreEqual(0, zeroCapacity.Count);
            Assert.AreEqual(0, withCapacity.Count);

            zeroCapacity.Add(1, 10);
            withCapacity.Add(2, 20);

            Assert.AreEqual(10, zeroCapacity[1]);
            Assert.AreEqual(20, withCapacity[2]);
        }

        [Test]
        public void Clear_ThenReAdd_DoesNotSeeStaleEntries()
        {
            const int COUNT = 20;
            using var map = new SharedArrayMap<int, int>(4);

            for (var i = 0; i < COUNT; i++)
            {
                map.Add(i, i * 10);
            }

            map.Clear();

            Assert.AreEqual(0, map.Count);

            for (var i = 0; i < COUNT; i++)
            {
                Assert.IsFalse(map.ContainsKey(i), $"key {i}");
                Assert.IsFalse(map.TryGetValue(i, out _), $"key {i}");
            }

            for (var i = COUNT - 1; i >= 0; i--)
            {
                map.Add(i, i + 1);
            }

            Assert.AreEqual(COUNT, map.Count);

            for (var i = 0; i < COUNT; i++)
            {
                Assert.AreEqual(i + 1, map[i], $"key {i}");
            }
        }

        [Test]
        public void GetOrAdd_KeyAddedAfterClear_StartsFromDefault()
        {
            using var map = new SharedArrayMap<int, int>(4);

            map.Add(1, 10);
            map.Add(2, 20);
            map.Clear();

            // SAFETY: The reference is read before any further mutation of the map.
            unsafe
            {
                Assert.AreEqual(0, map.GetOrAdd(3));
            }
        }

        [Test]
        public void GetOrAdd_ZeroCapacityMap_AddsAndAliasesTheStoredValue()
        {
            using var map = new SharedArrayMap<int, int>();

            // SAFETY: The reference is used before any further structural mutation of the map.
            unsafe
            {
                ref var first = ref map.GetOrAdd(5);

                Assert.AreEqual(0, first);

                first = 50;
            }

            // SAFETY: Both references address the same stored value and no structural mutation happens.
            unsafe
            {
                ref var again = ref map.GetOrAdd(5);

                Assert.AreEqual(50, again);

                again = 51;

                ref var withIndex = ref map.GetOrAdd(5, out var index);

                Assert.AreEqual(51, withIndex);
                Assert.AreEqual(0, index);

                withIndex = 52;
            }

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(52, map[5]);
            Assert.AreEqual(52, map.Values.Span[0]);
        }

        [Test]
        public void Remove_EveryPositionAndChain_MatchesTheSwapWithLastModel()
        {
            AssertRemoveMatchesModel(static i => i * 7);
            AssertRemoveMatchesModel(static i => new CollidingKey(i));
        }

        [Test]
        public void AddAndRemove_PseudoRandomSequenceOnOneChain_MatchesDictionary()
        {
            var random = new System.Random(20261018);
            var model = new Dictionary<int, int>();
            using var map = new SharedArrayMap<CollidingKey, int>(2);

            for (var step = 0; step < 2000; step++)
            {
                var key = random.Next(0, 24);
                var value = random.Next();
                var collidingKey = new CollidingKey(key);

                if (random.Next(0, 3) == 0)
                {
                    Assert.AreEqual(model.Remove(key), map.Remove(collidingKey), $"remove {key} step {step}");
                }
                else
                {
                    var expectedAdded = model.TryAdd(key, value);

                    Assert.AreEqual(expectedAdded, map.TryAdd(collidingKey, value), $"add {key} step {step}");
                }

                Assert.AreEqual(model.Count, map.Count, $"step {step}");
            }

            for (var key = 0; key < 24; key++)
            {
                var found = map.TryGetValue(new CollidingKey(key), out var actual);

                Assert.AreEqual(model.TryGetValue(key, out var expected), found, $"key {key}");
                Assert.AreEqual(expected, actual, $"key {key}");
            }
        }

        [Test]
        public void EnsureCapacity_SizeWithinTheCapacity_KeepsTheCapacityAndEntries()
        {
            using var map = new SharedArrayMap<int, int>(8);

            map.Add(1, 10);

            var capacity = map.Capacity;

            Assert.AreEqual(capacity, map.EnsureCapacity(0));
            Assert.AreEqual(capacity, map.EnsureCapacity(1));
            Assert.AreEqual(capacity, map.EnsureCapacity(capacity));
            Assert.AreEqual(capacity, map.Capacity);
            Assert.AreEqual(10, map[1]);
        }

        [Test]
        public void EnsureCapacity_PastTheCapacity_KeepsEveryEntryReachable()
        {
            using var map = new SharedArrayMap<CollidingKey, int>(4);

            for (var i = 0; i < 4; i++)
            {
                map.Add(new CollidingKey(i), i * 10);
            }

            var grown = map.EnsureCapacity(40);

            Assert.GreaterOrEqual(grown, 40);
            Assert.AreEqual(grown, map.Capacity);
            Assert.AreEqual(4, map.Count);

            for (var i = 0; i < 4; i++)
            {
                Assert.IsTrue(map.TryGetValue(new CollidingKey(i), out var value), $"key {i}");
                Assert.AreEqual(i * 10, value, $"key {i}");
            }

            map.Add(new CollidingKey(100), 1000);

            Assert.AreEqual(5, map.Count);
            Assert.AreEqual(1000, map[new CollidingKey(100)]);
        }

        [Test]
        public void Trim_EmptyMapWithCapacity_ShrinksToZeroAndStillAcceptsKeys()
        {
            using var map = new SharedArrayMap<int, int>(8);

            map.Trim();

            Assert.AreEqual(0, map.Capacity);
            Assert.AreEqual(0, map.Count);

            map.Add(1, 10);

            Assert.AreEqual(10, map[1]);
        }

        [Test]
        public void Trim_ZeroCapacityMap_KeepsTheMapUsable()
        {
            using var map = new SharedArrayMap<int, int>();

            map.Trim();

            Assert.AreEqual(0, map.Capacity);

            map.Add(1, 10);

            Assert.AreEqual(10, map[1]);
        }

        [Test]
        public void Trim_AfterRemovals_KeepsEveryRemainingEntry()
        {
            using var map = new SharedArrayMap<CollidingKey, int>(16);

            for (var i = 0; i < 10; i++)
            {
                map.Add(new CollidingKey(i), i * 10);
            }

            Assert.IsTrue(map.Remove(new CollidingKey(2)));
            Assert.IsTrue(map.Remove(new CollidingKey(9)));

            map.Trim();

            Assert.AreEqual(8, map.Count);
            Assert.AreEqual(8, map.Capacity);

            for (var i = 0; i < 10; i++)
            {
                var expected = i != 2 && i != 9;

                Assert.AreEqual(expected, map.TryGetValue(new CollidingKey(i), out var value), $"key {i}");

                if (expected)
                {
                    Assert.AreEqual(i * 10, value, $"key {i}");
                }
            }
        }

        [Test]
        public void IntersectAndExclude_EmptyMaps_BehaveLikeSetOperations()
        {
            using var zeroCapacityOther = new SharedArrayMap<int, long>();
            using var emptyOther = new SharedArrayMap<int, int>(4);
            using var zeroCapacitySelf = new SharedArrayMap<int, int>();
            using var emptySelf = new SharedArrayMap<int, int>(4);
            using var full = CreateMap(1, 2, 3);
            using var other = CreateMap(1, 2);

            full.Exclude(zeroCapacityOther);
            full.Exclude(emptyOther);

            Assert.AreEqual(3, full.Count);

            zeroCapacitySelf.Intersect(other);
            zeroCapacitySelf.Exclude(other);
            emptySelf.Intersect(other);
            emptySelf.Exclude(other);

            Assert.AreEqual(0, zeroCapacitySelf.Count);
            Assert.AreEqual(0, emptySelf.Count);

            full.Intersect(zeroCapacityOther);

            Assert.AreEqual(0, full.Count);
        }

        [Test]
        public void IntersectAndExclude_WithTheMapItself_KeepOrRemoveEveryEntry()
        {
            using var intersected = CreateMap(1, 2, 3);
            using var excluded = CreateMap(1, 2, 3);

            intersected.Intersect(intersected);
            excluded.Exclude(excluded);

            Assert.AreEqual(3, intersected.Count);
            Assert.IsTrue(intersected.ContainsKey(1));
            Assert.IsTrue(intersected.ContainsKey(2));
            Assert.IsTrue(intersected.ContainsKey(3));
            Assert.AreEqual(0, excluded.Count);
        }

        [Test]
        public void Dispose_Map_ReportsNotCreatedAndEmpty()
        {
            var map = new SharedArrayMap<int, int>(4);

            map.Add(1, 10);
            map.Dispose();

            Assert.IsFalse(map.IsCreated);
            Assert.AreEqual(0, map.Count);
            Assert.AreEqual(0, map.Capacity);
        }

        [Test]
        public void GetEnumerator_EmptyMaps_YieldNothing()
        {
            using var zeroCapacity = new SharedArrayMap<int, int>();
            using var withCapacity = new SharedArrayMap<int, int>(4);
            var count = 0;

            // SAFETY: The maps outlive the enumerators and are not modified while they run.
            unsafe
            {
                foreach (var pair in zeroCapacity)
                {
                    count++;
                }

                foreach (var pair in withCapacity)
                {
                    count++;
                }

                foreach (var pair in (IEnumerable<SharedArrayMapKeyValuePair<int, int, int>>)zeroCapacity)
                {
                    count++;
                }

                foreach (var pair in (IEnumerable)withCapacity)
                {
                    count++;
                }

                foreach (var key in zeroCapacity.Keys)
                {
                    count++;
                }

                foreach (var key in withCapacity.Keys)
                {
                    count++;
                }

                foreach (var key in (IEnumerable<int>)zeroCapacity.Keys)
                {
                    count++;
                }

                foreach (var key in (IEnumerable)withCapacity.Keys)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void InternalCounters_FollowTheMapContent()
        {
            using var map = new SharedArrayMap<CollidingKey, int>(8);

            Assert.AreEqual(0, map.FreeValueCellIndexRO);
            Assert.AreEqual(0u, map.CollisionsRO);

            var version = map.VersionRO;

            map.Add(new CollidingKey(1), 10);

            Assert.AreEqual(1, map.FreeValueCellIndexRO);
            Assert.AreEqual(0u, map.CollisionsRO);
            Assert.AreNotEqual(version, map.VersionRO);

            map.Add(new CollidingKey(2), 20);

            Assert.AreEqual(2, map.FreeValueCellIndexRO);
            Assert.AreEqual(1u, map.CollisionsRO);

            version = map.VersionRO;

            _ = map.ContainsKey(new CollidingKey(1));
            _ = map.TryGetValue(new CollidingKey(2), out _);
            _ = map[new CollidingKey(1)];
            _ = map.Count;

            Assert.AreEqual(version, map.VersionRO);

            map.VersionRW++;

            Assert.AreEqual(version + 1, map.VersionRO);

            map.FreeValueCellIndexRW = 0;

            Assert.AreEqual(0, map.Count);
            Assert.DoesNotThrow(() => map.CheckRead());
        }

        [Test]
        public void FastModBucketsMultiplierRO_CapacityMap_ComputesTheBucketRemainder()
        {
            using var map = new SharedArrayMap<int, int>(8);
            var divisor = (uint)map._buckets.Capacity;
            var multiplier = map.FastModBucketsMultiplierRO;
            var values = new uint[] { 0, 1, divisor - 1, divisor, divisor + 1, 12345, uint.MaxValue };

            for (var i = 0; i < values.Length; i++)
            {
                var actual = HashHelpers.FastMod(values[i], divisor, multiplier);

                Assert.AreEqual(values[i] % divisor, actual, $"{values[i]}");
            }
        }

        [Test]
        public void AsReadOnly_EmptyMapWithCapacity_ReportsNoEntries()
        {
            using var map = new SharedArrayMap<int, int>(4);

            // SAFETY: map outlives the views, which are only inspected here.
            unsafe
            {
                var view = map.AsReadOnly();
                var direct = new SharedArrayMap<int, int, int>.ReadOnly(map);

                Assert.IsTrue(view.IsCreated);
                Assert.IsTrue(direct.IsCreated);
                Assert.AreEqual(0, view.Count);
                Assert.AreEqual(map.Capacity, view.Capacity);
                Assert.IsFalse(view.ContainsKey(1));
                Assert.IsFalse(view.TryGetValue(1, out var value));
                Assert.AreEqual(0, value);
                Assert.IsFalse(view.TryFindIndex(1, out var index));
                Assert.AreEqual(0, index);
                Assert.AreEqual(0, view.Values.Length);
                Assert.IsTrue(view.Keys.IsValid);
            }
        }

        [Test]
        public void ReadOnly_ZeroCapacityMap_IsCreatedAndDefaultIsNotCreated()
        {
            using var map = new SharedArrayMap<int, int>();

            // SAFETY: The owner remains alive and unmodified while its view is inspected.
            unsafe
            {
                Assert.IsTrue(map.AsReadOnly().IsCreated);
            }

            Assert.IsFalse(default(SharedArrayMap<int, int, int>.ReadOnly).IsCreated);
        }

        [Test]
        public void AsReadOnly_ZeroCapacityMap_FindsNothing()
        {
            using var map = new SharedArrayMap<int, int>();

            // SAFETY: map outlives the view, which is only inspected here.
            unsafe
            {
                var view = map.AsReadOnly();

                Assert.AreEqual(0, view.Count);
                Assert.IsFalse(view.ContainsKey(1));
                Assert.IsFalse(view.TryGetValue(1, out var value));
                Assert.AreEqual(0, value);
                Assert.IsFalse(view.TryFindIndex(1, out _));
                Assert.AreEqual(0, view.Values.Length);
            }
        }

        [Test]
        public void ReadOnlyGetEnumerator_EmptyMapWithCapacity_YieldsNothing()
        {
            using var map = new SharedArrayMap<int, int>(4);
            var count = 0;

            // SAFETY: map outlives the view and its enumerators and is not modified while they run.
            unsafe
            {
                var view = map.AsReadOnly();

                foreach (var pair in view)
                {
                    count++;
                }

                foreach (var key in view.Keys)
                {
                    count++;
                }

                foreach (var key in (IEnumerable<int>)view.Keys)
                {
                    count++;
                }

                foreach (var key in (IEnumerable)view.Keys)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ReadOnlyGetEnumerator_ZeroCapacityMap_YieldsNothing()
        {
            using var map = new SharedArrayMap<int, int>();
            var count = 0;

            // SAFETY: map outlives the view and its enumerators and is not modified while they run.
            unsafe
            {
                var view = map.AsReadOnly();

                foreach (var pair in view)
                {
                    count++;
                }

                foreach (var key in view.Keys)
                {
                    count++;
                }

                foreach (var key in (IEnumerable<int>)view.Keys)
                {
                    count++;
                }

                foreach (var key in (IEnumerable)view.Keys)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ReadOnlyValues_FollowTheCountThroughRemovals()
        {
            using var map = new SharedArrayMap<int, int>(4);

            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);

            // SAFETY: map outlives the view, which is used before any resize.
            unsafe
            {
                var view = map.AsReadOnly();

                Assert.AreEqual(3, view.Values.Length);

                Assert.IsTrue(map.Remove(1));

                var values = view.Values;

                Assert.AreEqual(2, values.Length);
                Assert.AreEqual(30, values[0]);
                Assert.AreEqual(20, values[1]);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnlyValues_IndexPastTheCount_Throws()
        {
            using var map = new SharedArrayMap<int, int>(8);

            map.Add(1, 10);
            map.Add(2, 20);

            // SAFETY: map outlives the view and the slice, which are only inspected here.
            unsafe
            {
                var values = map.AsReadOnly().Values;

                Assert.AreEqual(2, values.Length);
                Assert.Catch(() => _ = values[2]);
                Assert.Catch(() => _ = values[-1]);
            }
        }

        [Test]
        public void ReadOnlyValues_ElementSizes_RoundTripBitExact()
        {
            AssertValuesRoundTrip(new byte[] { 0, 255, 128 });
            AssertValuesRoundTrip(new[] { long.MinValue, -1L, long.MaxValue });
            AssertValuesRoundTrip(new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 0, b = 128 } });
            AssertValuesRoundTrip(new[] { new Wide { a = long.MinValue, b = -1, c = long.MaxValue } });
        }

        [Test]
        public void ReadOnlyKeyValuePair_ValueOutsideTheStoredValues_Throws()
        {
            using var nativeValues = new NativeArray<int>(new[] { 30, 40 }, Allocator.Temp);
            var key = 7;
            SharedArrayMapReadOnlyKeyValuePair<int, int, int> negative;
            SharedArrayMapReadOnlyKeyValuePair<int, int, int> past;

            // SAFETY: nativeValues remains allocated and unmodified while the pairs are consumed.
            unsafe
            {
                negative = new SharedArrayMapReadOnlyKeyValuePair<int, int, int>(in key, nativeValues.AsReadOnly(), -1);
                past = new SharedArrayMapReadOnlyKeyValuePair<int, int, int>(in key, nativeValues.AsReadOnly(), 2);

                Assert.Throws<IndexOutOfRangeException>(() => _ = negative.Value);
                Assert.Throws<IndexOutOfRangeException>(() => _ = past.Value);
                Assert.Throws<IndexOutOfRangeException>(() => past.Deconstruct(out _, out _));
            }
        }

        [Test]
        public void KeyValuePairs_DefaultPairs_HaveDefaultKeysAndAreNotValid()
        {
            var managed = default(SharedArrayMapKeyValuePair<int, int, int>);
            var readOnly = default(SharedArrayMapReadOnlyKeyValuePair<int, int, int>);

            Assert.IsFalse(managed.IsValid);
            Assert.IsFalse(readOnly.IsValid);
            Assert.AreEqual(0, managed.Key);
            Assert.AreEqual(0, readOnly.Key);
        }

        [Test]
        public void OnBeforeSerialize_EmptyMaps_StoreAnEmptyPayload()
        {
            using var zeroCapacity = new SharedArrayMap<int, int>();
            using var withCapacity = new SharedArrayMap<int, int>(4);

            ((ISerializationCallbackReceiver)zeroCapacity).OnBeforeSerialize();
            ((ISerializationCallbackReceiver)withCapacity).OnBeforeSerialize();

            Assert.That(zeroCapacity._serializedEntries, Is.Not.Null);
            Assert.That(zeroCapacity._serializedEntries, Is.Empty);
            Assert.That(withCapacity._serializedEntries, Is.Not.Null);
            Assert.That(withCapacity._serializedEntries, Is.Empty);
        }

        [Test]
        public void OnBeforeSerialize_DisposedMap_StoresNoPayload()
        {
            var map = new SharedArrayMap<int, int>(4);

            map.Add(1, 10);
            map.Dispose();

            ((ISerializationCallbackReceiver)map).OnBeforeSerialize();

            Assert.That(map._serializedEntries, Is.Null);
        }

        [Test]
        public void OnBeforeSerialize_MapAfterRemovals_StoresOnlyTheLiveEntries()
        {
            using var map = new SharedArrayMap<CollidingKey, int>(8);

            for (var i = 0; i < 5; i++)
            {
                map.Add(new CollidingKey(i), i * 10);
            }

            Assert.IsTrue(map.Remove(new CollidingKey(1)));

            ((ISerializationCallbackReceiver)map).OnBeforeSerialize();

            var entries = map._serializedEntries;

            Assert.AreEqual(4, entries.Length);

            var seen = new Dictionary<int, int>();

            for (var i = 0; i < entries.Length; i++)
            {
                seen.Add(entries[i].key.Value, entries[i].value);
            }

            CollectionAssert.AreEquivalent(
                new Dictionary<int, int> { { 0, 0 }, { 2, 20 }, { 3, 30 }, { 4, 40 } },
                seen
            );
        }

        private static void AssertRemoveMatchesModel<TKey>(Func<int, TKey> makeKey)
            where TKey : unmanaged, IEquatable<TKey>
        {
            const int COUNT = 6;

            for (var removeAt = 0; removeAt < COUNT; removeAt++)
            {
                using var map = new SharedArrayMap<TKey, int>(2);
                var keys = new List<int>();
                var values = new List<int>();

                for (var i = 0; i < COUNT; i++)
                {
                    map.Add(makeKey(i), 100 + i);
                    keys.Add(i);
                    values.Add(100 + i);
                }

                var removedKey = keys[removeAt];

                Assert.IsTrue(map.Remove(makeKey(removedKey), out var index, out var value), $"removeAt {removeAt}");
                Assert.AreEqual(removeAt, index, $"removeAt {removeAt}");
                Assert.AreEqual(values[removeAt], value, $"removeAt {removeAt}");

                var last = keys.Count - 1;

                keys[removeAt] = keys[last];
                values[removeAt] = values[last];
                keys.RemoveAt(last);
                values.RemoveAt(last);

                Assert.AreEqual(keys.Count, map.Count, $"removeAt {removeAt}");
                CollectionAssert.AreEqual(values, map.Values.ToArray(), $"removeAt {removeAt}");
                Assert.IsFalse(map.ContainsKey(makeKey(removedKey)), $"removeAt {removeAt}");

                for (var i = 0; i < keys.Count; i++)
                {
                    var message = $"removeAt {removeAt} key {keys[i]}";

                    Assert.IsTrue(map.TryGetValue(makeKey(keys[i]), out var found), message);
                    Assert.AreEqual(values[i], found, message);
                }

                Assert.IsTrue(map.TryAdd(makeKey(removedKey), 7, out var newIndex), $"removeAt {removeAt}");
                Assert.AreEqual(keys.Count, newIndex, $"removeAt {removeAt}");
            }
        }

        private static void AssertValuesRoundTrip<TValue>(TValue[] values)
            where TValue : unmanaged
        {
            using var map = new SharedArrayMap<int, TValue>(values.Length);

            for (var i = 0; i < values.Length; i++)
            {
                map.Add(i, values[i]);
            }

            // SAFETY: map outlives the view and the slice, which are inspected before any mutation.
            unsafe
            {
                var slice = map.AsReadOnly().Values;

                Assert.AreEqual(values.Length, slice.Length);

                for (var i = 0; i < values.Length; i++)
                {
                    SpanBytesAssert.AreEqual(new[] { values[i] }, new[] { slice[i] });
                }
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

        private static SharedArrayMap<int, int> CreateMap(params int[] keys)
        {
            var map = new SharedArrayMap<int, int>(8);

            for (var i = 0; i < keys.Length; i++)
            {
                map.Add(keys[i], keys[i] * 10);
            }

            return map;
        }

        private static SharedArrayMapKeyValuePair<int, int, int> GetFirstPair(SharedArrayMap<int, int> map)
        {
            // SAFETY: map remains alive and unmodified while its first borrowed pair is returned.
            unsafe
            {
                var enumerator = map.GetEnumerator();

                try
                {
                    Assert.IsTrue(enumerator.MoveNext());
                    return enumerator.Current;
                }
                finally
                {
                    enumerator.Dispose();
                }
            }
        }

        private static void AssertPair(SharedArrayMapKeyValuePair<int, int, int> pair, int key, int value)
        {
            Assert.IsTrue(pair.IsValid);
            Assert.AreEqual(key, pair.Key);
            Assert.AreEqual(value, pair.Value);

            pair.Deconstruct(out var actualKey, out var actualValue);

            Assert.AreEqual(key, actualKey);
            Assert.AreEqual(value, actualValue);
        }

        private static void AssertPair(SharedArrayMapReadOnlyKeyValuePair<int, int, int> pair, int key, int value)
        {
            Assert.IsTrue(pair.IsValid);
            Assert.AreEqual(key, pair.Key);
            Assert.AreEqual(value, pair.Value);

            pair.Deconstruct(out var actualKey, out var actualValue);

            Assert.AreEqual(key, actualKey);
            Assert.AreEqual(value, actualValue);
        }

        [Test]
        public void Add_CollisionChurnPastBucketCount_RecomputeRecountsCollisions()
        {
            using var map = new SharedArrayMap<CollidingKey, int>(8);

            map.Add(new CollidingKey(1), 10);
            map.Add(new CollidingKey(2), 20); // cumulative collision 1

            for (var i = 0; i < 11; i++) // collisions 2..12; the add at i == 10 crosses 11
            {
                map.Add(new CollidingKey(3), 30);
                Assert.IsTrue(map.Remove(new CollidingKey(3)));
            }

            Assert.AreEqual(2, map.Count);
            Assert.IsTrue(map.TryGetValue(new CollidingKey(1), out var value1));
            Assert.AreEqual(10, value1);
            Assert.IsTrue(map.TryGetValue(new CollidingKey(2), out var value2));
            Assert.AreEqual(20, value2);

            // A recounted counter reflects live chains only: three colliding keys were in
            // the map at recompute time, which is two chain links. The stale-accumulation
            // bug reports 14 here (12 carried over + 2 recounted).
            Assert.LessOrEqual(map._collisions[0], (uint)map.Count);
        }
    }
}
