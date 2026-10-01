// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using EncosyTower.Collections;
using NUnit.Framework;

using SharedArrayNativeAPI = EncosyTower.Collections.Extensions.SharedArrayNativeExtensions;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArrayNativeExtensionsTests
    {
        [Test]
        public void ContainsAndIndexOf_FindExpectedValues()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArrayNative<int> view;

            // SAFETY: array remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = array.AsNative();
            }
            var comparer = new IntComparer();
            var three = 3;

            Assert.IsTrue(SharedArrayNativeAPI.Contains(in view, 3));
            Assert.IsTrue(SharedArrayNativeAPI.Contains(in view, in three));
            Assert.IsFalse(SharedArrayNativeAPI.Contains(in view, 4));
            Assert.AreEqual(2, SharedArrayNativeAPI.IndexOf(in view, 5));
            Assert.AreEqual(-1, SharedArrayNativeAPI.IndexOf(in view, 5, 3));
            Assert.AreEqual(2, SharedArrayNativeAPI.IndexOf(in view, 5, 1, 2));
            Assert.AreEqual(1, SharedArrayNativeAPI.IndexOf(in view, 3, comparer));
            Assert.AreEqual(1, SharedArrayNativeAPI.IndexOf(in view, in three, comparer));
        }

        [Test]
        public void BinarySearchAndSort_ReturnExpectedResults()
        {
            using var sorted = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            using var unsorted = new SharedArray<int>(new[] { 7, 5, 3, 1 });
            SharedArrayNative<int> sortedView;
            SharedArrayNative<int> unsortedView;

            // SAFETY: Both arrays remain alive while their borrowed native views are consumed.
            unsafe
            {
                sortedView = sorted.AsNative();
                unsortedView = unsorted.AsNative();
            }
            var comparer = new IntComparer();
            var five = 5;

            Assert.AreEqual(2, SharedArrayNativeAPI.BinarySearch(in sortedView, in five, comparer));
            SharedArrayNativeAPI.Sort(in unsortedView, comparer);
            CollectionAssert.AreEqual(new[] { 1, 3, 5, 7 }, unsorted.AsManagedArray());
        }
    }
}
