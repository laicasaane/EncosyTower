using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.NewtonsoftAotHelpers;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Core/NewtonsoftAotHelpers";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Core.Analyzers.NewtonsoftAotHelpers.NewtonsoftJsonAotHelperAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.NewtonsoftAotHelpers.NewtonsoftJsonAotHelperAnalyzer"
            , "SG_NEWTONSOFT_AOT_HELPER_0001"
            , "[NewtonsoftJsonAotHelper] cannot be applied to an abstract type"
            , "\"{0}\" is abstract. [NewtonsoftJsonAotHelper] requires a non-abstract, concrete type."
            , "NewtonsoftJsonAotHelperGenerator"
            , DiagnosticSeverity.Warning
            , true
            , "[NewtonsoftJsonAotHelper] is not supported on abstract types."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.NewtonsoftAotHelpers.NewtonsoftJsonAotHelperAnalyzer"
            , "SG_NEWTONSOFT_AOT_HELPER_0003"
            , "[NewtonsoftJsonAotHelper] requires a valid base type argument"
            , "\"{0}\" is annotated with [NewtonsoftJsonAotHelper] but the required base-type constructor argument " +
              "is missing or is not a type symbol."
            , "NewtonsoftJsonAotHelperGenerator"
            , DiagnosticSeverity.Warning
            , true
            , "[NewtonsoftJsonAotHelper(typeof(T))] requires exactly one typeof(T) argument specifying the base " +
              "type " +
              "to scan for."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.NewtonsoftAotHelpers.NewtonsoftJsonAotHelperAnalyzer"
            , "SG_NEWTONSOFT_AOT_HELPER_0004"
            , "[NewtonsoftJsonAotHelper] cannot be applied to a generic type"
            , "\"{0}\" is a generic type. [NewtonsoftJsonAotHelper] requires a non-generic type."
            , "NewtonsoftJsonAotHelperGenerator"
            , DiagnosticSeverity.Warning
            , true
            , "[NewtonsoftJsonAotHelper] is not supported on generic types."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
