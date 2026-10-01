using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests;

internal static class SuppressorTestHelper
{
    internal static Task VerifyAsync<TSuppressor>(
          string source
        , bool isSuppressed
        , CancellationToken token = default
    )
        where TSuppressor : DiagnosticSuppressor, new()
    {
        var test = new CSharpAnalyzerTest<TSuppressor, DefaultVerifier> {
            TestCode = source,
            ReferenceAssemblies = TestReferenceHelper.FrameworkReferences,
            CompilerDiagnostics = CompilerDiagnostics.Warnings,
        };

        test.TestState.AdditionalReferences.AddRange(TestReferenceHelper.RuntimeReferences);
        test.SolutionTransforms.Add((solution, projectId) =>
            solution.WithProjectParseOptions(
                projectId,
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10)
            )
        );
        test.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerWarning("CS0657").WithLocation(0).WithIsSuppressed(isSuppressed)
        );

        return test.RunAsync(token);
    }
}
