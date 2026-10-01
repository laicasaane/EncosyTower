using System;
using System.Threading.Tasks;
using EncosyTower.Processing;
using EncosyTower.Tasks;
using EncosyTower.Types;
using NUnit.Framework;

namespace EncosyTower.Tests.Processing
{
    public class HandlerIdentityTests
    {
        [Test]
        public void StatelessSyncVoid_ContextAndContextFreeDelegatesShareCanonicalId()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var processCount = 0;
            Action<SyncVoidRequest> contextFreeHandler =
                request => processCount += request.Value;
            Action<SyncVoidRequest, ProcessingContext> contextualHandler =
                (request, _) => processCount += request.Value;

            var contextFreeRegistry = hub.Register(contextFreeHandler);
            var id = (TypeId)Type<Action<SyncVoidRequest, ProcessingContext>>.Id;
            hub.Process(new SyncVoidRequest(1));

            Assert.AreEqual(1, processCount);
            Assert.IsTrue(hub.Unregister(id));

            var contextualRegistry = hub.Register(contextualHandler);
            contextFreeRegistry.Unregister();
            hub.Process(new SyncVoidRequest(2));

            Assert.AreEqual(3, processCount);
            contextualRegistry.Unregister();
            Assert.IsFalse(hub.TryProcess(new SyncVoidRequest(1), context: default));
        }

        [Test]
        public void StatelessSyncResult_ContextAndContextFreeDelegatesShareCanonicalId()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            Func<SyncResultRequest, int> contextFreeHandler =
                static request => request.Value;
            Func<SyncResultRequest, ProcessingContext, int> contextualHandler =
                static (request, _) => request.Value;

            var contextFreeRegistry = hub.Register(contextFreeHandler);
            var id = (TypeId)Type<Func<SyncResultRequest, ProcessingContext, int>>.Id;

            Assert.AreEqual(7, hub.Process<SyncResultRequest, int>(new SyncResultRequest(7)));
            Assert.IsTrue(hub.Unregister(id));

            var contextualRegistry = hub.Register<SyncResultRequest, int>(contextualHandler);
            contextFreeRegistry.Unregister();

