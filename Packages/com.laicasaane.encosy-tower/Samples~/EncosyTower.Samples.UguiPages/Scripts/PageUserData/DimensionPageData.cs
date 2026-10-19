using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.State;

namespace EncosyTower.Samples.UguiPages.PageUserData
{
    /// <summary>
    /// The page user data that every page of the sample gets in
    /// <see cref="EncosyTower.PageFlows.PageContext.UserData"/>.
    /// </summary>
    /// <remarks>Demonstrates passing app data to pages through the call context.</remarks>
    internal sealed record class DimensionPageData(AppState State, BookLocation Location);
}
