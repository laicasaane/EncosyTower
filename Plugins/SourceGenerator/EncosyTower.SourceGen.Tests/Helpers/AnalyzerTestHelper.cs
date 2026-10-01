using System.Collections.Immutable;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests;

internal static class AnalyzerTestHelper
{
    internal static Task VerifyAsync<TAnalyzer>(
          string source
        , IReadOnlyList<DiagnosticResult>? expectedDiagnostics = null
        , IEnumerable<MetadataReference>? runtimeReferences = null
        , string featureLocalStubSource = ""
        , CancellationToken token = default
    )
        where TAnalyzer : DiagnosticAnalyzer, new()
        => VerifyAsync<TAnalyzer>(
              new[] { new NamedSource("Test0.cs", source) }
            , expectedDiagnostics
            , runtimeReferences
            , string.IsNullOrEmpty(featureLocalStubSource)
                ? null
                : new[] { new NamedSource("FeatureLocalStub.cs", featureLocalStubSource) }
            , token
        );

    internal static Task VerifyAsync<TAnalyzer>(
          IReadOnlyList<NamedSource> sources
        , IReadOnlyList<DiagnosticResult>? expectedDiagnostics = null
        , IEnumerable<MetadataReference>? runtimeReferences = null
        , IReadOnlyList<NamedSource>? featureLocalStubSources = null
        , CancellationToken token = default
    )
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var analyzer = new TAnalyzer();
        var diagnosticOptions = analyzer.SupportedDiagnostics.ToImmutableDictionary(
              static descriptor => descriptor.Id
            , static descriptor => ToReportDiagnostic(descriptor.DefaultSeverity)
        );
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> {
            ReferenceAssemblies = TestReferenceHelper.FrameworkReferences,
        };

        AddSources(test.TestState.Sources, sources);
        test.TestState.AdditionalReferences.AddRange(runtimeReferences ?? TestReferenceHelper.RuntimeReferences);

        if (featureLocalStubSources is not null)
        {
            AddSources(test.TestState.Sources, featureLocalStubSources);
        }

        if (expectedDiagnostics is not null)
        {
            test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        }

        test.SolutionTransforms.Add((solution, projectId) => {
            solution = solution.WithProjectParseOptions(
                projectId,
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10)
            );
            var options = solution.GetProject(projectId)?.CompilationOptions
                ?? throw new InvalidOperationException("Compilation options are unavailable.");
            return solution.WithProjectCompilationOptions(
                projectId,
                options.WithSpecificDiagnosticOptions(diagnosticOptions)
            );
        });

        return test.RunAsync(token);
    }

    private static void AddSources(SourceFileList destination, IReadOnlyList<NamedSource> sources)
    {
        foreach (var source in sources)
        {
            destination.Add((source.Path, source.Source));
        }
    }

    private static ReportDiagnostic ToReportDiagnostic(DiagnosticSeverity severity)
        => severity switch {
            DiagnosticSeverity.Hidden => ReportDiagnostic.Hidden,
            DiagnosticSeverity.Info => ReportDiagnostic.Info,
            DiagnosticSeverity.Warning => ReportDiagnostic.Warn,
            DiagnosticSeverity.Error => ReportDiagnostic.Error,
            _ => throw new InvalidEnumArgumentException(nameof(severity), (int)severity, typeof(DiagnosticSeverity)),
        };
}
