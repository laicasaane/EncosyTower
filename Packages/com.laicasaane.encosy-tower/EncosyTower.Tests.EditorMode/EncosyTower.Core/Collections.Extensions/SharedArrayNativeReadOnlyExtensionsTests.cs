// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

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
    }
}
