using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UitkPages.Ids
{
    using Config = EncosyTower.Samples.UitkPages.Ids;

    /// <summary>The runes that the outer dimensions hold.</summary>
    /// <remarks>The asset of a rune is its rune image; <see cref="Config.AssetKeyAPI"/> composes its key.</remarks>
    [EnumExtensions]
    [EnumMembersForTemplate(typeof(AssetType_EnumTemplate), 800)]
    [KindForUnionId(typeof(AssetId), 800, "Rune")]
    internal enum Rune : byte
    {
        Ehwaz,
        Naudiz,
        Kaun,
        Othala,
        Sowilo,
        Isa,
    }
}
