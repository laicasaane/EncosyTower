// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using EncosyTower.Collections;
using NUnit.Framework;

using SharedArrayReadOnlyAPI = EncosyTower.Collections.Extensions.SharedArrayReadOnlyExtensions;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArrayReadOnlyExtensionsTests
    {
        [Test]
        public void ContainsAndIndexOf_FindExpectedValues()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArray<int>.ReadOnly view = array;
            var comparer = new IntComparer();
            var three = 3;

            Assert.IsTrue(SharedArrayReadOnlyAPI.Contains(view, 3));
            Assert.IsTrue(SharedArrayReadOnlyAPI.Contains(view, in three));
            Assert.IsTrue(SharedArrayReadOnlyAPI.Contains(view, 3, comparer));
            Assert.IsTrue(SharedArrayReadOnlyAPI.Contains(view, in three, comparer));
            Assert.IsFalse(SharedArrayReadOnlyAPI.Contains(view, 4));
            Assert.AreEqual(2, SharedArrayReadOnlyAPI.IndexOf(view, 5));
            Assert.AreEqual(-1, SharedArrayReadOnlyAPI.IndexOf(view, 5, 3));
            Assert.AreEqual(2, SharedArrayReadOnlyAPI.IndexOf(view, 5, 1, 2));
        }

        [Test]
        public void BinarySearch_FullAndRangeReturnExpectedResults()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArray<int>.ReadOnly view = array;
            var comparer = new IntComparer();
            var five = 5;

            Assert.AreEqual(2, SharedArrayReadOnlyAPI.BinarySearch(view, 5, comparer));
            Assert.AreEqual(2, SharedArrayReadOnlyAPI.BinarySearch(view, in five, comparer));
            Assert.Less(SharedArrayReadOnlyAPI.BinarySearch(view, 4, comparer), 0);
            Assert.AreEqual(1, SharedArrayReadOnlyAPI.BinarySearch(view, 1, 2, 3, comparer));
        }
    }
}
