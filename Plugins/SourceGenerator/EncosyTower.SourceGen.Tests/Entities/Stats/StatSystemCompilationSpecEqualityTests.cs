using EncosyTower.Entities.Stats.Generators;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public sealed class StatSystemCompilationSpecEqualityTests
{
    [TestMethod]
    public void StatSystemCompilationSpec_EachFactParticipatesInEquality()
    {
        var compilation = new CompilationSpec("Assembly", true);
        var baseline = new StatSystemCompilationSpec(compilation, true);
        var copy = new StatSystemCompilationSpec(compilation, true);
        var otherAssembly = compilation with { AssemblyName = "Other" };
        var invalid = compilation with { IsValid = false };

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, baseline with { Compilation = otherAssembly });
        Assert.AreNotEqual(baseline, baseline with { Compilation = invalid });
        Assert.AreNotEqual(baseline, baseline with { LatiosCore = false });
    }
}
