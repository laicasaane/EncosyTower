using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.History
{
    /// <summary>Sends replayed calls to a flow through its page messages and reads their results.</summary>
    public static class PageHistoryReplay
    {
        /// <summary>Replays a show by key with <see cref="EncosyTower.PageFlows.PubSub.ShowPageMessage"/>.</summary>
        /// <param name="flow">The flow to replay on.</param>
        /// <param name="assetKey">The key of the page asset to show.</param>
        /// <param name="context">The context of the call. The method puts a new receipt in it.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow applied the call; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// The result comes from a <see cref="PageCallReceipt"/>. When no flow reports to it, the call fails with the
        /// <c>ReplayUnanswered</c> case, or the <c>Cancelled</c> case when the token is cancelled.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="flow"/> is <c>null</c>.</exception>
        public static async UnityTask<Success<PageFlowError>> ShowAsync(
              [NotNull] IPageFlowRecordable flow
            , AssetKey assetKey
            , PageContext context
            , CancellationToken token
        )
        {
            ThrowHelper.ThrowIfNull(flow);

            var receipt = new PageCallReceipt();
            var publisher = flow.Publisher.Scope(flow.FlowScope);
            var message = new ShowPageMessage(assetKey, context with { Receipt = receipt });

            var published = await ShowPageMessage.Async.Publish(in publisher, message, CreatePublishingContext(token));

            if (published.TryGetFailure(out var publishError) && publishError.IsSubscriberNotFound == false)
            {
                return PageFlowError.FromPublish(publishError);
            }

            return ReadResult(flow, receipt, token);
        }

        /// <summary>
        /// Replays a hide of the current page with <see cref="EncosyTower.PageFlows.PubSub.HideActivePageMessage"/>.
        /// </summary>
        /// <param name="flow">The flow to replay on.</param>
        /// <param name="context">The context of the call. The method puts a new receipt in it.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow applied the call; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>See <see cref="ShowAsync"/> for how the result is read.</remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="flow"/> is <c>null</c>.</exception>
        public static async UnityTask<Success<PageFlowError>> HideActiveAsync(
              [NotNull] IPageFlowRecordable flow
            , PageContext context
            , CancellationToken token
        )
        {
            ThrowHelper.ThrowIfNull(flow);

            var receipt = new PageCallReceipt();
            var publisher = flow.Publisher.Scope(flow.FlowScope);
            var message = new HideActivePageMessage(context with { Receipt = receipt });

            var published = await HideActivePageMessage.Async.Publish(
                  in publisher
                , message
                , CreatePublishingContext(token)
            );

            if (published.TryGetFailure(out var publishError) && publishError.IsSubscriberNotFound == false)
            {
                return PageFlowError.FromPublish(publishError);
            }

            return ReadResult(flow, receipt, token);
        }

        /// <summary>
        /// Replays a show by index with <see cref="EncosyTower.PageFlows.PubSub.ShowPageAtIndexMessage"/>.
        /// </summary>
        /// <param name="flow">The flow to replay on.</param>
        /// <param name="index">The index of the page to show.</param>
        /// <param name="context">The context of the call. The method puts a new receipt in it.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow applied the call; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>See <see cref="ShowAsync"/> for how the result is read.</remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="flow"/> is <c>null</c>.</exception>
        public static async UnityTask<Success<PageFlowError>> ShowAtAsync(
              [NotNull] IPageFlowRecordable flow
            , int index
            , PageContext context
            , CancellationToken token
        )
        {
            ThrowHelper.ThrowIfNull(flow);

            var receipt = new PageCallReceipt();
            var publisher = flow.Publisher.Scope(flow.FlowScope);
            var message = new ShowPageAtIndexMessage(index, context with { Receipt = receipt });

            var published = await ShowPageAtIndexMessage.Async.Publish(
                  in publisher
                , message
                , CreatePublishingContext(token)
            );

            if (published.TryGetFailure(out var publishError) && publishError.IsSubscriberNotFound == false)
            {
                return PageFlowError.FromPublish(publishError);
            }

            return ReadResult(flow, receipt, token);
        }

        /// <summary>
        /// Replays a hide by index with <see cref="EncosyTower.PageFlows.PubSub.HidePageAtIndexMessage"/>.
        /// </summary>
        /// <param name="flow">The flow to replay on.</param>
        /// <param name="index">The index of the page to hide.</param>
        /// <param name="context">The context of the call. The method puts a new receipt in it.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow applied the call; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>See <see cref="ShowAsync"/> for how the result is read.</remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="flow"/> is <c>null</c>.</exception>
        public static async UnityTask<Success<PageFlowError>> HideAtAsync(
              [NotNull] IPageFlowRecordable flow
            , int index
            , PageContext context
            , CancellationToken token
        )
        {
            ThrowHelper.ThrowIfNull(flow);

            var receipt = new PageCallReceipt();
            var publisher = flow.Publisher.Scope(flow.FlowScope);
            var message = new HidePageAtIndexMessage(index, context with { Receipt = receipt });

            var published = await HidePageAtIndexMessage.Async.Publish(
                  in publisher
                , message
                , CreatePublishingContext(token)
            );

            if (published.TryGetFailure(out var publishError) && publishError.IsSubscriberNotFound == false)
            {
                return PageFlowError.FromPublish(publishError);
            }

            return ReadResult(flow, receipt, token);
        }

        internal static UnityTask<Success<PageFlowError>> CannotReplayAsync(
              IPageHistoryNavigator navigator
            , PageHistoryEntry entry
        )
        {
            var error = PageFlowError.ReplayNotSupported(navigator.GetType().Name, entry.Kind, entry.FlowId);
            return UnityTask.FromResult<Success<PageFlowError>>(error);
        }

        internal static bool IsLearnOnly(in Success<PageFlowError> result)
            => result.TryGetFailure(out var error)
            && error.IsSubscriberNotFound
            && error.HasSuppressedFailure() == false
            && result.IsApplied();

        private static PublishingContext CreatePublishingContext(CancellationToken token)
            => PublishingContext.Default(token: token);

        private static Success<PageFlowError> ReadResult(
              IPageFlowRecordable flow
            , PageCallReceipt receipt
            , CancellationToken token
        )
        {
            if (receipt.IsReported)
            {
                return receipt.Result;
            }

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            return PageFlowError.ReplayUnanswered(flow.FlowId);
        }
    }
}
