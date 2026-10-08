using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tasks;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Pooled timer behind <c>Yield</c>, <c>Delay</c>, <c>NextFrameAsync</c> and <c>DelayFrameAsync</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Create</c> rents a promise, schedules its cached <c>_tick</c> delegate on <see cref="PlayerLoopScheduler"/>
    /// and returns <see cref="AwaitAndReleaseAsync"/>. Each tick checks the <see cref="DelayKind"/>; when the wait is
    /// over, it completes a pooled <see cref="UnityTaskCompletionSource"/> once. Otherwise it schedules itself again.
    /// </para>
    /// <para>
    /// <c>_completed</c> is set with <see cref="Interlocked"/> so that only one of a tick and a
    /// <c>cancelImmediately</c> callback completes the source. <c>_releases</c> counts two owners: the tick chain and
    /// <see cref="AwaitAndReleaseAsync"/>. The second <see cref="Release"/> disposes the cancellation registration and
    /// returns the promise to the pool.
    /// </para>
    /// <para>
    /// Unity's <c>Time</c> members work on the main thread only. The start time and frame are captured when the promise
    /// is created on the main thread; otherwise they are captured at the first tick. In the Editor outside Play Mode,
    /// frame waits count ticks and delays use real time.
    /// </para>
    /// </remarks>
    internal sealed class UnityTaskDelayPromise
    {
        private const int MAX_POOL_SIZE = 256;

        private static readonly object s_poolLock = new();
        private static readonly Stack<UnityTaskDelayPromise> s_pool = new(MAX_POOL_SIZE);

        private readonly Action _tick;
        private UnityTaskCompletionSource _source;
        private CancellationToken _token;
        private CancellationTokenRegistration _registration;
        private UnityTaskDelayType _delayType;
        private UnityTaskTiming _timing;
        private DelayKind _kind;
        private double _remaining;
        private double _startedAt;
        private int _startFrame;
        private int _delayFrames;
#if UNITY_EDITOR
        private int _elapsedTicks;
#endif
        private bool _startCaptured;
        private int _completed;
        private int _releases;

        private UnityTaskDelayPromise()
        {
            _tick = Tick;
        }

        internal static UnityTask Create(
              DelayKind kind
            , TimeSpan delay
            , UnityTaskDelayType delayType
            , UnityTaskTiming timing
            , bool cancelImmediately
            , CancellationToken token
        )
            => Create(
                  kind: kind
                , delay: delay
                , delayFrames: 0
                , delayType: delayType
                , timing: timing
                , cancelImmediately: cancelImmediately
                , token: token
            );

        /// <summary>
        /// Rents and initializes a promise, schedules its first tick and returns the task that awaits it.
        /// </summary>
        /// <remarks>
        /// A token that is already canceled completes the source as canceled and releases the tick chain's ownership
        /// without scheduling a tick.
        /// </remarks>
        internal static UnityTask Create(
              DelayKind kind
            , TimeSpan delay
            , int delayFrames
            , UnityTaskDelayType delayType
            , UnityTaskTiming timing
            , bool cancelImmediately
            , CancellationToken token
        )
        {
            var promise = Rent();

            promise._token = token;
            promise._delayFrames = delayFrames;
#if UNITY_EDITOR
            promise._elapsedTicks = 0;
#endif
            promise._delayType = delayType;
            promise._timing = timing;
            promise._kind = kind;
            promise._remaining = delay.TotalSeconds;
            promise._startCaptured = false;
            promise._completed = 0;
            promise._releases = 0;

            if (UnityTaskThreadContext.CurrentAffinity == UnityTaskThreadAffinity.MainThread)
            {
                promise.CaptureStart();
            }

            if (token.IsCancellationRequested)
            {
                promise.Cancel();
                promise.Release();
            }
            else
            {
                if (cancelImmediately && token.CanBeCanceled)
                {
                    promise._registration = AwaitableCancellation.Register(
                          token
                        , static state => ((UnityTaskDelayPromise)state).Cancel()
                        , promise
                    );
                }

                PlayerLoopScheduler.Schedule(timing, promise._tick);
            }

            return AwaitAndReleaseAsync(promise);
        }

        /// <summary>
        /// Takes a promise from the pool, or creates one, and gives it a ready <see cref="UnityTaskCompletionSource"/>.
        /// </summary>
        private static UnityTaskDelayPromise Rent()
        {
            UnityTaskDelayPromise promise;

            lock (s_poolLock)
            {
                promise = s_pool.Count == 0 ? new() : s_pool.Pop();
            }

            if (promise._source == null)
            {
                promise._source = new();
            }
            else
            {
                promise._source.Reset();
            }

            return promise;
        }

        /// <summary>
        /// Awaits the source, so that it is read and returned to its pool, then releases the awaiting method's
        /// ownership of the promise.
        /// </summary>
        private static async UnityTask AwaitAndReleaseAsync(UnityTaskDelayPromise promise)
        {
            try
            {
                await promise._source.Task;
            }
            finally
            {
                promise.Release();
            }
        }

        /// <summary>
        /// Records the current real time and frame count as the start of the wait.
        /// </summary>
        private void CaptureStart()
        {
            _startedAt = Time.realtimeSinceStartupAsDouble;
            _startFrame = Time.frameCount;
            _startCaptured = true;
        }

        /// <summary>
        /// Runs once per player loop pass: ends the tick chain when the promise is completed or canceled or its wait is
        /// over, otherwise schedules itself again.
        /// </summary>
        private void Tick()
        {
            // A cancelImmediately callback already completed the source; only the tick chain's release is left.
            if (Volatile.Read(ref _completed) != 0)
            {
                Release();
                return;
            }

            if (_token.IsCancellationRequested)
            {
                Cancel();
                Release();
                return;
            }

            if (_startCaptured == false)
            {
                CaptureStart();
            }

            if (IsComplete())
            {
                // Loses to a concurrent cancelImmediately callback if it set _completed first.
                if (Interlocked.Exchange(location1: ref _completed, value: 1) == 0)
                {
                    _source.TrySetResult();
                }

                Release();
                return;
            }

            PlayerLoopScheduler.Schedule(_timing, _tick);
        }

        /// <summary>
        /// Returns whether the wait of the current <see cref="DelayKind"/> is over.
        /// </summary>
        private bool IsComplete()
        {
            switch (_kind)
            {
                case DelayKind.Yield:
                {
                    return true;
                }

                case DelayKind.NextFrame:
                {
                    return IsNextFrame();
                }

                case DelayKind.DelayFrame:
                {
                    return IsDelayFrameComplete();
                }

                default:
                {
                    return IsDelayComplete();
                }
            }
        }

        /// <summary>
        /// Returns whether the frame count has passed the start frame; always true in the Editor outside Play Mode.
        /// </summary>
        private bool IsNextFrame()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                return true;
            }
