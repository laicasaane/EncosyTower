using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Loaders;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.AtlasedSprites
{
    using Error = AtlasedSpriteKeyError;

    partial struct AtlasedSpriteKeyResources
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
        public readonly async UnityTask<Option<Sprite>> TryLoadAsync(CancellationToken token = default)
        {
            var result = await LoadOrErrorAsync(token);
            return result.Value;
        }

        public readonly async UnityTask<Result<Sprite, AtlasedSpriteKeyError>> LoadOrErrorAsync(
            CancellationToken token = default
        )
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((AtlasedSpriteKey)this);
            }

            var atlasResult = await Atlas.LoadOrErrorAsync(token);

            if (atlasResult.TryGetError(out var atlasError))
            {
                return Error.From(atlasError, (AtlasedSpriteKey)this);
            }

            if (atlasResult.TryGetValue(out var atlasValue) == false)
            {
                return Error.Undefined((AtlasedSpriteKey)this);
            }

            var spriteResult = atlasValue.GetSpriteOrError(_sprite);

            if (spriteResult.TryGetValue(out var spriteValue))
            {
                return spriteValue;
            }

            if (spriteResult.TryGetError(out var spriteError))
            {
                return spriteError;
            }

            return Error.Undefined((AtlasedSpriteKey)this);
        }
    }
}
