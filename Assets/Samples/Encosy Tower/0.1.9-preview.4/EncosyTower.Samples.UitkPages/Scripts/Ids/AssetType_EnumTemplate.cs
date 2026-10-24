using EncosyTower.EnumExtensions;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>The template of <see cref="AssetType"/>, the one enum of every asset the Book loads.</summary>
    /// <remarks>
    /// <see cref="NoneType"/> gives <see cref="AssetType.None"/>; each category enum adds its members at its own
    /// offset.
    /// </remarks>
    [EnumTemplate]
    [EnumTemplateMembersFromEnum(typeof(NoneType), 0)]
    internal readonly partial struct AssetType_EnumTemplate { }
}
