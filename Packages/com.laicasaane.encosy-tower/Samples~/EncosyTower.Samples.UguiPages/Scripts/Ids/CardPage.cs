using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UguiPages.Ids
{
    /// <summary>The card pages of a dimension screen.</summary>
    [EnumExtensions]
    [EnumMembersForTemplate(typeof(AssetType_EnumTemplate), 600)]
    [KindForUnionId(typeof(AssetId), 600, "Card")]
    internal enum CardPage : byte
    {
        Concept,
        InPractice,
    }
}
