using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Logging;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;

namespace EncosyTower.Tests.PubSub
{
    public sealed class HandlerResultTests
    {
        [Test]
        public async Task AllHandlerFamilies_ReturnSuccessAndPreserveInputs()
        {
            using var messenger = CreateMessenger();
            using var cancellation = new CancellationTokenSource();
            var subscriptions = new List<ISubscription>();
            var state = new State();
            var message = new Message();
            var logger = new StringBuilderLogger();
            var invocationCount = 0;
            Message receivedMessage = null;
            ILogger receivedLogger = null;

            void Observe(Message value, PublishingContext context)
            {
                invocationCount++;
                receivedMessage = value ?? receivedMessage;
                receivedLogger = context.Logger;
            }

            var subscriber = messenger.Subscriber.Global();
            var stateful = subscriber.WithState(state);

            subscriptions.Add(subscriber.Subscribe<Message>(() => Observe(null, default)));
            subscriptions.Add(subscriber.Subscribe<Message>(value => Observe(value, default)));
            subscriptions.Add(subscriber.Subscribe<Message>(
                context => Observe(null, context)
            ));

            subscriptions.Add(subscriber.Subscribe<Message>(Observe));

            subscriptions.Add(subscriber.Subscribe<Message>(
                () => {
                    Observe(null, default);
                    return UnityTask.CompletedTask;
                }
            ));

            subscriptions.Add(subscriber.Subscribe<Message>(
                value => {
                    Observe(value, default);
                    return UnityTask.CompletedTask;
                }
            ));

            subscriptions.Add(subscriber.Subscribe<Message>(
                context => {
                    Observe(null, context);
                    return UnityTask.CompletedTask;
                }
            ));

            subscriptions.Add(subscriber.Subscribe<Message>(
                (value, context) => {
                    Observe(value, context);
                    return UnityTask.CompletedTask;
                }
            ));

            subscriptions.Add(stateful.Subscribe<Message>(
                _ => Observe(null, default)
            ));

            subscriptions.Add(stateful.Subscribe<Message>(
                (_, value) => Observe(value, default)
            ));

            subscriptions.Add(stateful.Subscribe<Message>(
                (_, context) => Observe(null, context)
            ));

            subscriptions.Add(stateful.Subscribe<Message>(
                (_, value, context) => Observe(value, context)
            ));

            subscriptions.Add(stateful.Subscribe<Message>(
                _ => {
                    Observe(null, default);
                    return UnityTask.CompletedTask;
                }
            ));

            subscriptions.Add(stateful.Subscribe<Message>(
                (_, value) => {
                    Observe(value, default);
                    return UnityTask.CompletedTask;
                }
            ));

            subscriptions.Add(stateful.Subscribe<Message>(
                (_, context) => {
                    Observe(null, context);
                    return UnityTask.CompletedTask;
                }
            ));

            subscriptions.Add(stateful.Subscribe<Message>(
                (_, value, context) => {
                    Observe(value, context);
                    return UnityTask.CompletedTask;
                }

            ));

            try
            {
                cancellation.Cancel();
                var canceledContext = PublishingContext.Default(
                    warnNoSubscriber: false,
                    logger: logger,
                    token: cancellation.Token
                );

                foreach (var subscription in subscriptions)
                {
                    var result = InvokeHandle(GetHandler(subscription), message, canceledContext);

                    Assert.IsTrue(GetResultSuccess(result));
                    await GetResultTask(result);
                }

                Assert.AreEqual(0, invocationCount);

                var context = PublishingContext.Default(warnNoSubscriber: false, logger: logger);

                foreach (var subscription in subscriptions)
                {
                    var result = InvokeHandle(GetHandler(subscription), message, context);

                    Assert.IsTrue(GetResultSuccess(result));
                    await GetResultTask(result);
                }

                Assert.AreEqual(16, invocationCount);
                Assert.AreSame(message, receivedMessage);
                Assert.AreSame(logger, receivedLogger);
            }
            finally
            {
                subscriptions.Unsubscribe();
            }
        }

        [Test]
        public async Task AsyncHandler_ResultRetainsActualTask()
        {
            using var messenger = CreateMessenger();
            var completion = new TaskCompletionSource<object>();
            var expected = AwaitCompletionAsync(completion.Task);
            using var subscription = messenger.Subscriber.Global()
                .Subscribe<Message>(() => expected);
            var result = InvokeHandle(
                GetHandler(subscription),
                new Message(),
                PublishingContext.Default(warnNoSubscriber: false)
            );
            var actual = GetResultTask(result);

            Assert.IsTrue(GetResultSuccess(result));
            Assert.IsFalse(actual.IsCompleted);

            completion.SetResult(null);
            await actual;
        }

        [Test]
        public void InvalidResultGuard_ThrowsWithExactMessage()
        {
            var helper = typeof(Messenger).Assembly.GetType(
                  "EncosyTower.PubSub.ThrowHelper"
                , throwOnError: true
            );
            var method = helper.GetMethod(
                "ThrowIfHandlerResultIsInvalid",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            Assert.IsNotNull(method);

            var exception = Assert.Throws<TargetInvocationException>(
                () => method.Invoke(null, new object[] { false, typeof(HandlerResultTests) })
            );

            Assert.IsInstanceOf<InvalidOperationException>(exception.InnerException);
            Assert.AreEqual(
                $"The message handler of type {typeof(HandlerResultTests)} returned an invalid result.",
                exception.InnerException.Message
            );
        }

        private static Messenger CreateMessenger()
            => new(ArrayPool<UnityTask>.Shared);

        private static object GetHandler(ISubscription subscription)
        {
            var field = subscription.GetType().GetField("_handler", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(field);
            return field.GetValue(subscription);
        }

        private static object InvokeHandle(object handler, Message message, PublishingContext context)
        {
            var method = handler.GetType().GetMethod("Handle", BindingFlags.Instance | BindingFlags.Public);

            Assert.IsNotNull(method);
            return method.Invoke(handler, new object[] { message, context });
        }

        private static bool GetResultSuccess(object result)
            => (bool)result.GetType().GetProperty("IsSuccess").GetValue(result);

        private static UnityTask GetResultTask(object result)
            => (UnityTask)result.GetType().GetMethod("GetValueOrThrow").Invoke(result, Array.Empty<object>());

        private static async UnityTask AwaitCompletionAsync(Task task)
            => await task;

        private sealed class Message : IMessage
        {
        }

        private sealed class State
        {
        }
    }
}
