using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    public sealed class DeadWeakStateTests
    {
        [Test]
        public async Task DeadWeakState_AllEightHandlerShapesReturnErrorsAndExactRemove()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var references = RunOnShortLivedThread(
                () => new[] {
                    RegisterSyncVoid(hub),
                    RegisterContextualSyncVoid(hub),
                    RegisterSyncResult(hub),
                    RegisterContextualSyncResult(hub),
                    RegisterAsyncVoid(hub),
                    RegisterAsyncResult(hub),
                    RegisterContextualAsyncVoid(hub),
                    RegisterContextualAsyncResult(hub),
                }
            );

            CollectGarbage(references);

            Assert.Throws<InvalidOperationException>(
                () => hub.Process(new SyncVoidRequest())
            );

            var logger = new StringBuilderLogger();
            var syncResult = hub.TryProcess<SyncResultRequest, int>(
                new SyncResultRequest(),
                ProcessingContext.Default(logger: logger)
            );
            var contextualSyncVoid = hub.TryProcess(
                new ContextualSyncVoidRequest(),
                ProcessingContext.Default(logger: logger)
            );

            Assert.IsFalse(syncResult.TryGetValue(out _));
            Assert.IsFalse(contextualSyncVoid);
            Assert.Throws<InvalidOperationException>(
                () => hub.Process<ContextualSyncResultRequest, int>(new ContextualSyncResultRequest())
            );
            Assert.Throws<InvalidOperationException>(
                () => hub.ProcessAsync(new AsyncVoidRequest())
            );

            var asyncResult = await hub.TryProcessAsync<AsyncResultRequest, int>(
                new AsyncResultRequest(),
                ProcessingContext.Default(logger: logger)
            );
            var contextualAsyncVoid = await hub.TryProcessAsync(
                new ContextualAsyncVoidRequest(),
                ProcessingContext.Default(logger: logger)
            );

            Assert.IsFalse(asyncResult.TryGetValue(out _));
            Assert.IsFalse(contextualAsyncVoid);
            Assert.Throws<InvalidOperationException>(
                () => hub.ProcessAsync<ContextualAsyncResultRequest, int>(new ContextualAsyncResultRequest())
            );

            Assert.AreEqual(4, logger.LogEntryCount);
            StringAssert.Contains("is not alive anymore", logger.ToString());
            LogAssert.NoUnexpectedReceived();

            Assert.IsFalse(hub.Unregister((TypeId)Type<Action<SyncVoidRequest, ProcessingContext>>.Id));
            Assert.IsFalse(hub.Unregister((TypeId)Type<Action<ContextualSyncVoidRequest, ProcessingContext>>.Id));
            Assert.IsFalse(hub.Unregister((TypeId)Type<Func<SyncResultRequest, ProcessingContext, int>>.Id));
            Assert.IsFalse(hub.Unregister((TypeId)Type<Func<ContextualSyncResultRequest, ProcessingContext, int>>.Id));
            Assert.IsFalse(hub.Unregister((TypeId)Type<Func<AsyncVoidRequest, ProcessingContext, UnityTask>>.Id));
            Assert.IsFalse(
                hub.Unregister((TypeId)Type<Func<AsyncResultRequest, ProcessingContext, UnityTask<int>>>.Id)
            );
            Assert.IsFalse(
                hub.Unregister((TypeId)Type<Func<ContextualAsyncVoidRequest, ProcessingContext, UnityTask>>.Id)
            );
            Assert.IsFalse(
                hub.Unregister((TypeId)Type<Func<ContextualAsyncResultRequest, ProcessingContext, UnityTask<int>>>.Id)
            );

            GC.KeepAlive(hub);
        }

        [Test]
        public async Task WaitStrategy_RemovesDeadEntryAndInvokesReplacementExactlyOnce()
        {
            using var processor = new Processor();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var hub = processor.Global();
            var weakState = RunOnShortLivedThread(() => RegisterWaitHandler(hub));

            CollectGarbage(weakState);

            var replacementCount = 0;
            var pending = hub.ProcessAsync(
                new WaitRequest(),
                ProcessingContext.WaitForHandler(token: cancellation.Token)
            );

            hub.Register<WaitRequest>(
                _ =>
                {
                    replacementCount++;
                    return UnityTask.CompletedTask;
                }
            );

            await pending;

            Assert.AreEqual(1, replacementCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void OldRegistry_CannotRemoveReplacementWithSameCanonicalId()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var replacementCount = 0;
            Action<RegistryRequest> original = static _ => { };
            Action<RegistryRequest> replacement = _ => replacementCount++;
            var oldRegistry = hub.Register(original);
            var originalHandler = GetOnlyRegisteredHandler(hub);

            oldRegistry.Unregister();
            hub.Register(replacement);

            oldRegistry.Dispose();
            hub.Process(new RegistryRequest());

            Assert.AreEqual(1, replacementCount);
            GC.KeepAlive(originalHandler);
            GC.KeepAlive(original);
        }

        [Test]
        public async Task RegistrationWaitCancellation_UsesDirectAndTryContracts()
        {
            using var processor = new Processor();
            using var cancellation = new CancellationTokenSource();
            var hub = processor.Global();

            cancellation.Cancel();

            Assert.IsFalse(
                await hub.TryProcessAsync(
                    new TryWaitCancellationRequest(),
                    ProcessingContext.WaitForHandler(token: cancellation.Token)
                )
            );

            var result = await hub.TryProcessAsync<TryWaitResultCancellationRequest, int>(
                new TryWaitResultCancellationRequest(),
                ProcessingContext.WaitForHandler(token: cancellation.Token)
            );

            Assert.IsFalse(result.TryGetValue(out _));

            var cancellationPropagated = false;

            try
            {
                await hub.ProcessAsync(
                    new DirectWaitCancellationRequest(),
                    ProcessingContext.WaitForHandler(token: cancellation.Token)
                );
            }
            catch (OperationCanceledException)
            {
                cancellationPropagated = true;
            }

            Assert.IsTrue(cancellationPropagated);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task StartedHandlerCancellation_PropagatesOutsideRegistrationWaitCatch()
        {
            using var processor = new Processor();
            using var cancellation = new CancellationTokenSource();
            var hub = processor.Global();
            var startedCount = 0;

            hub.Register<StartedCancellationRequest>(
                (_, context) =>
                {
                    startedCount++;
                    return UnityTask.NextFrameAsync(context.Token);
                }
            );
            cancellation.Cancel();

            var cancellationPropagated = false;

            try
            {
                await hub.TryProcessAsync(
                    new StartedCancellationRequest(),
                    ProcessingContext.DropIfNoHandler(warnNoHandler: false, token: cancellation.Token)
                );
            }
            catch (OperationCanceledException)
            {
                cancellationPropagated = true;
            }

            Assert.AreEqual(1, startedCount);
            Assert.IsTrue(cancellationPropagated);
            LogAssert.NoUnexpectedReceived();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TResult RunOnShortLivedThread<TResult>(Func<TResult> action)
        {
            TResult result = default;
            Exception failure = null;

            // Unity's conservative GC can retain stale pointers from a live test stack.
            // Ending this thread removes the registration stack before collection.
            var thread = new Thread(
                () =>
                {
                    try
                    {
                        result = action();
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                    }
                }
            );

            thread.Start();
            thread.Join();

            Assert.IsNull(failure, failure?.ToString());
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RegisterSyncVoid(Processor.Hub<GlobalScope> hub)
        {
            var state = new State();

            hub.WithState(state).Register<SyncVoidRequest>(static (_, _) => { });
            return new WeakReference(state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RegisterSyncResult(Processor.Hub<GlobalScope> hub)
        {
            var state = new State();

            hub.WithState(state).Register<SyncResultRequest, int>(
                static (_, _) => 1
            );
            return new WeakReference(state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RegisterContextualSyncVoid(Processor.Hub<GlobalScope> hub)
        {
            var state = new State();

            hub.WithState(state).Register<ContextualSyncVoidRequest>(
                static (_, _, _) => { }
            );
            return new WeakReference(state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RegisterContextualSyncResult(Processor.Hub<GlobalScope> hub)
        {
            var state = new State();

            hub.WithState(state).Register<ContextualSyncResultRequest, int>(
                static (_, _, _) => 1
            );
            return new WeakReference(state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RegisterAsyncVoid(Processor.Hub<GlobalScope> hub)
        {
            var state = new State();

            hub.WithState(state).Register<AsyncVoidRequest>(
                static (_, _) => UnityTask.CompletedTask
            );
            return new WeakReference(state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RegisterAsyncResult(Processor.Hub<GlobalScope> hub)
        {
            var state = new State();

            hub.WithState(state).Register<AsyncResultRequest, int>(
                static (_, _) => UnityTask.FromResult(1)
            );
            return new WeakReference(state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RegisterContextualAsyncVoid(Processor.Hub<GlobalScope> hub)
        {
            var state = new State();

            hub.WithState(state).Register<ContextualAsyncVoidRequest>(
                static (_, _, _) => UnityTask.CompletedTask
            );
            return new WeakReference(state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RegisterContextualAsyncResult(Processor.Hub<GlobalScope> hub)
        {
            var state = new State();

            hub.WithState(state).Register<ContextualAsyncResultRequest, int>(
                static (_, _, _) => UnityTask.FromResult(1)
            );
            return new WeakReference(state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RegisterWaitHandler(Processor.Hub<GlobalScope> hub)
        {
            var state = new State();

            hub.WithState(state).Register<WaitRequest>(
                static (_, _) => UnityTask.CompletedTask
            );
            return new WeakReference(state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CollectGarbage(params WeakReference[] references)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                if (Array.TrueForAll(references, static reference => reference.IsAlive == false))
                {
                    break;
                }
            }

            foreach (var reference in references)
            {
                Assert.IsFalse(reference.IsAlive);
            }
        }

        private static object GetOnlyRegisteredHandler<TScope>(Processor.Hub<TScope> hub)
        {
            var hubMapField = typeof(Processor.Hub<TScope>).GetField(
                "_map",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(hubMapField);

            var hubMap = hubMapField.GetValue(hub);
            var handlersField = hubMap.GetType().GetField("_map", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(handlersField);

            var handlers = handlersField.GetValue(hubMap) as IDictionary;

            Assert.IsNotNull(handlers);
            Assert.AreEqual(1, handlers.Count);

            foreach (DictionaryEntry entry in handlers)
            {
                return entry.Value;
            }

            Assert.Fail("The handler map was unexpectedly empty.");
            return null;
        }

        private readonly struct SyncVoidRequest : IRequest
        {
        }

        private readonly struct SyncResultRequest : IRequest<int>
        {
        }

        private readonly struct ContextualSyncVoidRequest : IRequest
        {
        }

        private readonly struct ContextualSyncResultRequest : IRequest<int>
        {
        }

        private readonly struct AsyncVoidRequest : IAsyncRequest
        {
        }

        private readonly struct AsyncResultRequest : IAsyncRequest<int>
        {
        }

        private readonly struct ContextualAsyncVoidRequest : IAsyncRequest
        {
        }

        private readonly struct ContextualAsyncResultRequest : IAsyncRequest<int>
        {
        }

        private readonly struct WaitRequest : IAsyncRequest
        {
        }

        private readonly struct RegistryRequest : IRequest
        {
        }

        private readonly struct TryWaitCancellationRequest : IAsyncRequest
        {
        }

        private readonly struct TryWaitResultCancellationRequest : IAsyncRequest<int>
        {
        }

        private readonly struct DirectWaitCancellationRequest : IAsyncRequest
        {
        }

        private readonly struct StartedCancellationRequest : IAsyncRequest
        {
        }

        private sealed class State
        {
        }
    }
}
