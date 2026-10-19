using System;
using System.Text;
using EncosyTower.Common;
using EncosyTower.StringIds;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.StringIds
{
    public class UnmanagedStringTests
    {
        private const string TABLE_NAME = "level_table_asset";

        [TestCase("level_table_asset")]
        [TestCase("a")]
        [TestCase("")]
        [TestCase("\u00dcn\u00efc\u00f6d\u00e9 \u2713")]
        public void GetHashCode64_EqualsFnv1aOfUtf8Bytes(string text)
        {
            var value = (UnmanagedString)text;

            Assert.AreEqual(ExpectedHash(text), value.GetHashCode64());
        }

        [Test]
        public void GetHashCode64_EqualsFnv1aOfUtf8Bytes_LongText()
        {
            var text = new string('x', 300);
            var value = (UnmanagedString)text;

            Assert.AreEqual(ExpectedHash(text), value.GetHashCode64());
        }

        [Test]
        public void GetHashCode64_IsStableAcrossStackHeavyCalls()
        {
            var value = (UnmanagedString)TABLE_NAME;
            var expected = ExpectedHash(TABLE_NAME);

            var first = value.GetHashCode64();
            StackOverwriter.Fill(value: 0xCD);

            var second = value.GetHashCode64();
            StackOverwriter.Fill(value: 0x00);

            var third = value.GetHashCode64();

            Assert.AreEqual(expected, first);
            Assert.AreEqual(expected, second);
            Assert.AreEqual(expected, third);
        }

        [Test]
        public void AsReadOnlySpan_ViewsTheValue_SeesLaterWrites()
        {
            var value = (UnmanagedString)"abc";
            var view = value.AsReadOnlySpan();

            value.AsSpan()[0] = (byte)'z';

            Assert.AreEqual(3, view.Length);
            Assert.AreEqual((byte)'z', view[0]);
        }

        [Test]
        public void AsReadOnlySpan_MatchesUtf8Bytes_AfterStackHeavyCalls()
        {
            var value = (UnmanagedString)TABLE_NAME;
            var view = value.AsReadOnlySpan();

            StackOverwriter.Fill(value: 0xCD);

            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(TABLE_NAME), view.ToArray());
        }

        [Test]
        public void AsSpan_Default_IsEmpty()
        {
            var value = default(UnmanagedString);

            Assert.AreEqual(0, value.Length);
            Assert.IsTrue(value.AsSpan().IsEmpty);
            Assert.IsTrue(value.AsReadOnlySpan().IsEmpty);
        }

        [Test]
        public void AsSpan_EmptyString_IsEmpty()
        {
            var value = (UnmanagedString)string.Empty;

            Assert.IsTrue(value.IsEmpty);
            Assert.IsTrue(value.AsSpan().IsEmpty);
            Assert.IsTrue(value.AsReadOnlySpan().IsEmpty);
            Assert.AreEqual(ExpectedHash(string.Empty), value.GetHashCode64());
        }

        [TestCase("A\U0001F600B")]
        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("\u2713\u2713\u2713")]
        [TestCase("\u00dcn\u00efc\u00f6d\u00e9 \u2713")]
        public void AsSpan_NonAsciiText_EqualsUtf8BytesAndHash(string text)
        {
            var value = (UnmanagedString)text;
            var expected = Encoding.UTF8.GetBytes(text);

            StackOverwriter.Fill(value: 0xCD);

            CollectionAssert.AreEqual(expected, value.AsSpan().ToArray());
            CollectionAssert.AreEqual(expected, value.AsReadOnlySpan().ToArray());
            Assert.AreEqual(ExpectedHash(text), value.GetHashCode64());
            Assert.AreEqual(text, value.ToString());
        }

        [Test]
        public void AsSpan_ExactCapacityText_KeepsEveryByte()
        {
            var text = new string('a', UnmanagedString.UTF8MaxLengthInBytes);
            var value = (UnmanagedString)text;

            Assert.AreEqual(UnmanagedString.UTF8MaxLengthInBytes, value.AsSpan().Length);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), value.AsSpan().ToArray());
            Assert.AreEqual(ExpectedHash(text), value.GetHashCode64());
        }

        [Test]
        public void AsSpan_ExactCapacityMultiByteText_KeepsEveryByte()
        {
            var text = new string('\u00e9', (UnmanagedString.UTF8MaxLengthInBytes - 1) / 2) + "a";
            var value = (UnmanagedString)text;

            Assert.AreEqual(UnmanagedString.UTF8MaxLengthInBytes, value.AsSpan().Length);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), value.AsSpan().ToArray());
        }

        [Test]
        public void AsSpan_ViewsTheValue_SeesLaterAssignments()
        {
            var value = (UnmanagedString)"abc";
            var view = value.AsSpan();

            value = (UnmanagedString)"xyz";
            StackOverwriter.Fill(value: 0xCD);

            Assert.AreEqual(3, view.Length);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("xyz"), view.ToArray());
        }

        [Test]
        public void AsSpan_WriteThroughSpan_ChangesTheStringAndTheReadOnlyView()
        {
            var value = (UnmanagedString)"abc";
            var readOnlyView = value.AsReadOnlySpan();

            value.AsSpan()[2] = (byte)'z';

            Assert.AreEqual("abz", value.ToString());
            Assert.AreEqual((byte)'z', readOnlyView[2]);
        }

        [Test]
        public void AsSpan_MatchesUtf8Bytes_AfterStackHeavyCalls()
        {
            var value = (UnmanagedString)TABLE_NAME;
            var view = value.AsSpan();

            StackOverwriter.Fill(value: 0xCD);

            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(TABLE_NAME), view.ToArray());
        }

        private static ulong ExpectedHash(string text)
            => HashValue64.FNV1a((ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(text));
    }
}
