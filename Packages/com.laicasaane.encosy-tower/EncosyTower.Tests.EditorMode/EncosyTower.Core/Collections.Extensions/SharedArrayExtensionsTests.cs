// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

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
    }
}
