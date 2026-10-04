using EncosyTower.Common;
using EncosyTower.Serialization;
using NUnit.Framework;

namespace EncosyTower.Tests.Serialization
{
    public sealed class OverridableTests
    {
        [Test]
        public void Equality_UsesValueAndOverrideState()
        {
            var overridden = new Overridable<int>(5, isOverridden: true);

            Assert.IsTrue(overridden.Equals(new Overridable<int>(5, isOverridden: true)));
            Assert.AreEqual(overridden.GetHashCode(), new Overridable<int>(5, isOverridden: true).GetHashCode());
            Assert.IsFalse(overridden.Equals(new Overridable<int>(5, isOverridden: false)));
            Assert.IsFalse(overridden.Equals(new Overridable<int>(6, isOverridden: true)));
            Assert.IsTrue(overridden.Equals((object)new Overridable<int>(5, isOverridden: true)));
            Assert.IsFalse(overridden.Equals((object)5));
        }

        [Test]
        public void GetValueOrDefault_ReturnsTheOverrideOnlyWhenOverridden()
        {
            Assert.AreEqual(5, new Overridable<int>(5, isOverridden: true).GetValueOrDefault(9));
            Assert.AreEqual(9, new Overridable<int>(5, isOverridden: false).GetValueOrDefault(9));
            Assert.AreEqual(0, default(Overridable<int>).GetValueOrDefault());
        }

        [Test]
        public void TryGetValue_SucceedsOnlyWhenOverridden()
        {
            Assert.IsTrue(new Overridable<int>(5, isOverridden: true).TryGetValue(out var value));
            Assert.AreEqual(5, value);
            Assert.IsFalse(new Overridable<int>(5, isOverridden: false).TryGetValue(out _));
        }

        [Test]
        public void OptionConversions_MapOverrideStateToSomeAndNone()
        {
            Option<int> some = new Overridable<int>(5, isOverridden: true);
            Option<int> none = new Overridable<int>(5, isOverridden: false);
            Overridable<int> fromSome = Option.Some(3);
            Overridable<int> fromNone = Option<int>.None;

            Assert.IsTrue(some.TryGetValue(out var someValue));
            Assert.AreEqual(5, someValue);
            Assert.IsFalse(none.HasValue);
            Assert.AreEqual(new Overridable<int>(3, isOverridden: true), fromSome);
            Assert.AreEqual(default(Overridable<int>), fromNone);
        }

        [Test]
        public void ImplicitFromValue_KeepsTheValueWithoutOverriding()
        {
            Overridable<int> overridable = 7;

            Assert.AreEqual(7, overridable.value);
            Assert.IsFalse(overridable.isOverridden);
        }
    }
}
