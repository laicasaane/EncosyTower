using System;
using System.Runtime.InteropServices;
using EncosyTower.Pooling;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

using EntityId = UnityEngine.EntityId;
using GameObjectId = EncosyTower.UnityExtensions.UnityEntityId<UnityEngine.GameObject>;
using TransformId = EncosyTower.UnityExtensions.UnityEntityId<UnityEngine.Transform>;

namespace EncosyTower.Tests.Core.Pooling
{
    public class SceneObjectPoolTests
    {
        [Test]
        public void IdTypes_HaveTheSameSizeAsEntityId()
        {
            Assert.AreEqual(UnsafeUtility.SizeOf<EntityId>(), UnsafeUtility.SizeOf<GameObjectId>());
            Assert.AreEqual(UnsafeUtility.SizeOf<EntityId>(), UnsafeUtility.SizeOf<TransformId>());
        }

        [Test]
        public void CastIdsToEntityIds_KeepsTheLengthAndTheValues()
        {
            using var temp = new TempGameObjects();
            var objects = new[] { temp.Create("A"), temp.Create("B"), temp.Create("C") };
            var gameObjectIds = new GameObjectId[objects.Length];
            var transformIds = new TransformId[objects.Length];

            for (var i = 0; i < objects.Length; i++)
            {
                gameObjectIds[i] = new GameObjectId(objects[i]);
                transformIds[i] = new TransformId(objects[i].transform);
            }

            ReadOnlySpan<GameObjectId> readOnlyGameObjectIds = gameObjectIds;
            ReadOnlySpan<TransformId> readOnlyTransformIds = transformIds;

            var castGameObjectIds = MemoryMarshal.Cast<GameObjectId, EntityId>(readOnlyGameObjectIds);
            var castTransformIds = MemoryMarshal.Cast<TransformId, EntityId>(readOnlyTransformIds);

            Assert.AreEqual(objects.Length, castGameObjectIds.Length);
            Assert.AreEqual(objects.Length, castTransformIds.Length);

            for (var i = 0; i < objects.Length; i++)
            {
                Assert.AreEqual(objects[i].GetEntityId(), castGameObjectIds[i], $"{i}");
                Assert.AreEqual(objects[i].transform.GetEntityId(), castTransformIds[i], $"{i}");
            }
        }

        [Test]
        public void CastIdsToEntityIds_EmptySpan_StaysEmpty()
        {
            Assert.AreEqual(0, MemoryMarshal.Cast<GameObjectId, EntityId>(Span<GameObjectId>.Empty).Length);
            Assert.AreEqual(0, MemoryMarshal.Cast<GameObjectId, EntityId>(ReadOnlySpan<GameObjectId>.Empty).Length);
            Assert.AreEqual(0, MemoryMarshal.Cast<TransformId, EntityId>(ReadOnlySpan<TransformId>.Empty).Length);
        }

        [Test]
        public void CastIdsToEntityIds_DefaultId_StaysDefault()
        {
            var ids = new TransformId[1];
            var cast = MemoryMarshal.Cast<TransformId, EntityId>(ids.AsSpan());

            Assert.AreEqual(1, cast.Length);
            Assert.AreEqual(default(EntityId), cast[0]);
        }

        [TestCase(0u)]
        [TestCase(1u)]
        [TestCase(16u)]
        public void NewPool_StartsEmpty(uint initialCapacity)
        {
            using var pool = new SceneObjectPool(initialCapacity);

            Assert.AreEqual(0, pool.UnusedCount);
            Assert.IsNull(pool.Source);
            Assert.IsFalse(pool.Scene.IsValid());
            Assert.IsFalse(pool.TrimCloneSuffix);
            Assert.AreEqual(RentingStrategy.Default, pool.RentingStrategy);
            Assert.AreEqual(ReturningStrategy.Default, pool.ReturningStrategy);
        }

