using EncosyTower.Types;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Types;

public class TypeIdVaultTests
{
    [Test]
    public void RegistrationOrder_DoesNotSplitIds()
    {
        var id = typeof(Probe).GetOrRegisterId();

        Assert.AreEqual(id, (TypeId)Type<Probe>.Id);
        Assert.AreEqual(id, RuntimeTypeCache.GetInfo(typeof(Probe)).Id);
    }

    private sealed class Probe { }
}
