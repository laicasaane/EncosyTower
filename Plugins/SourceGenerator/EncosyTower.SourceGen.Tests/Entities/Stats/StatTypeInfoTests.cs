using EncosyTower.Entities.Stats.Generators;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public sealed class StatTypeInfoTests
{
    [TestMethod]
    public void Table_HasExpectedInvariantShape()
    {
        var all = StatTypeInfo.All;

        Assert.AreEqual(76, all.Length);
        Assert.AreEqual("None", all[0].type);
        Assert.AreEqual("None", all[0].typeName);
        Assert.AreEqual("g__ETES", all[0].namespaceName);
        Assert.AreEqual(1, all[0].size);

        var typeNames = new HashSet<string>(StringComparer.Ordinal);
        var enumNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var info in all)
        {
            Assert.IsTrue(typeNames.Add(info.type), $"Duplicate type: {info.type}");
            Assert.IsTrue(enumNames.Add(info.typeName), $"Duplicate type name: {info.typeName}");
            Assert.IsTrue(info.size > 0, $"Invalid size for {info.type}");
            Assert.IsTrue(
                info.namespaceName is "" or "g__ETES" or "g__UM",
                $"Invalid namespace for {info.type}: {info.namespaceName}"
            );
        }
    }

    [DataTestMethod]
    [DataRow("sbyte", "SByte")]
    [DataRow("byte", "Byte")]
    [DataRow("short", "Short")]
    [DataRow("ushort", "UShort")]
    [DataRow("int", "Int")]
    [DataRow("uint", "UInt")]
    [DataRow("long", "Long")]
    [DataRow("ulong", "ULong")]
    public void TryGetEnumTypeName_ReturnsOrdinalMapping(string underlyingType, string expected)
    {
        Assert.IsTrue(StatTypeInfo.TryGetEnumTypeName(underlyingType, out var actual));
        Assert.AreEqual(expected, actual);
    }
}
