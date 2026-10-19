using EncosyTower.PolyEnumStructs;

namespace EncosyTower.PageFlows
{
    /// <summary>Why an abort request had no effect.</summary>
    /// <remarks>
    /// <para>The failure is one of these cases:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Case</term>
    /// <description>When it occurs</description>
    /// </listheader>
    /// <item>
    /// <term><c>FlowIsIdle</c></term>
    /// <description>The flow runs no call and has no call waiting.</description>
    /// </item>
    /// <item>
    /// <term><c>TransitionAlreadyEnded</c></term>
    /// <description>
    /// The call already ran its transition to the end; it no longer accepts a request.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>OperationNotAbortable</c></term>
    /// <description>The call, such as an add or a remove, cannot be aborted.</description>
    /// </item>
    /// <item>
    /// <term><c>RecoveringFromFailure</c></term>
    /// <description>The call failed and is undoing its changes. The case holds that failure.</description>
    /// </item>
    /// <item>
    /// <term><c>InvalidHandle</c></term>
    /// <description>
    /// The handle is not bound to a call: it is the default handle, or its source serves no call yet.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>OperationDropped</c></term>
    /// <description>The call ended without running, because the flow was in transition.</description>
    /// </item>
    /// <item>
    /// <term><c>OperationCompleted</c></term>
    /// <description>The call already ended successfully.</description>
    /// </item>
    /// <item>
    /// <term><c>OperationFailed</c></term>
    /// <description>The call already ended with a failure. The case holds that failure.</description>
    /// </item>
    /// <item>
    /// <term><c>OperationAborted</c></term>
    /// <description>The call already ended aborted. The case holds who asked and the mode.</description>
    /// </item>
    /// <item>
    /// <term><c>AbortAlreadyRequested</c></term>
    /// <description>The call already accepted an earlier request. The case holds who asked and the mode.</description>
    /// </item>
    /// </list>
    /// </remarks>
    [PolyEnumStruct]
    public readonly partial struct PageAbortError
    {
        /// <summary>The flow runs no call and has no call waiting.</summary>
        public readonly partial struct FlowIsIdle { }

        /// <summary>The call already ran its transition to the end; it no longer accepts a request.</summary>
        public readonly partial struct TransitionAlreadyEnded { }

        /// <summary>The call, such as an add or a remove, cannot be aborted.</summary>
        public readonly partial struct OperationNotAbortable { }

        /// <summary>The call failed and is undoing its changes.</summary>
        /// <param name="Failure">The failure the call is recovering from.</param>
        public readonly partial record struct RecoveringFromFailure(PageFlowError Failure);

        /// <summary>
        /// The handle is not bound to a call: it is the default handle, or its source serves no call yet.
        /// </summary>
        public readonly partial struct InvalidHandle { }

        /// <summary>The call ended without running, because the flow was in transition.</summary>
        public readonly partial struct OperationDropped { }

        /// <summary>The call already ended successfully.</summary>
        public readonly partial struct OperationCompleted { }

        /// <summary>The call already ended with a failure.</summary>
        /// <param name="Failure">The failure the call ended with.</param>
        public readonly partial record struct OperationFailed(PageFlowError Failure);

        /// <summary>The call already ended aborted.</summary>
        /// <param name="Origin">Who asked for the abort.</param>
        /// <param name="Mode">The mode of the abort.</param>
        public readonly partial record struct OperationAborted(PageAbortOrigin Origin, PageAbortMode Mode);

        /// <summary>The call already accepted an earlier abort request.</summary>
        /// <param name="Origin">Who made the accepted request.</param>
        /// <param name="Mode">The mode of the accepted request.</param>
        public readonly partial record struct AbortAlreadyRequested(PageAbortOrigin Origin, PageAbortMode Mode);
    }
}
