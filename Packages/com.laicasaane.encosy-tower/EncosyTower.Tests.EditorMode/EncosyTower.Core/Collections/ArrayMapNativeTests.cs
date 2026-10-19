using System;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Collections.Unsafe;
using EncosyTower.TypeWraps;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class ArrayMapNativeTests
    {
        [WrapRecord]
        public readonly partial record struct UintId(uint Value);

        [Test]
        public void Constructor_CreatesValidMap()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            Assert.IsTrue(map.IsCreated);
            Assert.AreEqual(0, map.Count);
            Assert.GreaterOrEqual(map.Capacity, 4);
        }

        [Test]
        public void DefaultMap_IsNotCreated()
        {
            ArrayMapNative<int, int> map = default;

            Assert.IsFalse(map.IsCreated);
        }

        [Test]
        public void Add_StoresValues()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(20, map[2]);
            Assert.AreEqual(30, map[3]);
        }

        [Test]
        public void Add_DuplicateKey_Throws()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.Throws<InvalidOperationException>(() => map.Add(1, 20));
        }

        [Test]
        public void Indexer_Set_AddsOrOverwrites()
        {
            // Not using a 'using' variable because the indexer setter cannot be
            // invoked on a readonly local (CS1654).
            var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            try
            {
                map[1] = 10;
                Assert.AreEqual(1, map.Count);
                Assert.AreEqual(10, map[1]);

                map[1] = 99;
                Assert.AreEqual(1, map.Count);
                Assert.AreEqual(99, map[1]);
            }
            finally
            {
                map.Dispose();
            }
        }

        [Test]
        public void TryAdd_ReturnsFalseOnDuplicate()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            Assert.IsTrue(map.TryAdd(1, 10, out var index0));
            Assert.AreEqual(0, index0);

            Assert.IsFalse(map.TryAdd(1, 20, out var index1));
            Assert.AreEqual(0, index1);
            Assert.AreEqual(10, map[1]);
        }

        [Test]
        public void ContainsKey_ReflectsContent()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.IsTrue(map.ContainsKey(1));
            Assert.IsFalse(map.ContainsKey(2));
        }

        [Test]
        public void TryGetValue_ReturnsExpected()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            Assert.IsTrue(map.TryGetValue(1, out var value1));
            Assert.AreEqual(10, value1);

            Assert.IsTrue(map.TryGetValue(2, out var value2));
            Assert.AreEqual(20, value2);

            Assert.IsFalse(map.TryGetValue(3, out var missing));
            Assert.AreEqual(0, missing);
        }

        [Test]
        public void FindIndex_ReturnsMinusOneWhenMissing()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.AreEqual(0, map.FindIndex(1));
            Assert.AreEqual(-1, map.FindIndex(2));
        }

        [Test]
        public void GetOrAdd_AddsWhenMissing()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            ref var value = ref map.GetOrAdd(1);
            Assert.AreEqual(0, value);
            value = 42;

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(42, map[1]);
        }

        [Test]
        public void GetOrAdd_ReturnsExistingByRef()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            ref var value = ref map.GetOrAdd(1);
            Assert.AreEqual(10, value);
            value = 20;

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(20, map[1]);
        }

        [Test]
        public void GetOrAdd_WithBuilder_UsesBuilderOnlyWhenMissing()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            var value = map.GetOrAdd(1, () => 7);
            Assert.AreEqual(7, value);

            var existing = map.GetOrAdd(1, () => 999);
            Assert.AreEqual(7, existing);

            map.Dispose();
        }

        [Test]
        public void GetOrAdd_OutIndex_ReportsIndex()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            map.GetOrAdd(1, out var index0) = 5;
            Assert.AreEqual(0, index0);

            map.GetOrAdd(2, out var index1) = 6;
            Assert.AreEqual(1, index1);

            map.GetOrAdd(1, out var indexExisting);
            Assert.AreEqual(0, indexExisting);
        }

        [Test]
        public void GetOrAdd_WithFuncRef_PassesParameter()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            var param = 3;
            var value = map.GetOrAdd(1, (ref int p) => p * 10, ref param);

            Assert.AreEqual(30, value);

            map.Dispose();
        }

        [Test]
        public void GetValueByRef_ReturnsRef()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            ref var value = ref map.GetValueByRef(1);
            value = 99;

            Assert.AreEqual(99, map[1]);
        }

        [Test]
        public void GetValueByRef_MissingKey_Throws()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            Assert.Throws<KeyNotFoundException>(() => map.GetValueByRef(99));
        }

        [Test]
        public void Remove_RemovesKey()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);

            Assert.IsTrue(map.Remove(2));

            Assert.AreEqual(2, map.Count);
            Assert.IsFalse(map.ContainsKey(2));
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(30, map[3]);
        }

        [Test]
        public void Remove_MissingKey_ReturnsFalse()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.IsFalse(map.Remove(99));
            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void Remove_OutValue_ReportsRemoved()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            Assert.IsTrue(map.Remove(1, out var index, out var value));
            Assert.AreEqual(10, value);
            Assert.GreaterOrEqual(index, 0);
        }

        [Test]
        public void Remove_LastInserted_KeepsRest()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            for (var i = 0; i < 5; i++)
            {
                map.Add(i, i * 10);
            }

            Assert.IsTrue(map.Remove(4));

            Assert.AreEqual(4, map.Count);

            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(i * 10, map[i]);
            }
        }

        [Test]
        public void RemoveAndReadd_StaysConsistent()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            for (var i = 0; i < 10; i++)
            {
                map.Add(i, i);
            }

            for (var i = 0; i < 10; i += 2)
            {
                Assert.IsTrue(map.Remove(i));
            }

            Assert.AreEqual(5, map.Count);

            for (var i = 0; i < 10; i += 2)
            {
                map.Add(i, i + 100);
            }

            Assert.AreEqual(10, map.Count);

            for (var i = 0; i < 10; i++)
            {
                var expected = (i % 2 == 0) ? i + 100 : i;
                Assert.AreEqual(expected, map[i]);
            }
        }

        [Test]
        public void Clear_ResetsCount()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            map.Clear();

            Assert.AreEqual(0, map.Count);
            Assert.IsFalse(map.ContainsKey(1));

            map.Add(1, 99);
            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(99, map[1]);
        }

        [Test]
        public void Recycle_ResetsCountButKeepsValuesArray()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            map.Recycle();

            Assert.AreEqual(0, map.Count);
            Assert.IsFalse(map.ContainsKey(1));
        }

        [Test]
        public void Grow_PastInitialCapacity_KeepsAllEntries()
        {
            const int N = 200;
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

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
        public void EnsureCapacity_GrowsCapacity()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            var before = map.Capacity;

            map.EnsureCapacity(before + 100);

            Assert.Greater(map.Capacity, before);
        }

        [Test]
        public void IncreaseCapacityBy_GrowsCapacity()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            var before = map.Capacity;

            map.IncreaseCapacityBy(50);

            Assert.GreaterOrEqual(map.Capacity, before + 50);
        }

        [Test]
        public void Trim_ShrinksCapacityToCount()
        {
            using var map = new ArrayMapNative<int, int>(64, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            map.Trim();

            Assert.AreEqual(2, map.Capacity);
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(20, map[2]);
        }

        [Test]
        public void Enumerator_VisitsAllPairs()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            for (var i = 0; i < 5; i++)
            {
                map.Add(i, i * 10);
            }

            var seen = new Dictionary<int, int>();

            foreach (var pair in map)
            {
                seen[pair.Key] = pair.Value;
            }

            Assert.AreEqual(5, seen.Count);

            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(i * 10, seen[i]);
            }
        }

        [Test]
        public void Enumerator_ModifyDuringIteration_Throws()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var pair in map)
                {
                    map.Add(pair.Key + 100, 0);
                }
            });
        }

        [Test]
        public void Keys_EnumeratesAllKeys()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            for (var i = 0; i < 4; i++)
            {
                map.Add(i, i);
            }

            var keys = new HashSet<int>();

            foreach (var key in map.Keys)
            {
                keys.Add(key);
            }

            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3 }, keys);
        }

        [Test]
        public void Values_SliceMatchesCount()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);

            var values = map.Values;

            Assert.AreEqual(3, values.Length);
        }

        [Test]
        public void CopyConstructor_DeepCopiesContent()
        {
            using var source = new ArrayMapNative<int, int>(8, Allocator.Temp);
            source.Add(1, 10);
            source.Add(2, 20);

            using var copy = new ArrayMapNative<int, int>(source, Allocator.Temp);

            Assert.AreEqual(source.Count, copy.Count);
            Assert.AreEqual(10, copy[1]);
            Assert.AreEqual(20, copy[2]);

            copy.Add(3, 30);
            Assert.IsFalse(source.ContainsKey(3));
        }

        [Test]
        public void CopyConstructor_FromInvalidSource_Throws()
        {
            ArrayMapNative<int, int> source = default;

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                using var copy = new ArrayMapNative<int, int>(source, Allocator.Temp);
            });

            Assert.That(exception!.ParamName, Is.EqualTo(nameof(source)));
        }

        [Test]
        public void Reinterpret_ReusesUnderlyingValues()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 0);

            var reinterpreted = map.Reinterpret<uint>();

            Assert.AreEqual(map.Count, reinterpreted.Count);
            Assert.IsTrue(reinterpreted.ContainsKey(1));
        }

        [Test]
        public void Intersect_KeepsOnlySharedKeys()
        {
            using var a = new ArrayMapNative<int, int>(8, Allocator.Temp);
            using var b = new ArrayMapNative<int, int>(8, Allocator.Temp);

            a.Add(1, 1);
            a.Add(2, 2);
            a.Add(3, 3);

            b.Add(2, 0);
            b.Add(3, 0);
            b.Add(4, 0);

            a.Intersect(in b);

            Assert.AreEqual(2, a.Count);
            Assert.IsFalse(a.ContainsKey(1));
            Assert.IsTrue(a.ContainsKey(2));
            Assert.IsTrue(a.ContainsKey(3));
        }

        [Test]
        public void Exclude_RemovesSharedKeys()
        {
            using var a = new ArrayMapNative<int, int>(8, Allocator.Temp);
            using var b = new ArrayMapNative<int, int>(8, Allocator.Temp);

            a.Add(1, 1);
            a.Add(2, 2);
            a.Add(3, 3);

            b.Add(2, 0);
            b.Add(3, 0);

            a.Exclude(in b);

            Assert.AreEqual(1, a.Count);
            Assert.IsTrue(a.ContainsKey(1));
            Assert.IsFalse(a.ContainsKey(2));
            Assert.IsFalse(a.ContainsKey(3));
        }

        [Test]
        public void Union_MergesAndOverwrites()
        {
            using var a = new ArrayMapNative<int, int>(8, Allocator.Temp);
            using var b = new ArrayMapNative<int, int>(8, Allocator.Temp);

            a.Add(1, 1);
            a.Add(2, 2);

            b.Add(2, 200);
            b.Add(3, 300);

            a.Union(in b);

            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(1, a[1]);
            Assert.AreEqual(200, a[2]);
            Assert.AreEqual(300, a[3]);
        }

        [Test]
        public void AsReadOnly_ReflectsContent()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            var readOnly = map.AsReadOnly();

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(2, readOnly.Count);
            Assert.AreEqual(10, readOnly[1]);
            Assert.IsTrue(readOnly.ContainsKey(2));
            Assert.IsTrue(readOnly.TryGetValue(2, out var value));
            Assert.AreEqual(20, value);
            Assert.IsFalse(readOnly.ContainsKey(99));
        }

        [Test]
        public void AsReadOnly_TryGetValue_TypeWrap_ReturnsExpected()
        {
            using var map = new ArrayMapNative<UintId, uint>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            Asserts(map.AsReadOnly());

            static void Asserts(ArrayMapNative<UintId, uint>.ReadOnly readOnly)
            {
                Assert.IsTrue(readOnly.TryGetValue(1, out var value1));
                Assert.AreEqual(10, value1);

                Assert.IsTrue(readOnly.TryGetValue(2, out var value2));
                Assert.AreEqual(20, value2);

                Assert.IsFalse(readOnly.TryGetValue(3, out var missing));
                Assert.AreEqual(0, missing);
            }
        }

        [Test]
        public void AsReadOnly_Enumerator_VisitsAllPairs()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            for (var i = 0; i < 4; i++)
            {
                map.Add(i, i * 10);
            }

            var seen = new Dictionary<int, int>();

            foreach (var pair in map.AsReadOnly())
            {
                seen[pair.Key] = pair.Value;
            }

            Assert.AreEqual(4, seen.Count);

            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(i * 10, seen[i]);
            }
        }

        [Test]
        public void ReadOnly_AllPublicMembersReflectOwner()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            var readOnly = map.AsReadOnly();

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(map.Capacity, readOnly.Capacity);
            Assert.AreEqual(2, readOnly.Count);
            Assert.IsTrue(readOnly.Keys.IsValid);
            Assert.AreEqual(2, readOnly.Values.Length);
            Assert.AreEqual(10, readOnly.Values[0]);
            Assert.AreEqual(10, readOnly[1]);
            Assert.IsTrue(readOnly.ContainsKey(2));
            Assert.IsFalse(readOnly.ContainsKey(3));
            Assert.IsTrue(readOnly.TryGetValue(2, out var value));
            Assert.AreEqual(20, value);
            Assert.IsFalse(readOnly.TryGetValue(3, out _));
            Assert.AreEqual(10, readOnly.GetValueByRef(1));
            Assert.IsTrue(readOnly.TryFindIndex(2, out var index));
            Assert.AreEqual(1, index);
            Assert.IsFalse(readOnly.TryFindIndex(3, out _));
            Assert.AreEqual(0, readOnly.FindIndex(1));
            Assert.AreEqual(-1, readOnly.FindIndex(3));
        }

        [Test]
        public void Enumerators_DirectConstructionMovementResetCurrentAndDispose()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(7, 70);
            var readOnly = map.AsReadOnly();

            var keys = new ArrayMapNative<int, int>.KeyEnumerable(in readOnly);
            Assert.IsTrue(keys.IsValid);

            var keyEnumerator = new ArrayMapNative<int, int>.KeyEnumerator(in readOnly);
            Assert.IsTrue(keyEnumerator.IsValid);
            Assert.IsTrue(keyEnumerator.MoveNext());
            Assert.AreEqual(7, keyEnumerator.Current);
            Assert.IsFalse(keyEnumerator.MoveNext());
            keyEnumerator.Reset();
            Assert.IsTrue(keyEnumerator.MoveNext());
            keyEnumerator.Dispose();

            var enumerableEnumerator = keys.GetEnumerator();
            Assert.IsTrue(enumerableEnumerator.MoveNext());
            Assert.AreEqual(7, enumerableEnumerator.Current);
            enumerableEnumerator.Dispose();

            var ownerEnumerator = new ArrayMapNativeKeyValueEnumerator<int, int>(in map);
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

            var readOnlyEnumerator = new ArrayMapNativeReadOnlyKeyValueEnumerator<int, int>(in readOnly);
            Assert.IsTrue(readOnlyEnumerator.IsValid);
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            AssertPair(readOnlyEnumerator.Current, 7, 70);
            Assert.IsFalse(readOnlyEnumerator.MoveNext());
            readOnlyEnumerator.Reset();
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            readOnlyEnumerator.Dispose();

            var viewEnumerator = readOnly.GetEnumerator();
            Assert.IsTrue(viewEnumerator.MoveNext());
            viewEnumerator.Dispose();

            Assert.IsFalse(default(ArrayMapNative<int, int>.KeyEnumerable).IsValid);
            Assert.IsFalse(default(ArrayMapNative<int, int>.KeyEnumerator).IsValid);
            Assert.IsFalse(default(ArrayMapNativeKeyValueEnumerator<int, int>).IsValid);
            Assert.IsFalse(default(ArrayMapNativeReadOnlyKeyValueEnumerator<int, int>).IsValid);
            Assert.IsFalse(default(ArrayMapNativeKeyValuePair<int, int>).IsValid);
            Assert.IsFalse(default(ArrayMapNativeReadOnlyKeyValuePair<int, int>).IsValid);
        }

        [Test]
        public void IncreaseCapacityToAndTryFindIndex_GrowAndReportBothPaths()
        {
            using var map = new ArrayMapNative<int, int>(2, Allocator.Temp);
            map.Add(1, 10);
            var requestedCapacity = map.Capacity + 5;

            map.IncreaseCapacityTo(requestedCapacity);

            Assert.GreaterOrEqual(map.Capacity, requestedCapacity);
            Assert.IsTrue(map.TryFindIndex(1, out var index));
            Assert.AreEqual(0, index);
            Assert.IsFalse(map.TryFindIndex(2, out _));
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            Assert.IsTrue(map.IsCreated);

            map.Dispose();

            Assert.IsFalse(map.IsCreated);
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.TempJob);
            map.Add(1, 10);

            var handle = map.Dispose(default);

            Assert.IsFalse(map.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void Collisions_ManyKeys_AllRetrievable()
        {
            const int N = 500;
            using var map = new ArrayMapNative<int, int>(2, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                map.Add(i, i);
            }

            for (var i = 0; i < N; i++)
            {
                Assert.IsTrue(map.TryGetValue(i, out var value), $"missing key {i}");
                Assert.AreEqual(i, value);
            }
        }

        private static void AssertPair(ArrayMapNativeKeyValuePair<int, int> pair, int key, int value)
        {
            Assert.IsTrue(pair.IsValid);
            Assert.AreEqual(key, pair.Key);
            Assert.AreEqual(value, pair.Value);

            pair.Deconstruct(out var actualKey, out var actualValue);

            Assert.AreEqual(key, actualKey);
            Assert.AreEqual(value, actualValue);
        }

        private static void AssertPair(ArrayMapNativeReadOnlyKeyValuePair<int, int> pair, int key, int value)
        {
            Assert.IsTrue(pair.IsValid);
            Assert.AreEqual(key, pair.Key);
            Assert.AreEqual(value, pair.Value);

            pair.Deconstruct(out var actualKey, out var actualValue);

            Assert.AreEqual(key, actualKey);
            Assert.AreEqual(value, actualValue);
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAddsAndDisposes(Allocator allocator)
        {
            var map = new ArrayMapNative<int, int>(4, allocator);

            try
            {
                map.Add(1, 10);

                Assert.IsTrue(map.IsCreated);
                Assert.AreEqual(1, map.Count);
                Assert.AreEqual(10, map[1]);
            }
            finally
            {
                map.Dispose();
            }

            Assert.IsFalse(map.IsCreated);
        }

        [Test]
        public void Constructor_ZeroCapacity_CreatesEmptyMapAndGrowsOnAdd()
        {
            var map = new ArrayMapNative<int, int>(0, Allocator.Temp);

            try
            {
                Assert.IsTrue(map.IsCreated);
                Assert.AreEqual(0, map.Count);
                Assert.AreEqual(0, map.Capacity);

                map.Add(1, 10);
                map.Add(2, 20);

                Assert.AreEqual(2, map.Count);
                Assert.GreaterOrEqual(map.Capacity, 2);
                Assert.AreEqual(10, map[1]);
                Assert.AreEqual(20, map[2]);
            }
            finally
            {
                map.Dispose();
            }
        }

        [Test]
        public void CopyConstructor_EmptySource_CreatesIndependentEmptyMap()
        {
            using var source = new ArrayMapNative<int, int>(4, Allocator.Temp);
            using var copy = new ArrayMapNative<int, int>(source, Allocator.Temp);

            Assert.IsTrue(copy.IsCreated);
            Assert.AreEqual(0, copy.Count);

            copy.Add(1, 10);

            Assert.AreEqual(1, copy.Count);
            Assert.AreEqual(0, source.Count);
            Assert.IsFalse(source.ContainsKey(1));
        }

        [Test]
        public void EmptyMap_Queries_ReturnEmptyResults()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            var keyCount = 0;

            foreach (var key in map.Keys)
            {
                keyCount++;
            }

            var enumerator = map.GetEnumerator();

            Assert.AreEqual(0, keyCount);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.IsFalse(map.ContainsKey(1));
            Assert.IsFalse(map.TryGetValue(1, out var value));
            Assert.AreEqual(0, value);
            Assert.AreEqual(-1, map.FindIndex(1));
            Assert.IsFalse(map.TryFindIndex(1, out _));
            Assert.AreEqual(0, map.Values.Length);
            Assert.IsFalse(map.Remove(1));
            Assert.DoesNotThrow(() => map.Clear());
            Assert.DoesNotThrow(() => map.Recycle());
            Assert.AreEqual(0, map.Count);

            var capacity = map.Capacity;

            Assert.AreEqual(capacity, map.EnsureCapacity(capacity - 1));
            Assert.AreEqual(capacity, map.EnsureCapacity(capacity));
            Assert.AreEqual(capacity, map.Capacity);
        }

        [Test]
        public void Remove_OnEmptyMap_ReturnsFalseAndDefaultOutputs()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            Assert.IsFalse(map.Remove(5, out var index, out var value));
            Assert.AreEqual(0, index);
            Assert.AreEqual(0, value);
            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void SetOperations_EmptyOperands_ProduceTheSetResult()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            using var empty = new ArrayMapNative<int, int>(4, Allocator.Temp);
            using var emptyBytes = new ArrayMapNative<int, byte>(4, Allocator.Temp);

            map.Add(1, 10);
            map.Add(2, 20);

            map.Exclude(in empty);
            map.Union(in empty);

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(20, map[2]);

            map.Exclude(in emptyBytes);

            Assert.AreEqual(2, map.Count);

            map.Intersect(in emptyBytes);

            Assert.AreEqual(0, map.Count);
            Assert.IsFalse(map.ContainsKey(1));

            empty.Intersect(in map);
            empty.Exclude(in map);
            empty.Union(in empty);

            Assert.AreEqual(0, empty.Count);
        }

        [Test]
        public void SetOperations_DefaultOperand_Throws()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            ArrayMapNative<int, int> notCreated = default;
            ArrayMapNative<int, byte> notCreatedBytes = default;

            Assert.Throws<ArgumentException>(() => map.Intersect(in notCreatedBytes));
            Assert.Throws<ArgumentException>(() => map.Exclude(in notCreatedBytes));
            Assert.Throws<ArgumentException>(() => map.Union(in notCreated));
        }

        [Test]
        public void Trim_EmptyMap_ShrinksToZeroAndStaysUsable()
        {
            var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            try
            {
                map.Trim();

                Assert.IsTrue(map.IsCreated);
                Assert.AreEqual(0, map.Capacity);
                Assert.AreEqual(0, map.Count);

                map.Add(1, 10);

                Assert.AreEqual(1, map.Count);
                Assert.AreEqual(10, map[1]);
            }
            finally
            {
                map.Dispose();
            }
        }

        [Test]
        public void Values_CountBelowCapacity_LengthIsCountAndSeesLaterWrites()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            var values = map.Values;

            Assert.AreEqual(2, values.Length);
            Assert.GreaterOrEqual(map.Capacity, 8);
            CollectionAssert.AreEqual(new[] { 10, 20 }, values.ToArray());

            map.GetValueByRef(2) = 99;

            Assert.AreEqual(99, values[1]);
        }

        [Test]
        public void Values_ElementShapes_RoundTripBitExact()
        {
            using var small = new ArrayMapNative<int, Rgb>(4, Allocator.Temp);
            using var wide = new ArrayMapNative<int, Wide>(4, Allocator.Temp);
            using var longKeys = new ArrayMapNative<long, byte>(4, Allocator.Temp);

            small.Add(1, new Rgb { r = 1, g = 2, b = 3 });
            small.Add(2, new Rgb { r = 250, g = 251, b = 252 });
            wide.Add(1, new Wide { a = long.MinValue, b = -1L, c = long.MaxValue });
            longKeys.Add(long.MaxValue, 255);
            longKeys.Add(long.MinValue, 1);

            var smallValues = small.Values;
            var wideValues = wide.Values;

            Assert.AreEqual(2, smallValues.Length);
            Assert.AreEqual(1, smallValues[0].r);
            Assert.AreEqual(3, smallValues[0].b);
            Assert.AreEqual(250, smallValues[1].r);
            Assert.AreEqual(252, smallValues[1].b);
            Assert.AreEqual(long.MinValue, wideValues[0].a);
            Assert.AreEqual(-1L, wideValues[0].b);
            Assert.AreEqual(long.MaxValue, wideValues[0].c);
            Assert.AreEqual(252, small[2].b);
            Assert.AreEqual(long.MaxValue, wide[1].c);
            Assert.AreEqual(255, longKeys[long.MaxValue]);
            Assert.AreEqual(1, longKeys[long.MinValue]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Values_SliceUsedAfterOwnerDispose_Throws()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Persistent);
            map.Add(1, 10);
            var values = map.Values;

            map.Dispose();

            Assert.Catch(() => _ = values[0]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Members_AfterOwnerDispose_Throw()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Persistent);
            var other = new ArrayMapNative<int, byte>(4, Allocator.Persistent);
            var otherSame = new ArrayMapNative<int, int>(4, Allocator.Persistent);

            try
            {
                map.Add(1, 10);
                map.Dispose();

                Assert.Catch(() => _ = map.Capacity);
                Assert.Catch(() => _ = map.Count);
                Assert.Catch(() => _ = map.Keys);
                Assert.Catch(() => _ = map.Values);
                Assert.Catch(() => _ = map[1]);
                Assert.Catch(() => map[1] = 5);
                Assert.Catch(() => map.GetEnumerator());
                Assert.Catch(() => map.Add(2, 20));
                Assert.Catch(() => map.TryAdd(2, 20, out _));
                Assert.Catch(() => map.Recycle());
                Assert.Catch(() => map.Clear());
                Assert.Catch(() => map.ContainsKey(1));
                Assert.Catch(() => map.TryGetValue(1, out _));
                Assert.Catch(() => map.GetOrAdd(1));
                Assert.Catch(() => map.GetOrAdd(1, out _));
                Assert.Catch(() => map.GetValueByRef(1));
                Assert.Catch(() => map.EnsureCapacity(8));
                Assert.Catch(() => map.IncreaseCapacityBy(1));
                Assert.Catch(() => map.IncreaseCapacityTo(8));
                Assert.Catch(() => map.Remove(1));
                Assert.Catch(() => map.Remove(1, out _, out _));
                Assert.Catch(() => map.Trim());
                Assert.Catch(() => map.TryFindIndex(1, out _));
                Assert.Catch(() => map.FindIndex(1));
                Assert.Catch(() => map.Intersect(in other));
                Assert.Catch(() => map.Exclude(in other));
                Assert.Catch(() => map.Union(in otherSame));
                Assert.Catch(() => map.AddValue(2, out _));
                Assert.Catch(() => map.GetValueRefAt(0));
                Assert.Catch(() => map.BumpVersion());
                Assert.Catch(() => map.AsValuesSpan());
            }
            finally
            {
                other.Dispose();
                otherSame.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Dispose_Twice_Throws()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Persistent);

            map.Dispose();

            Assert.IsFalse(map.IsCreated);
            Assert.Catch(() => map.Dispose());
        }

        [Test]
        public void Dispose_DefaultMap_IsNoOp()
        {
            ArrayMapNative<int, int> map = default;

            Assert.DoesNotThrow(() => map.Dispose());

            var handle = map.Dispose(default);

            Assert.DoesNotThrow(() => handle.Complete());
            Assert.IsFalse(map.IsCreated);
        }

        [Test]
        public void DisposeJob_PersistentAllocator_CompletesAndMarksNotCreated()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Persistent);
            map.Add(1, 10);

            var handle = map.Dispose(default);

            Assert.IsFalse(map.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void Reinterpret_SameSizeViews_ShareTheMapAndReinterpretBits()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            try
            {
                var emptyView = map.Reinterpret<uint>();

                Assert.IsTrue(emptyView.IsCreated);
                Assert.AreEqual(0, emptyView.Count);

                map.Add(1, -1);
                map.Add(3, 0x3F800000);

                var unsignedView = map.Reinterpret<uint>();
                var floatView = map.Reinterpret<float>();

                Assert.AreEqual(2, unsignedView.Count);
                Assert.AreEqual(uint.MaxValue, unsignedView[1]);
                Assert.AreEqual(1f, floatView[3]);

                unsignedView[2] = 7u;

                Assert.AreEqual(3, map.Count);
                Assert.AreEqual(7, map[2]);
                Assert.AreEqual(3, floatView.Count);
            }
            finally
            {
                map.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_DifferentSize_Throws()
        {
            var bytes = new ArrayMapNative<int, byte>(4, Allocator.Temp);
            var ints = new ArrayMapNative<int, int>(4, Allocator.Temp);

            try
            {
                Assert.Throws<InvalidOperationException>(() => bytes.Reinterpret<long>());
                Assert.Throws<InvalidOperationException>(() => ints.Reinterpret<short>());
                Assert.Throws<InvalidOperationException>(() => ints.Reinterpret<Rgb>());
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
            var map = new ArrayMapNative<int, int>(4, Allocator.Persistent);
            map.Add(1, 10);
            var view = map.Reinterpret<uint>();

            map.Dispose();

            Assert.Catch(() => _ = view.Count);
            Assert.Catch(() => _ = view[1]);
            Assert.Catch(() => view.ContainsKey(1));
        }

        [Test]
        public void TryAdd_FullMap_GrowsAndKeepsEntries()
        {
            using var map = new ArrayMapNative<int, int>(2, Allocator.Temp);

            Assert.IsTrue(map.TryAdd(1, 10, out var first));
            Assert.IsTrue(map.TryAdd(2, 20, out var second));
            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(2, map.Capacity);

            Assert.IsTrue(map.TryAdd(3, 30, out var third));
            Assert.IsFalse(map.TryAdd(2, 99, out var duplicate));

            Assert.AreEqual(0, first);
            Assert.AreEqual(1, second);
            Assert.AreEqual(2, third);
            Assert.AreEqual(second, duplicate);
            Assert.AreEqual(3, map.Count);
            Assert.GreaterOrEqual(map.Capacity, 3);
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(20, map[2]);
            Assert.AreEqual(30, map[3]);
        }

        [Test]
        public void Recycle_EmptyMap_IsNoOp()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            map.Recycle();

            Assert.AreEqual(0, map.Count);
            Assert.GreaterOrEqual(map.Capacity, 4);
        }

        [Test]
        public void Recycle_ThenAddPastTheOldCount_StoresOnlyTheNewEntries()
        {
            using var map = new ArrayMapNative<int, int>(2, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            map.Recycle();

            map.Add(3, 30);
            map.Add(4, 40);
            map.Add(5, 50);

            Assert.AreEqual(3, map.Count);
            Assert.IsFalse(map.ContainsKey(1));
            Assert.IsFalse(map.ContainsKey(2));
            Assert.AreEqual(30, map[3]);
            Assert.AreEqual(40, map[4]);
            Assert.AreEqual(50, map[5]);
        }

        [Test]
        public void GetOrAdd_FullMap_GrowsAndReturnsWritableReference()
        {
            using var map = new ArrayMapNative<int, int>(2, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            ref var added = ref map.GetOrAdd(3);
            added = 30;

            ref var addedWithIndex = ref map.GetOrAdd(4, out var index);
            addedWithIndex = 40;

            Assert.AreEqual(3, index);
            Assert.AreEqual(4, map.Count);
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(20, map[2]);
            Assert.AreEqual(30, map[3]);
            Assert.AreEqual(40, map[4]);
        }

        [Test]
        public void GetOrAdd_SameKeyTwice_ReturnsReferencesToOneSlot()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            ref var first = ref map.GetOrAdd(1);
            ref var second = ref map.GetOrAdd(1);
            first = 5;

            Assert.AreEqual(5, second);
            Assert.AreEqual(5, map[1]);
            Assert.AreEqual(1, map.Count);

            second = 6;

            Assert.AreEqual(6, first);
            Assert.AreEqual(6, map[1]);
        }

        [Test]
        public void Remove_FirstKey_MovesTheLastEntryIntoTheHole()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);
            map.Add(4, 40);

            Assert.IsTrue(map.Remove(1, out var index, out var removed));

            Assert.AreEqual(0, index);
            Assert.AreEqual(10, removed);
            Assert.AreEqual(3, map.Count);
            CollectionAssert.AreEqual(new[] { 40, 20, 30 }, map.Values.ToArray());
            Assert.AreEqual(0, map.FindIndex(4));
            Assert.AreEqual(1, map.FindIndex(2));
            Assert.AreEqual(2, map.FindIndex(3));
            Assert.IsFalse(map.ContainsKey(1));
            Assert.AreEqual(40, map[4]);
        }

        [Test]
        public void Remove_LastRemainingKey_LeavesAnEmptyMapThatAcceptsNewKeys()
        {
            using var map = new ArrayMapNative<int, int>(2, Allocator.Temp);
            map.Add(1, 10);

            Assert.IsTrue(map.Remove(1));
            Assert.AreEqual(0, map.Count);

            map.Add(1, 11);
            map.Add(2, 22);

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(11, map[1]);
            Assert.AreEqual(22, map[2]);
        }

        [Test]
        public void AddValue_NewAndExistingKey_ReportsWhetherTheKeyWasAdded()
        {
            var map = new ArrayMapNative<int, int>(2, Allocator.Temp);

            try
            {
                Assert.IsTrue(map.AddValue(1, out var first));
                Assert.IsTrue(map.AddValue(2, out var second));
                Assert.IsTrue(map.AddValue(3, out var third));
                Assert.IsFalse(map.AddValue(2, out var again));

                Assert.AreEqual(0, first);
                Assert.AreEqual(1, second);
                Assert.AreEqual(2, third);
                Assert.AreEqual(second, again);
                Assert.AreEqual(3, map.Count);
                Assert.GreaterOrEqual(map.Capacity, 3);
            }
            finally
            {
                map.Dispose();
            }
        }

        [Test]
        public void GetValueRefAt_WritesThroughToTheKeyedValue()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(5, 50);
            map.Add(6, 60);

            ref var second = ref map.GetValueRefAt(map.FindIndex(6));
            second = 66;

            Assert.AreEqual(66, map[6]);
            Assert.AreEqual(50, map[5]);
            Assert.AreEqual(66, map.GetValueRefAt(1));

            map.GetValueByRef(5) = 55;

            Assert.AreEqual(55, map.GetValueRefAt(0));
        }

        [Test]
        public void BumpVersion_InvalidatesRunningEnumerators()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            var enumerator = map.GetEnumerator();
            var keyEnumerator = map.Keys.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(keyEnumerator.MoveNext());

            map.BumpVersion();

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => keyEnumerator.MoveNext());
        }

        [Test]
        public void AsValuesSpan_CoversTheCountAndAliasesTheStorage()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            Assert.AreEqual(0, map.AsValuesSpan().Length);

            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);

            var span = map.AsValuesSpan();

            Assert.AreEqual(3, span.Length);
            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, span.ToArray());

            span[1] = 99;
            map.GetValueByRef(3) = 33;

            Assert.AreEqual(99, map[2]);
            Assert.AreEqual(33, span[2]);
        }

        [Test]
        public void AsValuesSpan_ElementShapes_RoundTripBitExact()
        {
            using var small = new ArrayMapNative<int, Rgb>(4, Allocator.Temp);
            using var wide = new ArrayMapNative<int, Wide>(4, Allocator.Temp);

            small.Add(1, new Rgb { r = 7, g = 8, b = 9 });
            wide.Add(1, new Wide { a = 1L, b = long.MinValue, c = long.MaxValue });

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
        public void KeyEnumerable_ModifiedDuringEnumeration_Throws()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            var enumerator = map.Keys.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            map.Add(3, 30);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public void EnumeratorsAndKeyEnumerable_FromDefaultMap_Throw()
        {
            ArrayMapNative<int, int> notCreated = default;
            ArrayMapNative<int, int>.ReadOnly notCreatedReadOnly = default;

            Assert.Throws<ArgumentException>(() => _ = new ArrayMapNativeKeyValueEnumerator<int, int>(in notCreated));
            Assert.Throws<ArgumentException>(
                () => _ = new ArrayMapNativeReadOnlyKeyValueEnumerator<int, int>(in notCreatedReadOnly)
            );
            Assert.Throws<ArgumentException>(
                () => _ = new ArrayMapNative<int, int>.KeyEnumerable(in notCreatedReadOnly)
            );
            Assert.Throws<ArgumentException>(
                () => _ = new ArrayMapNative<int, int>.KeyEnumerator(in notCreatedReadOnly)
            );
        }

        [Test]
        public void Enumerators_EmptyMap_MoveNextReturnsFalse()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            var readOnly = map.AsReadOnly();

            var ownerEnumerator = new ArrayMapNativeKeyValueEnumerator<int, int>(in map);
            var readOnlyEnumerator = new ArrayMapNativeReadOnlyKeyValueEnumerator<int, int>(in readOnly);
            var keyEnumerator = new ArrayMapNative<int, int>.KeyEnumerator(in readOnly);

            Assert.IsTrue(ownerEnumerator.IsValid);
            Assert.IsFalse(ownerEnumerator.MoveNext());
            Assert.IsFalse(readOnlyEnumerator.MoveNext());
            Assert.IsFalse(keyEnumerator.MoveNext());
        }

        [Test]
        public unsafe void KeyValuePairs_NullMap_AreNotValidAndKeepTheirKey()
        {
            var pair = new ArrayMapNativeKeyValuePair<int, int>(7, null, 0);
            var readOnlyPair = new ArrayMapNativeReadOnlyKeyValuePair<int, int>(8, null, 0);

            Assert.IsFalse(pair.IsValid);
            Assert.AreEqual(7, pair.Key);
            Assert.IsFalse(readOnlyPair.IsValid);
            Assert.AreEqual(8, readOnlyPair.Key);
        }

        [Test]
        public unsafe void NativeDispose_NullHeader_IsNoOp()
        {
            Assert.DoesNotThrow(() => ArrayMapNativeDispose.Dispose<int, int>(null));
            Assert.DoesNotThrow(() => default(ArrayMapNativeDispose).Dispose());
        }

        [Test]
        public unsafe void NativeDispose_AllocatedHeader_ReleasesIt()
        {
            var data = ArrayMapUnsafe<int, int>.Alloc(4, Allocator.Persistent);
            data->Add(1, 10);

            Assert.IsTrue(data->IsCreated);
            Assert.AreEqual(1, data->Count);

            ArrayMapNativeDispose.Dispose<int, int>(data);
        }

        [Test]
        public void ReadOnly_EmptyMap_ReturnsEmptyResults()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            var readOnly = map.AsReadOnly();
            var keyCount = 0;

            foreach (var key in readOnly.Keys)
            {
                keyCount++;
            }

            var enumerator = readOnly.GetEnumerator();

            Assert.AreEqual(0, keyCount);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, readOnly.Count);
            Assert.AreEqual(0, readOnly.Values.Length);
            Assert.AreEqual(0, readOnly.AsValuesReadOnlySpan().Length);
            Assert.IsFalse(readOnly.ContainsKey(1));
            Assert.IsFalse(readOnly.TryGetValue(1, out _));
            Assert.AreEqual(-1, readOnly.FindIndex(1));
            Assert.IsFalse(readOnly.TryFindIndex(1, out _));
            Assert.Throws<KeyNotFoundException>(() => readOnly.GetValueByRef(1));
        }

        [Test]
        public void ReadOnly_ConstructorFromDefaultMap_Throws()
        {
            ArrayMapNative<int, int> notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new ArrayMapNative<int, int>.ReadOnly(notCreated));
            Assert.Throws<ArgumentException>(() => notCreated.AsReadOnly());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnly_Members_AfterOwnerDispose_Throw()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Persistent);
            map.Add(1, 10);
            var readOnly = map.AsReadOnly();

            map.Dispose();

            Assert.Catch(() => _ = readOnly.Capacity);
            Assert.Catch(() => _ = readOnly.Count);
            Assert.Catch(() => _ = readOnly.Keys);
            Assert.Catch(() => _ = readOnly.Values);
            Assert.Catch(() => _ = readOnly[1]);
            Assert.Catch(() => readOnly.GetEnumerator());
            Assert.Catch(() => readOnly.ContainsKey(1));
            Assert.Catch(() => readOnly.TryGetValue(1, out _));
            Assert.Catch(() => readOnly.GetValueByRef(1));
            Assert.Catch(() => readOnly.FindIndex(1));
            Assert.Catch(() => readOnly.TryFindIndex(1, out _));
            Assert.Catch(() => readOnly.AsValuesReadOnlySpan());
        }

        [Test]
        public void ReadOnly_UncheckedVersionAndKeyAt_ReflectTheMap()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            var readOnly = map.AsReadOnly();
            var initialVersion = readOnly.UncheckedVersion;

            map.Add(7, 70);
            map.Add(8, 80);

            Assert.AreNotEqual(initialVersion, readOnly.UncheckedVersion);
            Assert.AreEqual(7, readOnly.KeyAt(0));
            Assert.AreEqual(8, readOnly.KeyAt(1));

            var versionBeforeBump = readOnly.UncheckedVersion;

            map.BumpVersion();

            Assert.AreNotEqual(versionBeforeBump, readOnly.UncheckedVersion);
        }

        [Test]
        public void ReadOnly_AsValuesReadOnlySpan_CoversTheCountAndSeesWrites()
        {
            using var map = new ArrayMapNative<int, Rgb>(8, Allocator.Temp);
            map.Add(1, new Rgb { r = 1, g = 2, b = 3 });
            map.Add(2, new Rgb { r = 4, g = 5, b = 6 });

            var readOnly = map.AsReadOnly();
            var span = readOnly.AsValuesReadOnlySpan();

            Assert.AreEqual(2, span.Length);
            Assert.AreEqual(3, span[0].b);
            Assert.AreEqual(4, span[1].r);

            map.GetValueByRef(1).g = 22;

            Assert.AreEqual(22, span[0].g);
            Assert.AreEqual(22, readOnly[1].g);
            Assert.AreEqual(2, readOnly.Values.Length);
            Assert.AreEqual(6, readOnly.Values[1].b);
        }

        [Test]
        public void ReadOnly_ModifiedDuringEnumeration_Throws()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            var readOnly = map.AsReadOnly();
            var enumerator = readOnly.GetEnumerator();
            var keyEnumerator = readOnly.Keys.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(keyEnumerator.MoveNext());

            map.Add(3, 30);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => keyEnumerator.MoveNext());
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
