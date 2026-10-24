using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UguiPages.Ids
{
    /// <summary>The screen pages: one fills the Book at a time, in the screen history.</summary>
    [EnumExtensions]
    [EnumMembersForTemplate(typeof(AssetType_EnumTemplate), 100)]
    [KindForUnionId(typeof(AssetId), 100, "Screen")]
    internal enum ScreenPage : byte
    {
        MainMenu,
        Entrance,

        /// <summary>The screen of the core dimension.</summary>
        CoreDimension,

        /// <summary>The screen of an outer dimension.</summary>
        Dimension,
    }
}
