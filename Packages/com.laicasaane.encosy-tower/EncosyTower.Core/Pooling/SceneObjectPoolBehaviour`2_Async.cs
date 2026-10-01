#if UNITY_MATHEMATICS

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Loaders;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
    partial class SceneObjectPoolBehaviour<TKey, TKeyComparer>
        where TKey : ITryLoad<GameObject>, ITryLoadAsync<GameObject>
        where TKeyComparer : IEqualityComparer<TKey>, new()
    {
        protected async UnityTask InitializeAsync(
              [NotNull] IReadOnlyCollection<KeyEntry<TKey>> entries
            , int desiredJobCount = -1
            , CancellationToken token = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(entries);
            Dispose();

            var poolMap = _poolMap = new(entries.Count, new TKeyComparer());
            var trimCloneSuffix = TrimCloneSuffix;
            var initialCapacity = 1u;
            Option<Scene> firstScene = Option.None;

            foreach (var (key, scene, entryCapacity, position, rotation, scale) in entries)
            {
                if (poolMap.ContainsKey(key))
                {
                    continue;
                }

                var prefabOpt = await key.TryLoadAsync(token);

                if (prefabOpt.TryGetValue(out var prefab) == false)
                {
                    continue;
                }

                if (firstScene.HasValue == false)
                {
                    firstScene = scene;
                }

                initialCapacity += entryCapacity;

                var pool = new SceneObjectPool(entryCapacity) {
                    Scene = scene,
                    Source = prefab,
                    TrimCloneSuffix = trimCloneSuffix,
                };

                AddToMap(poolMap, key, pool, position, rotation, scale);
            }

            var defaultScene = firstScene.GetValueOrDefault(gameObject.scene);

            _context ??= new SceneObjectPoolContext();
            _context.Initialize((int)initialCapacity, desiredJobCount);
            _context.CreateDefaultGameObject(defaultScene, HideDefaultGameObject, GetType());

            await OnInitializeAsync();
        }

        protected virtual UnityTask OnInitializeAsync()
            => UnityTask.CompletedTask;

        protected async UnityTask<bool> RegisterAsync(
              [NotNull] TKey key
            , Scene scene
            , uint initialCapacity
            , CancellationToken token = default
            , Option<Vector3> defaultPosition = default
            , Option<Vector3> defaultRotation = default
            , Option<Vector3> defaultScale = default
        )
        {
            Debugging.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(key);
            AssertInitialization(this);

            if (_poolMap.ContainsKey(key))
            {
                return false;
            }

            var prefabOpt = await key.TryLoadAsync(token);

            if (prefabOpt.TryGetValue(out var prefab) == false)
            {
                return false;
            }

            var pool = new SceneObjectPool(initialCapacity) {
                Scene = scene,
                Source = prefab,
                TrimCloneSuffix = TrimCloneSuffix,
            };

            AddToMap(_poolMap, key, pool, defaultPosition, defaultRotation, defaultScale);

            _context.IncreaseCapacityBy((int)initialCapacity);

            return true;
        }
    }
}

#endif
