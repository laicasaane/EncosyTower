using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>The attachment pages: they are attached to a screen.</summary>
    [EnumExtensions]
    [EnumMembersForTemplate(typeof(AssetType_EnumTemplate), 400)]
    [KindForUnionId(typeof(AssetId), 400, "Attachment")]
    internal enum AttachmentPage : byte
    {
        NavigationBar,
    }
}
