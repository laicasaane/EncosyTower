using System;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.StringIds;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Testing
{
    public static class UnmanagedStringVaultCopyAssert
    {
        public static List<UnmanagedString> Collect(IEnumerable<UnmanagedString> source)
        {
            var result = new List<UnmanagedString>();

            foreach (var item in source)
            {
                result.Add(item);
            }

            return result;
        }

        public static void CopyToDefaultLength(
              ICopyToSpan<UnmanagedString> vault
            , IReadOnlyList<UnmanagedString> expected
        )
        {
            var destination = new UnmanagedString[expected.Count];

            vault.CopyTo(destination);

            AssertEqualFrom(expected, 0, destination);
        }

        public static void TryCopyToDefaultLength(
              ITryCopyToSpan<UnmanagedString> vault
            , IReadOnlyList<UnmanagedString> expected
        )
        {
            var destination = new UnmanagedString[expected.Count];

            Assert.IsTrue(vault.TryCopyTo(destination));

            AssertEqualFrom(expected, 0, destination);
        }

        public static void CopyToSourceStartAndLength(
              ICopyToSpan<UnmanagedString> vault
            , IReadOnlyList<UnmanagedString> expected
        )
        {
            var destination = new UnmanagedString[2];

            vault.CopyTo(1, destination, 2);

            AssertEqualFrom(expected, 1, destination);
        }

        public static void TryCopyToSourceStartAndLength(
              ITryCopyToSpan<UnmanagedString> vault
            , IReadOnlyList<UnmanagedString> expected
        )
        {
            var destination = new UnmanagedString[2];

            Assert.IsTrue(vault.TryCopyTo(1, destination, 2));

            AssertEqualFrom(expected, 1, destination);
        }

        public static void TryCopyToStartPastDestinationLength(
              ITryCopyToSpan<UnmanagedString> vault
            , IReadOnlyList<UnmanagedString> expected
        )
        {
            Assert.GreaterOrEqual(expected.Count, 8);

            var destination = new UnmanagedString[2];

            Assert.IsTrue(vault.TryCopyTo(5, destination, 2));

            AssertEqualFrom(expected, 5, destination);
        }

        public static void TryCopyToLengthPastDestination(ITryCopyToSpan<UnmanagedString> vault)
        {
            var destination = new UnmanagedString[2];
            var copied = true;

            Assert.DoesNotThrow(() => copied = vault.TryCopyTo(0, destination, destination.Length + 1));

            Assert.IsFalse(copied);
        }

        public static void TryCopyToInvalidRange(ITryCopyToSpan<UnmanagedString> vault, int count)
        {
            var destination = new UnmanagedString[count + 2];

            Assert.IsFalse(TryCopyWithoutThrow(vault, -1, destination, 1), "negative start");
            Assert.IsFalse(TryCopyWithoutThrow(vault, count, destination, 1), "start at count");
            Assert.IsFalse(TryCopyWithoutThrow(vault, 0, destination, count + 1), "length past count");
            Assert.IsFalse(TryCopyWithoutThrow(vault, 1, destination, -1), "negative length");
        }

        public static void CopyToInvalidRangeThrows(ICopyToSpan<UnmanagedString> vault, int count)
        {
            var destination = new UnmanagedString[count + 2];

            Assert.Throws<ArgumentOutOfRangeException>(() => vault.CopyTo(-1, destination, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => vault.CopyTo(count, destination, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => vault.CopyTo(0, destination, count + 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => vault.CopyTo(1, destination, -1));
        }

        public static void CopyToLengthPastDestinationThrows(ICopyToSpan<UnmanagedString> vault)
        {
            var destination = new UnmanagedString[1];

            Assert.Throws<ArgumentOutOfRangeException>(() => vault.CopyTo(0, destination, 2));
        }

        public static void CopyToEmptyVault(ICopyToSpan<UnmanagedString> vault)
        {
            var destination = new UnmanagedString[1];
            destination[0] = "marker";

            Assert.DoesNotThrow(() => vault.CopyTo(destination));

            Assert.IsTrue(destination[0].IsEmpty);
        }

        public static void TryCopyToEmptyVault(ITryCopyToSpan<UnmanagedString> vault)
        {
            var destination = new UnmanagedString[1];
            destination[0] = "marker";
            var copied = false;

            Assert.DoesNotThrow(() => copied = vault.TryCopyTo(destination));

            Assert.IsTrue(copied);
            Assert.IsTrue(destination[0].IsEmpty);
        }

        private static bool TryCopyWithoutThrow(
              ITryCopyToSpan<UnmanagedString> vault
            , int sourceStartIndex
            , UnmanagedString[] destination
            , int length
        )
        {
            var copied = true;

            Assert.DoesNotThrow(
                () => copied = vault.TryCopyTo(sourceStartIndex, destination, length)
                , $"start {sourceStartIndex}, length {length}"
            );

            return copied;
        }

        private static void AssertEqualFrom(
              IReadOnlyList<UnmanagedString> expected
            , int expectedStartIndex
            , UnmanagedString[] actual
        )
        {
            var length = actual.Length;

            for (var i = 0; i < length; i++)
            {
                Assert.AreEqual(expected[expectedStartIndex + i], actual[i], $"index {expectedStartIndex + i}");
            }
        }
    }
}
