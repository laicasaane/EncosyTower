using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Entities.Stats.Analyzers;
using EncosyTower.Entities.Stats.Generators;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public class StatCollectionEntryCountTests
{
    private const string COLLECTION_HINT_PREFIX = "Stats.StatCollection.";
    private const string INPUT_PATH = "Test0.cs";

    private const string SOURCE = """
        using System;
        using EncosyTower.Entities.Stats;
        using Unity.Entities;

        namespace TestProject;

        [StatSystem(StatDataSize.Size8)]
        public static partial class StatsApi { }

        [StatCollection(typeof(StatsApi), 4294967294u)]
        public partial struct Stats : IComponentData
        {
            [StatData(StatVariantType.Float)]
            public partial struct Hp { }
        {{ENTRY}}
        }
        {{TRAILER}}
        """;

    private const string OTHER_PART = """

        public partial struct Stats
        {
            [StatData(StatVariantType.Int)]
            public partial struct Gold { }
        }
        """;

    [TestMethod]
    public async Task AcceptedEntries_AreCounted()
    {
        var run = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] { CreateSource("    [StatData(StatVariantType.Int)]\n    public partial struct Gold { }") }
            , CreateGenerators()
            , CreateAnalyzers()
            , new[] {
                new DiagnosticResult(StatCollectionDiagnosticAnalyzer.TypeIdOffsetOverflow)
                    .WithSpan(INPUT_PATH, 11, 23, 11, 28)
                    .WithArguments(4294967294u, 2, "Stats"),
            }
        );

        Assert.IsNull(FindSource(run, COLLECTION_HINT_PREFIX));
        Assert.IsNotNull(FindSource(run, "Gold.StatData."));
    }

    [TestMethod]
    public Task RecordStructEntry_IsReportedAndNotCounted()
        => VerifyNotCountedAsync(
              new[] { CreateSource("    [StatData(StatVariantType.Int)]\n    public partial record struct Gold { }") }
            , CreateGenerators()
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.MustNotBeRecordOrReadOnly)
                .WithSpan(INPUT_PATH, 16, 34, 16, 38)
                .WithArguments("Gold")
        );

    [TestMethod]
    public Task PositionalRecordEntry_IsReportedAndNotCounted()
        => VerifyNotCountedAsync(
              new[] {
                  CreateSource("    [StatData(StatVariantType.Int)]\n    public partial record struct Gold(int A);"),
              }
            , CreateGenerators()
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.MustNotBeRecordOrReadOnly)
                .WithSpan(INPUT_PATH, 16, 34, 16, 38)
                .WithArguments("Gold")
        );

    [TestMethod]
    public Task ReadOnlyEntry_IsReportedAndNotCounted()
        => VerifyNotCountedAsync(
              new[] { CreateSource("    [StatData(StatVariantType.Int)]\n    public readonly partial struct Gold { }") }
            , CreateGenerators()
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.MustNotBeRecordOrReadOnly)
                .WithSpan(INPUT_PATH, 16, 36, 16, 40)
                .WithArguments("Gold")
        );

    [TestMethod]
    public Task GenericEntry_IsNotCounted()
        => VerifyNotCountedAsync(
              new[] { CreateSource("    [StatData(StatVariantType.Int)]\n    public partial struct Box<T> { }") }
            , CreateGenerators()
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.MustNotBeGeneric)
                .WithSpan(INPUT_PATH, 16, 27, 16, 30)
                .WithArguments("Box")
        );

    [TestMethod]
    public Task EntryInAnotherPart_IsReportedAndNotCounted()
        => VerifyNotCountedAsync(
              new[] { CreateSource(string.Empty, OTHER_PART) }
            , CreateGenerators()
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.StatDataOutsideAttributedPart)
                .WithSpan(INPUT_PATH, 21, 27, 21, 31)
                .WithArguments("Gold", "Stats")
        );

    [TestMethod]
    public Task NoneEntry_IsSkippedAndNotCounted()
        => VerifyNotCountedAsync(
              new[] { CreateSource("    [StatData(StatVariantType.None)]\n    public partial struct Nothing { }") }
            , CreateGenerators()
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.StatVariantTypeMustNotBeNone)
                .WithSpan(INPUT_PATH, 15, 6, 15, 36)
                .WithArguments("Nothing")
        );

    [TestMethod]
    public Task NonEnumTypeOfEntry_IsNotCounted()
        => VerifyNotCountedAsync(
              new[] { CreateSource("    [StatData(typeof(int))]\n    public partial struct Broken { }") }
            , CreateGenerators()
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.TypeofArgMustBeEnum)
                .WithSpan(INPUT_PATH, 15, 6, 15, 27)
                .WithArguments("int")
        );

    [TestMethod]
    public Task UndefinedVariantEntry_IsReportedAndNotCounted()
        => VerifyNotCountedAsync(
              new[] { CreateSource("    [StatData((StatVariantType)200)]\n    public partial struct Odd { }") }
            , CreateGenerators()
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.StatVariantTypeMustBeDefined)
                .WithSpan(INPUT_PATH, 15, 6, 15, 36)
                .WithArguments("Odd", 200)
        );

    [TestMethod]
    public Task GeneratedEnumEntry_IsNotCounted()
        => VerifyNotCountedAsync(
              new[] {
                  ProducerFixtures.ScreenTypeProducer,
                  CreateSource("    [StatData(typeof(ScreenType))]\n    public partial struct Screen { }"),
              }
            , new IIncrementalGenerator[] {
                new StatCollectionGenerator(),
                new StatDataGenerator(),
                new StatSystemGenerator(),
                new EnumTemplateGenerator(),
            }
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.TypeofArgIsGenerated)
                .WithSpan(INPUT_PATH, 15, 22, 15, 32)
                .WithArguments("TestProject.ScreenType", "[StatData]", "Screen", ProducerFixtures.ENUM_TEMPLATE_TOOL)
        );

    private static NamedSource CreateSource(string entry, string trailer = "")
        => new(INPUT_PATH, SOURCE.Replace("{{ENTRY}}", entry).Replace("{{TRAILER}}", trailer));

    private static IIncrementalGenerator[] CreateGenerators()
        => new IIncrementalGenerator[] {
            new StatCollectionGenerator(),
            new StatDataGenerator(),
            new StatSystemGenerator(),
        };

    private static DiagnosticAnalyzer[] CreateAnalyzers()
        => new DiagnosticAnalyzer[] {
            new StatCollectionDiagnosticAnalyzer(),
            new StatDataDiagnosticAnalyzer(),
        };

    private static async Task VerifyNotCountedAsync(
          IReadOnlyList<NamedSource> sources
        , IReadOnlyList<IIncrementalGenerator> generators
        , params DiagnosticResult[] expectedDiagnostics
    )
    {
        var run = await GeneratorAnalyzerTestHelper.VerifyAsync(
              sources
            , generators
            , CreateAnalyzers()
            , expectedDiagnostics
        );
        var control = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] { CreateSource(string.Empty) }
            , CreateGenerators()
            , CreateAnalyzers()
            , Array.Empty<DiagnosticResult>()
        );
        var collection = FindSource(run, COLLECTION_HINT_PREFIX);

        Assert.IsNotNull(collection, "The StatCollection output is missing.");
        Assert.AreEqual(FindSource(control, COLLECTION_HINT_PREFIX), collection);
    }

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
