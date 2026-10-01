using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Data.DataTableAssets;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Data/DataTableAssets";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Data.Analyzers.DataTableAssets.DataTableAssetAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Data.Analyzers.DataTableAssets.DataTableAssetAnalyzer"
            , "SG_DATA_TABLE_ASSET_0001"
            , "Must be either a struct, a class or an enum to replace type argument"
            , "Type \"{0}\" is not applicable to replace \"{1}\", must be either a struct, a class or an enum"
            , "DataTableAssetGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Must be either a struct, a class or an enum."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
