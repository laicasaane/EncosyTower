using System;
using EncosyTower.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Core.Collections
{
    public sealed class ThrowHelperTests
    {
        [Test]
        public void GetCollectionTypeName_UnknownOrUndefined_ReturnsCollection()
        {
            Assert.That(
                ThrowHelper.GetCollectionTypeName(ThrowHelper.CollectionType.Unknown),
                Is.EqualTo("collection")
            );
            Assert.That(
                ThrowHelper.GetCollectionTypeName((ThrowHelper.CollectionType)int.MaxValue),
                Is.EqualTo("collection")
            );
        }

        [Test]
        public void GetCollectionTypeName_ArrayUnsafe_ReturnsCurrentTypeName()
        {
            var name = ThrowHelper.GetCollectionTypeName(ThrowHelper.CollectionType.ArrayUnsafe);

            Assert.That(name, Is.EqualTo("ArrayUnsafe<T>"));
        }

        [Test]
        public void ThrowIfUnsafeCollectionIsDisposed_Invalid_ThrowsObjectDisposedException()
        {
            var exception = Assert.Throws<ObjectDisposedException>(() =>
                ThrowHelper.ThrowIfUnsafeCollectionIsDisposed(false, ThrowHelper.CollectionType.ArrayUnsafe)
            );

            Assert.That(exception!.Message, Is.EqualTo("The ArrayUnsafe<T> is already disposed."));
        }

        [Test]
        public void ThrowIfUnsafeCollectionAllocatorIsInvalid_Invalid_ThrowsInvalidOperationException()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ThrowHelper.ThrowIfUnsafeCollectionAllocatorIsInvalid(false, ThrowHelper.CollectionType.ReferenceUnsafe)
            );

            Assert.That(
                exception!.Message,
                Is.EqualTo(
                    "The ReferenceUnsafe<T> can not be Disposed because it was not allocated with a valid allocator."
                )
            );
        }

        [Test]
        public void ThrowIfUnsafeCollectionTypeIsManaged_Invalid_ThrowsInvalidOperationException()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ThrowHelper.ThrowIfUnsafeCollectionTypeIsManaged<int>(false, ThrowHelper.CollectionType.ReferenceUnsafe)
            );

            Assert.That(
                exception!.Message,
                Is.EqualTo("System.Int32 used in ReferenceUnsafe<T> must be unmanaged (contain no managed types).")
            );
        }

        [Test]
        public void ThrowIfEmpty_InvalidQueue_ThrowsInvalidOperationException()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ThrowHelper.ThrowIfEmpty(false, ThrowHelper.CollectionType.QueueUnsafe)
            );

            Assert.That(exception!.Message, Is.EqualTo("the queue is empty"));
        }

        [Test]
        public void ThrowIfSourceCollectionIsNotCreated_Invalid_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                ThrowHelper.ThrowIfSourceCollectionIsNotCreated(false, ThrowHelper.CollectionType.ArraySetUnsafe)
            );

            Assert.That(exception!.ParamName, Is.EqualTo("source"));
            Assert.That(exception.Message, Does.StartWith("The source set is not created."));
        }

        [Test]
        public void ThrowIfNativeSourceCollectionIsNotCreated_Invalid_ThrowsInvalidOperationException()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ThrowHelper.ThrowIfNativeSourceCollectionIsNotCreated(false, ThrowHelper.CollectionType.ArrayMapNative)
            );

            Assert.That(exception!.Message, Is.EqualTo("The source map is not created."));
        }

        [Test]
        public void ThrowIfArgumentIndexIsNegative_Invalid_ThrowsArgumentOutOfRangeException()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                ThrowHelper.ThrowIfArgumentIndexIsNegative(false)
            );

            Assert.That(exception!.ParamName, Is.EqualTo("index"));
        }

        [Test]
        public void ThrowIfMissingLinkedListNode_InvalidArrayMap_ThrowsInvalidOperationException()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ThrowHelper.ThrowIfMissingLinkedListNode(false, ThrowHelper.CollectionType.ArrayMap)
            );

            Assert.That(exception!.Message, Is.EqualTo("The linked-list successor is missing."));
        }

        [Test]
        public void ThrowIfMissingLinkedListNode_InvalidSharedArrayMapUnsafe_ThrowsInvalidOperationException()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ThrowHelper.ThrowIfMissingLinkedListNode(false, ThrowHelper.CollectionType.SharedArrayMapUnsafe)
            );

            Assert.That(exception!.Message, Is.EqualTo("This should never happen"));
        }

        [Test]
        public void LogWarningIfHashCodeIsNotImplemented_MissingOverride_ReportsWarning()
        {
            LogAssert.Expect(
                  LogType.Warning
                , "MissingHashCode does not implement GetHashCode and will potentially cause "
                    + "unwanted allocations (boxing)"
            );

            ThrowHelper.LogWarningIfHashCodeIsNotImplemented<MissingHashCode>();
        }

        [Test]
        public void ThrowIfSerializedItemIsDuplicate_Invalid_ReportsWarning()
        {
            LogAssert.Expect(
                  LogType.Warning
                , "Duplicate item at serialized index 2 was ignored while deserializing "
                    + "ArrayMap<TKey, TValue>. The first occurrence was kept."
            );

            ThrowHelper.ThrowIfSerializedItemIsDuplicate(
                  false
                , ThrowHelper.CollectionType.ArrayMap
                , 2
            );
        }

        [Test]
        public void ThrowIfSerializedItemIsDuplicate_Valid_DoesNotReportWarning()
        {
            ThrowHelper.ThrowIfSerializedItemIsDuplicate(
                  true
                , ThrowHelper.CollectionType.ArrayMap
                , 2
            );

            LogAssert.NoUnexpectedReceived();
        }

        private readonly struct MissingHashCode
        {
        }
    }
}
