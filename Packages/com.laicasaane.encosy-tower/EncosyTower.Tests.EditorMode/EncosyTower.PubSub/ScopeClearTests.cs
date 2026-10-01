using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.PubSub
{
    public sealed class ScopeClearTests
    {
        [Test]
        public async Task TargetUnityScope_ClearRemovesAllMessageTypes()
        {
            using var messenger = CreateMessenger();
            var scope = new GameObject();
            var synchronousCount = 0;
            var asynchronousCount = 0;

            try
            {
                var subscriber = messenger.Subscriber.UnityScope(scope);
                var publisher = messenger.Publisher.UnityScope(scope);
                subscriber.Subscribe<FirstMessage>(() => synchronousCount++);
                subscriber.Subscribe<SecondMessage>(() => {
                    asynchronousCount++;
                    return UnityTask.CompletedTask;
                });

                subscriber.Clear();
                subscriber.Clear();
                publisher.Publish(new FirstMessage(), SilentContext());
                await publisher.PublishAsync(new SecondMessage(), SilentContext());

                Assert.AreEqual(0, synchronousCount);
                Assert.AreEqual(0, asynchronousCount);
            }
            finally
            {
                DestroyImmediate(scope);
            }
        }

        [Test]
        public void Clear_IsNarrowToTargetScopeAndMessenger()
        {
            using var messenger = CreateMessenger();
            using var otherMessenger = CreateMessenger();
            var targetScope = new GameObject();
            var otherScope = new GameObject();
            var targetCount = 0;
            var otherCount = 0;
            var valueCount = 0;
            var globalCount = 0;
            var otherMessengerCount = 0;

            try
            {
                var targetSubscriber = messenger.Subscriber.UnityScope(targetScope);
                targetSubscriber.Subscribe<FirstMessage>(() => targetCount++);
                messenger.Subscriber.UnityScope(otherScope).Subscribe<FirstMessage>(() => otherCount++);
                messenger.Subscriber.Scope(new Scope(1)).Subscribe<FirstMessage>(() => valueCount++);
                messenger.Subscriber.Global().Subscribe<FirstMessage>(() => globalCount++);
                otherMessenger.Subscriber.UnityScope(targetScope).Subscribe<FirstMessage>(() => otherMessengerCount++);

                targetSubscriber.Clear();
                messenger.Publisher.UnityScope(targetScope).Publish(new FirstMessage(), SilentContext());
                messenger.Publisher.UnityScope(otherScope).Publish(new FirstMessage(), SilentContext());
                messenger.Publisher.Scope(new Scope(1)).Publish(new FirstMessage(), SilentContext());
                messenger.Publisher.Global().Publish(new FirstMessage(), SilentContext());
                otherMessenger.Publisher.UnityScope(targetScope).Publish(new FirstMessage(), SilentContext());

                Assert.AreEqual(0, targetCount);
                Assert.AreEqual(1, otherCount);
                Assert.AreEqual(1, valueCount);
                Assert.AreEqual(1, globalCount);
                Assert.AreEqual(1, otherMessengerCount);
            }
            finally
            {
                DestroyImmediate(targetScope);
                DestroyImmediate(otherScope);
            }
        }

        [Test]
        public void Clear_PreservesCachedPublisherAndOldAndFreshSubscribers()
        {
            using var messenger = CreateMessenger();
            var scope = new GameObject();
            var oldCount = 0;
            var freshCount = 0;

            try
            {
                var oldSubscriber = messenger.Subscriber.UnityScope(scope);
                using var cachedPublisher = messenger.Publisher.UnityScope(scope).Cache(() => new FirstMessage());
                oldSubscriber.Subscribe<FirstMessage>(() => { });
                oldSubscriber.Clear();
                oldSubscriber.Subscribe<FirstMessage>(() => oldCount++);
                messenger.Subscriber.UnityScope(scope).Subscribe<FirstMessage>(() => freshCount++);
                cachedPublisher.Publish(SilentContext());

                Assert.AreEqual(1, oldCount);
                Assert.AreEqual(1, freshCount);
            }
            finally
            {
                DestroyImmediate(scope);
            }
        }

        [Test]
        public void CapturedUnitySubscriber_ClearAfterDestroyRemovesHandlers()
        {
            using var messenger = CreateMessenger();
            var scope = new GameObject();
            var subscriber = messenger.Subscriber.UnityScope(scope);
            var publisher = messenger.Publisher.UnityScope(scope);
            var capturedId = subscriber.Scope.Value;
            var count = 0;
            subscriber.Subscribe<FirstMessage>(() => count++);

            DestroyImmediate(scope);
            subscriber.Clear();
            publisher.Publish(new FirstMessage(), SilentContext());

            Assert.AreEqual(capturedId, subscriber.Scope.Value);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void OldToken_AfterClearCannotRemoveReplacement()
        {
            using var messenger = CreateMessenger();
            var subscriber = messenger.Subscriber.Scope(new Scope(1));
            var count = 0;
            Action handler = () => count++;
            var oldSubscription = subscriber.Subscribe<FirstMessage>(handler);

            subscriber.Clear();
            using var replacement = subscriber.Subscribe<FirstMessage>(handler);
            oldSubscription.Dispose();
            messenger.Publisher.Scope(new Scope(1)).Publish(new FirstMessage(), SilentContext());

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Clear_PreservesSharedSubscriptionCollection()
        {
            using var messenger = CreateMessenger();
            var subscriptions = new List<ISubscription>();
            var targetCount = 0;
            var pivotCount = 0;
            var target = messenger.Subscriber.Scope(new Scope(1)).WithSubscriptions(subscriptions);
            var pivot = target.WithScope(new Scope(2));

            try
            {
                target.Subscribe<FirstMessage>(() => targetCount++);
                pivot.Subscribe<FirstMessage>(() => pivotCount++);
                target.Clear();

                Assert.AreSame(subscriptions, target.Subscriptions);
                Assert.AreEqual(2, subscriptions.Count);
                messenger.Publisher.Scope(new Scope(1)).Publish(new FirstMessage(), SilentContext());
                messenger.Publisher.Scope(new Scope(2)).Publish(new FirstMessage(), SilentContext());

                Assert.AreEqual(0, targetCount);
                Assert.AreEqual(1, pivotCount);
            }
            finally
            {
                subscriptions.Unsubscribe();
            }
        }

        [Test]
        public void StatefulSubscribers_ClearAndPreserveState()
        {
            using var messenger = CreateMessenger();
            var unityScope = new GameObject();
            var valueState = new State();
            var unityState = new State();
            var valueCount = 0;
            var unityCount = 0;

            try
            {
                var valueSubscriber = messenger.Subscriber.Scope(new Scope(1)).WithState(valueState);
                var unitySubscriber = messenger.Subscriber.UnityScope(unityScope).WithState(unityState);
                valueSubscriber.Subscribe<FirstMessage>(_ => valueCount++);
                unitySubscriber.Subscribe<FirstMessage>(_ => unityCount++);
                valueSubscriber.Clear();
                unitySubscriber.Clear();
                messenger.Publisher.Scope(new Scope(1)).Publish(new FirstMessage(), SilentContext());
                messenger.Publisher.UnityScope(unityScope).Publish(new FirstMessage(), SilentContext());

                Assert.AreSame(valueState, valueSubscriber.State);
                Assert.AreSame(unityState, unitySubscriber.State);
                Assert.AreEqual(0, valueCount);
                Assert.AreEqual(0, unityCount);
            }
            finally
            {
                DestroyImmediate(unityScope);
            }
        }

        [Test]
        public void Clear_IsIdempotentAndAllowsReplacement()
        {
            using var messenger = CreateMessenger();
            var subscriber = messenger.Subscriber.Scope(new Scope(1));
            var count = 0;
            subscriber.Subscribe<FirstMessage>(() => count++);

            subscriber.Clear();
            subscriber.Clear();
            subscriber.Subscribe<FirstMessage>(() => count++);
            messenger.Publisher.Scope(new Scope(1)).Publish(new FirstMessage(), SilentContext());

            Assert.AreEqual(1, count);
        }

        private static Messenger CreateMessenger()
            => new(ArrayPool<UnityTask>.Shared);

        private static PublishingContext SilentContext()
            => PublishingContext.Default(warnNoSubscriber: false);

        private static void DestroyImmediate(UnityEngine.Object obj)
        {
            if (ReferenceEquals(obj, null) == false)
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        private readonly struct FirstMessage : IMessage
        {
        }

        private readonly struct SecondMessage : IMessage
        {
        }

        private readonly struct Scope
        {
            public Scope(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private sealed class State
        {
        }
    }
}
