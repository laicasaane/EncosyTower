// Adapted from Unity.Collections.Tests/ListExtensionsTests.cs.

using System;
using EncosyTower.Collections;
using NUnit.Framework;

using SharedQueueNativeReadOnlyAPI = EncosyTower.Collections.Extensions.SharedQueueNativeReadOnlyExtensions;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedQueueNativeReadOnlyExtensionsTests
    {
        [Test]
        public void Contains_DefaultAndComparerFindExpectedValues()
        {
            using var queue = new SharedQueue<int>(new[] { 1, 3, 5 }.AsSpan());
            SharedQueueNative<int>.ReadOnly view;

            // SAFETY: queue remains alive and unmodified while its borrowed read-only native view is consumed.
            unsafe
            {
                view = queue.AsNative().AsReadOnly();
            }
            var comparer = new IntComparer();

            Assert.IsTrue(SharedQueueNativeReadOnlyAPI.Contains(in view, 3));
            Assert.IsTrue(SharedQueueNativeReadOnlyAPI.Contains(in view, 3, comparer));
            Assert.IsFalse(SharedQueueNativeReadOnlyAPI.Contains(in view, 4));
            Assert.IsFalse(SharedQueueNativeReadOnlyAPI.Contains(in view, 4, comparer));
        }
    }
}
