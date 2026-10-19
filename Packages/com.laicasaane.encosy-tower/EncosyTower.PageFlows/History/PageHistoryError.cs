using System.Runtime.CompilerServices;
using EncosyTower.PolyEnumStructs;

namespace EncosyTower.PageFlows.History
{
    /// <summary>Describes why a navigation history call failed.</summary>
    /// <remarks>
    /// <para>The failure is one of these cases:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Case</term>
    /// <description>When it occurs</description>
    /// </listheader>
    /// <item>
    /// <term><c>Undefined</c></term>
    /// <description>The default value; no history call returns it.</description>
    /// </item>
    /// <item>
    /// <term><c>NothingToGoBack</c></term>
    /// <description>The history has no current entry to go back from.</description>
    /// </item>
    /// <item>
    /// <term><c>NothingToGoForward</c></term>
    /// <description>The history has no entry after the current one.</description>
    /// </item>
    /// <item>
    /// <term><c>FlowNotFound</c></term>
    /// <description>The flow of the entry is not covered, or it is destroyed or disposed.</description>
    /// </item>
    /// <item>
    /// <term><c>ReplayFailed</c></term>
    /// <description>The flow returned a failure for the replayed call. The case holds it.</description>
    /// </item>
    /// <item>
    /// <term><c>ReplayAborted</c></term>
    /// <description>The replayed call was aborted. The case holds the failure.</description>
    /// </item>
    /// <item>
    /// <term><c>NotAttached</c></term>
    /// <description>The history covers no flow.</description>
    /// </item>
    /// <item>
    /// <term><c>AlreadyAttached</c></term>
    /// <description>The history already covers flows. Detach it first.</description>
    /// </item>
    /// <item>
    /// <term><c>NoFlowToAttach</c></term>
    /// <description>The attach names no flow to cover.</description>
    /// </item>
    /// <item>
    /// <term><c>FlowAlreadyCovered</c></term>
    /// <description>Another navigation history already covers a flow.</description>
    /// </item>
    /// <item>
    /// <term><c>Busy</c></term>
    /// <description>The history is replaying an entry.</description>
    /// </item>
    /// </list>
    /// </remarks>
    [PolyEnumStruct]
    public readonly partial struct PageHistoryError
    {
        partial interface IEnumCase
        {
            string ToMessage();
        }

        /// <summary>The default value; no history call returns it.</summary>
        public readonly partial record struct Undefined()
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => "An unknown page history error occurred.";
        }

        /// <summary>The history has no current entry to go back from.</summary>
        public readonly partial record struct NothingToGoBack()
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => "The history has no entry to go back from.";
        }

        /// <summary>The history has no entry after the current one.</summary>
        public readonly partial record struct NothingToGoForward()
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => "The history has no entry to go forward to.";
        }

        /// <summary>The flow of the entry is not covered, or it is destroyed or disposed.</summary>
        /// <param name="FlowId">The identifier of the flow.</param>
        public readonly partial record struct FlowNotFound(string FlowId)
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => $"The flow '{FlowId}' is not attached to this history or no longer exists.";
        }

        /// <summary>The flow returned a failure for the replayed call.</summary>
        /// <param name="Error">The failure the flow returned.</param>
        public readonly partial record struct ReplayFailed(PageFlowError Error)
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => $"Replaying the history entry failed: {Error.ToMessage()}";
        }

        /// <summary>The replayed call was aborted.</summary>
        /// <param name="Error">The failure the flow returned.</param>
        public readonly partial record struct ReplayAborted(PageFlowError Error)
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => $"Replaying the history entry was aborted: {Error.ToMessage()}";
        }

        /// <summary>The history covers no flow.</summary>
        public readonly partial record struct NotAttached()
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => "The history is not attached to a codex or a flow.";
        }

        /// <summary>The history already covers flows.</summary>
        public readonly partial record struct AlreadyAttached()
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => "The history is already attached; detach it first.";
        }

        /// <summary>The attach names no flow to cover.</summary>
        public readonly partial record struct NoFlowToAttach()
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => "There is no page flow to attach the history to.";
        }

        /// <summary>Another navigation history already covers a flow.</summary>
        /// <param name="FlowId">The identifier of the flow.</param>
        public readonly partial record struct FlowAlreadyCovered(string FlowId)
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => $"The flow '{FlowId}' is already covered by another navigation history.";
        }

        /// <summary>The history is replaying an entry.</summary>
        public readonly partial record struct Busy()
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => "The history is already replaying an entry.";
        }
    }
}
