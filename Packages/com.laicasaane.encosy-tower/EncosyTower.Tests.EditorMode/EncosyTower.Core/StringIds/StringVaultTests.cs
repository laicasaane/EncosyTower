using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EncosyTower.Ids;
using EncosyTower.StringIds;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.StringIds;

public class StringVaultTests
{
    private const int CAPACITY = 4;
    private const int CAPACITY_LARGE = 16;
    private const string TABLE_NAME = "level_table_asset";
    private const string UNICODE_NAME = "\u00dcn\u00efc\u00f6d\u00e9 \u2713";

    [Test]
    public void Clear_ThenIntern_OldIdIsNotDefined()
    {
        using var vault = new StringVault(CAPACITY);

        vault.GetOrMakeId("a");
        var oldId = vault.GetOrMakeId("b");

        vault.Clear();
        vault.GetOrMakeId("fresh");

        Assert.IsFalse(vault.ContainsId(oldId));
        Assert.IsFalse(vault.TryGetManagedString(oldId, out _));
    }

    [Test]
    public void Enumerators_StopAtCount()
    {
        using var vault = new StringVault(CAPACITY);

        vault.GetOrMakeId("a");
        vault.GetOrMakeId("b");

        var managedCount = 0;

        foreach (var str in vault)
        {
            Assert.IsNotNull(str);
            managedCount++;
        }

        var unmanagedCount = 0;

        foreach (var _ in vault.AsReadOnly())
        {
            unmanagedCount++;
        }

        Assert.AreEqual(vault.Count, managedCount);
        Assert.AreEqual(vault.Count, unmanagedCount);
    }

    [Test]
    public void Intern_SameStringTwice_ReturnsSameId()
    {
        using var vault = new StringVault(CAPACITY);
        var countBefore = vault.Count;

        var first = vault.GetOrMakeId("same");
        var second = vault.GetOrMakeId("same");

        Assert.AreEqual(first, second);
        Assert.AreEqual(countBefore + 1, vault.Count);
    }

    [Test]
    public void GetOrMakeId_SameTextFromEverySource_ReturnsOneId()
    {
        var builder = new StringBuilder("level_");
        builder.Append("table_asset");
        var runtimeText = builder.ToString();

        using var vault = new StringVault(CAPACITY);
        var countBefore = vault.Count;

        var fromLiteral = vault.GetOrMakeId(TABLE_NAME);
        StackOverwriter.Fill(value: 0xCD);

        var fromRuntime = vault.GetOrMakeId(runtimeText);
        StackOverwriter.Fill(value: 0x00);

        var fromUnmanaged = vault.GetOrMakeId((UnmanagedString)TABLE_NAME);
        StackOverwriter.Fill(value: 0xCD);

        UnmanagedString fixedSource = (FixedString512Bytes)TABLE_NAME;
        var fromFixed = vault.GetOrMakeId(fixedSource);
        StackOverwriter.Fill(value: 0x00);

        Assert.AreEqual(fromLiteral, fromRuntime);
        Assert.AreEqual(fromLiteral, fromUnmanaged);
        Assert.AreEqual(fromLiteral, fromFixed);
        Assert.AreEqual(countBefore + 1, vault.Count);

        Assert.IsTrue(vault.TryGetId(TABLE_NAME, out var foundManaged));
        Assert.AreEqual(fromLiteral, foundManaged);

        Assert.IsTrue(vault.TryGetId((UnmanagedString)TABLE_NAME, out var foundUnmanaged));
        Assert.AreEqual(fromLiteral, foundUnmanaged);

        Assert.IsTrue(vault.TryGetUnmanagedString(fromLiteral, out var storedUnmanaged));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(TABLE_NAME), storedUnmanaged.AsReadOnlySpan().ToArray());

