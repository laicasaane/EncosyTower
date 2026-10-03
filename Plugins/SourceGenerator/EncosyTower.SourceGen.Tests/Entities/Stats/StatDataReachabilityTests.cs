using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Entities.Stats.Analyzers;
using EncosyTower.Entities.Stats.Generators;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public class StatDataReachabilityTests
{
    private const string T = ProducerFixtures.ENUM_TEMPLATE_TOOL;

    private const string HEADER = """
        using EncosyTower.Entities.Stats;

        namespace TestProject;


        """;

    [TestMethod]
    public async Task StatData_GeneratedEnum_SkipsTargetAndReports()
    {
        var run = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  ProducerFixtures.ScreenTypeProducer,
                  new NamedSource(
                        "Test0.cs"
                      , HEADER + """
                          [StatData(typeof(ScreenType))]
                          public partial struct Screen { }

                          [StatData(typeof(MenuScreen))]
                          public partial struct Menu { }

                          [StatData(StatVariantType.Float)]
                          public partial struct Hp { }
                          """
                  ),
              }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator(), new StatDataGenerator() }
            , new DiagnosticAnalyzer[] { new StatDataDiagnosticAnalyzer() }
            , new[] { D(5, 18, 5, 28, "TestProject.ScreenType", "[StatData]", "Screen", T) }
        );

        Assert.IsNull(FindSource(run, "Screen.StatData."));
        Assert.IsNotNull(FindSource(run, "Menu.StatData."));
        Assert.IsNotNull(FindSource(run, "Hp.StatData."));
    }

    [TestMethod]
    public async Task StatData_GeneratedEnumFromReferencedAssembly_NoDiagnostic()
    {
        var reference = await GeneratorTestHelper.CompileToReferenceAsync(
              ProducerFixtures.SCREEN_TYPE_PRODUCER
            , "Reachability.Producer"
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );

        var run = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  new NamedSource(
                        "Test0.cs"
                      , HEADER + """
                          [StatData(typeof(ScreenType))]
                          public partial struct Screen { }
                          """
                  ),
              }
            , new IIncrementalGenerator[] { new StatDataGenerator() }
            , new DiagnosticAnalyzer[] { new StatDataDiagnosticAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );

        Assert.IsNotNull(FindSource(run, "Screen.StatData."));
    }

    [TestMethod]
    public async Task StatData_MissingType_ReportsOnlyExistingDiagnostics()
    {
        var run = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  new NamedSource(
                        "Test0.cs"
                      , HEADER + """
                          [StatData(typeof(Missing))]
                          public partial struct Lost { }
                          """
                  ),
              }
            , new IIncrementalGenerator[] { new StatDataGenerator() }
            , new DiagnosticAnalyzer[] { new StatDataDiagnosticAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan("Test0.cs", 5, 18, 5, 25)
                    .WithArguments("Missing"),
                new DiagnosticResult(StatDataDiagnosticAnalyzer.TypeofArgMustBeEnum)
                    .WithSpan("Test0.cs", 5, 2, 5, 27)
                    .WithArguments("Missing"),
            }
        );

        Assert.IsNull(FindSource(run, "Lost.StatData."));
    }

    [DataTestMethod]
    [DataRow("ScreenType")]
    [DataRow("Holder<ScreenType>.Kind")]
    public async Task StatCollectionEntry_GeneratedEnum_ReportsOnceAndSkipsEntry(string enumType)
    {
        var source = $$"""
            using System;
            using EncosyTower.Entities.Stats;
            using Unity.Entities;

            namespace TestProject;

            public class Holder<T>
            {
                public enum Kind : byte { A, B }
            }

            [StatSystem(StatDataSize.Size8)]
            public static partial class StatsApi { }

            [StatCollection(typeof(StatsApi), 1000)]
            public partial struct Stats : IComponentData
            {
                [StatData(StatVariantType.Float)]
                public partial struct Hp { }

                [StatData(typeof({{enumType}}))]
                public partial struct Screen { }
            }
            """;

        var controlSource = source.Replace(
              $"\n\n    [StatData(typeof({enumType}))]\n    public partial struct Screen {{ }}"
            , ""
        );

        Assert.AreNotEqual(source, controlSource);

        var run = await VerifyStatCollectionAsync(
              source
            , D(21, 22, 21, 22 + enumType.Length, "TestProject.ScreenType", "[StatData]", "Screen", T)
        );

        var control = await VerifyStatCollectionAsync(controlSource);

        Assert.IsNull(FindSource(run, "Screen.StatData."));
        Assert.IsNotNull(FindSource(run, "Hp.StatData."));
        Assert.AreEqual(FindSource(control, "Stats.StatCollection."), FindSource(run, "Stats.StatCollection."));
    }

    private static Task<GeneratorDriverRunResult> VerifyStatCollectionAsync(
          string source
        , params DiagnosticResult[] expected
    )
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] { ProducerFixtures.ScreenTypeProducer, new NamedSource("Test0.cs", source) }
            , new IIncrementalGenerator[] {
                new EnumTemplateGenerator(),
                new StatDataGenerator(),
                new StatCollectionGenerator(),
                new StatSystemGenerator(),
            }
            , new DiagnosticAnalyzer[] {
                new StatDataDiagnosticAnalyzer(),
                new StatCollectionDiagnosticAnalyzer(),
            }
            , expected
        );

    private static DiagnosticResult D(
          int startLine
        , int startColumn
        , int endLine
        , int endColumn
        , params object[] arguments
    )
        => new DiagnosticResult(StatDataDiagnosticAnalyzer.TypeofArgIsGenerated)
            .WithSpan("Test0.cs", startLine, startColumn, endLine, endColumn)
            .WithArguments(arguments);

    private static string? FindSource(GeneratorDriverRunResult run, string hintPrefix)
    {
        var matches = run.Results
            .SelectMany(static result => result.GeneratedSources)
            .Where(source => source.HintName.StartsWith(hintPrefix, StringComparison.Ordinal))
            .ToArray();

        Assert.IsTrue(matches.Length < 2, $"More than one generated source starts with '{hintPrefix}'.");
        return matches.Length == 1 ? matches[0].SourceText.ToString() : null;
    }
}
