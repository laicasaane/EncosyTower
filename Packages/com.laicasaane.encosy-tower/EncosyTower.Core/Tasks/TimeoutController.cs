using System;
using System.Threading;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Produces reusable timeout tokens measured on the shared Encosy player-loop scheduler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Behaviour:</b> <see cref="Timeout(TimeSpan)"/> returns a token that is cancelled when the timeout elapses.
    /// Call <see cref="Reset"/> after the guarded operation completes so that the timer stops and the same token can be
    /// reused for the next call. After a timeout, the next <see cref="Timeout(TimeSpan)"/> creates a new token.
    /// </para>
    /// <para>
    /// <b>Thread:</b> use it on the main thread.
    /// </para>
    /// <para>
    /// The name matches <c>Cysharp.Threading.Tasks.TimeoutController</c>. A file that imports both namespaces refers
    /// to this type by its full name.
    /// </para>
    /// <para>
    /// <b>Counterparts:</b> UniTask: <c>Cysharp.Threading.Tasks.TimeoutController</c>; Unity: none.
    /// </para>
    /// </remarks>
    public sealed class TimeoutController : IDisposable
    {
        private readonly UnityTaskDelayType _delayType;
        private readonly UnityTaskTiming _timing;
        private readonly CancellationTokenSource _originalLinkSource;
        private CancellationTokenSource _timeoutSource;
        private CancellationTokenSource _linkedSource;
        private PlayerLoopTimer _timer;
        private bool _isDisposed;

        /// <summary>
        /// Creates a controller.
        /// </summary>
        /// <param name="delayType">The clock that measures each timeout.</param>
        /// <param name="timing">The player-loop phase at which the timeout is checked.</param>
        public TimeoutController(
              UnityTaskDelayType delayType = UnityTaskDelayType.DeltaTime
            , UnityTaskTiming timing = UnityTaskTiming.Update
        )
        {
            _timeoutSource = new CancellationTokenSource();
            _delayType = delayType;
            _timing = timing;
        }

        /// <summary>
        /// Creates a controller whose tokens are also cancelled by <paramref name="linkCancellationTokenSource"/>.
        /// </summary>
        /// <param name="linkCancellationTokenSource">
        /// A source whose cancellation also cancels the timeout tokens.
        /// </param>
        /// <param name="delayType">The clock that measures each timeout.</param>
        /// <param name="timing">The player-loop phase at which the timeout is checked.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="linkCancellationTokenSource"/> is <c>null</c>.
        /// </exception>
        public TimeoutController(
              CancellationTokenSource linkCancellationTokenSource
            , UnityTaskDelayType delayType = UnityTaskDelayType.DeltaTime
            , UnityTaskTiming timing = UnityTaskTiming.Update
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(linkCancellationTokenSource);

            _timeoutSource = new CancellationTokenSource();
            _originalLinkSource = linkCancellationTokenSource;
            _linkedSource = CreateLinkedSource();
            _delayType = delayType;
            _timing = timing;
        }

        /// <summary>
        /// Starts or restarts the timeout and returns its token.
        /// </summary>
        /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
        /// <returns>A token cancelled when the timeout elapses.</returns>
        /// <exception cref="ObjectDisposedException">The controller is disposed.</exception>
        public CancellationToken Timeout(int millisecondsTimeout)
            => Timeout(TimeSpan.FromMilliseconds(millisecondsTimeout));

        /// <summary>
        /// Starts or restarts the timeout and returns its token.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>A token cancelled when the timeout elapses.</returns>
        /// <exception cref="ObjectDisposedException">The controller is disposed.</exception>
        public CancellationToken Timeout(TimeSpan timeout)
        {
            if (_isDisposed)
            {
                ThrowHelper.ThrowTimeoutControllerDisposed();
            }

            if (_originalLinkSource != null && _originalLinkSource.IsCancellationRequested)
            {
                return _originalLinkSource.Token;
            }

            if (_timeoutSource.IsCancellationRequested)
            {
                _timeoutSource.Dispose();
                _timeoutSource = new CancellationTokenSource();

                if (_linkedSource != null)
                {
                    _linkedSource.Cancel();
                    _linkedSource.Dispose();
                    _linkedSource = CreateLinkedSource();
                }

                _timer?.Dispose();
                _timer = null;
            }

            var token = (_linkedSource ?? _timeoutSource).Token;

            if (_timer == null)
            {
                _timer = PlayerLoopTimer.StartNew(
                      interval: timeout
                    , periodic: false
                    , delayType: _delayType
                    , timing: _timing
                    , token: token
                    , timerCallback: static state => ((CancellationTokenSource)state).Cancel()
                    , state: _timeoutSource
                );
            }
            else
            {
                _timer.Restart(timeout);
            }

            return token;
        }

        /// <summary>
        /// Returns whether the last timeout elapsed.
        /// </summary>
        /// <returns><c>true</c> when the current timeout token was cancelled by the timer.</returns>
        public bool IsTimeout()
            => _timeoutSource.IsCancellationRequested;

        /// <summary>
        /// Stops the current timer so that the current token stays usable.
        /// </summary>
        public void Reset()
        {
            _timer?.Stop();
        }

        /// <summary>
        /// Stops the timer and releases the token sources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _timer?.Dispose();
            _timeoutSource.Cancel();
            _timeoutSource.Dispose();

            if (_linkedSource != null)
            {
                _linkedSource.Cancel();
                _linkedSource.Dispose();
            }
        }

        /// <summary>
        /// Creates a source that is cancelled when either the timeout source or the linked source passed to the
        /// constructor is cancelled.
        /// </summary>
        /// <returns>A new linked source; the caller owns and disposes it.</returns>
        private CancellationTokenSource CreateLinkedSource()
            => CancellationTokenSource.CreateLinkedTokenSource(_timeoutSource.Token, _originalLinkSource.Token);
    }
}
