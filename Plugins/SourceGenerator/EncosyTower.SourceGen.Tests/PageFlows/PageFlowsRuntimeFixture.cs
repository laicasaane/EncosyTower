using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using EncosyTower.PageFlows.Generators;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Tests.PageFlows;

internal static class PageFlowsRuntimeFixture
{
    private const string ASSEMBLY_NAME = "EncosyTower.SourceGen.Tests.PageFlows.Input";

    private static readonly CSharpParseOptions s_parseOptions = CSharpParseOptions.Default
        .WithLanguageVersion(LanguageVersion.CSharp10);

    internal const string MarkerSource = """
        namespace EncosyTower.PageFlows
        {
            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class PageFlowScopeCollectionAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class SkipSourceGeneratorsForAssemblyAttribute : System.Attribute { }

            public readonly record struct PageFlowScope(int Value);

            public interface IPageFlowScopeCollection
            {
                System.ReadOnlyMemory<string> ScopeIdentifiers { get; }

                bool TrySetScope(string identifier, PageFlowScope scope);
            }
        }
        """;

    internal static Task<PageFlowsRun> RunAsync(string source, CancellationToken token = default)
        => RunAsync([new NamedSource("Scopes.cs", source)], previous: null, token);

    internal static async Task<PageFlowsRun> RunAsync(
          IReadOnlyList<NamedSource> sources
        , [AllowNull] PageFlowsRun previous
        , CancellationToken token = default
    )
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var compilation = CreateCompilation(sources, references, previous?.InputCompilation, token);
        var driver = previous?.Driver ?? CSharpGeneratorDriver.Create(
              new[] { new PageFlowScopeCollectionGenerator().AsSourceGenerator() }
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
              0
            , diagnostics.Length
            , string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()))
        );
        Assert.AreEqual(1, result.Results.Length);
        Assert.IsNull(result.Results[0].Exception);
        Assert.AreEqual(0, result.Results[0].Diagnostics.Length);
        return new PageFlowsRun(driver, compilation, outputCompilation, result.Results[0]);
    }

    internal static void AssertNoOutputErrors(PageFlowsRun run)
    {
        var errors = run.OutputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(
              0
            , errors.Length
            , string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString()))
        );
    }

    internal static object? InvokeStatic(PageFlowsRun run, string typeName, string methodName)
    {
        using var stream = new MemoryStream();
        var emitResult = run.OutputCompilation.Emit(stream);

        Assert.IsTrue(
              emitResult.Success
            , string.Join(Environment.NewLine, emitResult.Diagnostics.Select(static item => item.ToString()))
        );

        var context = new AssemblyLoadContext(ASSEMBLY_NAME, isCollectible: true);

        try
        {
            stream.Position = 0;
            var assembly = context.LoadFromStream(stream);
            var method = assembly.GetType(typeName, throwOnError: true)!
                .GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)!;

            return method.Invoke(obj: null, parameters: null);
        }
        finally
        {
            context.Unload();
        }
    }

    private static CSharpCompilation CreateCompilation(
          IReadOnlyList<NamedSource> sources
        , ImmutableArray<MetadataReference> references
        , [AllowNull] Compilation previousCompilation
        , CancellationToken token
    )
    {
        var previousTrees = previousCompilation?.SyntaxTrees.ToDictionary(
              static tree => tree.FilePath
            , StringComparer.Ordinal
        ) ?? new Dictionary<string, SyntaxTree>(StringComparer.Ordinal);

        var allSources = new List<NamedSource>(sources.Count + 1) { new("PageFlowsMarkers.cs", MarkerSource) };
        allSources.AddRange(sources);

        var trees = new List<SyntaxTree>(allSources.Count);

        foreach (var source in allSources)
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
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }
}

internal sealed record PageFlowsRun(
      GeneratorDriver Driver
    , Compilation InputCompilation
    , Compilation OutputCompilation
    , GeneratorRunResult Result
)
{
    internal IReadOnlyList<GeneratedSourceResult> Sources => Result.GeneratedSources;
}
