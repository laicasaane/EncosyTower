#if UNITY_LOCALIZATION

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Tasks;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.Localization
{
    public static partial class L10nKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<string> LocalizeAsync(
              this L10nKey<string> key
            , CancellationToken token = default
        )
            => LocalizeAsyncInternal(key, token, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<string> LocalizeAsync(
              this L10nKey<string> key
            , params object[] arguments
        )
            => LocalizeAsyncInternal(key, default, arguments);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<string> LocalizeAsync(
              this L10nKey<string> key
            , CancellationToken token
            , params object[] arguments
        )
            => LocalizeAsyncInternal(key, token, arguments);

        private static async UnityTask<string> LocalizeAsyncInternal(
              L10nKey<string> key
            , CancellationToken token
            , object[] arguments
        )
        {
            if (key.IsValid == false) return string.Empty;

            var handle = arguments == null || arguments.Length < 1
                ? LocalizationSettings.StringDatabase.GetLocalizedStringAsync(key.Table, key.Entry)
                : LocalizationSettings.StringDatabase.GetLocalizedStringAsync(key.Table, key.Entry, arguments)
                ;

            if (handle.IsValid() == false)
            {
                return default;
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
                return default;
            }

            return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : default;
        }

        public static async UnityTask<TObject> LocalizeAssetAsync<TObject>(
              this L10nKey<TObject> key
            , CancellationToken token = default
        )
            where TObject : UnityEngine.Object
        {
            if (key.IsValid == false) return default;

            var handle = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TObject>(key.Table, key.Entry);

            if (handle.IsValid() == false)
            {
                return default;
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
                return default;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                return default;
            }

            return handle.Result;
        }
    }
}

#endif
