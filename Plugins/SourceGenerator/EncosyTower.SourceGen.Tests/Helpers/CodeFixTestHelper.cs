using System.Collections.Immutable;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;

namespace EncosyTower.SourceGen.Tests;

internal static class CodeFixTestHelper
{
    internal readonly record struct ActionContract(string Title, string? EquivalenceKey);

    internal static Task VerifyAsync<TAnalyzer, TCodeFix>(
          string source
        , string fixedSource
        , string equivalenceKey
        , IEnumerable<MetadataReference>? runtimeReferences = null
        , string featureLocalStubSource = ""
        , IEnumerable<DiagnosticResult>? fixedDiagnostics = null
        , CancellationToken token = default
    )
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var analyzer = new TAnalyzer();
        var diagnosticOptions = analyzer.SupportedDiagnostics.ToImmutableDictionary(
              static descriptor => descriptor.Id
            , static descriptor => ToReportDiagnostic(descriptor.DefaultSeverity)
        );
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> {
            ReferenceAssemblies = TestReferenceHelper.FrameworkReferences,
            TestCode = source,
            FixedCode = fixedSource,
            CodeActionEquivalenceKey = equivalenceKey,
            NumberOfIncrementalIterations = 1,
            NumberOfFixAllInDocumentIterations = 1,
            NumberOfFixAllInProjectIterations = 1,
        };

        test.TestState.AdditionalReferences.AddRange(runtimeReferences ?? TestReferenceHelper.RuntimeReferences);
        test.OptionsTransforms.Add(static options => options.WithChangedOption(
              FormattingOptions.NewLine
            , LanguageNames.CSharp
            , "\n"
        ));

        if (string.IsNullOrEmpty(featureLocalStubSource) == false)
        {
            test.TestState.Sources.Add(("FeatureLocalStub.cs", featureLocalStubSource));
            test.FixedState.Sources.Add(("FeatureLocalStub.cs", featureLocalStubSource));
        }

        if (fixedDiagnostics is not null)
        {
            test.FixedState.ExpectedDiagnostics.AddRange(fixedDiagnostics);
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

    internal static async Task<ImmutableArray<ActionContract>> GetActionsAsync<TAnalyzer, TCodeFix>(
          string source
        , CancellationToken token = default
    )
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        using var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var solution = workspace.CurrentSolution
            .AddProject(ProjectInfo.Create(
                  projectId
                , VersionStamp.Default
                , "CodeFixActions"
                , "CodeFixActions"
                , LanguageNames.CSharp
                , parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10)
                , compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            ))
            .AddMetadataReferences(projectId, references)
            .AddDocument(documentId, "Test0.cs", source);
        var document = solution.GetDocument(documentId)
            ?? throw new InvalidOperationException("Code-fix action document is unavailable.");
        var compilation = await document.Project.GetCompilationAsync(token)
            ?? throw new InvalidOperationException("Code-fix action compilation is unavailable.");
        var analyzer = new TAnalyzer();
        var diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer))
            .GetAnalyzerDiagnosticsAsync(token);
        var provider = new TCodeFix();
        var diagnostic = diagnostics.Single(diagnostic =>
            provider.FixableDiagnosticIds.Contains(diagnostic.Id)
        );
        var actions = ImmutableArray.CreateBuilder<ActionContract>();
        var context = new CodeFixContext(
              document
            , diagnostic
            , (action, _) => actions.Add(new ActionContract(action.Title, action.EquivalenceKey))
            , token
        );

        await provider.RegisterCodeFixesAsync(context);
        return actions.ToImmutable();
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
