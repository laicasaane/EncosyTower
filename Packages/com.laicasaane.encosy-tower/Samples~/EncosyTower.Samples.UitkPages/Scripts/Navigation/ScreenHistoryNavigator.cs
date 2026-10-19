using System;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.History;
using EncosyTower.Tasks;

namespace EncosyTower.Samples.UitkPages.Navigation
{
    /// <summary>
    /// Replays the Screen flow history: Back shows the previous screen instead of hiding the current one.
    /// </summary>
    /// <remarks>
    /// Demonstrates a custom <see cref="IPageHistoryNavigator"/> that wraps a built-in
    /// <see cref="PageStackSingleNavigator"/> and uses <see cref="PageHistoryReplay"/>.
    /// </remarks>
    internal sealed class ScreenHistoryNavigator : IPageHistoryNavigator
    {
        private readonly IPageFlowRecordable _flow;
        private readonly PageHistory _history;
        private readonly PageStackSingleNavigator _stackNavigator;

        public ScreenHistoryNavigator(IPageFlowRecordable flow, PageHistory history)
        {
            _flow = flow;
            _history = history;
            _stackNavigator = new PageStackSingleNavigator(flow);
        }

        public UnityTask<Success<PageFlowError>> BackAsync(
              PageHistoryEntry entry
            , PageAsyncOperation operation
            , CancellationToken token
        )
        {
            var previousIndex = _history.CurrentIndex - 1;

            if (entry.Kind != PageNavigationKind.Show || (uint)previousIndex >= (uint)_history.Entries.Count)
            {
                return _stackNavigator.BackAsync(entry, operation, token);
            }

            var previous = _history.Entries[previousIndex];

            if (previous.Kind != PageNavigationKind.Show
                || previous.To.IsValid == false
                || string.Equals(previous.FlowId, entry.FlowId, StringComparison.Ordinal) == false
            )
            {
                return _stackNavigator.BackAsync(entry, operation, token);
            }

            var context = previous.Context with {
                AsyncOperation = operation,
                IssuedBy = entry.Context.IssuedBy,
            };

            return PageHistoryReplay.ShowAsync(_flow, previous.To.AssetKey, context, token);
        }

        public UnityTask<Success<PageFlowError>> ForwardAsync(
              PageHistoryEntry entry
            , PageAsyncOperation operation
            , CancellationToken token
        )
            => _stackNavigator.ForwardAsync(entry, operation, token);
    }
}
