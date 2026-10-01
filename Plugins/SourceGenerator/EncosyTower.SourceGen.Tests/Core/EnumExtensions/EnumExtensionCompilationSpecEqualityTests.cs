using EncosyTower.Core.Generators.EnumExtensions;

namespace EncosyTower.SourceGen.Tests.Core.EnumExtensions;

[TestClass]
public sealed class EnumExtensionCompilationSpecEqualityTests
{
    [TestMethod]
    public void EnumExtensionCompilationSpec_EachFactParticipatesInEquality()
    {
        var compilation = new CompilationSpec("Assembly", true);
        var baseline = new EnumExtensionCompilationSpec(compilation, true);
        var copy = new EnumExtensionCompilationSpec(compilation, true);
        var otherAssembly = compilation with { AssemblyName = "Other" };
        var invalid = compilation with { IsValid = false };

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, baseline with { Compilation = otherAssembly });
        Assert.AreNotEqual(baseline, baseline with { Compilation = invalid });
        Assert.AreNotEqual(baseline, baseline with { UnityCollections = false });
    }
}
