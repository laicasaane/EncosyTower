#if UNITY_COLLECTIONS

using System;
using System.Collections.Generic;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Tests.Core.Collections
{
    public class NativeListExtensionsTests
    {
        [Test]
        public void CollectionMembers_ManageCapacityRangesViewsAndDefaultAccess()
        {
            NativeList<int> list = default;

            try
            {
                EncosyNativeListExtensions.NewOrClear(
                      ref list
                    , 4
                    , AllocatorManager.Persistent
                );

                Assert.IsTrue(list.IsCreated);
                Assert.GreaterOrEqual(list.Capacity, 4);

                var capacity = list.Capacity;
                EncosyNativeListExtensions.IncreaseCapacityBy(list, 3);
                Assert.GreaterOrEqual(list.Capacity, capacity + 3);

                var target = list.Capacity + 2;
                EncosyNativeListExtensions.IncreaseCapacityTo(list, target);
                Assert.GreaterOrEqual(list.Capacity, target);

                var fixedCapacity = list.Capacity;
                EncosyNativeListExtensions.IncreaseCapacityTo(list, fixedCapacity - 1);
                Assert.AreEqual(fixedCapacity, list.Capacity);

                list.Add(1);
                list.Add(4);
                ReadOnlySpan<int> tail = new[] { 5, 6 };
                EncosyNativeListExtensions.AddRange(list, tail);

                CollectionAssert.AreEqual(
                      new[] { 1, 4, 5, 6 }
                    , EncosyNativeListExtensions.AsReadOnlySpan(list).ToArray()
                );

                list.Clear();
                list.Add(1);
                list.Add(4);

                Span<int> inserted = EncosyNativeListExtensions.InsertRangeSpan(list, 1, 2);
                inserted[0] = 2;
                inserted[1] = 3;

                CollectionAssert.AreEqual(
                      new[] { 1, 2, 3, 4 }
                    , EncosyNativeListExtensions.AsReadOnlySpan(list).ToArray()
                );

                Span<int> insertedWithEnd = EncosyNativeListExtensions.InsertRangeWithBeginEndSpan(
                      list
                    , 2
                    , 4
                );
                insertedWithEnd[0] = 20;
                insertedWithEnd[1] = 30;

                Assert.AreEqual(2, insertedWithEnd.Length);
                CollectionAssert.AreEqual(
                      new[] { 1, 2, 20, 30, 3, 4 }
                    , EncosyNativeListExtensions.AsReadOnlySpan(list).ToArray()
                );

                Span<int> emptyInsert = EncosyNativeListExtensions.InsertRangeWithBeginEndSpan(
                      list
                    , 1
                    , 1
                );
                Assert.IsTrue(emptyInsert.IsEmpty);

                Span<int> writable = EncosyNativeListExtensions.AsSpan(list);
                writable[0] = 10;
                ReadOnlySpan<int> readOnly = EncosyNativeListExtensions.AsReadOnlySpan(list);

                Assert.AreEqual(10, readOnly[0]);
                Assert.AreEqual(10, EncosyNativeListExtensions.ElementAtOrDefault(list, 0));
                Assert.AreEqual(0, EncosyNativeListExtensions.ElementAtOrDefault(list, -1));
                Assert.AreEqual(
                      0
                    , EncosyNativeListExtensions.ElementAtOrDefault(list, list.Length)
                );

                EncosyNativeListExtensions.NewOrClear(
                      ref list
                    , 1
                    , AllocatorManager.Persistent
                );

                Assert.AreEqual(0, list.Length);
            }
            finally
            {
                EncosyNativeListExtensions.DisposeIfCreated(list);
            }
        }

        [Test]
        public void DisposalMembers_HandleCreatedAndDefaultLists()
        {
            var synchronous = new NativeList<int>(1, AllocatorManager.Persistent);
            var scheduled = new NativeList<int>(1, AllocatorManager.Persistent);
            var unset = new NativeList<int>(1, AllocatorManager.Persistent);

            try
            {
                EncosyNativeListExtensions.DisposeIfCreated(synchronous);
                synchronous = default;

                JobHandle handle = EncosyNativeListExtensions.DisposeIfCreated(scheduled, default);
                handle.Complete();
                scheduled = default;

                EncosyNativeListExtensions.DisposeIfCreated(default(NativeList<int>));
                var unchanged = EncosyNativeListExtensions.DisposeIfCreated(
                      default(NativeList<int>)
                    , default
                );
                unchanged.Complete();

                EncosyNativeListExtensions.DisposeUnset(ref unset);
                Assert.IsFalse(unset.IsCreated);
            }
            finally
            {
                EncosyNativeListExtensions.DisposeIfCreated(synchronous);
                EncosyNativeListExtensions.DisposeIfCreated(scheduled);
                EncosyNativeListExtensions.DisposeIfCreated(unset);
            }
        }

        [Test]
        public void AddRange_EmptySpan_LeavesTheListUnchanged()
        {
            var list = new NativeList<int>(4, AllocatorManager.Persistent);

            try
            {
                list.Add(1);

                EncosyNativeListExtensions.AddRange(list, ReadOnlySpan<int>.Empty);

                CollectionAssert.AreEqual(new[] { 1 }, list.AsArray().ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_SpanOverTheOwnStorageWithoutGrowth_MatchesListAddRange()
        {
            var list = new NativeList<int>(6, AllocatorManager.Persistent);

            try
            {
                list.Add(1);
                list.Add(2);
                list.Add(3);

                var expected = new List<int> { 1, 2, 3 };
                expected.AddRange(new[] { 1, 2, 3 });

                EncosyNativeListExtensions.AddRange(list, EncosyNativeListExtensions.AsReadOnlySpan(list));

                CollectionAssert.AreEqual(expected, list.AsArray().ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_PartialSpanOverTheOwnStorageWithoutGrowth_MatchesListAddRange()
        {
            var list = new NativeList<int>(6, AllocatorManager.Persistent);

            try
            {
                list.Add(1);
                list.Add(2);
                list.Add(3);

                var expected = new List<int> { 1, 2, 3 };
                expected.AddRange(new[] { 2, 3 });

                EncosyNativeListExtensions.AddRange(list, EncosyNativeListExtensions.AsReadOnlySpan(list)[1..]);

                CollectionAssert.AreEqual(expected, list.AsArray().ToArray());
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_PastTheCapacity_GrowsAndKeepsEveryElement()
        {
            var list = new NativeList<int>(1, AllocatorManager.Persistent);

            try
            {
                list.Add(1);

                var items = new int[40];

                for (var i = 0; i < items.Length; i++)
                {
                    items[i] = i + 2;
                }

                EncosyNativeListExtensions.AddRange(list, items);

                Assert.AreEqual(41, list.Length);
                Assert.AreEqual(1, list[0]);
                CollectionAssert.AreEqual(items, list.AsArray().ToArray()[1..]);
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AddRange_ElementTypes_CopyBitExact()
        {
            AssertAddRangeCopiesBitExact<byte>(a: 0xA5, b: 0xFF, c: 0x01);
            AssertAddRangeCopiesBitExact(
                  a: new Rgb { r = 1, g = 2, b = 3 }
                , b: new Rgb { r = 255, g = 254, b = 253 }
                , c: new Rgb { r = 7, g = 0, b = 9 }
            );
            AssertAddRangeCopiesBitExact(a: long.MinValue, b: long.MaxValue, c: -1L);
            AssertAddRangeCopiesBitExact(
                  a: new Wide { a = 1, b = -2, c = long.MaxValue }
                , b: new Wide { a = long.MinValue, b = 0, c = -1 }
                , c: new Wide { a = 5, b = 6, c = 7 }
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AddRange_AfterDispose_Throws()
        {
            var list = new NativeList<int>(4, AllocatorManager.Persistent);
            list.Dispose();

            Assert.Catch(() => EncosyNativeListExtensions.AddRange<int>(list, new int[] { 1 }));
        }

        [TestCase(0)]
        [TestCase(4)]
        public void AsSpan_EmptyList_IsEmpty(int capacity)
        {
            var list = new NativeList<int>(capacity, AllocatorManager.Persistent);

            try
            {
                Assert.IsTrue(EncosyNativeListExtensions.AsSpan(list).IsEmpty);
                Assert.IsTrue(EncosyNativeListExtensions.AsReadOnlySpan(list).IsEmpty);
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AsSpan_ElementTypes_RoundTripBitExact()
        {
            AssertSpansRoundTripBitExact<byte>(a: 0xA5, b: 0xFF, c: 0x01);
            AssertSpansRoundTripBitExact(
                  a: new Rgb { r = 1, g = 2, b = 3 }
                , b: new Rgb { r = 255, g = 254, b = 253 }
                , c: new Rgb { r = 7, g = 0, b = 9 }
            );
            AssertSpansRoundTripBitExact(a: long.MinValue, b: long.MaxValue, c: -1L);
            AssertSpansRoundTripBitExact(
                  a: new Wide { a = 1, b = -2, c = long.MaxValue }
                , b: new Wide { a = long.MinValue, b = 0, c = -1 }
                , c: new Wide { a = 5, b = 6, c = 7 }
            );
        }

        [Test]
        public void AsSpan_TwoViews_SeeWritesThroughEachOtherAndTheList()
        {
            var list = new NativeList<int>(4, AllocatorManager.Persistent);

            try
            {
                list.Add(1);
                list.Add(2);
                list.Add(3);

                var writable = EncosyNativeListExtensions.AsSpan(list);
                var readOnly = EncosyNativeListExtensions.AsReadOnlySpan(list);

                writable[1] = 20;

                Assert.AreEqual(20, readOnly[1]);
                Assert.AreEqual(20, list[1]);

                list[2] = 30;

                Assert.AreEqual(30, writable[2]);
                Assert.AreEqual(30, readOnly[2]);
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void AsSpan_ViewsOnlyTheLength_NotTheCapacity()
        {
            var list = new NativeList<int>(16, AllocatorManager.Persistent);

            try
            {
                list.Add(1);
                list.Add(2);

                Assert.AreEqual(2, EncosyNativeListExtensions.AsSpan(list).Length);
                Assert.AreEqual(2, EncosyNativeListExtensions.AsReadOnlySpan(list).Length);
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AsSpan_AfterDispose_Throws()
        {
            var list = new NativeList<int>(4, AllocatorManager.Persistent);
            list.Add(1);
            list.Dispose();

            Assert.Catch(() => EncosyNativeListExtensions.AsSpan(list));
            Assert.Catch(() => EncosyNativeListExtensions.AsReadOnlySpan(list));
        }

        private static void AssertAddRangeCopiesBitExact<T>(T a, T b, T c)
            where T : unmanaged
        {
            var list = new NativeList<T>(1, AllocatorManager.Persistent);

            try
            {
                var items = new[] { a, b, c };

                EncosyNativeListExtensions.AddRange<T>(list, items);

                Assert.AreEqual(3, list.Length);
                Assert.AreEqual(a, list[0]);
                Assert.AreEqual(b, list[1]);
                Assert.AreEqual(c, list[2]);
            }
            finally
            {
                list.Dispose();
            }
        }

        private static void AssertSpansRoundTripBitExact<T>(T a, T b, T c)
            where T : unmanaged
        {
            var list = new NativeList<T>(3, AllocatorManager.Persistent);

            try
            {
                list.Resize(3, NativeArrayOptions.ClearMemory);

                var writable = EncosyNativeListExtensions.AsSpan(list);
                writable[0] = a;
                writable[1] = b;
                writable[2] = c;

                var readOnly = EncosyNativeListExtensions.AsReadOnlySpan(list);

                Assert.AreEqual(3, readOnly.Length);
                Assert.AreEqual(a, readOnly[0]);
                Assert.AreEqual(b, readOnly[1]);
                Assert.AreEqual(c, readOnly[2]);
                Assert.AreEqual(a, list[0]);
                Assert.AreEqual(b, list[1]);
                Assert.AreEqual(c, list[2]);
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
    }
}

#endif
