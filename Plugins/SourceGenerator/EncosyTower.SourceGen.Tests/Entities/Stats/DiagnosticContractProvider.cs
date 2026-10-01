using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Entities/Stats";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Entities.Stats.Analyzers.StatCollectionDiagnosticAnalyzer),
        typeof(global::EncosyTower.Entities.Stats.Analyzers.StatDataDiagnosticAnalyzer),
        typeof(global::EncosyTower.Entities.Stats.Analyzers.StatSystemDiagnosticAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Stats.Analyzers.StatCollectionDiagnosticAnalyzer"
            , "SG_STAT_COLLECTION_0001"
            , "Type argument of [StatCollection] must have [StatSystem]"
            , "\"{0}\" does not have the [StatSystem] attribute. The typeof argument of [StatCollection] must " +
              "resolve to a type attributed with [StatSystem]."
            , "StatCollectionGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The type passed to [StatCollection(typeof(...))] must be attributed with [StatSystem]."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Stats.Analyzers.StatCollectionDiagnosticAnalyzer"
            , "SG_STAT_COLLECTION_0002"
            , "typeIdOffset + StatData count exceeds uint.MaxValue"
            , "The combination of typeIdOffset ({0}) and the number of [StatData] members ({1}) in \"{2}\" exceeds " +
              "uint.MaxValue. Reduce typeIdOffset or the number of [StatData] members."
            , "StatCollectionGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The combination of typeIdOffset and the number of [StatData] nested structs must not exceed " +
              "uint.MaxValue."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Stats.Analyzers.StatCollectionDiagnosticAnalyzer"
            , "SG_STAT_COLLECTION_0003"
            , "[StatCollection] can only be applied to a struct"
            , "\"{0}\" is not a struct. [StatCollection] can only be applied to struct types."
            , "StatCollectionGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[StatCollection] can only be applied to struct types."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Stats.Analyzers.StatCollectionDiagnosticAnalyzer"
            , "SG_STAT_COLLECTION_0004"
            , "[StatCollection] cannot be applied to an non-generic struct"
            , "\"{0}\" is a generic type. [StatCollection] can only be applied to non-generic structs."
            , "StatCollectionGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[StatCollection] can only be applied to non-generic struct types."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Stats.Analyzers.StatDataDiagnosticAnalyzer"
            , "SG_STAT_DATA_0001"
            , "[StatData] can only be applied to a struct"
            , "\"{0}\" is not a struct. [StatData] can only be applied to struct types."
            , "StatDataGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[StatData] can only be applied to struct types."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Stats.Analyzers.StatDataDiagnosticAnalyzer"
            , "SG_STAT_DATA_0002"
            , "[StatData] cannot be applied to a generic struct"
            , "\"{0}\" is a generic type. [StatData] can only be applied to non-generic structs."
            , "StatDataGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[StatData] can only be applied to non-generic struct types."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Stats.Analyzers.StatDataDiagnosticAnalyzer"
            , "SG_STAT_DATA_0003"
            , "StatVariantType.None is not a valid argument for [StatData]"
            , "The [StatData] attribute on \"{0}\" uses StatVariantType.None, which is not a valid variant type " +
              "and " +
              "will produce no output."
            , "StatDataGenerator"
            , DiagnosticSeverity.Error
            , true
            , "StatVariantType.None is reserved and cannot be used as the type argument for [StatData]. Use a " +
              "concrete StatVariantType value instead."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Stats.Analyzers.StatDataDiagnosticAnalyzer"
            , "SG_STAT_DATA_0004"
            , "typeof argument of [StatData] must be an enum type"
            , "\"{0}\" is not an enum. The typeof argument of [StatData] must resolve to an enum type."
            , "StatDataGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The type passed to [StatData(typeof(...))] must be an enum type."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Entities.Stats.Analyzers.StatSystemDiagnosticAnalyzer"
            , "SG_STAT_SYSTEM_0001"
            , "[StatSystem] cannot be applied to a generic type"
            , "\"{0}\" is a generic type. [StatSystem] can only be applied to non-generic types."
            , "StatSystemGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[StatSystem] can only be applied to non-generic types."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
