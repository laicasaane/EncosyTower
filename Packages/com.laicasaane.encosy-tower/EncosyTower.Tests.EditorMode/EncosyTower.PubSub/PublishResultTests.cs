using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.PubSub
{
    public sealed class PublishResultTests
    {
        private const int DEAD_HANDLER_COUNT = 8;

        [Test]
        public async Task SyncThrowInGroup_RunsEveryHandler_ReturnsThatException()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var failure = new InvalidOperationException("middle");
            var calls = 0;
            var subscriber = messenger.Subscriber.Global();

            void CountFirst()
            {
                calls++;
            }

            void CountThenThrow()
            {
                calls++;
                throw failure;
            }

            void CountLast()
            {
                calls++;
            }

            using var first = subscriber.Subscribe<Message>(CountFirst);
            using var middle = subscriber.Subscribe<Message>(CountThenThrow);
            using var last = subscriber.Subscribe<Message>(CountLast);

            var result = await messenger.Publisher.Global().PublishAsync(new Message(), SilentContext(logger));

            Assert.AreEqual(3, calls);
            var error = AssertFailure(result);
            AssertHandlerFaulted(error, failure);
            Assert.AreEqual(0, SuppressedCount(error));
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task AsyncFaultsInTwoGroups_PrimaryIsEarlierGroup_LaterIsSuppressed()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var earlyFailure = new InvalidOperationException("early");
            var lateFailure = new InvalidOperationException("late");
            var lateRan = false;
            var subscriber = messenger.Subscriber.Global();

            UnityTask FailEarly()
            {
                return UnityTask.FromException(earlyFailure);
            }

            UnityTask FailLate()
            {
                lateRan = true;
                return UnityTask.FromException(lateFailure);
            }

            using var late = subscriber.Subscribe<Message>(FailLate, order: -1);
            using var early = subscriber.Subscribe<Message>(FailEarly, order: 1);

            var result = await messenger.Publisher.Global().PublishAsync(new Message(), SilentContext(logger));

            Assert.IsTrue(lateRan);
            var error = AssertFailure(result);
            AssertHandlerFaulted(error, earlyFailure);
            Assert.AreEqual(1, SuppressedCount(error));
            AssertSuppressedHandlerFaulted(error, 0, lateFailure);
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task DeadStatefulSubscriber_ReturnsStateUnavailable_ThenHandlerIsGone()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var publisher = messenger.Publisher.Global();

            RegisterCollectedDeadHandlers(messenger);

            var result = await publisher.PublishAsync(new DeadWeakStateTests.Message(), SilentContext(logger));

            AssertAllStateUnavailable(AssertFailure(result), typeof(DeadWeakStateTests.State));

            var second = await publisher.PublishAsync(new DeadWeakStateTests.Message(), SilentContext(logger));

            Assert.IsTrue(AssertFailure(second).IsSubscriberNotFound);
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task NoSubscriber_ReturnsSubscriberNotFound_LogsNothing()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();

            var result = await messenger.Publisher.Global().PublishAsync(new Message(), SilentContext(logger));

            AssertSubscriberNotFound(AssertFailure(result), typeof(GlobalScope), typeof(Message));
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task CanceledHandlerTask_WithFaultingSibling_ThrowsAndLogsTheSiblingOnce()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var siblingFailure = new InvalidOperationException("sibling");
            var subscriber = messenger.Subscriber.Global();

            UnityTask FailSibling()
            {
                return UnityTask.FromException(siblingFailure);
            }

            using var canceled = subscriber.Subscribe<Message>(CreateCanceledTask);
            using var sibling = subscriber.Subscribe<Message>(FailSibling);

            var thrown = await CatchCancellationAsync(
                messenger.Publisher.Global().PublishAsync(new Message(), SilentContext(logger))
            );

            Assert.IsTrue(thrown);
            Assert.AreEqual(1, logger.LogEntryCount);
            StringAssert.Contains("sibling", logger.ToString());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task TokenCanceledByFirstGroup_ThrowsAfterThatGroup_LaterGroupDoesNotRun()
        {
            using var messenger = CreateMessenger();
            using var source = new CancellationTokenSource();
            var logger = new StringBuilderLogger();
            var laterRan = false;
            var subscriber = messenger.Subscriber.Global();

            void CancelToken()
            {
                source.Cancel();
            }

            void RecordLater()
            {
                laterRan = true;
            }

            using var first = subscriber.Subscribe<Message>(CancelToken, order: 1);
            using var later = subscriber.Subscribe<Message>(RecordLater, order: -1);

            var context = PublishingContext.Default(logger: logger, token: source.Token);
            var task = messenger.Publisher.Global().PublishAsync(new Message(), context);
            var thrown = await CatchCancellationAsync(task);

            Assert.IsTrue(thrown);
            Assert.IsFalse(laterRan);
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task ForwardingInterceptor_ReturnsTheHandlerFailure()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var failure = new InvalidOperationException("handler");

            void Throw()
            {
                throw failure;
            }

            messenger.Interceptors.AddInterceptor(new ForwardingInterceptor());
            using var subscription = messenger.Subscriber.Global().Subscribe<Message>(Throw);

            var result = await messenger.Publisher.Global().PublishAsync(new Message(), SilentContext(logger));

            AssertHandlerFaulted(AssertFailure(result), failure);
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task InterceptorWithoutContinuation_ReturnsSuccess()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var calls = 0;

            void Count()
            {
                calls++;
            }

            messenger.Interceptors.AddInterceptor(new SwallowingInterceptor());
            using var subscription = messenger.Subscriber.Global().Subscribe<Message>(Count);

            var result = await messenger.Publisher.Global().PublishAsync(new Message(), SilentContext(logger));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, calls);
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task ThrowingInterceptor_PropagatesItsException()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var interceptor = new ThrowingInterceptor();

            messenger.Interceptors.AddInterceptor(interceptor);
            using var subscription = messenger.Subscriber.Global().Subscribe<Message>(Ignore);

            Exception thrown = null;

            try
            {
                var result = await messenger.Publisher.Global().PublishAsync(new Message(), SilentContext(logger));

                Assert.Fail($"The publish must throw the interceptor's exception; it returned {result.IsSuccess}.");
            }
            catch (InvalidOperationException ex)
            {
                thrown = ex;
            }

            Assert.AreSame(interceptor.Failure, thrown);
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task InterceptorThrowsAfterAFailedContinuation_PropagatesTheException_LogsTheFailureOnce()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var failure = new InvalidOperationException("handler");

            void Throw()
            {
                throw failure;
            }

            messenger.Interceptors.AddInterceptor(new AsyncThrowingInterceptor());
            using var subscription = messenger.Subscriber.Global().Subscribe<Message>(Throw);

            Exception thrown = null;

            try
            {
                var result = await messenger.Publisher.Global().PublishAsync(new Message(), SilentContext(logger));

                Assert.Fail($"The publish must throw the interceptor's exception; it returned {result.IsSuccess}.");
            }
            catch (InvalidOperationException ex)
            {
                thrown = ex;
            }

            Assert.IsNotNull(thrown);
            Assert.AreEqual(AsyncThrowingInterceptor.MESSAGE, thrown.Message);
            Assert.AreEqual(1, logger.LogEntryCount);
            StringAssert.Contains("handler", logger.ToString());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task CachedPublisher_SyncThrowInGroup_RunsEveryHandler_ReturnsThatException()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var failure = new InvalidOperationException("middle");
            var calls = 0;
            var subscriber = messenger.Subscriber.Global();

            void CountFirst()
            {
                calls++;
            }

            void CountThenThrow()
            {
                calls++;
                throw failure;
            }

            void CountLast()
            {
                calls++;
            }

            using var first = subscriber.Subscribe<Message>(CountFirst);
            using var middle = subscriber.Subscribe<Message>(CountThenThrow);
            using var last = subscriber.Subscribe<Message>(CountLast);
            using var cached = messenger.Publisher.Global().Cache<Message>(static () => new Message());

            var result = await cached.PublishAsync(SilentContext(logger));

            Assert.AreEqual(3, calls);
            var error = AssertFailure(result);
            AssertHandlerFaulted(error, failure);
            Assert.AreEqual(0, SuppressedCount(error));
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task CachedPublisher_DeadStatefulSubscriber_ReturnsStateUnavailable_ThenHandlerIsGone()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            using var cached = messenger.Publisher.Global()
                .Cache<DeadWeakStateTests.Message>(static () => new DeadWeakStateTests.Message());

            RegisterCollectedDeadHandlers(messenger);

            var result = await cached.PublishAsync(SilentContext(logger));

            AssertAllStateUnavailable(AssertFailure(result), typeof(DeadWeakStateTests.State));

            var second = await cached.PublishAsync(SilentContext(logger));

            Assert.IsTrue(AssertFailure(second).IsSubscriberNotFound);
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task WaitForSubscriber_WaitsOnTheRequestedScopeOnly()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var requestedCount = 0;
            var otherCount = 0;

            void CountRequested()
            {
                requestedCount++;
            }

            void CountOther()
            {
                otherCount++;
            }

            var task = messenger.Publisher.Scope(new Scope(1))
                .PublishAsync(new Message(), PublishingContext.WaitForSubscriber(logger));

            using var other = messenger.Subscriber.Scope(new Scope(2)).Subscribe<Message>(CountOther);

            await UnityTask.NextFrameAsync();

            Assert.IsFalse(task.IsCompleted);
            Assert.AreEqual(0, otherCount);

            using var requested = messenger.Subscriber.Scope(new Scope(1)).Subscribe<Message>(CountRequested);

            var result = await task;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, requestedCount);
            Assert.AreEqual(0, otherCount);
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void SyncPublish_FaultingHandler_LogsOnceThroughContextLogger()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();
            var failure = new InvalidOperationException("handler");

            void Throw()
            {
                throw failure;
            }

            using var subscription = messenger.Subscriber.Global().Subscribe<Message>(Throw);

            messenger.Publisher.Global().Publish(new Message(), SilentContext(logger));

            Assert.AreEqual(1, logger.LogEntryCount);
            StringAssert.Contains("handler", logger.ToString());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void SyncPublish_SyncThrowingInterceptor_LogsOnceThroughContextLogger()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();

            messenger.Interceptors.AddInterceptor(new ThrowingInterceptor());
            using var subscription = messenger.Subscriber.Global().Subscribe<Message>(Ignore);

            messenger.Publisher.Global().Publish(new Message(), SilentContext(logger));

            Assert.AreEqual(1, logger.LogEntryCount);
            StringAssert.Contains(ThrowingInterceptor.MESSAGE, logger.ToString());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void SyncPublish_AsyncThrowingInterceptor_LogsOnceThroughContextLogger()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();

            messenger.Interceptors.AddInterceptor(new AsyncThrowingInterceptor());
            using var subscription = messenger.Subscriber.Global().Subscribe<Message>(Ignore);

            messenger.Publisher.Global().Publish(new Message(), SilentContext(logger));

            Assert.AreEqual(1, logger.LogEntryCount);
            StringAssert.Contains(AsyncThrowingInterceptor.MESSAGE, logger.ToString());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void SyncPublish_NoSubscriber_LogsOneWarningThroughContextLogger()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();

            messenger.Publisher.Global().Publish(new Message(), SilentContext(logger));

            Assert.AreEqual(1, logger.LogEntryCount);
            StringAssert.Contains("[Warning] Found no subscriber for", logger.ToString());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void SyncPublish_CanceledHandler_LogsNothing()
        {
            using var messenger = CreateMessenger();
            var logger = new StringBuilderLogger();

            using var subscription = messenger.Subscriber.Global().Subscribe<Message>(CreateCanceledTask);

            messenger.Publisher.Global().Publish(new Message(), SilentContext(logger));

            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        private static Messenger CreateMessenger()
            => new(ArrayPool<UnityTask>.Shared);

        private static PublishingContext SilentContext(EncosyTower.Logging.ILogger logger)
            => PublishingContext.Default(logger: logger);

        private static void Ignore()
        {
        }

        private static UnityTask CreateCanceledTask()
            => UnityTask.FromCanceled(new CancellationToken(canceled: true));

        private static async Task<bool> CatchCancellationAsync(UnityTask<Success<PublishError>> task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                return true;
            }

            return false;
        }

        private static void RegisterCollectedDeadHandlers(Messenger messenger)
        {
            var weakState = DeadWeakStateTests.RunOnShortLivedThread(
                () => DeadWeakStateTests.RegisterDeadHandlers(messenger)
            );

            DeadWeakStateTests.CollectGarbage(weakState);
        }

        private static PublishError AssertFailure(in Success<PublishError> result)
        {
            Assert.IsTrue(result.TryGetFailure(out var error));
            return error;
        }

        private static int SuppressedCount(in PublishError error)
            => error.SuppressedErrors.Length;

        private static void AssertHandlerFaulted(in PublishError error, Exception expected)
        {
            Assert.IsTrue(error.TryGetException(out var exception));
            Assert.AreSame(expected, exception);
        }

        private static void AssertSuppressedHandlerFaulted(in PublishError error, int index, Exception expected)
        {
            var suppressed = error.SuppressedErrors;

            Assert.Less(index, suppressed.Length);
            AssertHandlerFaulted(suppressed[index], expected);
        }

        private static void AssertAllStateUnavailable(in PublishError error, Type stateType)
        {
            Assert.IsTrue(error.TryGetStateType(out var primaryStateType));
            Assert.AreEqual(stateType, primaryStateType);

            var suppressed = error.SuppressedErrors;
            var count = suppressed.Length;

            Assert.AreEqual(DEAD_HANDLER_COUNT - 1, count);

            for (var i = 0; i < count; i++)
            {
                Assert.IsTrue(suppressed[i].TryGetStateType(out var suppressedStateType));
                Assert.AreEqual(stateType, suppressedStateType);
            }
        }

        private static void AssertSubscriberNotFound(in PublishError error, Type scopeType, Type messageType)
        {
            Assert.IsTrue(error.IsSubscriberNotFound);
            Assert.IsTrue(error._error.TryGetValue(out PublishError.Error.SubscriberNotFound notFound));
            Assert.AreEqual(scopeType, notFound.ScopeType);
            Assert.AreEqual(messageType, notFound.MessageType);
            Assert.AreEqual(0, SuppressedCount(error));
        }

        private sealed class Message : IMessage
        {
        }

        private readonly struct Scope : IEquatable<Scope>
        {
            private readonly int _value;

            public Scope(int value)
            {
                _value = value;
            }

            public bool Equals(Scope other)
                => _value == other._value;

            public override bool Equals(object obj)
                => obj is Scope other && Equals(other);

            public override int GetHashCode()
                => _value.GetHashCode();
        }

        private sealed class ForwardingInterceptor : IMessageInterceptor<Message>
        {
            public UnityTask InterceptAsync(
                  Message message
                , PublishingContext context
                , PublishContinuation<Message> continuation
            )
            {
                return continuation(message, context);
            }
        }

        private sealed class SwallowingInterceptor : IMessageInterceptor<Message>
        {
            public UnityTask InterceptAsync(
                  Message message
                , PublishingContext context
                , PublishContinuation<Message> continuation
            )
            {
                return UnityTask.CompletedTask;
            }
        }

        private sealed class ThrowingInterceptor : IMessageInterceptor<Message>
        {
            public const string MESSAGE = "sync interceptor";

            public readonly InvalidOperationException Failure = new(MESSAGE);

            public UnityTask InterceptAsync(
                  Message message
                , PublishingContext context
                , PublishContinuation<Message> continuation
            )
            {
                throw Failure;
            }
        }

        private sealed class AsyncThrowingInterceptor : IMessageInterceptor<Message>
        {
            public const string MESSAGE = "async interceptor";

            public async UnityTask InterceptAsync(
                  Message message
                , PublishingContext context
                , PublishContinuation<Message> continuation
            )
            {
                await continuation(message, context);
                throw new InvalidOperationException(MESSAGE);
            }
        }
    }
}
