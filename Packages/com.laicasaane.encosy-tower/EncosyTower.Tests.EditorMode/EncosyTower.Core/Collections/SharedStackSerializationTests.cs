using EncosyTower.Collections;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedStackTests
    {
        [Test]
        public void Serialization_AssetRoundTripRestoresLifoOrderAndNativeView()
        {
            ClassCollectionSerializationTestUtility.AssertAssetRoundTrip(
                source =>
                {
                    source._sharedStack.Push(14);
                    source._sharedStack.Push(15);
                    source._sharedStack.Push(16);
                },
                copy =>
                {
                    CollectionAssert.AreEqual(new[] { 16, 15, 14 }, copy._sharedStack.ToArray());
                    // SAFETY: The deserialized stack remains alive while its temporary native view is inspected.
                    unsafe
                    {
                        Assert.That(copy._sharedStack.AsNative().IsCreated, Is.True);
                    }
                }
            );
        }

        [Test]
        public void Serialization_ItemPayloadAdoptsArrayAndReleasesTransportReference()
        {
            using var stack = new SharedStack<int>();
            var items = new[] { 9, 10 };
            stack._serializedItems = items;

            ((ISerializationCallbackReceiver)stack).OnAfterDeserialize();

            Assert.That(stack._buffer.AsManagedArray(), Is.SameAs(items));
            Assert.That(stack._serializedItems, Is.Null);

            ((ISerializationCallbackReceiver)stack).OnBeforeSerialize();

            CollectionAssert.AreEqual(items, stack._serializedItems);
        }

        [Test]
        public void Serialization_NullPayloadRestoresEmptyCreatedStack()
        {
            using var stack = new SharedStack<int>();
            stack._serializedItems = null;

            ((ISerializationCallbackReceiver)stack).OnAfterDeserialize();

            Assert.That(stack.Count, Is.Zero);
            // SAFETY: stack remains alive while its temporary native view is inspected.
            unsafe
            {
                Assert.That(stack.AsNative().IsCreated, Is.True);
            }
        }
    }
}
