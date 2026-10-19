using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.History
{
    /// <summary>Replays the history entries of a single page stack.</summary>
    /// <remarks>
    /// It replays <c>Show</c> and <c>Hide</c> entries. Any other kind of entry fails with the <c>ReplayNotSupported</c>
    /// case of <see cref="PageFlowError"/>.
    /// </remarks>
    public sealed class PageStackSingleNavigator : IPageHistoryNavigator
    {
        private readonly IPageFlowRecordable _flow;

        /// <summary>Creates a navigator for a single page stack.</summary>
        /// <param name="flow">The flow to replay entries on.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="flow"/> is <c>null</c>.</exception>
        public PageStackSingleNavigator([NotNull] IPageFlowRecordable flow)
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
                case PageNavigationKind.Show:
                {
                    return entry.From.IsValid == false
                        ? PageHistoryReplay.HideActiveAsync(_flow, context, token)
                        : PageHistoryReplay.ShowAsync(_flow, entry.From.AssetKey, context, token);
                }

                case PageNavigationKind.Hide:
                {
                    return PageHistoryReplay.ShowAsync(_flow, entry.From.AssetKey, context, token);
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
                case PageNavigationKind.Show:
                {
                    return PageHistoryReplay.ShowAsync(_flow, entry.To.AssetKey, context, token);
                }

                case PageNavigationKind.Hide:
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
