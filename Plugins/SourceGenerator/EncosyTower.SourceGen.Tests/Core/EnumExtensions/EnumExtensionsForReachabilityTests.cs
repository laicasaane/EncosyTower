using EncosyTower.Core.Analyzers.EnumExtensions;
using EncosyTower.Core.Generators.EnumExtensions;
using EncosyTower.Core.Generators.EnumTemplates;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.EnumExtensions;

[TestClass]
public sealed class EnumExtensionsForReachabilityTests
{
    private const string TOOL = ProducerFixtures.ENUM_TEMPLATE_TOOL;

    private const string HEADER = """
        using EncosyTower.EnumExtensions;

        namespace TestProject;


        """;

    private const string REFERENCED_HEADER = """
        using EncosyTower.EnumExtensions;
        using TestProject;

        namespace Consumer;


        """;

    [TestMethod]
    public async Task GeneratedEnum_ReportsAtTypeOfType()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  ProducerFixtures.ScreenTypeProducer,
                  new NamedSource(
                        GeneratorAnalyzerTestHelper.INPUT_PATH
                      , HEADER + """
                          [EnumExtensionsFor(typeof(ScreenType))]
                          public static partial class ScreenTypeHelpers { }

                          [EnumExtensionsFor(typeof(MenuScreen))]
                          public static partial class MenuScreenHelpers { }
                          """
                  ),
              }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator(), new EnumExtensionsForGenerator() }
            , new DiagnosticAnalyzer[] { new EnumExtensionsAnalyzer() }
            , new[] {
                new DiagnosticResult(EnumExtensionsAnalyzer.EnumTypeIsGenerated)
                    .OnTestLine(line: 5, startColumn: 27, endColumn: 37)
                    .WithArguments("TestProject.ScreenType", "[EnumExtensionsFor]", "ScreenTypeHelpers", TOOL),
            }
        );

        Assert.IsFalse(result.HasGeneratedSource("ScreenTypeHelpers.EnumExtensionsFor."));
        Assert.IsTrue(result.HasGeneratedSource("MenuScreenHelpers.EnumExtensionsFor."));
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
              REFERENCED_HEADER + """
                  [EnumExtensionsFor(typeof(ScreenType))]
                  public static partial class ScreenTypeHelpers { }
                  """
            , new IIncrementalGenerator[] { new EnumExtensionsForGenerator() }
            , new DiagnosticAnalyzer[] { new EnumExtensionsAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );

        Assert.IsTrue(result.HasGeneratedSource("ScreenTypeHelpers.EnumExtensionsFor."));
    }

    [TestMethod]
    public async Task MissingType_KeepsExistingDiagnostics()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              HEADER + """
                  [EnumExtensionsFor(typeof(Missing))]
                  public static partial class LostHelpers { }
                  """
            , new IIncrementalGenerator[] { new EnumExtensionsForGenerator() }
            , new DiagnosticAnalyzer[] { new EnumExtensionsAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .OnTestLine(line: 5, startColumn: 27, endColumn: 34)
                    .WithArguments("Missing"),
                new DiagnosticResult(EnumExtensionsAnalyzer.TypeArgumentMustBeEnum)
                    .OnTestLine(line: 5, startColumn: 2, endColumn: 36)
                    .WithArguments("Missing"),
            }
        );

        Assert.IsFalse(result.HasGeneratedSource("LostHelpers.EnumExtensionsFor."));
    }
}
