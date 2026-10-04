using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tasks;

namespace EncosyTower.Tasks
{
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
        private bool _startCaptured;
        private int _completed;
        private int _consumed;
        private int _scheduled;
        private int _returned;

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
        {
            var promise = Rent();

            promise._token = token;
            promise._delayType = delayType;
            promise._timing = timing;
            promise._kind = kind;
            promise._remaining = delay.TotalSeconds;
            promise._startCaptured = false;
            promise._completed = 0;
            promise._consumed = 0;
            promise._returned = 0;

            if (UnityTaskThreadContext.CurrentAffinity == UnityTaskThreadAffinity.MainThread)
            {
                promise.CaptureStart();
            }

            if (token.IsCancellationRequested)
            {
                promise._scheduled = 0;
                promise.Cancel();
            }
            else
            {
                promise._scheduled = 1;

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

        private static async UnityTask AwaitAndReleaseAsync(UnityTaskDelayPromise promise)
        {
            try
            {
                await promise._source.Task;
            }
            finally
            {
                Interlocked.Exchange(ref promise._consumed, 1);
                promise.TryReturn();
            }
        }

        private void CaptureStart()
        {
            _startedAt = Time.realtimeSinceStartupAsDouble;
            _startFrame = Time.frameCount;
            _startCaptured = true;
        }

        private void Tick()
        {
            if (Volatile.Read(ref _completed) != 0)
            {
                Interlocked.Exchange(ref _scheduled, 0);
                TryReturn();
                return;
            }

            if (_token.IsCancellationRequested)
            {
                Interlocked.Exchange(ref _scheduled, 0);
                Cancel();
                TryReturn();
                return;
            }

            if (_startCaptured == false)
            {
                CaptureStart();
            }

            if (IsComplete())
            {
                Interlocked.Exchange(ref _scheduled, 0);

                if (Interlocked.Exchange(ref _completed, 1) == 0)
                {
                    _source.TrySetResult();
                }

                TryReturn();
                return;
            }

            PlayerLoopScheduler.Schedule(_timing, _tick);
        }

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

                default:
                {
                    return IsDelayComplete();
                }
            }
        }

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

        private void Cancel()
        {
            if (Interlocked.Exchange(ref _completed, 1) == 0)
            {
                _source.TrySetException(new OperationCanceledException(_token));
            }

            TryReturn();
        }

        private void TryReturn()
        {
            if (Volatile.Read(ref _consumed) == 0 || Volatile.Read(ref _scheduled) != 0)
            {
                return;
            }

            if (Interlocked.Exchange(ref _returned, 1) != 0)
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

        internal enum DelayKind
        {
            Delay,
            Yield,
            NextFrame,
        }
    }
}
