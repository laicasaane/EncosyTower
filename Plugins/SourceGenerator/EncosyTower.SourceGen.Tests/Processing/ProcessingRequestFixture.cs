using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using EncosyTower.Processing.Generators;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Tests.Processing;

internal static class ProcessingRequestFixture
{
    private const string ASSEMBLY_NAME = "EncosyTower.SourceGen.Tests.Processing.Input";

    private static readonly CSharpParseOptions s_parseOptions = CSharpParseOptions.Default
        .WithLanguageVersion(LanguageVersion.CSharp10);

    internal static async Task<ProcessingRun> RunAsync(
        IReadOnlyList<NamedSource> sources,
        [AllowNull] ProcessingRun previous,
        CancellationToken token = default
    )
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var allSources = new List<NamedSource>(sources.Count + 3) {
            new("ProcessingAttribute.cs", ProcessingRuntimeFixture.MarkerSource),
            new("ProcessingRuntime.cs", ProcessingRuntimeFixture.RuntimeSource),
            new("ProcessingScopes.cs", ProcessingRuntimeFixture.ScopeSource),
        };
        allSources.AddRange(sources);
        var compilation = CreateCompilation(allSources, references, previous?.InputCompilation, token);
        var driver = previous?.Driver ?? CSharpGeneratorDriver.Create(
              new[] { new ProcessingRequestGenerator().AsSourceGenerator() }
            , parseOptions: s_parseOptions
            , driverOptions: new GeneratorDriverOptions(
                  disabledOutputs: default
                , trackIncrementalGeneratorSteps: true
            )
        );
        driver = driver.RunGeneratorsAndUpdateCompilation(
              compilation
            , out var outputCompilation
            , out var diagnostics
            , token
        );
        var result = driver.GetRunResult();

        Assert.AreEqual(
            0,
            diagnostics.Length,
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()))
        );
        Assert.AreEqual(1, result.Results.Length);
        Assert.IsNull(result.Results[0].Exception);
        Assert.AreEqual(0, result.Results[0].Diagnostics.Length);

        return new ProcessingRun(driver, compilation, outputCompilation, result.Results[0]);
    }

    internal static Task<ProcessingRun> RunAsync(string source, CancellationToken token = default)
        => RunAsync([new NamedSource("Request.cs", source)], previous: null, token);

    internal static Task<ProcessingRun> RunAsync(
        IReadOnlyList<NamedSource> sources,
        CancellationToken token = default
    )
        => RunAsync(sources, previous: null, token);

    internal static void AssertNoOutputErrors(ProcessingRun run)
    {
        var errors = run.OutputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(
            0,
            errors.Length,
            string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString()))
        );
    }

    private static CSharpCompilation CreateCompilation(
        IReadOnlyList<NamedSource> sources,
        ImmutableArray<MetadataReference> references,
        [AllowNull] Compilation previousCompilation,
        CancellationToken token
    )
    {
        var previousTrees = previousCompilation?.SyntaxTrees.ToDictionary(
              static tree => tree.FilePath
            , StringComparer.Ordinal
        ) ?? new Dictionary<string, SyntaxTree>(StringComparer.Ordinal);
        var trees = new List<SyntaxTree>(sources.Count);

        foreach (var source in sources)
        {
            var text = SourceText.From(source.Source, Encoding.UTF8);

            if (previousTrees.TryGetValue(source.Path, out var previousTree)
                && previousTree.GetText(token).ContentEquals(text)
            )
            {
                trees.Add(previousTree);
            }
            else
            {
                trees.Add(CSharpSyntaxTree.ParseText(text, s_parseOptions, source.Path, token));
            }
        }

        return CSharpCompilation.Create(
              ASSEMBLY_NAME
            , trees
            , references
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
        );
    }
}

internal sealed record ProcessingRun(
    GeneratorDriver Driver,
    Compilation InputCompilation,
    Compilation OutputCompilation,
    GeneratorRunResult Result
)
{
    internal IReadOnlyList<GeneratedSourceResult> Sources => Result.GeneratedSources;

    internal string CombinedSource
        => string.Join(
            Environment.NewLine,
            Sources.OrderBy(static source => source.HintName, StringComparer.Ordinal)
                .Select(static source => source.SourceText.ToString())
        );
}
