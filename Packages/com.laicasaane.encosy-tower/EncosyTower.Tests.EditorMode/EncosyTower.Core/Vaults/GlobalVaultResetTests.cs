using EncosyTower.Ids;
using EncosyTower.Vaults;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Vaults;

public class GlobalVaultResetTests
{
    [Test]
    public void PlayModeReset_ClearsAllMaps()
    {
        var typedId = new Id<Probe>(value: 101);
        var valueId2 = new Id2(x: 102, y: 103);
        var objectId2 = new Id2(x: 104, y: 105);
        var obj = new Probe();

        GlobalValueVault<int>.TrySet(id: typedId, value: 1);
        GlobalValueVault<int>.TrySet(id: valueId2, value: 2);
        GlobalObjectVault.TryAdd(objectId2, obj);

        GlobalValueVaultEditor.InitWhenDomainReloadDisabled();
        GlobalObjectVault.InitWhenDomainReloadDisabled();

        Assert.IsFalse(GlobalValueVault<int>.Contains(typedId));
        Assert.IsFalse(GlobalValueVault<int>.Contains(valueId2));
        Assert.IsFalse(GlobalObjectVault.Contains<Probe>(objectId2));

        Assert.IsTrue(GlobalValueVault<int>.TryAdd(id: typedId, value: 1));
        Assert.IsTrue(GlobalValueVault<int>.TryAdd(id: valueId2, value: 2));
        Assert.IsTrue(GlobalObjectVault.TryAdd(objectId2, obj));

        GlobalValueVault<int>.TryRemove(typedId, out _);
        GlobalValueVault<int>.TryRemove(valueId2, out _);
        GlobalObjectVault.TryRemove(objectId2, obj);
    }

    private sealed class Probe { }
}
