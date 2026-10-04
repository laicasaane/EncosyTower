using EncosyTower.PageFlows;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows
{
    public sealed class PageFlowCodexValidatorTests
    {
        private static readonly string[] s_scopes = { "Screen", "Popup", "FreeTop" };

        [Test]
        public void MatchingIdentifiers_AreValid()
        {
            var result = PageFlowCodexValidator.Validate(new[] { "Popup", "Screen", "FreeTop" }, s_scopes);

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(-1, result.FirstEmptyIndex);
            CollectionAssert.IsEmpty(result.DuplicateIdentifiers);
            CollectionAssert.IsEmpty(result.DefinitionsWithoutScope);
            CollectionAssert.IsEmpty(result.ScopesWithoutDefinition);
        }

        [Test]
        public void EmptyIdentifier_ReportsItsFirstIndex()
        {
            var result = PageFlowCodexValidator.Validate(new[] { "Screen", "", "Popup", null, "FreeTop" }, s_scopes);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.FirstEmptyIndex);
            CollectionAssert.IsEmpty(result.DuplicateIdentifiers);
            CollectionAssert.IsEmpty(result.DefinitionsWithoutScope);
            CollectionAssert.IsEmpty(result.ScopesWithoutDefinition);
        }

        [Test]
        public void RepeatedIdentifier_IsReportedOnce()
        {
            var result = PageFlowCodexValidator.Validate(
                  new[] { "Screen", "Popup", "Screen", "FreeTop", "Screen" }
                , s_scopes
            );

            Assert.IsFalse(result.IsValid);
            CollectionAssert.AreEqual(new[] { "Screen" }, result.DuplicateIdentifiers);
            CollectionAssert.IsEmpty(result.DefinitionsWithoutScope);
            CollectionAssert.IsEmpty(result.ScopesWithoutDefinition);
        }

        [Test]
        public void DefinitionWithoutScope_IsReported()
        {
            var result = PageFlowCodexValidator.Validate(new[] { "Screen", "Popup", "FreeTop", "Toast" }, s_scopes);

            Assert.IsFalse(result.IsValid);
            CollectionAssert.AreEqual(new[] { "Toast" }, result.DefinitionsWithoutScope);
            CollectionAssert.IsEmpty(result.ScopesWithoutDefinition);
        }

        [Test]
        public void ScopeWithoutDefinition_IsReported()
        {
            var result = PageFlowCodexValidator.Validate(new[] { "Screen" }, s_scopes);

            Assert.IsFalse(result.IsValid);
            CollectionAssert.IsEmpty(result.DefinitionsWithoutScope);
            CollectionAssert.AreEqual(new[] { "Popup", "FreeTop" }, result.ScopesWithoutDefinition);
        }

        [Test]
        public void AllProblems_AreReportedTogetherInFirstOccurrenceOrder()
        {
            var result = PageFlowCodexValidator.Validate(
                  new[] { "Toast", "Screen", "", "Toast", "Screen", "Banner" }
                , s_scopes
            );

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.FirstEmptyIndex);
            CollectionAssert.AreEqual(new[] { "Toast", "Screen" }, result.DuplicateIdentifiers);
            CollectionAssert.AreEqual(new[] { "Toast", "Banner" }, result.DefinitionsWithoutScope);
            CollectionAssert.AreEqual(new[] { "Popup", "FreeTop" }, result.ScopesWithoutDefinition);
        }
    }
}
