using System;
using System.Collections.Generic;
using EncosyTower.Buffers;
using EncosyTower.Collections;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public partial class SharedArrayMapUnsafeTests
    {
        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Alloc_AcceptedAllocator_CreatesAndFreesTheHeader(Allocator allocator)
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);

            // SAFETY: The header borrows storage, which outlives it; Free runs exactly once in finally.
            unsafe
            {
                var data = SharedArrayMapUnsafe<int, int>.Alloc(
                      storage.valuesInfo
                    , storage.capacity
                    , storage.values
                    , storage.capacity
                    , storage.buckets
                    , storage.bucketCapacity
                    , storage.fastModBucketsMultiplier
                    , storage.collisions
                    , storage.freeValueCellIndex
                    , storage.version
                    , allocator
                );

                try
                {
                    data->Add(1, 10);

                    Assert.AreEqual(4, data->Capacity);
                    Assert.AreEqual(3, data->BucketCapacity);
                    Assert.AreEqual(1, data->Count);
                    Assert.AreEqual(1, storage.Count);
                    Assert.AreEqual(10, (*data)[1]);
                }
                finally
                {
                    SharedArrayMapUnsafe<int, int>.Free(data, allocator);
                }
            }
        }

        [Test]
        public void Alloc_ZeroCapacity_CreatesAHeaderOfAnEmptyFixedMap()
        {
            using var storage = new SharedArrayMapStorage<int, int>(0, 3);

            // SAFETY: The header borrows storage, which outlives it; Free runs exactly once in finally.
            unsafe
            {
                var data = SharedArrayMapUnsafe<int, int>.Alloc(
                      storage.valuesInfo
                    , storage.capacity
                    , storage.values
                    , storage.capacity
                    , storage.buckets
                    , storage.bucketCapacity
                    , storage.fastModBucketsMultiplier
                    , storage.collisions
                    , storage.freeValueCellIndex
                    , storage.version
                    , Allocator.Persistent
                );

                try
                {
                    Assert.AreEqual(0, data->Capacity);
                    Assert.AreEqual(0, data->Count);
                    Assert.IsFalse(data->ContainsKey(1));
                    Assert.IsFalse(data->TryGetValue(1, out var value));
                    Assert.AreEqual(0, value);
                }
                finally
                {
                    SharedArrayMapUnsafe<int, int>.Free(data, Allocator.Persistent);
                }
            }
        }

        [Test]
        public void Free_NullPointer_DoesNothingForEveryStrategy()
        {
            // SAFETY: Free documents that a null header is ignored, so no memory is read or released.
            unsafe
            {
                SharedArrayMapUnsafe<int, int>.Free(null, default(AllocatorStrategy));
                SharedArrayMapUnsafe<int, int>.Free(null, Allocator.Temp);
                SharedArrayMapUnsafe<int, int>.Free(null, Allocator.Persistent);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ZeroCapacityMap_AddOfAnyKey_Throws()
        {
            using var storage = new SharedArrayMapStorage<int, int>(0, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => header.Add(1, 10));
                Assert.Throws<InvalidOperationException>(() => header.TryAdd(1, 10));
                Assert.Throws<InvalidOperationException>(() => header[1] = 10);
                Assert.Throws<InvalidOperationException>(() => header.GetOrAdd(1));
                Assert.AreEqual(0, storage.Count);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ZeroBucketMap_Lookups_Throw()
        {
            using var storage = new SharedArrayMapStorage<int, int>(2, 0);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => header.ContainsKey(1));
                Assert.Throws<InvalidOperationException>(() => header.TryGetValue(1, out _));
                Assert.Throws<InvalidOperationException>(() => header.TryFindIndex(1, out _));
                Assert.Throws<InvalidOperationException>(() => header.GetIndex(1));
            }
        }

        [Test]
        public void EmptyMap_CountVersionAndLookups_ReportNothing()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                Assert.AreEqual(4, header.Capacity);
                Assert.AreEqual(3, header.BucketCapacity);
                Assert.AreEqual(0, header.Count);
                Assert.AreEqual(0, header.Version);
                Assert.IsFalse(header.ContainsKey(1));
                Assert.IsFalse(header.TryGetValue(1, out var value));
                Assert.AreEqual(0, value);
                Assert.IsFalse(header.TryFindIndex(1, out var index));
                Assert.AreEqual(0, index);
                Assert.IsFalse(header.Remove(1));
                Assert.IsFalse(header.Remove(1, out var removedIndex, out var removedValue));
                Assert.AreEqual(0, removedIndex);
                Assert.AreEqual(0, removedValue);
            }
        }

        [Test]
        public void Clear_EmptyAndFilledMap_ResetsTheCountAndStaysUsable()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Clear();

                Assert.AreEqual(0, header.Version);

                header.Add(1, 10);
                header.Add(2, 20);

                var version = header.Version;

                header.Clear();

                Assert.AreEqual(0, header.Count);
                Assert.AreEqual(version + 1, header.Version);
                Assert.IsFalse(header.ContainsKey(1));
                Assert.IsFalse(header.ContainsKey(2));

                header.Add(2, 99);

                Assert.AreEqual(99, header[2]);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Add_ExactlyFullThenOnePast_ThrowsAndKeepsContent()
        {
            using var storage = new SharedArrayMapStorage<int, int>(3, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1, 10);
                header.Add(2, 20);

                var item = 30;

                Assert.IsTrue(header.TryAdd(3, in item, out var index));
                Assert.AreEqual(2, index);
                Assert.AreEqual(3, header.Count);
                Assert.Throws<InvalidOperationException>(() => header.Add(4, 40));
                Assert.Throws<InvalidOperationException>(() => header.TryAdd(4, 40));
                Assert.Throws<InvalidOperationException>(() => header[4] = 40);
                Assert.Throws<InvalidOperationException>(() => header.GetOrAdd(4));
                Assert.AreEqual(3, storage.Count);
                Assert.AreEqual(10, header[1]);
                Assert.AreEqual(20, header[2]);
                Assert.AreEqual(30, header[3]);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Add_DuplicateKey_ThrowsAndKeepsTheValue()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1, 10);

                Assert.Throws<InvalidOperationException>(() => header.Add(1, 20));
                Assert.IsFalse(header.TryAdd(1, 30, out var index));
                Assert.AreEqual(0, index);
                Assert.AreEqual(10, header[1]);
                Assert.AreEqual(1, header.Count);
            }
        }

        [Test]
        public void Indexer_ExistingKeyOnAFullMap_Overwrites()
        {
            using var storage = new SharedArrayMapStorage<int, int>(2, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header[1] = 10;
                header[2] = 20;
                header[1] = 11;

                Assert.AreEqual(2, header.Count);
                Assert.AreEqual(11, header[1]);
                Assert.AreEqual(20, header[2]);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexerAndGetIndex_MissingKey_ThrowKeyNotFound()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                Assert.Throws<KeyNotFoundException>(() => _ = header[1]);
                Assert.Throws<KeyNotFoundException>(() => header.GetIndex(1));
                Assert.Throws<KeyNotFoundException>(() => header.GetValueByRef(1));

                header.Add(1, 10);

                Assert.Throws<KeyNotFoundException>(() => _ = header[2]);
                Assert.Throws<KeyNotFoundException>(() => header.GetIndex(2));
                Assert.AreEqual(0, header.GetIndex(1));
            }
        }

        [Test]
        public void KeyAtAndValueAt_FilledMap_ReturnTheInsertionOrderedEntries()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(7, 70);
                header.Add(8, 80);

                Assert.AreEqual(7, header.KeyAt(0));
                Assert.AreEqual(8, header.KeyAt(1));
                Assert.AreEqual(70, header.ValueAt(0));
                Assert.AreEqual(80, header.ValueAt(1));
            }
        }

        [Test]
        public void KeyAtAndValueAt_IndexOutsideTheCapacity_ThrowIndexOutOfRange()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(7, 70);

                Assert.Throws<IndexOutOfRangeException>(() => header.KeyAt(-1));
                Assert.Throws<IndexOutOfRangeException>(() => header.KeyAt(4));
                Assert.Throws<IndexOutOfRangeException>(() => header.KeyAt(int.MaxValue));
                Assert.Throws<IndexOutOfRangeException>(() => _ = header.ValueAt(-1));
                Assert.Throws<IndexOutOfRangeException>(() => _ = header.ValueAt(4));
                Assert.Throws<IndexOutOfRangeException>(() => _ = header.ValueAt(int.MaxValue));
            }
        }

        [Test]
        public void ValueAt_ReferenceAliasesTheSharedValuesBuffer()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(7, 70);

                ref readonly var value = ref header.ValueAt(0);

                storage.values[0] = 71;

                Assert.AreEqual(71, value);

                header[7] = 72;

                Assert.AreEqual(72, value);
            }
        }

        [Test]
        public void GetOrAdd_ExistingAndMissingKeys_ReturnReferencesIntoTheSharedValuesBuffer()
        {
            using var storage = new SharedArrayMapStorage<int, int>(3, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                ref var added = ref header.GetOrAdd(1);

                Assert.AreEqual(0, added);

                added = 10;

                var versionBeforeLookup = header.Version;

                ref var existing = ref header.GetOrAdd(1, out var existingIndex);

                Assert.AreEqual(0, existingIndex);
                Assert.AreEqual(10, existing);
                Assert.AreEqual(versionBeforeLookup + 1, header.Version);

                existing = 11;
                header.GetOrAdd(2, out var newIndex) = 20;

                Assert.AreEqual(1, newIndex);
                Assert.AreEqual(11, storage.values[0]);
                Assert.AreEqual(20, storage.values[1]);
                Assert.AreEqual(2, header.Count);
            }
        }

        [Test]
        public void GetValueByRef_ExistingKey_ReturnsAReferenceIntoTheSharedValuesBuffer()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1, 10);

                var version = header.Version;

                ref var value = ref header.GetValueByRef(1);
                value = 99;

                Assert.AreEqual(99, storage.values[0]);
                Assert.AreEqual(version + 1, header.Version);
            }
        }

        [Test]
        public void Remove_EveryKey_MatchesDictionary()
        {
            const int COUNT = 6;

            for (var removed = 1; removed <= COUNT; removed++)
            {
                using var storage = new SharedArrayMapStorage<int, int>(COUNT, 5);
                var header = storage.CreateHeader();
                var expected = new Dictionary<int, int>();

                // SAFETY: storage owns the blocks the header borrows and outlives every call below.
                unsafe
                {
                    for (var i = 1; i <= COUNT; i++)
                    {
                        header.Add(i, i * 10);
                        expected.Add(i, i * 10);
                    }

                    Assert.IsTrue(header.Remove(removed, out var index, out var value), $"removed {removed}");
                    Assert.AreEqual(removed - 1, index, $"removed {removed}");
                    Assert.AreEqual(removed * 10, value, $"removed {removed}");

                    expected.Remove(removed);

                    Assert.AreEqual(expected.Count, header.Count, $"removed {removed}");
                    Assert.IsFalse(header.ContainsKey(removed), $"removed {removed}");

                    foreach (var pair in expected)
                    {
                        Assert.IsTrue(header.TryGetValue(pair.Key, out var actual), $"removed {removed}, {pair.Key}");
                        Assert.AreEqual(pair.Value, actual, $"removed {removed}, key {pair.Key}");
                    }
                }
            }
        }

        [Test]
        public void Remove_CollidingKeysInEveryOrder_KeepsTheRemainingKeysReachable()
        {
            const int COUNT = 5;

            for (var removed = 0; removed < COUNT; removed++)
            {
                using var storage = new SharedArrayMapStorage<CollidingKey, int>(COUNT, 3);
                var header = storage.CreateHeader();

                // SAFETY: storage owns the blocks the header borrows and outlives every call below.
                unsafe
                {
                    for (var i = 0; i < COUNT; i++)
                    {
                        header.Add(new CollidingKey(i), i * 7);
                    }

                    Assert.IsTrue(header.Remove(new CollidingKey(removed)));
                    Assert.IsFalse(header.Remove(new CollidingKey(removed)));
                    Assert.AreEqual(COUNT - 1, header.Count);

                    for (var i = 0; i < COUNT; i++)
                    {
                        var found = header.TryGetValue(new CollidingKey(i), out var value);

                        Assert.AreEqual(i != removed, found, $"removed {removed}, key {i}");

                        if (found)
                        {
                            Assert.AreEqual(i * 7, value, $"removed {removed}, key {i}");
                        }
                    }
                }
            }
        }

        [Test]
        public void Add_MoreKeysThanBuckets_RebuildsTheBucketsAndKeepsEveryKeyReachable()
        {
            const int COUNT = 12;
            using var storage = new SharedArrayMapStorage<int, int>(COUNT, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                for (var i = 0; i < COUNT; i++)
                {
                    header.Add(i, i * 3);
                }

                Assert.AreEqual(COUNT, header.Count);

                for (var i = 0; i < COUNT; i++)
                {
                    Assert.IsTrue(header.TryFindIndex(i, out var index), $"missing {i}");
                    Assert.AreEqual(i, index);
                    Assert.AreEqual(i * 3, header[i], $"key {i}");
                }
            }
        }

        [Test]
        public void CopyTo_FilledMap_CopiesEveryBufferAndReportsTheState()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1, 10);
                header.Add(2, 20);

                var valuesInfo = new ArrayMapNode<int>[4];
                var values = new int[4];
                var buckets = new int[3];

                header.CopyTo(valuesInfo, values, buckets, out var count, out var collisions, out var multiplier);

                Assert.AreEqual(2, count);
                Assert.AreEqual(*storage.collisions, collisions);
                Assert.AreEqual(*storage.fastModBucketsMultiplier, multiplier);
                Assert.AreEqual(1, valuesInfo[0].key);
                Assert.AreEqual(2, valuesInfo[1].key);
                CollectionAssert.AreEqual(new[] { 10, 20 }, new[] { values[0], values[1] });

                for (var i = 0; i < buckets.Length; i++)
                {
                    Assert.AreEqual(storage.buckets[i], buckets[i], $"bucket {i}");
                }
            }
        }

        [Test]
        public void CopyTo_EmptyMapAndLargerDestination_CopiesTheWholeCapacityAndReportsZero()
        {
            using var storage = new SharedArrayMapStorage<int, int>(2, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.CopyTo(
                      new ArrayMapNode<int>[5]
                    , new int[5]
                    , new int[5]
                    , out var count
                    , out var collisions
                    , out _
                );

                Assert.AreEqual(0, count);
                Assert.AreEqual(0u, collisions);
            }
        }

        [Test]
        public void CopyTo_DestinationShorterThanTheStorage_ThrowsArgumentException()
        {
            using var storage = new SharedArrayMapStorage<int, int>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1, 10);

                Assert.Throws<ArgumentException>(
                    () => header.CopyTo(new ArrayMapNode<int>[3], new int[4], new int[3], out _, out _, out _)
                );
                Assert.Throws<ArgumentException>(
                    () => header.CopyTo(new ArrayMapNode<int>[4], new int[3], new int[3], out _, out _, out _)
                );
                Assert.Throws<ArgumentException>(
                    () => header.CopyTo(new ArrayMapNode<int>[4], new int[4], new int[2], out _, out _, out _)
                );
            }
        }

        [Test]
        public void Intersect_AndExclude_WithAnotherHeader_UseKeysOnly()
        {
            using var storage = new SharedArrayMapStorage<int, int>(6, 5);
            using var otherStorage = new SharedArrayMapStorage<int, long>(6, 5);
            var header = storage.CreateHeader();
            var other = otherStorage.CreateHeader();

            // SAFETY: both storages own the blocks their headers borrow and outlive every call below; the address of
            // the local header is not used after the call.
            unsafe
            {
                header.Add(1, 10);
                header.Add(2, 20);
                header.Add(3, 30);
                other.Add(2, 200L);
                other.Add(3, 300L);
                other.Add(4, 400L);

                header.Exclude(&other);

                Assert.AreEqual(1, header.Count);
                Assert.AreEqual(10, header[1]);

                header.Add(2, 21);
                header.Add(5, 50);
                header.Intersect(&other);

                Assert.AreEqual(1, header.Count);
                Assert.AreEqual(21, header[2]);
            }
        }

        [Test]
        public void SetOperations_WithEmptyOrSameHeader_MatchDictionary()
        {
            using var storage = new SharedArrayMapStorage<int, int>(6, 5);
            using var emptyStorage = new SharedArrayMapStorage<int, int>(6, 5);
            var header = storage.CreateHeader();
            var empty = emptyStorage.CreateHeader();

            // SAFETY: both storages own the blocks their headers borrow and outlive every call below; the addresses
            // of the local headers are not used after the calls.
            unsafe
            {
                header.Add(1, 10);
                header.Add(2, 20);
                header.Add(3, 30);

                header.Union(&empty);
                header.Exclude(&empty);
                header.Union(&header);
                header.Intersect(&header);

                Assert.AreEqual(3, header.Count);
                Assert.AreEqual(10, header[1]);
                Assert.AreEqual(20, header[2]);
                Assert.AreEqual(30, header[3]);

                header.Exclude(&header);

                Assert.AreEqual(0, header.Count);

                header.Add(4, 40);
                header.Intersect(&empty);

                Assert.AreEqual(0, header.Count);

                empty.Union(&header);

                Assert.AreEqual(0, empty.Count);
            }
        }

        [Test]
        public void Union_AnotherHeader_MergesAndOverwritesTheSharedKeys()
        {
            using var storage = new SharedArrayMapStorage<int, int>(6, 5);
            using var otherStorage = new SharedArrayMapStorage<int, int>(6, 5);
            var header = storage.CreateHeader();
            var other = otherStorage.CreateHeader();

            // SAFETY: both storages own the blocks their headers borrow and outlive every call below; the address of
            // the local header is not used after the call.
            unsafe
            {
                header.Add(1, 1);
                header.Add(2, 2);
                other.Add(2, 200);
                other.Add(3, 300);

                header.Union(&other);

                Assert.AreEqual(3, header.Count);
                Assert.AreEqual(1, header[1]);
                Assert.AreEqual(200, header[2]);
                Assert.AreEqual(300, header[3]);
            }
        }

        [Test]
        public void Elements_ByteElement_RoundTripBitExact()
        {
            AssertValueRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Elements_ThreeByteStruct_RoundTripBitExact()
        {
            AssertValueRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Elements_LongElement_RoundTripBitExact()
        {
            AssertValueRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Elements_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertValueRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = 0, b = 0, c = 0 }
                , new Wide { a = long.MinValue, b = 7, c = -1 }
            );
        }

        private static void AssertValueRoundTrip<TValue>(TValue a, TValue b, TValue c)
            where TValue : unmanaged
        {
            var name = typeof(TValue).Name;
            using var storage = new SharedArrayMapStorage<int, TValue>(4, 3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1, a);
                header[2] = b;
                header.GetOrAdd(3) = c;

                Assert.AreEqual(3, header.Count, name);
                Assert.AreEqual(a, header[1], name);
                Assert.AreEqual(b, header[2], name);
                Assert.AreEqual(c, header[3], name);
                Assert.AreEqual(b, header.ValueAt(1), name);
                Assert.IsTrue(header.TryGetValue(3, out var found), name);
                Assert.AreEqual(c, found, name);
                Assert.IsTrue(header.Remove(2, out _, out var removed), name);
                Assert.AreEqual(b, removed, name);
                Assert.AreEqual(a, header[1], name);
                Assert.AreEqual(c, header[3], name);

                var valuesInfo = new ArrayMapNode<int>[4];
                var values = new TValue[4];

                header.CopyTo(valuesInfo, values, new int[3], out var count, out _, out _);

                Assert.AreEqual(2, count, name);
                Assert.AreEqual(a, values[0], name);
                Assert.AreEqual(c, values[1], name);
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
