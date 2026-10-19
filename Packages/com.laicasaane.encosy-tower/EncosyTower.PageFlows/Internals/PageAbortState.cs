using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Flows;

using HideInCallstack = UnityEngine.HideInCallstackAttribute;

namespace EncosyTower.PageFlows.Internals
{
    /// <summary>The steps a call goes through while it can be aborted.</summary>
    /// <remarks>
    /// A call moves from <c>Unbound</c> (no flow serves it yet) to <c>Waiting</c> (bound, queued behind the running
    /// call), <c>Running</c>, and then <c>Ended</c>. <c>Recovering</c> means the call failed and undoes its changes;
    /// <c>WindowClosed</c> means the transition ran to its end. Both refuse abort requests.
    /// </remarks>
    internal enum PageAbortStage : byte
    {
        Unbound,
        Waiting,
        Running,
        Recovering,
        WindowClosed,
        Ended,
    }

    internal enum PageAbortOutcome : byte
    {
        None,
        Completed,
        Failed,
        Aborted,
        Dropped,
    }

    internal enum PageAbortCallKind : byte
    {
        Abortable,
        NotAbortable,
    }

    /// <summary>Tracks one call of a flow for aborting: its stage, its abort request and its outcome.</summary>
    /// <remarks>
    /// <para>
    /// The flow creates one for each call, or binds the one a <see cref="PageAbortSource"/> owns. A state binds to
    /// one call only.
    /// </para>
    /// <para>
    /// <see cref="Request"/> checks in this order: the state is unbound, the call ended, an abort was requested
    /// already, the call is not abortable, the call is recovering, the abort window is closed. The first match
    /// decides the <see cref="PageAbortError"/>; no match accepts the request.
    /// </para>
    /// </remarks>
    internal sealed class PageAbortState
    {
        // Never disposed: no timer and no wait handle is read, so the sources own no unmanaged resource.
        private readonly CancellationTokenSource _abortCts = new();

        private CancellationTokenSource _callbackCts;
        private CancellationTokenRegistration _callerRegistration;
        private CancellationToken _callbackToken;
        private List<PageFlowError> _suppressedErrors;
        private List<PageFlowError> _softErrors;
        private List<PageFlowError> _setAsideErrors;
        private bool _hasStartedCall;
        private ILogger _logger;

        internal PageAbortStage Stage { get; private set; }

        internal PageAbortRegistry Registry { get; private set; }

        internal bool IsAbortable { get; private set; }

        internal bool IsRequested { get; private set; }

        internal PageAbortOrigin RequestOrigin { get; private set; }

        internal PageAbortMode RequestMode { get; private set; }

        internal PageFlowError Failure { get; private set; }

        internal PageAbortOutcome Outcome { get; private set; }

        internal CancellationToken AbortToken => _abortCts.Token;

        internal IReadOnlyList<PageFlowError> SuppressedErrors => _suppressedErrors;

        // Errors set aside while a cancellation won (B4, D85). The call throws, so none of them is in a result.
        internal IReadOnlyList<PageFlowError> SetAsideErrors => _setAsideErrors;

        // True when the wrapper that bound this state reports the set-aside errors; the state then never logs them.
        internal bool HasSetAsideReader { get; private set; }

        // After End, a set-aside without a reader is logged by the flow directly.
        internal bool IsBound => Stage is not (PageAbortStage.Unbound or PageAbortStage.Ended);

        internal void AddSuppressedError(in PageFlowError error)
        {
            _suppressedErrors ??= new List<PageFlowError>();
            _suppressedErrors.Add(error);
        }

        internal void AddSoftError(in PageFlowError error)
        {
            _softErrors ??= new List<PageFlowError>();
            _softErrors.Add(error);
        }

        internal void MarkSetAsideReader()
            => HasSetAsideReader = true;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void SetAside(in PageFlowError error)
        {
            _setAsideErrors ??= new List<PageFlowError>();
            _setAsideErrors.Add(error);
        }

        // Moves the undo errors into the set-aside errors when a cancellation wins over an abort's undo.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void SetAsideSuppressedErrors()
        {
            var suppressed = _suppressedErrors;

            if (suppressed == null)
            {
                return;
            }

            _suppressedErrors = null;

            if (_setAsideErrors == null)
            {
                _setAsideErrors = suppressed;
                return;
            }

            _setAsideErrors.AddRange(suppressed);
        }

        internal Success<PageFlowError> MergeSoftErrors(in Success<PageFlowError> result)
        {
            var soft = _softErrors;

            if (soft == null)
            {
                return result;
            }

            _softErrors = null;
            return PageFlow.MergeSoft(result, soft);
        }

