// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using EncosyTower.Collections;
using NUnit.Framework;

using NativeReadOnlyAPI = EncosyTower.Collections.Extensions.SharedListNativeReadOnlyExtensions;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedListNativeReadOnlyExtensionsTests
    {
        [Test]
        public void Contains_OverloadsFindExpectedValues()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed read-only native view is consumed.
            unsafe
            {
                view = list.AsNative().AsReadOnly();
            }
            var three = 3;

            Assert.IsTrue(NativeReadOnlyAPI.Contains(in view, 3));
            Assert.IsTrue(NativeReadOnlyAPI.Contains(in view, in three));
            Assert.IsFalse(NativeReadOnlyAPI.Contains(in view, 4));
        }

        [Test]
        public void BinarySearch_FullAndRangeReturnExpectedResults()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed read-only native view is consumed.
            unsafe
            {
                view = list.AsNative().AsReadOnly();
            }
            var comparer = new IntComparer();
            var five = 5;
            var seven = 7;

            Assert.AreEqual(2, NativeReadOnlyAPI.BinarySearch(in view, 5, comparer));
            Assert.AreEqual(2, NativeReadOnlyAPI.BinarySearch(in view, in five, comparer));
            Assert.Less(NativeReadOnlyAPI.BinarySearch(in view, 4, comparer), 0);
            Assert.AreEqual(1, NativeReadOnlyAPI.BinarySearch(in view, 1, 2, 3, comparer));
            Assert.Less(NativeReadOnlyAPI.BinarySearch(in view, 1, 2, in seven, comparer), 0);
            Assert.Less(NativeReadOnlyAPI.BinarySearch(in view, 1, 2, 7, comparer), 0);
        }

        [Test]
        public void IndexOf_OverloadsRespectStartAndCount()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed read-only native view is consumed.
            unsafe
            {
                view = list.AsNative().AsReadOnly();
            }
            var comparer = new IntComparer();
            var five = 5;

            Assert.AreEqual(2, NativeReadOnlyAPI.IndexOf(in view, 5));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, 4));
            Assert.AreEqual(2, NativeReadOnlyAPI.IndexOf(in view, 5, 2));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, 5, 3));
            Assert.AreEqual(2, NativeReadOnlyAPI.IndexOf(in view, 5, 1, 2));
            Assert.AreEqual(2, NativeReadOnlyAPI.IndexOf(in view, in five));
            Assert.AreEqual(2, NativeReadOnlyAPI.IndexOf(in view, in five, 2));
            Assert.AreEqual(2, NativeReadOnlyAPI.IndexOf(in view, in five, 1, 2));
            Assert.AreEqual(2, NativeReadOnlyAPI.IndexOf(in view, 5, comparer));
            Assert.AreEqual(2, NativeReadOnlyAPI.IndexOf(in view, in five, comparer));
        }

        [Test]
        public void UncreatedView_Searches_ThrowArgumentException()
        {
            var view = default(SharedListNative<int>.ReadOnly);
            var comparer = new IntComparer();
            var item = 1;

            Assert.Throws<ArgumentException>(() => NativeReadOnlyAPI.Contains(in view, 1));
            Assert.Throws<ArgumentException>(() => NativeReadOnlyAPI.Contains(in view, in item));
            Assert.Throws<ArgumentException>(() => NativeReadOnlyAPI.IndexOf(in view, 1));
            Assert.Throws<ArgumentException>(() => NativeReadOnlyAPI.IndexOf(in view, in item));
            Assert.Throws<ArgumentException>(() => NativeReadOnlyAPI.IndexOf(in view, 1, comparer));
            Assert.Throws<ArgumentException>(() => NativeReadOnlyAPI.IndexOf(in view, in item, comparer));
            Assert.Throws<ArgumentException>(() => NativeReadOnlyAPI.BinarySearch(in view, 1, comparer));
            Assert.Throws<ArgumentException>(() => NativeReadOnlyAPI.BinarySearch(in view, in item, comparer));
        }

        [Test]
        public void EmptyList_Searches_ReportNothing()
        {
            using var list = new SharedList<int>(4);
            SharedListNative<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative().AsReadOnly();
            }
            var comparer = new IntComparer();
            var item = 1;

            Assert.IsFalse(NativeReadOnlyAPI.Contains(in view, 1));
            Assert.IsFalse(NativeReadOnlyAPI.Contains(in view, in item));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, 1));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, in item));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, 1, 0, 0));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, in item, 0, 0));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, 1, comparer));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, in item, comparer));
            Assert.Less(NativeReadOnlyAPI.BinarySearch(in view, 1, comparer), 0);
            Assert.Less(NativeReadOnlyAPI.BinarySearch(in view, in item, comparer), 0);
            Assert.Less(NativeReadOnlyAPI.BinarySearch(in view, 0, 0, 1, comparer), 0);
        }

        [Test]
        public void IndexOf_RangeEndingAtTheLastElement_FindsItAndAnEmptyRangeAtTheEndFindsNothing()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative().AsReadOnly();
            }

            Assert.AreEqual(3, NativeReadOnlyAPI.IndexOf(in view, 7, 3, 1));
            Assert.AreEqual(3, NativeReadOnlyAPI.IndexOf(in view, 7, 3));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, 7, 4, 0));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, 7, 4));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, 1, 1, 3));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_InvalidRange_Throws()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative().AsReadOnly();
            }
            var item = 1;

            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.IndexOf(in view, 1, -1));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.IndexOf(in view, 1, 5));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.IndexOf(in view, 1, 0, 5));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.IndexOf(in view, 1, 3, 2));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.IndexOf(in view, 1, 1, -1));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.IndexOf(in view, 1, -1, 2));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.IndexOf(in view, in item, -1));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.IndexOf(in view, in item, 0, 5));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_RangeBeyondCountButWithinCapacity_Throws()
        {
            using var list = new SharedList<int>(8);

            list.Add(1);
            list.Add(3);

            SharedListNative<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative().AsReadOnly();
            }

            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.IndexOf(in view, 1, 0, 3));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.IndexOf(in view, 1, 3));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void BinarySearch_InvalidRange_Throws()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int>.ReadOnly view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative().AsReadOnly();
            }
            var comparer = new IntComparer();
            var item = 5;

            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.BinarySearch(in view, -1, 2, 5, comparer));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.BinarySearch(in view, 0, -1, 5, comparer));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.BinarySearch(in view, 3, 2, 5, comparer));
            Assert.Throws<InvalidOperationException>(() => NativeReadOnlyAPI.BinarySearch(in view, 5, 0, 5, comparer));
            Assert.Throws<InvalidOperationException>(
                () => NativeReadOnlyAPI.BinarySearch(in view, -1, 2, in item, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => NativeReadOnlyAPI.BinarySearch(in view, 3, 2, in item, comparer)
            );
        }
    }
}
