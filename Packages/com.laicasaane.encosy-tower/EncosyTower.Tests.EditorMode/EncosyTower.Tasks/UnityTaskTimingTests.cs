using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Tasks
{
    public class UnityTaskTimingTests
    {
        [Test]
        public async Task NextFrameAsync_WithTiming_Completes()
        {
            var task = UnityTask.NextFrameAsync(UnityTaskTiming.LastUpdate);

            Assert.IsFalse(task.IsCompleted);
            await task;
        }

        [Test]
        public async Task NextFrameAsync_PreCanceledToken_ThrowsWithToken()
        {
            using var cancellation = new CancellationTokenSource();

            cancellation.Cancel();

            var task = UnityTask.NextFrameAsync(UnityTaskTiming.Update, token: cancellation.Token);
            var exception = await CaptureExceptionAsync(task);

            Assert.IsInstanceOf<OperationCanceledException>(exception);
            Assert.AreEqual(cancellation.Token, ((OperationCanceledException)exception).CancellationToken);
        }

        [Test]
        public async Task DelayFrameAsync_Completes()
        {
            var task = UnityTask.DelayFrameAsync(2);

            Assert.IsFalse(task.IsCompleted);
            await task;
            await UnityTask.DelayFrameAsync(0);
        }

        [Test]
        public void DelayFrameAsync_NegativeCount_Throws()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => UnityTask.DelayFrameAsync(-1));

            Assert.AreEqual("delayFrameCount", exception.ParamName);
        }

        [Test]
        public async Task WaitForFixedUpdateAndSeconds_Complete()
        {
            await UnityTask.WaitForFixedUpdateAsync();
            await UnityTask.WaitForSecondsAsync(0f);
            await UnityTask.WaitForSecondsAsync(0, ignoreTimeScale: true);
        }

        [Test]
        public async Task WaitForEndOfFrameAsync_PreCanceledToken_Throws()
        {
            using var cancellation = new CancellationTokenSource();

            cancellation.Cancel();

            var exception = await CaptureExceptionAsync(UnityTask.WaitForEndOfFrameAsync(cancellation.Token));

            Assert.IsInstanceOf<OperationCanceledException>(exception);
        }

        [Test]
        public async Task WaitWhile_PredicateFalse_CompletesSynchronously()
        {
            var task = UnityTask.WaitWhile(static () => false);

            Assert.IsTrue(task.IsCompleted);
            await task;
        }

        [Test]
        public async Task WaitUntil_WithTiming_CompletesAfterPredicateBecomesTrue()
        {
            var holder = new Holder();
            var task = UnityTask.WaitUntil(holder, static state => state.value > 0, UnityTaskTiming.LastUpdate);

            Assert.IsFalse(task.IsCompleted);

            holder.value = 1;

            await task;
        }

        [Test]
        public async Task WaitUntilCanceledAsync_CompleteImmediately_CompletesOnCancel()
        {
            using var cancellation = new CancellationTokenSource();
            var task = UnityTask.WaitUntilCanceledAsync(cancellation.Token, completeImmediately: true);

            Assert.IsFalse(task.IsCompleted);

            cancellation.Cancel();

            Assert.IsTrue(task.IsCompleted);
            await task;
        }

        [Test]
        public async Task WaitUntilCanceledAsync_Polling_CompletesAfterCancel()
        {
            using var cancellation = new CancellationTokenSource();
            var task = UnityTask.WaitUntilCanceledAsync(cancellation.Token);

            Assert.IsFalse(task.IsCompleted);

            cancellation.Cancel();

            await task;
        }

        [Test]
        public async Task WaitUntilValueChangedAsync_ReturnsNewValue()
        {
            var holder = new Holder();
            var task = UnityTask.WaitUntilValueChangedAsync(holder, static state => state.value);

            holder.value = 5;

            Assert.AreEqual(5, await task);
        }

        [Test]
        public async Task WaitUntilValueChangedAsync_DestroyedTarget_Cancels()
        {
            var gameObject = new GameObject("WaitUntilValueChanged");
            var task = UnityTask.WaitUntilValueChangedAsync(gameObject, static state => state.name);

            UnityEngine.Object.DestroyImmediate(gameObject);

            var exception = await CaptureExceptionAsync(task);

            Assert.IsInstanceOf<OperationCanceledException>(exception);
        }

        [Test]
        public async Task UnityTaskVoid_FaultLogsOnce_CancellationSilent()
        {
            var faulted = new TaskCompletionSource<object>();
            var canceled = new TaskCompletionSource<object>();

            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: void failure"));

            FaultAfterYieldAsync(faulted).Forget();
            CancelAfterYieldAsync(canceled).Forget();

            await faulted.Task;
            await canceled.Task;
            await Task.Yield();

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PlayModeTransition_RunsQueuedOnceAndDropsRequeued()
        {
            var counts = new int[2];

            UnityTask.Post(First, UnityTaskTiming.LastUpdate);
            PlayerLoopScheduler.FlushAndClear();

            Assert.AreEqual(1, counts[0]);
            Assert.AreEqual(0, counts[1]);

            await UnityTask.DelayFrameAsync(3);

            Assert.AreEqual(0, counts[1]);

            void First()
            {
                counts[0]++;
                UnityTask.Post(Second, UnityTaskTiming.LastUpdate);
            }

            void Second()
                => counts[1]++;
        }

        [Test]
        public void FireAndForgetFactories_RunTheMethod()
        {
            var count = 0;

            UnityTask.Void(Increment);
            UnityTask.Action(Increment)();
            UnityTask.UnityAction(Increment)();
            UnityTask.UnityAction<int>(IncrementBy)(2);

            Assert.AreEqual(5, count);

            async UnityTaskVoid Increment()
            {
                await UnityTask.CompletedTask;
                count++;
            }

            async UnityTaskVoid IncrementBy(int amount)
            {
                await UnityTask.CompletedTask;
                count += amount;
            }
        }

        private static async UnityTaskVoid FaultAfterYieldAsync(TaskCompletionSource<object> reached)
        {
            await UnityTask.Yield();
            reached.SetResult(null);
            throw new InvalidOperationException("void failure");
        }

        private static async UnityTaskVoid CancelAfterYieldAsync(TaskCompletionSource<object> reached)
        {
            await UnityTask.Yield();
            reached.SetResult(null);
            throw new OperationCanceledException();
        }

        private static async Task<Exception> CaptureExceptionAsync(UnityTask task)
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

        private static async Task<Exception> CaptureExceptionAsync<T>(UnityTask<T> task)
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

        private sealed class Holder
        {
            public int value;
        }
    }
}
