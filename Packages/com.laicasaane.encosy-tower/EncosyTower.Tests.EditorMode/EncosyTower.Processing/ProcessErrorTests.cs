using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Processing;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Processing
{
    public sealed class ProcessErrorTests
    {
        [Test]
        public async Task TryProcess_UnregisteredRequest_ReturnsHandlerNotFoundWithoutLogging()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var logger = new StringBuilderLogger();
            var context = ProcessingContext.Default(logger: logger);

            ProcessErrorAssert.IsHandlerNotFound(
                  ProcessErrorAssert.GetError(hub.TryProcess(new SyncVoidRequest(), context))
                , typeof(GlobalScope)
                , typeof(SyncVoidRequest)
                , resultType: null
            );
            ProcessErrorAssert.IsHandlerNotFound(
                  ProcessErrorAssert.GetError(hub.TryProcess<SyncResultRequest, int>(new SyncResultRequest(), context))
                , typeof(GlobalScope)
                , typeof(SyncResultRequest)
                , typeof(int)
            );
            ProcessErrorAssert.IsHandlerNotFound(
                  ProcessErrorAssert.GetError(await hub.TryProcessAsync(new AsyncVoidRequest(), context))
                , typeof(GlobalScope)
                , typeof(AsyncVoidRequest)
                , resultType: null
            );
            ProcessErrorAssert.IsHandlerNotFound(
                  ProcessErrorAssert.GetError(
                      await hub.TryProcessAsync<AsyncResultRequest, int>(new AsyncResultRequest(), context)
                  )
                , typeof(GlobalScope)
                , typeof(AsyncResultRequest)
                , typeof(int)
            );

            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task TryProcess_RequestRegisteredWithOtherResultType_ReturnsHandlerTypeMismatch()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var logger = new StringBuilderLogger();
            var context = ProcessingContext.Default(logger: logger);

            hub.Register<MismatchRequest>(static _ => UnityTask.CompletedTask);

            var syncResult = hub.TryProcess<MismatchRequest, UnityTask>(new MismatchRequest(), context);

            ProcessErrorAssert.IsHandlerTypeMismatch(
                  ProcessErrorAssert.GetError(syncResult)
                , typeof(GlobalScope)
                , typeof(MismatchRequest)
                , typeof(EncosyTower.Processing.Internals.Async.AsyncProcessHandler<MismatchRequest>)
            );

            hub.Clear();
            hub.Register<MismatchRequest, UnityTask>(static _ => UnityTask.CompletedTask);

            ProcessErrorAssert.IsHandlerTypeMismatch(
                  ProcessErrorAssert.GetError(await hub.TryProcessAsync(new MismatchRequest(), context))
                , typeof(GlobalScope)
                , typeof(MismatchRequest)
                , typeof(EncosyTower.Processing.Internals.Sync.ProcessHandler<MismatchRequest, UnityTask>)
            );

            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void TryProcess_DeadState_ReturnsStateUnavailableAndUnregistersHandler()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var logger = new StringBuilderLogger();
            var context = ProcessingContext.Default(logger: logger);
            var weakState = DeadWeakStateTests.RunOnShortLivedThread(() => RegisterStatefulHandler(hub));

            DeadWeakStateTests.CollectGarbage(weakState);

            var first = ProcessErrorAssert.GetError(hub.TryProcess(new StatefulRequest(), context));

            ProcessErrorAssert.IsStateUnavailable(first, typeof(State));
            StringAssert.Contains("is not alive anymore", first.ToMessage());

            ProcessErrorAssert.IsHandlerNotFound(
                  ProcessErrorAssert.GetError(hub.TryProcess(new StatefulRequest(), context))
                , typeof(GlobalScope)
                , typeof(StatefulRequest)
                , resultType: null
            );

            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task TryProcess_HandlerReturningNull_IsSuccess()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var logger = new StringBuilderLogger();
            var context = ProcessingContext.Default(logger: logger);

            hub.Register<NullRequest, string>(static _ => null);
            hub.Register<AsyncNullRequest, string>(static _ => UnityTask.FromResult<string>(null));

            var syncResult = hub.TryProcess<NullRequest, string>(new NullRequest(), context);
            var asyncResult = await hub.TryProcessAsync<AsyncNullRequest, string>(new AsyncNullRequest(), context);

            Assert.IsTrue(syncResult.IsSuccess);
            Assert.IsNull(syncResult.GetValueOrDefault());
            Assert.IsTrue(asyncResult.IsSuccess);
            Assert.IsNull(asyncResult.GetValueOrDefault());
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task TryProcessAsync_WaitForHandlerCancelledWhileWaiting_ThrowsOperationCanceledException()
        {
            using var processor = new Processor();
            using var cancellation = new CancellationTokenSource();
            var hub = processor.Global();
            var context = ProcessingContext.WaitForHandler(token: cancellation.Token);
            var pending = hub.TryProcessAsync(new AsyncVoidRequest(), context);
            var pendingResult = hub.TryProcessAsync<AsyncResultRequest, int>(new AsyncResultRequest(), context);

            cancellation.Cancel();

            var voidCancelled = false;
            var resultCancelled = false;

            try
            {
                await pending;
            }
            catch (OperationCanceledException)
            {
                voidCancelled = true;
            }

            try
            {
                await pendingResult;
            }
            catch (OperationCanceledException)
            {
                resultCancelled = true;
            }

            Assert.IsTrue(voidCancelled);
            Assert.IsTrue(resultCancelled);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Register_Duplicate_ReturnsHandlerAlreadyRegisteredWithoutLogging()
        {
            using var processor = new Processor();
            var registries = new List<ProcessRegistry>();
            var hub = processor.Global().WithRegistries(registries);
            var logger = new StringBuilderLogger();
            var firstCount = 0;
            var secondCount = 0;
            Action<DuplicateRequest> first = _ => firstCount++;
            Action<DuplicateRequest> second = _ => secondCount++;

            var firstResult = hub.Register(first, logger);
            var secondResult = hub.Register(second, logger);

            Assert.IsTrue(firstResult.IsSuccess);
            Assert.IsTrue(firstResult.GetValueOrThrow().IsCreated);
            ProcessErrorAssert.IsHandlerAlreadyRegistered(
                  ProcessErrorAssert.GetError(secondResult)
                , typeof(EncosyTower.Processing.Internals.Sync.ProcessHandler<DuplicateRequest>)
            );
            StringAssert.Contains("already been registered", secondResult.GetErrorOrThrow().ToMessage());
            Assert.AreEqual(1, registries.Count);
            Assert.AreEqual(0, logger.LogEntryCount);

            hub.Process(new DuplicateRequest());

            Assert.AreEqual(1, firstCount);
            Assert.AreEqual(0, secondCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void DefaultHub_TryProcessAndRegister_ThrowInvalidOperationException()
        {
#if DISABLE_ENCOSY_CHECKS
            Assert.Ignore("The hub guard is not compiled when DISABLE_ENCOSY_CHECKS is defined.");
#else
            var hub = default(Processor.Hub<GlobalScope>);
            var logger = new StringBuilderLogger();
            Action<DuplicateRequest> process = static _ => { };

            AssertHubNotCreated(Assert.Throws<InvalidOperationException>(() => hub.TryProcess(new SyncVoidRequest())));
            AssertHubNotCreated(
                Assert.Throws<InvalidOperationException>(
                    () => hub.TryProcess<SyncResultRequest, int>(new SyncResultRequest())
                )
            );
            AssertHubNotCreated(Assert.Throws<InvalidOperationException>(() => hub.Register(process, logger)));
            AssertHubNotCreated(
                Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await hub.TryProcessAsync(new AsyncVoidRequest())
                )
            );
            AssertHubNotCreated(
                Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await hub.TryProcessAsync<AsyncResultRequest, int>(new AsyncResultRequest())
                )
            );

            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
#endif
        }

        private static void AssertHubNotCreated(InvalidOperationException exception)
        {
            StringAssert.StartsWith("EncosyTower.Processing.Processor+Hub`1[", exception.Message);
            StringAssert.Contains(
                  "is not created. Get a hub from Processor.Global(), Processor.Scope() or Processor.UnityScope()."
                , exception.Message
            );
        }

        private static WeakReference RegisterStatefulHandler(Processor.Hub<GlobalScope> hub)
        {
            var state = new State();

            hub.WithState(state).Register<StatefulRequest>(static (_, _) => { });
            return new WeakReference(state);
        }

        private readonly struct SyncVoidRequest : IRequest
        {
        }

        private readonly struct SyncResultRequest : IRequest<int>
        {
        }

        private readonly struct AsyncVoidRequest : IAsyncRequest
        {
        }

        private readonly struct AsyncResultRequest : IAsyncRequest<int>
        {
        }

        private readonly struct MismatchRequest : IRequest<UnityTask>, IAsyncRequest
        {
        }

        private readonly struct StatefulRequest : IRequest
        {
        }

        private readonly struct NullRequest : IRequest<string>
        {
        }

        private readonly struct AsyncNullRequest : IAsyncRequest<string>
        {
        }

        private readonly struct DuplicateRequest : IRequest
        {
        }

        private sealed class State
        {
        }
    }

    internal static class ProcessErrorAssert
    {
        public static ProcessError GetError(Success<ProcessError> result)
        {
            Assert.IsTrue(result.TryGetFailure(out var error));
            return error;
        }

        public static ProcessError GetError<TValue>(Result<TValue, ProcessError> result)
        {
            Assert.IsTrue(result.IsError);
            Assert.IsTrue(result.TryGetError(out var error));
            return error;
        }

        public static void IsHandlerNotFound(in ProcessError error, Type scopeType, Type requestType, Type resultType)
        {
            Assert.IsTrue(error._error.TryGetValue(out ProcessError.Error.HandlerNotFound found));
            Assert.AreEqual(scopeType, found.ScopeType);
            Assert.AreEqual(requestType, found.RequestType);
            Assert.AreEqual(resultType, found.ResultType);
            StringAssert.Contains("Cannot find any process handler", error.ToMessage());
        }

        public static void IsHandlerTypeMismatch(
              in ProcessError error
            , Type scopeType
            , Type requestType
            , Type handlerType
        )
        {
            Assert.IsTrue(error._error.TryGetValue(out ProcessError.Error.HandlerTypeMismatch mismatch));
            Assert.AreEqual(scopeType, mismatch.ScopeType);
            Assert.AreEqual(requestType, mismatch.RequestType);
            Assert.AreEqual(handlerType, mismatch.HandlerType);
            StringAssert.Contains("invalid type", error.ToMessage());
        }

        public static void IsStateUnavailable(in ProcessError error, Type stateType)
        {
            Assert.IsTrue(error._error.TryGetValue(out ProcessError.Error.StateUnavailable unavailable));
            Assert.AreEqual(stateType, unavailable.StateType);
            Assert.IsTrue(error.TryGetStateType(out var actual));
            Assert.AreEqual(stateType, actual);
        }

        public static void IsHandlerAlreadyRegistered(in ProcessError error, Type handlerType)
        {
            Assert.IsTrue(error._error.TryGetValue(out ProcessError.Error.HandlerAlreadyRegistered registered));
            Assert.AreEqual(handlerType, registered.HandlerType);
            Assert.IsFalse(error.TryGetStateType(out _));
        }

        public static void IsNotFound<TScope, TRequest>(Success<ProcessError> result)
            => IsHandlerNotFound(GetError(result), typeof(TScope), typeof(TRequest), resultType: null);

        public static void IsNotFound<TScope, TRequest, TResult>(Result<TResult, ProcessError> result)
            => IsHandlerNotFound(GetError(result), typeof(TScope), typeof(TRequest), typeof(TResult));
    }
}
