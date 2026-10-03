using EncosyTower.Entities.Analyzers.Entities.TypeHandles;
using EncosyTower.Entities.Generators.Entities.TypeHandles;
using EncosyTower.Entities.Stats.Generators;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Entities.TypeHandles;

[TestClass]
public class TypeHandleReachabilityTests
{
    private const string S = ProducerFixtures.STAT_SYSTEM_TOOL;

    private const string HEADER = """
        using EncosyTower.Entities;
        using Unity.Entities;

        namespace TestProject;


        """;

    [TestMethod]
    public async Task TypeHandle_GeneratedBufferElement_SkipsEntryAndReports()
    {
        var run = await VerifyWithProducerAsync(
              HEADER + """
                  public struct Health : IComponentData { }

                  [TypeHandle(typeof(StatsApi.StatModifier), true)]
                  [TypeHandle(typeof(Health))]
                  public partial struct Handles { }

                  [TypeHandle(typeof(StatsApi.Stat))]
                  public partial struct StatOnlyHandles { }
                  """
            , D(8, 20, 8, 41, "TestProject.StatsApi+StatModifier", "[TypeHandle]", "Handles", S)
            , D(12, 20, 12, 33, "TestProject.StatsApi+Stat", "[TypeHandle]", "StatOnlyHandles", S)
        );

        var control = await VerifyWithProducerAsync(
            HEADER + """
                public struct Health : IComponentData { }

                [TypeHandle(typeof(Health))]
                public partial struct Handles { }
                """
        );

        Assert.IsNull(FindSource(run, "StatOnlyHandles.EntityTypeHandle."));
        Assert.IsNotNull(FindSource(run, "Handles.EntityTypeHandle."));
        Assert.AreEqual(
              FindSource(control, "Handles.EntityTypeHandle.")
            , FindSource(run, "Handles.EntityTypeHandle.")
        );
    }

    [TestMethod]
    public async Task TypeHandle_GeneratedTypeArgument_SkipsEntryAndReports()
    {
        var run = await VerifyWithProducerAsync(
              HEADER + """
                  public struct Tagged<T> : IBufferElementData { }

                  [TypeHandle(typeof(Tagged<StatsApi.StatObserver>))]
                  public partial struct TaggedHandles { }
                  """
            , D(8, 20, 8, 49, "TestProject.StatsApi+StatObserver", "[TypeHandle]", "TaggedHandles", S)
        );

        Assert.IsNull(FindSource(run, "TaggedHandles.EntityTypeHandle."));
    }

    [TestMethod]
    public async Task TypeHandle_GeneratedTypeFromReferencedAssembly_NoDiagnostic()
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
                          [TypeHandle(typeof(StatsApi.Stat), true)]
                          public partial struct Handles { }
                          """
                  ),
              }
            , new IIncrementalGenerator[] { new TypeHandleGenerator() }
            , new DiagnosticAnalyzer[] { new TypeHandleDiagnosticAnalyzer() }
            , Array.Empty<DiagnosticResult>()
            , additionalReferences: new[] { reference }
        );

        Assert.IsNotNull(FindSource(run, "Handles.EntityTypeHandle."));
    }

    [TestMethod]
    public async Task TypeHandle_MissingType_ReportsOnlyExistingDiagnostics()
    {
        var run = await GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  new NamedSource(
                        "Test0.cs"
                      , HEADER + """
                          [TypeHandle(typeof(Missing))]
                          public partial struct Handles { }
                          """
                  ),
              }
            , new IIncrementalGenerator[] { new TypeHandleGenerator() }
            , new DiagnosticAnalyzer[] { new TypeHandleDiagnosticAnalyzer() }
            , new[] {
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan("Test0.cs", 6, 20, 6, 27)
                    .WithArguments("Missing"),
                new DiagnosticResult(TypeHandleDiagnosticAnalyzer.ManagedTypeNotSupported)
                    .WithSpan("Test0.cs", 6, 2, 6, 29)
                    .WithArguments("Missing"),
            }
        );

        Assert.IsNull(FindSource(run, "Handles.EntityTypeHandle."));
    }

    private static Task<GeneratorDriverRunResult> VerifyWithProducerAsync(
          string source
        , params DiagnosticResult[] expected
    )
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] { ProducerFixtures.StatsApiProducer, new NamedSource("Test0.cs", source) }
            , new IIncrementalGenerator[] { new StatSystemGenerator(), new TypeHandleGenerator() }
            , new DiagnosticAnalyzer[] { new TypeHandleDiagnosticAnalyzer() }
            , expected
        );

    private static DiagnosticResult D(
          int startLine
        , int startColumn
        , int endLine
        , int endColumn
        , params object[] arguments
    )
        => new DiagnosticResult(TypeHandleDiagnosticAnalyzer.GeneratedTypeNotSupported)
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
