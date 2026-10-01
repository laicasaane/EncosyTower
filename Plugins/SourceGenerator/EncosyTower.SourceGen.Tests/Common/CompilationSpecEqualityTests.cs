namespace EncosyTower.SourceGen.Tests.Common;

[TestClass]
public sealed class CompilationSpecEqualityTests
{
    [TestMethod]
    public void CompilationSpec_EachFactParticipatesInEquality()
    {
        var baseline = new CompilationSpec("Assembly", true);
        var copy = new CompilationSpec("Assembly", true);

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, baseline with { AssemblyName = "Other" });
        Assert.AreNotEqual(baseline, baseline with { IsValid = false });
    }
}
