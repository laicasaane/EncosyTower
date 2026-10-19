// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
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

        [Test]
        public void UncreatedView_Searches_ThrowArgumentException()
        {
            var view = default(SharedArray<int>.ReadOnly);
            var comparer = new IntComparer();
            var item = 1;

            Assert.Throws<ArgumentException>(() => SharedArrayReadOnlyAPI.Contains(view, 1));
            Assert.Throws<ArgumentException>(() => SharedArrayReadOnlyAPI.Contains(view, in item));
            Assert.Throws<ArgumentException>(() => SharedArrayReadOnlyAPI.Contains(view, 1, comparer));
            Assert.Throws<ArgumentException>(() => SharedArrayReadOnlyAPI.Contains(view, in item, comparer));
            Assert.Throws<ArgumentException>(() => SharedArrayReadOnlyAPI.IndexOf(view, 1));
            Assert.Throws<ArgumentException>(() => SharedArrayReadOnlyAPI.IndexOf(view, in item));
            Assert.Throws<ArgumentException>(() => SharedArrayReadOnlyAPI.IndexOf(view, 1, comparer));
            Assert.Throws<ArgumentException>(() => SharedArrayReadOnlyAPI.IndexOf(view, in item, comparer));
            Assert.Throws<ArgumentException>(() => SharedArrayReadOnlyAPI.BinarySearch(view, 1, comparer));
            Assert.Throws<ArgumentException>(() => SharedArrayReadOnlyAPI.BinarySearch(view, in item, comparer));
        }

        [Test]
        public void EmptyArray_Searches_ReportNothing()
        {
            using var array = new SharedArray<int>(new int[0]);
            SharedArray<int>.ReadOnly view = array;
            var comparer = new IntComparer();
            var item = 1;

            Assert.IsFalse(SharedArrayReadOnlyAPI.Contains(view, 1));
            Assert.IsFalse(SharedArrayReadOnlyAPI.Contains(view, in item));
            Assert.IsFalse(SharedArrayReadOnlyAPI.Contains(view, 1, comparer));
            Assert.IsFalse(SharedArrayReadOnlyAPI.Contains(view, in item, comparer));
            Assert.AreEqual(-1, SharedArrayReadOnlyAPI.IndexOf(view, 1));
            Assert.AreEqual(-1, SharedArrayReadOnlyAPI.IndexOf(view, in item));
            Assert.AreEqual(-1, SharedArrayReadOnlyAPI.IndexOf(view, 1, 0, 0));
            Assert.AreEqual(-1, SharedArrayReadOnlyAPI.IndexOf(view, in item, 0, 0));
            Assert.AreEqual(-1, SharedArrayReadOnlyAPI.IndexOf(view, 1, comparer));
            Assert.AreEqual(-1, SharedArrayReadOnlyAPI.IndexOf(view, in item, comparer));
            Assert.Less(SharedArrayReadOnlyAPI.BinarySearch(view, 1, comparer), 0);
            Assert.Less(SharedArrayReadOnlyAPI.BinarySearch(view, in item, comparer), 0);
            Assert.Less(SharedArrayReadOnlyAPI.BinarySearch(view, 0, 0, 1, comparer), 0);
        }

        [Test]
        public void IndexOf_RangeEndingAtTheLastElement_FindsItAndAnEmptyRangeAtTheEndFindsNothing()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArray<int>.ReadOnly view = array;

            Assert.AreEqual(3, SharedArrayReadOnlyAPI.IndexOf(view, 7, 3, 1));
            Assert.AreEqual(3, SharedArrayReadOnlyAPI.IndexOf(view, 7, 3));
            Assert.AreEqual(-1, SharedArrayReadOnlyAPI.IndexOf(view, 7, 4, 0));
            Assert.AreEqual(-1, SharedArrayReadOnlyAPI.IndexOf(view, 7, 4));
            Assert.AreEqual(-1, SharedArrayReadOnlyAPI.IndexOf(view, 1, 1, 3));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_InvalidRange_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArray<int>.ReadOnly view = array;
            var item = 1;

            Assert.Throws<InvalidOperationException>(() => SharedArrayReadOnlyAPI.IndexOf(view, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedArrayReadOnlyAPI.IndexOf(view, 1, 5));
            Assert.Throws<InvalidOperationException>(() => SharedArrayReadOnlyAPI.IndexOf(view, 1, 0, 5));
            Assert.Throws<InvalidOperationException>(() => SharedArrayReadOnlyAPI.IndexOf(view, 1, 3, 2));
            Assert.Throws<InvalidOperationException>(() => SharedArrayReadOnlyAPI.IndexOf(view, 1, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedArrayReadOnlyAPI.IndexOf(view, 1, -1, 2));
            Assert.Throws<InvalidOperationException>(() => SharedArrayReadOnlyAPI.IndexOf(view, in item, -1));
            Assert.Throws<InvalidOperationException>(() => SharedArrayReadOnlyAPI.IndexOf(view, in item, 0, 5));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_IndexPlusCountOverflowingInt_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArray<int>.ReadOnly view = array;

            Assert.Catch(() => SharedArrayReadOnlyAPI.IndexOf(view, 1, int.MaxValue, 1));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void BinarySearch_InvalidRange_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArray<int>.ReadOnly view = array;
            var comparer = new IntComparer();
            var item = 5;

            Assert.Throws<InvalidOperationException>(
                () => SharedArrayReadOnlyAPI.BinarySearch(view, -1, 2, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedArrayReadOnlyAPI.BinarySearch(view, 0, -1, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedArrayReadOnlyAPI.BinarySearch(view, 3, 2, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedArrayReadOnlyAPI.BinarySearch(view, -1, 2, in item, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedArrayReadOnlyAPI.BinarySearch(view, 3, 2, in item, comparer)
            );
        }

        [Test]
        public void AfterOwnerDispose_Searches_Throw()
        {
            var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArray<int>.ReadOnly view = array;
            var comparer = new IntComparer();

            array.Dispose();

            Assert.Catch(() => SharedArrayReadOnlyAPI.Contains(view, 3));
            Assert.Catch(() => SharedArrayReadOnlyAPI.Contains(view, 3, comparer));
            Assert.Catch(() => SharedArrayReadOnlyAPI.IndexOf(view, 3));
            Assert.Catch(() => SharedArrayReadOnlyAPI.IndexOf(view, 3, comparer));
            Assert.Catch(() => SharedArrayReadOnlyAPI.BinarySearch(view, 3, comparer));
        }
    }
}
