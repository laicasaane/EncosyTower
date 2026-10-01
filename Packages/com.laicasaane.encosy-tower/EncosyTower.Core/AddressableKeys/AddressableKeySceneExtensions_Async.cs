#if UNITY_ADDRESSABLES

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace EncosyTower.AddressableKeys
{
    using Error = AddressableKeyError;

    public static partial class AddressableKeySceneExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<SceneInstance> LoadAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode = LoadSceneMode.Single
            , bool activateOnLoad = true
            , int priority = 100
            , CancellationToken token = default
        )
        {
            var result = await TryLoadGetHandleAsync(key, mode, activateOnLoad, priority, token);
            return result.GetValueOrDefault().Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<Option<SceneInstance>> TryLoadAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode = LoadSceneMode.Single
            , bool activateOnLoad = true
            , int priority = 100
            , CancellationToken token = default
        )
        {
            var result = await TryLoadGetHandleAsync(key, mode, activateOnLoad, priority, token);
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<Result<SceneInstance, AddressableKeyError>> LoadOrErrorAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode = LoadSceneMode.Single
            , bool activateOnLoad = true
            , int priority = 100
            , CancellationToken token = default
        )
        {
            var result = await LoadGetHandleOrErrorAsync(key, mode, activateOnLoad, priority, token);

            if (result.TryGetValue(out var value))
            {
                return value.Value;
            }

            if (result.TryGetError(out var error))
            {
                return error;
            }

            return Error.Undefined((AddressableKey)key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<ValueHandlePair<SceneInstance>> LoadGetHandleAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode = LoadSceneMode.Single
            , bool activateOnLoad = true
            , int priority = 100
            , CancellationToken token = default
        )
        {
            var result = await TryLoadGetHandleAsync(key, mode, activateOnLoad, priority, token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<Option<ValueHandlePair<SceneInstance>>> TryLoadGetHandleAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode
            , bool activateOnLoad
            , int priority
            , CancellationToken token = default
        )
        {
            var result = await LoadGetHandleOrErrorAsync(key, mode, activateOnLoad, priority, token);
            return result.Value;
        }

        public static async UnityTask<Result<ValueHandlePair<SceneInstance>, AddressableKeyError>> LoadGetHandleOrErrorAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode
            , bool activateOnLoad
            , int priority
            , CancellationToken token = default
        )
        {
            if (key.IsValid == false)
            {
                return Error.InvalidKey((AddressableKey)key);
            }

            try
            {
                var handle = Addressables.LoadSceneAsync(key.Value.Value, mode, activateOnLoad, priority);

                if (handle.IsValid() == false)
                {
                    handle.TryRelease();
                    return Error.InvalidHandle((AddressableKey)key);
                }

                while (handle.IsDone == false)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    await UnityTask.NextFrameAsync(token);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }

                if (token.IsCancellationRequested)
                {
                    handle.TryRelease();
                    return Error.CancelledRequest((AddressableKey)key);
                }

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return new ValueHandlePair<SceneInstance>(handle.Result, handle);
                }

                handle.TryRelease();
                return Error.FailedStatus((AddressableKey)key, handle.Status);
            }
            catch (Exception ex)
            {
                return Error.Exception((AddressableKey)key, ex);
            }
        }
    }
}

#endif
