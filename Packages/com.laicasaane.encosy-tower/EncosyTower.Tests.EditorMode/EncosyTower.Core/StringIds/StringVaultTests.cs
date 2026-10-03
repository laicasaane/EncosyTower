using EncosyTower.StringIds;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.StringIds;

public class StringVaultTests
{
    private const int CAPACITY = 4;

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
}
