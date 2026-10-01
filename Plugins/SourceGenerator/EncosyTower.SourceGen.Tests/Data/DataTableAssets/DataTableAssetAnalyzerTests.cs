using EncosyTower.Data.Analyzers.DataTableAssets;

namespace EncosyTower.SourceGen.Tests.Data.DataTableAssets;

[TestClass]
public class DataTableAssetAnalyzerTests
{
    private const string STUB_ATTRIBUTES = DataTableAssetAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<DataTableAssetAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<DataTableAssetAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<DataTableAssetAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task ValidIdAndDataTypes_NoDiagnostics()
        => RunAsync("""
                public struct Id { }
                public struct Data { }

                [EncosyTower.Databases.DataTableAsset]
                public partial class Asset : EncosyTower.Databases.DataTableAsset<Id, Data> { }
            """);

    [TestMethod]
    public Task ClassWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public partial class Asset : EncosyTower.Databases.DataTableAsset<System.IComparable, System.IDisposable> { }
            """);

    [TestMethod]
    public Task InterfaceAsIdType_ReportsMustBeApplicable()
        => RunAsync(
              """
                  public struct Data { }

                  [EncosyTower.Databases.DataTableAsset]
                  public partial class {|#0:Asset|} : EncosyTower.Databases.DataTableAsset<System.IComparable, Data> { }
              """
            , new DiagnosticResult(DataTableAssetAnalyzer.MustBeApplicableForTypeArgument)
                .WithLocation(0)
                .WithArguments("IComparable", "TDataId")
        );

    [TestMethod]
    public Task InterfaceAsDataType_ReportsMustBeApplicable()
        => RunAsync(
              """
                  public struct Id { }

                  [EncosyTower.Databases.DataTableAsset]
                  public partial class {|#0:Asset|} : EncosyTower.Databases.DataTableAsset<Id, System.IDisposable> { }
              """
            , new DiagnosticResult(DataTableAssetAnalyzer.MustBeApplicableForTypeArgument)
                .WithLocation(0)
                .WithArguments("IDisposable", "TData")
        );

    [TestMethod]
    public Task ThreeTypeArgVariant_NoDiagnostics()
        => RunAsync("""
                public struct Id { }
                public struct Data { }
                public struct ConvertedId { }

                [EncosyTower.Databases.DataTableAsset]
                public partial class Asset : EncosyTower.Databases.DataTableAsset<Id, Data, ConvertedId> { }
            """);
}
