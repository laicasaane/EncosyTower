using System;
using EncosyTower.Common;

namespace EncosyTower.PageFlows.History
{
    /// <summary>Records a back stack: a show, push or select adds an entry; a matching hide or pop goes back.</summary>
    /// <remarks>
    /// A hide or pop that does not leave the page of the current entry changes nothing. This is the strategy
    /// of <see cref="PageHistory"/> by default.
    /// </remarks>
    public sealed class PageBackStackHistoryStrategy : IPageHistoryRecordingStrategy
    {
        /// <summary>Gets whether a navigation leaves the page that an entry went to.</summary>
        /// <param name="entry">The recorded entry.</param>
        /// <param name="navigation">The navigation that completed.</param>
        /// <returns>
        /// <c>true</c> when the entry and the navigation have the same flow, and the entry goes to the page that the
        /// navigation leaves.
        /// </returns>
        public static bool Matches(in PageHistoryEntry entry, in PageNavigation navigation)
            => string.Equals(entry.FlowId, navigation.FlowId, StringComparison.Ordinal)
            && entry.To.AssetKey.Equals(navigation.From.AssetKey)
            && Option.Equals(entry.To.Index, navigation.From.Index)
            ;

        /// <inheritdoc/>
        public PageHistoryChange OnNavigated(
              in PageNavigation navigation
            , ReadOnlySpan<PageHistoryEntry> entries
            , int currentIndex
        )
        {
            switch (navigation.Kind)
            {
                case PageNavigationKind.Show:
                case PageNavigationKind.Push:
                case PageNavigationKind.ShowAt:
                {
                    return new PageHistoryChange.Navigate(PageHistoryEntry.FromNavigation(navigation));
                }

                case PageNavigationKind.Hide:
                case PageNavigationKind.Pop:
                case PageNavigationKind.HideAt:
                {
                    if ((uint)currentIndex < (uint)entries.Length && Matches(entries[currentIndex], navigation))
                    {
                        return new PageHistoryChange.GoBack(Steps: 1);
                    }

                    return new PageHistoryChange.None();
                }

                default:
                {
                    return new PageHistoryChange.None();
                }
            }
        }
    }
}
