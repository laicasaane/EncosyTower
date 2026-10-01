using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using EncosyTower.Processing;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Processing
{
    public class UnityScopeAndClearTests
    {
        [Test]
        public void UnityScope_DestroyedAfterCapture_RetainsEntityId()
        {
            using var processor = new Processor();
            var scope = new GameObject();
            var hub = processor.UnityScope(scope);
            var id = hub.Scope.Value;

            UnityEngine.Object.DestroyImmediate(scope);

            Assert.AreEqual(id, hub.Scope.Value);
            Assert.IsFalse(hub.Scope.IsValid);
        }

        [Test]
        public void Scope_RejectsLiveDestroyedAndObjectTypedUnityObjects()
        {
            using var processor = new Processor();
            var live = new GameObject();
            object objectTyped = live;
            var destroyed = new GameObject();

            UnityEngine.Object.DestroyImmediate(destroyed);

            Assert.Throws<ArgumentException>(() => processor.Scope(live));
            Assert.Throws<ArgumentException>(() => processor.Scope(objectTyped));
            Assert.Throws<ArgumentException>(() => processor.Scope(destroyed));

            UnityEngine.Object.DestroyImmediate(live);
        }

        [Test]
        public void UnityScopeAndPivot_RejectNullAndDestroyedObjects()
        {
            using var processor = new Processor();
            GameObject nullScope = null;
            var destroyedScope = new GameObject();

            UnityEngine.Object.DestroyImmediate(destroyedScope);

            Assert.Throws<ArgumentNullException>(() => processor.UnityScope(nullScope));
            Assert.Throws<ArgumentNullException>(() => processor.Global().WithUnityScope(nullScope));
            Assert.Throws<ArgumentNullException>(() => processor.UnityScope(destroyedScope));
            Assert.Throws<ArgumentNullException>(() => processor.Global().WithUnityScope(destroyedScope));
        }

        [Test]
        public void Clear_AfterUnityObjectDestruction_UsesSavedHub()
        {
            using var processor = new Processor();
            var scope = new GameObject();
            var hub = processor.UnityScope(scope);
            var count = 0;

            hub.Register<SyncRequest>(_ => count++);
            UnityEngine.Object.DestroyImmediate(scope);

            hub.Clear();
            Assert.IsFalse(hub.TryProcess(new SyncRequest(), context: default));
            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task Clear_AffectsOnlyOneLeafIncludingAsyncHandlers()
        {
            using var processor = new Processor();
            var first = processor.Scope(new Scope(1));
            var second = processor.Scope(new Scope(2));
            var firstCount = 0;
            var secondCount = 0;

            first.Register<SyncRequest>(_ => firstCount++);
            second.Register<SyncRequest>(_ => secondCount++);
            first.Register<AsyncRequest>(static _ => UnityTask.CompletedTask);
            second.Register<AsyncRequest>(static _ => UnityTask.CompletedTask);

            first.Clear();

            Assert.IsFalse(first.TryProcess(new SyncRequest(), context: default));
            Assert.IsTrue(second.TryProcess(new SyncRequest(), context: default));
            Assert.IsFalse(await first.TryProcessAsync(new AsyncRequest(), context: default));
            Assert.IsTrue(await second.TryProcessAsync(new AsyncRequest(), context: default));
            Assert.AreEqual(0, firstCount);
            Assert.AreEqual(1, secondCount);
        }

        [Test]
        public void Clear_PreservesTheLeafForOldAndFreshHubs()
        {
            using var processor = new Processor();
            var oldHub = processor.Scope(new Scope(1));
            var oldLeaf = GetLeaf(oldHub);
            var count = 0;

            oldHub.Register<SyncRequest>(_ => count++);
            oldHub.Clear();

            var freshHub = processor.Scope(new Scope(1));

            Assert.AreSame(oldLeaf, GetLeaf(freshHub));

            freshHub.Register<SyncRequest>(_ => count++);
            oldHub.Process(new SyncRequest());

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Clear_OldRegistryCannotRemoveAReplacementHandler()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var oldCount = 0;
            var replacementCount = 0;
            var oldRegistry = hub.Register<SyncRequest>(_ => oldCount++);

            hub.Clear();

            hub.Register<SyncRequest>(_ => replacementCount++);
            oldRegistry.Dispose();
            hub.Process(new SyncRequest());

            Assert.AreEqual(0, oldCount);
            Assert.AreEqual(1, replacementCount);
        }

        [Test]
        public void Clear_PreservesStateRegistriesAndOtherScopes()
        {
            using var firstProcessor = new Processor();
            using var secondProcessor = new Processor();
            var registries = new List<ProcessRegistry>();
            var state = new State();
            var scoped = firstProcessor.Scope(new Scope(1)).WithRegistries(registries).WithState(state);
            var global = firstProcessor.Global();
            var otherScope = firstProcessor.Scope(new Scope(2));
            var otherProcessor = secondProcessor.Scope(new Scope(1));
            var globalCount = 0;
            var otherScopeCount = 0;
            var otherProcessorCount = 0;

            scoped.Register<SyncRequest>((_, _) => { });
            global.Register<SyncRequest>(_ => globalCount++);
            otherScope.Register<SyncRequest>(_ => otherScopeCount++);
            otherProcessor.Register<SyncRequest>(_ => otherProcessorCount++);

            scoped.Clear();

            Assert.AreSame(state, scoped.State);
            Assert.AreSame(registries, scoped.Registries);
            Assert.AreEqual(1, registries.Count);

            global.Process(new SyncRequest());
            otherScope.Process(new SyncRequest());
            otherProcessor.Process(new SyncRequest());

            Assert.AreEqual(1, globalCount);
            Assert.AreEqual(1, otherScopeCount);
            Assert.AreEqual(1, otherProcessorCount);
        }

        private static object GetLeaf<TScope>(Processor.Hub<TScope> hub)
        {
            var field = typeof(Processor.Hub<TScope>).GetField(
                "_map",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(field);
            return field.GetValue(hub);
        }

        private readonly struct Scope
        {
            public Scope(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private sealed class State { }

        private readonly struct SyncRequest : IRequest { }

        private readonly struct AsyncRequest : IAsyncRequest { }
    }
}
