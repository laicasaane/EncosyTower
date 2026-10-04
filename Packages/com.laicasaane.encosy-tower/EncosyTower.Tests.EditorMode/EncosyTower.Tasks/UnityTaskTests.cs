using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
using Cysharp.Threading.Tasks;
#else
using EncosyTower.UnityExtensions;
#endif

namespace EncosyTower.Tests.Tasks
{
    public class UnityTaskTests
    {
        private static bool s_allocationBoolSink;
        private static int s_allocationIntSink;

        [Test]
        public async Task DefaultUnityTask_CompletesSuccessfully()
        {
            var task = default(UnityTask);

            Assert.IsTrue(task.IsCompleted);
            await task;
        }

        [Test]
        public async Task DefaultUnityTaskOfT_CompletesWithDefaultResult()
        {
            var task = default(UnityTask<int>);

            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(0, await task);
        }

        [Test]
        public async Task AsyncUnityTask_CompletesSynchronously()
        {
            var task = CompleteSynchronouslyAsync();

            Assert.IsTrue(task.IsCompleted);
            await task;
        }

        [Test]
        public async Task AsyncUnityTaskOfT_CompletesSynchronously()
        {
            var task = ReturnSynchronouslyAsync(42);

            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(42, await task);
        }

        [Test]
        public async Task AsyncUnityTask_SuspendsAndCompletes()
        {
            var completion = new TaskCompletionSource<object>();
            var task = AwaitAsync(completion.Task);

            Assert.IsFalse(task.IsCompleted);
            completion.SetResult(null);
            await task;
        }

        [Test]
        public async Task AsyncUnityTaskOfT_SuspendsAndReturnsResult()
        {
            var completion = new TaskCompletionSource<int>();
            var task = AwaitAndReturnAsync(completion.Task);

            Assert.IsFalse(task.IsCompleted);
            completion.SetResult(42);
            Assert.AreEqual(42, await task);
        }

        [Test]
        public async Task AsyncLambdasAndLocalFunctions_UseBothBuilders()
        {
            Func<UnityTask> action = async () => await Task.CompletedTask;
            Func<UnityTask<int>> function = async () => await Task.FromResult(42);

            await action();
            Assert.AreEqual(42, await function());
            await LocalActionAsync();
            Assert.AreEqual(84, await LocalFunctionAsync());

            static async UnityTask LocalActionAsync()
                => await Task.CompletedTask;

            static async UnityTask<int> LocalFunctionAsync()
                => await Task.FromResult(84);
        }

        [Test]
        public async Task ExceptionsBeforeAndAfterSuspension_Propagate()
        {
            var before = ThrowBeforeSuspensionAsync("before");
            var completion = new TaskCompletionSource<object>();
            var after = ThrowAfterSuspensionAsync(completion.Task, "after");

            var beforeException = await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => await before
            );
            Assert.AreEqual("before", beforeException.Message);
            Assert.IsFalse(after.IsCompleted);

