using EncosyTower.Databases.Authoring.Analyzers;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

[TestClass]
public class MisplacedConverterAttributeTests
{
    private const string STUB_ATTRIBUTES = DatabaseAuthoringAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using EncosyTower.Data;\n    using EncosyTower.Databases;\n    using EncosyTower.Databases.Authoring;\n{body}\n}}\n";

    private static Task RunDataPropertyAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<ConverterForDataPropertyDiagnosticAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    private static Task RunTableAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<ConverterForTableDiagnosticAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task ConverterForDataProperty_OnNonAuthorType_ReportsMisplaced()
        => RunDataPropertyAsync(
              """
                  public class ConvA { public int Convert(string s) => 0; }

                  public class MyData : IData { }

                  [{|#0:ConverterForDataProperty(typeof(MyData), "Foo", typeof(ConvA))|}]
                  public partial class NotAuthor { }
              """
            , new DiagnosticResult(ConverterForDataPropertyDiagnosticAnalyzer.MisplacedConverterAttribute)
                .WithLocation(0)
                .WithArguments("ConverterForDataProperty")
        );

    [TestMethod]
    public Task ConverterForDataProperty_OnAuthorType_NoDiagnostics()
        => RunDataPropertyAsync("""
                public class ConvA { public int Convert(string s) => 0; }

                public class MyData : IData { }

                public class MyTable : DataTableAssetBase<int, MyData> { }

                [Database]
                public partial class MyDb
                {
                    [Table]
                    public MyTable Items { get; }
                }

                [ConverterForDataProperty(typeof(MyData), "Foo", typeof(ConvA))]
                [AuthorDatabase(typeof(MyDb))]
                public partial class MyAuthor { }
            """);

    [TestMethod]
    public Task ConverterForTable_OnNonAuthorType_ReportsMisplaced()
        => RunTableAsync(
              """
                  public class ConvA { public int Convert(string s) => 0; }

                  [{|#0:ConverterForTable("Items", typeof(ConvA))|}]
                  public partial class NotAuthor { }
              """
            , new DiagnosticResult(ConverterForTableDiagnosticAnalyzer.MisplacedConverterAttribute)
                .WithLocation(0)
                .WithArguments("ConverterForTable")
        );

    [TestMethod]
    public Task ConverterForTable_OnAuthorType_NoDiagnostics()
        => RunTableAsync("""
                public class ConvA { public int Convert(string s) => 0; }

                public class MyData : IData { }

                public class MyTable : DataTableAssetBase<int, MyData> { }

                [Database]
                public partial class MyDb
                {
                    [Table]
                    public MyTable Items { get; }
                }

                [ConverterForTable("Items", typeof(ConvA))]
                [AuthorDatabase(typeof(MyDb))]
                public partial class MyAuthor { }
            """);
}
