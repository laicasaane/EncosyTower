using EncosyTower.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class ArrayMapTests
    {
        [Test]
        public void Serialization_AssetRoundTripRestoresLogicalValues()
        {
            ClassCollectionSerializationTestUtility.AssertAssetRoundTrip(
                source =>
                {
                    Assert.That(source._arrayMap.TryAdd(1, 10), Is.True);
                    Assert.That(source._arrayMap.TryAdd(2, 20), Is.True);
                },
                copy =>
                {
                    Assert.That(copy._arrayMap.Count, Is.EqualTo(2));
                    Assert.That(copy._arrayMap[1], Is.EqualTo(10));
                    Assert.That(copy._arrayMap[2], Is.EqualTo(20));
                }
            );
        }

        [Test]
        public void Serialization_NullPayloadRestoresEmptyCreatedMap()
        {
            using var map = new ArrayMap<int, int>();
            map._serializedEntries = null;

            ((ISerializationCallbackReceiver)map).OnAfterDeserialize();

            Assert.That(map.Count, Is.Zero);
            Assert.That(map._valuesInfo.IsCreated, Is.True);
        }

        [Test]
        public void Serialization_DuplicateRowsKeepFirstAndWarn()
        {
            using var map = new ArrayMap<string, int>();
            map._serializedEntries = new[] {
                new SerializedMapEntry<string, int> { key = null, value = -1 },
                new SerializedMapEntry<string, int> { key = "one", value = 10 },
                new SerializedMapEntry<string, int> { key = "one", value = 20 },
            };

            LogAssert.Expect(
                  LogType.Warning
                , "Duplicate item at serialized index 2 was ignored while deserializing "
                    + "ArrayMap<TKey, TValue>. The first occurrence was kept."
            );

            ((ISerializationCallbackReceiver)map).OnAfterDeserialize();

            Assert.That(map.Count, Is.EqualTo(1));
            Assert.That(map["one"], Is.EqualTo(10));
            LogAssert.NoUnexpectedReceived();
        }
    }
}
