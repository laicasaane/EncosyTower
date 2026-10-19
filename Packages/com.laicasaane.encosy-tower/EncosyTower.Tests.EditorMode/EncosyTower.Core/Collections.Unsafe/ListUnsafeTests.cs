using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public partial class ListUnsafeTests
    {
        [Test]
        public void CapacityConstructor_CreatesEmptyListAndSupportsZeroCapacity()
        {
            using var list = new ListUnsafe<int>(4, new AllocatorStrategy(Allocator.Temp));
            using var zeroCapacity = new ListUnsafe<int>(0, Allocator.Temp);

            Assert.IsTrue(list.IsCreated);
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(4, list.Capacity);
            Assert.IsTrue(zeroCapacity.IsCreated);
            Assert.AreEqual(0, zeroCapacity.Count);
            Assert.AreEqual(0, zeroCapacity.Capacity);

            zeroCapacity.Add(7);

            Assert.IsTrue(zeroCapacity.IsCreated);
            Assert.AreEqual(1, zeroCapacity.Count);
            Assert.AreEqual(7, zeroCapacity[0]);
        }

        [Test]
        public void SpanConstructor_CopiesSource()
        {
            var source = new[] { 1, 2, 3 };
            using var list = new ListUnsafe<int>(source, Allocator.Temp);

            Assert.IsTrue(list.IsCreated);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(3, list.Capacity);
            CollectionAssert.AreEqual(source, list.ToArray());
        }

        [Test]
        public void Add_OverloadsGrowAndPreserveOrder()
        {
            using var list = new ListUnsafe<int>(1, Allocator.Temp);
            var second = 2;

            list.Add(1);
            list.Add(in second);

            for (var i = 3; i <= 20; i++)
            {
                list.Add(i);
            }

            Assert.AreEqual(20, list.Count);
            Assert.GreaterOrEqual(list.Capacity, 20);

            for (var i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(i + 1, list[i]);
            }
        }

        [Test]
        public void Insert_OverloadsSupportFirstMiddleAndLastPositions()
        {
            using var list = new ListUnsafe<int>(2, Allocator.Temp);
            var middle = 2;

            list.Insert(0, 1);
            list.Insert(1, 3);
            list.Insert(1, in middle);
            list.Insert(list.Count, 4);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void AddRange_OverloadsAppendRequestedItems()
        {
            using var list = new ListUnsafe<int>(1, Allocator.Temp);

            list.AddRange(new[] { 1, 2 });
            list.AddRange(new[] { 3, 4, 99 }, 2);
            list.AddRange(ReadOnlySpan<int>.Empty);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void IndexerAndElementAt_ReturnWritableReferences()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2 }, Allocator.Temp);

            ref var first = ref list[0];
            ref var second = ref list.ElementAt(1);
            first = 10;
            second = 20;

            CollectionAssert.AreEqual(new[] { 10, 20 }, list.ToArray());
        }

        [Test]
        public void CopyFromAndTryCopyFrom_AllOverloadsCopyOrReturnFalse()
        {
            using var list = new ListUnsafe<int>(new[] { 0, 0, 0, 0, 0, 0 }, Allocator.Temp);

            list.CopyFrom(new[] { 1, 2, 3 });
            list.CopyFrom(new[] { 4, 5, 99 }, 2);
            list.CopyFrom(2, new[] { 6, 7 });
            list.CopyFrom(4, new[] { 8, 9, 99 }, 2);

            CollectionAssert.AreEqual(new[] { 4, 5, 6, 7, 8, 9 }, list.ToArray());
            Assert.IsTrue(list.TryCopyFrom(new[] { 10, 11 }));
            Assert.IsTrue(list.TryCopyFrom(new[] { 12, 13, 99 }, 2));
            Assert.IsTrue(list.TryCopyFrom(2, new[] { 14, 15 }));
            Assert.IsTrue(list.TryCopyFrom(4, new[] { 16, 17, 99 }, 2));

            var expected = new[] { 12, 13, 14, 15, 16, 17 };
            CollectionAssert.AreEqual(expected, list.ToArray());
            Assert.IsFalse(list.TryCopyFrom(5, new[] { 18, 19 }));
            Assert.IsFalse(list.TryCopyFrom(5, new[] { 18, 19 }, 2));
            CollectionAssert.AreEqual(expected, list.ToArray());
        }

        [Test]
        public void CopyToAndTryCopyTo_AllOverloadsCopyOrReturnFalse()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var array = new int[6];
            var full = new int[4];
            var partial = new int[3];
            var offset = new int[2];
            var range = new int[3];

            list.CopyTo(array, 1);
            list.CopyTo(full);
            list.CopyTo(partial.AsSpan(), 2);
            list.CopyTo(1, offset);
            list.CopyTo(1, range, 2);

            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 0 }, array);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full);
            CollectionAssert.AreEqual(new[] { 1, 2, 0 }, partial);
            CollectionAssert.AreEqual(new[] { 2, 3 }, offset);
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, range);
            Assert.IsTrue(list.TryCopyTo(new int[4]));
            Assert.IsTrue(list.TryCopyTo(new int[3], 2));
            Assert.IsTrue(list.TryCopyTo(1, new int[3]));
            Assert.IsTrue(list.TryCopyTo(1, new int[3], 2));

            var failedDestination = new[] { 9, 9 };
            Assert.IsFalse(list.TryCopyTo(3, failedDestination));
            Assert.IsFalse(list.TryCopyTo(3, failedDestination, 2));
            CollectionAssert.AreEqual(new[] { 9, 9 }, failedDestination);
        }

        [Test]
        public void ClearAndFastClear_ResetCountAndAllowReuse()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            list.Clear();

            Assert.AreEqual(0, list.Count);
            list.Add(4);
            Assert.AreEqual(4, list[0]);

            list.FastClear();

            Assert.AreEqual(0, list.Count);
            list.Add(5);
            Assert.AreEqual(5, list[0]);
        }

        [Test]
        public void RemoveFamilies_HandleBoundariesAndSwapBackOrder()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4, 5, 6 }, Allocator.Temp);

            list.RemoveAt(0);
            CollectionAssert.AreEqual(new[] { 2, 3, 4, 5, 6 }, list.ToArray());

            list.RemoveAt(list.Count - 1);
            CollectionAssert.AreEqual(new[] { 2, 3, 4, 5 }, list.ToArray());

            list.RemoveRange(1, 2);
            CollectionAssert.AreEqual(new[] { 2, 5 }, list.ToArray());

            list.RemoveRange(1, 0);
            CollectionAssert.AreEqual(new[] { 2, 5 }, list.ToArray());

            list.RemoveAtSwapBack(0);
            CollectionAssert.AreEqual(new[] { 5 }, list.ToArray());
        }

        [Test]
        public void AddReplicateFamilies_ReturnAliasedNewRanges()
        {
            using var list = new ListUnsafe<int>(8, Allocator.Temp);

            var defaults = list.AddReplicate(2);
            var values = list.AddReplicate(7, 3);
            var noInit = list.AddReplicateNoInit(2);
            noInit[0] = 8;
            noInit[1] = 9;

            CollectionAssert.AreEqual(new[] { 0, 0 }, defaults.ToArray());
            CollectionAssert.AreEqual(new[] { 7, 7, 7 }, values.ToArray());
            CollectionAssert.AreEqual(new[] { 8, 9 }, noInit.ToArray());
            CollectionAssert.AreEqual(new[] { 0, 0, 7, 7, 7, 8, 9 }, list.ToArray());

            defaults[0] = 10;
            Assert.AreEqual(10, list[0]);
        }

        [Test]
        public void CapacityOperations_GrowAndTrimWithoutChangingContent()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2 }, Allocator.Temp);
            var initialCapacity = list.Capacity;

            list.IncreaseCapacityBy(5);
            Assert.GreaterOrEqual(list.Capacity, initialCapacity + 5);
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());

            var requestedCapacity = list.Capacity + 5;
            list.IncreaseCapacityTo(requestedCapacity);
            Assert.GreaterOrEqual(list.Capacity, requestedCapacity);
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());

            list.Trim();

            Assert.AreEqual(list.Count, list.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public void SpansArraysAndReadOnlyViews_AliasOrCopyAsDocumented()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var span = list.AsSpan();
            var readOnlySpan = list.AsReadOnlySpan();
            var array = list.ToArray();
            var readOnly = list.AsReadOnly();
            ListUnsafe<int>.ReadOnly converted = list;
            ReadOnlySpan<int> convertedSpan = converted;

            span[0] = 10;
            list[1] = 20;
            array[2] = 30;

            Assert.AreEqual(10, list[0]);
            Assert.AreEqual(20, readOnlySpan[1]);
            Assert.AreEqual(10, readOnly[0]);
            Assert.AreEqual(20, converted[1]);
            Assert.AreEqual(3, convertedSpan[2]);
            Assert.AreEqual(3, list[2]);
        }

        [Test]
        public void Reinterpret_AliasesSameSizedStorage()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2 }, Allocator.Temp);
            var reinterpreted = list.Reinterpret<uint>();

            Assert.IsTrue(reinterpreted.IsCreated);
            Assert.AreEqual(list.Count, reinterpreted.Count);
            Assert.AreEqual(list.Capacity, reinterpreted.Capacity);
            Assert.AreEqual(1u, reinterpreted[0]);

            reinterpreted[1] = 20u;

            Assert.AreEqual(20, list[1]);
        }

        [Test]
        public void ReadOnly_AllPropertiesCopyMethodsViewsAndConversionsWork()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = list.AsReadOnly();
            ListUnsafe<int>.ReadOnly converted = list;
            ReadOnlySpan<int> convertedSpan = readOnly;
            var full = new int[4];
            var partial = new int[3];
            var offset = new int[3];
            var range = new int[3];

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(4, readOnly.Count);
            Assert.AreEqual(4, readOnly.Capacity);
            Assert.AreEqual(2, readOnly[1]);
            Assert.AreEqual(1, converted[0]);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, convertedSpan.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, readOnly.AsReadOnlySpan().ToArray());

            readOnly.CopyTo(full);
            readOnly.CopyTo(partial, 2);
            readOnly.CopyTo(1, offset);
            readOnly.CopyTo(1, range, 2);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full);
            CollectionAssert.AreEqual(new[] { 1, 2, 0 }, partial);
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, offset);
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, range);
            Assert.IsTrue(readOnly.TryCopyTo(new int[4]));
            Assert.IsTrue(readOnly.TryCopyTo(new int[3], 2));
            Assert.IsTrue(readOnly.TryCopyTo(1, new int[3]));
            Assert.IsTrue(readOnly.TryCopyTo(1, new int[3], 2));

            var failedDestination = new[] { 9, 9 };
            Assert.IsFalse(readOnly.TryCopyTo(3, failedDestination));
            Assert.IsFalse(readOnly.TryCopyTo(3, failedDestination, 2));
            CollectionAssert.AreEqual(new[] { 9, 9 }, failedDestination);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, readOnly.ToArray());
            list[0] = 10;
            Assert.AreEqual(10, readOnly[0]);
        }

        [Test]
        public void OwnerEnumerator_DirectMembersVisitValuesAndReset()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var enumerator = list.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
            enumerator.Dispose();
        }

        [Test]
        public void ReadOnlyEnumerator_DirectMembersVisitValuesAndReset()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var readOnly = list.AsReadOnly();
            var enumerator = readOnly.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Current);

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
            enumerator.Dispose();
        }

        [Test]
        public void Enumerator_DoesNotInvalidateOnOwnerMutation()
        {
            using var list = new ListUnsafe<int>(8, Allocator.Temp);
            list.AddRange(new[] { 1, 2, 3 });
            var enumerator = list.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);

            list[1] = 20;
            list.Add(4);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(20, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
            enumerator.Dispose();
        }

        [Test]
        public void Dispose_MarksNotCreatedAndDoubleDisposeIsNoOp()
        {
            var list = new ListUnsafe<int>(4, Allocator.Temp);
            list.Add(1);

            list.Dispose();

            Assert.IsFalse(list.IsCreated);
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, list.Capacity);
            Assert.DoesNotThrow(() => list.Dispose());
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var list = new ListUnsafe<int>(4, Allocator.TempJob);
            list.Add(1);

            var handle = list.Dispose(default);

            Assert.IsFalse(list.IsCreated);
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, list.Capacity);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Validation_RejectsInvalidIndexesRangesAmountsAndCapacity()
        {
            Assert.Throws<InvalidOperationException>(
                () => _ = new ListUnsafe<int>(-1, Allocator.Temp)
            );

            using var list = new ListUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var readOnly = list.AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => _ = list[-1]);
            Assert.Throws<InvalidOperationException>(() => _ = list.ElementAt(3));
            Assert.Throws<InvalidOperationException>(() => list.Insert(4, 4));
            Assert.Throws<InvalidOperationException>(() => list.RemoveAt(3));
            Assert.Throws<InvalidOperationException>(() => list.RemoveRange(3, 1));
            Assert.Throws<InvalidOperationException>(() => list.RemoveAtSwapBack(3));
            Assert.Throws<InvalidOperationException>(() => list.AddReplicate(0));
            Assert.Throws<InvalidOperationException>(() => list.AddReplicate(1, 0));
            Assert.Throws<InvalidOperationException>(() => list.AddReplicateNoInit(0));
            Assert.Throws<InvalidOperationException>(() => list.IncreaseCapacityTo(2));
            Assert.Throws<InvalidOperationException>(() => _ = readOnly[3]);
            Assert.Throws<InvalidOperationException>(() => list.Reinterpret<byte>());
        }

        [Test]
        public void RemoveRange_ZeroLengthBoundaries_PinContract()
        {
            // Pins the family-wide contract: RemoveRange validates startIndex < Count before
            // looking at length, so a zero-length remove at Count (including on an empty list)
            // throws, unlike BCL List<T>.RemoveRange. Zero-length removes at a valid index
            // are no-ops.
            using var empty = new ListUnsafe<int>(0, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => empty.RemoveRange(0, 0));

            using var list = new ListUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.RemoveRange(3, 0));

            list.RemoveRange(0, 0);
            list.RemoveRange(2, 0);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAndDisposes(Allocator allocator)
        {
            var list = new ListUnsafe<int>(4, allocator);

            try
            {
                list.Add(1);

                Assert.IsTrue(list.IsCreated);
                Assert.AreEqual(1, list.Count);
            }
            finally
            {
                list.Dispose();
            }

            Assert.IsFalse(list.IsCreated);
            Assert.AreEqual(0, list.Count);
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_JobHandle_AcceptedAllocator_ReleasesTheStorage(Allocator allocator)
        {
            var list = new ListUnsafe<int>(new[] { 1, 2 }, allocator);

            var handle = list.Dispose(default);

            Assert.IsFalse(list.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void Dispose_DefaultList_IsNoOp()
        {
            var list = default(ListUnsafe<int>);

            Assert.DoesNotThrow(() => list.Dispose());
            Assert.DoesNotThrow(() => list.Dispose(default).Complete());
            Assert.IsFalse(list.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_DefaultAllocatorStrategy_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new ListUnsafe<int>(4, default(AllocatorStrategy)));
            Assert.Throws<InvalidOperationException>(
                () => _ = new ListUnsafe<int>(new[] { 1 }, default(AllocatorStrategy))
            );
        }

        [Test]
        public void SpanConstructor_EmptySource_CreatesAnEmptyListThatCanGrow()
        {
            using var list = new ListUnsafe<int>(ReadOnlySpan<int>.Empty, Allocator.Temp);

            Assert.IsTrue(list.IsCreated);
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, list.AsSpan().Length);

            list.Add(5);

            Assert.AreEqual(5, list[0]);
        }

        [Test]
        public void UncreatedList_ViewsAreEmptyAndTrimDoesNothing()
        {
            var list = default(ListUnsafe<int>);

            Assert.IsFalse(list.IsCreated);
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, list.Capacity);
            Assert.AreEqual(0, list.AsSpan().Length);
            Assert.AreEqual(0, list.AsReadOnlySpan().Length);
            Assert.AreEqual(0, list.ToArray().Length);
            Assert.DoesNotThrow(() => list.Trim());
            Assert.DoesNotThrow(() => list.Clear());
            Assert.DoesNotThrow(() => list.FastClear());
            Assert.IsTrue(list.TryCopyTo(Span<int>.Empty));
            Assert.IsFalse(list.TryCopyTo(new int[1]));
            Assert.IsTrue(list.TryCopyFrom(ReadOnlySpan<int>.Empty));
            Assert.IsFalse(list.TryCopyFrom(new[] { 1 }));
        }

        [Test]
        public void AsReadOnly_UncreatedList_ThrowsArgumentException()
        {
            var list = default(ListUnsafe<int>);

            Assert.Throws<ArgumentException>(() => list.AsReadOnly());
            Assert.Throws<ArgumentException>(() => _ = (ListUnsafe<int>.ReadOnly)list);
            Assert.Throws<ArgumentException>(() => list.GetEnumerator());
        }

        [Test]
        public void Elements_ByteElement_RoundTripBitExact()
        {
            AssertElementRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Elements_ThreeByteStruct_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Elements_LongElement_RoundTripBitExact()
        {
            AssertElementRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Elements_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = 0, b = 0, c = 0 }
                , new Wide { a = long.MinValue, b = 7, c = -1 }
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ElementAt_EmptyList_Throws()
        {
            using var list = new ListUnsafe<int>(4, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.ElementAt(0));
            Assert.Throws<InvalidOperationException>(() => _ = list[0]);
        }

        [Test]
        public void ElementAt_ReferencesAliasTheStorageOfTheIndexer()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            ref var viaElementAt = ref list.ElementAt(2);
            ref var viaIndexer = ref list[2];

            viaElementAt = 30;

            Assert.AreEqual(30, viaIndexer);

            viaIndexer = 31;

            Assert.AreEqual(31, list.ElementAt(2));
            Assert.AreEqual(31, list.AsReadOnlySpan()[2]);
        }

        [Test]
        public void Add_FullList_GrowsAndKeepsEveryElement()
        {
            var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                Assert.AreEqual(list.Count, list.Capacity);

                list.Add(5);

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, list.ToArray());
                Assert.GreaterOrEqual(list.Capacity, 5);
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void Add_InArgumentAliasingOwnStorageOnFullList_AppendsACopy()
        {
            var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                list.Add(in list.ElementAt(0));

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 1 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void Insert_InArgumentAliasingOwnStorageOnFullList_InsertsACopy()
        {
            var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                list.Insert(2, in list.ElementAt(0));

                var expected = new List<int> { 1, 2, 3, 4 };
                expected.Insert(2, expected[0]);

                CollectionAssert.AreEqual(expected, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void InsertRemoveAt_EveryPosition_MatchesListOfT()
        {
            const int COUNT = 5;
            var list = new ListUnsafe<int>(COUNT, Allocator.Temp);

            try
            {
                for (var index = 0; index <= COUNT; index++)
                {
                    list.Clear();
                    var expected = new List<int>();

                    for (var i = 1; i <= COUNT; i++)
                    {
                        list.Add(i);
                        expected.Add(i);
                    }

                    list.Insert(index, 99);
                    expected.Insert(index, 99);

                    CollectionAssert.AreEqual(expected, list.ToArray(), $"insert {index}");

                    list.RemoveAt(index);
                    expected.RemoveAt(index);

                    CollectionAssert.AreEqual(expected, list.ToArray(), $"remove {index}");
                }
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void Insert_IntoEmptyList_AddsTheOnlyElement()
        {
            using var list = new ListUnsafe<int>(0, Allocator.Temp);

            list.Insert(0, 7);

            CollectionAssert.AreEqual(new[] { 7 }, list.ToArray());
        }

        [Test]
        public void RemoveAt_SingleElementList_LeavesAnEmptyListThatCanGrow()
        {
            using var list = new ListUnsafe<int>(new[] { 1 }, Allocator.Temp);

            list.RemoveAt(0);

            Assert.AreEqual(0, list.Count);

            list.Add(2);

            CollectionAssert.AreEqual(new[] { 2 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void RemoveAtAndRemoveAtSwapBack_EmptyList_Throw()
        {
            using var list = new ListUnsafe<int>(4, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.RemoveAt(0));
            Assert.Throws<InvalidOperationException>(() => list.RemoveAtSwapBack(0));
        }

        [Test]
        public void RemoveAtSwapBack_EveryIndex_MovesTheLastElementIntoTheGap()
        {
            const int COUNT = 5;
            var list = new ListUnsafe<int>(COUNT, Allocator.Temp);

            try
            {
                for (var index = 0; index < COUNT; index++)
                {
                    list.Clear();
                    var expected = new List<int>();

                    for (var i = 1; i <= COUNT; i++)
                    {
                        list.Add(i);
                        expected.Add(i);
                    }

                    expected[index] = expected[COUNT - 1];
                    expected.RemoveAt(COUNT - 1);
                    list.RemoveAtSwapBack(index);

                    CollectionAssert.AreEqual(expected, list.ToArray(), $"index {index}");
                }
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void RemoveRange_EveryValidRange_MatchesListOfT()
        {
            const int COUNT = 6;
            var list = new ListUnsafe<int>(COUNT, Allocator.Temp);

            try
            {
                for (var start = 0; start < COUNT; start++)
                {
                    for (var length = 0; length <= COUNT - start; length++)
                    {
                        list.Clear();
                        var expected = new List<int>();

                        for (var i = 1; i <= COUNT; i++)
                        {
                            list.Add(i);
                            expected.Add(i);
                        }

                        list.RemoveRange(start, length);
                        expected.RemoveRange(start, length);

                        CollectionAssert.AreEqual(expected, list.ToArray(), $"start {start} length {length}");
                    }
                }
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_SpanOverOwnStorageOnFullList_AppendsACopy()
        {
            var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                list.AddRange(list.AsReadOnlySpan());

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 1, 2, 3, 4 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_PrefixOfOwnStorageOnFullList_AppendsACopy()
        {
            var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                list.AddRange(list.AsReadOnlySpan(), 2);

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 1, 2 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_SpanOverOwnStorageWithSpareCapacity_AppendsACopy()
        {
            var list = new ListUnsafe<int>(16, Allocator.Temp);

            try
            {
                list.AddRange(new[] { 1, 2, 3 });
                list.AddRange(list.AsReadOnlySpan());

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 2, 3 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_ExactlyFillsThenGrows_KeepsEveryElement()
        {
            var list = new ListUnsafe<int>(3, Allocator.Temp);

            try
            {
                list.AddRange(new[] { 1, 2, 3 });

                Assert.AreEqual(list.Count, list.Capacity);

                list.AddRange(new[] { 4 });

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_ZeroCountOrEmptyItems_ChangesNothing()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2 }, Allocator.Temp);

            list.AddRange(new[] { 9, 9 }, 0);
            list.AddRange(ReadOnlySpan<int>.Empty, 0);

            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AddRange_CountOutsideItems_Throws()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.AddRange(new[] { 9, 9 }, 3));
            Assert.Throws<InvalidOperationException>(() => list.AddRange(new[] { 9, 9 }, -1));
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public void CopyFrom_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsContent()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var source = new[] { 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyFrom(-1, source));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyFrom(5, source));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyFrom(3, source));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyFrom(0, source, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyFrom(0, source, -1));
            Assert.IsFalse(list.TryCopyFrom(-1, source));
            Assert.IsFalse(list.TryCopyFrom(3, source));
            Assert.IsFalse(list.TryCopyFrom(0, source, 3));
            Assert.IsFalse(list.TryCopyFrom(0, source, -1));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void CopyTo_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(-1, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(5, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(0, destination, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(0, new int[2], 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(new int[2], 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(destination, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(destination, -1));
            Assert.IsFalse(list.TryCopyTo(-1, destination));
            Assert.IsFalse(list.TryCopyTo(0, destination, 5));
            Assert.IsFalse(list.TryCopyTo(0, new int[2], 3));
            Assert.IsFalse(list.TryCopyTo(0, destination, -1));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void CopyToAndCopyFrom_EmptyListAndEmptySpans_CopyNothing()
        {
            using var list = new ListUnsafe<int>(4, Allocator.Temp);

            list.CopyTo(Span<int>.Empty);
            list.CopyTo(new int[3], 3);
            list.CopyFrom(ReadOnlySpan<int>.Empty);

            Assert.IsTrue(list.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(list.TryCopyFrom(ReadOnlySpan<int>.Empty));
            Assert.IsFalse(list.TryCopyTo(new int[1]));
            Assert.IsFalse(list.TryCopyFrom(new[] { 1 }));
        }

        [Test]
        public void CopyFrom_SourceOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var list = new ListUnsafe<int>(seed, Allocator.Temp);

            list.CopyFrom(1, list.AsReadOnlySpan()[..4]);

            var expectedForward = (int[])seed.Clone();
            Array.Copy(expectedForward, 0, expectedForward, 1, 4);
            CollectionAssert.AreEqual(expectedForward, list.ToArray());

            list.CopyFrom(seed);
            list.CopyFrom(0, list.AsReadOnlySpan()[1..5]);

            var expectedBackward = (int[])seed.Clone();
            Array.Copy(expectedBackward, 1, expectedBackward, 0, 4);
            CollectionAssert.AreEqual(expectedBackward, list.ToArray());
        }

        [Test]
        public void CopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var list = new ListUnsafe<int>(seed, Allocator.Temp);

            list.CopyTo(2, list.AsSpan()[..4]);

            var expectedBackward = (int[])seed.Clone();
            Array.Copy(expectedBackward, 2, expectedBackward, 0, 4);
            CollectionAssert.AreEqual(expectedBackward, list.ToArray());

            list.CopyFrom(seed);
            list.CopyTo(0, list.AsSpan()[2..]);

            var expectedForward = (int[])seed.Clone();
            Array.Copy(expectedForward, 0, expectedForward, 2, 4);
            CollectionAssert.AreEqual(expectedForward, list.ToArray());
        }

        [Test]
        public void ClearAndFastClear_DefaultList_KeepTheListUncreated()
        {
            var list = default(ListUnsafe<int>);

            list.Clear();
            list.FastClear();

            Assert.AreEqual(0, list.Count);
            Assert.IsFalse(list.IsCreated);
        }

        [Test]
        public void IncreaseCapacityTo_NotLargerThanCapacity_ReturnsTheCurrentCapacity()
        {
            using var list = new ListUnsafe<int>(4, Allocator.Temp);
            list.AddRange(new[] { 1, 2 });

            Assert.AreEqual(4, list.IncreaseCapacityTo(2));
            Assert.AreEqual(4, list.IncreaseCapacityTo(4));
            Assert.AreEqual(4, list.IncreaseCapacityBy(0));
            Assert.AreEqual(4, list.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public void IncreaseCapacityTo_ZeroCapacityList_AllocatesTheRequestedCapacity()
        {
            using var list = new ListUnsafe<int>(0, Allocator.Temp);

            var capacity = list.IncreaseCapacityTo(8);

            Assert.AreEqual(8, capacity);
            Assert.AreEqual(8, list.Capacity);
            Assert.AreEqual(0, list.Count);

            list.Add(1);

            CollectionAssert.AreEqual(new[] { 1 }, list.ToArray());
        }

        [Test]
        public void IncreaseCapacityBy_AmountOverflowingInt_ThrowsOverflow()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<OverflowException>(() => list.IncreaseCapacityBy(int.MaxValue));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IncreaseCapacityBy_NegativeAmountBelowCount_Throws()
        {
            using var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.IncreaseCapacityBy(-5));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void Trim_EmptyList_ReleasesTheCapacityAndStaysUsable()
        {
            using var list = new ListUnsafe<int>(8, Allocator.Temp);

            list.Trim();

            Assert.IsTrue(list.IsCreated);
            Assert.AreEqual(0, list.Capacity);

            list.Add(3);

            CollectionAssert.AreEqual(new[] { 3 }, list.ToArray());
        }

        [Test]
        public void AddReplicate_FullList_GrowsAndKeepsEveryElement()
        {
            var list = new ListUnsafe<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                var filled = list.AddReplicate(9, 2);
                var zeroed = list.AddReplicate(1);

                Assert.AreEqual(2, filled.Length);
                Assert.AreEqual(1, zeroed.Length);
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 9, 9, 0 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddReplicate_ZeroCapacityList_AddsTheRequestedAmount()
        {
            using var list = new ListUnsafe<int>(0, Allocator.Temp);

            var values = list.AddReplicate(5, 3);

            CollectionAssert.AreEqual(new[] { 5, 5, 5 }, values.ToArray());
            CollectionAssert.AreEqual(new[] { 5, 5, 5 }, list.ToArray());
        }

        [Test]
        public void AddReplicateNoInit_ZeroCapacityList_ReturnsWritableRange()
        {
            using var list = new ListUnsafe<int>(0, Allocator.Temp);

            var range = list.AddReplicateNoInit(2);
            range[0] = 4;
            range[1] = 8;

            CollectionAssert.AreEqual(new[] { 4, 8 }, list.ToArray());
        }

        [Test]
        public void Reinterpret_UncreatedList_ReturnsAnUncreatedView()
        {
            var list = default(ListUnsafe<int>);

            var view = list.Reinterpret<uint>();

            Assert.IsFalse(view.IsCreated);
            Assert.AreEqual(0, view.Count);
            Assert.AreEqual(0, view.AsSpan().Length);
        }

        [Test]
        public void Reinterpret_WriteThroughTheView_IsSeenByTheOwnerForFloatAndWideElements()
        {
            using var floats = new ListUnsafe<int>(new[] { 0, 0, 0 }, Allocator.Temp);
            using var wides = new ListUnsafe<Wide>(new Wide[2], Allocator.Temp);

            // The views are never disposed: they share the owners' allocations, so only the owners release them.
            var floatView = floats.Reinterpret<float>();
            var wideView = wides.Reinterpret<WideOther>();

            floatView[1] = 1.5f;
            wides[1] = new Wide { a = 1, b = -2, c = long.MaxValue };

            Assert.AreEqual(BitConverter.SingleToInt32Bits(1.5f), floats[1]);
            Assert.AreEqual(1L, wideView[1].x);
            Assert.AreEqual(-2L, wideView[1].y);
            Assert.AreEqual(long.MaxValue, wideView[1].z);
        }

        [Test]
        public void GetEnumerator_EmptyList_YieldsNothingForEveryEnumeratorForm()
        {
            using var list = new ListUnsafe<int>(4, Allocator.Temp);

            var structEnumerator = list.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)list).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)list).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [Test]
        public void ReadOnly_Enumerator_EmptyList_YieldsNothingForEveryEnumeratorForm()
        {
            using var list = new ListUnsafe<int>(4, Allocator.Temp);
            var readOnly = list.AsReadOnly();

            var structEnumerator = readOnly.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)readOnly).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)readOnly).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Alloc_AcceptedAllocator_CreatesAndFreesTheHeader(Allocator allocator)
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = ListUnsafe<int>.Alloc(4, allocator);

                try
                {
                    data->Add(1);

                    Assert.IsTrue(data->IsCreated);
                    Assert.AreEqual(1, data->Count);
                    Assert.AreEqual(1, (*data)[0]);
                }
                finally
                {
                    ListUnsafe<int>.Free(data);
                }
            }
        }

        [Test]
        public void Alloc_ZeroCapacity_CreatesAnEmptyHeaderThatCanGrow()
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = ListUnsafe<int>.Alloc(0, Allocator.Persistent);

                try
                {
                    Assert.IsTrue(data->IsCreated);
                    Assert.AreEqual(0, data->Capacity);

                    data->AddRange(new[] { 1, 2, 3 });

                    CollectionAssert.AreEqual(new[] { 1, 2, 3 }, data->ToArray());
                }
                finally
                {
                    ListUnsafe<int>.Free(data);
                }
            }
        }

        [Test]
        public void Alloc_SpanSource_CopiesTheSourceAndAcceptsAnEmptySource()
        {
            // SAFETY: Both headers returned by Alloc stay live until Free, which runs exactly once in finally.
            unsafe
            {
                var copy = ListUnsafe<int>.Alloc(new[] { 1, 2, 3 }, Allocator.Persistent);
                var empty = ListUnsafe<int>.Alloc(ReadOnlySpan<int>.Empty, Allocator.Persistent);

                try
                {
                    CollectionAssert.AreEqual(new[] { 1, 2, 3 }, copy->ToArray());
                    Assert.IsTrue(empty->IsCreated);
                    Assert.AreEqual(0, empty->Count);
                }
                finally
                {
                    ListUnsafe<int>.Free(copy);
                    ListUnsafe<int>.Free(empty);
                }
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Alloc_InvalidArgument_Throws()
        {
            // The header is allocated before the constructor validates the capacity (suspected defect D-17), so
            // this call leaks one header. Allocator.Temp is used so that Unity releases it without a leak report.
            // SAFETY: The call throws before it returns a header, so no pointer is used.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => ListUnsafe<int>.Alloc(-1, Allocator.Temp));
            }
        }

        [Test]
        public void Free_NullPointer_DoesNothing()
        {
            // SAFETY: Free documents that a null header is ignored, so no memory is read or released.
            unsafe
            {
                Assert.DoesNotThrow(() => ListUnsafe<int>.Free(null));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        [Category("Task045X2Regression")]
        public void Reinterpret_DisposeBorrowedView_LeavesOwnerWritableAndResizable(bool scheduled)
        {
            var owner = new ListUnsafe<int>(new[] { 17, 29 }, Allocator.Persistent);
            var view = owner.Reinterpret<uint>();

            try
            {
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
                CollectionAssert.AreEqual(new[] { 17, 29 }, owner.ToArray());
                owner[1] = 31;
                owner.Add(43);
                CollectionAssert.AreEqual(new[] { 17, 31, 43 }, owner.ToArray());
            }
            finally
            {
                view.Dispose();
                owner.Dispose();
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void Reinterpret_ChangeBorrowedCapacity_RejectsWithoutChangingOwner()
        {
            var owner = new ListUnsafe<int>(4, Allocator.Persistent);
            owner.AddRange(new[] { 17, 29 });
            var view = owner.Reinterpret<uint>();

            try
            {
                Assert.AreEqual(4, view.IncreaseCapacityTo(4));
                Assert.Throws<InvalidOperationException>(() => view.IncreaseCapacityTo(8));
                Assert.Throws<InvalidOperationException>(() => view.Trim());
                Assert.AreEqual(4, view.Capacity);
                Assert.AreEqual(2, view.Count);
                Assert.AreEqual(4, owner.Capacity);
                CollectionAssert.AreEqual(new[] { 17, 29 }, owner.ToArray());
                view[1] = 31;
                Assert.AreEqual(31, owner[1]);
            }
            finally
            {
                view.Dispose();
                owner.Dispose();
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void AddRange_NonzeroOffsetOwnSpanOnFullList_AppendsOriginalSlice()
        {
            var list = new ListUnsafe<int>(new[] { 11, 22, 33, 44 }, Allocator.Persistent);

            try
            {
                list.AddRange(list.AsReadOnlySpan().Slice(1, 2));

                CollectionAssert.AreEqual(new[] { 11, 22, 33, 44, 22, 33 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void Insert_InArgumentAliasingShiftedElementWithSpareCapacity_InsertsOriginalValue()
        {
            var list = new ListUnsafe<int>(8, Allocator.Persistent);

            try
            {
                list.AddRange(new[] { 11, 22, 33, 44 });
                list.Insert(1, in list.ElementAt(2));

                Assert.AreEqual(8, list.Capacity);
                CollectionAssert.AreEqual(new[] { 11, 33, 22, 33, 44 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var list = new ListUnsafe<T>(1, Allocator.Temp);

            try
            {
                list.Add(a);
                list.Add(in b);
                list.AddRange(new[] { c });
                list.Insert(1, c);

                CollectionAssert.AreEqual(new[] { a, c, b, c }, list.ToArray(), name);

                list.RemoveAt(1);

                Assert.AreEqual(a, list[0], name);
                Assert.AreEqual(b, list.ElementAt(1), name);
                Assert.AreEqual(c, list[2], name);

                var copy = new T[3];
                list.CopyTo(copy, 0);

                CollectionAssert.AreEqual(new[] { a, b, c }, copy, name);

                list.CopyFrom(new[] { c, b, a });

                CollectionAssert.AreEqual(new[] { c, b, a }, list.AsReadOnlySpan().ToArray(), name);

                var replicated = list.AddReplicate(a, 2);

                Assert.AreEqual(a, replicated[0], name);
                Assert.AreEqual(a, replicated[1], name);
                Assert.AreEqual(5, list.Count, name);
                Assert.AreEqual(a, list.AsReadOnly()[4], name);
            }
            finally
            {
                list.Dispose();
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
    }
}
