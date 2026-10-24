using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
    /// <summary>The settings of one page flow call, passed to the flow and on to the page hooks.</summary>
    /// <remarks>
    /// Create a new context for each call: a <see cref="PageAbortSource"/> and a <see cref="PageCallReport"/>
    /// serve one call.
    /// </remarks>
    public readonly record struct PageContext
    {
        /// <summary>
        /// What the call does when its flow is in transition. The default is <see cref="PageAsyncOperation.Drop"/>.
        /// </summary>
        public PageAsyncOperation AsyncOperation { get; init; }

        /// <summary>Whether the flow pools or destroys the page it removes. The default is pooling.</summary>
        public PageReturnOperation ReturnOperation { get; init; }

        /// <summary>The transition options of the page that shows.</summary>
        /// <remarks>
        /// A page whose <see cref="PageOptions"/> use <see cref="PageOptions.Options.forceUse"/> replaces them.
        /// </remarks>
        public PageTransitionOptions ShowOptions { get; init; }

        /// <summary>The transition options of the page that hides.</summary>
        /// <remarks>
        /// A page whose <see cref="PageOptions"/> use <see cref="PageOptions.Options.forceUse"/> replaces them.
        /// </remarks>
        public PageTransitionOptions HideOptions { get; init; }

        /// <summary>The index of the default page of a single-page list.</summary>
        /// <remarks>
        /// <para>
        /// A single-page list cannot hide the page at this index. After it hides another page, it shows the page
        /// at this index when it exists. Use a negative index for a list without a default page. Other flows
        /// ignore it.
        /// </para>
        /// <para>The default is 0.</para>
        /// </remarks>
        public int DefaultIndex { get; init; }

        /// <summary>Whether the hide of the transition runs after the show, instead of at the same time.</summary>
        public bool SequentialTransition { get; init; }

        /// <summary>Data that the caller passes to the page hooks. The flow does not read it.</summary>
        public Option<object> UserData { get; init; }

        /// <summary>
        /// The handle that can abort this show or hide. Set it from a <see cref="PageAbortSource"/>.
        /// The flow gives every holder its own handle, with that holder as its origin.
        /// </summary>
        public PageAbortHandle Abort { get; init; }

        /// <summary>
        /// Who issued this call. Navigation histories set it on the calls they replay; the wrappers copy it into
        /// PageNavigatedMessage.
        /// </summary>
        public object IssuedBy { get; init; }

        /// <summary>When set, the wrapper that handles this call reports its result here.</summary>
        public PageCallReport Report { get; init; }
    }
}