        [Test]
        public void Dispose_Twice_DoesNotThrow()
        {
            var pool = new SceneObjectPool(0u);

            pool.Dispose();

            Assert.DoesNotThrow(() => pool.Dispose());
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void Dispose_ClearsTheUnusedObjects()
        {
            using var temp = new TempGameObjects();
            var pool = new SceneObjectPool(2u);

            pool.Return(temp.Create(), ReturningStrategy.Default);
            pool.Dispose();

            Assert.AreEqual(0, pool.UnusedCount);
            Assert.IsNull(pool.Source);
        }

        [TestCase(-5)]
        [TestCase(0)]
        public void PrepoolDifference_NothingNeeded_ReturnsZeroWithoutSourceOrScene(int estimatedAmount)
        {
            using var pool = new SceneObjectPool(0u);

            Assert.AreEqual(0, pool.PrepoolDifference(estimatedAmount));
            Assert.AreEqual(0, pool.PrepoolDifference(estimatedAmount, ReturningStrategy.Default));
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void PrepoolDifference_EmptySpans_ReturnsZeroWithoutSourceOrScene()
        {
            using var pool = new SceneObjectPool(0u);

            var result = pool.PrepoolDifference(
                  Span<GameObjectId>.Empty
                , Span<TransformId>.Empty
                , ReturningStrategy.Default
            );

            Assert.AreEqual(0, result);
        }

        [Test]
        public void PrepoolDifference_EnoughUnusedObjects_ReturnsZero()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(2u);

            pool.Return(temp.Create(), ReturningStrategy.Default);
            pool.Return(temp.Create(), ReturningStrategy.Default);

            Assert.AreEqual(0, pool.PrepoolDifference(2));
            Assert.AreEqual(0, pool.PrepoolDifference(1));
            Assert.AreEqual(2, pool.UnusedCount);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(5)]
        public void ReleaseInstances_EmptyPool_DoesNothing(int keep)
        {
            using var pool = new SceneObjectPool(0u);
            var released = false;

            pool.ReleaseInstances(keep, _ => released = true);

            Assert.IsFalse(released);
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void ReleaseInstances_KeepAtLeastTheUnusedCount_DoesNothing()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(2u);
            var released = false;

            pool.Return(temp.Create(), ReturningStrategy.Default);
            pool.Return(temp.Create(), ReturningStrategy.Default);
            pool.ReleaseInstances(2, _ => released = true);
            pool.ReleaseInstances(3, _ => released = true);

            Assert.IsFalse(released);
            Assert.AreEqual(2, pool.UnusedCount);
        }

        [Test]
        public void Return_EmptySpans_DoNothing()
        {
            using var pool = new SceneObjectPool(0u);

            pool.Return(ReadOnlySpan<GameObject>.Empty, ReturningStrategy.Default);
            pool.Return(ReadOnlySpan<Transform>.Empty, ReturningStrategy.Default);
            pool.Return(ReadOnlySpan<GameObjectId>.Empty, ReturningStrategy.Default);
            pool.Return(ReadOnlySpan<TransformId>.Empty, ReturningStrategy.Default);
            pool.Return(ReadOnlySpan<GameObject>.Empty, ReadOnlySpan<Transform>.Empty, ReturningStrategy.Default);
            pool.Return(ReadOnlySpan<GameObjectId>.Empty, ReadOnlySpan<TransformId>.Empty, ReturningStrategy.Default);

            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void Return_DefaultIds_DoNothing()
        {
            using var pool = new SceneObjectPool(0u);

            pool.Return(default(GameObjectId), ReturningStrategy.Default);
            pool.Return(default(TransformId), ReturningStrategy.Default);

            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void Return_NullObjects_DoNothing()
        {
            using var pool = new SceneObjectPool(0u);

            pool.Return((GameObject)null, ReturningStrategy.Default);
            pool.Return((Transform)null, ReturningStrategy.Default);

            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void Return_GameObject_StoresItAndDeactivatesIt()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(1u);
            var go = temp.Create();

            pool.Return(go, ReturningStrategy.Default);

            Assert.AreEqual(1, pool.UnusedCount);
            Assert.IsFalse(go.activeSelf);
        }

        [Test]
        public void Return_DoNothingStrategy_KeepsTheObjectActive()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(1u);
            var go = temp.Create();

            pool.Return(go, ReturningStrategy.DoNothing);

            Assert.AreEqual(1, pool.UnusedCount);
            Assert.IsTrue(go.activeSelf);
        }

        [Test]
        public void Return_Transform_StoresTheOwningGameObject()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(1u);
            var go = temp.Create();

            pool.Return(go.transform, ReturningStrategy.Default);

            Assert.AreEqual(1, pool.UnusedCount);
            Assert.IsFalse(go.activeSelf);
            Assert.AreSame(go, pool.RentGameObject(RentingStrategy.Default));
        }

        [Test]
        public void RentGameObject_AfterReturn_GivesTheSameObjectBackAndActivatesIt()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(1u);
            var go = temp.Create();

            pool.Return(go, ReturningStrategy.Default);

            var rented = pool.RentGameObject(RentingStrategy.Default);

            Assert.AreSame(go, rented);
            Assert.IsTrue(rented.activeSelf);
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void RentGameObject_DoNothingStrategy_LeavesTheObjectInactive()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(1u);
            var go = temp.Create();

            pool.Return(go, ReturningStrategy.Default);

            var rented = pool.RentGameObject(RentingStrategy.DoNothing);

            Assert.AreSame(go, rented);
            Assert.IsFalse(rented.activeSelf);
        }

        [Test]
        public void RentGameObject_TwoReturned_GivesTheLastReturnedFirst()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(2u);
            var first = temp.Create("First");
            var second = temp.Create("Second");