        internal CancellationToken GetCallbackToken(CancellationToken callerToken)
            => _hasStartedCall ? _callbackToken : callerToken;

        internal bool TryBind(PageAbortRegistry registry, PageAbortCallKind kind, ILogger logger)
        {
            if (Stage != PageAbortStage.Unbound)
            {
                return false;
            }

            Registry = registry;
            IsAbortable = kind == PageAbortCallKind.Abortable;
            _logger = logger;
            Stage = PageAbortStage.Waiting;
            return true;
        }

        internal void Enter(CancellationToken callerToken)
        {
            if (Stage != PageAbortStage.Waiting)
            {
                return;
            }

            Stage = PageAbortStage.Running;
            _hasStartedCall = true;

            if (IsAbortable == false)
            {
                _callbackToken = callerToken;
                return;
            }

            if (callerToken.CanBeCanceled == false)
            {
                _callbackToken = _abortCts.Token;
                return;
            }

            _callbackCts = new();

            _callerRegistration = callerToken.Register(
                  static state => ((CancellationTokenSource)state).Cancel()
                , _callbackCts
            );

            _callbackToken = _callbackCts.Token;
        }

        internal void BeginRecovering(in PageFlowError failure)
        {
            if (IsRequested || Stage is not (PageAbortStage.Waiting or PageAbortStage.Running))
            {
                return;
            }

            Stage = PageAbortStage.Recovering;
            Failure = failure;
        }

        internal void CloseWindow()
        {
            if (Stage is PageAbortStage.Running or PageAbortStage.Recovering)
            {
                Stage = PageAbortStage.WindowClosed;
            }
        }

        internal void MarkDropped()
        {
            Outcome = PageAbortOutcome.Dropped;
        }

        internal void End(in Success<PageFlowError> result)
        {
            if (Stage == PageAbortStage.Ended)
            {
                return;
            }

            Stage = PageAbortStage.Ended;
            _callerRegistration.Dispose();
            LogSetAsideErrorsWithoutReader();

            if (Outcome != PageAbortOutcome.None)
            {
                return;
            }

            if (result.TryGetFailure(out var failure) == false || failure.IsSubscriberNotFound)
            {
                Outcome = PageAbortOutcome.Completed;
                return;
            }

            if (failure.IsAborted)
            {
                Outcome = PageAbortOutcome.Aborted;
                return;
            }

            Outcome = PageAbortOutcome.Failed;
            Failure = failure;
        }

        internal Success<PageAbortError> Request(PageAbortOrigin origin, PageAbortMode mode)
        {
            if (Stage == PageAbortStage.Unbound)
            {
                return (PageAbortError)new PageAbortError.InvalidHandle();
            }

            if (Stage == PageAbortStage.Ended)
            {
                return GetEndedError();
            }

            if (IsRequested)
            {
                return (PageAbortError)new PageAbortError.AbortAlreadyRequested(RequestOrigin, RequestMode);
            }

            if (IsAbortable == false)
            {
                return (PageAbortError)new PageAbortError.OperationNotAbortable();
            }

            if (Stage == PageAbortStage.Recovering)
            {
                return (PageAbortError)new PageAbortError.RecoveringFromFailure(Failure);
            }

            if (Stage == PageAbortStage.WindowClosed)
            {
                return (PageAbortError)new PageAbortError.TransitionAlreadyEnded();
            }

            IsRequested = true;
            RequestOrigin = origin;
            RequestMode = mode;

            Cancel(_abortCts);

            if (_callbackCts != null)
            {
                Cancel(_callbackCts);
            }

            return Success.Yes;
        }

        // D28: a call without a reader (a direct core call) has no channel for these errors, so they are logged once.
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.NoInlining)]
        private void LogSetAsideErrorsWithoutReader()
        {
            var errors = _setAsideErrors;

            if (errors == null || HasSetAsideReader)
            {
                return;
            }

            _setAsideErrors = null;

            var count = errors.Count;

            for (var i = 0; i < count; i++)
            {
                errors[i].Log(_logger);
            }
        }

        private PageAbortError GetEndedError()
            => Outcome switch {
                PageAbortOutcome.Completed => new PageAbortError.OperationCompleted(),
                PageAbortOutcome.Failed => new PageAbortError.OperationFailed(Failure),
                PageAbortOutcome.Aborted => new PageAbortError.OperationAborted(RequestOrigin, RequestMode),
                _ => new PageAbortError.OperationDropped(),
            };

        private void Cancel(CancellationTokenSource source)
        {
            try
            {
                source.Cancel();
            }
            catch (AggregateException ex)
            {
                _logger.LogException(ex);
            }
        }
    }
}
