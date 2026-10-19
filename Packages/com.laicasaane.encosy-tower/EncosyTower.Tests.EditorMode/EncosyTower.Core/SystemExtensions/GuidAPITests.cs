#if UNITY_COLLECTIONS

using System;
using EncosyTower.SystemExtensions;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.SystemExtensions
{
    public class GuidAPITests
    {
        private const string X_FORMAT = "{0x00112233,0x4455,0x6677,{0x88,0x99,0xaa,0xbb,0xcc,0xdd,0xee,0xff}}";

        [Test]
        public void TryParse_DefaultFixedString_ReturnsFalseWithEmptyGuid()
        {
            var parsed = GuidAPI.TryParse(guidString: default, result: out var result);

            Assert.IsFalse(parsed);
            Assert.AreEqual(Guid.Empty, result);
        }

        [TestCase("00112233-4455-6677-8899-aabbccddeeff")]
        [TestCase("00112233445566778899aabbccddeeff")]
        [TestCase("{00112233-4455-6677-8899-aabbccddeeff}")]
        [TestCase("(00112233-4455-6677-8899-aabbccddeeff)")]
        [TestCase("{0x00112233,0x4455,0x6677,{0x88,0x99,0xaa,0xbb,0xcc,0xdd,0xee,0xff}}")]
        [TestCase("00112233-4455-6677-8899-AABBCCDDEEFF")]
        [TestCase("ffffffff-ffff-ffff-ffff-ffffffffffff")]
        [TestCase("00000000-0000-0000-0000-000000000000")]
        [TestCase("")]
        [TestCase("not-a-guid")]
        [TestCase("00112233-4455-6677-8899-aabbccddeef")]
        [TestCase("00112233-4455-6677-8899-aabbccddeeffx")]
        [TestCase("00112233-4455-6677-8899-aabbccddeeffxx")]
        [TestCase("g0112233-4455-6677-8899-aabbccddeeff")]
        [TestCase("\u00e900112233-4455-6677-8899-aabbccddeeff")]
        [TestCase("{00112233-4455-6677-8899-aabbccddeeff")]
        public void TryParse_AnyText_MatchesManagedGuidTryParse(string text)
        {
            var guidString = new FixedString128Bytes(text);

            var expectedParsed = Guid.TryParse(text, out var expected);
            var parsed = GuidAPI.TryParse(guidString, out var result);

            Assert.AreEqual(expectedParsed, parsed);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TryParse_XFormatWithoutTrailingChars_ReturnsTrue()
        {
            var guidString = new FixedString128Bytes(X_FORMAT);

            var parsed = GuidAPI.TryParse(guidString, out var result);

            Assert.AreEqual(68, X_FORMAT.Length);
            Assert.IsTrue(parsed);
            Assert.AreEqual(new Guid("00112233-4455-6677-8899-aabbccddeeff"), result);
        }

        [TestCase("zzzz")]
        [TestCase("}")]
        [TestCase("00112233-4455-6677-8899-aabbccddeeff")]
        public void TryParse_XFormatWithTrailingChars_ReturnsFalse(string trailing)
        {
            var guidString = new FixedString128Bytes(X_FORMAT + trailing);

            var parsed = GuidAPI.TryParse(guidString, out var result);

            Assert.IsFalse(parsed);
            Assert.AreEqual(Guid.Empty, result);
        }

        [Test]
        public void TryParse_TextLongerThanTheFormatLimit_ReturnsFalse()
        {
            var text = "00112233-4455-6677-8899-aabbccddeeff" + new string('0', 80);
            var guidString = new FixedString128Bytes(text);

            Assert.IsFalse(Guid.TryParse(text, out _));
            Assert.IsFalse(GuidAPI.TryParse(guidString, out _));
        }

        [Test]
        public void TryParse_MaximumLengthText_DoesNotThrowAndReturnsFalse()
        {
            var text = new string('0', FixedString128Bytes.UTF8MaxLengthInBytes);
            var guidString = new FixedString128Bytes(text);
            var parsed = true;

            Assert.DoesNotThrow(() => parsed = GuidAPI.TryParse(guidString, out _));

            Assert.IsFalse(parsed);
        }
    }
}

#endif
