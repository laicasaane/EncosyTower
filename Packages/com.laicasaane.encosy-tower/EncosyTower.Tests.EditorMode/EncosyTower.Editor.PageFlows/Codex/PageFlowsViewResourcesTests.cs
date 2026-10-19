using EncosyTower.Editor.PageFlows;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Editor.PageFlows
{
    public sealed class PageFlowsViewResourcesTests
    {
        [Test]
        public void TransitionLabels_ReadFromResources()
        {
            var texts = PageFlowsViewResources.Get().Transition;

            Assert.AreEqual("Start", texts.Start);
            Assert.AreEqual("End", texts.End);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
