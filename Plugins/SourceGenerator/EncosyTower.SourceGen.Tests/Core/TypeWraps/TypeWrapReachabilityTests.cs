using EncosyTower.Core.Analyzers.TypeWraps;
using EncosyTower.Core.Generators.EnumExtensions;
using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Core.Generators.TypeWraps;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.TypeWraps;

[TestClass]
public sealed class TypeWrapReachabilityTests
{
    private const string TOOL = ProducerFixtures.ENUM_TEMPLATE_TOOL;
    private const string ENUM_EXTENSIONS_TOOL = "EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator";
    private const string GENERATED_PATH = "Generated/Produced.g.cs";
    private const string HAND_WRITTEN_PATH = "Produced.cs";

    private const string HEADER = """
        using EncosyTower.TypeWraps;

        namespace TestProject;


        """;

    [TestMethod]
    public async Task WrapRecord_GeneratedEnum_ReportsAtParameterType()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + """
                  [WrapRecord]
                  public readonly partial record struct ScreenScope(ScreenType _);

                  [WrapRecord]
                  public readonly partial record struct Health(int _);
                  """
            , D(6, 51, 6, 61, "TestProject.ScreenType", "[WrapRecord]", "ScreenScope", TOOL)
        );

        AssertNoOutput(result, "ScreenScope");
        AssertHasOutput(result, "Health");
    }

    [TestMethod]
    public async Task WrapType_GeneratedEnum_ReportsAtTypeOfType()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + """
                  [WrapType(typeof(ScreenType))]
                  public partial struct ScreenHandle { }
                  """
            , D(5, 18, 5, 28, "TestProject.ScreenType", "[WrapType]", "ScreenHandle", TOOL)
        );

        AssertNoOutput(result, "ScreenHandle");
    }

    [TestMethod]
    public async Task WrapType_ArrayInTypeArgument_ReportsWholeTypeOfType()
    {
        var result = await VerifyWithProducerAsync(
              "using System.Collections.Generic;\n" + HEADER + """
                  [WrapType(typeof(List<ScreenType[]>))]
                  public partial class ScreenList { }
                  """
            , D(6, 18, 6, 36, "TestProject.ScreenType", "[WrapType]", "ScreenList", TOOL)
        );

        AssertNoOutput(result, "ScreenList");
    }

    [TestMethod]
    public async Task WrapType_NestedGeneratedType_NamesContainingType()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              """
              using System;
              using EncosyTower.EnumExtensions;
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [Flags]
              [EnumExtensions]
              public enum Permission : byte
              {
                  None = 0,
                  Read = 1,
                  Write = 2,
              }

              [WrapType(typeof(PermissionExtended.Enumerator))]
              public partial struct PermissionCursor { }
              """
            , new IIncrementalGenerator[] { new EnumExtensionsGenerator(), new TypeWrapGenerator() }
            , new DiagnosticAnalyzer[] { new TypeWrapDiagnosticAnalyzer() }
            , new[] {
                D(
                      16
                    , 18
                    , 16
                    , 47
                    , "TestProject.PermissionExtended+Enumerator"
                    , "[WrapType]"
                    , "PermissionCursor"
                    , ENUM_EXTENSIONS_TOOL
                ),
            }
        );

        AssertNoOutput(result, "PermissionCursor");
    }

    [TestMethod]
    public async Task WrapType_GeneratedTypeArgumentOfContainingType_Reports()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + """
                  public class Holder<T>
                  {
                      public struct Inner { }
                  }

                  [WrapType(typeof(Holder<ScreenType>.Inner))]
                  public partial struct InnerHandle { }
                  """
            , D(10, 18, 10, 42, "TestProject.ScreenType", "[WrapType]", "InnerHandle", TOOL)
        );

        AssertNoOutput(result, "InnerHandle");
    }

    [TestMethod]
    public async Task WrapRecord_HandWrittenEnum_NoDiagnostic()
    {
        var result = await VerifyWithProducerAsync(
            HEADER + """
                [WrapRecord]
                public readonly partial record struct MenuScope(MenuScreen _);
                """
        );

        AssertHasOutput(result, "MenuScope");
    }

    [TestMethod]
    public async Task WrapRecord_HandWrittenPartialWithGeneratedPartial_NoDiagnostic()
    {
        var result = await VerifyWithProducerAsync(
            HEADER + """
                [WrapRecord]
                public readonly partial record struct TemplateScope(ScreenType_EnumTemplate _);
                """
        );

        AssertHasOutput(result, "TemplateScope");
    }

    [TestMethod]
    public async Task WrapRecord_MissingType_ReportsOnlyCompilerError()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              HEADER + """
                  [WrapRecord]
                  public readonly partial record struct LostScope(Missing _);
                  """
            , new IIncrementalGenerator[] { new TypeWrapGenerator() }
            , new DiagnosticAnalyzer[] { new TypeWrapDiagnosticAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan(GeneratorAnalyzerTestHelper.INPUT_PATH, 6, 49, 6, 56)
                    .WithArguments("Missing"),
            }
        );

        AssertNoOutput(result, "LostScope");
    }

    [TestMethod]
    public async Task WrapRecord_GeneratedEnumFromReferencedAssembly_NoDiagnostic()
    {
        var reference = await GeneratorTestHelper.CompileToReferenceAsync(
              ProducerFixtures.SCREEN_TYPE_PRODUCER
            , "Reachability.Producer"
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );

        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              HEADER + """
                  [WrapRecord]
                  public readonly partial record struct ScreenScope(ScreenType _);
                  """
            , new IIncrementalGenerator[] { new TypeWrapGenerator() }
            , new DiagnosticAnalyzer[] { new TypeWrapDiagnosticAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );

        AssertHasOutput(result, "ScreenScope");
    }

    [TestMethod]
    public Task GeneratedFileWithEncosyTowerTool_Reports()
        => VerifyRuleEdgeReportsAsync(
              new NamedSource(GENERATED_PATH, ProducedEnum(TOOL))
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrappedTypeIsGenerated)
                .WithLocation(0)
                .WithArguments("TestProject.Produced", "[WrapRecord]", "Wrapper", TOOL)
        );

    [TestMethod]
    public Task GeneratedFileWithThirdPartyTool_NoDiagnostic()
        => VerifyRuleEdgeSilentAsync(
              new NamedSource(GENERATED_PATH, ProducedEnum("ThirdParty.Generators.ProducerGenerator"))
        );

    [TestMethod]
    public Task GeneratedFileWithoutGeneratorsSegment_NoDiagnostic()
        => VerifyRuleEdgeSilentAsync(new NamedSource(GENERATED_PATH, ProducedEnum("EncosyTower.Tools.CodeGen")));

    [TestMethod]
    public Task GeneratedFileWithoutAttribute_NoDiagnostic()
        => VerifyRuleEdgeSilentAsync(
              new NamedSource(GENERATED_PATH, "namespace TestProject { public enum Produced { A } }")
        );

    [TestMethod]
    public Task HandWrittenFileWithEncosyTowerTool_NoDiagnostic()
        => VerifyRuleEdgeSilentAsync(new NamedSource(HAND_WRITTEN_PATH, ProducedEnum(TOOL)));

    [TestMethod]
    public Task HandWrittenPartialWithGeneratedPartial_NoDiagnostic()
        => VerifyRuleEdgeSilentAsync(
              new NamedSource(HAND_WRITTEN_PATH, "namespace TestProject { public partial struct Produced { } }")
            , new NamedSource(
                  GENERATED_PATH
                , "namespace TestProject { "
                    + $"[System.CodeDom.Compiler.GeneratedCode(\"{TOOL}\", \"1.0\")] "
                    + "public partial struct Produced { } }"
            )
        );

    private static DiagnosticResult D(
          int startLine
        , int startColumn
        , int endLine
        , int endColumn
        , params object[] arguments
    )
        => new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrappedTypeIsGenerated)
            .WithSpan(GeneratorAnalyzerTestHelper.INPUT_PATH, startLine, startColumn, endLine, endColumn)
            .WithArguments(arguments);

    private static Task<GeneratorDriverRunResult> VerifyWithProducerAsync(
          string source
        , params DiagnosticResult[] expected
    )
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  ProducerFixtures.ScreenTypeProducer,
                  new NamedSource(GeneratorAnalyzerTestHelper.INPUT_PATH, source),
              }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator(), new TypeWrapGenerator() }
            , new DiagnosticAnalyzer[] { new TypeWrapDiagnosticAnalyzer() }
            , expected
        );

    private static void AssertHasOutput(GeneratorDriverRunResult result, string name)
        => Assert.IsTrue(HasOutput(result, name), $"TypeWrap output for {name} is missing.");

    private static void AssertNoOutput(GeneratorDriverRunResult result, string name)
        => Assert.IsFalse(HasOutput(result, name), $"TypeWrap output for {name} is present.");

    private static bool HasOutput(GeneratorDriverRunResult result, string name)
    {
        var prefix = $"{name}.TypeWrap.";

        return result.Results
            .SelectMany(static generatorResult => generatorResult.GeneratedSources)
            .Any(source => source.HintName.StartsWith(prefix, StringComparison.Ordinal));
    }

    private static string ProducedEnum(string tool)
        => "namespace TestProject { "
            + $"[System.CodeDom.Compiler.GeneratedCode(\"{tool}\", \"1.0\")] "
            + "public enum Produced { A } }";

    private static Task VerifyRuleEdgeReportsAsync(NamedSource producedSource, DiagnosticResult expected)
        => VerifyRuleEdgeAsync("{|#0:Produced|}", new[] { producedSource }, new[] { expected });

    private static Task VerifyRuleEdgeSilentAsync(params NamedSource[] producedSources)
        => VerifyRuleEdgeAsync("Produced", producedSources, Array.Empty<DiagnosticResult>());

    private static Task VerifyRuleEdgeAsync(
          string producedReference
        , IReadOnlyList<NamedSource> producedSources
        , DiagnosticResult[] expected
    )
    {
        var test0 = TypeWrapsAnalyzerStubs.ATTRIBUTES
            + "\nnamespace TestProject { [EncosyTower.TypeWraps.WrapRecord] "
            + $"public readonly partial record struct Wrapper({producedReference} _); }}\n";

        var sources = new List<NamedSource> { new("Test0.cs", test0) };
        sources.AddRange(producedSources);

        return AnalyzerTestHelper.VerifyAsync<TypeWrapDiagnosticAnalyzer>(
              sources
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );
    }
}
