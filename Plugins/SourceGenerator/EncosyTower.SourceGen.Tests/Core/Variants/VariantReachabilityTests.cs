using EncosyTower.Core.Analyzers.Variants;
using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Core.Generators.Variants;
using Microsoft.CodeAnalysis.Diagnostics;
using CoreInternalVariantGenerator = EncosyTower.Core.Generators.Variants.InternalVariantGenerator;

namespace EncosyTower.SourceGen.Tests.Core.Variants;

[TestClass]
public sealed class VariantReachabilityTests
{
    private const string TOOL = ProducerFixtures.ENUM_TEMPLATE_TOOL;

    private const string HEADER = """
        using EncosyTower.Variants;

        namespace TestProject;


        """;

    private const string CONVERTERS_HEADER = """
        using EncosyTower.Variants.Converters;

        namespace TestProject;


        """;

    private const string REFERENCED_HEADER = """
        using EncosyTower.Variants;
        using TestProject;

        namespace Consumer;


        """;

    private const string SCREEN_VARIANT = """
        [Variant(typeof(ScreenType))]
        public readonly partial struct ScreenVariant { }
        """;

    private const string SCREEN_USAGE = """
        public sealed class Usage
        {
            public void Execute()
            {
                var screen = Variant<ScreenType>.GetConverter();
            }
        }
        """;

    [TestMethod]
    public async Task VariantAttribute_GeneratedType_ReportsAndSkipsVariant()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + SCREEN_VARIANT + "\n\n" + """
                  [Variant(typeof(MenuScreen))]
                  public readonly partial struct MenuVariant { }
                  """
            , VariantGenerators()
            , D(
                  line: 5
                , startColumn: 17
                , endColumn: 27
                , type: "TestProject.ScreenType"
                , position: "[Variant]"
                , target: "ScreenVariant"
            )
        );

        Assert.IsFalse(result.HasGeneratedSource("ScreenVariant.VariantStruct."));
        Assert.IsTrue(result.HasGeneratedSource("MenuVariant.VariantStruct."));

        var registration = result.GetGeneratedSourceText("Input.VariantRegistration.");

        StringAssert.Contains(registration, "MenuVariant");
        Assert.IsFalse(registration.Contains("ScreenVariant", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task GetConverter_GeneratedType_ReportsAndSkipsInternalVariant()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + """
                  public sealed class Usage
                  {
                      public void Execute()
                      {
                          var screen = Variant<ScreenType>.GetConverter();
                          var menu = Variant<MenuScreen>.GetConverter();
                      }
                  }
                  """
            , InternalVariantGenerators()
            , D(
                  line: 9
                , startColumn: 30
                , endColumn: 40
                , type: "TestProject.ScreenType"
                , position: "Variant<T>.GetConverter"
                , target: "ScreenType"
            )
        );

        Assert.IsFalse(result.HasGeneratedSource("ScreenType.InternalVariant."));
        Assert.IsTrue(result.HasGeneratedSource("MenuScreen.InternalVariant."));
        Assert.IsTrue(result.HasGeneratedSource("Input.InternalVariantRegistry."));
    }

    [TestMethod]
    public async Task CachedConverter_GeneratedType_Reports()
    {
        var result = await VerifyWithProducerAsync(
              CONVERTERS_HEADER + """
                  public sealed class Usage
                  {
                      public void Execute()
                      {
                          var cached = CachedVariantConverter<ScreenType>.Default;
                      }
                  }
                  """
            , InternalVariantGenerators()
            , D(
                  line: 9
                , startColumn: 45
                , endColumn: 55
                , type: "TestProject.ScreenType"
                , position: "CachedVariantConverter<T>.Default"
                , target: "ScreenType"
            )
        );

        Assert.IsFalse(result.HasGeneratedSource("ScreenType.InternalVariant."));
        Assert.IsFalse(result.HasGeneratedSource("Input.InternalVariantRegistry."));
    }

    [TestMethod]
    public async Task VariantAttribute_GeneratedTypeFromReferencedAssembly_NoDiagnostic()
    {
        var result = await VerifyReferencedAsync(REFERENCED_HEADER + SCREEN_VARIANT, VariantGenerators());

        Assert.IsTrue(result.HasGeneratedSource("ScreenVariant.VariantStruct."));
    }

    [TestMethod]
    public async Task GetConverter_GeneratedTypeFromReferencedAssembly_NoDiagnostic()
    {
        var result = await VerifyReferencedAsync(REFERENCED_HEADER + SCREEN_USAGE, InternalVariantGenerators());

        Assert.IsTrue(result.HasGeneratedSource("ScreenType.InternalVariant."));
    }

    [TestMethod]
    public async Task VariantAttribute_MissingType_ReportsOnlyCompilerError()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              HEADER + """
                  [Variant(typeof(Missing))]
                  public readonly partial struct LostVariant { }
                  """
            , VariantGenerators()
            , new DiagnosticAnalyzer[] { new VariantAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .OnTestLine(line: 5, startColumn: 17, endColumn: 24)
                    .WithArguments("Missing"),
            }
        );

        Assert.IsFalse(result.HasGeneratedSource("LostVariant.VariantStruct."));
    }

    [TestMethod]
    public async Task GetConverter_MissingType_ReportsOnlyCompilerError()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              HEADER + SCREEN_USAGE.Replace(
                  "var screen = Variant<ScreenType>.GetConverter();"
                , "var lost = Variant<Missing>.GetConverter();"
              )
            , InternalVariantGenerators()
            , new DiagnosticAnalyzer[] { new VariantAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .OnTestLine(line: 9, startColumn: 28, endColumn: 35)
                    .WithArguments("Missing"),
            }
        );

        Assert.IsFalse(result.HasGeneratedSource(string.Empty));
    }

    private static IIncrementalGenerator[] VariantGenerators()
        => new IIncrementalGenerator[] { new VariantStructGenerator(), new VariantRegistrationGenerator() };

    private static IIncrementalGenerator[] InternalVariantGenerators()
        => new IIncrementalGenerator[] { new CoreInternalVariantGenerator() };

    private static DiagnosticResult D(
          int line
        , int startColumn
        , int endColumn
        , string type
        , string position
        , string target
    )
        => new DiagnosticResult(VariantAnalyzer.VariantTypeIsGenerated)
            .OnTestLine(line, startColumn, endColumn)
            .WithArguments(type, position, target, TOOL);

    private static Task<GeneratorDriverRunResult> VerifyWithProducerAsync(
          string source
        , IIncrementalGenerator[] featureGenerators
        , params DiagnosticResult[] expected
    )
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  ProducerFixtures.ScreenTypeProducer,
                  new NamedSource(GeneratorAnalyzerTestHelper.INPUT_PATH, source),
              }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }.Concat(featureGenerators).ToArray()
            , new DiagnosticAnalyzer[] { new VariantAnalyzer() }
            , expected
        );

    private static async Task<GeneratorDriverRunResult> VerifyReferencedAsync(
          string source
        , IIncrementalGenerator[] featureGenerators
    )
    {
        var reference = await GeneratorTestHelper.CompileToReferenceAsync(
              ProducerFixtures.SCREEN_TYPE_PRODUCER
            , "Reachability.Producer"
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );

        return await GeneratorAnalyzerTestHelper.VerifyAsync(
              source
            , featureGenerators
            , new DiagnosticAnalyzer[] { new VariantAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );
    }
}
