using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
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
        public async Task AsyncMethodCreatedOnMain_CompletedOnWorker_ResumesOnMain()
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var source = new UnityTaskCompletionSource();
            var task = AwaitAndGetThreadIdAsync(source.Task);

            await CompleteOnWorkerAsync(source);
            var resumedThreadId = await task;

            Assert.AreEqual(mainThreadId, resumedThreadId);
            Assert.AreEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);
        }

        [Test]
        public async Task CompletionSourceCreatedOnWorker_ResumesOnWorker()
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var resumedThreadId = await Task.Run<int>(CreateAndAwaitOnWorkerAsync);

            Assert.AreNotEqual(mainThreadId, resumedThreadId);

            static async Task<int> CreateAndAwaitOnWorkerAsync()
            {
                var source = new UnityTaskCompletionSource();
                var completion = CompleteOnWorkerAsync(source);

                await source.Task;
                var threadId = Thread.CurrentThread.ManagedThreadId;

                await completion;
                return threadId;
            }
        }

        [Test]
        public async Task AsyncMethodCompletedOnWorker_ResumesOnCreatorMain()
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;

            var completedThreadId = await CompleteOnWorkerAndGetThreadIdAsync();
            var resumedThreadId = Thread.CurrentThread.ManagedThreadId;

            Assert.AreNotEqual(mainThreadId, completedThreadId);
            Assert.AreEqual(mainThreadId, resumedThreadId);
        }

        [Test]
        public async Task SynchronousAsyncMethod_IsCompletedFalseOnOtherThreadKind()
        {
            var task = CompleteSynchronouslyAsync();

            var isCompletedOnWorker = await Task.Factory.StartNew(
                  static state => ((UnityTask)state).IsCompleted
                , task
                , CancellationToken.None
                , TaskCreationOptions.DenyChildAttach
                , TaskScheduler.Default
            );

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(isCompletedOnWorker);
            await task;
        }

        [Test]
        public async Task CompletionSourceCreatedOnWorker_AwaitedOnMain_ResumesOnWorker()
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var source = await Task.Run<UnityTaskCompletionSource>(CreateCompletedSourceOnWorker);

            Assert.AreEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);

            var isCompletedOnMain = source.Task.IsCompleted;
            await source.Task;
            var resumedThreadId = Thread.CurrentThread.ManagedThreadId;

            Assert.IsFalse(isCompletedOnMain);
            Assert.AreNotEqual(mainThreadId, resumedThreadId);

            static UnityTaskCompletionSource CreateCompletedSourceOnWorker()
            {
                var result = new UnityTaskCompletionSource();
                result.SetResult();
                return result;
            }
        }

        [Test]
        public async Task IsCompleted_FalseWhenDoneButCurrentThreadKindDiffers()
        {
            var source = new UnityTaskCompletionSource();
            source.SetResult();
            var task = source.Task;

            var isCompletedOnWorker = await Task.Factory.StartNew(
                  static state => ((UnityTask)state).IsCompleted
                , task
                , CancellationToken.None
                , TaskCreationOptions.DenyChildAttach
                , TaskScheduler.Default
            );

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(isCompletedOnWorker);
            await task;
        }

        [Test]
        public async Task CompletionSource_SetTwiceThrows_TrySetTwiceReturnsFalse()
        {
            using var cancellation = new CancellationTokenSource();
            var source = new UnityTaskCompletionSource();
            source.SetResult();

            Assert.Throws<InvalidOperationException>(source.SetResult);
            Assert.Throws<InvalidOperationException>(() => source.SetException(new InvalidOperationException()));
            Assert.Throws<InvalidOperationException>(() => source.SetCanceled(cancellation.Token));
            Assert.IsFalse(source.TrySetResult());
            Assert.IsFalse(source.TrySetException(new InvalidOperationException()));
            Assert.IsFalse(source.TrySetCanceled(cancellation.Token));
            await source.Task;

            var genericSource = new UnityTaskCompletionSource<int>();
            genericSource.SetResult(1);

            Assert.Throws<InvalidOperationException>(() => genericSource.SetResult(2));
            Assert.Throws<InvalidOperationException>(() => genericSource.SetException(new InvalidOperationException()));
            Assert.Throws<InvalidOperationException>(() => genericSource.SetCanceled(cancellation.Token));
            Assert.IsFalse(genericSource.TrySetResult(3));
            Assert.IsFalse(genericSource.TrySetException(new InvalidOperationException()));
            Assert.IsFalse(genericSource.TrySetCanceled(cancellation.Token));
            Assert.AreEqual(1, await genericSource.Task);
        }

        [Test]
        public async Task OnCompletedAndUnsafeOnCompleted_InvokeExactlyOnce()
        {
            await AssertContinuationInvokedOnceAsync(unsafeContinuation: false);
            await AssertContinuationInvokedOnceAsync(unsafeContinuation: true);
        }

        [Test]
        public async Task SelectedNativeBridge_RoundTripsResult()
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

            await wrapper;
            Assert.AreEqual(42, await genericWrapper);
            await UnityTask.CompletedTask.AsAwaitable();
            Assert.AreEqual(7, await UnityTask.FromResult(7).AsAwaitable());
            Assert.AreEqual(9, await AwaitAndReturnAsync(Task.FromResult(9)).AsAwaitable());
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
            var recorder = Recorder.Get("GC.Alloc");
            recorder.enabled = false;
            recorder.FilterToCurrentThread();
            recorder.enabled = true;

            for (var i = 0; i < 1_000; i++)
            {
                ConsumeSelectedNative(native, genericNative);
            }

            recorder.enabled = false;
            recorder.CollectFromAllThreads();
            var allocated = recorder.sampleBlockCount;
