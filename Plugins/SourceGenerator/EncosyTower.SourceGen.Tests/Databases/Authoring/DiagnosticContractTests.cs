using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

[TestClass]
[DoNotParallelize]
public sealed class DiagnosticContractTests
{
    [TestMethod]
    public void RegisteredDiagnostics_MatchIndependentLiteralCatalog()
        => DiagnosticContractTestHelper.VerifyDiagnostics(new DiagnosticContractProvider());

    [TestMethod]
    public void RegisteredSuppressions_MatchIndependentLiteralCatalog()
        => DiagnosticContractTestHelper.VerifySuppressions(new DiagnosticContractProvider());
}
