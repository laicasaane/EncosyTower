using EncosyTower.Core.Generators.UnionIds;

namespace EncosyTower.SourceGen.Tests.Core.UnionIds;

[TestClass]
public sealed class UnionIdCompilationSpecEqualityTests
{
    [TestMethod]
    public void UnionIdCompilationSpec_EachFactParticipatesInEquality()
    {
        var compilation = new CompilationSpec("Assembly", true);
        var baseline = new UnionIdCompilationSpec(compilation, true, true, true, true);
        var copy = new UnionIdCompilationSpec(compilation, true, true, true, true);
        var otherAssembly = compilation with { AssemblyName = "Other" };
        var invalid = compilation with { IsValid = false };

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, baseline with { Compilation = otherAssembly });
        Assert.AreNotEqual(baseline, baseline with { Compilation = invalid });
        Assert.AreNotEqual(baseline, baseline with { EnableNullable = false });
        Assert.AreNotEqual(baseline, baseline with { Odin = false });
        Assert.AreNotEqual(baseline, baseline with { Unity = false });
        Assert.AreNotEqual(baseline, baseline with { UnityCollections = false });
    }
}
