using EncosyTower.Collections;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedListTests
    {
        [Test]
        public void Serialization_AssetRoundTripRestoresLogicalValuesAndNativeView()
        {
            ClassCollectionSerializationTestUtility.AssertAssetRoundTrip(
                source =>
                {
                    source._sharedList.Add(8);
                    source._sharedList.Add(9);
                },
                copy =>
                {
                    CollectionAssert.AreEqual(new[] { 8, 9 }, copy._sharedList.ToArray());
                    // SAFETY: The deserialized list remains alive while its temporary native view is inspected.
                    unsafe
                    {
                        Assert.That(copy._sharedList.AsNative().IsCreated, Is.True);
                    }
                }
            );
        }

        [Test]
        public void Serialization_ItemPayloadAdoptsArrayAndReleasesTransportReference()
        {
            using var list = new SharedList<int>();
            var items = new[] { 5, 6 };
            list._serializedItems = items;

            ((ISerializationCallbackReceiver)list).OnAfterDeserialize();

            Assert.That(list._buffer.AsManagedArray(), Is.SameAs(items));
            Assert.That(list._serializedItems, Is.Null);

            ((ISerializationCallbackReceiver)list).OnBeforeSerialize();

            CollectionAssert.AreEqual(items, list._serializedItems);
        }

        [Test]
        public void Serialization_NullPayloadRestoresEmptyCreatedList()
        {
            using var list = new SharedList<int>();
            list._serializedItems = null;

            ((ISerializationCallbackReceiver)list).OnAfterDeserialize();

            Assert.That(list.Count, Is.Zero);
            // SAFETY: list remains alive while its temporary native view is inspected.
            unsafe
            {
                Assert.That(list.AsNative().IsCreated, Is.True);
            }
        }
    }
}
