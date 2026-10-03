using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Data.Generators.Data;
using EncosyTower.Databases.Authoring.Analyzers;
using EncosyTower.Databases.Authoring.Generators;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

[TestClass]
public class DatabaseAuthoringReachabilityTests
{
    private const string CONTAINER_HINT = "DbAuthoring.DatabaseAuthoringSheetContainer.";
    private const string SCREEN_ROW_SHEET_HINT = "ScreenRowSheet.DatabaseAuthoringSheet.";
    private const string ROW_SHEET_HINT = "RowSheet.DatabaseAuthoringSheet.";
    private const string OTHER_SHEET_HINT = "OtherSheet.DatabaseAuthoringSheet.";
    private const string DATA_PROPERTY = "[DataProperty]";
    private const string SCREEN_MEMBER = "public ScreenType Screen => Get_Screen();";

    private const string HEADER = """
        #define ENCOSY_INCLUDE_AUTHORING

        using EncosyTower.Data;
        using EncosyTower.Databases;
        using EncosyTower.Databases.Authoring;

        namespace TestProject;


        """;

    [TestMethod]
    public Task DataPropertyType_GeneratedEnum_ReportsAndSkipsOnlyItsTable()
        => VerifySkipsScreenTableAsync(
              TwoTableSource(DATA_PROPERTY, SCREEN_MEMBER)
            , A(line: 16, startColumn: 12, endColumn: 22)
        );

    [TestMethod]
    public Task DataPropertyTypeOfArgument_GeneratedEnum_ReportsAtTypeOfOnly()
        => VerifySkipsScreenTableAsync(
              TwoTableSource("[DataProperty(typeof(ScreenType))]", SCREEN_MEMBER)
            , A(line: 15, startColumn: 26, endColumn: 36)
        );

    [TestMethod]
    public Task ManualAuthoringType_GeneratedEnum_ReportsAtTypeOf()
        => VerifySkipsScreenTableAsync(
              TwoTableSource(
                  "[DataProperty, EncosyTower.Data.Authoring.DataManualAuthoring(typeof(ScreenType))]"
                , "public int Level => Get_Level();"
              )
            , A(line: 15, startColumn: 74, endColumn: 84)
        );

    [TestMethod]
    public Task CollectionMemberType_GeneratedEnum_ReportsWholeMemberType()
        => VerifySkipsScreenTableAsync(
              TwoTableSource(
                  DATA_PROPERTY
                , "public System.Collections.Generic.List<ScreenType> Screens => new();"
              )
            , A(line: 16, startColumn: 12, endColumn: 55)
        );

