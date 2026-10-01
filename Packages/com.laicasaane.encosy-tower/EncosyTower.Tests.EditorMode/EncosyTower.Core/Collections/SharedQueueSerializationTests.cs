using EncosyTower.Collections;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedQueueTests
    {
        [Test]
        public void Serialization_AssetRoundTripRestoresFifoOrderAndNativeView()
        {
            ClassCollectionSerializationTestUtility.AssertAssetRoundTrip(
                source =>
                {
                    source._sharedQueue.Enqueue(10);
                    source._sharedQueue.Enqueue(11);
                    source._sharedQueue.Enqueue(12);
                    Assert.That(source._sharedQueue.Dequeue(), Is.EqualTo(10));
                    source._sharedQueue.Enqueue(13);
                },
                copy =>
                {
                    CollectionAssert.AreEqual(new[] { 11, 12, 13 }, copy._sharedQueue.ToArray());
                    // SAFETY: The deserialized queue remains alive while its temporary native view is inspected.
                    unsafe
                    {
                        Assert.That(copy._sharedQueue.AsNative().IsCreated, Is.True);
                    }
                }
            );
        }

        [Test]
        public void Serialization_ItemPayloadAdoptsArrayAndReleasesTransportReference()
        {
            using var queue = new SharedQueue<int>();
            var items = new[] { 7, 8 };
            queue._serializedItems = items;

            ((ISerializationCallbackReceiver)queue).OnAfterDeserialize();

            Assert.That(queue._buffer.AsManagedArray(), Is.SameAs(items));
            Assert.That(queue._serializedItems, Is.Null);

            ((ISerializationCallbackReceiver)queue).OnBeforeSerialize();

            CollectionAssert.AreEqual(items, queue._serializedItems);
        }

        [Test]
        public void Serialization_NullPayloadRestoresEmptyCreatedQueue()
        {
            using var queue = new SharedQueue<int>();
            queue._serializedItems = null;

            ((ISerializationCallbackReceiver)queue).OnAfterDeserialize();

            Assert.That(queue.Count, Is.Zero);
            // SAFETY: queue remains alive while its temporary native view is inspected.
            unsafe
            {
                Assert.That(queue.AsNative().IsCreated, Is.True);
            }
        }
    }
}