            completion.SetResult(null);
            var afterException = await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => await after
            );
            Assert.AreEqual("after", afterException.Message);
        }

        [Test]
        public async Task GenericExceptionsBeforeAndAfterSuspension_Propagate()
        {
            var before = ThrowGenericBeforeSuspensionAsync("before generic");
            var completion = new TaskCompletionSource<object>();
            var after = ThrowGenericAfterSuspensionAsync(completion.Task, "after generic");

            var beforeException = await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => _ = await before
            );
            Assert.AreEqual("before generic", beforeException.Message);
            Assert.IsFalse(after.IsCompleted);

            completion.SetResult(null);
            var afterException = await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => _ = await after
            );
            Assert.AreEqual("after generic", afterException.Message);
        }

        [Test]
        public async Task CancellationBeforeAndAfterSuspension_RethrowsSameInstance()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            foreach (var expected in CreateCancellations(cancellation.Token))
            {
                var completion = new TaskCompletionSource<object>();
                var before = ThrowInstanceBeforeSuspensionAsync(expected.before);
                var after = ThrowInstanceAfterSuspensionAsync(completion.Task, expected.after);

                var beforeException = await CaptureUnityTaskExceptionAsync(before);
                Assert.IsFalse(after.IsCompleted);

                completion.SetResult(null);
                var afterException = await CaptureUnityTaskExceptionAsync(after);

                AssertSameCancellation(expected.before, beforeException);
                AssertSameCancellation(expected.after, afterException);
            }
        }

        [Test]
        public async Task GenericCancellationBeforeAndAfterSuspension_RethrowsSameInstance()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            foreach (var expected in CreateCancellations(cancellation.Token))
            {
                var completion = new TaskCompletionSource<object>();
                var before = ThrowGenericInstanceBeforeSuspensionAsync(expected.before);
                var after = ThrowGenericInstanceAfterSuspensionAsync(completion.Task, expected.after);

                var beforeException = await CaptureUnityTaskExceptionAsync(before);
                Assert.IsFalse(after.IsCompleted);

                completion.SetResult(null);
                var afterException = await CaptureUnityTaskExceptionAsync(after);

                AssertSameCancellation(expected.before, beforeException);
                AssertSameCancellation(expected.after, afterException);
            }
        }

        [Test]
        public async Task FaultBeforeAndAfterSuspension_RethrowsSameInstanceAndStack()
        {
            var beforeFault = new InvalidOperationException("before");
            var afterFault = new InvalidOperationException("after");
            var completion = new TaskCompletionSource<object>();
            var before = ThrowInstanceBeforeSuspensionAsync(beforeFault);
            var after = ThrowInstanceAfterSuspensionAsync(completion.Task, afterFault);

            var beforeException = await CaptureUnityTaskExceptionAsync(before);
            Assert.IsFalse(after.IsCompleted);

            completion.SetResult(null);
            var afterException = await CaptureUnityTaskExceptionAsync(after);

            Assert.That(beforeException, Is.SameAs(beforeFault));
            Assert.That(afterException, Is.SameAs(afterFault));
            StringAssert.Contains(nameof(ThrowInstanceBeforeSuspensionAsync), beforeException.StackTrace);
            StringAssert.Contains(nameof(ThrowInstanceAfterSuspensionAsync), afterException.StackTrace);
        }

        [Test]
        public async Task GenericFaultBeforeAndAfterSuspension_RethrowsSameInstanceAndStack()
        {
            var beforeFault = new InvalidOperationException("before generic");
            var afterFault = new InvalidOperationException("after generic");
            var completion = new TaskCompletionSource<object>();
            var before = ThrowGenericInstanceBeforeSuspensionAsync(beforeFault);
            var after = ThrowGenericInstanceAfterSuspensionAsync(completion.Task, afterFault);

            var beforeException = await CaptureUnityTaskExceptionAsync(before);
            Assert.IsFalse(after.IsCompleted);

            completion.SetResult(null);
            var afterException = await CaptureUnityTaskExceptionAsync(after);

            Assert.That(beforeException, Is.SameAs(beforeFault));
            Assert.That(afterException, Is.SameAs(afterFault));
            StringAssert.Contains(nameof(ThrowGenericInstanceBeforeSuspensionAsync), beforeException.StackTrace);
            StringAssert.Contains(nameof(ThrowGenericInstanceAfterSuspensionAsync), afterException.StackTrace);
        }

        [Test]
        public async Task OnCompletedAndUnsafeOnCompleted_InvokeExactlyOnce()
        {
            await AssertContinuationInvokedOnceAsync(unsafeContinuation: false);
            await AssertContinuationInvokedOnceAsync(unsafeContinuation: true);
        }

        [Test]
        public async Task SelectedNativeBridge_PreservesIdentityAndResult()
        {
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
            var native = UniTask.CompletedTask;
            var genericNative = UniTask.FromResult(42);
            UnityTask wrapper = native;
            UnityTask<int> genericWrapper = genericNative;

            Assert.AreEqual(native, wrapper.AsUniTask());
            Assert.AreEqual(genericNative, genericWrapper.AsUniTask());
            await wrapper.AsUniTask();
            Assert.AreEqual(42, await genericWrapper.AsUniTask());
#else
            var source = new AwaitableCompletionSource();
            var genericSource = new AwaitableCompletionSource<int>();
            source.SetResult();
            genericSource.SetResult(42);
            var native = source.Awaitable;
            var genericNative = genericSource.Awaitable;
            UnityTask wrapper = native;
            UnityTask<int> genericWrapper = genericNative;

            Assert.AreSame(native, wrapper.AsAwaitable());
            Assert.AreSame(genericNative, genericWrapper.AsAwaitable());
            await wrapper.AsAwaitable();
            Assert.AreEqual(42, await genericWrapper.AsAwaitable());
#endif
        }

        [Test]
        public async Task SelectedNativeBridge_PreservesFaultAndCancellation()
        {
            var faultWrapper = AwaitAsync(Task.FromException(new InvalidOperationException("native fault")));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var canceledWrapper = AwaitAsync(Task.FromCanceled(cancellation.Token));

#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
            UnityTask faultRoundTrip = faultWrapper.AsUniTask();
            UnityTask canceledRoundTrip = canceledWrapper.AsUniTask();
#else
            UnityTask faultRoundTrip = faultWrapper.AsAwaitable();
            UnityTask canceledRoundTrip = canceledWrapper.AsAwaitable();
#endif

            var exception = await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => await faultRoundTrip
            );
            Assert.AreEqual("native fault", exception.Message);
            await CaptureExpectedExceptionAsync<OperationCanceledException>(
                async () => await canceledRoundTrip
            );
        }

