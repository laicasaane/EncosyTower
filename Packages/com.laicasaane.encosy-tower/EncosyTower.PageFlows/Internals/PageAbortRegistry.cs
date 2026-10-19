using System.Collections.Generic;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;

namespace EncosyTower.PageFlows.Internals
{
    /// <summary>Binds the abort state of each call of one flow, and routes aborts to the running call.</summary>
    /// <remarks>
    /// <see cref="AbortCurrent"/> aborts the running call, or the first waiting call when none runs.
    /// </remarks>
    internal sealed class PageAbortRegistry
    {
        internal const string REUSED_SOURCE_MESSAGE
            = "The PageAbortSource in PageContext.Abort is already bound to another call; this call runs without it.";

        private readonly List<PageAbortState> _waiting = new();

        private PageAbortState _current;

        /// <summary>Binds the abort state of a call and puts a flow-owned handle in the context.</summary>
        /// <remarks>
        /// A handle that a page or a transition received is not the caller's own: the call runs as if it had none.
        /// A <see cref="PageAbortSource"/> that serves another call is not reused: the call runs without it, and the
        /// flow logs an error.
        /// </remarks>
        internal PageAbortState BeginCall(ref PageContext context, PageAbortCallKind kind, ILogger logger)
        {
            var handle = context.Abort;
            var state = handle._state;

            if (handle._isCarrier && ReferenceEquals(state.Registry, this) && state.Stage == PageAbortStage.Waiting)
            {
                return state;
            }

            if (state != null && handle._origin.GetEnumCase() != PageAbortOrigin.EnumCase.Caller)
            {
                // A handle the flow gave to a page or a transition, not the caller's own:
                // this call runs as if it had none.
                state = null;
            }

            if (state != null && state.TryBind(this, kind, logger) == false)
            {
                logger.LogError(REUSED_SOURCE_MESSAGE);
                state = null;
            }

            if (state == null)
            {
                state = new PageAbortState();
                state.TryBind(this, kind, logger);
            }

            _waiting.Add(state);
            context = context with { Abort = PageAbortHandle.CreateCarrier(state) };
            return state;
        }

        internal void EnterCall(PageAbortState state, CancellationToken callerToken)
        {
            _waiting.Remove(state);
            _current = state;
            state.Enter(callerToken);
        }

        internal void EndCall(PageAbortState state, in Success<PageFlowError> result)
        {
            state.End(result);
            _waiting.Remove(state);

            if (ReferenceEquals(_current, state))
            {
                _current = null;
            }
        }

        internal Success<PageAbortError> AbortCurrent(PageAbortMode mode)
        {
            if (_current != null)
            {
                return _current.Request(new PageAbortOrigin.Flow(), mode);
            }

            if (_waiting.Count > 0)
            {
                return _waiting[0].Request(new PageAbortOrigin.Flow(), mode);
            }

            return (PageAbortError)new PageAbortError.FlowIsIdle();
        }
    }
}