#else
            const int COUNT = 128;

            var warmupTasks = new Awaitable[COUNT];
            var genericWarmupTasks = new Awaitable<int>[COUNT];
            var nativeTasks = new Awaitable[COUNT];
            var genericNativeTasks = new Awaitable<int>[COUNT];

            FillCompletedNative(warmupTasks, genericWarmupTasks);

            for (var i = 0; i < COUNT; i++)
            {
                ConsumeSelectedNative(warmupTasks[i], genericWarmupTasks[i]);
            }

            FillCompletedNative(nativeTasks, genericNativeTasks);

            var recorder = Recorder.Get("GC.Alloc");
            recorder.enabled = false;
            recorder.FilterToCurrentThread();
            recorder.enabled = true;

            for (var i = 0; i < COUNT; i++)
            {
                ConsumeSelectedNative(nativeTasks[i], genericNativeTasks[i]);
            }

            recorder.enabled = false;
            recorder.CollectFromAllThreads();
            var allocated = recorder.sampleBlockCount;
#endif

            Assert.AreEqual(0, allocated);
        }

        [Test]
        public async Task FromExceptionAndFromCanceled_PreserveInstanceAndToken()
        {
            using var cancellation = new CancellationTokenSource();
            var fault = new InvalidOperationException("fault");
            var canceled = new OperationCanceledException(cancellation.Token);

            var faultException = await CaptureUnityTaskExceptionAsync(UnityTask.FromException(fault));
            var canceledException = await CaptureUnityTaskExceptionAsync(UnityTask.FromException(canceled));
            var tokenException = await CaptureUnityTaskExceptionAsync(UnityTask.FromCanceled(cancellation.Token));

            var genericFault = await CaptureUnityTaskExceptionAsync(UnityTask.FromException<int>(fault));
            var genericCanceled = await CaptureUnityTaskExceptionAsync(UnityTask.FromException<int>(canceled));
            var genericToken = await CaptureUnityTaskExceptionAsync(UnityTask.FromCanceled<int>(cancellation.Token));

            Assert.That(faultException, Is.SameAs(fault));
            Assert.That(canceledException, Is.SameAs(canceled));
            Assert.That(genericFault, Is.SameAs(fault));
            Assert.That(genericCanceled, Is.SameAs(canceled));
            Assert.IsInstanceOf<OperationCanceledException>(tokenException);
            Assert.IsInstanceOf<OperationCanceledException>(genericToken);
            Assert.AreEqual(cancellation.Token, ((OperationCanceledException)tokenException).CancellationToken);
            Assert.AreEqual(cancellation.Token, ((OperationCanceledException)genericToken).CancellationToken);
        }

        [Test]
        public async Task FromResult_ManyOutstandingTasks_KeepDistinctResults()
        {
            const int COUNT = 300;

            var tasks = new UnityTask<int>[COUNT];

            for (var i = 0; i < COUNT; i++)
            {
                tasks[i] = (i & 1) == 0 ? UnityTask.FromResult(i) : UnityTask.GetCompleted(i);
            }

            for (var i = 0; i < COUNT; i++)
            {
                Assert.IsTrue(tasks[i].IsCompleted);
                Assert.AreEqual(i, await tasks[i]);
            }
        }

        [Test]
        public async Task CompletedFactoryTasks_CompleteInlineOnWorker()
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var failures = await Task.Run<string>(AwaitFactoriesOnWorkerAsync);

            Assert.IsEmpty(failures);

            async Task<string> AwaitFactoriesOnWorkerAsync()
            {
                var workerThreadId = Thread.CurrentThread.ManagedThreadId;
                var result = string.Empty;

                if (workerThreadId == mainThreadId)
                {
                    return "The worker ran on the main thread.";
                }

                result += Check(UnityTask.CompletedTask.IsCompleted, nameof(UnityTask.CompletedTask));
                result += Check(UnityTask.GetCompleted<int>(1).IsCompleted, nameof(UnityTask.GetCompleted));
                result += Check(UnityTask.FromResult(2).IsCompleted, nameof(UnityTask.FromResult));
                result += Check(UnityTask.FromException(new InvalidOperationException()).IsCompleted, "FromException");
                result += Check(UnityTask.FromCanceled().IsCompleted, nameof(UnityTask.FromCanceled));

                await UnityTask.CompletedTask;
                result += Check(Thread.CurrentThread.ManagedThreadId == workerThreadId, "await CompletedTask");

                result += Check(await UnityTask.GetCompleted<int>(1) == 1, "GetCompleted result");
                result += Check(Thread.CurrentThread.ManagedThreadId == workerThreadId, "await GetCompleted");

                result += Check(await UnityTask.FromResult(2) == 2, "FromResult result");
                result += Check(Thread.CurrentThread.ManagedThreadId == workerThreadId, "await FromResult");

                await CaptureUnityTaskExceptionAsync(UnityTask.FromException(new InvalidOperationException()));
                result += Check(Thread.CurrentThread.ManagedThreadId == workerThreadId, "await FromException");

                await CaptureUnityTaskExceptionAsync(UnityTask.FromCanceled());
                result += Check(Thread.CurrentThread.ManagedThreadId == workerThreadId, "await FromCanceled");

                return result;
            }

            static string Check(bool condition, string name)
                => condition ? string.Empty : $"{name} failed. ";
        }

        [Test]
        public async Task Yield_DoesNotCompleteSynchronously_AndCompletes()
        {
            var task = UnityTask.Yield();

            Assert.IsFalse(task.IsCompleted);
            await task;
        }

        [Test]
        public async Task NextFrameAsync_Completes()
        {
            await UnityTask.NextFrameAsync();
        }

        [Test]
        public async Task Delay_ImmediateAndPolledCancellation_PreserveToken()
        {
            using var canceled = new CancellationTokenSource();
            using var later = new CancellationTokenSource();
            canceled.Cancel();

            var immediate = UnityTask.Delay(millisecondsDelay: 10_000, token: canceled.Token);
            var polled = UnityTask.Delay(millisecondsDelay: 10_000, token: later.Token);

            Assert.IsTrue(immediate.IsCompleted);
            Assert.IsFalse(polled.IsCompleted);

            var immediateException = await CaptureUnityTaskExceptionAsync(immediate);
            later.Cancel();
            var polledException = await CaptureUnityTaskExceptionAsync(polled);

            Assert.IsInstanceOf<OperationCanceledException>(immediateException);
            Assert.IsInstanceOf<OperationCanceledException>(polledException);
            Assert.AreEqual(canceled.Token, ((OperationCanceledException)immediateException).CancellationToken);
            Assert.AreEqual(later.Token, ((OperationCanceledException)polledException).CancellationToken);
        }

        [Test]
        public async Task Delay_CancelImmediately_CompletesBeforeSchedulerTick()
        {
            using var cancellation = new CancellationTokenSource();
            var delay = UnityTask.Delay(millisecondsDelay: 10_000, cancelImmediately: true, token: cancellation.Token);

            cancellation.Cancel();
            var isCompletedAfterCancel = delay.IsCompleted;
            var exception = await CaptureUnityTaskExceptionAsync(delay);

            Assert.IsTrue(isCompletedAfterCancel);
            Assert.IsInstanceOf<OperationCanceledException>(exception);
            Assert.AreEqual(cancellation.Token, ((OperationCanceledException)exception).CancellationToken);
        }

        [Test]
        public async Task WaitUntil_PredicateTrue_CompletesSynchronously()
        {
            var task = UnityTask.WaitUntil(static () => true);

            Assert.IsTrue(task.IsCompleted);
            await task;
        }

        [Test]
        public async Task WaitUntil_PredicateThrows_Faults()
        {
            var fault = new InvalidOperationException("predicate");
            var task = UnityTask.WaitUntil(fault, static state => throw state);

            var exception = await CaptureUnityTaskExceptionAsync(task);

            Assert.That(exception, Is.SameAs(fault));
        }

        [Test]
        public async Task RunOnThreadPool_RunsOnWorker_ResumesOnMain()
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            using var cancellation = new CancellationTokenSource();

            var workThreadId = await UnityTask.RunOnThreadPool(static () => Thread.CurrentThread.ManagedThreadId);
            var resumedThreadId = Thread.CurrentThread.ManagedThreadId;

            var canceled = UnityTask.RunOnThreadPool(
                  static state => ((CancellationTokenSource)state).Cancel()
                , cancellation
                , cancellation.Token
            );

            var exception = await CaptureUnityTaskExceptionAsync(canceled);

            Assert.AreNotEqual(mainThreadId, workThreadId);
            Assert.AreEqual(mainThreadId, resumedThreadId);
            Assert.IsInstanceOf<OperationCanceledException>(exception);
            Assert.AreEqual(cancellation.Token, ((OperationCanceledException)exception).CancellationToken);
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
        public async Task WhenAny_TieResolvedByLowestIndex()
        {
            var tasks = new[] { UnityTask.FromResult(1), UnityTask.FromResult(2) };

            var (winArgumentIndex, result) = await UnityTask.WhenAny(tasks);

            Assert.AreEqual(0, winArgumentIndex);
            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task WhenAny_LoserObservedNotCanceled()
        {
            var winner = new UnityTaskCompletionSource<int>();
            var loser = new UnityTaskCompletionSource<int>();
            var any = UnityTask.WhenAny(new[] { winner.Task, loser.Task });

            winner.SetResult(7);
            var (winArgumentIndex, result) = await any;

            Assert.AreEqual(0, winArgumentIndex);
            Assert.AreEqual(7, result);
            Assert.IsTrue(loser.TrySetException(new InvalidOperationException("loser fault")));

            await Task.Yield();
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task WhenEach_CancellationBetweenMoves_Throws()
        {
            using var cancellation = new CancellationTokenSource();
            var first = new UnityTaskCompletionSource<int>();
            var second = new UnityTaskCompletionSource<int>();
            var enumerator = UnityTask.WhenEach(first.Task, second.Task).GetAsyncEnumerator(cancellation.Token);

            first.SetResult(1);
            var moved = await enumerator.MoveNextAsync();
            var current = enumerator.Current.Result;

            cancellation.Cancel();
            var nextException = await CaptureUnityTaskExceptionAsync(enumerator.MoveNextAsync());
            var furtherException = await CaptureUnityTaskExceptionAsync(enumerator.MoveNextAsync());

            second.SetResult(2);
            await enumerator.DisposeAsync();

            Assert.IsTrue(moved);
            Assert.AreEqual(1, current);
            Assert.IsInstanceOf<OperationCanceledException>(nextException);
            Assert.IsInstanceOf<OperationCanceledException>(furtherException);
            Assert.AreEqual(cancellation.Token, ((OperationCanceledException)nextException).CancellationToken);
            Assert.AreEqual(cancellation.Token, ((OperationCanceledException)furtherException).CancellationToken);
        }

        [Test]
        public async Task WhenEach_CancellationWhileMovePending_Throws()
        {
            using var cancellation = new CancellationTokenSource();
            var source = new UnityTaskCompletionSource<int>();
            var enumerator = UnityTask.WhenEach(source.Task).GetAsyncEnumerator(cancellation.Token);

            var pending = enumerator.MoveNextAsync();
            var wasPending = pending.IsCompleted == false;

            cancellation.Cancel();
            var pendingException = await CaptureUnityTaskExceptionAsync(pending);
            var laterException = await CaptureUnityTaskExceptionAsync(enumerator.MoveNextAsync());

            source.SetResult(1);
            await enumerator.DisposeAsync();

            Assert.IsTrue(wasPending);
            Assert.IsInstanceOf<OperationCanceledException>(pendingException);
            Assert.IsInstanceOf<OperationCanceledException>(laterException);
            Assert.AreEqual(cancellation.Token, ((OperationCanceledException)pendingException).CancellationToken);
            Assert.AreEqual(cancellation.Token, ((OperationCanceledException)laterException).CancellationToken);
        }

        [Test]
        public async Task WhenEach_DisposeCompletesPendingMoveWithFalse()
        {
            var source = new UnityTaskCompletionSource<int>();
            var enumerator = UnityTask.WhenEach(source.Task).GetAsyncEnumerator();

            var pending = enumerator.MoveNextAsync();
            await enumerator.DisposeAsync();
            var moved = await pending;

            source.SetResult(1);

            Assert.IsFalse(moved);
        }

        [Test]
        public async Task WhenEach_ResultDeliveredToPendingMove_IsCurrent()
        {
            var source = new UnityTaskCompletionSource<int>();
            var enumerator = UnityTask.WhenEach(source.Task).GetAsyncEnumerator();

            var pending = enumerator.MoveNextAsync();
            source.SetResult(5);

            var moved = await pending;
            var current = enumerator.Current.Result;
            var movedAfterEnd = await enumerator.MoveNextAsync();
            await enumerator.DisposeAsync();

            Assert.IsTrue(moved);
            Assert.AreEqual(5, current);
            Assert.IsFalse(movedAfterEnd);
        }

        [Test]
        public async Task WhenEach_DisposedEnumerator_IgnoresLaterUseAfterReuse()
        {
            var first = UnityTask.WhenEach(UnityTask.FromResult(1)).GetAsyncEnumerator();
            var drained = 0;

            while (await first.MoveNextAsync())
            {
                drained++;
            }

            await first.DisposeAsync();

            var second = UnityTask.WhenEach(UnityTask.FromResult(2)).GetAsyncEnumerator();
            var firstMoved = await first.MoveNextAsync();
            var firstCurrent = first.Current.Result;
            await first.DisposeAsync();

            var secondMoved = await second.MoveNextAsync();
            var secondCurrent = second.Current.Result;
            await second.DisposeAsync();

            Assert.AreEqual(1, drained);
            Assert.IsFalse(firstMoved);
            Assert.AreEqual(default(int), firstCurrent);
            Assert.IsTrue(secondMoved);
            Assert.AreEqual(2, secondCurrent);
        }

        [Test]
        public async Task WhenEach_ConcurrentCompletion_NoResultLost()
        {
            const int COUNT = 64;

            var sources = await Task.Run<UnityTaskCompletionSource<int>[]>(CreateSourcesOnWorker);
            var tasks = new UnityTask<int>[COUNT];

            for (var i = 0; i < COUNT; i++)
            {
                tasks[i] = sources[i].Task;
            }

            var enumerator = UnityTask.WhenEach(tasks).GetAsyncEnumerator();
            var firstMove = enumerator.MoveNextAsync();
            var completion = Task.Run(CompleteInParallel);
            var values = new HashSet<int>();
            var yielded = 0;

            if (await firstMove)
            {
                values.Add(enumerator.Current.Result);
                yielded++;
            }

            while (await enumerator.MoveNextAsync())
            {
                values.Add(enumerator.Current.Result);
                yielded++;
            }

            await completion;
            await enumerator.DisposeAsync();

            Assert.AreEqual(COUNT, yielded);
            Assert.AreEqual(COUNT, values.Count);

            static UnityTaskCompletionSource<int>[] CreateSourcesOnWorker()
            {
                var result = new UnityTaskCompletionSource<int>[COUNT];

                for (var i = 0; i < COUNT; i++)
                {
                    result[i] = new UnityTaskCompletionSource<int>();
                }

                return result;
            }

            void CompleteInParallel()
            {
                Parallel.For(0, COUNT, Complete);
            }

            void Complete(int index)
            {
                sources[index].SetResult(index);
            }
        }

        [Test]
        public async Task ContinueWith_SkippedOnFaultAndCancellation()
        {
            using var cancellation = new CancellationTokenSource();
            var fault = new InvalidOperationException("antecedent");
            var canceled = new OperationCanceledException(cancellation.Token);
            var calls = 0;

            var faultException = await CaptureUnityTaskExceptionAsync(
                UnityTask.FromException<int>(fault).ContinueWith(CountResult)
            );

            var canceledException = await CaptureUnityTaskExceptionAsync(
                UnityTask.FromException<int>(canceled).ContinueWith(CountResult)
            );

            var nonGenericException = await CaptureUnityTaskExceptionAsync(
                UnityTask.FromException(fault).ContinueWith(Count)
            );

            var doubled = await UnityTask.FromResult(20).ContinueWith(Double);
            await UnityTask.CompletedTask.ContinueWith(Count);

            Assert.That(faultException, Is.SameAs(fault));
            Assert.That(canceledException, Is.SameAs(canceled));
            Assert.That(nonGenericException, Is.SameAs(fault));
            Assert.AreEqual(40, doubled);
            Assert.AreEqual(1, calls);

            void CountResult(int value)
            {
                calls++;
            }

            void Count()
            {
                calls++;
            }

            static int Double(int value)
                => value * 2;
        }

        [Test]
        public async Task Forget_FaultLogsOnce_CancellationSilent()
        {
            using var cancellation = new CancellationTokenSource();
            var fault = UnityTask.FromException(new InvalidOperationException("forget once"));
            var canceled = UnityTask.FromCanceled(cancellation.Token);

            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: forget once"));
            fault.Forget();
            canceled.Forget();

            await Task.Yield();
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task Scheduler_ThrowingContinuation_LoggedAndLaterContinuationsRun()
        {
            var first = UnityTask.Yield();
            var second = UnityTask.Yield();
            var third = UnityTask.Yield();

            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: scheduler isolation"));
            second.GetAwaiter().UnsafeOnCompleted(ThrowFromScheduler);

            await first;
            await third;

            LogAssert.NoUnexpectedReceived();

            static void ThrowFromScheduler()
                => throw new InvalidOperationException("scheduler isolation");
        }

        [Test]
        public async Task WhenAll_Arity2And15_ReturnResultsInOrder()
        {
            var first = new UnityTaskCompletionSource<int>();
            var second = new UnityTaskCompletionSource<string>();
            var all2 = UnityTask.WhenAll(first.Task, second.Task);

            second.SetResult("second");
            first.SetResult(1);

            var (firstResult, secondResult) = await all2;
            var sources = CreateIntSources(15);

            var all15Task = UnityTask.WhenAll(
                  sources[0].Task
                , sources[1].Task
                , sources[2].Task
                , sources[3].Task
                , sources[4].Task
                , sources[5].Task
                , sources[6].Task
                , sources[7].Task
                , sources[8].Task
                , sources[9].Task
                , sources[10].Task
                , sources[11].Task
                , sources[12].Task
                , sources[13].Task
                , sources[14].Task
            );

            for (var i = sources.Length - 1; i >= 0; i--)
            {
                sources[i].SetResult(i * 10);
            }

            var all15 = await all15Task;

            Assert.AreEqual(1, firstResult);
            Assert.AreEqual("second", secondResult);
            Assert.AreEqual(0, all15.Item1);
            Assert.AreEqual(10, all15.Item2);
            Assert.AreEqual(20, all15.Item3);
            Assert.AreEqual(30, all15.Item4);
            Assert.AreEqual(40, all15.Item5);
            Assert.AreEqual(50, all15.Item6);
            Assert.AreEqual(60, all15.Item7);
            Assert.AreEqual(70, all15.Item8);
            Assert.AreEqual(80, all15.Item9);
            Assert.AreEqual(90, all15.Item10);
            Assert.AreEqual(100, all15.Item11);
            Assert.AreEqual(110, all15.Item12);
            Assert.AreEqual(120, all15.Item13);
            Assert.AreEqual(130, all15.Item14);
            Assert.AreEqual(140, all15.Item15);
        }

        [Test]
        public async Task WhenAny_Arity2And15_ReturnWinnerIndexAndResult()
        {
            var first = new UnityTaskCompletionSource<int>();
            var second = new UnityTaskCompletionSource<string>();
            var any2 = UnityTask.WhenAny(first.Task, second.Task);

            second.SetResult("second");
            first.SetResult(1);

            var winner2 = await any2;
            var sources = CreateIntSources(15);

            var any15Task = UnityTask.WhenAny(
                  sources[0].Task
                , sources[1].Task
                , sources[2].Task
                , sources[3].Task
                , sources[4].Task
                , sources[5].Task
                , sources[6].Task
                , sources[7].Task
                , sources[8].Task
                , sources[9].Task
                , sources[10].Task
                , sources[11].Task
                , sources[12].Task
                , sources[13].Task
                , sources[14].Task
            );

            sources[9].SetResult(90);

            for (var i = 0; i < sources.Length; i++)
            {
                sources[i].TrySetResult(i);
            }

            var winner15 = await any15Task;

            Assert.AreEqual(1, winner2.winArgumentIndex);
            Assert.AreEqual("second", winner2.result2);
            Assert.AreEqual(default(int), winner2.result1);
            Assert.AreEqual(9, winner15.winArgumentIndex);
            Assert.AreEqual(90, winner15.result10);
            Assert.AreEqual(default(int), winner15.result1);
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

        private static async UnityTask<int> CompleteOnWorkerAndGetThreadIdAsync()
        {
            await Task.Run(static () => { }).ConfigureAwait(false);
            return Thread.CurrentThread.ManagedThreadId;
        }

        private static UnityTaskCompletionSource<int>[] CreateIntSources(int count)
        {
            var sources = new UnityTaskCompletionSource<int>[count];

            for (var i = 0; i < count; i++)
            {
                sources[i] = new UnityTaskCompletionSource<int>();
            }

            return sources;
        }

        private static async UnityTask<int> AwaitAndGetThreadIdAsync(UnityTask task)
        {
            await task;
            return Thread.CurrentThread.ManagedThreadId;
        }

        private static Task CompleteOnWorkerAsync(UnityTaskCompletionSource source)
            => Task.Factory.StartNew(
                  static state => ((UnityTaskCompletionSource)state).TrySetResult()
                , source
                , CancellationToken.None
                , TaskCreationOptions.DenyChildAttach
                , TaskScheduler.Default
            );

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
        private static void FillCompletedNative(Awaitable[] tasks, Awaitable<int>[] genericTasks)
        {
            for (var i = 0; i < tasks.Length; i++)
            {
                var source = new AwaitableCompletionSource();
                var genericSource = new AwaitableCompletionSource<int>();
                source.SetResult();
                genericSource.SetResult(42);
                tasks[i] = source.Awaitable;
                genericTasks[i] = genericSource.Awaitable;
            }
        }

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
            awaiter.GetResult();
            s_allocationIntSink ^= genericAwaiter.GetResult();
        }
#endif
    }
}
