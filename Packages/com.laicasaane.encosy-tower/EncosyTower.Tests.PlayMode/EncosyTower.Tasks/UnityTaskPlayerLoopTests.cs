using System;
using System.Collections;
using System.Threading;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Tasks
{
    /// <summary>
    /// Runs UnityTask against real player-loop frames. The fixture runs in the Editor's Play Mode and in player builds,
    /// including IL2CPP, where generic sharing and the deferred pool return differ from the Editor.
    /// </summary>
    public class UnityTaskPlayerLoopTests
    {
        private int _mainThreadId;

        [SetUp]
        public void SetUp()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator Timings_EveryPhaseResumesOnMainThread()
            => UnityTask.ToCoroutine(async () => {
                foreach (UnityTaskTiming timing in Enum.GetValues(typeof(UnityTaskTiming)))
                {
                    await UnityTask.Yield(timing);
                    AssertMainThread();

                    var frame = Time.frameCount;

                    await UnityTask.NextFrameAsync(timing);
                    AssertMainThread();
                    Assert.Greater(Time.frameCount, frame, $"NextFrameAsync({timing}) resumed in the same frame.");
                }
            });

        [UnityTest]
        public IEnumerator FixedUpdateTimings_ResumeInsideFixedStep()
            => UnityTask.ToCoroutine(async () => {
                await UnityTask.Yield(UnityTaskTiming.FixedUpdate);
                Assert.IsTrue(Time.inFixedTimeStep);

                await UnityTask.WaitForFixedUpdateAsync();
                Assert.IsTrue(Time.inFixedTimeStep);
            });

        [UnityTest]
        public IEnumerator FrameWaits_AdvanceTheExpectedFrames()
            => UnityTask.ToCoroutine(async () => {
                var frame = Time.frameCount;

                await UnityTask.DelayFrameAsync(5);
                Assert.GreaterOrEqual(Time.frameCount - frame, 5);

                frame = Time.frameCount;
                await UnityTask.WaitForEndOfFrameAsync();
                Assert.AreEqual(frame, Time.frameCount, "WaitForEndOfFrameAsync did not resume in the same frame.");
                AssertMainThread();
            });

        [UnityTest]
        public IEnumerator TimeWaits_WaitAtLeastTheDuration()
            => UnityTask.ToCoroutine(async () => {
                var start = Time.realtimeSinceStartupAsDouble;

                await UnityTask.Delay(TimeSpan.FromMilliseconds(200), UnityTaskDelayType.Realtime);
                Assert.GreaterOrEqual(Time.realtimeSinceStartupAsDouble - start, 0.19);

                start = Time.realtimeSinceStartupAsDouble;
                await UnityTask.WaitForSecondsAsync(0.1f);
                Assert.GreaterOrEqual(Time.realtimeSinceStartupAsDouble - start, 0.09);

                start = Time.realtimeSinceStartupAsDouble;
                await UnityTask.WaitForSecondsAsync(0.1f, ignoreTimeScale: true);
                Assert.GreaterOrEqual(Time.realtimeSinceStartupAsDouble - start, 0.09);
            });

        [UnityTest]
        public IEnumerator ZeroTimeScale_PausesScaledDelayOnly()
            => UnityTask.ToCoroutine(async () => {
                Time.timeScale = 0f;

                var scaled = UnityTask.Delay(TimeSpan.FromMilliseconds(100));
                var unscaled = UnityTask.Delay(TimeSpan.FromMilliseconds(100), UnityTaskDelayType.UnscaledDeltaTime);
                var realtime = UnityTask.Delay(TimeSpan.FromMilliseconds(100), UnityTaskDelayType.Realtime);

                await UnityTask.Delay(TimeSpan.FromMilliseconds(400), UnityTaskDelayType.Realtime);

                Assert.IsTrue(unscaled.IsCompleted, "Unscaled delay did not complete while time scale was 0.");
                Assert.IsTrue(realtime.IsCompleted, "Realtime delay did not complete while time scale was 0.");
                Assert.IsFalse(scaled.IsCompleted, "Scaled delay completed while time scale was 0.");

                await unscaled;
                await realtime;

                Time.timeScale = 1f;
                await scaled;
            });

        [UnityTest]
        public IEnumerator AwaitableRelay_ForwardsFramesResultsFailuresAndCancellation()
            => UnityTask.ToCoroutine(async () => {
                var frame = Time.frameCount;

                await Awaitable.NextFrameAsync().AsUnityTask();
                Assert.Greater(Time.frameCount, frame);

                var start = Time.realtimeSinceStartupAsDouble;

                await Awaitable.WaitForSecondsAsync(0.1f).AsUnityTask();
                Assert.GreaterOrEqual(Time.realtimeSinceStartupAsDouble - start, 0.09);

                Assert.AreEqual(5, await ReturnAfterFrameAsync(5).AsUnityTask());

                var failure = await CaptureAsync(ThrowAfterFrameAsync().AsUnityTask());

                Assert.IsInstanceOf<InvalidOperationException>(failure);

                using var cancellation = new CancellationTokenSource();
                var pending = Awaitable.WaitForSecondsAsync(10f, cancellation.Token).AsUnityTask();

                cancellation.Cancel();

                var canceled = await CaptureAsync(pending);

                Assert.IsInstanceOf<OperationCanceledException>(canceled);
                AssertMainThread();
            });

        [UnityTest]
        public IEnumerator AwaitableRelay_ManyConcurrentRelaysAllComplete()
            => UnityTask.ToCoroutine(async () => {
                for (var round = 0; round < 3; round++)
                {
                    var tasks = new UnityTask[1000];

                    for (var i = 0; i < tasks.Length; i++)
                    {
                        tasks[i] = (i % 2) == 0
                            ? Awaitable.NextFrameAsync().AsUnityTask()
                            : ReturnAfterFrameAsync(i).AsUnityTask().ContinueWith(static _ => { });
                    }

                    await UnityTask.WhenAll(tasks);
                }
            });

        [UnityTest]
        public IEnumerator PooledPromises_SurviveRepeatedHeavyRounds()
            => UnityTask.ToCoroutine(async () => {
                for (var round = 0; round < 3; round++)
                {
                    var tasks = new UnityTask[2000];

                    for (var i = 0; i < tasks.Length; i++)
                    {
                        tasks[i] = (i % 4) switch {
                            0 => UnityTask.DelayFrameAsync(1 + (i % 3)),
                            1 => UnityTask.Yield(UnityTaskTiming.LastPostLateUpdate),
                            2 => UnityTask.Delay(TimeSpan.FromMilliseconds(i % 50)),
                            _ => UnityTask.NextFrameAsync(UnityTaskTiming.PreLateUpdate),
                        };
                    }

                    await UnityTask.WhenAll(tasks);
                }
            });

        [UnityTest]
        public IEnumerator Threading_SwitchesAndReturnsToMainThread()
            => UnityTask.ToCoroutine(async () => {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    Assert.Ignore("WebGL has no thread pool.");
                }

                var workerThreadId = 0;

                await UnityTask.RunOnThreadPool(() => workerThreadId = Thread.CurrentThread.ManagedThreadId);
                AssertMainThread();
                Assert.AreNotEqual(_mainThreadId, workerThreadId);

                await UnityTask.SwitchToThreadPoolAsync();
                Assert.AreNotEqual(_mainThreadId, Thread.CurrentThread.ManagedThreadId);

                await UnityTask.SwitchToMainThreadAsync();
                AssertMainThread();

                var value = await UnityTask.RunOnThreadPool(static () => 42);

                Assert.AreEqual(42, value);
                AssertMainThread();
            });

        [UnityTest]
        public IEnumerator Cancellation_TimersAndTimeoutsCancelPendingWaits()
            => UnityTask.ToCoroutine(async () => {
                using (var source = new CancellationTokenSource())
                {
                    UnityTaskCancellation.CancelAfterSlim(source, 100);

                    var exception = await CaptureAsync(UnityTask.Delay(TimeSpan.FromSeconds(10), token: source.Token));

                    Assert.IsInstanceOf<OperationCanceledException>(exception);
                }

                using (var source = new CancellationTokenSource())
                {
                    var handle = UnityTaskCancellation.CancelAfterSlim(source, 100);

                    handle.Dispose();
                    await UnityTask.Delay(TimeSpan.FromMilliseconds(250), UnityTaskDelayType.Realtime);
                    Assert.IsFalse(source.IsCancellationRequested, "Disposed CancelAfterSlim still canceled.");
                }

                using (var timeout = new TimeoutController())
                {
                    var exception = await CaptureAsync(UnityTask.Delay(
                          TimeSpan.FromSeconds(10)
                        , token: timeout.Timeout(TimeSpan.FromMilliseconds(100))
                    ));

                    Assert.IsInstanceOf<OperationCanceledException>(exception);
                    Assert.IsTrue(timeout.IsTimeout());
                }
            });

        [UnityTest]
        public IEnumerator PlayerLoopTimer_FiresPeriodicallyUntilDisposed()
            => UnityTask.ToCoroutine(async () => {
                var ticks = new int[1];
                var timer = PlayerLoopTimer.StartNew(
                      TimeSpan.FromMilliseconds(50)
                    , periodic: true
                    , UnityTaskDelayType.Realtime
                    , UnityTaskTiming.Update
                    , CancellationToken.None
                    , static state => ((int[])state)[0]++
                    , ticks
                );

                await UnityTask.Delay(TimeSpan.FromMilliseconds(400), UnityTaskDelayType.Realtime);
                timer.Dispose();

                var ticksAtDispose = ticks[0];

                Assert.GreaterOrEqual(ticksAtDispose, 3);

                await UnityTask.Delay(TimeSpan.FromMilliseconds(200), UnityTaskDelayType.Realtime);
                Assert.AreEqual(ticksAtDispose, ticks[0], "Timer kept firing after Dispose.");
            });

        [UnityTest]
        public IEnumerator DestroyTrigger_CancelsWhenGameObjectIsDestroyed()
            => UnityTask.ToCoroutine(async () => {
                var gameObject = new GameObject(nameof(DestroyTrigger_CancelsWhenGameObjectIsDestroyed));
                using var source = new CancellationTokenSource();

                UnityTaskCancellation.RegisterRaiseCancelOnDestroy(source, gameObject);

                var pending = UnityTask.WaitUntilCanceledAsync(source.Token);

                await UnityTask.NextFrameAsync();
                Assert.IsFalse(source.IsCancellationRequested);

                UnityEngine.Object.Destroy(gameObject);
                await UnityTask.NextFrameAsync();

                Assert.IsTrue(source.IsCancellationRequested);
                await pending;
            });

        [UnityTest]
        public IEnumerator AsyncOperations_CompleteAndReturnAssets()
            => UnityTask.ToCoroutine(async () => {
                var operation = Resources.UnloadUnusedAssets();

                await operation.ToUnityTask();
                Assert.IsTrue(operation.isDone);

                var asset = await Resources.LoadAsync<Texture2D>("encosy-tests-missing-asset").ToUnityTask();

                Assert.IsNull(asset);
            });

        [UnityTest]
        public IEnumerator Coroutines_InteropBothWays()
            => UnityTask.ToCoroutine(async () => {
                var gameObject = new GameObject(nameof(Coroutines_InteropBothWays));

                try
                {
                    var runner = gameObject.AddComponent<CoroutineRunner>();
                    var steps = new int[1];

                    await CountFrames(steps, 3).ToUnityTask(runner);
                    Assert.AreEqual(3, steps[0]);

                    var result = new int[1];

                    runner.StartCoroutine(UnityTask.DelayFrameAsync(2)
                        .ContinueWith(static () => 9)
                        .ToCoroutine(value => result[0] = value)
                    );

                    await UnityTask.WaitUntil(() => result[0] != 0);
                    Assert.AreEqual(9, result[0]);
                }
                finally
                {
                    UnityEngine.Object.Destroy(gameObject);
                }

                static IEnumerator CountFrames(int[] steps, int count)
                {
                    for (var i = 0; i < count; i++)
                    {
                        steps[0]++;
                        yield return null;
                    }
                }
            });

        [UnityTest]
        public IEnumerator Generics_ValueAndReferenceResultsFlowThroughCombinators()
            => UnityTask.ToCoroutine(async () => {
                var guid = Guid.NewGuid();
                var (number, text, id, big) = await UnityTask.WhenAll(
                      DelayedAsync(1)
                    , DelayedAsync("two")
                    , DelayedAsync(guid)
                    , DelayedAsync(new LargeValue { A = 1, D = 4 })
                );

                Assert.AreEqual(1, number);
                Assert.AreEqual("two", text);
                Assert.AreEqual(guid, id);
                Assert.AreEqual(4, big.D);

                var (winner, _, _) = await UnityTask.WhenAny(
                      DelayedAsync(10L, frames: 5)
                    , DelayedAsync(20L, frames: 1)
                );

                Assert.AreEqual(1, winner);

                var lazy = UnityTask.Lazy(static () => DelayedAsync(7.5));

                Assert.AreEqual(7.5, await lazy);
                Assert.AreEqual(7.5, await lazy);

                var completion = new UnityTaskCompletionSource<LargeValue>();

                UnityTask.Post(() => completion.TrySetResult(new LargeValue { B = 2 }), UnityTaskTiming.PostLateUpdate);
                Assert.AreEqual(2, (await completion.Task).B);

                var voidRan = new bool[1];

                VoidAsync(voidRan).Forget();
                await UnityTask.WaitUntil(() => voidRan[0]);
            });

        private void AssertMainThread()
            => Assert.AreEqual(_mainThreadId, Thread.CurrentThread.ManagedThreadId, "Resumed off the main thread.");

        private static async UnityTask<T> DelayedAsync<T>(T value, int frames = 2)
        {
            await UnityTask.DelayFrameAsync(frames);
            return value;
        }

        private static async UnityTaskVoid VoidAsync(bool[] ran)
        {
            await UnityTask.NextFrameAsync();
            ran[0] = true;
        }

        private static async Awaitable<int> ReturnAfterFrameAsync(int value)
        {
            await Awaitable.NextFrameAsync();
            return value;
        }

        private static async Awaitable ThrowAfterFrameAsync()
        {
            await Awaitable.NextFrameAsync();
            throw new InvalidOperationException("expected");
        }

        private static async UnityTask<Exception> CaptureAsync(UnityTask task)
        {
            try
            {
                await task;
                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private struct LargeValue
        {
            public long A;
            public long B;
            public long C;
            public long D;
        }

        private sealed class CoroutineRunner : MonoBehaviour
        {
        }
    }
}
