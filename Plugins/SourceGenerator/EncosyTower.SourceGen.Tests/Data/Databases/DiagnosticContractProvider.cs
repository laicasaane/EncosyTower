using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Data.Databases;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Data/Databases";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Data.Analyzers.Databases.DatabaseDiagnosticAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Data.Analyzers.Databases.DatabaseDiagnosticAnalyzer"
            , "SG_DATABASE_HORIZONTAL_0001"
            , "Not a typeof expression"
            , "The first argument must be a 'typeof' expression"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The first argument must be a 'typeof' expression."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Data.Analyzers.Databases.DatabaseDiagnosticAnalyzer"
            , "SG_DATABASE_HORIZONTAL_0002"
            , "Abstract type is not supported"
            , "The type \"{0}\" must not be abstract"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The type must not be abstract to be considered a valid target type."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Data.Analyzers.Databases.DatabaseDiagnosticAnalyzer"
            , "SG_DATABASE_HORIZONTAL_0003"
            , "Target type does not implement IData"
            , "The type \"{0}\" must implement IData interface"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The type must implement IData interface to be considered a valid target type."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Data.Analyzers.Databases.DatabaseDiagnosticAnalyzer"
            , "SG_DATABASE_HORIZONTAL_0004"
            , "Invalid property name"
            , "The property name must be a valid identifier"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The property name must be a valid identifier."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Data.Analyzers.Databases.DatabaseDiagnosticAnalyzer"
            , "SG_DATABASE_TABLE_0001"
            , "Abstract type is not supported"
            , "The type \"{0}\" must not be abstract"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The type must not be abstract to be considered a valid table."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Data.Analyzers.Databases.DatabaseDiagnosticAnalyzer"
            , "SG_DATABASE_TABLE_0002"
            , "Generic type is not supported"
            , "The type \"{0}\" must not be generic"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The type must not be generic to be considered a valid table."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Data.Analyzers.Databases.DatabaseDiagnosticAnalyzer"
            , "SG_DATABASE_TABLE_0003"
            , "Type must be derived from either DataTableAsset<TDataId, TData> or DataTableAsset<TDataId, TData, " +
              "TConvertedId>"
            , "The type \"{0}\" must be derived from either DataTableAsset<TDataId, TData> or " +
              "DataTableAsset<TDataId, TData, TConvertedId>"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The type must be derived from either DataTableAsset<TDataId, TData> or DataTableAsset<TDataId, " +
              "TData, " +
              "TConvertedId>."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
