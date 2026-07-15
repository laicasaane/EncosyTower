// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using EncosyTower.Collections;
using SharedStackReadOnlyAPI = EncosyTower.Collections.Extensions.SharedStackReadOnlyExtensions;
using NUnit.Framework;

namespace EncosyTower.Tests.EncosyTower.Collections
{
    public partial class SharedStackReadOnlyExtensionsTests
    {
        [Test]
        public void Contains_DefaultAndComparerFindExpectedValues()
        {
            using var stack = new SharedStack<int>(new[] { 1, 3, 5 }.AsSpan());
            var view = stack.AsReadOnly();
            var comparer = new IntComparer();

            Assert.IsTrue(SharedStackReadOnlyAPI.Contains(in view, 3));
            Assert.IsTrue(SharedStackReadOnlyAPI.Contains(in view, 3, comparer));
            Assert.IsFalse(SharedStackReadOnlyAPI.Contains(in view, 4));
            Assert.IsFalse(SharedStackReadOnlyAPI.Contains(in view, 4, comparer));
        }
    }
}
