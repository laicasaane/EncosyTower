namespace EncosyTower.PageFlows.History
{
    /// <summary>One recorded navigation of a page history.</summary>
    /// <param name="FlowId">The identifier of the flow that made the navigation.</param>
    /// <param name="Kind">The kind of the navigation.</param>
    /// <param name="From">The page the navigation hid or removed, if any.</param>
    /// <param name="To">The page the navigation showed, if any.</param>
    /// <param name="Context">
    /// The context of the call, without its <see cref="PageContext.IssuedBy"/>, <see cref="PageContext.Report"/> and
    /// <see cref="PageContext.Abort"/>.
    /// </param>
    public readonly record struct PageHistoryEntry(
          string FlowId
        , PageNavigationKind Kind
        , PageRef From
        , PageRef To
        , PageContext Context
    )
    {
        /// <summary>Creates an entry from a completed navigation.</summary>
        /// <param name="navigation">The navigation to record.</param>
        /// <returns>The entry. Its context has no issuer, report or abort handle.</returns>
        public static PageHistoryEntry FromNavigation(in PageNavigation navigation)
            => new(navigation.FlowId, navigation.Kind, navigation.From, navigation.To
                , navigation.Context with { IssuedBy = null, Report = null, Abort = default });
    }
}
