using System;
using System.Buffers;
using EncosyTower.Common;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.PubSub
{
    public sealed class MisuseGuardTests
    {
        [Test]
        public void NullMessage_ThrowsArgumentNullException_InEveryBuild()
        {
            using var messenger = CreateMessenger();
            var publisher = messenger.Publisher.Global();
            ClassMessage message = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => publisher.PublishAsync(message)
            );

            Assert.AreEqual("message", exception.ParamName);
            StringAssert.StartsWith("The message must not be null.", exception.Message);
            LogAssert.NoUnexpectedReceived();
        }

#if !DISABLE_ENCOSY_CHECKS
        [Test]
        public void DefaultPublisher_PublishAsync_ThrowsNotCreated()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => default(MessagePublisher.Publisher<GlobalScope>).PublishAsync(new StructMessage())
            );

            Assert.AreEqual(
                $"{typeof(MessagePublisher.Publisher<GlobalScope>)} is not created. "
                + "Get a publisher from MessagePublisher.Global() or MessagePublisher.Scope().",
                exception.Message
            );
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void DefaultCachedPublisher_PublishAsync_ThrowsNotCreated()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => default(CachedPublisher<GlobalScope, StructMessage>).PublishAsync()
            );

            Assert.AreEqual(
                $"{typeof(CachedPublisher<GlobalScope, StructMessage>)} is not created. "
                + "Get a cached publisher from MessagePublisher.GlobalCache(), MessagePublisher.Cache(), "
                + "MessagePublisher.UnityCache() or Publisher.Cache().",
                exception.Message
            );
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void DefaultUnityPublisher_PublishAsync_ThrowsNotCreated()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => default(MessagePublisher.UnityPublisher<GameObject>).PublishAsync<StructMessage>()
            );

            Assert.AreEqual(
                $"{typeof(MessagePublisher.UnityPublisher<GameObject>).Name} is not created. "
                + "Get a Unity publisher from MessagePublisher.UnityScope().",
                exception.Message
            );
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void DefaultSubscriber_Subscribe_ThrowsNotCreated()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => default(MessageSubscriber.Subscriber<GlobalScope>).Subscribe<StructMessage>(() => { })
            );

            Assert.AreEqual(
                $"{typeof(MessageSubscriber.Subscriber<GlobalScope>)} is not created. "
                + "Get a subscriber from MessageSubscriber.Global() or MessageSubscriber.Scope().",
                exception.Message
            );
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void DefaultInterceptors_AddInterceptor_ThrowsNotCreated()
        {
            IMessageInterceptor interceptor = new PassThroughInterceptor();

            var exception = Assert.Throws<InvalidOperationException>(
                () => default(MessageInterceptors).AddInterceptor(interceptor)
            );

            Assert.AreEqual(
                "MessageInterceptors is not created. Get it from Messenger.Interceptors.",
                exception.Message
            );
            LogAssert.NoUnexpectedReceived();
        }
#endif

        private static Messenger CreateMessenger()
            => new(ArrayPool<UnityTask>.Shared);

        private readonly struct StructMessage : IMessage
        {
        }

        private sealed class ClassMessage : IMessage
        {
        }

        private sealed class PassThroughInterceptor : IMessageInterceptor
        {
            public UnityTask InterceptAsync<TMessage>(
                  TMessage message
                , PublishingContext context
                , PublishContinuation<TMessage> continuation
            )
                where TMessage : IMessage
            {
                return continuation(message, context);
            }
        }
    }
}
