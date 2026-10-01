using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Entities;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Entities";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Entities.CodeRefactors.ISystemDiagnosticAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.CodeRefactors.ISystemDiagnosticAnalyzer"
            , "SG_ISYSTEM_0001"
            , "ISystem type can be completed"
            , "Type '{0}' is annotated with [ISystem] and may need ISystem members"
            , "ISystemDiagnosticAnalyzer"
            , DiagnosticSeverity.Hidden
            , true
            , "Type annotated with [ISystem] can be completed with lifecycle members."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
