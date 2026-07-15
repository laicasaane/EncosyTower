// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using EncosyTower.Collections;
using SharedStackNativeAPI = EncosyTower.Collections.Extensions.SharedStackNativeExtensions;
using NUnit.Framework;

namespace EncosyTower.Tests.EncosyTower.Collections
{
    public partial class SharedStackNativeExtensionsTests
    {
        [Test]
        public void Contains_DefaultAndComparerFindExpectedValues()
        {
            using var stack = new SharedStack<int>(new[] { 1, 3, 5 }.AsSpan());
            var view = stack.AsNative();
            var comparer = new IntComparer();

            Assert.IsTrue(SharedStackNativeAPI.Contains(in view, 3));
            Assert.IsTrue(SharedStackNativeAPI.Contains(in view, 3, comparer));
            Assert.IsFalse(SharedStackNativeAPI.Contains(in view, 4));
            Assert.IsFalse(SharedStackNativeAPI.Contains(in view, 4, comparer));
        }
    }
}
