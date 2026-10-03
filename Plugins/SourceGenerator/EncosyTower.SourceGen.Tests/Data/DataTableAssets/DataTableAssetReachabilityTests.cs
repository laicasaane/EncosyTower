using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Data.Analyzers.DataTableAssets;
using EncosyTower.Data.Generators.DataTableAssets;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Data.DataTableAssets;

[TestClass]
public class DataTableAssetReachabilityTests
{
    private const string HEADER = """
        using EncosyTower.Data;
        using EncosyTower.Databases;

        namespace TestProject;


        """;

    [TestMethod]
    public async Task IdType_GeneratedEnum_ReportsAndSkipsOnlyItsTarget()
    {
        var result = await VerifyWithProducerAsync(
              SourceA("ScreenType")
            , D(line: 12, startColumn: 54, endColumn: 64, target: "ScreenRows")
            , X(line: 12, startColumn: 22, endColumn: 32)
        );

        var control = await VerifyWithProducerAsync(SourceA("MenuScreen"));

        Assert.IsFalse(result.HasGeneratedSource("ScreenRows.DataTableAsset."));
        Assert.AreEqual(
              control.GetGeneratedSourceText("Rows.DataTableAsset.")
            , result.GetGeneratedSourceText("Rows.DataTableAsset.")
        );
    }

    [TestMethod]
    public async Task IdType_HandWrittenEnum_GeneratesEveryTarget()
    {
        var result = await VerifyWithProducerAsync(SourceA("MenuScreen"));

        Assert.IsTrue(result.HasGeneratedSource("ScreenRows.DataTableAsset."));
        Assert.IsTrue(result.HasGeneratedSource("Rows.DataTableAsset."));
    }

    [TestMethod]
    public async Task ConvertedIdType_GeneratedEnum_OmitsOnlyConvertId()
    {
        var result = await VerifyWithProducerAsync(
              SourceB("ScreenType")
            , D(line: 17, startColumn: 58, endColumn: 68, target: "Rows")
            , X(line: 17, startColumn: 22, endColumn: 26)
        );

        var text = result.GetGeneratedSourceText("Rows.DataTableAsset.");

        StringAssert.Contains(
              text
            , "protected sealed override global::TestProject.Key GetId(in global::TestProject.Row entry)"
        );
        Assert.IsFalse(text.Contains("ConvertId", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ConvertedIdType_HandWrittenEnum_GeneratesConvertId()
    {
        var result = await VerifyWithProducerAsync(SourceB("MenuScreen"));

        StringAssert.Contains(
              result.GetGeneratedSourceText("Rows.DataTableAsset.")
            , "ConvertId(global::TestProject.Key value)"
        );
    }

    [TestMethod]
    public async Task DataTypeArgument_GeneratedTypeArgument_ReportsWholeArgument()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + """
                  public readonly struct Row<T> : IDataWithId<int>
                  {
                      public int Id => 1;
                  }

                  [DataTableAsset]
                  public partial class Rows : DataTableAssetBase<int, Row<ScreenType>>
                  {
                  }
                  """
            , D(line: 12, startColumn: 53, endColumn: 68, target: "Rows")
            , X(line: 12, startColumn: 22, endColumn: 26)
        );

        Assert.IsFalse(result.HasGeneratedSource("Rows.DataTableAsset."));
    }

    [TestMethod]
    public async Task HandWrittenGetId_GeneratedEnum_NoDiagnostic()
    {
        var result = await VerifyWithProducerAsync(
            HEADER + """
                public readonly struct ScreenRow : IDataWithId<ScreenType>
                {
                    public ScreenType Id => default;
                }

                [DataTableAsset]
                public partial class ScreenRows : DataTableAssetBase<ScreenType, ScreenRow>
                {
                    protected override ScreenType GetId(in ScreenRow entry) => entry.Id;
                }
                """
        );

        Assert.IsFalse(result.HasGeneratedSource("ScreenRows.DataTableAsset."));
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
              SourceA("ScreenType")
            , new IIncrementalGenerator[] { new DataTableAssetGenerator() }
            , new DiagnosticAnalyzer[] { new DataTableAssetAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );

        Assert.IsTrue(result.HasGeneratedSource("ScreenRows.DataTableAsset."));
        Assert.IsTrue(result.HasGeneratedSource("Rows.DataTableAsset."));
    }

    [TestMethod]
    public async Task MissingConvertedIdType_ReportsOnlyCompilerErrors()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              HEADER + """
                  public readonly struct Row : IDataWithId<int>
                  {
                      public int Id => 1;
                  }

                  [DataTableAsset]
                  public partial class Rows : DataTableAssetBase<int, Row, Missing>
                  {
                  }
                  """
            , new IIncrementalGenerator[] { new DataTableAssetGenerator() }
            , new DiagnosticAnalyzer[] { new DataTableAssetAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan("Test0.cs", 12, 58, 12, 65)
                    .WithArguments("Missing"),
                X(line: 12, startColumn: 22, endColumn: 26),
            }
        );

        var text = result.GetGeneratedSourceText("Rows.DataTableAsset.");

        Assert.IsFalse(text.Contains("ConvertId", StringComparison.Ordinal));
    }

    private static string SourceA(string idType)
        => HEADER + $$"""
            public readonly struct ScreenRow : IDataWithId<{{idType}}>
            {
                public {{idType}} Id => default;
            }

            [DataTableAsset]
            public partial class ScreenRows : DataTableAssetBase<{{idType}}, ScreenRow>
            {
            }

            public readonly struct Row : IDataWithId<int>
            {
                public int Id => 1;
            }

            [DataTableAsset]
            public partial class Rows : DataTableAssetBase<int, Row>
            {
            }
            """;

    private static string SourceB(string convertedIdType)
        => HEADER + $$"""
            public readonly struct Key
            {
                public static implicit operator {{convertedIdType}}(Key key) => default;
            }

            public readonly struct Row : IDataWithId<Key>
            {
                public Key Id => default;
            }

            [DataTableAsset]
            public partial class Rows : DataTableAssetBase<Key, Row, {{convertedIdType}}>
            {
            }
            """;

    private static DiagnosticResult D(int line, int startColumn, int endColumn, string target)
        => new DiagnosticResult(DataTableAssetAnalyzer.TypeArgumentIsGenerated)
            .WithSpan("Test0.cs", line, startColumn, line, endColumn)
            .WithArguments("TestProject.ScreenType", "[DataTableAsset]", target, ProducerFixtures.ENUM_TEMPLATE_TOOL);

    private static DiagnosticResult X(int line, int startColumn, int endColumn)
        => DiagnosticResult.CompilerError("CS0534").WithSpan("Test0.cs", line, startColumn, line, endColumn);

    private static Task<GeneratorDriverRunResult> VerifyWithProducerAsync(
          string source
        , params DiagnosticResult[] expected
    )
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] { ProducerFixtures.ScreenTypeProducer, new NamedSource("Test0.cs", source) }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator(), new DataTableAssetGenerator() }
            , new DiagnosticAnalyzer[] { new DataTableAssetAnalyzer() }
            , expected
        );
}