        Assert.IsTrue(vault.TryGetManagedString(fromLiteral, out var storedManaged));
        Assert.AreEqual(TABLE_NAME, storedManaged);
    }

    [Test]
    public void GetOrMakeId_TableNames_RoundTripEveryName()
    {
        var names = new[]
        {
            "hero_table_asset",
            "enemy_table_asset",
            "string_table_asset",
            TABLE_NAME,
            UNICODE_NAME,
        };

        var count = names.Length;
        var ids = new StringId[count];

        using var vault = new StringVault(CAPACITY);

        for (var i = 0; i < count; i++)
        {
            ids[i] = vault.GetOrMakeId(names[i]);
            StackOverwriter.Fill(value: 0xCD);
        }

        for (var i = 0; i < count; i++)
        {
            for (var j = i + 1; j < count; j++)
            {
                Assert.AreNotEqual(ids[i], ids[j], $"{names[i]} / {names[j]}");
            }
        }

        for (var i = 0; i < count; i++)
        {
            var name = names[i];
            var unmanagedName = (UnmanagedString)name;

            Assert.IsTrue(vault.TryGetId(unmanagedName, out var found), name);
            Assert.AreEqual(ids[i], found, name);

            StackOverwriter.Fill(value: 0x00);

            Assert.AreEqual(ids[i], vault.GetOrMakeId(unmanagedName), name);

            Assert.IsTrue(vault.TryGetUnmanagedString(ids[i], out var storedUnmanaged), name);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(name), storedUnmanaged.AsReadOnlySpan().ToArray(), name);

            Assert.IsTrue(vault.TryGetManagedString(ids[i], out var storedManaged), name);
            Assert.AreEqual(name, storedManaged, name);
        }
    }

    [Test]
    public void TryGetUnmanagedString_NeverRegisteredId_ReturnsFalse()
    {
        using var vault = new StringVault(CAPACITY_LARGE);
        vault.GetOrMakeId("a");
        vault.GetOrMakeId("b");

        var neverRegistered = new StringId(new Id(vault.Count + 3));

        Assert.IsFalse(vault.ContainsId(neverRegistered));
        Assert.IsFalse(vault.TryGetUnmanagedString(neverRegistered, out _));
        Assert.IsFalse(vault.TryGetUnmanagedString(neverRegistered).HasValue);
    }

    [Test]
    public void TryGetUnmanagedString_DefaultId_ReturnsFalse()
    {
        using var vault = new StringVault(CAPACITY_LARGE);
        vault.GetOrMakeId("a");

        Assert.IsFalse(vault.ContainsId(default));
        Assert.IsFalse(vault.TryGetUnmanagedString(id: default, result: out var result));
        Assert.IsTrue(result.IsEmpty);
    }

    [Test]
    public void TryGetUnmanagedString_IdBeyondCapacity_ReturnsFalseWithEmptyString()
    {
        using var vault = new StringVault(CAPACITY);
        vault.GetOrMakeId("a");

        var beyondCapacity = new StringId(new Id(uint.MaxValue));

        Assert.IsFalse(vault.TryGetUnmanagedString(beyondCapacity, out var result));
        Assert.IsTrue(result.IsEmpty);
        Assert.IsFalse(vault.TryGetManagedString(beyondCapacity, out _));
    }

    [TestCase("A\U0001F600B")]
    [TestCase("\u00fc\u00ef\u00f6")]
    [TestCase("\u2713\u2713\u2713")]
    public void TryGetUnmanagedString_NonAsciiText_RoundTripsUtf8Bytes(string text)
    {
        using var vault = new StringVault(CAPACITY);
        var id = vault.GetOrMakeId(text);

        StackOverwriter.Fill(value: 0xCD);

        Assert.IsTrue(vault.TryGetUnmanagedString(id, out var stored));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), stored.AsReadOnlySpan().ToArray());
        Assert.IsTrue(vault.TryGetManagedString(id, out var storedManaged));
        Assert.AreEqual(text, storedManaged);
    }

    [Test]
    public void TryGetUnmanagedString_ExactCapacityString_RoundTripsEveryByte()
    {
        var text = new string('a', UnmanagedString.UTF8MaxLengthInBytes);

        using var vault = new StringVault(CAPACITY);
        var id = vault.GetOrMakeId(text);

        Assert.IsTrue(vault.TryGetUnmanagedString(id, out var stored));
        Assert.AreEqual(UnmanagedString.UTF8MaxLengthInBytes, stored.Length);
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), stored.AsReadOnlySpan().ToArray());
    }

    [Test]
    public void GetOrMakeId_StringLongerThanCapacity_CapsAtMaximumLengthWithoutThrowing()
    {
        var text = new string('x', UnmanagedString.UTF8MaxLengthInBytes + 100);

        using var vault = new StringVault(CAPACITY);
        var id = default(StringId);

        Assert.DoesNotThrow(() => id = vault.GetOrMakeId(text));

        Assert.IsTrue(vault.TryGetUnmanagedString(id, out var stored));
        Assert.AreEqual(UnmanagedString.UTF8MaxLengthInBytes, stored.Length);
        Assert.AreEqual(id, vault.GetOrMakeId(text));
    }

    [TestCase(508, "\u00e9", "")]
    [TestCase(507, "\u00e9", "\u00e9")]
    [TestCase(506, "\U0001F600", "")]
    public void GetOrMakeId_MultibyteCharacterAtCapacity_StoresLongestCompleteUtf8Prefix(
          int asciiLength
        , string suffix
        , string expectedSuffix
    )
    {
        var prefix = new string('a', asciiLength);
        var text = prefix + suffix;
        var expected = prefix + expectedSuffix;
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        using var vault = new StringVault(CAPACITY);

        var id = vault.GetOrMakeId(text);

        Assert.IsTrue(vault.TryGetManagedString(id, out var managed));
        Assert.AreEqual(expected, managed);
        Assert.IsTrue(vault.TryGetUnmanagedString(id, out var unmanaged));
        CollectionAssert.AreEqual(expectedBytes, unmanaged.AsReadOnlySpan().ToArray());
        Assert.AreEqual(expected, unmanaged.ToString());
        Assert.IsTrue(vault.TryGetId(text, out var found));
        Assert.AreEqual(id, found);
        Assert.AreEqual(id, vault.GetOrMakeId(expected));

        var readOnly = vault.AsReadOnly();

        Assert.IsTrue(readOnly.TryGetUnmanagedString(id, out var readOnlyString));
        CollectionAssert.AreEqual(expectedBytes, readOnlyString.AsReadOnlySpan().ToArray());
    }

    [TestCase(508, "\u00e9")]
    [TestCase(506, "\U0001F600")]
    public void GetOrMakeId_OverlongStringsWithSameCompletePrefix_ReuseCanonicalId(int asciiLength, string suffix)
    {
        var expected = new string('a', asciiLength);
        var firstText = expected + suffix + "first";
        var secondText = expected + suffix + "second";
        using var vault = new StringVault(CAPACITY);

        var first = vault.GetOrMakeId(firstText);
        var second = vault.GetOrMakeId(secondText);

        Assert.AreEqual(first, second);
        Assert.AreEqual(first, vault.GetOrMakeId(expected));
        Assert.AreEqual(2, vault.Count);
        Assert.IsTrue(vault.TryGetManagedString(first, out var managed));
        Assert.AreEqual(expected, managed);
        Assert.IsTrue(vault.TryGetUnmanagedString(first, out var unmanaged));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(expected), unmanaged.AsReadOnlySpan().ToArray());
        Assert.IsTrue(vault.TryGetId(secondText, out var found));
        Assert.AreEqual(first, found);
    }

    [Test]
    public void CopyTo_ManagedDefaultLength_CopiesEveryEntryIncludingTheInvalidSlot()
    {
        using var vault = CreateVault(count: 3);
        var destination = new string[vault.Count];

        vault.CopyTo(destination);

        CollectionAssert.AreEqual(new[] { string.Empty, "s0", "s1", "s2" }, destination);
    }

    [Test]
    public void CopyTo_ManagedSourceStartAndLength_CopiesTheMatchingSlice()
    {
        using var vault = CreateVault(count: 5);
        var destination = new string[2];

        vault.CopyTo(2, destination, 2);

        CollectionAssert.AreEqual(new[] { "s1", "s2" }, destination);
    }

    [Test]
    public void CopyTo_ManagedInvalidRange_ThrowsArgumentOutOfRange()
    {
        using var vault = CreateVault(count: 3);
        var destination = new string[8];

        Assert.Throws<ArgumentOutOfRangeException>(() => vault.CopyTo(-1, destination, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => vault.CopyTo(vault.Count, destination, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => vault.CopyTo(0, destination, vault.Count + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => vault.CopyTo(1, destination, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => vault.CopyTo(0, new string[1], 2));
    }

    [Test]
    public void TryCopyTo_ManagedStartPastDestinationLength_ReturnsTrue()
    {
        using var vault = CreateVault(count: 8);
        var destination = new string[2];

        Assert.IsTrue(vault.TryCopyTo(5, destination, 2));
        CollectionAssert.AreEqual(new[] { "s4", "s5" }, destination);
    }

    [Test]
    public void TryCopyTo_ManagedLengthPastDestination_ReturnsFalse()
    {
        using var vault = CreateVault(count: 8);
        var destination = new string[2];
        var copied = true;

        Assert.DoesNotThrow(() => copied = vault.TryCopyTo(0, destination, 3));

        Assert.IsFalse(copied);
    }

    [Test]
    public void TryCopyTo_ManagedInvalidRange_ReturnsFalse()
    {
        using var vault = CreateVault(count: 3);
        var destination = new string[8];
        var copied = new bool[4];

        Assert.DoesNotThrow(() => copied[0] = vault.TryCopyTo(-1, destination, 1));
        Assert.DoesNotThrow(() => copied[1] = vault.TryCopyTo(vault.Count, destination, 1));
        Assert.DoesNotThrow(() => copied[2] = vault.TryCopyTo(0, destination, vault.Count + 1));
        Assert.DoesNotThrow(() => copied[3] = vault.TryCopyTo(1, destination, -1));

        CollectionAssert.AreEqual(new[] { false, false, false, false }, copied);
    }

    [Test]
    public void CopyTo_DefaultLength_CopiesEveryRegisteredString()
    {
        using var vault = CreateVault(count: 8);
        var expected = UnmanagedStringVaultCopyAssert.Collect(vault.AsReadOnly());

        UnmanagedStringVaultCopyAssert.CopyToDefaultLength(vault, expected);
    }

    [Test]
    public void TryCopyTo_DefaultLength_ReturnsTrueAndCopiesEveryRegisteredString()
    {
        using var vault = CreateVault(count: 8);
        var expected = UnmanagedStringVaultCopyAssert.Collect(vault.AsReadOnly());

        UnmanagedStringVaultCopyAssert.TryCopyToDefaultLength(vault, expected);
    }

    [Test]
    public void CopyTo_SourceStartAndLength_CopiesTheMatchingSlice()
    {
        using var vault = CreateVault(count: 8);
        var expected = UnmanagedStringVaultCopyAssert.Collect(vault.AsReadOnly());

        UnmanagedStringVaultCopyAssert.CopyToSourceStartAndLength(vault, expected);
    }

    [Test]
    public void TryCopyTo_SourceStartAndLength_ReturnsTrueAndCopiesTheMatchingSlice()
    {
        using var vault = CreateVault(count: 8);
        var expected = UnmanagedStringVaultCopyAssert.Collect(vault.AsReadOnly());

        UnmanagedStringVaultCopyAssert.TryCopyToSourceStartAndLength(vault, expected);
    }

    [Test]
    public void TryCopyTo_StartPastDestinationLength_ReturnsTrue()
    {
        using var vault = CreateVault(count: 8);
        var expected = UnmanagedStringVaultCopyAssert.Collect(vault.AsReadOnly());

        UnmanagedStringVaultCopyAssert.TryCopyToStartPastDestinationLength(vault, expected);
    }

    [Test]
    public void TryCopyTo_LengthPastDestination_ReturnsFalse()
    {
        using var vault = CreateVault(count: 8);

        UnmanagedStringVaultCopyAssert.TryCopyToLengthPastDestination(vault);
    }

    [Test]
    public void TryCopyTo_InvalidRange_ReturnsFalse()
    {
        using var vault = CreateVault(count: 3);

        UnmanagedStringVaultCopyAssert.TryCopyToInvalidRange(vault, vault.Count);
    }

    [Test]
    public void CopyTo_InvalidRange_ThrowsArgumentOutOfRange()
    {
        using var vault = CreateVault(count: 3);

        UnmanagedStringVaultCopyAssert.CopyToInvalidRangeThrows(vault, vault.Count);
        UnmanagedStringVaultCopyAssert.CopyToLengthPastDestinationThrows(vault);
    }

    [Test]
    public void CopyTo_EmptyVault_CopiesTheInvalidSlot()
    {
        using var vault = new StringVault(CAPACITY);

        UnmanagedStringVaultCopyAssert.CopyToEmptyVault(vault);
    }

    [Test]
    public void TryCopyTo_EmptyVault_ReturnsTrueAndCopiesTheInvalidSlot()
    {
        using var vault = new StringVault(CAPACITY);

        UnmanagedStringVaultCopyAssert.TryCopyToEmptyVault(vault);
    }

    [Test]
    public void AsReadOnly_EmptyVault_ExposesOnlyTheInvalidSlot()
    {
        using var vault = new StringVault(CAPACITY);

        var view = vault.AsReadOnly();

        Assert.IsTrue(view.IsCreated);
        Assert.AreEqual(1, view.Count);
        Assert.AreEqual(vault.Capacity, view.Capacity);
        Assert.IsFalse(view.AllowEmptyString);
    }

    [Test]
    public void AsReadOnly_ViewsTheVault_SeesLaterRegistrationsWithoutGrowth()
    {
        using var vault = new StringVault(CAPACITY_LARGE);
        var view = vault.AsReadOnly();

        var id = vault.GetOrMakeId("later");
        UnmanagedString later = "later";

        Assert.AreEqual(vault.Count, view.Count);
        Assert.IsTrue(view.ContainsId(id));
        Assert.IsTrue(view.TryGetId(later, out var foundId));
        Assert.AreEqual(id, foundId);
        Assert.IsTrue(view.TryGetUnmanagedString(id, out var stored));
        Assert.AreEqual(later, stored);
    }

    [Test]
    public void Constructor_ReadOnlyFromVault_CopiesAllowEmptyStringAndReadsTheSameState()
    {
        using var vault = new StringVault(CAPACITY_LARGE, allowEmptyString: true);
        var id = vault.GetOrMakeId("alpha");

        var view = new StringVault.ReadOnly(vault);

        Assert.IsTrue(view.AllowEmptyString);
        Assert.AreEqual(vault.Count, view.Count);
        Assert.IsTrue(view.ContainsId(id));
    }

    [Test]
    public void GetEnumerator_ReadOnlyEmptyVault_YieldsOnlyTheInvalidSlot()
    {
        using var vault = new StringVault(CAPACITY);
        var view = vault.AsReadOnly();

        var items = new List<UnmanagedString>();

        foreach (var item in view)
        {
            items.Add(item);
        }

        Assert.AreEqual(1, items.Count);
        Assert.IsTrue(items[0].IsEmpty);
    }

    [Test]
    public void GetEnumerator_ReadOnlyInterfaceEnumerators_YieldTheSameSequenceAsTheStructEnumerator()
    {
        using var vault = CreateVault(count: 3);
        var view = vault.AsReadOnly();

        var expected = new List<UnmanagedString>();

        foreach (var item in view)
        {
            expected.Add(item);
        }

        var generic = UnmanagedStringVaultCopyAssert.Collect(view);
        var nonGenericCount = 0;
        var nonGeneric = ((IEnumerable)view).GetEnumerator();

        while (nonGeneric.MoveNext())
        {
            nonGenericCount++;
        }

        CollectionAssert.AreEqual(expected, generic);
        Assert.AreEqual(expected.Count, nonGenericCount);
        Assert.AreEqual(vault.Count, expected.Count);
    }

    [Test]
    public void CopyTo_ReadOnlyDefaultLength_CopiesEveryRegisteredString()
    {
        using var vault = CreateVault(count: 8);
        var view = vault.AsReadOnly();
        var expected = UnmanagedStringVaultCopyAssert.Collect(view);

        UnmanagedStringVaultCopyAssert.CopyToDefaultLength(view, expected);
    }

    [Test]
    public void TryCopyTo_ReadOnlyDefaultLength_ReturnsTrueAndCopiesEveryRegisteredString()
    {
        using var vault = CreateVault(count: 8);
        var view = vault.AsReadOnly();
        var expected = UnmanagedStringVaultCopyAssert.Collect(view);

        UnmanagedStringVaultCopyAssert.TryCopyToDefaultLength(view, expected);
    }

    [Test]
    public void CopyTo_ReadOnlySourceStartAndLength_CopiesTheMatchingSlice()
    {
        using var vault = CreateVault(count: 8);
        var view = vault.AsReadOnly();
        var expected = UnmanagedStringVaultCopyAssert.Collect(view);

        UnmanagedStringVaultCopyAssert.CopyToSourceStartAndLength(view, expected);
    }

    [Test]
    public void TryCopyTo_ReadOnlySourceStartAndLength_ReturnsTrueAndCopiesTheMatchingSlice()
    {
        using var vault = CreateVault(count: 8);
        var view = vault.AsReadOnly();
        var expected = UnmanagedStringVaultCopyAssert.Collect(view);

        UnmanagedStringVaultCopyAssert.TryCopyToSourceStartAndLength(view, expected);
    }

    [Test]
    public void TryCopyTo_ReadOnlyStartPastDestinationLength_ReturnsTrue()
    {
        using var vault = CreateVault(count: 8);
        var view = vault.AsReadOnly();
        var expected = UnmanagedStringVaultCopyAssert.Collect(view);

        UnmanagedStringVaultCopyAssert.TryCopyToStartPastDestinationLength(view, expected);
    }

    [Test]
    public void TryCopyTo_ReadOnlyLengthPastDestination_ReturnsFalse()
    {
        using var vault = CreateVault(count: 8);

        UnmanagedStringVaultCopyAssert.TryCopyToLengthPastDestination(vault.AsReadOnly());
    }

    [Test]
    public void TryCopyTo_ReadOnlyInvalidRange_ReturnsFalse()
    {
        using var vault = CreateVault(count: 3);

        UnmanagedStringVaultCopyAssert.TryCopyToInvalidRange(vault.AsReadOnly(), vault.Count);
    }

    [Test]
    public void CopyTo_ReadOnlyInvalidRange_ThrowsArgumentOutOfRange()
    {
        using var vault = CreateVault(count: 3);
        var view = vault.AsReadOnly();

        UnmanagedStringVaultCopyAssert.CopyToInvalidRangeThrows(view, vault.Count);
        UnmanagedStringVaultCopyAssert.CopyToLengthPastDestinationThrows(view);
    }

    [Test]
    public void CopyTo_ReadOnlyEmptyVault_CopiesTheInvalidSlot()
    {
        using var vault = new StringVault(CAPACITY);

        UnmanagedStringVaultCopyAssert.CopyToEmptyVault(vault.AsReadOnly());
    }

    [Test]
    public void TryCopyTo_ReadOnlyEmptyVault_ReturnsTrueAndCopiesTheInvalidSlot()
    {
        using var vault = new StringVault(CAPACITY);

        UnmanagedStringVaultCopyAssert.TryCopyToEmptyVault(vault.AsReadOnly());
    }

    [Test]
    public void AsReadOnly_UnregisteredIds_ReportFalse()
    {
        using var vault = new StringVault(CAPACITY_LARGE);
        vault.GetOrMakeId("a");
        var view = vault.AsReadOnly();

        var neverRegistered = new StringId(new Id(vault.Count + 3));
        var beyondCapacity = new StringId(new Id(uint.MaxValue));

        Assert.IsFalse(view.ContainsId(default));
        Assert.IsFalse(view.ContainsId(neverRegistered));
        Assert.IsFalse(view.ContainsId(beyondCapacity));
        Assert.IsFalse(view.TryGetUnmanagedString(neverRegistered, out _));
        Assert.IsFalse(view.TryGetUnmanagedString(beyondCapacity, out _));
        Assert.IsFalse(view.TryGetUnmanagedString(neverRegistered).HasValue);
        Assert.IsFalse(view.TryGetId(default(UnmanagedString), out _));
        Assert.IsFalse(view.TryGetId(default(UnmanagedString)).HasValue);
    }

    private static StringVault CreateVault(int count)
    {
        var vault = new StringVault(CAPACITY_LARGE);

        for (var i = 0; i < count; i++)
        {
            vault.GetOrMakeId($"s{i}");
        }

        return vault;
    }
}
