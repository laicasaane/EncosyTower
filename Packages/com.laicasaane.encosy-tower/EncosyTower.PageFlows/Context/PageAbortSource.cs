using EncosyTower.Common;
using EncosyTower.PageFlows.Internals;

namespace EncosyTower.PageFlows
{
    /// <summary>Owned by the calling code, like a CancellationTokenSource. One source serves one call.</summary>
    /// <remarks>
    /// A source that is already bound to a call cannot serve a second one: the second call runs without it, and
    /// the flow logs an error.
    /// </remarks>
    public sealed class PageAbortSource
    {
        private readonly PageAbortState _state = new();

        /// <summary>Creates a source whose requests have the caller as their origin.</summary>
        public PageAbortSource()
            => Handle = new PageAbortHandle(_state, new PageAbortOrigin.Caller(), false);

        /// <summary>Put this in <see cref="PageContext.Abort"/> of the one call this source serves.</summary>
        public PageAbortHandle Handle { get; }

        /// <summary>Gets whether the call accepted an abort request.</summary>
        public bool IsAbortRequested => _state.IsRequested;

        /// <summary>Asks the call to abort.</summary>
        /// <param name="mode">How the screen goes back.</param>
        /// <returns>
        /// Success when the call accepted the request; otherwise a failure; see <see cref="PageAbortError"/>.
        /// </returns>
        public Success<PageAbortError> Abort(PageAbortMode mode)
            => Handle.Request(mode);
    }
}
