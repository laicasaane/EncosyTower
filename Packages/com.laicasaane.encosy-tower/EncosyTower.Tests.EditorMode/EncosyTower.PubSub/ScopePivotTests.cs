using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Logging;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.PubSub
{
    public class ScopePivotTests
    {
        [Test]
        public void PublisherPivots_ChangeOnlyScopeAndPreserveRoot()
        {
            using var firstMessenger = CreateMessenger();
            using var secondMessenger = CreateMessenger();
            var firstCount = 0;
            var secondCount = 0;
            var subscriptions = new List<ISubscription>();

            try
            {
                var source = firstMessenger.Publisher.Scope(new ScopeA(1));
                var sameType = source.WithScope(new ScopeA(2));
                var otherType = source.WithScope(new ScopeB(3));
                var global = source.WithGlobalScope();

                subscriptions.Add(
                    firstMessenger.Subscriber.Scope(new ScopeB(3))
                        .Subscribe<Message>(() => firstCount++)
                );
                subscriptions.Add(
                    secondMessenger.Subscriber.Scope(new ScopeB(3))
                        .Subscribe<Message>(() => secondCount++)
                );

                otherType.Publish(new Message(), SilentContext());

                Assert.AreEqual(1, source.Scope.Value);
                Assert.AreEqual(2, sameType.Scope.Value);
                Assert.AreEqual(3, otherType.Scope.Value);
                Assert.IsTrue(global.IsCreated);
                Assert.AreEqual(1, firstCount);
                Assert.AreEqual(0, secondCount);
            }
            finally
            {
                subscriptions.Unsubscribe();
            }
        }

        [Test]
        public void UnityPublisherPivots_ChangeOnlyScopeAndPreserveRoot()
        {
            using var firstMessenger = CreateMessenger();
            using var secondMessenger = CreateMessenger();
            var firstScope = new GameObject();
            var secondScope = new GameObject();
            var firstCount = 0;
            var secondCount = 0;
            var subscriptions = new List<ISubscription>();

            try
            {
                var source = firstMessenger.Publisher.UnityScope(firstScope);
                var normal = source.WithScope(new ScopeA(2));
                var global = source.WithGlobalScope();
                var unity = source.WithUnityScope(secondScope);

                subscriptions.Add(
                    firstMessenger.Subscriber.UnityScope(secondScope)
                        .Subscribe<Message>(() => firstCount++)
                );
                subscriptions.Add(
                    secondMessenger.Subscriber.UnityScope(secondScope)
                        .Subscribe<Message>(() => secondCount++)
                );

                unity.Publish(new Message(), SilentContext());

                Assert.AreEqual(firstScope.GetEntityId(), source.Scope.Value);
                Assert.AreEqual(2, normal.Scope.Value);
                Assert.IsTrue(global.IsCreated);
                Assert.AreEqual(secondScope.GetEntityId(), unity.Scope.Value);
                Assert.AreEqual(1, firstCount);
                Assert.AreEqual(0, secondCount);
            }
            finally
            {
                subscriptions.Unsubscribe();
                DestroyImmediate(firstScope);
                DestroyImmediate(secondScope);
            }
        }

        [Test]
        public void SubscriberPivots_PreserveSubscriptionCollectionReference()
        {
            using var messenger = CreateMessenger();
            var subscriptions = new List<ISubscription>();
            var source = messenger.Subscriber.Scope(new ScopeA(1)).WithSubscriptions(subscriptions);

            var sameType = source.WithScope(new ScopeA(2));
            var otherType = source.WithScope(new ScopeB(3));
            var global = source.WithGlobalScope();

            Assert.AreSame(subscriptions, source.Subscriptions);
            Assert.AreSame(subscriptions, sameType.Subscriptions);
            Assert.AreSame(subscriptions, otherType.Subscriptions);
            Assert.AreSame(subscriptions, global.Subscriptions);
            Assert.AreEqual(1, source.Scope.Value);
        }

        [Test]
        public void StatefulSubscriberPivots_PreserveStateAndSubscriptionReferences()
        {
            using var messenger = CreateMessenger();
            var subscriptions = new List<ISubscription>();
            var state = new State();
            var source = messenger.Subscriber.Scope(new ScopeA(1)).WithSubscriptions(subscriptions).WithState(state);

            var sameType = source.WithScope(new ScopeA(2));
            var otherType = source.WithScope(new ScopeB(3));
            var global = source.WithGlobalScope();

            Assert.AreSame(state, sameType.State);
            Assert.AreSame(state, otherType.State);
            Assert.AreSame(state, global.State);
            Assert.AreSame(subscriptions, sameType.Subscriptions);
            Assert.AreSame(subscriptions, otherType.Subscriptions);
            Assert.AreSame(subscriptions, global.Subscriptions);
        }

        [Test]
        public void UnitySubscriberPivots_PreserveSubscriptionCollectionReference()
        {
            using var messenger = CreateMessenger();
            var firstScope = new GameObject();
            var secondScope = new GameObject();
            var subscriptions = new List<ISubscription>();

            try
            {
                var source = messenger.Subscriber.UnityScope(firstScope).WithSubscriptions(subscriptions);
                var normal = source.WithScope(new ScopeA(2));
                var global = source.WithGlobalScope();
                var unity = source.WithUnityScope(secondScope);

                Assert.AreSame(subscriptions, source.Subscriptions);
                Assert.AreSame(subscriptions, normal.Subscriptions);
                Assert.AreSame(subscriptions, global.Subscriptions);
                Assert.AreSame(subscriptions, unity.Subscriptions);
            }
            finally
            {
                DestroyImmediate(firstScope);
                DestroyImmediate(secondScope);
            }
        }

        [Test]
        public void StatefulUnitySubscriberPivots_PreserveStateAndSubscriptionReferences()
        {
            using var messenger = CreateMessenger();
            var firstScope = new GameObject();
            var secondScope = new GameObject();
            var subscriptions = new List<ISubscription>();
            var state = new State();

            try
            {
                var source = messenger.Subscriber.UnityScope(firstScope)
                    .WithSubscriptions(subscriptions)
                    .WithState(state);
                var normal = source.WithScope(new ScopeA(2));
                var global = source.WithGlobalScope();
                var unity = source.WithUnityScope(secondScope);

                Assert.AreSame(state, normal.State);
                Assert.AreSame(state, global.State);
                Assert.AreSame(state, unity.State);
                Assert.AreSame(subscriptions, normal.Subscriptions);
                Assert.AreSame(subscriptions, global.Subscriptions);
                Assert.AreSame(subscriptions, unity.Subscriptions);
            }
            finally
            {
                DestroyImmediate(firstScope);
                DestroyImmediate(secondScope);
            }
        }

        [Test]
        public void DefaultWrappers_AllPivotsRemainInvalidWithoutThrowing()
        {
            var unityScope = new GameObject();
            var state = new State();

            try
            {
                Assert.IsFalse(default(MessagePublisher.Publisher<ScopeA>).WithScope(new ScopeB(1)).IsCreated);
                Assert.IsFalse(default(MessagePublisher.Publisher<ScopeA>).WithGlobalScope().IsCreated);
                Assert.IsFalse(default(MessagePublisher.Publisher<ScopeA>).WithUnityScope(unityScope).IsCreated);
                Assert.IsFalse(default(MessagePublisher.UnityPublisher<GameObject>).WithScope(new ScopeA(1)).IsCreated);
                Assert.IsFalse(default(MessageSubscriber.Subscriber<ScopeA>).WithScope(new ScopeB(1)).IsCreated);
                Assert.IsFalse(default(MessageSubscriber.Subscriber<ScopeA>).WithUnityScope(unityScope).IsCreated);
                Assert.IsFalse(
                    default(MessageSubscriber.Subscriber<ScopeA>).WithState(state).WithGlobalScope().IsCreated
                );
                Assert.IsFalse(
                    default(MessageSubscriber.UnitySubscriber<GameObject>)
                        .WithState(state)
                        .WithScope(new ScopeA(1))
                        .IsCreated
                );
            }
            finally
            {
                DestroyImmediate(unityScope);
            }
        }

        [Test]
        public void DefaultPivotedWrappers_ConsumeUsingExistingValidationContract()
        {
            var logger = new StringBuilderLogger();
            var publisher = default(MessagePublisher.Publisher<ScopeA>).WithScope(new ScopeB(1));
            var subscriber = default(MessageSubscriber.Subscriber<ScopeA>).WithScope(new ScopeB(1));

#if DISABLE_ENCOSY_CHECKS
            Assert.Throws<NullReferenceException>(
                () => publisher.Publish(new Message(), SilentContext(logger))
            );
            Assert.Throws<NullReferenceException>(
                () => subscriber.Subscribe<Message>(() => { }, logger: logger)
            );
#else
            Assert.DoesNotThrow(
                () => publisher.Publish(new Message(), SilentContext(logger))
            );
            Assert.DoesNotThrow(
                () => subscriber.Subscribe<Message>(() => { }, logger: logger)
            );
            Assert.AreEqual(2, logger.LogEntryCount);
#endif
        }

        [Test]
        public void Pivots_DoNotMoveExistingSubscriptionsBetweenBrokers()
        {
            using var messenger = CreateMessenger();
            var subscriptions = new List<ISubscription>();
            var sourceCount = 0;
            var pivotCount = 0;

            try
            {
                var source = messenger.Subscriber.Scope(new ScopeA(1)).WithSubscriptions(subscriptions);
                source.Subscribe<Message>(() => sourceCount++);
                var pivot = source.WithScope(new ScopeA(2));
                pivot.Subscribe<Message>(() => pivotCount++);

                messenger.Publisher.Scope(new ScopeA(2)).Publish(new Message(), SilentContext());

                Assert.AreEqual(0, sourceCount);
                Assert.AreEqual(1, pivotCount);

                messenger.Publisher.Scope(new ScopeA(1)).Publish(new Message(), SilentContext());

                Assert.AreEqual(1, sourceCount);
                Assert.AreEqual(1, pivotCount);
                Assert.AreEqual(2, subscriptions.Count);
            }
            finally
            {
                subscriptions.Unsubscribe();
            }
        }

        [Test]
        public void GlobalPivot_UsesGlobalRouteAndPreservesOriginalSubscription()
        {
            using var messenger = CreateMessenger();
            var subscriptions = new List<ISubscription>();
            var sourceCount = 0;
            var globalCount = 0;

            try
            {
                var source = messenger.Subscriber.Scope(new ScopeA(1)).WithSubscriptions(subscriptions);
                source.Subscribe<Message>(() => sourceCount++);
                var global = source.WithGlobalScope();
                global.Subscribe<Message>(() => globalCount++);

                messenger.Publisher.Global().Publish(new Message(), SilentContext());

                Assert.AreEqual(0, sourceCount);
                Assert.AreEqual(1, globalCount);

                messenger.Publisher.Scope(new ScopeA(1)).Publish(new Message(), SilentContext());

                Assert.AreEqual(1, sourceCount);
                Assert.AreEqual(1, globalCount);
            }
            finally
            {
                subscriptions.Unsubscribe();
            }
        }

        [Test]
        public void UnityPivot_UsesCapturedUnityRouteAndPreservesOriginalSubscription()
        {
            using var messenger = CreateMessenger();
            var firstScope = new GameObject();
            var secondScope = new GameObject();
            var subscriptions = new List<ISubscription>();
            var firstCount = 0;
            var secondCount = 0;

            try
            {
                var source = messenger.Subscriber.UnityScope(firstScope).WithSubscriptions(subscriptions);
                source.Subscribe<Message>(() => firstCount++);
                var pivot = source.WithUnityScope(secondScope);
                pivot.Subscribe<Message>(() => secondCount++);

                messenger.Publisher.UnityScope(secondScope).Publish(new Message(), SilentContext());

                Assert.AreEqual(0, firstCount);
                Assert.AreEqual(1, secondCount);

                messenger.Publisher.UnityScope(firstScope).Publish(new Message(), SilentContext());

                Assert.AreEqual(1, firstCount);
                Assert.AreEqual(1, secondCount);
            }
            finally
            {
                subscriptions.Unsubscribe();
                DestroyImmediate(firstScope);
                DestroyImmediate(secondScope);
            }
        }

        [Test]
        public void LaterSubscriptions_AreAddedToPreservedCollection()
        {
            using var messenger = CreateMessenger();
            var subscriptions = new List<ISubscription>();
            var source = messenger.Subscriber.Scope(new ScopeA(1)).WithSubscriptions(subscriptions);
            var pivot = source.WithScope(new ScopeB(2));

            try
            {
                var subscription = pivot.Subscribe<Message>(() => { });

                Assert.AreEqual(1, subscriptions.Count);
                Assert.AreSame(subscription, subscriptions[0]);
                Assert.AreSame(subscriptions, pivot.Subscriptions);
            }
            finally
            {
                subscriptions.Unsubscribe();
            }
        }

        [Test]
        public void WithUnityScope_NullObjectMatchesRootFactoryBehavior()
        {
            using var messenger = CreateMessenger();
            GameObject scope = null;

#if DISABLE_ENCOSY_CHECKS
            Assert.Throws<NullReferenceException>(
                () => messenger.Publisher.UnityScope(scope)
            );
            Assert.Throws<NullReferenceException>(
                () => messenger.Publisher.Global().WithUnityScope(scope)
            );
#else
            var rootException = Assert.Throws<ArgumentNullException>(
                () => messenger.Publisher.UnityScope(scope)
            );
            var pivotException = Assert.Throws<ArgumentNullException>(
                () => messenger.Publisher.Global().WithUnityScope(scope)
            );

            Assert.AreEqual(nameof(scope), rootException.ParamName);
            Assert.AreEqual(nameof(scope), pivotException.ParamName);
#endif
        }

        [Test]
        public void WithUnityScope_DestroyedObjectMatchesRootFactoryBehavior()
        {
            using var messenger = CreateMessenger();
            var rootScope = new GameObject();
            var pivotScope = new GameObject();
            DestroyImmediate(rootScope);
            DestroyImmediate(pivotScope);

#if DISABLE_ENCOSY_CHECKS
            var root = messenger.Publisher.UnityScope(rootScope);
            var pivot = messenger.Publisher.Global().WithUnityScope(pivotScope);

            Assert.IsFalse(root.Scope.IsValid);
            Assert.IsFalse(pivot.Scope.IsValid);
#else
            var rootException = Assert.Throws<ArgumentNullException>(
                () => messenger.Publisher.UnityScope(rootScope)
            );
            var pivotException = Assert.Throws<ArgumentNullException>(
                () => messenger.Publisher.Global().WithUnityScope(pivotScope)
            );

            Assert.AreEqual("scope", rootException.ParamName);
            Assert.AreEqual("scope", pivotException.ParamName);
#endif
        }

        [Test]
        public void UnityScope_DestroyedAfterCaptureRetainsCapturedEntityId()
        {
            using var messenger = CreateMessenger();
            var scope = new GameObject();
            var publisher = messenger.Publisher.Global().WithUnityScope(scope);
            var capturedId = publisher.Scope.Value;

            DestroyImmediate(scope);

            Assert.AreEqual(capturedId, publisher.Scope.Value);
            Assert.IsFalse(publisher.Scope.IsValid);
        }

        [Test]
        public Task PublishAsync_Success_ReturnsTaskBufferOnceAndClears()
            => VerifyTaskArrayPoolReturnAsync(HandlerCompletion.Success);

        [Test]
        public Task PublishAsync_Fault_ReturnsTaskBufferOnceAndClears()
            => VerifyTaskArrayPoolReturnAsync(HandlerCompletion.Fault);

        [Test]
        public Task PublishAsync_Cancellation_ReturnsTaskBufferOnceAndClears()
            => VerifyTaskArrayPoolReturnAsync(HandlerCompletion.Cancellation);

        private static Messenger CreateMessenger()
            => new(ArrayPool<UnityTask>.Shared);

        private static async Task VerifyTaskArrayPoolReturnAsync(HandlerCompletion completion)
        {
            var taskArrayPool = new RecordingUnityTaskArrayPool();
            var logger = new StringBuilderLogger();
            using var messenger = new Messenger(taskArrayPool);
            using var subscription = messenger.Subscriber.Global()
                .Subscribe<Message>(() => CreateHandlerTask(completion));
            var completionObserved = completion == HandlerCompletion.Success;

            try
            {
                await messenger.Publisher.Global().PublishAsync(new Message(), SilentContext(logger));
            }
            catch (InvalidOperationException) when (completion == HandlerCompletion.Fault)
            {
                completionObserved = true;
            }
            catch (OperationCanceledException) when (
                completion == HandlerCompletion.Cancellation
            )
            {
                completionObserved = true;
            }

            if (!completionObserved && logger.LogEntryCount == 1)
            {
                completionObserved = completion switch {
                    HandlerCompletion.Fault => logger.ToString().Contains(nameof(InvalidOperationException)),
                    HandlerCompletion.Cancellation => logger.ToString().Contains(nameof(OperationCanceledException))
                        || logger.ToString().Contains(nameof(TaskCanceledException)),
                    _ => false,
                };
            }

            Assert.IsTrue(completionObserved);
            Assert.AreEqual(1, taskArrayPool.RentCount);
            Assert.AreEqual(1, taskArrayPool.ReturnCount);
            Assert.AreSame(taskArrayPool.RentedArray, taskArrayPool.ReturnedArray);
            Assert.IsTrue(taskArrayPool.ClearArray);
        }

        private static async UnityTask CreateHandlerTask(HandlerCompletion completion)
            => await (completion switch {
                HandlerCompletion.Success => Task.CompletedTask,
                HandlerCompletion.Fault => Task.FromException(new InvalidOperationException()),
                HandlerCompletion.Cancellation => Task.FromCanceled(new CancellationToken(canceled: true)),
                _ => Task.CompletedTask,
            });

        private static PublishingContext SilentContext(EncosyTower.Logging.ILogger logger = null)
            => PublishingContext.Default(warnNoSubscriber: false, logger: logger);

        private static void DestroyImmediate(UnityEngine.Object obj)
        {
            if (ReferenceEquals(obj, null) == false)
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        private readonly struct Message : IMessage
        {
        }

        private readonly struct ScopeA
        {
            public ScopeA(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct ScopeB
        {
            public ScopeB(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private sealed class State
        {
        }

        private sealed class RecordingUnityTaskArrayPool : ArrayPool<UnityTask>
        {
            private readonly UnityTask[] _array = new UnityTask[4];

            public int RentCount { get; private set; }

            public int ReturnCount { get; private set; }

            public UnityTask[] RentedArray { get; private set; }

            public UnityTask[] ReturnedArray { get; private set; }

            public bool ClearArray { get; private set; }

            public override UnityTask[] Rent(int minimumLength)
            {
                RentCount++;
                RentedArray = _array;
                return _array;
            }

            public override void Return(UnityTask[] array, bool clearArray = false)
            {
                ReturnCount++;
                ReturnedArray = array;
                ClearArray = clearArray;

                if (clearArray)
                {
                    Array.Clear(array, 0, array.Length);
                }
            }
        }

        private enum HandlerCompletion
        {
            Success,
            Fault,
            Cancellation,
        }
    }
}
