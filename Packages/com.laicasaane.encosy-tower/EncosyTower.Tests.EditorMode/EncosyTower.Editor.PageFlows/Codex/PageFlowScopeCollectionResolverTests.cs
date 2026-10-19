#if UNITY_UGUI

using System;
using EncosyTower.Editor.PageFlows;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages.Codex;
using EncosyTower.Tasks;
using EncosyTower.Tests.PageFlows;
using UnityEngine;
using NUnit.Framework;

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

            Assert.AreEqual(PageFlowScopeCollectionInfo.EnumCase.NoInitializer, info.GetEnumCase());
            Assert.IsTrue(info.TryGetValue(out PageFlowScopeCollectionInfo.NoInitializer noInitializer));
            Assert.AreEqual(nameof(IUguiPageCodexOnInitialize), noInitializer.InitializerInterfaceName);
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

            Assert.AreEqual(PageFlowScopeCollectionInfo.EnumCase.NullApplier, info.GetEnumCase());
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

        [Test]
        public void InterfaceInitializerWhoseApplierThrows_ReportsScopesUnreadable()
        {
            _gameObject.AddComponent<ThrowingApplierInitializer>();

            var info = Resolve();

            Assert.AreEqual(PageFlowScopeCollectionInfo.EnumCase.ScopesUnreadable, info.GetEnumCase());
            Assert.IsFalse(info.IsValid);
            Assert.IsTrue(info.TryGetValue(out PageFlowScopeCollectionInfo.ScopesUnreadable unreadable));
            Assert.AreEqual(nameof(ThrowingApplierInitializer), unreadable.InitializerTypeName);
            Assert.AreEqual(nameof(InvalidOperationException), unreadable.ExceptionTypeName);
            Assert.AreEqual("applier failed", unreadable.ExceptionMessage);
            Assert.AreEqual("Scope collection could not be read", info.ToProblemText());
            StringAssert.Contains("applier failed", info.ToTooltip());
        }

        [Test]
        public void BaseClassInitializerWhoseScopesThrow_ReportsScopesUnreadable()
        {
            _gameObject.AddComponent<ThrowingScopesInitializer>();

            var info = Resolve();

            Assert.AreEqual(PageFlowScopeCollectionInfo.EnumCase.ScopesUnreadable, info.GetEnumCase());
            Assert.IsTrue(info.TryGetValue(out PageFlowScopeCollectionInfo.ScopesUnreadable unreadable));
            Assert.AreEqual(nameof(ThrowingScopesInitializer), unreadable.InitializerTypeName);
            Assert.AreEqual("scopes failed", unreadable.ExceptionMessage);
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

        private sealed class ThrowingApplierInitializer : MonoBehaviour, IUguiPageCodexOnInitialize
        {
            public IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier
                => throw new InvalidOperationException("applier failed");

            public UnityTask OnInitializeAsync(UguiPageCodex codex)
                => UnityTask.CompletedTask;
        }

        private sealed class ThrowingScopesInitializer : UguiPageCodexInitializer<ThrowingScopes>
        {
            protected override UnityTask OnInitializeAsync(UguiPageCodex codex, ThrowingScopes scopes)
                => UnityTask.CompletedTask;
        }

#pragma warning disable SG_PAGEFLOWS_0006
        private readonly struct ThrowingScopes : IPageFlowScopeCollection
        {
            public ReadOnlyMemory<string> ScopeIdentifiers => throw new InvalidOperationException("scopes failed");

            public bool TrySetScope(string identifier, PageFlowScope scope)
                => false;
        }
#pragma warning restore SG_PAGEFLOWS_0006
    }
}

#endif
