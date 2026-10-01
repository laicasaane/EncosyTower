using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UnityEngine.Tasks
{
    public static partial class Awaitables
    {
        public static Awaitable Yield()
            => DelayPromise.Create(
                  TimeSpan.Zero
                , AwaitableDelayType.DeltaTime
                , AwaitablePlayerLoopTiming.Update
                , default
                , false
                , true
            );

        public static Awaitable Yield(AwaitablePlayerLoopTiming timing)
            => DelayPromise.Create(TimeSpan.Zero, AwaitableDelayType.DeltaTime, timing, default, false, true);

        public static Awaitable Yield(CancellationToken cancellationToken, bool cancelImmediately = false)
            => DelayPromise.Create(
                  TimeSpan.Zero
                , AwaitableDelayType.DeltaTime
                , AwaitablePlayerLoopTiming.Update
                , cancellationToken
                , cancelImmediately
                , true
            );

        public static Awaitable Yield(
              AwaitablePlayerLoopTiming timing
            , CancellationToken cancellationToken
            , bool cancelImmediately = false
        )
            => DelayPromise.Create(
                  TimeSpan.Zero
                , AwaitableDelayType.DeltaTime
                , timing
                , cancellationToken
                , cancelImmediately
                , true
            );

        public static Awaitable Delay(
              int millisecondsDelay
            , bool ignoreTimeScale = false
            , AwaitablePlayerLoopTiming delayTiming = AwaitablePlayerLoopTiming.Update
            , CancellationToken cancellationToken = default
            , bool cancelImmediately = false
        )
            => Delay(
                  TimeSpan.FromMilliseconds(ValidateMilliseconds(millisecondsDelay))
                , ignoreTimeScale
                , delayTiming
                , cancellationToken
                , cancelImmediately
            );

        public static Awaitable Delay(
              TimeSpan delayTimeSpan
            , bool ignoreTimeScale = false
            , AwaitablePlayerLoopTiming delayTiming = AwaitablePlayerLoopTiming.Update
            , CancellationToken cancellationToken = default
            , bool cancelImmediately = false
        )
            => DelayPromise.Create(
                  ValidateDelay(delayTimeSpan)
                , ignoreTimeScale ? AwaitableDelayType.UnscaledDeltaTime : AwaitableDelayType.DeltaTime
                , delayTiming
                , cancellationToken
                , cancelImmediately
                , false
            );

        public static Awaitable Delay(
              int millisecondsDelay
            , AwaitableDelayType delayType
            , AwaitablePlayerLoopTiming delayTiming = AwaitablePlayerLoopTiming.Update
            , CancellationToken cancellationToken = default
            , bool cancelImmediately = false
        )
            => Delay(
                  TimeSpan.FromMilliseconds(ValidateMilliseconds(millisecondsDelay))
                , delayType
                , delayTiming
                , cancellationToken
                , cancelImmediately
            );

        public static Awaitable Delay(
              TimeSpan delayTimeSpan
            , AwaitableDelayType delayType
            , AwaitablePlayerLoopTiming delayTiming = AwaitablePlayerLoopTiming.Update
            , CancellationToken cancellationToken = default
            , bool cancelImmediately = false
        )
            => DelayPromise.Create(
                  ValidateDelay(delayTimeSpan)
                , delayType
                , delayTiming
                , cancellationToken
                , cancelImmediately
                , false
            );

        private static int ValidateMilliseconds(int millisecondsDelay)
        {
            ThrowHelper.ThrowIfMillisecondsDelayInvalid(millisecondsDelay);

            return millisecondsDelay;
        }

        private static TimeSpan ValidateDelay(TimeSpan delayTimeSpan)
        {
            ThrowHelper.ThrowIfDelayInvalid(delayTimeSpan);

            return delayTimeSpan;
        }

        private sealed class DelayPromise
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly object s_poolLock = new();
            private static readonly Stack<DelayPromise> s_pool = new(MAX_POOL_SIZE);

            private readonly AwaitableCompletionSource _source = new();
            private readonly Action _tick;
            private CancellationToken _cancellationToken;
            private CancellationTokenRegistration _registration;
            private AwaitableDelayType _delayType;
            private AwaitablePlayerLoopTiming _timing;
            private double _remaining;
            private double _startedAt;
            private int _completed;
            private int _consumed;
            private int _scheduled;
            private int _returned;
            private bool _yieldOnly;

            private DelayPromise()
            {
                _tick = Tick;
            }

            internal static Awaitable Create(
                  TimeSpan delay
                , AwaitableDelayType delayType
                , AwaitablePlayerLoopTiming timing
                , CancellationToken cancellationToken
                , bool cancelImmediately
                , bool yieldOnly
            )
            {
                DelayPromise promise;

                lock (s_poolLock)
                {
                    promise = s_pool.Count == 0 ? new() : s_pool.Pop();
                }

                promise._cancellationToken = cancellationToken;
                promise._delayType = delayType;
                promise._timing = timing;
                promise._remaining = delay.TotalSeconds;
                promise._startedAt = yieldOnly ? 0 : Time.realtimeSinceStartupAsDouble;
                promise._completed = 0;
                promise._consumed = 0;
                promise._returned = 0;
                promise._yieldOnly = yieldOnly;

                if (cancellationToken.IsCancellationRequested)
                {
                    promise._scheduled = 0;
                    promise.Cancel();
                }
                else
                {
                    promise._scheduled = 1;

                    if (cancelImmediately && cancellationToken.CanBeCanceled)
                    {
                        promise._registration = AwaitableCancellation.Register(
                              cancellationToken
                            , static state => ((DelayPromise)state).Cancel()
                            , promise
                        );
                    }

                    AwaitablePlayerLoopScheduler.Schedule(timing, promise._tick);
                }

                return AwaitAndRelease(promise);
            }

            private static async Awaitable AwaitAndRelease(DelayPromise promise)
            {
                try
                {
                    await promise._source.Awaitable;
                }
                finally
                {
                    Interlocked.Exchange(ref promise._consumed, 1);
                    promise.TryReturn();
                }
            }

            private void Tick()
            {
                if (Volatile.Read(ref _completed) != 0)
                {
                    Interlocked.Exchange(ref _scheduled, 0);
                    TryReturn();
                    return;
                }

                if (_cancellationToken.IsCancellationRequested)
                {
                    Interlocked.Exchange(ref _scheduled, 0);
                    Cancel();
                    TryReturn();
                    return;
                }

                if (_yieldOnly || IsDelayComplete())
                {
                    Interlocked.Exchange(ref _scheduled, 0);

                    if (Interlocked.Exchange(ref _completed, 1) == 0)
                    {
                        _source.TrySetResult();
                    }

                    TryReturn();
                    return;
                }

                AwaitablePlayerLoopScheduler.Schedule(_timing, _tick);
            }

            private bool IsDelayComplete()
            {
#if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    return Time.realtimeSinceStartupAsDouble - _startedAt >= _remaining;
                }
#endif
                if (_delayType == AwaitableDelayType.Realtime)
                {
                    return Time.realtimeSinceStartupAsDouble - _startedAt >= _remaining;
                }

                _remaining -= _delayType == AwaitableDelayType.UnscaledDeltaTime
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;
                return _remaining <= 0;
            }

            private void Cancel()
            {
                if (Interlocked.Exchange(ref _completed, 1) == 0)
                {
                    _source.TrySetException(new OperationCanceledException(_cancellationToken));
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
                _cancellationToken = default;
                _source.Reset();

                lock (s_poolLock)
                {
                    if (s_pool.Count < MAX_POOL_SIZE)
                    {
                        s_pool.Push(this);
                    }
                }
            }
        }
    }
}
