using System.Runtime.CompilerServices;
using System.Threading;
#if UNITY_ADDRESSABLES
using EncosyTower.AddressableKeys;
#endif
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.ResourceKeys;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows.Loading
{
    /// <summary>Loads page assets by key, with Addressables or Resources.</summary>
    public static class PageAssetLoader
    {
        /// <summary>Gets whether the Addressables package is installed in the project.</summary>
        public static bool IsAddressablesAvailable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_ADDRESSABLES
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>Loads the asset of a key.</summary>
        /// <typeparam name="T">The type of the asset.</typeparam>
        /// <param name="strategy">
        /// How to load the asset. <see cref="PageLoaderStrategy.Addressables"/> loads with Resources when
        /// <see cref="IsAddressablesAvailable"/> is <c>false</c>.
        /// </param>
        /// <param name="assetKey">The key of the asset.</param>
        /// <param name="token">Cancels the load.</param>
        /// <returns>
        /// The lease of the loaded asset; otherwise the <see cref="PageAssetLoadError"/> that says why the load failed.
        /// </returns>
        /// <remarks>
        /// Release the lease with <see cref="PageAssetHandle{T}.Release"/> when the asset is no longer needed.
        /// </remarks>
        public static async UnityTask<Result<PageAssetHandle<T>, PageAssetLoadError>> LoadAsync<T>(
              PageLoaderStrategy strategy
            , AssetKey assetKey
            , CancellationToken token
        )
            where T : UnityEngine.Object
        {
#if UNITY_ADDRESSABLES
            if (strategy == PageLoaderStrategy.Addressables)
            {
                AddressableKey<T> addressableKey = assetKey;
                var addressableResult = await addressableKey.LoadGetHandleOrErrorAsync(token);

                if (addressableResult.TryGetValue(out var pair))
                {
                    return new PageAssetHandle<T>(pair.Value, pair.Handle);
                }

                if (addressableResult.TryGetError(out var addressableError))
                {
                    return PageAssetLoadError.Addressable(addressableError);
                }

                return PageAssetLoadError.Undefined();
            }
#endif

            ResourceKey<T> resourceKey = assetKey;
            var resourceResult = await resourceKey.LoadOrErrorAsync(token);

            if (resourceResult.TryGetValue(out var asset))
            {
                return new PageAssetHandle<T>(asset);
            }

            if (resourceResult.TryGetError(out var resourceError))
            {
                return PageAssetLoadError.Resource(resourceError);
            }

            return PageAssetLoadError.Undefined();
        }
    }
}
