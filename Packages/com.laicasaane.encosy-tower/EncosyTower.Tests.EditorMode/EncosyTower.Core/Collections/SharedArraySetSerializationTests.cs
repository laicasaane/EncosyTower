using EncosyTower.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArraySetTests
    {
        [Test]
        public void Serialization_AssetRoundTripRestoresLogicalValuesAndNativeView()
        {
            ClassCollectionSerializationTestUtility.AssertAssetRoundTrip(
                source =>
                {
                    Assert.That(source._sharedArraySet.Add(19), Is.True);
                    Assert.That(source._sharedArraySet.Add(20), Is.True);
                },
                copy =>
                {
                    CollectionAssert.AreEqual(
                          new[] { 19, 20 }
                        , copy._sharedArraySet.Items.ToArray()
                    );
                    // SAFETY: The deserialized set remains alive while its temporary native view is inspected.
                    unsafe
                    {
                        Assert.That(copy._sharedArraySet.AsNative().IsCreated, Is.True);
                    }
                }
            );
        }

        [Test]
        public void Serialization_ItemPayloadAdoptsArrayAndReleasesTransportReference()
        {
            using var set = new SharedArraySet<int>();
            var items = new[] { 11, 12 };
            set._serializedItems = items;

            ((ISerializationCallbackReceiver)set).OnAfterDeserialize();

            Assert.That(set._map._values.AsManagedArray(), Is.SameAs(items));
            Assert.That(set._serializedItems, Is.Null);

            ((ISerializationCallbackReceiver)set).OnBeforeSerialize();

            CollectionAssert.AreEqual(items, set._serializedItems);
        }

        [Test]
        public void Serialization_NullPayloadRestoresEmptyCreatedSet()
        {
            using var set = new SharedArraySet<int>();
            set._serializedItems = null;

            ((ISerializationCallbackReceiver)set).OnAfterDeserialize();

            Assert.That(set.Count, Is.Zero);
            // SAFETY: set remains alive while its temporary native view is inspected.
            unsafe
            {
                Assert.That(set.AsNative().IsCreated, Is.True);
            }
        }

        [Test]
        public void Serialization_DuplicateItemsKeepFirstAndWarn()
        {
            using var set = new SharedArraySet<int>();
            var items = new[] { 1, 1 };
            set._serializedItems = items;

            LogAssert.Expect(
                  LogType.Warning
                , "Duplicate item at serialized index 1 was ignored while deserializing "
                    + "SharedArraySet<T>. The first occurrence was kept."
            );

            ((ISerializationCallbackReceiver)set).OnAfterDeserialize();

            Assert.That(set.Count, Is.EqualTo(1));
            Assert.That(set.Contains(1), Is.True);
            Assert.That(set._map._values.AsManagedArray(), Is.SameAs(items));
            Assert.That(set._serializedItems, Is.Null);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
