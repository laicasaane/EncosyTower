using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.Types.Caches;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Core/Types/Caches";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Core.Analyzers.Types.Caches.RuntimeTypeCachesAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.Types.Caches.RuntimeTypeCachesAnalyzer"
            , "SG_RUNTIME_TYPE_CACHES_0001"
            , "Type parameter is not applicable"
            , "\"{0}\" is a type parameter thus it is not applicable for the \"{1}\" method"
            , "RuntimeTypeCachesGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Type parameter is not applicable."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.Types.Caches.RuntimeTypeCachesAnalyzer"
            , "SG_RUNTIME_TYPE_CACHES_0002"
            , "Only class or interface is applicable"
            , "\"{0}\" is not applicable because it is not class nor interface"
            , "RuntimeTypeCachesGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Only class or interface is applicable."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.Types.Caches.RuntimeTypeCachesAnalyzer"
            , "SG_RUNTIME_TYPE_CACHES_0003"
            , "Static class is not applicable"
            , "\"{0}\" is not applicable because it is static"
            , "RuntimeTypeCachesGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Static class is not applicable."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.Types.Caches.RuntimeTypeCachesAnalyzer"
            , "SG_RUNTIME_TYPE_CACHES_0004"
            , "Sealed class is not applicable"
            , "\"{0}\" is not applicable because it is sealed"
            , "RuntimeTypeCachesGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Sealed class is not applicable."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.Types.Caches.RuntimeTypeCachesAnalyzer"
            , "SG_RUNTIME_TYPE_CACHES_0005"
            , "Assembly name must be a string literal or constant"
            , "Assembly name must be a string literal or constant"
            , "RuntimeTypeCachesGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Assembly name must be a string literal or constant."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.Types.Caches.RuntimeTypeCachesAnalyzer"
            , "SG_RUNTIME_TYPE_CACHES_0006"
            , "Types from \"EncosyTower.Types.Caches\" are prohibited"
            , "\"{0}\" is prohibited"
            , "RuntimeTypeCachesGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Types from \"EncosyTower.Types.Caches\" are prohibited."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
