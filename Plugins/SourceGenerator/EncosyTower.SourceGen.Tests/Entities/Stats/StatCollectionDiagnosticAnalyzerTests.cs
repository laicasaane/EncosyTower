using EncosyTower.Entities.Stats.Analyzers;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public class StatCollectionDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = StatsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<StatCollectionDiagnosticAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<StatCollectionDiagnosticAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<StatCollectionDiagnosticAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task ValidCollection_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                public partial struct Sys { }

                [EncosyTower.Entities.Stats.StatCollection(typeof(Sys))]
                public partial struct Coll { }
            """);

    [TestMethod]
    public Task StructWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public struct Plain { }
            """);

    [TestMethod]
    public Task ClassWithStatCollection_ReportsMustBeStruct()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatCollection(typeof(int))]
                  public partial class {|#0:Coll|} { }
              """
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.MustBeStruct).WithLocation(0).WithArguments("Coll")
        );

    [TestMethod]
    public Task GenericStruct_ReportsMustNotBeNonGeneric()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatCollection(typeof(int))]
                  public partial struct {|#0:Coll|}<T> { }
              """
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.MustNotBeNonGeneric)
                .WithLocation(0)
                .WithArguments("Coll")
        );

    [TestMethod]
    public Task SystemTypeMissingStatSystem_ReportsStatSystemAttributeRequired()
        => RunAsync(
              """
                  public class Plain { }

                  [{|#0:EncosyTower.Entities.Stats.StatCollection(typeof(Plain))|}]
                  public partial struct Coll { }
              """
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.StatSystemAttributeRequired)
                .WithLocation(0)
                .WithArguments("TestProject.Plain")
        );

    [TestMethod]
    public Task TypeIdOffsetMaxWithStatDataMember_ReportsOverflow()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                  public partial struct Sys { }

                  [EncosyTower.Entities.Stats.StatCollection(typeof(Sys), 4294967295u)]
                  public partial struct {|#0:Coll|}
                  {
                      [EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.Float)]
                      public partial struct Hp { }
                  }
              """
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.TypeIdOffsetOverflow)
                .WithLocation(0)
                .WithArguments(4294967295u, 1, "Coll")
        );

    [TestMethod]
    public Task TypeIdOffsetMaxWithoutStatData_NoOverflow()
        => RunAsync("""
                [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                public partial struct Sys { }

                [EncosyTower.Entities.Stats.StatCollection(typeof(Sys), 4294967295u)]
                public partial struct Coll { }
            """);

    [DataTestMethod]
    [DataRow("public partial record struct {|#0:Coll|} { }")]
    [DataRow("public partial record struct {|#0:Coll|}(int Extra);")]
    [DataRow("public partial record struct {|#0:Coll|}();")]
    [DataRow("public readonly partial record struct {|#0:Coll|} { }")]
    [DataRow("public readonly partial struct {|#0:Coll|} { }")]
    public Task RecordOrReadOnlyStruct_ReportsMustNotBeRecordOrReadOnly(string declaration)
        => RunAsync(
              $$"""
                  [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                  public partial struct Sys { }

                  [EncosyTower.Entities.Stats.StatCollection(typeof(Sys))]
                  {{declaration}}
              """
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.MustNotBeRecordOrReadOnly)
                .WithLocation(0)
                .WithArguments("Coll")
        );

    [TestMethod]
    public Task StatDataInPartWithoutCollectionAttribute_ReportsOutsideAttributedPart()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                  public partial struct Sys { }

                  [EncosyTower.Entities.Stats.StatCollection(typeof(Sys))]
                  public partial struct Coll { }

                  public partial struct Coll
                  {
                      [EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.Float)]
                      public partial struct {|#0:Hp|} { }
                  }
              """
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.StatDataOutsideAttributedPart)
                .WithLocation(0)
                .WithArguments("Hp", "Coll")
        );
}
