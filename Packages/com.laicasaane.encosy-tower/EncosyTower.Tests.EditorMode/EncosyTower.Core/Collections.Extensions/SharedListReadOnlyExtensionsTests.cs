// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using EncosyTower.Collections;
using NUnit.Framework;

using SharedListReadOnlyAPI = EncosyTower.Collections.Extensions.SharedListReadOnlyExtensions;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedListReadOnlyExtensionsTests
    {
        [Test]
        public void Contains_OverloadsFindExpectedValues()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedList<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed read-only view is consumed.
            unsafe
            {
                view = list.AsReadOnly();
            }
            var comparer = new IntComparer();
            var three = 3;

            Assert.IsTrue(SharedListReadOnlyAPI.Contains(view, 3));
            Assert.IsTrue(SharedListReadOnlyAPI.Contains(view, in three));
            Assert.IsTrue(SharedListReadOnlyAPI.Contains(view, 3, comparer));
            Assert.IsTrue(SharedListReadOnlyAPI.Contains(view, in three, comparer));
            Assert.IsFalse(SharedListReadOnlyAPI.Contains(view, 4));
            Assert.IsFalse(SharedListReadOnlyAPI.Contains(view, 4, comparer));
        }

        [Test]
        public void BinarySearch_FullAndRangeReturnExpectedResults()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedList<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed read-only view is consumed.
            unsafe
            {
                view = list.AsReadOnly();
            }
            var comparer = new IntComparer();
            var five = 5;
            var seven = 7;

            Assert.AreEqual(2, SharedListReadOnlyAPI.BinarySearch(view, 5, comparer));
            Assert.AreEqual(2, SharedListReadOnlyAPI.BinarySearch(view, in five, comparer));
            Assert.Less(SharedListReadOnlyAPI.BinarySearch(view, 4, comparer), 0);
            Assert.AreEqual(1, SharedListReadOnlyAPI.BinarySearch(view, 1, 2, 3, comparer));
            Assert.Less(SharedListReadOnlyAPI.BinarySearch(view, 1, 2, in seven, comparer), 0);
            Assert.Less(SharedListReadOnlyAPI.BinarySearch(view, 1, 2, 7, comparer), 0);
        }

        [Test]
        public void IndexOf_OverloadsRespectStartAndCount()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedList<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed read-only view is consumed.
            unsafe
            {
                view = list.AsReadOnly();
            }
            var comparer = new IntComparer();
            var five = 5;

            Assert.AreEqual(2, SharedListReadOnlyAPI.IndexOf(view, 5));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, 4));
            Assert.AreEqual(2, SharedListReadOnlyAPI.IndexOf(view, 5, 2));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, 5, 3));
            Assert.AreEqual(2, SharedListReadOnlyAPI.IndexOf(view, 5, 1, 2));
            Assert.AreEqual(2, SharedListReadOnlyAPI.IndexOf(view, in five));
            Assert.AreEqual(2, SharedListReadOnlyAPI.IndexOf(view, in five, 2));
            Assert.AreEqual(2, SharedListReadOnlyAPI.IndexOf(view, in five, 1, 2));
            Assert.AreEqual(2, SharedListReadOnlyAPI.IndexOf(view, 5, comparer));
            Assert.AreEqual(2, SharedListReadOnlyAPI.IndexOf(view, in five, comparer));
        }

        [Test]
        public void UncreatedView_Searches_ThrowArgumentException()
        {
            var view = default(SharedList<int>.ReadOnly);
            var comparer = new IntComparer();
            var item = 1;

            Assert.Throws<ArgumentException>(() => SharedListReadOnlyAPI.Contains(view, 1));
            Assert.Throws<ArgumentException>(() => SharedListReadOnlyAPI.Contains(view, in item));
            Assert.Throws<ArgumentException>(() => SharedListReadOnlyAPI.Contains(view, 1, comparer));
            Assert.Throws<ArgumentException>(() => SharedListReadOnlyAPI.Contains(view, in item, comparer));
            Assert.Throws<ArgumentException>(() => SharedListReadOnlyAPI.IndexOf(view, 1));
            Assert.Throws<ArgumentException>(() => SharedListReadOnlyAPI.IndexOf(view, in item));
            Assert.Throws<ArgumentException>(() => SharedListReadOnlyAPI.IndexOf(view, 1, comparer));
            Assert.Throws<ArgumentException>(() => SharedListReadOnlyAPI.IndexOf(view, in item, comparer));
            Assert.Throws<ArgumentException>(() => SharedListReadOnlyAPI.BinarySearch(view, 1, comparer));
            Assert.Throws<ArgumentException>(() => SharedListReadOnlyAPI.BinarySearch(view, in item, comparer));
        }

        [Test]
        public void EmptyList_Searches_ReportNothing()
        {
            using var list = new SharedList<int>(4);
            SharedList<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsReadOnly();
            }
            var comparer = new IntComparer();
            var item = 1;

            Assert.IsFalse(SharedListReadOnlyAPI.Contains(view, 1));
            Assert.IsFalse(SharedListReadOnlyAPI.Contains(view, in item));
            Assert.IsFalse(SharedListReadOnlyAPI.Contains(view, 1, comparer));
            Assert.IsFalse(SharedListReadOnlyAPI.Contains(view, in item, comparer));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, 1));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, in item));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, 1, 0, 0));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, in item, 0, 0));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, 1, comparer));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, in item, comparer));
            Assert.Less(SharedListReadOnlyAPI.BinarySearch(view, 1, comparer), 0);
            Assert.Less(SharedListReadOnlyAPI.BinarySearch(view, in item, comparer), 0);
            Assert.Less(SharedListReadOnlyAPI.BinarySearch(view, 0, 0, 1, comparer), 0);
        }

        [Test]
        public void IndexOf_RangeEndingAtTheLastElement_FindsItAndAnEmptyRangeAtTheEndFindsNothing()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedList<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsReadOnly();
            }

            Assert.AreEqual(3, SharedListReadOnlyAPI.IndexOf(view, 7, 3, 1));
            Assert.AreEqual(3, SharedListReadOnlyAPI.IndexOf(view, 7, 3));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, 7, 4, 0));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, 7, 4));
            Assert.AreEqual(-1, SharedListReadOnlyAPI.IndexOf(view, 1, 1, 3));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_InvalidRange_Throws()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedList<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsReadOnly();
            }
            var item = 1;

            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.IndexOf(view, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.IndexOf(view, 1, 5));
            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.IndexOf(view, 1, 0, 5));
            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.IndexOf(view, 1, 3, 2));
            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.IndexOf(view, 1, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.IndexOf(view, 1, -1, 2));
            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.IndexOf(view, in item, -1));
            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.IndexOf(view, in item, 0, 5));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_RangeBeyondCountButWithinCapacity_Throws()
        {
            using var list = new SharedList<int>(8);

            list.Add(1);
            list.Add(3);

            SharedList<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsReadOnly();
            }

            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.IndexOf(view, 1, 0, 3));
            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.IndexOf(view, 1, 3));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void BinarySearch_InvalidRange_Throws()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedList<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsReadOnly();
            }
            var comparer = new IntComparer();
            var item = 5;

            Assert.Throws<InvalidOperationException>(
                () => SharedListReadOnlyAPI.BinarySearch(view, -1, 2, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedListReadOnlyAPI.BinarySearch(view, 0, -1, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.BinarySearch(view, 3, 2, 5, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedListReadOnlyAPI.BinarySearch(view, 5, 0, 5, comparer));
            Assert.Throws<InvalidOperationException>(
                () => SharedListReadOnlyAPI.BinarySearch(view, -1, 2, in item, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedListReadOnlyAPI.BinarySearch(view, 3, 2, in item, comparer)
            );
        }
    }
}
