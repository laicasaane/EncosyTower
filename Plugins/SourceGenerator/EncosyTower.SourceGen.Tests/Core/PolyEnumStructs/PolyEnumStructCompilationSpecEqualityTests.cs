using EncosyTower.Core.Generators.PolyEnumStructs;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumStructs;

[TestClass]
public sealed class PolyEnumStructCompilationSpecEqualityTests
{
    [TestMethod]
    public void PolyEnumStructCompilationSpec_EachFactParticipatesInEquality()
    {
        var compilation = new CompilationSpec("Assembly", true);
        var baseline = new PolyEnumStructCompilationSpec(compilation, true, true);
        var copy = new PolyEnumStructCompilationSpec(compilation, true, true);
        var otherAssembly = compilation with { AssemblyName = "Other" };
        var invalid = compilation with { IsValid = false };

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, baseline with { Compilation = otherAssembly });
        Assert.AreNotEqual(baseline, baseline with { Compilation = invalid });
        Assert.AreNotEqual(baseline, baseline with { EnableNullable = false });
        Assert.AreNotEqual(baseline, baseline with { UnityCollections = false });
    }
}
