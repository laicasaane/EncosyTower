using EncosyTower.Core.Analyzers.PolyEnumStructs;
using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Core.Generators.PolyEnumStructs;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumStructs;

[TestClass]
public sealed class PolyEnumStructReachabilityTests
{
    private const string TOOL = ProducerFixtures.ENUM_TEMPLATE_TOOL;
    private const string SCREEN_HINT = "Screen.PolyEnumStruct.";

    private const string HEADER = """
        using System.Runtime.InteropServices;
        using EncosyTower.PolyEnumStructs;

        namespace TestProject;


        """;

    private const string REFERENCED_HEADER = """
        using System.Runtime.InteropServices;
        using EncosyTower.PolyEnumStructs;
        using TestProject;

        namespace Consumer;


        """;

    private const string SCREEN = """
        [PolyEnumStruct]
        [StructLayout(LayoutKind.Explicit)]
        public partial struct Screen
        {
            public partial struct Opened
            {
                public ScreenType screen;
                public long frame;
            }

            public partial struct Closed { }
        }
        """;

    [TestMethod]
    public async Task ExplicitLayout_GeneratedCaseFieldType_ReportsAndSkipsTarget()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + SCREEN + "\n\n" + """
                  [PolyEnumStruct]
                  [StructLayout(LayoutKind.Explicit)]
                  public partial struct Padding
                  {
                      public partial struct Mixed
                      {
                          public int number;
                          public byte before;
                          public byte after;
                      }

                      public partial struct Nothing { }
                  }
                  """
            , D(line: 12, startColumn: 16, endColumn: 26)
        );

        Assert.IsFalse(result.HasGeneratedSource(SCREEN_HINT));
        Assert.IsTrue(result.HasGeneratedSource("Padding.PolyEnumStruct."));
    }

    [TestMethod]
    public async Task ExplicitLayout_GeneratedTypeInHandWrittenStruct_Reports()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + """
                  public struct ScreenSlot
                  {
                      public ScreenType screen;
                  }

                  [PolyEnumStruct]
                  [StructLayout(LayoutKind.Explicit)]
                  public partial struct Screen
                  {
                      public partial struct Opened
                      {
                          public ScreenSlot slot;
                      }

                      public partial struct Closed { }
                  }
                  """
            , D(line: 17, startColumn: 16, endColumn: 26)
        );

        Assert.IsFalse(result.HasGeneratedSource(SCREEN_HINT));
    }

    [TestMethod]
    public async Task ExplicitLayout_GeneratedTypeArgumentStoredByValue_Reports()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + """
                  public struct Holder<T>
                  {
                      public T value;
                  }

                  [PolyEnumStruct]
                  [StructLayout(LayoutKind.Explicit)]
                  public partial struct Screen
                  {
                      public partial struct Opened
                      {
                          public Holder<ScreenType> holder;
                      }

                      public partial struct Closed { }
                  }
                  """
            , D(line: 17, startColumn: 16, endColumn: 34)
        );

        Assert.IsFalse(result.HasGeneratedSource(SCREEN_HINT));
    }

    [TestMethod]
    public async Task ExplicitLayout_GeneratedRecordParameter_ReportsAtParameterType()
    {
        var result = await VerifyWithProducerAsync(
              HEADER + """
                  [PolyEnumStruct]
                  [StructLayout(LayoutKind.Explicit)]
                  public partial struct Screen
                  {
                      public partial record struct Opened(ScreenType Value);

                      public partial struct Closed { }
                  }
                  """
            , D(line: 10, startColumn: 41, endColumn: 51)
        );

        Assert.IsFalse(result.HasGeneratedSource(SCREEN_HINT));
    }

    [TestMethod]
    public async Task SequentialLayout_GeneratedCaseFieldType_KeepsOutput()
    {
        var result = await VerifyWithProducerAsync(
            HEADER + """
                [PolyEnumStruct]
                public partial struct Screen
                {
                    public partial struct Opened
                    {
                        public ScreenType screen;
                    }

                    public partial struct Closed { }
                }
                """
        );

        Assert.IsTrue(result.HasGeneratedSource(SCREEN_HINT));
    }

    [TestMethod]
    public async Task ExplicitLayout_HandWrittenEnumCaseField_KeepsOutput()
    {
        var result = await VerifyWithProducerAsync(
            HEADER + SCREEN.Replace("public ScreenType screen;", "public MenuScreen screen;")
        );

        Assert.IsTrue(result.HasGeneratedSource(SCREEN_HINT));
    }

    [TestMethod]
    public async Task ExplicitLayout_GeneratedEnumFromReferencedAssembly_KeepsOutput()
    {
        var reference = await GeneratorTestHelper.CompileToReferenceAsync(
              ProducerFixtures.SCREEN_TYPE_PRODUCER
            , "Reachability.Producer"
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );

        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              REFERENCED_HEADER + SCREEN
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
            , new DiagnosticAnalyzer[] { new PolyEnumStructAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );

        Assert.IsTrue(result.HasGeneratedSource(SCREEN_HINT));
    }

    [TestMethod]
    public async Task ExplicitLayout_MissingCaseFieldType_ReportsOnlyCompilerError()
    {
        var result = await GeneratorAnalyzerTestHelper.VerifyAsync(
              HEADER + SCREEN.Replace("public ScreenType screen;", "public Missing screen;")
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
            , new DiagnosticAnalyzer[] { new PolyEnumStructAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .OnTestLine(line: 12, startColumn: 16, endColumn: 23)
                    .WithArguments("Missing"),
            }
        );

        Assert.IsFalse(result.HasGeneratedSource(SCREEN_HINT));
    }

    [TestMethod]
    public Task RuleEdge_GeneratedFieldType_Reports()
        => VerifyRuleEdgeAsync(
              "public {|#0:Produced|} value;"
            , new DiagnosticResult(PolyEnumStructAnalyzer.CaseFieldTypeIsGenerated)
                .WithLocation(0)
                .WithArguments("TestProject.Produced", "[PolyEnumStruct]", "Message", TOOL)
        );

    [TestMethod]
    public Task RuleEdge_GeneratedTypeBehindArray_ReportsOnlyManagedReference()
        => VerifyRuleEdgeAsync(
              "public {|#0:Produced[]|} values;"
            , new DiagnosticResult(PolyEnumStructAnalyzer.CaseFieldHoldsManagedReference)
                .WithLocation(0)
                .WithArguments("Message", "values", "Named", "global::TestProject.Produced[]")
        );

    private static DiagnosticResult D(int line, int startColumn, int endColumn)
        => new DiagnosticResult(PolyEnumStructAnalyzer.CaseFieldTypeIsGenerated)
            .OnTestLine(line, startColumn, endColumn)
            .WithArguments("TestProject.ScreenType", "[PolyEnumStruct]", "Screen", TOOL);

    private static Task<GeneratorDriverRunResult> VerifyWithProducerAsync(
          string source
        , params DiagnosticResult[] expected
    )
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  ProducerFixtures.ScreenTypeProducer,
                  new NamedSource(GeneratorAnalyzerTestHelper.INPUT_PATH, source),
              }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator(), new PolyEnumStructGenerator() }
            , new DiagnosticAnalyzer[] { new PolyEnumStructAnalyzer() }
            , expected
        );

    private static Task VerifyRuleEdgeAsync(string field, DiagnosticResult expected)
    {
        var test0 = PolyEnumStructsAnalyzerStubs.ATTRIBUTES
            + "\nnamespace TestProject\n{\n"
            + "    [EncosyTower.PolyEnumStructs.PolyEnumStruct]\n"
            + "    [System.Runtime.InteropServices.StructLayout("
            + "System.Runtime.InteropServices.LayoutKind.Explicit)]\n"
            + "    public partial struct Message\n    {\n"
            + $"        public partial struct Named {{ {field} }}\n\n"
            + "        public partial struct Empty { }\n"
            + "    }\n}\n";

        var produced = "namespace TestProject { "
            + $"[System.CodeDom.Compiler.GeneratedCode(\"{TOOL}\", \"1.0\")] "
            + "public enum Produced { A } }";

        return AnalyzerTestHelper.VerifyAsync<PolyEnumStructAnalyzer>(
              new[] { new NamedSource("Test0.cs", test0), new NamedSource("Generated/Produced.g.cs", produced) }
            , new[] { expected }
            , runtimeReferences: Array.Empty<MetadataReference>()
        );
    }
}
