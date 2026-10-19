using EncosyTower.PolyEnumStructs;

namespace EncosyTower.PageFlows.History
{
    /// <summary>
    /// What one completed navigation does to a history. Returned by
    /// <see cref="IPageHistoryRecordingStrategy.OnNavigated"/>; the history applies it, then trims to its
    /// <see cref="PageHistory.MaxEntries"/>.
    /// </summary>
    [PolyEnumStruct]
    public readonly partial struct PageHistoryChange
    {
        /// <summary>
        /// Ignore this navigation (a hide that matches no entry, a call the strategy filters out).
        /// </summary>
        public readonly partial record struct None();

        /// <summary>
        /// A new navigation: drop the entries after the current index (Forward is cleared), add <c>Entry</c> after it,
        /// and make it current. <see cref="PageBackStackHistoryStrategy"/> returns this for show, push and select.
        /// </summary>
        public readonly partial record struct Navigate(PageHistoryEntry Entry);

        /// <summary>
        /// Record without navigating: add <c>Entry</c> at the end and keep every existing entry.
        /// <see cref="PageCallLogHistoryStrategy"/> returns this for every call, hides included.
        /// </summary>
        public readonly partial record struct Append(PageHistoryEntry Entry);

        /// <summary>
        /// The navigation went back by <c>Steps</c> entries (for example, the player closed the popup recorded at
        /// the current index). Moves the current index back; the entries stay for Forward.
        /// <see cref="PageBackStackHistoryStrategy"/> returns this for a matching hide or pop.
        /// </summary>
        public readonly partial record struct GoBack(int Steps);
    }
}
