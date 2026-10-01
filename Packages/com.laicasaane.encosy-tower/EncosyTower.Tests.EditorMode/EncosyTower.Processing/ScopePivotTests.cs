using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using EncosyTower.Processing;
using EncosyTower.Types;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Processing
{
    public class ScopePivotTests
    {
        [Test]
        public void LeafRetainsExactCreatingProcessor()
        {
            using var processor = new Processor();
            var hub = processor.Scope(new ScopeA(1));
            var map = GetLeaf(hub);
            var ownerProperty = map.GetType().GetProperty("Owner", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(ownerProperty);
            Assert.AreSame(processor, ownerProperty.GetValue(map));
        }

        [Test]
        public void StatelessPivots_RouteToOriginalOwnerAndIsolateScopeLeaves()
        {
            using var firstProcessor = new Processor();
            using var secondProcessor = new Processor();
            var source = firstProcessor.Scope(new ScopeA(1));
            var sameType = source.WithScope(new ScopeA(2));
            var otherType = source.WithScope(new ScopeB(3));
            var global = source.WithGlobalScope();
            var sameTypeCount = 0;
            var otherTypeCount = 0;
            var globalCount = 0;
            var otherOwnerCount = 0;

            Assert.AreSame(GetLeaf(sameType), GetLeaf(firstProcessor.Scope(new ScopeA(2))));
            Assert.AreSame(GetLeaf(otherType), GetLeaf(firstProcessor.Scope(new ScopeB(3))));
            Assert.AreSame(GetLeaf(global), GetLeaf(firstProcessor.Global()));
            Assert.AreNotSame(GetLeaf(otherType), GetLeaf(secondProcessor.Scope(new ScopeB(3))));

            sameType.Register<SyncRequest>(request => sameTypeCount += request.Value);
            otherType.Register<SyncRequest>(request => otherTypeCount += request.Value);
            global.Register<SyncRequest>(request => globalCount += request.Value);
            secondProcessor.Scope(new ScopeB(3))
                .Register<SyncRequest>(request => otherOwnerCount += request.Value);

            firstProcessor.Scope(new ScopeA(2)).Process(new SyncRequest(1));
            firstProcessor.Scope(new ScopeB(3)).Process(new SyncRequest(2));
            firstProcessor.Global().Process(new SyncRequest(3));
            secondProcessor.Scope(new ScopeB(3)).Process(new SyncRequest(4));

            Assert.AreEqual(1, source.Scope.Value);
            Assert.AreEqual(2, sameType.Scope.Value);
            Assert.AreEqual(3, otherType.Scope.Value);
            Assert.IsTrue(global.IsCreated);
            Assert.IsFalse(source.TryProcess(new SyncRequest(5), context: default));
            Assert.AreEqual(1, sameTypeCount);
            Assert.AreEqual(2, otherTypeCount);
            Assert.AreEqual(3, globalCount);
            Assert.AreEqual(4, otherOwnerCount);
        }

        [Test]
        public void Pivots_PreserveRegistryCollectionWithoutMovingHandlers()
        {
            using var processor = new Processor();
            var registries = new List<ProcessRegistry>();
            var sourceCount = 0;
            var pivotCount = 0;
            var source = processor.Scope(new ScopeA(1)).WithRegistries(registries);

            source.Register<SyncRequest>(request => sourceCount += request.Value);

            var pivot = source.WithScope(new ScopeA(2));

            Assert.AreSame(registries, source.Registries);
            Assert.AreSame(registries, pivot.Registries);
            Assert.AreEqual(1, registries.Count);
            Assert.IsFalse(pivot.TryProcess(new SyncRequest(1), context: default));

            pivot.Register<SyncRequest>(request => pivotCount += request.Value);
            source.Process(new SyncRequest(2));
            pivot.Process(new SyncRequest(3));

            Assert.AreEqual(2, registries.Count);
            Assert.AreEqual(2, sourceCount);
            Assert.AreEqual(3, pivotCount);
        }

        [Test]
        public void StatefulPivots_PreserveExactStateAndRegistryCollection()
        {
            using var processor = new Processor();
            var registries = new List<ProcessRegistry>();
            var state = new State();
            var source = processor.Scope(new ScopeA(1)).WithRegistries(registries).WithState(state);

            var defaultScope = source.WithScope<ScopeB>();
            var explicitScope = source.WithScope(new ScopeB(2));
            var global = source.WithGlobalScope();

            Assert.AreSame(state, defaultScope.State);
            Assert.AreSame(state, explicitScope.State);
            Assert.AreSame(state, global.State);
            Assert.AreSame(registries, defaultScope.Registries);
            Assert.AreSame(registries, explicitScope.Registries);
            Assert.AreSame(registries, global.Registries);
            Assert.AreEqual(0, defaultScope.Scope.Value);
            Assert.AreEqual(2, explicitScope.Scope.Value);
            Assert.AreEqual(1, source.Scope.Value);
        }

        [Test]
        public void DefaultHubPivots_PreserveValuesAndDeferValidationToTerminalOperations()
        {
            var state = new State();

#if DISABLE_ENCOSY_CHECKS
            Assert.Throws<NullReferenceException>(
                () => default(Processor.Hub<ScopeA>).WithScope(new ScopeB(1))
            );
            Assert.Throws<NullReferenceException>(
                () => default(Processor.Hub<ScopeA>).WithState(state).WithGlobalScope()
            );
#else
            var registries = new List<ProcessRegistry>();
            var source = CreateInvalidHub(new ScopeA(7), registries);
            var pivot = source.WithScope(new ScopeB(1));

            Assert.IsFalse(pivot.IsCreated);
            Assert.AreEqual(1, pivot.Scope.Value);
            Assert.AreSame(registries, pivot.Registries);
            Assert.AreSame(source.Registries, pivot.Registries);
            LogAssert.NoUnexpectedReceived();

            LogAssert.Expect(LogType.Error, "Processor.Hub must be retrieved via `Processor.Scope` API");
            Assert.IsFalse(pivot.TryProcess(new SyncRequest(1), context: default));

            var statefulSource = source.WithState(state);
            var statefulPivot = statefulSource.WithScope(new ScopeB(2));

            Assert.IsFalse(statefulPivot.IsCreated);
            Assert.AreEqual(2, statefulPivot.Scope.Value);
            Assert.AreSame(state, statefulPivot.State);
            Assert.AreSame(statefulSource.Registries, statefulPivot.Registries);
            LogAssert.NoUnexpectedReceived();

            LogAssert.Expect(LogType.Error, "Processor.Hub must be retrieved via `Processor.Scope` API");
            Assert.IsFalse(statefulPivot.Unregister(default(TypeId)));
#endif
        }

        [Test]
        public void LiveHub_StronglyRetainsOwnerAndMapGraph()
        {
            var hub = CreateRetainedHub(out var ownerReference);

            CollectGarbage();

            try
            {
                Assert.IsTrue(ownerReference.IsAlive);
                Assert.IsTrue(hub.WithScope(new ScopeB(2)).IsCreated);
            }
            finally
            {
                if (ownerReference.Target is Processor processor)
                {
                    processor.Dispose();
                }

                GC.KeepAlive(hub);
            }
        }

        [Test]
        public void PostDisposePivot_MatchesDirectOwnerScope()
        {
            var processor = new Processor();
            var source = processor.Scope(new ScopeA(1));

            processor.Dispose();

            try
            {
                var directException = Capture(
                    () => processor.Scope(new ScopeB(2))
                    , out Processor.Hub<ScopeB> direct
                );
                var pivotException = Capture(
                    () => source.WithScope(new ScopeB(2))
                    , out Processor.Hub<ScopeB> pivot
                );

                Assert.AreEqual(directException?.GetType(), pivotException?.GetType());
                Assert.AreEqual(directException?.Message, pivotException?.Message);

                if (directException == null)
                {
                    Assert.AreEqual(direct.IsCreated, pivot.IsCreated);
                    Assert.AreEqual(direct.Scope.Value, pivot.Scope.Value);

                    if (direct.IsCreated)
                    {
                        var processCount = 0;

                        direct.Register<SyncRequest>(request => processCount += request.Value);
                        pivot.Process(new SyncRequest(3));

                        Assert.AreEqual(3, processCount);
                    }
                }
            }
            finally
            {
                processor.Dispose();
            }
        }

        private static object GetLeaf<TScope>(Processor.Hub<TScope> hub)
        {
            var mapField = typeof(Processor.Hub<TScope>).GetField(
                "_map",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(mapField);
            return mapField.GetValue(hub);
        }

        private static Processor.Hub<TScope> CreateInvalidHub<TScope>(
              TScope scope
            , ICollection<ProcessRegistry> registries
        )
        {
            var constructor = Array.Find(
                typeof(Processor.Hub<TScope>).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic),
                static candidate => candidate.GetParameters().Length == 4
            );

            Assert.IsNotNull(constructor);

            return (Processor.Hub<TScope>)constructor.Invoke(
                new object[] { scope, null, registries, true }
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Processor.Hub<ScopeA> CreateRetainedHub(out WeakReference ownerReference)
        {
            var processor = new Processor();
            ownerReference = new WeakReference(processor);
            return processor.Scope(new ScopeA(1));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CollectGarbage()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private static Exception Capture<T>(Func<T> action, out T result)
        {
            try
            {
                result = action();
                return null;
            }
            catch (Exception exception)
            {
                result = default;
                return exception;
            }
        }

        private readonly struct ScopeA
        {
            public ScopeA(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct ScopeB
        {
            public ScopeB(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct SyncRequest : IRequest
        {
            public SyncRequest(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private sealed class State
        {
        }
    }
}
