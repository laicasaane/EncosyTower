using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>The tab pages of a dimension screen.</summary>
    [EnumExtensions]
    [EnumMembersForTemplate(typeof(AssetType_EnumTemplate), 500)]
    [KindForUnionId(typeof(AssetId), 500, "Tab")]
    internal enum TabPage : byte
    {
        Chronicle,
        Inscription,
    }
}
