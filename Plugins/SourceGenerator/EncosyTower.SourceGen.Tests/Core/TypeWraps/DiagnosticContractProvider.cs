using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.TypeWraps;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Core/TypeWraps";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Core.Analyzers.TypeWraps.TypeWrapDiagnosticAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.TypeWraps.TypeWrapDiagnosticAnalyzer"
            , "SG_TYPE_WRAP_0001"
            , "[WrapType] is not allowed on record"
            , "[WrapType] cannot be applied to record type \"{0}\". Use [WrapRecord] instead."
            , "TypeWrapGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[WrapType] is not allowed on record."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.TypeWraps.TypeWrapDiagnosticAnalyzer"
            , "SG_TYPE_WRAP_0002"
            , "[WrapType] first argument must be a typeof expression"
            , "[WrapType] on \"{0}\" requires the first argument to be a typeof(...) expression"
            , "TypeWrapGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[WrapType] first argument must be a typeof expression."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.TypeWraps.TypeWrapDiagnosticAnalyzer"
            , "SG_TYPE_WRAP_0003"
            , "[WrapType] MemberName must be a valid C# identifier"
            , "[WrapType] on \"{0}\" requires MemberName \"{1}\" to be a non-empty, valid C# identifier"
            , "TypeWrapGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[WrapType] MemberName must be a valid C# identifier."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.TypeWraps.TypeWrapDiagnosticAnalyzer"
            , "SG_TYPE_WRAP_0004"
            , "[WrapRecord] requires a positional record with exactly one parameter"
            , "[WrapRecord] on \"{0}\" requires a positional record declaration with exactly one parameter"
            , "TypeWrapGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[WrapRecord] requires a positional record with exactly one parameter."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.TypeWraps.TypeWrapDiagnosticAnalyzer"
            , "SG_TYPE_WRAP_0005"
            , "Wrapper class must not inherit a base class"
            , "Wrapper class \"{0}\" must not inherit a base class other than System.Object"
            , "TypeWrapGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Wrapper class must not inherit a base class."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.TypeWraps.TypeWrapDiagnosticAnalyzer"
            , "SG_TYPE_WRAP_0006"
            , "[WrapRecord] is not allowed on non-record declaration"
            , "[WrapRecord] cannot be applied to \"{0}\" because it is not a record declaration"
            , "TypeWrapGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[WrapRecord] is not allowed on non-record declaration."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