            pool.Return(first, ReturningStrategy.Default);
            pool.Return(second, ReturningStrategy.Default);

            Assert.AreSame(second, pool.RentGameObject(RentingStrategy.Default));
            Assert.AreSame(first, pool.RentGameObject(RentingStrategy.Default));
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void RentGameObjectIdAndTransformId_AfterReturn_MatchTheReturnedObject()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(1u);
            var go = temp.Create();

            pool.Return(go, ReturningStrategy.Default);
            Assert.AreEqual(go.GetEntityId(), pool.RentGameObjectId(RentingStrategy.Default).Value);
            Assert.IsTrue(go.activeSelf);

            pool.Return(go, ReturningStrategy.Default);
            Assert.AreEqual(go.transform.GetEntityId(), pool.RentTransformId(RentingStrategy.Default).Value);
            Assert.IsTrue(go.activeSelf);
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void RentTransform_AfterReturn_GivesTheTransformBackAndActivatesTheObject()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(1u);
            var go = temp.Create();

            pool.Return(go, ReturningStrategy.Default);

            var rented = pool.RentTransform(RentingStrategy.Default);

            Assert.AreSame(go.transform, rented);
            Assert.IsTrue(go.activeSelf);
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void RentIdSpan_AfterReturn_GivesTheLastReturnedIdsAndActivatesThem()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(3u);
            var objects = new[] { temp.Create("A"), temp.Create("B"), temp.Create("C") };
            var rented = new GameObjectId[2];

            for (var i = 0; i < objects.Length; i++)
            {
                pool.Return(objects[i], ReturningStrategy.Default);
            }

            pool.Rent(rented.AsSpan(), RentingStrategy.Default);

            Assert.AreEqual(objects[1].GetEntityId(), rented[0].Value);
            Assert.AreEqual(objects[2].GetEntityId(), rented[1].Value);
            Assert.IsFalse(objects[0].activeSelf);
            Assert.IsTrue(objects[1].activeSelf);
            Assert.IsTrue(objects[2].activeSelf);
            Assert.AreEqual(1, pool.UnusedCount);
        }

        [Test]
        public void RentTransformIdSpan_AfterReturn_GivesTheLastReturnedIds()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(3u);
            var objects = new[] { temp.Create("A"), temp.Create("B"), temp.Create("C") };
            var rented = new TransformId[2];

            for (var i = 0; i < objects.Length; i++)
            {
                pool.Return(objects[i], ReturningStrategy.Default);
            }

            pool.Rent(rented.AsSpan(), RentingStrategy.Default);

