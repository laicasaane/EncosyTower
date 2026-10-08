using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Tasks;
using NUnit.Framework;

namespace EncosyTower.Tests.Tasks
{
    public class UnityTaskFactoryTests
    {
        [Test]
        public async Task CreateAsync_CallsFactoryImmediately()
        {
            var calls = 0;
            var task = UnityTask.CreateAsync(Create);

            Assert.AreEqual(1, calls);
            Assert.AreEqual(7, await UnityTask.CreateAsync(static () => UnityTask.FromResult(7)));
            await task;

            UnityTask Create()
            {
                calls++;
                return UnityTask.CompletedTask;
            }
        }

        [Test]
        public async Task DeferAsync_CallsFactoryOnFirstAwait()
        {
            var calls = 0;
            var task = UnityTask.DeferAsync(Create);

            Assert.AreEqual(0, calls);
            Assert.AreEqual(3, await task);
            Assert.AreEqual(1, calls);

            UnityTask<int> Create()
            {
                calls++;
                return UnityTask.FromResult(3);
            }
        }

        [Test]
        public async Task DeferAsync_WithState_AwaitsSuspendedTask()
        {
            var source = new UnityTaskCompletionSource();
            var task = UnityTask.DeferAsync(source, static state => state.Task);

            Assert.IsFalse(task.IsCompleted);

            source.SetResult();

            await task;
        }

        [Test]
        public async Task NeverAsync_CompletesCanceledWhenTokenIsCancelled()
        {
            using var cancellation = new CancellationTokenSource();
            var task = UnityTask.NeverAsync<int>(cancellation.Token);

            Assert.IsFalse(task.IsCompleted);

            cancellation.Cancel();

            Assert.AreEqual(UnityTaskStatus.Canceled, task.Status);

            try
            {
                await task;
                Assert.Fail("Expected cancellation.");
            }
            catch (OperationCanceledException exception)
            {
                Assert.AreEqual(cancellation.Token, exception.CancellationToken);
            }
        }

        [Test]
        public void NeverAsync_UncancellableToken_NeverCompletes()
        {
            Assert.IsFalse(UnityTask.NeverAsync(CancellationToken.None).IsCompleted);
        }

        [Test]
        public async Task SwitchToThreadPoolThenMainThread_ReturnsToMainThread()
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var (workerThreadId, resumedThreadId) = await SwitchAndReturnAsync();

            Assert.AreNotEqual(mainThreadId, workerThreadId);
            Assert.AreEqual(mainThreadId, resumedThreadId);
        }

        [Test]
        public async Task ReturnToMainThread_AwaitUsing_EndsOnMainThread()
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var (workerThreadId, resumedThreadId) = await ReturnToMainThreadAsync();

            Assert.AreNotEqual(mainThreadId, workerThreadId);
            Assert.AreEqual(mainThreadId, resumedThreadId);
        }

        [Test]
        public async Task Post_RunsActionOnScheduler()
        {
            var ran = new TaskCompletionSource<int>();

            UnityTask.Post(Run);

            Assert.IsFalse(ran.Task.IsCompleted);
            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, await ran.Task);

            void Run()
                => ran.SetResult(Thread.CurrentThread.ManagedThreadId);
        }

        private static async UnityTask<(int worker, int resumed)> SwitchAndReturnAsync()
        {
            await UnityTask.SwitchToThreadPoolAsync();

            var workerThreadId = Thread.CurrentThread.ManagedThreadId;

            await UnityTask.SwitchToMainThreadAsync();

            return (workerThreadId, Thread.CurrentThread.ManagedThreadId);
        }

        private static async UnityTask<(int worker, int resumed)> ReturnToMainThreadAsync()
        {
            int workerThreadId;

            await using (UnityTask.ReturnToMainThread())
            {
                await UnityTask.SwitchToThreadPoolAsync();
                workerThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            return (workerThreadId, Thread.CurrentThread.ManagedThreadId);
        }
    }
}
