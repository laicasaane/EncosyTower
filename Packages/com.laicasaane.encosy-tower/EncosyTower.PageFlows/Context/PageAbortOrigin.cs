using EncosyTower.PolyEnumStructs;

namespace EncosyTower.PageFlows
{
    /// <summary>
    /// Who asked for an abort. Created by the flow; read it from the result or from
    /// <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/>.
    /// </summary>
    /// <remarks>
    /// <para>The origin is one of these cases:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Case</term>
    /// <description>Who asked</description>
    /// </listheader>
    /// <item>
    /// <term><c>Caller</c></term>
    /// <description>
    /// The code that owns the <see cref="PageAbortSource"/> put in <see cref="PageContext.Abort"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>Flow</c></term>
    /// <description>
    /// The flow itself, through <see cref="IPageFlow.AbortCurrent(PageAbortMode)"/> or an abort message.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>Page</c></term>
    /// <description>A page, through the handle that the flow gave to one of its hooks.</description>
    /// </item>
    /// <item>
    /// <term><c>Transition</c></term>
    /// <description>A transition, through the handle that the flow gave to one of its steps.</description>
    /// </item>
    /// </list>
    /// </remarks>
    [PolyEnumStruct]
    public readonly partial struct PageAbortOrigin
    {
        /// <summary>
        /// The code that owns the <see cref="PageAbortSource"/> put in <see cref="PageContext.Abort"/>.
        /// </summary>
        public readonly partial struct Caller { }

        /// <summary>
        /// The flow itself, through <see cref="IPageFlow.AbortCurrent(PageAbortMode)"/> or an abort message.
        /// </summary>
        public readonly partial struct Flow { }

        /// <summary>A page, through the handle that the flow gave to one of its hooks.</summary>
        /// <param name="Source">The page.</param>
        public readonly partial record struct Page(IPage Source);

        /// <summary>A transition, through the handle that the flow gave to one of its steps.</summary>
        /// <param name="Source">The transition.</param>
        public readonly partial record struct Transition(IPageTransition Source);
    }
}
