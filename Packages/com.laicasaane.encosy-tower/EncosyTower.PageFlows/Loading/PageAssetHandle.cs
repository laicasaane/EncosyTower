#if UNITY_ADDRESSABLES
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace EncosyTower.PageFlows.Loading
{
    /// <summary>A loaded page asset and the handle that keeps it loaded.</summary>
    public readonly struct PageAssetHandle<T>
        where T : UnityEngine.Object
    {
        private readonly T _asset;

#if UNITY_ADDRESSABLES
        private readonly AsyncOperationHandle<T> _handle;

        internal PageAssetHandle(T asset, AsyncOperationHandle<T> handle)
        {
            _asset = asset;
            _handle = handle;
        }
#endif

        internal PageAssetHandle(T asset)
        {
            _asset = asset;
#if UNITY_ADDRESSABLES
            _handle = default;
#endif
        }

        /// <summary>The loaded asset.</summary>
        public T Asset => _asset;

        /// <summary>Releases the Addressables handle; does nothing for a Resources asset or a default lease.</summary>
        public void Release()
        {
#if UNITY_ADDRESSABLES
            if (_handle.IsValid())
            {
                _handle.TryRelease();
            }
#endif
        }
    }
}