#if !UNITASK || ENCOSY_UNITYTASK_AWAITABLE
        [Test]
        public void NullAwaitableInputs_ThrowWithTaskParameter()
        {
            Awaitable task = null;
            Awaitable<int> genericTask = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => _ = (UnityTask)task
            );
            var genericException = Assert.Throws<ArgumentNullException>(
                () => _ = (UnityTask<int>)genericTask
            );

            Assert.AreEqual("task", exception.ParamName);
            Assert.AreEqual("task", genericException.ParamName);
        }
#endif

        [Test]
        public void SelectedNativeOperations_AddNoManagedAllocation()
        {
#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
            var native = UniTask.CompletedTask;
            var genericNative = UniTask.FromResult(42);

            ConsumeSelectedNative(native, genericNative);
            var before = GC.GetAllocatedBytesForCurrentThread();

            for (var i = 0; i < 1_000; i++)
            {
                ConsumeSelectedNative(native, genericNative);
            }

            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
#else
            var warmupSource = new AwaitableCompletionSource();
            var genericWarmupSource = new AwaitableCompletionSource<int>();
            warmupSource.SetResult();
            genericWarmupSource.SetResult(42);
            ConsumeSelectedNative(warmupSource.Awaitable, genericWarmupSource.Awaitable);

            const int COUNT = 128;
            var nativeTasks = new Awaitable[COUNT];
            var genericNativeTasks = new Awaitable<int>[COUNT];

            for (var i = 0; i < COUNT; i++)
            {
                var source = new AwaitableCompletionSource();
                var genericSource = new AwaitableCompletionSource<int>();
                source.SetResult();
                genericSource.SetResult(42);
                nativeTasks[i] = source.Awaitable;
                genericNativeTasks[i] = genericSource.Awaitable;
            }

            var before = GC.GetAllocatedBytesForCurrentThread();

            for (var i = 0; i < COUNT; i++)
            {
                ConsumeSelectedNative(nativeTasks[i], genericNativeTasks[i]);
            }

            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
#endif

            Assert.AreEqual(0L, allocated);
        }

        [Test]
        public async Task GenericAsUnityTask_DiscardsResult()
        {
            await UnityTask.FromResult(42).AsUnityTask();
        }

        [Test]
        public async Task WhenAll_EmptyOneAndMultipleTasksComplete()
        {
            await UnityTask.WhenAll(Array.Empty<UnityTask>());

            var one = UnityTask.CompletedTask;
            await UnityTask.WhenAll(new[] { one });

            var first = UnityTask.CompletedTask;
            var second = UnityTask.CompletedTask;
            await UnityTask.WhenAll(new[] { first, second });
        }

        [Test]
        public async Task GenericWhenAll_EmptyAndMultipleTasksPreserveOrder()
        {
            var empty = await UnityTask.WhenAll(Array.Empty<UnityTask<int>>());
            var first = UnityTask.FromResult(3);
            var second = UnityTask.FromResult(1);
            var third = UnityTask.FromResult(2);
            var results = await UnityTask.WhenAll(new[] { first, second, third });

            Assert.AreSame(Array.Empty<int>(), empty);
            CollectionAssert.AreEqual(new[] { 3, 1, 2 }, results);
        }

        [Test]
        public async Task WhenAll_CountConsumesOnlyPrefixAndLeavesTailUntouched()
        {
            var tailPendingSource = new TaskCompletionSource<object>();
            var prefix = UnityTask.CompletedTask;
            var tailPending = AwaitAsync(tailPendingSource.Task);
            var tailFault = AwaitAsync(Task.FromException(new InvalidOperationException("tail fault")));
            var tasks = new[] { prefix, tailPending, tailFault };

            await UnityTask.WhenAll(tasks, 1);

            Assert.IsFalse(tailPending.IsCompleted);
            tailPendingSource.SetResult(null);
            await tailPending;

            var tailException = await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => await tailFault
            );
            Assert.AreEqual("tail fault", tailException.Message);
        }

        [Test]
        public async Task WhenAll_CountZeroIgnoresEveryEntry()
        {
            var pendingSource = new TaskCompletionSource<object>();
            var pending = AwaitAsync(pendingSource.Task);

            await UnityTask.WhenAll(new[] { pending }, 0);

            Assert.IsFalse(pending.IsCompleted);
            pendingSource.SetResult(null);
            await pending;
        }

        [Test]
        public async Task WhenAll_CountFullLengthConsumesEveryEntry()
        {
            var first = UnityTask.CompletedTask;
            var second = UnityTask.CompletedTask;

            await UnityTask.WhenAll(new[] { first, second }, 2);
        }

        [TestCase(-1)]
        [TestCase(2)]
        public async Task WhenAll_InvalidCountThrowsWithCountParameter(int count)
        {
            var exception = await CaptureExpectedExceptionAsync<ArgumentOutOfRangeException>(
                async () => await UnityTask.WhenAll(new UnityTask[1], count)
            );

            Assert.AreEqual("count", exception.ParamName);
        }

        [TestCase(-1)]
        [TestCase(2)]
        public async Task GenericWhenAll_InvalidCountThrowsWithCountParameter(int count)
        {
            var exception = await CaptureExpectedExceptionAsync<ArgumentOutOfRangeException>(
                async () => _ = await UnityTask.WhenAll(new UnityTask<int>[1], count)
            );

            Assert.AreEqual("count", exception.ParamName);
        }

        [Test]
        public async Task WhenAll_NullArraysThrowWithTasksParameter()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => UnityTask.WhenAll(null)
            );
            var genericException = Assert.Throws<ArgumentNullException>(
                () => UnityTask.WhenAll((UnityTask<int>[])null)
            );
            var countException = await CaptureExpectedExceptionAsync<ArgumentNullException>(
                async () => await UnityTask.WhenAll(null, 0)
            );
            var genericCountException = await CaptureExpectedExceptionAsync<ArgumentNullException>(
                async () => _ = await UnityTask.WhenAll((UnityTask<int>[])null, 0)
            );

            Assert.AreEqual("tasks", exception.ParamName);
            Assert.AreEqual("tasks", genericException.ParamName);
            Assert.AreEqual("tasks", countException.ParamName);
            Assert.AreEqual("tasks", genericCountException.ParamName);
        }

        [Test]
        public async Task WhenAll_DefaultEntriesCompleteSuccessfully()
        {
            await UnityTask.WhenAll(
                new[] { default(UnityTask), default(UnityTask) }
            );
            var results = await UnityTask.WhenAll(
                new[] { default(UnityTask<int>), default(UnityTask<int>) }
            );

            CollectionAssert.AreEqual(new[] { 0, 0 }, results);
        }

        [Test]
        public async Task WhenAll_OneFaultPreservesSourceStack()
        {
            var fault = AwaitAsync(ThrowWithMarkerAsync());
            var exception = await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => await UnityTask.WhenAll(new[] { fault })
            );

            StringAssert.Contains(nameof(ThrowWithMarkerAsync), exception.StackTrace);
        }

        [Test]
        public async Task WhenAll_PropagatesFirstObservedExceptionWithoutAggregation()
        {
            var firstException = new AggregateException(
                  new InvalidOperationException("first")
                , new AggregateException(new ArgumentException("second"))
            );
            var nested = AwaitAsync(Task.FromException(firstException));
            var other = AwaitAsync(Task.FromException(new ApplicationException("third")));

            var exception = await CaptureExpectedExceptionAsync<AggregateException>(
                async () => await UnityTask.WhenAll(new[] { nested, other })
            );

            Assert.AreSame(firstException, exception);
            Assert.AreEqual(2, exception.InnerExceptions.Count);
            Assert.IsInstanceOf<AggregateException>(exception.InnerExceptions[1]);
        }

        [Test]
        public async Task WhenAll_CancellationOnlyRethrowsFirstCancellation()
        {
            using var firstCancellation = new CancellationTokenSource();
            using var secondCancellation = new CancellationTokenSource();
            firstCancellation.Cancel();
            secondCancellation.Cancel();
            var first = AwaitAsync(Task.FromCanceled(firstCancellation.Token));
            var second = AwaitAsync(Task.FromCanceled(secondCancellation.Token));

            var exception = await CaptureExpectedExceptionAsync<OperationCanceledException>(
                async () => await UnityTask.WhenAll(new[] { first, second })
            );

            Assert.AreEqual(firstCancellation.Token, exception.CancellationToken);
        }

        [Test]
        public async Task WhenAll_FirstFaultCompletesImmediatelyAndConsumesLaterTask()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var laterSource = new TaskCompletionSource<object>();
            var fault = AwaitAsync(Task.FromException(new InvalidOperationException("first fault")));
            var canceled = AwaitAsync(Task.FromCanceled(cancellation.Token));
            var later = AwaitAsync(laterSource.Task);
            var combined = UnityTask.WhenAll(new[] { fault, canceled, later });

            Assert.IsTrue(combined.IsCompleted);

            var exception = await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => await combined
            );

            Assert.AreEqual("first fault", exception.Message);
            laterSource.SetResult(null);
            await Task.Yield();
            Assert.IsTrue(laterSource.Task.IsCompletedSuccessfully);
        }

        [Test]
        public async Task Forget_CompletedAndSuspendedSuccessProduceNoLog()
        {
            default(UnityTask).Forget();
            var completion = new TaskCompletionSource<object>();
            var suspended = AwaitAsync(completion.Task);

            suspended.Forget();
            completion.SetResult(null);
            await Task.Yield();

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Forget_CancellationProducesNoLog()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var task = AwaitAsync(Task.FromCanceled(cancellation.Token));

            task.Forget();

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task Forget_FaultProducesOneExpectedLog()
        {
            var task = AwaitAsync(Task.FromException(new InvalidOperationException("forget failure")));

            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: forget failure"));
            task.Forget();
            await Task.Yield();
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Forget_GenericResultIsDiscarded()
        {
            var task = UnityTask.FromResult(42);

            task.Forget();

            LogAssert.NoUnexpectedReceived();
        }

        private static async UnityTask CompleteSynchronouslyAsync()
            => await Task.CompletedTask;

        private static async UnityTask<int> ReturnSynchronouslyAsync(int value)
            => await Task.FromResult(value);

        private static async UnityTask AwaitAsync(Task source)
            => await source;

        private static async UnityTask<int> AwaitAndReturnAsync(Task<int> source)
            => await source;

        private static async UnityTask ThrowBeforeSuspensionAsync(string message)
        {
            ThrowExpected(message);
            await Task.Yield();
        }

        private static async UnityTask ThrowAfterSuspensionAsync(Task source, string message)
        {
            await source;
            ThrowExpected(message);
        }

        private static async UnityTask<int> ThrowGenericBeforeSuspensionAsync(string message)
        {
            ThrowExpected(message);
            await Task.Yield();
            return 0;
        }

        private static async UnityTask<int> ThrowGenericAfterSuspensionAsync(Task source, string message)
        {
            await source;
            ThrowExpected(message);
            return 0;
        }

        private static void ThrowExpected(string message)
            => throw new InvalidOperationException(message);

        private static async UnityTask ThrowInstanceBeforeSuspensionAsync(Exception exception)
        {
            ThrowInstance(exception);
            await Task.Yield();
        }

        private static async UnityTask ThrowInstanceAfterSuspensionAsync(Task source, Exception exception)
        {
            await source;
            ThrowInstance(exception);
        }

        private static async UnityTask<int> ThrowGenericInstanceBeforeSuspensionAsync(Exception exception)
        {
            ThrowInstance(exception);
            await Task.Yield();
            return 0;
        }

        private static async UnityTask<int> ThrowGenericInstanceAfterSuspensionAsync(Task source, Exception exception)
        {
            await source;
            ThrowInstance(exception);
            return 0;
        }

        private static void ThrowInstance(Exception exception)
            => throw exception;

        private static (OperationCanceledException before, OperationCanceledException after)[] CreateCancellations(
            CancellationToken token
        )
            => new[] {
                (
                      (OperationCanceledException)new TaskCanceledException("stop")
                    , (OperationCanceledException)new TaskCanceledException("stop")
                ),
                (new OperationCanceledException(token), new OperationCanceledException(token)),
            };

        private static void AssertSameCancellation(OperationCanceledException expected, Exception actual)
        {
            Assert.That(actual, Is.SameAs(expected));
            Assert.AreEqual(expected.GetType(), actual.GetType());
            Assert.AreEqual(expected.Message, actual.Message);
            Assert.AreEqual(expected.CancellationToken, ((OperationCanceledException)actual).CancellationToken);
        }

        private static async Task<Exception> CaptureUnityTaskExceptionAsync(UnityTask task)
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

        private static async Task<Exception> CaptureUnityTaskExceptionAsync<T>(UnityTask<T> task)
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

        private static async Task AssertContinuationInvokedOnceAsync(bool unsafeContinuation)
        {
            var source = new TaskCompletionSource<object>();
            var task = AwaitAsync(source.Task);
            var awaiter = task.GetAwaiter();
            var continuation = new TaskCompletionSource<object>();
            var invocationCount = 0;

            if (unsafeContinuation)
            {
                awaiter.UnsafeOnCompleted(Complete);
            }
            else
            {
                awaiter.OnCompleted(Complete);
            }

            source.SetResult(null);
            await continuation.Task;
            awaiter.GetResult();

            Assert.AreEqual(1, invocationCount);

            void Complete()
            {
                Interlocked.Increment(ref invocationCount);
                continuation.TrySetResult(null);
            }
        }

        private static async Task ThrowWithMarkerAsync()
        {
            await Task.Yield();
            ThrowFromMarker();
        }

        private static void ThrowFromMarker()
            => throw new InvalidOperationException("marker");

        private static async Task<TException> CaptureExpectedExceptionAsync<TException>(
            Func<Task> action
        )
            where TException : Exception
        {
            try
            {
                await action();
            }
            catch (TException exception)
            {
                return exception;
            }

            Assert.Fail($"Expected exception of type {typeof(TException).FullName}.");
            return null;
        }

#if UNITASK && !ENCOSY_UNITYTASK_AWAITABLE
        private static void ConsumeSelectedNative(UniTask native, UniTask<int> genericNative)
        {
            UnityTask wrapper = native;
            UnityTask<int> genericWrapper = genericNative;
            var copy = wrapper;
            var genericCopy = genericWrapper;
            var awaiter = copy.GetAwaiter();
            var genericAwaiter = genericCopy.GetAwaiter();

            s_allocationBoolSink ^= copy.IsCompleted;
            s_allocationBoolSink ^= awaiter.IsCompleted;
            s_allocationBoolSink ^= copy.AsUniTask().GetAwaiter().IsCompleted;
            awaiter.GetResult();
            s_allocationIntSink ^= genericAwaiter.GetResult();
            s_allocationBoolSink ^= genericCopy.AsUniTask().GetAwaiter().IsCompleted;
        }
#else
        private static void ConsumeSelectedNative(Awaitable native, Awaitable<int> genericNative)
        {
            UnityTask wrapper = native;
            UnityTask<int> genericWrapper = genericNative;
            var copy = wrapper;
            var genericCopy = genericWrapper;
            var awaiter = copy.GetAwaiter();
            var genericAwaiter = genericCopy.GetAwaiter();

            s_allocationBoolSink ^= copy.IsCompleted;
            s_allocationBoolSink ^= awaiter.IsCompleted;
            s_allocationBoolSink ^= ReferenceEquals(native, copy.AsAwaitable());
            s_allocationBoolSink ^= ReferenceEquals(genericNative, genericCopy.AsAwaitable());
            awaiter.GetResult();
            s_allocationIntSink ^= genericAwaiter.GetResult();
        }
#endif
    }
}
