using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UitkPages.Ids
{
    using Config = EncosyTower.Samples.UitkPages.Ids;

    /// <summary>The dimensions of the tower: the six outer dimensions, then the core.</summary>
    /// <remarks>
    /// The asset of a dimension is its motif image; <see cref="Config.AssetKeyAPI"/> composes its key.
    /// </remarks>
    [EnumExtensions]
    [EnumMembersForTemplate(typeof(AssetType_EnumTemplate), 700)]
    [KindForUnionId(typeof(AssetId), 700, "Dimension")]
    internal enum Dimension : byte
    {
        PubSub,
        Processing,
        Databases,
        Mvvm,
        PageFlows,
        Persistence,

        /// <summary>The core: it opens when every outer dimension is discovered.</summary>
        Core,
    }
}
