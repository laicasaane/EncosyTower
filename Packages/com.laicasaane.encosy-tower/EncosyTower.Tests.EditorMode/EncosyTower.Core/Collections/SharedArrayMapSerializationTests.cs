using EncosyTower.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArrayMapTests
    {
        [Test]
        public void Serialization_AssetRoundTripRestoresLogicalValuesAndNativeView()
        {
            ClassCollectionSerializationTestUtility.AssertAssetRoundTrip(
                source =>
                {
                    Assert.That(source._sharedArrayMap.TryAdd(17, 170), Is.True);
                    Assert.That(source._sharedArrayMap.TryAdd(18, 180), Is.True);
                },
                copy =>
                {
                    Assert.That(copy._sharedArrayMap[17], Is.EqualTo(170));
                    Assert.That(copy._sharedArrayMap[18], Is.EqualTo(180));
                    // SAFETY: The deserialized map remains alive while its temporary native view is inspected.
                    unsafe
                    {
                        Assert.That(copy._sharedArrayMap.AsNative().IsCreated, Is.True);
                    }
                }
            );
        }

        [Test]
        public void Serialization_NullPayloadRestoresEmptyCreatedMap()
        {
            using var map = new SharedArrayMap<int, int>();
            map._serializedEntries = null;

            ((ISerializationCallbackReceiver)map).OnAfterDeserialize();

            Assert.That(map.Count, Is.Zero);
            // SAFETY: map remains alive while its temporary native view is inspected.
            unsafe
            {
                Assert.That(map.AsNative().IsCreated, Is.True);
            }
        }

        [Test]
        public void Serialization_DuplicateRowsKeepFirstAndWarn()
        {
            using var map = new SharedArrayMap<int, int>();
            map._serializedEntries = new[] {
                new SerializedMapEntry<int, int> { key = 1, value = 10 },
                new SerializedMapEntry<int, int> { key = 1, value = 20 },
            };

            LogAssert.Expect(
                  LogType.Warning
                , "Duplicate item at serialized index 1 was ignored while deserializing "
                    + "SharedArrayMap<TKey, TValue, TValueNative>. The first occurrence was kept."
            );

            ((ISerializationCallbackReceiver)map).OnAfterDeserialize();

            Assert.That(map.Count, Is.EqualTo(1));
            Assert.That(map[1], Is.EqualTo(10));
            LogAssert.NoUnexpectedReceived();
        }
    }
}
