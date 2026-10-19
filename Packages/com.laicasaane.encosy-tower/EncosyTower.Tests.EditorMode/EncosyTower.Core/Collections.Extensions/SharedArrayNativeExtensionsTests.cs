// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
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

        [Test]
        public void UncreatedView_Searches_ThrowArgumentException()
        {
            var view = default(SharedArrayNative<int>);
            var comparer = new IntComparer();
            var item = 1;

            Assert.Throws<ArgumentException>(() => SharedArrayNativeAPI.Contains(in view, 1));
            Assert.Throws<ArgumentException>(() => SharedArrayNativeAPI.Contains(in view, in item));
            Assert.Throws<ArgumentException>(() => SharedArrayNativeAPI.IndexOf(in view, 1));
            Assert.Throws<ArgumentException>(() => SharedArrayNativeAPI.IndexOf(in view, in item));
            Assert.Throws<ArgumentException>(() => SharedArrayNativeAPI.IndexOf(in view, 1, comparer));
            Assert.Throws<ArgumentException>(() => SharedArrayNativeAPI.IndexOf(in view, in item, comparer));
            Assert.Throws<ArgumentException>(() => SharedArrayNativeAPI.BinarySearch(in view, 1, comparer));
            Assert.Throws<ArgumentException>(() => SharedArrayNativeAPI.BinarySearch(in view, in item, comparer));
            Assert.Throws<ArgumentException>(() => SharedArrayNativeAPI.Sort(in view, comparer));
        }

        [Test]
        public void IndexOf_RangeEndingAtTheLastElement_FindsItAndAnEmptyRangeAtTheEndFindsNothing()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArrayNative<int> view;

            // SAFETY: array remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = array.AsNative();
            }

            Assert.AreEqual(3, SharedArrayNativeAPI.IndexOf(in view, 7, 3, 1));
            Assert.AreEqual(3, SharedArrayNativeAPI.IndexOf(in view, 7, 3));
            Assert.AreEqual(-1, SharedArrayNativeAPI.IndexOf(in view, 7, 4, 0));
            Assert.AreEqual(-1, SharedArrayNativeAPI.IndexOf(in view, 7, 4));
            Assert.AreEqual(-1, SharedArrayNativeAPI.IndexOf(in view, 1, 1, 3));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_InvalidRange_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArrayNative<int> view;

            // SAFETY: array remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = array.AsNative();
            }

            var item = 1;

            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.IndexOf(in view, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.IndexOf(in view, 1, 5));
            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.IndexOf(in view, 1, 0, 5));
            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.IndexOf(in view, 1, 3, 2));
            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.IndexOf(in view, 1, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.IndexOf(in view, 1, -1, 2));
            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.IndexOf(in view, in item, -1));
            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.IndexOf(in view, in item, 0, 5));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_IndexPlusCountOverflowingInt_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArrayNative<int> view;

            // SAFETY: array remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = array.AsNative();
            }

            Assert.Catch(() => SharedArrayNativeAPI.IndexOf(in view, 1, int.MaxValue, 1));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void BinarySearchAndSort_InvalidRange_ThrowAndKeepTheContent()
        {
            using var array = new SharedArray<int>(new[] { 7, 5, 3, 1 });
            SharedArrayNative<int> view;

            // SAFETY: array remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = array.AsNative();
            }

            var comparer = new IntComparer();
            var item = 5;

            Assert.Throws<InvalidOperationException>(
                () => SharedArrayNativeAPI.BinarySearch(in view, -1, 2, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedArrayNativeAPI.BinarySearch(in view, 0, -1, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedArrayNativeAPI.BinarySearch(in view, 3, 2, in item, comparer)
            );
            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.Sort(in view, -1, 2, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.Sort(in view, 0, -1, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedArrayNativeAPI.Sort(in view, 3, 2, comparer));
            CollectionAssert.AreEqual(new[] { 7, 5, 3, 1 }, array.AsManagedArray());
        }

        [Test]
        public void Sort_SingleElementOrEmptyRange_KeepsTheContent()
        {
            using var array = new SharedArray<int>(new[] { 9, 7, 5 });
            SharedArrayNative<int> view;

            // SAFETY: array remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = array.AsNative();
            }

            var comparer = new IntComparer();

            SharedArrayNativeAPI.Sort(in view, 1, 0, comparer);
            SharedArrayNativeAPI.Sort(in view, 3, 0, comparer);
            SharedArrayNativeAPI.Sort(in view, 2, 1, comparer);

            CollectionAssert.AreEqual(new[] { 9, 7, 5 }, array.AsManagedArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AfterOwnerDispose_Searches_Throw()
        {
            var array = new SharedArray<int>(new[] { 7, 5, 3, 1 });
            SharedArrayNative<int> view;

            // SAFETY: array stays alive until the Dispose below, after which every view access is rejected.
            unsafe
            {
                view = array.AsNative();
            }

            var comparer = new IntComparer();

            array.Dispose();

            Assert.Catch(() => SharedArrayNativeAPI.Contains(in view, 3));
            Assert.Catch(() => SharedArrayNativeAPI.IndexOf(in view, 3));
            Assert.Catch(() => SharedArrayNativeAPI.IndexOf(in view, 3, comparer));
            Assert.Catch(() => SharedArrayNativeAPI.BinarySearch(in view, 3, comparer));
            Assert.Catch(() => SharedArrayNativeAPI.Sort(in view, comparer));
        }
    }
}
