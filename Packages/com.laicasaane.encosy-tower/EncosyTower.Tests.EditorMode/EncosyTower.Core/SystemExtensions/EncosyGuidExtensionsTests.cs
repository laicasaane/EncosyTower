using System;
using EncosyTower.SystemExtensions;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.SystemExtensions
{
    public class EncosyGuidExtensionsTests
    {
        private static readonly Guid s_sample = new("00112233-4455-6677-8899-aabbccddeeff");

        [Test]
        public void AsReadOnlySpan_EmptyGuid_IsSixteenZeroBytes()
        {
            var guid = Guid.Empty;
            var view = guid.AsReadOnlySpan();

            Assert.AreEqual(16, view.Length);
            CollectionAssert.AreEqual(new byte[16], view.ToArray());
        }

        [Test]
        public void AsReadOnlySpan_ArrayElement_ViewsOnlyThatElement()
        {
            var first = new Guid("00112233-4455-6677-8899-aabbccddeeff");
            var second = new Guid("ffeeddcc-bbaa-9988-7766-554433221100");
            var guids = new[] { first, second };

            var view = guids[1].AsReadOnlySpan();

            StackOverwriter.Fill(value: 0xCD);
            guids[1] = Guid.Empty;

            CollectionAssert.AreEqual(new byte[16], view.ToArray());
            CollectionAssert.AreEqual(first.ToByteArray(), guids[0].AsReadOnlySpan().ToArray());
        }

        [Test]
        public void AsReadOnlySpan_Bytes_RoundTripThroughTheGuidConstructor()
        {
            var guids = new[] {
                s_sample,
                Guid.Empty,
                new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Guid.NewGuid(),
                Guid.NewGuid(),
            };

            var count = guids.Length;

            for (var i = 0; i < count; i++)
            {
                var guid = guids[i];

                Assert.AreEqual(guid, new Guid(guid.AsReadOnlySpan()), $"index {i}");
            }
        }

#if UNITY_COLLECTIONS
        [Test]
        public void ToFixedString_EmptyGuid_IsZeroFilledDFormat()
        {
            var guid = Guid.Empty;

            Assert.AreEqual("00000000-0000-0000-0000-000000000000", guid.ToFixedString().ToString());
        }

        [Test]
        public void ToFixedString_NoFormat_UsesDFormat()
        {
            var guid = s_sample;

            Assert.AreEqual(guid.ToString("D"), guid.ToFixedString().ToString());
        }

        [Test]
        public void ToFixedString_EmptyFormat_UsesDFormat()
        {
            var guid = s_sample;

            Assert.AreEqual(guid.ToString("D"), guid.ToFixedString(ReadOnlySpan<char>.Empty).ToString());
        }

        [TestCase("N")]
        [TestCase("D")]
        [TestCase("B")]
        [TestCase("P")]
        [TestCase("X")]
        [TestCase("n")]
        [TestCase("d")]
        [TestCase("b")]
        [TestCase("p")]
        [TestCase("x")]
        public void ToFixedString_Format_EqualsGuidToString(string format)
        {
            var guid = s_sample;

            Assert.AreEqual(guid.ToString(format), guid.ToFixedString(format.AsSpan()).ToString());
        }

        [TestCase("N")]
        [TestCase("D")]
        [TestCase("B")]
        [TestCase("P")]
        [TestCase("X")]
        public void ToFixedString_FormatOfEverySampleGuid_EqualsGuidToString(string format)
        {
            for (var i = 0; i < 32; i++)
            {
                var guid = Guid.NewGuid();

                Assert.AreEqual(guid.ToString(format), guid.ToFixedString(format.AsSpan()).ToString(), $"index {i}");
            }
        }

        [Test]
        public void ToFixedString_FormatOfMaximumGuid_UsesLowerCaseHexDigits()
        {
            var guid = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");

            Assert.AreEqual(guid.ToString("D"), guid.ToFixedString().ToString());
            Assert.AreEqual(guid.ToString("X"), guid.ToFixedString("X".AsSpan()).ToString());
        }

        [TestCase("Z")]
        [TestCase("DD")]
        [TestCase("0")]
        public void ToFixedString_UnknownFormat_ThrowsFormatException(string format)
        {
            Assert.Throws<FormatException>(() => s_sample.ToFixedString(format.AsSpan()));
        }
#endif
    }
}
