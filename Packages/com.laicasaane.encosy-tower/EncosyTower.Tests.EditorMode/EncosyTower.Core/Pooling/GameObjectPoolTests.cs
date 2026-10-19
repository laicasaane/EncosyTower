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
    public class GameObjectPoolTests
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

            var castGameObjectIds = MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds.AsSpan());
            var castTransformIds = MemoryMarshal.Cast<TransformId, EntityId>(transformIds.AsSpan());

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
            var ids = new GameObjectId[1];
            var cast = MemoryMarshal.Cast<GameObjectId, EntityId>(ids.AsSpan());

            Assert.AreEqual(1, cast.Length);
            Assert.AreEqual(default(EntityId), cast[0]);
        }

        [Test]
        public void CastIdsToEntityIds_WritingThroughTheCast_ChangesTheIds()
        {
            using var temp = new TempGameObjects();
            var go = temp.Create();
            var ids = new GameObjectId[2];
            var cast = MemoryMarshal.Cast<GameObjectId, EntityId>(ids.AsSpan());

            cast[1] = go.GetEntityId();

            Assert.IsFalse(ids[0].IsValid);
            Assert.AreEqual(go.GetEntityId(), ids[1].Value);
        }

        [Test]
        public void NewPool_StartsEmpty()
        {
            using var pool = new GameObjectPool();

            Assert.AreEqual(0, pool.UnusedCount);
            Assert.IsNull(pool.Prefab);
            Assert.IsFalse(pool.TrimCloneSuffix);
            Assert.AreEqual(RentingStrategy.Default, pool.RentingStrategy);
            Assert.AreEqual(ReturningStrategy.Default, pool.ReturningStrategy);
        }

        [Test]
        public void Prefab_SetNull_ThrowsNullReferenceException()
        {
            using var pool = new GameObjectPool();

            Assert.Throws<NullReferenceException>(() => pool.Prefab = null);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void Prepool_WithoutPrefab_ThrowsNullReferenceException(int amount)
        {
            using var pool = new GameObjectPool();

            Assert.Throws<NullReferenceException>(() => pool.Prepool(amount));
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void Dispose_Twice_DoesNotThrow()
        {
            var pool = new GameObjectPool();

            pool.Dispose();

            Assert.DoesNotThrow(() => pool.Dispose());
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void Dispose_ClearsThePrefabAndTheUnusedObjects()
        {
            using var temp = new TempGameObjects();
            var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };

            pool.Return(temp.Create(), ReturningStrategy.Default);
            pool.Dispose();

            Assert.AreEqual(0, pool.UnusedCount);
            Assert.IsNull(pool.Prefab);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(5)]
        public void ReleaseInstances_EmptyPool_DoesNothing(int keep)
        {
            using var pool = new GameObjectPool();
            var released = false;

            pool.ReleaseInstances(keep, _ => released = true);

            Assert.IsFalse(released);
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void ReleaseInstances_KeepAtLeastTheUnusedCount_DoesNothing()
        {
            using var temp = new TempGameObjects();
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
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
            using var pool = new GameObjectPool();

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
            using var pool = new GameObjectPool();

            pool.Return(default(GameObjectId), ReturningStrategy.Default);
            pool.Return(default(TransformId), ReturningStrategy.Default);

            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void Return_NullObjects_DoNothing()
        {
            using var pool = new GameObjectPool();

            pool.Return((GameObject)null, ReturningStrategy.Default);
            pool.Return((Transform)null, ReturningStrategy.Default);

            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void Return_DestroyedObject_DoesNothing()
        {
            using var temp = new TempGameObjects();
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
            var go = new GameObject("Destroyed");

            UnityEngine.Object.DestroyImmediate(go);
            pool.Return(go, ReturningStrategy.Default);

            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void Return_GameObject_StoresItAndDeactivatesIt()
        {
            using var temp = new TempGameObjects();
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
            var go = temp.Create();

            pool.Return(go, ReturningStrategy.Default);

            Assert.AreEqual(1, pool.UnusedCount);
            Assert.IsFalse(go.activeSelf);
        }

        [Test]
        public void Return_DoNothingStrategy_KeepsTheObjectActive()
        {
            using var temp = new TempGameObjects();
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
            var go = temp.Create();

            pool.Return(go, ReturningStrategy.DoNothing);

            Assert.AreEqual(1, pool.UnusedCount);
            Assert.IsTrue(go.activeSelf);
        }

        [Test]
        public void Return_Transform_StoresTheOwningGameObject()
        {
            using var temp = new TempGameObjects();
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
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
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
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
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
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
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
            var first = temp.Create("First");
            var second = temp.Create("Second");

            pool.Return(first, ReturningStrategy.Default);
            pool.Return(second, ReturningStrategy.Default);

            Assert.AreSame(second, pool.RentGameObject(RentingStrategy.Default));
            Assert.AreSame(first, pool.RentGameObject(RentingStrategy.Default));
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void RentTransformAndIds_AfterReturn_MatchTheReturnedObject()
        {
            using var temp = new TempGameObjects();
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
            var go = temp.Create();

            pool.Return(go, ReturningStrategy.Default);
            Assert.AreSame(go.transform, pool.RentTransform(RentingStrategy.Default));

            pool.Return(go, ReturningStrategy.Default);
            Assert.AreEqual(go.GetEntityId(), pool.RentGameObjectId(RentingStrategy.Default).Value);

            pool.Return(go, ReturningStrategy.Default);
            Assert.AreEqual(go.transform.GetEntityId(), pool.RentTransformId(RentingStrategy.Default).Value);
            Assert.AreEqual(0, pool.UnusedCount);
        }

        [Test]
        public void ReturnObjectSpan_ThenRentSpan_GivesTheLastReturnedObjects()
        {
            using var temp = new TempGameObjects();
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
            var objects = new[] { temp.Create("A"), temp.Create("B"), temp.Create("C") };
            var rented = new GameObject[2];

            pool.Return(objects.AsSpan(), ReturningStrategy.Default);

            Assert.AreEqual(3, pool.UnusedCount);
            Assert.IsFalse(objects[0].activeSelf);
            Assert.IsFalse(objects[2].activeSelf);

            pool.Rent(rented.AsSpan(), RentingStrategy.Default);

            Assert.AreSame(objects[1], rented[0]);
            Assert.AreSame(objects[2], rented[1]);
            Assert.IsTrue(objects[1].activeSelf);
            Assert.IsTrue(objects[2].activeSelf);
            Assert.IsFalse(objects[0].activeSelf);
            Assert.AreEqual(1, pool.UnusedCount);
        }

        [Test]
        public void ReturnIdSpan_ThenRentIdSpan_GivesTheLastReturnedIds()
        {
            using var temp = new TempGameObjects();
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
            var objects = new[] { temp.Create("A"), temp.Create("B"), temp.Create("C") };
            var ids = new GameObjectId[objects.Length];
            var rented = new GameObjectId[2];

            for (var i = 0; i < objects.Length; i++)
            {
                ids[i] = new GameObjectId(objects[i]);
            }

            pool.Return(ids.AsSpan(), ReturningStrategy.Default);

            Assert.AreEqual(3, pool.UnusedCount);
            Assert.IsFalse(objects[0].activeSelf);
            Assert.IsFalse(objects[1].activeSelf);
            Assert.IsFalse(objects[2].activeSelf);

            pool.Rent(rented.AsSpan(), RentingStrategy.Default);

            Assert.AreEqual(objects[1].GetEntityId(), rented[0].Value);
            Assert.AreEqual(objects[2].GetEntityId(), rented[1].Value);
            Assert.IsTrue(objects[1].activeSelf);
            Assert.IsTrue(objects[2].activeSelf);
            Assert.AreEqual(1, pool.UnusedCount);
        }

        [Test]
        public void ReturnTransformSpan_StoresTheOwningGameObjectsAndDeactivatesThem()
        {
            using var temp = new TempGameObjects();
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
            var objects = new[] { temp.Create("A"), temp.Create("B") };
            var transforms = new[] { objects[0].transform, objects[1].transform };

            pool.Return(transforms.AsSpan(), ReturningStrategy.Default);

            Assert.AreEqual(2, pool.UnusedCount);
            Assert.IsFalse(objects[0].activeSelf);
            Assert.IsFalse(objects[1].activeSelf);
            Assert.AreSame(objects[1], pool.RentGameObject(RentingStrategy.Default));
        }

        [Test]
        public void ReturnIdAndTransformIdPairs_StoreTheObjectsWithoutChangingTheirState()
        {
            using var temp = new TempGameObjects();
            using var pool = new GameObjectPool { Prefab = new GameObjectPrefab { Source = temp.Create() } };
            var go = temp.Create();
            var gameObjectIds = new[] { new GameObjectId(go) };
            var transformIds = new[] { new TransformId(go.transform) };

            pool.Return(gameObjectIds.AsSpan(), transformIds.AsSpan(), ReturningStrategy.DoNothing);

            Assert.AreEqual(1, pool.UnusedCount);
            Assert.IsTrue(go.activeSelf);
        }
    }
}
