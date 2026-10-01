using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    using Error = ResourceKeyError;
    using UnityObject = UnityEngine.Object;

    public static partial class ResourceKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> LoadAsync<T>(
              this ResourceKey key
            , CancellationToken token = default
        )
            where T : UnityObject
            => ((ResourceKey<T>)key).LoadAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<T>> TryLoadAsync<T>(
              this ResourceKey key
            , CancellationToken token = default
        )
            where T : UnityObject
            => ((ResourceKey<T>)key).TryLoadAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Result<T, ResourceKeyError>> LoadOrErrorAsync<T>(
              this ResourceKey key
            , CancellationToken token = default
        )
            where T : UnityObject
            => ((ResourceKey<T>)key).LoadOrErrorAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<GameObject> InstantiateAsync(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((ResourceKey<GameObject>)key).InstantiateAsync(parent, inWorldSpace, trimCloneSuffix, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<GameObject>> TryInstantiateAsync(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((ResourceKey<GameObject>)key).TryInstantiateAsync(parent, inWorldSpace, trimCloneSuffix, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Result<GameObject, ResourceKeyError>> InstantiateOrErrorAsync(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((ResourceKey<GameObject>)key).InstantiateOrErrorAsync(parent, inWorldSpace, trimCloneSuffix, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<TComponent> InstantiateAsync<TComponent>(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((ResourceKey<GameObject>)key).InstantiateAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<TComponent>> TryInstantiateAsync<TComponent>(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((ResourceKey<GameObject>)key).TryInstantiateAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Result<TComponent, ResourceKeyError>> InstantiateOrErrorAsync<TComponent>(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((ResourceKey<GameObject>)key).InstantiateOrErrorAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<GameObject> InstantiateAsync(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await InstantiateOrErrorAsync(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.Value.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<TComponent> InstantiateAsync<TComponent>(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateOrErrorAsync<TComponent>(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.Value.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<Option<GameObject>> TryInstantiateAsync(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await InstantiateOrErrorAsync(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<Option<TComponent>> TryInstantiateAsync<TComponent>(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateOrErrorAsync<TComponent>(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<Result<GameObject, ResourceKeyError>> InstantiateOrErrorAsync(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await InstantiateOrErrorAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);

            if (result.TryGetValue(out var value))
            {
                return value.Instanced;
            }

            if (result.TryGetError(out var error))
            {
                return error;
            }

            return Error.Undefined((ResourceKey)key);
        }

        public static async UnityTask<Result<TComponent, Error>> InstantiateOrErrorAsync<TComponent>(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateOrErrorAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);

            if (result.TryGetError(out var error))
            {
                return error;
            }

            if (result.TryGetValue(out var value) == false)
            {
                return Error.InvalidObject((ResourceKey)key);
            }

            if (value.Instanced.TryGetComponent<TComponent>(out var comp))
            {
                return comp;
            }

            UnityObject.Destroy(value.Instanced);
            return Error.MissingComponent((ResourceKey)key, value.Prefab, typeof(TComponent));
        }

        private static async UnityTask<Result<InstancedAndPrefab, ResourceKeyError>> InstantiateOrErrorAsyncInternal(
              ResourceKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
            , CancellationToken token
        )
        {
            if (key.IsValid == false)
            {
                return Error.InvalidKey((ResourceKey)key);
            }

            var loadResult = await key.LoadOrErrorAsync(token);

            if (loadResult.TryGetError(out var loadError))
            {
                return loadError;
            }

            if (loadResult.TryGetValue(out var prefab) == false)
            {
                return Error.InvalidObject((ResourceKey)key);
            }

            if (token.IsCancellationRequested)
            {
                return Error.CancelledRequest((ResourceKey)key);
            }

            try
            {
                var go = UnityObject.Instantiate(prefab, parent.Transform, inWorldSpace);

                if (go.IsInvalid())
                {
                    return Error.InvalidInstantiation((ResourceKey)key, prefab);
                }

                if (parent is { IsValid: true, IsScene: true })
                {
                    go.MoveToScene(parent.Scene);
                }

                if (token.IsCancellationRequested)
                {
                    UnityObject.Destroy(go);
                    return Error.CancelledRequest((ResourceKey)key);
                }

                if (trimCloneSuffix)
                {
                    go.TrimCloneSuffix();
                }

                return new InstancedAndPrefab(go, prefab);
            }
            catch (Exception ex)
            {
                return Error.Exception((ResourceKey)key, ex);
            }
        }
    }
}
