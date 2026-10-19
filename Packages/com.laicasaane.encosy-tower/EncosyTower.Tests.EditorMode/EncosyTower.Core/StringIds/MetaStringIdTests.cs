using System;
using EncosyTower.Ids;
using EncosyTower.StringIds;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.StringIds
{
    public class MetaStringIdTests
    {
        [Test]
        public void Constructor_Default_HasNoIdAndNoMeta()
        {
            var value = default(MetaStringId);

            Assert.IsFalse(value.Id.IsValid);
            Assert.AreEqual(0u, (uint)value.Id.Id);
            Assert.AreEqual(0u, (uint)value.Meta);
            Assert.AreEqual(0UL, (ulong)value);
        }

        [Test]
        public void Constructor_IdAndMeta_ReturnsBothParts()
        {
            var value = new MetaStringId(id: CreateStringId(3u), meta: new Id(9u));

            Assert.AreEqual(3u, (uint)value.Id.Id);
            Assert.AreEqual(9u, (uint)value.Meta);
            Assert.IsTrue(value.Id.IsValid);
        }

        [Test]
        public void Constructor_IdAndMeta_OverlaysIdInLowBitsAndMetaInHighBits()
        {
            var value = new MetaStringId(id: CreateStringId(3u), meta: new Id(9u));

            Assert.AreEqual(3UL | (9UL << 32), (ulong)value);
        }

        [Test]
        public void Constructor_MaximumParts_RoundTripsBitExact()
        {
            var value = new MetaStringId(id: CreateStringId(uint.MaxValue), meta: new Id(uint.MaxValue));

            Assert.AreEqual(uint.MaxValue, (uint)value.Id.Id);
            Assert.AreEqual(uint.MaxValue, (uint)value.Meta);
            Assert.AreEqual(ulong.MaxValue, (ulong)value);
        }

        [Test]
        public void Constructor_NegativeIntParts_KeepTheBitPattern()
        {
            var value = new MetaStringId(id: CreateStringId(new Id(-1)), meta: new Id(int.MinValue));

            Assert.AreEqual(uint.MaxValue, (uint)value.Id.Id);
            Assert.AreEqual(0x80000000u, (uint)value.Meta);
        }

        [Test]
        public void Constructor_OnlyIdSet_LeavesMetaZero()
        {
            var value = new MetaStringId(id: CreateStringId(uint.MaxValue), meta: new Id(0u));

            Assert.AreEqual(0u, (uint)value.Meta);
            Assert.AreEqual((ulong)uint.MaxValue, (ulong)value);
        }

        [Test]
        public void Constructor_OnlyMetaSet_LeavesIdZero()
        {
            var value = new MetaStringId(id: CreateStringId(0u), meta: new Id(uint.MaxValue));

            Assert.AreEqual(0u, (uint)value.Id.Id);
            Assert.AreEqual((ulong)uint.MaxValue << 32, (ulong)value);
        }

        [Test]
        public void Deconstruct_ReturnsTheConstructorParts()
        {
            var value = new MetaStringId(id: CreateStringId(5u), meta: new Id(6u));

            value.Deconstruct(out var id, out var meta);

            Assert.AreEqual(CreateStringId(5u), id);
            Assert.AreEqual(new Id(6u), meta);
        }

        [Test]
        public void TupleConversion_IdThenStringId_FirstElementBecomesIdAndSecondBecomesMeta()
        {
            MetaStringId value = (new Id(5u), CreateStringId(7u));

            Assert.AreEqual(5u, (uint)value.Id.Id);
            Assert.AreEqual(7u, (uint)value.Meta);
        }

        [Test]
        public void Equality_SameParts_IsEqualWithSameHash()
        {
            var first = new MetaStringId(id: CreateStringId(5u), meta: new Id(6u));
            var second = new MetaStringId(id: CreateStringId(5u), meta: new Id(6u));

            Assert.IsTrue(first == second);
            Assert.IsFalse(first != second);
            Assert.IsTrue(first.Equals(second));
            Assert.IsTrue(first.Equals((object)second));
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Test]
        public void Equality_DifferentIdOrMeta_IsNotEqual()
        {
            var value = new MetaStringId(id: CreateStringId(5u), meta: new Id(6u));
            var otherId = new MetaStringId(id: CreateStringId(6u), meta: new Id(6u));
            var otherMeta = new MetaStringId(id: CreateStringId(5u), meta: new Id(5u));

            Assert.IsTrue(value != otherId);
            Assert.IsTrue(value != otherMeta);
            Assert.IsFalse(value.Equals("not a MetaStringId"));
        }

        [Test]
        public void CompareTo_MetaIsMoreSignificantThanId()
        {
            var smallMetaLargeId = new MetaStringId(id: CreateStringId(100u), meta: new Id(0u));
            var largeMetaSmallId = new MetaStringId(id: CreateStringId(5u), meta: new Id(1u));

            Assert.Less(smallMetaLargeId.CompareTo(largeMetaSmallId), 0);
            Assert.Greater(largeMetaSmallId.CompareTo(smallMetaLargeId), 0);
            Assert.IsTrue(smallMetaLargeId < largeMetaSmallId);
            Assert.IsTrue(largeMetaSmallId > smallMetaLargeId);

            var smallCopy = smallMetaLargeId;
            var largeCopy = largeMetaSmallId;

            Assert.IsTrue(smallMetaLargeId <= smallCopy);
            Assert.IsTrue(largeMetaSmallId >= largeCopy);
            Assert.Greater(smallMetaLargeId.CompareTo((object)"not a MetaStringId"), 0);
        }

        [Test]
        public void TryFormat_EnoughSpace_WritesIdDashMeta()
        {
            var value = new MetaStringId(id: CreateStringId(5u), meta: new Id(7u));
            var destination = new char[8];

            var formatted = value.TryFormat(destination, out var charsWritten);

            Assert.IsTrue(formatted);
            Assert.AreEqual("5-7", new string(destination, 0, charsWritten));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void TryFormat_TooSmallDestination_ReturnsFalseWithZeroCharsWritten(int length)
        {
            var value = new MetaStringId(id: CreateStringId(5u), meta: new Id(7u));
            var destination = new char[length];

            var formatted = value.TryFormat(destination, out var charsWritten);

            Assert.IsFalse(formatted);
            Assert.AreEqual(0, charsWritten);
        }

        [Test]
        public void ToString_IdAndMeta_JoinsWithDash()
        {
            var value = new MetaStringId(id: CreateStringId(12u), meta: new Id(34u));

            Assert.AreEqual("12-34", value.ToString());
        }

        private static StringId CreateStringId(uint value)
            => new(new Id(value));

        private static StringId CreateStringId(Id value)
            => new(value);
    }
}
