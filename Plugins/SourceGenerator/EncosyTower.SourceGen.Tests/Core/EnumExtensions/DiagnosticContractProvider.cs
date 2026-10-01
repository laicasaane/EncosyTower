using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.EnumExtensions;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Core/EnumExtensions";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Core.Analyzers.EnumExtensions.EnumExtensionsAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.EnumExtensions.EnumExtensionsAnalyzer"
            , "SG_ENUM_EXT_FOR_0001"
            , "Type argument of [EnumExtensionsFor] must be an enum type"
            , "\"{0}\" is not an enum type. The typeof argument of [EnumExtensionsFor] must resolve to an enum."
            , "EnumExtensionsForGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The type passed to [EnumExtensionsFor(typeof(...))] must be an enum type."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
