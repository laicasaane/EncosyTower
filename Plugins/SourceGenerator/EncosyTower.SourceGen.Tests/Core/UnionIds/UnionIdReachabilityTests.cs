using EncosyTower.Core.Analyzers.UnionIds;
using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Core.Generators.UnionIds;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.UnionIds;

[TestClass]
public sealed class UnionIdReachabilityTests
{
    private const string TOOL = ProducerFixtures.ENUM_TEMPLATE_TOOL;

    private const string HEADER = """
        using EncosyTower.UnionIds;

        namespace TestProject;


        """;

    private const string REFERENCED_HEADER = """
        using EncosyTower.UnionIds;
        using TestProject;

        namespace Consumer;


        """;

    [TestMethod]
    public async Task GeneratedKind_ReportsAndSkipsTheUnionId()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  ProducerFixtures.ScreenTypeProducer,
                  new NamedSource(
                        GeneratorAnalyzerTestHelper.INPUT_PATH
                      , HEADER + """
                          [UnionId]
                          [UnionIdKind(typeof(ScreenType), 0)]
                          [UnionIdKind(typeof(MenuScreen), 1)]
                          public readonly partial struct ScreenId { }

                          [UnionId]
                          [UnionIdKind(typeof(MenuScreen), 0)]
                          public readonly partial struct MenuId { }
                          """
                  ),
              }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator(), new UnionIdGenerator() }
            , new DiagnosticAnalyzer[] { new UnionIdAnalyzer() }
            , new[] {
                new DiagnosticResult(UnionIdAnalyzer.KindTypeIsGenerated)
                    .OnTestLine(line: 6, startColumn: 21, endColumn: 31)
                    .WithArguments("TestProject.ScreenType", "[UnionIdKind]", "ScreenId", TOOL),
            }
        );

        Assert.IsFalse(result.HasGeneratedSource("ScreenId.UnionId."));
        Assert.IsTrue(result.HasGeneratedSource("MenuId.UnionId."));
    }

    [TestMethod]
    public async Task GeneratedKindFromReferencedAssembly_NoDiagnostic()
    {
        var reference = await GeneratorTestHelper.CompileToReferenceAsync(
              ProducerFixtures.SCREEN_TYPE_PRODUCER
            , "Reachability.Producer"
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );

        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              REFERENCED_HEADER + """
                  [UnionId]
                  [UnionIdKind(typeof(ScreenType), 0)]
                  public readonly partial struct ScreenId { }
                  """
            , new IIncrementalGenerator[] { new UnionIdGenerator() }
            , new DiagnosticAnalyzer[] { new UnionIdAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );

        Assert.IsTrue(result.HasGeneratedSource("ScreenId.UnionId."));
    }

    [TestMethod]
    public async Task MissingKind_KeepsExistingDiagnostics()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              HEADER + """
                  [UnionId]
                  [UnionIdKind(typeof(Missing), 0)]
                  public readonly partial struct LostId { }
                  """
            , new IIncrementalGenerator[] { new UnionIdGenerator() }
            , new DiagnosticAnalyzer[] { new UnionIdAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .OnTestLine(line: 6, startColumn: 21, endColumn: 28)
                    .WithArguments("Missing"),
                new DiagnosticResult(UnionIdAnalyzer.MustBeUnmanagedType)
                    .OnTestLine(line: 6, startColumn: 2, endColumn: 33),
            }
        );

        Assert.IsFalse(result.HasGeneratedSource("LostId.UnionId."));
    }
}
