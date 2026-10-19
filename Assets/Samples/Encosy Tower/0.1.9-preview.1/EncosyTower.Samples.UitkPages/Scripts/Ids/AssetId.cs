using System.Runtime.CompilerServices;
using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UitkPages.Ids
{
    using Config = EncosyTower.Samples.UitkPages.Ids;

    /// <summary>
    /// The id of any asset the Book loads: the kind is the asset's category, such as a page role or the motif images,
    /// and the id is the asset in that category.
    /// </summary>
    /// <remarks>
    /// Each category enum, such as <see cref="ScreenPage"/> or <see cref="Dimension"/>, adds itself as a kind with
    /// <see cref="KindForUnionIdAttribute"/> and adds its members to <see cref="AssetType"/> with
    /// <see cref="EnumMembersForTemplateAttribute"/>, at the same order, so a kind's value is the offset of its
    /// members in <see cref="AssetType"/>. The default id has the <see cref="IdKind.None"/> kind and names no asset.
    /// <see cref="Config.AssetKeyAPI"/> composes the key of an asset from the names of its kind and its member.
    /// </remarks>
    [UnionId(KindSettings = UnionIdKindSettings.PreserveOrder)]
    [UnionIdKind(typeof(NoneType), 0, nameof(NoneType.None))]
    internal readonly partial struct AssetId
    {
        /// <summary>Initializes the id of an asset from its member in <see cref="AssetType"/>.</summary>
        /// <param name="assetType">
        /// The asset; <see cref="AssetType.None"/> or an undefined value gives the default id.
        /// </param>
        public AssetId(AssetType assetType)
            : this()
        {
            if (assetType.TryConvert(out var order, out var value))
            {
                Kind = (IdKind)order;
                IdUnsigned = (byte)value;
            }
        }

        /// <summary>Converts an id to its member in <see cref="AssetType"/>.</summary>
        /// <param name="id">The id.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetType(AssetId id)
        {
            var value = (ushort)id.IdUnsigned;
            var order = (ushort)id.Kind;
            return (AssetType)(value + order);
        }

        /// <summary>Converts an id to its member in <see cref="AssetType"/>, with the enum extensions.</summary>
        /// <param name="id">The id.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetTypeExtended(AssetId id)
            => (AssetType)id;
    }
}
