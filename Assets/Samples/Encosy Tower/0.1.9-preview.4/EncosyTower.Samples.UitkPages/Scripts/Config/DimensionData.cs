using EncosyTower.AssetKeys;
using EncosyTower.Samples.UitkPages.Ids;

namespace EncosyTower.Samples.UitkPages.Config
{
    /// <summary>One dimension: its id and its texts; the key of its motif image comes from the id.</summary>
    internal sealed record class DimensionData(
          Dimension Id
        , string Title
        , string Lore
        , string Motif
        , string InscriptionObject
        , string Concept
        , string Practice
        , string Watch
        , int Index
    )
    {
        /// <summary>Gets the key of the motif image of the dimension.</summary>
        public AssetKey MotifKey => AssetKeyAPI.Key(Id);
    }
}
