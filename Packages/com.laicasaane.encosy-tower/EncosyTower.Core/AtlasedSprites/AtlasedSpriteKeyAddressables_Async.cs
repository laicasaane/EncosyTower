#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.AddressableKeys;
using EncosyTower.Common;
using EncosyTower.Loaders;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.AtlasedSprites
{
    using Error = AtlasedSpriteKeyError;
    using ValueHandlePair = ValueHandlePair<Sprite, SpriteAtlas>;

    partial struct AtlasedSpriteKeyAddressables
        : ILoadAsync<Sprite>
        , ITryLoadAsync<Sprite>
        , ILoadOrErrorAsync<Sprite, Error>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask<Sprite> LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask<ValueHandlePair<Sprite, SpriteAtlas>> LoadGetHandleAsync(
            CancellationToken token = default
        )
        {
            var result = await TryLoadGetHandleAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask<Option<Sprite>> TryLoadAsync(CancellationToken token = default)
        {
            var result = await LoadOrErrorAsync(token);
            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask<Option<ValueHandlePair<Sprite, SpriteAtlas>>> TryLoadGetHandleAsync(
            CancellationToken token = default
        )
        {
            var result = await LoadGetHandleOrErrorAsync(token);
            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask<Result<Sprite, AtlasedSpriteKeyError>> LoadOrErrorAsync(
            CancellationToken token = default
        )
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

            return Error.Undefined((AtlasedSpriteKey)this);
        }

        public readonly async UnityTask<Result<ValueHandlePair<Sprite, SpriteAtlas>, AtlasedSpriteKeyError>> LoadGetHandleOrErrorAsync(
            CancellationToken token = default
        )
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((AtlasedSpriteKey)this);
            }

            var atlasResult = await Atlas.LoadGetHandleOrErrorAsync(token);

            if (atlasResult.TryGetError(out var atlasError))
            {
                return Error.From(atlasError, (AtlasedSpriteKey)this);
            }

            if (atlasResult.TryGetValue(out var atlasValue) == false)
            {
                return Error.Undefined((AtlasedSpriteKey)this);
            }

            var spriteResult = atlasValue.Value.GetSpriteOrError(_sprite);

            if (spriteResult.TryGetValue(out var spriteValue))
            {
                return new ValueHandlePair(spriteValue, atlasValue.Handle);
            }

            if (spriteResult.TryGetError(out var spriteError))
            {
                return spriteError;
            }

            return Error.Undefined((AtlasedSpriteKey)this);
        }
    }
}

#endif
