using System;
using System.Collections.Generic;
using EncosyTower.Collections;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArraySetTests
    {
        [Test]
        public void OwnerAndReadOnly_ExposeUniqueSharedValues()
        {
            using var set = new SharedArraySet<int>(4);

            Assert.IsTrue(set.Add(10));
            Assert.IsTrue(set.Add(20));
            Assert.IsFalse(set.Add(10));

            // SAFETY: set remains the live owner while both read-only aliases are used.
            unsafe
            {
                var readOnly = set.AsReadOnly();
                SharedArraySet<int>.ReadOnly converted = set;

                Assert.AreEqual(2, set.Count);
                Assert.AreEqual(2, readOnly.Count);
                Assert.IsTrue(readOnly.Contains(10));
                Assert.IsTrue(converted.Contains(20));
                CollectionAssert.AreEquivalent(new[] { 10, 20 }, set.Items.ToArray());

                Assert.IsTrue(set.Remove(10));
                Assert.IsFalse(readOnly.Contains(10));
            }
        }

        [Test]
        public void CopyAndEnumeration_UseCurrentSetContents()
        {
            using var set = new SharedArraySet<int>(4);
            set.Add(3);
            set.Add(5);

            Span<int> copied = stackalloc int[2];

            // SAFETY: set remains live while the temporary read-only alias copies its values.
            unsafe
            {
                set.AsReadOnly().CopyTo(copied);
            }

            CollectionAssert.AreEquivalent(new[] { 3, 5 }, copied.ToArray());

            var enumerated = new List<int>();

            // SAFETY: set remains alive and unmodified during enumeration.
            unsafe
            {
                foreach (var value in set)
                {
                    enumerated.Add(value);
                }
            }

            CollectionAssert.AreEquivalent(new[] { 3, 5 }, enumerated);
        }

        [Test]
        public void SetOperations_MutateExpectedValues()
        {
            using var target = CreateSet(1, 2, 3);
            using var other = CreateSet(2, 3, 4);
            using var excluded = CreateSet(3);

            target.Intersect(other);

            CollectionAssert.AreEquivalent(new[] { 2, 3 }, target.Items.ToArray());

            target.Exclude(excluded);

            CollectionAssert.AreEquivalent(new[] { 2 }, target.Items.ToArray());

            target.Union(other);

            CollectionAssert.AreEquivalent(new[] { 2, 3, 4 }, target.Items.ToArray());
        }

        private static SharedArraySet<int> CreateSet(params int[] values)
        {
            var set = new SharedArraySet<int>(Math.Max(values.Length, 1));

            foreach (var value in values)
            {
                set.Add(value);
            }

            return set;
        }
    }
}
