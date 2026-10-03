using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests;

internal static class GeneratorAnalyzerTestHelper
{
    internal const string INPUT_ASSEMBLY_NAME = "EncosyTower.SourceGen.Tests.Input";
    internal const string INPUT_PATH = "Test0.cs";

    internal static Task<GeneratorDriverRunResult> VerifyAsync(
          string source
        , IReadOnlyList<IIncrementalGenerator> generators
        , IReadOnlyList<DiagnosticAnalyzer> analyzers
        , IReadOnlyList<DiagnosticResult> expectedDiagnostics
        , IEnumerable<MetadataReference>? additionalReferences = null
        , CancellationToken token = default
    )
        => VerifyAsync(
              new[] { new NamedSource(INPUT_PATH, source) }
            , generators
            , analyzers
            , expectedDiagnostics
            , additionalReferences
            , token
        );

    internal static async Task<GeneratorDriverRunResult> VerifyAsync(
          IReadOnlyList<NamedSource> sources
        , IReadOnlyList<IIncrementalGenerator> generators
        , IReadOnlyList<DiagnosticAnalyzer> analyzers
        , IReadOnlyList<DiagnosticResult> expectedDiagnostics
        , IEnumerable<MetadataReference>? additionalReferences = null
        , CancellationToken token = default
    )
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);

        if (additionalReferences != null)
        {
            references = references.AddRange(additionalReferences);
        }

        var compilation = GeneratorTestHelper.CreateCompilation(sources, INPUT_ASSEMBLY_NAME, references, token);
        var driver = GeneratorTestHelper.CreateDriver(generators).RunGeneratorsAndUpdateCompilation(
              compilation
            , out var outputCompilation
            , out var driverDiagnostics
            , token
        );

        var result = driver.GetRunResult();

        GeneratorTestHelper.AssertNoGeneratorFailures(driverDiagnostics, result);

        var options = new CompilationWithAnalyzersOptions(
              new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty)
            , onAnalyzerException: null
            , concurrentAnalysis: false
            , logAnalyzerExecutionTime: false
            , reportSuppressedDiagnostics: false
        );

        var diagnostics = await outputCompilation
            .WithAnalyzers(analyzers.ToImmutableArray(), options)
            .GetAllDiagnosticsAsync(token);

        var actual = diagnostics
            .Where(static diagnostic => diagnostic.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error)
            .Select(static diagnostic => (Diagnostic: diagnostic, Span: diagnostic.Location.GetLineSpan()))
            .OrderBy(static pair => pair.Span.Path, StringComparer.Ordinal)
            .ThenBy(static pair => pair.Span.StartLinePosition.Line)
            .ThenBy(static pair => pair.Span.StartLinePosition.Character)
            .ThenBy(static pair => pair.Diagnostic.Id, StringComparer.Ordinal)
            .ToArray();

        var expected = expectedDiagnostics
            .Select(static result => (Result: result, Span: GetExpectedSpan(result)))
            .OrderBy(static pair => pair.Span.Path, StringComparer.Ordinal)
            .ThenBy(static pair => pair.Span.StartLinePosition.Line)
            .ThenBy(static pair => pair.Span.StartLinePosition.Character)
            .ThenBy(static pair => pair.Result.Id, StringComparer.Ordinal)
            .ToArray();

        if (actual.Length != expected.Length)
        {
            Assert.Fail(
                $"Expected {expected.Length} diagnostic(s) but found {actual.Length}:\n"
                    + string.Join("\n", actual.Select(static pair => pair.Diagnostic.ToString()))
            );
        }

        var count = actual.Length;

        for (var i = 0; i < count; i++)
        {
            var (diagnostic, actualSpan) = actual[i];
            var (expectedResult, expectedSpan) = expected[i];
            var description = diagnostic.ToString();

            Assert.AreEqual(expectedResult.Id, diagnostic.Id, description);
            Assert.AreEqual(expectedResult.Severity, diagnostic.Severity, description);
            Assert.AreEqual(expectedSpan.Path, actualSpan.Path, description);
            Assert.AreEqual(expectedSpan.StartLinePosition, actualSpan.StartLinePosition, description);
            Assert.AreEqual(expectedSpan.EndLinePosition, actualSpan.EndLinePosition, description);

            if (expectedResult.MessageArguments == null)
            {
                continue;
            }

            var format = diagnostic.Descriptor.MessageFormat.ToString(CultureInfo.InvariantCulture);
            var expectedMessage = string.Format(CultureInfo.InvariantCulture, format, expectedResult.MessageArguments);

            Assert.AreEqual(expectedMessage, diagnostic.GetMessage(CultureInfo.InvariantCulture), description);
        }

        return result;
    }

    private static FileLinePositionSpan GetExpectedSpan(DiagnosticResult result)
    {
        if (result.Spans.Length != 1 || string.IsNullOrEmpty(result.Spans[0].Span.Path))
        {
            Assert.Fail(
                $"Expected diagnostic {result.Id} must carry exactly one WithSpan(path, ...) location; "
                    + "markup locations are not supported."
            );
        }

        return result.Spans[0].Span;
    }
}
