using System;
using EncosyTower.PageFlows.History;

namespace EncosyTower.PageFlows
{
    /// <summary>Decides what a completed navigation does to a history.</summary>
    public interface IPageHistoryRecordingStrategy
    {
        /// <summary>Called for each navigation a covered flow completed.</summary>
        /// <param name="navigation">The navigation that completed.</param>
        /// <param name="entries">The entries the history holds.</param>
        /// <param name="currentIndex">The index of the current entry, or <c>-1</c> when there is none.</param>
        /// <returns>What the history does with the navigation.</returns>
        PageHistoryChange OnNavigated(
              in PageNavigation navigation
            , ReadOnlySpan<PageHistoryEntry> entries
            , int currentIndex
        );
    }
}
