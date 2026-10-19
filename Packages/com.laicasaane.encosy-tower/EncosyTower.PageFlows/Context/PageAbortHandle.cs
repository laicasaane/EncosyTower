using System.Threading;
using EncosyTower.Common;
using EncosyTower.PageFlows.Internals;

namespace EncosyTower.PageFlows
{
    /// <summary>
    /// Lets its holder abort the one show or hide it was issued for.
    /// </summary>
    /// <remarks>
    /// The flow gives every holder its own handle, with that holder as its origin (the caller, a page or a
    /// transition), and every request made through it is reported with that origin. Passing the handle to other
    /// code, such as a dialog, lends that code your authority: its requests are reported as yours.
    /// Do this only on purpose.
    /// The handle works only until the operation publishes its
    /// <see cref="EncosyTower.PageFlows.PubSub.PageTransitionEndedMessage"/> messages;
    /// later requests return a <see cref="PageAbortError"/>.
    /// </remarks>
    public readonly struct PageAbortHandle : IIsValid
    {
        internal readonly PageAbortState _state;
        internal readonly PageAbortOrigin _origin;
        internal readonly bool _isCarrier;

        internal PageAbortHandle(PageAbortState state, PageAbortOrigin origin, bool isCarrier)
        {
            _state = state;
            _origin = origin;
            _isCarrier = isCarrier;
        }

        /// <summary>
        /// Gets whether the handle belongs to a call or to a <see cref="PageAbortSource"/>.
        /// The default handle does not.
        /// </summary>
        public bool IsValid => _state != null;

        /// <summary>Gets whether the call accepted an abort request.</summary>
        public bool IsRequested => _state is { IsRequested: true };

        /// <summary>Cancels when an abort is requested, so animations can stop.</summary>
        public CancellationToken Token => _state?.AbortToken ?? CancellationToken.None;

        /// <summary>Asks the call to abort.</summary>
        /// <param name="mode">How the screen goes back.</param>
        /// <returns>
        /// Success when the call accepted the request; otherwise a failure; see <see cref="PageAbortError"/>.
        /// </returns>
        public Success<PageAbortError> Request(PageAbortMode mode)
            => _state == null ? (PageAbortError)new PageAbortError.InvalidHandle() : _state.Request(_origin, mode);

        internal PageAbortHandle WithOrigin(PageAbortOrigin origin)
            => new(_state, origin, false);

        internal static PageAbortHandle CreateCarrier(PageAbortState state)
            => new(state, new PageAbortOrigin.Flow(), true);
    }
}
