namespace EncosyTower.SourceGen.Tests.Diagnostics;

internal interface IDiagnosticContractProvider
{
    string FeaturePath { get; }

    IReadOnlyList<Type> ComponentTypes { get; }

    IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; }

    IReadOnlyList<SuppressionDescriptorContract> Suppressions { get; }
}
