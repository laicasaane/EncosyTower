using System;
using System.Reflection;
using System.Runtime.InteropServices;
using EncosyTower.Pooling;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

using GameObjectId = EncosyTower.UnityExtensions.UnityEntityId<UnityEngine.GameObject>;
using TransformId = EncosyTower.UnityExtensions.UnityEntityId<UnityEngine.Transform>;

namespace EncosyTower.Tests.Core.Pooling
{
    public class GameObjectInfoTests
    {
        [Test]
        public void Size_IsTwelveBytes()
        {
            Assert.AreEqual(12, UnsafeUtility.SizeOf<GameObjectInfo>());
        }

        [Test]
        public void Offsets_AreContiguousAndMatchTheFields()
        {
            Assert.AreEqual(0, GameObjectInfo.OFFSET_GAMEOBJECT_ID);
            Assert.AreEqual(4, GameObjectInfo.OFFSET_TRANSFORM_ID);
            Assert.AreEqual(8, GameObjectInfo.OFFSET_TRANSFORM_ARRAY_INDEX);
            Assert.AreEqual(GameObjectInfo.OFFSET_GAMEOBJECT_ID, OffsetOf(nameof(GameObjectInfo.gameObjectId)));
            Assert.AreEqual(GameObjectInfo.OFFSET_TRANSFORM_ID, OffsetOf(nameof(GameObjectInfo.transformId)));
            Assert.AreEqual(
                  GameObjectInfo.OFFSET_TRANSFORM_ARRAY_INDEX
                , OffsetOf(nameof(GameObjectInfo.transformArrayIndex))
            );
        }

        [Test]
        public void IdSize_FillsTheSlotBeforeTheNextField()
        {
            Assert.AreEqual(
                  GameObjectInfo.OFFSET_TRANSFORM_ID - GameObjectInfo.OFFSET_GAMEOBJECT_ID
                , UnsafeUtility.SizeOf<GameObjectId>()
            );
            Assert.AreEqual(
                  GameObjectInfo.OFFSET_TRANSFORM_ARRAY_INDEX - GameObjectInfo.OFFSET_TRANSFORM_ID
                , UnsafeUtility.SizeOf<TransformId>()
            );
            Assert.AreEqual(sizeof(int), UnsafeUtility.SizeOf<GameObjectId>());
        }

        [Test]
        public void Default_HasInvalidIdsAndIndexZero()
        {
            var info = default(GameObjectInfo);

            Assert.IsFalse(info.gameObjectId.IsValid);
            Assert.IsFalse(info.transformId.IsValid);
            Assert.AreEqual(0, info.transformArrayIndex);
        }

        [Test]
        public void Default_IsAllZeroBytes()
        {
            var info = default(GameObjectInfo);
            var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref info, 1));

            for (var i = 0; i < bytes.Length; i++)
            {
                Assert.AreEqual(0, bytes[i], $"{i}");
            }
        }

        [Test]
        public void Fields_RoundTripRealIds()
        {
            using var temp = new TempGameObjects();
            var go = temp.Create();
            var info = new GameObjectInfo {
                gameObjectId = new GameObjectId(go),
                transformId = new TransformId(go.transform),
                transformArrayIndex = 7,
            };

            Assert.IsTrue(info.gameObjectId.IsValid);
            Assert.IsTrue(info.transformId.IsValid);
            Assert.AreEqual(go.GetEntityId(), info.gameObjectId.Value);
            Assert.AreEqual(go.transform.GetEntityId(), info.transformId.Value);
            Assert.AreEqual(7, info.transformArrayIndex);
            Assert.AreSame(go, info.gameObjectId.ToObject().GetValueOrThrow());
            Assert.AreSame(go.transform, info.transformId.ToObject().GetValueOrThrow());
        }

        [Test]
        public void Fields_WritingOneLeavesTheOthersUntouched()
        {
            using var temp = new TempGameObjects();
            var go = temp.Create();
            var info = new GameObjectInfo {
                gameObjectId = new GameObjectId(go),
                transformId = new TransformId(go.transform),
                transformArrayIndex = 7,
            };

            info.transformArrayIndex = int.MinValue;

            Assert.AreEqual(go.GetEntityId(), info.gameObjectId.Value);
            Assert.AreEqual(go.transform.GetEntityId(), info.transformId.Value);
            Assert.AreEqual(int.MinValue, info.transformArrayIndex);

            info.transformArrayIndex = -1;
            info.gameObjectId = default;

            Assert.IsFalse(info.gameObjectId.IsValid);
            Assert.AreEqual(go.transform.GetEntityId(), info.transformId.Value);
            Assert.AreEqual(-1, info.transformArrayIndex);

            info.transformId = default;

            Assert.IsFalse(info.transformId.IsValid);
            Assert.AreEqual(-1, info.transformArrayIndex);
        }

        [Test]
        public void Bytes_FollowTheOffsets()
        {
            using var temp = new TempGameObjects();
            var go = temp.Create();
            var gameObjectId = new GameObjectId(go);
            var transformId = new TransformId(go.transform);
            var info = new GameObjectInfo {
                gameObjectId = gameObjectId,
                transformId = transformId,
                transformArrayIndex = 0x01020304,
            };

            var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref info, 1));
            var gameObjectIdBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref gameObjectId, 1));
            var transformIdBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref transformId, 1));
            var indexBytes = BitConverter.GetBytes(0x01020304);

            Assert.IsTrue(bytes.Slice(GameObjectInfo.OFFSET_GAMEOBJECT_ID, 4).SequenceEqual(gameObjectIdBytes));
            Assert.IsTrue(bytes.Slice(GameObjectInfo.OFFSET_TRANSFORM_ID, 4).SequenceEqual(transformIdBytes));
            Assert.IsTrue(bytes.Slice(GameObjectInfo.OFFSET_TRANSFORM_ARRAY_INDEX, 4).SequenceEqual(indexBytes));
        }

        [Test]
        public void Copy_IsIndependentOfTheSource()
        {
            using var temp = new TempGameObjects();
            var go = temp.Create();
            var source = new GameObjectInfo {
                gameObjectId = new GameObjectId(go),
                transformId = new TransformId(go.transform),
                transformArrayIndex = 3,
            };

            var copy = source;
            source.transformArrayIndex = 4;
            source.gameObjectId = default;

            Assert.AreEqual(go.GetEntityId(), copy.gameObjectId.Value);
            Assert.AreEqual(3, copy.transformArrayIndex);
        }

        [Test]
        public void Array_OfInfos_KeepsEachElementIndependent()
        {
            using var temp = new TempGameObjects();
            var first = temp.Create("First");
            var second = temp.Create("Second");
            var infos = new GameObjectInfo[2];

            infos[0] = new GameObjectInfo {
                gameObjectId = new GameObjectId(first),
                transformId = new TransformId(first.transform),
                transformArrayIndex = 0,
            };
            infos[1] = new GameObjectInfo {
                gameObjectId = new GameObjectId(second),
                transformId = new TransformId(second.transform),
                transformArrayIndex = 1,
            };

            Assert.AreEqual(first.GetEntityId(), infos[0].gameObjectId.Value);
            Assert.AreEqual(second.GetEntityId(), infos[1].gameObjectId.Value);
            Assert.AreEqual(0, infos[0].transformArrayIndex);
            Assert.AreEqual(1, infos[1].transformArrayIndex);
        }

        private static int OffsetOf(string fieldName)
        {
            var field = typeof(GameObjectInfo).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);

            Assert.IsNotNull(field, fieldName);
            return UnsafeUtility.GetFieldOffset(field);
        }
    }
}
