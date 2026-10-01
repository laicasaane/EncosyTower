using EncosyTower.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class ArraySetTests
    {
        [Test]
        public void Serialization_AssetRoundTripRestoresLogicalValues()
        {
            ClassCollectionSerializationTestUtility.AssertAssetRoundTrip(
                source =>
                {
                    Assert.That(source._arraySet.Add(3), Is.True);
                    Assert.That(source._arraySet.Add(4), Is.True);
                },
                copy => CollectionAssert.AreEqual(new[] { 3, 4 }, copy._arraySet.Items.ToArray())
            );
        }

        [Test]
        public void Serialization_ItemPayloadAdoptsArrayAndReleasesTransportReference()
        {
            using var set = new ArraySet<int>();
            var items = new[] { 1, 2 };
            set._serializedItems = items;

            ((ISerializationCallbackReceiver)set).OnAfterDeserialize();

            Assert.That(set._values.AsManagedArray(), Is.SameAs(items));
            Assert.That(set._serializedItems, Is.Null);

            ((ISerializationCallbackReceiver)set).OnBeforeSerialize();

            CollectionAssert.AreEqual(items, set._serializedItems);
        }

        [Test]
        public void Serialization_NullPayloadRestoresEmptyCreatedSet()
        {
            using var set = new ArraySet<int>();
            set._serializedItems = null;

            ((ISerializationCallbackReceiver)set).OnAfterDeserialize();

            Assert.That(set.Count, Is.Zero);
            Assert.That(set._valuesInfo.IsCreated, Is.True);
        }

        [Test]
        public void Serialization_DuplicateItemsKeepFirstAndWarn()
        {
            using var set = new ArraySet<string>();
            var items = new[] { null, "one", "one" };
            set._serializedItems = items;

            LogAssert.Expect(
                  LogType.Warning
                , "Duplicate item at serialized index 2 was ignored while deserializing "
                    + "ArraySet<T>. The first occurrence was kept."
            );

            ((ISerializationCallbackReceiver)set).OnAfterDeserialize();

            Assert.That(set.Count, Is.EqualTo(1));
            Assert.That(set.Contains("one"), Is.True);
            Assert.That(set._values.AsManagedArray(), Is.SameAs(items));
            Assert.That(set._serializedItems, Is.Null);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
