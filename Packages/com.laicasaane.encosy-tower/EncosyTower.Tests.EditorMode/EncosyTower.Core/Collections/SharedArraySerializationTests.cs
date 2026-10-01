using EncosyTower.Collections;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArrayTests
    {
        [Test]
        public void Serialization_AssetRoundTripRestoresLogicalValuesAndNativeView()
        {
            ClassCollectionSerializationTestUtility.AssertAssetRoundTrip(
                source =>
                {
                    source._sharedArray.Dispose();
                    source._sharedArray = new(new[] { 5, 6 });
                },
                copy =>
                {
                    // SAFETY: The deserialized array remains alive while its temporary borrowed views are inspected.
                    unsafe
                    {
                        CollectionAssert.AreEqual(new[] { 5, 6 }, copy._sharedArray.AsReadOnlySpan().ToArray());
                        Assert.That(copy._sharedArray.AsNativeArray().IsCreated, Is.True);
                    }
                }
            );
        }

        [Test]
        public void Serialization_ItemPayloadAdoptsArrayAndReleasesTransportReference()
        {
            using var array = new SharedArray<int>(0);
            var items = new[] { 3, 4 };
            array._serializedItems = items;

            ((ISerializationCallbackReceiver)array).OnAfterDeserialize();

            Assert.That(array._buffer.AsManagedArray(), Is.SameAs(items));
            Assert.That(array._serializedItems, Is.Null);

            ((ISerializationCallbackReceiver)array).OnBeforeSerialize();

            CollectionAssert.AreEqual(items, array._serializedItems);
        }

        [Test]
        public void Serialization_NullPayloadRestoresEmptyArray()
        {
            using var array = new SharedArray<int>(0);
            array._serializedItems = null;

            ((ISerializationCallbackReceiver)array).OnAfterDeserialize();

            Assert.That(array.Length, Is.Zero);
            // SAFETY: array remains alive while its temporary native alias is inspected.
            unsafe
            {
                Assert.That(array.AsNativeArray().Length, Is.Zero);
            }
        }
    }
}
