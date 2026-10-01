using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class NoOpDiagnosticAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray<DiagnosticDescriptor>.Empty;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
