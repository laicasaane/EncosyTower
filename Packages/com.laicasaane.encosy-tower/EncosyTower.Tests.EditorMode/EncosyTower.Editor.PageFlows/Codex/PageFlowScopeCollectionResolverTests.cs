#if UNITY_UGUI

using System;
using EncosyTower.Editor.PageFlows;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Tasks;
using EncosyTower.Tests.PageFlows;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Editor.PageFlows
{
    public sealed class PageFlowScopeCollectionResolverTests
    {
        private GameObject _gameObject;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject(nameof(PageFlowScopeCollectionResolverTests));
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void NoInitializer_ReportsTheProblem()
        {
            var info = Resolve();

            Assert.AreEqual(ScopeCollectionProblem.NoInitializer, info.Problem);
            Assert.AreEqual(nameof(IUguiPageCodexOnInitialize), info.InitializerInterfaceName);
            Assert.AreEqual("No initializer component found", info.ToProblemText());
        }

        [Test]
        public void BaseClassInitializer_ReadsScopesWithoutCallingTheApplier()
        {
            _gameObject.AddComponent<BaseClassInitializer>();

            var info = Resolve();

            Assert.IsTrue(info.IsValid);
            Assert.AreEqual(nameof(TestScopes), info.TypeName);
            CollectionAssert.AreEqual(new[] { "A", "B" }, info.ScopeIdentifiers);
        }

        [Test]
        public void InterfaceInitializerWithoutApplier_ReportsTheProblem()
        {
            _gameObject.AddComponent<NullApplierInitializer>();

            var info = Resolve();

            Assert.AreEqual(ScopeCollectionProblem.NullApplier, info.Problem);
            Assert.AreEqual("Initializer returns no applier", info.ToProblemText());
        }

        [Test]
        public void InterfaceInitializerWithApplier_ReadsItsScopes()
        {
            _gameObject.AddComponent<ApplierInitializer>();

            var info = Resolve();

            Assert.IsTrue(info.IsValid);
            Assert.AreEqual(nameof(TestScopes), info.TypeName);
            CollectionAssert.AreEqual(new[] { "A", "B" }, info.ScopeIdentifiers);
        }

        private PageFlowScopeCollectionInfo Resolve()
            => PageFlowScopeCollectionResolver.Resolve(
                  _gameObject.transform
                , typeof(IUguiPageCodexOnInitialize)
                , typeof(UguiPageCodexInitializer<>)
            );

        private sealed class BaseClassInitializer : UguiPageCodexInitializer<TestScopes>, IUguiPageCodexOnInitialize
        {
            IPageFlowScopeCollectionApplier IUguiPageCodexOnInitialize.PageFlowScopeCollectionApplier
                => throw new InvalidOperationException("The resolver must not call this applier.");

            UnityTask IUguiPageCodexOnInitialize.OnInitializeAsync(UguiPageCodex codex)
                => UnityTask.CompletedTask;

            protected override UnityTask OnInitializeAsync(UguiPageCodex codex, TestScopes scopes)
                => UnityTask.CompletedTask;
        }

        private sealed class NullApplierInitializer : MonoBehaviour, IUguiPageCodexOnInitialize
        {
            public IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier => null;

            public UnityTask OnInitializeAsync(UguiPageCodex codex)
                => UnityTask.CompletedTask;
        }

        private sealed class ApplierInitializer : MonoBehaviour, IUguiPageCodexOnInitialize
        {
            private readonly PageFlowScopeCollectionApplier<TestScopes> _applier = new();

            public IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier => _applier;

            public UnityTask OnInitializeAsync(UguiPageCodex codex)
                => UnityTask.CompletedTask;
        }
    }
}

#endif
