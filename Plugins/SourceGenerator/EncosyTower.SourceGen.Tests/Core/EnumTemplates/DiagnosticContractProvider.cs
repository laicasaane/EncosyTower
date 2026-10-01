using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.EnumTemplates;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Core/EnumTemplates";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Core.Analyzers.EnumTemplates.EnumTemplateAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.EnumTemplates.EnumTemplateAnalyzer"
            , "SG_ENUM_TEMPLATE_0001"
            , "Not end with template suffix"
            , "The name of a union enum template must end with either \"_EnumTemplate\" or \"_Template\" suffix"
            , "EnumTemplateGenerator"
            , DiagnosticSeverity.Warning
            , true
            , "The name of a union enum template must end with either \"_EnumTemplate\" or \"_Template\" suffix"
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.EnumTemplates.EnumTemplateAnalyzer"
            , "SG_ENUM_TEMPLATE_0002"
            , "Not support underlying type"
            , "Only enums whose underlying type is either byte, ushort, uint or ulong are supported"
            , "EnumTemplateGenerator"
            , DiagnosticSeverity.Warning
            , true
            , "Only enums whose underlying type is either byte, ushort, uint or ulong are supported"
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.EnumTemplates.EnumTemplateAnalyzer"
            , "SG_ENUM_TEMPLATE_0004"
            , "Must specify type and order"
            , "Must specify type and order"
            , "EnumTemplateGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Must specify type and order"
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.EnumTemplates.EnumTemplateAnalyzer"
            , "SG_ENUM_TEMPLATE_0005"
            , "First argument must be a type-of expression"
            , "First argument must be a type-of expression"
            , "EnumTemplateGenerator"
            , DiagnosticSeverity.Error
            , true
            , "First argument must be a type-of expression"
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.EnumTemplates.EnumTemplateAnalyzer"
            , "SG_ENUM_TEMPLATE_0007"
            , "Unbound generic type not supported"
            , "\"{0}\" must be a non-generic type or a closed generic type"
            , "EnumTemplateGenerator"
            , DiagnosticSeverity.Warning
            , true
            , "Only non-generic types or fully closed generic types are supported"
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
