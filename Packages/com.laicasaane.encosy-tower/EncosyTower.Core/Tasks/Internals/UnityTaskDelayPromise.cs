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
        {
            var promise = Rent();

            promise._token = token;
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
                promise.Release();
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
                if (Interlocked.Exchange(location1: ref _completed, value: 1) == 0)
                {
                    _source.TrySetResult();
                }

                Release();
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
            if (Interlocked.Exchange(location1: ref _completed, value: 1) == 0)
            {
                _source.TrySetException(new OperationCanceledException(_token));
            }
        }

        private void Release()
        {
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

        internal enum DelayKind
        {
            Delay,
            Yield,
            NextFrame,
        }
    }
}
