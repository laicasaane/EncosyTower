using EncosyTower.AssetKeys;
using EncosyTower.Samples.UguiPages.Ids;

namespace EncosyTower.Samples.UguiPages.Config
{
    /// <summary>
    /// One dimension: its id, its texts and its colors; the key of its motif image comes from the id.
    /// </summary>
    internal sealed record class DimensionData(
          Dimension Id
        , string Title
        , string Lore
        , string Motif
        , string InscriptionObject
        , string Concept
        , string Practice
        , string Watch
        , ThemedColor Surface
        , ThemedColor Navigation
        , int Index
    )
    {
        /// <summary>Gets the key of the motif image of the dimension.</summary>
        public AssetKey MotifKey => AssetKeyAPI.Key(Id);
    }
}
