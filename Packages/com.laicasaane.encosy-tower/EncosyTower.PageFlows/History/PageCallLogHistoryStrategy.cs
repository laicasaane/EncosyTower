using System;

namespace EncosyTower.PageFlows.History
{
    /// <summary>Records every navigation as a new entry, hides included.</summary>
    /// <remarks>This is the strategy of <see cref="PageHistoryLog"/> by default.</remarks>
    public sealed class PageCallLogHistoryStrategy : IPageHistoryRecordingStrategy
    {
        /// <inheritdoc/>
        public PageHistoryChange OnNavigated(
              in PageNavigation navigation
            , ReadOnlySpan<PageHistoryEntry> entries
            , int currentIndex
        )
            => new PageHistoryChange.Append(PageHistoryEntry.FromNavigation(navigation));
    }
}
