using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.ResourceKeys;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
#if UNITY_ADDRESSABLES
    using EncosyTower.AddressableKeys;
#endif

    public static class PageAssetLoader
    {
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

        public static UnityTask<Option<T>> LoadAsync<T>(
              PageLoaderStrategy strategy
            , string assetKey
            , CancellationToken token
        )
            where T : UnityEngine.Object
        {
#if UNITY_ADDRESSABLES
            if (strategy == PageLoaderStrategy.Addressables)
            {
                return new AddressableKey(assetKey).TryLoadAsync<T>(token);
            }
#endif

            return new ResourceKey(assetKey).TryLoadAsync<T>(token);
        }
    }
}
