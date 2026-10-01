using System;
using System.Buffers;
using System.Linq;
using System.Reflection;
using System.Threading;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;

namespace EncosyTower.Tests.PubSub
{
    public sealed class SubscriptionIdentityTests
    {
        [Test]
        public void DuplicateRegistration_ReturnsSingletonAndTokenDoesNotOwnOriginal()
        {
            using var messenger = CreateMessenger();
            using var cancellation = new CancellationTokenSource();
            var subscriber = messenger.Subscriber.Global();
            var count = 0;
            Action handler = () => count++;
            using var original = subscriber.Subscribe<Message>(handler);
            using var duplicate = subscriber.Subscribe<Message>(handler);
            using var secondDuplicate = subscriber.Subscribe<Message>(handler);

            Assert.AreNotSame(original, duplicate);
            Assert.AreSame(duplicate, secondDuplicate);

            subscriber.Subscribe<Message>(handler, cancellation.Token);
            cancellation.Cancel();
            messenger.Publisher.Global().Publish(new Message(), SilentContext());

            Assert.AreEqual(1, count);
        }

        [Test]
        public void DuplicateStateRegistration_ReturnsSingletonSentinel()
        {
            using var messenger = CreateMessenger();
            var state = new State();
            var subscriber = messenger.Subscriber.Global().WithState(state);
            Action<State> handler = static _ => { };
            using var original = subscriber.Subscribe<Message>(handler, order: 2);
            using var duplicate = subscriber.Subscribe<Message>(handler, order: 2);
            using var secondDuplicate = subscriber.Subscribe<Message>(handler, order: 2);

            Assert.AreNotSame(original, duplicate);
            Assert.AreSame(duplicate, secondDuplicate);
        }

        [Test]
        public void OldHandle_AfterExactRemovalCannotRemoveSameIdReplacement()
        {
            using var messenger = CreateMessenger();
            var subscriber = messenger.Subscriber.Global();
            var count = 0;
            Action handler = () => count++;
            using var oldSubscription = subscriber.Subscribe<Message>(handler);
            var oldHandler = GetFieldValue(oldSubscription, "_handler");
            var brokerReference = GetFieldValue(oldSubscription, "_broker");
            var broker = GetWeakReferenceTarget(brokerReference);
            var removeHandler = broker.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(static method =>
                    method.Name == "RemoveHandler"
                    && method.GetParameters().Length == 2
                );

            var removed = (bool)removeHandler.Invoke(
                broker,
                new[] { oldHandler, 0 }
            );
            using var replacement = subscriber.Subscribe<Message>(handler);

            Assert.IsTrue(removed);
            Assert.DoesNotThrow(() => oldSubscription.Dispose());

            messenger.Publisher.Global().Publish(new Message(), SilentContext());

            Assert.AreEqual(1, count);
            GC.KeepAlive(oldHandler);
        }

        [Test]
        public void RepeatedDispose_IsHarmless()
        {
            using var messenger = CreateMessenger();
            var count = 0;
            var subscription = messenger.Subscriber.Global()
                .Subscribe<Message>(() => count++);

            subscription.Dispose();
            Assert.DoesNotThrow(() => subscription.Dispose());
            messenger.Publisher.Global().Publish(new Message(), SilentContext());

            Assert.AreEqual(0, count);
        }

        private static Messenger CreateMessenger()
            => new(ArrayPool<UnityTask>.Shared);

        private static object GetFieldValue(object owner, string fieldName)
        {
            var field = owner.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(field);
            return field.GetValue(owner);
        }

        private static object GetWeakReferenceTarget(object weakReference)
        {
            var method = weakReference.GetType().GetMethod("TryGetTarget", BindingFlags.Instance | BindingFlags.Public);
            var arguments = new object[] { null };

            Assert.IsNotNull(method);
            Assert.IsTrue((bool)method.Invoke(weakReference, arguments));
            Assert.IsNotNull(arguments[0]);
            return arguments[0];
        }

        private static PublishingContext SilentContext()
            => PublishingContext.Default(warnNoSubscriber: false);

        private readonly struct Message : IMessage
        {
        }

        private sealed class State
        {
        }
    }
}
