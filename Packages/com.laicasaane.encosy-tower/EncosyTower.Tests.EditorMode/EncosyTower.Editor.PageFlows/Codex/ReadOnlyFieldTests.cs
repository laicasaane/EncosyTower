using EncosyTower.Editor.PageFlows;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.PageFlows
{
    public sealed class ReadOnlyFieldTests
    {
        [Test]
        public void SetText_KeepsOnlyTheLatestIcon()
        {
            var field = new ReadOnlyField("Field");
            var first = new VisualElement();
            var second = new VisualElement();

            field.SetText("a", first);
            field.SetText("b", second);

            Assert.IsNull(first.parent);
            Assert.IsNotNull(second.parent);

            field.SetText("c");

            Assert.IsNull(second.parent);
            Assert.AreEqual(1, field.Query<Label>().Where(static label => label.text == "c").ToList().Count);
        }
    }
}