            Assert.AreEqual(9, hub.Process<SyncResultRequest, int>(new SyncResultRequest(9)));
            contextualRegistry.Unregister();
            Assert.IsFalse(
                hub.TryProcess<SyncResultRequest, int>(new SyncResultRequest(7), context: default).TryGetValue(out _)
            );
        }

        [Test]
        public void StatefulSyncVoid_ContextAndContextFreeDelegatesShareCanonicalId()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var state = new State();
            var statefulHub = hub.WithState(state);
            Action<State, SyncVoidRequest> contextFreeHandler =
                static (target, request) => target.Value += request.Value;
            Action<State, SyncVoidRequest, ProcessingContext> contextualHandler =
                static (target, request, _) => target.Value += request.Value;

            var contextFreeRegistry = statefulHub.Register(contextFreeHandler);
            var id = (TypeId)Type<Action<SyncVoidRequest, ProcessingContext>>.Id;
            hub.Process(new SyncVoidRequest(3));

            Assert.AreEqual(3, state.Value);
            Assert.IsTrue(hub.Unregister(id));

            var contextualRegistry = statefulHub.Register(contextualHandler);
            contextFreeRegistry.Unregister();
            hub.Process(new SyncVoidRequest(5));

            Assert.AreEqual(8, state.Value);
            contextualRegistry.Unregister();
            Assert.IsFalse(hub.TryProcess(new SyncVoidRequest(1), context: default));
            GC.KeepAlive(state);
        }

        [Test]
        public void StatefulSyncResult_ContextAndContextFreeDelegatesShareCanonicalId()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var state = new State { Value = 5 };
            var statefulHub = hub.WithState(state);
            Func<State, SyncResultRequest, int> contextFreeHandler =
                static (target, request) => target.Value + request.Value;
            Func<State, SyncResultRequest, ProcessingContext, int> contextualHandler =
                static (target, request, _) => target.Value + request.Value;

            var contextFreeRegistry = statefulHub.Register(contextFreeHandler);
            var id = (TypeId)Type<Func<SyncResultRequest, ProcessingContext, int>>.Id;

            Assert.AreEqual(12, hub.Process<SyncResultRequest, int>(new SyncResultRequest(7)));
            Assert.IsTrue(hub.Unregister(id));

            var contextualRegistry = statefulHub.Register<SyncResultRequest, int>(contextualHandler);
            contextFreeRegistry.Unregister();

            Assert.AreEqual(16, hub.Process<SyncResultRequest, int>(new SyncResultRequest(11)));
            contextualRegistry.Unregister();
            Assert.IsFalse(
                hub.TryProcess<SyncResultRequest, int>(new SyncResultRequest(7), context: default).TryGetValue(out _)
            );
            GC.KeepAlive(state);
        }

        [Test]
        public async Task StatelessAsyncVoid_ContextAndContextFreeDelegatesShareCanonicalId()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var processCount = 0;
            Func<AsyncVoidRequest, UnityTask> contextFreeHandler = request =>
            {
                processCount += request.Value;
                return UnityTask.CompletedTask;
            };
            Func<AsyncVoidRequest, ProcessingContext, UnityTask> contextualHandler =
                (request, _) =>
                {
                    processCount += request.Value;
                    return UnityTask.CompletedTask;
                };

            var contextFreeRegistry = hub.Register(contextFreeHandler);
            var id = (TypeId)Type<Func<AsyncVoidRequest, ProcessingContext, UnityTask>>.Id;
            await hub.ProcessAsync(new AsyncVoidRequest(2));

            Assert.AreEqual(2, processCount);
            Assert.IsTrue(hub.Unregister(id));

            var contextualRegistry = hub.Register(contextualHandler);
            contextFreeRegistry.Unregister();
            await hub.ProcessAsync(new AsyncVoidRequest(3));

            Assert.AreEqual(5, processCount);
            contextualRegistry.Unregister();
            Assert.IsFalse(await hub.TryProcessAsync(new AsyncVoidRequest(1), context: default));
        }

        [Test]
        public async Task StatelessAsyncResult_ContextAndContextFreeDelegatesShareCanonicalId()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            Func<AsyncResultRequest, UnityTask<int>> contextFreeHandler =
                static request => UnityTask.FromResult(request.Value);
            Func<AsyncResultRequest, ProcessingContext, UnityTask<int>> contextualHandler =
                static (request, _) => UnityTask.FromResult(request.Value);

            var contextFreeRegistry = hub.Register<AsyncResultRequest, int>(contextFreeHandler);
            var id = (TypeId)Type<Func<AsyncResultRequest, ProcessingContext, UnityTask<int>>>.Id;

            Assert.AreEqual(11, await hub.ProcessAsync<AsyncResultRequest, int>(new AsyncResultRequest(11)));
            Assert.IsTrue(hub.Unregister(id));

            var contextualRegistry = hub.Register<AsyncResultRequest, int>(contextualHandler);
            contextFreeRegistry.Unregister();

            Assert.AreEqual(13, await hub.ProcessAsync<AsyncResultRequest, int>(new AsyncResultRequest(13)));
            contextualRegistry.Unregister();

            var result = await hub.TryProcessAsync<AsyncResultRequest, int>(
                  new AsyncResultRequest(1)
                , context: default
            );

            Assert.IsFalse(result.TryGetValue(out _));
        }

        [Test]
        public async Task StatefulAsyncVoid_ContextAndContextFreeDelegatesShareCanonicalId()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var state = new State();
            var statefulHub = hub.WithState(state);
            Func<State, AsyncVoidRequest, UnityTask> contextFreeHandler =
                static (target, request) =>
                {
                    target.Value += request.Value;
                    return UnityTask.CompletedTask;
                };
            Func<State, AsyncVoidRequest, ProcessingContext, UnityTask> contextualHandler =
                static (target, request, _) =>
                {
                    target.Value += request.Value;
                    return UnityTask.CompletedTask;
                };

            var contextFreeRegistry = statefulHub.Register(contextFreeHandler);
            var id = (TypeId)Type<Func<AsyncVoidRequest, ProcessingContext, UnityTask>>.Id;
            await hub.ProcessAsync(new AsyncVoidRequest(17));

            Assert.AreEqual(17, state.Value);
            Assert.IsTrue(hub.Unregister(id));

            var contextualRegistry = statefulHub.Register(contextualHandler);
            contextFreeRegistry.Unregister();
            await hub.ProcessAsync(new AsyncVoidRequest(19));

            Assert.AreEqual(36, state.Value);
            contextualRegistry.Unregister();
            Assert.IsFalse(await hub.TryProcessAsync(new AsyncVoidRequest(1), context: default));
            GC.KeepAlive(state);
        }

        [Test]
        public async Task StatefulAsyncResult_ContextAndContextFreeDelegatesShareCanonicalId()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var state = new State { Value = 20 };
            var statefulHub = hub.WithState(state);
            Func<State, AsyncResultRequest, UnityTask<int>> contextFreeHandler =
                static (target, request) =>
                    UnityTask.FromResult(target.Value + request.Value);
            Func<State, AsyncResultRequest, ProcessingContext, UnityTask<int>> contextualHandler =
                static (target, request, _) =>
                    UnityTask.FromResult(target.Value + request.Value);

            var contextFreeRegistry = statefulHub.Register<AsyncResultRequest, int>(contextFreeHandler);
            var id = (TypeId)Type<Func<AsyncResultRequest, ProcessingContext, UnityTask<int>>>.Id;

            Assert.AreEqual(23, await hub.ProcessAsync<AsyncResultRequest, int>(new AsyncResultRequest(3)));
            Assert.IsTrue(hub.Unregister(id));

            var contextualRegistry = statefulHub.Register<AsyncResultRequest, int>(contextualHandler);
            contextFreeRegistry.Unregister();

            Assert.AreEqual(25, await hub.ProcessAsync<AsyncResultRequest, int>(new AsyncResultRequest(5)));
            contextualRegistry.Unregister();

            var result = await hub.TryProcessAsync<AsyncResultRequest, int>(
                  new AsyncResultRequest(1)
                , context: default
            );

            Assert.IsFalse(result.TryGetValue(out _));
            GC.KeepAlive(state);
        }

        [Test]
        public async Task StatefulAsyncVoid_CanonicalTypeIdAndRegistryRemovalRemainValid()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var state = new State();
            var statefulHub = hub.WithState(state);
            Func<State, AsyncVoidRequest, UnityTask> handler =
                static (_, _) => UnityTask.CompletedTask;

            statefulHub.Register(handler);
            var id = (TypeId)Type<Func<AsyncVoidRequest, ProcessingContext, UnityTask>>.Id;

            Assert.IsTrue(hub.Unregister(id));

            var registry = statefulHub.Register(handler);
            registry.Dispose();

            Assert.IsFalse(await hub.TryProcessAsync(new AsyncVoidRequest(1), context: default));
            GC.KeepAlive(state);
        }

        [Test]
        public async Task MissingHandlers_PreserveSyncAsyncBoolAndOptionBehavior()
        {
            using var processor = new Processor();
            var hub = processor.Global();

            Assert.Throws<InvalidOperationException>(
                () => hub.Process(new SyncVoidRequest(1))
            );
            Assert.Throws<InvalidOperationException>(
                () => hub.Process<SyncResultRequest, int>(new SyncResultRequest(1))
            );
            Assert.Throws<InvalidOperationException>(
                () => hub.ProcessAsync(new AsyncVoidRequest(1))
            );
            Assert.Throws<InvalidOperationException>(
                () => hub.ProcessAsync<AsyncResultRequest, int>(new AsyncResultRequest(1))
            );
            Assert.IsFalse(hub.TryProcess(new SyncVoidRequest(1), context: default));
            Assert.IsFalse(
                hub.TryProcess<SyncResultRequest, int>(new SyncResultRequest(1), context: default).TryGetValue(out _)
            );
            Assert.IsFalse(await hub.TryProcessAsync(new AsyncVoidRequest(1), context: default));

            var result = await hub.TryProcessAsync<AsyncResultRequest, int>(
                  new AsyncResultRequest(1)
                , context: default
            );

            Assert.IsFalse(result.TryGetValue(out _));
        }

        private readonly struct SyncVoidRequest : IRequest
        {
            public SyncVoidRequest(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct SyncResultRequest : IRequest<int>
        {
            public SyncResultRequest(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct AsyncVoidRequest : IAsyncRequest
        {
            public AsyncVoidRequest(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct AsyncResultRequest : IAsyncRequest<int>
        {
            public AsyncResultRequest(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private sealed class State
        {
            public int Value { get; set; }
        }
    }
}
