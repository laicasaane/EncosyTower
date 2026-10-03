using EncosyTower.Core.Analyzers.EnumTemplates;
using EncosyTower.Core.Generators.EnumTemplates;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.EnumTemplates;

[TestClass]
public sealed class EnumTemplateReachabilityTests
{
    private const string TOOL = ProducerFixtures.ENUM_TEMPLATE_TOOL;
    private const string PAGE_HINT = "Page_EnumTemplate.EnumTemplate.";

    private const string HEADER = """
        using EncosyTower.EnumExtensions;

        namespace TestProject;


        """;

    private const string REFERENCED_HEADER = """
        using EncosyTower.EnumExtensions;
        using TestProject;

        namespace Consumer;


        """;

    private const string PAGE = """
        [EnumTemplate]
        [EnumTemplateMembersFromEnum(typeof(ScreenType), 0)]
        [EnumTemplateMembersFromEnum(typeof(ExtraPage), 10)]
        public readonly partial struct Page_EnumTemplate { }

        public enum ExtraPage : byte
        {
            Settings,
            Credits,
        }
        """;

    [TestMethod]
    public async Task MembersFromGeneratedEnum_ReportsAndSkipsOnlyThatSource()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  ProducerFixtures.ScreenTypeProducer,
                  new NamedSource(GeneratorAnalyzerTestHelper.INPUT_PATH, HEADER + PAGE),
              }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
            , new DiagnosticAnalyzer[] { new EnumTemplateAnalyzer() }
            , new[] {
                new DiagnosticResult(EnumTemplateAnalyzer.MemberEnumIsGenerated)
                    .OnTestLine(line: 6, startColumn: 37, endColumn: 47)
                    .WithArguments(
                          "TestProject.ScreenType"
                        , "[EnumTemplateMembersFromEnum]"
                        , "Page_EnumTemplate"
                        , TOOL
                    ),
            }
        );

        var text = result.GetGeneratedSourceText(PAGE_HINT);

        StringAssert.Contains(text, "Settings = 10,");
        StringAssert.Contains(text, "Credits = 11,");
        Assert.IsFalse(text.Contains("ScreenType", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task MembersFromGeneratedEnumInReferencedAssembly_NoDiagnostic()
    {
        var reference = await GeneratorTestHelper.CompileToReferenceAsync(
              ProducerFixtures.SCREEN_TYPE_PRODUCER
            , "Reachability.Producer"
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );

        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              REFERENCED_HEADER + """
                  [EnumTemplate]
                  [EnumTemplateMembersFromEnum(typeof(ScreenType), 0)]
                  public readonly partial struct Page_EnumTemplate { }
                  """
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
            , new DiagnosticAnalyzer[] { new EnumTemplateAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );

        var text = result.GetGeneratedSourceText(PAGE_HINT);

        StringAssert.Contains(text, "Lobby = 0,");
        StringAssert.Contains(text, "Shop = 1,");
    }

    [TestMethod]
    public async Task MembersFromMissingType_ReportsOnlyCompilerError()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              HEADER + PAGE.Replace("typeof(ScreenType)", "typeof(Missing)")
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
            , new DiagnosticAnalyzer[] { new EnumTemplateAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .OnTestLine(line: 6, startColumn: 37, endColumn: 44)
                    .WithArguments("Missing"),
            }
        );

        var text = result.GetGeneratedSourceText(PAGE_HINT);

        StringAssert.Contains(text, "Settings = 10,");
        Assert.IsFalse(text.Contains("Missing", StringComparison.Ordinal));
    }
}
