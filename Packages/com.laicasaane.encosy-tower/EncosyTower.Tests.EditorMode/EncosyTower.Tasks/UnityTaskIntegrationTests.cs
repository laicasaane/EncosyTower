using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Tasks;
using NUnit.Framework;
using Unity.Jobs;
using UnityEngine;

namespace EncosyTower.Tests.Tasks
{
    public class UnityTaskIntegrationTests
    {
        [Test]
        public async Task ResourceRequest_ToUnityTask_CompletesWithNullForMissingAsset()
        {
            var asset = await Resources.LoadAsync<TextAsset>("EncosyTower.Tests/Missing/Asset").ToUnityTask();

            Assert.IsNull(asset);
        }

        [Test]
        public async Task ResourceRequest_Await_CompletesWithNullForMissingAsset()
        {
            var asset = await Resources.LoadAsync<TextAsset>("EncosyTower.Tests/Missing/Asset");

            Assert.IsNull(asset);
        }

        [Test]
        public async Task IEnumerator_Await_RunsNestedEnumerators()
        {
            var steps = new int[1];

            await Outer(steps);

            Assert.AreEqual(3, steps[0]);
        }

        [Test]
        public async Task JobHandle_Await_CompletesFinishedJob()
        {
            await default(JobHandle);
        }

        [Test]
        public async Task IEnumerator_ToUnityTask_RunsNestedEnumerators()
        {
            var steps = new int[1];

            await Outer(steps).ToUnityTask();

            Assert.AreEqual(3, steps[0]);
        }

        [Test]
        public async Task IEnumerator_WithCancellation_StopsBetweenSteps()
        {
            using var cancellation = new CancellationTokenSource();

            cancellation.Cancel();

            try
            {
                await Endless().WithCancellation(cancellation.Token);
                Assert.Fail("Expected cancellation.");
            }
            catch (OperationCanceledException exception)
            {
                Assert.AreEqual(cancellation.Token, exception.CancellationToken);
            }
        }

        [Test]
        public async Task CancelAfterSlim_CancelsSource()
        {
            using var source = new CancellationTokenSource();

            UnityTaskCancellation.CancelAfterSlim(source, TimeSpan.Zero);

            await UnityTask.WaitUntilCanceledAsync(source.Token);

            Assert.IsTrue(source.IsCancellationRequested);
        }

        [Test]
        public async Task CancelAfterSlim_DisposedHandle_DoesNotCancel()
        {
            using var source = new CancellationTokenSource();

            UnityTaskCancellation.CancelAfterSlim(source, TimeSpan.Zero).Dispose();

            await UnityTask.DelayFrameAsync(3);

            Assert.IsFalse(source.IsCancellationRequested);
        }

        [Test]
        public void RegisterRaiseCancelOnDestroy_CancelsWhenDestroyed()
        {
            using var source = new CancellationTokenSource();
            var gameObject = new GameObject("RaiseCancelOnDestroy");

            UnityTaskCancellation.RegisterRaiseCancelOnDestroy(source, gameObject);
            UnityEngine.Object.DestroyImmediate(gameObject);

            Assert.IsTrue(source.IsCancellationRequested);
        }

        [Test]
        public void AddTo_DisposesOnCancel()
        {
            using var source = new CancellationTokenSource();
            var disposable = new Disposable();

            UnityTaskCancellation.AddTo(disposable, source.Token);
            source.Cancel();

            Assert.IsTrue(disposable.disposed);
            Assert.IsTrue(UnityTaskCancellation.IsOperationCanceledException(new OperationCanceledException()));
        }

        [Test]
        public async Task ToCancellationToken_CancelledWhenTaskCompletes()
        {
            var source = new UnityTaskCompletionSource();
            var token = source.Task.ToCancellationToken();

            Assert.IsFalse(token.IsCancellationRequested);

            source.SetResult();

            await UnityTask.WaitUntilCanceledAsync(token, completeImmediately: true);

            Assert.IsTrue(token.IsCancellationRequested);
        }

        [Test]
        public async Task AsyncLazy_AwaitedTwice_RunsFactoryOnce()
        {
            var calls = 0;
            var lazy = UnityTask.Lazy(Create);

            Assert.AreEqual(0, calls);

            var first = lazy.Task;
            var second = lazy.Task;

            Assert.AreEqual(4, await first);
            Assert.AreEqual(4, await second);
            Assert.AreEqual(4, await lazy);
            Assert.AreEqual(1, calls);

            async UnityTask<int> Create()
            {
                calls++;
                await UnityTask.Yield();
                return 4;
            }
        }

        [Test]
        public async Task ToAsyncLazy_FaultIsSharedByEveryCaller()
        {
            var fault = new InvalidOperationException("lazy");
            var lazy = UnityTask.FromException(fault).ToAsyncLazy();

            Assert.AreSame(fault, await CaptureAsync(lazy.Task));
            Assert.AreSame(fault, await CaptureAsync(lazy.Task));
        }

        [Test]
        public async Task TimeoutController_TimeoutCancelsToken()
        {
            using var controller = new EncosyTower.Tasks.TimeoutController();
            var token = controller.Timeout(TimeSpan.Zero);

            await UnityTask.WaitUntilCanceledAsync(token);

            Assert.IsTrue(controller.IsTimeout());
        }

        [Test]
        public async Task TimeoutController_Reset_KeepsTokenUsable()
        {
            using var controller = new EncosyTower.Tasks.TimeoutController();
            var token = controller.Timeout(TimeSpan.FromSeconds(30));

            controller.Reset();

            await UnityTask.DelayFrameAsync(2);

            Assert.IsFalse(token.IsCancellationRequested);
            Assert.IsFalse(controller.IsTimeout());
        }

        [Test]
        public async Task PlayerLoopTimer_Periodic_FiresRepeatedly()
        {
            var fired = new int[1];
            var timer = EncosyTower.Tasks.PlayerLoopTimer.StartNew(
                  interval: TimeSpan.Zero
                , periodic: true
                , delayType: UnityTaskDelayType.Realtime
                , timing: UnityTaskTiming.Update
                , token: CancellationToken.None
                , timerCallback: static state => ((int[])state)[0]++
                , state: fired
            );

            await UnityTask.WaitUntil(fired, static state => state[0] >= 3);

            timer.Dispose();

            Assert.GreaterOrEqual(fired[0], 3);
        }

        private static IEnumerator Outer(int[] steps)
        {
            steps[0]++;
            yield return null;
            yield return Inner(steps);
            steps[0]++;
        }

        private static IEnumerator Inner(int[] steps)
        {
            yield return null;
            steps[0]++;
        }

        private static IEnumerator Endless()
        {
            while (true)
            {
                yield return null;
            }
        }

        private static async Task<Exception> CaptureAsync(UnityTask task)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                return exception;
            }

            Assert.Fail("Expected the task to throw.");
            return null;
        }

        private sealed class Disposable : IDisposable
        {
            public bool disposed;

            public void Dispose()
            {
                disposed = true;
            }
        }
    }
}
