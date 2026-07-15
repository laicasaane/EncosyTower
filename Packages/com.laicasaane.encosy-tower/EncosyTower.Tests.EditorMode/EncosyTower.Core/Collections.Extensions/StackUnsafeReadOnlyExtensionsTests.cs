using EncosyTower.Collections.Unsafe;
using StackUnsafeReadOnlyAPI = EncosyTower.Collections.Extensions.StackUnsafeReadOnlyExtensions;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.EncosyTower.Collections
{
    public partial class StackUnsafeReadOnlyExtensionsTests
    {
        [Test]
        public void Contains_ValueAndComparerOverloadsReturnExpectedResults()
        {
            using var stack = new StackUnsafe<int>(new[] { 1, 3, 5, 7 }, Allocator.Temp);
            var readOnly = stack.AsReadOnly();
            var comparer = new IntComparer();

            Assert.IsTrue(StackUnsafeReadOnlyAPI.Contains(in readOnly, 3));
            Assert.IsFalse(StackUnsafeReadOnlyAPI.Contains(in readOnly, 4));
            Assert.IsTrue(StackUnsafeReadOnlyAPI.Contains(in readOnly, 3, comparer));
            Assert.IsFalse(StackUnsafeReadOnlyAPI.Contains(in readOnly, 4, comparer));
        }
    }
}
