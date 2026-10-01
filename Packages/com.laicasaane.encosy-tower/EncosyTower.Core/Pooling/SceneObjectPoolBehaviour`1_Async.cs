#if UNITY_MATHEMATICS

using System.Threading;
using EncosyTower.Loaders;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
    partial class SceneObjectPoolBehaviour<TKey>
        where TKey : ITryLoad<GameObject>, ITryLoadAsync<GameObject>
    {
        protected async UnityTask InitializeAsync(
              TKey key
            , Scene scene
            , uint initialCapacity
            , int desiredJobCount = -1
            , CancellationToken token = default
        )
        {
            Dispose();

            var prefabOpt = await key.TryLoadAsync(token);

            if (prefabOpt.TryGetValue(out var prefab) == false)
            {
                return;
            }

            initialCapacity += 1;

            _pool = new SceneObjectPool(initialCapacity) {
                Scene = scene,
                Source = prefab,
                TrimCloneSuffix = TrimCloneSuffix,
            };

            _context ??= new SceneObjectPoolContext();
            _context.Initialize((int)initialCapacity, desiredJobCount);
            _context.CreateDefaultGameObject(scene, HideDefaultGameObject, GetType());

            await OnInitializeAsync();
        }

        protected virtual UnityTask OnInitializeAsync()
            => UnityTask.CompletedTask;
    }
}

#endif
