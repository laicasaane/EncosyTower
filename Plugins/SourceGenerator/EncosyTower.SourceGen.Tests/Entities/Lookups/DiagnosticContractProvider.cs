using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Entities.Lookups;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Entities/Lookups";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Entities.Analyzers.Entities.Lookups.LookupDiagnosticAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Analyzers.Entities.Lookups.LookupDiagnosticAnalyzer"
            , "SG_LOOKUPS_0001"
            , "Not a typeof expression"
            , "The first argument must be a 'typeof' expression"
            , "Lookups"
            , DiagnosticSeverity.Error
            , true
            , "The first argument must be a 'typeof' expression."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Analyzers.Entities.Lookups.LookupDiagnosticAnalyzer"
            , "SG_LOOKUPS_0002"
            , "Open generic type is not supported"
            , "The type \"{0}\" must not be open generic"
            , "Lookups"
            , DiagnosticSeverity.Error
            , true
            , "The type must not be open generic."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Analyzers.Entities.Lookups.LookupDiagnosticAnalyzer"
            , "SG_LOOKUPS_0003"
            , "Type that is managed or contains managed reference is not supported"
            , "The type \"{0}\" must be unmanaged"
            , "Lookups"
            , DiagnosticSeverity.Error
            , true
            , "The type must be unmanaged."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Analyzers.Entities.Lookups.LookupDiagnosticAnalyzer"
            , "SG_LOOKUPS_0004"
            , "Type must implement compatible interface"
            , "The type \"{0}\" must implement interface \"{1}\" to be compatible"
            , "Lookups"
            , DiagnosticSeverity.Error
            , true
            , "The type must implement compatible interface."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Analyzers.Entities.Lookups.LookupDiagnosticAnalyzer"
            , "SG_LOOKUPS_0005"
            , "Missing lookup marker interface"
            , "Struct \"{0}\" must implement exactly one lookup marker interface"
            , "Lookups"
            , DiagnosticSeverity.Error
            , true
            , "The struct must implement exactly one of the lookup marker interfaces (e.g. IBufferLookups, " +
              "IComponentLookups)."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Analyzers.Entities.Lookups.LookupDiagnosticAnalyzer"
            , "SG_LOOKUPS_0006"
            , "Multiple lookup marker interfaces are not supported"
            , "Struct \"{0}\" implements {1} lookup marker interfaces; only one is allowed"
            , "Lookups"
            , DiagnosticSeverity.Error
            , true
            , "The struct must implement exactly one lookup marker interface. Implementing two or more is not " +
              "supported."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
