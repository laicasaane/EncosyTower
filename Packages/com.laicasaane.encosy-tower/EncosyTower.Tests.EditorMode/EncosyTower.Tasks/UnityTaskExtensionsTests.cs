using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Tasks;
using NUnit.Framework;

namespace EncosyTower.Tests.Tasks
{
    public class UnityTaskExtensionsTests
    {
        [Test]
        public async Task SuppressCancellationThrowAsync_ReportsCancellation()
        {
            Assert.IsTrue(await UnityTask.FromCanceled().SuppressCancellationThrowAsync());
            Assert.IsFalse(await UnityTask.CompletedTask.SuppressCancellationThrowAsync());

            var (isCanceled, result) = await UnityTask.FromResult(4).SuppressCancellationThrowAsync();

            Assert.IsFalse(isCanceled);
            Assert.AreEqual(4, result);
            Assert.IsTrue((await UnityTask.FromCanceled<int>().SuppressCancellationThrowAsync()).isCanceled);
        }

        [Test]
        public async Task Forget_WithHandler_ReceivesFault()
        {
            var fault = new InvalidOperationException("handled");
            var received = new TaskCompletionSource<Exception>();

            UnityTask.FromException(fault).Forget(Handle);

            Assert.AreSame(fault, await received.Task);

            void Handle(Exception exception)
                => received.SetResult(exception);
        }

        [Test]
        public async Task AttachExternalCancellationAsync_CancelsPendingWait()
        {
            using var cancellation = new CancellationTokenSource();
            var source = new UnityTaskCompletionSource<int>();
            var task = source.Task.AttachExternalCancellationAsync(cancellation.Token);

            cancellation.Cancel();

            try
            {
                await task;
                Assert.Fail("Expected cancellation.");
            }
            catch (OperationCanceledException exception)
            {
                Assert.AreEqual(cancellation.Token, exception.CancellationToken);
            }

            source.SetResult(1);
        }

        [Test]
        public async Task AttachExternalCancellationAsync_TaskWins_ReturnsResult()
        {
            using var cancellation = new CancellationTokenSource();

            Assert.AreEqual(8, await UnityTask.FromResult(8).AttachExternalCancellationAsync(cancellation.Token));
        }

        [Test]
        public async Task TimeoutAsync_PendingTask_ThrowsTimeoutException()
        {
            var source = new UnityTaskCompletionSource();
            var exception = await CaptureAsync(async () => await source.Task.TimeoutAsync(TimeSpan.Zero));

            Assert.IsInstanceOf<TimeoutException>(exception);

            source.SetResult();
        }

        [Test]
        public async Task TimeoutWithoutExceptionAsync_CompletedTask_ReturnsResult()
        {
            var timeout = TimeSpan.FromSeconds(10);
            var (isTimeout, result) = await UnityTask.FromResult(6).TimeoutWithoutExceptionAsync(timeout);

            Assert.IsFalse(isTimeout);
            Assert.AreEqual(6, result);
        }

        [Test]
        public async Task UnwrapAsync_AwaitsInnerTask()
        {
            Assert.AreEqual(9, await UnityTask.FromResult(UnityTask.FromResult(9)).UnwrapAsync());
            Assert.AreEqual(2, await Task.FromResult(UnityTask.FromResult(2)).UnwrapAsync());
            Assert.AreEqual(3, await UnityTask.FromResult(Task.FromResult(3)).UnwrapAsync());
        }

        [Test]
        public async Task AsUnityTask_FromFaultedTask_RethrowsInnerException()
        {
            var fault = new InvalidOperationException("inner");

            try
            {
                await Task.FromException(fault).AsUnityTask();
                Assert.Fail("Expected the fault.");
            }
            catch (InvalidOperationException exception)
            {
                Assert.AreSame(fault, exception);
            }

            Assert.AreEqual(5, await Task.FromResult(5).AsUnityTask());
            Assert.AreEqual(7, await new ValueTask<int>(7).AsUnityTask());
        }

        [Test]
        public async Task AsTask_RoundTripsResultAndCancellation()
        {
            Assert.AreEqual(11, await UnityTask.FromResult(11).AsTask());
            Assert.AreEqual(12, await UnityTask.FromResult(12).AsValueTask());

            var canceled = UnityTask.FromCanceled().AsTask();
            var exception = await CaptureAsync(() => canceled);

            Assert.IsInstanceOf<OperationCanceledException>(exception);
            Assert.IsTrue(canceled.IsCanceled);
        }

        [Test]
        public void ToCoroutine_CompletesAndRethrowsWithoutHandler()
        {
            var results = 0;
            IEnumerator coroutine = UnityTask.FromResult(3).ToCoroutine(Add);

            Assert.IsFalse(coroutine.MoveNext());
            Assert.AreEqual(3, results);

            var faulted = UnityTask.FromException(new InvalidOperationException("coroutine")).ToCoroutine();

            Assert.Throws<InvalidOperationException>(() => faulted.MoveNext());

            void Add(int value)
                => results += value;
        }

        [Test]
        public async Task TupleAndArrayAwait_ReturnResults()
        {
            var (first, second) = await (UnityTask.FromResult(1), UnityTask.FromResult("two"));

            Assert.AreEqual(1, first);
            Assert.AreEqual("two", second);

            var values = await new[] { UnityTask.FromResult(3), UnityTask.FromResult(4) };

            CollectionAssert.AreEqual(new[] { 3, 4 }, values);

            await (UnityTask.CompletedTask, UnityTask.CompletedTask);
        }

        private static async Task<Exception> CaptureAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception exception)
            {
                return exception;
            }

            Assert.Fail("Expected an exception.");
            return null;
        }
    }
}
