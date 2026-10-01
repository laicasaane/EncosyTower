#if UNITY_ADDRESSABLES

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Loaders;
using EncosyTower.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.AddressableKeys
{
    using Error = AddressableKeyError;

    partial struct AddressableKey<T> : ILoadAsync<T>, ITryLoadAsync<T>, ILoadOrErrorAsync<T, Error>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask<T> LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask<ValueHandlePair<T>> LoadGetHandleAsync(CancellationToken token = default)
        {
            var result = await TryLoadGetHandleAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask<Option<T>> TryLoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadGetHandleAsync(token);
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask<Result<T, Error>> LoadOrErrorAsync(CancellationToken token = default)
        {
            var result = await LoadGetHandleOrErrorAsync(token);

            if (result.TryGetValue(out var value))
            {
                return value.Value;
            }

            if (result.TryGetError(out var error))
            {
                return error;
            }

            return Error.Undefined((AddressableKey)this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask<Option<ValueHandlePair<T>>> TryLoadGetHandleAsync(
            CancellationToken token = default
        )
        {
            var result = await LoadGetHandleOrErrorAsync(token);
            return result.Value;
        }

        public readonly async UnityTask<Result<ValueHandlePair<T>, Error>> LoadGetHandleOrErrorAsync(
            CancellationToken token = default
        )
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((AddressableKey)this);
            }

            try
            {
                var handle = Addressables.LoadAssetAsync<T>(Value.Value);

                if (handle.IsValid() == false)
                {
                    handle.TryRelease();
                    return Error.InvalidHandle((AddressableKey)this);
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
                    return Error.CancelledRequest((AddressableKey)this);
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    handle.TryRelease();
                    return Error.FailedStatus((AddressableKey)this, handle.Status);
                }

                var asset = handle.Result;

                if ((asset is UnityEngine.Object obj && obj) || asset != null)
                {
                    return new ValueHandlePair<T>(asset, handle);
                }

                handle.TryRelease();
                return Error.InvalidObject((AddressableKey)this);
            }
            catch (Exception ex)
            {
                return Error.Exception((AddressableKey)this, ex);
            }
        }
    }
}

#endif