            Assert.AreEqual(objects[1].transform.GetEntityId(), rented[0].Value);
            Assert.AreEqual(objects[2].transform.GetEntityId(), rented[1].Value);
            Assert.IsTrue(objects[1].activeSelf);
            Assert.IsTrue(objects[2].activeSelf);
            Assert.AreEqual(1, pool.UnusedCount);
        }

        [Test]
        public void ReturnObjectSpans_StoreTheObjectsAndDeactivateThem()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(4u);
            var objects = new[] { temp.Create("A"), temp.Create("B") };
            var transforms = new[] { temp.Create("C").transform, temp.Create("D").transform };

            pool.Return(objects.AsSpan(), ReturningStrategy.Default);
            pool.Return(transforms.AsSpan(), ReturningStrategy.Default);

            Assert.AreEqual(4, pool.UnusedCount);
            Assert.IsFalse(objects[0].activeSelf);
            Assert.IsFalse(objects[1].activeSelf);
            Assert.IsFalse(transforms[0].gameObject.activeSelf);
            Assert.IsFalse(transforms[1].gameObject.activeSelf);
        }

        [Test]
        public void ReturnIdAndTransformIdPairs_StoreTheObjectsAndDeactivateThem()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(2u);
            var objects = new[] { temp.Create("A"), temp.Create("B") };
            var gameObjectIds = new[] { new GameObjectId(objects[0]), new GameObjectId(objects[1]) };
            var transformIds = new[] { new TransformId(objects[0].transform), new TransformId(objects[1].transform) };

            pool.Return(gameObjectIds.AsSpan(), transformIds.AsSpan(), ReturningStrategy.Default);

            Assert.AreEqual(2, pool.UnusedCount);
            Assert.IsFalse(objects[0].activeSelf);
            Assert.IsFalse(objects[1].activeSelf);
        }

        [Test]
        public void ReturnGameObjectIdSpan_StoresTheObjectsAndDeactivatesThem()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(2u);
            var objects = new[] { temp.Create("A"), temp.Create("B") };
            var ids = new[] { new GameObjectId(objects[0]), new GameObjectId(objects[1]) };

            pool.Return(ids.AsSpan(), ReturningStrategy.Default);

            Assert.AreEqual(2, pool.UnusedCount);
            Assert.IsFalse(objects[0].activeSelf);
            Assert.IsFalse(objects[1].activeSelf);
        }

        [Test]
        public void ReturnTransformIdSpan_StoresTheObjectsAndDeactivatesThem()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(2u);
            var objects = new[] { temp.Create("A"), temp.Create("B") };
            var ids = new[] { new TransformId(objects[0].transform), new TransformId(objects[1].transform) };

            pool.Return(ids.AsSpan(), ReturningStrategy.Default);

            Assert.AreEqual(2, pool.UnusedCount);
            Assert.IsFalse(objects[0].activeSelf);
            Assert.IsFalse(objects[1].activeSelf);
        }

        [Test]
        public void ReturnIdSpans_DoNothingStrategy_KeepTheObjectsActive()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(2u);
            var objects = new[] { temp.Create("A"), temp.Create("B") };
            var gameObjectIds = new[] { new GameObjectId(objects[0]) };
            var transformIds = new[] { new TransformId(objects[1].transform) };

            pool.Return(gameObjectIds.AsSpan(), ReturningStrategy.DoNothing);
            pool.Return(transformIds.AsSpan(), ReturningStrategy.DoNothing);

            Assert.AreEqual(2, pool.UnusedCount);
            Assert.IsTrue(objects[0].activeSelf);
            Assert.IsTrue(objects[1].activeSelf);
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(10)]
        public void RemoveUnused_ByAmount_RemovesFromTheEnd(int amount)
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(3u);

            for (var i = 0; i < 3; i++)
            {
                pool.Return(temp.Create(), ReturningStrategy.Default);
            }

            var removed = pool.RemoveUnused(amount);

            Assert.AreEqual(Math.Min(amount, 3), removed);
            Assert.AreEqual(3 - removed, pool.UnusedCount);
        }

        [Test]
        public void RemoveUnused_EmptyPool_RemovesNothing()
        {
            using var pool = new SceneObjectPool(0u);

            Assert.AreEqual(0, pool.RemoveUnused(4));
            Assert.AreEqual(0, pool.RemoveUnused(Span<GameObjectId>.Empty, Span<TransformId>.Empty));
            Assert.AreEqual(0, pool.RemoveUnused(new GameObjectId[2], new TransformId[2]));
        }

        [Test]
        public void RemoveUnused_EmptyBuffers_RemovesNothing()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(1u);

            pool.Return(temp.Create(), ReturningStrategy.Default);

            Assert.AreEqual(0, pool.RemoveUnused(Span<GameObjectId>.Empty, Span<TransformId>.Empty));
            Assert.AreEqual(1, pool.UnusedCount);
        }

        [Test]
        public void RemoveUnused_BuffersSmallerThanThePool_ReceiveTheLastIds()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(3u);
            var objects = new[] { temp.Create("A"), temp.Create("B"), temp.Create("C") };
            var gameObjectIds = new GameObjectId[2];
            var transformIds = new TransformId[2];

            for (var i = 0; i < objects.Length; i++)
            {
                pool.Return(objects[i], ReturningStrategy.Default);
            }

            var removed = pool.RemoveUnused(gameObjectIds.AsSpan(), transformIds.AsSpan());

            Assert.AreEqual(2, removed);
            Assert.AreEqual(1, pool.UnusedCount);
            Assert.AreEqual(objects[1].GetEntityId(), gameObjectIds[0].Value);
            Assert.AreEqual(objects[2].GetEntityId(), gameObjectIds[1].Value);
            Assert.AreEqual(objects[1].transform.GetEntityId(), transformIds[0].Value);
            Assert.AreEqual(objects[2].transform.GetEntityId(), transformIds[1].Value);
        }

        [Test]
        public void RemoveUnused_BuffersLargerThanThePool_ReceiveAllIdsAtTheStart()
        {
            using var temp = new TempGameObjects();
            using var pool = new SceneObjectPool(2u);
            var objects = new[] { temp.Create("A"), temp.Create("B") };
            var gameObjectIds = new GameObjectId[4];
            var transformIds = new TransformId[4];

            for (var i = 0; i < objects.Length; i++)
            {
                pool.Return(objects[i], ReturningStrategy.Default);
            }

            var removed = pool.RemoveUnused(gameObjectIds.AsSpan(), transformIds.AsSpan());

            Assert.AreEqual(2, removed);
            Assert.AreEqual(0, pool.UnusedCount);
            Assert.AreEqual(objects[0].GetEntityId(), gameObjectIds[0].Value);
            Assert.AreEqual(objects[1].GetEntityId(), gameObjectIds[1].Value);
            Assert.IsFalse(gameObjectIds[2].IsValid);
            Assert.IsFalse(transformIds[3].IsValid);
        }
    }
}
