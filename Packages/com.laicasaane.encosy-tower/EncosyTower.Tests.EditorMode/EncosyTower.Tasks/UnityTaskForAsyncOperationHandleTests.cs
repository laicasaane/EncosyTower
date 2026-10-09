#if UNITY_ADDRESSABLES

using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.Tests.Tasks
{
    public class UnityTaskForAsyncOperationHandleTests
    {
        private ResourceManager _resourceManager;

        [SetUp]
        public void SetUp()
        {
            _resourceManager = new ResourceManager();
        }

        [TearDown]
        public void TearDown()
        {
            _resourceManager.Dispose();
        }

        [Test]
        public async Task CompletedHandle_ReturnsResult()
        {
            var handle = _resourceManager.CreateCompletedOperation(5, errorMsg: null);

            Assert.AreEqual(5, await handle.ToUnityTask());

            handle.Release();
        }

        [Test]
        public async Task PendingHandle_ReturnsResultAfterCompletion()
        {
            var operation = new PendingOperation();
            var handle = _resourceManager.StartOperation(operation, dependency: default);
            var task = handle.ToUnityTask();

            await UnityTask.NextFrameAsync();
            Assert.AreEqual(UnityTaskStatus.Pending, task.Status);

            operation.Complete(7, success: true, errorMsg: null);

            Assert.AreEqual(7, await task);

            handle.Release();
        }

        [Test]
        public async Task FailedHandle_ThrowsOperationException()
        {
            var expected = new InvalidOperationException("Failed on purpose.");
            var exceptionHandler = ResourceManager.ExceptionHandler;
            AsyncOperationHandle<int> handle;

            ResourceManager.ExceptionHandler = null;

            try
            {
                handle = _resourceManager.CreateCompletedOperationWithException(0, expected);
            }
            finally
            {
                ResourceManager.ExceptionHandler = exceptionHandler;
            }

            var typed = await CaptureAsync(async () => await handle.ToUnityTask());
            var untyped = await CaptureAsync(async () => await ((AsyncOperationHandle)handle).ToUnityTask());

            Assert.AreSame(expected, typed);
            Assert.AreSame(expected, untyped);

            handle.Release();
        }

        [Test]
        public void InvalidTypedHandle_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => default(AsyncOperationHandle<int>).ToUnityTask());
        }

        [Test]
        public async Task InvalidHandle_Completes()
        {
            await default(AsyncOperationHandle).ToUnityTask();
        }

        [Test]
        public async Task Cancellation_KeepsHandle()
        {
            using var cancellation = new CancellationTokenSource();
            var operation = new PendingOperation();
            var handle = _resourceManager.StartOperation(operation, dependency: default);
            var task = handle.WithCancellation(cancelImmediately: true, cancellation.Token);

            cancellation.Cancel();

            var exception = await CaptureAsync(async () => await task);

            Assert.IsInstanceOf<OperationCanceledException>(exception);

            operation.Complete(0, success: true, errorMsg: null);

            Assert.IsTrue(handle.IsValid());

            handle.Release();
        }

        [Test]
        public async Task CancellationWithAutoRelease_ReleasesHandle()
        {
            using var cancellation = new CancellationTokenSource();
            var operation = new PendingOperation();
            var handle = _resourceManager.StartOperation(operation, dependency: default);

            var task = handle.WithCancellation(
                  cancelImmediately: true
                , autoReleaseWhenCanceled: true
                , cancellation.Token
            );

            cancellation.Cancel();

            var exception = await CaptureAsync(async () => await task);

            Assert.IsInstanceOf<OperationCanceledException>(exception);

            operation.Complete(0, success: true, errorMsg: null);

            Assert.IsFalse(handle.IsValid());
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

        private sealed class PendingOperation : AsyncOperationBase<int>
        {
            protected override void Execute()
            {
            }
        }
    }
}

#endif
