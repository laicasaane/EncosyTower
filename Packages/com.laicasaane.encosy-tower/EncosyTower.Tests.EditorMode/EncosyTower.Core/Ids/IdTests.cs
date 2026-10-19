using System;
using EncosyTower.Ids;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Ids
{
    public class IdTests
    {
        [TestCase(0, 0u)]
        [TestCase(1, 1u)]
        [TestCase(-1, uint.MaxValue)]
        [TestCase(int.MaxValue, 0x7FFFFFFFu)]
        [TestCase(int.MinValue, 0x80000000u)]
        [TestCase(-2, 0xFFFFFFFEu)]
        public void Constructor_Int_KeepsTheBitPattern(int value, uint expected)
        {
            var id = new Id(value);

            Assert.AreEqual(expected, (uint)id);
        }

        [TestCase(0u)]
        [TestCase(1u)]
        [TestCase(0x7FFFFFFFu)]
        [TestCase(0x80000000u)]
        [TestCase(uint.MaxValue)]
        public void Constructor_UInt_RoundTripsThroughImplicitConversion(uint value)
        {
            var id = new Id(value);

            Assert.AreEqual(value, (uint)id);
            Assert.AreEqual(new Id(value), id);
        }

        [Test]
        public void Constructor_NegativeInt_EqualsTheUnsignedWithTheSameBits()
        {
            Id fromInt = -1;
            Id fromUInt = uint.MaxValue;

            Assert.IsTrue(fromInt == fromUInt);
            Assert.AreEqual(fromUInt.GetHashCode(), fromInt.GetHashCode());
        }

        [Test]
        public void Constructor_Default_IsZero()
        {
            var id = default(Id);

            Assert.AreEqual(0u, (uint)id);
            Assert.IsTrue(id == new Id(0));
        }

        [Test]
        public void CompareTo_NegativeIntId_OrdersAsUnsigned()
        {
            var large = new Id(-1);
            var small = new Id(1);

            Assert.Greater(large.CompareTo(small), 0);
            Assert.Less(small.CompareTo(large), 0);
            Assert.IsTrue(large > small);
            Assert.IsTrue(small < large);
            Assert.IsTrue(large >= large);
            Assert.IsTrue(small <= small);
        }

        [Test]
        public void ToString_NegativeIntId_PrintsTheUnsignedValue()
        {
            var id = new Id(-1);

            Assert.AreEqual("4294967295", id.ToString());
        }

        [TestCase("0", 0u)]
        [TestCase("42", 42u)]
        [TestCase("4294967295", uint.MaxValue)]
        public void TryParse_ValidText_ReturnsTheValue(string text, uint expected)
        {
            var parser = default(Id);

            var parsed = parser.TryParse(text.AsSpan(), out var result);

            Assert.IsTrue(parsed);
            Assert.AreEqual(expected, (uint)result);
        }

        [TestCase("")]
        [TestCase("-1")]
        [TestCase("4294967296")]
        [TestCase("abc")]
        [TestCase("1.5")]
        public void TryParse_InvalidText_ReturnsFalseWithDefault(string text)
        {
            var parser = default(Id);

            var parsed = parser.TryParse(text.AsSpan(), out var result);

            Assert.IsFalse(parsed);
            Assert.AreEqual(0u, (uint)result);
        }

        [Test]
        public void TryParse_NullString_ReturnsFalse()
        {
            var parser = default(Id);

            var parsed = parser.TryParse((string)null, out var result);

            Assert.IsFalse(parsed);
            Assert.AreEqual(0u, (uint)result);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        [TestCase(int.MaxValue)]
        [TestCase(int.MinValue)]
        public void Union_IntValue_OverlaysTheSameBitsAsUIntValue(int value)
        {
            var union = new Id.Union(value);

            Assert.AreEqual(value, union.intValue);
            Assert.AreEqual(unchecked((uint)value), union.uintValue);
        }

        [Test]
        public void Union_WriteThroughOneField_IsSeenByTheOtherField()
        {
            var union = new Id.Union(0);

            union.uintValue = 0xFFFFFFFEu;

            Assert.AreEqual(-2, union.intValue);

            union.intValue = int.MinValue;

            Assert.AreEqual(0x80000000u, union.uintValue);
        }
    }
}
