using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from Unity.Collections.Tests/NativeListTests.cs (list subset) against the
    // managed SharedList owner.
    public partial class SharedListTests
    {
        [Test]
        public void Constructor_IsEmpty()
        {
            using var list = new SharedList<int>(8);

            Assert.AreEqual(0, list.Count);
            Assert.GreaterOrEqual(list.Capacity, 8);
        }

        [Test]
        public void ConvenienceConstructors_CopySourcesAndReserveExtraCapacity()
        {
            var array = new[] { 1, 2, 3 };
            var segment = new ArraySegment<int>(new[] { 0, 4, 5, 0 }, 1, 2);
            ReadOnlySpan<int> span = new[] { 6, 7 };
            ICollection<int> collection = new List<int> { 8, 9 };
            using var native = new NativeArray<int>(new[] { 10, 11 }, Allocator.Temp);
            using var sliceOwner = new NativeArray<int>(new[] { 0, 12, 13, 0 }, Allocator.Temp);
            var slice = new NativeSlice<int>(sliceOwner, 1, 2);
            using var empty = new SharedList<int>();
            using var fromArray = new SharedList<int>(array);
            using var fromSegment = new SharedList<int>(in segment);
            using var fromSpan = new SharedList<int>(in span);
            using var fromNative = new SharedList<int>(native);
            using var fromSlice = new SharedList<int>(in slice);
            using var fromCollection = new SharedList<int>(collection);
            using var withExtraCapacity = new SharedList<int>(collection, 5);
            using var fromShared = new SharedList<int>(fromArray);

            Assert.AreEqual(0, empty.Count);
            CollectionAssert.AreEqual(array, fromArray.ToArray());
            CollectionAssert.AreEqual(new[] { 4, 5 }, fromSegment.ToArray());
            CollectionAssert.AreEqual(new[] { 6, 7 }, fromSpan.ToArray());
            CollectionAssert.AreEqual(new[] { 10, 11 }, fromNative.ToArray());
            CollectionAssert.AreEqual(new[] { 12, 13 }, fromSlice.ToArray());
            CollectionAssert.AreEqual(new[] { 8, 9 }, fromCollection.ToArray());
            CollectionAssert.AreEqual(array, fromShared.ToArray());
            Assert.GreaterOrEqual(withExtraCapacity.Capacity, collection.Count + 5);
        }

        [Test]
        public void GenericOwnerConstructors_CopyManagedAndNativeSources()
        {
            var array = new[] { 1, 2 };
            var segment = new ArraySegment<int>(new[] { 0, 3, 4 }, 1, 2);
            ReadOnlySpan<int> span = new[] { 5, 6 };
            ICollection<int> collection = new List<int> { 7, 8 };
            using var native = new NativeArray<uint>(new uint[] { 9, 10 }, Allocator.Temp);
            using var sliceOwner = new NativeArray<uint>(new uint[] { 0, 11, 12 }, Allocator.Temp);
            var slice = new NativeSlice<uint>(sliceOwner, 1, 2);
            using var empty = new SharedList<int, uint>();
            using var capacity = new SharedList<int, uint>(4);
            using var fromArray = new SharedList<int, uint>(array);
            using var fromSegment = new SharedList<int, uint>(in segment);
            using var fromSpan = new SharedList<int, uint>(in span);
            using var fromCollection = new SharedList<int, uint>(collection);
            using var withExtraCapacity = new SharedList<int, uint>(collection, 3);
            using var fromNative = new SharedList<int, uint>(native);
            using var fromSlice = new SharedList<int, uint>(in slice);

            Assert.AreEqual(0, empty.Count);
            Assert.GreaterOrEqual(capacity.Capacity, 4);
            CollectionAssert.AreEqual(array, fromArray.ToArray());
            CollectionAssert.AreEqual(new[] { 3, 4 }, fromSegment.ToArray());
            CollectionAssert.AreEqual(new[] { 5, 6 }, fromSpan.ToArray());
            CollectionAssert.AreEqual(new[] { 7, 8 }, fromCollection.ToArray());
            Assert.GreaterOrEqual(withExtraCapacity.Capacity, collection.Count + 3);
            CollectionAssert.AreEqual(new[] { 9, 10 }, fromNative.ToArray());
            CollectionAssert.AreEqual(new[] { 11, 12 }, fromSlice.ToArray());
        }

        [Test]
        public void Add_StoresValues()
        {
            using var list = new SharedList<int>(8);

            list.Add(10);
            list.Add(20);
            list.Add(30);

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(10, list[0]);
            Assert.AreEqual(20, list[1]);
            Assert.AreEqual(30, list[2]);
        }

        [Test]
        public void ValueAndInOverloads_AddAndInsertExpectedValues()
        {
            using var list = new SharedList<int, int>(4);
            var two = 2;
            var four = 4;

            list.Add(1);
            list.Add(in two);
            list.Insert(2, 3);
            list.Insert(3, in four);

            Assert.IsFalse(list.IsReadOnly);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void AddRange_AllOverloadsAppendRequestedValues()
        {
            using var list = new SharedList<int, int>(2);
            ReadOnlySpan<int> fullSpan = new[] { 5, 6 };
            ReadOnlySpan<int> partialSpan = new[] { 7, 8, 99 };
            IEnumerable<int> collection = new List<int> { 9, 10 };
            var iterator = YieldValues();

            list.AddRange(new[] { 1, 2 });
            list.AddRange(new[] { 3, 4, 99 }, 2);
            list.AddRange(fullSpan);
            list.AddRange(partialSpan, 2);
            list.AddRange(collection);
            list.AddRange(iterator);

            CollectionAssert.AreEqual(
                new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                list.ToArray()
            );
        }

        [Test]
        public void Add_GrowsPastInitialCapacity()
        {
            const int N = 200;
            using var list = new SharedList<int>(4);

            for (var i = 0; i < N; i++)
            {
                list.Add(i * 2);
            }

            Assert.AreEqual(N, list.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.AreEqual(i * 2, list[i], $"index {i}");
            }
        }

        [Test]
        public void Indexer_Set_Overwrites()
        {
            using var list = new SharedList<int>(4);
            list.Add(1);
            list.Add(2);

            list[1] = 99;

            Assert.AreEqual(99, list[1]);
        }

        [Test]
        public void Insert_ShiftsRight()
        {
            using var list = new SharedList<int>(8);
            list.Add(1);
            list.Add(3);

            list.Insert(1, 2);

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        [Test]
        public void RemoveAt_ShiftsLeft()
        {
            using var list = new SharedList<int>(8);
            list.Add(1);
            list.Add(2);
            list.Add(3);

            list.RemoveAt(1);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(3, list[1]);
        }

        [Test]
        public void Remove_RemovesFirstMatch()
        {
            using var list = new SharedList<int>(8);
            list.Add(1);
            list.Add(2);
            list.Add(3);

            Assert.IsTrue(list.Remove(2));
            Assert.IsFalse(list.Remove(99));

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(3, list[1]);
        }

        [Test]
        public void IndexOf_FindsValue()
        {
            using var list = new SharedList<int>(8);
            list.Add(10);
            list.Add(20);
            list.Add(30);

            Assert.AreEqual(1, list.IndexOf(20));
            Assert.AreEqual(-1, list.IndexOf(99));
        }

        [Test]
        public void SearchAndRemovalOverloads_FindAndRemoveExpectedValues()
        {
            using var list = new SharedList<int, int>(new[] { 1, 2, 3, 2, 4 });
            var two = 2;
            var missing = 9;

            Assert.IsTrue(list.Contains(2));
            Assert.IsFalse(list.Contains(missing));
            Assert.AreEqual(3, list.IndexOf(2, 2));
            Assert.AreEqual(3, list.IndexOf(2, 2, 2));
            Assert.AreEqual(1, list.IndexOf(in two));
            Assert.AreEqual(3, list.IndexOf(in two, 2));
            Assert.AreEqual(3, list.IndexOf(in two, 2, 2));
            Assert.AreEqual(-1, list.IndexOf(in missing));
            Assert.IsTrue(list.Remove(in two));

            list.RemoveRange(1, 2);
            list.AddRange(new[] { 5, 6 });
            list.RemoveAtSwapBack(1);

            CollectionAssert.AreEqual(new[] { 1, 6, 5 }, list.ToArray());
        }

        [Test]
        public void ElementAt_ReturnsRef()
        {
            using var list = new SharedList<int>(8);
            list.Add(5);

            // SAFETY: list remains live and stable while the borrowed element reference is used.
            unsafe
            {
                ref var slot = ref list.ElementAt(0);
                slot = 55;
            }

            Assert.AreEqual(55, list[0]);
        }

        [Test]
        public void CopyFromAndTryCopyFrom_AllOverloadsCopyOrReturnFalse()
        {
            using var list = SharedList<int, int>.Prefill(0, 6);

            list.CopyFrom(new[] { 1, 2 });
            list.CopyFrom(new[] { 3, 4, 99 }, 2);
            list.CopyFrom(2, new[] { 5, 6 });
            list.CopyFrom(4, new[] { 7, 8, 99 }, 2);

            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6, 7, 8 }, list.ToArray());

            Assert.IsTrue(list.TryCopyFrom(new[] { 9, 10 }));
            Assert.IsTrue(list.TryCopyFrom(new[] { 11, 12, 99 }, 2));
            Assert.IsTrue(list.TryCopyFrom(2, new[] { 13, 14 }));
            Assert.IsTrue(list.TryCopyFrom(4, new[] { 15, 16, 99 }, 2));
            Assert.IsFalse(list.TryCopyFrom(5, new[] { 17, 18 }));

            CollectionAssert.AreEqual(new[] { 11, 12, 13, 14, 15, 16 }, list.ToArray());
        }

        [Test]
        public void CopyToAndTryCopyTo_AllOverloadsCopyOrReturnFalse()
        {
            using var list = new SharedList<int, int>(new[] { 1, 2, 3, 4 });
            var array = new int[6];
            var full = new int[4];
            var partial = new int[2];
            var offset = new int[2];
            var explicitLength = new int[3];

            list.CopyTo(array, 1);
            list.CopyTo(full);
            list.CopyTo(partial.AsSpan(), 2);
            list.CopyTo(1, offset);
            list.CopyTo(1, explicitLength, 2);

            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 0 }, array);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full);
            CollectionAssert.AreEqual(new[] { 1, 2 }, partial);
            CollectionAssert.AreEqual(new[] { 2, 3 }, offset);
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, explicitLength);

            Assert.IsTrue(list.TryCopyTo(full));
            Assert.IsTrue(list.TryCopyTo(partial, 2));
            Assert.IsTrue(list.TryCopyTo(1, offset));
            Assert.IsTrue(list.TryCopyTo(1, explicitLength, 2));
            Assert.IsFalse(list.TryCopyTo(3, partial));
        }

        [Test]
        public void Clear_ResetsCount()
        {
            using var list = new SharedList<int>(8);
            list.Add(1);
            list.Add(2);

            list.Clear();

            Assert.AreEqual(0, list.Count);

            list.Add(7);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(7, list[0]);
        }

        [Test]
        public void AsSpan_ReflectsContent()
        {
            using var list = new SharedList<int>(8);
            list.Add(1);
            list.Add(2);
            list.Add(3);

            // SAFETY: list remains live and unmodified while the borrowed span is inspected.
            unsafe
            {
                var span = list.AsSpan();

                Assert.AreEqual(3, span.Length);
                Assert.AreEqual(1, span[0]);
                Assert.AreEqual(3, span[2]);
            }
        }

        [Test]
        public void CapacityReplicateViewsAndFactoriesExposeExpectedValues()
        {
            using var list = new SharedList<int, int>(4);
            list.AddRange(new[] { 1, 2 });
            var initialCapacity = list.Capacity;

            list.IncreaseCapacityBy(3);
            Assert.GreaterOrEqual(list.Capacity, initialCapacity + 3);

            var increasedCapacity = list.Capacity;
            list.IncreaseCapacityTo(increasedCapacity + 3);
            Assert.GreaterOrEqual(list.Capacity, increasedCapacity + 3);

            // SAFETY: Each borrowed span/reference is consumed before the next list mutation.
            unsafe
            {
                var defaults = list.AddReplicate(2);
                CollectionAssert.AreEqual(new[] { 0, 0 }, defaults.ToArray());

                var values = list.AddReplicate(7, 2);
                CollectionAssert.AreEqual(new[] { 7, 7 }, values.ToArray());

                var noInit = list.AddReplicateNoInit(1);
                noInit[0] = 8;
                Assert.AreEqual(8, list[^1]);
                list.RemoveAt(list.Count - 1);
            }
            Assert.AreEqual(6, list.Count);

            // SAFETY: list remains live and unmodified while both borrowed spans are used.
            unsafe
            {
                var readOnlySpan = list.AsReadOnlySpan();
                var writableSpan = list.AsSpan();
                writableSpan[0] = 10;

                Assert.AreEqual(10, readOnlySpan[0]);
            }

            list.Trim();

            Assert.AreEqual(list.Count, list.Capacity);

            using var defaultsList = SharedList<int, int>.Prefill(2);
            using var valuesList = SharedList<int, int>.Prefill(9, 2);

            CollectionAssert.AreEqual(new[] { 0, 0 }, defaultsList.ToArray());
            CollectionAssert.AreEqual(new[] { 9, 9 }, valuesList.ToArray());
        }

        [Test]
        public void ReadOnly_ConstructorConversionsCopiesViewsAndEnumeratorsExposeOwnerValues()
        {
            using var list = new SharedList<int, int>(new[] { 1, 2, 3, 4 });

            // SAFETY: list remains alive and unmodified while all borrowed aliases and enumerators are used.
            unsafe
            {
                var direct = new SharedList<int, int>.ReadOnly(list);
                SharedList<int, int>.ReadOnly implicitReadOnly = list;
                ReadOnlySpan<int> implicitSpan = implicitReadOnly;
                var empty = SharedList<int, int>.ReadOnly.Empty;
                var array = new int[6];
                var full = new int[4];
                var partial = new int[2];
                var offset = new int[2];
                var explicitLength = new int[3];

                Assert.IsTrue(direct.IsCreated);
                Assert.AreEqual(4, direct.Count);
                Assert.GreaterOrEqual(direct.Capacity, 4);
                Assert.IsTrue(direct.IsReadOnly);
                Assert.AreEqual(3, direct[2]);
                Assert.AreEqual(0, empty.Count);
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, implicitSpan.ToArray());
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, direct.AsReadOnlySpan().ToArray());
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, direct.ToArray());

                direct.CopyTo(array, 1);
                direct.CopyTo(full);
                direct.CopyTo(partial.AsSpan(), 2);
                direct.CopyTo(1, offset);
                direct.CopyTo(1, explicitLength, 2);

                CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 0 }, array);
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full);
                CollectionAssert.AreEqual(new[] { 1, 2 }, partial);
                CollectionAssert.AreEqual(new[] { 2, 3 }, offset);
                CollectionAssert.AreEqual(new[] { 2, 3, 0 }, explicitLength);

                Assert.IsTrue(direct.TryCopyTo(full));
                Assert.IsTrue(direct.TryCopyTo(partial, 2));
                Assert.IsTrue(direct.TryCopyTo(1, offset));
                Assert.IsTrue(direct.TryCopyTo(1, explicitLength, 2));
                Assert.IsFalse(direct.TryCopyTo(3, partial));

                var reinterpreted = direct.Reinterpret<uint>();
                var nativeReadOnly = direct.AsNative();

                Assert.AreEqual(1u, reinterpreted[0]);
                Assert.AreEqual(4, nativeReadOnly.Count);

                var ownerEnumerator = list.GetEnumerator();
                Assert.IsTrue(ownerEnumerator.MoveNext());
                Assert.AreEqual(1, ownerEnumerator.Current);
                ownerEnumerator.Reset();
                Assert.IsTrue(ownerEnumerator.MoveNext());
                ownerEnumerator.Dispose();

                var readOnlyEnumerator = direct.GetEnumerator();
                Assert.IsTrue(readOnlyEnumerator.MoveNext());
                Assert.AreEqual(1, readOnlyEnumerator.Current);
                readOnlyEnumerator.Reset();
                Assert.IsTrue(readOnlyEnumerator.MoveNext());
                readOnlyEnumerator.Dispose();

                var directEnumerator = new SharedList<int, int>.Enumerator(direct);
                Assert.IsTrue(directEnumerator.MoveNext());
                directEnumerator.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Dispose_InvalidatesExistingReadOnlyAndNativeViews()
        {
            var list = new SharedList<int, int>(new[] { 1, 2 });

            // SAFETY: This test intentionally retains borrowed aliases across owner disposal to verify checks.
            unsafe
            {
                var readOnly = list.AsReadOnly();
                SharedListNative<int> native = list;

                list.Dispose();

                Assert.IsTrue(readOnly.IsCreated);
                Assert.Catch(() => _ = readOnly.Count);
                Assert.Catch(() => _ = native.Count);
                Assert.DoesNotThrow(() => list.Dispose());
            }
        }

        [Test]
        public void RemoveRange_ZeroLengthBoundaries_PinContract()
        {
            // Pins the family-wide contract: RemoveRange validates startIndex < Count before
            // looking at length, so a zero-length remove at Count (including on an empty list)
            // throws, unlike BCL List<T>.RemoveRange. Zero-length removes at a valid index
            // are no-ops.
            using var empty = new SharedList<int>(4);

            Assert.Throws<InvalidOperationException>(() => empty.RemoveRange(0, 0));

            using var list = new SharedList<int>(new[] { 1, 2, 3 });

            Assert.Throws<InvalidOperationException>(() => list.RemoveRange(3, 0));

            list.RemoveRange(0, 0);
            list.RemoveRange(2, 0);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Indexer_OutOfRange_Throws()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3 });
            using var empty = new SharedList<int>();

            Assert.Throws<InvalidOperationException>(() => _ = list[-1]);
            Assert.Throws<InvalidOperationException>(() => _ = list[3]);
            Assert.Throws<InvalidOperationException>(() => _ = list[int.MaxValue]);
            Assert.Throws<InvalidOperationException>(() => list[-1] = 9);
            Assert.Throws<InvalidOperationException>(() => list[3] = 9);
            Assert.Throws<InvalidOperationException>(() => _ = empty[0]);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [Test]
        public void Prefill_ZeroAmount_ReturnsAnEmptyListThatStillGrows()
        {
            using var defaults = SharedList<int, int>.Prefill(0);
            using var values = SharedList<int, int>.Prefill(7, 0);

            Assert.AreEqual(0, defaults.Count);
            Assert.AreEqual(0, values.Count);

            defaults.Add(1);
            values.Add(2);

            CollectionAssert.AreEqual(new[] { 1 }, defaults.ToArray());
            CollectionAssert.AreEqual(new[] { 2 }, values.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Prefill_NegativeAmount_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => SharedList<int, int>.Prefill(-1));
            Assert.Throws<InvalidOperationException>(() => SharedList<int, int>.Prefill(7, -1));
        }

        [Test]
        public void Dispose_ZeroCapacityList_CanBeDisposedTwice()
        {
            var list = new SharedList<int>();

            Assert.DoesNotThrow(() => list.Dispose());
            Assert.DoesNotThrow(() => list.Dispose());
        }

        [Test]
        public void Add_ToZeroCapacityList_GrowsAndKeepsEveryItem()
        {
            using var list = new SharedList<int>();
            var expected = new List<int>();

            for (var i = 0; i < 40; i++)
            {
                var value = i * 3;

                if ((i & 1) == 0)
                {
                    list.Add(value);
                }
                else
                {
                    list.Add(in value);
                }

                expected.Add(value);
            }

            CollectionAssert.AreEqual(expected, list.ToArray());
        }

        [Test]
        public void Add_InArgumentAliasingOwnStorageOnFullList_AppendsACopy()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });

            // SAFETY: The reference addresses a live element; Add reads it while the list is still valid.
            unsafe
            {
                list.Add(in list.ElementAt(0));
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 1 }, list.ToArray());
        }

        [Test]
        public void Insert_IntoZeroCapacityList_AtCount_StoresTheItem()
        {
            using var list = new SharedList<int>();
            var item = 6;

            list.Insert(0, 5);
            list.Insert(1, in item);

            CollectionAssert.AreEqual(new[] { 5, 6 }, list.ToArray());
        }

        [Test]
        public void Insert_EveryPositionOnAFullList_MatchesListOfT()
        {
            var seed = new[] { 1, 2, 3, 4 };

            for (var index = 0; index <= seed.Length; index++)
            {
                using var list = new SharedList<int>((int[])seed.Clone());
                using var inList = new SharedList<int>((int[])seed.Clone());
                var expected = new List<int>(seed);
                var item = 99;

                list.Insert(index, 99);
                inList.Insert(index, in item);
                expected.Insert(index, 99);

                CollectionAssert.AreEqual(expected, list.ToArray(), $"index {index}");
                CollectionAssert.AreEqual(expected, inList.ToArray(), $"in index {index}");
            }
        }

        [Test]
        public void Insert_PastTheCapacity_KeepsEveryItemInOrder()
        {
            using var list = new SharedList<int>(4);
            var expected = new List<int>();

            for (var i = 0; i < 30; i++)
            {
                list.Insert(i / 2, i);
                expected.Insert(i / 2, i);
            }

            CollectionAssert.AreEqual(expected, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Insert_IndexOutOfRange_Throws()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3 });
            var item = 9;

            Assert.Throws<InvalidOperationException>(() => list.Insert(-1, 9));
            Assert.Throws<InvalidOperationException>(() => list.Insert(4, 9));
            Assert.Throws<InvalidOperationException>(() => list.Insert(int.MaxValue, 9));
            Assert.Throws<InvalidOperationException>(() => list.Insert(-1, in item));
            Assert.Throws<InvalidOperationException>(() => list.Insert(4, in item));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [Test]
        public void Insert_InArgumentAliasingOwnStorageOnFullList_InsertsACopy()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });
            var expected = new List<int> { 1, 2, 3, 4 };

            // SAFETY: The reference addresses a live element; Insert reads it while the list is still valid.
            unsafe
            {
                list.Insert(2, in list.ElementAt(0));
            }

            expected.Insert(2, expected[0]);

            CollectionAssert.AreEqual(expected, list.ToArray());
        }

        [Test]
        public void Insert_InArgumentAliasingAShiftedElementWithSpareCapacity_InsertsTheOriginalValue()
        {
            using var list = new SharedList<int>(16);
            var expected = new List<int> { 1, 2, 3, 4 };

            list.AddRange(new[] { 1, 2, 3, 4 });

            // SAFETY: The reference addresses a live element; Insert reads it while the list is still valid.
            unsafe
            {
                list.Insert(1, in list.ElementAt(2));
            }

            expected.Insert(1, expected[2]);

            CollectionAssert.AreEqual(expected, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ElementAt_OutOfRange_Throws()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3 });
            using var empty = new SharedList<int>();

            // SAFETY: The lambdas only provoke the range check; no reference escapes.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => _ = list.ElementAt(-1));
                Assert.Throws<InvalidOperationException>(() => _ = list.ElementAt(3));
                Assert.Throws<InvalidOperationException>(() => _ = list.ElementAt(int.MaxValue));
                Assert.Throws<InvalidOperationException>(() => _ = empty.ElementAt(0));
            }
        }

        [Test]
        public void ElementAt_WriteThroughOneReference_IsSeenByEveryOtherView()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3 });

            // SAFETY: list stays live and is not resized while the borrowed references are used.
            unsafe
            {
                ref var first = ref list.ElementAt(0);
                ref var again = ref list.ElementAt(0);

                first = 10;

                Assert.AreEqual(10, again);
                Assert.AreEqual(10, list[0]);
                Assert.AreEqual(10, list.AsReadOnlySpan()[0]);
                Assert.AreEqual(10, list.AsReadOnly()[0]);

                again = 20;

                Assert.AreEqual(20, first);
                Assert.AreEqual(20, list.AsSpan()[0]);
            }
        }

        [Test]
        public void AddRange_EmptyInputs_LeaveTheListUnchanged()
        {
            using var list = new SharedList<int>(new[] { 1, 2 });

            list.AddRange(Array.Empty<int>());
            list.AddRange(new[] { 5, 6 }, 0);
            list.AddRange(ReadOnlySpan<int>.Empty);
            list.AddRange(new[] { 5, 6 }.AsSpan(), 0);
            list.AddRange(new List<int>());
            list.AddRange(YieldNothing());

            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public void AddRange_ExactlyFillsThenGrows_KeepsEveryElement()
        {
            using var list = new SharedList<int>(4);
            var all = new int[100];

            for (var i = 0; i < all.Length; i++)
            {
                all[i] = i + 1;
            }

            list.AddRange(all.AsSpan(0, 4));

            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(4, list.Capacity);

            list.AddRange(new[] { 5 });

            Assert.AreEqual(5, list.Count);
            Assert.Greater(list.Capacity, 4);

            list.AddRange(all.AsSpan(5));

            CollectionAssert.AreEqual(all, list.ToArray());
        }

        [Test]
        public void AddRange_SpanOverOwnStorageOnFullList_AppendsACopy()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });

            // SAFETY: AddRange consumes the span before this block ends; the list outlives it.
            unsafe
            {
                list.AddRange(list.AsReadOnlySpan());
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void AddRange_PrefixOfOwnStorageOnFullList_AppendsACopy()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });

            // SAFETY: AddRange consumes the span before this block ends; the list outlives it.
            unsafe
            {
                list.AddRange(list.AsReadOnlySpan(), 2);
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 1, 2 }, list.ToArray());
        }

        [Test]
        public void AddRange_SpanOverOwnStorageWithSpareCapacity_AppendsACopy()
        {
            using var list = new SharedList<int>(16);

            list.AddRange(new[] { 1, 2, 3 });

            // SAFETY: AddRange consumes the span before this block ends; the list outlives it.
            unsafe
            {
                list.AddRange(list.AsReadOnlySpan());
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 2, 3 }, list.ToArray());
        }

        [Test]
        public void AddRange_CollectionIsTheListItself_AppendsACopy()
        {
            using var full = new SharedList<int>(new[] { 1, 2, 3, 4 });
            using var spare = new SharedList<int>(16);

            spare.AddRange(new[] { 1, 2, 3 });
            full.AddRange((IEnumerable<int>)full);
            spare.AddRange((IEnumerable<int>)spare);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 1, 2, 3, 4 }, full.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 2, 3 }, spare.ToArray());
        }

        [Test]
        public void AddRange_ElementSizes_RoundTripBitExact()
        {
            AssertAddRangeRoundTrip(
                new byte[] { 0, 255, 128 },
                new byte[] { 1, 2 }
            );
            AssertAddRangeRoundTrip(
                new[] { long.MinValue, -1L, long.MaxValue },
                new[] { 0x0102030405060708L }
            );
            AssertAddRangeRoundTrip(
                new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 0, b = 128 } },
                new[] { new Rgb { r = 9, g = 8, b = 7 } }
            );
            AssertAddRangeRoundTrip(
                new[] { new Wide { a = long.MinValue, b = -1, c = long.MaxValue } },
                new[] { new Wide { a = 1, b = 2, c = 3 }, new Wide { a = 4, b = 5, c = 6 } }
            );
        }

        [Test]
        public void CopyToAndCopyFrom_EmptyList_AcceptEmptySpans()
        {
            using var list = new SharedList<int>();
            using var withCapacity = new SharedList<int>(4);

            list.CopyTo(Span<int>.Empty);
            list.CopyTo(Array.Empty<int>(), 0);
            list.CopyFrom(ReadOnlySpan<int>.Empty);
            withCapacity.CopyTo(0, Span<int>.Empty);
            withCapacity.CopyFrom(0, ReadOnlySpan<int>.Empty);

            Assert.IsTrue(list.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(list.TryCopyFrom(ReadOnlySpan<int>.Empty));
            Assert.IsTrue(withCapacity.TryCopyTo(0, Span<int>.Empty));
            Assert.IsTrue(withCapacity.TryCopyFrom(0, ReadOnlySpan<int>.Empty));
            Assert.IsFalse(list.TryCopyTo(new int[1]));
            Assert.IsFalse(withCapacity.TryCopyFrom(new[] { 1 }));
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, withCapacity.Count);
        }

        [Test]
        public void CopyToAndCopyFrom_AtTheCount_AcceptEmptySpans()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });

            list.CopyTo(4, Span<int>.Empty);
            list.CopyFrom(4, ReadOnlySpan<int>.Empty);

            Assert.IsTrue(list.TryCopyTo(4, Span<int>.Empty));
            Assert.IsTrue(list.TryCopyFrom(4, ReadOnlySpan<int>.Empty));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void CopyTo_RangesOutsideTheListOrDestination_Throw()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });

            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(-1, new int[4]));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(5, new int[0]));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(1, new int[4]));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(0, new int[2], 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(0, new int[4], -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(new int[3], 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(new int[4], -1));
        }

        [Test]
        public void TryCopyTo_RangesOutsideTheListOrDestination_ReturnFalseAndLeaveTheDestination()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });
            var destination = new int[4];

            Assert.IsFalse(list.TryCopyTo(-1, destination));
            Assert.IsFalse(list.TryCopyTo(5, new int[0]));
            Assert.IsFalse(list.TryCopyTo(1, destination));
            Assert.IsFalse(list.TryCopyTo(0, new int[2], 3));
            Assert.IsFalse(list.TryCopyTo(0, destination, -1));
            Assert.IsFalse(list.TryCopyTo(int.MaxValue, destination));
            CollectionAssert.AreEqual(new int[4], destination);
        }

        [Test]
        public void CopyFrom_RangesOutsideTheListOrSource_ThrowAndKeepTheContent()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });

            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyFrom(-1, new[] { 9 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyFrom(4, new[] { 9 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyFrom(0, new[] { 9 }, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyFrom(0, new[] { 9 }, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyFrom(new[] { 1, 2, 3, 4, 5 }));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void TryCopyFrom_RangesOutsideTheListOrSource_ReturnFalseAndKeepTheContent()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });

            Assert.IsFalse(list.TryCopyFrom(-1, new[] { 9 }));
            Assert.IsFalse(list.TryCopyFrom(4, new[] { 9 }));
            Assert.IsFalse(list.TryCopyFrom(0, new[] { 9 }, 2));
            Assert.IsFalse(list.TryCopyFrom(0, new[] { 9 }, -1));
            Assert.IsFalse(list.TryCopyFrom(new[] { 1, 2, 3, 4, 5 }));
            Assert.IsFalse(list.TryCopyFrom(int.MaxValue, new[] { 9 }));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void CopyFrom_SourceOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var list = new SharedList<int>((int[])seed.Clone());

            // SAFETY: Each borrowed span is consumed by CopyFrom before the next mutation; the list outlives them.
            unsafe
            {
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
        }

        [Test]
        public void CopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var list = new SharedList<int>((int[])seed.Clone());

            // SAFETY: Each borrowed span is consumed by CopyTo before the next mutation; the list outlives them.
            unsafe
            {
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
        }

        [Test]
        public void CopyToAndCopyFrom_ElementSizes_RoundTripBitExact()
        {
            AssertCopyRoundTrip(new byte[] { 0, 255, 128 }, new byte[] { 7, 8, 9 });
            AssertCopyRoundTrip(new[] { long.MinValue, -1L, long.MaxValue }, new[] { 1L, 2L, 3L });
            AssertCopyRoundTrip(
                new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 0, b = 128 } },
                new[] { new Rgb { r = 9, g = 8, b = 7 }, new Rgb { r = 6, g = 5, b = 4 } }
            );
            AssertCopyRoundTrip(
                new[] { new Wide { a = long.MinValue, b = -1, c = long.MaxValue } },
                new[] { new Wide { a = 1, b = 2, c = 3 } }
            );
        }

        [Test]
        public void GetEnumerator_EmptyListWithCapacity_YieldsNothing()
        {
            using var list = new SharedList<int>(4);
            var count = 0;

            // SAFETY: list outlives the enumerators and is not modified while they run.
            unsafe
            {
                foreach (var item in list)
                {
                    count++;
                }

                foreach (var item in (IEnumerable<int>)list)
                {
                    count++;
                }

                foreach (var item in (IEnumerable)list)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void GetEnumerator_ZeroCapacityList_YieldsNothing()
        {
            using var list = new SharedList<int>();
            var count = 0;

            // SAFETY: list outlives the enumerators and is not modified while they run.
            unsafe
            {
                foreach (var item in list)
                {
                    count++;
                }

                foreach (var item in (IEnumerable<int>)list)
                {
                    count++;
                }

                foreach (var item in (IEnumerable)list)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Remove_FromEmptyList_ReturnsFalse()
        {
            using var list = new SharedList<int>();
            var item = 1;

            Assert.IsFalse(list.Remove(1));
            Assert.IsFalse(list.Remove(in item));
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void Remove_OnlyItem_EmptiesTheList()
        {
            using var list = new SharedList<int>(new[] { 5 });
            var item = 5;

            Assert.IsTrue(list.Remove(5));
            Assert.AreEqual(0, list.Count);

            list.Add(5);

            Assert.IsTrue(list.Remove(in item));
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void Remove_EveryPositionAndDuplicates_MatchesListOfT()
        {
            var seed = new[] { 1, 2, 3, 2, 4 };

            foreach (var target in new[] { 1, 2, 3, 4, 9 })
            {
                using var list = new SharedList<int>((int[])seed.Clone());
                using var inList = new SharedList<int>((int[])seed.Clone());
                var expected = new List<int>(seed);
                var item = target;

                var expectedResult = expected.Remove(target);

                Assert.AreEqual(expectedResult, list.Remove(target), $"target {target}");
                Assert.AreEqual(expectedResult, inList.Remove(in item), $"in target {target}");
                CollectionAssert.AreEqual(expected, list.ToArray(), $"target {target}");
                CollectionAssert.AreEqual(expected, inList.ToArray(), $"in target {target}");
            }
        }

        [Test]
        public void Remove_ItemNotPresent_DoesNotInvalidateEnumeration()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedList<int, int>.Enumerator enumerator;

            // SAFETY: list outlives the enumerator, which is only used inside this test.
            unsafe
            {
                enumerator = list.GetEnumerator();
            }

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsFalse(list.Remove(9));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);
        }

        [Test]
        public void Remove_InArgumentItemNotPresent_DoesNotInvalidateEnumeration()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            var absent = 9;
            SharedList<int, int>.Enumerator enumerator;

            // SAFETY: list outlives the enumerator, which is only used inside this test.
            unsafe
            {
                enumerator = list.GetEnumerator();
            }

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsFalse(list.Remove(in absent));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void RemoveAtSwapBack_OutOfRange_Throws()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3 });
            using var empty = new SharedList<int>();

            Assert.Throws<InvalidOperationException>(() => list.RemoveAtSwapBack(-1));
            Assert.Throws<InvalidOperationException>(() => list.RemoveAtSwapBack(3));
            Assert.Throws<InvalidOperationException>(() => list.RemoveAtSwapBack(int.MaxValue));
            Assert.Throws<InvalidOperationException>(() => empty.RemoveAtSwapBack(0));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [Test]
        public void RemoveAtSwapBack_EveryIndex_MovesTheLastItemIntoTheGap()
        {
            var seed = new[] { 10, 20, 30, 40 };

            for (var index = 0; index < seed.Length; index++)
            {
                using var list = new SharedList<int>((int[])seed.Clone());
                var expected = new List<int>(seed);

                expected[index] = expected[^1];
                expected.RemoveAt(expected.Count - 1);
                list.RemoveAtSwapBack(index);

                CollectionAssert.AreEqual(expected, list.ToArray(), $"index {index}");
            }
        }

        [Test]
        public void RemoveAtSwapBack_OnlyItem_EmptiesTheList()
        {
            using var list = new SharedList<int>(new[] { 5 });

            list.RemoveAtSwapBack(0);

            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void AsSpanAndAsReadOnlySpan_EmptyList_HaveLengthZero()
        {
            using var zeroCapacity = new SharedList<int>();
            using var withCapacity = new SharedList<int>(4);

            // SAFETY: The lists outlive the spans, which are only inspected here.
            unsafe
            {
                Assert.AreEqual(0, zeroCapacity.AsSpan().Length);
                Assert.AreEqual(0, zeroCapacity.AsReadOnlySpan().Length);
                Assert.AreEqual(0, withCapacity.AsSpan().Length);
                Assert.AreEqual(0, withCapacity.AsReadOnlySpan().Length);
            }
        }

        [Test]
        public void AsSpanAndAsReadOnlySpan_ElementSizes_RoundTripBitExact()
        {
            AssertSpanRoundTrip(new byte[] { 0, 255, 128 });
            AssertSpanRoundTrip(new[] { long.MinValue, -1L, long.MaxValue });
            AssertSpanRoundTrip(new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 0, b = 128 } });
            AssertSpanRoundTrip(new[] { new Wide { a = long.MinValue, b = -1, c = long.MaxValue } });
        }

        [Test]
        public void AddReplicate_ZeroAmount_ReturnsAnEmptySpanAndKeepsTheList()
        {
            using var list = new SharedList<int>(new[] { 1, 2 });

            // SAFETY: Each span is inspected before the next mutation; the list outlives them.
            unsafe
            {
                Assert.AreEqual(0, list.AddReplicate(0).Length);
                Assert.AreEqual(0, list.AddReplicate(7, 0).Length);
                Assert.AreEqual(0, list.AddReplicateNoInit(0).Length);
            }

            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public void AddReplicate_NegativeAmount_Throws()
        {
            using var list = new SharedList<int>(new[] { 1, 2 });

            Assert.Throws<ArgumentOutOfRangeException>(() => { list.AddReplicate(-1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { list.AddReplicate(7, -1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { list.AddReplicateNoInit(-1); });
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public void AddReplicate_PastTheCapacity_KeepsEarlierItemsAndFillsTheNewOnes()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });

            // SAFETY: Each span is used before the next mutation; the list outlives them.
            unsafe
            {
                var filled = list.AddReplicate(7, 3);

                Assert.AreEqual(3, filled.Length);

                filled[1] = 8;
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 7, 8, 7 }, list.ToArray());

            // SAFETY: The span is used before the next mutation; the list outlives it.
            unsafe
            {
                var zeroed = list.AddReplicate(20);

                Assert.AreEqual(20, zeroed.Length);
            }

            Assert.AreEqual(27, list.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 7, 8, 7 }, list.ToArray().AsSpan(0, 7).ToArray());
            CollectionAssert.AreEqual(new int[20], list.ToArray().AsSpan(7).ToArray());
        }

        [Test]
        public void AddReplicate_AfterClear_ZeroesTheReusedSlots()
        {
            using var list = new SharedList<int>(new[] { 5, 6, 7, 8 });

            list.Clear();

            // SAFETY: The span is discarded before the next mutation; the list outlives it.
            unsafe
            {
                list.AddReplicate(3);
            }

            CollectionAssert.AreEqual(new[] { 0, 0, 0 }, list.ToArray());
        }

        [Test]
        public void AddReplicateNoInit_AfterClear_ExposesTheReusedSlotsToWrites()
        {
            using var list = new SharedList<int>(new[] { 5, 6, 7, 8 });

            list.Clear();

            // SAFETY: The span is used before the next mutation; the list outlives it.
            unsafe
            {
                var span = list.AddReplicateNoInit(3);

                Assert.AreEqual(3, span.Length);

                span[0] = 1;
                span[1] = 2;
                span[2] = 3;
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [Test]
        public void AddReplicate_ElementSizes_RoundTripBitExact()
        {
            AssertReplicateRoundTrip((byte)255, 5);
            AssertReplicateRoundTrip(long.MinValue, 3);
            AssertReplicateRoundTrip(new Rgb { r = 255, g = 0, b = 128 }, 4);
            AssertReplicateRoundTrip(new Wide { a = long.MinValue, b = -1, c = long.MaxValue }, 3);
        }

        [Test]
        public void CountAndVersion_FollowTheMutationsOfTheList()
        {
            using var list = new SharedList<int>(8);

            Assert.AreEqual(0, list.CountRO);

            var version = list.VersionRO;

            list.Add(1);

            Assert.AreEqual(1, list.CountRO);
            Assert.AreNotEqual(version, list.VersionRO);

            version = list.VersionRO;

            _ = list[0];
            _ = list.Contains(1);
            _ = list.IndexOf(1);
            _ = list.ToArray();

            Assert.AreEqual(version, list.VersionRO);

            list.VersionRW++;

            Assert.AreEqual(version + 1, list.VersionRO);

            list.CountRW = 0;

            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void AsReadOnly_ViewSeesLaterWritesAndAppends()
        {
            using var list = new SharedList<int>(8);

            list.Add(1);

            // SAFETY: list stays live and is not resized while the view is used.
            unsafe
            {
                var view = list.AsReadOnly();

                list.Add(2);
                list[0] = 10;

                Assert.AreEqual(2, view.Count);
                Assert.AreEqual(10, view[0]);
                Assert.AreEqual(2, view[1]);
                CollectionAssert.AreEqual(new[] { 10, 2 }, view.ToArray());
                CollectionAssert.AreEqual(new[] { 10, 2 }, view.AsReadOnlySpan().ToArray());

                list.Clear();

                Assert.AreEqual(0, view.Count);
            }
        }

        [Test]
        public void AsReadOnly_EmptyLists_ReportNoItems()
        {
            using var zeroCapacity = new SharedList<int>();
            using var withCapacity = new SharedList<int>(4);

            // SAFETY: The lists outlive the views, which are only inspected here.
            unsafe
            {
                var zeroView = zeroCapacity.AsReadOnly();
                var capacityView = withCapacity.AsReadOnly();

                Assert.AreEqual(0, zeroView.Count);
                Assert.AreEqual(0, capacityView.Count);
                Assert.AreEqual(0, zeroView.ToArray().Length);
                Assert.AreEqual(0, capacityView.ToArray().Length);
                Assert.IsTrue(capacityView.IsCreated);
            }
        }

        [Test]
        public void ReadOnly_ZeroCapacityList_IsCreatedAndDefaultIsNotCreated()
        {
            using var list = new SharedList<int>();

            // SAFETY: The owner remains alive and unmodified while its view is inspected.
            unsafe
            {
                Assert.IsTrue(list.AsReadOnly().IsCreated);
            }

            Assert.IsFalse(default(SharedList<int, int>.ReadOnly).IsCreated);
        }

        [Test]
        public void ReadOnly_Default_IsNotCreated()
        {
            var view = default(SharedList<int, int>.ReadOnly);

            Assert.IsFalse(view.IsCreated);
        }

        [Test]
        public void ReadOnly_EmptyViewWithCapacity_EnumeratesNothing()
        {
            using var list = new SharedList<int>(4);
            var count = 0;

            // SAFETY: list outlives the view and its enumerators.
            unsafe
            {
                var view = list.AsReadOnly();

                foreach (var item in view)
                {
                    count++;
                }

                foreach (var item in (IEnumerable<int>)view)
                {
                    count++;
                }

                foreach (var item in (IEnumerable)view)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnlyIndexer_OutOfRange_Throws()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3 });
            using var empty = new SharedList<int>(4);

            // SAFETY: The lists outlive the views; the lambdas only provoke the range check.
            unsafe
            {
                var view = list.AsReadOnly();
                var emptyView = empty.AsReadOnly();

                Assert.Throws<InvalidOperationException>(() => _ = view[-1]);
                Assert.Throws<InvalidOperationException>(() => _ = view[3]);
                Assert.Throws<InvalidOperationException>(() => _ = view[int.MaxValue]);
                Assert.Throws<InvalidOperationException>(() => _ = emptyView[0]);
            }
        }

        [Test]
        public void ReadOnlyReinterpret_SameSizeTypes_ReadTheSameBits()
        {
            using var longs = new SharedList<long>(new[] { -1L, long.MinValue, 0L, long.MaxValue });
            using var bytes = new SharedList<byte>(new byte[] { 0, 255, 128 });

            // SAFETY: The lists outlive the views, which are only inspected here.
            unsafe
            {
                var asUnsigned = longs.AsReadOnly().Reinterpret<ulong>();
                var asDouble = longs.AsReadOnly().Reinterpret<double>();
                var asSigned = bytes.AsReadOnly().Reinterpret<sbyte>();

                Assert.AreEqual(4, asUnsigned.Count);
                Assert.AreEqual(ulong.MaxValue, asUnsigned[0]);
                Assert.AreEqual(unchecked((ulong)long.MinValue), asUnsigned[1]);
                Assert.AreEqual(0UL, asUnsigned[2]);
                Assert.AreEqual((ulong)long.MaxValue, asUnsigned[3]);
                Assert.AreEqual(long.MinValue, BitConverter.DoubleToInt64Bits(asDouble[1]));
                Assert.AreEqual(0.0, asDouble[2]);
                Assert.AreEqual(3, asSigned.Count);
                Assert.AreEqual((sbyte)0, asSigned[0]);
                Assert.AreEqual((sbyte)-1, asSigned[1]);
                Assert.AreEqual((sbyte)-128, asSigned[2]);
            }
        }

        [Test]
        public void ReadOnlyCopyTo_EmptyView_AcceptsEmptyDestinations()
        {
            using var list = new SharedList<int>(4);

            // SAFETY: The list outlives the view, which is only used here.
            unsafe
            {
                var view = list.AsReadOnly();

                view.CopyTo(Span<int>.Empty);
                view.CopyTo(Array.Empty<int>(), 0);
                view.CopyTo(0, Span<int>.Empty);

                Assert.IsTrue(view.TryCopyTo(Span<int>.Empty));
                Assert.IsTrue(view.TryCopyTo(0, Span<int>.Empty));
                Assert.IsFalse(view.TryCopyTo(new int[1]));
            }
        }

        [Test]
        public void ReadOnlyCopyTo_RangesOutsideTheViewOrDestination_ThrowOrReturnFalse()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });

            // SAFETY: The list outlives the view, which is only used here.
            unsafe
            {
                var view = list.AsReadOnly();
                var destination = new int[4];

                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(-1, new int[4]));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(5, new int[0]));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(1, new int[4]));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(0, new int[2], 3));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(0, new int[4], -1));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(new int[3], 1));
                Assert.IsFalse(view.TryCopyTo(-1, destination));
                Assert.IsFalse(view.TryCopyTo(5, new int[0]));
                Assert.IsFalse(view.TryCopyTo(1, destination));
                Assert.IsFalse(view.TryCopyTo(0, new int[2], 3));
                Assert.IsFalse(view.TryCopyTo(0, destination, -1));
                CollectionAssert.AreEqual(new int[4], destination);
            }
        }

        [Test]
        public void ReadOnlyCopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var list = new SharedList<int>((int[])seed.Clone());

            // SAFETY: The list outlives the view and the spans, which are consumed by each copy.
            unsafe
            {
                var view = list.AsReadOnly();

                view.CopyTo(2, list.AsSpan()[..4]);

                var expectedBackward = (int[])seed.Clone();
                Array.Copy(expectedBackward, 2, expectedBackward, 0, 4);
                CollectionAssert.AreEqual(expectedBackward, list.ToArray());

                list.CopyFrom(seed);

                Assert.IsTrue(view.TryCopyTo(0, list.AsSpan()[2..]));

                var expectedForward = (int[])seed.Clone();
                Array.Copy(expectedForward, 0, expectedForward, 2, 4);
                CollectionAssert.AreEqual(expectedForward, list.ToArray());
            }
        }

        [Test]
        public void ReadOnlyCopyTo_ElementSizes_RoundTripBitExact()
        {
            AssertReadOnlyCopyRoundTrip(new byte[] { 0, 255, 128 });
            AssertReadOnlyCopyRoundTrip(new[] { long.MinValue, -1L, long.MaxValue });
            AssertReadOnlyCopyRoundTrip(new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 0, b = 128 } });
            AssertReadOnlyCopyRoundTrip(new[] { new Wide { a = long.MinValue, b = -1, c = long.MaxValue } });
        }

        private static void AssertAddRangeRoundTrip<T>(T[] first, T[] second)
            where T : unmanaged
        {
            using var list = new SharedList<T>();
            var all = new T[first.Length + second.Length];

            first.CopyTo(all, 0);
            second.CopyTo(all, first.Length);
            list.AddRange(first);
            list.AddRange(second.AsSpan());

            // SAFETY: list outlives the span, which is only inspected here.
            unsafe
            {
                SpanBytesAssert.AreEqual(all, list.AsReadOnlySpan());
            }
        }

        private static void AssertCopyRoundTrip<T>(T[] first, T[] second)
            where T : unmanaged
        {
            using var list = new SharedList<T>((T[])first.Clone());
            var copy = new T[first.Length];

            list.CopyTo(copy);
            SpanBytesAssert.AreEqual(first, copy);

            list.CopyFrom(second.AsSpan(0, first.Length));

            // SAFETY: list outlives the span, which is only inspected here.
            unsafe
            {
                SpanBytesAssert.AreEqual(second.AsSpan(0, first.Length).ToArray(), list.AsReadOnlySpan());
            }
        }

        private static void AssertSpanRoundTrip<T>(T[] values)
            where T : unmanaged
        {
            using var list = new SharedList<T>((T[])values.Clone());

            // SAFETY: list outlives the spans, which are only inspected here.
            unsafe
            {
                var readOnly = list.AsReadOnlySpan();
                var writable = list.AsSpan();

                Assert.AreEqual(values.Length, readOnly.Length);
                Assert.AreEqual(values.Length, writable.Length);
                SpanBytesAssert.AreEqual(values, readOnly);
                SpanBytesAssert.AreEqual(values, writable);
            }
        }

        private static void AssertReplicateRoundTrip<T>(T value, int amount)
            where T : unmanaged
        {
            using var list = new SharedList<T>();
            var expected = new T[amount];

            expected.AsSpan().Fill(value);

            // SAFETY: The span is inspected before the next mutation; list outlives it.
            unsafe
            {
                var span = list.AddReplicate(value, amount);

                SpanBytesAssert.AreEqual(expected, span);
            }

            // SAFETY: list outlives the span, which is only inspected here.
            unsafe
            {
                SpanBytesAssert.AreEqual(expected, list.AsReadOnlySpan());
            }
        }

        private static void AssertReadOnlyCopyRoundTrip<T>(T[] values)
            where T : unmanaged
        {
            using var list = new SharedList<T>((T[])values.Clone());
            var copy = new T[values.Length];

            // SAFETY: list outlives the view, which is only used here.
            unsafe
            {
                var view = list.AsReadOnly();

                view.CopyTo(copy);
                SpanBytesAssert.AreEqual(values, copy);
                SpanBytesAssert.AreEqual(values, view.AsReadOnlySpan());
            }
        }

        private static IEnumerable<int> YieldNothing()
        {
            yield break;
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

        private static IEnumerable<int> YieldValues()
        {
            yield return 11;
            yield return 12;
        }
    }
}
