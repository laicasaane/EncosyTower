using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Tests;

internal sealed class ContractIncrementalGenerator : IIncrementalGenerator
{
    internal const string CANDIDATES_TRACKING_NAME = "ContractFixture.Candidates";
    internal const string VALID_SPECS_TRACKING_NAME = "ContractFixture.ValidSpecs";
    internal const string OUTPUTS_TRACKING_NAME = "ContractFixture.Outputs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidates = context.SyntaxProvider.CreateSyntaxProvider(
              static (node, _) => node is ClassDeclarationSyntax declaration
                && declaration.Identifier.ValueText.StartsWith("Contract", StringComparison.Ordinal)
            , static (syntaxContext, _) =>
                ((ClassDeclarationSyntax)syntaxContext.Node).Identifier.ValueText
        ).WithTrackingName(CANDIDATES_TRACKING_NAME);
        var validSpecs = candidates
            .Where(static name => name.Length > "Contract".Length)
            .WithTrackingName(VALID_SPECS_TRACKING_NAME);
        var outputs = validSpecs
            .Select(static (name, _) => new ContractOutput($"{name}.g.cs", RenderSource(name)))
            .WithTrackingName(OUTPUTS_TRACKING_NAME);

        context.RegisterSourceOutput(
            outputs,
            static (productionContext, output) => productionContext.AddSource(
                  output.HintName
                , SourceText.From(output.Source, Encoding.UTF8)
            )
        );
    }

    private static string RenderSource(string name)
        => $$"""
            namespace GeneratorContractFixture;

            internal static class {{name}}Generated
            {
                internal const string Value = "{{name}}";
            }
            """;

    private readonly record struct ContractOutput(string HintName, string Source);
}

internal sealed class ThrowingIncrementalGenerator : IIncrementalGenerator
{
    internal const string EXCEPTION_MESSAGE = "Deterministic generator fixture fault.";

    public void Initialize(IncrementalGeneratorInitializationContext context)
        => context.RegisterSourceOutput(
            context.CompilationProvider,
            static (_, _) => throw new InvalidOperationException(EXCEPTION_MESSAGE)
        );
}

internal sealed class DeterministicCancellationIncrementalGenerator : IIncrementalGenerator
{
    private readonly CancellationTokenSource _cancellationSource;

    internal DeterministicCancellationIncrementalGenerator(CancellationTokenSource cancellationSource)
    {
        _cancellationSource = cancellationSource;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classNames = context.SyntaxProvider.CreateSyntaxProvider(
              static (node, _) => node is ClassDeclarationSyntax declaration
                && declaration.Identifier.ValueText is
                    "GenerateBeforeCancellation" or "CancelGeneration"
            , static (syntaxContext, _) =>
                ((ClassDeclarationSyntax)syntaxContext.Node).Identifier.ValueText
        );

        context.RegisterSourceOutput(classNames, ProduceSourceOrCancel);
    }

    private void ProduceSourceOrCancel(SourceProductionContext context, string className)
    {
        if (string.Equals(className, "CancelGeneration", StringComparison.Ordinal))
        {
            _cancellationSource.Cancel();
            context.CancellationToken.ThrowIfCancellationRequested();
        }

        context.AddSource(
              $"{className}.g.cs"
            , SourceText.From("internal static class GeneratedBeforeCancellation { }\n", Encoding.UTF8)
        );
    }
}
