using EncosyTower.Collections;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedReferenceTests
    {
        [Test]
        public void Serialization_AssetRoundTripRestoresLogicalValueAndNativeView()
        {
            ClassCollectionSerializationTestUtility.AssertAssetRoundTrip(
                source => source._sharedReference.ValueRW = 7,
                copy =>
                {
                    Assert.That(copy._sharedReference.ValueRO, Is.EqualTo(7));
                    // SAFETY: The deserialized reference remains alive while its temporary native alias is inspected.
                    unsafe
                    {
                        Assert.That(copy._sharedReference.AsNativeArray().IsCreated, Is.True);
                    }
                }
            );
        }

        [Test]
        public void Serialization_DefaultPayloadRestoresCreatedReference()
        {
            using var reference = new SharedReference<int>();
            reference._serializedValue = default;

            ((ISerializationCallbackReceiver)reference).OnAfterDeserialize();

            Assert.That(reference.ValueRO, Is.Zero);
            // SAFETY: reference remains alive while its temporary native alias is inspected.
            unsafe
            {
                Assert.That(reference.AsNativeArray().IsCreated, Is.True);
            }
        }
    }
}
