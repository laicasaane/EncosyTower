// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using System.Collections.Generic;
using EncosyTower.Collections;
using NUnit.Framework;

using SharedListNativeAPI = EncosyTower.Collections.Extensions.SharedListNativeExtensions;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedListNativeExtensionsTests
    {
        [Test]
        public void ContainsAndRemove_FindAndRemoveValues()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int> view;

            // SAFETY: list remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = list.AsNative();
            }
            var three = 3;
            var seven = 7;

            Assert.IsTrue(SharedListNativeAPI.Contains(in view, 3));
            Assert.IsTrue(SharedListNativeAPI.Contains(in view, in three));
            Assert.IsFalse(SharedListNativeAPI.Contains(in view, 4));
            Assert.IsTrue(SharedListNativeAPI.Remove(in view, 3));
            Assert.IsTrue(SharedListNativeAPI.Remove(in view, in seven));
            Assert.IsFalse(SharedListNativeAPI.Remove(in view, 9));
            CollectionAssert.AreEqual(new[] { 1, 5 }, list.ToArray());
        }

        [Test]
        public void BinarySearch_FullAndRangeReturnExpectedResults()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int> view;

            // SAFETY: list remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = list.AsNative();
            }
            var comparer = new IntComparer();
            var five = 5;
            var seven = 7;

            Assert.AreEqual(2, SharedListNativeAPI.BinarySearch(in view, 5, comparer));
            Assert.AreEqual(2, SharedListNativeAPI.BinarySearch(in view, in five, comparer));
            Assert.Less(SharedListNativeAPI.BinarySearch(in view, 4, comparer), 0);
            Assert.AreEqual(1, SharedListNativeAPI.BinarySearch(in view, 1, 2, 3, comparer));
            Assert.Less(SharedListNativeAPI.BinarySearch(in view, 1, 2, in seven, comparer), 0);
            Assert.Less(SharedListNativeAPI.BinarySearch(in view, 1, 2, 7, comparer), 0);
        }

        [Test]
        public void IndexOf_OverloadsRespectStartAndCount()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int> view;

            // SAFETY: list remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = list.AsNative();
            }
            var comparer = new IntComparer();
            var five = 5;

            Assert.AreEqual(2, SharedListNativeAPI.IndexOf(in view, 5));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, 4));
            Assert.AreEqual(2, SharedListNativeAPI.IndexOf(in view, 5, 2));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, 5, 3));
            Assert.AreEqual(2, SharedListNativeAPI.IndexOf(in view, 5, 1, 2));
            Assert.AreEqual(2, SharedListNativeAPI.IndexOf(in view, in five));
            Assert.AreEqual(2, SharedListNativeAPI.IndexOf(in view, in five, 2));
            Assert.AreEqual(2, SharedListNativeAPI.IndexOf(in view, in five, 1, 2));
            Assert.AreEqual(2, SharedListNativeAPI.IndexOf(in view, 5, comparer));
            Assert.AreEqual(2, SharedListNativeAPI.IndexOf(in view, in five, comparer));
        }

        [Test]
        public void Sort_FullAndRangeSortAscending()
        {
            var comparer = new IntComparer();
            using var full = new SharedList<int>(7, 5, 3, 1);
            using var range = new SharedList<int>(9, 7, 5, 3, 1);
            SharedListNative<int> fullView;
            SharedListNative<int> rangeView;

            // SAFETY: Both owners remain alive while their borrowed native views are consumed.
            unsafe
            {
                fullView = full.AsNative();
                rangeView = range.AsNative();
            }

            SharedListNativeAPI.Sort(in fullView, comparer);
            SharedListNativeAPI.Sort(in rangeView, 1, 3, comparer);

            CollectionAssert.AreEqual(new[] { 1, 3, 5, 7 }, full.ToArray());
            CollectionAssert.AreEqual(new[] { 9, 3, 5, 7, 1 }, range.ToArray());
        }

        [Test]
        public void UncreatedView_Searches_ThrowArgumentException()
        {
            var view = default(SharedListNative<int>);
            var comparer = new IntComparer();
            var item = 1;

            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.Contains(in view, 1));
            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.Contains(in view, in item));
            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.IndexOf(in view, 1));
            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.IndexOf(in view, in item));
            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.IndexOf(in view, 1, comparer));
            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.IndexOf(in view, in item, comparer));
            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.BinarySearch(in view, 1, comparer));
            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.BinarySearch(in view, in item, comparer));
            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.Remove(in view, 1));
            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.Remove(in view, in item));
            Assert.Throws<ArgumentException>(() => SharedListNativeAPI.Sort(in view, comparer));
        }

        [Test]
        public void EmptyList_Searches_ReportNothing()
        {
            using var list = new SharedList<int>(4);
            SharedListNative<int> view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative();
            }
            var comparer = new IntComparer();
            var item = 1;

            Assert.IsFalse(SharedListNativeAPI.Contains(in view, 1));
            Assert.IsFalse(SharedListNativeAPI.Contains(in view, in item));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, 1));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, in item));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, 1, 0, 0));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, in item, 0, 0));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, 1, comparer));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, in item, comparer));
            Assert.Less(SharedListNativeAPI.BinarySearch(in view, 1, comparer), 0);
            Assert.Less(SharedListNativeAPI.BinarySearch(in view, in item, comparer), 0);
            Assert.Less(SharedListNativeAPI.BinarySearch(in view, 0, 0, 1, comparer), 0);
            Assert.IsFalse(SharedListNativeAPI.Remove(in view, 1));
            Assert.IsFalse(SharedListNativeAPI.Remove(in view, in item));
            Assert.DoesNotThrow(() => SharedListNativeAPI.Sort(in view, comparer));
            Assert.DoesNotThrow(() => SharedListNativeAPI.Sort(in view, 0, 0, comparer));
        }

        [Test]
        public void IndexOf_RangeEndingAtTheLastElement_FindsItAndAnEmptyRangeAtTheEndFindsNothing()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int> view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative();
            }

            Assert.AreEqual(3, SharedListNativeAPI.IndexOf(in view, 7, 3, 1));
            Assert.AreEqual(3, SharedListNativeAPI.IndexOf(in view, 7, 3));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, 7, 4, 0));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, 7, 4));
            Assert.AreEqual(-1, SharedListNativeAPI.IndexOf(in view, 1, 1, 3));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_InvalidRange_Throws()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int> view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative();
            }
            var item = 1;

            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.IndexOf(in view, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.IndexOf(in view, 1, 5));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.IndexOf(in view, 1, 0, 5));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.IndexOf(in view, 1, 3, 2));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.IndexOf(in view, 1, 1, -1));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.IndexOf(in view, 1, -1, 2));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.IndexOf(in view, in item, -1));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.IndexOf(in view, in item, 0, 5));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void IndexOf_RangeBeyondCountButWithinCapacity_Throws()
        {
            using var list = new SharedList<int>(8);

            list.Add(1);
            list.Add(3);

            SharedListNative<int> view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative();
            }

            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.IndexOf(in view, 1, 0, 3));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.IndexOf(in view, 1, 3));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void BinarySearch_InvalidRange_Throws()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int> view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative();
            }
            var comparer = new IntComparer();
            var item = 5;

            Assert.Throws<InvalidOperationException>(
                () => SharedListNativeAPI.BinarySearch(in view, -1, 2, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedListNativeAPI.BinarySearch(in view, 0, -1, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedListNativeAPI.BinarySearch(in view, 3, 2, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedListNativeAPI.BinarySearch(in view, 5, 0, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedListNativeAPI.BinarySearch(in view, -1, 2, in item, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => SharedListNativeAPI.BinarySearch(in view, 3, 2, in item, comparer)
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Sort_InvalidRange_ThrowsAndKeepsTheContent()
        {
            using var list = new SharedList<int>(7, 5, 3, 1);
            SharedListNative<int> view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative();
            }
            var comparer = new IntComparer();

            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.Sort(in view, -1, 2, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.Sort(in view, 0, -1, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.Sort(in view, 3, 2, comparer));
            Assert.Throws<InvalidOperationException>(() => SharedListNativeAPI.Sort(in view, 5, 0, comparer));
            CollectionAssert.AreEqual(new[] { 7, 5, 3, 1 }, list.ToArray());
        }

        [Test]
        public void Sort_SingleElementOrEmptyRange_KeepsTheContent()
        {
            using var list = new SharedList<int>(9, 7, 5);
            SharedListNative<int> view;

            // SAFETY: list remains alive and unmodified while its borrowed view is consumed.
            unsafe
            {
                view = list.AsNative();
            }
            var comparer = new IntComparer();

            SharedListNativeAPI.Sort(in view, 1, 0, comparer);
            SharedListNativeAPI.Sort(in view, 3, 0, comparer);
            SharedListNativeAPI.Sort(in view, 2, 1, comparer);

            CollectionAssert.AreEqual(new[] { 9, 7, 5 }, list.ToArray());
        }

        [Test]
        public void Remove_EveryPosition_MatchesList()
        {
            var source = new[] { 1, 3, 5, 7 };

            for (var i = 0; i < source.Length; i++)
            {
                using var list = new SharedList<int>(source);
                var expected = new List<int>(source);
                var item = source[i];
                SharedListNative<int> view;

                // SAFETY: list remains alive while its borrowed native view is consumed.
                unsafe
                {
                    view = list.AsNative();
                }

                Assert.AreEqual(expected.Remove(item), SharedListNativeAPI.Remove(in view, item), $"{i}");
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
                var item = source[i];
                SharedListNative<int> view;

                // SAFETY: list remains alive while its borrowed native view is consumed.
                unsafe
                {
                    view = list.AsNative();
                }

                Assert.AreEqual(expected.Remove(item), SharedListNativeAPI.Remove(in view, in item), $"{i}");
                CollectionAssert.AreEqual(expected, list.ToArray(), $"{i}");
            }
        }

        [Test]
        public void Remove_ItemNotPresent_LeavesTheContentAndEnumerationUntouched()
        {
            using var list = new SharedList<int>(1, 3, 5, 7);
            SharedListNative<int> view;
            SharedList<int, int>.Enumerator enumerator;

            // SAFETY: list remains alive while its borrowed native view and enumerator are consumed.
            unsafe
            {
                view = list.AsNative();
                enumerator = list.GetEnumerator();
            }

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsFalse(SharedListNativeAPI.Remove(in view, 9));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);
            CollectionAssert.AreEqual(new[] { 1, 3, 5, 7 }, list.ToArray());
        }

        [Test]
        public void Remove_DuplicateItems_RemovesOnlyTheFirstOccurrence()
        {
            using var list = new SharedList<int>(2, 4, 2, 4);
            SharedListNative<int> view;

            // SAFETY: list remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = list.AsNative();
            }

            Assert.IsTrue(SharedListNativeAPI.Remove(in view, 4));
            CollectionAssert.AreEqual(new[] { 2, 2, 4 }, list.ToArray());
        }
    }
}
