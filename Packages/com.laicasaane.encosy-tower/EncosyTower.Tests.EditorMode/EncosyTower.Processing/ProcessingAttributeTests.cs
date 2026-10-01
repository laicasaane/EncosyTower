using EncosyTower.CodeGen;
using EncosyTower.Processing;
using NUnit.Framework;

namespace EncosyTower.Tests.Processing
{
    public partial class ProcessingAttributeTests
    {
        [TestCase(ApiMode.Sync)]
        [TestCase(ApiMode.Async)]
        [TestCase(ApiMode.Both)]
        public void Constructor_PreservesModeWithDefaultStateAndScope(ApiMode mode)
        {
            var attribute = new ProcessingAttribute(mode);

            Assert.AreEqual(mode, attribute.Mode);
            Assert.AreEqual(StateMode.Both, attribute.State);
            Assert.IsNull(attribute.Scope);
        }

        [Test]
        public void Properties_PreserveRawValues()
        {
            var attribute = new ProcessingAttribute((ApiMode)99) {
                State = (StateMode)99,
                Scope = typeof(Scope),
            };

            Assert.AreEqual((ApiMode)99, attribute.Mode);
            Assert.AreEqual((StateMode)99, attribute.State);
            Assert.AreEqual(typeof(Scope), attribute.Scope);
        }

        private readonly struct Scope
        {
        }
    }
}
