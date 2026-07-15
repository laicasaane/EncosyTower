using EncosyTower.Collections.Unsafe;
using QueueUnsafeReadOnlyAPI = EncosyTower.Collections.Extensions.QueueUnsafeReadOnlyExtensions;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.EncosyTower.Collections
{
    public partial class QueueUnsafeReadOnlyExtensionsTests
    {
        [Test]
        public void Contains_ValueAndComparerOverloadsReturnExpectedResults()
        {
            using var queue = new QueueUnsafe<int>(new[] { 1, 3, 5, 7 }, Allocator.Temp);
            var readOnly = queue.AsReadOnly();
            var comparer = new IntComparer();

            Assert.IsTrue(QueueUnsafeReadOnlyAPI.Contains(in readOnly, 3));
            Assert.IsFalse(QueueUnsafeReadOnlyAPI.Contains(in readOnly, 4));
            Assert.IsTrue(QueueUnsafeReadOnlyAPI.Contains(in readOnly, 3, comparer));
            Assert.IsFalse(QueueUnsafeReadOnlyAPI.Contains(in readOnly, 4, comparer));
        }
    }
}
