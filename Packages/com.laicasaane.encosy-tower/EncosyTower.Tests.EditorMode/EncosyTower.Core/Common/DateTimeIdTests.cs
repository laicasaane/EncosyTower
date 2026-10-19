using System;
using EncosyTower.Common;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Common
{
    public class DateTimeIdTests
    {
        [Test]
        public void Constructor_DateOnly_LeavesTimeAndZoneZero()
        {
            var id = new DateTimeId(year: 2024, month: 5, day: 17);

            Assert.AreEqual(2024, id.Year);
            Assert.AreEqual(5, id.Month);
            Assert.AreEqual(17, id.Day);
            Assert.AreEqual(0, id.Hour);
            Assert.AreEqual(0, id.Minute);
            Assert.AreEqual(0, id.Second);
            Assert.AreEqual(0, id.Zone);
        }

        [Test]
        public void Constructor_DateAndTime_StoresEveryField()
        {
            var id = Create(year: 2024, month: 5, day: 17, hour: 13, minute: 45, second: 59);

            id.Deconstruct(
                  out var year
                , out var month
                , out var day
                , out var hour
                , out var minute
                , out var second
            );

            Assert.AreEqual(2024, year);
            Assert.AreEqual(5, month);
            Assert.AreEqual(17, day);
            Assert.AreEqual(13, hour);
            Assert.AreEqual(45, minute);
            Assert.AreEqual(59, second);
        }

        [Test]
        public void Constructor_MaximumFieldValues_RoundTripsEveryField()
        {
            const byte MAX = byte.MaxValue;
            var id = new DateTimeId(year: ushort.MaxValue, month: MAX, day: MAX, hour: MAX, minute: MAX, second: MAX);

            Assert.AreEqual(ushort.MaxValue, id.Year);
            Assert.AreEqual(byte.MaxValue, id.Month);
            Assert.AreEqual(byte.MaxValue, id.Day);
            Assert.AreEqual(byte.MaxValue, id.Hour);
            Assert.AreEqual(byte.MaxValue, id.Minute);
            Assert.AreEqual(byte.MaxValue, id.Second);
            Assert.AreEqual(0, id.Zone);
        }

        [Test]
        public void Constructor_DefaultStruct_IsNotValidAndAllFieldsZero()
        {
            var id = default(DateTimeId);

            Assert.IsFalse(id.IsValid);
            Assert.AreEqual(0, id.Year);
            Assert.AreEqual(0, id.Month);
            Assert.AreEqual(0, id.Day);
            Assert.AreEqual(0UL, (ulong)id);
        }

        [Test]
        public void Constructor_FromDateTime_RoundTripsThroughImplicitConversion()
        {
            var source = new DateTime(2024, 2, 29, 23, 59, 58, DateTimeKind.Utc);

            DateTimeId id = source;
            DateTime back = id;

            Assert.AreEqual(source, back);
            Assert.IsTrue(id.IsValid);
        }

        [Test]
        public void Constructor_FromDateTime_DropsFractionOfSecond()
        {
            var source = new DateTime(2024, 2, 29, 23, 59, 58, 999, DateTimeKind.Utc);

            DateTimeId id = source;
            DateTime back = id;

            Assert.AreEqual(source.AddMilliseconds(-999), back);
        }

        [Test]
        public void Constructor_AnyField_OverlaysTheRawValueBytes()
        {
            var id = Create(year: 0x0A0B, month: 0x0C, day: 0x0D, hour: 0x0E, minute: 0x0F, second: 0x10);

            var expected = 0UL;
            expected |= 0x10UL << 8;
            expected |= 0x0FUL << 16;
            expected |= 0x0EUL << 24;
            expected |= 0x0DUL << 32;
            expected |= 0x0CUL << 40;
            expected |= 0x0A0BUL << 48;

            Assert.AreEqual(expected, (ulong)id);
        }

        [Test]
        public void Constructor_RawValueConversion_ReadsBackEveryOverlaidField()
        {
            var raw = 0x0A0B0C0D0E0F10F1UL;

            var id = (DateTimeId)raw;

            Assert.AreEqual(0x0A0B, id.Year);
            Assert.AreEqual(0x0C, id.Month);
            Assert.AreEqual(0x0D, id.Day);
            Assert.AreEqual(0x0E, id.Hour);
            Assert.AreEqual(0x0F, id.Minute);
            Assert.AreEqual(0x10, id.Second);
            Assert.AreEqual(unchecked((sbyte)0xF1), id.Zone);
            Assert.AreEqual(raw, (ulong)id);
        }

        [Test]
        public void Constructor_ZoneInitializer_DoesNotDisturbTheNeighbouringFields()
        {
            var id = Create(year: 2000, month: 1, day: 2, hour: 3, minute: 4, second: 5) with { Zone = -1 };

            Assert.AreEqual(-1, id.Zone);
            Assert.AreEqual(2000, id.Year);
            Assert.AreEqual(1, id.Month);
            Assert.AreEqual(2, id.Day);
            Assert.AreEqual(3, id.Hour);
            Assert.AreEqual(4, id.Minute);
            Assert.AreEqual(5, id.Second);
            Assert.AreEqual(0xFFUL, (ulong)id & 0xFFUL);
        }

        [Test]
        public void Constructor_SecondInitializer_DoesNotDisturbTheZone()
        {
            var id = Create(year: 2000, month: 1, day: 2, hour: 3, minute: 4, second: 5) with { Zone = 7, Second = 40 };

            Assert.AreEqual(7, id.Zone);
            Assert.AreEqual(40, id.Second);
            Assert.AreEqual(4, id.Minute);
        }

        [TestCase(2024, 12, 31, 23, 59, 59, 2025, 1, 1, 0, 0, 0)]
        [TestCase(2024, 1, 2, 0, 0, 0, 2024, 1, 1, 23, 59, 59)]
        [TestCase(2024, 6, 15, 12, 31, 0, 2024, 6, 15, 12, 30, 59)]
        [TestCase(2024, 6, 15, 12, 30, 1, 2024, 6, 15, 12, 30, 0)]
        [TestCase(2001, 1, 1, 0, 0, 0, 2001, 1, 1, 0, 0, 0)]
        public void Constructor_RawOrdering_MatchesDateTimeOrdering(
              int y1, int mo1, int d1, int h1, int mi1, int s1
            , int y2, int mo2, int d2, int h2, int mi2, int s2
        )
        {
            var first = Create(y1, mo1, d1, h1, mi1, s1);
            var second = Create(y2, mo2, d2, h2, mi2, s2);

            var expected = new DateTime(y1, mo1, d1, h1, mi1, s1).CompareTo(new DateTime(y2, mo2, d2, h2, mi2, s2));

            Assert.AreEqual(Math.Sign(expected), Math.Sign(first.CompareTo(second)));
            Assert.AreEqual(expected == 0, first == second);
            Assert.AreEqual(expected < 0, first < second);
            Assert.AreEqual(expected > 0, first > second);
        }

        [Test]
        public void TryParse_EmptySpan_ReturnsFalseWithUnixEpoch()
        {
            var parser = default(DateTimeId);

            var parsed = parser.TryParse(ReadOnlySpan<char>.Empty, out var result);

            Assert.IsFalse(parsed);
            Assert.AreEqual(DateTimeId.UnixEpoch, result);
        }

        [Test]
        public void TryParse_NullString_ReturnsFalseWithUnixEpoch()
        {
            var parser = default(DateTimeId);

            var parsed = parser.TryParse((string)null, out var result);

            Assert.IsFalse(parsed);
            Assert.AreEqual(DateTimeId.UnixEpoch, result);
        }

        [Test]
        public void TryParse_ValidText_ReturnsEveryField()
        {
            var parser = default(DateTimeId);

            var parsed = parser.TryParse("2024_05_17T13:45:59".AsSpan(), out var result);

            Assert.IsTrue(parsed);
            Assert.AreEqual(Create(year: 2024, month: 5, day: 17, hour: 13, minute: 45, second: 59), result);
        }

        [Test]
        public void TryParse_LowerCaseSeparatorWithIgnoreCase_ReturnsTrue()
        {
            var parser = default(DateTimeId);

            var parsed = parser.TryParse("2024_05_17t13:45:59".AsSpan(), out var result, ignoreCase: true);

            Assert.IsTrue(parsed);
            Assert.AreEqual(Create(year: 2024, month: 5, day: 17, hour: 13, minute: 45, second: 59), result);
        }

        [Test]
        public void TryParse_LowerCaseSeparatorWithoutIgnoreCase_ReturnsFalse()
        {
            var parser = default(DateTimeId);

            var parsed = parser.TryParse("2024_05_17t13:45:59".AsSpan(), out var result, ignoreCase: false);

            Assert.IsFalse(parsed);
            Assert.AreEqual(DateTimeId.UnixEpoch, result);
        }

        [TestCase("2024_05_17")]
        [TestCase("2024_05_17T")]
        [TestCase("T13:45:59")]
        [TestCase("2024_05T13:45:59")]
        [TestCase("2024_05_17_01T13:45:59")]
        [TestCase("2024_05_17T13:45")]
        [TestCase("2024_05_17T13:45:59:01")]
        [TestCase("2024_05_17T13:45:59T01")]
        [TestCase("2024_05_17T13:45:256")]
        [TestCase("2024_05_17T13:45:-1")]
        [TestCase("65536_05_17T13:45:59")]
        [TestCase("abcd_05_17T13:45:59")]
        public void TryParse_MalformedText_ReturnsFalseWithUnixEpoch(string text)
        {
            var parser = default(DateTimeId);

            var parsed = parser.TryParse(text.AsSpan(), out var result);

            Assert.IsFalse(parsed);
            Assert.AreEqual(DateTimeId.UnixEpoch, result);
        }

        [Test]
        public void TryParse_NonPaddedFields_ReturnsTrue()
        {
            var parser = default(DateTimeId);

            var parsed = parser.TryParse("2024_5_7T3:4:5".AsSpan(), out var result);

            Assert.IsTrue(parsed);
            Assert.AreEqual(Create(year: 2024, month: 5, day: 7, hour: 3, minute: 4, second: 5), result);
        }

        [Test]
        public void TryParse_MaximumFieldValues_ReturnsTrue()
        {
            var parser = default(DateTimeId);

            var parsed = parser.TryParse("65535_255_255T255:255:255".AsSpan(), out var result);

            Assert.IsTrue(parsed);
            Assert.AreEqual(ushort.MaxValue, result.Year);
            Assert.AreEqual(byte.MaxValue, result.Second);
        }

        private static DateTimeId Create(int year, int month, int day, int hour, int minute, int second)
        {
            return new DateTimeId(
                  year: (ushort)year
                , month: (byte)month
                , day: (byte)day
                , hour: (byte)hour
                , minute: (byte)minute
                , second: (byte)second
            );
        }
    }
}