#endif
            return Time.frameCount > _startFrame;
        }

        /// <summary>
        /// Returns whether the requested number of frames has passed; the Editor outside Play Mode counts ticks.
        /// </summary>
        private bool IsDelayFrameComplete()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                return ++_elapsedTicks >= _delayFrames;
            }
#endif
            return Time.frameCount - _startFrame >= _delayFrames;
        }

        /// <summary>
        /// Returns whether the requested time has passed. Real time is compared with the start time; the other delay
        /// types subtract each frame's delta time from the remaining time.
        /// </summary>
        private bool IsDelayComplete()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                return Time.realtimeSinceStartupAsDouble - _startedAt >= _remaining;
            }
#endif
            if (_delayType == UnityTaskDelayType.Realtime)
            {
                return Time.realtimeSinceStartupAsDouble - _startedAt >= _remaining;
            }

            _remaining -= _delayType == UnityTaskDelayType.UnscaledDeltaTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            return _remaining <= 0;
        }

        /// <summary>
        /// Completes the source with an <see cref="OperationCanceledException"/> unless it is already completed.
        /// </summary>
        private void Cancel()
        {
            if (Interlocked.Exchange(location1: ref _completed, value: 1) == 0)
            {
                _source.TrySetException(new OperationCanceledException(_token));
            }
        }

        /// <summary>
        /// Drops one of the two owners; the second call clears the promise and returns it to the pool.
        /// </summary>
        private void Release()
        {
            // Owners are the tick chain and the awaiting method; only the later release recycles the promise.
            if (Interlocked.Increment(ref _releases) != 2)
            {
                return;
            }

            _registration.Dispose();
            _registration = default;
            _token = default;

            lock (s_poolLock)
            {
                if (s_pool.Count < MAX_POOL_SIZE)
                {
                    s_pool.Push(this);
                }
            }
        }

        /// <summary>
        /// The wait that a promise performs.
        /// </summary>
        internal enum DelayKind
        {
            Delay,
            Yield,
            NextFrame,
            DelayFrame,
        }
    }
}
