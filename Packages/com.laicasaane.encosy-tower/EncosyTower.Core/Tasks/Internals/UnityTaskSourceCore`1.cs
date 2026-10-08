using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// The completion state shared by every EncosyTower task source on both backends: completion sources, async method
    /// runners, delay promises and relays.
    /// </summary>
    /// <typeparam name="T">The type of the result; <see cref="object"/> for sources without a result.</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Completion:</b> <c>_completed</c> is incremented with <see cref="Interlocked"/>; only the call that moves it
    /// from 0 to 1 stores the outcome, so the first completion wins and later ones return <c>false</c>.
    /// </para>
    /// <para>
    /// <b>Continuation slot:</b> <c>_continuation</c> is <c>null</c> while nothing is registered and the operation is
    /// pending, <c>s_completedSentinel</c> when the operation completed before a continuation was registered, or the
    /// registered continuation. <see cref="OnCompleted"/> and <see cref="SignalCompletion"/> race on this slot with
    /// <c>CompareExchange</c>, so exactly one of them runs the continuation, and a second registration throws.
    /// </para>
    /// <para>
    /// <b>Thread:</b> <c>_affinity</c> is the creator's thread kind. Continuations always run through
    /// <see cref="UnityTaskThreadContext.Run"/>, which runs them inline on a matching thread kind and posts them
    /// otherwise.
    /// </para>
    /// <para>
    /// <b>Reuse:</b> <see cref="Reset"/> increments <c>_version</c>; tasks carry the version as their token and
    /// <see cref="ValidateToken"/> rejects a stale one. This is a mutable struct: owners keep it in a field and call it
    /// through that field, never through a copy.
    /// </para>
    /// </remarks>
    internal struct UnityTaskSourceCore<T>
    {
        private static readonly Action<object> s_completedSentinel = static _ => { };

        private T _result;
        private ExceptionDispatchInfo _error;
        private bool _errorIsCancellation;
        private int _completed;
        private short _version;
        private UnityTaskThreadAffinity _affinity;
        private Action<object> _continuation;
        private object _state;

        public readonly short Version => _version;

        /// <summary>
        /// Sets the thread kind on which continuations resume. <see cref="UnityTaskThreadAffinity.None"/> runs them
        /// inline on the completing thread.
        /// </summary>
        public void Prepare(UnityTaskThreadAffinity affinity)
        {
            _affinity = affinity;
        }

        /// <summary>
        /// Completes successfully when no other completion happened first; returns whether this call completed it.
        /// </summary>
        public bool TrySetResult(T result)
        {
            if (Interlocked.Increment(ref _completed) != 1)
            {
                return false;
            }

            _result = result;
            SignalCompletion();
            return true;
        }

        /// <summary>
        /// Completes with <paramref name="exception"/>, captured with its stack so it can be rethrown unchanged. An
        /// <see cref="OperationCanceledException"/> marks the status as canceled.
        /// </summary>
        public bool TrySetException(Exception exception)
        {
            if (Interlocked.Increment(ref _completed) != 1)
            {
                return false;
            }

            _error = ExceptionDispatchInfo.Capture(exception);
            _errorIsCancellation = exception is OperationCanceledException;
            SignalCompletion();
            return true;
        }

        /// <summary>
        /// Returns the state after validating <paramref name="token"/>; reports <see cref="UnityTaskStatus.Pending"/>
        /// on another thread kind so the awaiter suspends and is resumed on the creator's thread kind.
        /// </summary>
        public UnityTaskStatus GetStatus(short token)
        {
            ValidateToken(token);

            if (UnityTaskThreadContext.Matches(_affinity) == false)
            {
                return UnityTaskStatus.Pending;
            }

            return UnsafeGetStatus();
        }

        /// <summary>
        /// Returns the state without token or thread checks. The state is pending until the outcome is stored and the
        /// continuation slot is filled, which <see cref="SignalCompletion"/> does right after storing it.
        /// </summary>
        public UnityTaskStatus UnsafeGetStatus()
        {
            if (Volatile.Read(ref _continuation) == null || Volatile.Read(ref _completed) == 0)
            {
                return UnityTaskStatus.Pending;
            }

            if (_error == null)
            {
                return UnityTaskStatus.Succeeded;
            }

            return _errorIsCancellation ? UnityTaskStatus.Canceled : UnityTaskStatus.Faulted;
        }

        /// <summary>
        /// Registers the single continuation, or runs it at once when the operation already completed.
        /// </summary>
        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            Debugging.ThrowHelper.ThrowIfNull(continuation);
            ValidateToken(token);

            var previous = Volatile.Read(ref _continuation);

            if (previous == null)
            {
                _state = state;

                previous = Interlocked.CompareExchange(
                      location1: ref _continuation
                    , value: continuation
                    , comparand: null
                );
            }

            if (previous == null)
            {
                return;
            }

            if (ReferenceEquals(previous, s_completedSentinel) == false)
            {
                ThrowHelper.ThrowContinuationAlreadyRegistered();
            }

            UnityTaskThreadContext.Run(_affinity, continuation, state);
        }

        /// <summary>
        /// Throws when <paramref name="token"/> is stale or the operation has not completed. Runners call it before
        /// returning themselves to the pool, so an invalid read never recycles a live runner.
        /// </summary>
        public void ValidateResultAccess(short token)
        {
            ValidateToken(token);

            if (Volatile.Read(ref _completed) == 0)
            {
                ThrowHelper.ThrowNotCompleted();
            }
        }

        /// <summary>
        /// Returns the result or rethrows the stored exception with its original stack.
        /// </summary>
        public T GetResult(short token)
        {
            ValidateResultAccess(token);
            _error?.Throw();
            return _result;
        }

        /// <summary>
        /// Clears the outcome for reuse and increments the version so tasks created before the reset are rejected. The
        /// owner calls <see cref="Prepare"/> again afterwards.
        /// </summary>
        public void Reset()
        {
            unchecked
            {
                _version += 1;
            }

            _result = default;
            _error = null;
            _errorIsCancellation = false;
            _completed = 0;
            _affinity = UnityTaskThreadAffinity.None;
            _continuation = null;
            _state = null;
        }

        private readonly void ValidateToken(short token)
        {
            if (token != _version)
            {
                ThrowHelper.ThrowTokenMismatch();
            }
        }

        /// <summary>
        /// Publishes the stored outcome: installs the completed sentinel when no continuation is registered yet;
        /// otherwise runs the registered continuation on the creator's thread kind.
        /// </summary>
        private void SignalCompletion()
        {
            if (Volatile.Read(ref _continuation) == null
                && Interlocked.CompareExchange(
                      location1: ref _continuation
                    , value: s_completedSentinel
                    , comparand: null
                ) == null
            )
            {
                return;
            }

            UnityTaskThreadContext.Run(_affinity, _continuation, _state);
        }
    }
}
