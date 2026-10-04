using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine.Profiling;

namespace EncosyTower.Tests.Tasks
{
    public class UnityTaskAllocationTests
    {
        private const int WARMUP = 64;
        private const int COUNT = 1000;

        private static readonly Action s_runUpdate = CreateRunUpdate();
        private static readonly ManualAwaitable s_manual = new();
        private static readonly UnityTask[] s_tasks = new UnityTask[4];
        private static readonly UnityTask<int>[] s_intTasks = new UnityTask<int>[4];

        private static int s_sink;

        [Test]
        public void AsyncMethods_SteadyState_AllocateNothing()
        {
            Assert.AreEqual(0, CountAllocations(RunSynchronousMethod), "synchronous async method");
            Assert.AreEqual(0, CountAllocations(RunSynchronousGenericMethod), "synchronous async method<T>");
            Assert.AreEqual(0, CountAllocations(RunSuspendedMethod), "suspended async method");
            Assert.AreEqual(0, CountAllocations(RunNestedSuspendedMethod), "nested suspended await");
        }

        [Test]
        public void Timing_SteadyState_AllocatesNothing()
        {
            Assert.AreEqual(0, CountAllocations(RunYield), nameof(UnityTask.Yield));
            Assert.AreEqual(0, CountAllocations(RunDelay), nameof(UnityTask.Delay));
            Assert.AreEqual(0, CountAllocations(RunNextFrame), nameof(UnityTask.NextFrameAsync));
            Assert.AreEqual(0, CountAllocations(RunWaitUntil), nameof(UnityTask.WaitUntil));
        }

        [Test]
        public void Combinators_SteadyState_AllocateOnlyReturnedResults()
        {
            Assert.AreEqual(0, CountAllocations(RunWhenAll), "WhenAll(UnityTask[])");
            Assert.AreEqual(COUNT, CountAllocations(RunGenericWhenAll), "WhenAll<T>(UnityTask<T>[]) result array");
            Assert.AreEqual(0, CountAllocations(RunWhenAny), "WhenAny<T>(UnityTask<T>[])");
            Assert.AreEqual(0, CountAllocations(RunWhenAllArity), "WhenAll(task1, task2)");
            Assert.AreEqual(0, CountAllocations(RunWhenAnyArity), "WhenAny(task1, task2)");
        }

        [Test]
        public void ContinueWithAndForget_SteadyState_AllocateNothing()
        {
            Assert.AreEqual(0, CountAllocations(RunContinueWith), nameof(UnityTaskExtensions.ContinueWith));
            Assert.AreEqual(0, CountAllocations(RunForget), nameof(UnityTaskExtensions.Forget));
        }

        private static int CountAllocations(Action action)
        {
            for (var i = 0; i < WARMUP; i++)
            {
                action();
            }

            var recorder = Recorder.Get("GC.Alloc");
            recorder.enabled = false;
            recorder.FilterToCurrentThread();
            recorder.enabled = true;

            try
            {
                for (var i = 0; i < COUNT; i++)
                {
                    action();
                }
            }
            finally
            {
                recorder.enabled = false;
                recorder.CollectFromAllThreads();
            }

            return recorder.sampleBlockCount;
        }

        private static void RunSynchronousMethod()
        {
            SynchronousAsync().GetAwaiter().GetResult();
        }

        private static void RunSynchronousGenericMethod()
        {
            s_sink ^= SynchronousGenericAsync().GetAwaiter().GetResult();
        }

        private static void RunSuspendedMethod()
        {
            var task = SuspendAsync(s_manual);
            s_manual.Resume();
            task.GetAwaiter().GetResult();
        }

        private static void RunNestedSuspendedMethod()
        {
            var task = AwaitSuspendedAsync(s_manual);
            s_manual.Resume();
            task.GetAwaiter().GetResult();
        }

        private static void RunYield()
        {
            var task = UnityTask.Yield();
            s_runUpdate();
            task.GetAwaiter().GetResult();
        }

        private static void RunDelay()
        {
            var task = UnityTask.Delay(TimeSpan.Zero);
            s_runUpdate();
            task.GetAwaiter().GetResult();
        }

        private static void RunNextFrame()
        {
            var task = UnityTask.NextFrameAsync();
            s_runUpdate();
            task.GetAwaiter().GetResult();
        }

        private static void RunWaitUntil()
        {
            UnityTask.WaitUntil(static () => true).GetAwaiter().GetResult();
        }

        private static void RunWhenAll()
        {
            UnityTask.WhenAll(s_tasks).GetAwaiter().GetResult();
        }

        private static void RunGenericWhenAll()
        {
            s_sink ^= UnityTask.WhenAll(s_intTasks).GetAwaiter().GetResult().Length;
        }

        private static void RunWhenAny()
        {
            s_sink ^= UnityTask.WhenAny(s_intTasks).GetAwaiter().GetResult().winArgumentIndex;
        }

        private static void RunWhenAllArity()
        {
            var (first, second) = UnityTask.WhenAll(default(UnityTask<int>), default(UnityTask<int>))
                .GetAwaiter()
                .GetResult();

            s_sink ^= first + second;
        }

        private static void RunWhenAnyArity()
        {
            s_sink ^= UnityTask.WhenAny(default(UnityTask<int>), default(UnityTask<int>))
                .GetAwaiter()
                .GetResult()
                .winArgumentIndex;
        }

        private static void RunContinueWith()
        {
            s_sink ^= default(UnityTask<int>).ContinueWith(static value => value + 1).GetAwaiter().GetResult();
        }

        private static void RunForget()
        {
            default(UnityTask).Forget();
        }

        private static async UnityTask SynchronousAsync()
        {
            await UnityTask.CompletedTask;
        }

        private static async UnityTask<int> SynchronousGenericAsync()
        {
            await UnityTask.CompletedTask;
            return 1;
        }

        private static async UnityTask SuspendAsync(ManualAwaitable awaitable)
        {
            await awaitable;
        }

        private static async UnityTask AwaitSuspendedAsync(ManualAwaitable awaitable)
        {
            await SuspendAsync(awaitable);
        }

        private static Action CreateRunUpdate()
        {
            var method = typeof(UnityTask).Assembly
                .GetType("EncosyTower.Tasks.PlayerLoopScheduler")
                .GetMethod("RunUpdate", BindingFlags.NonPublic | BindingFlags.Static);

            return (Action)Delegate.CreateDelegate(typeof(Action), method);
        }

        private sealed class ManualAwaitable : ICriticalNotifyCompletion
        {
            private Action _continuation;

            public bool IsCompleted => false;

            public ManualAwaitable GetAwaiter()
                => this;

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                _continuation = continuation;
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                _continuation = continuation;
            }

            public void Resume()
            {
                var continuation = _continuation;
                _continuation = null;
                continuation();
            }
        }
    }
}
