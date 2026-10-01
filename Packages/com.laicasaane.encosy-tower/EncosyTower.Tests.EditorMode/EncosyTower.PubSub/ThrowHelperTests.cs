using System;
using System.Buffers;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.PubSub
{
    public sealed class ThrowHelperTests
    {
        [Test]
        public void Messenger_NullTaskArrayPool_ThrowsBeforeConstruction()
        {
            ArrayPool<UnityTask> taskArrayPool = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => new Messenger(taskArrayPool)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(taskArrayPool)));
        }

        [Test]
        public void MessageInterceptors_NullInterceptor_ThrowsBeforeBrokerLookup()
        {
            using var messenger = CreateMessenger();
            IMessageInterceptor interceptor = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => messenger.Interceptors.AddInterceptor(interceptor)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(interceptor)));
        }

        [Test]
        public void MessageSubscriber_NullHandler_ThrowsBeforeDelegateCapture()
        {
            using var messenger = CreateMessenger();
            Action handler = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => messenger.Subscriber.Global().Subscribe<Message>(handler)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(handler)));
        }

        [Test]
        public void MessagePublisher_NullCreateFunc_ThrowsBeforeCacheLookup()
        {
            using var messenger = CreateMessenger();
            Func<Message> createFunc = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => messenger.Publisher.GlobalCache(createFunc)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(createFunc)));
        }

        [Test]
        public void MessageSubscriber_NullState_ThrowsBeforeSubscriberConstruction()
        {
            using var messenger = CreateMessenger();
            State state = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => messenger.Subscriber.Global().WithState(state)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(state)));
        }

        [Test]
        public void MessageSubscriber_DestroyedUnityState_ThrowsBeforeSubscriberConstruction()
        {
            using var messenger = CreateMessenger();
            var state = new GameObject();
            UnityEngine.Object.DestroyImmediate(state);

            var exception = Assert.Throws<ArgumentNullException>(
                () => messenger.Subscriber.Global().WithState(state)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(state)));
        }

        [Test]
        public void MessagePublisher_DestroyedUnityScope_ThrowsBeforeBrokerLookup()
        {
            using var messenger = CreateMessenger();
            var scope = new GameObject();
            UnityEngine.Object.DestroyImmediate(scope);

            var exception = Assert.Throws<ArgumentNullException>(
                () => messenger.Publisher.UnityScope(scope)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(scope)));
        }

        private static Messenger CreateMessenger()
            => new(ArrayPool<UnityTask>.Shared);

        private readonly struct Message : IMessage
        {
        }

        private sealed class State
        {
        }
    }
}
