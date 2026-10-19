using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.History
{
    /// <summary>Replays the history entries of a single page list.</summary>
    /// <remarks>
    /// It replays <c>ShowAt</c> and <c>HideAt</c> entries. Any other kind of entry fails with the
    /// <c>ReplayNotSupported</c> case of <see cref="PageFlowError"/>.
    /// </remarks>
    public sealed class PageListSingleNavigator : IPageHistoryNavigator
    {
        private readonly IPageFlowRecordable _flow;

        /// <summary>Creates a navigator for a single page list.</summary>
        /// <param name="flow">The flow to replay entries on.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="flow"/> is <c>null</c>.</exception>
        public PageListSingleNavigator([NotNull] IPageFlowRecordable flow)
        {
            ThrowHelper.ThrowIfNull(flow);
            _flow = flow;
        }

        /// <inheritdoc/>
        public UnityTask<Success<PageFlowError>> BackAsync(
              PageHistoryEntry entry
            , PageAsyncOperation operation
            , CancellationToken token
        )
        {
            var context = entry.Context with { AsyncOperation = operation };

            switch (entry.Kind)
            {
                case PageNavigationKind.ShowAt:
                {
                    if (entry.From.IsValid == false)
                    {
                        return PageHistoryReplay.HideActiveAsync(_flow, context, token);
                    }

                    if (entry.From.Index.TryGetValue(out var index) == false)
                    {
                        return PageHistoryReplay.CannotReplayAsync(this, entry);
                    }

                    return PageHistoryReplay.ShowAtAsync(_flow, index, context, token);
                }

                case PageNavigationKind.HideAt:
                {
                    if (entry.From.Index.TryGetValue(out var index) == false)
                    {
                        return PageHistoryReplay.CannotReplayAsync(this, entry);
                    }

                    return PageHistoryReplay.ShowAtAsync(_flow, index, context, token);
                }

                default:
                {
                    return PageHistoryReplay.CannotReplayAsync(this, entry);
                }
            }
        }

        /// <inheritdoc/>
        public UnityTask<Success<PageFlowError>> ForwardAsync(
              PageHistoryEntry entry
            , PageAsyncOperation operation
            , CancellationToken token
        )
        {
            var context = entry.Context with { AsyncOperation = operation };

            switch (entry.Kind)
            {
                case PageNavigationKind.ShowAt:
                {
                    if (entry.To.Index.TryGetValue(out var index) == false)
                    {
                        return PageHistoryReplay.CannotReplayAsync(this, entry);
                    }

                    return PageHistoryReplay.ShowAtAsync(_flow, index, context, token);
                }

                case PageNavigationKind.HideAt:
                {
                    return PageHistoryReplay.HideActiveAsync(_flow, context, token);
                }

                default:
                {
                    return PageHistoryReplay.CannotReplayAsync(this, entry);
                }
            }
        }
    }
}
