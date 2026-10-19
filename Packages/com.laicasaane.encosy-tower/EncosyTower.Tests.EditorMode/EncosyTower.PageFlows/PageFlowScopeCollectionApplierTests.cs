using System;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.PubSub;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows
{
    public sealed class PageFlowScopeCollectionApplierTests
    {
        private static readonly PageFlowScope s_scopeA = new(new Id3(1, 2, 3));
        private static readonly PageFlowScope s_scopeB = new(new Id3(4, 5, 6));

        [Test]
        public void ScopeIdentifiers_ListPropertiesInDeclarationOrder()
        {
            var applier = new PageFlowScopeCollectionApplier<TestScopes>();

            CollectionAssert.AreEqual(new[] { "A", "B" }, applier.ScopeIdentifiers.ToArray());
        }

        [Test]
        public void Build_WithEveryScope_StoresTheCollection()
        {
            var applier = new PageFlowScopeCollectionApplier<TestScopes>();
            var scopes = CreateMap(includeB: true);

            var built = applier.Build(scopes);

            Assert.IsTrue(built.IsSuccess);
            Assert.IsTrue(applier.TryGet(out var collection));
            Assert.AreEqual(s_scopeA, collection.A);
            Assert.AreEqual(s_scopeB, collection.B);
        }

        [Test]
        public void Build_WithMissingScope_ReportsItAndKeepsThePreviousCollection()
        {
            var applier = new PageFlowScopeCollectionApplier<TestScopes>();

            Assert.IsTrue(applier.Build(CreateMap(includeB: true)).IsSuccess);

            var otherScopes = new ArrayMap<string, PageFlowScope>();
            otherScopes.TryAdd("A", s_scopeB);

            var built = applier.Build(otherScopes);

            Assert.IsTrue(built.IsFailure);
            Assert.IsTrue(built.TryGetFailure(out var error));

            Assert.AreEqual(
                  expected: "Cannot find a flow scope for the property 'B' of the scope collection."
                , actual: error.ToMessage()
            );

            Assert.IsTrue(applier.TryGet(out var collection));
            Assert.AreEqual(s_scopeA, collection.A);
            Assert.AreEqual(s_scopeB, collection.B);
        }

        [Test]
        public void Build_WithRefusedScope_ReportsItAndDoesNotStoreTheCollection()
        {
            var applier = new PageFlowScopeCollectionApplier<RefusingScopes>();
            var scopes = CreateMap(includeB: false);

            var built = applier.Build(scopes);

            Assert.IsTrue(built.IsFailure);
            Assert.IsTrue(built.TryGetFailure(out var error));

            Assert.AreEqual(
                  expected: "The scope collection did not accept a flow scope for the property 'A'."
                , actual: error.ToMessage()
            );

            Assert.IsFalse(applier.TryGet(out _));
        }

        [Test]
        public void Build_AndApplyTo_SetsTheCollectionOnAPageThatNeedsIt()
        {
            var applier = new PageFlowScopeCollectionApplier<TestScopes>();
            var page = new TestPage();

            Assert.IsTrue(applier.Build(CreateMap(includeB: true)).IsSuccess);
            ((IPageFlowScopeCollectionApplier)applier).ApplyTo(page);

            Assert.IsTrue(page.Collection.TryGetValue(out var collection));
            Assert.AreEqual(s_scopeA, collection.A);
            Assert.AreEqual(s_scopeB, collection.B);
        }

        private static ArrayMap<string, PageFlowScope> CreateMap(bool includeB)
        {
            var map = new ArrayMap<string, PageFlowScope>();
            map.TryAdd("A", s_scopeA);

            if (includeB)
            {
                map.TryAdd("B", s_scopeB);
            }

            return map;
        }

        private sealed class TestPage : IPageNeedsFlowScopeCollection<TestScopes>
        {
            public Option<TestScopes> Collection { get; private set; }

            public Option<TestScopes> FlowScopeCollection
            {
                set => Collection = value;
            }
        }
    }

#pragma warning disable SG_PAGEFLOWS_0006
    internal readonly struct RefusingScopes : IPageFlowScopeCollection
    {
        public ReadOnlyMemory<string> ScopeIdentifiers => new[] { "A" };

        public bool TrySetScope(string identifier, PageFlowScope scope)
            => false;
    }
#pragma warning restore SG_PAGEFLOWS_0006

    [PageFlowScopeCollection]
    internal partial struct TestScopes
    {
        public PageFlowScope A { get; private set; }

        public PageFlowScope B { get; private set; }
    }
}
