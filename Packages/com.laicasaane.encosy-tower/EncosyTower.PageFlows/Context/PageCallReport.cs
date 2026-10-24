using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
    /// <summary>
    /// Receives the result of one page flow call issued through a message. One report serves one call.
    /// </summary>
    /// <remarks>
    /// Put the report in <see cref="PageContext.Report"/>. The flow wrapper that handles the call reports its
    /// result, whether it was issued through a message or by a direct call on the wrapper. A call that no wrapper
    /// handles leaves the report unreported.
    /// <para>
    /// When several flows of one scope handle the same call, only the first flow's result is kept. The later flows'
    /// failures still reach the <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/> and
    /// <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/> listeners, whose <c>Context.Report</c> is this
    /// report.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var report = new PageCallReport();
    /// var context = new PageContext { Report = report };
    ///
    /// await HideAllPagesMessage.Async.Publish(in publisher, new HideAllPagesMessage(context), publishing);
    ///
    /// if (report.IsReported &amp;&amp; report.Result.TryGetFailure(out var error)) error.Log(logger);
    /// </code>
    /// </example>
    public sealed class PageCallReport
    {
        /// <summary>Gets whether a flow reported its result.</summary>
        public bool IsReported { get; private set; }

        /// <summary>Gets the identifier of the flow that reported.</summary>
        public string FlowId { get; private set; }

        /// <summary>Gets the result of the call. It is meaningful only when <see cref="IsReported"/> is true.</summary>
        public Success<PageFlowError> Result { get; private set; }

        /// <summary>Reports the result of the call. Only the first report counts.</summary>
        /// <param name="flowId">The identifier of the flow that handled the call.</param>
        /// <param name="result">The result of the call.</param>
        public void Report(string flowId, in Success<PageFlowError> result)
        {
            // The first report wins.
            if (IsReported)
            {
                return;
            }

            IsReported = true;
            FlowId = flowId;
            Result = result;
        }
    }
}
