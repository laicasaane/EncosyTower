using System.Threading;
using EncosyTower.Common;
using EncosyTower.PageFlows.History;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    /// <summary>
    /// Replays history entries on one flow type, backwards and forwards.
    /// </summary>
    /// <remarks>
    /// A <see cref="PageHistory"/> creates one navigator for each covered flow through
    /// <see cref="PageHistory.NavigatorFactory"/>. The built-in navigators send the replayed call to the flow with
    /// <see cref="PageHistoryReplay"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// public sealed class StackNavigator : IPageHistoryNavigator
    /// {
    ///     private readonly IPageFlowRecordable _flow;
    ///
    ///     public StackNavigator(IPageFlowRecordable flow) => _flow = flow;
    ///
    ///     public UnityTask&lt;Success&lt;PageFlowError&gt;&gt; BackAsync(
    ///         PageHistoryEntry entry, PageAsyncOperation operation, CancellationToken token)
    ///         => PageHistoryReplay.HideActiveAsync(_flow, entry.Context with { AsyncOperation = operation }, token);
    ///
    ///     // ForwardAsync shows entry.To.AssetKey with PageHistoryReplay.ShowAsync the same way.
    /// }
    /// </code>
    /// </example>
    public interface IPageHistoryNavigator
    {
        /// <summary>Undoes the navigation that an entry recorded.</summary>
        /// <param name="entry">The entry to undo.</param>
        /// <param name="operation">How the replayed call behaves when its flow is in transition.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow applied the replayed call; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        UnityTask<Success<PageFlowError>> BackAsync(
              PageHistoryEntry entry
            , PageAsyncOperation operation
            , CancellationToken token
        );

        /// <summary>Repeats the navigation that an entry recorded.</summary>
        /// <param name="entry">The entry to repeat.</param>
        /// <param name="operation">How the replayed call behaves when its flow is in transition.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the flow applied the replayed call; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        UnityTask<Success<PageFlowError>> ForwardAsync(
              PageHistoryEntry entry
            , PageAsyncOperation operation
            , CancellationToken token
        );
    }
}
