using System;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// A timer that runs a callback on the shared Encosy player-loop scheduler after an interval, once or
    /// periodically.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Behaviour:</b> the interval is measured with <see cref="UnityTaskDelayType"/> rules; in the Editor outside
    /// Play Mode it uses real time. The timer stops when stopped, disposed, or when its token is cancelled. An
    /// exception thrown by the callback is logged and the timer continues.
    /// </para>
    /// <para>
    /// <b>Thread:</b> create, restart and stop it on the main thread. The callback runs on the main thread.
    /// </para>
    /// <para>
    /// The name matches <c>Cysharp.Threading.Tasks.PlayerLoopTimer</c>. A file that imports both namespaces refers to
    /// this type by its full name.
    /// </para>
    /// <para>
    /// <b>Counterparts:</b> UniTask: <c>Cysharp.Threading.Tasks.PlayerLoopTimer</c>; Unity: none.
    /// </para>
    /// </remarks>
    public sealed class PlayerLoopTimer : IDisposable
    {
        private readonly bool _periodic;
        private readonly UnityTaskDelayType _delayType;
        private readonly UnityTaskTiming _timing;
        private readonly CancellationToken _token;
        private readonly Action<object> _callback;
        private readonly object _state;
        private readonly Action _tick;
        private TimeSpan _interval;
        private double _elapsed;
        private double _startedAt;
        private int _startFrame;
        private bool _isRunning;
        private bool _stopRequested;
        private bool _isDisposed;

        /// <summary>
        /// Stores the settings of a stopped timer and creates the delegate that is scheduled on each check.
        /// </summary>
        /// <remarks>
        /// <c>_tick</c> is created once, so rescheduling the timer on every frame allocates no delegate. Use
        /// <see cref="Create"/> or <see cref="StartNew"/>, which also validate the callback.
        /// </remarks>
        private PlayerLoopTimer(
              TimeSpan interval
            , bool periodic
            , UnityTaskDelayType delayType
            , UnityTaskTiming timing
            , CancellationToken token
            , Action<object> timerCallback
            , object state
        )
        {
            _interval = interval;
            _periodic = periodic;
            _delayType = delayType;
            _timing = timing;
            _token = token;
            _callback = timerCallback;
            _state = state;
            _tick = Tick;
        }

        /// <summary>
        /// Creates a stopped timer.
        /// </summary>
        /// <param name="interval">The time between the start and each callback.</param>
        /// <param name="periodic"><c>true</c> to run the callback every interval; <c>false</c> to run it once.</param>
        /// <param name="delayType">The clock that measures <paramref name="interval"/>.</param>
        /// <param name="timing">The player-loop phase at which the timer is checked.</param>
        /// <param name="token">The token that stops the timer.</param>
        /// <param name="timerCallback">The callback to run.</param>
        /// <param name="state">The state passed to <paramref name="timerCallback"/>.</param>
        /// <returns>A timer that starts on <see cref="Restart()"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timerCallback"/> is <c>null</c>.</exception>
        public static PlayerLoopTimer Create(
              TimeSpan interval
            , bool periodic
            , UnityTaskDelayType delayType
            , UnityTaskTiming timing
            , CancellationToken token
            , Action<object> timerCallback
            , object state
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(timerCallback);
            return new(interval, periodic, delayType, timing, token, timerCallback, state);
        }

        /// <summary>
        /// Creates and starts a timer.
        /// </summary>
        /// <param name="interval">The time between the start and each callback.</param>
        /// <param name="periodic"><c>true</c> to run the callback every interval; <c>false</c> to run it once.</param>
        /// <param name="delayType">The clock that measures <paramref name="interval"/>.</param>
        /// <param name="timing">The player-loop phase at which the timer is checked.</param>
        /// <param name="token">The token that stops the timer.</param>
        /// <param name="timerCallback">The callback to run.</param>
        /// <param name="state">The state passed to <paramref name="timerCallback"/>.</param>
        /// <returns>A running timer.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timerCallback"/> is <c>null</c>.</exception>
        public static PlayerLoopTimer StartNew(
              TimeSpan interval
            , bool periodic
            , UnityTaskDelayType delayType
            , UnityTaskTiming timing
            , CancellationToken token
            , Action<object> timerCallback
            , object state
        )
        {
            var timer = Create(interval, periodic, delayType, timing, token, timerCallback, state);
            timer.Restart();
            return timer;
        }

        /// <summary>
        /// Starts the timer, or restarts the current interval when it is running.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The timer is disposed.</exception>
        public void Restart()
        {
            if (_isDisposed)
            {
                ThrowHelper.ThrowTimerDisposed();
            }

            _elapsed = 0d;
            _startedAt = Time.realtimeSinceStartupAsDouble;
            _startFrame = Time.frameCount;
            _stopRequested = false;

            // Restart while running reuses the tick that is already scheduled, so the timer is never scheduled twice.
            if (_isRunning == false)
            {
                _isRunning = true;
                PlayerLoopScheduler.Schedule(_timing, _tick);
            }
        }

        /// <summary>
        /// Changes the interval, then restarts the timer.
        /// </summary>
        /// <param name="interval">The new interval.</param>
        /// <exception cref="ObjectDisposedException">The timer is disposed.</exception>
        public void Restart(TimeSpan interval)
        {
            _interval = interval;
            Restart();
        }

        /// <summary>
        /// Stops the timer before its next check.
        /// </summary>
        public void Stop()
        {
            _stopRequested = true;
        }

        /// <summary>
        /// Stops the timer permanently.
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;
        }

        /// <summary>
        /// Checks the timer once and schedules the next check.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Runs on the scheduler at <c>_timing</c>. It ends the timer, without scheduling again, when the timer is
        /// disposed, stopped or cancelled, and after the callback of a timer that is not periodic.
        /// </para>
        /// <para>
        /// A periodic timer measures the next interval from the callback. An exception from the callback is logged and
        /// does not stop the timer.
        /// </para>
        /// </remarks>
        private void Tick()
        {
            if (_isDisposed || _stopRequested || _token.IsCancellationRequested)
            {
                _isRunning = false;
                return;
            }

            if (IsElapsed())
            {
                try
                {
                    _callback(_state);
                }
                catch (Exception exception)
                {
                    ThrowHelper.LogUnobservedException(exception);
                }

                if (_periodic == false)
                {
                    _isRunning = false;
                    return;
                }

                _elapsed = 0d;
                _startedAt = Time.realtimeSinceStartupAsDouble;
            }

            PlayerLoopScheduler.Schedule(_timing, _tick);
        }

        /// <summary>
        /// Returns whether the interval has elapsed since the timer started or last fired.
        /// </summary>
        /// <returns><c>true</c> if the interval has elapsed.</returns>
        /// <remarks>
        /// In the Editor outside Play Mode, and for <see cref="UnityTaskDelayType.Realtime"/>, it compares
        /// <c>Time.realtimeSinceStartupAsDouble</c> with the start time. Otherwise it adds the delta time of the
        /// current frame to <c>_elapsed</c>: <c>Time.unscaledDeltaTime</c> for
        /// <see cref="UnityTaskDelayType.UnscaledDeltaTime"/> and <c>Time.deltaTime</c> for the other types.
        /// </remarks>
        private bool IsElapsed()
        {
            var intervalSeconds = _interval.TotalSeconds;

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                return Time.realtimeSinceStartupAsDouble - _startedAt >= intervalSeconds;
            }
#endif
            if (_delayType == UnityTaskDelayType.Realtime)
            {
                return Time.realtimeSinceStartupAsDouble - _startedAt >= intervalSeconds;
            }

            // A timer that started in this frame has no complete frame to count yet.
            if (Time.frameCount == _startFrame)
            {
                return false;
            }

            _elapsed += _delayType == UnityTaskDelayType.UnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;
            return _elapsed >= intervalSeconds;
        }
    }
}
