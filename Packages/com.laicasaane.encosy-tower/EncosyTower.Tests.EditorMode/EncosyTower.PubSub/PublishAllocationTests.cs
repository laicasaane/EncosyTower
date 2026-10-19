using System;
using System.Buffers;
using EncosyTower.Common;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine.Profiling;

namespace EncosyTower.Tests.PubSub
{
    public sealed class PublishAllocationTests
    {
        private const int WARMUP = 64;
        private const int COUNT = 1000;

        private static readonly Message s_message = new();

        private static object s_sink;
        private static int s_syncCalls;
        private static int s_asyncCalls;
        private static int s_failures;
        private static int s_hash;
        private static bool s_isCreated;

        [SetUp]
        public void SetUp()
        {
            s_sink = null;
            s_syncCalls = 0;
            s_asyncCalls = 0;
            s_failures = 0;
        }

        [Test]
        public void PositiveControl_DetectsAllocation()
        {
            Assert.GreaterOrEqual(CountAllocations(AllocateObject), COUNT);
        }

        [Test]
        public void PublishAsync_SyncHandlerOnly_SteadyState_AllocatesNothing()
        {
            using var messenger = CreateMessenger();
            using var syncSubscription = messenger.Subscriber.Global().Subscribe<Message>(CountSync);

            Assert.AreEqual(0, CountAllocations(() => PublishAsyncAndWait(messenger)));
            Assert.AreEqual(WARMUP + COUNT, s_syncCalls);
            Assert.AreEqual(0, s_failures);
        }

        [Test]
        public void PublishAsync_AsyncHandlerOnly_SteadyState_AllocatesNothing()
        {
            using var messenger = CreateMessenger();
            using var asyncSubscription = messenger.Subscriber.Global().Subscribe<Message>(CountAsync);

            Assert.AreEqual(0, CountAllocations(() => PublishAsyncAndWait(messenger)));
            Assert.AreEqual(WARMUP + COUNT, s_asyncCalls);
            Assert.AreEqual(0, s_failures);
        }

        [Test]
        public void PublishAsync_SyncAndAsyncHandlers_SteadyState_AllocatesNothing()
        {
            using var messenger = CreateMessenger();
            var subscriber = messenger.Subscriber.Global();
            using var syncSubscription = subscriber.Subscribe<Message>(CountSync);
            using var asyncSubscription = subscriber.Subscribe<Message>(CountAsync);

            Assert.AreEqual(0, CountAllocations(() => PublishAsyncAndWait(messenger)));
            Assert.AreEqual(WARMUP + COUNT, s_syncCalls);
            Assert.AreEqual(WARMUP + COUNT, s_asyncCalls);
            Assert.AreEqual(0, s_failures);
        }

        [Test]
        public void Publish_SyncHandler_SteadyState_AllocatesNothing()
        {
            using var messenger = CreateMessenger();
            using var syncSubscription = messenger.Subscriber.Global().Subscribe<Message>(CountSync);

            Assert.AreEqual(0, CountAllocations(() => PublishAndForget(messenger)));
            Assert.AreEqual(WARMUP + COUNT, s_syncCalls);
        }

        [Test]
        public void GlobalScope_GetHashCode_SteadyState_AllocatesNothing()
        {
            Assert.AreEqual(default(GlobalScope).GetHashCode(), new GlobalScope().GetHashCode());
            Assert.AreEqual(0, CountAllocations(HashGlobalScope));
        }

        [Test]
        public void Global_SteadyState_AllocatesNothing()
        {
            using var messenger = CreateMessenger();

            Assert.AreEqual(0, CountAllocations(() => GetGlobalPublisher(messenger)));
        }

        private static Messenger CreateMessenger()
            => new(ArrayPool<UnityTask>.Shared);

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

        private static void AllocateObject()
        {
            s_sink = new object();
        }

        private static void GetGlobalPublisher(Messenger messenger)
        {
            s_isCreated ^= messenger.Publisher.Global().IsCreated;
        }

        private static void HashGlobalScope()
        {
            s_hash ^= default(GlobalScope).GetHashCode();
        }

        private static void PublishAsyncAndWait(Messenger messenger)
        {
            var result = messenger.Publisher.Global()
                .PublishAsync(s_message, PublishingContext.Default())
                .GetAwaiter()
                .GetResult();

            if (result.TryGetFailure(out _))
            {
                s_failures++;
            }
        }

        private static void PublishAndForget(Messenger messenger)
        {
            messenger.Publisher.Global().Publish(s_message, PublishingContext.Default());
        }

        private static void CountSync()
        {
            s_syncCalls++;
        }

        private static UnityTask CountAsync()
        {
            s_asyncCalls++;
            return UnityTask.CompletedTask;
        }

        private sealed class Message : IMessage
        {
        }
    }
}
