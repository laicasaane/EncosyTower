using System;
using System.Collections.Generic;
using EncosyTower.Collections;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Collections
{
    public class SharedArraySetNativeTests
    {
        [Test]
        public void NativeAndReadOnlyViews_ReflectOwnerMutations()
        {
            using var owner = new SharedArraySet<int>(8);
            owner.Add(1);

            // SAFETY: owner remains live while all native and read-only aliases are used.
            unsafe
            {
                var view = owner.AsNative();
                Assert.IsTrue(view.Add(2));
                Assert.IsFalse(view.Add(2));

                var readOnly = view.AsReadOnly();
                SharedArraySetNative<int>.ReadOnly converted = view;

                Assert.IsTrue(view.IsCreated);
                Assert.AreEqual(2, owner.Count);
                Assert.IsTrue(owner.Contains(2));
                Assert.IsTrue(readOnly.Contains(1));
                Assert.IsTrue(converted.Contains(2));

                view.Remove(1);

                Assert.IsFalse(owner.Contains(1));
                Assert.AreEqual(1, readOnly.Count);
            }
        }

        [Test]
        public void NativeSetOperations_MutateOwnerStorage()
        {
            using var targetOwner = CreateSet(1, 2, 3);
            using var otherOwner = CreateSet(2, 3, 4);
            // SAFETY: Both owners remain live and stable while their native aliases interact.
            unsafe
            {
                var target = targetOwner.AsNative();
                var other = otherOwner.AsNative();

                target.Intersect(in other);

                Assert.AreEqual(2, targetOwner.Count);
                Assert.IsTrue(targetOwner.Contains(2));
                Assert.IsTrue(targetOwner.Contains(3));

                target.Exclude(in other);

                Assert.AreEqual(0, targetOwner.Count);

                target.Union(in other);

                Assert.AreEqual(3, targetOwner.Count);
                Assert.IsTrue(targetOwner.Contains(4));
            }
        }

        [Test]
        public void NativeEnumerationAndCopy_UseCurrentOwnerContents()
        {
            using var owner = CreateSet(7, 9);
            Span<int> copied = stackalloc int[2];

            // SAFETY: owner remains live and unmodified while aliases and enumeration are consumed.
            unsafe
            {
                var view = owner.AsNative();
                view.AsReadOnly().CopyTo(copied);

                CollectionAssert.AreEquivalent(new[] { 7, 9 }, copied.ToArray());

                var enumerated = new List<int>();

                foreach (var value in view)
                {
                    enumerated.Add(value);
                }

                CollectionAssert.AreEquivalent(new[] { 7, 9 }, enumerated);
            }
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
