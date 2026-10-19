using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>The overlay pages: they show over every screen and dialog.</summary>
    [EnumExtensions]
    [EnumMembersForTemplate(typeof(AssetType_EnumTemplate), 300)]
    [KindForUnionId(typeof(AssetId), 300, "Overlay")]
    internal enum OverlayPage : byte
    {
        FloatingContext,
        Toast,
    }
}
