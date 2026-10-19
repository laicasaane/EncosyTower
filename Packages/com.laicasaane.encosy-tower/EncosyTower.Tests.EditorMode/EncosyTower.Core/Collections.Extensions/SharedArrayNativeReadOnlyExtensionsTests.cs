// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using EncosyTower.Collections;
using NUnit.Framework;

using NativeReadOnlyAPI = EncosyTower.Collections.Extensions.SharedArrayNativeReadOnlyExtensions;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArrayNativeReadOnlyExtensionsTests
    {
        [Test]
        public void ContainsAndIndexOf_FindExpectedValues()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArrayNative<int>.ReadOnly view;

            // SAFETY: array remains alive and unmodified while its borrowed read-only native view is consumed.
            unsafe
            {
                view = array.AsNative().AsReadOnly();
            }
            var comparer = new IntComparer();
            var three = 3;

            Assert.IsTrue(NativeReadOnlyAPI.Contains(in view, 3));
            Assert.IsTrue(NativeReadOnlyAPI.Contains(in view, in three));
            Assert.IsFalse(NativeReadOnlyAPI.Contains(in view, 4));
            Assert.AreEqual(2, NativeReadOnlyAPI.IndexOf(in view, 5));
            Assert.AreEqual(-1, NativeReadOnlyAPI.IndexOf(in view, 5, 3));
            Assert.AreEqual(2, NativeReadOnlyAPI.IndexOf(in view, 5, 1, 2));
            Assert.AreEqual(1, NativeReadOnlyAPI.IndexOf(in view, 3, comparer));
            Assert.AreEqual(1, NativeReadOnlyAPI.IndexOf(in view, in three, comparer));
        }

        [Test]
        public void BinarySearch_FullAndRangeReturnExpectedResults()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArrayNative<int>.ReadOnly view;

            // SAFETY: array remains alive and unmodified while its borrowed read-only native view is consumed.
            unsafe
            {
                view = array.AsNative().AsReadOnly();
            }
            var comparer = new IntComparer();
            var five = 5;

            Assert.AreEqual(2, NativeReadOnlyAPI.BinarySearch(in view, 5, comparer));
            Assert.AreEqual(2, NativeReadOnlyAPI.BinarySearch(in view, in five, comparer));
            Assert.Less(NativeReadOnlyAPI.BinarySearch(in view, 4, comparer), 0);
            Assert.AreEqual(1, NativeReadOnlyAPI.BinarySearch(in view, 1, 2, 3, comparer));
        }

        [Test]
        public void UncreatedView_Searches_ThrowArgumentException()
        {
            var view = default(SharedArrayNative<int>.ReadOnly);
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
        public void IndexOf_RangeEndingAtTheLastElement_FindsItAndAnEmptyRangeAtTheEndFindsNothing()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArrayNative<int>.ReadOnly view;

            // SAFETY: array remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = array.AsNative().AsReadOnly();
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
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArrayNative<int>.ReadOnly view;

            // SAFETY: array remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = array.AsNative().AsReadOnly();
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
        public void IndexOf_IndexPlusCountOverflowingInt_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArrayNative<int>.ReadOnly view;

            // SAFETY: array remains alive while its borrowed native view is consumed.
            unsafe
            {
                view = array.AsNative().AsReadOnly();
            }

            Assert.Catch(() => NativeReadOnlyAPI.IndexOf(in view, 1, int.MaxValue, 1));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AfterOwnerDispose_Searches_Throw()
        {
            var array = new SharedArray<int>(new[] { 7, 5, 3, 1 });
            SharedArrayNative<int>.ReadOnly view;

            // SAFETY: array stays alive until the Dispose below, after which every view access is rejected.
            unsafe
            {
                view = array.AsNative().AsReadOnly();
            }

            var comparer = new IntComparer();

            array.Dispose();

            Assert.Catch(() => NativeReadOnlyAPI.Contains(in view, 3));
            Assert.Catch(() => NativeReadOnlyAPI.IndexOf(in view, 3));
            Assert.Catch(() => NativeReadOnlyAPI.IndexOf(in view, 3, comparer));
            Assert.Catch(() => NativeReadOnlyAPI.BinarySearch(in view, 3, comparer));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void BinarySearch_InvalidRange_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 3, 5, 7 });
            SharedArrayNative<int>.ReadOnly view;

            // SAFETY: array remains alive and unmodified while its borrowed read-only native view is consumed.
            unsafe
            {
                view = array.AsNative().AsReadOnly();
            }

            var comparer = new IntComparer();
            var item = 5;

            Assert.Throws<InvalidOperationException>(
                () => NativeReadOnlyAPI.BinarySearch(in view, -1, 2, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => NativeReadOnlyAPI.BinarySearch(in view, 0, -1, 5, comparer)
            );
            Assert.Throws<InvalidOperationException>(
                () => NativeReadOnlyAPI.BinarySearch(in view, 3, 2, in item, comparer)
            );
        }
    }
}
