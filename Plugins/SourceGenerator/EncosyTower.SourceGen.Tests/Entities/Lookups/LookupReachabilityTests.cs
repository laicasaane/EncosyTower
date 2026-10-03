using EncosyTower.Entities.Analyzers.Entities.Lookups;
using EncosyTower.Entities.Generators.Entities.Lookups;
using EncosyTower.Entities.Stats.Generators;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Entities.Lookups;

[TestClass]
public class LookupReachabilityTests
{
    private const string S = ProducerFixtures.STAT_SYSTEM_TOOL;

    private const string HEADER = """
        using EncosyTower.Entities;
        using Unity.Entities;

        namespace TestProject;


        """;

    [TestMethod]
    public async Task Lookup_GeneratedBufferElement_SkipsEntryAndReports()
    {
        var run = await VerifyWithProducerAsync(
              HEADER + """
                  public struct Health : IBufferElementData { }

                  [Lookup(typeof(StatsApi.Stat), true)]
                  [Lookup(typeof(Health))]
                  public partial struct Lookups : IBufferLookups { }

                  [Lookup(typeof(StatsApi.StatObserver))]
                  public partial struct ObserverLookups : IBufferLookups { }
                  """
            , D(8, 16, 8, 29, "TestProject.StatsApi+Stat", "[Lookup]", "Lookups", S)
            , D(12, 16, 12, 37, "TestProject.StatsApi+StatObserver", "[Lookup]", "ObserverLookups", S)
        );

        var control = await VerifyWithProducerAsync(
            HEADER + """
                public struct Health : IBufferElementData { }

                [Lookup(typeof(Health))]
                public partial struct Lookups : IBufferLookups { }
                """
        );

        Assert.IsNull(FindSource(run, "ObserverLookups.EntityLookup."));
        Assert.IsNotNull(FindSource(run, "Lookups.EntityLookup."));
        Assert.AreEqual(FindSource(control, "Lookups.EntityLookup."), FindSource(run, "Lookups.EntityLookup."));
    }

    [TestMethod]
    public async Task Lookup_GeneratedTypeArgument_SkipsEntryAndReports()
    {
        var run = await VerifyWithProducerAsync(
              HEADER + """
                  public struct Tagged<T> : IBufferElementData { }

                  [Lookup(typeof(Tagged<StatsApi.StatModifier>))]
                  public partial struct TaggedLookups : IBufferLookups { }
                  """
            , D(8, 16, 8, 45, "TestProject.StatsApi+StatModifier", "[Lookup]", "TaggedLookups", S)
        );

        Assert.IsNull(FindSource(run, "TaggedLookups.EntityLookup."));
    }

    [TestMethod]
    public async Task Lookup_GeneratedTypeFromReferencedAssembly_NoDiagnostic()
    {
        var reference = await GeneratorTestHelper.CompileToReferenceAsync(
              ProducerFixtures.STATS_API_PRODUCER
            , "Reachability.StatsProducer"
            , new IIncrementalGenerator[] { new StatSystemGenerator() }
        );

        var run = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  new NamedSource(
                        "Test0.cs"
                      , HEADER + """
                          [Lookup(typeof(StatsApi.Stat), true)]
                          public partial struct Lookups : IBufferLookups { }
                          """
                  ),
              }
            , new IIncrementalGenerator[] { new LookupGenerator() }
            , new DiagnosticAnalyzer[] { new LookupDiagnosticAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );

        Assert.IsNotNull(FindSource(run, "Lookups.EntityLookup."));
    }

    [TestMethod]
    public async Task Lookup_MissingType_ReportsOnlyExistingDiagnostics()
    {
        var run = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  new NamedSource(
                        "Test0.cs"
                      , HEADER + """
                          [Lookup(typeof(Missing))]
                          public partial struct Lookups : IBufferLookups { }
                          """
                  ),
              }
            , new IIncrementalGenerator[] { new LookupGenerator() }
            , new DiagnosticAnalyzer[] { new LookupDiagnosticAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan("Test0.cs", 6, 16, 6, 23)
                    .WithArguments("Missing"),
                new DiagnosticResult(LookupDiagnosticAnalyzer.ManagedTypeNotSupported)
                    .WithSpan("Test0.cs", 6, 2, 6, 25)
                    .WithArguments("Missing"),
            }
        );

        Assert.IsNull(FindSource(run, "Lookups.EntityLookup."));
    }

    private static Task<GeneratorDriverRunResult> VerifyWithProducerAsync(
          string source
        , params DiagnosticResult[] expected
    )
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] { ProducerFixtures.StatsApiProducer, new NamedSource("Test0.cs", source) }
            , new IIncrementalGenerator[] { new StatSystemGenerator(), new LookupGenerator() }
            , new DiagnosticAnalyzer[] { new LookupDiagnosticAnalyzer() }
            , expected
        );

    private static DiagnosticResult D(
          int startLine
        , int startColumn
        , int endLine
        , int endColumn
        , params object[] arguments
    )
        => new DiagnosticResult(LookupDiagnosticAnalyzer.GeneratedTypeNotSupported)
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
