// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using System.Collections.Generic;
using EncosyTower.Collections;
using NUnit.Framework;

using SharedListAPI = EncosyTower.Collections.Extensions.SharedListExtensions;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedListExtensionsTests
    {
        [Test]
        public void ContainsAndRemove_ComparerFindsAndRemovesValues()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            var comparer = new IntComparer();
            var three = 3;
            var seven = 7;

            Assert.IsTrue(SharedListAPI.Contains(list, 3, comparer));
            Assert.IsTrue(SharedListAPI.Contains(list, in three, comparer));
            Assert.IsFalse(SharedListAPI.Contains(list, 4, comparer));
            Assert.IsTrue(SharedListAPI.Remove(list, 3, comparer));
            Assert.IsTrue(SharedListAPI.Remove(list, in seven, comparer));
            Assert.IsFalse(SharedListAPI.Remove(list, 9, comparer));
            CollectionAssert.AreEqual(new[] { 1, 5 }, list.ToArray());
        }

        [Test]
        public void BinarySearch_FullAndRangeReturnExpectedResults()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            var comparer = new IntComparer();
            var five = 5;
            var seven = 7;

            Assert.AreEqual(2, SharedListAPI.BinarySearch(list, 5, comparer));
            Assert.AreEqual(2, SharedListAPI.BinarySearch(list, in five, comparer));
            Assert.Less(SharedListAPI.BinarySearch(list, 4, comparer), 0);
            Assert.AreEqual(1, SharedListAPI.BinarySearch(list, 1, 2, 3, comparer));
            Assert.Less(SharedListAPI.BinarySearch(list, 1, 2, in seven, comparer), 0);
            Assert.Less(SharedListAPI.BinarySearch(list, 1, 2, 7, comparer), 0);
        }

        [Test]
        public void IndexOf_OverloadsRespectStartAndCount()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            var comparer = new IntComparer();
            var seven = 7;

            Assert.AreEqual(2, SharedListAPI.IndexOf(list, 5));
            Assert.AreEqual(-1, SharedListAPI.IndexOf(list, 4));
            Assert.AreEqual(2, SharedListAPI.IndexOf(list, 5, 2));
            Assert.AreEqual(-1, SharedListAPI.IndexOf(list, 5, 3));
            Assert.AreEqual(2, SharedListAPI.IndexOf(list, 5, 1, 2));
            Assert.AreEqual(3, SharedListAPI.IndexOf(list, 7, comparer));
            Assert.AreEqual(3, SharedListAPI.IndexOf(list, in seven, comparer));
        }

        [Test]
        public void Sort_FullAndRangeSortAscending()
        {
            var comparer = new IntComparer();
            using var full = new SharedList<int>(7, 5, 3, 1);
            using var range = new SharedList<int>(9, 7, 5, 3, 1);

            SharedListAPI.Sort(full, comparer);
            SharedListAPI.Sort(range, 1, 3, comparer);

            CollectionAssert.AreEqual(new[] { 1, 3, 5, 7 }, full.ToArray());
            CollectionAssert.AreEqual(new[] { 9, 3, 5, 7, 1 }, range.ToArray());
        }

        [Test]
        public void EmptyList_Searches_ReportNothing()
        {
            using var list = new SharedList<int>(4);
            var comparer = new IntComparer();
            var item = 1;

            Assert.IsFalse(SharedListAPI.Contains(list, 1, comparer));
            Assert.IsFalse(SharedListAPI.Contains(list, in item, comparer));
            Assert.IsFalse(SharedListAPI.Remove(list, 1, comparer));
            Assert.IsFalse(SharedListAPI.Remove(list, in item, comparer));
            Assert.AreEqual(-1, SharedListAPI.IndexOf(list, 1));
            Assert.AreEqual(-1, SharedListAPI.IndexOf(list, 1, 0));
            Assert.AreEqual(-1, SharedListAPI.IndexOf(list, 1, 0, 0));
            Assert.AreEqual(-1, SharedListAPI.IndexOf(list, 1, comparer));
            Assert.AreEqual(-1, SharedListAPI.IndexOf(list, in item, comparer));
            Assert.Less(SharedListAPI.BinarySearch(list, 1, comparer), 0);
            Assert.Less(SharedListAPI.BinarySearch(list, in item, comparer), 0);
            Assert.Less(SharedListAPI.BinarySearch(list, 0, 0, 1, comparer), 0);
            Assert.DoesNotThrow(() => SharedListAPI.Sort(list, comparer));
            Assert.DoesNotThrow(() => SharedListAPI.Sort(list, 0, 0, comparer));
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void IndexOf_RangeEndingAtTheLastElement_FindsItAndAnEmptyRangeAtTheEndFindsNothing()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);

            Assert.AreEqual(3, SharedListAPI.IndexOf(list, 7, 3, 1));
            Assert.AreEqual(3, SharedListAPI.IndexOf(list, 7, 3));
            Assert.AreEqual(-1, SharedListAPI.IndexOf(list, 7, 4, 0));
            Assert.AreEqual(-1, SharedListAPI.IndexOf(list, 7, 4));
            Assert.AreEqual(-1, SharedListAPI.IndexOf(list, 1, 1, 3));
        }

        [Test]
        public void Remove_EveryPosition_MatchesList()
        {
            var source = new[] { 1, 3, 5, 7 };

            for (var i = 0; i < source.Length; i++)
            {
                using var list = new SharedList<int>(source);
                var expected = new List<int>(source);
                var comparer = new IntComparer();

                Assert.AreEqual(expected.Remove(source[i]), SharedListAPI.Remove(list, source[i], comparer), $"{i}");
                CollectionAssert.AreEqual(expected, list.ToArray(), $"{i}");
            }
        }

        [Test]
        public void Remove_InArgumentEveryPosition_MatchesList()
        {
            var source = new[] { 1, 3, 5, 7 };

            for (var i = 0; i < source.Length; i++)
            {
                using var list = new SharedList<int>(source);
                var expected = new List<int>(source);
                var comparer = new IntComparer();
                var item = source[i];

                Assert.AreEqual(expected.Remove(item), SharedListAPI.Remove(list, in item, comparer), $"{i}");
                CollectionAssert.AreEqual(expected, list.ToArray(), $"{i}");
            }
        }

        [Test]
        public void Remove_OnlyElement_LeavesAnEmptyList()
        {
            using var list = new SharedList<int>(new[] { 5 });
            var comparer = new IntComparer();

            Assert.IsTrue(SharedListAPI.Remove(list, 5, comparer));
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void Remove_DuplicateItems_RemovesOnlyTheFirstOccurrence()
        {
            using var list = new SharedList<int>(2, 4, 2, 4);
            var comparer = new IntComparer();

            Assert.IsTrue(SharedListAPI.Remove(list, 4, comparer));
            CollectionAssert.AreEqual(new[] { 2, 2, 4 }, list.ToArray());
        }

        [Test]
        public void Remove_ItemNotPresent_LeavesTheContentUnchanged()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            var comparer = new IntComparer();

            Assert.IsFalse(SharedListAPI.Remove(list, 9, comparer));
            CollectionAssert.AreEqual(new[] { 1, 3, 5, 7 }, list.ToArray());
        }

        [Test]
        public void Remove_ItemNotPresent_DoesNotInvalidateEnumeration()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            var comparer = new IntComparer();
            SharedList<int, int>.Enumerator enumerator;

            // SAFETY: list outlives the enumerator, which is only used inside this test.
            unsafe
            {
                enumerator = list.GetEnumerator();
            }

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsFalse(SharedListAPI.Remove(list, 9, comparer));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);
        }

        [Test]
        public void Remove_InArgumentItemNotPresent_DoesNotInvalidateEnumeration()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            var comparer = new IntComparer();
            var absent = 9;
            SharedList<int, int>.Enumerator enumerator;

            // SAFETY: list outlives the enumerator, which is only used inside this test.
            unsafe
            {
                enumerator = list.GetEnumerator();
            }

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsFalse(SharedListAPI.Remove(list, in absent, comparer));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Remove_ItemPresent_InvalidatesEnumeration()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            var comparer = new IntComparer();
            SharedList<int, int>.Enumerator enumerator;

            // SAFETY: list outlives the enumerator, which is only used inside this test.
            unsafe
            {
                enumerator = list.GetEnumerator();
            }

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(SharedListAPI.Remove(list, 3, comparer));
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Test]
        public void NullList_Throws()
        {
            var comparer = new IntComparer();
            var item = 1;

            Assert.Throws<ArgumentNullException>(
                () => SharedListAPI.Contains<int, int, IntComparer>(null, 1, comparer)
            );
            Assert.Throws<ArgumentNullException>(
                () => SharedListAPI.Contains<int, int, IntComparer>(null, in item, comparer)
            );
            Assert.Throws<ArgumentNullException>(
                () => SharedListAPI.Remove<int, int, IntComparer>(null, 1, comparer)
            );
            Assert.Throws<ArgumentNullException>(
                () => SharedListAPI.Remove<int, int, IntComparer>(null, in item, comparer)
            );
            Assert.Throws<ArgumentNullException>(() => SharedListAPI.IndexOf<int, int>(null, 1));
            Assert.Throws<ArgumentNullException>(
                () => SharedListAPI.IndexOf<int, int, IntComparer>(null, 1, comparer)
            );
            Assert.Throws<ArgumentNullException>(
                () => SharedListAPI.BinarySearch<int, int, IntComparer>(null, 1, comparer)
            );
            Assert.Throws<ArgumentNullException>(() => SharedListAPI.Sort<int, int, IntComparer>(null, comparer));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_InvalidRange_Throws()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);

            Assert.Throws<InvalidOperationException>(() => SharedListAPI.IndexOf(list, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.IndexOf(list, 1, 5));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.IndexOf(list, 1, 0, 5));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.IndexOf(list, 1, 3, 2));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.IndexOf(list, 1, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.IndexOf(list, 1, -1, 2));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_RangeBeyondCountButWithinCapacity_Throws()
        {
            using var list = new SharedList<int>(8);

            list.Add(1);
            list.Add(3);

            Assert.Throws<InvalidOperationException>(() => SharedListAPI.IndexOf(list, 1, 0, 3));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.IndexOf(list, 1, 3));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void BinarySearch_InvalidRange_Throws()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            var comparer = new IntComparer();
            var item = 5;

            Assert.Throws<InvalidOperationException>(() => SharedListAPI.BinarySearch(list, -1, 2, 5, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.BinarySearch(list, 0, -1, 5, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.BinarySearch(list, 3, 2, 5, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.BinarySearch(list, 5, 0, 5, comparer));
            Assert.Throws<InvalidOperationException>(
                () => SharedListAPI.BinarySearch(list, -1, 2, in item, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedListAPI.BinarySearch(list, 3, 2, in item, comparer)
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Sort_InvalidRange_ThrowsAndKeepsTheContent()
        {
            using var list = new SharedList<int>(7, 5, 3, 1);
            var comparer = new IntComparer();

            Assert.Throws<InvalidOperationException>(() => SharedListAPI.Sort(list, -1, 2, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.Sort(list, 0, -1, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.Sort(list, 3, 2, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedListAPI.Sort(list, 5, 0, comparer));
            CollectionAssert.AreEqual(new[] { 7, 5, 3, 1 }, list.ToArray());
        }

        [Test]
        public void Sort_SingleElementOrEmptyRange_KeepsTheContent()
        {
            using var list = new SharedList<int>(9, 7, 5);
            var comparer = new IntComparer();

            SharedListAPI.Sort(list, 1, 0, comparer);
            SharedListAPI.Sort(list, 3, 0, comparer);
            SharedListAPI.Sort(list, 2, 1, comparer);

            CollectionAssert.AreEqual(new[] { 9, 7, 5 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AfterDispose_Searches_Throw()
        {
            var list = new SharedList<int>(1, 3, 5, 7);
            var comparer = new IntComparer();

            list.Dispose();

            Assert.Catch(() => SharedListAPI.Contains(list, 3, comparer));
            Assert.Catch(() => SharedListAPI.IndexOf(list, 3));
            Assert.Catch(() => SharedListAPI.BinarySearch(list, 3, comparer));
            Assert.Catch(() => SharedListAPI.Sort(list, comparer));
        }
    }

    internal readonly struct IntComparer : IComparer<int>, IEqualityComparer<int>
    {
        public int Compare(int x, int y)
            => x.CompareTo(y);

        public bool Equals(int x, int y)
            => x == y;

        public int GetHashCode(int value)
            => value;
    }
}
