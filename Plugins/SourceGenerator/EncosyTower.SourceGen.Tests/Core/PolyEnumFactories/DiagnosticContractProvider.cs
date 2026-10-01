using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumFactories;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Core/PolyEnumFactories";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0001"
            , "[PolyEnumFactoryFor] target must be partial"
            , "\"{0}\" is decorated with [PolyEnumFactoryFor] but is not declared as partial. Add the partial " +
              "keyword to allow code generation."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Types decorated with [PolyEnumFactoryFor] must be partial so the generator can extend them."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0003"
            , "[PolyEnumFactoryFor] target type must be a [PolyEnumStruct]"
            , "Type \"{0}\" passed to [PolyEnumFactoryFor] is not decorated with [PolyEnumStruct]. Factory " +
              "generation requires a poly-enum struct."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[PolyEnumFactoryFor(typeof(T))] requires T to be decorated with [PolyEnumStruct]."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0005"
            , "[PolyEnumFactoryFor] target type has no case structs"
            , "Type \"{0}\" has no eligible case structs. The generated factory will only contain an Undefined() " +
              "method."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Warning
            , true
            , "The poly-enum struct passed to [PolyEnumFactoryFor] should declare at least one nested case struct."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0007"
            , "Open poly-enum target and factory arity must match"
            , "Open target \"{0}\" has effective arity {1}, but factory \"{2}\" has effective arity {3}."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Fully open poly-enum targets bind factory type parameters positionally and require equal effective " +
              "arity."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0008"
            , "Open poly-enum target and factory constraints must match"
            , "Open target \"{0}\" and factory \"{1}\" have incompatible positional type parameter constraints."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Fully open poly-enum target constraints must be semantically equivalent after positional substitution."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0006"
            , "Case constructor with out parameter is ignored"
            , "Constructor of case struct \"{0}\" has an out parameter and will be skipped by [PolyEnumFactoryFor] " +
              "code generation."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Warning
            , true
            , "Factory methods cannot forward out parameters. Such constructors are ignored when generating " +
              "factories."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
