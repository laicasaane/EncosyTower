using EncosyTower.Core.Analyzers.UnionIds;

namespace EncosyTower.SourceGen.Tests.Core.UnionIds;

[TestClass]
public class UnionIdAnalyzerTests
{
    private const string STUB_ATTRIBUTES = UnionIdsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<UnionIdAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<UnionIdAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<UnionIdAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task StructWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public struct Plain { public int v; }
            """);

    [TestMethod]
    public Task ValidUnionIdAndKind_NoDiagnostics()
        => RunAsync("""
                public struct K { public int v; }

                [EncosyTower.UnionIds.UnionId]
                [EncosyTower.UnionIds.UnionIdKind(typeof(K), 0UL)]
                public struct Id { public int v; }
            """);

    [TestMethod]
    public Task DuplicateKindName_ReportsSameKindIsIgnored()
        => RunAsync(
              """
                  public struct K1 { public int v; }
                  public struct K2 { public int v; }

                  [EncosyTower.UnionIds.UnionId]
                  [EncosyTower.UnionIds.UnionIdKind(typeof(K1), 0UL, "Same")]
                  [{|#0:EncosyTower.UnionIds.UnionIdKind(typeof(K2), 1UL, "Same")|}]
                  public struct Id { }
              """
            , new DiagnosticResult(UnionIdAnalyzer.SameKindIsIgnored)
                .WithLocation(0)
                .WithArguments("Same", "TestProject.K1")
        );

    [TestMethod]
    public Task UnionIdStructWithManagedField_ReportsMustBeUnmanagedType()
        => RunAsync(
              """
                  [EncosyTower.UnionIds.UnionId]
                  public struct {|#0:Id|} { public string s; }
              """
            , new DiagnosticResult(UnionIdAnalyzer.MustBeUnmanagedType).WithLocation(0)
        );

    [TestMethod]
    public Task KindForUnionIdSelf_ReportsKindTypeCannotBeIdType()
        => RunAsync(
              """
                  [{|#0:EncosyTower.UnionIds.KindForUnionId(typeof(Self), 0UL)|}]
                  public struct Self { public int v; }
              """
            , new DiagnosticResult(UnionIdAnalyzer.KindTypeCannotBeIdType)
                .WithLocation(0)
                .WithArguments("TestProject.Self")
        );

    [TestMethod]
    public Task DuplicateKindType_ReportsTypeAlreadyDeclared()
        => RunAsync(
              """
                  public struct K { public int v; }

                  [EncosyTower.UnionIds.UnionId]
                  [EncosyTower.UnionIds.UnionIdKind(typeof(K), 0UL)]
                  [{|#0:EncosyTower.UnionIds.UnionIdKind(typeof(K), 1UL)|}]
                  public struct Id { }
              """
            , new DiagnosticResult(UnionIdAnalyzer.TypeAlreadyDeclared).WithLocation(0).WithArguments("TestProject.K")
        );

    [TestMethod]
    public Task SixteenByteKind_ReportsKindSizeMustBeSmallerThan16Bytes()
        => RunAsync(
              """
                  public struct BigKind { public ulong a; public ulong b; }

                  [EncosyTower.UnionIds.UnionId]
                  [{|#0:EncosyTower.UnionIds.UnionIdKind(typeof(BigKind), 0UL, "Big")|}]
                  public struct Id { }
              """
            , new DiagnosticResult(UnionIdAnalyzer.KindSizeMustBeSmallerThan16Bytes)
                .WithLocation(0)
                .WithArguments("Big")
        );
}