    [TestMethod]
    public async Task NestedDataMemberType_GeneratedEnum_SkipsEveryTableReachingIt()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + """
                  [Data]
                  public partial struct Reward
                  {
                      [DataProperty]
                      public int Amount => Get_Amount();

                      [DataProperty]
                      public ScreenType Screen => Get_Screen();
                  }

                  [Data]
                  public partial struct Row : IDataWithId<int>
                  {
                      [DataProperty]
                      public int Id => 1;

                      [DataProperty]
                      public Reward Bonus => Get_Bonus();
                  }

                  [Data]
                  public partial struct Other : IDataWithId<int>
                  {
                      [DataProperty]
                      public int Id => 1;
                  }

                  [DataTableAsset]
                  public partial class Rows : DataTableAssetBase<int, Row>
                  {
                      protected override int GetId(in Row entry) => entry.Id;
                  }

                  [DataTableAsset]
                  public partial class Others : DataTableAssetBase<int, Other>
                  {
                      protected override int GetId(in Other entry) => entry.Id;
                  }

                  [Database]
                  public readonly partial struct Db
                  {
                      [Table]
                      public readonly Rows Items => default;

                      [Table]
                      public readonly Others Extras => default;
                  }

                  [AuthorDatabase(typeof(Db))]
                  public partial struct DbAuthoring { }
                  """
            , A(line: 16, startColumn: 12, endColumn: 22)
        );

        var container = result.GetGeneratedSourceText(CONTAINER_HINT);

        Assert.IsFalse(result.HasGeneratedSource(ROW_SHEET_HINT));
        Assert.IsTrue(result.HasGeneratedSource(OTHER_SHEET_HINT));
        StringAssert.Contains(container, "Others_OtherSheet_Extras");
        Assert.IsFalse(container.Contains("Rows_RowSheet_Items", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task HandWrittenEnum_GeneratesEveryTable()
    {
        var result = await VerifyControlAsync();
        var container = result.GetGeneratedSourceText(CONTAINER_HINT);

        Assert.IsTrue(result.HasGeneratedSource(SCREEN_ROW_SHEET_HINT));
        Assert.IsTrue(result.HasGeneratedSource(ROW_SHEET_HINT));
        StringAssert.Contains(container, "ScreenRows_ScreenRowSheet_Screens");
        StringAssert.Contains(container, "Rows_RowSheet_Items");
    }

    [TestMethod]
    public async Task GeneratedEnumFromReferencedAssembly_NoDiagnostic()
    {
        var reference = await GeneratorTestHelper.CompileToReferenceAsync(
              ProducerFixtures.SCREEN_TYPE_PRODUCER
            , "Reachability.Producer"
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );

        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              TwoTableSource(DATA_PROPERTY, SCREEN_MEMBER)
            , new IIncrementalGenerator[] { new DataGenerator(), new DatabaseAuthoringGenerator() }
            , new DiagnosticAnalyzer[] { new DatabaseAuthoringDiagnosticAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );

        Assert.IsTrue(result.HasGeneratedSource(SCREEN_ROW_SHEET_HINT));
        Assert.IsTrue(result.HasGeneratedSource(ROW_SHEET_HINT));
    }

    [TestMethod]
    public async Task MissingType_ReportsOnlyCompilerError()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              HEADER + """
                  [Data]
                  public partial struct ScreenRow : IDataWithId<int>
                  {
                      [DataProperty]
                      public int Id => 1;

                      [DataProperty]
                      public Missing Screen => default;
                  }

                  [DataTableAsset]
                  public partial class ScreenRows : DataTableAssetBase<int, ScreenRow>
                  {
                      protected override int GetId(in ScreenRow entry) => entry.Id;
                  }

                  [Database]
                  public readonly partial struct Db
                  {
                      [Table]
                      public readonly ScreenRows Screens => default;
                  }

                  [AuthorDatabase(typeof(Db))]
                  public partial struct DbAuthoring { }
                  """
            , new IIncrementalGenerator[] { new DatabaseAuthoringGenerator() }
            , new DiagnosticAnalyzer[] { new DatabaseAuthoringDiagnosticAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan("Test0.cs", 16, 12, 16, 19)
                    .WithArguments("Missing"),
            }
        );

        Assert.IsFalse(result.HasGeneratedSource(string.Empty));
    }

    private static async Task VerifySkipsScreenTableAsync(string source, DiagnosticResult expected)
    {
        var result = await VerifyWithProducerAsync(source, expected);
        var control = await VerifyControlAsync();
        var container = result.GetGeneratedSourceText(CONTAINER_HINT);

        Assert.IsFalse(result.HasGeneratedSource(SCREEN_ROW_SHEET_HINT));
        Assert.AreEqual(
              control.GetGeneratedSourceText(ROW_SHEET_HINT)
            , result.GetGeneratedSourceText(ROW_SHEET_HINT)
        );
        StringAssert.Contains(container, "Rows_RowSheet_Items");
        Assert.IsFalse(container.Contains("ScreenRows_ScreenRowSheet_Screens", StringComparison.Ordinal));
    }

    private static Task<GeneratorDriverRunResult> VerifyControlAsync()
        => VerifyWithProducerAsync(TwoTableSource(DATA_PROPERTY, "public MenuScreen Screen => Get_Screen();"));

    private static string TwoTableSource(string attributeLine, string memberLine)
        => HEADER + $$"""
            [Data]
            public partial struct ScreenRow : IDataWithId<int>
            {
                [DataProperty]
                public int Id => 1;

                {{attributeLine}}
                {{memberLine}}
            }

            [Data]
            public partial struct Row : IDataWithId<int>
            {
                [DataProperty]
                public int Id => 1;
            }

            [DataTableAsset]
            public partial class ScreenRows : DataTableAssetBase<int, ScreenRow>
            {
                protected override int GetId(in ScreenRow entry) => entry.Id;
            }

            [DataTableAsset]
            public partial class Rows : DataTableAssetBase<int, Row>
            {
                protected override int GetId(in Row entry) => entry.Id;
            }

            [Database]
            public readonly partial struct Db
            {
                [Table]
                public readonly ScreenRows Screens => default;

                [Table]
                public readonly Rows Items => default;
            }

            [AuthorDatabase(typeof(Db))]
            public partial struct DbAuthoring { }
            """;

    private static DiagnosticResult A(int line, int startColumn, int endColumn)
        => new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.DataMemberTypeIsGenerated)
            .WithSpan("Test0.cs", line, startColumn, line, endColumn)
            .WithArguments(
                  "TestProject.ScreenType"
                , "[AuthorDatabase]"
                , "DbAuthoring"
                , ProducerFixtures.ENUM_TEMPLATE_TOOL
            );

    private static Task<GeneratorDriverRunResult> VerifyWithProducerAsync(
          string source
        , params DiagnosticResult[] expected
    )
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] { ProducerFixtures.ScreenTypeProducer, new NamedSource("Test0.cs", source) }
            , new IIncrementalGenerator[] {
                new EnumTemplateGenerator(),
                new DataGenerator(),
                new DatabaseAuthoringGenerator(),
            }
            , new DiagnosticAnalyzer[] { new DatabaseAuthoringDiagnosticAnalyzer() }
            , expected
        );
}
