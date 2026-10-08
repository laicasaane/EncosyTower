using System;
using System.Threading.Tasks;
using EncosyTower.Tasks;
using NUnit.Framework;

namespace EncosyTower.Tests.Tasks
{
    public class UnityTaskStatusTests
    {
        [Test]
        public async Task Status_ReportsEachState()
        {
            var source = new UnityTaskCompletionSource<int>();
            var pending = source.Task;

            Assert.AreEqual(UnityTaskStatus.Pending, pending.Status);

            source.SetResult(1);

            Assert.AreEqual(UnityTaskStatus.Succeeded, pending.Status);
            Assert.AreEqual(1, await pending);
            Assert.AreEqual(UnityTaskStatus.Succeeded, default(UnityTask).Status);
            Assert.AreEqual(UnityTaskStatus.Succeeded, UnityTask.FromResult(2).Status);
            Assert.AreEqual(UnityTaskStatus.Faulted, UnityTask.FromException(new InvalidOperationException()).Status);
            Assert.AreEqual(UnityTaskStatus.Canceled, UnityTask.FromCanceled<int>().Status);
        }

        [Test]
        public async Task Preserve_AwaitedTwice_ReturnsSameResult()
        {
            var source = new UnityTaskCompletionSource<int>();
            var preserved = source.Task.Preserve();

            source.SetResult(5);

            Assert.AreEqual(5, await preserved);
            Assert.AreEqual(5, await preserved);
            Assert.AreEqual(UnityTaskStatus.Succeeded, preserved.Status);
        }

        [Test]
        public async Task Preserve_AwaitedTwice_RethrowsSameException()
        {
            var fault = new InvalidOperationException("preserved");
            var source = new UnityTaskCompletionSource();
            var preserved = source.Task.Preserve();

            source.SetException(fault);

            Assert.AreSame(fault, await CaptureExceptionAsync(preserved));
            Assert.AreSame(fault, await CaptureExceptionAsync(preserved));
            Assert.AreEqual(UnityTaskStatus.Faulted, preserved.Status);
        }

        [Test]
        public void CompletionSource_UnsafeGetStatus_ReflectsCompletion()
        {
            var source = new UnityTaskCompletionSource();
            var genericSource = new UnityTaskCompletionSource<int>();

            Assert.AreEqual(UnityTaskStatus.Pending, source.UnsafeGetStatus());
            Assert.AreEqual(UnityTaskStatus.Pending, genericSource.UnsafeGetStatus());

            source.TrySetCanceled();
            genericSource.TrySetResult(3);

            Assert.AreEqual(UnityTaskStatus.Canceled, source.UnsafeGetStatus());
            Assert.AreEqual(UnityTaskStatus.Succeeded, genericSource.UnsafeGetStatus());
        }

        [Test]
        public void ToString_ReportsStatus()
        {
            var source = new UnityTaskCompletionSource();
            var task = source.Task;

            Assert.AreEqual("(Pending)", task.ToString());

            source.SetResult();

            Assert.AreEqual("(Succeeded)", task.ToString());
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
    }
}
