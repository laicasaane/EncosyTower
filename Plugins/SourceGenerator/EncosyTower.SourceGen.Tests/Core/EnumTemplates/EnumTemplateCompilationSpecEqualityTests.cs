using EncosyTower.Core.Generators.EnumTemplates;

namespace EncosyTower.SourceGen.Tests.Core.EnumTemplates;

[TestClass]
public sealed class EnumTemplateCompilationSpecEqualityTests
{
    [TestMethod]
    public void EnumTemplateCompilationSpec_EachFactParticipatesInEquality()
    {
        var compilation = new CompilationSpec("Assembly", true);
        var baseline = new EnumTemplateCompilationSpec(compilation, true);
        var copy = new EnumTemplateCompilationSpec(compilation, true);
        var otherAssembly = compilation with { AssemblyName = "Other" };
        var invalid = compilation with { IsValid = false };

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, baseline with { Compilation = otherAssembly });
        Assert.AreNotEqual(baseline, baseline with { Compilation = invalid });
        Assert.AreNotEqual(baseline, baseline with { UnityCollections = false });
    }
}
