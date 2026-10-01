#if UNITY_EDITOR

using System;
using System.Threading.Tasks;
using EncosyTower.Databases.Settings;
using NUnit.Framework;

namespace EncosyTower.Tests.Databases.Settings
{
    public sealed class ConversionTaskTests
    {
        [Test]
        public void Update_IncompleteTaskKeepsSubscriptionAndProgress()
        {
            var completion = new TaskCompletionSource<bool>();
            var calls = new CallCounts();
            var conversion = CreateConversionTask(completion.Task, calls);

            conversion.Update();

            AssertCounts(calls, unsubscribe: 0, removeProgress: 0, logException: 0, refresh: 0);
        }

        [TestCase(false, 0)]
        [TestCase(true, 1)]
        public void Update_SuccessCleansUpOnceAndRefreshesOnlyForTrue(bool result, int expectedRefreshes)
        {
            var calls = new CallCounts();
            var conversion = CreateConversionTask(Task.FromResult(result), calls);

            Assert.DoesNotThrow(conversion.Update);
            Assert.DoesNotThrow(conversion.Update);

            AssertCounts(calls, unsubscribe: 1, removeProgress: 1, logException: 0, refresh: expectedRefreshes);
        }

        [Test]
        public void Update_CanceledTaskCleansUpWithoutReadingResult()
        {
            var calls = new CallCounts();
            var conversion = CreateConversionTask(Task.FromCanceled<bool>(new(true)), calls);

            Assert.DoesNotThrow(conversion.Update);
            Assert.DoesNotThrow(conversion.Update);

            AssertCounts(calls, unsubscribe: 1, removeProgress: 1, logException: 0, refresh: 0);
        }

        [Test]
        public void Update_FaultedTaskLogsOnceAndDoesNotReadResult()
        {
            var calls = new CallCounts();
            var task = Task.FromException<bool>(new InvalidOperationException("failure"));
            var conversion = CreateConversionTask(task, calls);

            Assert.DoesNotThrow(conversion.Update);
            Assert.DoesNotThrow(conversion.Update);

            AssertCounts(calls, unsubscribe: 1, removeProgress: 1, logException: 1, refresh: 0);
            Assert.That(calls.Exception, Is.TypeOf<AggregateException>());
            Assert.That(calls.Exception.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        private static ConversionTask CreateConversionTask(Task<bool> task, CallCounts calls)
            => new(
                  task
                , () => calls.Unsubscribe++
                , () => calls.RemoveProgress++
                , exception =>
                {
                    calls.LogException++;
                    calls.Exception = exception;
                }
                , () => calls.Refresh++
            );

        private static void AssertCounts(
              CallCounts calls
            , int unsubscribe
            , int removeProgress
            , int logException
            , int refresh
        )
        {
            Assert.That(calls.Unsubscribe, Is.EqualTo(unsubscribe));
            Assert.That(calls.RemoveProgress, Is.EqualTo(removeProgress));
            Assert.That(calls.LogException, Is.EqualTo(logException));
            Assert.That(calls.Refresh, Is.EqualTo(refresh));
        }

        private sealed class CallCounts
        {
            public int Unsubscribe;
            public int RemoveProgress;
            public int LogException;
            public int Refresh;
            public Exception Exception;
        }
    }
}

#endif
