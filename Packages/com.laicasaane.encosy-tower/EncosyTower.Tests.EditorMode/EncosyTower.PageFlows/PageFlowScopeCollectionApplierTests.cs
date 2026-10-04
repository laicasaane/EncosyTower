using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.PageFlows;
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
        public void TryBuild_WithEveryScope_StoresTheCollection()
        {
            var applier = new PageFlowScopeCollectionApplier<TestScopes>();
            var scopes = CreateMap(includeB: true);

            var built = applier.TryBuild(scopes, out var missingIdentifier);

            Assert.IsTrue(built);
            Assert.IsNull(missingIdentifier);
            Assert.IsTrue(applier.TryGet(out var collection));
            Assert.AreEqual(s_scopeA, collection.A);
            Assert.AreEqual(s_scopeB, collection.B);
        }

        [Test]
        public void TryBuild_WithMissingScope_ReportsItAndKeepsThePreviousCollection()
        {
            var applier = new PageFlowScopeCollectionApplier<TestScopes>();

            Assert.IsTrue(applier.TryBuild(CreateMap(includeB: true), out _));

            var otherScopes = new ArrayMap<string, PageFlowScope>();
            otherScopes.TryAdd("A", s_scopeB);

            var built = applier.TryBuild(otherScopes, out var missingIdentifier);

            Assert.IsFalse(built);
            Assert.AreEqual("B", missingIdentifier);
            Assert.IsTrue(applier.TryGet(out var collection));
            Assert.AreEqual(s_scopeA, collection.A);
            Assert.AreEqual(s_scopeB, collection.B);
        }

        [Test]
        public void ApplyTo_SetsTheCollectionOnAPageThatNeedsIt()
        {
            var applier = new PageFlowScopeCollectionApplier<TestScopes>();
            var page = new TestPage();

            applier.TryBuild(CreateMap(includeB: true), out _);
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

    [PageFlowScopeCollection]
    internal partial struct TestScopes
    {
        public PageFlowScope A { get; private set; }

        public PageFlowScope B { get; private set; }
    }
}
