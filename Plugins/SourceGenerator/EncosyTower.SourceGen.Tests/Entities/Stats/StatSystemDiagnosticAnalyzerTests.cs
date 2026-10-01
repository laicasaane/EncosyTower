using EncosyTower.Entities.Stats.Analyzers;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public class StatSystemDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = StatsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<StatSystemDiagnosticAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<StatSystemDiagnosticAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<StatSystemDiagnosticAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task NonGenericClass_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                public static class Sys { }
            """);

    [TestMethod]
    public Task NonGenericStruct_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                public partial struct Sys { }
            """);

    [TestMethod]
    public Task ClassWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public class Plain<T> { }
            """);

    [TestMethod]
    public Task GenericClass_ReportsMustNotBeGeneric()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                  public static class {|#0:Sys|}<T> { }
              """
            , new DiagnosticResult(StatSystemDiagnosticAnalyzer.MustNotBeGeneric).WithLocation(0).WithArguments("Sys")
        );

    [TestMethod]
    public Task GenericStruct_ReportsMustNotBeGeneric()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                  public partial struct {|#0:Sys|}<T> { }
              """
            , new DiagnosticResult(StatSystemDiagnosticAnalyzer.MustNotBeGeneric).WithLocation(0).WithArguments("Sys")
        );
}
