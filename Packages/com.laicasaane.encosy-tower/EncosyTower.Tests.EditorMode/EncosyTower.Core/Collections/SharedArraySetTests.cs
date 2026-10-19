using System;
using System.Collections;
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

        [Test]
        public void GetEnumerator_EmptySets_YieldNothing()
        {
            using var zeroCapacity = new SharedArraySet<int>();
            using var withCapacity = new SharedArraySet<int>(4);
            var count = 0;

            // SAFETY: The sets outlive the enumerators and are not modified while they run.
            unsafe
            {
                foreach (var value in zeroCapacity)
                {
                    count++;
                }

                foreach (var value in withCapacity)
                {
                    count++;
                }

                foreach (var value in (IEnumerable<int>)zeroCapacity)
                {
                    count++;
                }

                foreach (var value in (IEnumerable)withCapacity)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Enumerator_EmptySet_MoveNextReturnsFalseAndResetKeepsItEmpty()
        {
            using var set = new SharedArraySet<int>();

            // SAFETY: set outlives the enumerator and is not modified while it runs.
            unsafe
            {
                var enumerator = new SharedArraySetEnumerator<int>(set);

                Assert.IsTrue(enumerator.IsValid);
                Assert.IsFalse(enumerator.MoveNext());

                enumerator.Reset();

                Assert.IsFalse(enumerator.MoveNext());

                enumerator.Dispose();
            }

            Assert.IsFalse(default(SharedArraySetEnumerator<int>).IsValid);
        }

        [Test]
        public void AsReadOnly_EmptySets_ReportNoItems()
        {
            using var zeroCapacity = new SharedArraySet<int>();
            using var withCapacity = new SharedArraySet<int>(4);
            var item = 1;

            // SAFETY: The sets outlive the views, which are only inspected here.
            unsafe
            {
                var zeroView = zeroCapacity.AsReadOnly();
                var capacityView = withCapacity.AsReadOnly();

                Assert.IsTrue(zeroView.IsCreated);
                Assert.IsTrue(capacityView.IsCreated);
                Assert.AreEqual(0, zeroView.Count);
                Assert.AreEqual(0, capacityView.Count);
                Assert.AreEqual(withCapacity.Capacity, capacityView.Capacity);
                Assert.AreEqual(0, zeroView.Items.Length);
                Assert.AreEqual(0, capacityView.Items.Length);
                Assert.IsFalse(zeroView.Contains(1));
                Assert.IsFalse(capacityView.Contains(in item));

                zeroView.CopyTo(Span<int>.Empty);
                capacityView.CopyTo(0, Span<int>.Empty);

                Assert.IsTrue(zeroView.TryCopyTo(Span<int>.Empty));
                Assert.IsTrue(capacityView.TryCopyTo(0, Span<int>.Empty));
                Assert.IsFalse(capacityView.TryCopyTo(new int[1]));
            }
        }

        [Test]
        public void ReadOnlyGetEnumerator_EmptySets_YieldNothing()
        {
            using var zeroCapacity = new SharedArraySet<int>();
            using var withCapacity = new SharedArraySet<int>(4);
            var count = 0;

            // SAFETY: The sets outlive the views and their enumerators and are not modified while they run.
            unsafe
            {
                var zeroView = zeroCapacity.AsReadOnly();
                var capacityView = withCapacity.AsReadOnly();
                var emptyView = SharedArraySet<int>.ReadOnly.Empty;

                foreach (var value in zeroView)
                {
                    count++;
                }

                foreach (var value in capacityView)
                {
                    count++;
                }

                foreach (var value in emptyView)
                {
                    count++;
                }

                foreach (var value in (IEnumerable<int>)zeroView)
                {
                    count++;
                }

                foreach (var value in (IEnumerable)capacityView)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ReadOnly_DefaultAndEmpty_ReportCreationState()
        {
            var defaultView = default(SharedArraySet<int>.ReadOnly);
            var emptyView = SharedArraySet<int>.ReadOnly.Empty;

            Assert.IsFalse(defaultView.IsCreated);
            Assert.IsTrue(emptyView.IsCreated);
            Assert.AreEqual(0, emptyView.Count);
            Assert.AreEqual(0, emptyView.Items.Length);
            Assert.IsFalse(emptyView.Contains(1));
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
