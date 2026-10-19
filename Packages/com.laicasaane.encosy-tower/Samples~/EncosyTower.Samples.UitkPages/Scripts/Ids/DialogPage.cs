using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>The dialog pages: they open over the screen, one on top of another.</summary>
    [EnumExtensions]
    [EnumMembersForTemplate(typeof(AssetType_EnumTemplate), 200)]
    [KindForUnionId(typeof(AssetId), 200, "Dialog")]
    internal enum DialogPage : byte
    {
        Map,
        More,
        Echo,
        TowerIncantation,
        Settings,

        /// <summary>A dialog without an asset: the sample opens it to show how a missing page fails.</summary>
        LostAppendix,
    }
}
