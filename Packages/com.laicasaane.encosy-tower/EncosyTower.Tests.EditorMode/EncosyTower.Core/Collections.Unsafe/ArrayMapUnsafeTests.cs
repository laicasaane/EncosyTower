using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Common;
using EncosyTower.TypeWraps;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    // Adapted from Unity.Collections.Tests/UnsafeHashMapTests.cs + NativeHashMapTests.cs,
    // mirroring the ArrayMapNativeTests compatibility gate against the raw Unsafe header.
    public partial class ArrayMapUnsafeTests
    {
        [WrapRecord]
        public readonly partial record struct UintId(uint Value);

        [Test]
        public void Constructor_CreatesValidMap()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            Assert.IsTrue(map.IsCreated);
            Assert.AreEqual(0, map.Count);
            Assert.GreaterOrEqual(map.Capacity, 4);
        }

        [Test]
        public void DefaultMap_IsNotCreated()
        {
            ArrayMapUnsafe<int, int> map = default;

            Assert.IsFalse(map.IsCreated);
        }

        [Test]
        public void Add_StoresValues()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

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
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.Throws<InvalidOperationException>(() => map.Add(1, 20));
        }

        [Test]
        public void Indexer_Set_AddsOrOverwrites()
        {
            var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

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
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            Assert.IsTrue(map.TryAdd(1, 10, out var index0));
            Assert.AreEqual(0, index0);

            Assert.IsFalse(map.TryAdd(1, 20, out var index1));
            Assert.AreEqual(0, index1);
            Assert.AreEqual(10, map[1]);
        }

        [Test]
        public void ContainsKey_ReflectsContent()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.IsTrue(map.ContainsKey(1));
            Assert.IsFalse(map.ContainsKey(2));
        }

        [Test]
        public void TryGetValue_ReturnsExpected()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
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
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.AreEqual(0, map.FindIndex(1));
            Assert.AreEqual(-1, map.FindIndex(2));
        }

        [Test]
        public void GetOrAdd_AddsWhenMissing()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            ref var value = ref map.GetOrAdd(1);
            Assert.AreEqual(0, value);
            value = 42;

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(42, map[1]);
        }

        [Test]
        public void GetOrAdd_ReturnsExistingByRef()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            ref var value = ref map.GetOrAdd(1);
            Assert.AreEqual(10, value);
            value = 20;

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(20, map[1]);
        }

        [Test]
        public void GetOrAdd_OutIndex_ReportsIndex()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            map.GetOrAdd(1, out var index0) = 5;
            Assert.AreEqual(0, index0);

            map.GetOrAdd(2, out var index1) = 6;
            Assert.AreEqual(1, index1);

            map.GetOrAdd(1, out var indexExisting);
            Assert.AreEqual(0, indexExisting);
        }

        [Test]
        public void GetValueByRef_ReturnsRef()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            ref var value = ref map.GetValueByRef(1);
            value = 99;

            Assert.AreEqual(99, map[1]);
        }

        [Test]
        public void Remove_RemovesKey()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
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
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.IsFalse(map.Remove(99));
            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void Remove_OutValue_ReportsRemoved()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            Assert.IsTrue(map.Remove(1, out var index, out var value));
            Assert.AreEqual(10, value);
            Assert.GreaterOrEqual(index, 0);
        }

        [Test]
        public void RemoveAndReadd_StaysConsistent()
        {
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);

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
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
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
        public void Recycle_ResetsCount()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
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
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

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
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            var before = map.Capacity;

            map.EnsureCapacity(before + 100);

            Assert.Greater(map.Capacity, before);
        }

        [Test]
        public void IncreaseCapacityBy_GrowsCapacity()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            var before = map.Capacity;

            map.IncreaseCapacityBy(50);

            Assert.GreaterOrEqual(map.Capacity, before + 50);
        }

        [Test]
        public void Trim_ShrinksCapacityToCount()
        {
            using var map = new ArrayMapUnsafe<int, int>(64, Allocator.Temp);
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
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);

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
        public void Keys_EnumeratesAllKeys()
        {
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);

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
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);

            var values = map.Values;

            Assert.AreEqual(3, values.Length);
        }

        [Test]
        public void CopyConstructor_DeepCopiesContent()
        {
            using var source = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);
            source.Add(1, 10);
            source.Add(2, 20);

            using var copy = new ArrayMapUnsafe<int, int>(source, Allocator.Temp);

            Assert.AreEqual(source.Count, copy.Count);
            Assert.AreEqual(10, copy[1]);
            Assert.AreEqual(20, copy[2]);

            copy.Add(3, 30);
            Assert.IsFalse(source.ContainsKey(3));
        }

        [Test]
        public void CopyConstructor_AfterBucketGrowth_AllKeysResolve()
        {
            // Regression for the copy-ctor bucket-capacity fix: collisions grow the source
            // buckets past GetPrime(capacity); the copy must allocate dest buckets from the
            // source bucket count, not GetPrime(capacity), or keys are lost after copy.
            const int N = 500;
            using var source = new ArrayMapUnsafe<int, int>(2, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                source.Add(i, i * 7);
            }

            using var copy = new ArrayMapUnsafe<int, int>(source, Allocator.Temp);

            Assert.AreEqual(N, copy.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.IsTrue(copy.TryGetValue(i, out var value), $"missing key {i} after copy");
                Assert.AreEqual(i * 7, value);
            }
        }

        [Test]
        public void Reinterpret_ReusesUnderlyingValues()
        {
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);
            map.Add(1, 0);

            var reinterpreted = map.Reinterpret<uint>();

            Assert.AreEqual(map.Count, reinterpreted.Count);
            Assert.IsTrue(reinterpreted.ContainsKey(1));
        }

        [Test]
        public void Intersect_KeepsOnlySharedKeys()
        {
            using var a = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);
            using var b = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);

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
            using var a = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);
            using var b = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);

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
            using var a = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);
            using var b = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);

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
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);
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
            using var map = new ArrayMapUnsafe<UintId, uint>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            Asserts(map.AsReadOnly());

            static void Asserts(ArrayMapUnsafe<UintId, uint>.ReadOnly readOnly)
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
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);

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
        public void ReadOnly_ConstructorAndAllPublicMembersReflectOwner()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            var direct = new ArrayMapUnsafe<int, int>.ReadOnly(map);
            var fromOwner = map.AsReadOnly();

            Assert.IsTrue(direct.IsCreated);
            Assert.AreEqual(map.Capacity, direct.Capacity);
            Assert.AreEqual(2, direct.Count);
            Assert.IsTrue(direct.Keys.IsValid);
            Assert.AreEqual(2, direct.Values.Length);
            Assert.AreEqual(10, direct.Values[0]);
            Assert.AreEqual(10, direct[1]);
            Assert.IsTrue(direct.ContainsKey(2));
            Assert.IsFalse(direct.ContainsKey(3));
            Assert.IsTrue(direct.TryGetValue(2, out var value));
            Assert.AreEqual(20, value);
            Assert.IsFalse(direct.TryGetValue(3, out _));
            Assert.AreEqual(10, direct.GetValueByRef(1));
            Assert.IsTrue(direct.TryFindIndex(2, out var index));
            Assert.AreEqual(1, index);
            Assert.IsFalse(direct.TryFindIndex(3, out _));
            Assert.AreEqual(0, direct.FindIndex(1));
            Assert.AreEqual(-1, direct.FindIndex(3));
            Assert.AreEqual(20, fromOwner[2]);
        }

        [Test]
        public void Enumerators_DirectConstructionMovementResetCurrentAndDispose()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            map.Add(7, 70);
            var readOnly = map.AsReadOnly();

            var keys = new ArrayMapUnsafe<int, int>.KeyEnumerable(in readOnly);
            Assert.IsTrue(keys.IsValid);

            var keyEnumerator = new ArrayMapUnsafe<int, int>.KeyEnumerator(in readOnly);
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

            var ownerEnumerator = new ArrayMapUnsafeKeyValueEnumerator<int, int>(in map);
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

            var readOnlyEnumerator = new ArrayMapUnsafeReadOnlyKeyValueEnumerator<int, int>(in readOnly);
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

            Assert.IsFalse(default(ArrayMapUnsafe<int, int>.KeyEnumerable).IsValid);
            Assert.IsFalse(default(ArrayMapUnsafe<int, int>.KeyEnumerator).IsValid);
            Assert.IsFalse(default(ArrayMapUnsafeKeyValueEnumerator<int, int>).IsValid);
            Assert.IsFalse(default(ArrayMapUnsafeReadOnlyKeyValueEnumerator<int, int>).IsValid);
        }

        [Test]
        public void KeyValuePairs_PublicConstructorsPropertiesValidityAndDeconstructReflectBuffer()
        {
            using var buffer = new BufferUnsafe<int>(2, Allocator.Temp);
            buffer[0] = 10;
            buffer[1] = 20;
            var bufferReadOnly = buffer.AsReadOnly();
            var key = 7;
            var pair = new ArrayMapUnsafeKeyValuePair<int, int>(in key, in buffer, 1);
            var readOnlyPair = new ArrayMapUnsafeReadOnlyKeyValuePair<int, int>(in key, in bufferReadOnly, 0);

            AssertPair(pair, 7, 20);
            AssertPair(readOnlyPair, 7, 10);
            Assert.IsFalse(default(ArrayMapUnsafeKeyValuePair<int, int>).IsValid);
            Assert.IsFalse(default(ArrayMapUnsafeReadOnlyKeyValuePair<int, int>).IsValid);
        }

        [Test]
        public void IncreaseCapacityToAndTryFindIndex_GrowAndReportBothPaths()
        {
            using var map = new ArrayMapUnsafe<int, int>(2, Allocator.Temp);
            map.Add(1, 10);
            var requestedCapacity = map.Capacity + 5;

            map.IncreaseCapacityTo(requestedCapacity);

            Assert.GreaterOrEqual(map.Capacity, requestedCapacity);
            Assert.IsTrue(map.TryFindIndex(1, out var index));
            Assert.AreEqual(0, index);
            Assert.IsFalse(map.TryFindIndex(2, out _));
        }

        [Test]
        public void AllocCapacityAndFree_OwnsLiveHeader()
        {
            // SAFETY: The pointer is freed exactly once in finally and is not used after free.
            unsafe
            {
                ArrayMapUnsafe<int, int>* data = null;

                try
                {
                    data = ArrayMapUnsafe<int, int>.Alloc(4, Allocator.Temp);

                    Assert.AreNotEqual(IntPtr.Zero, (IntPtr)data);
                    Assert.IsTrue(data->IsCreated);

                    data->Add(1, 10);

                    Assert.AreEqual(10, (*data)[1]);
                }
                finally
                {
                    if (data != null)
                    {
                        ArrayMapUnsafe<int, int>.Free(data);
                        data = null;
                    }
                }
            }
        }

        [Test]
        public void AllocSourceAndFree_DeepCopiesLiveHeader()
        {
            using var source = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            source.Add(1, 10);

            // SAFETY: The pointer is freed exactly once in finally and is not used after free.
            unsafe
            {
                ArrayMapUnsafe<int, int>* data = null;

                try
                {
                    data = ArrayMapUnsafe<int, int>.Alloc(in source, Allocator.Temp);

                    Assert.AreNotEqual(IntPtr.Zero, (IntPtr)data);
                    Assert.AreEqual(10, (*data)[1]);

                    data->Add(2, 20);

                    Assert.IsFalse(source.ContainsKey(2));
                }
                finally
                {
                    if (data != null)
                    {
                        ArrayMapUnsafe<int, int>.Free(data);
                        data = null;
                    }
                }
            }
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var map = new ArrayMapUnsafe<int, int>(4, Allocator.TempJob);
            map.Add(1, 10);

            var handle = map.Dispose(default);

            Assert.IsFalse(map.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            Assert.IsTrue(map.IsCreated);

            map.Dispose();

            Assert.IsFalse(map.IsCreated);
        }

        [Test]
        public void Collisions_ManyKeys_AllRetrievable()
        {
            const int N = 500;
            using var map = new ArrayMapUnsafe<int, int>(2, Allocator.Temp);

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

        [TestCase(false)]
        [TestCase(true)]
        [Category("Task045X2Regression")]
        public void Reinterpret_DisposeBorrowedView_LeavesEveryOwnerBufferUsable(bool scheduled)
        {
            var owner = new ArrayMapUnsafe<int, int>(2, Allocator.Persistent);

            try
            {
                owner.Add(1, 17);
                owner.Add(2, 29);
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
                Assert.AreEqual(17, owner[1]);
                Assert.AreEqual(29, owner[2]);
                owner.Add(3, 43);
                Assert.AreEqual(43, owner[3]);
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
            var owner = new ArrayMapUnsafe<int, int>(4, Allocator.Persistent);

            try
            {
                owner.Add(1, 17);
                owner.Add(2, 29);
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
                    Assert.AreEqual(17, owner[1]);
                    Assert.AreEqual(29, owner[2]);
                }
                finally
                {
                    view.Dispose();
                }

                owner.Add(3, 43);
                Assert.AreEqual(43, owner[3]);
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
            var owner = new ArrayMapUnsafe<CollisionKey, int>(2, Allocator.Persistent);

            try
            {
                owner.EnsureCapacity(20);

                for (var i = 1; i <= 4; i++)
                {
                    owner.Add(new CollisionKey(i), i * 10);
                }

                var view = owner.Reinterpret<int>();

                try
                {
                    Assert.Throws<InvalidOperationException>(() => view.Add(new CollisionKey(5), 50));
                    Assert.AreEqual(4, view.Count);
                    Assert.AreEqual(4, owner.Count);
                    Assert.IsFalse(view.ContainsKey(new CollisionKey(5)));
                    Assert.IsFalse(owner.ContainsKey(new CollisionKey(5)));
                    CollectionAssert.AreEqual(new[] { 10, 20, 30, 40 }, owner.Values.ToArray());

                    for (var i = 1; i <= 4; i++)
                    {
                        Assert.AreEqual(i * 10, owner[new CollisionKey(i)]);
                    }
                }
                finally
                {
                    view.Dispose();
                }

                owner.Add(new CollisionKey(5), 50);
                Assert.AreEqual(5, owner.Count);
                Assert.AreEqual(50, owner[new CollisionKey(5)]);
            }
            finally
            {
                owner.Dispose();
            }
        }

        private static void AssertPair(ArrayMapUnsafeKeyValuePair<int, int> pair, int key, int value)
        {
            Assert.IsTrue(pair.IsValid);
            Assert.AreEqual(key, pair.Key);
            Assert.AreEqual(value, pair.Value);

            pair.Deconstruct(out var actualKey, out var actualValue);

            Assert.AreEqual(key, actualKey);
            Assert.AreEqual(value, actualValue);
        }

        private static void AssertPair(ArrayMapUnsafeReadOnlyKeyValuePair<int, int> pair, int key, int value)
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
        public void Constructor_AcceptedAllocator_CreatesAndDisposes(Allocator allocator)
        {
            var map = new ArrayMapUnsafe<int, int>(4, allocator);

            try
            {
                map.Add(1, 10);

                Assert.IsTrue(map.IsCreated);
                Assert.AreEqual(1, map.Count);
            }
            finally
            {
                map.Dispose();
            }

            Assert.IsFalse(map.IsCreated);
            Assert.AreEqual(0, map.Count);
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_JobHandle_AcceptedAllocator_ReleasesTheStorage(Allocator allocator)
        {
            var map = new ArrayMapUnsafe<int, int>(4, allocator);
            map.Add(1, 10);

            var handle = map.Dispose(default);

            Assert.IsFalse(map.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
            Assert.DoesNotThrow(() => map.Dispose(default).Complete());
        }

        [Test]
        public void Dispose_DefaultOrAlreadyDisposedMap_IsNoOp()
        {
            var uncreated = default(ArrayMapUnsafe<int, int>);
            var disposed = new ArrayMapUnsafe<int, int>(4, Allocator.Persistent);
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
            Assert.Catch(() => _ = new ArrayMapUnsafe<int, int>(-1, Allocator.Temp));
        }

        [Test]
        public void Constructor_ZeroCapacity_CreatesAUsableMap()
        {
            const int N = 50;
            using var map = new ArrayMapUnsafe<int, int>(0, Allocator.Temp);

            Assert.IsTrue(map.IsCreated);
            Assert.AreEqual(0, map.Count);
            Assert.IsFalse(map.ContainsKey(1));

            for (var i = 0; i < N; i++)
            {
                map.Add(i, i * 2);
            }

            Assert.AreEqual(N, map.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.IsTrue(map.TryGetValue(i, out var value), $"missing {i}");
                Assert.AreEqual(i * 2, value);
            }
        }

        [Test]
        public void CopyConstructor_NotCreatedOrDisposedSource_ThrowsArgumentException()
        {
            var uncreated = default(ArrayMapUnsafe<int, int>);
            var disposed = new ArrayMapUnsafe<int, int>(4, Allocator.Persistent);
            disposed.Dispose();

            Assert.Throws<ArgumentException>(() => _ = new ArrayMapUnsafe<int, int>(uncreated, Allocator.Temp));
            Assert.Throws<ArgumentException>(() => _ = new ArrayMapUnsafe<int, int>(disposed, Allocator.Temp));
        }

        [Test]
        public void CopyConstructor_EmptySource_CreatesAnIndependentUsableMap()
        {
            using var source = new ArrayMapUnsafe<int, int>(0, Allocator.Temp);
            using var copy = new ArrayMapUnsafe<int, int>(source, Allocator.Temp);

            Assert.IsTrue(copy.IsCreated);
            Assert.AreEqual(0, copy.Count);

            copy.Add(1, 10);

            Assert.AreEqual(10, copy[1]);
            Assert.AreEqual(0, source.Count);
        }

        [Test]
        public void Values_EmptyOrUncreatedMap_IsEmpty()
        {
            using var empty = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            var uncreated = default(ArrayMapUnsafe<int, int>);

            Assert.AreEqual(0, empty.Values.Length);
            Assert.AreEqual(0, uncreated.Values.Length);
        }

        [Test]
        public void Values_ByteElement_RoundTripBitExact()
        {
            AssertValueRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Values_ThreeByteStruct_RoundTripBitExact()
        {
            AssertValueRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Values_LongElement_RoundTripBitExact()
        {
            AssertValueRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Values_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertValueRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = 0, b = 0, c = 0 }
                , new Wide { a = long.MinValue, b = 7, c = -1 }
            );
        }

        [Test]
        public void Keys_ByteElement_RoundTripBitExact()
        {
            AssertKeyRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Keys_ThreeByteStruct_RoundTripBitExact()
        {
            AssertKeyRoundTrip(
                  new RgbKey { r = 1, g = 2, b = 3 }
                , new RgbKey { r = 0, g = 0, b = 0 }
                , new RgbKey { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Keys_LongElement_RoundTripBitExact()
        {
            AssertKeyRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Keys_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertKeyRoundTrip(
                  new WideKey { a = 1, b = -2, c = long.MaxValue }
                , new WideKey { a = 0, b = 0, c = 0 }
                , new WideKey { a = long.MinValue, b = 7, c = -1 }
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Lookups_UncreatedMap_Throw()
        {
            var map = default(ArrayMapUnsafe<int, int>);

            Assert.Throws<InvalidOperationException>(() => map.ContainsKey(1));
            Assert.Throws<InvalidOperationException>(() => map.TryGetValue(1, out _));
            Assert.Throws<InvalidOperationException>(() => map.TryFindIndex(1, out _));
            Assert.Throws<InvalidOperationException>(() => map.FindIndex(1));
        }

        [Test]
        public void Keys_UncreatedMap_ThrowsArgumentException()
        {
            var map = default(ArrayMapUnsafe<int, int>);

            Assert.Throws<ArgumentException>(() => _ = map.Keys);
            Assert.Throws<ArgumentException>(() => map.AsReadOnly());
        }

        [Test]
        public void FindIndexAndLookups_EmptyMap_ReportMissing()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            Assert.AreEqual(-1, map.FindIndex(1));
            Assert.IsFalse(map.TryFindIndex(1, out var index));
            Assert.AreEqual(0, index);
            Assert.IsFalse(map.ContainsKey(1));
            Assert.IsFalse(map.TryGetValue(1, out var value));
            Assert.AreEqual(0, value);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void GetValueByRef_MissingKey_Throws()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.Throws<KeyNotFoundException>(() => map.GetValueByRef(2));
            Assert.AreEqual(10, map[1]);
        }

        [Test]
        public void Add_ExactlyFullThenOnePast_KeepsEveryEntry()
        {
            using var map = new ArrayMapUnsafe<int, int>(3, Allocator.Temp);

            for (var i = 0; i < map.Capacity; i++)
            {
                map.Add(i, i * 5);
            }

            Assert.AreEqual(map.Count, map.Capacity);

            var past = map.Capacity;

            map.Add(past, past * 5);

            for (var i = 0; i <= past; i++)
            {
                Assert.AreEqual(i * 5, map[i], $"key {i}");
            }
        }

        [Test]
        public void TryAdd_ExactlyFullThenOnePast_ReportsSequentialIndices()
        {
            using var map = new ArrayMapUnsafe<int, int>(2, Allocator.Temp);

            for (var i = 0; i < 12; i++)
            {
                Assert.IsTrue(map.TryAdd(i, i + 100, out var index), $"key {i}");
                Assert.AreEqual(i, index);
            }

            Assert.IsFalse(map.TryAdd(3, 0, out var existing));
            Assert.AreEqual(3, existing);
            Assert.AreEqual(103, map[3]);
        }

        [Test]
        public void Recycle_EmptyMapOrFilledMap_KeepsTheMapUsable()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            map.Recycle();

            Assert.AreEqual(0, map.Count);

            map.Add(1, 10);
            map.Add(2, 20);
            map.Recycle();

            Assert.AreEqual(0, map.Count);
            Assert.IsFalse(map.ContainsKey(1));
            Assert.IsTrue(map.TryAdd(2, 99, out var index));
            Assert.AreEqual(0, index);
            Assert.AreEqual(99, map[2]);
        }

        [Test]
        public void Clear_EmptyMap_KeepsTheMapUsable()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            map.Clear();
            map.Clear();

            Assert.AreEqual(0, map.Count);

            map.Add(1, 10);

            Assert.AreEqual(10, map[1]);
        }

        [Test]
        public void GetOrAdd_GrowthWhileAdding_ReturnsReferencesIntoTheGrownStorage()
        {
            using var map = new ArrayMapUnsafe<int, int>(2, Allocator.Temp);

            for (var i = 0; i < 20; i++)
            {
                map.GetOrAdd(i, out var index) = i * 3;

                Assert.AreEqual(i, index);
            }

            ref var first = ref map.GetOrAdd(0);
            first = -1;

            for (var i = 1; i < 20; i++)
            {
                Assert.AreEqual(i * 3, map[i], $"key {i}");
            }

            Assert.AreEqual(-1, map[0]);
            Assert.AreEqual(20, map.Count);
        }

        [Test]
        public void GetOrAdd_ZeroCapacityMap_AddsTheKeyWithADefaultValue()
        {
            using var map = new ArrayMapUnsafe<int, int>(0, Allocator.Temp);

            ref var value = ref map.GetOrAdd(7);

            Assert.AreEqual(0, value);

            value = 70;

            Assert.AreEqual(70, map[7]);
            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void GetOrAdd_ReferenceAliasesTheValuesSpan()
        {
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);

            map.GetOrAdd(1, out var index) = 5;
            var values = map.Values;

            Assert.AreEqual(5, values[index]);

            map.GetOrAdd(1) = 6;

            Assert.AreEqual(6, values[index]);
        }

        [Test]
        public void AddValue_NewAndExistingKeys_ReportIndexAndGrowPastCapacity()
        {
            using var map = new ArrayMapUnsafe<int, int>(2, Allocator.Temp);

            Assert.IsTrue(map.AddValue(5, out var firstIndex));
            Assert.AreEqual(0, firstIndex);
            Assert.IsFalse(map.AddValue(5, out var existingIndex));
            Assert.AreEqual(0, existingIndex);
            Assert.IsTrue(map.AddValue(6, out var otherIndex));
            Assert.AreEqual(1, otherIndex);

            for (var i = 10; i < 30; i++)
            {
                Assert.IsTrue(map.AddValue(i, out var index));
                Assert.AreEqual(i - 8, index);
            }

            Assert.AreEqual(22, map.Count);
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
                    , ArrayMapUnsafe<int, int>.Reduce(hashcode, bucketCount, multiplier)
                    , $"{hashcode} % {bucketCount}"
                );
            }
        }

        [Test]
        public void Remove_EmptyMap_ReturnsFalseWithDefaultOutputs()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            Assert.IsFalse(map.Remove(1));
            Assert.IsFalse(map.Remove(1, out var index, out var value));
            Assert.AreEqual(0, index);
            Assert.AreEqual(0, value);
            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void Remove_EveryKey_MatchesDictionary()
        {
            const int COUNT = 6;

            for (var removed = 1; removed <= COUNT; removed++)
            {
                using var map = new ArrayMapUnsafe<int, int>(COUNT, Allocator.Temp);
                var expected = new Dictionary<int, int>();

                for (var i = 1; i <= COUNT; i++)
                {
                    map.Add(i, i * 10);
                    expected.Add(i, i * 10);
                }

                Assert.IsTrue(map.Remove(removed, out var index, out var value), $"removed {removed}");
                Assert.AreEqual(removed - 1, index, $"removed {removed}");
                Assert.AreEqual(removed * 10, value, $"removed {removed}");

                expected.Remove(removed);

                Assert.AreEqual(expected.Count, map.Count, $"removed {removed}");
                CollectionAssert.AreEquivalent(expected.Values, map.Values.ToArray(), $"removed {removed}");

                foreach (var pair in expected)
                {
                    Assert.IsTrue(map.TryGetValue(pair.Key, out var actual), $"removed {removed}, key {pair.Key}");
                    Assert.AreEqual(pair.Value, actual, $"removed {removed}, key {pair.Key}");
                }

                Assert.IsFalse(map.ContainsKey(removed), $"removed {removed}");
            }
        }

        [Test]
        public void Remove_CollidingKeysInEveryOrder_KeepsTheRemainingKeysReachable()
        {
            const int COUNT = 5;

            for (var removed = 0; removed < COUNT; removed++)
            {
                using var map = new ArrayMapUnsafe<CollidingKey, int>(2, Allocator.Temp);

                for (var i = 0; i < COUNT; i++)
                {
                    map.Add(new CollidingKey(i), i * 7);
                }

                Assert.IsTrue(map.Remove(new CollidingKey(removed)));
                Assert.IsFalse(map.Remove(new CollidingKey(removed)));
                Assert.AreEqual(COUNT - 1, map.Count);

                for (var i = 0; i < COUNT; i++)
                {
                    var found = map.TryGetValue(new CollidingKey(i), out var value);

                    Assert.AreEqual(i != removed, found, $"removed {removed}, key {i}");

                    if (found)
                    {
                        Assert.AreEqual(i * 7, value, $"removed {removed}, key {i}");
                    }
                }
            }
        }

        [Test]
        public void EnsureCapacityAndIncreaseCapacity_NotLargerThanCapacity_KeepCapacityAndContent()
        {
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            var capacity = map.Capacity;

            Assert.AreEqual(capacity, map.EnsureCapacity(0));
            Assert.AreEqual(capacity, map.EnsureCapacity(capacity));
            Assert.AreEqual(capacity, map.IncreaseCapacityTo(1));
            Assert.AreEqual(capacity, map.IncreaseCapacityBy(0));
            Assert.AreEqual(capacity, map.Capacity);
            Assert.AreEqual(10, map[1]);
        }

        [Test]
        public void Trim_EmptyMap_ReleasesTheCapacityAndStaysUsable()
        {
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);

            map.Trim();

            Assert.AreEqual(0, map.Capacity);
            Assert.IsTrue(map.IsCreated);

            map.Add(3, 30);

            Assert.AreEqual(30, map[3]);
        }

        [Test]
        public void SetOperations_WithEmptyOrSameMap_MatchDictionary()
        {
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);
            using var empty = new ArrayMapUnsafe<int, int>(0, Allocator.Temp);
            using var otherValues = new ArrayMapUnsafe<int, long>(0, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);

            map.Union(in empty);
            map.Exclude(in empty);
            map.Exclude(in otherValues);

            CollectionAssert.AreEquivalent(new[] { 10, 20, 30 }, map.Values.ToArray());

            map.Union(in map);
            map.Intersect(in map);

            CollectionAssert.AreEquivalent(new[] { 10, 20, 30 }, map.Values.ToArray());

            map.Intersect(in otherValues);

            Assert.AreEqual(0, map.Count);

            empty.Union(in map);

            Assert.AreEqual(0, empty.Count);

            map.Add(4, 40);
            map.Add(5, 50);
            map.Add(6, 60);
            map.Exclude(in map);

            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void Intersect_AndExclude_WithAMapOfAnotherValueType_UseKeysOnly()
        {
            using var map = new ArrayMapUnsafe<int, int>(8, Allocator.Temp);
            using var other = new ArrayMapUnsafe<int, long>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);
            other.Add(2, 200L);
            other.Add(3, 300L);
            other.Add(4, 400L);

            map.Exclude(in other);

            CollectionAssert.AreEquivalent(new[] { 10 }, map.Values.ToArray());

            map.Add(2, 21);
            map.Add(5, 50);
            map.Intersect(in other);

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(21, map[2]);
        }

        [Test]
        public void Reinterpret_UncreatedMap_ReturnsAnUncreatedView()
        {
            var map = default(ArrayMapUnsafe<int, int>);

            var view = map.Reinterpret<uint>();

            Assert.IsFalse(view.IsCreated);
            Assert.AreEqual(0, view.Count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_DifferentSize_Throws()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => map.Reinterpret<ushort>());
            Assert.Throws<InvalidOperationException>(() => map.Reinterpret<long>());
        }

        [Test]
        public void Reinterpret_WriteThroughTheView_IsSeenByTheOwnerAndTheReverse()
        {
            var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            try
            {
                map.Add(1, 0);
                map.Add(2, 0);

                // The view is never disposed: it shares the owner's buffers, so only the owner releases them.
                var view = map.Reinterpret<float>();

                view[1] = 1.5f;
                map[2] = BitConverter.SingleToInt32Bits(-2.25f);

                Assert.AreEqual(BitConverter.SingleToInt32Bits(1.5f), map[1]);
                Assert.AreEqual(-2.25f, view[2]);
                Assert.AreEqual(2, view.Count);
                Assert.IsTrue(view.ContainsKey(1));
                Assert.AreEqual(1.5f, view.Values[0]);
            }
            finally
            {
                map.Dispose();
            }
        }

        [Test]
        public void Reinterpret_StructLargerThanSixteenBytes_AliasesEveryField()
        {
            using var map = new ArrayMapUnsafe<int, Wide>(4, Allocator.Temp);
            map.Add(1, new Wide { a = 1, b = -2, c = long.MaxValue });

            var view = map.Reinterpret<WideOther>();

            Assert.AreEqual(1L, view[1].x);
            Assert.AreEqual(-2L, view[1].y);
            Assert.AreEqual(long.MaxValue, view[1].z);

            view[1] = new WideOther { x = long.MinValue, y = 0, z = -1 };

            Assert.AreEqual(long.MinValue, map[1].a);
            Assert.AreEqual(0L, map[1].b);
            Assert.AreEqual(-1L, map[1].c);
        }

        [Test]
        public void KeyEnumerable_EmptyMap_YieldsNothingForEveryEnumeratorForm()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            var readOnly = map.AsReadOnly();
            var keys = new ArrayMapUnsafe<int, int>.KeyEnumerable(in readOnly);

            var structEnumerator = keys.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)keys).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)keys).GetEnumerator();

            Assert.IsTrue(keys.IsValid);
            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
            Assert.DoesNotThrow(() => structEnumerator.Dispose());
            Assert.DoesNotThrow(() => structEnumerator.Dispose());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void KeyEnumerator_UncreatedReadOnly_MoveNextThrows()
        {
            var readOnly = default(ArrayMapUnsafe<int, int>.ReadOnly);

            var enumerator = new ArrayMapUnsafe<int, int>.KeyEnumerator(in readOnly);

            Assert.IsFalse(enumerator.IsValid);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public void KeyValueEnumerator_EmptyMap_YieldsNothingAndDisposesRepeatedly()
        {
            using var map = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);

            var enumerator = new ArrayMapUnsafeKeyValueEnumerator<int, int>(in map);

            Assert.IsTrue(enumerator.IsValid);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.DoesNotThrow(() => enumerator.Dispose());
            Assert.DoesNotThrow(() => enumerator.Dispose());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void KeyValueEnumerator_UncreatedMap_MoveNextThrows()
        {
            var map = default(ArrayMapUnsafe<int, int>);

            var enumerator = new ArrayMapUnsafeKeyValueEnumerator<int, int>(in map);

            Assert.IsFalse(enumerator.IsValid);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public void KeyValuePair_DefaultAndSingleElementBuffer_ReportKeyAndValidity()
        {
            var uncreated = default(ArrayMapUnsafeKeyValuePair<int, int>);
            var buffer = new BufferUnsafe<int>(1, Allocator.Temp);

            try
            {
                buffer[0] = 42;

                var key = 9;
                var pair = new ArrayMapUnsafeKeyValuePair<int, int>(in key, in buffer, 0);

                Assert.IsFalse(uncreated.IsValid);
                Assert.AreEqual(0, uncreated.Key);
                Assert.IsTrue(pair.IsValid);
                Assert.AreEqual(9, pair.Key);
                Assert.AreEqual(42, pair.Value);

                pair.Deconstruct(out var actualKey, out var actualValue);

                Assert.AreEqual(9, actualKey);
                Assert.AreEqual(42, actualValue);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void KeyValuePairDebugProxy_ReportsKeyAndValue()
        {
            var buffer = new BufferUnsafe<int>(1, Allocator.Temp);

            try
            {
                buffer[0] = 42;

                var key = 9;
                var pair = new ArrayMapUnsafeKeyValuePair<int, int>(in key, in buffer, 0);
                var proxy = new ArrayMapUnsafeKeyValuePairDebugProxy<int, int>(in pair);
                var uncreated = default(ArrayMapUnsafeKeyValuePair<int, int>);
                var uncreatedProxy = new ArrayMapUnsafeKeyValuePairDebugProxy<int, int>(in uncreated);

                Assert.AreEqual(9, proxy.Key);
                Assert.AreEqual(42, proxy.Value);
                Assert.AreEqual(0, uncreatedProxy.Key);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void DebugProxy_EmptyAndFilledMap_ReportCountAndItems()
        {
            using var empty = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            using var filled = new ArrayMapUnsafe<int, int>(4, Allocator.Temp);
            filled.Add(7, 70);
            filled.Add(8, 80);

            var emptyProxy = new ArrayMapUnsafeDebugProxy<int, int>(in empty);
            var filledProxy = new ArrayMapUnsafeDebugProxy<int, int>(in filled);
            var items = filledProxy.Items;

            Assert.AreEqual(0u, emptyProxy.Count);
            Assert.AreEqual(0, emptyProxy.Items.Length);
            Assert.AreEqual(2u, filledProxy.Count);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual(7, items[0].Key);
            Assert.AreEqual(70, items[0].Value);
            Assert.AreEqual(8, items[1].Key);
            Assert.AreEqual(80, items[1].Value);
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Alloc_AcceptedAllocator_CreatesAndFreesTheHeader(Allocator allocator)
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = ArrayMapUnsafe<int, int>.Alloc(4, allocator);

                try
                {
                    data->Add(1, 10);

                    Assert.IsTrue(data->IsCreated);
                    Assert.AreEqual(1, data->Count);
                    Assert.AreEqual(10, (*data)[1]);
                }
                finally
                {
                    ArrayMapUnsafe<int, int>.Free(data);
                }
            }
        }

        [Test]
        public void Alloc_ZeroCapacityAndEmptySource_CreateUsableHeaders()
        {
            using var source = new ArrayMapUnsafe<int, int>(0, Allocator.Temp);

            // SAFETY: Both headers returned by Alloc stay live until Free, which runs exactly once in finally.
            unsafe
            {
                var zero = ArrayMapUnsafe<int, int>.Alloc(0, Allocator.Persistent);
                var copy = ArrayMapUnsafe<int, int>.Alloc(in source, Allocator.Persistent);

                try
                {
                    zero->Add(1, 10);
                    copy->Add(2, 20);

                    Assert.AreEqual(10, (*zero)[1]);
                    Assert.AreEqual(20, (*copy)[2]);
                    Assert.AreEqual(0, source.Count);
                }
                finally
                {
                    ArrayMapUnsafe<int, int>.Free(zero);
                    ArrayMapUnsafe<int, int>.Free(copy);
                }
            }
        }

        [Test]
        public void Alloc_InvalidArgument_Throws()
        {
            var uncreated = default(ArrayMapUnsafe<int, int>);

            // The header is allocated before the constructor validates the source (suspected defect D-17), so this
            // call leaks one header. Allocator.Temp is used so that Unity releases it without a leak report.
            // SAFETY: The call throws before it returns a header, so no pointer is used.
            unsafe
            {
                Assert.Throws<ArgumentException>(() => ArrayMapUnsafe<int, int>.Alloc(in uncreated, Allocator.Temp));
            }
        }

        [Test]
        public void Free_NullPointer_DoesNothing()
        {
            // SAFETY: Free documents that a null header is ignored, so no memory is read or released.
            unsafe
            {
                Assert.DoesNotThrow(() => ArrayMapUnsafe<int, int>.Free(null));
            }
        }

        private static void AssertValueRoundTrip<TValue>(TValue a, TValue b, TValue c)
            where TValue : unmanaged
        {
            var name = typeof(TValue).Name;
            var map = new ArrayMapUnsafe<int, TValue>(1, Allocator.Temp);

            try
            {
                map.Add(1, a);
                map[2] = b;
                map.GetOrAdd(3) = c;

                Assert.AreEqual(3, map.Count, name);
                Assert.AreEqual(a, map[1], name);
                Assert.AreEqual(b, map[2], name);
                Assert.AreEqual(c, map[3], name);
                CollectionAssert.AreEqual(new[] { a, b, c }, map.Values.ToArray(), name);
                Assert.IsTrue(map.TryGetValue(2, out var found), name);
                Assert.AreEqual(b, found, name);
                Assert.IsTrue(map.Remove(2, out _, out var removed), name);
                Assert.AreEqual(b, removed, name);
                Assert.AreEqual(a, map[1], name);
                Assert.AreEqual(c, map[3], name);
                Assert.AreEqual(default(TValue), map.GetOrAdd(9), name);
            }
            finally
            {
                map.Dispose();
            }
        }

        private static void AssertKeyRoundTrip<TKey>(TKey k1, TKey k2, TKey k3)
            where TKey : unmanaged, IEquatable<TKey>
        {
            var name = typeof(TKey).Name;
            var map = new ArrayMapUnsafe<TKey, int>(1, Allocator.Temp);

            try
            {
                map.Add(k1, 1);
                map.Add(k2, 2);
                map[k3] = 3;

                Assert.AreEqual(3, map.Count, name);
                Assert.IsTrue(map.ContainsKey(k1), name);
                Assert.IsTrue(map.TryGetValue(k2, out var value), name);
                Assert.AreEqual(2, value, name);
                Assert.AreEqual(3, map[k3], name);
                Assert.IsTrue(map.Remove(k2), name);
                Assert.IsFalse(map.ContainsKey(k2), name);

                var keys = new List<TKey>();

                foreach (var key in map.Keys)
                {
                    keys.Add(key);
                }

                CollectionAssert.AreEquivalent(new[] { k1, k3 }, keys, name);
            }
            finally
            {
                map.Dispose();
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

        private struct WideOther
        {
            public long x;
            public long y;
            public long z;
        }

        private struct RgbKey : IEquatable<RgbKey>
        {
            public byte r;
            public byte g;
            public byte b;

            public bool Equals(RgbKey other)
                => r == other.r && g == other.g && b == other.b;

            public override bool Equals(object obj)
                => obj is RgbKey other && Equals(other);

            public override int GetHashCode()
                => r | (g << 8) | (b << 16);
        }

        private struct WideKey : IEquatable<WideKey>
        {
            public long a;
            public long b;
            public long c;

            public bool Equals(WideKey other)
                => a == other.a && b == other.b && c == other.c;

            public override bool Equals(object obj)
                => obj is WideKey other && Equals(other);

            public override int GetHashCode()
                => HashCode.Combine(a, b, c);
        }

        private readonly struct CollisionKey : IEquatable<CollisionKey>
        {
            private readonly int _value;

            public CollisionKey(int value)
            {
                _value = value;
            }

            public bool Equals(CollisionKey other)
                => _value == other._value;

            public override bool Equals(object obj)
                => obj is CollisionKey other && Equals(other);

            public override int GetHashCode()
                => 1;
        }
    }
}
