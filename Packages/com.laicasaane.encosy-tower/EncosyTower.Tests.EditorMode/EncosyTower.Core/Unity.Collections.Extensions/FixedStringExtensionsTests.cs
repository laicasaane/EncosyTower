#if UNITY_COLLECTIONS

using System;
using System.Text;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections
{
    public class FixedStringExtensionsTests
    {
        [Test]
        public void ToFixedString_AllValueOverloadsReturnExpectedText()
        {
            Assert.AreEqual("True", EncosyFixedStringExtensions.ToFixedString(true).ToString());
            Assert.AreEqual("False", EncosyFixedStringExtensions.ToFixedString(false).ToString());
            Assert.AreEqual("12", EncosyFixedStringExtensions.ToFixedString((byte)12).ToString());
            Assert.AreEqual(
                  "-12"
                , EncosyFixedStringExtensions.ToFixedString((sbyte)-12).ToString()
            );
            Assert.AreEqual(
                  "-123"
                , EncosyFixedStringExtensions.ToFixedString((short)-123).ToString()
            );
            Assert.AreEqual(
                  "123"
                , EncosyFixedStringExtensions.ToFixedString((ushort)123).ToString()
            );
            Assert.AreEqual("-456", EncosyFixedStringExtensions.ToFixedString(-456).ToString());
            Assert.AreEqual("456", EncosyFixedStringExtensions.ToFixedString(456u).ToString());
            Assert.AreEqual("-789", EncosyFixedStringExtensions.ToFixedString(-789L).ToString());
            Assert.AreEqual("789", EncosyFixedStringExtensions.ToFixedString(789UL).ToString());
            Assert.AreEqual("1.5", EncosyFixedStringExtensions.ToFixedString(1.5f).ToString());
            Assert.AreEqual(
                  "2"
                , EncosyFixedStringExtensions.ToFixedString(new Index(2)).ToString()
            );

            var range = new Range(new Index(1), new Index(3));

            Assert.AreEqual("[1, 3]", EncosyFixedStringExtensions.ToFixedString(range).ToString());
        }

        [Test]
        public void CastMembers_AllSourceOverloadsCopyText()
        {
            FixedString32Bytes source32 = "abc";
            FixedString64Bytes source64 = "abc";
            FixedString128Bytes source128 = "abc";
            FixedString512Bytes source512 = "abc";
            FixedString4096Bytes source4096 = "abc";

            var from32 = EncosyFixedStringExtensions.CastTo<FixedString128Bytes>(in source32);
            var from64 = EncosyFixedStringExtensions.CastTo<FixedString128Bytes>(in source64);
            var from128 = EncosyFixedStringExtensions.CastTo<FixedString512Bytes>(in source128);
            var from512 = EncosyFixedStringExtensions.CastTo<FixedString4096Bytes>(in source512);
            var from4096 = EncosyFixedStringExtensions.CastTo<FixedString4096Bytes>(in source4096);
            var generic = EncosyFixedStringExtensions
                .Cast<FixedString32Bytes, FixedString128Bytes>(source32);
            var marker = EncosyFixedStringExtensions.CastTo(
                  source32
                , GenericT.T<FixedString128Bytes>()
            );

            Assert.AreEqual("abc", from32.ToString());
            Assert.AreEqual("abc", from64.ToString());
            Assert.AreEqual("abc", from128.ToString());
            Assert.AreEqual("abc", from512.ToString());
            Assert.AreEqual("abc", from4096.ToString());
            Assert.AreEqual("abc", generic.ToString());
            Assert.AreEqual("abc", marker.ToString());
        }

        [Test]
        public void CopyAndAppendMembers_HandleEmptyExactAndTruncatedInput()
        {
            FixedString32Bytes utf16 = "old";
            ReadOnlySpan<char> emptyChars = ReadOnlySpan<char>.Empty;
            ReadOnlySpan<char> abcChars = "abc".AsSpan();

            Assert.AreEqual(
                  CopyError.None
                , EncosyFixedStringExtensions.CopyFrom(ref utf16, emptyChars)
            );
            Assert.AreEqual(string.Empty, utf16.ToString());
            Assert.AreEqual(
                  CopyError.None
                , EncosyFixedStringExtensions.CopyFrom(ref utf16, abcChars)
            );
            Assert.AreEqual("abc", utf16.ToString());

            var exactText = new string('x', utf16.Capacity);
            ReadOnlySpan<char> exactChars = exactText.AsSpan();

            Assert.AreEqual(
                  CopyError.None
                , EncosyFixedStringExtensions.CopyFrom(ref utf16, exactChars)
            );
            Assert.AreEqual(exactText, utf16.ToString());

            ReadOnlySpan<char> tooManyChars = new string('y', utf16.Capacity + 1).AsSpan();

            Assert.AreEqual(
                  CopyError.Truncation
                , EncosyFixedStringExtensions.CopyFrom(ref utf16, tooManyChars)
            );

            FixedString32Bytes truncatedChars = default;
            var truncatedCharResult = EncosyFixedStringExtensions.CopyFromTruncated(
                  ref truncatedChars
                , tooManyChars
            );

            Assert.AreEqual(CopyError.Truncation, truncatedCharResult);
            Assert.AreEqual(truncatedChars.Capacity, truncatedChars.Length);

            FixedString32Bytes appendedChars = "a";

            Assert.AreEqual(
                  FormatError.None
                , EncosyFixedStringExtensions.Append(ref appendedChars, "bc".AsSpan())
            );
            Assert.AreEqual("abc", appendedChars.ToString());

            var fullBeforeAppend = new string('q', appendedChars.Capacity);
            appendedChars = fullBeforeAppend;

            Assert.AreEqual(
                  FormatError.Overflow
                , EncosyFixedStringExtensions.Append(ref appendedChars, "z".AsSpan())
            );
            Assert.AreEqual(fullBeforeAppend, appendedChars.ToString());

            var abcBytes = Encoding.UTF8.GetBytes("abc");
            ReadOnlySpan<byte> emptyBytes = ReadOnlySpan<byte>.Empty;
            ReadOnlySpan<byte> utf8Bytes = abcBytes;
            FixedString32Bytes utf8 = "old";

            Assert.AreEqual(
                  CopyError.None
                , EncosyFixedStringExtensions.CopyFrom(ref utf8, emptyBytes)
            );
            Assert.AreEqual(string.Empty, utf8.ToString());
            Assert.AreEqual(
                  CopyError.None
                , EncosyFixedStringExtensions.CopyFrom(ref utf8, utf8Bytes)
            );
            Assert.AreEqual("abc", utf8.ToString());

            var exactBytesArray = Encoding.UTF8.GetBytes(new string('p', utf8.Capacity));
            ReadOnlySpan<byte> exactBytes = exactBytesArray;

            Assert.AreEqual(
                  CopyError.None
                , EncosyFixedStringExtensions.CopyFrom(ref utf8, exactBytes)
            );
            Assert.AreEqual(new string('p', utf8.Capacity), utf8.ToString());

            var tooManyBytesArray = Encoding.UTF8.GetBytes(new string('r', utf8.Capacity + 1));
            ReadOnlySpan<byte> tooManyBytes = tooManyBytesArray;

            Assert.AreEqual(
                  CopyError.Truncation
                , EncosyFixedStringExtensions.CopyFrom(ref utf8, tooManyBytes)
            );

            FixedString32Bytes truncatedBytes = default;
            var truncatedByteResult = EncosyFixedStringExtensions.CopyFromTruncated(
                  ref truncatedBytes
                , tooManyBytes
            );

            Assert.AreEqual(CopyError.Truncation, truncatedByteResult);
            Assert.AreEqual(truncatedBytes.Capacity, truncatedBytes.Length);

            FixedString32Bytes appendedBytes = "a";
            ReadOnlySpan<byte> suffixBytes = Encoding.UTF8.GetBytes("bc");

            Assert.AreEqual(
                  FormatError.None
                , EncosyFixedStringExtensions.Append(ref appendedBytes, suffixBytes)
            );
            Assert.AreEqual("abc", appendedBytes.ToString());

            var fullBeforeByteAppend = new string('s', appendedBytes.Capacity);
            appendedBytes = fullBeforeByteAppend;
            ReadOnlySpan<byte> finalByte = new byte[] { (byte)'z' };

            Assert.AreEqual(
                  FormatError.Overflow
                , EncosyFixedStringExtensions.Append(ref appendedBytes, finalByte)
            );
            Assert.AreEqual(fullBeforeByteAppend, appendedBytes.ToString());
        }

        [Test]
        public void OutputViewsNativeTextAndFormatting_ReturnExpectedContent()
        {
            FixedString64Bytes value = "abc";
            var destination = new char[3];
            var shortDestination = new char[2];

            Assert.AreEqual(
                  CopyError.None
                , EncosyFixedStringExtensions.CopyTo(value, destination, out var copiedLength)
            );
            Assert.AreEqual(3, copiedLength);
            CollectionAssert.AreEqual(new[] { 'a', 'b', 'c' }, destination);
            Assert.AreEqual(
                  CopyError.Truncation
                , EncosyFixedStringExtensions.CopyTo(value, shortDestination, out _)
            );

            var builder = new StringBuilder("start:");
            EncosyFixedStringExtensions.AppendTo(value, builder, out var appendedLength);

            Assert.AreEqual(3, appendedLength);
            Assert.AreEqual("start:abc", builder.ToString());

            Span<byte> writable = EncosyFixedStringExtensions.AsSpan(ref value);
            writable[0] = (byte)'z';
            ReadOnlySpan<byte> readOnly = EncosyFixedStringExtensions.AsReadOnlySpan(ref value);

            Assert.AreEqual("zbc", value.ToString());
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("zbc"), readOnly.ToArray());

            var formatted = new char[3];
            Assert.IsTrue(
                EncosyFixedStringExtensions.TryFormat(value, formatted, out var charsWritten)
            );
            Assert.AreEqual(3, charsWritten);
            CollectionAssert.AreEqual(new[] { 'z', 'b', 'c' }, formatted);

            var formattedWithArguments = new char[3];
            Assert.IsTrue(
                EncosyFixedStringExtensions.TryFormat(
                      value
                    , formattedWithArguments
                    , out var argumentCharsWritten
                    , "ignored".AsSpan()
                    , null
                )
            );
            Assert.AreEqual(3, argumentCharsWritten);
            CollectionAssert.AreEqual(new[] { 'z', 'b', 'c' }, formattedWithArguments);

            var tooShort = new char[2];
            Assert.IsFalse(
                EncosyFixedStringExtensions.TryFormat(value, tooShort, out var failedCharsWritten)
            );
            Assert.AreEqual(0, failedCharsWritten);

            NativeText allocatorText = default;
            NativeText handleText = default;

            try
            {
                allocatorText = EncosyFixedStringExtensions.ToNativeText(
                      value
                    , Allocator.Persistent
                );
                handleText = EncosyFixedStringExtensions.ToNativeText(
                      value
                    , AllocatorManager.Persistent
                );

                Assert.AreEqual("zbc", allocatorText.ToString());
                Assert.AreEqual("zbc", handleText.ToString());
            }
            finally
            {
                if (allocatorText.IsCreated)
                {
                    allocatorText.Dispose();
                }

                if (handleText.IsCreated)
                {
                    handleText.Dispose();
                }
            }
        }

        [Test]
        public void AsReadOnlySpan_ViewsTheVariable_SeesLaterWrites()
        {
            FixedString512Bytes value = "abc";
            var view = EncosyFixedStringExtensions.AsReadOnlySpan(ref value);

            EncosyFixedStringExtensions.AsSpan(ref value)[0] = (byte)'z';

            Assert.AreEqual((byte)'z', view[0]);
        }

        [Test]
        public void ToNativeText_RoundTripsEveryFixedStringSize()
        {
            AssertToNativeTextRoundTrips<FixedString32Bytes>(FixedString32Bytes.UTF8MaxLengthInBytes);
            AssertToNativeTextRoundTrips<FixedString64Bytes>(FixedString64Bytes.UTF8MaxLengthInBytes);
            AssertToNativeTextRoundTrips<FixedString128Bytes>(FixedString128Bytes.UTF8MaxLengthInBytes);
            AssertToNativeTextRoundTrips<FixedString512Bytes>(FixedString512Bytes.UTF8MaxLengthInBytes);
            AssertToNativeTextRoundTrips<FixedString4096Bytes>(FixedString4096Bytes.UTF8MaxLengthInBytes);
        }

        private static void AssertToNativeTextRoundTrips<TFixedString>(int byteCount)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            var chars = new char[byteCount];

            for (var i = 0; i < byteCount; i++)
            {
                chars[i] = (char)('a' + i % 26);
            }

            var text = new string(chars);
            var source = default(TFixedString);

            Assert.AreEqual(CopyError.None, EncosyFixedStringExtensions.CopyFrom(ref source, text.AsSpan()));
            Assert.AreEqual(byteCount, source.Length);

            NativeText allocatorText = default;
            NativeText handleText = default;

            try
            {
                allocatorText = EncosyFixedStringExtensions.ToNativeText(source, Allocator.Persistent);
                StackOverwriter.Fill(value: 0xCD);

                handleText = EncosyFixedStringExtensions.ToNativeText(source, AllocatorManager.Persistent);
                StackOverwriter.Fill(value: 0x00);

                Assert.AreEqual(byteCount, allocatorText.Length);
                Assert.AreEqual(text, allocatorText.ToString());
                Assert.AreEqual(byteCount, handleText.Length);
                Assert.AreEqual(text, handleText.ToString());
            }
            finally
            {
                if (allocatorText.IsCreated)
                {
                    allocatorText.Dispose();
                }

                if (handleText.IsCreated)
                {
                    handleText.Dispose();
                }
            }
        }

        [Test]
        public void CopyFromTruncated_Utf8SelfCopy_KeepsTheContent()
        {
            FixedString64Bytes value = "abcdef";

            var result = EncosyFixedStringExtensions.CopyFromTruncated(
                  ref value
                , EncosyFixedStringExtensions.AsReadOnlySpan(ref value)
            );

            Assert.AreEqual(CopyError.None, result);
            Assert.AreEqual("abcdef", value.ToString());
        }

        [Test]
        public void CopyFromTruncated_Utf8OverlappingSourceShiftedLeft_MatchesArrayCopy()
        {
            FixedString64Bytes value = "abcdef";
            var expected = Encoding.UTF8.GetBytes("abcdef");
            Array.Copy(expected, 1, expected, 0, 5);
            var expectedText = Encoding.UTF8.GetString(expected, 0, 5);

            var result = EncosyFixedStringExtensions.CopyFromTruncated(
                  ref value
                , EncosyFixedStringExtensions.AsReadOnlySpan(ref value)[1..]
            );

            Assert.AreEqual(CopyError.None, result);
            Assert.AreEqual(expectedText, value.ToString());
        }

        [TestCase("abc")]
        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("\u2713\u2713")]
        [TestCase("A\U0001F600B")]
        public void CopyFromTruncated_EverySizeAndText_RoundTripsBitExact(string text)
        {
            AssertCopyFromTruncatedRoundTrips<FixedString32Bytes>(text);
            AssertCopyFromTruncatedRoundTrips<FixedString64Bytes>(text);
            AssertCopyFromTruncatedRoundTrips<FixedString128Bytes>(text);
            AssertCopyFromTruncatedRoundTrips<FixedString512Bytes>(text);
            AssertCopyFromTruncatedRoundTrips<FixedString4096Bytes>(text);
        }

        [Test]
        public void Append_LongChars_ReturnsOverflowWithoutChangingString()
        {
            var longChars = new string('x', 10_000);
            FixedString32Bytes small = "abc";
            FixedString4096Bytes large = "abc";

            var smallResult = EncosyFixedStringExtensions.Append(ref small, longChars.AsSpan());
            var largeResult = EncosyFixedStringExtensions.Append(ref large, longChars.AsSpan());

            Assert.AreEqual(FormatError.Overflow, smallResult);
            Assert.AreEqual("abc", small.ToString());
            Assert.AreEqual(FormatError.Overflow, largeResult);
            Assert.AreEqual("abc", large.ToString());
        }

        [Test]
        public void Append_EmptyChars_LeavesTheStringUnchanged()
        {
            FixedString32Bytes value = "abc";

            var result = EncosyFixedStringExtensions.Append(ref value, ReadOnlySpan<char>.Empty);

            Assert.AreEqual(FormatError.None, result);
            Assert.AreEqual("abc", value.ToString());
        }

        [Test]
        public void Append_NonAsciiChars_AppendsTheUtf8Bytes()
        {
            const string SUFFIX = "\u00fc\u2713\U0001F600";

            FixedString128Bytes value = "a";

            var result = EncosyFixedStringExtensions.Append(ref value, SUFFIX.AsSpan());

            Assert.AreEqual(FormatError.None, result);
            CollectionAssert.AreEqual(
                  Encoding.UTF8.GetBytes("a" + SUFFIX)
                , EncosyFixedStringExtensions.AsReadOnlySpan(ref value).ToArray()
            );
        }

        [Test]
        public void Append_CharsFillingTheExactCapacity_SucceedsAndTheNextCharOverflows()
        {
            var fill = new string('\u2713', 9) + "ab";

            FixedString32Bytes value = default;

            var exact = EncosyFixedStringExtensions.Append(ref value, fill.AsSpan());

            Assert.AreEqual(FormatError.None, exact);
            Assert.AreEqual(value.Capacity, value.Length);
            Assert.AreEqual(fill, value.ToString());

            var overflow = EncosyFixedStringExtensions.Append(ref value, "z".AsSpan());

            Assert.AreEqual(FormatError.Overflow, overflow);
            Assert.AreEqual(fill, value.ToString());
        }

        [Test]
        public void CopyTo_EmptyString_WritesNothingAndReportsZeroLength()
        {
            var value = default(FixedString32Bytes);
            var destination = new[] { 'x', 'y' };

            var result = EncosyFixedStringExtensions.CopyTo(value, destination, out var length);

            Assert.AreEqual(CopyError.None, result);
            Assert.AreEqual(0, length);
            CollectionAssert.AreEqual(new[] { 'x', 'y' }, destination);
        }

        [Test]
        public void CopyTo_EmptyStringAndEmptyDestination_Succeeds()
        {
            var value = default(FixedString32Bytes);

            var result = EncosyFixedStringExtensions.CopyTo(value, Span<char>.Empty, out var length);

            Assert.AreEqual(CopyError.None, result);
            Assert.AreEqual(0, length);
        }

        [Test]
        public void CopyTo_NonEmptyStringAndEmptyDestination_ReturnsTruncation()
        {
            FixedString32Bytes value = "abc";

            var result = EncosyFixedStringExtensions.CopyTo(value, Span<char>.Empty, out _);

            Assert.AreEqual(CopyError.Truncation, result);
        }

        [TestCase("abc")]
        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("\u2713\u2713")]
        [TestCase("A\U0001F600B")]
        public void CopyTo_NonAsciiText_EqualsTheManagedChars(string text)
        {
            var value = (FixedString128Bytes)text;
            var destination = new char[text.Length];

            var result = EncosyFixedStringExtensions.CopyTo(value, destination, out var length);

            Assert.AreEqual(CopyError.None, result);
            Assert.AreEqual(text.Length, length);
            CollectionAssert.AreEqual(text.ToCharArray(), destination);
        }

        [TestCase("abc")]
        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("A\U0001F600B")]
        public void CopyTo_DestinationOneCharShort_ReturnsTruncation(string text)
        {
            var value = (FixedString128Bytes)text;
            var destination = new char[text.Length - 1];

            var result = EncosyFixedStringExtensions.CopyTo(value, destination, out _);

            Assert.AreEqual(CopyError.Truncation, result);
        }

        [Test]
        public void CopyTo_StringFillingTheExactCapacity_CopiesEveryChar()
        {
            FixedString32Bytes value = default;
            var text = new string('a', value.Capacity);
            value = text;
            var destination = new char[text.Length];

            var result = EncosyFixedStringExtensions.CopyTo(value, destination, out var length);

            Assert.AreEqual(CopyError.None, result);
            Assert.AreEqual(text.Length, length);
            Assert.AreEqual(text, new string(destination));
        }

        [Test]
        public void AppendTo_EmptyString_AppendsNothing()
        {
            var value = default(FixedString32Bytes);
            var builder = new StringBuilder("start");

            EncosyFixedStringExtensions.AppendTo(value, builder, out var length);

            Assert.AreEqual(0, length);
            Assert.AreEqual("start", builder.ToString());
        }

        [TestCase("abc")]
        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("\u2713\u2713")]
        [TestCase("A\U0001F600B")]
        public void AppendTo_NonAsciiText_AppendsTheManagedText(string text)
        {
            var value = (FixedString128Bytes)text;
            var builder = new StringBuilder("start:");

            EncosyFixedStringExtensions.AppendTo(value, builder, out var length);

            Assert.AreEqual(text.Length, length);
            Assert.AreEqual("start:" + text, builder.ToString());
        }

        [Test]
        public void AppendTo_StringFillingTheExactCapacity_AppendsEveryChar()
        {
            var text = new string('\u2713', 9) + "ab";
            var value = (FixedString32Bytes)text;
            var builder = new StringBuilder();

            EncosyFixedStringExtensions.AppendTo(value, builder, out var length);

            Assert.AreEqual(value.Capacity, value.Length);
            Assert.AreEqual(text.Length, length);
            Assert.AreEqual(text, builder.ToString());
        }

        [Test]
        public void AsSpan_EmptyString_IsEmpty()
        {
            var value = default(FixedString32Bytes);

            Assert.IsTrue(EncosyFixedStringExtensions.AsSpan(ref value).IsEmpty);
            Assert.IsTrue(EncosyFixedStringExtensions.AsReadOnlySpan(ref value).IsEmpty);
        }

        [Test]
        public void AsSpan_ViewsTheVariable_SeesLaterAssignments()
        {
            FixedString512Bytes value = "abc";
            var view = EncosyFixedStringExtensions.AsSpan(ref value);

            value = "xyz";
            StackOverwriter.Fill(value: 0xCD);

            Assert.AreEqual(3, view.Length);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("xyz"), view.ToArray());
        }

        [Test]
        public void AsSpan_WriteMultiByteBytes_ChangesTheString()
        {
            FixedString64Bytes value = "ab";
            var bytes = Encoding.UTF8.GetBytes("\u00fc");

            var span = EncosyFixedStringExtensions.AsSpan(ref value);
            bytes.AsSpan().CopyTo(span);

            Assert.AreEqual("\u00fc", value.ToString());
        }

        [TestCase("abc")]
        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("\u2713\u2713")]
        [TestCase("A\U0001F600B")]
        public void AsSpan_EverySize_ViewsTheStringBytes(string text)
        {
            AssertSpansEqualUtf8Bytes<FixedString32Bytes>(text);
            AssertSpansEqualUtf8Bytes<FixedString64Bytes>(text);
            AssertSpansEqualUtf8Bytes<FixedString128Bytes>(text);
            AssertSpansEqualUtf8Bytes<FixedString512Bytes>(text);
            AssertSpansEqualUtf8Bytes<FixedString4096Bytes>(text);
        }

        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("A\U0001F600B")]
        public void AsReadOnlySpan_NonAsciiText_EqualsUtf8BytesAfterStackHeavyCalls(string text)
        {
            var value = (FixedString128Bytes)text;
            var view = EncosyFixedStringExtensions.AsReadOnlySpan(ref value);

            StackOverwriter.Fill(value: 0xCD);

            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), view.ToArray());
        }

        [Test]
        public void ToNativeText_EmptyFixedString_CreatesEmptyText()
        {
            var value = default(FixedString32Bytes);
            NativeText allocatorText = default;
            NativeText handleText = default;

            try
            {
                allocatorText = EncosyFixedStringExtensions.ToNativeText(value, Allocator.Persistent);
                handleText = EncosyFixedStringExtensions.ToNativeText(value, AllocatorManager.Persistent);

                Assert.IsTrue(allocatorText.IsCreated);
                Assert.AreEqual(0, allocatorText.Length);
                Assert.IsTrue(handleText.IsCreated);
                Assert.AreEqual(0, handleText.Length);
            }
            finally
            {
                if (allocatorText.IsCreated)
                {
                    allocatorText.Dispose();
                }

                if (handleText.IsCreated)
                {
                    handleText.Dispose();
                }
            }
        }

        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("\u2713\u2713")]
        [TestCase("A\U0001F600B")]
        public void ToNativeText_NonAsciiText_RoundTripsTheUtf8Bytes(string text)
        {
            var value = (FixedString128Bytes)text;
            NativeText allocatorText = default;
            NativeText handleText = default;

            try
            {
                allocatorText = EncosyFixedStringExtensions.ToNativeText(value, Allocator.Persistent);
                handleText = EncosyFixedStringExtensions.ToNativeText(value, AllocatorManager.Persistent);

                Assert.AreEqual(value.Length, allocatorText.Length);
                Assert.AreEqual(text, allocatorText.ToString());
                Assert.AreEqual(value.Length, handleText.Length);
                Assert.AreEqual(text, handleText.ToString());
            }
            finally
            {
                if (allocatorText.IsCreated)
                {
                    allocatorText.Dispose();
                }

                if (handleText.IsCreated)
                {
                    handleText.Dispose();
                }
            }
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void ToNativeText_AcceptedAllocator_CreatesAnIndependentCopy(Allocator allocator)
        {
            FixedString64Bytes value = "abc";
            var text = EncosyFixedStringExtensions.ToNativeText(value, allocator);

            try
            {
                value = "xyz";

                Assert.AreEqual("abc", text.ToString());
            }
            finally
            {
                text.Dispose();
            }
        }

        [Test]
        public void ToNativeText_TempAndTempJobHandles_CreateIndependentCopies()
        {
            FixedString64Bytes value = "abc";
            var temp = EncosyFixedStringExtensions.ToNativeText(value, AllocatorManager.Temp);
            var tempJob = EncosyFixedStringExtensions.ToNativeText(value, AllocatorManager.TempJob);

            try
            {
                value = "xyz";

                Assert.AreEqual("abc", temp.ToString());
                Assert.AreEqual("abc", tempJob.ToString());
            }
            finally
            {
                tempJob.Dispose();
                temp.Dispose();
            }
        }

        private static void AssertCopyFromTruncatedRoundTrips<TFixedString>(string text)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            ReadOnlySpan<byte> utf8 = bytes;
            var fromBytes = default(TFixedString);
            var fromChars = default(TFixedString);

            var bytesResult = EncosyFixedStringExtensions.CopyFromTruncated(ref fromBytes, utf8);
            var charsResult = EncosyFixedStringExtensions.CopyFromTruncated(ref fromChars, text.AsSpan());

            Assert.AreEqual(CopyError.None, bytesResult);
            Assert.AreEqual(CopyError.None, charsResult);

            CollectionAssert.AreEqual(bytes, EncosyFixedStringExtensions.AsReadOnlySpan(ref fromBytes).ToArray());
            CollectionAssert.AreEqual(bytes, EncosyFixedStringExtensions.AsReadOnlySpan(ref fromChars).ToArray());
        }

        private static void AssertSpansEqualUtf8Bytes<TFixedString>(string text)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var value = default(TFixedString);

            Assert.AreEqual(CopyError.None, EncosyFixedStringExtensions.CopyFrom(ref value, text.AsSpan()));

            CollectionAssert.AreEqual(bytes, EncosyFixedStringExtensions.AsSpan(ref value).ToArray());
            CollectionAssert.AreEqual(bytes, EncosyFixedStringExtensions.AsReadOnlySpan(ref value).ToArray());
        }
    }
}

#endif
