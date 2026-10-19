
namespace EncosyTower.PageFlows.Loading
{
    /// <summary>Selects how a page asset is loaded.</summary>
    public enum PageLoaderStrategy
    {
        /// <summary>
        /// Load the asset with Addressables. Without the Addressables package, the loader uses Resources instead.
        /// </summary>
        Addressables,

        /// <summary>Load the asset with Resources.</summary>
        Resources,
    }
}
