namespace EncosyTower.PageFlows
{
    /// <summary>What a call does when its flow is already in transition.</summary>
    public enum PageAsyncOperation
    {
        /// <summary>The call ends at once with the <c>InTransition</c> case of <see cref="PageFlowError"/>.</summary>
        Drop,

        /// <summary>The call throws an <see cref="System.InvalidOperationException"/>.</summary>
        Exception,

        /// <summary>
        /// The call waits until the flow leaves its transition, then runs.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The wait ends early when the call is cancelled, with the <c>Cancelled</c> case of
        /// <see cref="PageFlowError"/>, or when an abort is requested for the waiting call, with the
        /// <c>Withdrawn</c> case. After 5 seconds the flow logs a warning and keeps waiting.
        /// </para>
        /// <para>
        /// Code that runs inside a page flow operation must not await another operation on the same flow:
        /// a <see cref="PageAsyncOperation.Sequential"/> call then waits forever.
        /// </para>
        /// </remarks>
        Sequential,
    }
}
