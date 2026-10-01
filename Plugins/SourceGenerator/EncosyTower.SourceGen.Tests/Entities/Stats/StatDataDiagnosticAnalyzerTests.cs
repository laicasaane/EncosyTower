using EncosyTower.Entities.Stats.Analyzers;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public class StatDataDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = StatsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<StatDataDiagnosticAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<StatDataDiagnosticAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<StatDataDiagnosticAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task NonGenericStructWithVariantType_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.Float)]
                public partial struct Hp { }
            """);

    [TestMethod]
    public Task NonGenericStructWithEnumType_NoDiagnostics()
        => RunAsync("""
                public enum SampleKind : byte { A, B }

                [EncosyTower.Entities.Stats.StatData(typeof(SampleKind))]
                public partial struct Kind { }
            """);

    [TestMethod]
    public Task StructWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public struct Plain { }
            """);

    [TestMethod]
    public Task ClassWithStatData_ReportsMustBeStruct()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.Float)]
                  public partial class {|#0:Hp|} { }
              """
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.MustBeStruct).WithLocation(0).WithArguments("Hp")
        );

    [TestMethod]
    public Task GenericStruct_ReportsMustNotBeGeneric()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.Float)]
                  public partial struct {|#0:Hp|}<T> { }
              """
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.MustNotBeGeneric).WithLocation(0).WithArguments("Hp")
        );

    [TestMethod]
    public Task StatVariantTypeNone_ReportsMustNotBeNone()
        => RunAsync(
              """
                  [{|#0:EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.None)|}]
                  public partial struct Hp { }
              """
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.StatVariantTypeMustNotBeNone)
                .WithLocation(0)
                .WithArguments("Hp")
        );

    [TestMethod]
    public Task TypeofNonEnum_ReportsTypeofArgMustBeEnum()
        => RunAsync(
              """
                  public class NotAnEnum { }

                  [{|#0:EncosyTower.Entities.Stats.StatData(typeof(NotAnEnum))|}]
                  public partial struct Hp { }
              """
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.TypeofArgMustBeEnum)
                .WithLocation(0)
                .WithArguments("TestProject.NotAnEnum")
        );
}
