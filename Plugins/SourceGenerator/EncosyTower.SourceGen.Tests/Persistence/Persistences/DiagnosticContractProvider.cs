using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Persistence.Persistences;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Persistence/Persistences";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Persistence.Analyzers.PersistenceDiagnosticAnalyzer),
        typeof(
            global::EncosyTower.Persistence.Analyzers
                .PersistFieldAttributeWithTargetsDiagnosticSuppressor
        ),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Persistence.Analyzers.PersistenceDiagnosticAnalyzer"
            , "SG_PERSISTENCE_0001"
            , "Type must have only 01 constructor whose parameters are either PersistStoreBase<T> or " +
              "IPersistAccessor"
            , "Type \"{0}\" must have only 01 constructor whose parameters are either PersistStoreBase<T> or " +
              "IPersistAccessor"
            , "Persistence"
            , DiagnosticSeverity.Warning
            , true
            , "Type must have only 01 constructor whose parameters are either PersistStoreBase<T> or " +
              "IPersistAccessor"
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Persistence.Analyzers.PersistenceDiagnosticAnalyzer"
            , "SG_PERSISTENCE_0002"
            , "Constructor parameter must be either PersistStoreBase<T> or IPersistAccessor"
            , "Parameter \"{1}\" of constructor of type \"{0}\" must be either PersistStoreBase<T> or " +
              "IPersistAccessor"
            , "Persistence"
            , DiagnosticSeverity.Warning
            , true
            , "Constructor parameter must be either PersistStoreBase<T> or IPersistAccessor"
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Persistence.Analyzers.PersistenceDiagnosticAnalyzer"
            , "SG_PERSISTENCE_0003"
            , "[PersistAccessor] type must not be abstract"
            , "Type \"{0}\" is marked [PersistAccessor] but is abstract and cannot be processed by the source " +
              "generator"
            , "Persistence"
            , DiagnosticSeverity.Warning
            , true
            , "A type marked with [PersistAccessor] must not be abstract"
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Persistence.Analyzers.PersistenceDiagnosticAnalyzer"
            , "SG_PERSISTENCE_0005"
            , "[PersistAccessor] type must not be a generic type"
            , "Type \"{0}\" is marked [PersistAccessor] but is a generic type and cannot be processed by the " +
              "source " +
              "generator"
            , "Persistence"
            , DiagnosticSeverity.Warning
            , true
            , "A type marked with [PersistAccessor] must not be a generic type"
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions { get; } = new[] {
        new SuppressionDescriptorContract(
              "EncosyTower.Persistence.Analyzers.PersistFieldAttributeWithTargetsDiagnosticSuppressor"
            , "SG_PERSIST_SUPPRESS_0001"
            , "CS0657"
            , "Fields named 'id', '_id', 'm_id', 'version', '_version', 'm_version' can use "
              + "[property:] attribute lists to forward attributes to the generated properties"
        ),
    };
}
