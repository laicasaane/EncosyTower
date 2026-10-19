using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Processing;
using EncosyTower.Tasks;
using EncosyTower.Types;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Processing
{
    public sealed class LoggerAndThrowingContractTests
    {
        [Test]
        public void ProcessingContext_FactoriesExposeExactDefaultsAndPreserveValues()
        {
            using var cancellation = new CancellationTokenSource();
            var logger = new StringBuilderLogger();
            var defaultValue = default(ProcessingContext);
            var defaultFactory = ProcessingContext.Default();
            var dropFactory = ProcessingContext.DropIfNoHandler();
            var waitFactory = ProcessingContext.WaitForHandler();

            Assert.AreEqual(ProcessingStrategy.DropIfNoHandler, defaultValue.Strategy);
            Assert.AreSame(DevLogger.Default, defaultValue.Logger);
            Assert.AreEqual(default(CancellationToken), defaultValue.Token);

            Assert.AreEqual(ProcessingStrategy.DropIfNoHandler, defaultFactory.Strategy);
            Assert.AreSame(DevLogger.Default, defaultFactory.Logger);
            Assert.AreEqual(default(CancellationToken), defaultFactory.Token);

            Assert.AreEqual(ProcessingStrategy.DropIfNoHandler, dropFactory.Strategy);
            Assert.AreSame(DevLogger.Default, dropFactory.Logger);
            Assert.AreEqual(default(CancellationToken), dropFactory.Token);

            Assert.AreEqual(ProcessingStrategy.WaitForHandler, waitFactory.Strategy);
            Assert.AreSame(DevLogger.Default, waitFactory.Logger);
            Assert.AreEqual(default(CancellationToken), waitFactory.Token);

            AssertContextPreservesValues(
                ProcessingContext.Default(logger: logger, callerInfo: default, token: cancellation.Token),
                ProcessingStrategy.DropIfNoHandler,
                logger,
                cancellation.Token
            );
            AssertContextPreservesValues(
                ProcessingContext.DropIfNoHandler(logger: logger, callerInfo: default, token: cancellation.Token),
                ProcessingStrategy.DropIfNoHandler,
                logger,
                cancellation.Token
            );
            AssertContextPreservesValues(
                ProcessingContext.WaitForHandler(logger, default, cancellation.Token),
                ProcessingStrategy.WaitForHandler,
                logger,
                cancellation.Token
            );
        }

        [Test]
        public void MissingTry_ReturnsHandlerNotFoundWithoutLogging()
        {
            using var processor = new Processor();
            var hub = processor.Global();

            ProcessErrorAssert.IsNotFound<GlobalScope, SyncRequest>(
                  hub.TryProcess(new SyncRequest(), context: default)
            );
            LogAssert.NoUnexpectedReceived();

            var logger = new StringBuilderLogger();

            ProcessErrorAssert.IsNotFound<GlobalScope, SyncRequest>(
                  hub.TryProcess(new SyncRequest(), ProcessingContext.Default(logger: logger))
            );
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void InvalidHubAndDuplicateRegistration_ThrowOrReturnWithoutLogging()
        {
            var invalidLogger = new StringBuilderLogger();

#if !DISABLE_ENCOSY_CHECKS
            var invalidHub = default(Processor.Hub<GlobalScope>);

            Assert.Throws<InvalidOperationException>(() => invalidHub.Unregister(default(TypeId), invalidLogger));
#endif

            Assert.AreEqual(0, invalidLogger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();

            using var processor = new Processor();
            var hub = processor.Global();
            var duplicateLogger = new StringBuilderLogger();
            Action<SyncRequest> first = static _ => { };
            Action<SyncRequest> second = static _ => { };

            Assert.IsTrue(hub.Register(first, duplicateLogger).IsSuccess);

            var duplicate = hub.Register(second, duplicateLogger);

            Assert.IsTrue(duplicate.IsError);
            Assert.IsTrue(duplicate.TryGetError(out var error));
            StringAssert.Contains("already been registered", error.ToMessage());
            Assert.AreEqual(0, duplicateLogger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task ThrowingAndSuccessfulProcessPaths_ProduceNoLog()
        {
            using var processor = new Processor();
            var hub = processor.Global();

            Assert.Throws<InvalidOperationException>(() => hub.Process(new SyncRequest()));
            Assert.Throws<InvalidOperationException>(() => hub.ProcessAsync(new AsyncRequest()));
            LogAssert.NoUnexpectedReceived();

            var logger = new StringBuilderLogger();
            var syncCount = 0;
            var asyncCount = 0;

            hub.Register<SyncRequest>(_ => syncCount++, logger);
            hub.Register<AsyncRequest>(
                _ =>
                {
                    asyncCount++;
                    return UnityTask.CompletedTask;
                },
                logger
            );

            var syncResult = hub.TryProcess(new SyncRequest(), ProcessingContext.Default(logger: logger));
            var asyncResult = await hub.TryProcessAsync(new AsyncRequest(), ProcessingContext.Default(logger: logger));

            Assert.IsTrue(syncResult.IsSuccess);
            Assert.IsTrue(asyncResult.IsSuccess);
            Assert.AreEqual(1, syncCount);
            Assert.AreEqual(1, asyncCount);
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task ContextualHandlers_ReceiveExactContextAcrossAllShapes()
        {
            using var processor = new Processor();
            using var cancellation = new CancellationTokenSource();
            var hub = processor.Global();
            var state = new State();
            var statefulHub = hub.WithState(state);
            var logger = new StringBuilderLogger();
            var context = new ProcessingContext() {
                Strategy = ProcessingStrategy.WaitForHandler,
                Logger = logger,
                Token = cancellation.Token,
            };
            var observed = default(ProcessingContext);

            hub.Register<StatelessSyncVoidContextRequest>(
                (_, actual) => observed = actual
            );
            hub.Process(new StatelessSyncVoidContextRequest(), context);
            AssertContextPreservesValues(observed, context.Strategy, logger, context.Token);

            hub.Register<StatelessSyncResultContextRequest, int>(
                (_, actual) =>
                {
                    observed = actual;
                    return 1;
                }
            );
            Assert.AreEqual(
                1,
                hub.Process<StatelessSyncResultContextRequest, int>(new StatelessSyncResultContextRequest(), context)
            );
            AssertContextPreservesValues(observed, context.Strategy, logger, context.Token);

            statefulHub.Register<StatefulSyncVoidContextRequest>(
                (_, _, actual) => observed = actual
            );
            hub.Process(new StatefulSyncVoidContextRequest(), context);
            AssertContextPreservesValues(observed, context.Strategy, logger, context.Token);

            statefulHub.Register<StatefulSyncResultContextRequest, int>(
                (_, _, actual) =>
                {
                    observed = actual;
                    return 2;
                }
            );
            Assert.AreEqual(
                2,
                hub.Process<StatefulSyncResultContextRequest, int>(new StatefulSyncResultContextRequest(), context)
            );
            AssertContextPreservesValues(observed, context.Strategy, logger, context.Token);

            hub.Register<StatelessAsyncVoidContextRequest>(
                (_, actual) =>
                {
                    observed = actual;
                    return UnityTask.CompletedTask;
                }
            );
            await hub.ProcessAsync(new StatelessAsyncVoidContextRequest(), context);
            AssertContextPreservesValues(observed, context.Strategy, logger, context.Token);

            hub.Register<StatelessAsyncResultContextRequest, int>(
                (_, actual) =>
                {
                    observed = actual;
                    return UnityTask.FromResult(3);
                }
            );
            Assert.AreEqual(
                3,
                await hub.ProcessAsync<StatelessAsyncResultContextRequest, int>(
                    new StatelessAsyncResultContextRequest(),
                    context
                )
            );
            AssertContextPreservesValues(observed, context.Strategy, logger, context.Token);

            statefulHub.Register<StatefulAsyncVoidContextRequest>(
                (_, _, actual) =>
                {
                    observed = actual;
                    return UnityTask.CompletedTask;
                }
            );
            await hub.ProcessAsync(new StatefulAsyncVoidContextRequest(), context);
            AssertContextPreservesValues(observed, context.Strategy, logger, context.Token);

            statefulHub.Register<StatefulAsyncResultContextRequest, int>(
                (_, _, actual) =>
                {
                    observed = actual;
                    return UnityTask.FromResult(4);
                }
            );
            Assert.AreEqual(
                4,
                await hub.ProcessAsync<StatefulAsyncResultContextRequest, int>(
                    new StatefulAsyncResultContextRequest(),
                    context
                )
            );
            AssertContextPreservesValues(observed, context.Strategy, logger, context.Token);

            GC.KeepAlive(state);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task ContextFreeHandlers_IgnoreCancelledInvocationContext()
        {
            using var processor = new Processor();
            using var cancellation = new CancellationTokenSource();
            var hub = processor.Global();
            var syncCount = 0;
            var asyncCount = 0;

            hub.Register<ContextFreeSyncRequest>(_ => syncCount++);
            hub.Register<ContextFreeAsyncRequest>(
                _ =>
                {
                    asyncCount++;
                    return UnityTask.CompletedTask;
                }
            );
            cancellation.Cancel();

            var context = ProcessingContext.DropIfNoHandler(token: cancellation.Token);

            hub.Process(new ContextFreeSyncRequest(), context);
            await hub.ProcessAsync(new ContextFreeAsyncRequest(), context);

            Assert.AreEqual(1, syncCount);
            Assert.AreEqual(1, asyncCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void DefaultResultPayloads_RemainSuccessfulHandlerResults()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var logger = new StringBuilderLogger();

            hub.Register<IntRequest, int>(static _ => 0, logger);
            hub.Register<BoolRequest, bool>(static _ => false, logger);
            hub.Register<StringRequest, string>(static _ => null, logger);

            Assert.AreEqual(0, hub.Process<IntRequest, int>(new IntRequest()));
            Assert.IsFalse(hub.Process<BoolRequest, bool>(new BoolRequest()));
            Assert.IsNull(hub.Process<StringRequest, string>(new StringRequest()));

            Assert.IsTrue(
                hub.TryProcess<IntRequest, int>(
                    new IntRequest(),
                    ProcessingContext.Default(logger: logger)
                ).TryGetValue(out var intResult)
            );
            Assert.AreEqual(0, intResult);
            Assert.IsTrue(
                hub.TryProcess<BoolRequest, bool>(
                    new BoolRequest(),
                    ProcessingContext.Default(logger: logger)
                ).TryGetValue(out var boolResult)
            );
            Assert.IsFalse(boolResult);

            var stringResult = hub.TryProcess<StringRequest, string>(
                new StringRequest(),
                ProcessingContext.Default(logger: logger)
            );

            Assert.IsTrue(stringResult.IsSuccess);
            Assert.IsNull(stringResult.GetValueOrDefault());
            Assert.IsNull(hub.Process<StringRequest, string>(new StringRequest()));
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        private static void AssertContextPreservesValues(
              in ProcessingContext context
            , ProcessingStrategy strategy
            , ILogger logger
            , CancellationToken token
        )
        {
            Assert.AreEqual(strategy, context.Strategy);
            Assert.AreSame(logger, context.Logger);
            Assert.AreEqual(token, context.Token);
        }

        private readonly struct SyncRequest : IRequest
        {
        }

        private readonly struct AsyncRequest : IAsyncRequest
        {
        }

        private readonly struct IntRequest : IRequest<int>
        {
        }

        private readonly struct BoolRequest : IRequest<bool>
        {
        }

        private readonly struct StringRequest : IRequest<string>
        {
        }

        private readonly struct StatelessSyncVoidContextRequest : IRequest
        {
        }

        private readonly struct StatelessSyncResultContextRequest : IRequest<int>
        {
        }

        private readonly struct StatefulSyncVoidContextRequest : IRequest
        {
        }

        private readonly struct StatefulSyncResultContextRequest : IRequest<int>
        {
        }

        private readonly struct StatelessAsyncVoidContextRequest : IAsyncRequest
        {
        }

        private readonly struct StatelessAsyncResultContextRequest : IAsyncRequest<int>
        {
        }

        private readonly struct StatefulAsyncVoidContextRequest : IAsyncRequest
        {
        }

        private readonly struct StatefulAsyncResultContextRequest : IAsyncRequest<int>
        {
        }

        private readonly struct ContextFreeSyncRequest : IRequest
        {
        }

        private readonly struct ContextFreeAsyncRequest : IAsyncRequest
        {
        }

        private sealed class State
        {
        }
    }
}
