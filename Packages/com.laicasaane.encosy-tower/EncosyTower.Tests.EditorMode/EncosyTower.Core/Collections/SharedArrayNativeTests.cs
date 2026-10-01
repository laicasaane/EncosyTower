using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArrayNativeTests
    {
        [Test]
        public void CreatedStateLengthIndexerAndElementReferenceReflectOwner()
        {
            SharedArrayNative<int> uncreated = default;
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: owner remains live while the borrowed view and element reference are used.
            unsafe
            {
                var array = owner.AsNative();

                ref var item = ref array.ElementAt(1);
                item = 20;

                Assert.IsFalse(uncreated.IsCreated);
                Assert.IsTrue(array.IsCreated);
                Assert.AreEqual(3, array.Length);
                Assert.AreEqual(20, owner[1]);
            }
        }

        [Test]
        public void CopyClearSpansAndNativeSliceOperateOnBorrowedStorage()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3, 4 });

            // SAFETY: owner remains live while all borrowed spans, slices, and the native view are consumed.
            unsafe
            {
                var array = owner.AsNative();

                array.CopyFrom(1, new[] { 8, 9 }, 2);
                CollectionAssert.AreEqual(new[] { 1, 8, 9, 4 }, array.AsReadOnlySpan().ToArray());

                var destination = new int[2];
                array.CopyTo(1, destination, 2);
                CollectionAssert.AreEqual(new[] { 8, 9 }, destination);

                array.AsSpan()[0] = 7;
                var nativeSlice = array.AsNativeSlice();
                nativeSlice[3] = 6;
                CollectionAssert.AreEqual(new[] { 7, 8, 9, 6 }, array.ToArray());

                array.Clear();
                CollectionAssert.AreEqual(new[] { 0, 0, 0, 0 }, owner.AsManagedArray());
            }
        }

        [Test]
        public void Reinterpret_EqualSizeAliasesStorageAndDifferentSizeThrows()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: owner remains live while equal-size aliases and the rejection path are exercised.
            unsafe
            {
                var array = owner.AsNative();
                var reinterpreted = array.Reinterpret<uint>();

                reinterpreted[0] = 42;

                Assert.AreEqual(42, owner[0]);
                Assert.Throws<InvalidOperationException>(() => array.Reinterpret<ushort>());
            }
        }

        [Test]
        public void Enumeration_VisitsFixedLengthAndSupportsInterfaces()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });
            var values = new List<int>();

            // SAFETY: owner remains live and unmodified while every borrowed enumerator is consumed.
            unsafe
            {
                var array = owner.AsNative();

                foreach (var value in array)
                {
                    values.Add(value);
                }

                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, values);

                IEnumerable<int> generic = array;
                using var genericEnumerator = generic.GetEnumerator();
                Assert.IsTrue(genericEnumerator.MoveNext());
                Assert.AreEqual(1, genericEnumerator.Current);

                IEnumerable nongeneric = array;
                var enumerator = nongeneric.GetEnumerator();
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(1, enumerator.Current);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ViewCreatedBeforeOwnerDispose_ThrowsOnAccess()
        {
            var owner = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: This test intentionally keeps a borrowed view past owner disposal to verify safety checks.
            unsafe
            {
                var stale = owner.AsNative();

                owner.Dispose();

                Assert.Catch(() => _ = stale[0]);
            }
        }
    }
}
