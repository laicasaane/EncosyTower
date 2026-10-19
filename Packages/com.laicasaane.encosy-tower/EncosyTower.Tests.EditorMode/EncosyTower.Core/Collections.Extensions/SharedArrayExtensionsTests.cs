// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using EncosyTower.Collections;
using NUnit.Framework;

using SharedArrayAPI = EncosyTower.Collections.Extensions.SharedArrayExtensions;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArrayExtensionsTests
    {
        [Test]
        public void ContainsAndIndexOf_FindExpectedValues()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            var comparer = new IntComparer();
            var three = 3;

            Assert.IsTrue(SharedArrayAPI.Contains(array, 3, comparer));
            Assert.IsTrue(SharedArrayAPI.Contains(array, in three, comparer));
            Assert.IsFalse(SharedArrayAPI.Contains(array, 4, comparer));
            Assert.AreEqual(2, SharedArrayAPI.IndexOf(array, 5));
            Assert.AreEqual(-1, SharedArrayAPI.IndexOf(array, 5, 3));
            Assert.AreEqual(2, SharedArrayAPI.IndexOf(array, 5, 1, 2));
            Assert.AreEqual(1, SharedArrayAPI.IndexOf(array, 3, comparer));
            Assert.AreEqual(1, SharedArrayAPI.IndexOf(array, in three, comparer));
        }

        [Test]
        public void BinarySearch_FullAndRangeReturnExpectedResults()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            var comparer = new IntComparer();
            var five = 5;
            var seven = 7;

            Assert.AreEqual(2, SharedArrayAPI.BinarySearch(array, 5, comparer));
            Assert.AreEqual(2, SharedArrayAPI.BinarySearch(array, in five, comparer));
            Assert.Less(SharedArrayAPI.BinarySearch(array, 4, comparer), 0);
            Assert.AreEqual(1, SharedArrayAPI.BinarySearch(array, 1, 2, 3, comparer));
            Assert.Less(SharedArrayAPI.BinarySearch(array, 1, 2, in seven, comparer), 0);
        }

        [Test]
        public void Sort_FullAndRangeSortAscending()
        {
            var comparer = new IntComparer();
            using var full = new SharedArray<int>(new[] { 7, 5, 3, 1 });
            using var range = new SharedArray<int>(new[] { 9, 7, 5, 3, 1 });

            SharedArrayAPI.Sort(full, comparer);
            SharedArrayAPI.Sort(range, 1, 3, comparer);

            CollectionAssert.AreEqual(new[] { 1, 3, 5, 7 }, full.AsManagedArray());
            CollectionAssert.AreEqual(new[] { 9, 3, 5, 7, 1 }, range.AsManagedArray());
        }

        [Test]
        public void EmptyArray_Searches_ReportNothing()
        {
            using var array = new SharedArray<int>(new int[0]);
            var comparer = new IntComparer();
            var item = 1;

            Assert.IsFalse(SharedArrayAPI.Contains(array, 1, comparer));
            Assert.IsFalse(SharedArrayAPI.Contains(array, in item, comparer));
            Assert.AreEqual(-1, SharedArrayAPI.IndexOf(array, 1));
            Assert.AreEqual(-1, SharedArrayAPI.IndexOf(array, 1, 0));
            Assert.AreEqual(-1, SharedArrayAPI.IndexOf(array, 1, 0, 0));
            Assert.AreEqual(-1, SharedArrayAPI.IndexOf(array, 1, comparer));
            Assert.AreEqual(-1, SharedArrayAPI.IndexOf(array, in item, comparer));
            Assert.Less(SharedArrayAPI.BinarySearch(array, 1, comparer), 0);
            Assert.Less(SharedArrayAPI.BinarySearch(array, in item, comparer), 0);
            Assert.Less(SharedArrayAPI.BinarySearch(array, 0, 0, 1, comparer), 0);
            Assert.DoesNotThrow(() => SharedArrayAPI.Sort(array, comparer));
            Assert.DoesNotThrow(() => SharedArrayAPI.Sort(array, 0, 0, comparer));
        }

        [Test]
        public void IndexOf_RangeEndingAtTheLastElement_FindsItAndAnEmptyRangeAtTheEndFindsNothing()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });

            Assert.AreEqual(3, SharedArrayAPI.IndexOf(array, 7, 3, 1));
            Assert.AreEqual(3, SharedArrayAPI.IndexOf(array, 7, 3));
            Assert.AreEqual(-1, SharedArrayAPI.IndexOf(array, 7, 4, 0));
            Assert.AreEqual(-1, SharedArrayAPI.IndexOf(array, 7, 4));
            Assert.AreEqual(-1, SharedArrayAPI.IndexOf(array, 1, 1, 3));
        }

        [Test]
        public void Sort_SingleElementOrEmptyRange_KeepsTheContent()
        {
            using var array = new SharedArray<int>(new[] { 9, 7, 5 });
            var comparer = new IntComparer();

            SharedArrayAPI.Sort(array, 1, 0, comparer);
            SharedArrayAPI.Sort(array, 3, 0, comparer);
            SharedArrayAPI.Sort(array, 2, 1, comparer);

            CollectionAssert.AreEqual(new[] { 9, 7, 5 }, array.AsManagedArray());
        }

        [Test]
        public void NullArray_Throws()
        {
            var comparer = new IntComparer();
            var item = 1;

            Assert.Throws<ArgumentNullException>(
                () => SharedArrayAPI.Contains<int, int, IntComparer>(null, 1, comparer)
            );
            Assert.Throws<ArgumentNullException>(
                () => SharedArrayAPI.Contains<int, int, IntComparer>(null, in item, comparer)
            );
            Assert.Throws<ArgumentNullException>(() => SharedArrayAPI.IndexOf<int, int>(null, 1));
            Assert.Throws<ArgumentNullException>(
                () => SharedArrayAPI.IndexOf<int, int, IntComparer>(null, 1, comparer)
            );
            Assert.Throws<ArgumentNullException>(
                () => SharedArrayAPI.BinarySearch<int, int, IntComparer>(null, 1, comparer)
            );
            Assert.Throws<ArgumentNullException>(() => SharedArrayAPI.Sort<int, int, IntComparer>(null, comparer));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_InvalidRange_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });

            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.IndexOf(array, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.IndexOf(array, 1, 5));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.IndexOf(array, 1, 0, 5));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.IndexOf(array, 1, 3, 2));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.IndexOf(array, 1, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.IndexOf(array, 1, -1, 2));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_IndexPlusCountOverflowingInt_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });

            Assert.Catch(() => SharedArrayAPI.IndexOf(array, 1, int.MaxValue, 1));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void BinarySearch_InvalidRange_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            var comparer = new IntComparer();
            var item = 5;

            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.BinarySearch(array, -1, 2, 5, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.BinarySearch(array, 0, -1, 5, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.BinarySearch(array, 3, 2, 5, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.BinarySearch(array, 5, 0, 5, comparer));
            Assert.Throws<InvalidOperationException>(
                () => SharedArrayAPI.BinarySearch(array, -1, 2, in item, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedArrayAPI.BinarySearch(array, 3, 2, in item, comparer)
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Sort_InvalidRange_ThrowsAndKeepsTheContent()
        {
            using var array = new SharedArray<int>(new[] { 7, 5, 3, 1 });
            var comparer = new IntComparer();

            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.Sort(array, -1, 2, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.Sort(array, 0, -1, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.Sort(array, 3, 2, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedArrayAPI.Sort(array, 5, 0, comparer));
            CollectionAssert.AreEqual(new[] { 7, 5, 3, 1 }, array.AsManagedArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AfterDispose_Searches_Throw()
        {
            var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            var comparer = new IntComparer();

            array.Dispose();

            Assert.Catch(() => SharedArrayAPI.Contains(array, 3, comparer));
            Assert.Catch(() => SharedArrayAPI.IndexOf(array, 3));
            Assert.Catch(() => SharedArrayAPI.IndexOf(array, 3, comparer));
            Assert.Catch(() => SharedArrayAPI.BinarySearch(array, 3, comparer));
            Assert.Catch(() => SharedArrayAPI.Sort(array, comparer));
        }
    }
}
