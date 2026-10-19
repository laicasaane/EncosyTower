using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Buffers;
using EncosyTower.Collections;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class ListNativeTests
    {
        [Test]
        public void Constructor_CreatesEmptyListAndSupportsZeroCapacity()
        {
            using var list = new ListNative<int>(4, Allocator.Temp);
            using var zeroCapacity = new ListNative<int>(0, Allocator.Temp);

            Assert.IsTrue(list.IsCreated);
            Assert.AreEqual(0, list.Count);
            Assert.GreaterOrEqual(list.Capacity, 4);

            zeroCapacity.Add(7);

            Assert.AreEqual(1, zeroCapacity.Count);
            Assert.AreEqual(7, zeroCapacity[0]);
        }

        [Test]
        public void SpanConstructor_CopiesSource()
        {
            var source = new[] { 1, 2, 3 };
            using var list = new ListNative<int>(source, Allocator.Temp);

            CollectionAssert.AreEqual(source, list.ToArray());
        }

        [Test]
        public void Add_GrowsPastInitialCapacity()
        {
            const int N = 200;
            using var list = new ListNative<int>(2, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                list.Add(i * 2);
            }

            Assert.AreEqual(N, list.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.AreEqual(i * 2, list[i]);
            }
        }

        [Test]
        public void ValueAndInOverloads_AddAndInsertExpectedValues()
        {
            using var list = new ListNative<int>(4, Allocator.Temp);
            var two = 2;
            var three = 3;
            var four = 4;

            list.Add(1);
            list.Add(in two);
            list.Insert(2, three);
            list.Insert(3, in four);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void InsertAndRemoveFamilies_PreserveExpectedOrder()
        {
            using var list = new ListNative<int>(8, Allocator.Temp);
            list.AddRange(new[] { 1, 3, 4, 5, 6 });

            list.Insert(1, 2);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list.ToArray());

            list.RemoveAt(2);
            CollectionAssert.AreEqual(new[] { 1, 2, 4, 5, 6 }, list.ToArray());

            list.RemoveRange(1, 2);
            CollectionAssert.AreEqual(new[] { 1, 5, 6 }, list.ToArray());

            list.RemoveAtSwapBack(0);
            CollectionAssert.AreEqual(new[] { 6, 5 }, list.ToArray());
        }

        [Test]
        public void IndexerAndElementAt_MutateValues()
        {
            var list = new ListNative<int>(4, Allocator.Temp);

            try
            {
                list.AddRange(new[] { 1, 2 });
                list[0] = 10;
                ref var value = ref list.ElementAt(1);
                value = 20;

                CollectionAssert.AreEqual(new[] { 10, 20 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_FamiliesAppendRequestedItems()
        {
            using var list = new ListNative<int>(2, Allocator.Temp);
            using var native = new NativeArray<int>(new[] { 5, 6, 7 }, Allocator.Temp);
            var slice = new NativeSlice<int>(native, 1, 2);

            list.AddRange(new[] { 1, 2 });
            list.AddRange(new[] { 3, 4, 99 }, 2);
            list.AddRange(native);
            list.AddRange(native, 2);
            list.AddRange(in slice);
            list.AddRange(in slice, 1);

            CollectionAssert.AreEqual(
                new[] { 1, 2, 3, 4, 5, 6, 7, 5, 6, 6, 7, 6 },
                list.ToArray()
            );
        }

        [Test]
        public void AddReplicate_FamiliesReturnNewRanges()
        {
            using var list = new ListNative<int>(8, Allocator.Temp);

            var defaults = list.AddReplicate(2);
            var values = list.AddReplicate(7, 3);
            var noInit = list.AddReplicateNoInit(2);
            noInit[0] = 8;
            noInit[1] = 9;

            CollectionAssert.AreEqual(new[] { 0, 0 }, defaults.ToArray());
            CollectionAssert.AreEqual(new[] { 7, 7, 7 }, values.ToArray());
            CollectionAssert.AreEqual(new[] { 0, 0, 7, 7, 7, 8, 9 }, list.ToArray());
        }

        [Test]
        public void CopyFromAndTryCopyFrom_SpanFamiliesCopyOrReturnFalse()
        {
            using var list = ListNative<int>.Prefill(0, 6, Allocator.Temp);

            list.CopyFrom(new[] { 1, 2, 3 });
            list.CopyFrom(new[] { 4, 5, 99 }, 2);
            list.CopyFrom(2, new[] { 6, 7 });
            list.CopyFrom(4, new[] { 8, 9, 99 }, 2);

            CollectionAssert.AreEqual(new[] { 4, 5, 6, 7, 8, 9 }, list.ToArray());

            Assert.IsTrue(list.TryCopyFrom(new[] { 10, 11 }));
            Assert.IsTrue(list.TryCopyFrom(new[] { 12, 13, 99 }, 2));
            Assert.IsTrue(list.TryCopyFrom(2, new[] { 14, 15 }));
            Assert.IsTrue(list.TryCopyFrom(4, new[] { 16, 17, 99 }, 2));
            Assert.IsFalse(list.TryCopyFrom(5, new[] { 18, 19 }));

            CollectionAssert.AreEqual(new[] { 12, 13, 14, 15, 16, 17 }, list.ToArray());
        }

        [Test]
        public void CopyToAndTryCopyTo_SpanFamiliesCopyOrReturnFalse()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var full = new int[4];
            var partial = new int[2];
            var offset = new int[2];
            var explicitLength = new int[3];
            var array = new int[6];

            list.CopyTo(full);
            list.CopyTo(partial.AsSpan(), 2);
            list.CopyTo(1, offset);
            list.CopyTo(1, explicitLength, 2);
            list.CopyTo(array, 1);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full);
            CollectionAssert.AreEqual(new[] { 1, 2 }, partial);
            CollectionAssert.AreEqual(new[] { 2, 3 }, offset);
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, explicitLength);
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 0 }, array);

            Assert.IsTrue(list.TryCopyTo(full));
            Assert.IsTrue(list.TryCopyTo(partial, 2));
            Assert.IsTrue(list.TryCopyTo(1, offset));
            Assert.IsTrue(list.TryCopyTo(1, explicitLength, 2));
            Assert.IsFalse(list.TryCopyTo(3, partial));
        }

        [Test]
        public void CopyTo_NativeFamiliesCopyRequestedRanges()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            using var native = new NativeArray<int>(3, Allocator.Temp);
            using var sliceOwner = new NativeArray<int>(4, Allocator.Temp);
            var slice = new NativeSlice<int>(sliceOwner, 1, 2);

            list.CopyTo(1, native);
            list.CopyTo(2, in slice, 2);

            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, native.ToArray());
            CollectionAssert.AreEqual(new[] { 0, 3, 4, 0 }, sliceOwner.ToArray());
        }

        [Test]
        public void CopyTo_RemainingNativeOverloadsCopyRequestedRanges()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            using var full = new NativeArray<int>(4, Allocator.Temp);
            using var partial = new NativeArray<int>(3, Allocator.Temp);
            using var sliceOwner = new NativeArray<int>(4, Allocator.Temp);
            var slice = new NativeSlice<int>(sliceOwner, 1, 3);

            list.CopyTo(full);
            list.CopyTo(1, partial, 2);
            list.CopyTo(1, in slice);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full.ToArray());
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, partial.ToArray());
            CollectionAssert.AreEqual(new[] { 0, 2, 3, 4 }, sliceOwner.ToArray());
        }

        [Test]
        public void ClearAndFastClear_ResetCountAndAllowReuse()
        {
            using var list = new ListNative<int>(4, Allocator.Temp);
            list.AddRange(new[] { 1, 2, 3 });

            list.Clear();

            Assert.AreEqual(0, list.Count);
            list.Add(4);

            list.FastClear();

            Assert.AreEqual(0, list.Count);
            list.Add(5);
            Assert.AreEqual(5, list[0]);
        }

        [Test]
        public void CapacityOperations_GrowAndTrimToCount()
        {
            using var list = new ListNative<int>(4, Allocator.Temp);
            list.AddRange(new[] { 1, 2 });
            var initialCapacity = list.Capacity;

            list.IncreaseCapacityBy(5);
            Assert.GreaterOrEqual(list.Capacity, initialCapacity + 5);

            var grownCapacity = list.Capacity;
            list.IncreaseCapacityTo(grownCapacity + 5);
            Assert.GreaterOrEqual(list.Capacity, grownCapacity + 5);

            list.Trim();

            Assert.AreEqual(list.Count, list.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public void ArraysAndViews_ExposeEquivalentAliasedContent()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            var span = list.AsSpan();
            var readOnlySpan = list.AsReadOnlySpan();
            var slice = list.AsNativeSlice();

            span[0] = 10;
            slice[2] = 30;

            CollectionAssert.AreEqual(new[] { 10, 2, 30 }, list.ToArray());
            Assert.AreEqual(10, readOnlySpan[0]);
            Assert.AreEqual(30, readOnlySpan[2]);
        }

        [Test]
        public void Reinterpret_ExposesSameSizedValues()
        {
            using var list = new ListNative<int>(new[] { 1, 2 }, Allocator.Temp);
            var reinterpreted = list.Reinterpret<uint>();

            Assert.AreEqual(list.Count, reinterpreted.Count);
            Assert.AreEqual(1u, reinterpreted[0]);
            Assert.AreEqual(2u, reinterpreted[1]);
        }

        [Test]
        public void AsReadOnly_ReflectsOwnerContent()
        {
            using var list = new ListNative<int>(new[] { 1, 2 }, Allocator.Temp);
            var readOnly = list.AsReadOnly();

            list.Add(3);

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(3, readOnly.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, readOnly.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, readOnly.AsReadOnlySpan().ToArray());
        }

        [Test]
        public void ReadOnly_PropertiesConversionAndCopyOverloadsExposeOwnerValues()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            ListNative<int>.ReadOnly readOnly = list;
            var full = new int[4];
            var partial = new int[2];
            var offset = new int[2];
            var explicitLength = new int[3];

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(4, readOnly.Count);
            Assert.GreaterOrEqual(readOnly.Capacity, 4);
            Assert.AreEqual(3, readOnly[2]);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, readOnly.AsReadOnlySpan().ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, readOnly.ToArray());

            readOnly.CopyTo(full);
            readOnly.CopyTo(partial.AsSpan(), 2);
            readOnly.CopyTo(1, offset);
            readOnly.CopyTo(1, explicitLength, 2);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full);
            CollectionAssert.AreEqual(new[] { 1, 2 }, partial);
            CollectionAssert.AreEqual(new[] { 2, 3 }, offset);
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, explicitLength);

            Assert.IsTrue(readOnly.TryCopyTo(full));
            Assert.IsTrue(readOnly.TryCopyTo(partial, 2));
            Assert.IsTrue(readOnly.TryCopyTo(1, offset));
            Assert.IsTrue(readOnly.TryCopyTo(1, explicitLength, 2));
            Assert.IsFalse(readOnly.TryCopyTo(3, partial));
        }

        [Test]
        public void Enumerator_VisitsValuesInOrder()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var visited = new int[list.Count];
            var index = 0;

            foreach (var value in list)
            {
                visited[index++] = value;
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, visited);
        }

        [Test]
        public void OwnerAndReadOnlyEnumerators_MoveResetAndDisposeDirectly()
        {
            using var list = new ListNative<int>(new[] { 1, 2 }, Allocator.Temp);
            var ownerEnumerator = list.GetEnumerator();

            Assert.IsTrue(ownerEnumerator.MoveNext());
            Assert.AreEqual(1, ownerEnumerator.Current);
            ownerEnumerator.Reset();
            Assert.IsTrue(ownerEnumerator.MoveNext());
            Assert.AreEqual(1, ownerEnumerator.Current);
            ownerEnumerator.Dispose();

            var readOnlyEnumerator = list.AsReadOnly().GetEnumerator();

            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(1, readOnlyEnumerator.Current);
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            Assert.AreEqual(2, readOnlyEnumerator.Current);
            Assert.IsFalse(readOnlyEnumerator.MoveNext());
            readOnlyEnumerator.Reset();
            Assert.IsTrue(readOnlyEnumerator.MoveNext());
            readOnlyEnumerator.Dispose();
        }

        [Test]
        public void Enumerator_MutationDuringIteration_Throws()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var enumerator = list.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());

            list.Add(4);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var list = new ListNative<int>(4, Allocator.Temp);
            Assert.IsTrue(list.IsCreated);

            list.Dispose();

            Assert.IsFalse(list.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void UseAfterDispose_Throws()
        {
            var list = new ListNative<int>(4, Allocator.Temp);
            list.Add(1);
            list.Dispose();

            Assert.Catch(() => _ = list.Count);
        }

        [Test]
        public void DoubleDispose_DoesNotThrow()
        {
            var list = new ListNative<int>(4, Allocator.Temp);
            list.Dispose();

            Assert.DoesNotThrow(() => list.Dispose());
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var list = new ListNative<int>(4, Allocator.TempJob);
            list.Add(1);

            var handle = list.Dispose(default);

            Assert.IsFalse(list.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void Prefill_FactoriesCreateExpectedContent()
        {
            using var defaults = ListNative<int>.Prefill(3, Allocator.Temp);
            using var values = ListNative<int>.Prefill(7, 3, Allocator.Temp);

            CollectionAssert.AreEqual(new[] { 0, 0, 0 }, defaults.ToArray());
            CollectionAssert.AreEqual(new[] { 7, 7, 7 }, values.ToArray());
        }

        [Test]
        public void RemoveRange_ZeroLengthBoundaries_PinContract()
        {
            // Pins the family-wide contract: RemoveRange validates startIndex < Count before
            // looking at length, so a zero-length remove at Count (including on an empty list)
            // throws, unlike BCL List<T>.RemoveRange. Zero-length removes at a valid index
            // are no-ops.
            using var empty = new ListNative<int>(0, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => empty.RemoveRange(0, 0));

            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.RemoveRange(3, 0));

            list.RemoveRange(0, 0);
            list.RemoveRange(2, 0);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAddsAndDisposes(Allocator allocator)
        {
            var list = new ListNative<int>(4, allocator);

            try
            {
                list.Add(10);

                Assert.IsTrue(list.IsCreated);
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(10, list[0]);
            }
            finally
            {
                list.Dispose();
            }

            Assert.IsFalse(list.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_InvalidArguments_Throw()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new ListNative<int>(4, default(AllocatorStrategy)));
            Assert.Throws<InvalidOperationException>(() => _ = new ListNative<int>(-1, Allocator.Temp));
            Assert.Throws<InvalidOperationException>(
                () => _ = new ListNative<int>(new[] { 1 }, default(AllocatorStrategy))
            );
        }

        [Test]
        public void SpanConstructor_EmptySource_CreatesEmptyListThatCanGrow()
        {
            using var list = new ListNative<int>(ReadOnlySpan<int>.Empty, Allocator.Temp);

            Assert.IsTrue(list.IsCreated);
            Assert.AreEqual(0, list.Count);

            list.Add(1);

            CollectionAssert.AreEqual(new[] { 1 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexerAndElementAt_OutOfRange_Throw()
        {
            using var empty = new ListNative<int>(4, Allocator.Temp);
            var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            try
            {
                Assert.Throws<InvalidOperationException>(() => _ = empty[0]);
                Assert.Throws<InvalidOperationException>(() => empty.ElementAt(0));
                Assert.Throws<InvalidOperationException>(() => _ = list[-1]);
                Assert.Throws<InvalidOperationException>(() => _ = list[3]);
                Assert.Throws<InvalidOperationException>(() => _ = list[int.MaxValue]);
                Assert.Throws<InvalidOperationException>(() => list[3] = 9);
                Assert.Throws<InvalidOperationException>(() => list.ElementAt(-1));
                Assert.Throws<InvalidOperationException>(() => list.ElementAt(3));
                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void ElementAt_ReferenceAliasesTheIndexerAndTheSpan()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            ref var viaElementAt = ref list.ElementAt(2);
            var span = list.AsSpan();

            viaElementAt = 30;

            Assert.AreEqual(30, list[2]);
            Assert.AreEqual(30, span[2]);

            span[1] = 20;

            Assert.AreEqual(20, list.ElementAt(1));
        }

        [Test]
        public void Insert_EveryPositionOnAFullList_MatchesListOfT()
        {
            const int COUNT = 5;

            for (var index = 0; index <= COUNT; index++)
            {
                var list = new ListNative<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Persistent);
                var expected = new List<int> { 1, 2, 3, 4, 5 };

                try
                {
                    list.Insert(index, 99);
                    expected.Insert(index, 99);

                    CollectionAssert.AreEqual(expected, list.ToArray(), $"insert {index}");

                    var value = 77;

                    list.Insert(index, in value);
                    expected.Insert(index, value);

                    CollectionAssert.AreEqual(expected, list.ToArray(), $"insert in {index}");
                }
                finally
                {
                    list.Dispose();
                }
            }
        }

        [Test]
        public void Insert_IntoEmptyList_AddsTheOnlyElement()
        {
            using var list = new ListNative<int>(0, Allocator.Temp);

            list.Insert(0, 7);

            CollectionAssert.AreEqual(new[] { 7 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Insert_IndexOutsideTheList_Throws()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var value = 9;

            Assert.Throws<InvalidOperationException>(() => list.Insert(4, 9));
            Assert.Throws<InvalidOperationException>(() => list.Insert(-1, 9));
            Assert.Throws<InvalidOperationException>(() => list.Insert(4, in value));
            Assert.Throws<InvalidOperationException>(() => list.Insert(-1, in value));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [Test]
        public void Add_InArgumentAliasingOwnStorageOnFullList_AppendsACopy()
        {
            var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

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
            var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

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
        public void AddRange_SpanOverOwnStorageOnFullList_AppendsACopy()
        {
            var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

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
            var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

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
        public void AddRange_ExactlyFillsThenGrows_KeepsEveryElement()
        {
            var list = new ListNative<int>(3, Allocator.Temp);

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
        public void AddRange_NativeSourcesOnAFullList_GrowAndKeepEveryElement()
        {
            using var native = new NativeArray<int>(new[] { 5, 6, 7 }, Allocator.Temp);
            var slice = new NativeSlice<int>(native, 1, 2);
            var list = new ListNative<int>(new[] { 1, 2 }, Allocator.Persistent);

            try
            {
                list.AddRange(native);
                list.AddRange(native, 1);
                list.AddRange(in slice);
                list.AddRange(in slice, 1);

                CollectionAssert.AreEqual(new[] { 1, 2, 5, 6, 7, 5, 6, 7, 6 }, list.ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_ZeroCountOrEmptyItems_ChangesNothing()
        {
            using var native = new NativeArray<int>(new[] { 5, 6 }, Allocator.Temp);
            using var emptyNative = new NativeArray<int>(0, Allocator.Temp);
            var slice = new NativeSlice<int>(native, 0, 2);
            using var list = new ListNative<int>(new[] { 1, 2 }, Allocator.Temp);

            list.AddRange(new[] { 9, 9 }, 0);
            list.AddRange(ReadOnlySpan<int>.Empty, 0);
            list.AddRange(native, 0);
            list.AddRange(emptyNative);
            list.AddRange(in slice, 0);

            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AddRange_CountOutsideItems_Throws()
        {
            using var native = new NativeArray<int>(new[] { 5, 6 }, Allocator.Temp);
            var slice = new NativeSlice<int>(native, 0, 2);
            using var list = new ListNative<int>(new[] { 1, 2 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.AddRange(new[] { 9, 9 }, 3));
            Assert.Throws<InvalidOperationException>(() => list.AddRange(new[] { 9, 9 }, -1));
            Assert.Throws<InvalidOperationException>(() => list.AddRange(native, 3));
            Assert.Throws<InvalidOperationException>(() => list.AddRange(native, -1));
            Assert.Throws<InvalidOperationException>(() => list.AddRange(in slice, 3));
            Assert.Throws<InvalidOperationException>(() => list.AddRange(in slice, -1));
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public void CopyFrom_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsContent()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
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
            using var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
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
        public void CopyTo_NativeDestinationsOutsideTheList_Throw()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            using var small = new NativeArray<int>(2, Allocator.Temp);
            using var sliceOwner = new NativeArray<int>(2, Allocator.Temp);
            var slice = new NativeSlice<int>(sliceOwner);

            Assert.Catch(() => list.CopyTo(0, small, 3));
            Assert.Catch(() => list.CopyTo(-1, small));
            Assert.Catch(() => list.CopyTo(3, small));
            Assert.Catch(() => list.CopyTo(3, in slice, 2));
            Assert.Catch(() => list.CopyTo(0, in slice, 3));
            CollectionAssert.AreEqual(new[] { 0, 0 }, small.ToArray());
            CollectionAssert.AreEqual(new[] { 0, 0 }, sliceOwner.ToArray());
        }

        [Test]
        public void CopyToAndCopyFrom_EmptyListAndEmptySpans_CopyNothing()
        {
            using var list = new ListNative<int>(4, Allocator.Temp);

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
            using var list = new ListNative<int>(seed, Allocator.Temp);

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
            using var list = new ListNative<int>(seed, Allocator.Temp);

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
        public void ClearAndFastClear_EmptyList_AreNoOpsThatKeepTheListUsable()
        {
            using var list = new ListNative<int>(0, Allocator.Temp);

            list.Clear();
            list.FastClear();

            Assert.IsTrue(list.IsCreated);
            Assert.AreEqual(0, list.Count);

            list.Add(1);

            CollectionAssert.AreEqual(new[] { 1 }, list.ToArray());
        }

        [Test]
        public void IncreaseCapacityTo_NotLargerThanCapacity_ReturnsTheCurrentCapacity()
        {
            using var list = new ListNative<int>(4, Allocator.Temp);
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
            using var list = new ListNative<int>(0, Allocator.Temp);

            var capacity = list.IncreaseCapacityTo(8);

            Assert.AreEqual(8, capacity);
            Assert.AreEqual(8, list.Capacity);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void IncreaseCapacityBy_AmountOverflowingInt_ThrowsOverflow()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<OverflowException>(() => list.IncreaseCapacityBy(int.MaxValue));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IncreaseCapacityBy_NegativeAmountBelowCount_Throws()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.IncreaseCapacityBy(-5));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void Trim_EmptyList_ReleasesTheCapacityAndStaysUsable()
        {
            using var list = new ListNative<int>(8, Allocator.Temp);

            list.Trim();

            Assert.IsTrue(list.IsCreated);
            Assert.AreEqual(0, list.Capacity);

            list.Add(3);

            CollectionAssert.AreEqual(new[] { 3 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void RemoveAtAndRemoveAtSwapBack_OutOfRange_Throw()
        {
            using var empty = new ListNative<int>(4, Allocator.Temp);
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => empty.RemoveAt(0));
            Assert.Throws<InvalidOperationException>(() => empty.RemoveAtSwapBack(0));
            Assert.Throws<InvalidOperationException>(() => list.RemoveAt(-1));
            Assert.Throws<InvalidOperationException>(() => list.RemoveAt(3));
            Assert.Throws<InvalidOperationException>(() => list.RemoveAtSwapBack(-1));
            Assert.Throws<InvalidOperationException>(() => list.RemoveAtSwapBack(3));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [Test]
        public void RemoveAtAndRemoveAtSwapBack_EveryIndex_MatchListOfT()
        {
            const int COUNT = 5;
            var list = new ListNative<int>(COUNT, Allocator.Temp);

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

                    list.RemoveAt(index);
                    expected.RemoveAt(index);

                    CollectionAssert.AreEqual(expected, list.ToArray(), $"remove {index}");

                    list.Clear();
                    expected.Clear();

                    for (var i = 1; i <= COUNT; i++)
                    {
                        list.Add(i);
                        expected.Add(i);
                    }

                    expected[index] = expected[COUNT - 1];
                    expected.RemoveAt(COUNT - 1);
                    list.RemoveAtSwapBack(index);

                    CollectionAssert.AreEqual(expected, list.ToArray(), $"swap back {index}");
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
            var list = new ListNative<int>(COUNT, Allocator.Temp);

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
        [TestRequiresCollectionChecks]
        public void RemoveRange_RangeOutsideTheList_Throws()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.RemoveRange(-1, 1));
            Assert.Throws<InvalidOperationException>(() => list.RemoveRange(2, 2));
            Assert.Throws<InvalidOperationException>(() => list.RemoveRange(0, 4));
            Assert.Throws<InvalidOperationException>(() => list.RemoveRange(0, -1));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [Test]
        public void AsSpanAsReadOnlySpanAndToArray_EmptyList_AreEmpty()
        {
            using var list = new ListNative<int>(4, Allocator.Temp);

            Assert.AreEqual(0, list.AsSpan().Length);
            Assert.AreEqual(0, list.AsReadOnlySpan().Length);
            Assert.AreEqual(0, list.ToArray().Length);
            Assert.AreEqual(0, list.AsNativeSlice().Length);
        }

        [Test]
        public void AsNativeSlice_CountBelowCapacity_LengthIsCountAndAliasesTheList()
        {
            using var list = new ListNative<int>(8, Allocator.Temp);
            list.AddRange(new[] { 1, 2, 3 });

            var slice = list.AsNativeSlice();

            Assert.AreEqual(3, slice.Length);

            slice[1] = 20;
            list.ElementAt(2) = 30;

            Assert.AreEqual(20, list[1]);
            Assert.AreEqual(30, slice[2]);
            CollectionAssert.AreEqual(new[] { 1, 20, 30 }, slice.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AsNativeSlice_UsedAfterOwnerDispose_Throws()
        {
            var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var slice = list.AsNativeSlice();

            list.Dispose();

            Assert.Catch(() => _ = slice[0]);
        }

        [Test]
        public void AddReplicate_FullList_GrowsAndKeepsEveryElement()
        {
            var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                var filled = list.AddReplicate(9, 2);
                var zeroed = list.AddReplicate(1);

                Assert.AreEqual(2, filled.Length);
                Assert.AreEqual(1, zeroed.Length);
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 9, 9, 0 }, list.ToArray());

                zeroed[0] = 5;

                Assert.AreEqual(5, list[6]);
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddReplicate_ZeroCapacityList_AddsTheRequestedAmount()
        {
            using var list = new ListNative<int>(0, Allocator.Temp);

            var values = list.AddReplicate(5, 3);

            CollectionAssert.AreEqual(new[] { 5, 5, 5 }, values.ToArray());

            var range = list.AddReplicateNoInit(2);
            range[0] = 4;
            range[1] = 8;

            CollectionAssert.AreEqual(new[] { 5, 5, 5, 4, 8 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AddReplicate_ZeroOrNegativeAmount_Throws()
        {
            using var list = new ListNative<int>(new[] { 1, 2 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.AddReplicate(0));
            Assert.Throws<InvalidOperationException>(() => list.AddReplicate(-1));
            Assert.Throws<InvalidOperationException>(() => list.AddReplicate(1, 0));
            Assert.Throws<InvalidOperationException>(() => list.AddReplicate(1, -1));
            Assert.Throws<InvalidOperationException>(() => list.AddReplicateNoInit(0));
            Assert.Throws<InvalidOperationException>(() => list.AddReplicateNoInit(-1));
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public void Reinterpret_EmptyList_ViewIsEmptyAndShareTheOwner()
        {
            using var list = new ListNative<int>(4, Allocator.Temp);

            var view = list.Reinterpret<uint>();

            Assert.IsTrue(view.IsCreated);
            Assert.AreEqual(0, view.Count);

            view.Add(7u);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(7, list[0]);
        }

        [Test]
        public void Reinterpret_WriteThroughTheView_IsSeenByTheOwnerForFloatAndWideElements()
        {
            using var floats = new ListNative<int>(new[] { 0, 0, 0 }, Allocator.Temp);
            using var wides = new ListNative<Wide>(new Wide[2], Allocator.Temp);

            // The views are never disposed: they share the owners' allocations, so only the owners release them.
            var floatView = floats.Reinterpret<float>();
            var wideView = wides.Reinterpret<WideOther>();

            floatView[1] = 1.5f;
            wides.ElementAt(1) = new Wide { a = 1, b = -2, c = long.MaxValue };

            Assert.AreEqual(BitConverter.SingleToInt32Bits(1.5f), floats[1]);
            Assert.AreEqual(1L, wideView[1].x);
            Assert.AreEqual(-2L, wideView[1].y);
            Assert.AreEqual(long.MaxValue, wideView[1].z);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_DifferentSize_Throws()
        {
            using var list = new ListNative<int>(new[] { 1, 2 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => list.Reinterpret<byte>());
            Assert.Throws<InvalidOperationException>(() => list.Reinterpret<long>());
            Assert.Throws<InvalidOperationException>(() => list.Reinterpret<Rgb>());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_ViewUsedAfterOwnerDispose_Throws()
        {
            var list = new ListNative<int>(new[] { 1, 2 }, Allocator.Persistent);
            var view = list.Reinterpret<uint>();

            list.Dispose();

            Assert.Catch(() => _ = view.Count);
            Assert.Catch(() => _ = view[0]);
        }

        [Test]
        public void GetEnumerator_EmptyList_YieldsNothingForEveryEnumeratorForm()
        {
            using var list = new ListNative<int>(4, Allocator.Temp);

            var structEnumerator = list.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)list).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)list).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [Test]
        public void Enumerator_Exhausted_StaysFinishedUntilReset()
        {
            using var list = new ListNative<int>(new[] { 1, 2 }, Allocator.Temp);
            var enumerator = list.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
        }

        [Test]
        public void Enumerator_ModifiedList_MoveNextAndResetThrow()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var enumerator = list.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            list.Clear();

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
        }

        [Test]
        public unsafe void Enumerator_ConstructedFromDefaultReadOnly_Throws()
        {
            ListNative<int>.ReadOnly notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new ListNative<int>.Enumerator(null, in notCreated));
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
        public void Members_AfterOwnerDispose_Throw()
        {
            var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var native = new NativeArray<int>(new[] { 4, 5 }, Allocator.Persistent);

            try
            {
                var slice = new NativeSlice<int>(native, 0, 2);
                var value = 9;

                list.Dispose();

                Assert.Catch(() => _ = list.Count);
                Assert.Catch(() => _ = list.Capacity);
                Assert.Catch(() => _ = list[0]);
                Assert.Catch(() => list[0] = 1);
                Assert.Catch(() => list.ElementAt(0));
                Assert.Catch(() => list.Add(1));
                Assert.Catch(() => list.Add(in value));
                Assert.Catch(() => list.Insert(0, 1));
                Assert.Catch(() => list.Insert(0, in value));
                Assert.Catch(() => list.AddRange(new[] { 1 }));
                Assert.Catch(() => list.AddRange(new[] { 1 }, 1));
                Assert.Catch(() => list.AddRange(native));
                Assert.Catch(() => list.AddRange(native, 1));
                Assert.Catch(() => list.AddRange(in slice));
                Assert.Catch(() => list.AddRange(in slice, 1));
                Assert.Catch(() => list.CopyFrom(new[] { 1 }));
                Assert.Catch(() => list.CopyFrom(new[] { 1 }, 1));
                Assert.Catch(() => list.CopyFrom(0, new[] { 1 }));
                Assert.Catch(() => list.CopyFrom(0, new[] { 1 }, 1));
                Assert.Catch(() => list.TryCopyFrom(new[] { 1 }));
                Assert.Catch(() => list.TryCopyFrom(new[] { 1 }, 1));
                Assert.Catch(() => list.TryCopyFrom(0, new[] { 1 }));
                Assert.Catch(() => list.TryCopyFrom(0, new[] { 1 }, 1));
                Assert.Catch(() => list.CopyTo(new int[3], 0));
                Assert.Catch(() => list.CopyTo(new int[3]));
                Assert.Catch(() => list.CopyTo(new int[3], 1));
                Assert.Catch(() => list.CopyTo(0, new int[3]));
                Assert.Catch(() => list.CopyTo(0, new int[3], 1));
                Assert.Catch(() => list.TryCopyTo(new int[3]));
                Assert.Catch(() => list.TryCopyTo(0, new int[3], 1));
                Assert.Catch(() => list.CopyTo(native));
                Assert.Catch(() => list.CopyTo(0, native, 1));
                Assert.Catch(() => list.CopyTo(0, in slice));
                Assert.Catch(() => list.CopyTo(0, in slice, 1));
                Assert.Catch(() => list.Clear());
                Assert.Catch(() => list.FastClear());
                Assert.Catch(() => list.RemoveAt(0));
                Assert.Catch(() => list.RemoveRange(0, 1));
                Assert.Catch(() => list.RemoveAtSwapBack(0));
                Assert.Catch(() => list.ToArray());
                Assert.Catch(() => list.AsSpan());
                Assert.Catch(() => list.AsReadOnlySpan());
                Assert.Catch(() => list.AsNativeSlice());
                Assert.Catch(() => list.AddReplicate(1));
                Assert.Catch(() => list.AddReplicate(1, 1));
                Assert.Catch(() => list.AddReplicateNoInit(1));
                Assert.Catch(() => list.IncreaseCapacityBy(1));
                Assert.Catch(() => list.IncreaseCapacityTo(8));
                Assert.Catch(() => list.Trim());
                Assert.Catch(() => list.GetEnumerator());
                Assert.Catch(() => list.AsReadOnly());
                Assert.Catch(() => list.Reinterpret<uint>());
            }
            finally
            {
                native.Dispose();
            }
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_AcceptedAllocator_MarksNotCreatedAndIsRepeatable(Allocator allocator)
        {
            var list = new ListNative<int>(4, allocator);
            list.Add(1);

            list.Dispose();

            Assert.IsFalse(list.IsCreated);
            Assert.DoesNotThrow(() => list.Dispose());
        }

        [Test]
        public void Dispose_DefaultList_IsNoOp()
        {
            ListNative<int> list = default;

            Assert.DoesNotThrow(() => list.Dispose());

            var handle = list.Dispose(default);

            Assert.DoesNotThrow(() => handle.Complete());
            Assert.IsFalse(list.IsCreated);
        }

        [Test]
        public void DisposeJob_PersistentAllocator_CompletesAndMarksNotCreated()
        {
            var list = new ListNative<int>(4, Allocator.Persistent);
            list.Add(1);

            var handle = list.Dispose(default);

            Assert.IsFalse(list.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void NativeDispose_NullHeader_IsNoOp()
        {
            Assert.DoesNotThrow(() => default(ListNativeDispose).Dispose());
        }

        [Test]
        public unsafe void NativeDispose_AllocatedHeader_ReleasesIt()
        {
            var data = ListUnsafe<int>.Alloc(4, Allocator.Persistent);
            data->Add(1);

            Assert.AreEqual(1, data->Count);

            var dispose = new ListNativeDispose { m_Data = data };
            dispose.Dispose();
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Prefill_AcceptedAllocator_CreatesFilledList(Allocator allocator)
        {
            var defaults = ListNative<int>.Prefill(2, allocator);
            var values = ListNative<int>.Prefill(7, 2, allocator);

            try
            {
                CollectionAssert.AreEqual(new[] { 0, 0 }, defaults.ToArray());
                CollectionAssert.AreEqual(new[] { 7, 7 }, values.ToArray());
            }
            finally
            {
                defaults.Dispose();
                values.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Prefill_ZeroAmount_Throws()
        {
            // AddReplicate(0) rejects the amount after the list was created, so the product leaks the created list
            // (suspected defect D-21). Allocator.Temp is used so that Unity releases it without a leak report.
            Assert.Throws<InvalidOperationException>(() => ListNative<int>.Prefill(0, Allocator.Temp));
            Assert.Throws<InvalidOperationException>(() => ListNative<int>.Prefill(5, 0, Allocator.Temp));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Prefill_NegativeAmountOrInvalidAllocator_ThrowsBeforeCreatingTheList()
        {
            Assert.Throws<InvalidOperationException>(() => ListNative<int>.Prefill(-1, Allocator.Temp));
            Assert.Throws<InvalidOperationException>(() => ListNative<int>.Prefill(2, default(AllocatorStrategy)));
            Assert.Throws<InvalidOperationException>(() => ListNative<int>.Prefill(5, 2, default(AllocatorStrategy)));
        }

        [Test]
        public void ReadOnly_Constructor_FromDefaultList_Throws()
        {
            ListNative<int> notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new ListNative<int>.ReadOnly(notCreated));
            Assert.Throws<ArgumentException>(() => notCreated.AsReadOnly());
            Assert.Throws<ArgumentException>(() => _ = (ListNative<int>.ReadOnly)notCreated);
        }

        [Test]
        public void ReadOnly_EmptyList_ReturnsEmptyResults()
        {
            using var list = new ListNative<int>(4, Allocator.Temp);
            var readOnly = list.AsReadOnly();
            var enumerator = readOnly.GetEnumerator();

            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, readOnly.Count);
            Assert.AreEqual(0, readOnly.AsReadOnlySpan().Length);
            Assert.AreEqual(0, readOnly.ToArray().Length);
            Assert.IsTrue(readOnly.TryCopyTo(Span<int>.Empty));
            Assert.IsFalse(readOnly.TryCopyTo(new int[1]));
            Assert.DoesNotThrow(() => readOnly.CopyTo(Span<int>.Empty));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnly_IndexerOutOfRange_Throws()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var readOnly = list.AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => _ = readOnly[-1]);
            Assert.Throws<InvalidOperationException>(() => _ = readOnly[3]);
            Assert.Throws<InvalidOperationException>(() => _ = readOnly[int.MaxValue]);
        }

        [Test]
        public void ReadOnly_CopyToInvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            using var list = new ListNative<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var readOnly = list.AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(-1, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(5, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(0, destination, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(0, new int[2], 3));
            Assert.IsFalse(readOnly.TryCopyTo(-1, destination));
            Assert.IsFalse(readOnly.TryCopyTo(0, destination, 5));
            Assert.IsFalse(readOnly.TryCopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void ReadOnly_CopyToDestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var list = new ListNative<int>(seed, Allocator.Temp);
            var readOnly = list.AsReadOnly();

            readOnly.CopyTo(2, list.AsSpan()[..4]);

            var expected = (int[])seed.Clone();
            Array.Copy(expected, 2, expected, 0, 4);
            CollectionAssert.AreEqual(expected, list.ToArray());
        }

        [Test]
        public void ReadOnly_ElementShapes_RoundTripBitExact()
        {
            using var small = new ListNative<Rgb>(new[] { new Rgb { r = 1, g = 2, b = 3 } }, Allocator.Temp);
            using var wide = new ListNative<Wide>(
                  new[] { new Wide { a = long.MinValue, b = 0, c = long.MaxValue } }
                , Allocator.Temp
            );
            var smallView = small.AsReadOnly();
            var wideView = wide.AsReadOnly();

            Assert.AreEqual(1, smallView.AsReadOnlySpan().Length);
            Assert.AreEqual(3, smallView[0].b);
            Assert.AreEqual(2, smallView.AsReadOnlySpan()[0].g);
            Assert.AreEqual(long.MinValue, wideView[0].a);
            Assert.AreEqual(long.MaxValue, wideView.AsReadOnlySpan()[0].c);

            var copy = new Wide[1];
            wideView.CopyTo(copy);

            Assert.AreEqual(long.MaxValue, copy[0].c);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnly_Members_AfterOwnerDispose_Throw()
        {
            var list = new ListNative<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var readOnly = list.AsReadOnly();

            list.Dispose();

            Assert.Catch(() => _ = readOnly.Count);
            Assert.Catch(() => _ = readOnly.Capacity);
            Assert.Catch(() => _ = readOnly[0]);
            Assert.Catch(() => readOnly.AsReadOnlySpan());
            Assert.Catch(() => readOnly.ToArray());
            Assert.Catch(() => readOnly.CopyTo(new int[3]));
            Assert.Catch(() => readOnly.TryCopyTo(new int[3]));
            Assert.Catch(() => readOnly.GetEnumerator());
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var list = new ListNative<T>(1, Allocator.Temp);

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
